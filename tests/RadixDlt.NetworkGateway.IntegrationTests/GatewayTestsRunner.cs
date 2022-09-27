using Newtonsoft.Json;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EcdsaSecp256k1PublicKey = RadixDlt.NetworkGateway.GatewayApiSdk.Model.EcdsaSecp256k1PublicKey;
using PublicKeyType = RadixDlt.NetworkGateway.GatewayApiSdk.Model.PublicKeyType;
using TransactionPreviewRequestFlags = RadixDlt.CoreApiSdk.Model.TransactionPreviewRequestFlags;
using TransactionStatus = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionStatus;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class GatewayTestsRunner
{
    private HttpClient? _client;

    private CoreApiStub _coreApiStub;

    public GatewayTestsRunner()
    {
        _coreApiStub = new CoreApiStub();
    }

    public async Task<GatewayTestsRunner> WaitUntilAllTransactionsAreIngested(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);

        await Task.Delay(timeout.Value);

        return this;
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

    public GatewayTestsRunner MockGenesis()
    {
        var json = System.IO.File.ReadAllText("genesis.json");

        _coreApiStub.CoreApiStubDefaultConfiguration.CommittedTransactionsResponse = JsonConvert.DeserializeObject<CommittedTransactionsResponse>(json);

        return this;
    }

    public CoreApiStub ArrangeMempoolTransactionStatusTest(string databaseName, TransactionStatus.StatusEnum expectedStatus)
    {
        switch (expectedStatus)
        {
            case TransactionStatus.StatusEnum.FAILED:
                _coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.Failed;
                break;
            case TransactionStatus.StatusEnum.PENDING:
                _coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.SubmittedOrKnownInNodeMempool;
                break;
            case TransactionStatus.StatusEnum.CONFIRMED:
                _coreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.Committed;
                break;
        }

        _client = TestGatewayApiFactory.Create(_coreApiStub, databaseName).Client;
        TestDataAggregatorFactory.Create(_coreApiStub, databaseName);

        return _coreApiStub;
    }

    public CoreApiStub ArrangeSubmittedTransactionStatusTest(string databaseName)
    {
        _client = TestGatewayApiFactory.Create(_coreApiStub, databaseName).Client;
        TestDataAggregatorFactory.Create(_coreApiStub, databaseName);

        return _coreApiStub;
    }

    public CoreApiStub ArrangeGatewayVersionsTest(string databaseName)
    {
        _client = TestGatewayApiFactory.Create(_coreApiStub, databaseName).Client;
        TestDataAggregatorFactory.Create(_coreApiStub, databaseName);

        // set custom gatewayApi and openSchemaApi versions

        return _coreApiStub;
    }

    public CoreApiStub ArrangeTransactionRecentTest(string databaseName)
    {
        _client = TestGatewayApiFactory.Create(_coreApiStub, databaseName).Client;
        TestDataAggregatorFactory.Create(_coreApiStub, databaseName);

        // set custom transaction data

        return _coreApiStub;
    }

    public CoreApiStub ArrangeTransactionPreviewTest(string databaseName)
    {
        _client = TestGatewayApiFactory.Create(_coreApiStub, databaseName).Client;
        TestDataAggregatorFactory.Create(_coreApiStub, databaseName);

        // set preview request
        _coreApiStub.CoreApiStubDefaultConfiguration.TransactionPreviewRequest = new GatewayApiSdk.Model.TransactionPreviewRequest(
            manifest: new ManifestBuilder().CallMethod("021c77780d10210ec9f0ea4a372ab39e09f2222c07c9fb6e5cfc81", "CALL_FUNCTION").Build(),
            blobsHex: new List<string>() { "blob hex" },
            costUnitLimit: 1L,
            tipPercentage: 5L,
            nonce: "nonce",
            signerPublicKeys: new List<GatewayApiSdk.Model.PublicKey>()
            {
                new(new EcdsaSecp256k1PublicKey(
                    keyType: PublicKeyType.EcdsaSecp256k1,
                    keyHex: "010000000000000000000000000000000000000000000000000001")),
            },
            flags: new GatewayApiSdk.Model.TransactionPreviewRequestFlags(unlimitedLoan: false));

        return _coreApiStub;
    }
}
