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
        // TODO further improvements:
        // - queries with WHERE xxx = ANY(<list of 12345 ids>) are probably not very performant
        // - replace with proper Activity at some point to eliminate stopwatches and primitive counters
        // - when it comes to fungibles and nonFungibles and their respective xxxResourceHistory we should change that to VaultHistory
        // - avoid dbContextFactory - just make sure we use scoped services with single dbContext (UoW) passed through .ctor
        // - quite a few sequential database reads could be done in parallel once with ditch EF Core (dbContext)
        // - read helpers could immediately return empty collections on empty inputs
        // - ProcessTransactions should be divided into smaller methods invoked in chain

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

            var extendLedgerReport = await ProcessTransactions(dbContext, ledgerExtension, token);

            await CreateOrUpdateLedgerStatus(dbContext, extendLedgerReport.FinalTransaction, latestSyncTarget.TargetStateVersion, token);

            await dbContext.SaveChangesAsync(token);
            await tx.CommitAsync(token);

            var dbEntriesWritten =
                extendLedgerReport.PreparationReport.RawTxnUpsertTouchedRecords
                + extendLedgerReport.PreparationReport.PendingTransactionsTouchedRecords
                + extendLedgerReport.PreparationReport.PreparationEntriesTouched
                + extendLedgerReport.RowsInserted;

            return new CommitTransactionsReport(
                ledgerExtension.CommittedTransactions.Count,
                extendLedgerReport.FinalTransaction,
                extendLedgerReport.PreparationReport.RawTxnPersistenceMs,
                extendLedgerReport.PreparationReport.PendingTransactionUpdateMs,
                (long)extendLedgerReport.ContentHandlingDuration.TotalMilliseconds,
                (long)extendLedgerReport.DbReadDuration.TotalMilliseconds,
                (long)extendLedgerReport.DbWriteDuration.TotalMilliseconds,
                dbEntriesWritten
            );
        }
        catch (Exception)
        {
            await tx.RollbackAsync(token);

            throw;
        }
    }

    private record PreparationReport(
        long RawTxnPersistenceMs,
        int RawTxnUpsertTouchedRecords,
        long PendingTransactionUpdateMs,
        int PendingTransactionsTouchedRecords,
        int PreparationEntriesTouched
    );

    private record ExtendLedgerReport(
        TransactionSummary FinalTransaction,
        PreparationReport PreparationReport,
        int RowsInserted,
        TimeSpan DbReadDuration,
        TimeSpan DbWriteDuration,
        TimeSpan ContentHandlingDuration);

    private async Task<ExtendLedgerReport> ProcessTransactions(ReadWriteDbContext dbContext, ConsistentLedgerExtension ledgerExtension, CancellationToken token)
    {
        var rowsInserted = 0;
        var dbReadDuration = TimeSpan.Zero;
        var dbWriteDuration = TimeSpan.Zero;
        var outerStopwatch = Stopwatch.StartNew();
        var referencedEntities = new ReferencedEntityDictionary();
        var childToParentEntities = new Dictionary<string, string>();

        var readHelper = new ReadHelper(dbContext);
        var writeHelper = new WriteHelper(dbContext);

        var lastTransactionSummary = ledgerExtension.LatestTransactionSummary;

        var ledgerTransactionsToAdd = new List<LedgerTransaction>();
        var entitiesToAdd = new List<Entity>();

        PreparationReport preparationReport;
        SequencesHolder sequences;

        // step: preparation (previously RawTransactionWriter logic)
        {
            var rawUserTransactionStatusByPayloadHash = new Dictionary<ValueBytes, CoreModel.TransactionStatus>();
            var rawUserTransactionByPayloadHash = new Dictionary<ValueBytes, RawUserTransaction>();

            foreach (var ct in ledgerExtension.CommittedTransactions.Where(ct => ct.LedgerTransaction is CoreModel.UserLedgerTransaction))
            {
                var nt = ((CoreModel.UserLedgerTransaction)ct.LedgerTransaction).NotarizedTransaction;

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
                var transactionPayloadHashList = rawUserTransactionByPayloadHash.Keys.Select(x => (byte[])x).ToList(); // List<> are optimised for PostgreSQL lookups

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

                long? nextEpoch = null;
                long? newRoundInEpoch = null;
                DateTime? newRoundTimestamp = null;

                if (ct.Receipt.NextEpoch != null)
                {
                    nextEpoch = ct.Receipt.NextEpoch.Epoch;
                }

                if (ct.LedgerTransaction is CoreModel.ValidatorLedgerTransaction vlt)
                {
                    switch (vlt.ValidatorTransaction)
                    {
                        case CoreModel.TimeUpdateValidatorTransaction timeUpdate:
                            newRoundInEpoch = timeUpdate.RoundInEpoch;
                            newRoundTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(timeUpdate.ProposerTimestamp.UnixTimestampMs).UtcDateTime;
                            break;
                    }
                }

                if (ct.LedgerTransaction is CoreModel.SystemLedgerTransaction)
                {
                    // no-op so far
                }

                foreach (var newSubstate in stateUpdates.CreatedSubstates.Concat(stateUpdates.UpdatedSubstates))
                {
                    var sid = newSubstate.SubstateId;
                    var sd = newSubstate.SubstateData;

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

                    if (sd is CoreModel.IGlobalEntityPointer globalEntityPointer)
                    {
                        foreach (var pointer in globalEntityPointer.GetPointers())
                        {
                            referencedEntities.MarkSeenGlobalAddress(pointer.GlobalAddress);
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
                            re.PostResolveConfigure((FungibleResourceManagerEntity e) => e.Divisibility = resourceManager.FungibleDivisibility);
                        }

                        if (resourceManager.ResourceType == CoreModel.ResourceType.NonFungible)
                        {
                            var type = resourceManager.NonFungibleIdType ?? throw new InvalidOperationException("NonFungibleIdType must be set.");

                            re.PostResolveConfigure((NonFungibleResourceManagerEntity e) => e.NonFungibleIdType = type switch
                            {
                                CoreModel.NonFungibleIdType.String => NonFungibleIdType.String,
                                CoreModel.NonFungibleIdType.Number => NonFungibleIdType.Number,
                                CoreModel.NonFungibleIdType.Bytes => NonFungibleIdType.Bytes,
                                CoreModel.NonFungibleIdType.UUID => NonFungibleIdType.UUID,
                                _ => throw new ArgumentOutOfRangeException(),
                            });
                        }
                    }

                    if (sd is CoreModel.ComponentInfoSubstate componentInfo)
                    {
                        referencedEntities.MarkSeenGlobalAddress(componentInfo.PackageAddress);

                        re.PostResolveConfigure((ComponentEntity e) =>
                        {
                            var packageAddress = RadixAddressCodec.Decode(componentInfo.PackageAddress).Data.ToHex();

                            e.PackageId = referencedEntities.GetByGlobal(packageAddress).DatabaseId;
                            e.BlueprintName = componentInfo.BlueprintName;
                        });
                    }

                    if (sd is CoreModel.PackageInfoSubstate packageInfo)
                    {
                        re.PostResolveConfigure((PackageEntity e) => e.Code = packageInfo.GetCodeBytes());
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

                var isStartOfEpoch = lastTransactionSummary.IsEndOfEpoch;
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
                    Epoch: isStartOfEpoch ? lastTransactionSummary.Epoch + 1 : lastTransactionSummary.Epoch,
                    RoundInEpoch: newRoundInEpoch ?? lastTransactionSummary.RoundInEpoch,
                    IndexInEpoch: isStartOfEpoch ? 0 : lastTransactionSummary.IndexInEpoch + 1,
                    IndexInRound: isStartOfRound ? 0 : lastTransactionSummary.IndexInRound + 1,
                    IsEndOfEpoch: nextEpoch != null,
                    TransactionAccumulator: ct.LedgerTransaction.GetPayloadBytes());

                LedgerTransaction ledgerTransaction = ct.LedgerTransaction switch
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
                ledgerTransaction.RoundInEpoch = summary.RoundInEpoch;
                ledgerTransaction.IndexInEpoch = summary.IndexInEpoch;
                ledgerTransaction.IndexInRound = summary.IndexInRound;
                ledgerTransaction.IsEndOfEpoch = summary.IsEndOfEpoch;
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

            var knownDbEntities = await readHelper.ExistingEntitiesFor(referencedEntities, token);

            dbReadDuration += sw.Elapsed;

            foreach (var knownDbEntity in knownDbEntities.Values)
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
                    AccessControllerEntity => CoreModel.EntityType.AccessController,
                    ValidatorEntity => CoreModel.EntityType.Validator,
                    _ => throw new ArgumentOutOfRangeException(nameof(knownDbEntity), knownDbEntity.GetType().Name),
                };

                referencedEntities.GetOrAdd(knownDbEntity.Address.ToHex(), address => new ReferencedEntity(address, entityType, knownDbEntity.FromStateVersion));
            }

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
                    CoreModel.EntityType.AccessController => new AccessControllerEntity(),
                    CoreModel.EntityType.Validator => new ValidatorEntity(),
                    _ => throw new ArgumentOutOfRangeException(nameof(re.Type), re.Type, null),
                };

                dbEntity.Id = sequences.EntitySequence++;
                dbEntity.FromStateVersion = re.StateVersion;
                dbEntity.Address = re.IdHex.ConvertFromHex();
                dbEntity.GlobalAddress = re.GlobalAddressHex == null ? null : (RadixAddress)re.GlobalAddressHex.ConvertFromHex();

                re.Resolve(dbEntity);
                entitiesToAdd.Add(dbEntity);
            }

            referencedEntities.OnAllEntitiesResolved();

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

                referencedEntities.Get(childAddress).PostResolveConfigure((Entity dbe) =>
                {
                    dbe.AncestorIds = allAncestors;
                    dbe.ParentAncestorId = parentId.Value;
                    dbe.OwnerAncestorId = ownerId.Value;
                    dbe.GlobalAncestorId = globalId.Value;
                });
            }

            referencedEntities.InvokePostResolveConfiguration();

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
        var validatorSetChanges = new List<ValidatorSetChange>();
        var entityAccessRulesChainHistoryToAdd = new List<EntityAccessRulesChainHistory>();
        var componentEntityStateToAdd = new List<ComponentEntityStateHistory>();
        var validatorKeyHistoryToAdd = new Dictionary<ValidatorKeyLookup, ValidatorPublicKeyHistory>();

        // step: scan all substates to figure out changes
        {
            foreach (var ct in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = ct.StateVersion;
                var stateUpdates = ct.Receipt.StateUpdates;

                foreach (var newSubstate in stateUpdates.CreatedSubstates.Concat(stateUpdates.UpdatedSubstates))
                {
                    var sid = newSubstate.SubstateId;
                    var sd = newSubstate.SubstateData;

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
                        switch (vault.ResourceAmount)
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
                                throw new UnreachableException();
                        }
                    }

                    if (sd is CoreModel.NonFungibleStoreEntrySubstate nonFungibleStoreEntry)
                    {
                        var resourceManagerEntity = referencedEntities.GetByDatabaseId(re.DatabaseGlobalAncestorId);

                        nonFungibleIdStoreChanges.Add(new NonFungibleIdChange(re, resourceManagerEntity, nonFungibleStoreEntry.NonFungibleId.SimpleRep, nonFungibleStoreEntry.IsDeleted, nonFungibleStoreEntry.NonFungibleData, stateVersion));
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

                    if (sd is CoreModel.ValidatorSubstate validator)
                    {
                        var lookup = new ValidatorKeyLookup(referencedEntities.Get(sid.EntityIdHex).DatabaseId, validator.PublicKey.KeyType.ToModel(), validator.PublicKey.GetKeyBytes());

                        validatorKeyHistoryToAdd[lookup] = new ValidatorPublicKeyHistory
                        {
                            Id = sequences.ValidatorPublicKeyHistorySequence++,
                            FromStateVersion = stateVersion,
                            ValidatorEntityId = lookup.ValidatorEntityId,
                            KeyType = lookup.PublicKeyType,
                            Key = lookup.PublicKey,
                        };

                        componentEntityStateToAdd.Add(new ComponentEntityStateHistory
                        {
                            Id = sequences.ComponentEntityStateHistorySequence++,
                            FromStateVersion = stateVersion,
                            ComponentEntityId = referencedEntities.Get(sid.EntityIdHex).DatabaseId,
                            State = validator.ToJson(),
                        });
                    }

                    if (sd is CoreModel.ValidatorSetSubstate validatorSet && sid.SubstateKeyType == CoreModel.SubstateKeyType.CurrentValidatorSet)
                    {
                        var change = validatorSet.ValidatorSet
                            .Select(v =>
                            {
                                var vid = referencedEntities.GetByGlobal(RadixAddressCodec.Decode(v.Address).Data.ToHex()).DatabaseId;

                                return new ValidatorKeyLookup(vid, v.Key.KeyType.ToModel(), v.Key.GetKeyBytes());
                            })
                            .ToList();

                        validatorSetChanges.Add(new ValidatorSetChange(validatorSet.Epoch, change, stateVersion));
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
            var existingValidatorKeys = await readHelper.ExistingValidatorKeysFor(validatorSetChanges, token);

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

            var validatorActiveSetHistoryToAdd = validatorSetChanges
                .Select(e =>
                {
                    return new ValidatorActiveSetHistory
                    {
                        Id = sequences.ValidatorActiveSetHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        ValidatorPublicKeyHistoryIds = e.ValidatorSet
                            .Select(v => existingValidatorKeys.GetOrAdd(v, _ => validatorKeyHistoryToAdd[v]).Id)
                            .ToArray(),
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
            rowsInserted += await writeHelper.CopyValidatorKeyHistory(validatorKeyHistoryToAdd.Values, token);
            rowsInserted += await writeHelper.CopyValidatorActiveSetHistory(validatorActiveSetHistoryToAdd, token);
            await writeHelper.UpdateSequences(sequences, token);

            dbWriteDuration += sw.Elapsed;
        }

        var contentHandlingDuration = outerStopwatch.Elapsed - dbReadDuration - dbWriteDuration;

        return new ExtendLedgerReport(lastTransactionSummary, preparationReport, rowsInserted, dbReadDuration, dbWriteDuration, contentHandlingDuration);
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

    private async Task CreateOrUpdateLedgerStatus(ReadWriteDbContext dbContext, TransactionSummary finalTransaction, long latestSyncTarget, CancellationToken token)
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
            RoundInEpoch: lastTransaction.RoundInEpoch,
            IndexInEpoch: lastTransaction.IndexInEpoch,
            IndexInRound: lastTransaction.IndexInRound,
            IsEndOfEpoch: lastTransaction.IsEndOfEpoch,
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
            RoundInEpoch: 0,
            IndexInEpoch: 0,
            IndexInRound: 0,
            IsEndOfEpoch: false,
            TransactionAccumulator: new byte[32]
        );
    }
}
