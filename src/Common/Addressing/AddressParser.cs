namespace Common.Addressing;

public static class AddressParser
{
    public static bool TryParse(AddressHrps hrps, string address, out RadixAddress? radixAddress, out string? errorMessage)
    {
        if (!TryDecode(address, out var bech32Data, out var exception))
        {
            radixAddress = null;
            errorMessage = $"Failed to decode address: {exception!.Message}";
            return false;
        }

        var (addressHrp, addressData, _) = bech32Data!;

        if (addressHrp == hrps.AccountHrp)
        {
            radixAddress = new RadixAddress(RadixAddressType.Account, addressHrp, addressData);
            errorMessage = null;
            return true;
        }

        if (addressHrp.EndsWith(hrps.ResourceHrpSuffix))
        {
            radixAddress = new RadixAddress(RadixAddressType.Resource, addressHrp, addressData);
            errorMessage = null;
            return true;
        }

        if (addressHrp == hrps.NodeHrp)
        {
            radixAddress = new RadixAddress(RadixAddressType.Node, addressHrp, addressData);
            errorMessage = null;
            return true;
        }

        if (addressHrp == hrps.ValidatorHrp)
        {
            radixAddress = new RadixAddress(RadixAddressType.Validator, addressHrp, addressData);
            errorMessage = null;
            return true;
        }

        radixAddress = null;
        errorMessage = $"Address HRP was {addressHrp} but didn't match any known types of address";
        return false;
    }

    private static bool TryDecode(string bechEncoded, out RadixBech32Data? bech32Data, out AddressException? exception)
    {
        try
        {
            bech32Data = RadixBech32.Decode(bechEncoded);
            exception = null;
            return true;
        }
        catch (AddressException ex)
        {
            bech32Data = null;
            exception = ex;
            return false;
        }
    }
}
