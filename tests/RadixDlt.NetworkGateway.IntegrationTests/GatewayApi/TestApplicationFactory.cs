using GatewayApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi
{
    public class TestApplicationFactory : WebApplicationFactory<GatewayApiStartup>
    {
        public static readonly string NETWORK_NAME = "localnet";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;

                    var logger = scopedServices
                        .GetRequiredService<ILogger<TestApplicationFactory>>();
                }
            });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
                config.AddJsonFile("appsettings.Development.json", false, true);
            })
            .UseStartup<GatewayApiStartup>();

            return hostBuilder;
        }
    }
}
