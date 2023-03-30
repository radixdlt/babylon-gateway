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
using RadixDlt.NetworkGateway.DataAggregator;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.RadixEngineToolkit.Model.Value;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Array = System.Array;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class PostgresLedgerExtenderService : ILedgerExtenderService
{
    private record ExtendLedgerReport(TransactionSummary FinalTransaction, int RowsTouched, TimeSpan DbReadDuration, TimeSpan DbWriteDuration, TimeSpan ContentHandlingDuration);

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
            await tx.CommitAsync(token);

            return new CommitTransactionsReport(
                ledgerExtension.CommittedTransactions.Count,
                extendLedgerReport.FinalTransaction,
                (long)extendLedgerReport.ContentHandlingDuration.TotalMilliseconds,
                (long)extendLedgerReport.DbReadDuration.TotalMilliseconds,
                (long)extendLedgerReport.DbWriteDuration.TotalMilliseconds,
                extendLedgerReport.RowsTouched
            );
        }
        catch (Exception)
        {
            await tx.RollbackAsync(token);

            throw;
        }
    }

    private async Task<int> UpdatePendingTransactions(ReadWriteDbContext dbContext, List<CoreModel.CommittedTransaction> committedTransactions, CancellationToken token)
    {
        /*
         * TODO replace with something like with no version control check but with change - we want to run this query with priority and let other operations fail instead
         *
         * UPDATE pending_transactions pt
         * SET
         *    status = define committed success/failure somehow (success if part of some array?),
         *    commit_timestamp = {utcNow or some timestamp from committedTransactions coll},
         *    last_failure_reason = null,
         *    last_failure_timestamp = null,
         *    version_control = -1
         * FROM (
         *    SELECT * FROM pending_transactions WHERE payload_hash = ANY(...)
         * ) ptu
         * WHERE pt.id = ptu.id
         * RETURNING ptu.*;
         *
         * and then simple foreach loop notifying the observers
         */

        var userTransactionStatusByPayloadHash = new Dictionary<ValueBytes, PendingTransactionStatus>(committedTransactions.Count);

        foreach (var committedTransaction in committedTransactions.Where(ct => ct.LedgerTransaction is CoreModel.UserLedgerTransaction))
        {
            var nt = ((CoreModel.UserLedgerTransaction)committedTransaction.LedgerTransaction).NotarizedTransaction;

            userTransactionStatusByPayloadHash[nt.GetHashBytes()] = committedTransaction.Receipt.Status switch
            {
                CoreModel.TransactionStatus.Succeeded => PendingTransactionStatus.CommittedSuccess,
                CoreModel.TransactionStatus.Failed => PendingTransactionStatus.CommittedFailure,
                _ => throw new UnreachableException($"Didn't expect {committedTransaction.Receipt.Status} value"),
            };
        }

        var payloadHashes = userTransactionStatusByPayloadHash.Keys.Select(x => (byte[])x).ToList();

        var toUpdate = await dbContext.PendingTransactions
            .Where(pt => pt.Status != PendingTransactionStatus.CommittedSuccess && pt.Status != PendingTransactionStatus.CommittedFailure)
            .Where(pt => payloadHashes.Contains(pt.PayloadHash))
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
                    pendingTransaction.LastFailureTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                    pendingTransaction.LastFailureReason
                );
            }

            pendingTransaction.MarkAsCommitted(userTransactionStatusByPayloadHash[pendingTransaction.PayloadHash], _clock.UtcNow);
        }

        // If this errors (due to changes to the MempoolTransaction.Status ConcurrencyToken), we may have to consider
        // something like: https://docs.microsoft.com/en-us/ef/core/saving/concurrency
        var result = await dbContext.SaveChangesAsync(token);

        await _observers.ForEachAsync(x => x.TransactionsMarkedCommittedCount(toUpdate.Count));

        return result;
    }

    private async Task<ExtendLedgerReport> ProcessTransactions(ReadWriteDbContext dbContext, ConsistentLedgerExtension ledgerExtension, CancellationToken token)
    {
        var rowsInserted = 0;
        var rowsUpdated = 0;
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

        SequencesHolder sequences;

        // fetch the next id of each and every different entity type we manually insert to the database
        // later on we also manually update their values
        {
            var sw = Stopwatch.StartNew();

            sequences = await readHelper.LoadSequences(token);

            dbReadDuration += sw.Elapsed;
        }

        // update the status of all the pending transactions that might have been commited with this ledger extension
        {
            var sw = Stopwatch.StartNew();

            rowsUpdated += await UpdatePendingTransactions(dbContext, ledgerExtension.CommittedTransactions, token);

            dbWriteDuration += sw.Elapsed;
        }

        // step: scan for any referenced entities
        {
            var hrp = _networkConfigurationProvider.GetHrpDefinition();

            foreach (var commitedTransaction in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = commitedTransaction.StateVersion;
                var stateUpdates = commitedTransaction.Receipt.StateUpdates;

                long? nextEpoch = null;
                long? newRoundInEpoch = null;
                DateTime? newRoundTimestamp = null;
                LedgerTransactionKindFilterConstraint? kindFilterConstraint = null;

                if (commitedTransaction.LedgerTransaction is CoreModel.ValidatorLedgerTransaction vlt)
                {
                    switch (vlt.ValidatorTransaction)
                    {
                        case CoreModel.RoundUpdateValidatorTransaction roundUpdate:
                            newRoundTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(roundUpdate.ProposerTimestamp.UnixTimestampMs).UtcDateTime;
                            break;
                    }
                }

                if (commitedTransaction.LedgerTransaction is CoreModel.SystemLedgerTransaction)
                {
                    // no-op so far
                }

                if (commitedTransaction.LedgerTransaction is CoreModel.UserLedgerTransaction)
                {
                    kindFilterConstraint = LedgerTransactionKindFilterConstraint.User;
                }

                foreach (var newGlobalEntity in stateUpdates.NewGlobalEntities)
                {
                    var referencedEntity = referencedEntities
                        .GetOrAdd(
                            newGlobalEntity.EntityReference.EntityIdHex,
                            _ => new ReferencedEntity(newGlobalEntity.EntityReference.EntityIdHex, newGlobalEntity.EntityReference.EntityType, stateVersion)
                        );

                    referencedEntity.Globalize((GlobalAddress)newGlobalEntity.GlobalAddress);
                    referencedEntity.WithTypeHint(newGlobalEntity.EntityReference.EntityType switch
                    {
                        CoreModel.EntityType.NormalComponent => typeof(NormalComponentEntity),
                        CoreModel.EntityType.Package => typeof(PackageEntity),
                        CoreModel.EntityType.EpochManager => typeof(EpochManagerEntity),
                        CoreModel.EntityType.Clock => typeof(ClockEntity),
                        CoreModel.EntityType.Validator => typeof(ValidatorEntity),
                        CoreModel.EntityType.AccessController => typeof(AccessControllerEntity),
                        CoreModel.EntityType.Account => typeof(AccountComponentEntity),
                        CoreModel.EntityType.Identity => typeof(IdentityEntity),
                        CoreModel.EntityType.KeyValueStore => typeof(KeyValueStoreEntity),
                        CoreModel.EntityType.Vault => typeof(VaultEntity),
                        CoreModel.EntityType.FungibleResource => typeof(FungibleResourceEntity),
                        CoreModel.EntityType.NonFungibleResource => typeof(NonFungibleResourceEntity),
                        _ => throw new ArgumentOutOfRangeException(),
                    });
                }

                foreach (var substate in stateUpdates.CreatedSubstates.Concat(stateUpdates.UpdatedSubstates).ToList())
                {
                    var substateId = substate.SubstateId;
                    var substateData = substate.SubstateData;

                    if (substateData is CoreModel.EpochManagerSubstate epochManagerSubstate)
                    {
                        newRoundInEpoch = epochManagerSubstate.Round;

                        if (epochManagerSubstate.Round == 0)
                        {
                            nextEpoch = epochManagerSubstate.Epoch;
                            kindFilterConstraint = LedgerTransactionKindFilterConstraint.EpochChange;
                        }
                    }

                    var referencedEntity = referencedEntities.GetOrAdd(substateId.EntityIdHex, _ => new ReferencedEntity(substateId.EntityIdHex, substateId.EntityType, stateVersion));

                    if (substateData is CoreModel.IEntityOwner entityOwner)
                    {
                        foreach (var oe in entityOwner.GetOwnedEntities())
                        {
                            referencedEntities.GetOrAdd(oe.EntityIdHex, _ => new ReferencedEntity(oe.EntityIdHex, oe.EntityType, stateVersion)).IsImmediateChildOf(referencedEntity);
                            childToParentEntities[oe.EntityIdHex] = substateId.EntityIdHex;
                        }
                    }

                    if (substateData is CoreModel.IRoyaltyVaultHolder royaltyVaultHolder && royaltyVaultHolder.TryGetRoyaltyVault(out var rv))
                    {
                        referencedEntities.GetOrAdd(rv.EntityIdHex, _ => new ReferencedEntity(rv.EntityIdHex, rv.EntityType, stateVersion)).IsImmediateChildOf(referencedEntity);
                        childToParentEntities[rv.EntityIdHex] = substateId.EntityIdHex;

                        referencedEntity.PostResolveConfigure((IRoyaltyVaultHolder e) => e.RoyaltyVaultEntityId = referencedEntities.Get(rv.EntityIdHex).DatabaseId);
                        referencedEntities.Get(rv.EntityIdHex).PostResolveConfigure((VaultEntity e) => e.RoyaltyVaultOfEntityId = referencedEntity.DatabaseId);
                    }

                    if (substateData is CoreModel.IGlobalAddressPointer globalEntityPointer)
                    {
                        foreach (var globalAddress in globalEntityPointer.GetGlobalAddresses())
                        {
                            referencedEntities.MarkSeenGlobalAddress((GlobalAddress)globalAddress);
                        }
                    }

                    if (substateData is CoreModel.FungibleResourceManagerSubstate fungibleResourceManager)
                    {
                        referencedEntity.PostResolveConfigure((FungibleResourceEntity e) => e.Divisibility = fungibleResourceManager.Divisibility);
                    }

                    if (substateData is CoreModel.NonFungibleResourceManagerSubstate nonFungibleResourceManager)
                    {
                        var dataTable = nonFungibleResourceManager.NonFungibleDataTable;

                        referencedEntity.PostResolveConfigure((NonFungibleResourceEntity e) => e.NonFungibleIdType = nonFungibleResourceManager.NonFungibleIdType switch
                        {
                            CoreModel.NonFungibleIdType.String => NonFungibleIdType.String,
                            CoreModel.NonFungibleIdType.Integer => NonFungibleIdType.Integer,
                            CoreModel.NonFungibleIdType.Bytes => NonFungibleIdType.Bytes,
                            CoreModel.NonFungibleIdType.UUID => NonFungibleIdType.UUID,
                            _ => throw new ArgumentOutOfRangeException(nameof(e.NonFungibleIdType), e.NonFungibleIdType, "Unexpected value of NonFungibleIdType"),
                        });

                        referencedEntities
                            .GetOrAdd(dataTable.EntityIdHex, _ => new ReferencedEntity(dataTable.EntityIdHex, dataTable.EntityType, stateVersion))
                            .PostResolveConfigure((KeyValueStoreEntity e) => e.StoreOfNonFungibleResourceEntityId = referencedEntity.DatabaseId);
                    }

                    if (substateData is CoreModel.VaultInfoSubstate vaultInfo)
                    {
                        referencedEntity.PostResolveConfigure((VaultEntity e) =>
                        {
                            e.ResourceEntityId = referencedEntities.GetByGlobal((GlobalAddress)vaultInfo.ResourceAddress).DatabaseId;
                        });
                    }

                    if (substateData is CoreModel.TypeInfoSubstate typeInfoSubstate)
                    {
                        switch (typeInfoSubstate.Details)
                        {
                            case CoreModel.KeyValueStoreTypeInfoDetails _:
                                // TODO schema not supported yet
                                break;
                            case CoreModel.ObjectTypeInfoDetails objectDetails:
                                referencedEntity.PostResolveConfigure((ComponentEntity e) =>
                                {
                                    e.PackageId = referencedEntities.GetByGlobal((GlobalAddress)objectDetails.PackageAddress).DatabaseId;
                                    e.BlueprintName = objectDetails.BlueprintName;
                                });
                                break;
                            default:
                                throw new UnreachableException($"Didn't expect '{typeInfoSubstate.Details.Type}' value");
                        }
                    }

                    if (substateData is CoreModel.PackageCodeSubstate packageCode)
                    {
                        referencedEntity.PostResolveConfigure((PackageEntity e) => e.Code = packageCode.GetCodeBytes());
                    }

                    if (substateData is CoreModel.PackageCodeTypeSubstate packageCodeType)
                    {
                        referencedEntity.PostResolveConfigure((PackageEntity e) => e.CodeType = packageCodeType.CodeType);
                    }

                    if (substateData is CoreModel.ValidatorSubstate validator)
                    {
                        referencedEntity.PostResolveConfigure((ValidatorEntity e) =>
                        {
                            e.EpochManagerEntityId = referencedEntities.GetByGlobal((GlobalAddress)validator.EpochManagerAddress).DatabaseId;
                            e.StakeVaultEntityId = referencedEntities.Get(validator.StakeVault.EntityIdHex).DatabaseId;
                            e.UnstakeVaultEntityId = referencedEntities.Get(validator.UnstakeVault.EntityIdHex).DatabaseId;
                        });
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
                    TransactionAccumulator: commitedTransaction.LedgerTransaction.GetPayloadBytes());

                LedgerTransaction ledgerTransaction = commitedTransaction.LedgerTransaction switch
                {
                    CoreModel.UserLedgerTransaction ult => new UserLedgerTransaction
                    {
                        PayloadHash = ult.NotarizedTransaction.GetHashBytes(),
                        IntentHash = ult.NotarizedTransaction.SignedIntent.Intent.GetHashBytes(),
                        SignedIntentHash = ult.NotarizedTransaction.SignedIntent.GetHashBytes(),
                    },
                    CoreModel.ValidatorLedgerTransaction => new ValidatorLedgerTransaction(),
                    CoreModel.SystemLedgerTransaction => new SystemLedgerTransaction(),
                    _ => throw new UnreachableException(),
                };

                var feeSummary = commitedTransaction.Receipt.FeeSummary;

                ledgerTransaction.StateVersion = commitedTransaction.StateVersion;
                ledgerTransaction.Status = commitedTransaction.Receipt.Status.ToModel();
                ledgerTransaction.ErrorMessage = commitedTransaction.Receipt.ErrorMessage;
                ledgerTransaction.TransactionAccumulator = commitedTransaction.GetAccumulatorHashBytes();
                // TODO commented out as incompatible with current Core API version
                ledgerTransaction.Message = null; // message: transaction.Metadata.Message?.ConvertFromHex(),
                ledgerTransaction.Epoch = summary.Epoch;
                ledgerTransaction.RoundInEpoch = summary.RoundInEpoch;
                ledgerTransaction.IndexInEpoch = summary.IndexInEpoch;
                ledgerTransaction.IndexInRound = summary.IndexInRound;
                ledgerTransaction.IsEndOfEpoch = summary.IsEndOfEpoch;
                ledgerTransaction.FeePaid = feeSummary != null
                    ? TokenAmount.FromDecimalString(commitedTransaction.Receipt.FeeSummary.XrdTotalExecutionCost)
                    : null;
                ledgerTransaction.TipPaid = feeSummary != null
                    ? TokenAmount.FromDecimalString(commitedTransaction.Receipt.FeeSummary.XrdTotalTipped)
                    : null;
                ledgerTransaction.RoundTimestamp = summary.RoundTimestamp;
                ledgerTransaction.CreatedTimestamp = summary.CreatedTimestamp;
                ledgerTransaction.NormalizedRoundTimestamp = summary.NormalizedRoundTimestamp;
                ledgerTransaction.KindFilterConstraint = kindFilterConstraint;
                ledgerTransaction.RawPayload = commitedTransaction.LedgerTransaction.GetUnboxedPayloadBytes();
                ledgerTransaction.EngineReceipt = commitedTransaction.Receipt.ToJson();

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
                    AccountComponentEntity => CoreModel.EntityType.Account,
                    FungibleResourceEntity => CoreModel.EntityType.FungibleResource,
                    NonFungibleResourceEntity => CoreModel.EntityType.NonFungibleResource,
                    ClockEntity => CoreModel.EntityType.Clock,
                    ValidatorEntity => CoreModel.EntityType.Validator,
                    VaultEntity => CoreModel.EntityType.Vault,
                    PackageEntity => CoreModel.EntityType.Package,
                    IdentityEntity => CoreModel.EntityType.Identity,
                    AccessControllerEntity => CoreModel.EntityType.AccessController,
                    ComponentEntity => CoreModel.EntityType.NormalComponent,
                    KeyValueStoreEntity => CoreModel.EntityType.KeyValueStore,
                    _ => throw new ArgumentOutOfRangeException(nameof(knownDbEntity), knownDbEntity.GetType().Name),
                };

                referencedEntities.GetOrAdd(knownDbEntity.Address.ToHex(), address => new ReferencedEntity(address, entityType, knownDbEntity.FromStateVersion));
            }

            foreach (var referencedEntity in referencedEntities.All)
            {
                if (knownDbEntities.ContainsKey(referencedEntity.IdHex))
                {
                    referencedEntity.Resolve(knownDbEntities[referencedEntity.IdHex]);

                    continue;
                }

                Entity dbEntity = referencedEntity.Type switch
                {
                    CoreModel.EntityType.EpochManager => new EpochManagerEntity(),
                    CoreModel.EntityType.FungibleResource => referencedEntity.CreateUsingTypeHint<FungibleResourceEntity>(),
                    CoreModel.EntityType.NonFungibleResource => referencedEntity.CreateUsingTypeHint<NonFungibleResourceEntity>(),
                    // If the component is a local / owned component, it doesn't have a Component/Account type hint
                    // from the address, so assume it's a normal component for now until we can do better from the ComponentInfo
                    CoreModel.EntityType.NormalComponent => referencedEntity.CreateUsingTypeHintOrDefault<ComponentEntity>(typeof(NormalComponentEntity)),
                    CoreModel.EntityType.Package => new PackageEntity(),
                    CoreModel.EntityType.Vault => new VaultEntity(),
                    CoreModel.EntityType.KeyValueStore => new KeyValueStoreEntity(),
                    CoreModel.EntityType.Clock => new ClockEntity(),
                    CoreModel.EntityType.AccessController => new AccessControllerEntity(),
                    CoreModel.EntityType.Validator => new ValidatorEntity(),
                    CoreModel.EntityType.Identity => new IdentityEntity(),
                    CoreModel.EntityType.Account => new AccountComponentEntity(),
                    _ => throw new ArgumentOutOfRangeException(nameof(referencedEntity.Type), referencedEntity.Type, null),
                };

                dbEntity.Id = sequences.EntitySequence++;
                dbEntity.FromStateVersion = referencedEntity.StateVersion;
                dbEntity.Address = referencedEntity.IdHex.ConvertFromHex();
                dbEntity.GlobalAddress = referencedEntity.GlobalAddress;

                referencedEntity.Resolve(dbEntity);
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

                    if (!ownerId.HasValue && currentParent.CanBeOwnerAncestor)
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
        var nonFungibleIdChanges = new List<NonFungibleIdChange>();
        var metadataChanges = new List<MetadataChange>();
        var resourceSupplyChanges = new List<ResourceSupplyChange>();
        var validatorSetChanges = new List<ValidatorSetChange>();
        var entityAccessRulesChainHistoryToAdd = new List<EntityAccessRulesChainHistory>();
        var entityStateToAdd = new List<EntityStateHistory>();
        var validatorKeyHistoryToAdd = new Dictionary<ValidatorKeyLookup, ValidatorPublicKeyHistory>();

        // step: scan all substates to figure out changes
        {
            foreach (var committedTransaction in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = committedTransaction.StateVersion;
                var stateUpdates = committedTransaction.Receipt.StateUpdates;

                foreach (var substate in stateUpdates.CreatedSubstates.Concat(stateUpdates.UpdatedSubstates))
                {
                    var substateId = substate.SubstateId;
                    var substateData = substate.SubstateData;

                    var referencedEntity = referencedEntities.Get(substateId.EntityIdHex);

                    if (substateData is CoreModel.MetadataEntrySubstate metadata)
                    {
                        // TODO decompile SBOR or use ValueBytes
                        var keyHex = metadata.KeyHex;
                        var valueHex = metadata.DataStruct?.StructData.DataHex;
                        var isDeleted = metadata.IsDeleted;

                        metadataChanges.Add(new MetadataChange(referencedEntity, keyHex, valueHex, isDeleted, stateVersion));
                    }

                    if (substateData is CoreModel.FungibleResourceManagerSubstate fungibleResourceManager)
                    {
                        resourceSupplyChanges.Add(new ResourceSupplyChange(referencedEntity, TokenAmount.FromDecimalString(fungibleResourceManager.TotalSupply), stateVersion));
                    }

                    if (substateData is CoreModel.NonFungibleResourceManagerSubstate nonFungibleResourceManager)
                    {
                        resourceSupplyChanges.Add(new ResourceSupplyChange(referencedEntity, TokenAmount.FromDecimalString(nonFungibleResourceManager.TotalSupply), stateVersion));
                    }

                    if (substateData is CoreModel.VaultFungibleSubstate vaultFungible)
                    {
                        var amount = TokenAmount.FromDecimalString(vaultFungible.Amount);
                        var resourceEntity = referencedEntities.GetByDatabaseId(referencedEntity.GetDatabaseEntity<VaultEntity>().ResourceEntityId);

                        fungibleVaultChanges.Add(new FungibleVaultChange(referencedEntity, resourceEntity, amount, stateVersion));
                    }

                    if (substateData is CoreModel.VaultNonFungibleSubstate vaultNonFungible)
                    {
                        var resourceEntity = referencedEntities.GetByDatabaseId(referencedEntity.GetDatabaseEntity<VaultEntity>().ResourceEntityId);

                        nonFungibleVaultChanges.Add(new NonFungibleVaultChange(referencedEntity, resourceEntity, vaultNonFungible.NonFungibleIds.Select(nfid => nfid.SimpleRep).ToList(), stateVersion));
                    }

                    if (substateData is CoreModel.KeyValueStoreEntrySubstate keyValueStoreEntry)
                    {
                        var nfStoreOf = referencedEntity.GetDatabaseEntity<KeyValueStoreEntity>().StoreOfNonFungibleResourceEntityId;

                        if (nfStoreOf.HasValue)
                        {
                            var resourceManagerEntity = referencedEntities.GetByDatabaseId(nfStoreOf.Value);

                            nonFungibleIdChanges.Add(new NonFungibleIdChange(referencedEntity, resourceManagerEntity, keyValueStoreEntry.KeyNonFungibleLocalId.SimpleRep,
                                keyValueStoreEntry.IsDeleted, keyValueStoreEntry.DataStruct?.StructData.GetDataBytes(), stateVersion));
                        }
                    }

                    if (substateData is CoreModel.AccessRulesSubstate accessRulesSubstate)
                    {
                        AccessRulesChainSubtype subtype = substateId.ModuleType switch
                        {
                            CoreModel.ModuleType.AccessRules => AccessRulesChainSubtype.None,
                            CoreModel.ModuleType.AccessRules1 => AccessRulesChainSubtype.ResourceManagerVaultAccessRulesChain,
                            _ => throw new UnreachableException($"Didn't expect {substateId.ModuleType} value"),
                        };

                        entityAccessRulesChainHistoryToAdd.Add(new EntityAccessRulesChainHistory
                        {
                            Id = sequences.EntityAccessRulesChainHistorySequence++,
                            FromStateVersion = stateVersion,
                            EntityId = referencedEntities.Get(substateId.EntityIdHex).DatabaseId,
                            Subtype = subtype,
                            AccessRulesChain = JsonConvert.SerializeObject(accessRulesSubstate.AccessRules),
                        });
                    }

                    if (substateData is CoreModel.ComponentStateSubstate componentState)
                    {
                        entityStateToAdd.Add(new EntityStateHistory
                        {
                            Id = sequences.EntityStateHistorySequence++,
                            FromStateVersion = stateVersion,
                            EntityId = referencedEntities.Get(substateId.EntityIdHex).DatabaseId,
                            State = componentState.DataStruct.StructData.ToJson(),
                        });
                    }

                    if (substateData is CoreModel.ValidatorSubstate validator)
                    {
                        var lookup = new ValidatorKeyLookup(referencedEntities.Get(substateId.EntityIdHex).DatabaseId, validator.PublicKey.KeyType.ToModel(), validator.PublicKey.GetKeyBytes());

                        validatorKeyHistoryToAdd[lookup] = new ValidatorPublicKeyHistory
                        {
                            Id = sequences.ValidatorPublicKeyHistorySequence++,
                            FromStateVersion = stateVersion,
                            ValidatorEntityId = lookup.ValidatorEntityId,
                            KeyType = lookup.PublicKeyType,
                            Key = lookup.PublicKey,
                        };

                        entityStateToAdd.Add(new EntityStateHistory
                        {
                            Id = sequences.EntityStateHistorySequence++,
                            FromStateVersion = stateVersion,
                            EntityId = referencedEntities.Get(substateId.EntityIdHex).DatabaseId,
                            State = validator.ToJson(),
                        });
                    }

                    if (substateData is CoreModel.ValidatorSetSubstate validatorSet && substateId.SubstateKeyType == CoreModel.SubstateKeyType.CurrentValidatorSet)
                    {
                        var change = validatorSet.ValidatorSet
                            .ToDictionary(
                                v =>
                                {
                                    var vid = referencedEntities.GetByGlobal((GlobalAddress)v.Address).DatabaseId;

                                    return new ValidatorKeyLookup(vid, v.Key.KeyType.ToModel(), v.Key.GetKeyBytes());
                                },
                                v => TokenAmount.FromDecimalString(v.Stake));

                        validatorSetChanges.Add(new ValidatorSetChange(validatorSet.Epoch, change, stateVersion));
                    }
                }
            }
        }

        // step: now that all the fundamental data is inserted (entities & substates) we can insert some denormalized data
        {
            var sw = Stopwatch.StartNew();

            var mostRecentMetadataHistory = await readHelper.MostRecentEntityMetadataHistoryFor(metadataChanges, token);
            var mostRecentEntityResourceAggregateHistory = await readHelper.MostRecentEntityResourceAggregateHistoryFor(fungibleVaultChanges, nonFungibleVaultChanges, token);
            var mostRecentEntityResourceAggregatedVaultsHistory = await readHelper.MostRecentEntityResourceAggregatedVaultsHistoryFor(fungibleVaultChanges, nonFungibleVaultChanges, token);
            var mostRecentEntityResourceVaultAggregateHistory = await readHelper.MostRecentEntityResourceVaultAggregateHistoryFor(fungibleVaultChanges, nonFungibleVaultChanges, token);
            var mostRecentNonFungibleIdStoreHistory = await readHelper.MostRecentNonFungibleIdStoreHistoryFor(nonFungibleIdChanges, token);
            var mostRecentResourceEntitySupplyHistory = await readHelper.MostRecentResourceEntitySupplyHistoryFor(resourceSupplyChanges, token);
            var existingNonFungibleIdData = await readHelper.ExistingNonFungibleIdDataFor(nonFungibleIdChanges, nonFungibleVaultChanges, token);
            var existingValidatorKeys = await readHelper.ExistingValidatorKeysFor(validatorSetChanges, token);

            dbReadDuration += sw.Elapsed;

            var entityMetadataHistoryToAdd = new List<EntityMetadataHistory>();
            var entityResourceAggregateHistoryCandidates = new List<EntityResourceAggregateHistory>();
            var entityResourceAggregatedVaultsHistoryToAdd = new List<EntityResourceAggregatedVaultsHistory>();
            var entityResourceVaultAggregateHistoryToAdd = new List<EntityResourceVaultAggregateHistory>();
            var nonFungibleIdStoreHistoryToAdd = new Dictionary<NonFungibleStoreLookup, NonFungibleIdStoreHistory>();
            var nonFungibleIdDataToAdd = new List<NonFungibleIdData>();
            var nonFungibleIdsMutableDataHistoryToAdd = new List<NonFungibleIdMutableDataHistory>();

            foreach (var metadataChange in metadataChanges)
            {
                EntityMetadataHistory metadataHistory;

                if (!mostRecentMetadataHistory.TryGetValue(metadataChange.ReferencedEntity.DatabaseId, out var previous) || previous.FromStateVersion != metadataChange.StateVersion)
                {
                    metadataHistory = new EntityMetadataHistory
                    {
                        Id = sequences.EntityMetadataHistorySequence++,
                        FromStateVersion = metadataChange.StateVersion,
                        EntityId = metadataChange.ReferencedEntity.DatabaseId,
                        Keys = new List<string>(),
                        Values = new List<byte[]>(),
                        UpdatedAtStateVersions = new List<long>(),
                    };

                    if (previous != null)
                    {
                        metadataHistory.Keys.AddRange(previous.Keys);
                        metadataHistory.Values.AddRange(previous.Values);
                        metadataHistory.UpdatedAtStateVersions.AddRange(previous.UpdatedAtStateVersions);
                    }

                    entityMetadataHistoryToAdd.Add(metadataHistory);
                    mostRecentMetadataHistory[metadataChange.ReferencedEntity.DatabaseId] = metadataHistory;
                }
                else
                {
                    metadataHistory = previous;
                }

                var key = ScryptoSborUtils.ConvertFromScryptoSborString(metadataChange.KeyHex, _networkConfigurationProvider.GetNetworkId());
                var currentPosition = metadataHistory.Keys.IndexOf(key);

                if (currentPosition != -1)
                {
                    metadataHistory.Keys.RemoveAt(currentPosition);
                    metadataHistory.Values.RemoveAt(currentPosition);
                    metadataHistory.UpdatedAtStateVersions.RemoveAt(currentPosition);
                }

                if (!metadataChange.IsDeleted)
                {
                    metadataHistory.Keys.Insert(0, key);
                    metadataHistory.Values.Insert(0, metadataChange.ValueHex?.ConvertFromHex() ?? throw new InvalidOperationException("impossible x3")); // TODO improve
                    metadataHistory.UpdatedAtStateVersions.Insert(0, metadataChange.StateVersion);
                }
            }

            foreach (var e in nonFungibleIdChanges)
            {
                var nonFungibleIdData = existingNonFungibleIdData.GetOrAdd(new NonFungibleIdLookup(e.ReferencedResource.DatabaseId, e.NonFungibleId), _ =>
                {
                    var ret = new NonFungibleIdData
                    {
                        Id = sequences.NonFungibleIdDataSequence++,
                        FromStateVersion = e.StateVersion,
                        KeyValueStoreEntityId = e.ReferencedStore.DatabaseId,
                        NonFungibleResourceEntityId = e.ReferencedResource.DatabaseId,
                        NonFungibleId = e.NonFungibleId,
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
                        KeyValueStoreEntityId = e.ReferencedStore.DatabaseId,
                        NonFungibleResourceEntityId = e.ReferencedStore.DatabaseGlobalAncestorId,
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
                    MutableData = e.MutableData,
                });

                if (!nonFungibleIdStore.NonFungibleIdDataIds.Contains(nonFungibleIdData.Id))
                {
                    nonFungibleIdStore.NonFungibleIdDataIds.Add(nonFungibleIdData.Id);
                }
            }

            void AggregateEntityResource(
                ReferencedEntity referencedVault,
                ReferencedEntity referencedResource,
                long stateVersion,
                bool fungibleResource,
                TokenAmount? tmpFungibleBalance,
                long? tmpNonFungibleTotalCount)
            {
                // TODO we most likely want to introduce FungibleVaultEntity and NonFungibleVaultEntity

                if (referencedVault.GetDatabaseEntity<VaultEntity>().IsRoyaltyVault)
                {
                    return;
                }

                if (fungibleResource)
                {
                    var tmpBalance = tmpFungibleBalance ?? throw new InvalidOperationException("impossible x1"); // TODO improve

                    AggregateEntityFungibleResourceVaultInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId, tmpBalance, referencedVault.DatabaseId);
                    AggregateEntityFungibleResourceVaultInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId, tmpBalance, referencedVault.DatabaseId);
                }
                else
                {
                    var tmpTotalCount = tmpNonFungibleTotalCount ?? throw new InvalidOperationException("impossible x2"); // TODO improve

                    AggregateEntityNonFungibleResourceVaultInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId, tmpTotalCount, referencedVault.DatabaseId);
                    AggregateEntityNonFungibleResourceVaultInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId, tmpTotalCount, referencedVault.DatabaseId);
                }

                AggregateEntityResourceInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId);
                AggregateEntityResourceInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId);
                AggregateEntityResourceVaultInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId, referencedVault.DatabaseId);
                AggregateEntityResourceVaultInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId, referencedVault.DatabaseId);

                // TODO rename tmpBalance->delta and drop tmpResourceVaultEntityId once TX events become available
                void AggregateEntityFungibleResourceVaultInternal(long entityId, long resourceEntityId, TokenAmount tmpBalance, long tmpResourceVaultEntityId)
                {
                    var lookup = new EntityResourceLookup(entityId, resourceEntityId);

                    if (!mostRecentEntityResourceAggregatedVaultsHistory.TryGetValue(lookup, out var aggregate) || aggregate.FromStateVersion != stateVersion)
                    {
                        var previousTmpTmp = aggregate?.TmpTmpRemoveMeOnceTxEventsBecomeAvailable ?? string.Empty;

                        aggregate = new EntityFungibleResourceAggregatedVaultsHistory
                        {
                            Id = sequences.EntityResourceAggregatedVaultsHistorySequence++,
                            FromStateVersion = stateVersion,
                            EntityId = entityId,
                            ResourceEntityId = resourceEntityId,
                            TmpTmpRemoveMeOnceTxEventsBecomeAvailable = previousTmpTmp,
                        };

                        entityResourceAggregatedVaultsHistoryToAdd.Add(aggregate);
                        mostRecentEntityResourceAggregatedVaultsHistory[lookup] = aggregate;
                    }

                    // TODO replace with simple aggregate.Balance += delta once TX events become available
                    var tmpSum = TokenAmount.Zero;
                    var tmpExists = false;
                    var tmpColl = aggregate.TmpTmpRemoveMeOnceTxEventsBecomeAvailable
                        .Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(e =>
                        {
                            var parts = e.Split('=');
                            var vaultId = long.Parse(parts[0]);
                            var balance = TokenAmount.FromDecimalString(parts[1]);

                            if (vaultId == tmpResourceVaultEntityId)
                            {
                                tmpExists = true;
                                balance = tmpBalance;
                            }

                            tmpSum += balance;

                            return $"{vaultId}={balance}";
                        })
                        .ToList();

                    if (tmpExists == false)
                    {
                        tmpColl.Add($"{tmpResourceVaultEntityId}={tmpBalance}");
                        tmpSum += tmpBalance;
                    }

                    aggregate.TmpTmpRemoveMeOnceTxEventsBecomeAvailable = string.Join(";", tmpColl);

                    ((EntityFungibleResourceAggregatedVaultsHistory)aggregate).Balance = tmpSum;
                }

                // TODO rename tmpTotalCount->delta and drop tmpResourceVaultEntityId once TX events become available
                void AggregateEntityNonFungibleResourceVaultInternal(long entityId, long resourceEntityId, long tmpTotalCount, long tmpResourceVaultEntityId)
                {
                    var lookup = new EntityResourceLookup(entityId, resourceEntityId);

                    if (!mostRecentEntityResourceAggregatedVaultsHistory.TryGetValue(lookup, out var aggregate) || aggregate.FromStateVersion != stateVersion)
                    {
                        var previousTmpTmp = aggregate?.TmpTmpRemoveMeOnceTxEventsBecomeAvailable ?? string.Empty;

                        aggregate = new EntityNonFungibleResourceAggregatedVaultsHistory
                        {
                            Id = sequences.EntityResourceAggregatedVaultsHistorySequence++,
                            FromStateVersion = stateVersion,
                            EntityId = entityId,
                            ResourceEntityId = resourceEntityId,
                            TmpTmpRemoveMeOnceTxEventsBecomeAvailable = previousTmpTmp,
                        };

                        entityResourceAggregatedVaultsHistoryToAdd.Add(aggregate);
                        mostRecentEntityResourceAggregatedVaultsHistory[lookup] = aggregate;
                    }

                    // TODO replace with simple aggregate.TotalCount += delta once TX events become available
                    var tmpSum = 0L;
                    var tmpExists = false;
                    var tmpColl = aggregate.TmpTmpRemoveMeOnceTxEventsBecomeAvailable
                        .Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(e =>
                        {
                            var parts = e.Split('=');
                            var vaultId = long.Parse(parts[0]);
                            var totalCount = long.Parse(parts[1]);

                            if (vaultId == tmpResourceVaultEntityId)
                            {
                                tmpExists = true;
                                totalCount = tmpTotalCount;
                            }

                            tmpSum += totalCount;

                            return $"{vaultId}={totalCount}";
                        })
                        .ToList();

                    if (tmpExists == false)
                    {
                        tmpColl.Add($"{tmpResourceVaultEntityId}={tmpTotalCount}");
                        tmpSum += tmpTotalCount;
                    }

                    aggregate.TmpTmpRemoveMeOnceTxEventsBecomeAvailable = string.Join(";", tmpColl);

                    ((EntityNonFungibleResourceAggregatedVaultsHistory)aggregate).TotalCount = tmpSum;
                }

                void AggregateEntityResourceInternal(long entityId, long resourceEntityId)
                {
                    // we only want to create new aggregated resource history entry if
                    // - given resource is seen for the very first time,
                    // - given resource is already stored but has been updated and this update caused change of order (this is evaluated right before db persistence)

                    if (mostRecentEntityResourceAggregateHistory.TryGetValue(entityId, out var aggregate))
                    {
                        var existingResourceCollection = fungibleResource
                            ? aggregate.FungibleResourceEntityIds
                            : aggregate.NonFungibleResourceEntityIds;

                        // we're already the most recent one, there's nothing more to do
                        if (existingResourceCollection.IndexOf(resourceEntityId) == 0)
                        {
                            return;
                        }
                    }

                    if (aggregate == null || aggregate.FromStateVersion != stateVersion)
                    {
                        aggregate = aggregate == null
                            ? EntityResourceAggregateHistory.Create(sequences.EntityResourceAggregateHistorySequence++, entityId, stateVersion)
                            : EntityResourceAggregateHistory.CopyOf(sequences.EntityResourceAggregateHistorySequence++, aggregate, stateVersion);

                        entityResourceAggregateHistoryCandidates.Add(aggregate);
                        mostRecentEntityResourceAggregateHistory[entityId] = aggregate;
                    }

                    if (fungibleResource)
                    {
                        aggregate.TryUpsertFungible(resourceEntityId, stateVersion);
                    }
                    else
                    {
                        aggregate.TryUpsertNonFungible(resourceEntityId, stateVersion);
                    }
                }

                void AggregateEntityResourceVaultInternal(long entityId, long resourceEntityId, long resourceVaultEntityId)
                {
                    var lookup = new EntityResourceVaultLookup(entityId, resourceEntityId);

                    if (mostRecentEntityResourceVaultAggregateHistory.TryGetValue(lookup, out var existingAggregate))
                    {
                        if (existingAggregate.VaultEntityIds.Contains(resourceVaultEntityId))
                        {
                            return;
                        }
                    }

                    var aggregate = existingAggregate;

                    if (aggregate == null || aggregate.FromStateVersion != stateVersion)
                    {
                        aggregate = new EntityResourceVaultAggregateHistory
                        {
                            Id = sequences.EntityResourceVaultAggregateHistorySequence++,
                            FromStateVersion = stateVersion,
                            EntityId = entityId,
                            ResourceEntityId = resourceEntityId,
                            VaultEntityIds = new List<long>(existingAggregate?.VaultEntityIds.ToArray() ?? Array.Empty<long>()),
                        };

                        entityResourceVaultAggregateHistoryToAdd.Add(aggregate);
                        mostRecentEntityResourceVaultAggregateHistory[lookup] = aggregate;
                    }

                    aggregate.VaultEntityIds.Add(resourceVaultEntityId);
                }
            }

            var entityFungibleVaultHistoryToAdd = fungibleVaultChanges
                .Select(e =>
                {
                    AggregateEntityResource(e.ReferencedVault, e.ReferencedResource, e.StateVersion, true, e.Balance, null);

                    return new EntityFungibleVaultHistory
                    {
                        Id = sequences.EntityVaultHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.ReferencedResource.DatabaseId,
                        VaultEntityId = e.ReferencedVault.DatabaseId,
                        IsRoyaltyVault = e.ReferencedVault.GetDatabaseEntity<VaultEntity>().IsRoyaltyVault,
                        Balance = e.Balance,
                    };
                })
                .ToList();

            var entityNonFungibleVaultHistoryToAdd = nonFungibleVaultChanges
                .Select(e =>
                {
                    AggregateEntityResource(e.ReferencedVault, e.ReferencedResource, e.StateVersion, false, null, e.NonFungibleIds.Count);

                    return new EntityNonFungibleVaultHistory
                    {
                        Id = sequences.EntityVaultHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.ReferencedResource.DatabaseId,
                        VaultEntityId = e.ReferencedVault.DatabaseId,
                        NonFungibleIds = e.NonFungibleIds.Select(nfid => existingNonFungibleIdData[new NonFungibleIdLookup(e.ReferencedResource.DatabaseId, nfid)].Id).ToList(),
                    };
                })
                .ToList();

            var resourceEntitySupplyHistoryToAdd = resourceSupplyChanges
                .Select(e =>
                {
                    var previous = mostRecentResourceEntitySupplyHistory.GetOrAdd(
                        e.ResourceEntity.DatabaseId,
                        _ => new ResourceEntitySupplyHistory { TotalSupply = TokenAmount.Zero, TotalMinted = TokenAmount.Zero, TotalBurnt = TokenAmount.Zero, });

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

                    var entry = new ResourceEntitySupplyHistory
                    {
                        Id = sequences.ResourceEntitySupplyHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        ResourceEntityId = e.ResourceEntity.DatabaseId,
                        TotalSupply = e.TotalSupply,
                        TotalMinted = totalMinted,
                        TotalBurnt = totalBurnt,
                    };

                    mostRecentResourceEntitySupplyHistory[e.ResourceEntity.DatabaseId] = entry;

                    return entry;
                })
                .ToList();

            var validatorActiveSetHistoryToAdd = validatorSetChanges
                .SelectMany(e =>
                {
                    return e.ValidatorSet.Select(vs => new ValidatorActiveSetHistory
                    {
                        Id = sequences.ValidatorActiveSetHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        Epoch = e.Epoch,
                        ValidatorPublicKeyHistoryId = existingValidatorKeys.GetOrAdd(vs.Key, _ => validatorKeyHistoryToAdd[vs.Key]).Id,
                        Stake = vs.Value,
                    });
                })
                .ToList();

            var entityResourceAggregateHistoryToAdd = entityResourceAggregateHistoryCandidates.Where(x => x.ShouldBePersisted()).ToList();

            sw = Stopwatch.StartNew();

            rowsInserted += await writeHelper.CopyEntityStateHistory(entityStateToAdd, token);
            rowsInserted += await writeHelper.CopyEntityAccessRulesChainHistory(entityAccessRulesChainHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityMetadataHistory(entityMetadataHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityResourceAggregatedVaultsHistory(entityResourceAggregatedVaultsHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityResourceAggregateHistory(entityResourceAggregateHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityResourceVaultAggregateHistory(entityResourceVaultAggregateHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityVaultHistory(entityFungibleVaultHistoryToAdd, entityNonFungibleVaultHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleIdData(nonFungibleIdDataToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleIdMutableDataHistory(nonFungibleIdsMutableDataHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleIdStoreHistory(nonFungibleIdStoreHistoryToAdd.Values, token);
            rowsInserted += await writeHelper.CopyResourceEntitySupplyHistory(resourceEntitySupplyHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyValidatorKeyHistory(validatorKeyHistoryToAdd.Values, token);
            rowsInserted += await writeHelper.CopyValidatorActiveSetHistory(validatorActiveSetHistoryToAdd, token);
            await writeHelper.UpdateSequences(sequences, token);

            dbWriteDuration += sw.Elapsed;
        }

        var contentHandlingDuration = outerStopwatch.Elapsed - dbReadDuration - dbWriteDuration;

        return new ExtendLedgerReport(lastTransactionSummary, rowsInserted + rowsUpdated, dbReadDuration, dbWriteDuration, contentHandlingDuration);
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

        await dbContext.SaveChangesAsync(token);
    }

    private async Task<TransactionSummary> GetTopOfLedger(ReadWriteDbContext dbContext, CancellationToken token)
    {
        var lastTransaction = await dbContext.LedgerTransactions
            .AsNoTracking()
            .OrderByDescending(lt => lt.StateVersion)
            .FirstOrDefaultAsync(token);

        var lastOverview = lastTransaction == null
            ? null
            : new TransactionSummary(
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
