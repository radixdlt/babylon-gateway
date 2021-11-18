using Common.Numerics;
using System.Numerics;
using System.Text;
using Xunit;

namespace Tests.MiniUnitTests.Common.Numerics;

public class TokenAmountTests
{
    [Theory]
    [InlineData(false, "123", "", "123")]
    [InlineData(false, "", "12", "0.12")]
    [InlineData(false, "000", "13444", "0.13444")]
    [InlineData(false, "0", "0", "0")]
    [InlineData(false, "11", "000000000000000009", "11.000000000000000009")] // Does hold 18 dp
    [InlineData(false, "11", "0000000000000000009", "11")] // Only holds 18 dp (truncating)
    [InlineData(false, "123.34", "", "NaN")]
    [InlineData(false, "ab", "cd", "NaN")]
    [InlineData(false, "ab", "0", "NaN")]
    [InlineData(true, "1", "0", "-1")]
    [InlineData(true, "-1", "0", "NaN")] // Invalid call
    [InlineData(true, "-1", "-1", "NaN")] // Invalid call
    [InlineData(true, "", "-1", "NaN")] // Invalid call
    [InlineData(false, "12", "-3", "NaN")] // Invalid call
    public void Create_TokenAmountFromStringParts_ReadsCorrectly(bool isNegative, string wholePart, string fractionalPart, string expected)
    {
        Assert.Equal(expected, TokenAmount.FromStringParts(isNegative, wholePart, fractionalPart).ToString());
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("0.12", "0.12")]
    [InlineData("000.13444", "0.13444")]
    [InlineData("0.0", "0")]
    [InlineData("11.000000000000000009", "11.000000000000000009")] // Does hold 18 dp
    [InlineData("11.0000000000000000019", "11.000000000000000001")] // Does not hold 19 dp, truncates
    [InlineData("123.34", "123.34")]
    [InlineData("NaN", "NaN")]
    [InlineData("Infinity", "NaN")]
    [InlineData("-Infinity", "NaN")]
    [InlineData("-123", "-123")]
    [InlineData("-123.24", "-123.24")]
    [InlineData("-11.000000000000000009", "-11.000000000000000009")] // Does hold 18 dp
    [InlineData("-11.0000000000000000019", "-11.000000000000000001")] // Does not hold 19 dp, truncates
    [InlineData("-0.0000000000000000019", "-0.000000000000000001")] // Does not hold 19 dp, truncates
    [InlineData("-0.0000000000000000007", "0")] // Does not hold 19 dp, truncates and removes decimal point
    public void Create_FromString_ToStringReadsCorrectly(string decimalString, string expected)
    {
        Assert.Equal(expected, TokenAmount.FromDecimalString(decimalString).ToString());
    }

    [Theory]
    [InlineData("0", "0.000000000000000000")]
    [InlineData("1", "0.000000000000000001")]
    [InlineData("123", "0.000000000000000123")]
    [InlineData("12345678900123456789", "12.345678900123456789")]
    [InlineData("-123", "-0.000000000000000123")]
    [InlineData("-123456789001234567", "-0.123456789001234567")]
    [InlineData("-1234567890012345678", "-1.234567890012345678")]
    public void Create_FromSubUnitsString_ReadsCorrectlyAtFullPrecision(string subUnitsStr, string expected)
    {
        Assert.Equal(expected, TokenAmount.FromSubUnitsString(subUnitsStr).ToStringFullPrecision());
    }

    [Fact]
    public void Equate_NaNWithNaN_ReturnsTrue()
    {
        // This agrees with https://docs.microsoft.com/en-us/dotnet/api/system.double.nan?view=net-5.0
        // (it's just the operators such as == that return false)
        Assert.Equal(TokenAmount.NaN, TokenAmount.NaN);
    }

    [Fact]
    public void Equate_SameTokenAmount_ReturnsTrue()
    {
        Assert.Equal(TokenAmount.FromDecimalString("123"), TokenAmount.FromDecimalString("123"));
    }

    [Fact]
    public void Equate_DifferentTokenAmount_ReturnsFalse()
    {
        Assert.NotEqual(TokenAmount.FromDecimalString("123"), TokenAmount.FromDecimalString("1234"));
    }

    [Fact]
    public void Equate_NaNWithNoneNaN_ReturnsFalse()
    {
        Assert.NotEqual(TokenAmount.FromDecimalString("1"), TokenAmount.NaN);
    }

    [Fact]
    public void Equate_SameAmountCreatedTwoDifferentWays_ReturnsTrue()
    {
        Assert.Equal(TokenAmount.FromDecimalString("123"), TokenAmount.FromSubUnitsString("123000000000000000000"));
    }

    [Fact]
    public void GivenANumberTooLargeForPostgres_WhenConvertedToPostgresDecimal_ReturnsNaN()
    {
        var tokenAmount = TokenAmount.FromSubUnits(BigInteger.Pow(10, 1000));
        Assert.Equal("NaN", tokenAmount.ToPostgresDecimal());
    }

    [Fact]
    public void GivenANumberInsidePostgresLimit_WhenConvertedToPostgresDecimal_ReturnsNumberAsString()
    {
        // 10^(995 - 18) = 10^(977)
        var tokenAmount = TokenAmount.FromSubUnits(BigInteger.Pow(10, 995));
        var postgresDecimal = tokenAmount.ToPostgresDecimal();
        var expected = new StringBuilder()
            .Append('1').Append(Enumerable.Range(0, 977).Select(_ => '0').ToArray()) // 1000000... with 977 digits of 0
            .ToString();

        Assert.Equal(expected, postgresDecimal);
    }

    [Fact]
    public void GivenNaN_SubunitsIsZero()
    {
        Assert.Throws<ArithmeticException>(() => TokenAmount.NaN.GetSubUnits());
    }

    public static IEnumerable<object[]> IsNaN_Data => new List<object[]>
        {
            new object[] { TokenAmount.NaN, true },
            new object[] { TokenAmount.FromSubUnits(BigInteger.Zero), false },
            new object[] { TokenAmount.FromSubUnits(BigInteger.One), false },
            new object[] { TokenAmount.FromSubUnits(BigInteger.MinusOne), false },
            new object[] { TokenAmount.FromStringParts(false, "1", "234"), false },
            new object[] { TokenAmount.FromStringParts(false, "-1", "234"), true }, // Invalid call
            new object[] { TokenAmount.FromStringParts(true, "1", "234"), false },
            new object[] { TokenAmount.FromStringParts(false, "1", "-234"), true }, // Invalid call
            new object[] { TokenAmount.FromStringParts(true, "1", "-234"), true }, // Invalid call
        };

    [Theory]
    [MemberData(nameof(IsNaN_Data))]
    public void GivenTokenAmount_IsNaN_ReturnsCorrectly(TokenAmount tokenAmount, bool expectedIsNaN)
    {
        Assert.Equal(expectedIsNaN, tokenAmount.IsNaN());
    }
}
