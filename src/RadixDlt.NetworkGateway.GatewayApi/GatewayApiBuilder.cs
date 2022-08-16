using Microsoft.Extensions.DependencyInjection;

namespace RadixDlt.NetworkGateway.GatewayApi;

public class GatewayApiBuilder
{
    public GatewayApiBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}
