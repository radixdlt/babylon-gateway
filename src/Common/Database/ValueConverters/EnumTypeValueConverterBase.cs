using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Common.Database.ValueConverters;

public class EnumTypeValueConverterBase<TEnum> : ValueConverter<TEnum, string>
    where TEnum : notnull
{
    public EnumTypeValueConverterBase(IReadOnlyDictionary<TEnum, string> conversion, IReadOnlyDictionary<string, TEnum> inverseConversion)
        : base(
            value => conversion[value],
            value => inverseConversion[value]
        )
    {
    }

    protected static Dictionary<TOut, TIn> Invert<TIn, TOut>(IReadOnlyDictionary<TIn, TOut> conversion)
        where TIn : notnull
        where TOut : notnull
    {
        return conversion.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }
}
