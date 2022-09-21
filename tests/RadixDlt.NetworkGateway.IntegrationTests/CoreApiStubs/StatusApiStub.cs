using RadixDlt.CoreApiSdk.Api;
using RadixDlt.CoreApiSdk.Model;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class StatusApiStub : StatusApi
{
    private readonly NetworkStatusResponse _networkStatusResponse;

    public StatusApiStub(NetworkStatusResponse networkStatusResponse)
    {
        _networkStatusResponse = networkStatusResponse;
    }

    public override async Task<NetworkStatusResponse> StatusNetworkStatusPostAsync(
        NetworkStatusRequest networkStatusRequest, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_networkStatusResponse);
    }
}
