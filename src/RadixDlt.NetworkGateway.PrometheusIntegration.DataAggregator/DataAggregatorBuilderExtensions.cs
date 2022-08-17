using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using RadixDlt.NetworkGateway.DataAggregator;
using RadixDlt.NetworkGateway.DataAggregator.Workers.GlobalWorkers;
using RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

namespace RadixDlt.NetworkGateway.PrometheusIntegration.DataAggregator;

public static class DataAggregatorBuilderExtensions
{
    public static DataAggregatorBuilder UsePrometheusMetrics(this DataAggregatorBuilder builder)
    {
        builder.Services
            .AddSingleton<MetricsObserver>()
            .AddSingleton<IGlobalWorkerObserver>(provider => provider.GetRequiredService<MetricsObserver>())
            .AddSingleton<INodeWorkerObserver>(provider => provider.GetRequiredService<MetricsObserver>());

        builder.CoreApiHttpClientBuilder
            .UseHttpClientMetrics();

        return builder;
    }
}
