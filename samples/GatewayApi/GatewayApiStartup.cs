using Prometheus;
using RadixDlt.NetworkGateway.Frontend;
using RadixDlt.NetworkGateway.Frontend.Services;

namespace GatewayApi;

public class GatewayApiStartup
{
    private readonly string _roConnectionString;
    private readonly string _rwConnectionString;
    private readonly int _prometheusMetricsPort;
    private readonly bool _enableSwagger;

    public GatewayApiStartup(IConfiguration configuration)
    {
        _roConnectionString = configuration.GetConnectionString("ReadOnlyDbContext");
        _rwConnectionString = configuration.GetConnectionString("ReadWriteDbContext");
        _prometheusMetricsPort = configuration.GetValue<int>("PrometheusMetricsPort");
        _enableSwagger = configuration.GetValue<bool>("EnableSwagger");
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddNetworkGatewayFrontend(_roConnectionString, _rwConnectionString);

        if (_enableSwagger)
        {
            services
                .AddSwaggerGen()
                .AddSwaggerGenNewtonsoftSupport();
        }

        services
            .AddEndpointsApiExplorer()
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
        if (_enableSwagger)
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
