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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public class TransactionEndpointTests
{
    private readonly ITestOutputHelper _testConsole;

    public TransactionEndpointTests(ITestOutputHelper testConsole)
    {
        _testConsole = testConsole;
    }

    [Fact]
    public void TestTransactionRecent()
    {
        // Arrange
        using var gatewayRunner = new GatewayTestsRunner(_testConsole);

        var coreApiStub = gatewayRunner
            .WithTestHeader(MethodBase.GetCurrentMethod()!.Name)
            .MockGenesis()
            .ArrangeTransactionRecentTest(nameof(TestTransactionRecent));

        // Act
        var task = gatewayRunner
            .WaitUntilAllTransactionsAreIngested().Result
            .ActAsync<RecentTransactionsResponse>(
            "/transaction/recent",
            JsonContent.Create(new RecentTransactionsRequest()));

        task.Wait();
        var payload = task.Result;

        // Assert
        payload.Transactions.ShouldNotBeNull();
        payload.Transactions.Count.Should().BeGreaterThan(0);

        payload.LedgerState.ShouldNotBeNull();
        payload.LedgerState.Network.Should().Be(coreApiStub.CoreApiStubDefaultConfiguration.NetworkName);
        payload.LedgerState._Version.Should().Be(1);

        gatewayRunner.TearDown();
    }

    [Fact]
    public void TestTransactionPreviewShouldPass()
    {
        // Arrange
        using var gatewayRunner = new GatewayTestsRunner(_testConsole);

        var coreApiStub = gatewayRunner
            .WithTestHeader(MethodBase.GetCurrentMethod()!.Name)
            .MockGenesis()
            .ArrangeTransactionPreviewTest(nameof(TestTransactionPreviewShouldPass));

        var json = coreApiStub.CoreApiStubDefaultConfiguration.TransactionPreviewRequest.ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var task = gatewayRunner
            .WaitUntilAllTransactionsAreIngested().Result
            .ActAsync<TransactionPreviewResponse>("/transaction/preview", content);

        task.Wait();
        var payload = task.Result;

        var coreApiPayload = JsonConvert.DeserializeObject<RadixDlt.CoreApiSdk.Model.TransactionPreviewResponse>(payload.CoreApiResponse.ToString()!);

        // Assert
        coreApiPayload.ShouldNotBeNull();
        coreApiPayload.Receipt.ShouldNotBeNull();
        coreApiPayload.Receipt.Status.Should().Be(CoreApiSdk.Model.TransactionStatus.Succeeded);

        gatewayRunner.TearDown();
    }

    [Fact(Skip = "Disabled until MempoolTrackerWorker is re-enabled")]
    public void MempoolTransactionStatusShouldBeFailed()
    {
        // Arrange
        using var gatewayRunner = new GatewayTestsRunner(_testConsole);
        var coreApiStubs = gatewayRunner
            .WithTestHeader(MethodBase.GetCurrentMethod()!.Name)
            .MockGenesis()
            .ArrangeMempoolTransactionStatusTest(
            nameof(MempoolTransactionStatusShouldBeFailed),
            TransactionStatus.StatusEnum.FAILED);
        var transactionIdentifier =
            new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, coreApiStubs.CoreApiStubDefaultConfiguration.MempoolTransactionHash);

        var json = new TransactionStatusRequest(transactionIdentifier).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var task = gatewayRunner
            .WaitUntilAllTransactionsAreIngested().Result.ActAsync<TransactionStatusResponse>("/transaction/status", content);

        task.Wait();
        var payload = task.Result;

        // Assert
        var status = payload.Transaction.TransactionStatus.Status;
        status.Should().Be(TransactionStatus.StatusEnum.FAILED);
    }

    [Fact(Skip = "Disabled until MempoolTrackerWorker is re-enabled")]
    public void MempoolTransactionStatusShouldBeConfirmed()
    {
        // Arrange
        using var gatewayRunner = new GatewayTestsRunner(_testConsole);

        var coreApiStubs = gatewayRunner
            .WithTestHeader(MethodBase.GetCurrentMethod()!.Name)
            .MockGenesis()
            .ArrangeMempoolTransactionStatusTest(
            nameof(MempoolTransactionStatusShouldBeConfirmed),
            TransactionStatus.StatusEnum.CONFIRMED);
        var transactionIdentifier =
            new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, coreApiStubs.CoreApiStubDefaultConfiguration.MempoolTransactionHash);

        var json = new TransactionStatusRequest(transactionIdentifier).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var task = gatewayRunner
            .WaitUntilAllTransactionsAreIngested().Result.ActAsync<TransactionStatusResponse>("/transaction/status", content);

        task.Wait();
        var payload = task.Result;

        // Assert
        var status = payload.Transaction.TransactionStatus.Status;
        status.Should().Be(TransactionStatus.StatusEnum.CONFIRMED);
    }

    [Fact(Skip = "Disabled until MempoolTrackerWorker is re-enabled")]
    public void MempoolTransactionStatusShouldBePending()
    {
        // Arrange
        var gatewayRunner = new GatewayTestsRunner(_testConsole);
        var coreApiStubs = gatewayRunner
            .WithTestHeader(MethodBase.GetCurrentMethod()!.Name)
            .MockGenesis()
            .ArrangeMempoolTransactionStatusTest(
            nameof(MempoolTransactionStatusShouldBePending),
            TransactionStatus.StatusEnum.PENDING);
        var transactionIdentifier =
            new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, coreApiStubs.CoreApiStubDefaultConfiguration.MempoolTransactionHash);

        var json = new TransactionStatusRequest(transactionIdentifier).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var task = gatewayRunner
            .WaitUntilAllTransactionsAreIngested().Result
            .ActAsync<TransactionStatusResponse>("/transaction/status", content);

        task.Wait();
        var payload = task.Result;

        // Assert
        var status = payload.Transaction.TransactionStatus.Status;
        status.Should().Be(TransactionStatus.StatusEnum.PENDING);
    }

    [Fact]
    public void TestTransactionSubmit()
    {
        // Arrange
        using var gatewayRunner = new GatewayTestsRunner(_testConsole);

        var coreApiStubs = gatewayRunner
            .WithTestHeader(MethodBase.GetCurrentMethod()!.Name)
            .MockGenesis()
            .ArrangeSubmitTransactionTest(
                nameof(TestTransactionSubmit));

        var hexTransaction = Convert
            .ToHexString(Encoding.UTF8.GetBytes(coreApiStubs.CoreApiStubDefaultConfiguration.SubmitTransaction))
            .ToLowerInvariant();

        var json = new TransactionSubmitRequest(hexTransaction).ToJson();

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var task = gatewayRunner
            .WaitUntilAllTransactionsAreIngested().Result
            .ActAsync<TransactionSubmitResponse>("/transaction/submit", content);

        task.Wait();
        var payload = task.Result;

        // Assert
        payload.Duplicate.Should().Be(false);

        // TODO: should also return intent hash
        // payload.IntentHash.ShouldNoBreNull();

        gatewayRunner.TearDown();
    }

    [Fact(Skip ="TransactionSubmitResponse and/or RecentTransactionsResponse should return IntentHash")]
    public void SubmittedTransactionStatusShouldBeConfirmed()
    {
        // Arrange
        using var gatewayRunner = new GatewayTestsRunner(_testConsole);

        var coreApiStubs = gatewayRunner
            .WithTestHeader(MethodBase.GetCurrentMethod()!.Name)
            .MockGenesis()
            .ArrangeSubmittedTransactionStatusTest(
            nameof(SubmittedTransactionStatusShouldBeConfirmed));

        // Act
        var taskRecent = gatewayRunner
            .WaitUntilAllTransactionsAreIngested().Result
            .ActAsync<RecentTransactionsResponse>(
                "/transaction/recent",
                JsonContent.Create(new RecentTransactionsRequest()));

        taskRecent.Wait();
        var recentTransactions = taskRecent.Result;

        var hash = recentTransactions.Transactions[0].TransactionIdentifier.Hash;
        var transactionIdentifier = new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, hash);
        var json = new TransactionStatusRequest(transactionIdentifier).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var taskAct = gatewayRunner.ActAsync<TransactionStatusResponse>("/transaction/status", content);
        taskAct.Wait();
        var payload = taskAct.Result;

        // Assert
        var status = payload.Transaction.TransactionStatus.Status;
        status.Should().Be(TransactionStatus.StatusEnum.CONFIRMED);

        gatewayRunner.TearDown();
    }
}
