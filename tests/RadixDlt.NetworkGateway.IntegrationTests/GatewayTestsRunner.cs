using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using TransactionStatus = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionStatus;
using TransactionSubmitRequest = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionSubmitRequest;

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

        StateUpdatesStore = new StateUpdatesStore();

        _testConsole = testConsole;
        _databaseName = testName;

        WriteTestHeader(testName);

        _databaseName = testName;
    }

    public CoreApiStub CoreApiStub { get; }

    public StateUpdatesStore StateUpdatesStore { get; }

    public GatewayTestsRunner WithAccount(string accountAddress, string token, long balance)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine($"Account: {accountAddress}, {token} {balance}");

        var account = new AccountBuilder(
                CoreApiStub.CoreApiStubDefaultConfiguration,
                StateUpdatesStore)
            .WithPublicKey(AddressHelper.GenerateRandomPublicKey())
            .WithFixedAddress(accountAddress)
            .WithTokenName(token)
            .WithBalance(balance)
            .Build();

        StateUpdatesStore.AddStateUpdates(account);

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

    public GatewayTestsRunner MockTokensTransfer(string fromAccount, string toAccount, string tokenName, int amountToTransfer)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine($"Transferring {amountToTransfer} tokens from account: {fromAccount} to account {toAccount}");

        var json = new TransactionSubmitRequest(new Transactions(CoreApiStub.CoreApiStubDefaultConfiguration).SubmitTransactionHex).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _request = ("/transaction/submit", content);

        // since we don't send a real transaction, accounts should be updated in memory store?

        // take tokens from account A
        UpdateAccountBalance(fromAccount, amountToTransfer * (-1));

        // deposit tokens to account B
        UpdateAccountBalance(toAccount, amountToTransfer);

        return this;
    }

    public long GetAccountBalance(string accountAddress)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var accountUpSubstate = StateUpdatesStore.GetLastUpstateByGlobalAddress(accountAddress);

        // TODO: finds only the 1st vault!!!
        var vaultEntityAddressHex = ((accountUpSubstate!.SubstateData.ActualInstance as ComponentStateSubstate)!).OwnedEntities.First(v => v.EntityType == EntityType.Vault).EntityAddressHex;

        var vaultUpSubstate = StateUpdatesStore.GetLastUpstateByEntityAddressHex(vaultEntityAddressHex);

        var vaultResourceAmount = vaultUpSubstate.SubstateData.GetVaultSubstate().ResourceAmount.GetFungibleResourceAmount();

        // TODO: divisibility??? Optimize and make it a separate function
        var attos = double.Parse(vaultResourceAmount!.AmountAttos);
        var tokens = attos / Math.Pow(10, 18);

        _testConsole.WriteLine($"Account: {accountAddress} balance: {tokens}");

        return Convert.ToInt64(tokens);
    }

    public void UpdateAccountBalance(string accountAddress, long amountToTransfer)
    {
        var accountUpSubstate = StateUpdatesStore.GetLastUpstateByGlobalAddress(accountAddress);

        var vaultEntityAddressHex = ((accountUpSubstate!.SubstateData.ActualInstance as ComponentStateSubstate)!).OwnedEntities.First(v => v.EntityType == EntityType.Vault).EntityAddressHex;

        var vaultUpSubstate = StateUpdatesStore.GetLastUpstateByEntityAddressHex(vaultEntityAddressHex);

        var vaultResourceAmount = vaultUpSubstate.SubstateData.GetVaultSubstate().ResourceAmount.GetFungibleResourceAmount();

        // TODO: divisibility??? Optimize and make it a separate function
        var attos = double.Parse(vaultResourceAmount!.AmountAttos);
        var newTokenBalance = attos + (amountToTransfer * Math.Pow(10, 18));
        var newAttos = Convert.ToDecimal(newTokenBalance, CultureInfo.InvariantCulture);
        vaultResourceAmount!.AmountAttos = newAttos.ToString(CultureInfo.InvariantCulture);
    }
}
