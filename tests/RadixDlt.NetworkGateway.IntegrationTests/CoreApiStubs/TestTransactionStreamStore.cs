using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public TestPendingTransaction()
    {
        StateUpdates = new StateUpdates(
            new List<SubstateId>(),
            new List<UpSubstate>(),
            new List<DownSubstate>(),
            new List<GlobalEntityId>());
    }

    public long StateVersion { get; set; }

    public string Manifest { get; set; } = string.Empty;

    public StateUpdates? StateUpdates { get; set; }

    public (string? RequestUri, HttpContent? Content, bool MarkAsCommitted) Request { get; set; }

    public bool IsGenesis { get; set; }

    public string? IntentHash { get; set; }

    public string? AccountAddress { get; set; }
}

[DataContract]
public class TestTransactionStreamStore
{
    private readonly CoreApiStubRequestsAndResponses _requestsAndResponses;
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

    public TestTransactionStreamStore(CoreApiStubRequestsAndResponses requestsAndResponses, StateUpdatesStore stateUpdatesStore, ITestOutputHelper testConsole)
    {
        _requestsAndResponses = requestsAndResponses;
        _stateUpdatesStore = stateUpdatesStore;
        _testConsole = testConsole;
    }

    public void QueueGenesisTransaction()
    {
        var stateUpdatesList = new List<StateUpdates>();

        _testConsole.WriteLine("XRD resource");
        var tokens = new FungibleResourceBuilder(_stateUpdatesStore.StateUpdates)
            .WithResourceName("XRD")
            .WithFixedAddressHex(GenesisData.GenesisResourceManagerAddressHex)
            .WithTotalSupplyAttos(GenesisData.GenesisAmountAttos)
            .Build();

        stateUpdatesList.Add(tokens);

        _testConsole.WriteLine("SysFaucet vault");
        var vault = new VaultBuilder()
            .WithFungibleTokensResourceAddress(GenesisData.GenesisResourceManagerAddressHex)
            .WithFixedAddressHex(GenesisData.GenesisXrdVaultAddressHex)
            .WithFungibleResourceAmountAttos(GenesisData.GenesisAmountAttos)
            .Build();

        stateUpdatesList.Add(vault);

        _testConsole.WriteLine("SysFaucet package");
        var faucetPackage = new PackageBuilder()
            .WithBlueprints(new List<IBlueprint> { new SysFaucetBlueprint() })
            .WithFixedAddressHex(GenesisData.SysFaucetPackageAddressHex)
            .Build();

        stateUpdatesList.Add(faucetPackage);

        _testConsole.WriteLine("SysFaucet component");
        var componentInfo = new ComponentBuilder(ComponentHrp.SystemComponentHrp)
            .WithFixedAddressHex(GenesisData.SysFaucetComponentAddressHex)
            .WithComponentInfoSubstate(GenesisData.SysFaucetInfoSubstate)
            .Build();

        stateUpdatesList.Add(componentInfo);

        _testConsole.WriteLine("System component info");
        var systemComponentInfo = new ComponentBuilder(ComponentHrp.SystemComponentHrp)
            .WithSystemStateSubstate(epoch: 0L)
            .Build();

        stateUpdatesList.Add(systemComponentInfo);

        _testConsole.WriteLine("SysFaucet component state");
        var componentState = new ComponentBuilder(ComponentHrp.SystemComponentHrp)
            .WithFixedAddressHex(GenesisData.SysFaucetComponentAddressHex)
            .WithComponentStateSubstate(
                GenesisData.SysFaucetStateSubstate(GenesisData.GenesisXrdVaultAddressHex, "000000000000000000000000000000000000000000000000000000000000000001000000"))
            .Build();

        stateUpdatesList.Add(componentState);

        _testConsole.WriteLine("Account package");
        var accountPackage = new PackageBuilder()
            // .WithFixedAddress(GenesisData.AccountPackageAddress)
            .WithFixedAddressHex(GenesisData.AccountPackageAddressHex)
            .Build();

        stateUpdatesList.Add(accountPackage);

        var json = new TransactionSubmitRequest(new HexTransactions().SubmitTransactionHex).ToJson();

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        AddPendingTransaction(new TestPendingTransaction()
        {
            StateVersion = MaxStateVersion,
            StateUpdates = stateUpdatesList.Combine(),
            Request = ("genesis transaction - not api call", content, MarkAsCommitted: true),
            IsGenesis = true,
        });

        MarkPendingTransactionAsCommitted(GetPendingTransaction());
    }

    public void QueueCreateAccountTransaction(string accountAddress, string publicKey, string token, int lockFee)
    {
        var stateUpdatesList = new List<StateUpdates>();

        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine("Transaction receipt");

        // CALL_METHOD ComponentAddress(\"GenesisData.SysFaucetComponentAddress\") \"lock_fee\" Decimal(\"10\");
        // CALL_METHOD ComponentAddress(\"GenesisData.SysFaucetComponentAddress\") \"free_xrd\";
        // TAKE_FROM_WORKTOP ResourceAddress(\"GenesisData.GenesisResourceManagerAddress\") Bucket(\"bucket1\");
        // CALL_FUNCTION PackageAddress(\"GenesisData.AccountPackageAddress\") \"Account\" \"new_with_resource\" Enum(\"Protected\", Enum(\"ProofRule\", Enum(\"Require\", Enum(\"StaticNonFungible\", NonFungibleAddress(\"000000000000000000000000000000000000000000000000000002300721000000${publicKey}\"))))) Bucket(\"bucket1\");

        var manifest = new ManifestBuilder()
            .WithLockFeeMethod(GenesisData.SysFaucetComponentAddress, $"{lockFee}")
            .WithCallMethod(GenesisData.SysFaucetComponentAddress, "free_xrd")
            .WithTakeFromWorktop(GenesisData.GenesisResourceManagerAddress, "bucket1")
            .WithNewAccountWithNonFungibleResource(publicKey, "bucket1")
            .Build();

        var pendingTransaction = QueueSubmitTransaction(manifest: manifest);

        pendingTransaction.AccountAddress = accountAddress;
    }

    public void QueueTokensTransferTransaction(string fromAccount, string toAccount, string tokenName, int amountToTransfer, string transactionIntentHash)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var stateUpdatesList = new List<StateUpdates>();

        // CALL_METHOD ComponentAddress(\"system_tdx_a_1qsqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqs2ufe42\") \"lock_fee\" Decimal(\"10\");\n
        // CALL_METHOD ComponentAddress(\"account_tdx_a_1qvq2ft73ku5d7maxhjraupya3n7ms2984z0l7rtlrnqqf0axcu\") \"withdraw_by_amount\" Decimal(\"100\") ResourceAddress(\"resource_sim1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqu57yag\");\n
        // TAKE_FROM_WORKTOP_BY_AMOUNT Decimal(\"100\") ResourceAddress(\"resource_tdx_a_1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqegh4k9\") Bucket(\"bucket1\");\n
        // CALL_METHOD ComponentAddress(\"account_tdx_a_1qvxvg4rt6w002cqa5akmg7j3xm9r2mkpye25h7d7e3xqeyskss\") \"deposit\" Bucket(\"bucket1\");\n

        // TODO: resource_sim1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqu57yag - who is this?
        var manifest = new ManifestBuilder()
            .WithLockFeeMethod(GenesisData.SysFaucetComponentAddress, "10")
            .WithCallMethod(fromAccount, "withdraw_by_amount",
                new[]
                {
                    $"Decimal(\"{amountToTransfer}\")",
                    $"ResourceAddress(\"resource_sim1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqu57yag\")",
                })
             .WithTakeFromWorktopByAmountMethod(GenesisData.GenesisResourceManagerAddress, amountToTransfer.ToString(), "bucket1")
            .WithDepositToAccountMethod(toAccount, "bucket1")
            .Build();

        QueueSubmitTransaction(manifest, transactionIntentHash);
    }

    public void QueueRecentTransaction()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var json = new RecentTransactionsRequest().ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        AddPendingTransaction(new TestPendingTransaction()
        {
            StateVersion = MaxStateVersion + 1,
            Request = ("/transaction/recent", content, MarkAsCommitted: false),
        });
    }

    public void QueueGatewayVersions()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        AddPendingTransaction(new TestPendingTransaction()
        {
            StateVersion = MaxStateVersion + 1,
            Request = ("/gateway", JsonContent.Create(new object()), MarkAsCommitted: false),
        });
    }

    public TestPendingTransaction QueueSubmitTransaction(string manifest, string? transactionIntentHash = default(string))
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var json = new TransactionSubmitRequest(new HexTransactions().SubmitTransactionHex).ToJson();

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var pendingTransaction = new TestPendingTransaction()
        {
            StateVersion = MaxStateVersion + 1, Manifest = manifest, Request = ("/transaction/submit", content, MarkAsCommitted: true), IntentHash = transactionIntentHash,
        };

        AddPendingTransaction(pendingTransaction);

        return pendingTransaction;
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
        var transactionPreviewRequest = new GatewayApiSdk.Model.TransactionPreviewRequest(
            manifest: manifest,
            blobsHex: new List<string>(),
            costUnitLimit: costUnitLimit,
            tipPercentage: tipPercentage,
            nonce: nonce,
            signerPublicKeys: signerPublicKeys,
            flags: flags
        );

        var json = transactionPreviewRequest.ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // build TransactionPreviewResponse
        var stateUpdatesList = new List<StateUpdates>();

        var tokenStates = new FungibleResourceBuilder(_stateUpdatesStore.StateUpdates)
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
            StateUpdates = stateUpdatesList.Combine(),
            Request = ("/transaction/preview", content, MarkAsCommitted: false),
        });

        _requestsAndResponses.TransactionPreviewResponse = new CoreApiSdk.Model.TransactionPreviewResponse(
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

    public TestCommittedTransaction? MarkPendingTransactionAsCommitted(TestPendingTransaction? pendingTransaction)
    {
        TransactionReceipt? transactionReceipt = null;

        if (pendingTransaction == null)
        {
            return null;
        }

        // add transaction state updates to the global store
        if (pendingTransaction.StateUpdates != null)
        {
            _stateUpdatesStore.AddStateUpdates(pendingTransaction.StateUpdates);
        }

        // update global state versions
        if (pendingTransaction.IsGenesis)
        {
            _testConsole.WriteLine("Genesis transaction receipt");
            transactionReceipt = new TransactionReceiptBuilder()
                .WithStateUpdates(pendingTransaction.StateUpdates!)
                .WithTransactionStatus(TransactionStatus.Succeeded)
                .WithFeeSummary(GenesisData.GenesisFeeSummary)
                .Build();

            CommittedTransactions.Add(new TestCommittedTransaction()
            {
                StateVersion = MaxStateVersion,
                NotarizedTransaction = null, // TODO
                Receipt = transactionReceipt,
            });
        }
        else
        {
            UpdateStateVersions();
            transactionReceipt = IssueTransactionReceipt(pendingTransaction);
        }

        var committedTransaction = new TestCommittedTransaction()
        {
            StateVersion = MaxStateVersion,
            NotarizedTransaction = null, // TODO
            Receipt = transactionReceipt,
        };

        CommittedTransactions.Add(committedTransaction);

        return committedTransaction;
    }

    public void MarkPendingTransactionAsCompleted(TestPendingTransaction? pendingTransaction)
    {
        PendingTransactions.Remove(pendingTransaction);
    }

    public Task<CommittedTransactionsResponse> GetTransactions(long fromStateVersion, int count)
    {
        var toStateVersion = Math.Min(MaxStateVersion, fromStateVersion + count);

        var transactions = CommittedTransactions.Where(t => t.StateVersion >= fromStateVersion && t.StateVersion <= toStateVersion).Select(t => new CommittedTransaction(t.StateVersion, null, t.Receipt)).ToList();

        return Task.FromResult(new CommittedTransactionsResponse(
            fromStateVersion: fromStateVersion,
            toStateVersion: toStateVersion,
            maxStateVersion: MaxStateVersion,
            transactions
        ));
    }

    public Task<NetworkConfigurationResponse> GetNetworkConfiguration(CancellationToken token)
    {
        // TODO: is there an api call to fetch the configuration?
        return Task.FromResult(new NetworkConfigurationResponse(
            new NetworkConfigurationResponseVersion(_requestsAndResponses.CoreVersion, _requestsAndResponses.ApiVersion),
            GenesisData.NetworkDefinition.LogicalName,
            GenesisData.NetworkDefinition.HrpSuffix));
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

    private TransactionReceipt IssueTransactionReceipt(TestPendingTransaction pendingTransaction)
    {
        var tempAllStateUpdates = new StateUpdates(
            _stateUpdatesStore.StateUpdates.DownVirtualSubstates,
            _stateUpdatesStore.StateUpdates.UpSubstates,
            _stateUpdatesStore.StateUpdates.DownSubstates,
            _stateUpdatesStore.StateUpdates.NewGlobalEntities);

        var stateUpdatesList = new List<StateUpdates>();

        var newVaultTotalAttos = string.Empty;

        BigInteger tokensAmountToTransferAttos = 0;

        if (pendingTransaction.StateUpdates != null)
        {
            stateUpdatesList.Add(pendingTransaction.StateUpdates);
        }

        // default fee summary
        FeeSummary feeSummary = new FeeSummary(
            GenesisData.GenesisFeeSummary.LoanFullyRepaid,
            GenesisData.GenesisFeeSummary.CostUnitLimit,
            GenesisData.GenesisFeeSummary.CostUnitConsumed,
            GenesisData.GenesisFeeSummary.CostUnitPriceAttos,
            GenesisData.GenesisFeeSummary.TipPercentage,
            GenesisData.GenesisFeeSummary.XrdBurnedAttos,
            GenesisData.GenesisFeeSummary.XrdTippedAttos);

        var manifestInstructions = ManifestParser.Parse(pendingTransaction.Manifest);

        foreach (var instruction in manifestInstructions)
        {
            switch (instruction.OpCode)
            {
                case InstructionOp.LockFee:
                    _testConsole.WriteLine($"Locking fees on {instruction.Address}");
                    feeSummary = _stateUpdatesStore.CalculateFeeSummary(); // _stateUpdatesStore.LockFee();
                    break;

                case InstructionOp.FreeXrd:
                    _testConsole.WriteLine($"Taking 1000 tokens from vault owned by {instruction.Address}");
                    var freeTokens = tempAllStateUpdates.TakeTokensFromVault(GenesisData.SysFaucetComponentAddress, feeSummary, 1000, out newVaultTotalAttos);

                    stateUpdatesList.Add(freeTokens);
                    break;

                case InstructionOp.TakeFromWorktop:
                    {
                        var resourceAddress = instruction.Address;
                        var bucketName = instruction.Parameters[0].Value;

                        _testConsole.WriteLine($"TakeFromWorktop: Moving tokens to bucket '{bucketName}'");

                        // create a new vault up and down substates

                        var resourceXrdVaultAddressHex = string.Empty;

                        if (resourceAddress == GenesisData.GenesisResourceManagerAddress)
                        {
                            resourceXrdVaultAddressHex = GenesisData.GenesisXrdVaultAddressHex;
                        }

                        var version = tempAllStateUpdates.GetLastUpSubstateByEntityAddressHex(resourceXrdVaultAddressHex)._Version;

                        var vault = new VaultBuilder()
                            .WithFixedAddressHex(resourceXrdVaultAddressHex)
                            .WithFungibleResourceAmountAttos(newVaultTotalAttos)
                            .WithDownState(new DownSubstate(
                                new SubstateId(
                                    EntityType.Vault,
                                    resourceXrdVaultAddressHex,
                                    SubstateType.Vault,
                                    Convert.ToHexString(Encoding.UTF8.GetBytes("substateKeyHex")).ToLowerInvariant()
                                ), substateDataHash: "hash", version)
                            ).Build();

                        stateUpdatesList.Add(vault);
                    }

                    break;

                case InstructionOp.TakeFromWorktopByAmount:
                    break;

                case InstructionOp.CreateNewAccount:
                    {
                        _testConsole.WriteLine($"CreateNewAccount: Creating new account {pendingTransaction.AccountAddress}");

                        // build account states
                        var account = new AccountBuilder(tempAllStateUpdates)
                            .WithPublicKey(AddressHelper.GenerateRandomPublicKey())
                            .WithFixedAddress(pendingTransaction.AccountAddress!)
                            .WithTokenName("XRD")
                            .WithTotalAmountAttos(newVaultTotalAttos)
                            .WithComponentInfoSubstate(new ComponentInfoSubstate(
                                entityType: EntityType.Component,
                                substateType: SubstateType.ComponentInfo,
                                packageAddress: GenesisData.AccountPackageAddress,
                                blueprintName: GenesisData.AccountBlueprintName))
                            .Build();

                        stateUpdatesList.Add(account);
                    }

                    break;

                case InstructionOp.WithdrawByAmount:
                    {
                        var accountAddress = instruction.Address;

                        tokensAmountToTransferAttos = TokenAttosConverter.Tokens2Attos(instruction.Parameters[0].Value);

                        var tokensToWithdraw = TokenAttosConverter.Attos2Tokens(tokensAmountToTransferAttos);

                        _testConsole.WriteLine($"WithdrawByAmount: Withdrawing {tokensToWithdraw} tokens from account {accountAddress}");

                        // faucet vault's up and down substates
                        // TODO: who is paying the fees? faucet or sender?
                        var faucetVault = tempAllStateUpdates.TakeTokensFromVault(GenesisData.SysFaucetComponentAddress, feeSummary, 0, out newVaultTotalAttos);

                        stateUpdatesList.Add(faucetVault);

                        // account's vault up and down substates
                        var accountVault = tempAllStateUpdates.TakeTokensFromVault(accountAddress!, feeSummary, tokensToWithdraw, out newVaultTotalAttos);

                        stateUpdatesList.Add(accountVault);
                    }

                    break;

                case InstructionOp.Deposit:
                    {
                        var accountAddress = instruction.Address;
                        var bucketName1 = instruction.Parameters[0].Value;

                        var tokensToDeposit = TokenAttosConverter.Attos2Tokens(tokensAmountToTransferAttos);

                        _testConsole.WriteLine($"Deposit: Depositing {tokensToDeposit} tokens to account {accountAddress}");

                        var accountVaultDownSubstate = tempAllStateUpdates.GetLastVaultDownSubstateByEntityAddress(GenesisData.SysFaucetComponentAddress);

                        var accountVaultUpSubstate = tempAllStateUpdates.GetLastVaultUpSubstateByEntityAddress(accountAddress);

                        var downVirtualSubstates = new List<SubstateId>();
                        var downSubstates = new List<DownSubstate?>();
                        var upSubstates = new List<UpSubstate>();

                        var globalEntityIds = new List<GlobalEntityId>();

                        // add new vault down substate
                        var newAccountVaultDownSubstate = accountVaultDownSubstate.CloneSubstate();
                        if (newAccountVaultDownSubstate != null)
                        {
                            newAccountVaultDownSubstate._Version = accountVaultUpSubstate._Version;
                            downSubstates.Add(newAccountVaultDownSubstate);
                        }

                        // add new vault up state
                        var newAccountVaultUpSubstate = accountVaultUpSubstate.CloneSubstate();
                        newAccountVaultUpSubstate._Version += 1;

                        var newAccountVaultSubstate = newAccountVaultUpSubstate.SubstateData.GetVaultSubstate();

                        var vaultResourceAmount = newAccountVaultSubstate.ResourceAmount.GetFungibleResourceAmount();
                        var vaultResourceAmountAttos = TokenAttosConverter.ParseAttosFromString(vaultResourceAmount.AmountAttos);

                        // a receiver doesn't pay fees, right?
                        // var feesAttos = feeSummary.CostUnitConsumed
                        //                 * TokenAttosConverter.String2Attos(feeSummary.CostUnitPriceAttos);
                        //
                        // var newAttosBalance = vaultResourceAmountAttos - tokenAmountAttos - feesAttos;

                        var newAttosBalance = vaultResourceAmountAttos + tokensAmountToTransferAttos;

                        vaultResourceAmount!.AmountAttos = newAttosBalance.ToString();

                        newVaultTotalAttos = tokensAmountToTransferAttos.ToString();

                        upSubstates.Add(newAccountVaultUpSubstate);

                        stateUpdatesList.Add(new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, globalEntityIds));
                    }

                    break;
            }

            tempAllStateUpdates = tempAllStateUpdates.Add(stateUpdatesList.Combine());
        }

        var transactionReceipt = new TransactionReceiptBuilder()
            .WithStateUpdates(stateUpdatesList.Combine())
            .WithFeeSummary(feeSummary)
            .WithTransactionStatus(TransactionStatus.Succeeded)
            .Build();

        // add new state updates to the global store
        _stateUpdatesStore.StateUpdates = _stateUpdatesStore.StateUpdates.Add(stateUpdatesList.Combine());

        return transactionReceipt;
    }
}
