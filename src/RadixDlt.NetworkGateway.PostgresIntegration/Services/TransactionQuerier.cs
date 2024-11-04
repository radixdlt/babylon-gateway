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
using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Interceptors;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using RadixDlt.NetworkGateway.PostgresIntegration.Services.PendingTransactions;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal record Event(string Name, string Emitter, GatewayModel.ProgrammaticScryptoSborValue Data);

internal class TransactionQuerier : ITransactionQuerier
{
    private readonly record struct TransactionReceiptEventLookup(long EntityId, ValueBytes SchemaHash);

    private readonly record struct SchemaLookup(long EntityId, ValueBytes SchemaHash);

    private readonly ReadOnlyDbContext _dbContext;
    private readonly ReadWriteDbContext _rwDbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IDapperWrapper _dapperWrapper;
    private readonly IEntityQuerier _entityQuerier;

    public TransactionQuerier(
        ReadOnlyDbContext dbContext,
        ReadWriteDbContext rwDbContext,
        INetworkConfigurationProvider networkConfigurationProvider,
        IDapperWrapper dapperWrapper,
        IEntityQuerier entityQuerier)
    {
        _dbContext = dbContext;
        _rwDbContext = rwDbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
        _dapperWrapper = dapperWrapper;
        _entityQuerier = entityQuerier;
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
            .Concat(
                request.SearchCriteria.Events.SelectMany(
                    e =>
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
            .Select(a => a)
            .ToList();

        var entityAddressToId = await _entityQuerier.ResolveEntityIds(referencedAddresses, atLedgerState, token);

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
        else if (request.SearchCriteria.Kind == LedgerTransactionKindFilter.UserOnly)
        {
            searchQuery = searchQuery
                .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                .OfType<TransactionTypeLedgerTransactionMarker>()
                .Where(oltm => oltm.TransactionType == LedgerTransactionMarkerTransactionType.User)
                .Where(oltm => oltm.StateVersion <= upperStateVersion && oltm.StateVersion >= (lowerStateVersion ?? oltm.StateVersion))
                .Select(oltm => oltm.StateVersion);
        }
        else if (request.SearchCriteria.Kind == LedgerTransactionKindFilter.EpochChangeOnly)
        {
            searchQuery = searchQuery
                .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                .OfType<EpochChangeLedgerTransactionMarker>()
                .Where(ecltm => ecltm.EpochChange)
                .Where(ecltm => ecltm.StateVersion <= upperStateVersion && ecltm.StateVersion >= (lowerStateVersion ?? ecltm.StateVersion))
                .Select(ecltm => ecltm.StateVersion);
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
            .OfType<BaseUserLedgerTransaction>()
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
            .OfType<BaseUserLedgerTransaction>()
            .Where(ult => ult.StateVersion <= ledgerState.StateVersion && ult.IntentHash == intentHash)
            .Select(
                ult => new CommittedTransactionSummary(
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

    private async Task<List<GatewayModel.CommittedTransactionInfo>> GetTransactions(
        List<long> transactionStateVersions,
        GatewayModel.TransactionDetailsOptIns optIns,
        CancellationToken token)
    {
        var transactions = await TransactionDetailsQuery.Execute(_dapperWrapper, _dbContext, transactionStateVersions, optIns, token);
        var entityIdToAddressMap = await _entityQuerier.ResolveEntityAddresses(transactions.SelectMany(x => x.AffectedGlobalEntities).ToHashSet().ToList(), token);

        var schemaLookups = transactions
            .SelectMany(x => x.Events)
            .Select(x => new TransactionReceiptEventLookup(x.EntityId, x.SchemaHash))
            .ToHashSet();

        Dictionary<SchemaLookup, byte[]> schemas = new Dictionary<SchemaLookup, byte[]>();

        if (optIns.ReceiptEvents && schemaLookups.Any())
        {
            var entityIds = schemaLookups.Select(x => x.EntityId).ToList();
            var schemaHashes = schemaLookups.Select(x => (byte[])x.SchemaHash).ToList();

            schemas = await _dbContext
                .SchemaEntryDefinition
                .FromSqlInterpolated(
                    $@"
SELECT *
FROM schema_entry_definition
WHERE (entity_id, schema_hash) IN (SELECT UNNEST({entityIds}), UNNEST({schemaHashes}))")
                .AnnotateMetricName("GetEventSchemas")
                .ToDictionaryAsync(x => new SchemaLookup(x.EntityId, x.SchemaHash), x => x.Schema, token);
        }

        var networkId = (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id;
        var mappedTransactions = MapTransactions(transactions, transactionStateVersions, optIns, entityIdToAddressMap, schemas, networkId);
        return mappedTransactions;
    }

    private async Task<ICollection<PendingTransactionSummary>> LookupPendingTransactionsByIntentHash(string intentHash, CancellationToken token = default)
    {
        return await _rwDbContext
            .PendingTransactions
            .Where(pt => pt.IntentHash == intentHash)
            .Select(
                pt =>
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

    private List<GatewayModel.CommittedTransactionInfo> MapTransactions(
        IList<TransactionDetailsQuery.LedgerTransactionQueryResult> transactions,
        IList<long> transactionStateVersions,
        GatewayModel.TransactionDetailsOptIns optIns,
        IDictionary<long, EntityAddress> entityIdToAddressMap,
        IDictionary<SchemaLookup, byte[]> schemas,
        byte networkId)
    {
        var mappedTransactions = new List<GatewayModel.CommittedTransactionInfo>();

        foreach (var transaction in transactions.OrderBy(lt => transactionStateVersions.IndexOf(lt.StateVersion)).ToList())
        {
            GatewayModel.TransactionBalanceChanges? balanceChanges = null;

            if (optIns.BalanceChanges && transaction.BalanceChanges != null)
            {
                var storedBalanceChanges = JsonConvert.DeserializeObject<CoreModel.CommittedTransactionBalanceChanges>(transaction.BalanceChanges);

                if (storedBalanceChanges != null)
                {
                    balanceChanges = storedBalanceChanges.ToGatewayModel();
                }
                else
                {
                    throw new InvalidOperationException("Unable to deserialize stored balance changes into CoreModel.CommittedTransactionBalanceChanges");
                }
            }

            if (!optIns.ReceiptEvents)
            {
                mappedTransactions.Add(MapSingleTransaction(transaction, optIns, entityIdToAddressMap, null, balanceChanges));
            }
            else
            {
                List<Event> events = new List<Event>();

                foreach (var @event in transaction.Events)
                {
                    if (!schemas.TryGetValue(new SchemaLookup(@event.EntityId, @event.SchemaHash), out var schema))
                    {
                        throw new UnreachableException($"Unable to find schema for given hash {@event.SchemaHash.ToHex()}");
                    }

                    var eventData = ScryptoSborUtils.DataToProgrammaticJson(@event.Data, schema, @event.KeyTypeKind, @event.TypeIndex, networkId);
                    events.Add(new Event(@event.Name, @event.Emitter, eventData));
                }

                mappedTransactions.Add(MapSingleTransaction(transaction, optIns, entityIdToAddressMap, events, balanceChanges));
            }
        }

        return mappedTransactions;
    }

    private GatewayModel.CommittedTransactionInfo MapSingleTransaction(
        TransactionDetailsQuery.LedgerTransactionQueryResult lt,
        GatewayModel.TransactionDetailsOptIns optIns,
        IDictionary<long, EntityAddress> entityIdToAddressMap,
        IList<Event>? events,
        GatewayModel.TransactionBalanceChanges? transactionBalanceChanges)
    {
        string? payloadHash = null;
        string? intentHash = null;
        string? rawHex = null;
        JRaw? message = null;
        string? manifestInstructions = null;
        List<GatewayModel.ManifestClass>? manifestClasses = null;

        if (lt.Discriminator is LedgerTransactionType.User or LedgerTransactionType.UserV2)
        {
            payloadHash = lt.PayloadHash;
            intentHash = lt.IntentHash;
            rawHex = optIns.RawHex ? lt.RawPayload!.ToHex() : null;
            message = lt.Message != null ? new JRaw(lt.Message) : null;
            manifestInstructions = optIns.ManifestInstructions ? lt.ManifestInstructions : null;
            manifestClasses = lt.ManifestClasses.Select(mc => mc.ToGatewayModel()).ToList();
        }

        var status = lt.ReceiptStatus switch
        {
            LedgerTransactionStatus.Succeeded => GatewayModel.TransactionStatus.CommittedSuccess,
            LedgerTransactionStatus.Failed => GatewayModel.TransactionStatus.CommittedFailure,
            _ => throw new UnreachableException($"Didn't expect {lt.ReceiptStatus} value"),
        };

        return new GatewayModel.CommittedTransactionInfo(
            stateVersion: lt.StateVersion,
            epoch: lt.Epoch,
            round: lt.RoundInEpoch,
            roundTimestamp: lt.RoundTimestamp.AsUtcIsoDateWithMillisString(),
            transactionStatus: status,
            affectedGlobalEntities: optIns.AffectedGlobalEntities ? lt.AffectedGlobalEntities.Select(x => entityIdToAddressMap[x].ToString()).ToList() : null,
            payloadHash: payloadHash,
            intentHash: intentHash,
            feePaid: lt.FeePaid.ToString(),
            confirmedAt: lt.RoundTimestamp,
            errorMessage: lt.ReceiptErrorMessage,
            rawHex: rawHex,
            receipt: new GatewayModel.TransactionReceipt
            {
                ErrorMessage = lt.ReceiptErrorMessage,
                Status = status,
                Output = optIns.ReceiptOutput && lt.ReceiptOutput != null ? new JRaw(lt.ReceiptOutput) : null,
                FeeSummary = optIns.ReceiptFeeSummary ? new JRaw(lt.ReceiptFeeSummary) : null,
                FeeDestination = optIns.ReceiptFeeDestination && lt.ReceiptFeeDestination != null ? new JRaw(lt.ReceiptFeeDestination) : null,
                FeeSource = optIns.ReceiptFeeSource && lt.ReceiptFeeSource != null ? new JRaw(lt.ReceiptFeeSource) : null,
                CostingParameters = optIns.ReceiptCostingParameters ? new JRaw(lt.ReceiptCostingParameters) : null,
                NextEpoch = lt.ReceiptNextEpoch != null ? new JRaw(lt.ReceiptNextEpoch) : null,
                StateUpdates = optIns.ReceiptStateChanges && lt.ReceiptStateUpdates != null ? new JRaw(lt.ReceiptStateUpdates) : null,
                Events = optIns.ReceiptEvents ? events?.Select(x => new GatewayModel.EventsItem(x.Name, new JRaw(x.Emitter), x.Data)).ToList() : null,
            },
            message: message,
            balanceChanges: optIns.BalanceChanges ? transactionBalanceChanges : null,
            manifestInstructions: manifestInstructions,
            manifestClasses: manifestClasses
        );
    }
}
