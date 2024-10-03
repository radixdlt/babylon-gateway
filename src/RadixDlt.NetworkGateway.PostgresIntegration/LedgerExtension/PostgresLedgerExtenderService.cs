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
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.DataAggregator;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
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
    private readonly ITopOfLedgerProvider _topOfLedgerProvider;
    private readonly IEnumerable<ILedgerExtenderServiceObserver> _observers;
    private readonly IClock _clock;

    public PostgresLedgerExtenderService(
        ILogger<PostgresLedgerExtenderService> logger,
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        INetworkConfigurationProvider networkConfigurationProvider,
        IEnumerable<ILedgerExtenderServiceObserver> observers,
        IClock clock,
        ITopOfLedgerProvider topOfLedgerProvider)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _networkConfigurationProvider = networkConfigurationProvider;
        _observers = observers;
        _clock = clock;
        _topOfLedgerProvider = topOfLedgerProvider;
    }

    public async Task<CommitTransactionsReport> CommitTransactions(ConsistentLedgerExtension ledgerExtension, CancellationToken token = default)
    {
        // Create own context for ledger extension unit of work
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        await using var tx = await dbContext.Database.BeginTransactionAsync(token);

        try
        {
            var topOfLedgerSummary = await _topOfLedgerProvider.GetTopOfLedger(token);

            TransactionConsistencyValidator.AssertLatestTransactionConsistent(ledgerExtension.LatestTransactionSummary.StateVersion, topOfLedgerSummary.StateVersion);

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
        var timestamp = _clock.UtcNow;
        var payloadHashes = committedTransactions
            .Where(ct => ct.LedgerTransaction is CoreModel.UserLedgerTransaction)
            .Select(ct => ((CoreModel.UserLedgerTransaction)ct.LedgerTransaction).NotarizedTransaction.HashBech32m)
            .ToList();

        var pendingTransactions = await dbContext
            .PendingTransactions
            .AsNoTracking()
            .Where(pt => payloadHashes.Contains(pt.PayloadHash))
            .Where(pt => pt.LedgerDetails.PayloadLedgerStatus == PendingTransactionPayloadLedgerStatus.PermanentlyRejected)
            .Select(
                pt => new
            {
                pt.PayloadHash,
                pt.LedgerDetails.PayloadLedgerStatus,
                pt.GatewayHandling.FirstSubmittedToGatewayTimestamp,
                pt.LedgerDetails.LatestRejectionTimestamp,
                pt.LedgerDetails.LatestRejectionReason,
            })
            .AnnotateMetricName()
            .ToListAsync(token);

        foreach (var details in pendingTransactions)
        {
            if (details.PayloadLedgerStatus == PendingTransactionPayloadLedgerStatus.PermanentlyRejected)
            {
                await _observers.ForEachAsync(x => x.TransactionMarkedCommittedWhichWasPermanentlyRejected());

                _logger.LogError(
                    "Transaction with payload hash {PayloadHash} which was first submitted to Gateway at {FirstGatewaySubmissionTime} was marked permanently rejected at {FailureTime} due to \"{FailureReason}\" but has now been marked committed",
                    details.PayloadHash,
                    details.FirstSubmittedToGatewayTimestamp.AsUtcIsoDateToSecondsForLogs(),
                    details.LatestRejectionTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                    details.LatestRejectionReason
                );
            }

            await _observers.ForEachAsync(x => x.TransactionsCommittedWithGatewayLatency(timestamp - details.FirstSubmittedToGatewayTimestamp));
        }

        // Change to UpdateAsync when EFCore fixes this bug: https://github.com/dotnet/efcore/issues/29690#issuecomment-1726182209
        var updatedCount = await dbContext.Database
            .ExecuteSqlInterpolatedAsync(
                $@"
UPDATE pending_transactions
    SET
        payload_status = 'committed',
        intent_status = 'committed',
        commit_timestamp = {timestamp},
        resubmit_from_timestamp = NULL,
        handling_status_reason = 'Concluded as committed'
    WHERE
        payload_hash = ANY({payloadHashes})
",
                token);

        await _observers.ForEachAsync(x => x.TransactionsMarkedCommittedCount(updatedCount));

        return updatedCount;
    }

    private async Task<ExtendLedgerReport> ProcessTransactions(ReadWriteDbContext dbContext, ConsistentLedgerExtension ledgerExtension, CancellationToken token)
    {
        var networkConfiguration = await _networkConfigurationProvider.GetNetworkConfiguration(token);
        var rowsInserted = 0;
        var rowsUpdated = 0;
        var dbReadDuration = TimeSpan.Zero;
        var dbWriteDuration = TimeSpan.Zero;
        var outerStopwatch = Stopwatch.StartNew();
        var referencedEntities = new ReferencedEntityDictionary();
        var childToParentEntities = new Dictionary<EntityAddress, EntityAddress>();
        var manifestExtractedAddresses = new Dictionary<long, ManifestAddressesExtractor.ManifestAddresses>();
        var manifestClasses = new Dictionary<long, List<LedgerTransactionManifestClass>>();

        var readHelper = new ReadHelper(dbContext, _observers, token);
        var writeHelper = new WriteHelper(dbContext, _observers, token);

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

            await _observers.ForEachAsync(x => x.StageCompleted(nameof(UpdatePendingTransactions), sw.Elapsed, null));
        }

        var processorContext = new ProcessorContext(sequences, readHelper, writeHelper, networkConfiguration, token);
        var relationshipProcessor = new RelationshipProcessor(referencedEntities);

        // step: scan for any referenced entities
        {
            var sw = Stopwatch.StartNew();

            referencedEntities.MarkSeenAddress((EntityAddress)networkConfiguration.WellKnownAddresses.TransactionTracker);
            referencedEntities.MarkSeenAddress((EntityAddress)networkConfiguration.WellKnownAddresses.ConsensusManager);
            referencedEntities.MarkSeenAddress((EntityAddress)networkConfiguration.WellKnownAddresses.Xrd);

            foreach (var committedTransaction in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = committedTransaction.ResultantStateIdentifiers.StateVersion;
                var stateUpdates = committedTransaction.Receipt.StateUpdates;
                var events = committedTransaction.Receipt.Events ?? new List<CoreModel.Event>();

                try
                {
                    long? epochUpdate = null;
                    long? roundInEpochUpdate = null;
                    DateTime? roundTimestampUpdate = null;

                    if (committedTransaction.LedgerTransaction is CoreModel.UserLedgerTransaction userLedgerTransaction)
                    {
                        ledgerTransactionMarkersToAdd.Add(
                            new OriginLedgerTransactionMarker
                        {
                            Id = sequences.LedgerTransactionMarkerSequence++,
                            StateVersion = stateVersion,
                            OriginType = LedgerTransactionMarkerOriginType.User,
                        });

                        var coreInstructions = userLedgerTransaction.NotarizedTransaction.SignedIntent.Intent.Instructions;
                        var coreBlobs = userLedgerTransaction.NotarizedTransaction.SignedIntent.Intent.BlobsHex;
                        using var manifestInstructions = ToolkitModel.Instructions.FromString(coreInstructions, networkConfiguration.Id);
                        using var toolkitManifest = new ToolkitModel.TransactionManifest(manifestInstructions, coreBlobs.Values.Select(x => x.ConvertFromHex()).ToArray());

                        var extractedAddresses = ManifestAddressesExtractor.ExtractAddresses(toolkitManifest, networkConfiguration.Id);

                        foreach (var address in extractedAddresses.All())
                        {
                            referencedEntities.MarkSeenAddress(address);
                        }

                        manifestExtractedAddresses[stateVersion] = extractedAddresses;

                        var manifestSummary = toolkitManifest.Summary(networkConfiguration.Id);

                        for (var i = 0; i < manifestSummary.classification.Length; ++i)
                        {
                            var manifestClass = manifestSummary.classification[i].ToModel();

                            manifestClasses
                                .GetOrAdd(stateVersion, _ => new List<LedgerTransactionManifestClass>())
                                .Add(manifestClass);

                            ledgerTransactionMarkersToAdd.Add(
                                new ManifestClassMarker
                            {
                                Id = sequences.LedgerTransactionMarkerSequence++,
                                StateVersion = stateVersion,
                                ManifestClass = manifestClass,
                                IsMostSpecific = i == 0,
                            });
                        }
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

                        referencedEntity.WithTypeHint(
                            newGlobalEntity.EntityType switch
                        {
                            CoreModel.EntityType.GlobalPackage => typeof(GlobalPackageEntity),
                            CoreModel.EntityType.GlobalConsensusManager => typeof(GlobalConsensusManager),
                            CoreModel.EntityType.GlobalValidator => typeof(GlobalValidatorEntity),
                            CoreModel.EntityType.GlobalGenericComponent => typeof(GlobalGenericComponentEntity),
                            CoreModel.EntityType.GlobalAccount => typeof(GlobalAccountEntity),
                            CoreModel.EntityType.GlobalAccountLocker => typeof(GlobalAccountLockerEntity),
                            CoreModel.EntityType.GlobalIdentity => typeof(GlobalIdentityEntity),
                            CoreModel.EntityType.GlobalAccessController => typeof(GlobalAccessControllerEntity),
                            CoreModel.EntityType.GlobalVirtualSecp256k1Account => typeof(GlobalAccountEntity),
                            CoreModel.EntityType.GlobalVirtualSecp256k1Identity => typeof(GlobalIdentityEntity),
                            CoreModel.EntityType.GlobalVirtualEd25519Account => typeof(GlobalAccountEntity),
                            CoreModel.EntityType.GlobalVirtualEd25519Identity => typeof(GlobalIdentityEntity),
                            CoreModel.EntityType.GlobalFungibleResource => typeof(GlobalFungibleResourceEntity),
                            CoreModel.EntityType.InternalFungibleVault => typeof(InternalFungibleVaultEntity),
                            CoreModel.EntityType.GlobalNonFungibleResource => typeof(GlobalNonFungibleResourceEntity),
                            CoreModel.EntityType.InternalNonFungibleVault => typeof(InternalNonFungibleVaultEntity),
                            CoreModel.EntityType.InternalGenericComponent => typeof(InternalGenericComponentEntity),
                            CoreModel.EntityType.InternalKeyValueStore => typeof(InternalKeyValueStoreEntity),
                            CoreModel.EntityType.GlobalOneResourcePool => typeof(GlobalOneResourcePoolEntity),
                            CoreModel.EntityType.GlobalTwoResourcePool => typeof(GlobalTwoResourcePoolEntity),
                            CoreModel.EntityType.GlobalMultiResourcePool => typeof(GlobalMultiResourcePoolEntity),
                            CoreModel.EntityType.GlobalTransactionTracker => typeof(GlobalTransactionTrackerEntity),
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
                            referencedEntity.PostResolveConfigure((GlobalFungibleResourceEntity e) =>
                            {
                                e.Divisibility = fungibleResourceManager.Value.Divisibility;
                            });
                        }

                        if (substateData is CoreModel.NonFungibleResourceManagerFieldMutableFieldsSubstate mutableFields)
                        {
                            referencedEntity.PostResolveConfigure((GlobalNonFungibleResourceEntity e) =>
                            {
                                e.NonFungibleDataMutableFields = mutableFields.Value.MutableFields.Select(x => x.Name).ToList();
                            });
                        }

                        if (substateData is CoreModel.NonFungibleResourceManagerFieldIdTypeSubstate nonFungibleResourceManagerFieldIdTypeSubstate)
                        {
                            referencedEntity.PostResolveConfigure((GlobalNonFungibleResourceEntity e) =>
                            {
                                e.NonFungibleIdType = nonFungibleResourceManagerFieldIdTypeSubstate.Value.NonFungibleIdType switch
                                {
                                    CoreModel.NonFungibleIdType.String => NonFungibleIdType.String,
                                    CoreModel.NonFungibleIdType.Integer => NonFungibleIdType.Integer,
                                    CoreModel.NonFungibleIdType.Bytes => NonFungibleIdType.Bytes,
                                    CoreModel.NonFungibleIdType.RUID => NonFungibleIdType.RUID,
                                    _ => throw new ArgumentOutOfRangeException(nameof(e.NonFungibleIdType), e.NonFungibleIdType, "Unexpected value of NonFungibleIdType"),
                                };
                            });
                        }

                        if (substateData is CoreModel.TypeInfoModuleFieldTypeInfoSubstate typeInfoSubstate && typeInfoSubstate.Value.Details is CoreModel.ObjectTypeInfoDetails objectDetails)
                        {
                            referencedEntity.PostResolveConfigure((ComponentEntity e) =>
                            {
                                e.AssignedModuleIds = objectDetails.ModuleVersions
                                    .Select(x =>
                                    {
                                        return x.Module switch
                                        {
                                            CoreModel.AttachedModuleId.Metadata => ModuleId.Metadata,
                                            CoreModel.AttachedModuleId.Royalty => ModuleId.Royalty,
                                            CoreModel.AttachedModuleId.RoleAssignment => ModuleId.RoleAssignment,
                                            _ => throw new ArgumentOutOfRangeException(nameof(x.Module), x.Module, "Unexpected value of AssignedModule"),
                                        };
                                    })
                                    .OrderBy(x => x)
                                    .ToList();

                                e.BlueprintName = objectDetails.BlueprintInfo.BlueprintName;
                                e.BlueprintVersion = objectDetails.BlueprintInfo.BlueprintVersion;
                            });
                        }

                        relationshipProcessor.ScanUpsert(substateData, referencedEntity, stateVersion);
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

                    foreach (var @event in events)
                    {
                        foreach (var entityAddress in @event.Type.GetEntityAddresses())
                        {
                            referencedEntities.MarkSeenAddress((EntityAddress)entityAddress);
                        }
                    }

                    if (committedTransaction.BalanceChanges != null)
                    {
                        foreach (var entityAddress in committedTransaction.BalanceChanges.GetEntityAddresses())
                        {
                            referencedEntities.MarkSeenAddress((EntityAddress)entityAddress);
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
                        TransactionTreeHash: lastTransactionSummary.TransactionTreeHash,
                        ReceiptTreeHash: lastTransactionSummary.ReceiptTreeHash,
                        StateTreeHash: lastTransactionSummary.StateTreeHash,
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
                            RawPayload = ult.NotarizedTransaction.GetPayloadBytes(),
                            ManifestInstructions = ult.NotarizedTransaction.SignedIntent.Intent.Instructions,
                            ManifestClasses = manifestClasses.TryGetValue(stateVersion, out var mc) ? mc.ToArray() : Array.Empty<LedgerTransactionManifestClass>(),
                        },
                        CoreModel.RoundUpdateLedgerTransaction => new RoundUpdateLedgerTransaction(),
                        CoreModel.FlashLedgerTransaction => new FlashLedgerTransaction(),
                        _ => throw new UnreachableException($"Unsupported transaction type: {committedTransaction.LedgerTransaction.GetType()}"),
                    };

                    ledgerTransaction.StateVersion = stateVersion;
                    ledgerTransaction.TransactionTreeHash = committedTransaction.ResultantStateIdentifiers.TransactionTreeHash;
                    ledgerTransaction.ReceiptTreeHash = committedTransaction.ResultantStateIdentifiers.ReceiptTreeHash;
                    ledgerTransaction.StateTreeHash = committedTransaction.ResultantStateIdentifiers.StateTreeHash;
                    ledgerTransaction.Epoch = summary.Epoch;
                    ledgerTransaction.RoundInEpoch = summary.RoundInEpoch;
                    ledgerTransaction.IndexInEpoch = summary.IndexInEpoch;
                    ledgerTransaction.IndexInRound = summary.IndexInRound;
                    ledgerTransaction.FeePaid = committedTransaction.Receipt.FeeSummary.TotalFee();
                    ledgerTransaction.TipPaid = committedTransaction.Receipt.FeeSummary.TotalTip();
                    ledgerTransaction.AffectedGlobalEntities = default!; // configured later on
                    ledgerTransaction.RoundTimestamp = summary.RoundTimestamp;
                    ledgerTransaction.CreatedTimestamp = summary.CreatedTimestamp;
                    ledgerTransaction.NormalizedRoundTimestamp = summary.NormalizedRoundTimestamp;
                    ledgerTransaction.ReceiptStateUpdates = committedTransaction.Receipt.StateUpdates.ToJson();
                    ledgerTransaction.ReceiptStatus = committedTransaction.Receipt.Status.ToModel();
                    ledgerTransaction.ReceiptFeeSummary = committedTransaction.Receipt.FeeSummary.ToJson();
                    ledgerTransaction.ReceiptErrorMessage = committedTransaction.Receipt.ErrorMessage;
                    ledgerTransaction.ReceiptOutput = committedTransaction.Receipt.Output != null ? JsonConvert.SerializeObject(committedTransaction.Receipt.Output) : null;
                    ledgerTransaction.ReceiptNextEpoch = committedTransaction.Receipt.NextEpoch?.ToJson();
                    ledgerTransaction.ReceiptCostingParameters = committedTransaction.Receipt.CostingParameters.ToJson();
                    ledgerTransaction.ReceiptFeeSource = committedTransaction.Receipt.FeeSource?.ToJson();
                    ledgerTransaction.ReceiptFeeDestination = committedTransaction.Receipt.FeeDestination?.ToJson();
                    ledgerTransaction.BalanceChanges = committedTransaction.BalanceChanges?.ToJson();
                    ledgerTransactionsToAdd.Add(ledgerTransaction);

                    if (committedTransaction.Receipt.NextEpoch != null)
                    {
                        ledgerTransactionMarkersToAdd.Add(
                            new OriginLedgerTransactionMarker
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

            await _observers.ForEachAsync(x => x.StageCompleted("scan_for_referenced_entities", sw.Elapsed, null));
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
                    GlobalPackageEntity => CoreModel.EntityType.GlobalPackage,
                    GlobalConsensusManager => CoreModel.EntityType.GlobalConsensusManager,
                    GlobalValidatorEntity => CoreModel.EntityType.GlobalValidator,
                    GlobalGenericComponentEntity => CoreModel.EntityType.GlobalGenericComponent,
                    GlobalAccountEntity => CoreModel.EntityType.GlobalAccount,
                    GlobalAccountLockerEntity => CoreModel.EntityType.GlobalAccountLocker,
                    GlobalIdentityEntity => CoreModel.EntityType.GlobalIdentity,
                    GlobalAccessControllerEntity => CoreModel.EntityType.GlobalAccessController,
                    // skipped GlobalVirtualSecp256k1Account, GlobalVirtualSecp256k1Identity, GlobalVirtualEd25519Account and GlobalVirtualEd25519Identity as they are virtual
                    GlobalFungibleResourceEntity => CoreModel.EntityType.GlobalFungibleResource,
                    InternalFungibleVaultEntity => CoreModel.EntityType.InternalFungibleVault,
                    GlobalNonFungibleResourceEntity => CoreModel.EntityType.GlobalNonFungibleResource,
                    InternalNonFungibleVaultEntity => CoreModel.EntityType.InternalNonFungibleVault,
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
                    CoreModel.EntityType.GlobalPackage => new GlobalPackageEntity(),
                    CoreModel.EntityType.GlobalConsensusManager => new GlobalConsensusManager(),
                    CoreModel.EntityType.GlobalValidator => new GlobalValidatorEntity(),
                    CoreModel.EntityType.GlobalGenericComponent => new GlobalGenericComponentEntity(),
                    CoreModel.EntityType.GlobalAccount => new GlobalAccountEntity(),
                    CoreModel.EntityType.GlobalIdentity => new GlobalIdentityEntity(),
                    CoreModel.EntityType.GlobalAccessController => new GlobalAccessControllerEntity(),
                    CoreModel.EntityType.GlobalVirtualSecp256k1Account => new GlobalAccountEntity(),
                    CoreModel.EntityType.GlobalVirtualSecp256k1Identity => new GlobalIdentityEntity(),
                    CoreModel.EntityType.GlobalVirtualEd25519Account => new GlobalAccountEntity(),
                    CoreModel.EntityType.GlobalVirtualEd25519Identity => new GlobalIdentityEntity(),
                    CoreModel.EntityType.GlobalFungibleResource => new GlobalFungibleResourceEntity(),
                    CoreModel.EntityType.InternalFungibleVault => new InternalFungibleVaultEntity(),
                    CoreModel.EntityType.GlobalNonFungibleResource => new GlobalNonFungibleResourceEntity(),
                    CoreModel.EntityType.InternalNonFungibleVault => new InternalNonFungibleVaultEntity(),
                    CoreModel.EntityType.InternalGenericComponent => new InternalGenericComponentEntity(),
                    CoreModel.EntityType.InternalKeyValueStore => new InternalKeyValueStoreEntity(),
                    CoreModel.EntityType.GlobalOneResourcePool => new GlobalOneResourcePoolEntity(),
                    CoreModel.EntityType.GlobalTwoResourcePool => new GlobalTwoResourcePoolEntity(),
                    CoreModel.EntityType.GlobalMultiResourcePool => new GlobalMultiResourcePoolEntity(),
                    CoreModel.EntityType.GlobalTransactionTracker => new GlobalTransactionTrackerEntity(),
                    CoreModel.EntityType.GlobalAccountLocker => new GlobalAccountLockerEntity(),
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

                // we must rely on high priority PostResolveConfigure as other callbacks rely on the entity hierarchy tree
                referencedEntities
                    .Get(childAddress)
                    .PostResolveConfigureHigh((Entity dbe) =>
                    {
                        dbe.AncestorIds = allAncestors;
                        dbe.ParentAncestorId = parentId.Value;
                        dbe.OwnerAncestorId = ownerId.Value;
                        dbe.GlobalAncestorId = globalId.Value;
                    });
            }

            referencedEntities.InvokePostResolveConfiguration();

            await _observers.ForEachAsync(x => x.StageCompleted("resolve_and_create_entities", sw.Elapsed, null));
        }

        var resourceSupplyChanges = new List<ResourceSupplyChange>();
        var nonFungibleSchemaHistoryToAdd = new List<NonFungibleSchemaHistory>();
        var keyValueStoreSchemaHistoryToAdd = new List<KeyValueStoreSchemaHistory>();

        var entityStateProcessor = new EntityStateProcessor(processorContext, referencedEntities);
        var entityMetadataProcessor = new EntityMetadataProcessor(processorContext);
        var entitySchemaProcessor = new EntitySchemaProcessor(processorContext);
        var componentMethodRoyaltyProcessor = new ComponentMethodRoyaltyProcessor(processorContext);
        var entityRoleAssignmentProcessor = new EntityRoleAssignmentProcessor(processorContext);
        var packageCodeProcessor = new PackageCodeProcessor(processorContext);
        var packageBlueprintProcessor = new PackageBlueprintProcessor(processorContext, referencedEntities);
        var accountAuthorizedDepositorsProcessor = new AccountAuthorizedDepositorsProcessor(processorContext, referencedEntities);
        var accountResourcePreferenceRulesProcessor = new AccountResourcePreferenceRulesProcessor(processorContext, referencedEntities);
        var accountDefaultDepositRuleProcessor = new AccountDefaultDepositRuleProcessor(processorContext);
        var keyValueStoreProcessor = new KeyValueStoreProcessor(processorContext);
        var validatorProcessor = new ValidatorProcessor(processorContext, referencedEntities);
        var validatorEmissionProcessor = new ValidatorEmissionProcessor(processorContext);
        var accountLockerProcessor = new AccountLockerProcessor(processorContext, referencedEntities);
        var globalEventEmitterProcessor = new GlobalEventEmitterProcessor(processorContext, referencedEntities, networkConfiguration);
        var affectedGlobalEntitiesProcessor = new AffectedGlobalEntitiesProcessor(processorContext, referencedEntities, networkConfiguration);
        var standardMetadataProcessor = new StandardMetadataProcessor(processorContext, referencedEntities);
        var entityResourceProcessor = new EntityResourceProcessor(processorContext, dbContext, _observers);
        var vaultProcessor = new VaultProcessor(processorContext);

        // step: scan all substates & events to figure out changes
        {
            var sw = Stopwatch.StartNew();

            foreach (var committedTransaction in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = committedTransaction.ResultantStateIdentifiers.StateVersion;
                var stateUpdates = committedTransaction.Receipt.StateUpdates;
                var events = committedTransaction.Receipt.Events ?? new List<CoreModel.Event>();
                long? passingEpoch = null;

                try
                {
                    foreach (var substate in stateUpdates.UpsertedSubstates)
                    {
                        var substateId = substate.SubstateId;
                        var substateData = substate.Value.SubstateData;
                        var referencedEntity = referencedEntities.Get((EntityAddress)substateId.EntityAddress);

                        if (substateData is CoreModel.ConsensusManagerFieldStateSubstate consensusManagerFieldStateSubstate)
                        {
                            if (consensusManagerFieldStateSubstate.Value.Round == 0)
                            {
                                passingEpoch = consensusManagerFieldStateSubstate.Value.Epoch - 1;
                            }
                        }

                        if (substateData is CoreModel.TypeInfoModuleFieldTypeInfoSubstate typeInfoSubstate)
                        {
                            if (typeInfoSubstate.TryGetNonFungibleDataSchemaDetails(out var nonFungibleDataSchemaDetails))
                            {
                                var schemaDefiningEntityId = !string.IsNullOrEmpty(nonFungibleDataSchemaDetails.Value.SchemaDefiningEntityAddress)
                                    ? referencedEntities.Get((EntityAddress)nonFungibleDataSchemaDetails.Value.SchemaDefiningEntityAddress).DatabaseId
                                    : referencedEntity.DatabaseId;

                                nonFungibleSchemaHistoryToAdd.Add(
                                    new NonFungibleSchemaHistory
                                {
                                    Id = sequences.NonFungibleSchemaHistorySequence++,
                                    ResourceEntityId = referencedEntity.DatabaseId,
                                    SchemaHash = nonFungibleDataSchemaDetails.Value.SchemaHash.ConvertFromHex(),
                                    SborTypeKind = nonFungibleDataSchemaDetails.Value.SborTypeKind.ToModel(),
                                    TypeIndex = nonFungibleDataSchemaDetails.Value.TypeIndex,
                                    SchemaDefiningEntityId = schemaDefiningEntityId,
                                    FromStateVersion = stateVersion,
                                });
                            }

                            if (typeInfoSubstate.Value.Details is CoreModel.KeyValueStoreTypeInfoDetails
                                && typeInfoSubstate.TryGetKeyValueStoreKeySchemaDetails(out var keySchemaDetails)
                                && typeInfoSubstate.TryGetKeyValueStoreValueSchemaDetails(out var valueSchemaDetails))
                            {
                                var keySchemaDefiningEntityId = !string.IsNullOrEmpty(keySchemaDetails.Value.SchemaDefiningEntityAddress)
                                    ? referencedEntities.Get((EntityAddress)keySchemaDetails.Value.SchemaDefiningEntityAddress).DatabaseId
                                    : referencedEntity.DatabaseId;

                                var valueSchemaDefiningEntityId = !string.IsNullOrEmpty(valueSchemaDetails.Value.SchemaDefiningEntityAddress)
                                    ? referencedEntities.Get((EntityAddress)valueSchemaDetails.Value.SchemaDefiningEntityAddress).DatabaseId
                                    : referencedEntity.DatabaseId;

                                keyValueStoreSchemaHistoryToAdd.Add(
                                    new KeyValueStoreSchemaHistory
                                {
                                    Id = sequences.KeyValueSchemaHistorySequence++,
                                    KeyValueStoreEntityId = referencedEntity.DatabaseId,
                                    KeySchemaDefiningEntityId = keySchemaDefiningEntityId,
                                    KeySchemaHash = keySchemaDetails.Value.SchemaHash.ConvertFromHex(),
                                    KeySborTypeKind = keySchemaDetails.Value.SborTypeKind.ToModel(),
                                    KeyTypeIndex = keySchemaDetails.Value.TypeIndex,
                                    ValueSchemaDefiningEntityId = valueSchemaDefiningEntityId,
                                    ValueSchemaHash = valueSchemaDetails.Value.SchemaHash.ConvertFromHex(),
                                    ValueSborTypeKind = valueSchemaDetails.Value.SborTypeKind.ToModel(),
                                    ValueTypeIndex = valueSchemaDetails.Value.TypeIndex,
                                    FromStateVersion = stateVersion,
                                });
                            }
                        }

                        entityStateProcessor.VisitUpsert(substate, referencedEntity, stateVersion);
                        entityMetadataProcessor.VisitUpsert(substate, referencedEntity, stateVersion);
                        entitySchemaProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        componentMethodRoyaltyProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        entityRoleAssignmentProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        packageCodeProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        packageBlueprintProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        accountResourcePreferenceRulesProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        accountDefaultDepositRuleProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        accountAuthorizedDepositorsProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        keyValueStoreProcessor.VisitUpsert(substate, referencedEntity, stateVersion);
                        validatorProcessor.VisitUpsert(substateData, referencedEntity, stateVersion, passingEpoch);
                        accountLockerProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        affectedGlobalEntitiesProcessor.VisitUpsert(referencedEntity, stateVersion);
                        standardMetadataProcessor.VisitUpsert(substateData, referencedEntity, stateVersion);
                        vaultProcessor.VisitUpsert(substateData, referencedEntity, stateVersion, substate);
                        entityResourceProcessor.VisitUpsert(substateData, referencedEntity, stateVersion, substate);
                    }

                    foreach (var deletedSubstate in stateUpdates.DeletedSubstates)
                    {
                        var substateId = deletedSubstate.SubstateId;
                        var referencedEntity = referencedEntities.GetOrAdd((EntityAddress)substateId.EntityAddress, ea => new ReferencedEntity(ea, substateId.EntityType, stateVersion));

                        affectedGlobalEntitiesProcessor.VisitDelete(referencedEntity, stateVersion);
                        packageCodeProcessor.VisitDelete(substateId, referencedEntity, stateVersion);
                        vaultProcessor.VisitDelete(substateId, referencedEntity, stateVersion);
                        entitySchemaProcessor.VisitDelete(substateId, referencedEntity, stateVersion);
                    }

                    var transaction = ledgerTransactionsToAdd.Single(x => x.StateVersion == stateVersion);

                    transaction.AffectedGlobalEntities = affectedGlobalEntitiesProcessor.GetAllAffectedGlobalEntities(stateVersion).ToArray();
                    transaction.ReceiptEventEmitters = events.Select(e => e.Type.Emitter.ToJson()).ToArray();
                    transaction.ReceiptEventNames = events.Select(e => e.Type.Name).ToArray();
                    transaction.ReceiptEventSbors = events.Select(e => e.Data.GetDataBytes()).ToArray();
                    transaction.ReceiptEventSchemaEntityIds = events.Select(e => referencedEntities.Get((EntityAddress)e.Type.TypeReference.FullTypeId.EntityAddress).DatabaseId).ToArray();
                    transaction.ReceiptEventSchemaHashes = events.Select(e => e.Type.TypeReference.FullTypeId.SchemaHash.ConvertFromHex()).ToArray();
                    transaction.ReceiptEventTypeIndexes = events.Select(e => e.Type.TypeReference.FullTypeId.LocalTypeId.Id).ToArray();
                    transaction.ReceiptEventSborTypeKinds = events.Select(e => e.Type.TypeReference.FullTypeId.LocalTypeId.Kind.ToModel()).ToArray();

                    foreach (var @event in events)
                    {
                        globalEventEmitterProcessor.VisitEvent(@event, stateVersion);

                        if (@event.Type.Emitter is not CoreModel.MethodEventEmitterIdentifier methodEventEmitter
                            || methodEventEmitter.ObjectModuleId != CoreModel.ModuleId.Main
                            || methodEventEmitter.Entity.EntityType == CoreModel.EntityType.GlobalGenericComponent
                            || methodEventEmitter.Entity.EntityType == CoreModel.EntityType.InternalGenericComponent)
                        {
                            continue;
                        }

                        var eventEmitterEntity = referencedEntities.Get((EntityAddress)methodEventEmitter.Entity.EntityAddress);

                        using var decodedEvent = EventDecoder.DecodeEvent(@event, networkConfiguration.Id);

                        validatorEmissionProcessor.VisitEvent(decodedEvent, eventEmitterEntity, stateVersion);

                        if (EventDecoder.TryGetFungibleVaultWithdrawalEvent(decodedEvent, out var fungibleVaultWithdrawalEvent))
                        {
                            ledgerTransactionMarkersToAdd.Add(
                                new EventLedgerTransactionMarker
                            {
                                Id = sequences.LedgerTransactionMarkerSequence++,
                                StateVersion = stateVersion,
                                EventType = LedgerTransactionMarkerEventType.Withdrawal,
                                EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                                ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalFungibleVaultEntity>().GetResourceEntityId(),
                                Quantity = TokenAmount.FromDecimalString(fungibleVaultWithdrawalEvent.AsStr()),
                            });
                        }
                        else if (EventDecoder.TryGetFungibleVaultDepositEvent(decodedEvent, out var fungibleVaultDepositEvent))
                        {
                            ledgerTransactionMarkersToAdd.Add(
                                new EventLedgerTransactionMarker
                            {
                                Id = sequences.LedgerTransactionMarkerSequence++,
                                StateVersion = stateVersion,
                                EventType = LedgerTransactionMarkerEventType.Deposit,
                                EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                                ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalFungibleVaultEntity>().GetResourceEntityId(),
                                Quantity = TokenAmount.FromDecimalString(fungibleVaultDepositEvent.AsStr()),
                            });
                        }
                        else if (EventDecoder.TryGetNonFungibleVaultWithdrawalEvent(decodedEvent, out var nonFungibleVaultWithdrawalEvent))
                        {
                            ledgerTransactionMarkersToAdd.Add(
                                new EventLedgerTransactionMarker
                            {
                                Id = sequences.LedgerTransactionMarkerSequence++,
                                StateVersion = stateVersion,
                                EventType = LedgerTransactionMarkerEventType.Withdrawal,
                                EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                                ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>().GetResourceEntityId(),
                                Quantity = TokenAmount.FromDecimalString(nonFungibleVaultWithdrawalEvent.Length.ToString()),
                            });
                        }
                        else if (EventDecoder.TryGetNonFungibleVaultDepositEvent(decodedEvent, out var nonFungibleVaultDepositEvent))
                        {
                            ledgerTransactionMarkersToAdd.Add(
                                new EventLedgerTransactionMarker
                            {
                                Id = sequences.LedgerTransactionMarkerSequence++,
                                StateVersion = stateVersion,
                                EventType = LedgerTransactionMarkerEventType.Deposit,
                                EntityId = eventEmitterEntity.DatabaseGlobalAncestorId,
                                ResourceEntityId = eventEmitterEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>().GetResourceEntityId(),
                                Quantity = TokenAmount.FromDecimalString(nonFungibleVaultDepositEvent.Length.ToString()),
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
                            var mintedCount = TokenAmount.FromDecimalString(nonFungibleResourceMintedEvent.ids.Length.ToString());
                            resourceSupplyChanges.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Minted: mintedCount));
                        }
                        else if (EventDecoder.TryGetNonFungibleResourceBurnedEvent(decodedEvent, out var nonFungibleResourceBurnedEvent))
                        {
                            var burnedCount = TokenAmount.FromDecimalString(nonFungibleResourceBurnedEvent.ids.Length.ToString());
                            resourceSupplyChanges.Add(new ResourceSupplyChange(eventEmitterEntity.DatabaseId, stateVersion, Burned: burnedCount));
                        }
                    }

                    if (manifestExtractedAddresses.TryGetValue(stateVersion, out var extractedAddresses))
                    {
                        foreach (var proofResourceAddress in extractedAddresses.PresentedProofs.Select(x => x.ResourceAddress).ToHashSet())
                        {
                            if (referencedEntities.TryGet(proofResourceAddress, out var re))
                            {
                                ledgerTransactionMarkersToAdd.Add(
                                    new ManifestAddressLedgerTransactionMarker
                                    {
                                        Id = sequences.LedgerTransactionMarkerSequence++,
                                        StateVersion = stateVersion,
                                        OperationType = LedgerTransactionMarkerOperationType.BadgePresented,
                                        EntityId = re.DatabaseId,
                                    });
                            }
                        }

                        foreach (var address in extractedAddresses.ResourceAddresses)
                        {
                            if (referencedEntities.TryGet(address, out var re))
                            {
                                ledgerTransactionMarkersToAdd.Add(
                                    new ManifestAddressLedgerTransactionMarker
                                {
                                    Id = sequences.LedgerTransactionMarkerSequence++,
                                    StateVersion = stateVersion,
                                    OperationType = LedgerTransactionMarkerOperationType.ResourceInUse,
                                    EntityId = re.DatabaseId,
                                });
                            }
                        }

                        foreach (var address in extractedAddresses.AccountsRequiringAuth)
                        {
                            if (referencedEntities.TryGet(address, out var re))
                            {
                                ledgerTransactionMarkersToAdd.Add(
                                    new ManifestAddressLedgerTransactionMarker
                                {
                                    Id = sequences.LedgerTransactionMarkerSequence++,
                                    StateVersion = stateVersion,
                                    OperationType = LedgerTransactionMarkerOperationType.AccountOwnerMethodCall,
                                    EntityId = re.DatabaseId,
                                });
                            }
                        }

                        foreach (var address in extractedAddresses.AccountsDepositedInto)
                        {
                            if (referencedEntities.TryGet(address, out var re))
                            {
                                ledgerTransactionMarkersToAdd.Add(
                                    new ManifestAddressLedgerTransactionMarker
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
                                ledgerTransactionMarkersToAdd.Add(
                                    new ManifestAddressLedgerTransactionMarker
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

            await _observers.ForEachAsync(x => x.StageCompleted("scan_for_changes", sw.Elapsed, null));
        }

        // step: now that all the fundamental data is inserted we can insert some denormalized data
        {
            var sw = Stopwatch.StartNew();

            var mostRecentResourceEntitySupplyHistory = await readHelper.MostRecentResourceEntitySupplyHistoryFor(resourceSupplyChanges, token);

            await entityMetadataProcessor.LoadDependencies();
            await entitySchemaProcessor.LoadDependencies();
            await componentMethodRoyaltyProcessor.LoadDependencies();
            await entityRoleAssignmentProcessor.LoadDependencies();
            await packageCodeProcessor.LoadDependencies();
            await packageBlueprintProcessor.LoadDependencies();
            await validatorProcessor.LoadDependencies();
            await validatorEmissionProcessor.LoadDependencies();
            await keyValueStoreProcessor.LoadDependencies();
            await accountAuthorizedDepositorsProcessor.LoadDependencies();
            await accountResourcePreferenceRulesProcessor.LoadDependencies();
            await accountLockerProcessor.LoadDependencies();
            await standardMetadataProcessor.LoadDependencies();
            await entityResourceProcessor.LoadDependencies();
            await vaultProcessor.LoadDependencies();

            dbReadDuration += sw.Elapsed;

            entityMetadataProcessor.ProcessChanges();
            entitySchemaProcessor.ProcessChanges();
            componentMethodRoyaltyProcessor.ProcessChanges();
            entityRoleAssignmentProcessor.ProcessChanges();
            packageCodeProcessor.ProcessChanges();
            packageBlueprintProcessor.ProcessChanges();
            accountDefaultDepositRuleProcessor.ProcessChanges();
            accountAuthorizedDepositorsProcessor.ProcessChanges();
            accountResourcePreferenceRulesProcessor.ProcessChanges();
            keyValueStoreProcessor.ProcessChanges();
            validatorProcessor.ProcessChanges();
            validatorEmissionProcessor.ProcessChanges();
            accountLockerProcessor.ProcessChanges();
            ledgerTransactionMarkersToAdd.AddRange(globalEventEmitterProcessor.CreateTransactionMarkers());
            ledgerTransactionMarkersToAdd.AddRange(affectedGlobalEntitiesProcessor.CreateTransactionMarkers());
            standardMetadataProcessor.ProcessChanges();
            entityResourceProcessor.ProcessChanges();
            vaultProcessor.ProcessChanges();

            var resourceEntitySupplyHistoryToAdd = resourceSupplyChanges
                .GroupBy(x => new { x.ResourceEntityId, x.StateVersion })
                .Select(
                    group =>
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

            await _observers.ForEachAsync(x => x.StageCompleted("process_changes", sw.Elapsed, null));

            sw = Stopwatch.StartNew();

            rowsInserted += await writeHelper.CopyEntity(entitiesToAdd, token);
            rowsInserted += await writeHelper.CopyLedgerTransaction(ledgerTransactionsToAdd, token);
            rowsInserted += await writeHelper.CopyLedgerTransactionMarkers(ledgerTransactionMarkersToAdd, token);
            rowsInserted += await writeHelper.CopyResourceEntitySupplyHistory(resourceEntitySupplyHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyNonFungibleDataSchemaHistory(nonFungibleSchemaHistoryToAdd, token);
            rowsInserted += await writeHelper.CopyKeyValueStoreSchemaHistory(keyValueStoreSchemaHistoryToAdd, token);

            rowsInserted += await entityStateProcessor.SaveEntities();
            rowsInserted += await entityMetadataProcessor.SaveEntities();
            rowsInserted += await entitySchemaProcessor.SaveEntities();
            rowsInserted += await componentMethodRoyaltyProcessor.SaveEntities();
            rowsInserted += await entityRoleAssignmentProcessor.SaveEntities();
            rowsInserted += await packageCodeProcessor.SaveEntities();
            rowsInserted += await packageBlueprintProcessor.SaveEntities();
            rowsInserted += await accountDefaultDepositRuleProcessor.SaveEntities();
            rowsInserted += await accountAuthorizedDepositorsProcessor.SaveEntities();
            rowsInserted += await accountResourcePreferenceRulesProcessor.SaveEntities();
            rowsInserted += await keyValueStoreProcessor.SaveEntities();
            rowsInserted += await validatorProcessor.SaveEntities();
            rowsInserted += await validatorEmissionProcessor.SaveEntities();
            rowsInserted += await accountLockerProcessor.SaveEntities();
            rowsInserted += await standardMetadataProcessor.SaveEntities();
            rowsInserted += await entityResourceProcessor.SaveEntities();
            rowsInserted += await vaultProcessor.SaveEntities();

            await writeHelper.UpdateSequences(sequences, token);

            dbWriteDuration += sw.Elapsed;

            await _observers.ForEachAsync(x => x.StageCompleted("write_all", sw.Elapsed, null));
        }

        var contentHandlingDuration = outerStopwatch.Elapsed - dbReadDuration - dbWriteDuration;

        return new ExtendLedgerReport(lastTransactionSummary, rowsInserted + rowsUpdated, dbReadDuration, dbWriteDuration, contentHandlingDuration);
    }
}
