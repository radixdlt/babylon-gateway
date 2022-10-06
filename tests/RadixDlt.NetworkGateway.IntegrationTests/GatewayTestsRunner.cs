using FluentAssertions;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using EcdsaSecp256k1PublicKey = RadixDlt.NetworkGateway.GatewayApiSdk.Model.EcdsaSecp256k1PublicKey;
using PublicKey = RadixDlt.NetworkGateway.GatewayApiSdk.Model.PublicKey;
using PublicKeyType = RadixDlt.NetworkGateway.GatewayApiSdk.Model.PublicKeyType;
using TransactionPreviewRequestFlags = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionPreviewRequestFlags;
using TransactionSubmitRequest = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionSubmitRequest;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public partial class GatewayTestsRunner : IDisposable
{
    private readonly ITestOutputHelper _testConsole;

    private readonly StateUpdatesStore _stateUpdatesStore;

    private readonly TestTransactionStreamStore _transactionStreamStore;

    private readonly string _databaseName;

    private TestDataAggregatorFactory? _dataAggregatorFactory;
    private TestGatewayApiFactory? _gatewayApiFactory;

    public GatewayTestsRunner(
        NetworkDefinition networkDefinition,
        string testName,
        ITestOutputHelper testConsole)
    {
        // clean up and initialize
        CoreApiStub = new CoreApiStub { CoreApiStubDefaultConfiguration = { NetworkDefinition = networkDefinition } };

        _testConsole = testConsole;
        _databaseName = testName;

        _stateUpdatesStore = new StateUpdatesStore(_testConsole);
        _transactionStreamStore = new TestTransactionStreamStore(CoreApiStub, _stateUpdatesStore, testConsole);

        WriteTestHeader(testName);
    }

    public CoreApiStub CoreApiStub { get; }

    public GatewayTestsRunner WithAccount(string accountAddress, string token, long balance)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine($"Account: {accountAddress}, {token}, {balance}");

        _transactionStreamStore.QueueAccountTransaction(accountAddress, token, balance);

        return this;
    }

    // public GatewayTestsRunner ArrangeMempoolTransactionStatusTest(TransactionStatus.StatusEnum expectedStatus)
    // {
    //     _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);
    //
    //     switch (expectedStatus)
    //     {
    //         case TransactionStatus.StatusEnum.FAILED:
    //             CoreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
    //                 MempoolTransactionStatus.Failed;
    //             break;
    //         case TransactionStatus.StatusEnum.PENDING:
    //             CoreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
    //                 MempoolTransactionStatus.SubmittedOrKnownInNodeMempool;
    //             break;
    //         case TransactionStatus.StatusEnum.CONFIRMED:
    //             CoreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionStatus =
    //                 MempoolTransactionStatus.Committed;
    //             break;
    //     }
    //
    //     var transactionIdentifier =
    //         new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, CoreApiStub.CoreApiStubDefaultConfiguration.MempoolTransactionHash);
    //
    //     var json = new TransactionStatusRequest(transactionIdentifier).ToJson();
    //     var content = new StringContent(json, Encoding.UTF8, "application/json");
    //
    //     _request = ("/transaction/status", content);
    //
    //     return this;
    // }

    // public GatewayTestsRunner ArrangeTransactionStatusTest(RecentTransactionsResponse recentTransactions)
    // {
    //     _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);
    //
    //     var hash = recentTransactions.Transactions[0].TransactionIdentifier.Hash;
    //     var transactionIdentifier = new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, hash);
    //     var json = new TransactionStatusRequest(transactionIdentifier).ToJson();
    //     var content = new StringContent(json, Encoding.UTF8, "application/json");
    //
    //     _request = ("/transaction/status", content);
    //
    //     return this;
    // }

    public GatewayTestsRunner MockSubmitTransaction()
    {
        // TODO: submit what?
        return this;
    }

    public GatewayTestsRunner MockGatewayVersions()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _transactionStreamStore.QueueGatewayVersions();

        return this;
    }

    public GatewayTestsRunner MockRecentTransactions()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _transactionStreamStore.QueueRecentTransaction();

        return this;
    }

    public GatewayTestsRunner MockTokensTransfer(string fromAccount, string toAccount, string tokenName, int amountToTransfer)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _transactionStreamStore.QueueTokensTransferTransaction(fromAccount, toAccount, tokenName, amountToTransfer);

        return this;
    }

    public long GetAccountBalance(string accountAddress)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var accountUpSubstate = _stateUpdatesStore.GetLastUpSubstateByEntityAddress(accountAddress);

        // TODO: finds only the 1st vault!!!
        var vaultEntityAddressHex = ((accountUpSubstate!.SubstateData.ActualInstance as ComponentStateSubstate)!).OwnedEntities.First(v => v.EntityType == EntityType.Vault).EntityAddressHex;

        var vaultUpSubstate = _stateUpdatesStore.GetLastUpSubstateByEntityAddressHex(vaultEntityAddressHex);

        var vaultResourceAmount = vaultUpSubstate.SubstateData.GetVaultSubstate().ResourceAmount.GetFungibleResourceAmount();

        // TODO: divisibility??? Optimize and make it a separate function
        var attos = double.Parse(vaultResourceAmount!.AmountAttos);
        var tokens = attos / Math.Pow(10, 18);

        _testConsole.WriteLine($"Account: {accountAddress} balance: {tokens}");

        return Convert.ToInt64(tokens);
    }

    public GatewayTestsRunner MockA2BTransferPreviewTransaction()
    {
        var manifest = new ManifestBuilder()
            .WithLockFeeMethod(AddressHelper.GenerateRandomAddress(CoreApiStub.CoreApiStubDefaultConfiguration.NetworkDefinition.SystemComponentHrp), "10")
            .WithWithdrawByAmountMethod(AddressHelper.GenerateRandomAddress(CoreApiStub.CoreApiStubDefaultConfiguration.NetworkDefinition.AccountComponentHrp), "100",
                AddressHelper.GenerateRandomAddress(CoreApiStub.CoreApiStubDefaultConfiguration.NetworkDefinition.ResourceHrp))
            .WithTakeFromWorktopByAmountMethod(AddressHelper.GenerateRandomAddress(CoreApiStub.CoreApiStubDefaultConfiguration.NetworkDefinition.ResourceHrp), "100", "bucket1")
            .WithDepositToAccountMethod(AddressHelper.GenerateRandomAddress(CoreApiStub.CoreApiStubDefaultConfiguration.NetworkDefinition.AccountComponentHrp), "bucket1")
            .Build();

        var signerPublicKeys = new List<PublicKey>
        {
            new(new EcdsaSecp256k1PublicKey(
                PublicKeyType.EcdsaSecp256k1, "010000000000000000000000000000001")),
        };

        var flags = new TransactionPreviewRequestFlags(unlimitedLoan: false);

        _transactionStreamStore.QueuePreviewTransaction(manifest, costUnitLimit: 0L, tipPercentage: 0L, nonce: string.Empty, signerPublicKeys: signerPublicKeys, flags: flags);

        return this;
    }
}
