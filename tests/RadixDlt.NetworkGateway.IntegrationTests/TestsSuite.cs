using RadixDlt.NetworkGateway.IntegrationTests.CoreMocks;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class TestsSuite
{
    private CoreApiMocks? _coreApiMocks;

    public GatewayTestsRunner GatewayTestRunner { get; }

    public TestsSuite(CoreApiMocks coreApiMocks, string databaseName)
    {
        _coreApiMocks = coreApiMocks;

        // inject core api mocks
        GatewayTestRunner = new GatewayTestsRunner(coreApiMocks, databaseName);
    }
}
