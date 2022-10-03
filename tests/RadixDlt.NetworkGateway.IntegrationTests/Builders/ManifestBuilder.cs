using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class ManifestBuilder : BuilderBase<string>
{
    private readonly List<string> _instructions = new();

    public override string Build()
    {
        return string.Join(";", _instructions);
    }

    public ManifestBuilder CallMethod(string componentAddress, string methodName, string[]? args = null)
    {
        _instructions.Add($"\"component_address\":\"{componentAddress}\",\"method_name\":\"{methodName}\",\"arguments\":[{args}]");

        return this;
}
}
