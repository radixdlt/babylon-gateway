using Dapper;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using System;
using System.Data;

namespace RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;

public sealed class TokenAmountHandler : SqlMapper.TypeHandler<TokenAmount>
{
    public override void SetValue(IDbDataParameter parameter, TokenAmount value)
    {
        throw new NotImplementedException();
    }

    public override TokenAmount Parse(object value)
    {
        if (value is string v)
        {
            return TokenAmount.FromSubUnitsString(v);
        }

        throw new ArgumentException($"Not supported value type: {value.GetType()}");
    }
}
