// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Common.Addressing;

public record RadixBech32Data(string Hrp, byte[] AddressData, Bech32.Variant Variant);

public static class RadixBech32
{
    public static string Encode(string hrp, byte[] addressData, Bech32.Variant variant)
    {
        return Bech32.EncodeFromRawData(hrp, EncodeAddressDataInBase32(addressData), variant);
    }

    public static RadixBech32Data Decode(string encoded)
    {
        var (hrp, rawBase32Data, variant) = Bech32.DecodeToRawData(encoded);
        return new RadixBech32Data(hrp, DecodeBase32IntoAddressData(rawBase32Data), variant);
    }

    /// <summary>
    /// Defines how the 5-bit per byte (base32 per byte) data should be decoded.
    /// This will likely making use of ConvertBits to unpack to 8 bits per byte.
    /// </summary>
    private static byte[] DecodeBase32IntoAddressData(ReadOnlySpan<byte> base32EncodedData)
    {
        return Bech32.ConvertBits(base32EncodedData, 5, 8, false);
    }

    /// <summary>
    /// Defines how the data should be encoded as 5-bits per byte (base32 per byte) for the Bech32 data part.
    /// This will likely making use of ConvertBits to convert from 8 bits per byte to 5 bits per byte.
    /// </summary>
    private static ReadOnlySpan<byte> EncodeAddressDataInBase32(byte[] dataToEncode)
    {
        return Bech32.ConvertBits(dataToEncode, 8, 5, true);
    }
}
