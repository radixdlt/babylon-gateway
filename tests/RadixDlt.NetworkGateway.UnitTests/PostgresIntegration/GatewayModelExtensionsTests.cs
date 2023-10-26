using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.PostgresIntegration;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.PostgresIntegration;

public class GatewayModelExtensionsTests
{
    [Fact]
    public void FixOutputProgrammaticJson_RestoresProgrammaticJson()
    {
        var expected = JToken.Parse("""[{"hex":"5c2100","programmatic_json":{"kind":"Tuple","fields":[]}},{"hex":"5c90f8404b3919ad7613d89336a95a402ce6192ab1e3a3aed7ba23e8a65fec16","programmatic_json":{"kind":"Own","value":"internal_component_rdx1lpqykwge44mp8kynx6545spvucvj4v0r5whd0w3razn9lmqkjnvdrn"}}]""");
        var result = GatewayModelExtensions.FixOutputProgrammaticJson("""[{"hex":"5c2100","programmatic_json":null},{"hex":"5c90f8404b3919ad7613d89336a95a402ce6192ab1e3a3aed7ba23e8a65fec16","programmatic_json":null}]""", 1);
        var actual = JToken.Parse(result.ToString());

        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("""[{"hex":"5c2100"}]""")]
    [InlineData("""[{"raw":"5c2100","programmatic_json":null}]""")]
    [InlineData("""[{"hex":"5c2100","programmatic_json":{}}]""")]
    [InlineData("""{"hex":"5c2100","programmatic_json":null}""")]
    public void FixOutputProgrammaticJson_DoesNothingOnInvalidInput(string input)
    {
        var expected = JToken.Parse(input);
        var result = GatewayModelExtensions.FixOutputProgrammaticJson(input, 1);
        var actual = JToken.Parse(result.ToString());

        actual.Should().BeEquivalentTo(expected);
    }
}
