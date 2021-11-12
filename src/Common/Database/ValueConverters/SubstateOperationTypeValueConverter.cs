using Common.Database.Models.Ledger.Operations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Common.Database.ValueConverters;

public class SubstateOperationTypeValueConverter : ValueConverter<SubstateOperationType, string>
{
    public SubstateOperationTypeValueConverter()
        : base(
            so => ToDatabaseString(so),
            so => FromDatabaseString(so)
        )
    {
    }

    private static string ToDatabaseString(SubstateOperationType substateOperationType)
    {
        return substateOperationType switch
        {
            SubstateOperationType.Up => "UP",
            SubstateOperationType.Down => "DOWN",
            _ => throw new ArgumentOutOfRangeException(nameof(substateOperationType), substateOperationType, null),
        };
    }

    private static SubstateOperationType FromDatabaseString(string databaseString)
    {
        return databaseString switch
        {
            "UP" => SubstateOperationType.Up,
            "DOWN" => SubstateOperationType.Down,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseString), databaseString, null),
        };
    }
}
