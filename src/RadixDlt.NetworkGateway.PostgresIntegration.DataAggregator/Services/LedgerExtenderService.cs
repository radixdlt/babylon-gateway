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
using RadixDlt.NetworkGateway.Commons.CoreCommunications;
using RadixDlt.NetworkGateway.Commons.Numerics;
using RadixDlt.NetworkGateway.Commons.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class LedgerExtenderService : ILedgerExtenderService
{
    private readonly IOptionsMonitor<TransactionAssertionsOptions> _transactionAssertionsOptionsMonitor;
    private readonly ILogger<LedgerExtenderService> _logger;
    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly IRawTransactionWriter _rawTransactionWriter;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IClock _clock;

    private record ProcessTransactionReport(
        long TransactionContentHandlingMs,
        long DbDependenciesLoadingMs,
        int TransactionContentDbActionsCount,
        long LocalDbContextActionsMs
    );

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
        var processTransactionReport = ledgerExtensionReport.ProcessTransactionReport;

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
            processTransactionReport.TransactionContentHandlingMs,
            processTransactionReport.DbDependenciesLoadingMs,
            processTransactionReport.TransactionContentDbActionsCount,
            processTransactionReport.LocalDbContextActionsMs,
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
        ProcessTransactionReport ProcessTransactionReport,
        TransactionSummary FinalTransactionSummary,
        int EntriesWritten,
        long DbPersistenceMs
    );

    private async Task<LedgerExtensionReport> ExtendLedger(ConsistentLedgerExtension ledgerExtension, SyncTarget latestSyncTarget, CancellationToken token)
    {
        // Create own context for ledger extension unit of work
        await using var ledgerExtensionDbContext = await _dbContextFactory.CreateDbContextAsync(token);
        await using var tx = await ledgerExtensionDbContext.Database.BeginTransactionAsync(token);

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

    private async Task<ProcessTransactionReport> BulkProcessTransactionDependenciesAndEntityCreation(
        ReadWriteDbContext dbContext,
        List<CommittedTransactionData> transactions,
        CancellationToken cancellationToken
    )
    {
        var transactionContentProcessingMs = await CodeStopwatch.TimeInMs(
            async () => await ProcessTransactions(dbContext, transactions, cancellationToken)
        );

        return new ProcessTransactionReport(123, 321, 123, 321);
    }

    private async Task ProcessTransactions(ReadWriteDbContext dbContext, List<CommittedTransactionData> transactionsData, CancellationToken token)
    {
        var referencedEntities = new Dictionary<string, ReferencedEntity>();
        var downedSubstates = new List<DownedSubstate>();
        var uppedSubstates = new List<UppedSubstate>();
        var parentEntities = new Dictionary<long, long>();

        // step 1: scan for entities
        {
            foreach (var transactionData in transactionsData)
            {
                var dbTransaction = TransactionMapping.CreateLedgerTransaction(transactionData);

                dbContext.LedgerTransactions.Add(dbTransaction);

                var stateVersion = transactionData.CommittedTransaction.StateVersion;
                var stateUpdates = transactionData.CommittedTransaction.Receipt.StateUpdates;

                foreach (var upSubstate in stateUpdates.UpSubstates)
                {
                    var sid = upSubstate.SubstateId;
                    var re = referencedEntities.GetOrAdd(sid.EntityAddress, _ => new ReferencedEntity(sid.EntityAddress, sid.EntityType, stateVersion));
                    var us = new UppedSubstate(re, sid.SubstateKey, sid.SubstateType, upSubstate._Version, Convert.FromHexString(upSubstate.SubstateDataHash), stateVersion, upSubstate.SubstateData);

                    uppedSubstates.Add(us);
                }

                foreach (var downSubstate in stateUpdates.DownSubstates)
                {
                    var sid = downSubstate.SubstateId;
                    var re = referencedEntities.GetOrAdd(sid.EntityAddress, _ => new ReferencedEntity(sid.EntityAddress, sid.EntityType, stateVersion));
                    var ds = new DownedSubstate(re, sid.SubstateKey, sid.SubstateType, downSubstate._Version, Convert.FromHexString(downSubstate.SubstateDataHash), stateVersion);

                    downedSubstates.Add(ds);
                }

                foreach (var downVirtualSubstate in stateUpdates.DownVirtualSubstates)
                {
                    // TODO not sure how to handle those; not sure what they even are

                    referencedEntities.GetOrAdd(downVirtualSubstate.EntityAddress, _ => new ReferencedEntity(downVirtualSubstate.EntityAddress, downVirtualSubstate.EntityType, stateVersion));
                }

                foreach (var newGlobalEntity in stateUpdates.NewGlobalEntities)
                {
                    referencedEntities[newGlobalEntity.EntityAddress].Globalize(newGlobalEntity.GlobalAddressStr, newGlobalEntity.GlobalAddressBytes);
                }
            }
        }

        // step 2: resolve known types (optionally create missing entities)
        {
            var entityAddresses = referencedEntities.Keys.ToList();

            var knownDbEntities = await dbContext.TmpEntities
                .Where(e => entityAddresses.Contains(e.Address))
                .ToDictionaryAsync(e => e.Address, token);

            foreach (var e in referencedEntities.Values)
            {
                if (knownDbEntities.ContainsKey(e.Address))
                {
                    e.Resolve(knownDbEntities[e.Address]);

                    continue;
                }

                TmpBaseEntity dbEntity = e.Type switch
                {
                    EntityType.System => new TmpSystemEntity(),
                    EntityType.ResourceManager => new TmpResourceManagerEntity(),
                    EntityType.Component => new TmpComponentEntity(),
                    EntityType.Package => new TmpPackageEntity(),
                    EntityType.Vault => new TmpVaultEntity(),
                    EntityType.KeyValueStore => new TmpKeyValueStoreEntity(),
                    _ => throw new Exception("bla bla bla x2"),
                };

                dbEntity.Address = e.Address;
                dbEntity.GlobalAddress = e.GlobalAddressBytes;
                dbEntity.FromStateVersion = e.StateVersion;

                dbContext.TmpEntities.Add(dbEntity);

                e.Resolve(dbEntity);
            }

            await dbContext.SaveChangesAsync(token);
        }

        // step 3: insert all newly seen substates first as some substates we want to delete might not even exist yet!
        {
            TmpResourceManagerSubstate CreateResourceManagerSubstate(UppedSubstate us)
            {
                var data = us.Data.GetResourceManagerSubstate();

                // TODO handle metadata

                return new TmpResourceManagerSubstate
                {
                    TotalSupply = TokenAmount.FromSubUnitsString(data.TotalSupplyAttos),
                    FungibleDivisibility = data.FungibleDivisibility,
                };
            }

            TmpComponentStateSubstate CreateComponentStateSubstate(UppedSubstate us)
            {
                var data = us.Data.GetComponentStateSubstate();

                // TODO handle referenced_entities

                foreach (var oe in data.DataStruct.OwnedEntities)
                {
                    parentEntities[referencedEntities[oe.EntityAddress].DatabaseId] = us.ReferencedEntity.DatabaseId;
                }

                return new TmpComponentStateSubstate();
            }

            TmpVaultSubstate CreateTmpVaultSubstate(UppedSubstate us)
            {
                var data = us.Data.GetVaultSubstate();

                // TODO handle fungible vs non-fungible properly (waiting for CoreApi to decide how they're going to represent the data)

                var fra = data.ResourceAmount.GetFungibleResourceAmount();

                return new TmpVaultSubstate
                {
                    Amount = TokenAmount.FromSubUnitsString(fra.AmountAttos),
                };
            }

            TmpKeyValueStoreEntrySubstate CreateKeyValueStoreEntrySubstate(UppedSubstate us)
            {
                // TODO handle owned_entities and referenced_entities properly (not sure if we can ensure references types have been seen)

                return new TmpKeyValueStoreEntrySubstate();
            }

            foreach (var us in uppedSubstates)
            {
                TmpBaseSubstate dbSubstate = us.Type switch
                {
                    SubstateType.System => new TmpSystemSubstate(),
                    SubstateType.ResourceManager => CreateResourceManagerSubstate(us),
                    SubstateType.ComponentInfo => new TmpComponentInfoSubstate(),
                    SubstateType.ComponentState => CreateComponentStateSubstate(us),
                    SubstateType.Package => new TmpPackageSubstate(),
                    SubstateType.Vault => CreateTmpVaultSubstate(us),
                    SubstateType.NonFungible => new TmpNonFungibleSubstate(),
                    SubstateType.KeyValueStoreEntry => CreateKeyValueStoreEntrySubstate(us),
                    _ => throw new Exception("bla bla bla x3"),
                };

                dbSubstate.Key = us.Key;
                dbSubstate.EntityId = us.ReferencedEntity.DatabaseId;
                dbSubstate.FromStateVersion = us.StateVersion;
                dbSubstate.DataHash = us.DataHash;
                dbSubstate.Version = us.Version;

                dbContext.TmpSubstates.Add(dbSubstate);

                us.Resolve(dbSubstate);
            }

            await dbContext.SaveChangesAsync(token);
        }

        // step 4: update entity hierarchy
        {
            var ids = new List<long>();
            var parentIds = new List<long>();

            foreach (var (entityId, parentId) in parentEntities)
            {
                ids.Add(entityId);
                parentIds.Add(parentId);
            }

            var idsParameter = new NpgsqlParameter("@entity_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = ids };
            var parentIdsParameter = new NpgsqlParameter("@parent_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = parentIds };

            var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $@"
UPDATE tmp_entities AS e
SET parent_id = data.parent_id
FROM (
    SELECT * FROM UNNEST({idsParameter}, {parentIdsParameter}) d(id, parent_id)
) AS data
WHERE e.id = data.id
", token);

            if (parentEntities.Count != affected)
            {
                throw new Exception("bla bla bla x4");
            }
        }

        // step 5: now, that we're sure all the substates exists we can remove some of them
        {
            var substateKeys = new List<string>();
            var entityIds = new List<long>();
            var versions = new List<long>();
            var toStateVersions = new List<long>();

            foreach (var ds in downedSubstates)
            {
                substateKeys.Add(ds.Key);
                entityIds.Add(ds.ReferencedEntity.DatabaseId);
                versions.Add(ds.Version);
                toStateVersions.Add(ds.StateVersion);
            }

            var substateIdsParameter = new NpgsqlParameter("@substate_keys", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = substateKeys };
            var entityIdsParameter = new NpgsqlParameter("@entity_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = entityIds };
            var versionsParameter = new NpgsqlParameter("@versions", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = versions };
            var toStateVersionsParameter = new NpgsqlParameter("@to_state_versions", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = toStateVersions };

            var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $@"
UPDATE tmp_substates AS s
SET is_deleted = true, to_state_version = data.to_state_version
FROM (
    SELECT * FROM UNNEST({substateIdsParameter}, {entityIdsParameter}, {versionsParameter}, {toStateVersionsParameter}) d(key, entity_id, version, to_state_version)
) AS data
WHERE s.key = data.key AND s.entity_id = data.entity_id AND s.version = data.version
", token);

            if (downedSubstates.Count != affected)
            {
                throw new Exception("bla bla bla x5");
            }
        }
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
