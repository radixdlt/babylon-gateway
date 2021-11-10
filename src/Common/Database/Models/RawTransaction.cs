using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models;

/// <summary>
/// The raw transaction details and payload.
/// </summary>
[Table("raw_transactions")]
public class RawTransaction
{
    [Key]
    [Column(name: "transaction_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public byte[] TransactionIdentifierHash { get; set; }

    /// <summary>
    /// The timestamp when the transaction was committed through this gateway.
    ///
    /// We may not have this timestamp if the transaction wasn't submitted through this gateway,
    /// and was first seen in the mempool or on-ledger.
    /// </summary>
    [Column("submitted_timestamp")]
    public DateTime? SubmittedTimestamp { get; set; }

    /// <summary>
    /// The payload of the transaction.
    ///
    /// We may not have this payload if we have just seen the transaction in the mempool list.
    /// </summary>
    [Column("payload")]
    public byte[]? Payload { get; set; }

    public RawTransaction(byte[] transactionIdentifierHash, DateTime? submittedTimestamp = null, byte[]? payload = null)
    {
        TransactionIdentifierHash = transactionIdentifierHash;
        SubmittedTimestamp = submittedTimestamp;
        Payload = payload;
    }

#pragma warning disable CS8618

    // Empty constructor created for use with Upsert library
    public RawTransaction()
    {
    }

#pragma warning restore CS8618
}
