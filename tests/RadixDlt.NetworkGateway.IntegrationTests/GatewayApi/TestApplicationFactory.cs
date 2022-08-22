using GatewayApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using Moq;
using Microsoft.Extensions.Options;
using System;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi
{
    public class TestApplicationFactory : WebApplicationFactory<GatewayApiStartup>
    {
        public static readonly string NetworkName = "integration_tests_net";

        private Mock<ICoreNodesSelectorService> _coreNodesSelectorServiceMock;

        public TestApplicationFactory()
        {
            _coreNodesSelectorServiceMock = new Mock<ICoreNodesSelectorService>();
            _coreNodesSelectorServiceMock.Setup(x => x.GetRandomTopTierCoreNode()).Returns(
                new CoreApiNode()
                {
                    CoreApiAddress = "http://localhost:3333",
                    Name = "node1",
                    Enabled = true,
                });
        }

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

                services.AddSingleton(_coreNodesSelectorServiceMock.Object);
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
            });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
            })
            .UseStartup<GatewayApiStartup>();

            return hostBuilder;
        }
    }
}
