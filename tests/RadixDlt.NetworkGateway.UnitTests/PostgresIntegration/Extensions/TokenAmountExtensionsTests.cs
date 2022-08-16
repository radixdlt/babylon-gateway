using FluentAssertions;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.Common.Numerics;
using System.Linq;
using System.Numerics;
using System.Text;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.PostgresIntegration.Extensions;

public class TokenAmountExtensionsTests
{
    [Fact]
    public void GivenANumberTooLargeForPostgres_WhenConvertedToPostgresDecimal_ReturnsNaN()
    {
        var tokenAmount = TokenAmount.FromSubUnits(BigInteger.Pow(10, 1000));
        var postgresDecimal = tokenAmount.ToPostgresDecimal();

        postgresDecimal.Should().Be("NaN");
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

        postgresDecimal.Should().Be(expected);
    }
}
