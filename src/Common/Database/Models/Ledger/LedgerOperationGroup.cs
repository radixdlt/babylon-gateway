using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database.Models.Ledger;

/// <summary>
/// A notable operation group under a transaction.
/// </summary>
// OnModelCreating: Has composite key (state_version, operation_group_index)
[Table("operation_groups")]
public class LedgerOperationGroup
{
    public LedgerOperationGroup(long resultantStateVersion, int operationGroupIndex, InferredAction? inferredAction)
    {
        ResultantStateVersion = resultantStateVersion;
        OperationGroupIndex = operationGroupIndex;
        InferredAction = inferredAction;
    }

    private LedgerOperationGroup()
    {
    }

    [Column(name: "state_version")]
    public long ResultantStateVersion { get; set; }

    [ForeignKey(nameof(ResultantStateVersion))]
    public LedgerTransaction LedgerTransaction { get; set; }

    [Column(name: "operation_group_index")]
    public int OperationGroupIndex { get; set; }

    // See [Owned] InferredAction below.
    public InferredAction? InferredAction { get; set; }
}

[Owned]
public class InferredAction
{
    public InferredAction(string type, string? fromAddress, string? toAddress, TokenAmount? amount, string? resourceIdentifier)
    {
        Type = type;
        FromAddress = fromAddress;
        ToAddress = toAddress;
        Amount = amount;
        ResourceIdentifier = resourceIdentifier;
    }

    private InferredAction()
    {
    }

    [Column(name: "inferred_action_type")]
    public string Type { get; set; }

    [Column(name: "inferred_action_from")]
    public string? FromAddress { get; set; }

    [Column(name: "inferred_action_to")]
    public string? ToAddress { get; set; }

    [Column(name: "inferred_action_amount")]
    public TokenAmount? Amount { get; set; }

    [Column(name: "inferred_action_rri")]
    public string? ResourceIdentifier { get; set; }
}
