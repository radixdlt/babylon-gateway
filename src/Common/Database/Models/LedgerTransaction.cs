using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models;

/// <summary>
/// A transaction committed onto the radix ledger.
/// This table forms a shell, to which other properties are connected.
/// </summary>
[Table("ledger_transactions")]
public class LedgerTransaction
{
    [Key]
    [Column(name: "state_version")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long StateVersion { get; set; }

    // This is provided to enable a constraint to ensure there are no gaps in the StateVersion (see OnModelCreating).
    [Column(name: "parent_state_version")]
    public long? ParentStateVersion { get; set; }

    [ForeignKey("ParentStateVersion")]
    public LedgerTransaction Parent { get; set; }

    [Column(name: "transaction_id")]
    public byte[] TransactionIdentifier { get; set; }

    [ForeignKey("TransactionId")]
    public RawTransaction RawTransaction { get; set; }

    [Column(name: "transaction_accumulator")]
    public byte[] TransactionAccumulator { get; set; }

    [Column(name: "message")]
    public string? Message { get; set; }

    [Column(name: "fee_paid")]
    [Precision(1000, 18)]
    public TokenAmount FeePaid { get; set; }

    [Column(name: "epoch")]
    public long Epoch { get; set; }

    [Column(name: "index_in_epoch")]
    public int IndexInEpoch { get; set; }

    [Column(name: "is_end_of_epoch")]
    public bool IsEndOfEpoch { get; set; }

    [Column(name: "timestamp")]
    public DateTime Timestamp { get; set; }

    public LedgerTransaction(long stateVersion, long? parentStateVersion, LedgerTransaction parent, byte[] transactionIdentifier, RawTransaction rawTransaction, byte[] transactionAccumulator, string? message, TokenAmount feePaid, long epoch, int indexInEpoch, bool isEndOfEpoch, DateTime timestamp)
    {
        StateVersion = stateVersion;
        ParentStateVersion = parentStateVersion;
        Parent = parent;
        TransactionIdentifier = transactionIdentifier;
        RawTransaction = rawTransaction;
        TransactionAccumulator = transactionAccumulator;
        Message = message;
        FeePaid = feePaid;
        Epoch = epoch;
        IndexInEpoch = indexInEpoch;
        IsEndOfEpoch = isEndOfEpoch;
        Timestamp = timestamp;
    }
}
