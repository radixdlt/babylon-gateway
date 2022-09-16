using FluentAssertions;
using RadixDlt.NetworkGateway.Commons;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreMocks;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class GatewayTestsRunner
{
    private GatewayTestServerFactory _gatewayTestServer;

    public GatewayTestsRunner(CoreApiMocks coreApiMocks, string databaseName)
    {
        _gatewayTestServer = GatewayTestServerFactory.Create(coreApiMocks, databaseName);
        Client = _gatewayTestServer.GatewayApiHttpClient;
    }

    public HttpClient Client { get; }

    public async Task<TransactionStatus.StatusEnum> GetTransactionStatus(TransactionIdentifier transactionIdentifier)
    {
        string json = new TransactionStatusRequest(transactionIdentifier).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await Client.PostAsync("/transaction/status", content);

        var payload = await response.ParseToObjectAndAssert<TransactionStatusResponse>();

        return payload.Transaction.TransactionStatus.Status;
    }
}
