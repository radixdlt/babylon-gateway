using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using EcdsaSecp256k1PublicKey = RadixDlt.NetworkGateway.GatewayApiSdk.Model.EcdsaSecp256k1PublicKey;
using PublicKey = RadixDlt.NetworkGateway.GatewayApiSdk.Model.PublicKey;
using PublicKeyType = RadixDlt.NetworkGateway.GatewayApiSdk.Model.PublicKeyType;
using TransactionPreviewRequestFlags = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionPreviewRequestFlags;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public partial class GatewayTestsRunner : IDisposable
{
    private readonly string _databaseName;

    private readonly StateUpdatesStore _stateUpdatesStore;
    private readonly ITestOutputHelper _testConsole;

    private readonly TestTransactionStreamStore _transactionStreamStore;

    private TestDataAggregatorFactory? _dataAggregatorFactory;
    private TestGatewayApiFactory? _gatewayApiFactory;

    public GatewayTestsRunner(
        string testName,
        ITestOutputHelper testConsole)
    {
        _testConsole = testConsole;
        _databaseName = testName;

        WriteTestHeader(testName);

        _testConsole.WriteLine("Initializing GatewayTestsRunner...");

        _stateUpdatesStore = new StateUpdatesStore(_testConsole);

        var requestsAndResponses = new CoreApiStubRequestsAndResponses();

        _transactionStreamStore = new TestTransactionStreamStore(requestsAndResponses, _stateUpdatesStore, testConsole);

        CoreApiStub = new CoreApiStub(requestsAndResponses, _transactionStreamStore);
    }

    public CoreApiStub CoreApiStub { get; }

    public GatewayTestsRunner WithAccount(string accountAddress, string publicKey, string token, long tokenAmount = 1000, int lockFee = 10)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine($"Account: {accountAddress}, {token}, {tokenAmount}");

        _transactionStreamStore.QueueCreateAccountTransaction(accountAddress, publicKey, token, lockFee);

        return this;
    }

    // public GatewayTestsRunner ArrangeMempoolTransactionStatusTest(TransactionStatus.StatusEnum expectedStatus)
    // {
    //     _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);
    //
    //     switch (expectedStatus)
    //     {
    //         case TransactionStatus.StatusEnum.FAILED:
    //             CoreApiStub.RequestsAndResponses.MempoolTransactionStatus =
    //                 MempoolTransactionStatus.Failed;
    //             break;
    //         case TransactionStatus.StatusEnum.PENDING:
    //             CoreApiStub.RequestsAndResponses.MempoolTransactionStatus =
    //                 MempoolTransactionStatus.SubmittedOrKnownInNodeMempool;
    //             break;
    //         case TransactionStatus.StatusEnum.CONFIRMED:
    //             CoreApiStub.RequestsAndResponses.MempoolTransactionStatus =
    //                 MempoolTransactionStatus.Committed;
    //             break;
    //     }
    //
    //     var transactionIdentifier =
    //         new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, CoreApiStub.RequestsAndResponses.MempoolTransactionHash);
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

    public GatewayTestsRunner MockTokensTransfer(string fromAccount, string toAccount, string tokenName, int amountToTransfer, string tokensTransferTransactionIntentHash)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _transactionStreamStore.QueueTokensTransferTransaction(fromAccount, toAccount, tokenName, amountToTransfer, tokensTransferTransactionIntentHash);

        return this;
    }

    public double GetAccountBalance(string accountAddress)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var accountUpSubstate = _stateUpdatesStore.StateUpdates.GetLastUpSubstateByEntityAddress(accountAddress);

        // TODO: finds only the 1st vault!!!
        var vaultEntityAddressHex = (accountUpSubstate?.SubstateData.ActualInstance as ComponentStateSubstate)?.OwnedEntities.First(v => v.EntityType == EntityType.Vault)
            .EntityAddressHex;

        var vaultUpSubstate = _stateUpdatesStore.StateUpdates.GetLastUpSubstateByEntityAddressHex(vaultEntityAddressHex);

        var vaultResourceAmount = vaultUpSubstate.SubstateData.GetVaultSubstate().ResourceAmount.GetFungibleResourceAmount();

        var tokens = TokenAttosConverter.Attos2Tokens(TokenAttosConverter.ParseAttosFromString(vaultResourceAmount!.AmountAttos));

        _testConsole.WriteLine($"Account: {accountAddress} balance: {tokens}");

        return Math.Round(tokens, 4);
    }

    public GatewayTestsRunner MockA2BTransferPreviewTransaction()
    {
        var manifest = new ManifestBuilder()
            .WithLockFeeMethod(AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.SystemComponentHrp), "1")
            .WithWithdrawByAmountMethod(AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.AccountComponentHrp), "100",
                AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.ResourceHrp))
            .WithTakeFromWorktopByAmountMethod(AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.ResourceHrp), "100", "bucket1")
            .WithDepositToAccountMethod(AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.AccountComponentHrp), "bucket1")
            .Build();

        var signerPublicKeys = new List<PublicKey>
        {
            new(new EcdsaSecp256k1PublicKey(PublicKeyType.EcdsaSecp256k1.ToString(), "010000000000000000000000000000001")),
        };

        var flags = new TransactionPreviewRequestFlags(false);

        _transactionStreamStore.QueuePreviewTransaction(manifest, 0L, 0L, string.Empty, signerPublicKeys, flags);

        return this;
    }
}
