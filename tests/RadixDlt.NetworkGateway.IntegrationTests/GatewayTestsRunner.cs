using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Abstractions;
using TransactionStatus = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionStatus;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public partial class GatewayTestsRunner
{
    private readonly ITestOutputHelper _testConsole;
    private readonly string _testName; // also a database name

    private TestDataAggregatorFactory? _dataAggregatorFactory;
    private TestGatewayApiFactory? _gatewayApiFactory;

    private (string? RequestUri, HttpContent? Content) _request;

    public GatewayTestsRunner(ITestOutputHelper testConsole, string testName)
    {
        _testConsole = testConsole;
        _testName = testName;

        WriteTestHeader();

        // clean up global entities and request/response data
        CoreApiStub = new CoreApiStub();

        PrepareEnvironment();
    }

    public CoreApiStub CoreApiStub { get; }

    public async Task<T> RunAndWaitUntilAllTransactionsAreIngested<T>(TimeSpan? timeout = null)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.NameFromAsync());

        // start test servers and create a database
        Initialize(_testName);

        // make the api call
        var payload = await ActAsync<T>(_request.RequestUri, _request.Content);

        // wait a bit
        timeout ??= TimeSpan.FromSeconds(5);
        await WaitAsync(timeout.Value);

        return payload;
    }

    public async Task<GatewayTestsRunner> WaitUntilAllTransactionsAreIngested(TimeSpan? timeout = null)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.NameFromAsync());

        timeout ??= TimeSpan.FromSeconds(10);

        await WaitAsync(timeout.Value);

        return this;
    }

    public async Task<T> ActAsync<T>(string? requestUri, HttpContent? content)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.NameFromAsync());

        if (requestUri == null)
        {
            throw new Exception("Gateway api uri is missing.");
        }

        if (_gatewayApiFactory == null)
        {
            throw new Exception("Gateway http client is not initialized.");
        }

        using var response = await _gatewayApiFactory.Client.PostAsync(requestUri, content);

        var payload = await response.ParseToObjectAndAssert<T>();

        return payload;
    }

    public GatewayTestsRunner WithAccount(string accountName, string token, long balance)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine($"Account: {accountName}, {token} {balance}");

        var (accountEntity, account) = new AccountBuilder(CoreApiStub.CoreApiStubDefaultConfiguration, CoreApiStub.GlobalEntities)
            .WithAccountName(accountName)
            .WithPublicKey(AddressHelper.GenerateRandomPublicKey())
            .WithTokenName(token)
            .WithBalance(balance)
            .Build();

        CoreApiStub.GlobalEntities.Add(accountEntity);
        CoreApiStub.GlobalEntities.AddStateUpdates(account);

        return this;
    }

    public CoreApiStub ArrangeMempoolTransactionStatusTest(string databaseName, TransactionStatus.StatusEnum expectedStatus)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        switch (expectedStatus)
        {
            case TransactionStatus.StatusEnum.FAILED:
                CoreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.Failed;
                break;
            case TransactionStatus.StatusEnum.PENDING:
                CoreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.SubmittedOrKnownInNodeMempool;
                break;
            case TransactionStatus.StatusEnum.CONFIRMED:
                CoreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
                    MempoolTransactionStatus.Committed;
                break;
        }

        Initialize(databaseName);

        return CoreApiStub;
    }

    public CoreApiStub ArrangeSubmittedTransactionStatusTest(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        Initialize(databaseName);

        return CoreApiStub;
    }

    public CoreApiStub ArrangeSubmitTransactionTest(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        Initialize(databaseName);

        return CoreApiStub;
    }

    public CoreApiStub ArrangeGatewayVersionsTest(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        Initialize(databaseName);

        // set custom gatewayApi and openSchemaApi versions

        return CoreApiStub;
    }

    public CoreApiStub ArrangeTransactionRecentTest(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        Initialize(databaseName);

        // set custom transaction data

        return CoreApiStub;
    }

    private void WriteTestHeader()
    {
        _testConsole.WriteLine($"\n{new string('-', 50)}");
        _testConsole.WriteLine($"{_testName} test");
        _testConsole.WriteLine($"{new string('-', 50)}");
    }

    private async Task WaitAsync(TimeSpan timeout)
    {
        await Task.Delay(timeout);
    }
}
