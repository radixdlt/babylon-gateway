using Microsoft.Extensions.DependencyInjection;

namespace RadixDlt.NetworkGateway.DataAggregator;

public class DataAggregatorBuilder
{
    public DataAggregatorBuilder(IServiceCollection services, IHttpClientBuilder coreApiHttpClientBuilder)
    {
        Services = services;
        CoreApiHttpClientBuilder = coreApiHttpClientBuilder;
    }

    public IServiceCollection Services { get; }

    public IHttpClientBuilder CoreApiHttpClientBuilder { get; }
}
