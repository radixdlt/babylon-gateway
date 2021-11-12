using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

/// <summary>
/// A base class for a stored Substate. It is UP (active) if DownStateVersion is null, else it's DOWN (inactive).
/// </summary>
public abstract class SubstateBase
{
    [Column(name: "up_state_version")]
    public long UpStateVersion { get; set; }

    [Column(name: "up_operation_group_index")]
    public int UpOperationGroupIndex { get; set; }

    // OnModelCreating: Define relationship to OperationGroup via composite FK (Cascade delete)
    public OperationGroup UpOperationGroup { get; set; }

    [Column(name: "up_operation_index_in_group")]
    public int UpOperationIndexInGroup { get; set; }

    [Column(name: "down_state_version")]
    [ConcurrencyCheck]
    public long? DownStateVersion { get; set; }

    [Column(name: "down_operation_group_index")]
    public int? DownOperationGroupIndex { get; set; }

    // OnModelCreating: Define relationship to OperationGroup via composite FK (no cascade delete - needs manual clear-up)
    public OperationGroup? DownOperationGroup { get; set; }

    [Column(name: "down_operation_index_in_group")]
    public int? DownOperationIndexInGroup { get; set; }

    [Column(name: "substate_identifier")]
    public byte[] SubstateIdentifier { get; set; }
}
