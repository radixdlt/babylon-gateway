using FluentAssertions;
using Newtonsoft.Json;
using RadixDlt.NetworkGateway.GatewayApi.Endpoints;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System.Net.Http.Json;
using System.Text;
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
        using HttpResponseMessage response = await client.PostAsync("/gateway", JsonContent.Create(new object()));

        // Assert
        var payload = await response.ParseToObjectAndAssert<GatewayResponse>();

        payload.GatewayApi.Should().NotBeNull();
        payload.GatewayApi._Version.Should().Be("2.0.0");
    }

    [Fact]
    public async Task TestTransactionRecent()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await GetRecentTransactions(client);

        // Assert
        response.LedgerState.Should().NotBeNull();
        response.LedgerState.Network = TestApplicationFactory.NETWORK_NAME;
        response.LedgerState._Version.Should().Be(1);

        response.Transactions.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TestTransactionStatus()
    {
        // Arrange
        var client = _factory.CreateClient();

        var recentTransactions = await GetRecentTransactions(client);

        var transactionidentifier = recentTransactions.Transactions[0].TransactionIdentifier;

        // Act
        string json = new TransactionStatusRequest(transactionidentifier).ToJson();

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/transaction/status", content);

        // Assert
        var payload = await response.ParseToObjectAndAssert<TransactionStatusResponse>();

        payload.Transaction.TransactionIdentifier.Hash.Length.Should().Be(64);
        payload.Transaction.TransactionStatus.LedgerStateVersion.Should().Be(1);
        payload.Transaction.TransactionStatus.Status.Should().Be(TransactionStatus.StatusEnum.CONFIRMED);
    }

    [Fact]
    public void TestValidateOpenApiSchema()
    {
        // validate TransactionController
        GatewayApiSpecValidator.ValidateController(typeof(TransactionController), "/transaction/");
    }

    private async Task<RecentTransactionsResponse> GetRecentTransactions(HttpClient client)
    {
        using HttpResponseMessage response = await client.PostAsync(
            "/transaction/recent",
            JsonContent.Create(new RecentTransactionsRequest()));

        var payload = await response.ParseToObjectAndAssert<RecentTransactionsResponse>();

        payload.Transactions.Should().NotBeNull();

        return payload;
    }
}
