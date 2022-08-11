using FluentAssertions;
using Newtonsoft.Json;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi;

public class GatewayEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public GatewayEndpointTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TestSchemaVersion()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        using var response = await client.PostAsync("/gateway", JsonContent.Create(new object()));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string json = await response.Content.ReadAsStringAsync();
        var payload = JsonConvert.DeserializeObject<GatewayResponse>(json);

        // Assert
        payload.Should().NotBeNull();
        payload.GatewayApi.Should().NotBeNull();
        payload.GatewayApi._Version.Should().Be("2.0.0");
    }
}
