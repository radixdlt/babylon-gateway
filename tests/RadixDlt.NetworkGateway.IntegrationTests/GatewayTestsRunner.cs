using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class GatewayTestsRunner
{
    private HttpClient? _client;

    public async Task ActAsync()
    {
        await Task.Delay(10000);
    }

    public async Task<T> ActAsync<T>(string? requestUri, HttpContent? content)
    {
        if (requestUri == null)
        {
            throw new Exception("Gateway api uri is missing.");
        }

        if (_client == null)
        {
            throw new Exception("Gateway http client is not initialized.");
        }

        using var response = await _client.PostAsync(requestUri, content);

        var payload = await response.ParseToObjectAndAssert<T>();

        return payload;
    }

    public CoreApiStub ArrangeTransactionStatusTest(string databaseName, TransactionStatus.StatusEnum expectedStatus)
    {
        var coreApiStub = new CoreApiStub();

        switch (expectedStatus)
        {
            case TransactionStatus.StatusEnum.FAILED:
                coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.Failed;
                break;
            case TransactionStatus.StatusEnum.PENDING:
                coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.SubmittedOrKnownInNodeMempool;
                break;
            case TransactionStatus.StatusEnum.CONFIRMED:
                coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.Committed;
                break;
        }

        _client = TestGatewayApiFactory.Create(coreApiStub, databaseName).Client;
        TestDataAggregatorFactory.Create(coreApiStub, databaseName);

        return coreApiStub;
    }

    public CoreApiStub ArrangeGatewayVersionsTest(string databaseName)
    {
        var coreApiStub = new CoreApiStub();
        _client = TestGatewayApiFactory.Create(coreApiStub, databaseName).Client;

        // set custom gatewayApi and openSchemaApi versions

        return coreApiStub;
    }
}
