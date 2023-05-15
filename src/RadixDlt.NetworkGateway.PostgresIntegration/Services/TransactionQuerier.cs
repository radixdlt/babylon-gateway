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
using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Interceptors;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class TransactionQuerier : ITransactionQuerier
{
    private readonly ReadOnlyDbContext _dbContext;
    private readonly ReadWriteDbContext _rwDbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public TransactionQuerier(ReadOnlyDbContext dbContext, ReadWriteDbContext rwDbContext, INetworkConfigurationProvider networkConfigurationProvider)
    {
        _dbContext = dbContext;
        _rwDbContext = rwDbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<TransactionPageWithoutTotal> GetTransactionStream(TransactionStreamPageRequest request, GatewayModel.LedgerState atLedgerState, CancellationToken token = default)
    {
        var referencedAddresses = request.SearchCriteria.ManifestAccountsDepositedInto
            .Concat(request.SearchCriteria.ManifestAccountsWithdrawnFrom)
            .Concat(request.SearchCriteria.ManifestResources)
            .ToList();

        var entityAddressToId = await GetEntityIds(referencedAddresses, token);

        var upperStateVersion = request.AscendingOrder
            ? atLedgerState.StateVersion
            : request.Cursor?.StateVersionBoundary ?? atLedgerState.StateVersion;

        var lowerStateVersion = request.AscendingOrder
            ? request.Cursor?.StateVersionBoundary ?? request.FromStateVersion
            : request.FromStateVersion;

        var searchQuery = _dbContext.LedgerTransactions
            .Where(lt => lt.StateVersion <= upperStateVersion && lt.StateVersion > (lowerStateVersion ?? lt.StateVersion))
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

                searchQuery = searchQuery
                    .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                    .OfType<ManifestAddressLedgerTransactionMarker>()
                    .Where(maltm => maltm.OperationType == AbcOperationType.AccountDepositedInto && maltm.EntityId == entityId)
                    .Select(maltm => maltm.StateVersion);
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

                searchQuery = searchQuery
                    .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                    .OfType<ManifestAddressLedgerTransactionMarker>()
                    .Where(maltm => maltm.OperationType == AbcOperationType.AccountWithdrawnFrom && maltm.EntityId == entityId)
                    .Select(maltm => maltm.StateVersion);
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

                searchQuery = searchQuery
                    .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                    .OfType<ManifestAddressLedgerTransactionMarker>()
                    .Where(maltm => maltm.OperationType == AbcOperationType.ResourceInUse && maltm.EntityId == entityId)
                    .Select(maltm => maltm.StateVersion);
            }
        }

        if (request.SearchCriteria.HasTransactionEventConstraints())
        {
            userKindFilterImplicitlyApplied = true;

            AbcEventType? eventType = null;
            long? eventEntityId = null;
            long? eventResourceId = null;

            if (request.SearchCriteria.WithdrawalEventsOnly)
            {
                eventType = AbcEventType.Withdrawal;
            }
            else if (request.SearchCriteria.DepositEventsOnly)
            {
                eventType = AbcEventType.Deposit;
            }

            if (request.SearchCriteria.EventEmitterEntityId.HasValue)
            {
                eventEntityId = request.SearchCriteria.EventEmitterEntityId.Value;
            }

            if (request.SearchCriteria.EventResourceEntityId.HasValue)
            {
                eventResourceId = request.SearchCriteria.EventResourceEntityId.Value;
            }

            searchQuery = searchQuery
                .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                .OfType<EventLedgerTransactionMarker>()
                .Where(eltm => eltm.EventType == (eventType ?? eltm.EventType) && eltm.EntityId == (eventEntityId ?? eltm.EntityId) && eltm.ResourceEntityId == (eventResourceId ?? eltm.ResourceEntityId))
                .Select(eltm => eltm.StateVersion);
        }

        if (request.SearchCriteria.KindFilter == LedgerTransactionKindFilter.UserOnly && userKindFilterImplicitlyApplied)
        {
            // already handled
        }
        else if (request.SearchCriteria.KindFilter == LedgerTransactionKindFilter.AllAnnotated)
        {
            searchQuery = searchQuery
                .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                .OfType<OriginLedgerTransactionMarker>()
                .Select(oltm => oltm.StateVersion);
        }
        else
        {
            var originType = request.SearchCriteria.KindFilter switch
            {
                LedgerTransactionKindFilter.UserOnly => AbcOriginType.User,
                LedgerTransactionKindFilter.EpochChangeOnly => AbcOriginType.EpochChange,
                _ => throw new UnreachableException($"Unexpected value of kindFilter: {request.SearchCriteria.KindFilter}"),
            };

            searchQuery = searchQuery
                .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                .OfType<OriginLedgerTransactionMarker>()
                .Where(oltm => oltm.OriginType == originType)
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
            .ToListAsync(token);

        var transactions = await GetTransactions(stateVersions.Take(request.PageSize).ToList(), token);

        var nextCursor = stateVersions.Count == request.PageSize + 1
            ? new GatewayModel.LedgerTransactionsCursor(stateVersions.Last())
            : null;

        return new TransactionPageWithoutTotal(nextCursor, transactions);
    }

    public async Task<DetailsLookupResult?> LookupCommittedTransaction(byte[] intentHash, GatewayModel.TransactionCommittedDetailsOptIns optIns, GatewayModel.LedgerState ledgerState, bool withDetails, CancellationToken token = default)
    {
        var stateVersion = await _dbContext.LedgerTransactions
            .OfType<UserLedgerTransaction>()
            .Where(ult => ult.StateVersion <= ledgerState.StateVersion && ult.IntentHash == intentHash)
            .Select(ult => ult.StateVersion)
            .FirstOrDefaultAsync(token);

        if (stateVersion == default)
        {
            return null;
        }

        return withDetails
            ? await GetTransactionWithDetails(stateVersion, optIns, token)
            : new DetailsLookupResult((await GetTransactions(new List<long> { stateVersion }, token)).First(), null);
    }

    public async Task<ICollection<StatusLookupResult>> LookupPendingTransactionsByIntentHash(byte[] intentHash, CancellationToken token = default)
    {
        var pendingTransactions = await _rwDbContext.PendingTransactions
            .Where(pt => pt.IntentHash == intentHash)
            .ToListAsync(token);

        return pendingTransactions.Select(pt => new StatusLookupResult(pt.PayloadHash.ToHex(), pt.Status.ToGatewayModel(), pt.LastFailureReason)).ToArray();
    }

    private async Task<List<GatewayModel.CommittedTransactionInfo>> GetTransactions(List<long> transactionStateVersions, CancellationToken token)
    {
        var transactions = await _dbContext.LedgerTransactions
            .Where(ult => transactionStateVersions.Contains(ult.StateVersion))
            .ToListAsync(token);

        return transactions
            .OrderBy(lt => transactionStateVersions.IndexOf(lt.StateVersion))
            .Select(MapToGatewayAccountTransaction)
            .ToList();
    }

    private async Task<DetailsLookupResult> GetTransactionWithDetails(long stateVersion, GatewayModel.TransactionCommittedDetailsOptIns optInProperties, CancellationToken token)
    {
        // TODO ideally we'd like to run those as either single query or separate ones but without await between them

        var transaction = await _dbContext.LedgerTransactions
            .OfType<UserLedgerTransaction>()
            .Where(ult => ult.StateVersion == stateVersion)
            .OrderByDescending(lt => lt.StateVersion)
            .FirstAsync(token);

        List<Entity> referencedEntities = new List<Entity>();

        if (transaction.ReferencedEntities.Any())
        {
            referencedEntities = await _dbContext.Entities
                .Where(e => transaction.ReferencedEntities.Contains(e.Id))
                .ToListAsync(token);
        }

        return MapToGatewayAccountTransactionWithDetails(transaction, referencedEntities, optInProperties);
    }

    private async Task<Dictionary<string, long>> GetEntityIds(List<string> addresses, CancellationToken token = default)
    {
        return await _dbContext.Entities
            .Where(e => e.GlobalAddress != null && addresses.Contains(e.GlobalAddress.Value))
            .Select(e => new { e.Id, e.GlobalAddress!.Value })
            .ToDictionaryAsync(e => e.Value.ToString(), e => e.Id, token);
    }

    private GatewayModel.CommittedTransactionInfo MapToGatewayAccountTransaction(LedgerTransaction lt)
    {
        string? payloadHashHex = null;
        string? intentHashHex = null;

        if (lt is UserLedgerTransaction ult)
        {
            payloadHashHex = ult.PayloadHash.ToHex();
            intentHashHex = ult.IntentHash.ToHex();
        }

        return new GatewayModel.CommittedTransactionInfo(
            stateVersion: lt.StateVersion,
            epoch: lt.Epoch,
            round: lt.RoundInEpoch,
            transactionStatus: MapTransactionStatus(lt.EngineReceipt.Status),
            payloadHashHex: payloadHashHex,
            intentHashHex: intentHashHex,
            feePaid: lt.FeePaid.HasValue ? new GatewayModel.TokenAmount(lt.FeePaid.Value.ToString(), _networkConfigurationProvider.GetWellKnownAddresses().Xrd) : null,
            confirmedAt: lt.RoundTimestamp,
            errorMessage: lt.EngineReceipt.ErrorMessage
        );
    }

    private DetailsLookupResult MapToGatewayAccountTransactionWithDetails(UserLedgerTransaction ult, List<Entity> referencedEntities, GatewayModel.TransactionCommittedDetailsOptIns optIns)
    {
        return new DetailsLookupResult(MapToGatewayAccountTransaction(ult), new GatewayModel.TransactionCommittedDetailsResponseDetails(
            rawHex : optIns.RawHex ? ult.RawPayload.ToHex() : null,
            receipt: new GatewayModel.TransactionReceipt
            {
                ErrorMessage = ult.EngineReceipt.ErrorMessage,
                Status = MapTransactionStatus(ult.EngineReceipt.Status),
                Items = new JRaw(ult.EngineReceipt.Items),
                FeeSummary = optIns.ReceiptFeeSummary ? new JRaw(ult.EngineReceipt.FeeSummary) : null,
                NextEpoch = ult.EngineReceipt.NextEpoch != null ? new JRaw(ult.EngineReceipt.NextEpoch) : null,
                StateUpdates = optIns.ReceiptStateChanges ? new JRaw(ult.EngineReceipt.StateUpdates) : null,
                Events = optIns.ReceiptEvents ? new JRaw(ult.EngineReceipt.Events) : null,
            },
            referencedGlobalEntities: referencedEntities.Where(re => re.GlobalAddress != null).Select(re => re.GlobalAddress.ToString()).ToList(),
            messageHex: ult.Message?.ToHex()
        ));
    }

    private GatewayModel.TransactionStatus MapTransactionStatus(LedgerTransactionStatus status) => status switch
    {
        LedgerTransactionStatus.Succeeded => GatewayModel.TransactionStatus.CommittedSuccess,
        LedgerTransactionStatus.Failed => GatewayModel.TransactionStatus.CommittedFailure,
        _ => throw new UnreachableException($"Didn't expect {status} value"),
    };
}
