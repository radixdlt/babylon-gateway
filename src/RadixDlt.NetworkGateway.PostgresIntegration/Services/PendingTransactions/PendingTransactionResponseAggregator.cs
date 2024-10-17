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

using RadixDlt.NetworkGateway.Abstractions.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services.PendingTransactions;

internal record PendingTransactionSummary(
    string PayloadHash,
    long EndEpochExclusive,
    DateTime? ResubmitFromTimestamp,
    string? HandlingStatusReason,
    PendingTransactionPayloadLedgerStatus PayloadStatus,
    PendingTransactionIntentLedgerStatus IntentStatus,
    string? InitialRejectionReason,
    string? LatestRejectionReason,
    string? LastSubmissionError);

internal class PendingTransactionResponseAggregator
{
    private static readonly GatewayModel.TransactionIntentStatus[] _statusesToReturnPermanentlyRejectsAtEpoch =
    {
        GatewayModel.TransactionIntentStatus.Unknown,
        GatewayModel.TransactionIntentStatus.LikelyButNotCertainRejection,
        GatewayModel.TransactionIntentStatus.Pending,
    };

    private readonly GatewayModel.LedgerState _ledgerState;
    private readonly string? _committedPayloadHash;
    private readonly long? _committedStateVersion;
    private readonly List<GatewayModel.TransactionStatusResponseKnownPayloadItem> _knownPayloads = new();
    private readonly GatewayModel.TransactionIntentStatus? _committedIntentStatus;
    private readonly string? _committedErrorMessage;
    private PendingTransactionIntentLedgerStatus _mostAccuratePendingTransactionIntentLedgerStatus = PendingTransactionIntentLedgerStatus.Unknown;
    private string? _rejectionReasonForMostAccurateIntentLedgerStatus;
    private long? _rejectionEpoch;

    internal PendingTransactionResponseAggregator(GatewayModel.LedgerState ledgerState, TransactionQuerier.CommittedTransactionSummary? committedTransactionSummary)
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
            LedgerTransactionStatus.Succeeded => (GatewayModel.TransactionStatus.CommittedSuccess, GatewayModel.TransactionPayloadStatus.CommittedSuccess,
                GatewayModel.TransactionIntentStatus.CommittedSuccess),
            LedgerTransactionStatus.Failed => (GatewayModel.TransactionStatus.CommittedFailure, GatewayModel.TransactionPayloadStatus.CommittedFailure,
                GatewayModel.TransactionIntentStatus.CommittedFailure),
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

        _rejectionEpoch = pendingTransactionSummary.EndEpochExclusive;

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
        if (_ledgerState.Epoch >= pendingTransactionSummary.EndEpochExclusive)
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

        var permanentlyRejectsAtEpoch = _statusesToReturnPermanentlyRejectsAtEpoch.Contains(intentStatus) ? _rejectionEpoch : null;

        return new GatewayModel.TransactionStatusResponse(
            ledgerState: _ledgerState,
            status: legacyIntentStatus,
            intentStatus,
            intentStatusDescription: GetIntentStatusDescription(intentStatus),
            knownPayloads: _knownPayloads,
            committedStateVersion: _committedStateVersion,
            permanentlyRejectsAtEpoch: (long?)permanentlyRejectsAtEpoch,
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
