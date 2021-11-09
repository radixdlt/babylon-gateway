using Common.Addressing;
using Xunit;

namespace Tests.MiniUnitTests.Common.Addressing;

public class RadixBech32Tests
{
    [Theory]
    [InlineData("rdx1qspc9xtaqmquy88jvvuadjleudnsk0tt8dlhuwmcfv4zlqlya8ny3lc5jarf6")] // Radix Mainnet Wallet
    [InlineData("rv1qf2x63qx4jdaxj83kkw2yytehvvmu6r2xll5gcp6c9rancmrfsgfwttnczx")] // Radix Validator
    [InlineData("xrd_rr1qy5wfsfh")] // Radix Token definition Address (XRD)
    [InlineData("usdc_rb1qvqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq6gwwwd")] // Radix Test Token definition Address
    [InlineData("rn1qwa45jjf40vuf9xlq85xym86yegnm95h3g8d4lul5hskrws9gm6ykyx7hsf")] // Radix Node Address
    public void WhenGiven_EncodedStringWithValidRadixAddress_DecodeAndReencodeIsIdentity(string encodedString)
    {
        var decodedData = RadixBech32.Decode(encodedString);

        var reEncodedString = RadixBech32.Encode(decodedData.Hrp, decodedData.AddressData, decodedData.Variant);
        Assert.Equal(encodedString.ToLowerInvariant(), reEncodedString);
    }

    [Theory]
    [InlineData("rdx1qspc9xtaqmquy88jvvuadjleudnsk0tt8dlhuwmcfv4zlqlya8ny3lc5jarf6", "040382997D06C1C21CF26339D6CBF9E3670B3D6B3B7F7E3B784B2A2F83E4E9E648FF")] // Radix Mainnet Wallet
    [InlineData("rv1qf2x63qx4jdaxj83kkw2yytehvvmu6r2xll5gcp6c9rancmrfsgfwttnczx", "02546D4406AC9BD348F1B59CA21179BB19BE686A37FF44603AC147D9E3634C1097")] // Radix Validator
    [InlineData("xrd_rr1qy5wfsfh", "01")] // Radix Token definition Address (XRD)
    [InlineData("usdc_rb1qvqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq6gwwwd", "030000000000000000000000000000000000000000000000000000")] // Radix Test Token definition Address
    [InlineData("rn1qwa45jjf40vuf9xlq85xym86yegnm95h3g8d4lul5hskrws9gm6ykyx7hsf", "03BB5A4A49ABD9C494DF01E8626CFA26513D96978A0EDAFF9FA5E161BA0546F44B")] // Radix Node Address
    public void WhenGiven_EncodedStringWithValidRadixAddress_DecodeGivesAddress(string encodedString, string expectedAddress)
    {
        var decodedData = RadixBech32.Decode(encodedString);
        Assert.Equal(expectedAddress, Convert.ToHexString(decodedData.AddressData));
    }

    public static IEnumerable<object[]> Invalid_Bech32Strings => new List<object[]>
    {
        new object[] { (char)0x20 + "1nwldj5" }, // HRP character out of range
        new object[] { (char)0x7F + "1axkwrx" }, // HRP character out of range
        new object[] { (char)0x80 + "1eym55h" }, // HRP character out of range
        new object[] { "de1lg7wt" + (char)0xFF }, // Invalid character in checksum
        new object[] { "an84characterslonghumanreadablepartthatcontainsthenumber1andtheexcludedcharactersbio1569pvx" }, // HRP character out of range
        new object[] { "pzry9x0s0muk" }, // No separator character
        new object[] { "1pzry9x0s0muk" }, // Empty HRP
        new object[] { "x1b4n0q5v" }, // Invalid data character
        new object[] { "li1dgmt3" }, // Too short checksum
        new object[] { "A1G7SGD8" }, // checksum calculated with uppercase form of HRP
        new object[] { "10a06t8" }, // empty HRP
        new object[] { "vb1qvx0emaq0tua6md7wu9c047mm5krrwnlfl8c7ws3jm2s9uf4vxcyvrwrazz" }, // Bad checksum
    };

    [Theory]
    [MemberData(nameof(Invalid_Bech32Strings))]
    public void WhenGiven_InvalidEncodedString_DecodeThrows(string encodedString)
    {
        Assert.Throws<AddressException>(() => Bech32.DecodeToRawData(encodedString));
    }
}
