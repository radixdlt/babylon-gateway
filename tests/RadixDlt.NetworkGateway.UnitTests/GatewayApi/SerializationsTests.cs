using RadixDlt.NetworkGateway.GatewayApi;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.GatewayApi;

public class SerializationsTests
{
    private record InputType(string SomeString, int SomeInt);

    private readonly InputType _input = new InputType("abc", 123);

    [Fact]
    public void SerializationDeserializationPayload()
    {
        var base64json = Serializations.AsBase64Json(_input);
        var output = Serializations.FromBase64JsonOrDefault<InputType>(base64json);

        Assert.StrictEqual(_input, output);
    }
}
