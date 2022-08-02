using RadixDlt.NetworkGateway.Configuration;

namespace RadixDlt.NetworkGateway.DataAggregator;

public class DataAggregatorStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddNetworkGatewayDataAggregator();

        services
            .AddControllers()
            .AddNewtonsoftJson();

        services
            .AddHealthChecks();
    }

    public void Configure(IApplicationBuilder application)
    {
        application
            .UseRouting()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
    }
}
