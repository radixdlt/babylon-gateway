﻿using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.Commons.Extensions;
using System;
using System.Text;

namespace RadixDlt.NetworkGateway.IntegrationTests.Utilities;

public static class AddressHelper
{
    public static string AddressToHex(string address)
    {
        return RadixBech32.Decode(address).Data.ToHex();
    }

    public static string AddressFromHex(string addressHex, string hrp)
    {
        return RadixBech32.Bech32EncodeRawAddressData(
            hrp,
            Convert.FromHexString(addressHex));
    }

    public static string GenerateRandomAddress(string hrpSuffix)
    {
        var res = new Random();

        // String of alphabets
        var str = "abcdefghijklmnopqrstuvwxyz";
        var size = (68 - hrpSuffix.Length) / 2;

        var addressData = "1";

        for (var i = 0; i < size; i++)
        {
            var x = res.Next(str.Length);
            addressData = addressData + str[x];
        }

        return RadixBech32.Bech32EncodeRawAddressData(hrpSuffix, Encoding.Default.GetBytes(addressData));
    }

    public static string GenerateRandomPublicKey()
    {
        var publicKey = new byte[33];
        new Random().NextBytes(publicKey);

        return Convert.ToHexString(publicKey).ToLower();
    }
}
