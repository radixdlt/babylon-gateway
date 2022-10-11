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

using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons;
using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.Commons.Extensions;
using RadixDlt.NetworkGateway.Commons.Numerics;
using RadixDlt.NetworkGateway.Commons.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class LedgerExtenderService : ILedgerExtenderService
{
    private readonly ILogger<LedgerExtenderService> _logger;
    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly IRawTransactionWriter _rawTransactionWriter;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IClock _clock;

    public LedgerExtenderService(
        ILogger<LedgerExtenderService> logger,
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        IRawTransactionWriter rawTransactionWriter,
        INetworkConfigurationProvider networkConfigurationProvider,
        IClock clock)
    {
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

    public async Task<CommitTransactionsReport> CommitTransactions(ConsistentLedgerExtension ledgerExtension, SyncTargetCarrier latestSyncTarget, CancellationToken token)
    {
        var preparationReport = await PrepareForLedgerExtension(ledgerExtension, token);
        var ledgerExtensionReport = await ExtendLedger(ledgerExtension, latestSyncTarget.TargetStateVersion, token);
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
            (long)processTransactionReport.ContentHandlingDuration.TotalMilliseconds,
            (long)processTransactionReport.DbReadDuration.TotalMilliseconds,
            -1, // TODO unused
            -1, // TODO unused
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
    private async Task<PreparationForLedgerExtensionReport> PrepareForLedgerExtension(ConsistentLedgerExtension ledgerExtension, CancellationToken token)
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
            td.TransactionSummary.StateVersion,
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

    private async Task<LedgerExtensionReport> ExtendLedger(ConsistentLedgerExtension ledgerExtension, long latestSyncTarget, CancellationToken token)
    {
        // Create own context for ledger extension unit of work
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        await using var tx = await dbContext.Database.BeginTransactionAsync(token);

        try
        {
            var processTransactionsReport = await ProcessTransactions(dbContext, ledgerExtension.TransactionData, token);
            var finalTransactionSummary = ledgerExtension.TransactionData.Last().TransactionSummary;

            await CreateOrUpdateLedgerStatus(dbContext, finalTransactionSummary, latestSyncTarget, token);

            var (remainingRowsTouched, remainingDbDuration) = await CodeStopwatch.TimeInMs(
                () => dbContext.SaveChangesAsync(token)
            );

            await tx.CommitAsync(token);

            return new LedgerExtensionReport(
                processTransactionsReport,
                finalTransactionSummary,
                processTransactionsReport.RowsTouched + remainingRowsTouched,
                ((long)processTransactionsReport.DbWriteDuration.TotalMilliseconds) + remainingDbDuration);
        }
        catch (Exception)
        {
            await tx.RollbackAsync(token);

            throw;
        }
    }

    private record ProcessTransactionsReport(
        int RowsTouched,
        TimeSpan DbReadDuration,
        TimeSpan DbWriteDuration,
        TimeSpan ContentHandlingDuration
    );

    private async Task<ProcessTransactionsReport> ProcessTransactions(ReadWriteDbContext dbContext, List<CommittedTransactionData> transactionsData, CancellationToken token)
    {
        // TODO further improvements:
        // - queries with WHERE xxx = ANY(<list of 12345 ids>) are probably not very performant
        // - we must somehow benefit from EF-stored configuration (type mapping etc.)

        // TODO replace with proper Activity at some point
        var rowsInserted = 0;
        var rowsUpdated = 0;
        var dbReadDuration = TimeSpan.Zero;
        var dbWriteDuration = TimeSpan.Zero;
        var outerStopwatch = Stopwatch.StartNew();

        // TODO replace usage of HEX-encoded strings in favor of raw RadixAddress?
        var ledgerTransactions = new List<LedgerTransaction>(transactionsData.Count);
        var referencedEntities = new ReferencedEntityDictionary();
        var uppedSubstates = new List<UppedSubstate>();
        var childToParentEntities = new Dictionary<string, string>();
        var fungibleResourceChanges = new List<FungibleResourceChange>();
        var nonFungibleResourceChanges = new List<NonFungibleResourceChange>();
        var metadataChanges = new List<MetadataChange>();
        var fungibleResourceSupplyChanges = new List<FungibleResourceSupply>();
        var dbConn = (NpgsqlConnection)dbContext.Database.GetDbConnection();

        SequencesHolder sequences;

        // step: load current sequences
        {
            var sw = Stopwatch.StartNew();

            sequences = await dbConn.QueryFirstAsync<SequencesHolder>(
                @"
SELECT
    nextval('entities_id_seq') AS EntitySequence,
    nextval('entity_metadata_history_id_seq') AS EntityMetadataHistorySequence,
    nextval('entity_resource_aggregate_history_id_seq') AS EntityResourceAggregateHistorySequence,
    nextval('entity_resource_history_id_seq') AS EntityResourceHistorySequence,
    nextval('fungible_resource_supply_history_id_seq') AS FungibleResourceSupplyHistorySequence");

            dbReadDuration += sw.Elapsed;
        }

        // step: scan for any referenced entities
        {
            foreach (var transactionData in transactionsData)
            {
                ledgerTransactions.Add(TransactionMapping.CreateLedgerTransaction(transactionData));

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

                    referencedEntities.GetOrAdd(sid.EntityAddressHex, _ => new ReferencedEntity(sid.EntityAddressHex, sid.EntityType, stateVersion));
                }

                foreach (var downVirtualSubstate in stateUpdates.DownVirtualSubstates)
                {
                    // TODO not sure how to handle those; not sure what they even are

                    referencedEntities.GetOrAdd(downVirtualSubstate.EntityAddressHex, _ => new ReferencedEntity(downVirtualSubstate.EntityAddressHex, downVirtualSubstate.EntityType, stateVersion));
                }

                foreach (var newGlobalEntity in stateUpdates.NewGlobalEntities)
                {
                    referencedEntities[newGlobalEntity.EntityAddressHex].Globalize(newGlobalEntity.GlobalAddressHex, newGlobalEntity.GlobalAddress);
                }
            }
        }

        // step: resolve known types & optionally create missing entities
        {
            ComponentEntity CreateComponentEntity(ReferencedEntity re, HrpDefinition hrp)
            {
                if (re.GlobalHrpAddress?.StartsWith(hrp.AccountComponent) == true)
                {
                    return new AccountComponentEntity();
                }

                if (re.GlobalHrpAddress?.StartsWith(hrp.SystemComponent) == true)
                {
                    return new SystemComponentEntity();
                }

                return new NormalComponentEntity();
            }

            var entityAddresses = referencedEntities.Keys.Select(x => x.ConvertFromHex()).ToList();
            var entityAddressesParameter = new NpgsqlParameter("@entity_ids", NpgsqlDbType.Array | NpgsqlDbType.Bytea) { Value = entityAddresses };

            var sw = Stopwatch.StartNew();
            var knownDbEntities = await dbContext.Entities
                .FromSqlInterpolated($@"
SELECT *
FROM entities
WHERE id IN(
    SELECT DISTINCT UNNEST(ARRAY[id, parent_id, owner_ancestor_id, global_ancestor_id]) AS id
    FROM entities
    WHERE address = ANY({entityAddressesParameter})
)")
                .AsNoTracking()
                .ToDictionaryAsync(e => ((byte[])e.Address).ToHex(), token);

            dbReadDuration += sw.Elapsed;

            var dbEntities = new List<Entity>();

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
                    EntityType.Component => CreateComponentEntity(e, _networkConfigurationProvider.GetHrpDefinition()),
                    EntityType.Package => new PackageEntity(),
                    EntityType.Vault => new VaultEntity(),
                    EntityType.KeyValueStore => new ValueStoreEntity(),
                    _ => throw new ArgumentOutOfRangeException(nameof(e.Type), e.Type, null),
                };

                dbEntity.Id = sequences.NextEntity;
                dbEntity.FromStateVersion = e.StateVersion;
                dbEntity.Address = e.Address.ConvertFromHex();
                dbEntity.GlobalAddress = e.GlobalAddress;

                e.Resolve(dbEntity);
                dbEntities.Add(dbEntity);
            }

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

                referencedEntities[childAddress].ResolveParentalIds(referencedEntities[parentAddress].DatabaseId, ownerAncestor.DatabaseId, globalAncestor.DatabaseId);
            }

            sw = Stopwatch.StartNew();

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY entities (id, from_state_version, address, global_address, parent_id, owner_ancestor_id, global_ancestor_id, type) FROM STDIN (FORMAT BINARY)", token))
            {
                // TODO ouh, we must somehow reuse information already held by EF
                var typeMapping = new Dictionary<Type, string>
                {
                    [typeof(SystemEntity)] = "system",
                    [typeof(ResourceManagerEntity)] = "resource_manager",
                    [typeof(NormalComponentEntity)] = "normal_component",
                    [typeof(AccountComponentEntity)] = "account_component",
                    [typeof(SystemComponentEntity)] = "system_component",
                    [typeof(PackageEntity)] = "package",
                    [typeof(ValueStoreEntity)] = "value_store",
                    [typeof(VaultEntity)] = "vault",
                };

                foreach (var dbEntity in dbEntities)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(dbEntity.Id, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(dbEntity.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(dbEntity.Address.AsByteArray(), NpgsqlDbType.Bytea, token);
                    await writer.WriteNullableAsync(dbEntity.GlobalAddress.AsByteArray(), NpgsqlDbType.Bytea, token);
                    await writer.WriteNullableAsync(dbEntity.ParentId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullableAsync(dbEntity.OwnerAncestorId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullableAsync(dbEntity.GlobalAncestorId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(typeMapping[dbEntity.GetType()], NpgsqlDbType.Text, token);
                }

                await writer.CompleteAsync(token);
            }

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY ledger_transactions (state_version, status, payload_hash, intent_hash, signed_intent_hash, transaction_accumulator, is_user_transaction, message, fee_paid, tip_paid, epoch, index_in_epoch, round_in_epoch, is_start_of_epoch, is_start_of_round, referenced_entities, round_timestamp, created_timestamp, normalized_timestamp) FROM STDIN (FORMAT BINARY)", token))
            {
                var statusConverter = new LedgerTransactionStatusValueConverter().ConvertToProvider;

                foreach (var lt in ledgerTransactions)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(lt.StateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(statusConverter(lt.Status), NpgsqlDbType.Text, token);
                    await writer.WriteAsync(lt.PayloadHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(lt.IntentHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(lt.SignedIntentHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(lt.TransactionAccumulator, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(lt.IsUserTransaction, NpgsqlDbType.Boolean, token);
                    await writer.WriteNullableAsync(lt.Message, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(lt.FeePaid.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteAsync(lt.TipPaid.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteAsync(lt.Epoch, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(lt.IndexInEpoch, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(lt.RoundInEpoch, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(lt.IsStartOfEpoch, NpgsqlDbType.Boolean, token);
                    await writer.WriteAsync(lt.IsStartOfRound, NpgsqlDbType.Boolean, token);
                    await writer.WriteAsync(referencedEntities.OfStateVersion(lt.StateVersion).Select(re => re.DatabaseId).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(lt.RoundTimestamp.UtcDateTime, NpgsqlDbType.TimestampTz, token);
                    await writer.WriteAsync(lt.CreatedTimestamp.UtcDateTime, NpgsqlDbType.TimestampTz, token);
                    await writer.WriteAsync(lt.NormalizedRoundTimestamp.UtcDateTime, NpgsqlDbType.TimestampTz, token);
                }

                await writer.CompleteAsync(token);
            }

            rowsInserted += dbEntities.Count + ledgerTransactions.Count;
            dbWriteDuration += sw.Elapsed;
        }

        // step: scan all substates to figure out changes
        {
            void HandleResourceManagerSubstate(UppedSubstate us)
            {
                var data = us.Data.GetResourceManagerSubstate();
                var totalSupply = TokenAmount.FromSubUnitsString(data.TotalSupplyAttos);

                metadataChanges.Add(new MetadataChange(us.ReferencedEntity, data.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), us.StateVersion));
                fungibleResourceSupplyChanges.Add(new FungibleResourceSupply(us.ReferencedEntity, totalSupply, TokenAmount.Zero, TokenAmount.Zero, us.StateVersion)); // TODO support mint & burnt
            }

            void HandleComponentStateSubstate(UppedSubstate us)
            {
                var data = us.Data.GetComponentStateSubstate();
            }

            void HandleVaultSubstate(UppedSubstate us)
            {
                var data = us.Data.GetVaultSubstate();

                // TODO handle fungible vs non-fungible properly (waiting for CoreApi to decide how they're going to represent the data)

                // TODO ugh...
                var resourceAmount = data.ResourceAmount.ActualInstance;
                var substateEntity = us.ReferencedEntity;

                if (resourceAmount is FungibleResourceAmount fra)
                {
                    var amount = TokenAmount.FromSubUnitsString(fra.AmountAttos);

                    var resourceAddress = RadixBech32.Decode(fra.ResourceAddress).Data.ToHex();
                    var resourceEntity = referencedEntities[resourceAddress];

                    fungibleResourceChanges.Add(new FungibleResourceChange(substateEntity, resourceEntity, amount, us.StateVersion));

                    return;
                }

                if (resourceAmount is NonFungibleResourceAmount nfra)
                {
                    var resourceAddress = RadixBech32.Decode(nfra.ResourceAddress).Data.ToHex();
                    var resourceEntity = referencedEntities[resourceAddress];

                    nonFungibleResourceChanges.Add(new NonFungibleResourceChange(substateEntity, resourceEntity, nfra.NfIdsHex, us.StateVersion));

                    return;
                }

                throw new Exception("bla bla bla bla x9"); // TODO fix me
            }

            void HandleKeyValueStoreEntrySubstate(UppedSubstate us)
            {
                // TODO handle referenced_entities properly (not sure if we can ensure references types have been seen)
            }

            foreach (var us in uppedSubstates)
            {
                switch (us.Type)
                {
                    case SubstateType.System:
                        // TODO handle somehow
                        break;
                    case SubstateType.ResourceManager:
                        HandleResourceManagerSubstate(us);
                        break;
                    case SubstateType.ComponentInfo:
                        // TODO handle somehow
                        break;
                    case SubstateType.ComponentState:
                        HandleComponentStateSubstate(us);
                        break;
                    case SubstateType.Package:
                        // TODO handle somehow
                        break;
                    case SubstateType.Vault:
                        HandleVaultSubstate(us);
                        break;
                    case SubstateType.NonFungible:
                        // TODO handle somehow
                        break;
                    case SubstateType.KeyValueStoreEntry:
                        HandleKeyValueStoreEntrySubstate(us);
                        break;
                    default:
                        throw new Exception("bleh"); // TODO fix me
                }
            }
        }

        // step: now that all the fundamental data is inserted (entities & substates) we can insert some denormalized data
        {
            // entity_id => state_version => resource_id[] (added or removed)
            var aggregateDelta = new Dictionary<long, Dictionary<long, AggregateChange>>();

            var fungibles = fungibleResourceChanges
                .Select(e =>
                {
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

            var fungibleSupplies = fungibleResourceSupplyChanges
                .Select(e => new FungibleResourceSupplyHistory
                {
                    FromStateVersion = e.StateVersion,
                    ResourceEntityId = e.ResourceEntity.DatabaseId,
                    TotalSupply = e.TotalSupply,
                    TotalMinted = e.TotalMinted,
                    TotalBurnt = e.TotalBurnt,
                })
                .ToList();

            var sw = Stopwatch.StartNew();
            var aggregateDeltaIds = aggregateDelta.Keys.ToList();
            var existingAggregates = await dbContext.EntityResourceAggregateHistory
                .AsNoTracking()
                .Where(e => e.IsMostRecent)
                .Where(e => aggregateDeltaIds.Contains(e.EntityId))
                .ToDictionaryAsync(e => e.EntityId, token);

            dbReadDuration += sw.Elapsed;

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

            sw = Stopwatch.StartNew();

            if (existingAggregates.Any())
            {
                // update only those aggregates that will be modified in next step
                var ids = existingAggregates.Values.Select(e => e.Id).Intersect(lastAggregateByEntity.Keys).ToList();
                var idsParameter = new NpgsqlParameter("@ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = ids };

                if (ids.Any())
                {
                    var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE entity_resource_aggregate_history SET is_most_recent = false WHERE id = ANY({idsParameter})",
                        token);

                    if (ids.Count != affected)
                    {
                        throw new Exception("bla bla bla x6"); // TODO fix me
                    }

                    rowsUpdated += affected;
                }
            }

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY entity_resource_aggregate_history (id, from_state_version, entity_id, is_most_recent, fungible_resource_ids, non_fungible_resource_ids) FROM STDIN (FORMAT BINARY)", token))
            {
                foreach (var aggregate in aggregates)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(sequences.NextEntityResourceAggregateHistory, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.IsMostRecent, NpgsqlDbType.Boolean, token);
                    await writer.WriteAsync(aggregate.FungibleResourceIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(aggregate.NonFungibleResourceIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                }

                await writer.CompleteAsync(token);
            }

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY entity_resource_history (id, from_state_version, owner_entity_id, global_entity_id, resource_entity_id, type, balance, ids_count, ids) FROM STDIN (FORMAT BINARY)", token))
            {
                var type = "fungible";

                foreach (var fungible in fungibles)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(sequences.NextEntityResourceHistory, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.OwnerEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.GlobalEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(type, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(fungible.Balance.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                }

                type = "non_fungible";

                foreach (var nonFungible in nonFungibles)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(sequences.NextEntityResourceHistory, NpgsqlDbType.Bigint, token);
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

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY entity_metadata_history (id, from_state_version, entity_id, keys, values) FROM STDIN (FORMAT BINARY)", token))
            {
                foreach (var md in metadata)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(sequences.NextEntityMetadataHistory, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(md.Keys, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
                    await writer.WriteAsync(md.Values, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
                }

                await writer.CompleteAsync(token);
            }

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY fungible_resource_supply_history (id, from_state_version, resource_entity_id, total_supply, total_minted, total_burnt) FROM STDIN (FORMAT BINARY)", token))
            {
                foreach (var fs in fungibleSupplies)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(sequences.NextFungibleResourceSupplyHistory, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fs.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fs.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fs.TotalSupply.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteAsync(fs.TotalMinted.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteAsync(fs.TotalBurnt.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                }

                await writer.CompleteAsync(token);
            }

            rowsInserted += aggregates.Count + fungibles.Count + nonFungibles.Count + metadata.Count;
            dbWriteDuration += sw.Elapsed;
        }

        // step: update sequences
        {
            var sw = Stopwatch.StartNew();

            await dbConn.QueryFirstAsync(
                @"
SELECT
    setval('entities_id_seq', @EntitySequence),
    setval('entity_metadata_history_id_seq', @EntityMetadataHistorySequence),
    setval('entity_resource_aggregate_history_id_seq', @EntityResourceAggregateHistorySequence),
    setval('entity_resource_history_id_seq', @EntityResourceHistorySequence),
    setval('fungible_resource_supply_history_id_seq', @FungibleResourceSupplyHistorySequence)",
                new
                {
                    EntitySequence = sequences.EntitySequence,
                    EntityMetadataHistorySequence = sequences.EntityMetadataHistorySequence,
                    EntityResourceAggregateHistorySequence = sequences.EntityResourceAggregateHistorySequence,
                    EntityResourceHistorySequence = sequences.EntityResourceHistorySequence,
                    FungibleResourceSupplyHistorySequence = sequences.FungibleResourceSupplyHistorySequence,
                });

            dbWriteDuration += sw.Elapsed;
        }

        var contentHandlingDuration = outerStopwatch.Elapsed - dbReadDuration - dbWriteDuration;

        return new ProcessTransactionsReport(rowsInserted + rowsUpdated, dbReadDuration, dbWriteDuration, contentHandlingDuration);
    }

    private async Task CreateOrUpdateLedgerStatus(
        ReadWriteDbContext dbContext,
        TransactionSummary finalTransactionSummary,
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
        ledgerStatus.TopOfLedgerStateVersion = finalTransactionSummary.StateVersion;
        ledgerStatus.TargetStateVersion = latestSyncTarget;
    }
}
