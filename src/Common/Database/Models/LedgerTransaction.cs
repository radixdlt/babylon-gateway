using Common.Numerics;
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
    [Column(name: "transaction_index")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long TransactionIndex { get; set; }

    // This is provided to enable a constraint to ensure there are no gaps in the ledger (see OnModelCreating).
    [Column(name: "parent_transaction_index")]
    public long? ParentTransactionIndex { get; set; }

    [ForeignKey("ParentTransactionIndex")]
    public LedgerTransaction? Parent { get; set; }

    [Column(name: "transaction_id")]
    public byte[] TransactionIdentifier { get; set; }

    [ForeignKey("TransactionIdentifier")]
    public RawTransaction? RawTransaction { get; set; }

    [Column(name: "transaction_accumulator")]
    public byte[] TransactionAccumulator { get; set; }

    [Column(name: "state_version")]
    public long ResultantStateVersion { get; set; }

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

    public LedgerTransaction(long transactionIndex, long? parentTransactionIndex, byte[] transactionIdentifier, byte[] transactionAccumulator, long resultantStateVersion, byte[]? message, TokenAmount feePaid, long epoch, int indexInEpoch, bool isEndOfEpoch, DateTime timestamp)
    {
        TransactionIndex = transactionIndex;
        ParentTransactionIndex = parentTransactionIndex;
        TransactionIdentifier = transactionIdentifier;
        TransactionAccumulator = transactionAccumulator;
        ResultantStateVersion = resultantStateVersion;
        Message = message;
        FeePaid = feePaid;
        Epoch = epoch;
        IndexInEpoch = indexInEpoch;
        IsEndOfEpoch = isEndOfEpoch;
        Timestamp = timestamp;
    }
}
