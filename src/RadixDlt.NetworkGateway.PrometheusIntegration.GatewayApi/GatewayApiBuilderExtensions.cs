using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using RadixDlt.NetworkGateway.GatewayApi;
using RadixDlt.NetworkGateway.GatewayApi.Endpoints;
using RadixDlt.NetworkGateway.GatewayApi.Services;

namespace RadixDlt.NetworkGateway.PrometheusIntegration.GatewayApi;

public static class GatewayApiBuilderExtensions
{
    public static GatewayApiBuilder UsePrometheusMetrics(this GatewayApiBuilder builder)
    {
        builder.Services
            .AddSingleton<MetricObserver>()
            .AddSingleton<IExceptionObserver>(provider => provider.GetRequiredService<MetricObserver>())
            .AddSingleton<ICoreNodeHealthCheckerObserver>(provider => provider.GetRequiredService<MetricObserver>())
            .AddSingleton<IConstructionAndSubmissionServiceObserver>(provider => provider.GetRequiredService<MetricObserver>());

        builder.CoreApiHttpClientBuilder
            .UseHttpClientMetrics();

        builder.CoreNodeHealthCheckerClientBuilder
            .UseHttpClientMetrics();

        return builder;
    }
}
