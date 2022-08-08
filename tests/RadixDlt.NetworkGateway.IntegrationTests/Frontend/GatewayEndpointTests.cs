using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.Frontend;
using RadixDlt.NetworkGateway.FrontendSdk.Model;
using System.Net.Http.Json;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests.Frontend;

public class GatewayEndpointTests
{
    private class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddNetworkGatewayFrontend("unused", "unused");

            services
                .AddControllers();
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

    [Fact]
    public async Task Test()
    {
        var waf = new WebApplicationFactory<Startup>();
        var client = waf.CreateClient();

        using var response = await client.PostAsync("/gateway", JsonContent.Create(new object()));
        var payload = await response.Content.ReadFromJsonAsync<GatewayResponse>();

        Assert.NotNull(payload);
        Assert.NotNull(payload.GatewayApi);
        Assert.Equal("2.0.0", payload.GatewayApi.OpenApiSchemaVersion);
    }
}
