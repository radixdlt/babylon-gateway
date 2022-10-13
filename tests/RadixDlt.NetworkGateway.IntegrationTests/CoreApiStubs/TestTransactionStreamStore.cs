/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

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
using PublicKey = RadixDlt.NetworkGateway.GatewayApiSdk.Model.PublicKey;
using TransactionPreviewRequest = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionPreviewRequest;
using TransactionPreviewRequestFlags = RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionPreviewRequestFlags;
using TransactionPreviewResponse = RadixDlt.CoreApiSdk.Model.TransactionPreviewResponse;
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

    public string AccountAddress { get; set; } = string.Empty;
}

[DataContract]
public class TestTransactionStreamStore
{
    private readonly CoreApiStubRequestsAndResponses _requestsAndResponses;
    private readonly StateUpdatesStore _stateUpdatesStore;
    private readonly ITestOutputHelper _testConsole;

    public TestTransactionStreamStore(CoreApiStubRequestsAndResponses requestsAndResponses, StateUpdatesStore stateUpdatesStore, ITestOutputHelper testConsole)
    {
        _requestsAndResponses = requestsAndResponses;
        _stateUpdatesStore = stateUpdatesStore;
        _testConsole = testConsole;
    }

    [DataMember(Name = "from_state_version")]
    public long FromStateVersion { get; set; } = 1L;

    [DataMember(Name = "to_state_version")]
    public long ToStateVersion { get; set; } = 1L;

    [DataMember(Name = "max_state_version")]
    public long MaxStateVersion { get; set; } = 1L;

    [DataMember(Name = "transactions")]
    public List<TestCommittedTransaction> CommittedTransactions { get; set; } = new();

    public List<TestPendingTransaction?> PendingTransactions { get; set; } = new();

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
            .WithFixedAddressHex(GenesisData.GenesisXrdVaultAddressHex)
            .WithFungibleTokensResourceAddress(GenesisData.GenesisResourceManagerAddress)
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
            .WithSystemStateSubstate(0L)
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

        AddPendingTransaction(new TestPendingTransaction
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

        var pendingTransaction = QueueSubmitTransaction(manifest);

        pendingTransaction.AccountAddress = accountAddress;
    }

    public void QueueTokensTransferTransaction(string fromAccount, string toAccount, string tokenName, int amountToTransfer, string transactionIntentHash)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        // CALL_METHOD ComponentAddress(\"GenesisData.SysFaucetComponentAddress\") \"lock_fee\" Decimal(\"10\");\n
        // CALL_METHOD ComponentAddress(\"fromAccount\") \"withdraw_by_amount\" Decimal(\"100\") ResourceAddress(\"resource_sim1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqu57yag\");\n
        // TAKE_FROM_WORKTOP_BY_AMOUNT Decimal(\"100\") ResourceAddress(\"resource_tdx_a_1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqegh4k9\") Bucket(\"bucket1\");\n
        // CALL_METHOD ComponentAddress(\"toAccount\") \"deposit\" Bucket(\"bucket1\");\n

        // TODO: resource_sim1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqu57yag - who is this?
        var manifest = new ManifestBuilder()
            .WithLockFeeMethod(GenesisData.SysFaucetComponentAddress, "10")
            .WithCallMethod(fromAccount, "withdraw_by_amount",
                new[] { $"Decimal(\"{amountToTransfer}\")", "ResourceAddress(\"resource_sim1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqu57yag\")" })
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

        AddPendingTransaction(new TestPendingTransaction { StateVersion = MaxStateVersion + 1, Request = ("/transaction/recent", content, MarkAsCommitted: false) });
    }

    public void QueueGatewayVersions()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        AddPendingTransaction(new TestPendingTransaction { StateVersion = MaxStateVersion + 1, Request = ("/gateway", JsonContent.Create(new object()), MarkAsCommitted: false) });
    }

    public TestPendingTransaction QueueSubmitTransaction(string manifest, string? transactionIntentHash = default)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var json = new TransactionSubmitRequest(new HexTransactions().SubmitTransactionHex).ToJson();

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var pendingTransaction = new TestPendingTransaction
        {
            StateVersion = MaxStateVersion + 1,
            Manifest = manifest,
            Request = ("/transaction/submit", content, MarkAsCommitted: true),
            IntentHash = transactionIntentHash,
        };

        AddPendingTransaction(pendingTransaction);

        return pendingTransaction;
    }

    public void QueuePreviewTransaction(
        string manifest,
        long costUnitLimit,
        long tipPercentage,
        string nonce,
        List<PublicKey> signerPublicKeys,
        TransactionPreviewRequestFlags flags)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var transactionPreviewRequest = new TransactionPreviewRequest(
            manifest,
            new List<string>(),
            costUnitLimit,
            tipPercentage,
            nonce,
            signerPublicKeys,
            flags
        );

        var json = transactionPreviewRequest.ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var stateUpdatesList = new List<StateUpdates>();

        var tokenStates = new FungibleResourceBuilder(_stateUpdatesStore.StateUpdates)
            .WithResourceName("PreviewToken")
            .Build();

        stateUpdatesList.Add(tokenStates);

        var transactionReceipt = new TransactionReceiptBuilder()
            .WithStateUpdates(stateUpdatesList.Combine())
            .WithTransactionStatus(TransactionStatus.Succeeded)
            .Build();

        AddPendingTransaction(new TestPendingTransaction
        {
            StateVersion = MaxStateVersion,
            StateUpdates = stateUpdatesList.Combine(),
            Request = ("/transaction/preview", content, MarkAsCommitted: false),
        });

        _requestsAndResponses.TransactionPreviewResponse = new TransactionPreviewResponse(
            transactionReceipt,
            new List<ResourceChange>
            {
                new(
                    "resource address",
                    "component address",
                    new EntityId(EntityType.Component, "entity address"),
                    "0"),
            },
            new List<TransactionPreviewResponseLogsInner> { new("level: debug", "message") });
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

    public void MarkPendingTransactionAsFailed(TestPendingTransaction? pendingTransaction)
    {
        if (pendingTransaction == null)
        {
            return;
        }

        // TODO: should we add failed transaction state updates to the global store
        if (pendingTransaction.StateUpdates != null)
        {
            _stateUpdatesStore.AddStateUpdates(pendingTransaction.StateUpdates);
        }

        PendingTransactions.Remove(pendingTransaction);
    }

    public void MarkPendingTransactionAsCommitted(TestPendingTransaction? pendingTransaction)
    {
        if (pendingTransaction == null)
        {
            return;
        }

        // add transaction state updates to the global store
        if (pendingTransaction.StateUpdates != null)
        {
            _stateUpdatesStore.AddStateUpdates(pendingTransaction.StateUpdates);
        }

        TransactionReceipt? transactionReceipt;

        // update global state versions
        if (pendingTransaction.IsGenesis)
        {
            _testConsole.WriteLine("Genesis transaction receipt");
            transactionReceipt = new TransactionReceiptBuilder()
                .WithStateUpdates(pendingTransaction.StateUpdates!)
                .WithTransactionStatus(TransactionStatus.Succeeded)
                .WithFeeSummary(GenesisData.GenesisFeeSummary)
                .Build();
        }
        else
        {
            UpdateStateVersions();
            transactionReceipt = IssueTransactionReceipt(pendingTransaction);
        }

        var committedTransaction = new TestCommittedTransaction
        {
            StateVersion = MaxStateVersion,
            NotarizedTransaction = null, // TODO
            Receipt = transactionReceipt,
        };

        CommittedTransactions.Add(committedTransaction);
    }

    public void MarkPendingTransactionAsCompleted(TestPendingTransaction? pendingTransaction)
    {
        PendingTransactions.Remove(pendingTransaction);
    }

    public Task<CommittedTransactionsResponse> GetTransactions(long fromStateVersion, int count)
    {
        var toStateVersion = Math.Min(MaxStateVersion, fromStateVersion + count);

        var transactions = CommittedTransactions.Where(t => t.StateVersion >= fromStateVersion && t.StateVersion <= toStateVersion)
            .Select(t => new CommittedTransaction(t.StateVersion, null, t.Receipt)).ToList();

        return Task.FromResult(new CommittedTransactionsResponse(
            fromStateVersion,
            toStateVersion,
            MaxStateVersion,
            transactions
        ));
    }

    public Task<NetworkConfigurationResponse> GetNetworkConfiguration(CancellationToken token)
    {
        // TODO: Do we want to use a network configuration of one the active networks? /v0/status/network-configuration
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

        var transactionStateUpdatesList = new List<StateUpdates>();

        var newFaucetBalanceAttos = string.Empty;
        var newAccountBalanceAttos = string.Empty;

        BigInteger tokensAmountToTransferAttos = 0;

        if (pendingTransaction.StateUpdates != null)
        {
            transactionStateUpdatesList.Add(pendingTransaction.StateUpdates);
        }

        var feeSummary = GenesisData.GenesisFeeSummary;

        var manifestInstructions = ManifestParser.Parse(pendingTransaction.Manifest);

        foreach (var instruction in manifestInstructions)
        {
            if (instruction.OpCode == InstructionOp.CreateNewAccount)
            {
                instruction.Address = pendingTransaction.AccountAddress;
            }

            ProcessInstruction(instruction, ref feeSummary, tempAllStateUpdates, transactionStateUpdatesList, ref newFaucetBalanceAttos, ref newAccountBalanceAttos,
                ref tokensAmountToTransferAttos);

            tempAllStateUpdates = tempAllStateUpdates.Add(transactionStateUpdatesList.Combine());
        }

        var transactionReceipt = new TransactionReceiptBuilder()
            .WithStateUpdates(transactionStateUpdatesList.Combine())
            .WithFeeSummary(feeSummary)
            .WithTransactionStatus(TransactionStatus.Succeeded)
            .Build();

        // add new state updates to the global store
        _stateUpdatesStore.StateUpdates = _stateUpdatesStore.StateUpdates.Add(transactionStateUpdatesList.Combine());

        return transactionReceipt;
    }

    private void ProcessInstruction(Instruction instruction, ref FeeSummary feeSummary, StateUpdates tempAllStateUpdates, List<StateUpdates> transactionStateUpdatesList,
        ref string newFaucetBalanceAttos, ref string newAccountBalanceAttos, ref BigInteger tokensAmountToTransferAttos)
    {
        switch (instruction.OpCode)
        {
            case InstructionOp.LockFee:
                _testConsole.WriteLine($"Locking fees on {instruction.Address}");
                feeSummary = _stateUpdatesStore.CalculateFeeSummary(); // _stateUpdatesStore.LockFee();
                break;

            case InstructionOp.FreeXrd:
                var defaultAccountBalance = 1000;
                _testConsole.WriteLine($"Freeing {defaultAccountBalance} tokens from sys faucet");

                var freeTokens = tempAllStateUpdates.TakeTokensFromVault(GenesisData.SysFaucetComponentAddress, feeSummary, defaultAccountBalance, out newFaucetBalanceAttos);

                _testConsole.WriteLine($"New faucet balance: {TokenAttosConverter.Attos2Tokens(newFaucetBalanceAttos):#,###.##}");

                newAccountBalanceAttos = TokenAttosConverter.Tokens2Attos(defaultAccountBalance).ToString();

                transactionStateUpdatesList.Add(freeTokens);

                break;

            case InstructionOp.TakeFromWorktop:
                var bucketName = instruction.Parameters[0].Value;
                _testConsole.WriteLine($"TakeFromWorktop: Moving tokens to bucket '{bucketName}'");
                break;

            case InstructionOp.TakeFromWorktopByAmount:
                var bucketNameBy = instruction.Parameters[0].Value;
                _testConsole.WriteLine($"TakeFromWorktopByAmount: Moving tokens to bucket '{bucketNameBy}'");
                break;

            case InstructionOp.CreateNewAccount:
                {
                    _testConsole.WriteLine($"CreateNewAccount: Creating new account {instruction.Address}");

                    // build account states
                    var account = new AccountBuilder(tempAllStateUpdates)
                        .WithPublicKey(AddressHelper.GenerateRandomPublicKey())
                        .WithFixedAddress(instruction.Address)
                        .WithTokenName("XRD")
                        .WithTotalAmountAttos(newAccountBalanceAttos)
                        .WithComponentInfoSubstate(new ComponentInfoSubstate(
                            EntityType.Component,
                            SubstateType.ComponentInfo,
                            GenesisData.AccountPackageAddress,
                            GenesisData.AccountBlueprintName))
                        .Build();

                    _testConsole.WriteLine($"New account {instruction.Address} balance is: {TokenAttosConverter.Attos2Tokens(newAccountBalanceAttos):#.##}");

                    transactionStateUpdatesList.Add(account);
                }

                break;

            case InstructionOp.WithdrawByAmount:
                {
                    if (GenesisData.NetworkDefinition.Id == (int)NetworkEnum.Adapanet)
                    {
                        // faucet vault's up and down substates (faucet pays the fees on aplhanet)
                        WithdrawByAmount(ref transactionStateUpdatesList, tempAllStateUpdates, feeSummary, GenesisData.SysFaucetComponentAddress, 0, out newFaucetBalanceAttos);
                    }

                    tokensAmountToTransferAttos = TokenAttosConverter.Tokens2Attos(instruction.Parameters[0].Value);

                    var tokensToWithdraw = TokenAttosConverter.Attos2Tokens(tokensAmountToTransferAttos);

                    WithdrawByAmount(ref transactionStateUpdatesList, tempAllStateUpdates, GenesisData.GenesisFeeSummary, instruction.Address, tokensToWithdraw,
                        out newAccountBalanceAttos);

                    _testConsole.WriteLine($"New account {instruction.Address} balance is: {TokenAttosConverter.Attos2Tokens(newAccountBalanceAttos):#.##}");
                }

                break;

            case InstructionOp.Deposit:
                var tokensToDeposit = TokenAttosConverter.Attos2Tokens(tokensAmountToTransferAttos);

                DepositToAccount(ref transactionStateUpdatesList, tempAllStateUpdates, instruction.Address, tokensToDeposit, out newAccountBalanceAttos);

                _testConsole.WriteLine($"New account {instruction.Address} balance is: {TokenAttosConverter.Attos2Tokens(newAccountBalanceAttos):#.##}");

                break;
        }
    }

    private void WithdrawByAmount(
        ref List<StateUpdates> transactionStateUpdatesList,
        StateUpdates allStateUpdates,
        FeeSummary feeSummary,
        string accountAddress,
        double tokensToWithdraw,
        out string newVaultTotalAttos)
    {
        var feesAttos = feeSummary.CostUnitConsumed
                        * TokenAttosConverter.ParseAttosFromString(feeSummary.CostUnitPriceAttos);

        var totalAttosToWithdraw = TokenAttosConverter.Tokens2Attos(tokensToWithdraw) + feesAttos;

        _testConsole.WriteLine($"WithdrawByAmount: withdrawing {TokenAttosConverter.Attos2Tokens(totalAttosToWithdraw)} tokens from account {accountAddress}");

        // account's vault up and down substates
        var accountVault = allStateUpdates.TakeTokensFromVault(accountAddress!, feeSummary, tokensToWithdraw, out newVaultTotalAttos);

        transactionStateUpdatesList.Add(accountVault);
    }

    private void DepositToAccount(
        ref List<StateUpdates> transactionStateUpdatesList,
        StateUpdates allStateUpdates,
        string accountAddress,
        double tokensToDeposit,
        out string newAccountBalanceAttos)
    {
        _testConsole.WriteLine($"Deposit: Depositing {tokensToDeposit} tokens to account {accountAddress}");

        var accountVaultDownSubstate = allStateUpdates.GetLastVaultDownSubstateByEntityAddress(GenesisData.SysFaucetComponentAddress);

        var accountVaultUpSubstate = allStateUpdates.GetLastVaultUpSubstateByEntityAddress(accountAddress);

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

        newAccountBalanceAttos = (vaultResourceAmountAttos + TokenAttosConverter.Tokens2Attos(tokensToDeposit)).ToString();

        vaultResourceAmount!.AmountAttos = newAccountBalanceAttos;

        upSubstates.Add(newAccountVaultUpSubstate);

        transactionStateUpdatesList.Add(new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, globalEntityIds));
    }
}
