using RadixCoreApi.GeneratedClient.Model;
using System.Diagnostics.CodeAnalysis;

namespace DataAggregator.Extensions;

public static class CoreApiModelExtensions
{
    public static bool HasSubstantiveOperations(this OperationGroup operationGroup)
    {
        return operationGroup.Operations.Any(IsNotRoundDataOrValidatorBftData);
    }

    public static bool IsNotRoundDataOrValidatorBftData(this Operation operation)
    {
        // TODO:NG-25 - ensure that ValidatorBFTData is captured at end of epoch
        // If data is null this also returns false.
        return operation.Data?.DataObject is not (RoundData or ValidatorBFTData);
    }

    public static bool IsCreateOf<TDataObject>(this Operation operation, [NotNullWhen(true)] out TDataObject? dataObject)
        where TDataObject : DataObject
    {
        if (operation.Data?.Action == Data.ActionEnum.CREATE &&
            operation.Data.DataObject is TDataObject dataObject2)
        {
            dataObject = dataObject2;
            return true;
        }

        dataObject = null;
        return false;
    }

    public static bool IsDeleteOf<TDataObject>(this Operation operation, [NotNullWhen(true)] out TDataObject? dataObject)
        where TDataObject : DataObject
    {
        if (operation.Data?.Action == Data.ActionEnum.DELETE &&
            operation.Data.DataObject is TDataObject dataObject2)
        {
            dataObject = dataObject2;
            return true;
        }

        dataObject = null;
        return false;
    }
}
