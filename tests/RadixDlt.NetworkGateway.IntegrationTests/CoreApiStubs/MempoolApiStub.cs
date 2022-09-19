using RadixDlt.CoreApiSdk.Api;
using RadixDlt.CoreApiSdk.Model;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class MempoolApiStub : MempoolApi
{
    private readonly MempoolResponse _mempoolResponse;

    public MempoolApiStub(MempoolResponse mempoolResponse)
    {
        _mempoolResponse = mempoolResponse;
    }

    public override async Task<MempoolResponse> MempoolPostAsync(MempoolRequest mempoolRequest, CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        return await Task.FromResult(_mempoolResponse);
    }
}
