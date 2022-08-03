using RadixDlt.NetworkGateway.DataAggregator.Configuration;

namespace RadixDlt.NetworkGateway.DataAggregatorApplication;

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
