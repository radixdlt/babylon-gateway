using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.GatewayApi;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddNetworkGatewayApi();

            services
            .AddControllers()
            .AddControllersAsServices()
            .AddNewtonsoftJson();
        }

        public void Configure(IApplicationBuilder application)
        {
            application
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
