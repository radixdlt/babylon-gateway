using RadixDlt.NetworkGateway.Contracts.Api.Model;

namespace RadixDlt.NetworkGateway.Endpoints;

public class GatewayEndpoint
{
    public async Task<GatewayResponse> Status(int? myInt, CancellationToken token = default)
    {
        await Task.Delay(12, token);

        if (myInt == 22)
        {
            throw new Exception("bboooooo");
        }

        return new GatewayResponse(new GatewayApiVersions("1.2.3", "3.2.1"), new LedgerState("my_network", 123456, "my_ts"));
    }

    public async Task<TransactionBuildRequest> Status(TransactionBuildRequest request)
    {
        await Task.Delay(1);

        return request;
    }
}
