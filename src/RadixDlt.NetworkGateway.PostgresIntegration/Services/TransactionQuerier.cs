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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
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
    private readonly ITransactionBalanceChangesService _transactionBalanceChangesService;
    private readonly ILogger _logger;

    public TransactionQuerier(
        ReadOnlyDbContext dbContext,
        ReadWriteDbContext rwDbContext,
        INetworkConfigurationProvider networkConfigurationProvider,
        ITransactionBalanceChangesService transactionBalanceChangesService,
        ILogger<TransactionQuerier> logger)
    {
        _dbContext = dbContext;
        _rwDbContext = rwDbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
        _transactionBalanceChangesService = transactionBalanceChangesService;
        _logger = logger;
    }

    public async Task<TransactionPageWithoutTotal> GetTransactionStream(TransactionStreamPageRequest request, GatewayModel.LedgerState atLedgerState, CancellationToken token = default)
    {
        var referencedAddresses = request
            .SearchCriteria
            .ManifestAccountsDepositedInto
            .Concat(request.SearchCriteria.ManifestAccountsWithdrawnFrom)
            .Concat(request.SearchCriteria.ManifestResources)
            .Concat(request.SearchCriteria.AffectedGlobalEntities)
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

                searchQuery = searchQuery
                    .Join(_dbContext.LedgerTransactionMarkers, sv => sv, ltm => ltm.StateVersion, (sv, ltm) => ltm)
                    .OfType<ManifestAddressLedgerTransactionMarker>()
                    .Where(maltm => maltm.OperationType == LedgerTransactionMarkerOperationType.AccountDepositedInto && maltm.EntityId == entityId)
                    .Where(maltm => maltm.StateVersion <= upperStateVersion && maltm.StateVersion >= (lowerStateVersion ?? maltm.StateVersion))
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
                    .Where(maltm => maltm.OperationType == LedgerTransactionMarkerOperationType.AccountWithdrawnFrom && maltm.EntityId == entityId)
                    .Where(maltm => maltm.StateVersion <= upperStateVersion && maltm.StateVersion >= (lowerStateVersion ?? maltm.StateVersion))
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
                    .Where(maltm => maltm.OperationType == LedgerTransactionMarkerOperationType.ResourceInUse && maltm.EntityId == entityId)
                    .Where(maltm => maltm.StateVersion <= upperStateVersion && maltm.StateVersion >= (lowerStateVersion ?? maltm.StateVersion))
                    .Select(maltm => maltm.StateVersion);
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

    private record CommittedTransactionSummary(
        long StateVersion,
        string PayloadHash,
        LedgerTransactionStatus Status,
        string? ErrorMessage);

    private record PendingTransactionSummary(
        string PayloadHash,
        ulong EndEpochExclusive,
        DateTime? ResubmitFromTimestamp,
        string? HandlingStatusReason,
        PendingTransactionPayloadLedgerStatus PayloadStatus,
        PendingTransactionIntentLedgerStatus IntentStatus,
        string? InitialRejectionReason,
        string? LatestRejectionReason,
        string? LastSubmissionError);

    private class PendingTransactionResponseAggregator
    {
        private readonly GatewayModel.LedgerState _ledgerState;
        private readonly string? _committedPayloadHash;
        private readonly long? _committedStateVersion;
        private readonly List<GatewayModel.TransactionStatusResponseKnownPayloadItem> _knownPayloads = new();
        private readonly GatewayModel.TransactionIntentStatus? _committedIntentStatus;
        private readonly string? _committedErrorMessage;
        private PendingTransactionIntentLedgerStatus _mostAccuratePendingTransactionIntentLedgerStatus = PendingTransactionIntentLedgerStatus.Unknown;
        private string? _rejectionReasonForMostAccurateIntentLedgerStatus;

        internal PendingTransactionResponseAggregator(GatewayModel.LedgerState ledgerState, CommittedTransactionSummary? committedTransactionSummary)
        {
            _ledgerState = ledgerState;

            if (committedTransactionSummary is null)
            {
                return;
            }

            _committedPayloadHash = committedTransactionSummary.PayloadHash;
            _committedStateVersion = committedTransactionSummary.StateVersion;

            var (legacyStatus, payloadStatus, intentStatus) = committedTransactionSummary.Status switch
            {
                LedgerTransactionStatus.Succeeded => (GatewayModel.TransactionStatus.CommittedSuccess, GatewayModel.TransactionPayloadStatus.CommittedSuccess, GatewayModel.TransactionIntentStatus.CommittedSuccess),
                LedgerTransactionStatus.Failed => (GatewayModel.TransactionStatus.CommittedFailure, GatewayModel.TransactionPayloadStatus.CommittedFailure, GatewayModel.TransactionIntentStatus.CommittedFailure),
            };

            _committedIntentStatus = intentStatus;
            _committedErrorMessage = committedTransactionSummary.ErrorMessage;

            _knownPayloads.Add(new GatewayModel.TransactionStatusResponseKnownPayloadItem(
                payloadHash: committedTransactionSummary.PayloadHash,
                status: legacyStatus,
                payloadStatus: payloadStatus,
                payloadStatusDescription: GetPayloadStatusDescription(payloadStatus),
                errorMessage: committedTransactionSummary.ErrorMessage,
                handlingStatus: GatewayModel.TransactionPayloadGatewayHandlingStatus.Concluded,
                handlingStatusReason: "The transaction is committed",
                submissionError: null
            ));
        }

        internal void AddPendingTransaction(PendingTransactionSummary pendingTransactionSummary)
        {
            if (pendingTransactionSummary.PayloadHash == _committedPayloadHash)
            {
                return;
            }

            var (legacyStatus, payloadStatus) = pendingTransactionSummary.PayloadStatus switch
            {
                PendingTransactionPayloadLedgerStatus.Unknown => (GatewayModel.TransactionStatus.Unknown, GatewayModel.TransactionPayloadStatus.Unknown),
                PendingTransactionPayloadLedgerStatus.Committed => (GatewayModel.TransactionStatus.Pending, GatewayModel.TransactionPayloadStatus.CommitPendingOutcomeUnknown),
                PendingTransactionPayloadLedgerStatus.CommitPending => (GatewayModel.TransactionStatus.Pending, GatewayModel.TransactionPayloadStatus.CommitPendingOutcomeUnknown),
                PendingTransactionPayloadLedgerStatus.ClashingCommit => (GatewayModel.TransactionStatus.Rejected, GatewayModel.TransactionPayloadStatus.PermanentlyRejected),
                PendingTransactionPayloadLedgerStatus.PermanentlyRejected => (GatewayModel.TransactionStatus.Rejected, GatewayModel.TransactionPayloadStatus.PermanentlyRejected),
                PendingTransactionPayloadLedgerStatus.TransientlyAccepted => (GatewayModel.TransactionStatus.Pending, GatewayModel.TransactionPayloadStatus.Pending),
                PendingTransactionPayloadLedgerStatus.TransientlyRejected => (GatewayModel.TransactionStatus.Pending, GatewayModel.TransactionPayloadStatus.TemporarilyRejected),
            };

            var handlingStatus = pendingTransactionSummary.ResubmitFromTimestamp switch
            {
                not null => GatewayModel.TransactionPayloadGatewayHandlingStatus.HandlingSubmission,
                null => GatewayModel.TransactionPayloadGatewayHandlingStatus.Concluded,
            };

            if (pendingTransactionSummary.IntentStatus.AggregationPriorityAcrossKnownPayloads() >= _mostAccuratePendingTransactionIntentLedgerStatus.AggregationPriorityAcrossKnownPayloads())
            {
                _mostAccuratePendingTransactionIntentLedgerStatus = pendingTransactionSummary.IntentStatus;
                _rejectionReasonForMostAccurateIntentLedgerStatus = pendingTransactionSummary.LatestRejectionReason;
            }

            var initialRejectionReason = pendingTransactionSummary.InitialRejectionReason;
            var latestRejectionReason = pendingTransactionSummary.LatestRejectionReason;

            // If the intent's EndEpochExclusive has been reached, then the payload and intent must be permanently rejected (assuming they're not already committed).
            if ((ulong)_ledgerState.Epoch >= pendingTransactionSummary.EndEpochExclusive)
            {
                // The if statement is defence-in-depth to avoid replacing a Committed status with PermanentlyRejected.
                // This shouldn't matter, because we'd see a committed transaction in this case, which would trump the _mostAccuratePendingTransactionIntentLedgerStatus when we
                // resolve the status. But best to be defensive anyway.
                if (PendingTransactionIntentLedgerStatus.PermanentRejection.AggregationPriorityAcrossKnownPayloads() >=
                    _mostAccuratePendingTransactionIntentLedgerStatus.AggregationPriorityAcrossKnownPayloads())
                {
                    _mostAccuratePendingTransactionIntentLedgerStatus = PendingTransactionIntentLedgerStatus.PermanentRejection;
                    _rejectionReasonForMostAccurateIntentLedgerStatus = $"Transaction has expired, as its expiry epoch of {pendingTransactionSummary.EndEpochExclusive} has been reached.";
                }

                // The if statement is defence-in-depth to avoid replacing a Committed status with PermanentlyRejected.
                // (Even though this shouldn't be possible because of the early return if pendingTransactionSummary.PayloadHash == _committedPayloadHash)
                if (PendingTransactionPayloadLedgerStatus.PermanentlyRejected.ReplacementPriority() >= pendingTransactionSummary.PayloadStatus.ReplacementPriority())
                {
                    payloadStatus = GatewayModel.TransactionPayloadStatus.PermanentlyRejected;
                    legacyStatus = GatewayModel.TransactionStatus.Rejected;
                    latestRejectionReason = $"Transaction has expired, as its expiry epoch of {pendingTransactionSummary.EndEpochExclusive} has been reached.";
                    initialRejectionReason ??= latestRejectionReason;
                }
            }

            _knownPayloads.Add(new GatewayModel.TransactionStatusResponseKnownPayloadItem(
                payloadHash: pendingTransactionSummary.PayloadHash,
                status: legacyStatus,
                payloadStatus: payloadStatus,
                payloadStatusDescription: GetPayloadStatusDescription(payloadStatus),
                errorMessage: initialRejectionReason,
                latestErrorMessage: latestRejectionReason == initialRejectionReason ? null : latestRejectionReason,
                handlingStatus: handlingStatus,
                handlingStatusReason: pendingTransactionSummary.HandlingStatusReason,
                submissionError: pendingTransactionSummary.LastSubmissionError
            ));
        }

        internal GatewayModel.TransactionStatusResponse IntoResponse()
        {
            var (intentStatus, errorMessage) = _committedIntentStatus != null
                ? (_committedIntentStatus.Value, _committedErrorMessage)
                : (
                    _mostAccuratePendingTransactionIntentLedgerStatus switch
                    {
                        PendingTransactionIntentLedgerStatus.Unknown => GatewayModel.TransactionIntentStatus.Unknown,
                        PendingTransactionIntentLedgerStatus.Committed => GatewayModel.TransactionIntentStatus.CommitPendingOutcomeUnknown,
                        PendingTransactionIntentLedgerStatus.CommitPending => GatewayModel.TransactionIntentStatus.CommitPendingOutcomeUnknown,
                        PendingTransactionIntentLedgerStatus.PermanentRejection => GatewayModel.TransactionIntentStatus.PermanentlyRejected,
                        PendingTransactionIntentLedgerStatus.PossibleToCommit => GatewayModel.TransactionIntentStatus.Pending,
                        PendingTransactionIntentLedgerStatus.LikelyButNotCertainRejection => GatewayModel.TransactionIntentStatus.LikelyButNotCertainRejection,
                    },
                    _rejectionReasonForMostAccurateIntentLedgerStatus
                );

            var legacyIntentStatus = intentStatus switch
            {
                GatewayModel.TransactionIntentStatus.Unknown => GatewayModel.TransactionStatus.Unknown,
                GatewayModel.TransactionIntentStatus.CommittedSuccess => GatewayModel.TransactionStatus.CommittedSuccess,
                GatewayModel.TransactionIntentStatus.CommittedFailure => GatewayModel.TransactionStatus.CommittedFailure,
                GatewayModel.TransactionIntentStatus.CommitPendingOutcomeUnknown => GatewayModel.TransactionStatus.Pending,
                GatewayModel.TransactionIntentStatus.PermanentlyRejected => GatewayModel.TransactionStatus.Rejected,
                GatewayModel.TransactionIntentStatus.LikelyButNotCertainRejection => GatewayModel.TransactionStatus.Pending,
                GatewayModel.TransactionIntentStatus.Pending => GatewayModel.TransactionStatus.Pending,
            };

            return new GatewayModel.TransactionStatusResponse(
                ledgerState: _ledgerState,
                status: legacyIntentStatus,
                intentStatus,
                intentStatusDescription: GetIntentStatusDescription(intentStatus),
                knownPayloads: _knownPayloads,
                committedStateVersion: _committedStateVersion,
                errorMessage
            );
        }

        private string GetPayloadStatusDescription(GatewayModel.TransactionPayloadStatus payloadStatus)
        {
            return payloadStatus switch
            {
                GatewayModel.TransactionPayloadStatus.Unknown =>
                    "No information is known about the possible outcome of this transaction payload.",
                GatewayModel.TransactionPayloadStatus.CommittedSuccess =>
                    "This particular payload for this transaction has been committed to the ledger as a success. For more information, use the /transaction/committed-details endpoint.",
                GatewayModel.TransactionPayloadStatus.CommittedFailure =>
                    "This particular payload for this transaction has been committed to the ledger as a failure. For more information, use the /transaction/committed-details endpoint.",
                GatewayModel.TransactionPayloadStatus.CommitPendingOutcomeUnknown =>
                    "This particular payload for this transaction has been committed to the ledger, but the Gateway is still waiting for further details about its result.",
                GatewayModel.TransactionPayloadStatus.PermanentlyRejected =>
                    "This particular payload for this transaction has been permanently rejected. It is not possible for this particular transaction payload to be committed to this network.",
                GatewayModel.TransactionPayloadStatus.TemporarilyRejected =>
                    "This particular payload for this transaction was rejected at its last execution. It may still be possible for this transaction payload to be committed to this network.",
                GatewayModel.TransactionPayloadStatus.Pending =>
                    "This particular payload for this transaction has been accepted into a node's mempool on the network. It's possible but not certain that it will be committed to this network.",
            };
        }

        private string GetIntentStatusDescription(GatewayModel.TransactionIntentStatus intentStatus)
        {
            return intentStatus switch
            {
                GatewayModel.TransactionIntentStatus.Unknown =>
                    "No information is known about the possible outcome of this transaction.",
                GatewayModel.TransactionIntentStatus.CommittedSuccess =>
                    "This transaction has been committed to the ledger as a success. For more information, use the /transaction/committed-details endpoint.",
                GatewayModel.TransactionIntentStatus.CommittedFailure =>
                    "This transaction has been committed to the ledger as a failure. For more information, use the /transaction/committed-details endpoint.",
                GatewayModel.TransactionIntentStatus.CommitPendingOutcomeUnknown =>
                    "This transaction has been committed to the ledger, but the Gateway is still waiting for further details about its result.",
                GatewayModel.TransactionIntentStatus.PermanentlyRejected =>
                    "This transaction is permanently rejected, so can never be committed to this network.",
                GatewayModel.TransactionIntentStatus.Pending =>
                    "A payload for this transaction has been accepted into a node's mempool on the network. It's possible but not certain that it will be committed to this network.",
                GatewayModel.TransactionIntentStatus.LikelyButNotCertainRejection =>
                    "All known payload/s for this transaction have been temporarily rejected at their last execution. It may still be possible for this transaction to be committed to this network.",
            };
        }
    }

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
        {optIns.ReceiptEvents} AS with_receipt_events
)
SELECT
    state_version, epoch, round_in_epoch, index_in_epoch,
    index_in_round, fee_paid, tip_paid, affected_global_entities,
    round_timestamp, created_timestamp, normalized_round_timestamp,
    receipt_status, receipt_fee_source, receipt_fee_destination, receipt_error_message,
    transaction_tree_hash, receipt_tree_hash, state_tree_hash,
    discriminator, payload_hash, intent_hash, signed_intent_hash, message,
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
    CASE WHEN configuration.with_receipt_events THEN receipt_event_sbor_type_kinds ELSE '{{}}'::sbor_type_kind[] END AS receipt_event_sbor_type_kinds
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
                .SchemaHistory
                .FromSqlInterpolated($@"
WITH variables (entity_id, schema_hash) AS (
    SELECT UNNEST({entityIds}), UNNEST({schemaHashes})
)
SELECT sh.*
FROM variables var
INNER JOIN schema_history sh ON sh.entity_id = var.entity_id AND sh.schema_hash = var.schema_hash")
                .AnnotateMetricName("GetEventSchemas")
                .ToDictionaryAsync(x => new SchemaLookup(x.EntityId, x.SchemaHash), x => x.Schema, token);
        }

        List<GatewayModel.CommittedTransactionInfo> mappedTransactions = new List<GatewayModel.CommittedTransactionInfo>();
        var networkId = _networkConfigurationProvider.GetNetworkId();
        var orderedTransactions = transactions.OrderBy(lt => transactionStateVersions.IndexOf(lt.StateVersion)).ToList();

        Dictionary<long, CoreModel.LtsCommittedTransactionOutcome>? balanceChangesPerTransaction = null;

        if (optIns.BalanceChanges)
        {
            balanceChangesPerTransaction = await _transactionBalanceChangesService.GetTransactionBalanceChanges(orderedTransactions.Select(x => x.StateVersion).ToList(), token);
        }

        foreach (var transaction in orderedTransactions)
        {
            GatewayModel.TransactionBalanceChanges? balanceChanges = null;

            if (balanceChangesPerTransaction?.TryGetValue(transaction.StateVersion, out var bc) == true)
            {
                balanceChanges = bc.ToGatewayModel();
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
