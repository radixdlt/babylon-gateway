using Prometheus;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.Services;

namespace DataAggregator;

public class DataAggregatorStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddNetworkGatewayDataAggregator();

        services
            .AddEndpointsApiExplorer();

        services
            .AddControllers()
            .AddControllersAsServices()
            .AddNewtonsoftJson();

        services
            .AddHealthChecks()
            .AddCheck<AggregatorHealthCheck>("aggregator_health_check")
            .AddDbContextCheck<AggregatorDbContext>("database_connection_check")
            .ForwardToPrometheus();
    }

    public void Configure(IApplicationBuilder application, IConfiguration configuration, ILogger<DataAggregatorStartup> logger)
    {
        application
            .UseAuthentication()
            .UseAuthorization()
            .UseCors()
            .UseHttpMetrics()
            .UseRouting()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });

        StartMetricServer(configuration, logger);
    }

    private void StartMetricServer(IConfiguration configuration, ILogger logger)
    {
        var metricPort = configuration.GetValue<int>("PrometheusMetricsPort");

        if (metricPort != 0)
        {
            logger.LogInformation("Starting metrics server on port http://localhost:{MetricPort}", metricPort);

            new KestrelMetricServer(port: metricPort).Start();
        }
        else
        {
            logger.LogInformation("PrometheusMetricsPort not configured - not starting metric server");
        }
    }
}
