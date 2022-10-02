using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using TransactionStatus = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionStatus;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public partial class GatewayTestsRunner : IDisposable
{
    private readonly ITestOutputHelper _testConsole;

    private TestDataAggregatorFactory? _dataAggregatorFactory;
    private TestGatewayApiFactory? _gatewayApiFactory;

    private string _databaseName;

    private (string? RequestUri, HttpContent? Content) _request;

    public GatewayTestsRunner(
        NetworkDefinition networkDefinition,
        string testName,
        ITestOutputHelper testConsole)
    {
        // clean up and initialize
        CoreApiStub = new CoreApiStub { CoreApiStubDefaultConfiguration = { NetworkDefinition = networkDefinition } };

        _testConsole = testConsole;
        _databaseName = testName;

        WriteTestHeader(testName);

        _databaseName = testName;
    }

    public CoreApiStub CoreApiStub { get; }

    public GatewayTestsRunner WithAccount(string accountName, string token, long balance)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine($"Account: {accountName}, {token} {balance}");

        var (accountEntity, account) = new AccountBuilder(
                CoreApiStub.CoreApiStubDefaultConfiguration,
                CoreApiStub.GlobalEntities)
            .WithAccountName(accountName)
            .WithPublicKey(AddressHelper.GenerateRandomPublicKey())
            .WithTokenName(token)
            .WithBalance(balance)
            .Build();

        CoreApiStub.GlobalEntities.Add(accountEntity);
        CoreApiStub.GlobalEntities.AddStateUpdates(account);

        return this;
    }

    public GatewayTestsRunner ArrangeMempoolTransactionStatusTest(TransactionStatus.StatusEnum expectedStatus)
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

        var transactionIdentifier =
            new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, CoreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionHash);

        var json = new TransactionStatusRequest(transactionIdentifier).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _request = ("/transaction/status", content);

        return this;
    }

    public GatewayTestsRunner ArrangeSubmittedTransactionStatusTest(RecentTransactionsResponse recentTransactions)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var hash = recentTransactions.Transactions[0].TransactionIdentifier.Hash;
        var transactionIdentifier = new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, hash);
        var json = new TransactionStatusRequest(transactionIdentifier).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _request = ("/transaction/status", content);

        return this;
    }

    public GatewayTestsRunner ArrangeSubmitTransactionTest()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var json = new TransactionSubmitRequest(new Transactions(CoreApiStub.CoreApiStubDefaultConfiguration).SubmitTransactionHex).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _request = ("/transaction/submit", content);

        return this;
    }

    public GatewayTestsRunner ArrangeGatewayVersionsTest()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _request = ("/gateway", JsonContent.Create(new object()));

        // set custom gatewayApi and openSchemaApi versions

        return this;
    }

    public GatewayTestsRunner ArrangeTransactionRecentTest()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var json = new RecentTransactionsRequest().ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _request = ("/transaction/recent", content);

        // set custom transaction data

        return this;
    }
}
