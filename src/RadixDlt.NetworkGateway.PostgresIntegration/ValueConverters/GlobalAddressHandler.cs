using Dapper;
using RadixDlt.NetworkGateway.Abstractions;
using System.Data;

namespace RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;

public sealed class GlobalAddressHandler : SqlMapper.TypeHandler<GlobalAddress>
{
    public override void SetValue(IDbDataParameter parameter, GlobalAddress value)
    {
        parameter.Value = value;
    }

    public override GlobalAddress Parse(object value) => (GlobalAddress)(string)value;
}
