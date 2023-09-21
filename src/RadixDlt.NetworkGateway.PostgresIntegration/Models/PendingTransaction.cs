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
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using RadixDlt.NetworkGateway.Abstractions.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable InvertIf

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

/// <summary>
/// A record of a transaction payload submitted to this Gateway.
/// </summary>
[Table("pending_transactions")]
internal class PendingTransaction
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// The Bech32m encoded payload hash.
    /// </summary>
    [Column("payload_hash")]
    public string PayloadHash { get; private set; }

    /// <summary>
    /// The Bech32m encoded intent hash.
    /// </summary>
    [Column("intent_hash")]
    public string IntentHash { get; private set; }

    /// <summary>
    /// The epoch at which this transaction will no longer be valid.
    /// </summary>
    [Column("end_epoch_exclusive")]
    public ulong EndEpochExclusive { get; private set; }

    [Column("payload_id")]
    public long PayloadId { get; private set; }

    /// <summary>
    /// The payload of the transaction.
    /// </summary>
    [Required]
    [ForeignKey(nameof(PayloadId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public PendingTransactionPayload Payload { get; private set; }

    // TODO(David) - do we want to adjust this / change the concurrency token paradigm?
    // We want Aggregator or anything final to still change the token, but to ignore the check / clobber on write

    /// <summary>
    /// Used as optimistic locking guard where we increase this value on every significant update.
    /// </summary>
    /// <remarks>
    /// The ledger extender service might update this entity without any checks as it is considered to have highest priority and it is expected that other services should detect
    /// concurrency issues instead.
    /// </remarks>
    [Column("xmin")]
    [Timestamp]
    public uint VersionControl { get; private set; }

    public static PendingTransaction NewAsSubmittedForFirstTimeToGateway(
        PendingTransactionHandlingConfig handlingConfig,
        string payloadHash,
        string intentHash,
        ulong endEpochExclusive,
        byte[] notarizedTransaction,
        DateTime timestamp
    )
    {
        return new PendingTransaction
        {
            PayloadHash = payloadHash,
            IntentHash = intentHash,
            EndEpochExclusive = endEpochExclusive,
            Payload = new PendingTransactionPayload
            {
                NotarizedTransactionBlob = notarizedTransaction,
            },
            LedgerDetails = PendingTransactionLedgerDetails.NewUnknown(),
            GatewayHandling = PendingTransactionGatewayHandling.NewlySubmittedToGatewayWithNodeSubmissionPending(timestamp, handlingConfig),
            NetworkDetails = PendingTransactionNetworkDetails.NewWithNodeSubmissionPending(timestamp),
        };
    }

    public void HandleNodeSubmissionResult(
        PendingTransactionHandlingConfig handlingConfig,
        string submittedToNodeName,
        NodeSubmissionResult nodeSubmissionResult,
        DateTime handledAt,
        ulong? currentEpoch
    )
    {
        LedgerDetails.HandleSubmissionResult(nodeSubmissionResult, handledAt);
        NetworkDetails.HandleNodeSubmissionResult(submittedToNodeName, nodeSubmissionResult);
        GatewayHandling.HandleNodeSubmissionResult(nodeSubmissionResult);
        RetireIfNecessary(handlingConfig, handledAt, currentEpoch);
    }

    public void MarkAsCommitted(
        long stateVersion,
        bool isSuccess,
        string? failureReason,
        DateTime markedCommittedAt
    )
    {
        LedgerDetails.HandleCommitted(stateVersion, isSuccess, failureReason, markedCommittedAt);
        GatewayHandling.MarkAsNoLongerSubmitting("Concluded as committed");
    }

    /// <summary>
    /// Returns if the submission should proceed.
    /// </summary>
    public bool UpdateForPendingSubmissionOrRetirement(
        PendingTransactionHandlingConfig handlingConfig,
        DateTime currentTime,
        ulong? currentEpoch)
    {
        var retired = RetireIfNecessary(handlingConfig, currentTime, currentEpoch);
        if (!retired)
        {
            GatewayHandling.HandleNodeSubmissionPending(currentTime, handlingConfig);
            NetworkDetails.HandleNodeSubmissionPending(currentTime);
        }

        return !retired;
    }

    /// <summary>
    /// Returns true if retirement was necessary.
    /// </summary>
    private bool RetireIfNecessary(
        PendingTransactionHandlingConfig handlingConfig,
        DateTime currentTime,
        ulong? currentEpoch)
    {
        if (LedgerDetails.PayloadLedgerStatus.ShouldStopSubmittingTransactionToNetwork())
        {
            GatewayHandling.MarkAsNoLongerSubmitting($"Concluded due to PayloadLedgerStatus of {LedgerDetails.PayloadLedgerStatus}");
            return true;
        }

        if (currentEpoch >= EndEpochExclusive)
        {
            GatewayHandling.MarkAsNoLongerSubmitting("Concluded due to its end epoch being reached");
            return true;
        }

        var withinSubmissionCount = GatewayHandling.AttemptedSubmissionToNodesCount < handlingConfig.MaxSubmissionsBeforeGivingUp;
        if (!withinSubmissionCount)
        {
            GatewayHandling.MarkAsNoLongerSubmitting("Concluded due to exceeding max submission to node count");
            return true;
        }

        var withinSubmissionCutoff = currentTime <= (GatewayHandling.FirstSubmittedToGatewayTimestamp + handlingConfig.StopResubmittingAfter);
        if (!withinSubmissionCutoff)
        {
            GatewayHandling.MarkAsNoLongerSubmitting("Concluded due to exceeding max time since submission to the Gateway");
            return true;
        }

        return false;
    }

    // [Owned] below
    public PendingTransactionLedgerDetails LedgerDetails { get; private set; }

    // [Owned] below
    public PendingTransactionGatewayHandling GatewayHandling { get; private set; }

    // [Owned] below
    public PendingTransactionNetworkDetails NetworkDetails { get; private set; }
}

/// <summary>
/// Designed to be in a separate table, so that it's only loaded when it's explicitly requested.
/// This aims to avoid loading large blobs into memory when they're not needed.
/// </summary>
[Table("pending_transaction_payloads")]
internal class PendingTransactionPayload
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("notarized_transaction_blob")]
    public byte[] NotarizedTransactionBlob { get; set; }
}

/// <summary>
/// The part of the PendingTransaction which pertains to the status / likely result of the transaction.
/// </summary>
[Owned]
internal record PendingTransactionLedgerDetails
{
    private PendingTransactionPayloadLedgerStatus _payloadLedgerStatus;
    private PendingTransactionIntentLedgerStatus _intentLedgerStatus;

    /// <summary>
    /// The status of the transaction payload, as discovered by submission to nodes.
    /// </summary>
    [Column("payload_status")]
    public PendingTransactionPayloadLedgerStatus PayloadLedgerStatus
    {
        get => _payloadLedgerStatus;
        private set
        {
            if (value.ReplacementPriority() >= _payloadLedgerStatus.ReplacementPriority())
            {
                _payloadLedgerStatus = value;
            }
        }
    }

    /// <summary>
    /// The status of the transaction intent, as discovered by submission of this particular payload to nodes.
    /// Note that if we are tracking multiple payloads for a single intent, we will need to aggregate across
    /// those intents.
    /// </summary>
    [Column("intent_status")]
    public PendingTransactionIntentLedgerStatus IntentLedgerStatus
    {
        get => _intentLedgerStatus;
        private set
        {
            if (value.ReplacementPriorityForSinglePayload() >= _intentLedgerStatus.ReplacementPriorityForSinglePayload())
            {
                _intentLedgerStatus = value;
            }
        }
    }

    [Column("first_failure_reason")]
    public string? FirstFailureReason { get; private set; }

    [Column("latest_failure_reason")]
    public string? LatestFailureReason { get; private set; }

    [Column("latest_failure_timestamp")]
    public DateTime? LatestFailureTimestamp { get; private set; }

    /// <summary>
    /// The timestamp when the Gateway discovered that the transaction was committed to the DB ledger.
    /// </summary>
    [Column("commit_timestamp")]
    public DateTime? CommitTimestamp { get; private set; }

    /// <summary>
    /// The state version of the transaction, if we know it has been committed.
    /// </summary>
    [Column("state_version")]
    public long? CommitStateVersion { get; private set; }

    private PendingTransactionLedgerDetails()
    {
    }

    internal static PendingTransactionLedgerDetails NewUnknown()
    {
        return new PendingTransactionLedgerDetails
        {
            PayloadLedgerStatus = PendingTransactionPayloadLedgerStatus.Unknown,
            IntentLedgerStatus = PendingTransactionIntentLedgerStatus.Unknown,
        };
    }

    internal void HandleSubmissionResult(NodeSubmissionResult nodeSubmissionResult, DateTime submitResultAt)
    {
        PayloadLedgerStatus = nodeSubmissionResult.PayloadStatus();
        IntentLedgerStatus = nodeSubmissionResult.IntentStatus();
        var failureReason = nodeSubmissionResult.GetExecutionFailureReason();
        if (!string.IsNullOrEmpty(failureReason))
        {
            FirstFailureReason ??= failureReason;
            LatestFailureReason = failureReason;
            LatestFailureTimestamp = submitResultAt;
        }

        // ReSharper disable once InvertIf - it's clearer like this, not inverted
        if (nodeSubmissionResult is NodeSubmissionResult.IntentAlreadyCommitted { CommittedAsDetails: var details })
        {
            CommitTimestamp = submitResultAt;
            CommitStateVersion = details.StateVersion;
        }
    }

    internal void HandleCommitted(long stateVersion, bool isSuccess, string? failureReason, DateTime submitResultAt)
    {
        if (isSuccess)
        {
            PayloadLedgerStatus = PendingTransactionPayloadLedgerStatus.CommittedSuccess;
            IntentLedgerStatus = PendingTransactionIntentLedgerStatus.CommittedSuccess;
        }
        else
        {
            PayloadLedgerStatus = PendingTransactionPayloadLedgerStatus.CommittedFailure;
            IntentLedgerStatus = PendingTransactionIntentLedgerStatus.CommittedFailure;
            if (!string.IsNullOrEmpty(failureReason))
            {
                FirstFailureReason ??= failureReason;
                LatestFailureReason = failureReason;
            }

            LatestFailureTimestamp = submitResultAt;
        }

        CommitTimestamp = submitResultAt;
        CommitStateVersion = stateVersion;
    }
}

/// <summary>
/// The part of the PendingTransaction concerning the Gateway's handling of the submission of this transaction to the network,
/// the presence of the transaction in node mempools, and submission of the transaction to nodes.
/// </summary>
[Owned]
internal record PendingTransactionGatewayHandling
{
    /// <summary>
    /// The timestamp when the transaction can next be resubmitted to nodes.
    /// If this is null then the Gateway is not trying to resubmit this transaction.
    /// </summary>
    [Column("resubmit_from_timestamp")]
    public DateTime? ResubmitFromTimestamp { get; private set; }

    /// <summary>
    /// Additional details as to the current handling status, as communicated by the ResubmitFromTimestamp.
    /// </summary>
    [Column("handling_status_reason")]
    public string? HandlingStatusReason { get; private set; }

    /// <summary>
    /// The timestamp when the transaction was initially submitted to a node through this gateway.
    /// </summary>
    [Column("first_submitted_to_gateway_timestamp")]
    public DateTime FirstSubmittedToGatewayTimestamp { get; private set; }

    [Column("node_submission_count")]
    public int AttemptedSubmissionToNodesCount { get; private set; }

    private PendingTransactionGatewayHandling()
    {
    }

    internal static PendingTransactionGatewayHandling NewlySubmittedToGatewayWithNodeSubmissionPending(
        DateTime submittedAt,
        PendingTransactionHandlingConfig handlingConfig
    )
    {
        var handling = new PendingTransactionGatewayHandling
        {
            FirstSubmittedToGatewayTimestamp = submittedAt,
            HandlingStatusReason = "Submitting to the network due to being submitted to Gateway",
        };
        handling.HandleNodeSubmissionPending(submittedAt, handlingConfig);
        return handling;
    }

    internal void HandleNodeSubmissionPending(DateTime submittingAt, PendingTransactionHandlingConfig handlingConfig)
    {
        ResubmitFromTimestamp = submittingAt + (handlingConfig.BaseTimeBetweenResubmissions * Math.Pow(handlingConfig.ResubmissionDelayBackoffExponent, AttemptedSubmissionToNodesCount));
        AttemptedSubmissionToNodesCount += 1;
    }

    internal void HandleNodeSubmissionResult(
        NodeSubmissionResult nodeSubmissionResult)
    {
        if (nodeSubmissionResult.ShouldStopSubmittingPermanently())
        {
            MarkAsNoLongerSubmitting("Concluded due to seeing permanent rejection.");
        }
    }

    internal void MarkAsNoLongerSubmitting(string reason)
    {
        HandlingStatusReason = reason;
        ResubmitFromTimestamp = null;
    }
}

/// <summary>
/// The part of the PendingTransaction concerning the presence of the transaction in node mempools, and submission of the transaction to nodes.
/// </summary>
[Owned]
internal record PendingTransactionNetworkDetails
{
    /// <summary>
    /// The timestamp when the transaction was last submitted to a node.
    /// </summary>
    [Column("latest_node_submission_timestamp")]
    public DateTime? LatestNodeSubmissionTimestamp { get; private set; }

    /// <summary>
    /// The timestamp when the transaction was last submitted to a node.
    /// </summary>
    [Column("latest_submitted_to_node_name")]
    public string? LatestSubmittedToNodeName { get; private set; }

    [Column("latest_node_submission_was_accepted")]
    public bool LatestNodeSubmissionWasAccepted { get; private set; }

    /// <summary>
    /// The last error when submitting to a node - eg mempool full.
    /// </summary>
    [Column("last_submit_error")]
    public string? LastSubmitErrorTitle { get; private set; }

    private PendingTransactionNetworkDetails()
    {
    }

    internal static PendingTransactionNetworkDetails NewWithNodeSubmissionPending(DateTime currentTime)
    {
        var details = new PendingTransactionNetworkDetails();
        details.HandleNodeSubmissionPending(currentTime);
        return details;
    }

    internal void HandleNodeSubmissionPending(DateTime timestamp)
    {
        LatestNodeSubmissionTimestamp = timestamp;
    }

    internal void HandleNodeSubmissionResult(
        string submittedToNodeName,
        NodeSubmissionResult nodeSubmissionResult)
    {
        LatestNodeSubmissionWasAccepted = nodeSubmissionResult is NodeSubmissionResult.AcceptedIntoMempool;
        LastSubmitErrorTitle ??= nodeSubmissionResult.GetApiSubmitErrorTitle();
        LatestSubmittedToNodeName = submittedToNodeName;
    }
}
