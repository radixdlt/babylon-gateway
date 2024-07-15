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
using Newtonsoft.Json;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Interceptors;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class TransactionQuerier : ITransactionQuerier
{
    private record SchemaLookup(long EntityId, ValueBytes SchemaHash);

    internal record Event(string Name, string Emitter, GatewayModel.ProgrammaticScryptoSborValue Data);

    private readonly ReadOnlyDbContext _dbContext;
    private readonly ReadWriteDbContext _rwDbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public TransactionQuerier(
        ReadOnlyDbContext dbContext,
        ReadWriteDbContext rwDbContext,
        INetworkConfigurationProvider networkConfigurationProvider)
    {
        _dbContext = dbContext;
        _rwDbContext = rwDbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<TransactionPageWithoutTotal> GetTransactionStream(TransactionStreamPageRequest request, GatewayModel.LedgerState atLedgerState, CancellationToken token = default)
    {
        var referencedAddresses = request
            .SearchCriteria
            .ManifestAccountsDepositedInto
            .Concat(request.SearchCriteria.AccountsWithManifestOwnerMethodCalls)
            .Concat(request.SearchCriteria.AccountsWithoutManifestOwnerMethodCalls)
            .Concat(request.SearchCriteria.ManifestAccountsWithdrawnFrom)
            .Concat(request.SearchCriteria.ManifestResources)
            .Concat(request.SearchCriteria.BadgesPresented)
            .Concat(request.SearchCriteria.AffectedGlobalEntities)
            .Concat(request.SearchCriteria.EventGlobalEmitters)
            .Concat(request.SearchCriteria.Events.SelectMany(e =>
            {
                var addresses = new List<EntityAddress>();

                if (e.EmitterEntityAddress.HasValue)
                {
                    addresses.Add(e.EmitterEntityAddress.Value);
                }

                if (e.ResourceAddress.HasValue)
                {
                    addresses.Add(e.ResourceAddress.Value);
                }

                return addresses;
            }))
            .Select(a => (string)a)
            .ToList();

        var entityAddressToId = await GetEntityIds(referencedAddresses, token);

        var upperStateVersion = request.AscendingOrder
            ? atLedgerState.StateVersion
            : request.Cursor?.StateVersionBoundary ?? atLedgerState.StateVersion;

        var lowerStateVersion = request.AscendingOrder
            ? request.Cursor?.StateVersionBoundary ?? request.FromStateVersion
            : request.FromStateVersion;

        var searchQuery = _dbContext
            .LedgerTransactionMarkers
            .Where(lt => lt.StateVersion <= upperStateVersion && lt.StateVersion >= (lowerStateVersion ?? lt.StateVersion))
            .Select(lt => lt.StateVersion);

        var userKindFilterImplicitlyApplied = false;

        if (request.SearchCriteria.ManifestAccountsDepositedInto.Any())
        {
            userKindFilterImplicitlyApplied = true;

            foreach (var entityAddress in request.SearchCriteria.ManifestAccountsDepositedInto)
            {
                if (!entityAddressToId.TryGetValue(entityAddress, out var entityId))
                {
                    return TransactionPageWithoutTotal.Empty;
                }

                searchQuery = ApplyLedgerTransactionMarkerOperationTypeFilter(entityId, LedgerTransactionMarkerOperationType.AccountDepositedInto, searchQuery);
            }
        }

        if (request.SearchCriteria.ManifestAccountsWithdrawnFrom.Any())
        {
            userKindFilterImplicitlyApplied = true;

            foreach (var entityAddress in request.SearchCriteria.ManifestAccountsWithdrawnFrom)
            {
                if (!entityAddressToId.TryGetValue(entityAddress, out var entityId))
                {
                    return TransactionPageWithoutTotal.Empty;
                }

                searchQuery = ApplyLedgerTransactionMarkerOperationTypeFilter(entityId, LedgerTransactionMarkerOperationType.AccountWithdrawnFrom, searchQuery);
            }
        }

        if (request.SearchCriteria.BadgesPresented.Any())
        {
            userKindFilterImplicitlyApplied = true;

            foreach (var entityAddress in request.SearchCriteria.BadgesPresented)
            {
                if (!entityAddressToId.TryGetValue(entityAddress, out var entityId))
                {
                    return TransactionPageWithoutTotal.Empty;
                }

                searchQuery = ApplyLedgerTransactionMarkerOperationTypeFilter(entityId, LedgerTransactionMarkerOperationType.BadgePresented, searchQuery);
            }
        }

        if (request.SearchCriteria.ManifestResources.Any())
        {
            userKindFilterImplicitlyApplied = true;

            foreach (var entityAddress in request.SearchCriteria.ManifestResources)
            {
                if (!entityAddressToId.TryGetValue(entityAddress, out var entityId))
                {
                    return TransactionPageWithoutTotal.Empty;
                }

                searchQuery = ApplyLedgerTransactionMarkerOperationTypeFilter(entityId, LedgerTransactionMarkerOperationType.ResourceInUse, searchQuery);
            }
        }

        if (request.SearchCriteria.AffectedGlobalEntities.Any())
        {
            foreach (var entityAddress in request.SearchCriteria.AffectedGlobalEntities)
            {
                if (!entityAddressToId.TryGetValue(entityAddress, out var entityId))
                {
                    return TransactionPageWithoutTotal.Empty;
                }

                searchQuery = searchQuery
                    .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                    .OfType<AffectedGlobalEntityTransactionMarker>()
                    .Where(agetm => agetm.EntityId == entityId)
                    .Where(agetm => agetm.StateVersion <= upperStateVersion && agetm.StateVersion >= (lowerStateVersion ?? agetm.StateVersion))
                    .Select(agetm => agetm.StateVersion);
            }
        }

        if (request.SearchCriteria.EventGlobalEmitters.Any())
        {
            foreach (var entityAddress in request.SearchCriteria.EventGlobalEmitters)
            {
                if (!entityAddressToId.TryGetValue(entityAddress, out var entityId))
                {
                    return TransactionPageWithoutTotal.Empty;
                }

                searchQuery = searchQuery
                    .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                    .OfType<EventGlobalEmitterTransactionMarker>()
                    .Where(agetm => agetm.EntityId == entityId)
                    .Where(agetm => agetm.StateVersion <= upperStateVersion && agetm.StateVersion >= (lowerStateVersion ?? agetm.StateVersion))
                    .Select(agetm => agetm.StateVersion);
            }
        }

        if (request.SearchCriteria.Events.Any())
        {
            userKindFilterImplicitlyApplied = true;

            foreach (var @event in request.SearchCriteria.Events)
            {
                var eventType = @event.Event switch
                {
                    LedgerTransactionEventFilter.EventType.Withdrawal => LedgerTransactionMarkerEventType.Withdrawal,
                    LedgerTransactionEventFilter.EventType.Deposit => LedgerTransactionMarkerEventType.Deposit,
                    _ => throw new UnreachableException($"Didn't expect {@event.Event} value"),
                };

                long? eventEmitterEntityId = null;
                long? eventResourceEntityId = null;

                if (@event.EmitterEntityAddress.HasValue)
                {
                    if (!entityAddressToId.TryGetValue(@event.EmitterEntityAddress.Value, out var id))
                    {
                        return TransactionPageWithoutTotal.Empty;
                    }

                    eventEmitterEntityId = id;
                }

                if (@event.ResourceAddress.HasValue)
                {
                    if (!entityAddressToId.TryGetValue(@event.ResourceAddress.Value, out var id))
                    {
                        return TransactionPageWithoutTotal.Empty;
                    }

                    eventResourceEntityId = id;
                }

                searchQuery = searchQuery
                    .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                    .OfType<EventLedgerTransactionMarker>()
                    .Where(eltm => eltm.EventType == eventType && eltm.EntityId == (eventEmitterEntityId ?? eltm.EntityId) && eltm.ResourceEntityId == (eventResourceEntityId ?? eltm.ResourceEntityId))
                    .Where(eltm => eltm.StateVersion <= upperStateVersion && eltm.StateVersion >= (lowerStateVersion ?? eltm.StateVersion))
                    .Select(eltm => eltm.StateVersion);
            }
        }

        if (request.SearchCriteria.ManifestClassFilter != null)
        {
            userKindFilterImplicitlyApplied = true;

            var manifestClass = request.SearchCriteria.ManifestClassFilter.Class switch
            {
                LedgerTransactionManifestClass.General => LedgerTransactionManifestClass.General,
                LedgerTransactionManifestClass.Transfer => LedgerTransactionManifestClass.Transfer,
                LedgerTransactionManifestClass.ValidatorStake => LedgerTransactionManifestClass.ValidatorStake,
                LedgerTransactionManifestClass.ValidatorUnstake => LedgerTransactionManifestClass.ValidatorUnstake,
                LedgerTransactionManifestClass.ValidatorClaim => LedgerTransactionManifestClass.ValidatorClaim,
                LedgerTransactionManifestClass.AccountDepositSettingsUpdate => LedgerTransactionManifestClass.AccountDepositSettingsUpdate,
                LedgerTransactionManifestClass.PoolContribution => LedgerTransactionManifestClass.PoolContribution,
                LedgerTransactionManifestClass.PoolRedemption => LedgerTransactionManifestClass.PoolRedemption,
                _ => throw new UnreachableException($"Didn't expect {request.SearchCriteria.ManifestClassFilter.Class} value"),
            };

            searchQuery = searchQuery
                .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                .OfType<ManifestClassMarker>()
                .Where(ttm => ttm.ManifestClass == manifestClass)
                .Where(ttm => (request.SearchCriteria.ManifestClassFilter.MatchOnlyMostSpecificType && ttm.IsMostSpecific) || !request.SearchCriteria.ManifestClassFilter.MatchOnlyMostSpecificType)
                .Where(eltm => eltm.StateVersion <= upperStateVersion && eltm.StateVersion >= (lowerStateVersion ?? eltm.StateVersion))
                .Select(eltm => eltm.StateVersion);
        }

        if (request.SearchCriteria.AccountsWithManifestOwnerMethodCalls.Any())
        {
            userKindFilterImplicitlyApplied = true;

            foreach (var entityAddress in request.SearchCriteria.AccountsWithManifestOwnerMethodCalls)
            {
                if (!entityAddressToId.TryGetValue(entityAddress, out var entityId))
                {
                    return TransactionPageWithoutTotal.Empty;
                }

                searchQuery = ApplyLedgerTransactionMarkerOperationTypeFilter(entityId, LedgerTransactionMarkerOperationType.AccountOwnerMethodCall, searchQuery);
            }
        }

        if (request.SearchCriteria.AccountsWithoutManifestOwnerMethodCalls.Any())
        {
            foreach (var entityAddress in request.SearchCriteria.AccountsWithoutManifestOwnerMethodCalls)
            {
                if (!entityAddressToId.TryGetValue(entityAddress, out var entityId))
                {
                    return TransactionPageWithoutTotal.Empty;
                }

                var withManifestOwnerCall = _dbContext
                    .LedgerTransactionMarkers
                    .OfType<ManifestAddressLedgerTransactionMarker>()
                    .Where(maltm => maltm.OperationType == LedgerTransactionMarkerOperationType.AccountOwnerMethodCall && maltm.EntityId == entityId)
                    .Where(maltm => maltm.StateVersion <= upperStateVersion && maltm.StateVersion >= (lowerStateVersion ?? maltm.StateVersion))
                    .Select(y => y.StateVersion);

                searchQuery = searchQuery.Where(x => withManifestOwnerCall.All(y => y != x));
            }
        }

        if (request.SearchCriteria.Kind == LedgerTransactionKindFilter.UserOnly && userKindFilterImplicitlyApplied)
        {
            // already handled
        }
        else if (request.SearchCriteria.Kind == LedgerTransactionKindFilter.AllAnnotated)
        {
            // already handled as every TX found in LedgerTransactionMarker table is implicitly annotated
        }
        else
        {
            var originType = request.SearchCriteria.Kind switch
            {
                LedgerTransactionKindFilter.UserOnly => LedgerTransactionMarkerOriginType.User,
                LedgerTransactionKindFilter.EpochChangeOnly => LedgerTransactionMarkerOriginType.EpochChange,
                _ => throw new UnreachableException($"Unexpected value of kindFilter: {request.SearchCriteria.Kind}"),
            };

            searchQuery = searchQuery
                .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                .OfType<OriginLedgerTransactionMarker>()
                .Where(oltm => oltm.OriginType == originType)
                .Where(oltm => oltm.StateVersion <= upperStateVersion && oltm.StateVersion >= (lowerStateVersion ?? oltm.StateVersion))
                .Select(oltm => oltm.StateVersion);
        }

        if (request.AscendingOrder)
        {
            searchQuery = searchQuery.OrderBy(sv => sv);
        }
        else
        {
            searchQuery = searchQuery.OrderByDescending(sv => sv);
        }

        var stateVersions = await searchQuery
            .TagWith(ForceDistinctInterceptor.Apply)
            .Take(request.PageSize + 1)
            .AnnotateMetricName("GetTransactionsStateVersions")
            .ToListAsync(token);

        var transactions = await GetTransactions(stateVersions.Take(request.PageSize).ToList(), request.OptIns, token);

        var nextCursor = stateVersions.Count == request.PageSize + 1
            ? new GatewayModel.LedgerTransactionsCursor(stateVersions.Last())
            : null;

        return new TransactionPageWithoutTotal(nextCursor, transactions);

        IQueryable<long> ApplyLedgerTransactionMarkerOperationTypeFilter(long entityId, LedgerTransactionMarkerOperationType operationType, IQueryable<long> query)
        {
            return query
                .Join(
                    _dbContext.LedgerTransactionMarkers,
                    stateVersion => stateVersion,
                    ledgerTransactionMarker => ledgerTransactionMarker.StateVersion,
                    (stateVersion, ledgerTransactionMarker) => ledgerTransactionMarker)
                .OfType<ManifestAddressLedgerTransactionMarker>()
                .Where(maltm => maltm.OperationType == operationType && maltm.EntityId == entityId)
                .Where(maltm => maltm.StateVersion <= upperStateVersion && maltm.StateVersion >= (lowerStateVersion ?? maltm.StateVersion))
                .Select(maltm => maltm.StateVersion);
        }
    }

    public async Task<GatewayModel.CommittedTransactionInfo?> LookupCommittedTransaction(
        string intentHash,
        GatewayModel.TransactionDetailsOptIns optIns,
        GatewayModel.LedgerState ledgerState,
        bool withDetails,
        CancellationToken token = default)
    {
        var stateVersion = await _dbContext
            .LedgerTransactions
            .OfType<UserLedgerTransaction>()
            .Where(ult => ult.StateVersion <= ledgerState.StateVersion && ult.IntentHash == intentHash)
            .Select(ult => ult.StateVersion)
            .AnnotateMetricName()
            .FirstOrDefaultAsync(token);

        if (stateVersion == default)
        {
            return null;
        }

        var transactions = await GetTransactions(new List<long> { stateVersion }, optIns, token);

        return transactions.First();
    }

    internal record CommittedTransactionSummary(
        long StateVersion,
        string PayloadHash,
        LedgerTransactionStatus Status,
        string? ErrorMessage);

    public async Task<GatewayModel.TransactionStatusResponse> ResolveTransactionStatusResponse(GatewayModel.LedgerState ledgerState, string intentHash, CancellationToken token = default)
    {
        var maybeCommittedTransactionSummary = await _dbContext
            .LedgerTransactions
            .OfType<UserLedgerTransaction>()
            .Where(ult => ult.StateVersion <= ledgerState.StateVersion && ult.IntentHash == intentHash)
            .Select(ult => new CommittedTransactionSummary(
                ult.StateVersion,
                ult.PayloadHash,
                ult.EngineReceipt.Status,
                ult.EngineReceipt.ErrorMessage))
            .AnnotateMetricName()
            .FirstOrDefaultAsync(token);

        var aggregator = new PendingTransactionResponseAggregator(ledgerState, maybeCommittedTransactionSummary);

        var pendingTransactions = await LookupPendingTransactionsByIntentHash(intentHash, token);

        foreach (var pendingTransaction in pendingTransactions)
        {
            aggregator.AddPendingTransaction(pendingTransaction);
        }

        return aggregator.IntoResponse();
    }

    private async Task<ICollection<PendingTransactionSummary>> LookupPendingTransactionsByIntentHash(string intentHash, CancellationToken token = default)
    {
        return await _rwDbContext
            .PendingTransactions
            .Where(pt => pt.IntentHash == intentHash)
            .Select(pt =>
                new PendingTransactionSummary(
                    pt.PayloadHash,
                    pt.EndEpochExclusive,
                    pt.GatewayHandling.ResubmitFromTimestamp,
                    pt.GatewayHandling.HandlingStatusReason,
                    pt.LedgerDetails.PayloadLedgerStatus,
                    pt.LedgerDetails.IntentLedgerStatus,
                    pt.LedgerDetails.InitialRejectionReason,
                    pt.LedgerDetails.LatestRejectionReason,
                    pt.NetworkDetails.LastSubmitErrorTitle
                )
            )
            .AnnotateMetricName()
            .AsNoTracking()
            .ToListAsync(token);
    }

    private async Task<List<GatewayModel.CommittedTransactionInfo>> GetTransactions(
        List<long> transactionStateVersions,
        GatewayModel.TransactionDetailsOptIns optIns,
        CancellationToken token)
    {
        var transactions = await _dbContext
            .LedgerTransactions
            .FromSqlInterpolated(@$"
WITH configuration AS (
    SELECT
        {optIns.RawHex} AS with_raw_payload,
        true AS with_receipt_costing_parameters,
        true AS with_receipt_fee_summary,
        true AS with_receipt_next_epoch,
        {optIns.ReceiptOutput} AS with_receipt_output,
        {optIns.ReceiptStateChanges} AS with_receipt_state_updates,
        {optIns.ReceiptEvents} AS with_receipt_events,
        {optIns.BalanceChanges} AS with_balance_changes,
        {optIns.ManifestInstructions} AS with_manifest_instructions
)
SELECT
    state_version, epoch, round_in_epoch, index_in_epoch,
    index_in_round, fee_paid, tip_paid, affected_global_entities,
    round_timestamp, created_timestamp, normalized_round_timestamp,
    receipt_status, receipt_fee_source, receipt_fee_destination, receipt_error_message,
    transaction_tree_hash, receipt_tree_hash, state_tree_hash,
    discriminator, payload_hash, intent_hash, signed_intent_hash, message, manifest_classes,
    CASE WHEN configuration.with_raw_payload THEN raw_payload ELSE ''::bytea END AS raw_payload,
    CASE WHEN configuration.with_receipt_costing_parameters THEN receipt_costing_parameters ELSE '{{}}'::jsonb END AS receipt_costing_parameters,
    CASE WHEN configuration.with_receipt_fee_summary THEN receipt_fee_summary ELSE '{{}}'::jsonb END AS receipt_fee_summary,
    CASE WHEN configuration.with_receipt_next_epoch THEN receipt_next_epoch ELSE '{{}}'::jsonb END AS receipt_next_epoch,
    CASE WHEN configuration.with_receipt_output THEN receipt_output ELSE '{{}}'::jsonb END AS receipt_output,
    CASE WHEN configuration.with_receipt_state_updates THEN receipt_state_updates ELSE '{{}}'::jsonb END AS receipt_state_updates,
    CASE WHEN configuration.with_receipt_events THEN receipt_event_emitters ELSE '{{}}'::jsonb[] END AS receipt_event_emitters,
    CASE WHEN configuration.with_receipt_events THEN receipt_event_names ELSE '{{}}'::text[] END AS receipt_event_names,
    CASE WHEN configuration.with_receipt_events THEN receipt_event_sbors ELSE '{{}}'::bytea[] END AS receipt_event_sbors,
    CASE WHEN configuration.with_receipt_events THEN receipt_event_schema_entity_ids ELSE '{{}}'::bigint[] END AS receipt_event_schema_entity_ids,
    CASE WHEN configuration.with_receipt_events THEN receipt_event_schema_hashes ELSE '{{}}'::bytea[] END AS receipt_event_schema_hashes,
    CASE WHEN configuration.with_receipt_events THEN receipt_event_type_indexes ELSE '{{}}'::bigint[] END AS receipt_event_type_indexes,
    CASE WHEN configuration.with_receipt_events THEN receipt_event_sbor_type_kinds ELSE '{{}}'::sbor_type_kind[] END AS receipt_event_sbor_type_kinds,
    CASE WHEN configuration.with_balance_changes THEN balance_changes ELSE '{{}}'::jsonb END AS balance_changes,
    CASE WHEN configuration.with_manifest_instructions THEN manifest_instructions ELSE ''::text END AS manifest_instructions
FROM ledger_transactions, configuration
WHERE state_version = ANY({transactionStateVersions})")
            .AnnotateMetricName("GetTransactions")
            .ToListAsync(token);

        var entityIdToAddressMap = await GetEntityAddresses(transactions.SelectMany(x => x.AffectedGlobalEntities).ToHashSet().ToList(), token);

        var schemaLookups = transactions
            .SelectMany(x => x.EngineReceipt.Events.GetEventLookups())
            .ToHashSet();

        Dictionary<SchemaLookup, byte[]> schemas = new Dictionary<SchemaLookup, byte[]>();

        if (optIns.ReceiptEvents && schemaLookups.Any())
        {
            var entityIds = schemaLookups.Select(x => x.EntityId).ToList();
            var schemaHashes = schemaLookups.Select(x => (byte[])x.SchemaHash).ToList();

            schemas = await _dbContext
                .SchemaEntryDefinition
                .FromSqlInterpolated($@"
SELECT *
FROM schema_entry_definition
WHERE (entity_id, schema_hash) IN (SELECT UNNEST({entityIds}), UNNEST({schemaHashes}))")
                .AnnotateMetricName("GetEventSchemas")
                .ToDictionaryAsync(x => new SchemaLookup(x.EntityId, x.SchemaHash), x => x.Schema, token);
        }

        List<GatewayModel.CommittedTransactionInfo> mappedTransactions = new List<GatewayModel.CommittedTransactionInfo>();
        var networkId = (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id;

        foreach (var transaction in transactions.OrderBy(lt => transactionStateVersions.IndexOf(lt.StateVersion)).ToList())
        {
            GatewayModel.TransactionBalanceChanges? balanceChanges = null;

            if (optIns.BalanceChanges && transaction.BalanceChanges != null)
            {
                var storedBalanceChanges = JsonConvert.DeserializeObject<CoreModel.CommittedTransactionBalanceChanges>(transaction.BalanceChanges);

                if (storedBalanceChanges == null)
                {
                    throw new InvalidOperationException("Unable to deserialize stored balance changes into CoreModel.CommittedTransactionBalanceChanges");
                }

                balanceChanges = storedBalanceChanges.ToGatewayModel();
            }

            if (!optIns.ReceiptEvents || schemaLookups?.Any() == false)
            {
                mappedTransactions.Add(transaction.ToGatewayModel(optIns, entityIdToAddressMap, null, balanceChanges));
            }
            else
            {
                List<Event> events = new List<Event>();

                foreach (var @event in transaction.EngineReceipt.Events.GetEvents())
                {
                    if (!schemas.TryGetValue(new SchemaLookup(@event.EntityId, @event.SchemaHash), out var schema))
                    {
                        throw new UnreachableException($"Unable to find schema for given hash {@event.SchemaHash.ToHex()}");
                    }

                    var eventData = ScryptoSborUtils.DataToProgrammaticJson(@event.Data, schema, @event.KeyTypeKind, @event.TypeIndex, networkId);
                    events.Add(new Event(@event.Name, @event.Emitter, eventData));
                }

                mappedTransactions.Add(transaction.ToGatewayModel(optIns, entityIdToAddressMap, events, balanceChanges));
            }
        }

        return mappedTransactions;
    }

    private async Task<Dictionary<string, long>> GetEntityIds(List<string> addresses, CancellationToken token = default)
    {
        if (!addresses.Any())
        {
            return new Dictionary<string, long>();
        }

        return await _dbContext
            .Entities
            .Where(e => addresses.Contains(e.Address))
            .Select(e => new { e.Id, e.Address })
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Address.ToString(), e => e.Id, token);
    }

    private async Task<Dictionary<long, string>> GetEntityAddresses(List<long> entityIds, CancellationToken token = default)
    {
        if (!entityIds.Any())
        {
            return new Dictionary<long, string>();
        }

        return await _dbContext
            .Entities
            .Where(e => entityIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Address })
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Id, e => e.Address.ToString(), token);
    }
}
