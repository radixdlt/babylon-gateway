using RadixDlt.NetworkGateway.Contracts.Api.Model;

namespace RadixDlt.NetworkGateway.Endpoints;

public class GatewayEndpoint
{
    public async Task<GatewayResponse> Status(CancellationToken token = default)
    {
        await Task.Delay(1, token);

        return new GatewayResponse(new GatewayApiVersions("123", "321"), new LedgerState("network", 123, "some timestamp"));
    }
}
