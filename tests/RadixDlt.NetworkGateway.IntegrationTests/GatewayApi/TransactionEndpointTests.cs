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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi;

public class TransactionEndpointTests
{
    [Fact]
    public async Task TestTransactionRecent()
    {
        var client = TestInitializationFactory.CreateClient(nameof(TestTransactionRecent));

        var payload = await GetRecentTransactions(client);

        payload.LedgerState.ShouldNotBeNull();
        payload.LedgerState.Network.Should().Be(DbSeedHelper.NetworkName);
        payload.LedgerState._Version.Should().Be(1);
        payload.Transactions.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TestTransactionStatus()
    {
        var client = TestInitializationFactory.CreateClient(nameof(TestTransactionStatus));

        // Arrange
        var recentTransactions = await GetRecentTransactions(client);
        var transactionIdentifier = new TransactionLookupIdentifier(TransactionLookupOrigin.Intent, recentTransactions.Transactions[0].IntentHashHex); // TODO not sure if valid origin

        // Act
        string json = new TransactionStatusRequest(transactionIdentifier).ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("/transaction/status", content);

        // Assert
        var payload = await response.ParseToObjectAndAssert<TransactionStatusResponse>();

        payload.Transaction.TransactionStatus.StateVersion.Should().Be(1);
        payload.Transaction.TransactionStatus.Status.Should().Be(TransactionStatus.StatusEnum.Succeeded);
        payload.Transaction.PayloadHashHex.Length.Should().Be(NetworkGatewayConstants.Transaction.IdentifierHashLength);
        payload.Transaction.IntentHashHex.Length.Should().Be(NetworkGatewayConstants.Transaction.IdentifierHashLength);
        payload.Transaction.TransactionAccumulatorHex.Length.Should().Be(NetworkGatewayConstants.Transaction.IdentifierHashLength);
    }

    [Fact]
    public async Task TestTransactionSubmit()
    {
        var client = TestInitializationFactory.CreateClient(nameof(TestTransactionSubmit));

        // Arrange
        string json = new TransactionSubmitRequest(GenerateSampleNotarizedTransaction()).ToJson();

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await client.PostAsync("/transaction/submit", content);

        // Assert
        var payload = await response.ParseToObjectAndAssert<TransactionSubmitResponse>();

        payload.Duplicate.Should().BeFalse();
    }

    private async Task<RecentTransactionsResponse> GetRecentTransactions(HttpClient client)
    {
        using HttpResponseMessage response = await client.PostAsync(
            "/transaction/recent",
            JsonContent.Create(new RecentTransactionsRequest()));

        var payload = await response.ParseToObjectAndAssert<RecentTransactionsResponse>();

        payload.Transactions.ShouldNotBeNull();

        return payload;
    }

    // TODO this shouldn't use hardcoded value
    private string GenerateSampleNotarizedTransaction()
    {
        return "1002000000100200000010020000001009000000070107f00a00000000000000000a64000000000000000a0500000000000000" +
               "9121000000038258493e79d7cb71a655dc71ae429d010891590a2d33c63c60cf54b162cba21c01000980969800090500000010" +
               "010000003011040000000a00000043616c6c4d6574686f6403000000811b000000040000000000000000000000000000000000" +
               "0000000000000000010c080000006c6f636b5f66656530072a0000001001000000a1200000000000a0dec5adc9353600000000" +
               "000000000000000000000000000000000000000a00000043616c6c4d6574686f6403000000811b000000040000000000000000" +
               "0000000000000000000000000000000000010c08000000667265655f78726430070500000010000000000f00000054616b6546" +
               "726f6d576f726b746f7001000000b61b0000000000000000000000000000000000000000000000000000000000040c00000043" +
               "616c6c46756e6374696f6e04000000801b0000000100000000000000000000000000000000000000000000000000030c070000" +
               "004163636f756e740c110000006e65775f776974685f7265736f7572636530071f00000010020000001108000000416c6c6f77" +
               "416c6c00000000b10400000000020000302101000000020000009121000000038258493e79d7cb71a655dc71ae429d01089159" +
               "0a2d33c63c60cf54b162cba21c924000000006e55ec51b6a10059b0aee80a07e44d2874104a6e0a6db3191c851d543a69cdc3f" +
               "19d4e98853397f5cdca462be2e258e3ccdda0e8804be2b5a5715aaab75d97892400000006facf3ad44960827bec2bf13cbb8e0" +
               "f8fbeab50113aa7b13a03f02072d66a70f65bd6b1723fc3509568fc5895d482cb20817e32aa503d76f84060071289048c5";
    }
}
