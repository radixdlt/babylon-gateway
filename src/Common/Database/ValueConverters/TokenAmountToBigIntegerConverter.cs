using Common.Numerics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Numerics;

namespace Common.Database.ValueConverters;

public class TokenAmountToBigIntegerConverter : ValueConverter<TokenAmount, BigInteger>
{
    public TokenAmountToBigIntegerConverter()
        : base(v => v.GetSubUnits(), v => TokenAmount.FromSubUnits(v))
    {
    }
}
