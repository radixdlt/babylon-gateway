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
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.DataAggregator;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Array = System.Array;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

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

        var userTransactionStatusByPayloadHash = new Dictionary<string, PendingTransactionStatus>(committedTransactions.Count);

        foreach (var committedTransaction in committedTransactions.Where(ct => ct.LedgerTransaction is CoreModel.UserLedgerTransaction))
        {
            var ult = ((CoreModel.UserLedgerTransaction)committedTransaction.LedgerTransaction).NotarizedTransaction;

            userTransactionStatusByPayloadHash[ult.HashBech32m] = committedTransaction.Receipt.Status switch
            {
                CoreModel.TransactionStatus.Succeeded => PendingTransactionStatus.CommittedSuccess,
                CoreModel.TransactionStatus.Failed => PendingTransactionStatus.CommittedFailure,
                _ => throw new UnreachableException($"Didn't expect {committedTransaction.Receipt.Status} value"),
            };
        }

        var payloadHashes = userTransactionStatusByPayloadHash.Keys.ToList();

        var toUpdate = await dbContext
            .PendingTransactions
            .Where(pt => pt.Status != PendingTransactionStatus.CommittedSuccess && pt.Status != PendingTransactionStatus.CommittedFailure)
            .Where(pt => payloadHashes.Contains(pt.PayloadHash))
            .ToListAsync(token);

        if (toUpdate.Count == 0)
        {
            return 0;
        }

        foreach (var pendingTransaction in toUpdate)
        {
            if (pendingTransaction.Status is PendingTransactionStatus.RejectedPermanently)
            {
                await _observers.ForEachAsync(x => x.TransactionsMarkedCommittedWhichWasFailed());

                _logger.LogError(
                    "Transaction with payload hash {PayloadHash} which was first/last submitted to Gateway at {FirstGatewaySubmissionTime}/{LastGatewaySubmissionTime} and last marked missing from mempool at {LastMissingFromMempoolTimestamp} was mark {FailureTransiency} at {FailureTime} due to \"{FailureReason}\" but has now been marked committed",
                    pendingTransaction.PayloadHash,
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
        var childToParentEntities = new Dictionary<EntityAddress, EntityAddress>();
        var manifestExtractedAddresses = new Dictionary<long, ManifestAddressesExtractor.ManifestAddresses>();

        var readHelper = new ReadHelper(dbContext);
        var writeHelper = new WriteHelper(dbContext);

        var lastTransactionSummary = ledgerExtension.LatestTransactionSummary;

        var ledgerTransactionsToAdd = new List<LedgerTransaction>();
        var ledgerTransactionMarkersToAdd = new List<LedgerTransactionMarker>();
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
            foreach (var committedTransaction in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = committedTransaction.ResultantStateIdentifiers.StateVersion;
                var stateUpdates = committedTransaction.Receipt.StateUpdates;

                try
                {
                    long? epochUpdate = null;
                    long? roundInEpochUpdate = null;
                    DateTime? roundTimestampUpdate = null;

                    if (committedTransaction.LedgerTransaction is CoreModel.UserLedgerTransaction userLedgerTransaction)
                    {
                        ledgerTransactionMarkersToAdd.Add(new OriginLedgerTransactionMarker
                        {
                            Id = sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OriginType = LedgerTransactionMarkerOriginType.User,
                        });

                        var coreInstructions = userLedgerTransaction.NotarizedTransaction.SignedIntent.Intent.Instructions;
                        var coreBlobs = userLedgerTransaction.NotarizedTransaction.SignedIntent.Intent.BlobsHex;
                        using var manifestInstructions = ToolkitModel.Instructions.FromString(coreInstructions, await _networkConfigurationProvider.GetNetworkId());
                        using var toolkitManifest = new ToolkitModel.TransactionManifest(manifestInstructions, coreBlobs.Values.Select(x => x.ConvertFromHex().ToList()).ToList());
                        var extractedAddresses = ManifestAddressesExtractor.ExtractAddresses(toolkitManifest);

                        foreach (var address in extractedAddresses.All())
                        {
                            referencedEntities.MarkSeenAddress(address);
                        }

                        manifestExtractedAddresses[stateVersion] = extractedAddresses;
                    }

                    if (committedTransaction.LedgerTransaction is CoreModel.RoundUpdateLedgerTransaction rult)
                    {
                        epochUpdate = lastTransactionSummary.Epoch != rult.RoundUpdateTransaction.Epoch ? rult.RoundUpdateTransaction.Epoch : null;
                        roundInEpochUpdate = rult.RoundUpdateTransaction.RoundInEpoch;
                        roundTimestampUpdate = DateTimeOffset.FromUnixTimeMilliseconds(rult.RoundUpdateTransaction.ProposerTimestamp.UnixTimestampMs).UtcDateTime;
                    }

                    foreach (var newGlobalEntity in stateUpdates.NewGlobalEntities)
                    {
                        var referencedEntity = referencedEntities.GetOrAdd((EntityAddress)newGlobalEntity.EntityAddress, ea => new ReferencedEntity(ea, newGlobalEntity.EntityType, stateVersion));

                        referencedEntity.WithTypeHint(newGlobalEntity.EntityType switch
                        {
                            CoreModel.EntityType.GlobalGenericComponent => typeof(GlobalGenericComponentEntity),
                            CoreModel.EntityType.GlobalPackage => typeof(GlobalPackageEntity),
                            CoreModel.EntityType.GlobalConsensusManager => typeof(GlobalConsensusManager),
                            CoreModel.EntityType.GlobalValidator => typeof(GlobalValidatorEntity),
                            CoreModel.EntityType.GlobalAccessController => typeof(GlobalAccessControllerEntity),
                            CoreModel.EntityType.GlobalVirtualEd25519Account => typeof(GlobalAccountEntity),
                            CoreModel.EntityType.GlobalVirtualSecp256k1Account => typeof(GlobalAccountEntity),
                            CoreModel.EntityType.GlobalAccount => typeof(GlobalAccountEntity),
                            CoreModel.EntityType.GlobalIdentity => typeof(GlobalIdentityEntity),
                            CoreModel.EntityType.GlobalVirtualEd25519Identity => typeof(GlobalIdentityEntity),
                            CoreModel.EntityType.GlobalVirtualSecp256k1Identity => typeof(GlobalIdentityEntity),
                            CoreModel.EntityType.GlobalFungibleResource => typeof(GlobalFungibleResourceEntity),
                            CoreModel.EntityType.GlobalNonFungibleResource => typeof(GlobalNonFungibleResourceEntity),
                            CoreModel.EntityType.GlobalTransactionTracker => typeof(GlobalTransactionTrackerEntity),
                            CoreModel.EntityType.GlobalOneResourcePool => typeof(GlobalOneResourcePoolEntity),
                            CoreModel.EntityType.GlobalTwoResourcePool => typeof(GlobalTwoResourcePoolEntity),
                            CoreModel.EntityType.GlobalMultiResourcePool => typeof(GlobalMultiResourcePoolEntity),
                            _ => throw new ArgumentOutOfRangeException(nameof(newGlobalEntity.EntityType), newGlobalEntity.EntityType.ToString()),
                        });
                    }

                    foreach (var substate in stateUpdates.UpsertedSubstates)
                    {
                        var substateId = substate.SubstateId;
                        var substateData = substate.Value.SubstateData;
                        var referencedEntity = referencedEntities.GetOrAdd((EntityAddress)substateId.EntityAddress, ea => new ReferencedEntity(ea, substateId.EntityType, stateVersion));

                        if (substateData is CoreModel.ConsensusManagerFieldCurrentTimeSubstate currentTime)
                        {
                            roundTimestampUpdate = DateTimeOffset.FromUnixTimeMilliseconds(currentTime.Value.ProposerTimestamp.UnixTimestampMs).UtcDateTime;
                        }

                        if (substateData is CoreModel.IEntityOwner entityOwner)
                        {
                            foreach (var oe in entityOwner.GetOwnedEntities())
                            {
                                referencedEntities
                                    .GetOrAdd((EntityAddress)oe.EntityAddress, ea => new ReferencedEntity(ea, oe.EntityType, stateVersion))
                                    .IsImmediateChildOf(referencedEntity);
                                childToParentEntities[(EntityAddress)oe.EntityAddress] = (EntityAddress)substateId.EntityAddress;
                            }
                        }

                        if (substateData is CoreModel.IRoyaltyVaultHolder royaltyVaultHolder && royaltyVaultHolder.TryGetRoyaltyVault(out var rv))
                        {
                            referencedEntities
                                .GetOrAdd((EntityAddress)rv.EntityAddress, _ => new ReferencedEntity((EntityAddress)rv.EntityAddress, rv.EntityType, stateVersion))
                                .IsImmediateChildOf(referencedEntity);
                            childToParentEntities[(EntityAddress)rv.EntityAddress] = (EntityAddress)substateId.EntityAddress;

                            referencedEntities
                                .Get((EntityAddress)rv.EntityAddress)
                                .PostResolveConfigure((InternalFungibleVaultEntity e) => e.RoyaltyVaultOfEntityId = referencedEntity.DatabaseId);
                        }

                        foreach (var entityAddress in substate.SystemStructure.GetEntityAddresses())
                        {
                            referencedEntities.MarkSeenAddress((EntityAddress)entityAddress);
                        }

                        if (substateData is CoreModel.IEntityAddressPointer parentAddressPointer)
                        {
                            foreach (var entityAddress in parentAddressPointer.GetEntityAddresses())
                            {
                                referencedEntities.MarkSeenAddress((EntityAddress)entityAddress);
                            }
                        }

                        if (substateData is CoreModel.FungibleResourceManagerFieldDivisibilitySubstate fungibleResourceManager)
                        {
                            referencedEntity.PostResolveConfigure((GlobalFungibleResourceEntity e) => e.Divisibility = fungibleResourceManager.Value.Divisibility);
                        }

                        if (substateData is CoreModel.NonFungibleResourceManagerFieldIdTypeSubstate nonFungibleResourceManagerFieldIdTypeSubstate)
                        {
                            referencedEntity.PostResolveConfigure((GlobalNonFungibleResourceEntity e) => e.NonFungibleIdType =
                                nonFungibleResourceManagerFieldIdTypeSubstate.Value.NonFungibleIdType switch
                                {
                                    CoreModel.NonFungibleIdType.String => NonFungibleIdType.String,
                                    CoreModel.NonFungibleIdType.Integer => NonFungibleIdType.Integer,
                                    CoreModel.NonFungibleIdType.Bytes => NonFungibleIdType.Bytes,
                                    CoreModel.NonFungibleIdType.RUID => NonFungibleIdType.RUID,
                                    _ => throw new ArgumentOutOfRangeException(nameof(e.NonFungibleIdType), e.NonFungibleIdType, "Unexpected value of NonFungibleIdType"),
                                });
                        }

                        if (substateData is CoreModel.TypeInfoModuleFieldTypeInfoSubstate typeInfoSubstate)
                        {
                            switch (typeInfoSubstate.Value.Details)
                            {
                                case CoreModel.ObjectTypeInfoDetails objectDetails:
                                    referencedEntity.PostResolveConfigure((ComponentEntity e) =>
                                    {
                                        e.PackageId = referencedEntities.Get((EntityAddress)objectDetails.BlueprintInfo.PackageAddress).DatabaseId;
                                        e.BlueprintName = objectDetails.BlueprintInfo.BlueprintName;
                                        e.BlueprintVersion = objectDetails.BlueprintInfo.BlueprintVersion;
                                    });

                                    if (objectDetails.BlueprintInfo.BlueprintName is CoreModel.NativeBlueprintNames.FungibleVault or CoreModel.NativeBlueprintNames.NonFungibleVault)
                                    {
                                        referencedEntity.PostResolveConfigure((VaultEntity e) =>
                                        {
                                            e.ResourceEntityId = referencedEntities.Get((EntityAddress)objectDetails.BlueprintInfo.OuterObject).DatabaseId;
                                        });
                                    }

                                    break;
                                case CoreModel.KeyValueStoreTypeInfoDetails:
                                    break;
                                default:
                                    throw new UnreachableException($"Didn't expect '{typeInfoSubstate.Value.Details.Type}' value");
                            }
                        }

                        if (substateData is CoreModel.ValidatorFieldStateSubstate validator)
                        {
                            referencedEntity.PostResolveConfigure((GlobalValidatorEntity e) =>
                            {
                                e.StakeVaultEntityId = referencedEntities.Get((EntityAddress)validator.Value.StakeXrdVault.EntityAddress).DatabaseId;
                                e.PendingXrdWithdrawVault = referencedEntities.Get((EntityAddress)validator.Value.PendingXrdWithdrawVault.EntityAddress).DatabaseId;
                                e.LockedOwnerStakeUnitVault = referencedEntities.Get((EntityAddress)validator.Value.LockedOwnerStakeUnitVault.EntityAddress).DatabaseId;
                                e.PendingOwnerStakeUnitUnlockVault = referencedEntities.Get((EntityAddress)validator.Value.PendingOwnerStakeUnitUnlockVault.EntityAddress).DatabaseId;
                            });
                        }

                        if (substateData is CoreModel.PackageCodeVmTypeEntrySubstate packageCodeVmType)
                        {
                            referencedEntity.PostResolveConfigure((GlobalPackageEntity e) =>
                            {
                                e.VmType = packageCodeVmType.Value.VmType.ToModel();
                            });
                        }
                    }

                    foreach (var deletedSubstate in stateUpdates.DeletedSubstates)
                    {
                        var sid = deletedSubstate.SubstateId;
                        referencedEntities.GetOrAdd((EntityAddress)sid.EntityAddress, ea => new ReferencedEntity(ea, sid.EntityType, stateVersion));

                        foreach (var entityAddress in deletedSubstate.SystemStructure.GetEntityAddresses())
                        {
                            referencedEntities.MarkSeenAddress((EntityAddress)entityAddress);
                        }
                    }

                    if (committedTransaction.Receipt.Events != null)
                    {
                        foreach (var @event in committedTransaction.Receipt.Events)
                        {
                            switch (@event.Type.Emitter)
                            {
                                case CoreModel.FunctionEventEmitterIdentifier functionEventEmitterIdentifier:
                                {
                                    var entityAddress = (EntityAddress)functionEventEmitterIdentifier.PackageAddress;
                                    referencedEntities.GetOrAdd(entityAddress, ea => new ReferencedEntity(ea, CoreModel.EntityType.GlobalPackage, stateVersion));
                                    break;
                                }

                                case CoreModel.MethodEventEmitterIdentifier methodEventEmitterIdentifier:
                                {
                                    var entityAddress = (EntityAddress)methodEventEmitterIdentifier.Entity.EntityAddress;

                                    referencedEntities.GetOrAdd(entityAddress, ea => new ReferencedEntity(ea, methodEventEmitterIdentifier.Entity.EntityType, stateVersion));
                                    break;
                                }

                                default:
                                    throw new ArgumentOutOfRangeException(nameof(@event.Type.Emitter), @event.Type.Emitter, null);
                            }
                        }
                    }

                    /* NB:
                       The Epoch Transition Transaction sort of fits between epochs, but it seems to fit slightly more naturally
                       as the _first_ transaction of a new epoch, as creates the next EpochData, and the RoundData to 0.
                    */

                    var isStartOfEpoch = epochUpdate.HasValue;
                    var isStartOfRound = roundInEpochUpdate.HasValue;
                    var roundTimestamp = roundTimestampUpdate ?? lastTransactionSummary.RoundTimestamp;
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
                        Epoch: epochUpdate ?? lastTransactionSummary.Epoch,
                        RoundInEpoch: roundInEpochUpdate ?? lastTransactionSummary.RoundInEpoch,
                        IndexInEpoch: isStartOfEpoch ? 0 : lastTransactionSummary.IndexInEpoch + 1,
                        IndexInRound: isStartOfRound ? 0 : lastTransactionSummary.IndexInRound + 1);

                    LedgerTransaction ledgerTransaction = committedTransaction.LedgerTransaction switch
                    {
                        CoreModel.GenesisLedgerTransaction => new GenesisLedgerTransaction(),
                        CoreModel.UserLedgerTransaction ult => new UserLedgerTransaction
                        {
                            PayloadHash = ult.NotarizedTransaction.HashBech32m,
                            IntentHash = ult.NotarizedTransaction.SignedIntent.Intent.HashBech32m,
                            SignedIntentHash = ult.NotarizedTransaction.SignedIntent.HashBech32m,
                            Message = ult.NotarizedTransaction.SignedIntent.Intent.Message?.ToJson(),
                        },
                        CoreModel.RoundUpdateLedgerTransaction => new RoundUpdateLedgerTransaction(),
                        _ => throw new UnreachableException(),
                    };

                    var feeSummary = committedTransaction.Receipt.FeeSummary;

                    ledgerTransaction.StateVersion = stateVersion;
                    ledgerTransaction.Epoch = summary.Epoch;
                    ledgerTransaction.RoundInEpoch = summary.RoundInEpoch;
                    ledgerTransaction.IndexInEpoch = summary.IndexInEpoch;
                    ledgerTransaction.IndexInRound = summary.IndexInRound;
                    ledgerTransaction.FeePaid = feeSummary != null
                        ? TokenAmount.FromDecimalString(committedTransaction.Receipt.FeeSummary.XrdTotalExecutionCost)
                        : null;
                    ledgerTransaction.TipPaid = feeSummary != null
                        ? TokenAmount.FromDecimalString(committedTransaction.Receipt.FeeSummary.XrdTotalTippingCost)
                        : null;
                    ledgerTransaction.RoundTimestamp = summary.RoundTimestamp;
                    ledgerTransaction.CreatedTimestamp = summary.CreatedTimestamp;
                    ledgerTransaction.NormalizedRoundTimestamp = summary.NormalizedRoundTimestamp;
                    ledgerTransaction.RawPayload = committedTransaction.LedgerTransaction.GetUnwrappedPayloadBytes();
                    ledgerTransaction.EngineReceipt = new TransactionReceipt
                    {
                        StateUpdates = committedTransaction.Receipt.StateUpdates.ToJson(),
                        Status = committedTransaction.Receipt.Status.ToModel(),
                        FeeSummary = committedTransaction.Receipt.FeeSummary.ToJson(),
                        ErrorMessage = committedTransaction.Receipt.ErrorMessage,
                        Output = committedTransaction.Receipt.Output != null ? JsonConvert.SerializeObject(committedTransaction.Receipt.Output) : null,
                        NextEpoch = committedTransaction.Receipt.NextEpoch?.ToJson(),
                        EventsSbors = committedTransaction.Receipt.Events?.Select(x => x.Data.GetDataBytes()).ToArray() ?? Array.Empty<byte[]>(),
                        CostingParameters = committedTransaction.Receipt.CostingParameters.ToJson(),
                        FeeDestination = committedTransaction.Receipt.FeeDestination?.ToJson(),
                        FeeSource = committedTransaction.Receipt.FeeSource?.ToJson(),
                        EventSchemaHashes = committedTransaction.Receipt.Events?.Select(x => x.Type.TypeReference.SchemaHash.ConvertFromHex()).ToArray() ?? Array.Empty<byte[]>(),
                        EventTypeIndexes = committedTransaction.Receipt.Events?.Select(x => x.Type.TypeReference.LocalTypeIndex.Index).ToArray() ?? Array.Empty<long>(),
                        EventSborTypeKinds = committedTransaction.Receipt.Events?.Select(x => x.Type.TypeReference.LocalTypeIndex.Kind.ToModel()).ToArray().ToArray() ?? Array.Empty<SborTypeKind>(),
                    };

                    ledgerTransactionsToAdd.Add(ledgerTransaction);

                    if (committedTransaction.Receipt.NextEpoch != null)
                    {
                        ledgerTransactionMarkersToAdd.Add(new OriginLedgerTransactionMarker
                        {
                            Id = sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OriginType = LedgerTransactionMarkerOriginType.EpochChange,
                        });
                    }

                    lastTransactionSummary = summary;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process transaction {StateVersion} at referenced entities scan stage", stateVersion);
                    throw;
                }
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
                    GlobalConsensusManager => CoreModel.EntityType.GlobalConsensusManager,
                    GlobalAccountEntity => CoreModel.EntityType.GlobalAccount,
                    GlobalFungibleResourceEntity => CoreModel.EntityType.GlobalFungibleResource,
                    GlobalNonFungibleResourceEntity => CoreModel.EntityType.GlobalNonFungibleResource,
                    GlobalValidatorEntity => CoreModel.EntityType.GlobalValidator,
                    InternalFungibleVaultEntity => CoreModel.EntityType.InternalFungibleVault,
                    InternalNonFungibleVaultEntity => CoreModel.EntityType.InternalNonFungibleVault,
                    GlobalPackageEntity => CoreModel.EntityType.GlobalPackage,
                    GlobalIdentityEntity => CoreModel.EntityType.GlobalIdentity,
                    GlobalAccessControllerEntity => CoreModel.EntityType.GlobalAccessController,
                    GlobalGenericComponentEntity => CoreModel.EntityType.GlobalGenericComponent,
                    InternalGenericComponentEntity => CoreModel.EntityType.InternalGenericComponent,
                    InternalKeyValueStoreEntity => CoreModel.EntityType.InternalKeyValueStore,
                    GlobalOneResourcePoolEntity => CoreModel.EntityType.GlobalOneResourcePool,
                    GlobalTwoResourcePoolEntity => CoreModel.EntityType.GlobalTwoResourcePool,
                    GlobalMultiResourcePoolEntity => CoreModel.EntityType.GlobalMultiResourcePool,
                    GlobalTransactionTrackerEntity => CoreModel.EntityType.GlobalTransactionTracker,
                    _ => throw new ArgumentOutOfRangeException(nameof(knownDbEntity), knownDbEntity.GetType().Name),
                };

                referencedEntities.GetOrAdd(knownDbEntity.Address, address => new ReferencedEntity(address, entityType, knownDbEntity.FromStateVersion));
            }

            foreach (var referencedEntity in referencedEntities.All)
            {
                if (knownDbEntities.TryGetValue(referencedEntity.Address, out var entity))
                {
                    referencedEntity.Resolve(entity);

                    continue;
                }

                Entity dbEntity = referencedEntity.Type switch
                {
                    CoreModel.EntityType.GlobalConsensusManager => new GlobalConsensusManager(),
                    CoreModel.EntityType.GlobalFungibleResource => new GlobalFungibleResourceEntity(),
                    CoreModel.EntityType.GlobalNonFungibleResource => new GlobalNonFungibleResourceEntity(),
                    CoreModel.EntityType.GlobalGenericComponent => new GlobalGenericComponentEntity(),
                    CoreModel.EntityType.InternalGenericComponent => new InternalGenericComponentEntity(),
                    CoreModel.EntityType.GlobalPackage => new GlobalPackageEntity(),
                    CoreModel.EntityType.InternalFungibleVault => new InternalFungibleVaultEntity(),
                    CoreModel.EntityType.InternalNonFungibleVault => new InternalNonFungibleVaultEntity(),
                    CoreModel.EntityType.InternalKeyValueStore => new InternalKeyValueStoreEntity(),
                    CoreModel.EntityType.GlobalAccessController => new GlobalAccessControllerEntity(),
                    CoreModel.EntityType.GlobalValidator => new GlobalValidatorEntity(),
                    CoreModel.EntityType.GlobalIdentity => new GlobalIdentityEntity(),
                    CoreModel.EntityType.GlobalAccount => new GlobalAccountEntity(),
                    CoreModel.EntityType.GlobalVirtualEd25519Identity => new GlobalIdentityEntity(),
                    CoreModel.EntityType.GlobalVirtualSecp256k1Identity => new GlobalIdentityEntity(),
                    CoreModel.EntityType.GlobalVirtualEd25519Account => new GlobalAccountEntity(),
                    CoreModel.EntityType.GlobalVirtualSecp256k1Account => new GlobalAccountEntity(),
                    CoreModel.EntityType.GlobalOneResourcePool => new GlobalOneResourcePoolEntity(),
                    CoreModel.EntityType.GlobalTwoResourcePool => new GlobalTwoResourcePoolEntity(),
                    CoreModel.EntityType.GlobalMultiResourcePool => new GlobalMultiResourcePoolEntity(),
                    CoreModel.EntityType.GlobalTransactionTracker => new GlobalTransactionTrackerEntity(),
                    _ => throw new ArgumentOutOfRangeException(nameof(referencedEntity.Type), referencedEntity.Type, "Unexpected entity type"),
                };

                dbEntity.Id = sequences.EntitySequence++;
                dbEntity.FromStateVersion = referencedEntity.StateVersion;
                dbEntity.Address = referencedEntity.Address;
                dbEntity.IsGlobal = referencedEntity.IsGlobal;

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

                referencedEntities
                    .Get(childAddress)
                    .PostResolveConfigure((Entity dbe) =>
                    {
                        dbe.AncestorIds = allAncestors;
                        dbe.ParentAncestorId = parentId.Value;
                        dbe.OwnerAncestorId = ownerId.Value;
                        dbe.GlobalAncestorId = globalId.Value;
                    });
            }

            referencedEntities.InvokePostResolveConfiguration();
        }

        var fungibleVaultChanges = new List<FungibleVaultChange>();
        var nonFungibleVaultChanges = new List<NonFungibleVaultChange>();
        var entityFungibleResourceBalanceChangeEvents = new List<EntityFungibleResourceBalanceChangeEvent>();
        var entityNonFungibleResourceBalanceChangeEvents = new List<EntityNonFungibleResourceBalanceChangeEvent>();
        var nonFungibleIdChanges = new List<NonFungibleIdChange>();
        var metadataChanges = new List<MetadataChange>();
        var resourceSupplyChanges = new List<ResourceSupplyChange>();
        var validatorSetChanges = new List<ValidatorSetChange>();
        var stateToAdd = new List<StateHistory>();
        var keyValueStoreEntryHistoryToAdd = new List<KeyValueStoreEntryHistory>();
        var componentMethodRoyaltiesToAdd = new List<ComponentMethodRoyaltyEntryHistory>();
        var packageBlueprintHistoryToAdd = new Dictionary<PackageBlueprintLookup, PackageBlueprintHistory>();
        var packageCodeHistoryToAdd = new List<PackageCodeHistory>();
        var schemaHistoryToAdd = new List<SchemaHistory>();
        var nonFungibleSchemaHistoryToAdd = new List<NonFungibleSchemaHistory>();
        var keyValueStoreSchemaHistoryToAdd = new List<KeyValueStoreSchemaHistory>();
        var validatorKeyHistoryToAdd = new Dictionary<ValidatorKeyLookup, ValidatorPublicKeyHistory>(); // TODO follow Pointer+ordered List pattern to ensure proper order of ingestion
        var accountDefaultDepositRuleHistoryToAdd = new List<AccountDefaultDepositRuleHistory>();
        var accountResourcePreferenceRuleHistoryToAdd = new List<AccountResourcePreferenceRuleHistory>();
        var roleAssignmentsChangePointers = new Dictionary<RoleAssignmentsChangePointerLookup, RoleAssignmentsChangePointer>();
        var roleAssignmentChanges = new List<RoleAssignmentsChangePointerLookup>();
        var validatorEmissionStatisticsToAdd = new List<ValidatorEmissionStatistics>();

        // step: scan all substates to figure out changes
        {
            foreach (var committedTransaction in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = committedTransaction.ResultantStateIdentifiers.StateVersion;
                var stateUpdates = committedTransaction.Receipt.StateUpdates;
                long? newEpoch = null;
                var affectedGlobalEntities = new HashSet<long>();

                try
                {
                    foreach (var substate in stateUpdates.UpsertedSubstates)
                    {
                        var substateId = substate.SubstateId;
                        var substateData = substate.Value.SubstateData;
                        var referencedEntity = referencedEntities.Get((EntityAddress)substateId.EntityAddress);
                        affectedGlobalEntities.Add(referencedEntity.AffectedGlobalEntityId);

                        if (substateData is CoreModel.MetadataModuleEntrySubstate metadata)
                        {
                            var isDeleted = metadata.Value == null;
                            var key = metadata.Key.Name;
                            var value = metadata.Value?.DataStruct.StructData.Hex.ConvertFromHex();

                            metadataChanges.Add(new MetadataChange(referencedEntity, key, value, isDeleted, metadata.IsLocked, stateVersion));
                        }

                        if (substateData is CoreModel.FungibleVaultFieldBalanceSubstate fungibleVaultFieldBalanceSubstate)
                        {
                            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalFungibleVaultEntity>();
                            var resourceEntity = referencedEntities.GetByDatabaseId(vaultEntity.ResourceEntityId);
                            var amount = TokenAmount.FromDecimalString(fungibleVaultFieldBalanceSubstate.Value.Amount);

                            fungibleVaultChanges.Add(new FungibleVaultChange(referencedEntity, resourceEntity, amount, stateVersion));

                            if (!vaultEntity.IsRoyaltyVault)
                            {
                                var previousAmountRaw = (substate.PreviousValue?.SubstateData as CoreModel.FungibleVaultFieldBalanceSubstate)?.Value.Amount;
                                var previousAmount = previousAmountRaw == null ? TokenAmount.Zero : TokenAmount.FromDecimalString(previousAmountRaw);
                                var delta = amount - previousAmount;

                                entityFungibleResourceBalanceChangeEvents.Add(new EntityFungibleResourceBalanceChangeEvent(referencedEntity.DatabaseGlobalAncestorId, resourceEntity.DatabaseId,
                                    delta, stateVersion));

                                if (referencedEntity.DatabaseGlobalAncestorId != referencedEntity.DatabaseOwnerAncestorId)
                                {
                                    entityFungibleResourceBalanceChangeEvents.Add(new EntityFungibleResourceBalanceChangeEvent(referencedEntity.DatabaseOwnerAncestorId, resourceEntity.DatabaseId,
                                        delta, stateVersion));
                                }
                            }
                        }

                        if (substateData is CoreModel.NonFungibleVaultFieldBalanceSubstate nonFungibleVaultFieldBalanceSubstate)
                        {
                            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
                            var resourceEntity = referencedEntities.GetByDatabaseId(vaultEntity.ResourceEntityId);
                            var amount = long.Parse(nonFungibleVaultFieldBalanceSubstate.Value.Amount);

                            var previousAmountRaw = (substate.PreviousValue?.SubstateData as CoreModel.NonFungibleVaultFieldBalanceSubstate)?.Value.Amount;
                            var previousAmount = previousAmountRaw == null ? 0 : long.Parse(previousAmountRaw);
                            var delta = amount - previousAmount;

                            entityNonFungibleResourceBalanceChangeEvents.Add(new EntityNonFungibleResourceBalanceChangeEvent(referencedEntity.DatabaseGlobalAncestorId, resourceEntity.DatabaseId,
                                delta, stateVersion));

                            if (referencedEntity.DatabaseGlobalAncestorId != referencedEntity.DatabaseOwnerAncestorId)
                            {
                                entityNonFungibleResourceBalanceChangeEvents.Add(new EntityNonFungibleResourceBalanceChangeEvent(referencedEntity.DatabaseOwnerAncestorId, resourceEntity.DatabaseId,
                                    delta, stateVersion));
                            }
                        }

                        if (substateData is CoreModel.NonFungibleVaultContentsIndexEntrySubstate nonFungibleVaultContentsIndexEntrySubstate)
                        {
                            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
                            var resourceEntity = referencedEntities.GetByDatabaseId(vaultEntity.ResourceEntityId);

                            nonFungibleVaultChanges.Add(new NonFungibleVaultChange(
                                referencedEntity,
                                resourceEntity,
                                nonFungibleVaultContentsIndexEntrySubstate.Key.NonFungibleLocalId.SimpleRep,
                                false,
                                stateVersion));
                        }

                        if (substateData is CoreModel.NonFungibleResourceManagerDataEntrySubstate nonFungibleResourceManagerDataEntrySubstate)
                        {
                            var resourceManagerEntityId = substateId.EntityAddress;
                            var resourceManagerEntity = referencedEntities.Get((EntityAddress)resourceManagerEntityId);

                            var nonFungibleId = ScryptoSborUtils.GetNonFungibleId((substateId.SubstateKey as CoreModel.MapSubstateKey)!.KeyHex);

                            nonFungibleIdChanges.Add(new NonFungibleIdChange(
                                resourceManagerEntity,
                                nonFungibleId,
                                nonFungibleResourceManagerDataEntrySubstate.Value == null,
                                nonFungibleResourceManagerDataEntrySubstate.IsLocked,
                                nonFungibleResourceManagerDataEntrySubstate.Value?.DataStruct.StructData.GetDataBytes(),
                                stateVersion));
                        }

                        if (substateData is CoreModel.GenericScryptoComponentFieldStateSubstate componentState)
                        {
                            if (substate.SystemStructure is not CoreModel.ObjectFieldStructure objectFieldStructure)
                            {
                                throw new UnreachableException($"Generic Scrypto components are expected to have ObjectFieldStructure. Got: {substate.SystemStructure.GetType()}");
                            }

                            var schemaDetails = objectFieldStructure.ValueSchema.GetSchemaDetails();

                            stateToAdd.Add(new SborStateHistory
                            {
                                Id = sequences.StateHistorySequence++,
                                FromStateVersion = stateVersion,
                                EntityId = referencedEntities.Get((EntityAddress)substateId.EntityAddress).DatabaseId,
                                SborState = componentState.Value.DataStruct.StructData.GetDataBytes(),
                                SchemaHash = schemaDetails.SchemaHash.ConvertFromHex(),
                                SborTypeKind = schemaDetails.SborTypeKind.ToModel(),
                                TypeIndex = schemaDetails.TypeIndex,
                                SchemaDefiningEntityId = referencedEntities.Get((EntityAddress)schemaDetails.SchemaDefiningEntityAddress).DatabaseId,
                            });
                        }

                        if (substateData is CoreModel.GenericKeyValueStoreEntrySubstate genericKeyValueStoreEntry)
                        {
                            keyValueStoreEntryHistoryToAdd.Add(new KeyValueStoreEntryHistory
                            {
                                Id = sequences.KeyValueStoreEntryHistorySequence++,
                                FromStateVersion = stateVersion,
                                KeyValueStoreEntityId = referencedEntity.DatabaseId,
                                Key = genericKeyValueStoreEntry.Key.KeyData.GetDataBytes(),
                                Value = genericKeyValueStoreEntry.Value?.Data.StructData.GetDataBytes(),
                                IsDeleted = genericKeyValueStoreEntry.Value == null,
                                IsLocked = genericKeyValueStoreEntry.IsLocked,
                            });
                        }

                        if (substateData is CoreModel.ValidatorFieldStateSubstate validator)
                        {
                            var lookup = new ValidatorKeyLookup(referencedEntities.Get((EntityAddress)substateId.EntityAddress).DatabaseId, validator.Value.PublicKey.KeyType.ToModel(),
                                validator.Value.PublicKey.GetKeyBytes());

                            validatorKeyHistoryToAdd[lookup] = new ValidatorPublicKeyHistory
                            {
                                Id = sequences.ValidatorPublicKeyHistorySequence++,
                                FromStateVersion = stateVersion,
                                ValidatorEntityId = lookup.ValidatorEntityId,
                                KeyType = lookup.PublicKeyType,
                                Key = lookup.PublicKey,
                            };

                            stateToAdd.Add(new JsonStateHistory
                            {
                                Id = sequences.StateHistorySequence++,
                                FromStateVersion = stateVersion,
                                EntityId = referencedEntities.Get((EntityAddress)substateId.EntityAddress).DatabaseId,
                                JsonState = validator.Value.ToJson(),
                            });
                        }

                        if (substateData is CoreModel.ConsensusManagerFieldStateSubstate consensusManagerFieldStateSubstate)
                        {
                            if (consensusManagerFieldStateSubstate.Value.Round == 0)
                            {
                                newEpoch = consensusManagerFieldStateSubstate.Value.Epoch;
                            }
                        }

                        if (substateData is CoreModel.ConsensusManagerFieldCurrentValidatorSetSubstate validatorSet)
                        {
                            var change = validatorSet.Value.ValidatorSet
                                .ToDictionary(
                                    v =>
                                    {
                                        var vid = referencedEntities.Get((EntityAddress)v.Address).DatabaseId;

                                        return new ValidatorKeyLookup(vid, v.Key.KeyType.ToModel(), v.Key.GetKeyBytes());
                                    },
                                    v => TokenAmount.FromDecimalString(v.Stake));

                            validatorSetChanges.Add(new ValidatorSetChange(newEpoch!.Value, change, stateVersion));
                        }

                        if (substateData is CoreModel.AccountFieldStateSubstate accountFieldState)
                        {
                            accountDefaultDepositRuleHistoryToAdd.Add(new AccountDefaultDepositRuleHistory
                            {
                                Id = sequences.AccountDefaultDepositRuleHistorySequence++,
                                FromStateVersion = stateVersion,
                                AccountEntityId = referencedEntity.DatabaseId,
                                DefaultDepositRule = accountFieldState.Value.DefaultDepositRule.ToModel(),
                            });

                            stateToAdd.Add(new JsonStateHistory
                            {
                                Id = sequences.StateHistorySequence++,
                                FromStateVersion = stateVersion,
                                EntityId = referencedEntities.Get((EntityAddress)substateId.EntityAddress).DatabaseId,
                                JsonState = accountFieldState.Value.ToJson(),
                            });
                        }

                        if (substateData is CoreModel.AccountResourcePreferenceEntrySubstate accountDepositRule)
                        {
                            accountResourcePreferenceRuleHistoryToAdd.Add(new AccountResourcePreferenceRuleHistory
                            {
                                Id = sequences.AccountResourceDepositRuleHistorySequence++,
                                FromStateVersion = stateVersion,
                                AccountEntityId = referencedEntity.DatabaseId,
                                ResourceEntityId = referencedEntities.Get((EntityAddress)accountDepositRule.Key.ResourceAddress).DatabaseId,
                                AccountResourcePreferenceRule = accountDepositRule.Value?.ResourcePreference.ToModel(),
                                IsDeleted = accountDepositRule.Value == null,
                            });
                        }

                        if (substateData is CoreModel.RoleAssignmentModuleFieldOwnerRoleSubstate accessRulesFieldOwnerRole)
                        {
                            roleAssignmentsChangePointers
                                .GetOrAdd(new RoleAssignmentsChangePointerLookup(referencedEntity.DatabaseId, stateVersion), lookup =>
                                {
                                    roleAssignmentChanges.Add(lookup);

                                    return new RoleAssignmentsChangePointer(referencedEntity, stateVersion);
                                })
                                .OwnerRole = accessRulesFieldOwnerRole;
                        }

                        if (substateData is CoreModel.RoleAssignmentModuleRuleEntrySubstate roleAssignmentEntry)
                        {
                            roleAssignmentsChangePointers
                                .GetOrAdd(new RoleAssignmentsChangePointerLookup(referencedEntity.DatabaseId, stateVersion), lookup =>
                                {
                                    roleAssignmentChanges.Add(lookup);

                                    return new RoleAssignmentsChangePointer(referencedEntity, stateVersion);
                                })
                                .Entries
                                .Add(roleAssignmentEntry);
                        }

                        if (substateData is CoreModel.PackageBlueprintDefinitionEntrySubstate packageBlueprintDefinition)
                        {
                            var lookup = new PackageBlueprintLookup(referencedEntity.DatabaseId, packageBlueprintDefinition.Key.BlueprintName, packageBlueprintDefinition.Key.BlueprintVersion);

                            packageBlueprintHistoryToAdd
                                .GetOrAdd(lookup, _ => new PackageBlueprintHistory
                                {
                                    Id = sequences.PackageBlueprintHistorySequence++,
                                    FromStateVersion = stateVersion,
                                    PackageEntityId = referencedEntity.DatabaseId,
                                    Name = lookup.Name,
                                    Version = lookup.BlueprintVersion,
                                })
                                .Definition = packageBlueprintDefinition.Value.Definition.ToJson();
                        }

                        if (substateData is CoreModel.PackageBlueprintDependenciesEntrySubstate packageBlueprintDependencies)
                        {
                            var lookup = new PackageBlueprintLookup(referencedEntity.DatabaseId, packageBlueprintDependencies.Key.BlueprintName, packageBlueprintDependencies.Key.BlueprintVersion);

                            packageBlueprintHistoryToAdd
                                    .GetOrAdd(lookup, _ => new PackageBlueprintHistory
                                    {
                                        Id = sequences.PackageBlueprintHistorySequence++,
                                        FromStateVersion = stateVersion,
                                        PackageEntityId = referencedEntity.DatabaseId,
                                        Name = lookup.Name,
                                        Version = lookup.BlueprintVersion,
                                    })
                                    .DependantEntityIds =
                                packageBlueprintDependencies.Value.Dependencies.Dependencies.Select(address => referencedEntities.Get((EntityAddress)address).DatabaseId).ToList();
                        }

                        if (substateData is CoreModel.PackageBlueprintRoyaltyEntrySubstate packageBlueprintRoyalty)
                        {
                            var lookup = new PackageBlueprintLookup(referencedEntity.DatabaseId, packageBlueprintRoyalty.Key.BlueprintName, packageBlueprintRoyalty.Key.BlueprintVersion);
                            var pb = packageBlueprintHistoryToAdd.GetOrAdd(lookup, _ => new PackageBlueprintHistory
                            {
                                Id = sequences.PackageBlueprintHistorySequence++,
                                FromStateVersion = stateVersion,
                                PackageEntityId = referencedEntity.DatabaseId,
                                Name = lookup.Name,
                                Version = lookup.BlueprintVersion,
                            });

                            pb.RoyaltyConfig = packageBlueprintRoyalty.Value.RoyaltyConfig.ToJson();
                            pb.RoyaltyConfigIsLocked = packageBlueprintRoyalty.IsLocked;
                        }

                        if (substateData is CoreModel.PackageBlueprintAuthTemplateEntrySubstate packageBlueprintAuthTemplate)
                        {
                            var lookup = new PackageBlueprintLookup(referencedEntity.DatabaseId, packageBlueprintAuthTemplate.Key.BlueprintName, packageBlueprintAuthTemplate.Key.BlueprintVersion);
                            var pb = packageBlueprintHistoryToAdd.GetOrAdd(lookup, _ => new PackageBlueprintHistory
                            {
                                Id = sequences.PackageBlueprintHistorySequence++,
                                FromStateVersion = stateVersion,
                                PackageEntityId = referencedEntity.DatabaseId,
                                Name = lookup.Name,
                                Version = lookup.BlueprintVersion,
                            });

                            pb.AuthTemplate = packageBlueprintAuthTemplate.Value.AuthConfig.ToJson();
                            pb.AuthTemplateIsLocked = packageBlueprintAuthTemplate.IsLocked;
                        }

                        if (substateData is CoreModel.PackageCodeOriginalCodeEntrySubstate packageCodeOriginalCode)
                        {
                            packageCodeHistoryToAdd.Add(new PackageCodeHistory
                            {
                                Id = sequences.PackageCodeHistorySequence++,
                                FromStateVersion = stateVersion,
                                PackageEntityId = referencedEntity.DatabaseId,
                                CodeHash = packageCodeOriginalCode.Key.CodeHash.ConvertFromHex(),
                                Code = packageCodeOriginalCode.Value.CodeHex.ConvertFromHex(),
                            });
                        }

                        if (substateData is CoreModel.SchemaEntrySubstate schema)
                        {
                            schemaHistoryToAdd.Add(new SchemaHistory
                            {
                                Id = sequences.SchemaHistorySequence++,
                                FromStateVersion = stateVersion,
                                EntityId = referencedEntity.DatabaseId,
                                SchemaHash = schema.Key.SchemaHash.ConvertFromHex(),
                                Schema = schema.Value.Schema.SborData.Hex.ConvertFromHex(),
                            });
                        }

                        if (substateData is CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate methodRoyaltyEntry)
                        {
                            componentMethodRoyaltiesToAdd.Add(new ComponentMethodRoyaltyEntryHistory
                            {
                                Id = sequences.ComponentMethodRoyaltyEntryHistorySequence++,
                                FromStateVersion = stateVersion,
                                EntityId = referencedEntity.DatabaseId,
                                MethodName = methodRoyaltyEntry.Key.MethodName,
                                RoyaltyAmount = methodRoyaltyEntry.Value?.ToJson(),
                                IsLocked = methodRoyaltyEntry.IsLocked,
                            });
                        }

                        if (substateData is CoreModel.TypeInfoModuleFieldTypeInfoSubstate typeInfoSubstate)
                        {
                            if (typeInfoSubstate.TryGetNonFungibleDataSchemaDetails(out var nonFungibleDataSchemaDetails))
                            {
                                nonFungibleSchemaHistoryToAdd.Add(new NonFungibleSchemaHistory
                                {
                                    Id = sequences.NonFungibleSchemaHistorySequence++,
                                    ResourceEntityId = referencedEntity.DatabaseId,
                                    SchemaHash = nonFungibleDataSchemaDetails.SchemaHash.ConvertFromHex(),
                                    SborTypeKind = nonFungibleDataSchemaDetails.LocalTypeIndex.Kind.ToModel(),
                                    TypeIndex = nonFungibleDataSchemaDetails.LocalTypeIndex.Index,
                                    FromStateVersion = stateVersion,
                                });
                            }

                            if (typeInfoSubstate.Value.Details is CoreModel.KeyValueStoreTypeInfoDetails keyValueStoreTypeInfoDetails)
                            {
                                var keySchemaDetails = keyValueStoreTypeInfoDetails.KeyValueStoreInfo.KeyGenericSubstitution;
                                var valueSchemaDetails = keyValueStoreTypeInfoDetails.KeyValueStoreInfo.ValueGenericSubstitution;

                                keyValueStoreSchemaHistoryToAdd.Add(new KeyValueStoreSchemaHistory
                                {
                                    Id = sequences.KeyValueSchemaHistorySequence++,
                                    KeyValueStoreEntityId = referencedEntity.DatabaseId,
                                    KeySchemaHash = keySchemaDetails.SchemaHash.ConvertFromHex(),
                                    KeySborTypeKind = keySchemaDetails.LocalTypeIndex.Kind.ToModel(),
                                    KeyTypeIndex = keySchemaDetails.LocalTypeIndex.Index,
                                    ValueSchemaHash = valueSchemaDetails.SchemaHash.ConvertFromHex(),
                                    ValueSborTypeKind = valueSchemaDetails.LocalTypeIndex.Kind.ToModel(),
                                    ValueTypeIndex = valueSchemaDetails.LocalTypeIndex.Index,
                                    FromStateVersion = stateVersion,
                                });
                            }
                        }

                        if (substateData is CoreModel.AccessControllerFieldStateSubstate accessControllerFieldState)
                        {
                            stateToAdd.Add(new JsonStateHistory
                            {
                                Id = sequences.StateHistorySequence++,
                                FromStateVersion = stateVersion,
                                EntityId = referencedEntities.Get((EntityAddress)substateId.EntityAddress).DatabaseId,
                                JsonState = accessControllerFieldState.Value.ToJson(),
                            });
                        }

                        if (substateData is CoreModel.OneResourcePoolFieldStateSubstate oneResourcePoolFieldStateSubstate)
                        {
                            stateToAdd.Add(new JsonStateHistory
                            {
                                Id = sequences.StateHistorySequence++,
                                FromStateVersion = stateVersion,
                                EntityId = referencedEntities.Get((EntityAddress)substateId.EntityAddress).DatabaseId,
                                JsonState = oneResourcePoolFieldStateSubstate.Value.ToJson(),
                            });
                        }

                        if (substateData is CoreModel.TwoResourcePoolFieldStateSubstate twoResourcePoolFieldStateSubstate)
                        {
                            stateToAdd.Add(new JsonStateHistory
                            {
                                Id = sequences.StateHistorySequence++,
                                FromStateVersion = stateVersion,
                                EntityId = referencedEntities.Get((EntityAddress)substateId.EntityAddress).DatabaseId,
                                JsonState = twoResourcePoolFieldStateSubstate.Value.ToJson(),
                            });
                        }

                        if (substateData is CoreModel.MultiResourcePoolFieldStateSubstate multiResourcePoolFieldStateSubstate)
                        {
                            stateToAdd.Add(new JsonStateHistory
                            {
                                Id = sequences.StateHistorySequence++,
                                FromStateVersion = stateVersion,
                                EntityId = referencedEntities.Get((EntityAddress)substateId.EntityAddress).DatabaseId,
                                JsonState = multiResourcePoolFieldStateSubstate.Value.ToJson(),
                            });
                        }
                    }

                    foreach (var deletedSubstate in stateUpdates.DeletedSubstates)
                    {
                        var substateId = deletedSubstate.SubstateId;
                        var referencedEntity = referencedEntities.GetOrAdd((EntityAddress)substateId.EntityAddress, ea => new ReferencedEntity(ea, substateId.EntityType, stateVersion));
                        affectedGlobalEntities.Add(referencedEntity.AffectedGlobalEntityId);

                        if (substateId.SubstateType == CoreModel.SubstateType.NonFungibleVaultContentsIndexEntry)
                        {
                            var resourceEntity = referencedEntities.GetByDatabaseId(referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>().ResourceEntityId);
                            var nonFungibleId = ScryptoSborUtils.GetNonFungibleId((substateId.SubstateKey as CoreModel.MapSubstateKey)!.KeyHex);

                            nonFungibleVaultChanges.Add(new NonFungibleVaultChange(
                                referencedEntity,
                                resourceEntity,
                                nonFungibleId,
                                true,
                                stateVersion));
                        }
                    }

                    var transaction = ledgerTransactionsToAdd.Single(x => x.StateVersion == stateVersion);
                    transaction.AffectedGlobalEntities = affectedGlobalEntities.ToArray();

                    ledgerTransactionMarkersToAdd.AddRange(affectedGlobalEntities.Select(affectedEntity => new AffectedGlobalEntityTransactionMarker
                    {
                        Id = sequences.LedgerTransactionMarkerSequence++,
                        EntityId = affectedEntity,
                        StateVersion = stateVersion,
                    }));

                    foreach (var @event in committedTransaction.Receipt.Events)
                    {
                        if (@event.Type.Emitter is not CoreModel.MethodEventEmitterIdentifier methodEventEmitter
                            || methodEventEmitter.ObjectModuleId != CoreModel.ObjectModuleId.Main
                            || methodEventEmitter.Entity.EntityType == CoreModel.EntityType.GlobalGenericComponent
                            || methodEventEmitter.Entity.EntityType == CoreModel.EntityType.InternalGenericComponent)
                        {
                            continue;
                        }

                        var eventEmitterEntity = referencedEntities.Get((EntityAddress)methodEventEmitter.Entity.EntityAddress);

                        using var decodedEvent = EventDecoder.DecodeEvent(@event, await _networkConfigurationProvider.GetNetworkId());

                        if (EventDecoder.TryGetValidatorEmissionsAppliedEvent(decodedEvent, out var validatorUptimeEvent))
                        {
                            validatorEmissionStatisticsToAdd.Add(new ValidatorEmissionStatistics
                            {
                                Id = sequences.ValidatorEmissionStatisticsSequence++,
                                ValidatorEntityId = eventEmitterEntity.DatabaseId,
                                EpochNumber = (long)validatorUptimeEvent.epoch,
                                ProposalsMade = (long)validatorUptimeEvent.proposalsMade,
                                ProposalsMissed = (long)validatorUptimeEvent.proposalsMissed,
                            });
                        }
                        else if (EventDecoder.TryGetFungibleVaultWithdrawalEvent(decodedEvent, out var fungibleVaultWithdrawalEvent))
                        {
                            ledgerTransactionMarkersToAdd.Add(new EventLedgerTransactionMarker
                            {
                                Id = sequences.LedgerTransactionMarkerSequence++,
                                StateVersion = stateVersion,
                                EventType = LedgerTransactionMarkerEventType.Withdrawal,
                                EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                                ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalFungibleVaultEntity>().ResourceEntityId,
                                Quantity = TokenAmount.FromDecimalString(fungibleVaultWithdrawalEvent.AsStr()),
                            });
                        }
                        else if (EventDecoder.TryGetFungibleVaultDepositEvent(decodedEvent, out var fungibleVaultDepositEvent))
                        {
                            ledgerTransactionMarkersToAdd.Add(new EventLedgerTransactionMarker
                            {
                                Id = sequences.LedgerTransactionMarkerSequence++,
                                StateVersion = stateVersion,
                                EventType = LedgerTransactionMarkerEventType.Deposit,
                                EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                                ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalFungibleVaultEntity>().ResourceEntityId,
                                Quantity = TokenAmount.FromDecimalString(fungibleVaultDepositEvent.AsStr()),
                            });
                        }
                        else if (EventDecoder.TryGetNonFungibleVaultWithdrawalEvent(decodedEvent, out var nonFungibleVaultWithdrawalEvent))
                        {
                            ledgerTransactionMarkersToAdd.Add(new EventLedgerTransactionMarker
                            {
                                Id = sequences.LedgerTransactionMarkerSequence++,
                                StateVersion = stateVersion,
                                EventType = LedgerTransactionMarkerEventType.Withdrawal,
                                EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                                ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>().ResourceEntityId,
                                Quantity = TokenAmount.FromDecimalString(nonFungibleVaultWithdrawalEvent.Count.ToString()),
                            });
                        }
                        else if (EventDecoder.TryGetNonFungibleVaultDepositEvent(decodedEvent, out var nonFungibleVaultDepositEvent))
                        {
                            ledgerTransactionMarkersToAdd.Add(new EventLedgerTransactionMarker
                            {
                                Id = sequences.LedgerTransactionMarkerSequence++,
                                StateVersion = stateVersion,
                                EventType = LedgerTransactionMarkerEventType.Deposit,
                                EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                                ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>().ResourceEntityId,
                                Quantity = TokenAmount.FromDecimalString(nonFungibleVaultDepositEvent.Count.ToString()),
                            });
                        }
                        else if (EventDecoder.TryGetFungibleResourceMintedEvent(decodedEvent, out var fungibleResourceMintedEvent))
                        {
                            var mintedAmount = TokenAmount.FromDecimalString(fungibleResourceMintedEvent.amount.AsStr());
                            resourceSupplyChanges.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Minted: mintedAmount));
                        }
                        else if (EventDecoder.TryGetFungibleResourceBurnedEvent(decodedEvent, out var fungibleResourceBurnedEvent))
                        {
                            var burnedAmount = TokenAmount.FromDecimalString(fungibleResourceBurnedEvent.amount.AsStr());
                            resourceSupplyChanges.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Burned: burnedAmount));
                        }
                        else if (EventDecoder.TryGetNonFungibleResourceMintedEvent(decodedEvent, out var nonFungibleResourceMintedEvent))
                        {
                            var mintedCount = TokenAmount.FromDecimalString(nonFungibleResourceMintedEvent.ids.Count.ToString());
                            resourceSupplyChanges.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Minted: mintedCount));
                        }
                        else if (EventDecoder.TryGetNonFungibleResourceBurnedEvent(decodedEvent, out var nonFungibleResourceBurnedEvent))
                        {
                            var burnedCount = TokenAmount.FromDecimalString(nonFungibleResourceBurnedEvent.ids.Count.ToString());
                            resourceSupplyChanges.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Burned: burnedCount));
                        }
                    }

                    if (manifestExtractedAddresses.TryGetValue(stateVersion, out var extractedAddresses))
                    {
                        foreach (var address in extractedAddresses.ResourceAddresses)
                        {
                            if (referencedEntities.TryGet(address, out var re))
                            {
                                ledgerTransactionMarkersToAdd.Add(new ManifestAddressLedgerTransactionMarker
                                {
                                    Id = sequences.LedgerTransactionMarkerSequence++,
                                    StateVersion = stateVersion,
                                    OperationType = LedgerTransactionMarkerOperationType.ResourceInUse,
                                    EntityId = re.DatabaseId,
                                });
                            }
                        }

                        foreach (var address in extractedAddresses.AccountsDepositedInto)
                        {
                            if (referencedEntities.TryGet(address, out var re))
                            {
                                ledgerTransactionMarkersToAdd.Add(new ManifestAddressLedgerTransactionMarker
                                {
                                    Id = sequences.LedgerTransactionMarkerSequence++,
                                    StateVersion = stateVersion,
                                    OperationType = LedgerTransactionMarkerOperationType.AccountDepositedInto,
                                    EntityId = re.DatabaseId,
                                });
                            }
                        }

                        foreach (var address in extractedAddresses.AccountsWithdrawnFrom)
                        {
                            if (referencedEntities.TryGet(address, out var re))
                            {
                                ledgerTransactionMarkersToAdd.Add(new ManifestAddressLedgerTransactionMarker
                                {
                                    Id = sequences.LedgerTransactionMarkerSequence++,
                                    StateVersion = stateVersion,
                                    OperationType = LedgerTransactionMarkerOperationType.AccountWithdrawnFrom,
                                    EntityId = re.DatabaseId,
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process transaction {StateVersion} at substate processing stage", stateVersion);
                    throw;
                }
            }
        }

        // step: now that all the fundamental data is inserted (entities & substates) we can insert some denormalized data
        {
            var sw = Stopwatch.StartNew();

            var mostRecentMetadataHistory = await readHelper.MostRecentEntityMetadataHistoryFor(metadataChanges, token);
            var mostRecentAggregatedMetadataHistory = await readHelper.MostRecentEntityAggregateMetadataHistoryFor(metadataChanges, token);
            var mostRecentAccessRulesEntryHistory = await readHelper.MostRecentEntityRoleAssignmentsEntryHistoryFor(roleAssignmentsChangePointers.Values, token);
            var mostRecentAccessRulesAggregateHistory = await readHelper.MostRecentEntityRoleAssignmentsAggregateHistoryFor(roleAssignmentChanges, token);
            var mostRecentEntityResourceAggregateHistory = await readHelper.MostRecentEntityResourceAggregateHistoryFor(fungibleVaultChanges, nonFungibleVaultChanges, token);
            var mostRecentEntityResourceAggregatedVaultsHistory =
                await readHelper.MostRecentEntityResourceAggregatedVaultsHistoryFor(entityFungibleResourceBalanceChangeEvents, entityNonFungibleResourceBalanceChangeEvents, token);
            var mostRecentEntityResourceVaultAggregateHistory = await readHelper.MostRecentEntityResourceVaultAggregateHistoryFor(fungibleVaultChanges, nonFungibleVaultChanges, token);
            var mostRecentNonFungibleIdStoreHistory = await readHelper.MostRecentNonFungibleIdStoreHistoryFor(nonFungibleIdChanges, token);
            var mostRecentResourceEntitySupplyHistory = await readHelper.MostRecentResourceEntitySupplyHistoryFor(resourceSupplyChanges, token);
            var mostRecentEntityNonFungibleVaultHistory = await readHelper.MostRecentEntityNonFungibleVaultHistory(nonFungibleVaultChanges, token);
            var existingNonFungibleIdData = await readHelper.ExistingNonFungibleIdDataFor(nonFungibleIdChanges, nonFungibleVaultChanges, token);
            var existingValidatorKeys = await readHelper.ExistingValidatorKeysFor(validatorSetChanges, token);

            dbReadDuration += sw.Elapsed;

            var entityMetadataHistoryToAdd = new List<EntityMetadataHistory>();
            var entityMetadataAggregateHistoryToAdd = new List<EntityMetadataAggregateHistory>();
            var entityResourceAggregateHistoryCandidates = new List<EntityResourceAggregateHistory>();
            var entityResourceAggregatedVaultsHistoryToAdd = new List<EntityResourceAggregatedVaultsHistory>();
            var entityResourceVaultAggregateHistoryToAdd = new List<EntityResourceVaultAggregateHistory>();
            var entityAccessRulesOwnerRoleHistoryToAdd = new List<EntityRoleAssignmentsOwnerRoleHistory>();
            var entityAccessRulesEntryHistoryToAdd = new List<EntityRoleAssignmentsEntryHistory>();
            var entityAccessRulesAggregateHistoryToAdd = new List<EntityRoleAssignmentsAggregateHistory>();
            var nonFungibleIdStoreHistoryToAdd = new Dictionary<NonFungibleStoreLookup, NonFungibleIdStoreHistory>();
            var nonFungibleIdDataToAdd = new List<NonFungibleIdData>();
            var nonFungibleIdsMutableDataHistoryToAdd = new List<NonFungibleIdDataHistory>();

            foreach (var metadataChange in metadataChanges)
            {
                var lookup = new MetadataLookup(metadataChange.ReferencedEntity.DatabaseId, metadataChange.Key);
                var metadataHistory = new EntityMetadataHistory
                {
                    Id = sequences.EntityMetadataHistorySequence++,
                    FromStateVersion = metadataChange.StateVersion,
                    EntityId = metadataChange.ReferencedEntity.DatabaseId,
                    Key = metadataChange.Key,
                    Value = metadataChange.Value,
                    IsDeleted = metadataChange.IsDeleted,
                    IsLocked = metadataChange.IsLocked,
                };

                entityMetadataHistoryToAdd.Add(metadataHistory);

                EntityMetadataAggregateHistory aggregate;

                if (!mostRecentAggregatedMetadataHistory.TryGetValue(metadataChange.ReferencedEntity.DatabaseId, out var previousAggregate) ||
                    previousAggregate.FromStateVersion != metadataChange.StateVersion)
                {
                    aggregate = new EntityMetadataAggregateHistory
                    {
                        Id = sequences.EntityMetadataAggregateHistorySequence++,
                        FromStateVersion = metadataChange.StateVersion,
                        EntityId = metadataChange.ReferencedEntity.DatabaseId,
                        MetadataIds = new List<long>(),
                    };

                    if (previousAggregate != null)
                    {
                        aggregate.MetadataIds.AddRange(previousAggregate.MetadataIds);
                    }

                    entityMetadataAggregateHistoryToAdd.Add(aggregate);
                    mostRecentAggregatedMetadataHistory[metadataChange.ReferencedEntity.DatabaseId] = aggregate;
                }
                else
                {
                    aggregate = previousAggregate;
                }

                if (mostRecentMetadataHistory.TryGetValue(lookup, out var previous))
                {
                    var currentPosition = aggregate.MetadataIds.IndexOf(previous.Id);

                    if (currentPosition != -1)
                    {
                        aggregate.MetadataIds.RemoveAt(currentPosition);
                    }
                }

                if (!metadataChange.IsDeleted)
                {
                    aggregate.MetadataIds.Insert(0, metadataHistory.Id);
                }

                mostRecentMetadataHistory[lookup] = metadataHistory;
            }

            foreach (var lookup in roleAssignmentChanges)
            {
                var accessRuleChange = roleAssignmentsChangePointers[lookup];

                EntityRoleAssignmentsOwnerRoleHistory? ownerRole = null;

                if (accessRuleChange.OwnerRole != null)
                {
                    ownerRole = new EntityRoleAssignmentsOwnerRoleHistory
                    {
                        Id = sequences.EntityRoleAssignmentsOwnerRoleHistorySequence++,
                        FromStateVersion = lookup.StateVersion,
                        EntityId = lookup.EntityId,
                        RoleAssignments = accessRuleChange.OwnerRole.Value.OwnerRole.ToJson(),
                    };

                    entityAccessRulesOwnerRoleHistoryToAdd.Add(ownerRole);
                }

                EntityRoleAssignmentsAggregateHistory aggregate;

                if (!mostRecentAccessRulesAggregateHistory.TryGetValue(lookup.EntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
                {
                    aggregate = new EntityRoleAssignmentsAggregateHistory
                    {
                        Id = sequences.EntityRoleAssignmentsAggregateHistorySequence++,
                        FromStateVersion = lookup.StateVersion,
                        EntityId = lookup.EntityId,
                        OwnerRoleId = ownerRole?.Id ?? previousAggregate?.OwnerRoleId ?? throw new InvalidOperationException("Unable to determine OwnerRoleId"),
                        EntryIds = new List<long>(),
                    };

                    if (previousAggregate != null)
                    {
                        aggregate.EntryIds.AddRange(previousAggregate.EntryIds);
                    }

                    entityAccessRulesAggregateHistoryToAdd.Add(aggregate);
                    mostRecentAccessRulesAggregateHistory[lookup.EntityId] = aggregate;
                }
                else
                {
                    aggregate = previousAggregate;
                }

                foreach (var entry in accessRuleChange.Entries)
                {
                    var entryLookup = new RoleAssignmentEntryLookup(lookup.EntityId, entry.Key.RoleKey, entry.Key.ObjectModuleId.ToModel());
                    var entryHistory = new EntityRoleAssignmentsEntryHistory
                    {
                        Id = sequences.EntityRoleAssignmentsEntryHistorySequence++,
                        FromStateVersion = lookup.StateVersion,
                        EntityId = lookup.EntityId,
                        KeyRole = entry.Key.RoleKey,
                        KeyModule = entry.Key.ObjectModuleId.ToModel(),
                        RoleAssignments = entry.Value?.AccessRule.ToJson(),
                        IsDeleted = entry.Value == null,
                    };

                    entityAccessRulesEntryHistoryToAdd.Add(entryHistory);

                    if (mostRecentAccessRulesEntryHistory.TryGetValue(entryLookup, out var previousEntry))
                    {
                        var currentPosition = aggregate.EntryIds.IndexOf(previousEntry.Id);

                        if (currentPosition != -1)
                        {
                            aggregate.EntryIds.RemoveAt(currentPosition);
                        }
                    }

                    // !entry.IsDeleted
                    if (entry.Value != null)
                    {
                        aggregate.EntryIds.Insert(0, entryHistory.Id);
                    }

                    mostRecentAccessRulesEntryHistory[entryLookup] = entryHistory;
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
                        NonFungibleResourceEntityId = e.ReferencedResource.DatabaseId,
                        NonFungibleId = e.NonFungibleId,
                    };

                    nonFungibleIdDataToAdd.Add(ret);

                    return ret;
                });

                var nonFungibleIdStore = nonFungibleIdStoreHistoryToAdd.GetOrAdd(new NonFungibleStoreLookup(e.ReferencedResource.DatabaseId, e.StateVersion), _ =>
                {
                    IEnumerable<long> previousNonFungibleIdDataIds = mostRecentNonFungibleIdStoreHistory.TryGetValue(e.ReferencedResource.DatabaseId, out var value)
                        ? value.NonFungibleIdDataIds
                        : Array.Empty<long>();

                    var ret = new NonFungibleIdStoreHistory
                    {
                        Id = sequences.NonFungibleIdStoreHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        NonFungibleResourceEntityId = e.ReferencedResource.DatabaseId,
                        NonFungibleIdDataIds = new List<long>(previousNonFungibleIdDataIds),
                    };

                    mostRecentNonFungibleIdStoreHistory[e.ReferencedResource.DatabaseId] = ret;

                    return ret;
                });

                nonFungibleIdsMutableDataHistoryToAdd.Add(new NonFungibleIdDataHistory
                {
                    Id = sequences.NonFungibleIdDataHistorySequence++,
                    FromStateVersion = e.StateVersion,
                    NonFungibleIdDataId = nonFungibleIdData.Id,
                    Data = e.MutableData,
                    IsDeleted = e.IsDeleted,
                    IsLocked = e.IsLocked,
                });

                if (!nonFungibleIdStore.NonFungibleIdDataIds.Contains(nonFungibleIdData.Id))
                {
                    nonFungibleIdStore.NonFungibleIdDataIds.Add(nonFungibleIdData.Id);
                }
            }

            void AggregateEntityResourceUsingSubstates(
                ReferencedEntity referencedVault,
                ReferencedEntity referencedResource,
                long stateVersion,
                bool fungibleResource)
            {
                if (referencedVault.GetDatabaseEntity<VaultEntity>() is InternalFungibleVaultEntity { IsRoyaltyVault: true })
                {
                    return;
                }

                AggregateEntityResourceInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId);
                AggregateEntityResourceVaultInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId, referencedVault.DatabaseId);

                if (referencedVault.DatabaseGlobalAncestorId != referencedVault.DatabaseOwnerAncestorId)
                {
                    AggregateEntityResourceInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId);
                    AggregateEntityResourceVaultInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId, referencedVault.DatabaseId);
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
                    AggregateEntityResourceUsingSubstates(e.ReferencedVault, e.ReferencedResource, e.StateVersion, true);

                    return new EntityFungibleVaultHistory
                    {
                        Id = sequences.EntityVaultHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.ReferencedResource.DatabaseId,
                        VaultEntityId = e.ReferencedVault.DatabaseId,
                        IsRoyaltyVault = e.ReferencedVault.GetDatabaseEntity<InternalFungibleVaultEntity>().IsRoyaltyVault,
                        Balance = e.Balance,
                    };
                })
                .ToList();

            var entityNonFungibleVaultHistoryToAdd = nonFungibleVaultChanges
                .GroupBy(x => new { x.StateVersion, x.ReferencedVault, x.ReferencedResource })
                .Select(e =>
                {
                    var vaultExists = mostRecentEntityNonFungibleVaultHistory.TryGetValue(e.Key.ReferencedVault.DatabaseId, out var existingVaultHistory);

                    var nfids = vaultExists ? existingVaultHistory!.NonFungibleIds : new List<long>();
                    var addedItems = e
                        .Where(x => !x.IsWithdrawal)
                        .Select(x => existingNonFungibleIdData[new NonFungibleIdLookup(e.Key.ReferencedResource.DatabaseId, x.NonFungibleId)].Id)
                        .ToList();

                    var deletedItems = e
                        .Where(x => x.IsWithdrawal)
                        .Select(x => existingNonFungibleIdData[new NonFungibleIdLookup(e.Key.ReferencedResource.DatabaseId, x.NonFungibleId)].Id)
                        .ToList();

                    // TODO: NonFungibleVaultContentsIndexEntrySubstate is published even if nfid already exist in vault, that's causing duplicates.
                    // Node is supposed to filter out that substate.
                    // To fix that temporarily we need to remove duplicates.
                    var uniqueAddedItems = addedItems.Except(nfids);

                    nfids.AddRange(uniqueAddedItems);
                    nfids.RemoveAll(x => deletedItems.Contains(x));

                    AggregateEntityResourceUsingSubstates(e.Key.ReferencedVault, e.Key.ReferencedResource, e.Key.StateVersion, false);

                    return new EntityNonFungibleVaultHistory
                    {
                        Id = sequences.EntityVaultHistorySequence++,
                        FromStateVersion = e.Key.StateVersion,
                        OwnerEntityId = e.Key.ReferencedVault.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.Key.ReferencedVault.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.Key.ReferencedResource.DatabaseId,
                        VaultEntityId = e.Key.ReferencedVault.DatabaseId,
                        NonFungibleIds = nfids.ToList(),
                    };
                })
                .ToList();

            foreach (var e in entityFungibleResourceBalanceChangeEvents)
            {
                var lookup = new EntityResourceLookup(e.EntityId, e.ResourceEntityId);

                EntityFungibleResourceAggregatedVaultsHistory aggregate;

                if (!mostRecentEntityResourceAggregatedVaultsHistory.TryGetValue(lookup, out var previous) || previous.FromStateVersion != e.StateVersion)
                {
                    var previousBalance = (previous as EntityFungibleResourceAggregatedVaultsHistory)?.Balance ?? TokenAmount.Zero;

                    aggregate = new EntityFungibleResourceAggregatedVaultsHistory
                    {
                        Id = sequences.EntityResourceAggregatedVaultsHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        EntityId = e.EntityId,
                        ResourceEntityId = e.ResourceEntityId,
                        Balance = previousBalance,
                    };

                    entityResourceAggregatedVaultsHistoryToAdd.Add(aggregate);
                    mostRecentEntityResourceAggregatedVaultsHistory[lookup] = aggregate;
                }
                else
                {
                    aggregate = (EntityFungibleResourceAggregatedVaultsHistory)previous;
                }

                aggregate.Balance += e.Delta;
            }

            foreach (var e in entityNonFungibleResourceBalanceChangeEvents)
            {
                var lookup = new EntityResourceLookup(e.EntityId, e.ResourceEntityId);

                EntityNonFungibleResourceAggregatedVaultsHistory aggregate;

                if (!mostRecentEntityResourceAggregatedVaultsHistory.TryGetValue(lookup, out var previous) || previous.FromStateVersion != e.StateVersion)
                {
                    var previousTotalCount = (previous as EntityNonFungibleResourceAggregatedVaultsHistory)?.TotalCount ?? 0;

                    aggregate = new EntityNonFungibleResourceAggregatedVaultsHistory
                    {
                        Id = sequences.EntityResourceAggregatedVaultsHistorySequence++,
                        FromStateVersion = e.StateVersion,
                        EntityId = e.EntityId,
                        ResourceEntityId = e.ResourceEntityId,
                        TotalCount = previousTotalCount,
                    };

                    entityResourceAggregatedVaultsHistoryToAdd.Add(aggregate);
                    mostRecentEntityResourceAggregatedVaultsHistory[lookup] = aggregate;
                }
                else
                {
                    aggregate = (EntityNonFungibleResourceAggregatedVaultsHistory)previous;
                }

                aggregate.TotalCount += e.Delta;
            }

            var resourceEntitySupplyHistoryToAdd = resourceSupplyChanges
                .GroupBy(x => new { x.ResourceEntityId, x.StateVersion })
                .Select(group =>
                {
                    var previous = mostRecentResourceEntitySupplyHistory.GetOrAdd(
                        group.Key.ResourceEntityId,
                        _ => new ResourceEntitySupplyHistory { TotalSupply = TokenAmount.Zero, TotalMinted = TokenAmount.Zero, TotalBurned = TokenAmount.Zero });

                    var minted = group
                        .Where(x => x.Minted.HasValue)
                        .Select(x => x.Minted)
                        .Aggregate(TokenAmount.Zero, (sum, x) => sum + x!.Value);

                    var burned = group
                        .Where(x => x.Burned.HasValue)
                        .Select(x => x.Burned)
                        .Aggregate(TokenAmount.Zero, (sum, x) => sum + x!.Value);

                    var totalSupply = previous.TotalSupply + minted - burned;
                    var totalMinted = previous.TotalMinted + minted;
                    var totalBurned = previous.TotalBurned + burned;

                    previous.TotalSupply = totalSupply;
                    previous.TotalMinted = totalMinted;
                    previous.TotalBurned = totalBurned;

                    return new ResourceEntitySupplyHistory
                    {
                        Id = sequences.ResourceEntitySupplyHistorySequence++,
                        FromStateVersion = group.Key.StateVersion,
                        ResourceEntityId = group.Key.ResourceEntityId,
                        TotalSupply = totalSupply,
                        TotalMinted = totalMinted,
                        TotalBurned = totalBurned,
                    };
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

            rowsInserted += await writeHelper.CopyEntity(entitiesToAdd, token);
            rowsInserted += await writeHelper.CopyLedgerTransaction(ledgerTransactionsToAdd, token);
            rowsInserted += await writeHelper.CopyLedgerTransactionMarkers(ledgerTransactionMarkersToAdd, token);
            rowsInserted += await writeHelper.CopyStateHistory(stateToAdd, token);
            rowsInserted += await writeHelper.CopyEntityMetadataHistory(entityMetadataHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityMetadataAggregateHistory(entityMetadataAggregateHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityRoleAssignmentsOwnerRoleHistory(entityAccessRulesOwnerRoleHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityRoleAssignmentsRulesEntryHistory(entityAccessRulesEntryHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityRoleAssignmentsAggregateHistory(entityAccessRulesAggregateHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityResourceAggregatedVaultsHistory(entityResourceAggregatedVaultsHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityResourceAggregateHistory(entityResourceAggregateHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityResourceVaultAggregateHistory(entityResourceVaultAggregateHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyEntityVaultHistory(entityFungibleVaultHistoryToAdd, entityNonFungibleVaultHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyComponentMethodRoyalties(componentMethodRoyaltiesToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleIdData(nonFungibleIdDataToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleIdDataHistory(nonFungibleIdsMutableDataHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleIdStoreHistory(nonFungibleIdStoreHistoryToAdd.Values, token);
            rowsInserted += await writeHelper.CopyResourceEntitySupplyHistory(resourceEntitySupplyHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyValidatorKeyHistory(validatorKeyHistoryToAdd.Values, token);
            rowsInserted += await writeHelper.CopyValidatorActiveSetHistory(validatorActiveSetHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyPackageBlueprintHistory(packageBlueprintHistoryToAdd.Values, token);
            rowsInserted += await writeHelper.CopyPackageCodeHistory(packageCodeHistoryToAdd, token);
            rowsInserted += await writeHelper.CopySchemaHistory(schemaHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyKeyValueStoreEntryHistory(keyValueStoreEntryHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyAccountDefaultDepositRuleHistory(accountDefaultDepositRuleHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyAccountResourcePreferenceRuleHistory(accountResourcePreferenceRuleHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyValidatorEmissionStatistics(validatorEmissionStatisticsToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleDataSchemaHistory(nonFungibleSchemaHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyKeyValueStoreSchemaHistory(keyValueStoreSchemaHistoryToAdd, token);
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

    private async Task<TransactionSummary> GetTopOfLedger(ReadWriteDbContext dbContext, CancellationToken token)
    {
        var lastTransaction = await dbContext.GetTopLedgerTransaction().FirstOrDefaultAsync(token);

        return lastTransaction == null
            ? await PreGenesisTransactionSummary()
            : new TransactionSummary(
                StateVersion: lastTransaction.StateVersion,
                RoundTimestamp: lastTransaction.RoundTimestamp,
                NormalizedRoundTimestamp: lastTransaction.NormalizedRoundTimestamp,
                CreatedTimestamp: lastTransaction.CreatedTimestamp,
                Epoch: lastTransaction.Epoch,
                RoundInEpoch: lastTransaction.RoundInEpoch,
                IndexInEpoch: lastTransaction.IndexInEpoch,
                IndexInRound: lastTransaction.IndexInRound
            );
    }

    private async Task<TransactionSummary> PreGenesisTransactionSummary()
    {
        // Nearly all of theses turn out to be unused!
        return new TransactionSummary(
            StateVersion: 0,
            RoundTimestamp: DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime,
            NormalizedRoundTimestamp: DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime,
            CreatedTimestamp: _clock.UtcNow,
            Epoch: await _networkConfigurationProvider.GetGenesisEpoch(),
            RoundInEpoch: await _networkConfigurationProvider.GetGenesisRound(),
            IndexInEpoch: -1, // invalid, but we increase it by one to in ProcessTransactions
            IndexInRound: -1 // invalid, but we increase it by one to in ProcessTransactions
        );
    }
}
