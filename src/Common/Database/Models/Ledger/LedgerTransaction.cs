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

using Common.Numerics;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger;

/// <summary>
/// A transaction committed onto the radix ledger.
/// This table forms a shell, to which other properties are connected.
/// </summary>
[Table("ledger_transactions")]
// OnModelCreating: We also define an index on Timestamp.
// OnModelCreating: We also define a composite index on (Epoch, EndOfView [Not Null]) which includes timestamp - to easily query when views happened.
public class LedgerTransaction
{
    public LedgerTransaction(long resultantStateVersion, long? parentStateVersion, byte[] transactionIdentifierHash, byte[] transactionAccumulator, byte[]? message, TokenAmount feePaid, byte[]? signedBy, long epoch, long indexInEpoch, bool isOnlyRoundChange, bool isEndOfEpoch, DateTime timestamp, long? endOfEpochRound)
    {
        ResultantStateVersion = resultantStateVersion;
        ParentStateVersion = parentStateVersion;
        TransactionIdentifierHash = transactionIdentifierHash;
        TransactionAccumulator = transactionAccumulator;
        Message = message;
        FeePaid = feePaid;
        SignedBy = signedBy;
        Epoch = epoch;
        IndexInEpoch = indexInEpoch;
        IsOnlyRoundChange = isOnlyRoundChange;
        IsEndOfEpoch = isEndOfEpoch;
        Timestamp = timestamp;
        EndOfEpochRound = endOfEpochRound;
    }

    private LedgerTransaction()
    {
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column(name: "state_version")]
    public long ResultantStateVersion { get; set; }

    [Column(name: "parent_state_version")]
    // OnModelCreating: This is provided to enable a constraint to ensure there are no gaps in the ledger
    public long? ParentStateVersion { get; set; }

    [ForeignKey(nameof(ParentStateVersion))]
    public LedgerTransaction? Parent { get; set; }

    [Column(name: "transaction_id")]
    // OnModelCreating: Also defined as an alternate key
    public byte[] TransactionIdentifierHash { get; set; }

    [ForeignKey(nameof(TransactionIdentifierHash))]
    public RawTransaction? RawTransaction { get; set; }

    [Column(name: "transaction_accumulator")]
    // OnModelCreating: Also defined as an alternate key
    public byte[] TransactionAccumulator { get; set; }

    [Column(name: "message")]
    public byte[]? Message { get; set; }

    [Column("fee_paid")]
    public TokenAmount FeePaid { get; set; }

    [Column("signed_by")]
    public byte[]? SignedBy { get; set; }

    [Column(name: "epoch")]
    public long Epoch { get; set; }

    [Column(name: "index_in_epoch")]
    public long IndexInEpoch { get; set; }

    /// <summary>
    /// For now, Round/View changes happen in their own transaction (or along with epoch changes).
    /// They are system-generated transactions with just a RoundData and ValidatorBftData update.
    /// At the moment IsOnlyRoundChange is equivalent to (FeePaid = 0 AND NOT IsEpochChange)
    /// But in case this changes, let's calculate whether a transaction only contains RoundData and ValidatorBftData
    /// and store this in the database.
    /// </summary>
    [Column("is_only_round_change")]
    public bool IsOnlyRoundChange { get; set; }

    [Column(name: "is_end_of_epoch")]
    public bool IsEndOfEpoch { get; set; }

    [Column(name: "timestamp")]
    public DateTime Timestamp { get; set; }

    [Column(name: "end_of_round")]
    public long? EndOfEpochRound { get; set; }

    public bool IsUserTransaction => FeePaid.IsPositive();

    public bool IsSystemTransaction => FeePaid.IsZero();
}
