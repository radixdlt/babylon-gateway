using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger;

/// <summary>
/// A transaction committed onto the radix ledger.
/// This table forms a shell, to which other properties are connected.
/// </summary>
[Table("ledger_transactions")]
[IndexAttribute(nameof(Timestamp))]
// OnModelCreating: We also define a composite index on (Epoch, EndOfView [Not Null])
public class LedgerTransaction
{
    public LedgerTransaction(long resultantStateVersion, long? parentStateVersion, byte[] transactionIdentifierHash, byte[] transactionAccumulator, byte[]? message, TokenAmount feePaid, long epoch, int indexInEpoch, bool isEndOfEpoch, DateTime timestamp, int? endOfEpochRound)
    {
        ResultantStateVersion = resultantStateVersion;
        ParentStateVersion = parentStateVersion;
        TransactionIdentifierHash = transactionIdentifierHash;
        TransactionAccumulator = transactionAccumulator;
        Message = message;
        FeePaid = feePaid;
        Epoch = epoch;
        IndexInEpoch = indexInEpoch;
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

    [Column(name: "epoch")]
    public long Epoch { get; set; }

    [Column(name: "index_in_epoch")]
    public int IndexInEpoch { get; set; }

    [Column(name: "is_end_of_epoch")]
    public bool IsEndOfEpoch { get; set; }

    [Column(name: "timestamp")]
    public DateTime Timestamp { get; set; }

    [Column(name: "end_of_round")]
    public int? EndOfEpochRound { get; set; }
}
