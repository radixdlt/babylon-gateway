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
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.DataAggregator;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.AccountSecurityRules;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.LedgerTransactionMarkers;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
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
    private record ExtendLedgerReport(TransactionSummary FinalTransaction, int RowsTouched, TimeSpan DbReadDuration, TimeSpan DbWriteDuration, TimeSpan ContentHandlingDuration);

    private readonly ILogger<PostgresLedgerExtenderService> _logger;
    private readonly ILogger<EntitiesByRoleRequirementProcessor> _entitiesByRoleRequirementProcessorLogger;
    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ITopOfLedgerProvider _topOfLedgerProvider;
    private readonly IEnumerable<ILedgerExtenderServiceObserver> _observers;
    private readonly IClock _clock;
    private readonly IOptionsMonitor<StorageOptions> _storageOptionsMonitor;
    private readonly IOptionsMonitor<LedgerProcessorsOptions> _ledgerProcessorsOptionsMonitor;

    public PostgresLedgerExtenderService(
        ILogger<PostgresLedgerExtenderService> logger,
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        INetworkConfigurationProvider networkConfigurationProvider,
        IEnumerable<ILedgerExtenderServiceObserver> observers,
        IClock clock,
        ITopOfLedgerProvider topOfLedgerProvider,
        IOptionsMonitor<StorageOptions> storageOptionsMonitor,
        IOptionsMonitor<LedgerProcessorsOptions> ledgerProcessorsOptionsMonitor,
        ILogger<EntitiesByRoleRequirementProcessor> entitiesByRoleRequirementProcessorLogger)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _networkConfigurationProvider = networkConfigurationProvider;
        _observers = observers;
        _clock = clock;
        _topOfLedgerProvider = topOfLedgerProvider;
        _storageOptionsMonitor = storageOptionsMonitor;
        _ledgerProcessorsOptionsMonitor = ledgerProcessorsOptionsMonitor;
        _entitiesByRoleRequirementProcessorLogger = entitiesByRoleRequirementProcessorLogger;
    }

    public async Task<CommitTransactionsReport> CommitTransactions(ConsistentLedgerExtension ledgerExtension, CancellationToken token = default)
    {
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
            .Where(ct => ct.LedgerTransaction is CoreModel.UserLedgerTransaction or CoreModel.UserLedgerTransactionV2)
            .Select(ct =>
            {
                return ct.LedgerTransaction switch
                {
                    CoreModel.UserLedgerTransaction ult => ult.NotarizedTransaction.HashBech32m,
                    CoreModel.UserLedgerTransactionV2 ultv2 => ultv2.NotarizedTransaction.HashBech32m,
                    _ => throw new UnreachableException($"Expected UserLedgerTransaction or UserLedgerTransactionV2 but found {ct.LedgerTransaction.GetType()}"),
                };
            })
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

        var readHelper = new ReadHelper(dbContext, _observers, token);
        var writeHelper = new WriteHelper(dbContext, _observers, token);

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

        var processorContext = new ProcessorContext(sequences, _storageOptionsMonitor.CurrentValue, readHelper, writeHelper, networkConfiguration, token);

        // Have to be created here as we're sharing them between
        // - ledgerTransactionProcessor (produces manifest class for transaction)
        // - LedgerTransactionMarkersProcessor (produces markers for manifest addresses and markers for manifest classes)
        var manifestProcessor = new ManifestProcessor(processorContext, referencedEntities, networkConfiguration);

        // Have to be created here as we're sharing them between
        // - ledgerTransactionProcessor (produces affected global entities per transaction)
        // - LedgerTransactionMarkersProcessor (produces markers affected global entities)
        var affectedGlobalEntitiesProcessor = new AffectedGlobalEntitiesProcessor(processorContext, referencedEntities, networkConfiguration);

        // Created explicitly here not as part of processors list as it's completely different processor and one of it's own.
        // Better to call it explicitly.
        var entityRelationshipProcessor = new EntityRelationshipProcessor(referencedEntities);

        // This processor has to be assigned here as we're using it to GetSummaryOfLastProcessedTransaction()
        // Can't be created as item in processors list.
        var ledgerTransactionProcessor = new LedgerTransactionProcessor(
            processorContext,
            _clock,
            referencedEntities,
            manifestProcessor,
            affectedGlobalEntitiesProcessor,
            writeHelper,
            ledgerExtension.LatestTransactionSummary);

        var processors = new List<IProcessorBase>
        {
            ledgerTransactionProcessor,
            new LedgerTransactionMarkersProcessor(manifestProcessor, affectedGlobalEntitiesProcessor, processorContext, referencedEntities, writeHelper, networkConfiguration),
            new ResourceSupplyProcessor(processorContext),
            new EntityStateProcessor(processorContext, referencedEntities),
            new EntityMetadataProcessor(processorContext),
            new EntitySchemaProcessor(processorContext, referencedEntities),
            new ComponentMethodRoyaltyProcessor(processorContext),
            new EntityRoleAssignmentProcessor(processorContext),
            new EntitiesByRoleRequirementProcessor(processorContext, dbContext, referencedEntities, _ledgerProcessorsOptionsMonitor, _observers, _entitiesByRoleRequirementProcessorLogger),
            new PackageCodeProcessor(processorContext),
            new PackageBlueprintProcessor(processorContext, referencedEntities),
            new AccountAuthorizedDepositorsProcessor(processorContext, referencedEntities),
            new AccountResourcePreferenceRulesProcessor(processorContext, referencedEntities),
            new AccountDefaultDepositRuleProcessor(processorContext),
            new KeyValueStoreProcessor(processorContext),
            new ValidatorProcessor(processorContext, referencedEntities),
            new ValidatorEmissionProcessor(processorContext),
            new AccountLockerProcessor(processorContext, referencedEntities),
            new StandardMetadataProcessor(processorContext, referencedEntities),
            new EntityResourceProcessor(processorContext, dbContext, _observers),
            new VaultProcessor(processorContext),
            new ImplicitRequirementsProcessor(processorContext, referencedEntities, dbContext, _observers),
        };

        // step: scan for any referenced entities
        {
            var sw = Stopwatch.StartNew();

            referencedEntities.MarkSeenAddress((EntityAddress)networkConfiguration.WellKnownAddresses.TransactionTracker);
            referencedEntities.MarkSeenAddress((EntityAddress)networkConfiguration.WellKnownAddresses.ConsensusManager);
            referencedEntities.MarkSeenAddress((EntityAddress)networkConfiguration.WellKnownAddresses.Xrd);
            referencedEntities.MarkSeenAddress((EntityAddress)networkConfiguration.WellKnownAddresses.Secp256k1SignatureVirtualBadge);
            referencedEntities.MarkSeenAddress((EntityAddress)networkConfiguration.WellKnownAddresses.Ed25519SignatureVirtualBadge);

            foreach (var committedTransaction in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = committedTransaction.ResultantStateIdentifiers.StateVersion;
                var stateUpdates = committedTransaction.Receipt.StateUpdates;
                var events = committedTransaction.Receipt.Events ?? new List<CoreModel.Event>();

                try
                {
                    foreach (var processor in processors.OfType<ITransactionScanProcessor>())
                    {
                        processor.OnTransactionScan(committedTransaction, stateVersion);
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
                            referencedEntity.PostResolveConfigure(
                                (GlobalFungibleResourceEntity e) =>
                                {
                                    e.Divisibility = fungibleResourceManager.Value.Divisibility;
                                });
                        }

                        if (substateData is CoreModel.NonFungibleResourceManagerFieldMutableFieldsSubstate mutableFields)
                        {
                            referencedEntity.PostResolveConfigure(
                                (GlobalNonFungibleResourceEntity e) =>
                                {
                                    e.NonFungibleDataMutableFields = mutableFields.Value.MutableFields.Select(x => x.Name).ToList();
                                });
                        }

                        if (substateData is CoreModel.NonFungibleResourceManagerFieldIdTypeSubstate nonFungibleResourceManagerFieldIdTypeSubstate)
                        {
                            referencedEntity.PostResolveConfigure(
                                (GlobalNonFungibleResourceEntity e) =>
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
                            referencedEntity.PostResolveConfigure(
                                (ComponentEntity e) =>
                                {
                                    e.AssignedModuleIds = objectDetails
                                        .ModuleVersions
                                        .Select(
                                            x =>
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

                        entityRelationshipProcessor.OnUpsertScan(substate, referencedEntity, stateVersion);

                        foreach (var processor in processors.OfType<ISubstateScanUpsertProcessor>())
                        {
                            processor.OnUpsertScan(substate, referencedEntity, stateVersion);
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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process transaction with state version: {StateVersion} at referenced entities scan stage", stateVersion);
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
                    .PostResolveConfigureHigh(
                        (Entity dbe) =>
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

        // step: scan all substates & events to figure out changes
        {
            var sw = Stopwatch.StartNew();

            foreach (var committedTransaction in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = committedTransaction.ResultantStateIdentifiers.StateVersion;
                var stateUpdates = committedTransaction.Receipt.StateUpdates;
                var events = committedTransaction.Receipt.Events ?? new List<CoreModel.Event>();

                foreach (var processor in processors.OfType<ITransactionProcessor>())
                {
                    processor.VisitTransaction(committedTransaction, stateVersion);
                }

                try
                {
                    foreach (var substate in stateUpdates.UpsertedSubstates)
                    {
                        var substateId = substate.SubstateId;
                        var referencedEntity = referencedEntities.Get((EntityAddress)substateId.EntityAddress);

                        foreach (var processor in processors.OfType<ISubstateUpsertProcessor>())
                        {
                            processor.VisitUpsert(substate, referencedEntity, stateVersion);
                        }
                    }

                    foreach (var deletedSubstate in stateUpdates.DeletedSubstates)
                    {
                        var substateId = deletedSubstate.SubstateId;
                        var referencedEntity = referencedEntities.GetOrAdd((EntityAddress)substateId.EntityAddress, ea => new ReferencedEntity(ea, substateId.EntityType, stateVersion));

                        foreach (var processor in processors.OfType<ISubstateDeleteProcessor>())
                        {
                            processor.VisitDelete(substateId, referencedEntity, stateVersion);
                        }
                    }

                    foreach (var @event in events)
                    {
                        foreach (var processor in processors.OfType<IEventProcessor>())
                        {
                            processor.VisitEvent(@event, stateVersion);
                        }

                        if (@event.Type.Emitter is not CoreModel.MethodEventEmitterIdentifier methodEventEmitter
                            || methodEventEmitter.ObjectModuleId != CoreModel.ModuleId.Main
                            || methodEventEmitter.Entity.EntityType == CoreModel.EntityType.GlobalGenericComponent
                            || methodEventEmitter.Entity.EntityType == CoreModel.EntityType.InternalGenericComponent)
                        {
                            continue;
                        }

                        var eventEmitterEntity = referencedEntities.Get((EntityAddress)methodEventEmitter.Entity.EntityAddress);
                        using var decodedEvent = EventDecoder.DecodeEvent(@event, networkConfiguration.Id);

                        foreach (var processor in processors.OfType<IDecodedEventProcessor>())
                        {
                            processor.VisitDecodedEvent(decodedEvent, eventEmitterEntity, stateVersion);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var x = stateVersion;
                    _logger.LogError(ex, "Failed to process transaction with state version: {StateVersion} at substate processing stage", stateVersion);
                    throw;
                }
            }

            await _observers.ForEachAsync(x => x.StageCompleted("scan_for_changes", sw.Elapsed, null));
        }

        // step: now that all the fundamental data is inserted we can insert some denormalized data
        {
            var sw = Stopwatch.StartNew();

            foreach (var processor in processors)
            {
                await processor.LoadDependenciesAsync();
            }

            dbReadDuration += sw.Elapsed;

            foreach (var processor in processors)
            {
                processor.ProcessChanges();
            }

            await _observers.ForEachAsync(x => x.StageCompleted("process_changes", sw.Elapsed, null));

            sw = Stopwatch.StartNew();

            rowsInserted += await writeHelper.CopyEntity(entitiesToAdd, token);

            foreach (var processor in processors)
            {
                rowsInserted += await processor.SaveEntitiesAsync();
            }

            await writeHelper.UpdateSequences(sequences, token);

            dbWriteDuration += sw.Elapsed;

            await _observers.ForEachAsync(x => x.StageCompleted("write_all", sw.Elapsed, null));
        }

        var contentHandlingDuration = outerStopwatch.Elapsed - dbReadDuration - dbWriteDuration;

        return new ExtendLedgerReport(
            ledgerTransactionProcessor.GetSummaryOfLastProcessedTransaction(),
            rowsInserted + rowsUpdated,
            dbReadDuration,
            dbWriteDuration,
            contentHandlingDuration);
    }
}
