using System.Threading;

namespace RadixDlt.NetworkGateway.GatewayApi.AspNetCore;

public interface IRequestAbortedFeature
{
    public CancellationToken CancellationToken { get; }
}

public class RequestAbortedFeature : IRequestAbortedFeature
{
    public RequestAbortedFeature(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
    }

    public CancellationToken CancellationToken { get; }
}
