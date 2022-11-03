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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
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
using CoreModel = RadixDlt.CoreApiSdk.Model;

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

    public async Task<TransactionSummary> GetLatestTransactionSummary(CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        return await GetTopOfLedger(dbContext, token);
    }

    public async Task<CommitTransactionsReport> CommitTransactions(ConsistentLedgerExtension ledgerExtension, SyncTargetCarrier latestSyncTarget, CancellationToken token = default)
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
            ledgerExtension.CommittedTransactions.Count,
            ledgerExtensionReport.FinalTransactionSummary,
            preparationReport.RawTxnPersistenceMs,
            preparationReport.MempoolTransactionUpdateMs,
            (long)processTransactionReport.ContentHandlingDuration.TotalMilliseconds,
            (long)processTransactionReport.DbReadDuration.TotalMilliseconds,
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

        var topOfLedgerSummary = await GetTopOfLedger(preparationDbContext, token);

        if (ledgerExtension.LatestTransactionSummary.StateVersion != topOfLedgerSummary.StateVersion)
        {
            throw new Exception(
                $"Tried to commit transactions with parent state version {ledgerExtension.LatestTransactionSummary.StateVersion} " +
                $"on top of a ledger with state version {topOfLedgerSummary.StateVersion}"
            );
        }

        if (topOfLedgerSummary.StateVersion == 0)
        {
            await EnsureDbLedgerIsInitialized(token);
        }

        var rawTransactions = ledgerExtension.CommittedTransactions
            .Where(ct => ct.LedgerTransaction.ActualInstance is CoreModel.UserLedgerTransaction)
            .Select(ct =>
            {
                var nt = ct.LedgerTransaction.GetUserLedgerTransaction().NotarizedTransaction;

                return new RawTransaction
                {
                    StateVersion = ct.StateVersion,
                    PayloadHash = nt.Hash.ConvertFromHex(),
                    Payload = nt.PayloadHex.ConvertFromHex(),
                };
            })
            .ToList();

        var (rawTransactionsTouched, rawTransactionCommitMs) = await CodeStopwatch.TimeInMs(
            () => _rawTransactionWriter.EnsureRawTransactionsCreatedOrUpdated(preparationDbContext, rawTransactions, token)
        );

        var (mempoolTransactionsTouched, mempoolTransactionUpdateMs) = await CodeStopwatch.TimeInMs(
            () => _rawTransactionWriter.EnsureMempoolTransactionsMarkedAsCommitted(preparationDbContext, ledgerExtension.CommittedTransactions, token)
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
            var (finalTransaction, report) = await ProcessTransactions(dbContext, ledgerExtension, token);

            await CreateOrUpdateLedgerStatus(dbContext, finalTransaction, latestSyncTarget, token);

            var (remainingRowsTouched, remainingDbDuration) = await CodeStopwatch.TimeInMs(
                () => dbContext.SaveChangesAsync(token)
            );

            await tx.CommitAsync(token);

            return new LedgerExtensionReport(
                report,
                finalTransaction,
                report.RowsTouched + remainingRowsTouched,
                ((long)report.DbWriteDuration.TotalMilliseconds) + remainingDbDuration);
        }
        catch (Exception)
        {
            await tx.RollbackAsync(token);

            throw;
        }
    }

    private record ProcessTransactionsResult(TransactionSummary FinalTransaction, ProcessTransactionsReport Report);

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

        // TODO replace with proper Activity at some point
        var rowsInserted = 0;
        var rowsUpdated = 0;
        var dbReadDuration = TimeSpan.Zero;
        var dbWriteDuration = TimeSpan.Zero;
        var outerStopwatch = Stopwatch.StartNew();

        // TODO replace usage of HEX-encoded strings in favor of raw RadixAddress?
        var ledgerTransactions = new List<LedgerTransaction>(ledgerExtension.CommittedTransactions.Count);
        var referencedEntities = new ReferencedEntityDictionary();
        var knownGlobalAddressesToLoad = new HashSet<string>();
        var childToParentEntities = new Dictionary<string, string>();

        var lastTransactionSummary = ledgerExtension.LatestTransactionSummary;
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
            foreach (var ct in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = ct.StateVersion;
                var stateUpdates = ct.Receipt.StateUpdates;

                long? newEpoch = null;
                long? newRoundInEpoch = null;
                DateTimeOffset? newRoundTimestamp = null;

                foreach (var upSubstate in stateUpdates.UpSubstates)
                {
                    var sid = upSubstate.SubstateId;
                    var sd = upSubstate.SubstateData.ActualInstance;

                    if (sd is CoreModel.GlobalSubstate globalData)
                    {
                        var target = globalData.TargetEntity;
                        var te = referencedEntities.GetOrAdd(target.TargetEntityIdHex, _ => new ReferencedEntity(target.TargetEntityIdHex, target.TargetEntityType, stateVersion));

                        te.Globalize(target.GlobalAddressHex, target.GlobalAddress);

                        // we do not want to store GlobalEntities as they bring no value from NG perspective
                        // GlobalAddress is essentially a property of other entities

                        continue;
                    }

                    var re = referencedEntities.GetOrAdd(sid.EntityIdHex, _ => new ReferencedEntity(sid.EntityIdHex, sid.EntityType, stateVersion));

                    if (sd is CoreModel.IOwner owner)
                    {
                        foreach (var oe in owner.OwnedEntities)
                        {
                            referencedEntities.GetOrAdd(oe.EntityIdHex, _ => new ReferencedEntity(oe.EntityIdHex, oe.EntityType, stateVersion)).IsChildOf(re);

                            childToParentEntities.Add(oe.EntityIdHex, sid.EntityIdHex);
                        }
                    }

                    if (sd is CoreModel.IGlobalResourcePointer globalResourcePointer)
                    {
                        foreach (var pointer in globalResourcePointer.Pointers)
                        {
                            // TODO ugh...
                            var globalAddress = RadixBech32.Decode(pointer.GlobalAddress).Data.ToHex();

                            knownGlobalAddressesToLoad.Add(globalAddress);
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
                    }

                    if (sd is CoreModel.SystemSubstate systemSubstate)
                    {
                        newEpoch = systemSubstate.Epoch;

                        // TODO this is just some dirty hack to ease-up integration process while CoreApi is still missing round support
                        newRoundInEpoch = 1;
                        newRoundTimestamp = _clock.UtcNow;
                    }

                    // TODO implement roundInEpoch the same way once CoreApi expose round/roundInEpoch
                    // if (operation.IsCreateOf<RoundData>(out var newRoundData))
                    // {
                    //     newRoundInEpoch = newRoundData.Round;
                    //
                    //     // NB - the first round of the ledger has Timestamp 0 for some reason. Let's ignore it and use the prev timestamp
                    //     if (newRoundData.Timestamp != 0)
                    //     {
                    //         newRoundTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(newRoundData.Timestamp);
                    //     }
                    // }
                }

                foreach (var downSubstate in stateUpdates.DownSubstates)
                {
                    var sid = downSubstate.SubstateId;

                    referencedEntities.GetOrAdd(sid.EntityIdHex, _ => new ReferencedEntity(sid.EntityIdHex, sid.EntityType, stateVersion));
                }

                foreach (var downVirtualSubstate in stateUpdates.DownVirtualSubstates)
                {
                    // TODO not sure how to handle those; not sure what they even are

                    referencedEntities.GetOrAdd(downVirtualSubstate.EntityIdHex, _ => new ReferencedEntity(downVirtualSubstate.EntityIdHex, downVirtualSubstate.EntityType, stateVersion));
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
                    IsStartOfRound: isStartOfRound);

                ledgerTransactions.Add(TransactionMapping.CreateLedgerTransaction(ct, summary)); // TODO inline this for now on

                lastTransactionSummary = summary;
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

            var entityAddresses = referencedEntities.Addresses.Select(x => x.ConvertFromHex()).ToList();
            var globalEntityAddresses = knownGlobalAddressesToLoad.Select(x => x.ConvertFromHex()).ToList();
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
                    SystemEntity => CoreModel.EntityType.System,
                    ResourceManagerEntity => CoreModel.EntityType.ResourceManager,
                    ComponentEntity => CoreModel.EntityType.Component,
                    PackageEntity => CoreModel.EntityType.Package,
                    KeyValueStoreEntity => CoreModel.EntityType.KeyValueStore,
                    VaultEntity => CoreModel.EntityType.Vault,
                    NonFungibleStoreEntity => CoreModel.EntityType.NonFungibleStore,
                    _ => throw new ArgumentOutOfRangeException(nameof(knownDbEntity), knownDbEntity.GetType().Name),
                };

                referencedEntities.GetOrAdd(knownDbEntity.Address.ToHex(), address => new ReferencedEntity(address, entityType, knownDbEntity.FromStateVersion));
            }

            var dbEntities = new List<Entity>();

            foreach (var e in referencedEntities.All)
            {
                if (knownDbEntities.ContainsKey(e.Address))
                {
                    e.Resolve(knownDbEntities[e.Address]);

                    continue;
                }

                Entity dbEntity = e.Type switch
                {
                    CoreModel.EntityType.System => new SystemEntity(),
                    CoreModel.EntityType.ResourceManager => e.CreateUsingTypeHint<ResourceManagerEntity>(),
                    CoreModel.EntityType.Component => CreateComponentEntity(e, _networkConfigurationProvider.GetHrpDefinition()),
                    CoreModel.EntityType.Package => new PackageEntity(),
                    CoreModel.EntityType.Vault => new VaultEntity(),
                    CoreModel.EntityType.KeyValueStore => new KeyValueStoreEntity(),
                    CoreModel.EntityType.Global => throw new ArgumentOutOfRangeException(nameof(e.Type), e.Type, "Global entities should be filtered out"),
                    CoreModel.EntityType.NonFungibleStore => new NonFungibleStoreEntity(),
                    _ => throw new ArgumentOutOfRangeException(nameof(e.Type), e.Type, null),
                };

                dbEntity.Id = sequences.NextEntity;
                dbEntity.FromStateVersion = e.StateVersion;
                dbEntity.Address = e.Address.ConvertFromHex();
                dbEntity.GlobalAddress = e.GlobalAddress == null ? null : (RadixAddress)e.GlobalAddress.ConvertFromHex();

                e.Resolve(dbEntity);
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

                    if (!globalId.HasValue && !currentParent.HasParent)
                    {
                        globalId = currentParent.DatabaseId;
                    }

                    currentParent = currentParent.HasParent ? currentParent.Parent : null;
                }

                if (parentId == null || ownerId == null || globalId == null)
                {
                    throw new Exception("bla bla bla x22");
                }

                referencedEntities.Get(childAddress).ResolveParentalIds(allAncestors.ToArray(), parentId.Value, ownerId.Value, globalId.Value);
            }

            sw = Stopwatch.StartNew();

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY entities (id, from_state_version, address, global_address, ancestor_ids, parent_ancestor_id, owner_ancestor_id, global_ancestor_id, discriminator) FROM STDIN (FORMAT BINARY)", token))
            {
                foreach (var dbEntity in dbEntities)
                {
                    if (dbContext.Model.FindEntityType(dbEntity.GetType())?.GetDiscriminatorValue() is not string discriminator)
                    {
                        throw new Exception("Unable to determine discriminator of entity " + dbEntity.Address.ToHex());
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
                }

                await writer.CompleteAsync(token);
            }

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY ledger_transactions (state_version, status, transaction_accumulator, message, epoch, index_in_epoch, round_in_epoch, is_start_of_epoch, is_start_of_round, referenced_entities, fee_paid, tip_paid, round_timestamp, created_timestamp, normalized_round_timestamp, discriminator, payload_hash, intent_hash, signed_intent_hash) FROM STDIN (FORMAT BINARY)", token))
            {
                var statusConverter = new LedgerTransactionStatusValueConverter().ConvertToProvider;

                if (dbContext.Model.FindEntityType(typeof(UserLedgerTransaction))?.GetDiscriminatorValue() is not string userDiscriminator)
                {
                    throw new Exception("Unable to determine discriminator of UserLedgerTransaction");
                }

                if (dbContext.Model.FindEntityType(typeof(ValidatorLedgerTransaction))?.GetDiscriminatorValue() is not string validatorDiscriminator)
                {
                    throw new Exception("Unable to determine discriminator of ValidatorLedgerTransaction");
                }

                foreach (var lt in ledgerTransactions)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(lt.StateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(statusConverter(lt.Status), NpgsqlDbType.Text, token);
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
                    await writer.WriteAsync(lt.RoundTimestamp.UtcDateTime, NpgsqlDbType.TimestampTz, token);
                    await writer.WriteAsync(lt.CreatedTimestamp.UtcDateTime, NpgsqlDbType.TimestampTz, token);
                    await writer.WriteAsync(lt.NormalizedRoundTimestamp.UtcDateTime, NpgsqlDbType.TimestampTz, token);

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

            rowsInserted += dbEntities.Count + ledgerTransactions.Count;
            dbWriteDuration += sw.Elapsed;
        }

        var fungibleResourceChanges = new List<FungibleResourceChange>();
        var nonFungibleResourceChanges = new List<NonFungibleResourceChange>();
        var metadataChanges = new List<MetadataChange>();
        var fungibleResourceSupplyChanges = new List<FungibleResourceSupply>();

        // step: scan all substates to figure out changes
        {
            foreach (var ct in ledgerExtension.CommittedTransactions)
            {
                var stateVersion = ct.StateVersion;
                var stateUpdates = ct.Receipt.StateUpdates;

                foreach (var upSubstate in stateUpdates.UpSubstates)
                {
                    var sid = upSubstate.SubstateId;
                    var sd = upSubstate.SubstateData.ActualInstance;

                    if (sd is CoreModel.GlobalSubstate)
                    {
                        continue;
                    }

                    var re = referencedEntities.Get(sid.EntityIdHex);

                    if (sd is CoreModel.ResourceManagerSubstate resourceManager)
                    {
                        metadataChanges.Add(new MetadataChange(re, resourceManager.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), stateVersion));

                        var totalSupply = TokenAmount.FromSubUnitsString(resourceManager.TotalSupplyAttos);

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
                                var amount = TokenAmount.FromSubUnitsString(fra.AmountAttos);
                                var resourceAddress = RadixBech32.Decode(fra.ResourceAddress).Data.ToHex();
                                var resourceEntity = referencedEntities.GetByGlobal(resourceAddress);

                                fungibleResourceChanges.Add(new FungibleResourceChange(re, resourceEntity, amount, stateVersion));

                                break;
                            }

                            case CoreModel.NonFungibleResourceAmount nfra:
                            {
                                var resourceAddress = RadixBech32.Decode(nfra.ResourceAddress).Data.ToHex();
                                var resourceEntity = referencedEntities.GetByGlobal(resourceAddress);

                                nonFungibleResourceChanges.Add(new NonFungibleResourceChange(re, resourceEntity, nfra.NfIdsHex, stateVersion));

                                break;
                            }

                            default:
                                throw new ArgumentOutOfRangeException(nameof(resourceAmount), resourceAmount.GetType().Name);
                        }
                    }
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
                        Ids = e.Ids.Select(id => id.ConvertFromHex()).ToArray(),
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

                    current.Apply(previous);

                    if (current.ShouldBePersisted(previous))
                    {
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

            var entityIds = aggregates.Select(a => a.EntityId).Distinct().ToList();
            var entityIdsParameter = new NpgsqlParameter("@entity_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = entityIds };

            var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE entity_resource_aggregate_history SET is_most_recent = false WHERE is_most_recent = true AND entity_id = ANY({entityIdsParameter})",
                token);

            rowsUpdated += affected;

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

            await using (var writer = await dbConn.BeginBinaryImportAsync("COPY entity_resource_history (id, from_state_version, owner_entity_id, global_entity_id, resource_entity_id, discriminator, balance, ids_count, ids) FROM STDIN (FORMAT BINARY)", token))
            {
                if (dbContext.Model.FindEntityType(typeof(EntityFungibleResourceHistory))?.GetDiscriminatorValue() is not string fungibleDiscriminator)
                {
                    throw new Exception("Unable to determine discriminator of EntityFungibleResourceHistory");
                }

                if (dbContext.Model.FindEntityType(typeof(EntityNonFungibleResourceHistory))?.GetDiscriminatorValue() is not string nonFungibleDiscriminator)
                {
                    throw new Exception("Unable to determine discriminator of EntityNonFungibleResourceHistory");
                }

                foreach (var fungible in fungibles)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(sequences.NextEntityResourceHistory, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.OwnerEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.GlobalEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungible.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(fungibleDiscriminator, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(fungible.Balance.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                }

                foreach (var nonFungible in nonFungibles)
                {
                    await writer.StartRowAsync(token);
                    await writer.WriteAsync(sequences.NextEntityResourceHistory, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.FromStateVersion, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.OwnerEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.GlobalEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungibleDiscriminator, NpgsqlDbType.Text, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(nonFungible.IdsCount, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(nonFungible.Ids, NpgsqlDbType.Array | NpgsqlDbType.Bytea, token);
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

        return new ProcessTransactionsResult(lastTransactionSummary, new ProcessTransactionsReport(rowsInserted + rowsUpdated, dbReadDuration, dbWriteDuration, contentHandlingDuration));
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
            IsStartOfRound: lastTransaction.IsStartOfRound
        );

        return lastOverview ?? PreGenesisTransactionSummary();
    }

    private TransactionSummary PreGenesisTransactionSummary()
    {
        // Nearly all of theses turn out to be unused!
        return new TransactionSummary(
            StateVersion: 0,
            RoundTimestamp: DateTimeOffset.FromUnixTimeSeconds(0),
            NormalizedRoundTimestamp: DateTimeOffset.FromUnixTimeSeconds(0),
            CreatedTimestamp: _clock.UtcNow,
            Epoch: 0,
            IndexInEpoch: 0,
            RoundInEpoch: 0,
            IsStartOfEpoch: false,
            IsStartOfRound: false
        );
    }
}
