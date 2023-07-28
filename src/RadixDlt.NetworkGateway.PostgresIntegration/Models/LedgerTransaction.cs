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
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

/// <summary>
/// A transaction committed onto the radix ledger.
/// This table forms a shell, to which other properties are connected.
/// </summary>
[Table("ledger_transactions")]
internal abstract class LedgerTransaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("state_version")]
    public long StateVersion { get; set; }

    [Column("message")]
    public byte[]? Message { get; set; }

    [Column("epoch")]
    public long Epoch { get; set; }

    [Column("round_in_epoch")]
    public long RoundInEpoch { get; set; }

    [Column("index_in_epoch")]
    public long IndexInEpoch { get; set; }

    [Column("index_in_round")]
    public long IndexInRound { get; set; }

    [Column("is_end_of_epoch")]
    public bool IsEndOfEpoch { get; set; }

    [Column("fee_paid")]
    public TokenAmount? FeePaid { get; set; }

    [Column("tip_paid")]
    public TokenAmount? TipPaid { get; set; }

    [Column("affected_global_entities")]
    public long[] AffectedGlobalEntities { get; set; }

    /// <summary>
    /// The round timestamp of a round where vertex V was voted on is derived as the median of the timestamp of the
    /// votes on the vertex's QC to its parent vertex. These votes come from a subset of validators performing
    /// consensus. As a consequence of this, the round timestamp is not guaranteed to be increasing.
    /// </summary>
    [Column("round_timestamp")]
    public DateTime RoundTimestamp { get; set; }

    /// <summary>
    /// The time of the DataAggregator server when the LedgerTransaction was added to the service.
    /// </summary>
    [Column("created_timestamp")]
    public DateTime CreatedTimestamp { get; set; }

    /// <summary>
    /// This timestamp attempts to be "sensible" - ie increasing and semi-resistant to network time attacks.
    /// It calculates itself by clamping RoundTimestamp between the previous NormalizedTimestamp and CreatedTimestamp.
    /// Thus it ensures that NormalizedTimestamp is non-decreasing, and not after the ingest time.
    /// </summary>
    [Column("normalized_round_timestamp")]
    public DateTime NormalizedRoundTimestamp { get; set; }

    /// <summary>
    /// The raw payload of the transaction.
    /// </summary>
    [Column("raw_payload")]
    public byte[] RawPayload { get; set; }

    public TransactionReceipt EngineReceipt { get; set; }
}

[Owned]
internal class TransactionReceipt
{
    [Column("receipt_status")]
    public LedgerTransactionStatus Status { get; set; }

    [Column("receipt_fee_summary", TypeName = "jsonb")]
    public string FeeSummary { get; set; }

    [Column("receipt_state_updates", TypeName = "jsonb")]
    public string StateUpdates { get; set; }

    [Column("receipt_next_epoch", TypeName = "jsonb")]
    public string? NextEpoch { get; set; }

    [Column("receipt_output", TypeName = "jsonb")]
    public string? Output { get; set; }

    [Column("receipt_error_message")]
    public string? ErrorMessage { get; set; }

    [Column("receipt_events_sbor")]
    public byte[][] EventsSbor { get; set; }

    [Column("receipt_events_schema_hash")]
    public byte[][] EventsSchemaHash { get; set; }

    [Column("receipt_events_type_index")]
    public int[] EventsTypeIndex { get; set; }

    [Column("receipt_events_sbor_type_kind")]
    public SborTypeKind[] EventsSborTypeKind { get; set; }
}

internal class GenesisLedgerTransaction : LedgerTransaction
{
}

internal class UserLedgerTransaction : LedgerTransaction
{
    /// <summary>
    /// The transaction payload hash, also known as the notarized transaction hash (for user transactions).
    /// This shouldn't be used for user transaction tracking, because it could be mutated in transit.
    /// The intent hash should be used for tracking of user transactions.
    /// </summary>
    [Column("payload_hash")]
    public byte[] PayloadHash { get; set; }

    /// <summary>
    /// The transaction intent hash. The engine ensures two transactions with the same intent hash cannot be committed.
    /// </summary>
    [Column("intent_hash")]
    public byte[] IntentHash { get; set; }

    /// <summary>
    /// The hash of the signed transaction, which is what the notary signs.
    /// </summary>
    [Column("signed_intent_hash")]
    public byte[] SignedIntentHash { get; set; }
}

internal class RoundUpdateLedgerTransaction : LedgerTransaction
{
}
