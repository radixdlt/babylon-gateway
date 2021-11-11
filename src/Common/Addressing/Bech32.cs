// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

using System.Text;

namespace Common.Addressing;

/// <summary>
/// <para>A class for encoding and decoding Bech32 (and Bech32M) strings.</para>
///
/// <para>
/// NB: A Bech32 encoded string consists of an ASCII human readable part "HRP", and a data part, and looks like:
/// "{HRP}1{data encoded 5-bits per byte using the bech32 Charset}".
/// </para>
///
/// <para>
/// Important note on decoding the 5-bits-per-byte data:
/// <list type="bullet">
/// <item><description>Bitcoin/Segwit addresses use a data encoding scheme of [1 char - the 5 bit "witness version"][The witness program bytes, converted into 5-bit chunks (padded with 0s if necessary)]</description></item>
/// <item><description>Radix addresses don't have a prefix, and instead just encode [address bytes, converted into 5-bit chunks (padded with 0s if necessary)]</description></item>
/// </list>
/// Note that these are inconsistent in terms of how they are padded. So instead, we just return the raw data as
/// 5-bits-per-byte, and let the decoding be done a level up, making use of the ConvertBits method.
/// </para>
///
/// <para>
/// Standards:
/// <list type="bullet">
///  <item><term>Bech32</term><description> is defined in <a href="https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki">BIP-173</a></description></item>
///  <item><term>Bech32M</term><description> is defined in <a href="https://github.com/bitcoin/bips/blob/master/bip-0350.mediawiki">BIP-350</a></description></item>
/// </list>
/// </para>
/// </summary>
public static class Bech32
{
    public enum Variant
    {
        Bech32,
        Bech32M,
    }

    // The Bech32 character set for encoding
    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

    // Radix does not make use of Bech32M at present
    private const int Bech32ChecksumConst = 1;
    private const int Bech32MChecksumConst = 0x2bc830a3;

    // The Bech32 character set for decoding
    // ReSharper disable once StaticMemberInGenericType
    private static readonly short[] _charsetRev =
    {
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        15, -1, 10, 17, 21, 20, 26, 30,  7,  5, -1, -1, -1, -1, -1, -1,
        -1, 29, -1, 24, 13, 25,  9,  8, 23, -1, 18, 22, 31, 27, 19, -1,
        1,   0,  3, 16, 11, 28, 12, 14,  6,  4,  2, -1, -1, -1, -1, -1,
        -1, 29, -1, 24, 13, 25,  9,  8, 23, -1, 18, 22, 31, 27, 19, -1,
        1,   0,  3, 16, 11, 28, 12, 14,  6,  4,  2, -1, -1, -1, -1, -1,
    };

    /// <summary>
    /// Each Bech32 data character only stores 5 bits, so we need to unpack back to 8bits.
    /// Depending on the Bech32 address, different unpacking schemes are used (see Segwit vs Radix), so this is
    /// left up to the implementer to use.
    /// </summary>
    public static byte[] ConvertBits(ReadOnlySpan<byte> data, int fromBits, int toBits, bool pad)
    {
        var accumulator = 0;
        var readBitsYetToWrite = 0;
        var bitMaskForWriting = (1 << toBits) - 1;
        var result = new List<byte>(64);

        foreach (var readByte in data)
        {
            if ((readByte >> fromBits) > 0)
            {
                throw new AddressException($"Invalid Bech32 string - byte in data has value > 2^{fromBits}");
            }

            accumulator = (accumulator << fromBits) | readByte;
            readBitsYetToWrite += fromBits;
            while (readBitsYetToWrite >= toBits)
            {
                readBitsYetToWrite -= toBits;
                result.Add((byte)((accumulator >> readBitsYetToWrite) & bitMaskForWriting));
            }
        }

        if (pad)
        {
            if (readBitsYetToWrite > 0)
            {
                result.Add((byte)((accumulator << (toBits - readBitsYetToWrite)) & bitMaskForWriting));
            }
        }
        else if (readBitsYetToWrite >= fromBits || (byte)((accumulator << (toBits - readBitsYetToWrite)) & bitMaskForWriting) != 0)
        {
            throw new AddressException("Invalid Bech32 string - bytes did not pad/unpad exactly");
        }

        return result.ToArray();
    }

    public record Bech32RawData(string Hrp, byte[] RawBase32Data, Variant Variant);

    public static string EncodeFromRawData(string hrp, ReadOnlySpan<byte> encodedBase32Data, Variant variant)
    {
        if (hrp.Length is < 1 or > 83)
        {
            throw new ArgumentException("Human-readable part must be between 1 and 83 characters");
        }

        hrp = hrp.ToLowerInvariant();
        var checksumBytes = CreateChecksumBytes(hrp, encodedBase32Data, variant);

        var stringBuilder = new StringBuilder(hrp.Length + 1 + encodedBase32Data.Length + checksumBytes.Length);
        stringBuilder.Append(hrp);
        stringBuilder.Append('1');
        foreach (var bit in encodedBase32Data)
        {
            stringBuilder.Append(Charset[bit]);
        }

        foreach (var bit in checksumBytes)
        {
            stringBuilder.Append(Charset[bit]);
        }

        return stringBuilder.ToString();
    }

    public static Bech32RawData DecodeToRawData(string encoded)
    {
        AssertBech32StringValid(encoded);
        int endOfHrpPosition = encoded.LastIndexOf('1');
        if (endOfHrpPosition < 1)
        {
            throw new AddressException("Bech32 string has no human-readable part");
        }

        int dataPartLength = encoded.Length - 1 - endOfHrpPosition;
        if (dataPartLength < 6)
        {
            throw new AddressException($"Bech32 data part too short: {dataPartLength} < 6");
        }

        Span<byte> encodedDataWithChecksum = new byte[dataPartLength]; // Can't stack alloc as needs to be returned

        for (int i = 0; i < dataPartLength; ++i)
        {
            var c = encoded[i + endOfHrpPosition + 1];
            if (_charsetRev[c] == -1)
            {
                throw new AddressException($"Bech32 string has invalid character {c} in data section at position {i + endOfHrpPosition + 1}");
            }

            encodedDataWithChecksum[i] = (byte)_charsetRev[c];
        }

        string hrp = encoded[..endOfHrpPosition].ToLowerInvariant();

        if (!VerifyChecksum(hrp, encodedDataWithChecksum, out var variant))
        {
            throw new AddressException($"Bech32 address was decoded with an invalid checksum");
        }

        var encodedData = encodedDataWithChecksum[..^6];

        return new Bech32RawData(hrp, encodedData.ToArray(), variant);
    }

    private static int CalculatePolymod(ReadOnlySpan<byte> values)
    {
        int checkSum = 1;
        foreach (var vI in values)
        {
            int c0 = (checkSum >> 25) & 0xff;
            checkSum = ((checkSum & 0x1ffffff) << 5) ^ (vI & 0xff);
            if ((c0 & 1) != 0)
            {
                checkSum ^= 0x3b6a57b2;
            }

            if ((c0 & 2) != 0)
            {
                checkSum ^= 0x26508e6d;
            }

            if ((c0 & 4) != 0)
            {
                checkSum ^= 0x1ea119fa;
            }

            if ((c0 & 8) != 0)
            {
                checkSum ^= 0x3d4233dd;
            }

            if ((c0 & 16) != 0)
            {
                checkSum ^= 0x2a1462b3;
            }
        }

        return checkSum;
    }

    private static byte[] ExpandHrp(string hrp)
    {
        int hrpLength = hrp.Length;
        var expandedHrp = new byte[(hrpLength * 2) + 1];
        for (int i = 0; i < hrpLength; i++)
        {
            var hrpCharacter = hrp[i];
            int characterLimitedToAscii = hrpCharacter & 0x7f;
            expandedHrp[i] = (byte)((characterLimitedToAscii >> 5) & 0x07);
            expandedHrp[i + hrpLength + 1] = (byte)(characterLimitedToAscii & 0x1f);
        }

        expandedHrp[hrpLength] = 0;
        return expandedHrp;
    }

    private static bool VerifyChecksum(string hrp, ReadOnlySpan<byte> encodedDataWithChecksum, out Variant variant)
    {
        var checkSum = CalculatePolymodWith(hrp, encodedDataWithChecksum, 0);
        switch (checkSum)
        {
            case Bech32ChecksumConst:
                variant = Variant.Bech32;
                return true;
            case Bech32MChecksumConst:
                variant = Variant.Bech32M;
                return true;
            default:
                variant = default;
                return false;
        }
    }

    private static byte[] CreateChecksumBytes(string hrp, ReadOnlySpan<byte> encodedData, Variant variant)
    {
        var bech32Checksum = variant switch
        {
            Variant.Bech32 => Bech32ChecksumConst,
            Variant.Bech32M => Bech32MChecksumConst,
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null),
        };
        var mod = CalculatePolymodWith(hrp, encodedData, 6) ^ bech32Checksum;

        byte[] checksumBytes = new byte[6];
        for (int i = 0; i < 6; ++i)
        {
            checksumBytes[i] = (byte)((mod >> (5 * (5 - i))) & 31);
        }

        return checksumBytes;
    }

    private static int CalculatePolymodWith(string hrp, ReadOnlySpan<byte> encodedData, int bonusZeroBytes)
    {
        var hrpExpanded = ExpandHrp(hrp);
        var totalLength = hrpExpanded.Length + encodedData.Length + bonusZeroBytes;
        Span<byte> combinedHrpAndData = totalLength > 256 ? new byte[totalLength] : stackalloc byte[totalLength];
        hrpExpanded.CopyTo(combinedHrpAndData);
        encodedData.CopyTo(combinedHrpAndData[hrpExpanded.Length..]);
        return CalculatePolymod(combinedHrpAndData);
    }

    private static void AssertBech32StringValid(string str)
    {
        switch (str.Length)
        {
            case < 8:
                throw new AddressException($"Bech32 string too short: {str.Length} < 8");
            case > 90:
                throw new AddressException($"Bech32 string too long: {str.Length} > 90");
        }

        var isLowerCase = false;
        var isUpperCase = false;
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (c < 33 || c > 126)
            {
                throw new AddressException($"Bech32 string has invalid character {c} at position {i}");
            }

            switch (c)
            {
                case >= 'a' and <= 'z' when isUpperCase:
                    throw new AddressException($"Bech32 string has invalid casing. It was upper case until {c} at position {i}");
                case >= 'a' and <= 'z':
                    isLowerCase = true;
                    break;
                case >= 'A' and <= 'Z' when isLowerCase:
                    throw new AddressException($"Bech32 string has invalid casing. It was lower case until {c} at position {i}");
                case >= 'A' and <= 'Z':
                    isUpperCase = true;
                    break;
            }
        }
    }
}
