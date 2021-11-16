using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.Extensions;

public record OperationGroupWithIndex(OperationGroup OperationGroup, int Index);
public record OperationWithIndex(Operation Operation, int Index);

public static class CoreApiModelExtensions
{
    public static bool HasSubstantiveOperations(this CommittedTransaction transaction)
    {
        return transaction.OperationGroups.Any(HasSubstantiveOperations);
    }

    public static IEnumerable<OperationGroupWithIndex> SubstantiveOperationGroups(this CommittedTransaction transaction)
    {
        return transaction.OperationGroups
            .Select((group, index) => new OperationGroupWithIndex(group, index))
            .Where(x => x.OperationGroup.HasSubstantiveOperations());
    }

    public static bool HasSubstantiveOperations(this OperationGroup operationGroup)
    {
        return operationGroup.Operations.Any(IsSubstantive);
    }

    public static IEnumerable<OperationWithIndex> SubstantiveOperations(this OperationGroup operationGroup)
    {
        return operationGroup.Operations
            .Select((op, index) => new OperationWithIndex(op, index))
            .Where(x => IsSubstantive(x.Operation));
    }

    private static bool IsSubstantive(this Operation operation)
    {
        return operation.Data?.DataObject == null
            || operation.Data.DataObject is RoundData or EpochData;
    }
}
