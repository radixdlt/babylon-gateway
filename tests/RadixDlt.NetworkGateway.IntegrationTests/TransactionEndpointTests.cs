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

using FluentAssertions;
using Newtonsoft.Json;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace RadixDlt.NetworkGateway.IntegrationTests;

[Collection("Gateway Api integration tests")]
public class TransactionEndpointTests
{
    private readonly ITestOutputHelper _testConsole;

    public TransactionEndpointTests(ITestOutputHelper testConsole)
    {
        _testConsole = testConsole;
        GenesisData.NetworkDefinition = NetworkDefinition.Get(NetworkEnum.IntegrationTests);
    }

    [Fact]
    public void TestTransactionRecent()
    {
        // Arrange
        using var gatewayRunner = new GatewayTestsRunner(MethodBase.GetCurrentMethod()!.Name, _testConsole)
            .MockGenesis()
            .MockRecentTransactions();

        // Act
        var task = gatewayRunner
            .RunAndWaitUntilAllTransactionsIngested<RecentTransactionsResponse>(callback: ValidateResponse);
        task.Wait();

        // Assert (callback method)
        void ValidateResponse(RecentTransactionsResponse payload, string intentHash)
        {
            _testConsole.WriteLine($"Validating {payload.GetType().Name} response");
            payload.Transactions.ShouldNotBeNull();
            payload.Transactions.Count.Should().BeGreaterThan(0);

            payload.LedgerState.ShouldNotBeNull();
            payload.LedgerState.Network.Should().Be(GenesisData.NetworkDefinition.LogicalName);
            payload.LedgerState._Version.Should().Be(1);
        }
    }

    [Fact]
    public void TestTransactionPreviewShouldPass()
    {
        // Arrange A2B Transfer preview
        using var gatewayRunner = new GatewayTestsRunner(MethodBase.GetCurrentMethod()!.Name, _testConsole)
            .MockGenesis()
            .MockA2BTransferPreviewTransaction();

        // Act
        var task = gatewayRunner
            .RunAndWaitUntilAllTransactionsIngested<TransactionPreviewResponse>(callback: ValidateResponse);
        task.Wait();

        // Assert (callback method)
        void ValidateResponse(TransactionPreviewResponse payload, string intentHash)
        {
            _testConsole.WriteLine($"Validating {payload.GetType().Name} response");
            var coreApiPayload = JsonConvert.DeserializeObject<RadixDlt.CoreApiSdk.Model.TransactionPreviewResponse>(payload.CoreApiResponse.ToString()!);

            // Assert
            coreApiPayload.ShouldNotBeNull();
            coreApiPayload.Receipt.ShouldNotBeNull();
            coreApiPayload.Receipt.Status.Should().Be(CoreApiSdk.Model.TransactionStatus.Succeeded);
        }

        gatewayRunner.SaveStateUpdatesToFile();
    }

    // [Fact(Skip = "Disabled until MempoolTrackerWorker is re-enabled")]
    // public void MempoolTransactionStatusShouldBeFailed()
    // {
    //     // Arrange
    //     using var gatewayRunner = new GatewayTestsRunner(_networkDefinition, MethodBase.GetCurrentMethod()!.Name, _testConsole)
    //         .MockGenesis()
    //         .ArrangeMempoolTransactionStatusTest(TransactionStatus.StatusEnum.FAILED);
    //
    //     // Act
    //     var task = gatewayRunner
    //         .RunAndWaitUntilAllTransactionsAreIngested<TransactionStatusResponse>();
    //
    //     task.Wait();
    //     var payload = task.Result;
    //
    //     // Assert
    //     var status = payload.Transaction.TransactionStatus.Status;
    //     status.Should().Be(TransactionStatus.StatusEnum.FAILED);
    // }
    //
    // [Fact(Skip = "Disabled until MempoolTrackerWorker is re-enabled")]
    // public void MempoolTransactionStatusShouldBeConfirmed()
    // {
    //     // Arrange
    //     using var gatewayRunner = new GatewayTestsRunner(_networkDefinition, MethodBase.GetCurrentMethod()!.Name, _testConsole)
    //         .MockGenesis()
    //         .ArrangeMempoolTransactionStatusTest(TransactionStatus.StatusEnum.CONFIRMED);
    //
    //     // Act
    //     var task = gatewayRunner
    //         .RunAndWaitUntilAllTransactionsAreIngested<TransactionStatusResponse>();
    //
    //     task.Wait();
    //     var payload = task.Result;
    //
    //     // Assert
    //     var status = payload.Transaction.TransactionStatus.Status;
    //     status.Should().Be(TransactionStatus.StatusEnum.CONFIRMED);
    // }
    //
    // [Fact(Skip = "Disabled until MempoolTrackerWorker is re-enabled")]
    // public void MempoolTransactionStatusShouldBePending()
    // {
    //     // Arrange
    //     var gatewayRunner = new GatewayTestsRunner(_networkDefinition, MethodBase.GetCurrentMethod()!.Name, _testConsole)
    //         .MockGenesis()
    //         .ArrangeMempoolTransactionStatusTest(TransactionStatus.StatusEnum.PENDING);
    //
    //     // Act
    //     var task = gatewayRunner
    //         .RunAndWaitUntilAllTransactionsAreIngested<TransactionStatusResponse>();
    //
    //     task.Wait();
    //     var payload = task.Result;
    //
    //     // Assert
    //     var status = payload.Transaction.TransactionStatus.Status;
    //     status.Should().Be(TransactionStatus.StatusEnum.PENDING);
    // }

    [Fact]
    public void TestTransactionSubmit()
    {
        // Arrange
        using var gatewayRunner = new GatewayTestsRunner(MethodBase.GetCurrentMethod()!.Name, _testConsole)
            .MockGenesis()
            .MockSubmitTransaction();

        // Act
        var task = gatewayRunner
            .RunAndWaitUntilAllTransactionsIngested<TransactionSubmitResponse>(callback: ValidateResponse);
        task.Wait();

        // Assert (callback method)
        void ValidateResponse(TransactionSubmitResponse payload, string intentHash)
        {
            _testConsole.WriteLine($"Validating {payload.GetType().Name} response");
            payload.Duplicate.Should().Be(false);

            // TODO: should also return intent hash
            // payload.IntentHash.ShouldNoBreNull();
        }
    }

    // [Fact(Skip ="TransactionSubmitResponse and/or RecentTransactionsResponse should return IntentHash")]
    // public void SubmittedTransactionStatusShouldBeConfirmed()
    // {
    //     // Arrange
    //     using var gatewayRunner = new GatewayTestsRunner(_networkDefinition, MethodBase.GetCurrentMethod()!.Name, _testConsole)
    //         .MockGenesis()
    //         .MockRecentTransactions();
    //
    //     // Act
    //     var taskRecent = gatewayRunner
    //         .RunAndWaitUntilAllTransactionsAreIngested<RecentTransactionsResponse>();
    //
    //     taskRecent.Wait();
    //     var recentTransactions = taskRecent.Result;
    //
    //     var taskAct = gatewayRunner.ArrangeTransactionStatusTest(recentTransactions)
    //         .RunAndWaitUntilAllTransactionsAreIngested<TransactionStatusResponse>();
    //     taskAct.Wait();
    //     var payload = taskAct.Result;
    //
    //     // Assert
    //     var status = payload.Transaction.TransactionStatus.Status;
    //     status.Should().Be(TransactionStatus.StatusEnum.CONFIRMED);
    // }

    [Fact]
    public void TokensTransferFromAccountAtoBShouldSucceed()
    {
        // Arrange
        var accountAAddress = AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.AccountComponentHrp);
        var accountAPublicKey = "0279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798";
        // var createAccountATransactionIntentHash = "f3949c58ea6f9c1e5bb0b917ae190d4a695527e842acda44bc1e18a5fc801b2d";

        var accountBAddress = AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.AccountComponentHrp);
        var accountBPublicKey = "03c00b2b2cfa2320d267f2cf2b43a8ac26d7e986f83d95038e927f3df383a470df";
        // var createAccountBTransactionIntentHash = "61ece2bbb206421642b1e4a6df6086ebf7a02e5d326a80cb2b886a0f5b0265c3";

        var tokensTransferTransactionIntentHash = "b06099131de839a7b381ef6d9ac3748dd6d7e3536c4a5a5299557585b2ed5f96";

        using var gatewayRunner = new GatewayTestsRunner(MethodBase.GetCurrentMethod()!.Name, _testConsole)
            .MockGenesis()
            .WithAccount(accountAAddress, accountAPublicKey, "XRD")
            .WithAccount(accountBAddress, accountBPublicKey, "XRD")
            .MockTokensTransfer(accountAAddress, accountBAddress, "XRD", 200, tokensTransferTransactionIntentHash);

        var task = gatewayRunner
            .RunAndWaitUntilAllTransactionsIngested<TransactionSubmitResponse>(callback: ValidateResponse);
        task.Wait();

        // Assert (callback method)
        void ValidateResponse(TransactionSubmitResponse payload, string intentHash)
        {
            _testConsole.WriteLine($"Validating {payload.GetType().Name} response");
            payload.Duplicate.Should().Be(false);

            if (intentHash == tokensTransferTransactionIntentHash)
            {
                gatewayRunner.GetAccountBalance(accountAAddress).Should().BeApproximately(795, 5, "paid network fees");

                gatewayRunner.GetAccountBalance(accountBAddress).Should().Be(1200);
            }
        }

        gatewayRunner.SaveStateUpdatesToFile();
    }
}
