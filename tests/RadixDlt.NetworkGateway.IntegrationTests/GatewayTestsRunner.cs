using FluentAssertions;
using RadixDlt.NetworkGateway.Commons;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class GatewayTestsRunner
{
    private HttpClient _client = new();

    public async Task ActAsync()
    {
        await Task.Delay(45000);
    }

    public void ArrangeTransactionStatusTest(string databaseName, TransactionStatus.StatusEnum expectedStatus)
    {
        var coreApiStub = new CoreApiStub();

        TestDataAggregatorFactory.Create(coreApiStub, databaseName);
        _client = TestGatewayApiFactory.Create(coreApiStub, databaseName).Client;
    }

    public async Task<TransactionStatus.StatusEnum> GetTransactionStatus(TransactionIdentifier transactionIdentifier)
    {
        var json = new TransactionStatusRequest(transactionIdentifier).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/transaction/status", content);

        var payload = await response.ParseToObjectAndAssert<TransactionStatusResponse>();

        return payload.Transaction.TransactionStatus.Status;
    }
}
