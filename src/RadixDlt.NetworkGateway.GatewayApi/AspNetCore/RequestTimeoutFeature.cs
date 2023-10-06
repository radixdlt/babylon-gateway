using System;
using System.Threading;

namespace RadixDlt.NetworkGateway.GatewayApi.AspNetCore;

public interface IRequestTimeoutFeature
{
    public CancellationToken CancellationToken { get; }

    public TimeSpan Timeout { get; }
}

public class RequestTimeoutFeature : IRequestTimeoutFeature
{
    public RequestTimeoutFeature(CancellationToken cancellationToken, TimeSpan timeoutAfter)
    {
        CancellationToken = cancellationToken;
        Timeout = timeoutAfter;
    }

    public CancellationToken CancellationToken { get; }

    public TimeSpan Timeout { get; }
}
