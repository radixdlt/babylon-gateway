using Common.Numerics;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger.Operations;

public abstract class BalanceOperationBase
{
    [Column(name: "state_version")]
    public long ResultantStateVersion { get; set; }

    [Column(name: "operation_group_index")]
    public int OperationGroupIndex { get; set; }

    // OnModelCreating: Define relationship to OperationGroup via composite FK
    public OperationGroup OperationGroup { get; set; }

    [Column(name: "operation_index_in_group")]
    public int OperationIndexInGroup { get; set; }

    [Column(name: "substate_identifier")]
    public byte[] SubstateIdentifier { get; set; }

    [Column(name: "substate_operation")]
    public SubstateOperationType SubstateOperation { get; set; }

    [Column(name: "amount")]
    public TokenAmount AmountDelta { get; set; }
}
