using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.GatewayApi;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi;

public class GatewayEndpointTests
{
    private class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddNetworkGatewayApi();

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

        payload.ShouldNotBeNull();
        payload.GatewayApi.ShouldNotBeNull();
        payload.GatewayApi.OpenApiSchemaVersion.Should().Be("2.0.0");
    }
}
