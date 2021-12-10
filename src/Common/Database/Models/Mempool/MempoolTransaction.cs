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

using Common.Database.ValueConverters;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Gateway = RadixGatewayApi.Generated.Model;

namespace Common.Database.Models.Mempool;

public enum MempoolTransactionStatus
{
    InNodeMempool, // A transaction which appears in at least one mempool at the last update
    Missing,       // A transaction which is no longer in any mempool
    Failed,        // A transaction which we have tried to (re)submit, but it returns a permanent error from the node (eg substate clash)
    Committed,     // A transaction which we know got committed to the ledger
                   // NOTE due to race conditions, it might be possible for a transaction to end up Pending/Missing even it's committed
}

public class MempoolTransactionSubmissionStatusValueConverter : EnumTypeValueConverterBase<MempoolTransactionStatus>
{
    private static readonly Dictionary<MempoolTransactionStatus, string> _conversion = new()
    {
        { MempoolTransactionStatus.InNodeMempool, "IN_NODE_MEMPOOL" },
        { MempoolTransactionStatus.Missing, "MISSING" },
        { MempoolTransactionStatus.Failed, "FAILED" },
        { MempoolTransactionStatus.Committed, "COMMITTED" },
    };

    public MempoolTransactionSubmissionStatusValueConverter()
        : base(_conversion, Invert(_conversion))
    {
    }
}

public enum MempoolTransactionFailureReason
{
    DoubleSpend,
    Timeout,
    Unknown,
    // Invalid shouldn't be possible, because they shouldn't make it to this table in the first place - mark as Unknown
}

public class MempoolTransactionFailureReasonValueConverter : EnumTypeValueConverterBase<MempoolTransactionFailureReason>
{
    private static readonly Dictionary<MempoolTransactionFailureReason, string> _conversion = new()
    {
        { MempoolTransactionFailureReason.DoubleSpend, "DOUBLE_SPEND" },
        { MempoolTransactionFailureReason.Timeout, "TIMEOUT" },
        { MempoolTransactionFailureReason.Unknown, "UNKNOWN" },
    };

    public MempoolTransactionFailureReasonValueConverter()
        : base(_conversion, Invert(_conversion))
    {
    }
}

/// <summary>
///  This stores all the data needed to construct a Gateway.TransactionInfo (alongside the other data in the DB).
/// </summary>
public record GatewayTransactionContents
{
    [JsonPropertyName("actions")]
    public List<Gateway.Action> Actions { get; set; }

    [JsonPropertyName("fee")]
    public string FeePaidSubunits { get; set; }

    [JsonPropertyName("message")]
    public string? MessageHex { get; set; }

    [JsonPropertyName("confirmed_time")]
    public DateTime? ConfirmedTime { get; set; }

    [JsonPropertyName("state_version")]
    public long? LedgerStateVersion { get; set; }
}

/// <summary>
/// A record of transactions submitted recently by this and other nodes.
/// </summary>
[Table("mempool_transactions")]
public class MempoolTransaction
{
    [Key]
    [Column(name: "transaction_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public byte[] TransactionIdentifierHash { get; set; }

    /// <summary>
    /// The payload of the transaction.
    /// </summary>
    [Column("payload")]
    public byte[] Payload { get; set; }

    /// <summary>
    /// True if the transaction was submitted by this gateway. In which case, the gateway is responsible for
    /// monitoring its status in the core API, and resubmitting.
    /// </summary>
    [Column("submitted_by_this_gateway")]
    public bool SubmittedByThisGateway { get; set; }

    /// <summary>
    /// The timestamp when the transaction was initially submitted to a node through this gateway.
    /// </summary>
    [Column("first_submitted_to_gateway_timestamp")]
    public Instant? FirstSubmittedToGatewayTimestamp { get; set; }

    /// <summary>
    /// The timestamp when the transaction was last submitted to a node.
    /// </summary>
    [Column("last_submitted_to_gateway_timestamp")]
    public Instant? LastSubmittedToGatewayTimestamp { get; set; }

    /// <summary>
    /// The timestamp when the transaction was last submitted to a node.
    /// </summary>
    [Column("last_submitted_to_node_timestamp")]
    public Instant? LastSubmittedToNodeTimestamp { get; set; }

    /// <summary>
    /// The timestamp when the transaction was last submitted to a node.
    /// </summary>
    [Column("last_submitted_to_node_name")]
    public string? LastSubmittedToNodeName { get; set; }

    /// <summary>
    /// The timestamp when the transaction was first seen in a node's mempool.
    /// </summary>
    [Column("first_seen_in_mempool_timestamp")]
    public Instant? FirstSeenInMempoolTimestamp { get; set; }

    /// <summary>
    /// The timestamp when the transaction was last seen in a node's mempool.
    /// </summary>
    [Column("last_seen_in_mempool_timestamp")]
    public Instant? LastSeenInMempoolTimestamp { get; set; }

    /// <summary>
    /// The timestamp when the transaction was committed to the DB ledger.
    /// </summary>
    [Column("commit_timestamp")]
    public Instant? CommitTimestamp { get; set; }

    // https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Cpoco
    [Column("transaction_contents", TypeName="jsonb")]
    public GatewayTransactionContents TransactionsContents { get; set; }

    [Column("submission_status")]
    public MempoolTransactionStatus Status { get; set; }

    [Column("submission_failure_reason")]
    public MempoolTransactionFailureReason? SubmissionFailureReason { get; set; }
}
