using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.AspNetCore;

internal class RequestTimeoutMiddleware : IMiddleware
{
    private readonly TimeSpan _timeout;

    public RequestTimeoutMiddleware(IOptions<EndpointOptions> endpointOptions)
    {
        _timeout = endpointOptions.Value.RequestTimeout;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);

        timeoutSource.CancelAfter(_timeout);

        context.RequestAborted = timeoutSource.Token;

        await next(context);
    }
}
