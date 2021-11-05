using Shared.Numerics;
using System.Numerics;
using System.Text;
using Xunit;

namespace Tests.MiniUnitTests.Common.Numerics;

public class TokenAmountTests
{
    [Theory]
    [InlineData("123", "", "123")]
    [InlineData("", "12", "0.12")]
    [InlineData("000", "13444", "0.13444")]
    [InlineData("0", "0", "0")]
    [InlineData("11", "000000000000000009", "11.000000000000000009")] // Does hold 18 dp
    [InlineData("11", "0000000000000000009", "11")] // Only holds 18 dp (truncating)
    [InlineData("123.34", "", "NaN")]
    [InlineData("ab", "cd", "NaN")]
    [InlineData("ab", "0", "NaN")]
    [InlineData("-1", "0", "NaN")]
    [InlineData("12", "-3", "NaN")]
    public void Create_TokenAmountFromStringParts_ReadsCorrectly(string wholePart, string fractionalPart, string expected)
    {
        Assert.Equal(expected, TokenAmount.FromStringPartsOrNaN(wholePart, fractionalPart).ToString());
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("0.12", "0.12")]
    [InlineData("000.13444", "0.13444")]
    [InlineData("0.0", "0")]
    [InlineData("11.000000000000000009", "11.000000000000000009")] // Does hold 18 dp
    [InlineData("123.34", "123.34")]
    [InlineData("NaN", "NaN")]
    [InlineData("Infinity", "NaN")]
    [InlineData("-Infinity", "NaN")]
    [InlineData("-123", "NaN")]
    public void Create_FromString_ReadsCorrectly(string postgresDecimal, string expected)
    {
        Assert.Equal(expected, TokenAmount.FromStringOrNaN(postgresDecimal).ToString());
    }

    [Theory]
    [InlineData("0", "0.000000000000000000")]
    [InlineData("1", "0.000000000000000001")]
    [InlineData("123", "0.000000000000000123")]
    [InlineData("12345678900123456789", "12.345678900123456789")]
    [InlineData("-123", "NaN")]
    public void Create_FromSubUnits_ReadsCorrectlyAtFullPrecision(string subUnitsStr, string expected)
    {
        Assert.Equal(expected, TokenAmount.FromSubUnitsStringOrNaN(subUnitsStr).ToStringFullPrecision());
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
        Assert.Equal(TokenAmount.FromStringOrNaN("123"), TokenAmount.FromStringOrNaN("123"));
    }

    [Fact]
    public void Equate_DifferentTokenAmount_ReturnsFalse()
    {
        Assert.NotEqual(TokenAmount.FromStringOrNaN("123"), TokenAmount.FromStringOrNaN("1234"));
    }

    [Fact]
    public void Equate_NaNWithNoneNaN_ReturnsFalse()
    {
        Assert.NotEqual(TokenAmount.FromStringOrNaN("1"), TokenAmount.NaN);
    }

    [Fact]
    public void Equate_SameAmountCreatedTwoDifferentWays_ReturnsTrue()
    {
        Assert.Equal(TokenAmount.FromStringOrNaN("123"), TokenAmount.FromSubUnitsStringOrNaN("123000000000000000000"));
    }

    [Fact]
    public void GivenANumberTooLargeForPostgres_WhenConvertedToPostgresDecimal_ReturnsNaN()
    {
        var tokenAmount = TokenAmount.FromSubUnitsOrNaN(BigInteger.Pow(10, 1000));
        Assert.Equal("NaN", tokenAmount.ToPostgresDecimal());
    }

    [Fact]
    public void GivenANumberInsidePostgresLimit_WhenConvertedToPostgresDecimal_ReturnsNumberAsString()
    {
        // 10^(995 - 18) = 10^(977)
        var tokenAmount = TokenAmount.FromSubUnitsOrNaN(BigInteger.Pow(10, 995));
        var postgresDecimal = tokenAmount.ToPostgresDecimal();
        var expected = new StringBuilder()
            .Append('1').Append(Enumerable.Range(0, 977).Select(_ => '0').ToArray()) // 1000000... with 977 digits of 0
            .ToString();

        Assert.Equal(expected, postgresDecimal);
    }
}
