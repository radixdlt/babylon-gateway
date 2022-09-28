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
using Npgsql;
using NpgsqlTypes;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons;
using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.Commons.Extensions;
using RadixDlt.NetworkGateway.Commons.Numerics;
using RadixDlt.NetworkGateway.Commons.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ComponentInfoSubstate = RadixDlt.NetworkGateway.PostgresIntegration.Models.ComponentInfoSubstate;
using ComponentStateSubstate = RadixDlt.NetworkGateway.PostgresIntegration.Models.ComponentStateSubstate;
using KeyValueStoreEntrySubstate = RadixDlt.NetworkGateway.PostgresIntegration.Models.KeyValueStoreEntrySubstate;
using NonFungibleSubstate = RadixDlt.NetworkGateway.PostgresIntegration.Models.NonFungibleSubstate;
using PackageSubstate = RadixDlt.NetworkGateway.PostgresIntegration.Models.PackageSubstate;
using ResourceManagerSubstate = RadixDlt.NetworkGateway.PostgresIntegration.Models.ResourceManagerSubstate;
using Substate = RadixDlt.NetworkGateway.PostgresIntegration.Models.Substate;
using SystemSubstate = RadixDlt.NetworkGateway.PostgresIntegration.Models.SystemSubstate;
using VaultSubstate = RadixDlt.NetworkGateway.PostgresIntegration.Models.VaultSubstate;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class LedgerExtenderService : ILedgerExtenderService
{
    private readonly IOptionsMonitor<TransactionAssertionsOptions> _transactionAssertionsOptionsMonitor;
    private readonly ILogger<LedgerExtenderService> _logger;
    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly IRawTransactionWriter _rawTransactionWriter;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IClock _clock;

    public LedgerExtenderService(
        IOptionsMonitor<TransactionAssertionsOptions> transactionAssertionsOptionsMonitor,
        ILogger<LedgerExtenderService> logger,
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        IRawTransactionWriter rawTransactionWriter,
        INetworkConfigurationProvider networkConfigurationProvider,
        IClock clock)
    {
        _transactionAssertionsOptionsMonitor = transactionAssertionsOptionsMonitor;
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _rawTransactionWriter = rawTransactionWriter;
        _networkConfigurationProvider = networkConfigurationProvider;
        _clock = clock;
    }

    public async Task<TransactionSummary> GetTopOfLedger(CancellationToken token)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        return await TransactionSummarisation.GetSummaryOfTransactionOnTopOfLedger(dbContext, _clock, token);
    }

    public async Task<CommitTransactionsReport> CommitTransactions(
        ConsistentLedgerExtension ledgerExtension,
        SyncTargetCarrier latestSyncTarget,
        CancellationToken token
    )
    {
        var preparationReport = await PrepareForLedgerExtension(ledgerExtension, token);

        var ledgerExtensionReport = await ExtendLedger(ledgerExtension, new SyncTarget { TargetStateVersion = latestSyncTarget.TargetStateVersion }, token);
        var processTransactionReport = ledgerExtensionReport.ProcessTransactionsReport;

        var dbEntriesWritten =
            preparationReport.RawTxnUpsertTouchedRecords
            + preparationReport.MempoolTransactionsTouchedRecords
            + preparationReport.PreparationEntriesTouched
            + ledgerExtensionReport.EntriesWritten;

        return new CommitTransactionsReport(
            ledgerExtension.TransactionData.Count,
            ledgerExtensionReport.FinalTransactionSummary,
            preparationReport.RawTxnPersistenceMs,
            preparationReport.MempoolTransactionUpdateMs,
            // TODO fix those
            321,
            321,
            321,
            321,
            // processTransactionReport.TransactionContentHandlingMs,
            // processTransactionReport.DbDependenciesLoadingMs,
            // processTransactionReport.TransactionContentDbActionsCount,
            // processTransactionReport.LocalDbContextActionsMs,
            ledgerExtensionReport.DbPersistenceMs,
            dbEntriesWritten
        );
    }

    private record PreparationForLedgerExtensionReport(
        long RawTxnPersistenceMs,
        int RawTxnUpsertTouchedRecords,
        long MempoolTransactionUpdateMs,
        int MempoolTransactionsTouchedRecords,
        int PreparationEntriesTouched
    );

    /// <summary>
    ///  This should be idempotent - ie can be repeated if the main commit task fails.
    /// </summary>
    private async Task<PreparationForLedgerExtensionReport> PrepareForLedgerExtension(
        ConsistentLedgerExtension ledgerExtension,
        CancellationToken token
    )
    {
        await using var preparationDbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var topOfLedgerSummary = await TransactionSummarisation.GetSummaryOfTransactionOnTopOfLedger(preparationDbContext, _clock, token);

        if (ledgerExtension.ParentSummary.StateVersion != topOfLedgerSummary.StateVersion)
        {
            throw new Exception(
                $"Tried to commit transactions with parent state version {ledgerExtension.ParentSummary.StateVersion} " +
                $"on top of a ledger with state version {topOfLedgerSummary.StateVersion}"
            );
        }

        if (topOfLedgerSummary.StateVersion == 0)
        {
            await EnsureDbLedgerIsInitialized(token);
        }

        var rawTransactions = ledgerExtension.TransactionData.Select(td => new RawTransaction(
            td.TransactionSummary.PayloadHash,
            td.TransactionContents
        )).ToList();

        var (rawTransactionsTouched, rawTransactionCommitMs) = await CodeStopwatch.TimeInMs(
            () => _rawTransactionWriter.EnsureRawTransactionsCreatedOrUpdated(preparationDbContext, rawTransactions, token)
        );

        var (mempoolTransactionsTouched, mempoolTransactionUpdateMs) = await CodeStopwatch.TimeInMs(
            () => _rawTransactionWriter.EnsureMempoolTransactionsMarkedAsCommitted(preparationDbContext, ledgerExtension.TransactionData, token)
        );

        var preparationEntriesTouched = await preparationDbContext.SaveChangesAsync(token);

        return new PreparationForLedgerExtensionReport(
            rawTransactionCommitMs,
            rawTransactionsTouched,
            mempoolTransactionUpdateMs,
            mempoolTransactionsTouched,
            preparationEntriesTouched
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

    private record LedgerExtensionReport(
        ProcessTransactionsReport ProcessTransactionsReport,
        TransactionSummary FinalTransactionSummary,
        int EntriesWritten,
        long DbPersistenceMs
    );

    private async Task<LedgerExtensionReport> ExtendLedger(ConsistentLedgerExtension ledgerExtension, SyncTarget latestSyncTarget, CancellationToken token)
    {
        // Create own context for ledger extension unit of work
        await using var ledgerExtensionDbContext = await _dbContextFactory.CreateDbContextAsync(token);
        await using var tx = await ledgerExtensionDbContext.Database.BeginTransactionAsync(token);

        try
        {
            var processTransactionReport = await BulkProcessTransactionDependenciesAndEntityCreation(
                ledgerExtensionDbContext,
                ledgerExtension.TransactionData,
                token
            );

            var finalTransactionSummary = ledgerExtension.TransactionData.Last().TransactionSummary;

            await CreateOrUpdateLedgerStatus(ledgerExtensionDbContext, finalTransactionSummary, latestSyncTarget, token);

            var (ledgerExtensionEntriesWritten, dbPersistenceMs) = await CodeStopwatch.TimeInMs(
                () => ledgerExtensionDbContext.SaveChangesAsync(token)
            );

            await tx.CommitAsync(token);

            return new LedgerExtensionReport(processTransactionReport, finalTransactionSummary, ledgerExtensionEntriesWritten, dbPersistenceMs);
        }
        catch (Exception)
        {
            await tx.RollbackAsync(token);

            throw;
        }
    }

    private async Task<ProcessTransactionsReport> BulkProcessTransactionDependenciesAndEntityCreation(
        ReadWriteDbContext dbContext,
        List<CommittedTransactionData> transactions,
        CancellationToken cancellationToken
    )
    {
        return await ProcessTransactions(dbContext, transactions, cancellationToken);
    }

    private record ProcessTransactionsReport(Dictionary<string, TimeSpan> Timers, Dictionary<string, string> Notes)
    {
        public TimeSpan Total => TimeSpan.FromMilliseconds(Timers.Where(t => t.Key.EndsWith("_total")).Select(t => t.Value.TotalMilliseconds).Sum());
    }

    private async Task<ProcessTransactionsReport> ProcessTransactions(ReadWriteDbContext dbContext, List<CommittedTransactionData> transactionsData, CancellationToken token)
    {
        var notes = new Dictionary<string, string>(); // TODO replace with proper Activity at some point
        var timers = new Dictionary<string, TimeSpan>(); // TODO replace with proper Activity at some point

        // TODO replace usage of HEX-encoded strings in favor of raw RadixAddress?
        // TODO EF sucks and creates individual INSERT for every single added object,
        // TODO maybe we should just use https://entityframework-extensions.net/bulk-savechanges (difficult to open-source)

        var referencedEntities = new Dictionary<string, ReferencedEntity>();
        var downedSubstates = new List<DownedSubstate>();
        var uppedSubstates = new List<UppedSubstate>();
        var childToParentEntities = new Dictionary<string, string>();
        var fungibleResourceChanges = new List<FungibleResourceChange>();
        var nonFungibleResourceChanges = new List<NonFungibleResourceChange>();
        var metadataChanges = new List<MetadataChange>();

        // step 1: scan for any referenced entities
        {
            var sw = Stopwatch.StartNew();

            foreach (var transactionData in transactionsData)
            {
                var dbTransaction = TransactionMapping.CreateLedgerTransaction(transactionData);

                dbContext.LedgerTransactions.Add(dbTransaction);

                var stateVersion = transactionData.CommittedTransaction.StateVersion;
                var stateUpdates = transactionData.CommittedTransaction.Receipt.StateUpdates;

                foreach (var upSubstate in stateUpdates.UpSubstates)
                {
                    var sid = upSubstate.SubstateId;
                    var re = referencedEntities.GetOrAdd(sid.EntityAddressHex, _ => new ReferencedEntity(sid.EntityAddressHex, sid.EntityType, stateVersion));
                    var us = new UppedSubstate(re, sid.SubstateKeyHex, sid.SubstateType, upSubstate._Version, Convert.FromHexString(upSubstate.SubstateDataHash), stateVersion, upSubstate.SubstateData);

                    if (us.Data.ActualInstance is IOwner owner)
                    {
                        foreach (var oe in owner.OwnedEntities)
                        {
                            referencedEntities.GetOrAdd(oe.EntityAddressHex, _ => new ReferencedEntity(oe.EntityAddressHex, oe.EntityType, stateVersion)).IsChildOf(re);

                            childToParentEntities.Add(oe.EntityAddressHex, sid.EntityAddressHex);
                        }
                    }

                    if (us.Data.ActualInstance is IResourcePointer resourcePointer)
                    {
                        foreach (var typedResource in resourcePointer.PointedResources)
                        {
                            // TODO ugh...
                            var resourceAddress = RadixBech32.Decode(typedResource.Address).Data.ToHex();

                            referencedEntities.GetOrAdd(resourceAddress, _ => new ReferencedEntity(resourceAddress, EntityType.ResourceManager, stateVersion));
                        }
                    }

                    uppedSubstates.Add(us);
                }

                foreach (var downSubstate in stateUpdates.DownSubstates)
                {
                    var sid = downSubstate.SubstateId;
                    var re = referencedEntities.GetOrAdd(sid.EntityAddressHex, _ => new ReferencedEntity(sid.EntityAddressHex, sid.EntityType, stateVersion));
                    var ds = new DownedSubstate(re, sid.SubstateKeyHex, sid.SubstateType, downSubstate._Version, Convert.FromHexString(downSubstate.SubstateDataHash), stateVersion);

                    downedSubstates.Add(ds);
                }

                foreach (var downVirtualSubstate in stateUpdates.DownVirtualSubstates)
                {
                    // TODO not sure how to handle those; not sure what they even are

                    referencedEntities.GetOrAdd(downVirtualSubstate.EntityAddressHex, _ => new ReferencedEntity(downVirtualSubstate.EntityAddressHex, downVirtualSubstate.EntityType, stateVersion));
                }

                foreach (var newGlobalEntity in stateUpdates.NewGlobalEntities)
                {
                    referencedEntities[newGlobalEntity.EntityAddressHex].Globalize(newGlobalEntity.GlobalAddressHex);
                }
            }

            notes.Add("step1_count", transactionsData.Count.ToString());
            timers.Add("step1_total", sw.Elapsed);
        }

        // step 2: resolve known types & optionally create missing entities
        {
            var sw = Stopwatch.StartNew();
            var c = 0;

            ComponentEntity CreateComponentEntity(ReferencedEntity re)
            {
                // TODO use some enum or something!

                var kind = "normal";

                if (re.Address.StartsWith(_networkConfigurationProvider.GetAddressHrps().AccountHrp))
                {
                    kind = "account";
                }
                else if (re.Address.StartsWith(_networkConfigurationProvider.GetAddressHrps().ValidatorHrp))
                {
                    kind = "validator";
                }

                return new ComponentEntity
                {
                    Kind = kind,
                };
            }

            var entityAddresses = referencedEntities.Keys.Select(x => x.ConvertFromHex()).ToList();
            var entityAddressesParameter = new NpgsqlParameter("@entity_ids", NpgsqlDbType.Array | NpgsqlDbType.Bytea) { Value = entityAddresses };

            var knownDbEntities = await dbContext.Entities
                .FromSqlInterpolated($@"
SELECT *
FROM entities
WHERE id IN(
    SELECT DISTINCT UNNEST(ARRAY[id, parent_id, owner_ancestor_id, global_ancestor_id]) AS id
    FROM entities
    WHERE address = ANY({entityAddressesParameter})
)")
                .ToDictionaryAsync(e => ((byte[])e.Address).ToHex(), token);

            notes.Add("step2_loadCount", knownDbEntities.Count.ToString());
            timers.Add("step2_load", sw.Elapsed);

            foreach (var e in referencedEntities.Values)
            {
                if (knownDbEntities.ContainsKey(e.Address))
                {
                    e.Resolve(knownDbEntities[e.Address]);

                    continue;
                }

                Entity dbEntity = e.Type switch
                {
                    EntityType.System => new SystemEntity(),
                    EntityType.ResourceManager => new ResourceManagerEntity(),
                    EntityType.Component => CreateComponentEntity(e),
                    EntityType.Package => new PackageEntity(),
                    EntityType.Vault => new VaultEntity(),
                    EntityType.KeyValueStore => new ValueStoreEntity(),
                    _ => throw new Exception("bla bla bla x2"), // TODO fix me
                };

                dbEntity.FromStateVersion = e.StateVersion;
                dbEntity.Address = e.Address.ConvertFromHex();
                dbEntity.GlobalAddress = e.GlobalAddressBytes == null ? null : (RadixAddress)e.GlobalAddressBytes;

                dbContext.Entities.Add(dbEntity);

                e.Resolve(dbEntity);

                c++;
            }

            await dbContext.SaveChangesAsync(token);

            notes.Add("step2_saveCount", c.ToString());
            timers.Add("step2_total", sw.Elapsed);
        }

        // step 3: insert all newly seen substates first as some substates we want to delete might not even exist yet!
        {
            var sw = Stopwatch.StartNew();
            var c = 0;

            ResourceManagerSubstate CreateResourceManagerSubstate(UppedSubstate us)
            {
                var data = us.Data.GetResourceManagerSubstate();

                metadataChanges.Add(new MetadataChange(us.ReferencedEntity, data.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), us.StateVersion));

                return new ResourceManagerSubstate
                {
                    TotalSupply = TokenAmount.FromSubUnitsString(data.TotalSupplyAttos),
                    FungibleDivisibility = data.FungibleDivisibility,
                };
            }

            ComponentStateSubstate CreateComponentStateSubstate(UppedSubstate us)
            {
                var data = us.Data.GetComponentStateSubstate();

                return new ComponentStateSubstate();
            }

            VaultSubstate CreateTmpVaultSubstate(UppedSubstate us)
            {
                var data = us.Data.GetVaultSubstate();

                // TODO handle fungible vs non-fungible properly (waiting for CoreApi to decide how they're going to represent the data)

                var substate = new VaultSubstate();

                // TODO ugh...
                var resourceAmount = data.ResourceAmount.ActualInstance;
                var substateEntity = us.ReferencedEntity;

                if (resourceAmount is FungibleResourceAmount fra)
                {
                    substate.Amount = TokenAmount.FromSubUnitsString(fra.AmountAttos);

                    var resourceAddress = RadixBech32.Decode(fra.ResourceAddress).Data.ToHex();
                    var resourceEntity = referencedEntities[resourceAddress];

                    fungibleResourceChanges.Add(new FungibleResourceChange(substateEntity, resourceEntity, substate.Amount, us.StateVersion));

                    return substate;
                }

                if (resourceAmount is NonFungibleResourceAmount nfra)
                {
                    var resourceAddress = RadixBech32.Decode(nfra.ResourceAddress).Data.ToHex();
                    var resourceEntity = referencedEntities[resourceAddress];

                    nonFungibleResourceChanges.Add(new NonFungibleResourceChange(substateEntity, resourceEntity, nfra.NfIdsHex, us.StateVersion));

                    return substate;
                }

                throw new Exception("bla bla bla bla x9"); // TODO fix me
            }

            KeyValueStoreEntrySubstate CreateKeyValueStoreEntrySubstate(UppedSubstate us)
            {
                // TODO handle referenced_entities properly (not sure if we can ensure references types have been seen)

                return new KeyValueStoreEntrySubstate();
            }

            foreach (var us in uppedSubstates)
            {
                Substate dbSubstate = us.Type switch
                {
                    SubstateType.System => new SystemSubstate(),
                    SubstateType.ResourceManager => CreateResourceManagerSubstate(us),
                    SubstateType.ComponentInfo => new ComponentInfoSubstate(),
                    SubstateType.ComponentState => CreateComponentStateSubstate(us),
                    SubstateType.Package => new PackageSubstate(),
                    SubstateType.Vault => CreateTmpVaultSubstate(us),
                    SubstateType.NonFungible => new NonFungibleSubstate(),
                    SubstateType.KeyValueStoreEntry => CreateKeyValueStoreEntrySubstate(us),
                    _ => throw new Exception("bla bla bla x3"), // TODO fix me
                };

                dbSubstate.FromStateVersion = us.StateVersion;
                dbSubstate.Key = us.Key.ConvertFromHex();
                dbSubstate.EntityId = us.ReferencedEntity.DatabaseId;
                dbSubstate.DataHash = us.DataHash;
                dbSubstate.Version = us.Version;

                dbContext.Substates.Add(dbSubstate);

                us.Resolve(dbSubstate);

                c++;
            }

            await dbContext.SaveChangesAsync(token);

            notes.Add("step3_saveCount", c.ToString());
            timers.Add("step3_total", sw.Elapsed);
        }

        // step 4: update entity hierarchy
        {
            var sw = Stopwatch.StartNew();

            var ids = new List<long>();
            var parentIds = new List<long>();
            var ownerIds = new List<long>();
            var globalIds = new List<long>();

            foreach (var (childAddress, parentAddress) in childToParentEntities)
            {
                var globalAncestor = referencedEntities[parentAddress];
                var ownerAncestor = referencedEntities[parentAddress];

                while (globalAncestor.HasParent)
                {
                    globalAncestor = globalAncestor.Parent;
                }

                while (!ownerAncestor.IsOwner)
                {
                    ownerAncestor = ownerAncestor.Parent;
                }

                ids.Add(referencedEntities[childAddress].DatabaseId);
                parentIds.Add(referencedEntities[parentAddress].DatabaseId);
                ownerIds.Add(ownerAncestor.DatabaseId);
                globalIds.Add(globalAncestor.DatabaseId);

                referencedEntities[childAddress].ResolveParentalIds(referencedEntities[parentAddress].DatabaseId, ownerAncestor.DatabaseId, globalAncestor.DatabaseId);
            }

            var idsParameter = new NpgsqlParameter("@entity_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = ids };
            var parentIdsParameter = new NpgsqlParameter("@parent_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = parentIds };
            var ownerIdsParameter = new NpgsqlParameter("@owner_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = ownerIds };
            var globalIdsParameter = new NpgsqlParameter("@global_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = globalIds };

            var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $@"
UPDATE entities AS e
SET parent_id = data.parent_id, owner_ancestor_id = data.owner_ancestor_id, global_ancestor_id = data.global_ancestor_id
FROM (
    SELECT * FROM UNNEST({idsParameter}, {parentIdsParameter}, {ownerIdsParameter}, {globalIdsParameter}) d(id, parent_id, owner_ancestor_id, global_ancestor_id)
) AS data
WHERE e.id = data.id
",
                token);

            if (childToParentEntities.Count != affected)
            {
                throw new Exception("bla bla bla x4"); // TODO fix me
            }

            notes.Add("step4_updated", affected.ToString());
            timers.Add("step4_total", sw.Elapsed);
        }

        // step 5: now, that we're sure all the substates exists we can remove some of them
        {
            var sw = Stopwatch.StartNew();

            var substateKeys = new List<byte[]>();
            var entityIds = new List<long>();
            var versions = new List<long>();
            var toStateVersions = new List<long>();

            foreach (var ds in downedSubstates)
            {
                substateKeys.Add(ds.Key.ConvertFromHex());
                entityIds.Add(ds.ReferencedEntity.DatabaseId);
                versions.Add(ds.Version);
                toStateVersions.Add(ds.StateVersion);
            }

            var substateIdsParameter = new NpgsqlParameter("@substate_keys", NpgsqlDbType.Array | NpgsqlDbType.Bytea) { Value = substateKeys };
            var entityIdsParameter = new NpgsqlParameter("@entity_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = entityIds };
            var versionsParameter = new NpgsqlParameter("@versions", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = versions };
            var toStateVersionsParameter = new NpgsqlParameter("@to_state_versions", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = toStateVersions };

            var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $@"
UPDATE substates AS s
SET is_deleted = true, to_state_version = data.to_state_version
FROM (
    SELECT * FROM UNNEST({substateIdsParameter}, {entityIdsParameter}, {versionsParameter}, {toStateVersionsParameter}) d(key, entity_id, version, to_state_version)
) AS data
WHERE s.key = data.key AND s.entity_id = data.entity_id AND s.version = data.version
", token);

            if (downedSubstates.Count != affected)
            {
                // TODO sometimes affected is off by exactly one, not sure why
                Console.WriteLine("again...");
                // throw new Exception("bla bla bla x5"); // TODO fix me
            }

            notes.Add("step5_updated", affected.ToString());
            timers.Add("step5_total", sw.Elapsed);
        }

        // step 6: now that all the fundamental data is inserted (entities & substates) we can insert some denormalized data
        {
            var sw = Stopwatch.StartNew();

            // entity_id => state_version => resource_id[] (added or removed)
            var aggregateDelta = new Dictionary<long, Dictionary<long, AggregateChange>>();

            var fungibles = fungibleResourceChanges
                .Select(e =>
                {
                    aggregateDelta.GetOrAdd(e.SubstateEntity.DatabaseId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendFungible(e.ResourceEntity.DatabaseId);
                    aggregateDelta.GetOrAdd(e.SubstateEntity.DatabaseOwnerAncestorId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendFungible(e.ResourceEntity.DatabaseId);
                    aggregateDelta.GetOrAdd(e.SubstateEntity.DatabaseGlobalAncestorId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendFungible(e.ResourceEntity.DatabaseId);

                    return new EntityFungibleResourceHistory
                    {
                        FromStateVersion = e.StateVersion,
                        OwnerEntityId = e.SubstateEntity.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.SubstateEntity.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.ResourceEntity.DatabaseId,
                        Balance = e.Balance,
                    };
                })
                .ToList();

            var nonFungibles = nonFungibleResourceChanges
                .Select(e =>
                {
                    // TODO handle removal (is_deleted)

                    aggregateDelta.GetOrAdd(e.SubstateEntity.DatabaseId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendNonFungible(e.ResourceEntity.DatabaseId);
                    aggregateDelta.GetOrAdd(e.SubstateEntity.DatabaseOwnerAncestorId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendNonFungible(e.ResourceEntity.DatabaseId);
                    aggregateDelta.GetOrAdd(e.SubstateEntity.DatabaseGlobalAncestorId, _ => new Dictionary<long, AggregateChange>()).GetOrAdd(e.StateVersion, _ => new AggregateChange(e.StateVersion)).AppendNonFungible(e.ResourceEntity.DatabaseId);

                    return new EntityNonFungibleResourceHistory
                    {
                        FromStateVersion = e.StateVersion,
                        OwnerEntityId = e.SubstateEntity.DatabaseOwnerAncestorId,
                        GlobalEntityId = e.SubstateEntity.DatabaseGlobalAncestorId,
                        ResourceEntityId = e.ResourceEntity.DatabaseId,
                        IdsCount = e.Ids.Count,
                        Ids = e.Ids.Select(id => referencedEntities[id].DatabaseId).ToArray(),
                    };
                })
                .ToList();

            var metadata = metadataChanges
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
                        FromStateVersion = e.StateVersion,
                        EntityId = e.ResourceEntity.DatabaseId,
                        Keys = keys.ToArray(),
                        Values = values.ToArray(),
                    };
                })
                .ToList();

            var aggregateDeltaIds = aggregateDelta.Keys.ToList();
            var existingAggregates = await dbContext.EntityResourceAggregateHistory
                .Where(e => e.IsMostRecent)
                .Where(e => aggregateDeltaIds.Contains(e.EntityId))
                .ToDictionaryAsync(e => e.EntityId, token);

            var aggregates = new List<EntityResourceAggregateHistory>();
            var lastAggregateByEntity = new Dictionary<long, EntityResourceAggregateHistory>();

            foreach (var (entityId, aggregateChange) in aggregateDelta)
            {
                if (existingAggregates.ContainsKey(entityId))
                {
                    var existingAggregate = existingAggregates[entityId];

                    aggregateChange.Add(existingAggregate.FromStateVersion, new AggregateChange(existingAggregate.FromStateVersion, existingAggregate.FungibleResourceIds, existingAggregate.NonFungibleResourceIds));
                }

                var orderedStateVersions = aggregateChange.Keys.OrderBy(k => k).ToArray();

                for (var i = 0; i < orderedStateVersions.Length; i++)
                {
                    var current = aggregateChange[orderedStateVersions[i]];
                    var previous = i > 0 ? aggregateChange[orderedStateVersions[i - 1]] : null;

                    if (!current.Persistable)
                    {
                        continue;
                    }

                    if (previous != null)
                    {
                        current.Merge(previous);
                    }

                    if (previous == null || current.FungibleIds.SequenceEqual(previous.FungibleIds) == false)
                    {
                        current.Resolve();

                        var dbAggregate = new EntityResourceAggregateHistory
                        {
                            EntityId = entityId,
                            FromStateVersion = orderedStateVersions[i],
                            FungibleResourceIds = current.FungibleIds.ToArray(),
                            NonFungibleResourceIds = current.NonFungibleIds.ToArray(),
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

            if (existingAggregates.Any())
            {
                // update only those aggregates that will be modified in next step
                var ids = existingAggregates.Values.Select(e => e.Id).Intersect(lastAggregateByEntity.Keys).ToList();
                var idsParameter = new NpgsqlParameter("@ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = ids };

                var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE entity_resource_aggregate_history SET is_most_recent = false WHERE id = ANY({idsParameter})",
                    token);

                if (ids.Count != affected)
                {
                    throw new Exception("bla bla bla x6"); // TODO fix me
                }

                notes.Add("step6_updateAffected", affected.ToString());
                timers.Add("step6_update", sw.Elapsed);
            }

            // TODO super temp, read from SEQ! (setval(getval + N)) - this is not super safe on its own but we're guaranteed DA is running in single instance (no concurrency)
            var metadata_pk = 1_000_000;
            var resource_pk = 1_000_000;
            var agg_pk = 1_000_000;
            var dbConn = (NpgsqlConnection)dbContext.Database.GetDbConnection();

            timers.Add("step6_pre", sw.Elapsed);

            await Task.WhenAll(
                WriteAggHist(dbConn, aggregates, token),
                WriteResHist(dbConn, fungibles, nonFungibles, token),
                WriteMetaDataHist(dbConn, metadata, token));

            async Task WriteAggHist(NpgsqlConnection dbConn, List<EntityResourceAggregateHistory> aggregates, CancellationToken token)
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY entity_resource_aggregate_history (id, from_state_version, entity_id, is_most_recent, fungible_resource_ids, non_fungible_resource_ids) FROM STDIN (FORMAT BINARY)", token);

                foreach (var aggregate in aggregates)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(agg_pk++, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.IsMostRecent, NpgsqlDbType.Boolean, token);
                    await writer.WriteAsync(aggregate.FungibleResourceIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.NonFungibleResourceIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                }

                await writer.CompleteAsync(token);
            }

            async Task WriteResHist(NpgsqlConnection dbConn, List<EntityFungibleResourceHistory> fungibles, List<EntityNonFungibleResourceHistory> nonFungibles, CancellationToken token)
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY entity_resource_history (id, from_state_version, owner_entity_id, global_entity_id, resource_entity_id, type, balance, ids_count, ids) FROM STDIN (FORMAT BINARY)", token);
                var type = "fungible";

                foreach (var fungible in fungibles)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(resource_pk++, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.OwnerEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.GlobalEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(type, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(fungible.Balance.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token); // TODO change type!
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                }

                type = "non_fungible";

                foreach (var nonFungible in nonFungibles)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(resource_pk++, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.OwnerEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.GlobalEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(type, NpgsqlDbType.Text, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(nonFungible.IdsCount, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.Ids, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                }

                await writer.CompleteAsync(token);
            }

            async Task WriteMetaDataHist(NpgsqlConnection dbConn, List<EntityMetadataHistory> metadata, CancellationToken token)
            {
                await using var writer = await dbConn.BeginBinaryImportAsync("COPY entity_metadata_history (id, from_state_version, entity_id, keys, values) FROM STDIN (FORMAT BINARY)", token);

                foreach (var md in metadata)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(metadata_pk++, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.Keys, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
                    await writer.WriteAsync(md.Values, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
                }

                await writer.CompleteAsync(token);
            }

            notes.Add("step6_aggCnt", aggregates.Count.ToString());
            notes.Add("step6_funCnt", fungibles.Count.ToString());
            notes.Add("step6_nonFunCnt", nonFungibles.Count.ToString());
            notes.Add("step6_metaCnt", metadata.Count.ToString());
            timers.Add("step6_total", sw.Elapsed);
        }

        return new ProcessTransactionsReport(timers, notes);
    }

    private async Task CreateOrUpdateLedgerStatus(
        ReadWriteDbContext dbContext,
        TransactionSummary finalTransactionSummary,
        SyncTarget latestSyncTarget,
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
        ledgerStatus.TopOfLedgerStateVersion = finalTransactionSummary.StateVersion;
        ledgerStatus.SyncTarget = latestSyncTarget;
    }
}
