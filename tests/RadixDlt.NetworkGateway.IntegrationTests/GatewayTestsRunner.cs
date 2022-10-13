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
