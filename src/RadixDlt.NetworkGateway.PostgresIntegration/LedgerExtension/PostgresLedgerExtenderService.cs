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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.DataAggregator;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
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
        // - replace with proper Activity at some point to eliminate stopwatches and primitive counters
        // - when it comes to fungibles and nonFungibles and their respective xxxResourceHistory we should change that to VaultHistory

        var rowsInserted = 0;
        var dbReadDuration = TimeSpan.Zero;
        var dbWriteDuration = TimeSpan.Zero;
        var outerStopwatch = Stopwatch.StartNew();
        var referencedEntities = new ReferencedEntityDictionary();
        var knownGlobalAddressesToLoad = new HashSet<string>();
        var childToParentEntities = new Dictionary<string, string>();
        var componentToGlobalPackage = new Dictionary<string, (string PackageAddress, string BlueprintName)>();
        var fungibleResourceManagerDivisibility = new Dictionary<string, int>();
        var nonFungibleResourceManagerNonFungibleIdType = new Dictionary<string, NonFungibleIdType>();
        var packageCode = new Dictionary<string, byte[]>();

        var ledgerTransactionsToAdd = new List<LedgerTransaction>();

        var lastTransactionSummary = ledgerExtension.LatestTransactionSummary;
        var readHelper = new ReadHelper(dbContext);
        var writeHelper = new WriteHelper(dbContext);

        PreparationReport preparationReport;
        SequencesHolder sequences;

        // step: preparation (previously RawTransactionWriter logic)
        {
            var rawUserTransactionStatusByPayloadHash = new Dictionary<byte[], CoreModel.TransactionStatus>(ByteArrayEqualityComparer.Default);
            var rawUserTransactionByPayloadHash = new Dictionary<byte[], RawUserTransaction>(ByteArrayEqualityComparer.Default);

            foreach (var ct in ledgerExtension.CommittedTransactions.Where(ct => ct.LedgerTransaction.ActualInstance is CoreModel.UserLedgerTransaction))
            {
                var nt = ct.LedgerTransaction.GetUserLedgerTransaction().NotarizedTransaction;

                rawUserTransactionStatusByPayloadHash[nt.GetHashBytes()] = ct.Receipt.Status;
                rawUserTransactionByPayloadHash[nt.GetHashBytes()] = new RawUserTransaction
                {
                    StateVersion = ct.StateVersion,
                    PayloadHash = nt.GetHashBytes(),
                    Payload = nt.GetPayloadBytes(),
                    Receipt = ct.Receipt.ToJson(),
                };
            }

            var rawUserTransactionsToAdd = rawUserTransactionByPayloadHash.Values.ToList();

            var (rawTransactionsTouched, rawTransactionCommitMs) = await CodeStopwatch.TimeInMs(async () =>
            {
                rowsInserted += await writeHelper.CopyRawUserTransaction(rawUserTransactionsToAdd, token);

                return rawUserTransactionsToAdd.Count;
            });

            var (pendingTransactionsTouched, pendingTransactionUpdateMs) = await CodeStopwatch.TimeInMs(async () =>
            {
                var transactionPayloadHashList = rawUserTransactionByPayloadHash.Keys.ToList(); // List<> are optimised for PostgreSQL lookups

                var toUpdate = await dbContext.PendingTransactions
                    .Where(pt => pt.Status != PendingTransactionStatus.CommittedSuccess && pt.Status != PendingTransactionStatus.CommittedFailure)
                    .Where(pt => transactionPayloadHashList.Contains(pt.PayloadHash))
                    .ToListAsync(token);

                if (toUpdate.Count == 0)
                {
                    return 0;
                }

                foreach (var pendingTransaction in toUpdate)
                {
                    if (pendingTransaction.Status is PendingTransactionStatus.RejectedPermanently or PendingTransactionStatus.RejectedTemporarily)
                    {
                        await _observers.ForEachAsync(x => x.TransactionsMarkedCommittedWhichWasFailed());

                        _logger.LogError(
                            "Transaction with payload hash {PayloadHash} which was first/last submitted to Gateway at {FirstGatewaySubmissionTime}/{LastGatewaySubmissionTime} and last marked missing from mempool at {LastMissingFromMempoolTimestamp} was mark {FailureTransiency} at {FailureTime} due to \"{FailureReason}\" but has now been marked committed",
                            pendingTransaction.PayloadHash.ToHex(),
                            pendingTransaction.FirstSubmittedToGatewayTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                            pendingTransaction.LastSubmittedToGatewayTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                            pendingTransaction.LastDroppedOutOfMempoolTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                            pendingTransaction.Status,
                            pendingTransaction.FailureTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                            pendingTransaction.FailureReason
                        );
                    }

                    switch (rawUserTransactionStatusByPayloadHash[pendingTransaction.PayloadHash])
                    {
                        case CoreModel.TransactionStatus.Succeeded:
                            pendingTransaction.MarkAsCommitted(true, _clock.UtcNow);
                            break;
                        case CoreModel.TransactionStatus.Failed:
                            pendingTransaction.MarkAsCommitted(false, _clock.UtcNow);
                            break;
                        default:
                            throw new UnreachableException();
                    }
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
            (sequences, var sequencesInitializeDuration) = await CodeStopwatch.Time(() => readHelper.LoadSequences(token));

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
                        case CoreModel.TimeUpdateValidatorTransaction timeUpdate:
                            newEpoch = timeUpdate.ConsensusEpoch;
                            newRoundInEpoch = timeUpdate.RoundInEpoch;
                            newRoundTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(timeUpdate.ProposerTimestampMs).UtcDateTime;
                            break;
                    }
                }

                if (ct.LedgerTransaction.ActualInstance is CoreModel.SystemLedgerTransaction)
                {
                    // no-op so far
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
                            te.WithTypeHint(target.GlobalAddress.StartsWith(hrp.AccountComponent) ? typeof(AccountComponentEntity) : typeof(NormalComponentEntity));
                        }

                        // we do not want to store GlobalEntities as they bring no value from NG perspective
                        // GlobalAddress is essentially a property of other entities

                        continue;
                    }

                    var re = referencedEntities.GetOrAdd(sid.EntityIdHex, _ => new ReferencedEntity(sid.EntityIdHex, sid.EntityType, stateVersion));

                    if (sd is CoreModel.IOwner owner)
                    {
                        foreach (var oe in owner.GetOwnedEntities())
                        {
                            referencedEntities.GetOrAdd(oe.EntityIdHex, _ => new ReferencedEntity(oe.EntityIdHex, oe.EntityType, stateVersion)).IsImmediateChildOf(re);

                            childToParentEntities[oe.EntityIdHex] = sid.EntityIdHex;
                        }
                    }

                    if (sd is CoreModel.IGlobalResourcePointer globalResourcePointer)
                    {
                        foreach (var pointer in globalResourcePointer.GetPointers())
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

                    if (sd is CoreModel.ComponentInfoSubstate componentInfo)
                    {
                        knownGlobalAddressesToLoad.Add(componentInfo.PackageAddress);
                        componentToGlobalPackage[sid.EntityIdHex] = (componentInfo.PackageAddress, componentInfo.BlueprintName);
                    }

                    if (sd is CoreModel.PackageInfoSubstate packageInfo)
                    {
                        packageCode[sid.EntityIdHex] = packageInfo.GetCodeBytes();
                    }

                    if (sd is CoreModel.ValidatorSetSubstate validatorSet)
                    {
                        // TODO this is known to be buggy as it is NEXT transaction that should be marked as beginning of the new epoch
                        newEpoch = validatorSet.Epoch;
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
                    TransactionAccumulator: ct.LedgerTransaction.GetPayloadBytes());

                LedgerTransaction ledgerTransaction = ct.LedgerTransaction.ActualInstance switch
                {
                    CoreModel.UserLedgerTransaction ult => new UserLedgerTransaction
                    {
                        PayloadHash = ult.NotarizedTransaction.GetHashBytes(),
                        IntentHash = ult.NotarizedTransaction.SignedIntent.Intent.GetHashBytes(),
                        SignedIntentHash = ult.NotarizedTransaction.SignedIntent.GetHashBytes(),
                    },
                    CoreModel.ValidatorLedgerTransaction => new ValidatorLedgerTransaction(),
                    CoreModel.SystemLedgerTransaction => new SystemLedgerTransaction(),
                    _ => throw new ArgumentOutOfRangeException(nameof(ct.LedgerTransaction), ct.LedgerTransaction, null),
                };

                ledgerTransaction.StateVersion = ct.StateVersion;
                ledgerTransaction.Status = ct.Receipt.Status.ToModel();
                ledgerTransaction.ErrorMessage = ct.Receipt.ErrorMessage;
                ledgerTransaction.TransactionAccumulator = ct.GetAccumulatorHashBytes();
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

                ledgerTransactionsToAdd.Add(ledgerTransaction);

                lastTransactionSummary = summary;
            }
        }

        // step: resolve known types & optionally create missing entities
        {
            var sw = Stopwatch.StartNew();

            var knownDbEntities = await readHelper.ExistingEntitiesFor(referencedEntities, knownGlobalAddressesToLoad, token);

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

            var entitiesToAdd = new List<Entity>();

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
                    // If the component is a local / owned component, it doesn't have a Component/Account type hint
                    // from the address, so assume it's a normal component for now until we can do better from the ComponentInfo
                    CoreModel.EntityType.Component => re.CreateUsingTypeHintOrDefault<ComponentEntity>(typeof(NormalComponentEntity)),
                    CoreModel.EntityType.Package => new PackageEntity(),
                    CoreModel.EntityType.Vault => new VaultEntity(),
                    CoreModel.EntityType.KeyValueStore => new KeyValueStoreEntity(),
                    CoreModel.EntityType.Global => throw new ArgumentOutOfRangeException(nameof(re.Type), re.Type, "Global entities should be filtered out"),
                    CoreModel.EntityType.NonFungibleStore => new NonFungibleStoreEntity(),
                    CoreModel.EntityType.Clock => new ClockEntity(),
                    _ => throw new ArgumentOutOfRangeException(nameof(re.Type), re.Type, null),
                };

                dbEntity.Id = sequences.EntitySequence++;
                dbEntity.FromStateVersion = re.StateVersion;
                dbEntity.Address = re.IdHex.ConvertFromHex();
                dbEntity.GlobalAddress = re.GlobalAddressHex == null ? null : (RadixAddress)re.GlobalAddressHex.ConvertFromHex();

                re.Resolve(dbEntity);
                entitiesToAdd.Add(dbEntity);
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
                    dbe.AncestorIds = allAncestors;
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

            rowsInserted += await writeHelper.CopyEntity(entitiesToAdd, token);
            rowsInserted += await writeHelper.CopyLedgerTransaction(ledgerTransactionsToAdd, referencedEntities, token);

            dbWriteDuration += sw.Elapsed;
        }

        var fungibleVaultChanges = new List<FungibleVaultChange>();
        var nonFungibleVaultChanges = new List<NonFungibleVaultChange>();
        var nonFungibleIdStoreChanges = new List<NonFungibleIdChange>();
        var metadataChanges = new List<MetadataChange>();
        var resourceManagerSupplyChanges = new List<ResourceManagerSupplyChange>();
        var entityAccessRulesChainHistoryToAdd = new List<EntityAccessRulesChainHistory>();
        var componentEntityStateToAdd = new List<ComponentEntityStateHistory>();

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
                        resourceManagerSupplyChanges.Add(new ResourceManagerSupplyChange(re, TokenAmount.FromDecimalString(resourceManager.TotalSupply), stateVersion));
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
                        var resourceManagerEntity = referencedEntities.GetByDatabaseId(re.DatabaseGlobalAncestorId);

                        nonFungibleIdStoreChanges.Add(new NonFungibleIdChange(re, resourceManagerEntity, nonFungibleStoreEntry.NonFungibleId.SimpleRep, nonFungibleStoreEntry.IsDeleted, nonFungibleStoreEntry.NonFungibleData, stateVersion));
                    }

                    if (sd is CoreModel.ComponentStateSubstate componentState)
                    {
                        componentEntityStateToAdd.Add(new ComponentEntityStateHistory
                        {
                            Id = sequences.ComponentEntityStateHistorySequence++,
                            FromStateVersion = stateVersion,
                            ComponentEntityId = referencedEntities.Get(sid.EntityIdHex).DatabaseId,
                            State = componentState.DataStruct.StructData.ToJson(),
                        });
                    }

                    if (sd is CoreModel.AccessRulesChainSubstate accessRulesChain)
                    {
                        AccessRulesChainSubtype subtype = sid.SubstateKeyType switch
                        {
                            CoreModel.SubstateKeyType.AccessRulesChain => AccessRulesChainSubtype.None,
                            CoreModel.SubstateKeyType.ResourceManagerVaultAccessRulesChain => AccessRulesChainSubtype.ResourceManagerVaultAccessRulesChain,
                            _ => throw new UnreachableException(),
                        };

                        entityAccessRulesChainHistoryToAdd.Add(new EntityAccessRulesChainHistory
                        {
                            Id = sequences.EntityAccessRulesChainHistorySequence++,
                            FromStateVersion = stateVersion,
                            EntityId = referencedEntities.Get(sid.EntityIdHex).DatabaseId,
                            Subtype = subtype,
                            AccessRulesChain = JsonConvert.SerializeObject(accessRulesChain.Chain),
                        });
                    }
                }
            }
        }

        // step: now that all the fundamental data is inserted (entities & substates) we can insert some denormalized data
        {
            var sw = Stopwatch.StartNew();

            var mostRecentEntityResourceAggregateHistory = await readHelper.MostRecentEntityResourceAggregateHistoryFor(fungibleVaultChanges, nonFungibleVaultChanges, token);
            var mostRecentNonFungibleIdStoreHistory = await readHelper.MostRecentNonFungibleIdStoreHistoryFor(nonFungibleIdStoreChanges, token);
            var mostRecentResourceManagerEntitySupplyHistory = await readHelper.MostRecentResourceManagerEntitySupplyHistoryFor(resourceManagerSupplyChanges, token);
            var existingNonFungibleIdData = await readHelper.ExistingNonFungibleIdDataFor(nonFungibleIdStoreChanges, nonFungibleVaultChanges, token);

            dbReadDuration += sw.Elapsed;

            var entityResourceAggregateHistoryToAdd = new List<EntityResourceAggregateHistory>();
            var nonFungibleIdStoreHistoryToAdd = new Dictionary<NonFungibleStoreLookup, NonFungibleIdStoreHistory>();
            var nonFungibleIdDataToAdd = new List<NonFungibleIdData>();
            var nonFungibleIdsMutableDataHistoryToAdd = new List<NonFungibleIdMutableDataHistory>();

            void AggregateEntityResource(long entityId, long resourceEntityId, long stateVersion, Func<EntityResourceAggregateHistory, List<long>> collectionSelector)
            {
                var existingAggregate = mostRecentEntityResourceAggregateHistory.GetOrAdd(entityId, _ =>
                {
                    var ret = new EntityResourceAggregateHistory
                    {
                        Id = sequences.EntityResourceAggregateHistorySequence++,
                        FromStateVersion = stateVersion,
                        EntityId = entityId,
                        FungibleResourceEntityIds = new List<long>(),
                        NonFungibleResourceEntityIds = new List<long>(),
                    };

                    entityResourceAggregateHistoryToAdd.Add(ret);

                    return ret;
                });

                var existingCollection = collectionSelector(existingAggregate);

                if (existingCollection.Contains(resourceEntityId))
                {
                    return;
                }

                var newAggregate = new EntityResourceAggregateHistory
                {
                    Id = sequences.EntityResourceAggregateHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = entityId,
                    FungibleResourceEntityIds = new List<long>(existingAggregate.FungibleResourceEntityIds),
                    NonFungibleResourceEntityIds = new List<long>(existingAggregate.NonFungibleResourceEntityIds),
                };

                var newCollection = collectionSelector(newAggregate);

                newCollection.Add(resourceEntityId);

                entityResourceAggregateHistoryToAdd.Add(newAggregate);
                mostRecentEntityResourceAggregateHistory[entityId] = newAggregate;
            }

            var entityFungibleResourceHistoryToAdd = fungibleVaultChanges
                .Select(e =>
                {
                    AggregateEntityResource(e.ReferencedVault.DatabaseOwnerAncestorId, e.ReferencedResource.DatabaseId, e.StateVersion, x => x.FungibleResourceEntityIds);
                    AggregateEntityResource(e.ReferencedVault.DatabaseGlobalAncestorId, e.ReferencedResource.DatabaseId, e.StateVersion, x => x.FungibleResourceEntityIds);

                    return new EntityFungibleResourceHistory
                    {
                        Id = sequences.EntityResourceHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.ReferencedResource.DatabaseId,
                        Balance = e.Balance,
                    };
                })
                .ToList();

            var entityMetadataHistoryToAdd = metadataChanges
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
                        Id = sequences.EntityMetadataHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        EntityId = e.ResourceEntity.DatabaseId,
                        Keys = keys,
                        Values = values,
                    };
                })
                .ToList();

            foreach (var e in nonFungibleIdStoreChanges)
            {
                var nonFungibleIdData = existingNonFungibleIdData.GetOrAdd(new NonFungibleIdLookup(e.ReferencedResource.DatabaseId, e.NonFungibleId), _ =>
                {
                    var ret = new NonFungibleIdData
                    {
                        Id = sequences.NonFungibleIdDataSequence++,
                        FromStateVersion = e.StateVersion,
                        NonFungibleStoreEntityId = e.ReferencedStore.DatabaseId,
                        NonFungibleResourceManagerEntityId = e.ReferencedResource.DatabaseId,
                        NonFungibleId = e.NonFungibleId,
                        ImmutableData = e.Data?.GetImmutableDataRawBytes() ?? Array.Empty<byte>(),
                    };

                    nonFungibleIdDataToAdd.Add(ret);

                    return ret;
                });
                var nonFungibleIdStore = nonFungibleIdStoreHistoryToAdd.GetOrAdd(new NonFungibleStoreLookup(e.ReferencedStore.DatabaseGlobalAncestorId, e.StateVersion), _ =>
                {
                    IEnumerable<long> previousNonFungibleIdDataIds = mostRecentNonFungibleIdStoreHistory.ContainsKey(e.ReferencedStore.DatabaseGlobalAncestorId)
                        ? mostRecentNonFungibleIdStoreHistory[e.ReferencedStore.DatabaseGlobalAncestorId].NonFungibleIdDataIds
                        : Array.Empty<long>();

                    var ret = new NonFungibleIdStoreHistory
                    {
                        Id = sequences.NonFungibleIdStoreHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        NonFungibleStoreEntityId = e.ReferencedStore.DatabaseId,
                        NonFungibleResourceManagerEntityId = e.ReferencedStore.DatabaseGlobalAncestorId,
                        NonFungibleIdDataIds = new List<long>(previousNonFungibleIdDataIds),
                    };

                    mostRecentNonFungibleIdStoreHistory[e.ReferencedStore.DatabaseGlobalAncestorId] = ret;

                    return ret;
                });

                nonFungibleIdsMutableDataHistoryToAdd.Add(new NonFungibleIdMutableDataHistory
                {
                    Id = sequences.NonFungibleIdMutableDataHistorySequence++,
                    FromStateVersion = e.StateVersion,
                    NonFungibleIdDataId = nonFungibleIdData.Id,
                    IsDeleted = e.IsDeleted,
                    MutableData = e.Data?.GetMutableDataRawBytes() ?? Array.Empty<byte>(),
                });

                if (!nonFungibleIdStore.NonFungibleIdDataIds.Contains(nonFungibleIdData.Id))
                {
                    nonFungibleIdStore.NonFungibleIdDataIds.Add(nonFungibleIdData.Id);
                }
            }

            var resourceManagerEntitySupplyHistoryToAdd = resourceManagerSupplyChanges
                .Select(e =>
                {
                    var previous = mostRecentResourceManagerEntitySupplyHistory.GetOrAdd(e.ResourceEntity.DatabaseId, _ => new ResourceManagerEntitySupplyHistory
                    {
                        TotalSupply = TokenAmount.Zero,
                        TotalMinted = TokenAmount.Zero,
                        TotalBurnt = TokenAmount.Zero,
                    });

                    TokenAmount totalMinted = previous.TotalMinted;
                    TokenAmount totalBurnt = previous.TotalBurnt;

                    if (previous.TotalSupply < e.TotalSupply)
                    {
                        totalMinted += e.TotalSupply - previous.TotalSupply;
                    }
                    else if (previous.TotalSupply > e.TotalSupply)
                    {
                        totalBurnt += previous.TotalSupply - e.TotalSupply;
                    }

                    var entry = new ResourceManagerEntitySupplyHistory
                    {
                        Id = sequences.ResourceManagerEntitySupplyHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        ResourceManagerEntityId = e.ResourceEntity.DatabaseId,
                        TotalSupply = e.TotalSupply,
                        TotalMinted = totalMinted,
                        TotalBurnt = totalBurnt,
                    };

                    mostRecentResourceManagerEntitySupplyHistory[e.ResourceEntity.DatabaseId] = entry;

                    return entry;
                })
                .ToList();

            var nonFungibleVaultsHistoryToAdd = nonFungibleVaultChanges
                .Select(e =>
                {
                    AggregateEntityResource(e.ReferencedVault.DatabaseOwnerAncestorId, e.ReferencedResource.DatabaseId, e.StateVersion, x => x.NonFungibleResourceEntityIds);
                    AggregateEntityResource(e.ReferencedVault.DatabaseGlobalAncestorId, e.ReferencedResource.DatabaseId, e.StateVersion, x => x.NonFungibleResourceEntityIds);

                    return new EntityNonFungibleResourceHistory
                    {
                        Id = sequences.EntityResourceHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.ReferencedResource.DatabaseId,
                        NonFungibleIds = e.NonFungibleIds.Select(nfid => existingNonFungibleIdData[new NonFungibleIdLookup(e.ReferencedResource.DatabaseId, nfid)].Id).ToList(),
                    };
                })
                .ToList();

            sw = Stopwatch.StartNew();

            rowsInserted += await writeHelper.CopyComponentEntityStateHistory(componentEntityStateToAdd, token);
            rowsInserted += await writeHelper.CopyEntityAccessRulesChainHistory(entityAccessRulesChainHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityMetadataHistory(entityMetadataHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityResourceAggregateHistory(entityResourceAggregateHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityResourceHistory(entityFungibleResourceHistoryToAdd, nonFungibleVaultsHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleIdData(nonFungibleIdDataToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleIdMutableDataHistory(nonFungibleIdsMutableDataHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleIdStoreHistory(nonFungibleIdStoreHistoryToAdd.Values, token);
            rowsInserted += await writeHelper.CopyResourceManagerEntitySupplyHistory(resourceManagerEntitySupplyHistoryToAdd, token);
            await writeHelper.UpdateSequences(sequences, token);

            dbWriteDuration += sw.Elapsed;
        }

        var contentHandlingDuration = outerStopwatch.Elapsed - dbReadDuration - dbWriteDuration;
        var processReport = new ProcessTransactionsReport(rowsInserted, dbReadDuration, dbWriteDuration, contentHandlingDuration);

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
