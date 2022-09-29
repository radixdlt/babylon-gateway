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

    public static string GenerateRandomAddress(string hrpSuffix)
    {
        Random res = new Random();

        // String of alphabets
        string str = "abcdefghijklmnopqrstuvwxyz";
        int size = (68 - hrpSuffix.Length) / 2;

        string addressData = "1";

        for (int i = 0; i < size; i++)
        {
            int x = res.Next(str.Length);
            addressData = addressData + str[x];
        }

        return RadixBech32.Bech32EncodeRawAddressData(hrpSuffix, Encoding.Default.GetBytes(addressData));
    }
}
