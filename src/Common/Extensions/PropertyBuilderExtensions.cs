using Common.Numerics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Common.Extensions;

public static class PropertBuilderExtensions
{
    public static PropertyBuilder<TProperty> AsTokenAmount<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        var tokenAmountConverter = new ValueConverter<TokenAmount, string>(
            v => v.ToPostgresDecimal(),
            v => TokenAmount.FromString(v)
        );
        builder.HasConversion(tokenAmountConverter);

        // TODO - Change to save as decimal when the following issue is fixed:
        // https://github.com/npgsql/npgsql/issues/3665#issuecomment-963371551

        /*
        var tokenAmountConverter = new ValueConverter<TokenAmount, BigInteger>(
            v => v.GetSubUnits(),
            v => TokenAmount.FromSubUnits(v)
        );
        builder
            .HasConversion(tokenAmountConverter)
            .HasColumnType("numeric")
            .HasPrecision(1000);
        */

        return builder;
    }
}
