using Common.Numerics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Common.Database.ValueConverters;

public class TokenAmountToStringValueConverter : ValueConverter<TokenAmount, string>
{
    public TokenAmountToStringValueConverter()
        : base(v => v.ToPostgresDecimal(), v => TokenAmount.FromString(v))
    {
    }
}
