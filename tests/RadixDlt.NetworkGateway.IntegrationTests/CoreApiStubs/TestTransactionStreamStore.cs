using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Xunit.Abstractions;
using TransactionStatus = RadixDlt.CoreApiSdk.Model.TransactionStatus;
using TransactionSubmitRequest = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionSubmitRequest;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

[DataContract]
public class TestCommittedTransaction
{
    [DataMember(Name = "state_version")]
    public long StateVersion { get; set; }

    [DataMember(Name = "notarized_transaction")]
    public NotarizedTransaction? NotarizedTransaction { get; set; }

    [DataMember(Name = "receipt")]
    public TransactionReceipt? Receipt { get; set; }
}

public class TestPendingTransaction
{
    public long StateVersion { get; set; }

    [DataMember(Name = "receipt")]
    public TransactionReceipt? Receipt { get; set; }

    public (string? RequestUri, HttpContent? Content, bool MarkAsCommitted) Request { get; set; }

    public bool IsGenesis { get; set; }
}

[DataContract]
public class TestTransactionStreamStore
{
    private readonly CoreApiStub _coreApiStub;
    private readonly StateUpdatesStore _stateUpdatesStore;
    private readonly ITestOutputHelper _testConsole;

    [DataMember(Name = "from_state_version")]
    public long FromStateVersion { get; set; } = 1L;

    [DataMember(Name = "to_state_version")]
    public long ToStateVersion { get; set; } = 1L;

    [DataMember(Name = "max_state_version")]
    public long MaxStateVersion { get; set; } = 1L;

    [DataMember(Name = "transactions")]
    public List<TestCommittedTransaction> CommittedTransactions { get; set; } = new();

    public List<TestPendingTransaction?> PendingTransactions { get; set; } = new();

    public TestTransactionStreamStore(CoreApiStub coreApiStub, StateUpdatesStore stateUpdatesStore, ITestOutputHelper testConsole)
    {
        _coreApiStub = coreApiStub;
        _stateUpdatesStore = stateUpdatesStore;
        _testConsole = testConsole;
    }

    public void QueueGenesisTransaction()
    {
        _testConsole.WriteLine("XRD resource");
        var tokens = new FungibleResourceBuilder(_coreApiStub.CoreApiStubDefaultConfiguration)
            .WithResourceName("XRD")
            .WithFixedAddress(GenesisData.GenesisResourceManagerAddress)
            .WithTotalSupplyAttos(GenesisData.GenesisAmountAttos)
            .Build();

        _stateUpdatesStore.AddStateUpdates(tokens);

        _testConsole.WriteLine("SysFaucet vault");
        var vault = new VaultBuilder(_coreApiStub.CoreApiStubDefaultConfiguration)
            .WithFungibleTokensResourceAddress(tokens.NewGlobalEntities[0].EntityAddressHex)
            .WithFungibleResourceAmountAttos(GenesisData.GenesisAmountAttos)
            .Build();

        _stateUpdatesStore.AddStateUpdates(vault);

        _testConsole.WriteLine("SysFaucet package");
        var faucetPackage = new PackageBuilder(_coreApiStub.CoreApiStubDefaultConfiguration)
            .WithBlueprints(new List<IBlueprint> { new SysFaucetBlueprint() })
            .WithFixedAddress(GenesisData.SysFaucetPackageAddress)
            .Build();

        _stateUpdatesStore.AddStateUpdates(faucetPackage);

        _testConsole.WriteLine("SysFaucet component");
        var componentInfo = new ComponentBuilder(_coreApiStub.CoreApiStubDefaultConfiguration, ComponentHrp.SystemComponentHrp)
            .WithFixedAddress(GenesisData.SysFaucetComponentAddress)
            .WithComponentInfoSubstate(GenesisData.SysFaucetInfoSubstate)
            .Build();

        _stateUpdatesStore.AddStateUpdates(componentInfo);

        _testConsole.WriteLine("System component info");
        var systemComponentInfo = new ComponentBuilder(_coreApiStub.CoreApiStubDefaultConfiguration, ComponentHrp.SystemComponentHrp)
            .WithSystemStateSubstate(epoch: 0L)
            .Build();

        _stateUpdatesStore.AddStateUpdates(systemComponentInfo);

        _testConsole.WriteLine("SysFaucet component state");
        var componentState = new ComponentBuilder(_coreApiStub.CoreApiStubDefaultConfiguration, ComponentHrp.SystemComponentHrp)
            .WithFixedAddress(GenesisData.SysFaucetComponentAddress)
            .WithComponentStateSubstate(
                GenesisData.SysFaucetStateSubstate(vault.NewGlobalEntities[0].EntityAddressHex, "000000000000000000000000000000000000000000000000000000000000000001000000"))
            .Build();

        _stateUpdatesStore.AddStateUpdates(componentState);

        _testConsole.WriteLine("Account package");
        var accountPackage = new PackageBuilder(_coreApiStub.CoreApiStubDefaultConfiguration)
            .WithFixedAddress(GenesisData.AccountPackageAddress)
            .Build();

        _stateUpdatesStore.AddStateUpdates(accountPackage);

        _testConsole.WriteLine("Transaction receipt");
        var transactionReceipt = new TransactionReceiptBuilder()
            .WithStateUpdates(_stateUpdatesStore.StateUpdates)
            .WithTransactionStatus(TransactionStatus.Succeeded)
            .WithFeeSummary(GenesisData.GenesisFeeSummary)
            .Build();

        var json = new TransactionSubmitRequest(new HexTransactions(_coreApiStub.CoreApiStubDefaultConfiguration).SubmitTransactionHex).ToJson();

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        AddPendingTransaction(new TestPendingTransaction()
        {
            StateVersion = MaxStateVersion,
            Receipt = transactionReceipt,
            Request = ("genesis transaction - not api call", content, MarkAsCommitted: true),
            IsGenesis = true,
        });

        MarkPendingTransactionAsCommitted(GetPendingTransaction());
    }

    public void QueueAccountTransaction(string accountAddress, string token, long tokenAmount)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine($"Account: {accountAddress}, {token}, {tokenAmount}");

        var stateUpdatesList = new List<StateUpdates>();

        // CALL_METHOD ComponentAddress("%s") "lock_fee" Decimal("10");
        var feeSummary = _stateUpdatesStore.LockFee();

        // CALL_METHOD ComponentAddress("%s") "free_xrd";
        var freeTokens = _stateUpdatesStore.GetFreeTokens(feeSummary, tokenAmount, out string totalAttos);

        stateUpdatesList.Add(freeTokens);

        // TAKE_FROM_WORKTOP ResourceAddress("%s") Bucket("xrd");
        // CALL_FUNCTION PackageAddress("%s") "Account" "new_with_resource" Enum("AllowAll") Bucket("xrd");

        // build account states
        var account = new AccountBuilder(
                _coreApiStub.CoreApiStubDefaultConfiguration,
                stateUpdatesList, _stateUpdatesStore)
            .WithPublicKey(AddressHelper.GenerateRandomPublicKey())
            .WithFixedAddress(accountAddress)
            .WithTokenName(token)
            .WithTotalAmountAttos(totalAttos)
            .WithComponentInfoSubstate(new ComponentInfoSubstate(
                entityType: EntityType.Component,
                substateType: SubstateType.ComponentInfo,
                packageAddress: GenesisData.AccountPackageAddress,
                blueprintName: GenesisData.AccountBlueprintName))
            .Build();

        stateUpdatesList.Add(account);

        _stateUpdatesStore.AddStateUpdates(stateUpdatesList.Combine());

        QueueSubmitTransaction(stateUpdatesList.Combine(), _stateUpdatesStore.CalculateFeeSummary());
    }

    public void QueueTokensTransferTransaction(string fromAccount, string toAccount, string tokenName, int amountToTransfer)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var stateUpdatesList = new List<StateUpdates>();

        // TODO: update states

        var feeSummarry = _stateUpdatesStore.CalculateFeeSummary();
        QueueSubmitTransaction(stateUpdatesList.Combine(), feeSummarry);

        // take tokens from account A
        _stateUpdatesStore.UpdateAccountBalance(fromAccount, amountToTransfer * (-1), feeSummarry);

        // deposit tokens to account B (sender pays the fees)
        _stateUpdatesStore.UpdateAccountBalance(toAccount, amountToTransfer, null);
    }

    public void QueueRecentTransaction()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var json = new RecentTransactionsRequest().ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        AddPendingTransaction(new TestPendingTransaction()
        {
            StateVersion = MaxStateVersion + 1,
            Receipt = null,
            Request = ("/transaction/recent", content, MarkAsCommitted: false),
        });
    }

    public void QueueGatewayVersions()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        AddPendingTransaction(new TestPendingTransaction()
        {
            StateVersion = MaxStateVersion + 1,
            Receipt = null,
            Request = ("/gateway", JsonContent.Create(new object()), MarkAsCommitted: false),
        });
    }

    public void QueueSubmitTransaction(StateUpdates stateUpdates, FeeSummary feeSummary)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        // TODO: create a new transaction with new substates
        // update from-to-max state versions

        _testConsole.WriteLine("Transaction receipt");
        var transactionReceipt = new TransactionReceiptBuilder()
            .WithStateUpdates(stateUpdates)
            .WithTransactionStatus(TransactionStatus.Succeeded)
            .WithFeeSummary(feeSummary)
            .Build();

        var json = new TransactionSubmitRequest(new HexTransactions(_coreApiStub.CoreApiStubDefaultConfiguration).SubmitTransactionHex).ToJson();

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        AddPendingTransaction(new TestPendingTransaction()
        {
            StateVersion = MaxStateVersion + 1,
            Receipt = transactionReceipt,
            Request = ("/transaction/submit", content, MarkAsCommitted: true),
        });
    }

    public void QueuePreviewTransaction(
        string manifest,
        long costUnitLimit,
        long tipPercentage,
        string nonce,
        List<GatewayApiSdk.Model.PublicKey> signerPublicKeys,
        GatewayApiSdk.Model.TransactionPreviewRequestFlags flags)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        // build TransactionPreviewRequest
        _coreApiStub.CoreApiStubDefaultConfiguration.TransactionPreviewRequest = new GatewayApiSdk.Model.TransactionPreviewRequest(
            manifest: manifest,
            blobsHex: new List<string>(),
            costUnitLimit: costUnitLimit,
            tipPercentage: tipPercentage,
            nonce: nonce,
            signerPublicKeys: signerPublicKeys,
            flags: flags
        );

        var json = _coreApiStub.CoreApiStubDefaultConfiguration.TransactionPreviewRequest.ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // build TransactionPreviewResponse
        var stateUpdatesList = new List<StateUpdates>();

        var tokenStates = new FungibleResourceBuilder(_coreApiStub.CoreApiStubDefaultConfiguration)
            .WithResourceName("PreviewToken")
            .Build();

        stateUpdatesList.Add(tokenStates);

        TransactionReceipt transactionReceipt = new TransactionReceiptBuilder()
            .WithStateUpdates(stateUpdatesList.Combine())
            .WithTransactionStatus(TransactionStatus.Succeeded)
            .Build();

        AddPendingTransaction(new TestPendingTransaction()
        {
            StateVersion = MaxStateVersion,
            Receipt = transactionReceipt,
            Request = ("/transaction/preview", content, MarkAsCommitted: false),
        });

        _coreApiStub.CoreApiStubDefaultConfiguration.TransactionPreviewResponse = new CoreApiSdk.Model.TransactionPreviewResponse(
            transactionReceipt,
            new List<ResourceChange>()
            {
                new(
                    "resource address",
                    "component address",
                    new EntityId(EntityType.Component, "entity address"),
                    amountAttos: "0"),
            },
            logs: new List<TransactionPreviewResponseLogsInner>()
            {
                new("level: debug", "message"),
            });
    }

    public TestPendingTransaction? GetPendingTransaction()
    {
        if (!PendingTransactions.Any())
        {
            return null;
        }

        var pendingTransaction = PendingTransactions.First();

        PendingTransactions.Remove(pendingTransaction);

        return pendingTransaction;
    }

    public void MarkPendingTransactionAsCommitted(TestPendingTransaction? pendingTransaction)
    {
        if (pendingTransaction == null)
        {
            return;
        }

        // update global state versions
        if (!pendingTransaction.IsGenesis)
        {
            UpdateStateVersions();
        }

        // add transaction state updates to the global store
        _stateUpdatesStore.AddStateUpdates(pendingTransaction.Receipt!.StateUpdates);

        CommittedTransactions.Add(new TestCommittedTransaction()
        {
            StateVersion = MaxStateVersion,
            NotarizedTransaction = null, // TODO
            Receipt = pendingTransaction.Receipt,
        });

        _coreApiStub.CoreApiStubDefaultConfiguration.CommittedTransactionsResponse = new CommittedTransactionsResponse(
            fromStateVersion: FromStateVersion,
            toStateVersion: ToStateVersion,
            maxStateVersion: MaxStateVersion,
            new List<CommittedTransaction>
            {
                new(stateVersion: pendingTransaction.StateVersion, notarizedTransaction: null, receipt: pendingTransaction.Receipt),
            }
        );
    }

    public void MarkPendingTransactionAsCompleted(TestPendingTransaction? pendingTransaction)
    {
        PendingTransactions.Remove(pendingTransaction);
    }

    private void AddPendingTransaction(TestPendingTransaction? pendingTransaction)
    {
        PendingTransactions.Add(pendingTransaction);
    }

    private void UpdateStateVersions()
    {
        ToStateVersion += 1;
        MaxStateVersion += 1;
    }
}
