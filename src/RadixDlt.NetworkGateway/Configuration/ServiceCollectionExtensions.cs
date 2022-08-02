using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.Endpoints;

namespace RadixDlt.NetworkGateway.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddNetworkGatewayDataAggregator(this IServiceCollection services)
    {
        services
            .AddHostedService<LedgerSynchronizationWorker>();

        services
            .AddOptions<DataAggregatorOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("DataAggregator"); // TODO is this how we want to Bind configuration by default?
    }

    public static void AddNetworkGatewayApi(this IServiceCollection services)
    {
        services
            .AddScoped<GatewayEndpoint>()
            .AddScoped<TransactionEndpoint>();

        services
            .AddOptions<ApiOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("Api"); // TODO is this how we want to Bind configuration by default?
    }
}
