using RadixDlt.NetworkGateway.Abstractions.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

[Table("ledger_transaction_events")]
internal class LedgerTransactionEvents
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("state_version")]
    public long StateVersion { get; set; }

    [Column("receipt_event_emitter_entity_ids")]
    public long[] ReceiptEventEmitterEntityIds { get; set; }

    [Column("receipt_event_emitters", TypeName = "jsonb[]")]
    public string[] ReceiptEventEmitters { get; set; }

    [Column("receipt_event_names", TypeName = "text[]")]
    public string[] ReceiptEventNames { get; set; }

    [Column("receipt_event_sbors")]
    public byte[][] ReceiptEventSbors { get; set; }

    [Column("receipt_event_schema_entity_ids")]
    public long[] ReceiptEventSchemaEntityIds { get; set; }

    [Column("receipt_event_schema_hashes")]
    public byte[][] ReceiptEventSchemaHashes { get; set; }

    [Column("receipt_event_type_indexes")]
    public long[] ReceiptEventTypeIndexes { get; set; }

    [Column("receipt_event_sbor_type_kinds")]
    public SborTypeKind[] ReceiptEventSborTypeKinds { get; set; }
}
