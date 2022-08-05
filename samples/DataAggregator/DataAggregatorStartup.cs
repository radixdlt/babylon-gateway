using Prometheus;
using RadixDlt.NetworkGateway.DataAggregator;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.Services;

namespace DataAggregator;

public class DataAggregatorStartup
{
    private readonly string _connectionString;
    private readonly int _prometheusMetricsPort;

    public DataAggregatorStartup(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AggregatorDbContext");
        _prometheusMetricsPort = configuration.GetValue<int>("PrometheusMetricsPort");
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddNetworkGatewayDataAggregator(_connectionString);

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

    public void Configure(IApplicationBuilder application, ILogger<DataAggregatorStartup> logger)
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

        StartMetricServer(logger);
    }

    private void StartMetricServer(ILogger logger)
    {
        if (_prometheusMetricsPort != 0)
        {
            logger.LogInformation("Starting metrics server on port http://localhost:{MetricPort}", _prometheusMetricsPort);

            new KestrelMetricServer(port: _prometheusMetricsPort).Start();
        }
        else
        {
            logger.LogInformation("PrometheusMetricsPort not configured - not starting metric server");
        }
    }
}
