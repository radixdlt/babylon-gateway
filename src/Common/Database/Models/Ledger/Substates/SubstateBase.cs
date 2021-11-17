using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Substates;

public enum SubstateState
{
    Up,
    Down,
}

/// <summary>
/// A base class for a stored Substate. It is UP (active) if DownStateVersion is null, else it's DOWN (inactive).
/// </summary>
// OnModelCreating: PK defined on (UpStateVersion, UpOperationGroupIndex, UpOperationIndexInGroup)
// OnModelCreating: Alternate key defined on SubstateIdentifier
public abstract class SubstateBase
{
    [Column(name: "up_state_version")]
    public long UpStateVersion { get; set; }

    [Column(name: "up_operation_group_index")]
    public int UpOperationGroupIndex { get; set; }

    // OnModelCreating: Define relationship to OperationGroup via composite FK (Cascade delete)
    public LedgerOperationGroup UpOperationGroup { get; set; }

    [Column(name: "up_operation_index_in_group")]
    public int UpOperationIndexInGroup { get; set; }

    [Column(name: "down_state_version")]
    [ConcurrencyCheck] // Ensure that the same substate can't be downed by two different state versions somehow
    public long? DownStateVersion { get; set; }

    [Column(name: "down_operation_group_index")]
    public int? DownOperationGroupIndex { get; set; }

    // OnModelCreating: Define relationship to OperationGroup via composite FK (no cascade delete - needs manual clear-up)
    public LedgerOperationGroup? DownOperationGroup { get; set; }

    [Column(name: "down_operation_index_in_group")]
    public int? DownOperationIndexInGroup { get; set; }

    [Column(name: "substate_identifier")]
    public byte[] SubstateIdentifier { get; set; }

    public SubstateState State =>
        (DownOperationGroup != null || DownStateVersion != null) ? SubstateState.Down : SubstateState.Up;

    public bool IsVirtual => IsVirtualIdentifier(SubstateIdentifier);

    /// <summary>
    /// If the substate identifier is longer than 36, the substate has a virtual parent.
    /// The virtual parent gives essentially default values to a non-existing child, and this non-existent "virtual"
    ///  child can be downed without ever formally being upped.
    /// </summary>
    public static bool IsVirtualIdentifier(byte[] identifier)
    {
        return identifier.Length > 36;
    }
}
