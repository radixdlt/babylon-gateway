using RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class TestGlobalEntity : GlobalEntityId
{
    public string Name { get; set; } = string.Empty;

    public string AccountPublicKey { get; set; } = string.Empty;
}
