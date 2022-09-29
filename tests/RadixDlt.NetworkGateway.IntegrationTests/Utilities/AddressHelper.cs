using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.Commons.Extensions;
using System;
using System.Text;

namespace RadixDlt.NetworkGateway.IntegrationTests.Utilities;

public static class AddressHelper
{
    public static string AddressToHex(string address)
    {
        // return Convert.ToHexString(Encoding.UTF8.GetBytes(address));
        return RadixBech32.Decode(address).Data.ToHex();
    }

    public static string AddressFromHex(string addressHex)
    {
        return Encoding.Default.GetString(Convert.FromHexString(addressHex));
    }
}
