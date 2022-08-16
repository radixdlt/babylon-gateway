using Microsoft.Extensions.DependencyInjection;

namespace RadixDlt.NetworkGateway.DataAggregator;

public class DataAggregatorBuilder
{
    public DataAggregatorBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}
