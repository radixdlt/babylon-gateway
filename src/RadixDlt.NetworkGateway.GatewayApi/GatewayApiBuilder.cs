using Microsoft.Extensions.DependencyInjection;

namespace RadixDlt.NetworkGateway.GatewayApi;

public class GatewayApiBuilder
{
    public GatewayApiBuilder(IServiceCollection services, IHttpClientBuilder coreApiHttpClientBuilder, IHttpClientBuilder coreNodeHealthCheckerClientBuilder)
    {
        Services = services;
        CoreApiHttpClientBuilder = coreApiHttpClientBuilder;
        CoreNodeHealthCheckerClientBuilder = coreNodeHealthCheckerClientBuilder;
    }

    public IServiceCollection Services { get; }

    public IHttpClientBuilder CoreApiHttpClientBuilder { get; }

    public IHttpClientBuilder CoreNodeHealthCheckerClientBuilder { get; }
}
