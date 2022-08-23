using GatewayApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration;
using System;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi
{
    public class TestApplicationFactory : WebApplicationFactory<GatewayApiStartup>
    {
        private Mock<INetworkConfigurationProvider> _networkConfigurationProviderMock;
        private Mock<ICoreNodesSelectorService> _coreNodesSelectorServiceMock;

        public TestApplicationFactory()
        {
            _networkConfigurationProviderMock = new Mock<INetworkConfigurationProvider>();
            _networkConfigurationProviderMock.Setup(x => x.GetNetworkName()).Returns(DbSeedHelper.NetworkName);

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
            _ = builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;

                    var logger = scopedServices
                        .GetRequiredService<ILogger<TestApplicationFactory>>();

                    var db = scopedServices.GetRequiredService<ReadOnlyDbContext>();

                    db.Database.EnsureCreated();

                    try
                    {
                        DbSeedHelper.InitializeDbForTests(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"An error occurred seeding the database with seed data. Error: {ex.Message}");
                    }
                }

                services.AddSingleton(_coreNodesSelectorServiceMock.Object);
                services.AddSingleton(_networkConfigurationProviderMock.Object);
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
