using Prometheus;
using RadixDlt.NetworkGateway.Frontend.Configuration;
using RadixDlt.NetworkGateway.Frontend.Services;

namespace GatewayApi;

public class GatewayApiStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddNetworkGatewayFrontend();

        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddSwaggerGenNewtonsoftSupport()
            .AddCors(options =>
            {
                options.AddDefaultPolicy(corsPolicyBuilder =>
                {
                    corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

        services
            .AddControllers()
            .AddControllersAsServices()
            .AddNewtonsoftJson();

        services
            .AddHealthChecks()
            .AddDbContextCheck<GatewayReadOnlyDbContext>("readonly_database_connection_check")
            .AddDbContextCheck<GatewayReadWriteDbContext>("readwrite_database_connection_check")
            .ForwardToPrometheus();
    }

    public void Configure(IApplicationBuilder application, IConfiguration configuration, ILogger<GatewayApiStartup> logger)
    {
        var isSwaggerEnabled = configuration.GetValue<bool>("EnableSwagger");

        if (isSwaggerEnabled)
        {
            application
                .UseSwagger()
                .UseSwaggerUI();
        }

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
