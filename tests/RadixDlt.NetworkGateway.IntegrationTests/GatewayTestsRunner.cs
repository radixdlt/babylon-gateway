using FluentAssertions;
using RadixDlt.NetworkGateway.Commons;
using RadixDlt.NetworkGateway.Commons.Model;
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
        await Task.Delay(10000);
    }

    public CoreApiStub ArrangeTransactionStatusTest(string databaseName, TransactionStatus.StatusEnum expectedStatus)
    {
        var coreApiStub = new CoreApiStub();

        switch (expectedStatus)
        {
            case TransactionStatus.StatusEnum.FAILED:
                coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransaction.TransactionStatus =
                    MempoolTransactionStatus.Failed;
                break;
            case TransactionStatus.StatusEnum.PENDING:
                coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransaction.TransactionStatus =
                    MempoolTransactionStatus.SubmittedOrKnownInNodeMempool;
                break;
            case TransactionStatus.StatusEnum.CONFIRMED:
                coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransaction.TransactionStatus =
                    MempoolTransactionStatus.Committed;
                break;
        }

        _client = TestGatewayApiFactory.Create(coreApiStub, databaseName).Client;
        TestDataAggregatorFactory.Create(coreApiStub, databaseName);

        return coreApiStub;
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
