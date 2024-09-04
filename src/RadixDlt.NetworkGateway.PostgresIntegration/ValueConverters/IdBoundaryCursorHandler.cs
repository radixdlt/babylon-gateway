using Dapper;
using RadixDlt.NetworkGateway.PostgresIntegration.Models.CustomTypes;
using System;
using System.Data;

namespace RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;

public sealed class IdBoundaryCursorHandler : SqlMapper.TypeHandler<IdBoundaryCursor>
{
    public override void SetValue(IDbDataParameter parameter, IdBoundaryCursor value)
    {
        throw new NotImplementedException();
    }

    public override IdBoundaryCursor Parse(object value)
    {
        if (value is object[] cursor && cursor[0] is long && cursor[1] is long)
        {
            return new IdBoundaryCursor((long)cursor[0], (long)cursor[1]);
        }

        throw new ArgumentException($"Not supported value type: {value.GetType()}");
    }
}
