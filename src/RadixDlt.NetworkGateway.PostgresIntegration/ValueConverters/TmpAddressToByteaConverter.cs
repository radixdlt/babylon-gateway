using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;

namespace RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;

internal class TmpAddressToByteaConverter : ValueConverter<TmpAddress, byte[]>
{
    public TmpAddressToByteaConverter()
        : base(v => v.GetBytes(), v => TmpAddress.FromByteArray(v))
    {
    }
}
