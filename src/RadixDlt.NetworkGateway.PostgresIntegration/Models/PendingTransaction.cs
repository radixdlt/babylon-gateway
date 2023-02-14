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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

/// <summary>
/// A record of transactions submitted recently by this and other nodes.
/// </summary>
[Table("pending_transactions")]
internal class PendingTransaction
{
    [Key]
    public long Id { get; set; }

    [Column("payload_hash")]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - Needed for EF Core
    public byte[] PayloadHash { get; private set; }

    [Column("intent_hash")]
    // OnModelCreating: Add intent_hash as alternate key.
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - Needed for EF Core
    public byte[] IntentHash { get; private set; }

    [Column("signed_intent_hash")]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - Needed for EF Core
    public byte[] SignedIntentHash { get; private set; }

    /// <summary>
    /// The payload of the transaction.
    /// </summary>
    [Column("notarized_transaction_blob")]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - Needed for EF Core
    public byte[] NotarizedTransactionBlob { get; private set; }

    [Column("status")]
    [ConcurrencyCheck]
    public PendingTransactionStatus Status { get; private set; }

    /// <summary>
    /// True if the transaction was submitted by this gateway. In which case, the gateway is responsible for
    /// monitoring its status in the core API, and resubmitting.
    /// </summary>
    [Column("submitted_by_this_gateway")]
    public bool SubmittedByThisGateway { get; private set; }

    /// <summary>
    /// The timestamp when the transaction was initially submitted to a node through this gateway.
    /// </summary>
    [Column("first_submitted_to_gateway_timestamp")]
    public DateTime? FirstSubmittedToGatewayTimestamp { get; private set; }

    /// <summary>
    /// The timestamp when the transaction was last submitted to a node.
    /// </summary>
    [Column("last_submitted_to_gateway_timestamp")]
    public DateTime? LastSubmittedToGatewayTimestamp { get; private set; }

    /// <summary>
    /// The timestamp when the transaction was last submitted to a node.
    /// </summary>
    [Column("last_submitted_to_node_timestamp")]
    public DateTime? LastSubmittedToNodeTimestamp { get; private set; }

    /// <summary>
    /// The timestamp when the transaction was last submitted to a node.
    /// </summary>
    [Column("last_submitted_to_node_name")]
    public string? LastSubmittedToNodeName { get; private set; }

    [Column("submission_count")]
    public int SubmissionToNodesCount { get; private set; }

    /// <summary>
    /// The timestamp when the transaction was first seen in a node's mempool.
    /// </summary>
    [Column("first_seen_in_mempool_timestamp")]
    public DateTime? FirstSeenInMempoolTimestamp { get; private set; }

    /// <summary>
    /// The timestamp when the transaction was last changed to a MISSING state.
    /// </summary>
    [Column("last_missing_from_mempool_timestamp")]
    public DateTime? LastDroppedOutOfMempoolTimestamp { get; private set; }

    /// <summary>
    /// The timestamp when the transaction was committed to the DB ledger.
    /// </summary>
    [Column("commit_timestamp")]
    public DateTime? CommitTimestamp { get; private set; }

    [Column("failure_reason")]
    public string? FailureReason { get; private set; }

    [Column("failure_timestamp")]
    public DateTime? FailureTimestamp { get; private set; }

    public static PendingTransaction NewFirstSeenInMempool(
        byte[] payloadHash,
        byte[] intentHash,
        byte[] notarizedTransaction,
        DateTime firstSeenAt
    )
    {
        var mempoolTransaction = new PendingTransaction
        {
            PayloadHash = payloadHash,
            IntentHash = intentHash,
            NotarizedTransactionBlob = notarizedTransaction,
        };

        mempoolTransaction.MarkAsSeenInAMempool(firstSeenAt);

        return mempoolTransaction;
    }

    public static PendingTransaction NewAsSubmittedForFirstTimeByGateway(
        byte[] payloadHash,
        byte[] intentHash,
        byte[] signedIntentHash,
        byte[] notarizedTransactionBlob,
        string submittedToNodeName,
        DateTime submittedTimestamp
    )
    {
        var pendingTransaction = new PendingTransaction
        {
            PayloadHash = payloadHash,
            IntentHash = intentHash,
            SignedIntentHash = signedIntentHash,
            NotarizedTransactionBlob = notarizedTransactionBlob,
            FirstSubmittedToGatewayTimestamp = submittedTimestamp,
        };

        pendingTransaction.MarkAsSubmittedToGateway(submittedTimestamp);

        // We assume it's been successfully submitted until we see an error and then mark it as an error then
        // This ensures the correct resubmission behaviour
        pendingTransaction.MarkAsAssumedSuccessfullySubmittedToNode(submittedToNodeName, submittedTimestamp);

        return pendingTransaction;
    }

    public void MarkAsMissing(DateTime timestamp)
    {
        Status = PendingTransactionStatus.Missing;
        LastDroppedOutOfMempoolTimestamp = timestamp;
    }

    public void MarkAsCommitted(PendingTransactionStatus status, DateTime timestamp)
    {
        Status = status;
        CommitTimestamp = timestamp;
        FailureReason = null;
        FailureTimestamp = null;
    }

    public void MarkAsSeenInAMempool(DateTime timestamp)
    {
        Status = PendingTransactionStatus.SubmittedOrKnownInNodeMempool;
        FirstSeenInMempoolTimestamp ??= timestamp;
    }

    public void MarkAsRejected(bool permanent, string failureReason, DateTime timestamp)
    {
        Status = permanent ? PendingTransactionStatus.RejectedPermanently : PendingTransactionStatus.RejectedTemporarily;
        FailureReason = failureReason;
        FailureTimestamp = timestamp;
    }

    public void MarkAsSubmittedToGateway(DateTime submittedAt)
    {
        SubmittedByThisGateway = true;
        FirstSubmittedToGatewayTimestamp ??= submittedAt;
        LastSubmittedToGatewayTimestamp = submittedAt;
    }

    public void MarkAsAssumedSuccessfullySubmittedToNode(string nodeSubmittedTo, DateTime submittedAt)
    {
        Status = PendingTransactionStatus.SubmittedOrKnownInNodeMempool;
        RecordSubmission(nodeSubmittedTo, submittedAt);
    }

    public void MarkAsFailedAfterSubmittedToNode(bool permanent, string nodeSubmittedTo, string failureReason, DateTime submittedAt, DateTime timestamp)
    {
        MarkAsRejected(permanent, failureReason, timestamp);
        RecordSubmission(nodeSubmittedTo, submittedAt);
    }

    private void RecordSubmission(string nodeSubmittedTo, DateTime submittedAt)
    {
        LastSubmittedToNodeTimestamp = submittedAt;
        LastSubmittedToNodeName = nodeSubmittedTo;
        SubmissionToNodesCount += 1;
    }
}
