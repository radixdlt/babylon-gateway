/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.DataAggregator;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class PostgresLedgerExtenderService : ILedgerExtenderService
{
    private readonly ILogger<PostgresLedgerExtenderService> _logger;
    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IEnumerable<ILedgerExtenderServiceObserver> _observers;
    private readonly IClock _clock;

    public PostgresLedgerExtenderService(
        ILogger<PostgresLedgerExtenderService> logger,
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        INetworkConfigurationProvider networkConfigurationProvider,
        IEnumerable<ILedgerExtenderServiceObserver> observers,
        IClock clock)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _networkConfigurationProvider = networkConfigurationProvider;
        _observers = observers;
        _clock = clock;
    }

    public async Task<TransactionSummary> GetLatestTransactionSummary(CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        return await GetTopOfLedger(dbContext, token);
    }

    public async Task<CommitTransactionsReport> CommitTransactions(ConsistentLedgerExtension ledgerExtension, SyncTargetCarrier latestSyncTarget, CancellationToken token = default)
    {
        var (preparationReport, ledgerExtensionReport) = await ExtendLedger(ledgerExtension, latestSyncTarget.TargetStateVersion, token);
        var processTransactionReport = ledgerExtensionReport.ProcessTransactionsReport;

        var dbEntriesWritten =
            preparationReport.RawTxnUpsertTouchedRecords
            + preparationReport.PendingTransactionsTouchedRecords
            + preparationReport.PreparationEntriesTouched
            + ledgerExtensionReport.EntriesWritten;

        return new CommitTransactionsReport(
            ledgerExtension.CommittedTransactions.Count,
            ledgerExtensionReport.FinalTransactionSummary,
            preparationReport.RawTxnPersistenceMs,
            preparationReport.PendingTransactionUpdateMs,
            (long)processTransactionReport.ContentHandlingDuration.TotalMilliseconds,
            (long)processTransactionReport.DbReadDuration.TotalMilliseconds,
            ledgerExtensionReport.DbPersistenceMs,
            dbEntriesWritten
        );
    }

    private async Task EnsureDbLedgerIsInitialized(CancellationToken token)
    {
        var created = await _networkConfigurationProvider.SaveLedgerNetworkConfigurationToDatabaseOnInitIfNotExists(token);

        if (created)
        {
            _logger.LogInformation(
                "Ledger initialized with network: {NetworkName}",
                _networkConfigurationProvider.GetNetworkName()
            );
        }
    }

    private record PreparationReport(
        long RawTxnPersistenceMs,
        int RawTxnUpsertTouchedRecords,
        long PendingTransactionUpdateMs,
        int PendingTransactionsTouchedRecords,
        int PreparationEntriesTouched
    );

    private record ExtensionReport(
        ProcessTransactionsReport ProcessTransactionsReport,
        TransactionSummary FinalTransactionSummary,
        int EntriesWritten,
        long DbPersistenceMs
    );

    private async Task<(PreparationReport PreparationReport, ExtensionReport ExtensionReport)> ExtendLedger(ConsistentLedgerExtension ledgerExtension, long latestSyncTarget, CancellationToken token)
    {
        // Create own context for ledger extension unit of work
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        await using var tx = await dbContext.Database.BeginTransactionAsync(token);

        try
        {
            var topOfLedgerSummary = await GetTopOfLedger(dbContext, token);

            TransactionConsistency.AssertLatestTransactionConsistent(ledgerExtension.LatestTransactionSummary.StateVersion, topOfLedgerSummary.StateVersion);

            if (topOfLedgerSummary.StateVersion == 0)
            {
                await EnsureDbLedgerIsInitialized(token);
            }

            var (finalTransaction, preparationReport, processReport) = await ProcessTransactions(dbContext, ledgerExtension, token);

            await CreateOrUpdateLedgerStatus(dbContext, finalTransaction, latestSyncTarget, token);

            var (remainingRowsTouched, remainingDbDuration) = await CodeStopwatch.TimeInMs(
                () => dbContext.SaveChangesAsync(token)
            );

            await tx.CommitAsync(token);

            var extensionReport = new ExtensionReport(
                processReport,
                finalTransaction,
                processReport.RowsTouched + remainingRowsTouched,
                ((long)processReport.DbWriteDuration.TotalMilliseconds) + remainingDbDuration);

            return (preparationReport, extensionReport);
        }
        catch (Exception)
        {
            await tx.RollbackAsync(token);

            throw;
        }
    }

    private record ProcessTransactionsResult(
        TransactionSummary FinalTransaction,
        PreparationReport PreparationReport,
        ProcessTransactionsReport Report);

    private record ProcessTransactionsReport(
        int RowsTouched,
        TimeSpan DbReadDuration,
        TimeSpan DbWriteDuration,
        TimeSpan ContentHandlingDuration
    );

    private async Task<ProcessTransactionsResult> ProcessTransactions(ReadWriteDbContext dbContext, ConsistentLedgerExtension ledgerExtension, CancellationToken token)
    {
        // TODO further improvements:
        // - queries with WHERE xxx = ANY(<list of 12345 ids>) are probably not very performant
        // - replace with proper Activity at some point

        var rowsInserted = 0;
        var rowsUpdated = 0;
        var dbReadDuration = TimeSpan.Zero;
        var dbWriteDuration = TimeSpan.Zero;
        var outerStopwatch = Stopwatch.StartNew();

        // TODO replace usage of HEX-encoded strings in favor of raw RadixAddress? how about global addresses?
        var ledgerTransactions = new List<LedgerTransaction>(ledgerExtension.CommittedTransactions.Count);
        var referencedEntities = new ReferencedEntityDictionary();
        var knownGlobalAddressesToLoad = new HashSet<string>();
        var childToParentEntities = new Dictionary<string, string>();
        var componentToGlobalPackage = new Dictionary<string, (string PackageAddress, string BlueprintName)>();
        var fungibleResourceManagerDivisibility = new Dictionary<string, int>();
        var nonFungibleResourceManagerNonFungibleIdType = new Dictionary<string, NonFungibleIdType>();
        var packageCode = new Dictionary<string, byte[]>();
        var entityRawAccessRulesChain = new Dictionary<AccessRulesChainLookup, string>();
        var componentEntityRawState = new Dictionary<string, string>();

        var lastTransactionSummary = ledgerExtension.LatestTransactionSummary;
        var dbConn = (NpgsqlConnection)dbContext.Database.GetDbConnection();

        PreparationReport preparationReport;
        SequencesHolder sequences;

        // step: preparation (previously RawTransactionWriter logic)
        {
            var rawUserTransactionsByPayloadHash = ledgerExtension.CommittedTransactions
                .Where(ct => ct.LedgerTransaction.ActualInstance is CoreModel.UserLedgerTransaction)
                .Select(ult =>
                {
                    var nt = ult.LedgerTransaction.GetUserLedgerTransaction().NotarizedTransaction;

                    return new RawUserTransaction
                    {
                        StateVersion = ult.StateVersion,
                        PayloadHash = nt.HashBytes,
                        Payload = nt.PayloadBytes,
                        Receipt = ult.Receipt.ToJson(),
                    };
                })
                .ToDictionary(x => x.PayloadHash, ByteArrayEqualityComparer.Default);

            var rawUserTransactions = rawUserTransactionsByPayloadHash.Values.ToList();

            var (rawTransactionsTouched, rawTransactionCommitMs) = await CodeStopwatch.TimeInMs(async () =>
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY raw_user_transactions (state_version, payload_hash, payload, receipt) FROM STDIN (FORMAT BINARY)", token);

                foreach (var rt in rawUserTransactions)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(rt.StateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(rt.PayloadHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(rt.Payload, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(rt.Receipt, NpgsqlDbType.Text, token);
                }

                await writer.CompleteAsync(token);

                return rawUserTransactions.Count;
            });

            var (pendingTransactionsTouched, pendingTransactionUpdateMs) = await CodeStopwatch.TimeInMs(async () =>
            {
                var transactionPayloadHashList = rawUserTransactionsByPayloadHash.Keys.ToList(); // List<> are optimised for PostgreSQL lookups

                var toUpdate = await dbContext.PendingTransactions
                    .Where(mt => mt.Status != PendingTransactionStatus.Committed && transactionPayloadHashList.Contains(mt.PayloadHash))
                    .ToListAsync(token);

                if (toUpdate.Count == 0)
                {
                    return 0;
                }

                foreach (var pendingTransaction in toUpdate)
                {
                    if (pendingTransaction.Status == PendingTransactionStatus.Failed)
                    {
                        await _observers.ForEachAsync(x => x.TransactionsMarkedCommittedWhichWasFailed());

                        _logger.LogError(
                            "Transaction with payload hash {PayloadHash} which was first/last submitted to Gateway at {FirstGatewaySubmissionTime}/{LastGatewaySubmissionTime} and last marked missing from mempool at {LastMissingFromMempoolTimestamp} was mark failed at {FailureTime} due to {FailureReason} ({FailureExplanation}) but has now been marked committed",
                            pendingTransaction.PayloadHash.ToHex(),
                            pendingTransaction.FirstSubmittedToGatewayTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                            pendingTransaction.LastSubmittedToGatewayTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                            pendingTransaction.LastDroppedOutOfMempoolTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                            pendingTransaction.FailureTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                            pendingTransaction.FailureReason?.ToString(),
                            pendingTransaction.FailureExplanation
                        );
                    }

                    pendingTransaction.MarkAsCommitted(_clock.UtcNow);
                }

                // If this errors (due to changes to the MempoolTransaction.Status ConcurrencyToken), we may have to consider
                // something like: https://docs.microsoft.com/en-us/ef/core/saving/concurrency
                var result = await dbContext.SaveChangesAsync(token);

                await _observers.ForEachAsync(x => x.TransactionsMarkedCommittedCount(toUpdate.Count));

                return result;
            });

            var preparationEntriesTouched = await dbContext.SaveChangesAsync(token);

            preparationReport = new PreparationReport(
                rawTransactionCommitMs,
                rawTransactionsTouched,
                pendingTransactionUpdateMs,
                pendingTransactionsTouched,
                preparationEntriesTouched
            );
        }

        // step: load current sequences
        {
            (sequences, var sequencesInitializeDuration) = await CodeStopwatch.Time(() => SequencesHolder.Initialize(dbConn, token));

            dbReadDuration += sequencesInitializeDuration;
        }

        // step: scan for any referenced entities
        {
            var hrp = _networkConfigurationProvider.GetHrpDefinition();

            foreach (var ct in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = ct.StateVersion;
                var stateUpdates = ct.Receipt.StateUpdates;

                long? newEpoch = null;
                long? newRoundInEpoch = null;
                DateTime? newRoundTimestamp = null;

                if (ct.LedgerTransaction.ActualInstance is CoreModel.ValidatorLedgerTransaction vlt)
                {
                    switch (vlt.ValidatorTransaction.ActualInstance)
                    {
                        case CoreModel.EpochUpdateValidatorTransaction epochUpdate:
                            newEpoch = epochUpdate.ScryptoEpoch;
                            break;
                        case CoreModel.TimeUpdateValidatorTransaction timeUpdate:
                            newRoundInEpoch = timeUpdate.RoundInEpoch;
                            newRoundTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(timeUpdate.ProposerTimestampMs).UtcDateTime;
                            break;
                    }
                }

                foreach (var newSubstate in stateUpdates.CreatedSubstates.Concat(stateUpdates.UpdatedSubstates))
                {
                    var sid = newSubstate.SubstateId;
                    var sd = newSubstate.SubstateData.ActualInstance;

                    if (sd is CoreModel.GlobalAddressSubstate globalAddress)
                    {
                        var target = globalAddress.TargetEntity;
                        var te = referencedEntities.GetOrAdd(target.TargetEntityIdHex, _ => new ReferencedEntity(target.TargetEntityIdHex, target.TargetEntityType, stateVersion));

                        te.Globalize(target.GlobalAddressHex);

                        if (target.TargetEntityType == CoreModel.EntityType.Component)
                        {
                            if (target.GlobalAddress.StartsWith(hrp.AccountComponent))
                            {
                                te.WithTypeHint(typeof(AccountComponentEntity));
                            }
                            else if (target.GlobalAddress.StartsWith(hrp.SystemComponent))
                            {
                                te.WithTypeHint(typeof(SystemComponentEntity));
                            }
                            else
                            {
                                te.WithTypeHint(typeof(NormalComponentEntity));
                            }
                        }

                        // we do not want to store GlobalEntities as they bring no value from NG perspective
                        // GlobalAddress is essentially a property of other entities

                        continue;
                    }

                    var re = referencedEntities.GetOrAdd(sid.EntityIdHex, _ => new ReferencedEntity(sid.EntityIdHex, sid.EntityType, stateVersion));

                    if (sd is CoreModel.IOwner owner)
                    {
                        foreach (var oe in owner.OwnedEntities)
                        {
                            referencedEntities.GetOrAdd(oe.EntityIdHex, _ => new ReferencedEntity(oe.EntityIdHex, oe.EntityType, stateVersion)).IsImmediateChildOf(re);

                            childToParentEntities[oe.EntityIdHex] = sid.EntityIdHex;
                        }
                    }

                    if (sd is CoreModel.IGlobalResourcePointer globalResourcePointer)
                    {
                        foreach (var pointer in globalResourcePointer.Pointers)
                        {
                            knownGlobalAddressesToLoad.Add(pointer.GlobalAddress);
                        }
                    }

                    if (sd is CoreModel.ResourceManagerSubstate resourceManager)
                    {
                        Type typeHint = resourceManager.ResourceType switch
                        {
                            CoreModel.ResourceType.Fungible => typeof(FungibleResourceManagerEntity),
                            CoreModel.ResourceType.NonFungible => typeof(NonFungibleResourceManagerEntity),
                            _ => throw new ArgumentOutOfRangeException(),
                        };

                        re.WithTypeHint(typeHint);

                        if (resourceManager.ResourceType == CoreModel.ResourceType.Fungible)
                        {
                            fungibleResourceManagerDivisibility[sid.EntityIdHex] = resourceManager.FungibleDivisibility;
                        }

                        if (resourceManager.ResourceType == CoreModel.ResourceType.NonFungible)
                        {
                            var type = resourceManager.NonFungibleIdType ?? throw new InvalidOperationException("NonFungibleIdType must be set.");

                            nonFungibleResourceManagerNonFungibleIdType[sid.EntityIdHex] = type switch
                            {
                                CoreModel.NonFungibleIdType.String => NonFungibleIdType.String,
                                CoreModel.NonFungibleIdType.U32 => NonFungibleIdType.U32,
                                CoreModel.NonFungibleIdType.U64 => NonFungibleIdType.U64,
                                CoreModel.NonFungibleIdType.Bytes => NonFungibleIdType.Bytes,
                                CoreModel.NonFungibleIdType.UUID => NonFungibleIdType.UUID,
                                _ => throw new ArgumentOutOfRangeException(),
                            };
                        }
                    }

                    if (sd is CoreModel.AccessRulesChainSubstate accessRulesChain)
                    {
                        var serialized = JsonConvert.SerializeObject(accessRulesChain.Chain);

                        if (sid.SubstateKeyType == CoreModel.SubstateKeyType.AccessRulesChain)
                        {
                            entityRawAccessRulesChain[new AccessRulesChainLookup(sid.EntityIdHex, AccessRulesChainSubtype.None)] = serialized;
                        }
                        else if (sid.SubstateKeyType == CoreModel.SubstateKeyType.ResourceManagerVaultAccessRulesChain)
                        {
                            entityRawAccessRulesChain[new AccessRulesChainLookup(sid.EntityIdHex, AccessRulesChainSubtype.ResourceManagerVaultAccessRulesChain)] = serialized;
                        }
                        else
                        {
                            throw new UnreachableException();
                        }
                    }

                    if (sd is CoreModel.ComponentInfoSubstate componentInfo)
                    {
                        knownGlobalAddressesToLoad.Add(componentInfo.PackageAddress);
                        componentToGlobalPackage[sid.EntityIdHex] = (componentInfo.PackageAddress, componentInfo.BlueprintName);
                    }

                    if (sd is CoreModel.ComponentStateSubstate componentState)
                    {
                        componentEntityRawState[sid.EntityIdHex] = componentState.DataStruct.StructData.ToJson();
                    }

                    if (sd is CoreModel.PackageInfoSubstate packageInfo)
                    {
                        packageCode[sid.EntityIdHex] = packageInfo.CodeBytes;
                    }
                }

                foreach (var deletedSubstate in stateUpdates.DeletedSubstates)
                {
                    var sid = deletedSubstate.SubstateId;

                    referencedEntities.GetOrAdd(sid.EntityIdHex, _ => new ReferencedEntity(sid.EntityIdHex, sid.EntityType, stateVersion));
                }

                /* NB:
                   The Epoch Transition Transaction sort of fits between epochs, but it seems to fit slightly more naturally
                   as the _first_ transaction of a new epoch, as creates the next EpochData, and the RoundData to 0.
                */

                var isStartOfEpoch = newEpoch != null;
                var isStartOfRound = newRoundInEpoch != null;
                var roundTimestamp = newRoundTimestamp ?? lastTransactionSummary.RoundTimestamp;
                var createdTimestamp = _clock.UtcNow;
                var normalizedRoundTimestamp = // Clamp between lastTransaction.NormalizedTimestamp and createdTimestamp
                    roundTimestamp < lastTransactionSummary.NormalizedRoundTimestamp ? lastTransactionSummary.NormalizedRoundTimestamp
                    : roundTimestamp > createdTimestamp ? createdTimestamp
                    : roundTimestamp;

                var summary = new TransactionSummary(
                    StateVersion: stateVersion,
                    RoundTimestamp: roundTimestamp,
                    NormalizedRoundTimestamp: normalizedRoundTimestamp,
                    CreatedTimestamp: createdTimestamp,
                    Epoch: newEpoch ?? lastTransactionSummary.Epoch,
                    IndexInEpoch: isStartOfEpoch ? 0 : lastTransactionSummary.IndexInEpoch + 1,
                    RoundInEpoch: newRoundInEpoch ?? lastTransactionSummary.RoundInEpoch,
                    IsStartOfEpoch: isStartOfEpoch,
                    IsStartOfRound: isStartOfRound,
                    TransactionAccumulator: ct.LedgerTransaction.PayloadBytes);

                LedgerTransaction ledgerTransaction = ct.LedgerTransaction.ActualInstance switch
                {
                    CoreModel.UserLedgerTransaction ult => new UserLedgerTransaction
                    {
                        PayloadHash = ult.NotarizedTransaction.HashBytes,
                        IntentHash = ult.NotarizedTransaction.SignedIntent.Intent.HashBytes,
                        SignedIntentHash = ult.NotarizedTransaction.SignedIntent.HashBytes,
                    },
                    CoreModel.ValidatorLedgerTransaction => new ValidatorLedgerTransaction(),
                    _ => throw new ArgumentOutOfRangeException(nameof(ct.LedgerTransaction), ct.LedgerTransaction, null),
                };

                ledgerTransaction.StateVersion = ct.StateVersion;
                ledgerTransaction.Status = ct.Receipt.Status.ToModel();
                ledgerTransaction.ErrorMessage = ct.Receipt.ErrorMessage;
                ledgerTransaction.TransactionAccumulator = ct.AccumulatorHashBytes;
                // TODO commented out as incompatible with current Core API version
                ledgerTransaction.Message = null; // message: transaction.Metadata.Message?.ConvertFromHex(),
                ledgerTransaction.Epoch = summary.Epoch;
                ledgerTransaction.IndexInEpoch = summary.IndexInEpoch;
                ledgerTransaction.RoundInEpoch = summary.RoundInEpoch;
                ledgerTransaction.IsStartOfEpoch = summary.IsStartOfEpoch;
                ledgerTransaction.IsStartOfRound = summary.IsStartOfRound;
                ledgerTransaction.FeePaid = TokenAmount.FromDecimalString(ct.Receipt.FeeSummary.XrdTotalExecutionCost);
                ledgerTransaction.TipPaid = TokenAmount.FromDecimalString(ct.Receipt.FeeSummary.XrdTotalTipped);
                ledgerTransaction.RoundTimestamp = summary.RoundTimestamp;
                ledgerTransaction.CreatedTimestamp = summary.CreatedTimestamp;
                ledgerTransaction.NormalizedRoundTimestamp = summary.NormalizedRoundTimestamp;

                ledgerTransactions.Add(ledgerTransaction);

                lastTransactionSummary = summary;
            }
        }

        // step: resolve known types & optionally create missing entities
        {
            var entityAddresses = referencedEntities.Addresses.Select(x => x.ConvertFromHex()).ToList();
            var globalEntityAddresses = knownGlobalAddressesToLoad.Select(x => RadixAddressCodec.Decode(x).Data).ToList();
            var entityAddressesParameter = new NpgsqlParameter("@entity_addresses", NpgsqlDbType.Array | NpgsqlDbType.Bytea) { Value = entityAddresses };
            var globalEntityAddressesParameter = new NpgsqlParameter("@global_entity_addresses", NpgsqlDbType.Array | NpgsqlDbType.Bytea) { Value = globalEntityAddresses };

            var sw = Stopwatch.StartNew();
            var knownDbEntities = await dbContext.Entities
                .FromSqlInterpolated($@"
SELECT *
FROM entities
WHERE id IN(
    SELECT DISTINCT UNNEST(id || ancestor_ids) AS id
    FROM entities
    WHERE address = ANY({entityAddressesParameter}) OR global_address = ANY({globalEntityAddressesParameter})
)")
                .AsNoTracking()
                .ToDictionaryAsync(e => ((byte[])e.Address).ToHex(), token);

            dbReadDuration += sw.Elapsed;

            foreach (var knownDbEntity in knownDbEntities.Values.Where(e => e.GlobalAddress != null))
            {
                var entityType = knownDbEntity switch
                {
                    EpochManagerEntity => CoreModel.EntityType.EpochManager,
                    ResourceManagerEntity => CoreModel.EntityType.ResourceManager,
                    ComponentEntity => CoreModel.EntityType.Component,
                    PackageEntity => CoreModel.EntityType.Package,
                    KeyValueStoreEntity => CoreModel.EntityType.KeyValueStore,
                    VaultEntity => CoreModel.EntityType.Vault,
                    NonFungibleStoreEntity => CoreModel.EntityType.NonFungibleStore,
                    ClockEntity => CoreModel.EntityType.Clock,
                    _ => throw new ArgumentOutOfRangeException(nameof(knownDbEntity), knownDbEntity.GetType().Name),
                };

                referencedEntities.GetOrAdd(knownDbEntity.Address.ToHex(), address => new ReferencedEntity(address, entityType, knownDbEntity.FromStateVersion));
            }

            var dbEntities = new List<Entity>();

            foreach (var re in referencedEntities.All)
            {
                if (knownDbEntities.ContainsKey(re.IdHex))
                {
                    re.Resolve(knownDbEntities[re.IdHex]);

                    continue;
                }

                Entity dbEntity = re.Type switch
                {
                    CoreModel.EntityType.EpochManager => new EpochManagerEntity(),
                    CoreModel.EntityType.ResourceManager => re.CreateUsingTypeHint<ResourceManagerEntity>(),
                    CoreModel.EntityType.Component => re.CreateUsingTypeHint<ComponentEntity>(),
                    CoreModel.EntityType.Package => new PackageEntity(),
                    CoreModel.EntityType.Vault => new VaultEntity(),
                    CoreModel.EntityType.KeyValueStore => new KeyValueStoreEntity(),
                    CoreModel.EntityType.Global => throw new ArgumentOutOfRangeException(nameof(re.Type), re.Type, "Global entities should be filtered out"),
                    CoreModel.EntityType.NonFungibleStore => new NonFungibleStoreEntity(),
                    CoreModel.EntityType.Clock => new ClockEntity(),
                    _ => throw new ArgumentOutOfRangeException(nameof(re.Type), re.Type, null),
                };

                dbEntity.Id = sequences.NextEntity;
                dbEntity.FromStateVersion = re.StateVersion;
                dbEntity.Address = re.IdHex.ConvertFromHex();
                dbEntity.GlobalAddress = re.GlobalAddressHex == null ? null : (RadixAddress)re.GlobalAddressHex.ConvertFromHex();

                re.Resolve(dbEntity);
                dbEntities.Add(dbEntity);
            }

            foreach (var (childAddress, parentAddress) in childToParentEntities)
            {
                var currentParent = referencedEntities.Get(parentAddress);
                long? parentId = null;
                long? ownerId = null;
                long? globalId = null;

                var allAncestors = new List<long>();

                while (currentParent != null)
                {
                    allAncestors.Add(currentParent.DatabaseId);
                    parentId ??= currentParent.DatabaseId;

                    if (!ownerId.HasValue && currentParent.CanBeOwner)
                    {
                        ownerId = currentParent.DatabaseId;
                    }

                    if (!globalId.HasValue && currentParent.IsGlobal)
                    {
                        globalId = currentParent.DatabaseId;
                    }

                    if (currentParent.HasImmediateParentReference)
                    {
                        currentParent = currentParent.ImmediateParentReference;
                    }
                    else if (currentParent.DatabaseParentAncestorId.HasValue)
                    {
                        currentParent = referencedEntities.GetByDatabaseId(currentParent.DatabaseParentAncestorId.Value);
                    }
                    else
                    {
                        currentParent = null;
                    }
                }

                if (parentId == null || ownerId == null || globalId == null)
                {
                    throw new InvalidOperationException($"Unable to compute ancestors of entity {childAddress}: parentId={parentId}, ownerId={ownerId}, globalId={globalId}.");
                }

                referencedEntities.Get(childAddress).ConfigureDatabaseEntity((Entity dbe) =>
                {
                    dbe.AncestorIds = allAncestors.ToArray();
                    dbe.ParentAncestorId = parentId.Value;
                    dbe.OwnerAncestorId = ownerId.Value;
                    dbe.GlobalAncestorId = globalId.Value;
                });
            }

            foreach (var (entityAddress, tuple) in componentToGlobalPackage)
            {
                referencedEntities.Get(entityAddress).ConfigureDatabaseEntity((ComponentEntity dbe) =>
                {
                    var packageAddress = RadixAddressCodec.Decode(tuple.PackageAddress).Data.ToHex();

                    dbe.PackageId = referencedEntities.GetByGlobal(packageAddress).DatabaseId;
                    dbe.BlueprintName = tuple.BlueprintName;
                });
            }

            foreach (var (entityAddress, divisibility) in fungibleResourceManagerDivisibility)
            {
                referencedEntities.Get(entityAddress).ConfigureDatabaseEntity((FungibleResourceManagerEntity dbe) => dbe.Divisibility = divisibility);
            }

            foreach (var (entityAddress, nonFungibleIdType) in nonFungibleResourceManagerNonFungibleIdType)
            {
                referencedEntities.Get(entityAddress).ConfigureDatabaseEntity((NonFungibleResourceManagerEntity dbe) => dbe.NonFungibleIdType = nonFungibleIdType);
            }

            foreach (var (entityAddress, code) in packageCode)
            {
                referencedEntities.Get(entityAddress).ConfigureDatabaseEntity((PackageEntity pe) => pe.Code = code);
            }

            sw = Stopwatch.StartNew();

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY entities (id, from_state_version, address, global_address, ancestor_ids, parent_ancestor_id, owner_ancestor_id, global_ancestor_id, discriminator, package_id, blueprint_name, divisibility, non_fungible_id_type, code) FROM STDIN (FORMAT BINARY)", token))
            {
                var nonFungibleIdTypeConverter = new NonFungibleIdTypeValueConverter().ConvertToProvider;

                foreach (var dbEntity in dbEntities)
                {
                    if (dbContext.Model.FindEntityType(dbEntity.GetType())?.GetDiscriminatorValue() is not string discriminator)
                    {
                        throw new InvalidOperationException("Unable to determine discriminator of entity " + dbEntity.Address.ToHex());
                    }

                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(dbEntity.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(dbEntity.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(dbEntity.Address.AsByteArray(), NpgsqlDbType.Bytea, token);
                    await writer.WriteNullableAsync(dbEntity.GlobalAddress.AsByteArray(), NpgsqlDbType.Bytea, token);
                    await writer.WriteNullableAsync(dbEntity.AncestorIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                    await writer.WriteNullableAsync(dbEntity.ParentAncestorId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullableAsync(dbEntity.OwnerAncestorId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullableAsync(dbEntity.GlobalAncestorId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(discriminator, NpgsqlDbType.Text, token);
                    await writer.WriteNullableAsync(dbEntity is ComponentEntity ce1 ? ce1.PackageId : null, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(dbEntity is ComponentEntity ce2 ? ce2.BlueprintName : null, NpgsqlDbType.Text, token);
                    await writer.WriteNullableAsync(dbEntity is FungibleResourceManagerEntity frme ? frme.Divisibility : null, NpgsqlDbType.Integer, token);
                    await writer.WriteAsync(dbEntity is NonFungibleResourceManagerEntity nfrme ? nonFungibleIdTypeConverter(nfrme.NonFungibleIdType) : null, NpgsqlDbType.Text, token);
                    await writer.WriteNullableAsync(dbEntity is PackageEntity pe ? pe.Code : null, NpgsqlDbType.Bytea, token);
                }

                await writer.CompleteAsync(token);
            }

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY ledger_transactions (state_version, status, error_message, transaction_accumulator, message, epoch, index_in_epoch, round_in_epoch, is_start_of_epoch, is_start_of_round, referenced_entities, fee_paid, tip_paid, round_timestamp, created_timestamp, normalized_round_timestamp, discriminator, payload_hash, intent_hash, signed_intent_hash) FROM STDIN (FORMAT BINARY)", token))
            {
                var statusConverter = new LedgerTransactionStatusValueConverter().ConvertToProvider;

                if (dbContext.Model.FindEntityType(typeof(UserLedgerTransaction))?.GetDiscriminatorValue() is not string userDiscriminator)
                {
                    throw new InvalidOperationException("Unable to determine discriminator of UserLedgerTransaction");
                }

                if (dbContext.Model.FindEntityType(typeof(ValidatorLedgerTransaction))?.GetDiscriminatorValue() is not string validatorDiscriminator)
                {
                    throw new InvalidOperationException("Unable to determine discriminator of ValidatorLedgerTransaction");
                }

                foreach (var lt in ledgerTransactions)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(lt.StateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(statusConverter(lt.Status), NpgsqlDbType.Text, token);
                    await writer.WriteAsync(lt.ErrorMessage, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(lt.TransactionAccumulator, NpgsqlDbType.Bytea, token);
                    await writer.WriteNullableAsync(lt.Message, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(lt.Epoch, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(lt.IndexInEpoch, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(lt.RoundInEpoch, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(lt.IsStartOfEpoch, NpgsqlDbType.Boolean, token);
                    await writer.WriteAsync(lt.IsStartOfRound, NpgsqlDbType.Boolean, token);
                    await writer.WriteAsync(referencedEntities.OfStateVersion(lt.StateVersion).Select(re => re.DatabaseId).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(lt.FeePaid.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteAsync(lt.TipPaid.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteAsync(lt.RoundTimestamp, NpgsqlDbType.TimestampTz, token);
                    await writer.WriteAsync(lt.CreatedTimestamp, NpgsqlDbType.TimestampTz, token);
                    await writer.WriteAsync(lt.NormalizedRoundTimestamp, NpgsqlDbType.TimestampTz, token);

                    switch (lt)
                    {
                        case UserLedgerTransaction ult:
                            await writer.WriteAsync(userDiscriminator, NpgsqlDbType.Text, token);
                            await writer.WriteAsync(ult.PayloadHash, NpgsqlDbType.Bytea, token);
                            await writer.WriteAsync(ult.IntentHash, NpgsqlDbType.Bytea, token);
                            await writer.WriteAsync(ult.SignedIntentHash, NpgsqlDbType.Bytea, token);
                            break;
                        case ValidatorLedgerTransaction:
                            await writer.WriteAsync(validatorDiscriminator, NpgsqlDbType.Text, token);
                            await writer.WriteNullAsync(token);
                            await writer.WriteNullAsync(token);
                            await writer.WriteNullAsync(token);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(lt), lt, null);
                    }
                }

                await writer.CompleteAsync(token);
            }

            if (entityRawAccessRulesChain.Any())
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY entity_access_rules_chain_history (id, from_state_version, entity_id, subtype, access_rules_chain) FROM STDIN (FORMAT BINARY)", token);

                var subtypeConverter = new AccessRulesChainSubtypeValueConverter().ConvertToProvider;

                foreach (var (lookup, accessRulesChain) in entityRawAccessRulesChain)
                {
                    var re = referencedEntities.Get(lookup.EntityIdHex);

                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(sequences.NextEntityAccessRulesChainHistory, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(re.StateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(re.DatabaseId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(subtypeConverter(lookup.Subtype), NpgsqlDbType.Text, token);
                    await writer.WriteAsync(accessRulesChain, NpgsqlDbType.Jsonb, token);
                }

                await writer.CompleteAsync(token);
            }

            if (componentEntityRawState.Any())
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY component_entity_state_history (id, from_state_version, component_entity_id, state) FROM STDIN (FORMAT BINARY)", token);

                foreach (var (entityAddress, state) in componentEntityRawState)
                {
                    var re = referencedEntities.Get(entityAddress);

                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(sequences.NextComponentEntityStateHistory, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(re.StateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(re.DatabaseId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(state, NpgsqlDbType.Jsonb, token);
                }

                await writer.CompleteAsync(token);
            }

            rowsInserted += dbEntities.Count + ledgerTransactions.Count + entityRawAccessRulesChain.Count + componentEntityRawState.Count;
            dbWriteDuration += sw.Elapsed;
        }

        var fungibleVaultChanges = new List<FungibleVaultChange>();
        var nonFungibleVaultChanges = new List<NonFungibleVaultChange>();
        var nonFungibleIdStoreChanges = new List<NonFungibleIdChange>();
        var metadataChanges = new List<MetadataChange>();
        var fungibleResourceSupplyChanges = new List<FungibleResourceSupply>();

        // step: scan all substates to figure out changes
        {
            foreach (var ct in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = ct.StateVersion;
                var stateUpdates = ct.Receipt.StateUpdates;

                foreach (var newSubstate in stateUpdates.CreatedSubstates.Concat(stateUpdates.UpdatedSubstates))
                {
                    var sid = newSubstate.SubstateId;
                    var sd = newSubstate.SubstateData.ActualInstance;

                    if (sd is CoreModel.GlobalAddressSubstate)
                    {
                        // we do not want to store GlobalEntities as they bring no value from NG perspective

                        continue;
                    }

                    var re = referencedEntities.Get(sid.EntityIdHex);

                    if (sd is CoreModel.MetadataSubstate metadata)
                    {
                        metadataChanges.Add(new MetadataChange(re, metadata.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), stateVersion));
                    }

                    if (sd is CoreModel.ResourceManagerSubstate resourceManager)
                    {
                        var totalSupply = TokenAmount.FromDecimalString(resourceManager.TotalSupply);

                        if (resourceManager.ResourceType == CoreModel.ResourceType.Fungible)
                        {
                            fungibleResourceSupplyChanges.Add(new FungibleResourceSupply(re, totalSupply, TokenAmount.Zero, TokenAmount.Zero, stateVersion)); // TODO support mint & burnt
                        }
                    }

                    if (sd is CoreModel.VaultSubstate vault)
                    {
                        var resourceAmount = vault.ResourceAmount.ActualInstance;

                        switch (resourceAmount)
                        {
                            case CoreModel.FungibleResourceAmount fra:
                            {
                                var amount = TokenAmount.FromDecimalString(fra.Amount);
                                var resourceAddress = RadixAddressCodec.Decode(fra.ResourceAddress).Data.ToHex();
                                var resourceEntity = referencedEntities.GetByGlobal(resourceAddress);

                                fungibleVaultChanges.Add(new FungibleVaultChange(re, resourceEntity, amount, stateVersion));

                                break;
                            }

                            case CoreModel.NonFungibleResourceAmount nfra:
                            {
                                var resourceAddress = RadixAddressCodec.Decode(nfra.ResourceAddress).Data.ToHex();
                                var resourceEntity = referencedEntities.GetByGlobal(resourceAddress);

                                nonFungibleVaultChanges.Add(new NonFungibleVaultChange(re, resourceEntity, nfra.NonFungibleIds.Select(nfid => nfid.SimpleRep).ToList(), stateVersion));

                                break;
                            }

                            default:
                                throw new ArgumentOutOfRangeException(nameof(resourceAmount), resourceAmount.GetType().Name);
                        }
                    }

                    if (sd is CoreModel.NonFungibleStoreEntrySubstate nonFungibleStoreEntry)
                    {
                        nonFungibleIdStoreChanges.Add(new NonFungibleIdChange(re, nonFungibleStoreEntry.NonFungibleId.SimpleRep, nonFungibleStoreEntry.IsDeleted, nonFungibleStoreEntry.NonFungibleData, stateVersion));
                    }
                }
            }
        }

        // step: now that all the fundamental data is inserted (entities & substates) we can insert some denormalized data
        {
            // entity_id => state_version => resource_id[] (added or removed)
            var vaultAggregateDelta = new Dictionary<long, Dictionary<long, AggregateChange>>();

            // TODO when it comes to fungibles and nonFungibles and their respective xxxResourceHistory we should change that to VaultHistory

            var fungibleVaultsHistory = fungibleVaultChanges
                .Select(e =>
                {
                    vaultAggregateDelta.GetOrAdd(e.ReferencedVault.DatabaseOwnerAncestorId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendFungible(e.ReferencedResource.DatabaseId);
                    vaultAggregateDelta.GetOrAdd(e.ReferencedVault.DatabaseGlobalAncestorId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendFungible(e.ReferencedResource.DatabaseId);

                    return new EntityFungibleResourceHistory
                    {
                        Id = sequences.NextEntityResourceHistory,
                        FromStateVersion = e.StateVersion,
                        OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.ReferencedResource.DatabaseId,
                        Balance = e.Balance,
                    };
                })
                .ToList();

            var nonFungibleVaultsHistory = nonFungibleVaultChanges
                .Select(e =>
                {
                    // TODO handle removal (is_deleted)

                    vaultAggregateDelta.GetOrAdd(e.ReferencedVault.DatabaseOwnerAncestorId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendNonFungible(e.ReferencedResource.DatabaseId);
                    vaultAggregateDelta.GetOrAdd(e.ReferencedVault.DatabaseGlobalAncestorId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendNonFungible(e.ReferencedResource.DatabaseId);

                    return new EntityNonFungibleResourceHistory
                    {
                        Id = sequences.NextEntityResourceHistory,
                        FromStateVersion = e.StateVersion,
                        OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.ReferencedResource.DatabaseId,
                        NonFungibleIdsCount = e.NonFungibleIds.Count,
                        NonFungibleIds = e.NonFungibleIds.ToArray(),
                    };
                })
                .ToList();

            var metadataHistory = metadataChanges
                .Select(e =>
                {
                    var keys = new List<string>();
                    var values = new List<string>();

                    foreach (var (key, value) in e.Metadata)
                    {
                        keys.Add(key);
                        values.Add(value);
                    }

                    return new EntityMetadataHistory
                    {
                        Id = sequences.NextEntityMetadataHistory,
                        FromStateVersion = e.StateVersion,
                        EntityId = e.ResourceEntity.DatabaseId,
                        Keys = keys.ToArray(),
                        Values = values.ToArray(),
                    };
                })
                .ToList();

            var fungibleSuppliesHistory = fungibleResourceSupplyChanges
                .Select(e => new FungibleResourceSupplyHistory
                {
                    Id = sequences.NextFungibleResourceSupplyHistory,
                    FromStateVersion = e.StateVersion,
                    ResourceEntityId = e.ResourceEntity.DatabaseId,
                    TotalSupply = e.TotalSupply,
                    TotalMinted = e.TotalMinted,
                    TotalBurnt = e.TotalBurnt,
                })
                .ToList();

            var sw = Stopwatch.StartNew();

            var vaultAggregateDeltaIds = vaultAggregateDelta.Keys.ToList();
            var existingVaultAggregates = await dbContext.EntityResourceAggregateHistory
                .AsNoTracking()
                .Where(e => e.IsMostRecent)
                .Where(e => vaultAggregateDeltaIds.Contains(e.EntityId))
                .ToDictionaryAsync(e => e.EntityId, token);

            // TODO is it guaranteed that given NonFungibleStore has always proper NF ResourceManager as its global ancestor?
            var xxx = nonFungibleIdStoreChanges.Select(x => x.ReferencedStore.DatabaseGlobalAncestorId).Distinct().ToList();
            var mostRecentNonFungibleStore = await dbContext.NonFungibleIdStoreHistory
                .FromSqlInterpolated(@$"
WITH entities (id) AS (
    SELECT UNNEST({xxx})
)
SELECT emh.*
FROM entities
INNER JOIN LATERAL (
    SELECT *
    FROM non_fungible_id_store_history
    WHERE non_fungible_resource_manager_entity_id = entities.id
    ORDER BY from_state_version DESC
    LIMIT 1
) emh ON true;")
                .AsNoTracking()
                .ToDictionaryAsync(e => e.NonFungibleResourceManagerEntityId, token);

            dbReadDuration += sw.Elapsed;

            var nonFungibleIdStoreHistoryToAdd = new Dictionary<NonFungibleStoreLookup, NonFungibleIdStoreHistory>();
            var nonFungibleIdDataToAdd = new List<NonFungibleIdData>();
            var nonFungibleIdsMutableDataHistoryToAdd = new List<NonFungibleIdMutableDataHistory>();

            foreach (var e in nonFungibleIdStoreChanges)
            {
                // TODO we must find all existing NonFungibleIdData by ResourceManagerId+NfId so that for we can handle situation where NFID is not created but its mutable data has changed
                // TODO only add NonFungibleIdData if does not exist, otherwise use existing one

                var nfidData = new NonFungibleIdData
                {
                    Id = sequences.NextNonFungibleIdData,
                    FromStateVersion = e.StateVersion,
                    NonFungibleStoreEntityId = e.ReferencedStore.DatabaseId,
                    // TODO is it guaranteed to be ResourceManager?
                    // TODO or maybe we should use OwnerAncestorId?
                    // TODO or maybe we should iterate until we find entity of appropriate type?
                    NonFungibleResourceManagerEntityId = e.ReferencedStore.DatabaseGlobalAncestorId,
                    NonFungibleId = e.NonFungibleId,
                    ImmutableData = e.Data?.ImmutableDataRawHex.StructData.DataBytes ?? Array.Empty<byte>(),
                };

                nonFungibleIdDataToAdd.Add(nfidData);
                nonFungibleIdsMutableDataHistoryToAdd.Add(new NonFungibleIdMutableDataHistory
                {
                    Id = sequences.NextNonFungibleIdMutableDataHistory,
                    FromStateVersion = e.StateVersion,
                    NonFungibleIdDataId = nfidData.Id,
                    IsDeleted = e.IsDeleted,
                    MutableData = e.Data?.MutableDataRawHex.StructData.DataBytes ?? Array.Empty<byte>(),
                });

                var store = nonFungibleIdStoreHistoryToAdd.GetOrAdd(new NonFungibleStoreLookup(e.ReferencedStore.DatabaseGlobalAncestorId, e.StateVersion), _ =>
                {
                    if (mostRecentNonFungibleStore.ContainsKey(e.ReferencedStore.DatabaseGlobalAncestorId))
                    {
                        return mostRecentNonFungibleStore[e.ReferencedStore.DatabaseGlobalAncestorId];
                    }

                    var ret = new NonFungibleIdStoreHistory
                    {
                        Id = sequences.NextNonFungibleIdStoreHistory,
                        FromStateVersion = e.StateVersion,
                        NonFungibleStoreEntityId = e.ReferencedStore.DatabaseId,
                        NonFungibleResourceManagerEntityId = e.ReferencedStore.DatabaseGlobalAncestorId,
                        NonFungibleIdDataIds = Array.Empty<long>(),
                    };

                    mostRecentNonFungibleStore[e.ReferencedStore.DatabaseGlobalAncestorId] = ret;

                    return ret;
                });

                store.NonFungibleIdDataIds = store.NonFungibleIdDataIds.Concat(new[] { nfidData.Id }).ToArray(); // TODO super ineffective
            }

            var aggregates = new List<EntityResourceAggregateHistory>();
            var lastAggregateByEntity = new Dictionary<long, EntityResourceAggregateHistory>();

            foreach (var (entityId, aggregateChange) in vaultAggregateDelta)
            {
                if (existingVaultAggregates.ContainsKey(entityId))
                {
                    var existingAggregate = existingVaultAggregates[entityId];

                    aggregateChange.Add(existingAggregate.FromStateVersion, new AggregateChange(existingAggregate.FromStateVersion, existingAggregate.FungibleResourceEntityIds, existingAggregate.NonFungibleResourceEntityIds));
                }

                var orderedStateVersions = aggregateChange.Keys.OrderBy(k => k).ToArray();

                for (var i = 0; i < orderedStateVersions.Length; i++)
                {
                    var current = aggregateChange[orderedStateVersions[i]];
                    var previous = i > 0 ? aggregateChange[orderedStateVersions[i - 1]] : null;

                    current.Apply(previous);

                    if (current.ShouldBePersisted(previous))
                    {
                        var dbAggregate = new EntityResourceAggregateHistory
                        {
                            Id = sequences.NextEntityResourceAggregateHistory,
                            EntityId = entityId,
                            FromStateVersion = orderedStateVersions[i],
                            FungibleResourceEntityIds = current.FungibleIds.ToArray(),
                            NonFungibleResourceEntityIds = current.NonFungibleIds.ToArray(),
                        };

                        aggregates.Add(dbAggregate);
                        lastAggregateByEntity[entityId] = dbAggregate;
                    }
                }
            }

            foreach (var aggregate in lastAggregateByEntity.Values)
            {
                aggregate.IsMostRecent = true;
            }

            sw = Stopwatch.StartNew();

            var entityIds = aggregates.Select(a => a.EntityId).Distinct().ToList();
            var entityIdsParameter = new NpgsqlParameter("@entity_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = entityIds };

            var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE entity_resource_aggregate_history SET is_most_recent = false WHERE is_most_recent = true AND entity_id = ANY({entityIdsParameter})",
                token);

            rowsUpdated += affected;

            if (aggregates.Any())
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY entity_resource_aggregate_history (id, from_state_version, entity_id, is_most_recent, fungible_resource_entity_ids, non_fungible_resource_entity_ids) FROM STDIN (FORMAT BINARY)", token);

                foreach (var aggregate in aggregates)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(aggregate.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.IsMostRecent, NpgsqlDbType.Boolean, token);
                    await writer.WriteAsync(aggregate.FungibleResourceEntityIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.NonFungibleResourceEntityIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                }

                await writer.CompleteAsync(token);
            }

            if (fungibleVaultsHistory.Any() || nonFungibleVaultsHistory.Any())
            {
                if (dbContext.Model.FindEntityType(typeof(EntityFungibleResourceHistory))?.GetDiscriminatorValue() is not string fungibleDiscriminator)
                {
                    throw new InvalidOperationException("Unable to determine discriminator of EntityFungibleResourceHistory");
                }

                if (dbContext.Model.FindEntityType(typeof(EntityNonFungibleResourceHistory))?.GetDiscriminatorValue() is not string nonFungibleDiscriminator)
                {
                    throw new InvalidOperationException("Unable to determine discriminator of EntityNonFungibleResourceHistory");
                }

                await using var writer = await dbConn.BeginBinaryImportAsync("COPY entity_resource_history (id, from_state_version, owner_entity_id, global_entity_id, resource_entity_id, discriminator, balance, non_fungible_ids_count, non_fungible_ids) FROM STDIN (FORMAT BINARY)", token);

                foreach (var fungible in fungibleVaultsHistory)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(fungible.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.OwnerEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.GlobalEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungibleDiscriminator, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(fungible.Balance.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                }

                foreach (var nonFungible in nonFungibleVaultsHistory)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(nonFungible.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.OwnerEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.GlobalEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungibleDiscriminator, NpgsqlDbType.Text, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(nonFungible.NonFungibleIdsCount, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.NonFungibleIds, NpgsqlDbType.Array | NpgsqlDbType.Bytea, token);
                }

                await writer.CompleteAsync(token);
            }

            if (metadataHistory.Any())
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY entity_metadata_history (id, from_state_version, entity_id, keys, values) FROM STDIN (FORMAT BINARY)", token);

                foreach (var md in metadataHistory)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(md.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.Keys, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
                    await writer.WriteAsync(md.Values, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
                }

                await writer.CompleteAsync(token);
            }

            if (fungibleSuppliesHistory.Any())
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY fungible_resource_supply_history (id, from_state_version, resource_entity_id, total_supply, total_minted, total_burnt) FROM STDIN (FORMAT BINARY)", token);

                foreach (var fs in fungibleSuppliesHistory)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(fs.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fs.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fs.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fs.TotalSupply.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteAsync(fs.TotalMinted.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteAsync(fs.TotalBurnt.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                }

                await writer.CompleteAsync(token);
            }

            if (nonFungibleIdDataToAdd.Any())
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY non_fungible_id_data (id, from_state_version, non_fungible_store_entity_id, non_fungible_resource_manager_entity_id, non_fungible_id, immutable_data) FROM STDIN (FORMAT BINARY)", token);

                foreach (var md in nonFungibleIdDataToAdd)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(md.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.NonFungibleStoreEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.NonFungibleResourceManagerEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.NonFungibleId, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(md.ImmutableData, NpgsqlDbType.Bytea, token);
                }

                await writer.CompleteAsync(token);
            }

            if (nonFungibleIdsMutableDataHistoryToAdd.Any())
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY non_fungible_id_mutable_data_history (id, from_state_version, non_fungible_id_data_id, is_deleted, mutable_data) FROM STDIN (FORMAT BINARY)", token);

                foreach (var md in nonFungibleIdsMutableDataHistoryToAdd)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(md.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.NonFungibleIdDataId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.IsDeleted, NpgsqlDbType.Boolean, token);
                    await writer.WriteAsync(md.MutableData, NpgsqlDbType.Bytea, token);
                }

                await writer.CompleteAsync(token);
            }

            if (nonFungibleIdStoreHistoryToAdd.Any())
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY non_fungible_id_store_history (id, from_state_version, non_fungible_store_entity_id, non_fungible_resource_manager_entity_id, non_fungible_id_data_ids) FROM STDIN (FORMAT BINARY)", token);

                foreach (var md in nonFungibleIdStoreHistoryToAdd.Values)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(md.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.NonFungibleStoreEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.NonFungibleResourceManagerEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.NonFungibleIdDataIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                }

                await writer.CompleteAsync(token);
            }

            rowsInserted += aggregates.Count + fungibleVaultsHistory.Count + nonFungibleVaultsHistory.Count + metadataHistory.Count + nonFungibleIdDataToAdd.Count + nonFungibleIdsMutableDataHistoryToAdd.Count;
            dbWriteDuration += sw.Elapsed;
        }

        // step: update sequences
        {
            var sequencesUpdateDuration = await CodeStopwatch.Time(() => sequences.Update(dbConn, token));

            dbWriteDuration += sequencesUpdateDuration;
        }

        var contentHandlingDuration = outerStopwatch.Elapsed - dbReadDuration - dbWriteDuration;
        var processReport = new ProcessTransactionsReport(rowsInserted + rowsUpdated, dbReadDuration, dbWriteDuration, contentHandlingDuration);

        return new ProcessTransactionsResult(lastTransactionSummary, preparationReport, processReport);
    }

    private async Task CreateOrUpdateLedgerStatus(
        ReadWriteDbContext dbContext,
        TransactionSummary finalTransaction,
        long latestSyncTarget,
        CancellationToken token
    )
    {
        var ledgerStatus = await dbContext.LedgerStatus.SingleOrDefaultAsync(token);

        if (ledgerStatus == null)
        {
            ledgerStatus = new LedgerStatus();
            dbContext.Add(ledgerStatus);
        }

        ledgerStatus.LastUpdated = _clock.UtcNow;
        ledgerStatus.TopOfLedgerStateVersion = finalTransaction.StateVersion;
        ledgerStatus.TargetStateVersion = latestSyncTarget;
    }

    private async Task<TransactionSummary> GetTopOfLedger(ReadWriteDbContext dbContext, CancellationToken token)
    {
        var lastTransaction = await dbContext.LedgerTransactions
            .AsNoTracking()
            .OrderByDescending(lt => lt.StateVersion)
            .FirstOrDefaultAsync(token);

        var lastOverview = lastTransaction == null ? null : new TransactionSummary(
            StateVersion: lastTransaction.StateVersion,
            RoundTimestamp: lastTransaction.RoundTimestamp,
            NormalizedRoundTimestamp: lastTransaction.NormalizedRoundTimestamp,
            CreatedTimestamp: lastTransaction.CreatedTimestamp,
            Epoch: lastTransaction.Epoch,
            IndexInEpoch: lastTransaction.IndexInEpoch,
            RoundInEpoch: lastTransaction.RoundInEpoch,
            IsStartOfEpoch: lastTransaction.IsStartOfEpoch,
            IsStartOfRound: lastTransaction.IsStartOfRound,
            TransactionAccumulator: lastTransaction.TransactionAccumulator
        );

        return lastOverview ?? PreGenesisTransactionSummary();
    }

    private TransactionSummary PreGenesisTransactionSummary()
    {
        // Nearly all of theses turn out to be unused!
        return new TransactionSummary(
            StateVersion: 0,
            RoundTimestamp: DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime,
            NormalizedRoundTimestamp: DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime,
            CreatedTimestamp: _clock.UtcNow,
            Epoch: 0,
            IndexInEpoch: 0,
            RoundInEpoch: 0,
            IsStartOfEpoch: false,
            IsStartOfRound: false,
            TransactionAccumulator: new byte[32]
        );
    }
}
