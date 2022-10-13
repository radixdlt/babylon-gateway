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

using Newtonsoft.Json;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public partial class GatewayTestsRunner
{
    public void Dispose()
    {
        TearDown();
    }

    public GatewayTestsRunner MockGenesis()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _transactionStreamStore.QueueGenesisTransaction();

        return this;
    }

    public async Task RunAndWaitUntilAllTransactionsIngested<T>(Action<T?, string?, Exception?> callback)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.NameFromAsync());

        Initialize(_databaseName);

        // run all pending transactions in the queue
        while (true)
        {
            var pendingTransaction = _transactionStreamStore.GetPendingTransaction();

            if (pendingTransaction == null)
            {
                _testConsole.WriteLine("No pending transactions found. Exiting...");
                return;
            }

            _testConsole.WriteLine($"Sending '{pendingTransaction!.Request.RequestUri}' POST request");

            var response = await ActAsync(pendingTransaction.Request.RequestUri, pendingTransaction.Request.Content);

            if (pendingTransaction.Request.MarkAsCommitted)
            {
                try
                {
                    _transactionStreamStore.MarkPendingTransactionAsCommitted(pendingTransaction);
                }
                catch (Exception ex)
                {
                    callback?.Invoke(default(T), pendingTransaction?.IntentHash, ex);
                    _transactionStreamStore.MarkPendingTransactionAsFailed(pendingTransaction);

                    return;
                }
                finally
                {
                    var t = WaitAsync(TimeSpan.FromSeconds(5));
                    t.Wait();
                }
            }

            _transactionStreamStore.MarkPendingTransactionAsCompleted(pendingTransaction);

            var canParse = await response.TryParse<T>();

            if (canParse)
            {
                if (pendingTransaction?.IntentHash != null)
                {
                    callback?.Invoke(await response.ParseToObjectAndAssert<T>(), pendingTransaction.IntentHash, null);
                }
            }
        }
    }

    public void SaveStateUpdatesToFile()
    {
        var statesDump = JsonConvert.SerializeObject(
            _transactionStreamStore,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        File.WriteAllText(_databaseName + ".json", statesDump);
    }

    // Tear down
    public void TearDown()
    {
        if (_dataAggregatorFactory != null)
        {
            _testConsole.WriteLine("Tearing down TestDataAggregatorFactory");
            _dataAggregatorFactory.Server.Dispose();
            _dataAggregatorFactory.Dispose();
            _dataAggregatorFactory = null;
        }

        if (_gatewayApiFactory != null)
        {
            _testConsole.WriteLine("Tearing down TestGatewayApiFactory");
            _gatewayApiFactory.Server.Dispose();
            _gatewayApiFactory.Dispose();
            _gatewayApiFactory = null;
        }
    }

    private async Task<HttpResponseMessage> ActAsync(string? requestUri, HttpContent? content)
    {
        if (requestUri == null)
        {
            throw new Exception("Gateway api uri is missing.");
        }

        if (_gatewayApiFactory == null)
        {
            throw new Exception("Gateway http client is not initialized.");
        }

        return await _gatewayApiFactory.Client.PostAsync(requestUri, content);
    }

    private async Task<T> ActAsync<T>(string? requestUri, HttpContent? content)
    {
        if (requestUri == null)
        {
            throw new Exception("Gateway api uri is missing.");
        }

        if (_gatewayApiFactory == null)
        {
            throw new Exception("Gateway http client is not initialized.");
        }

        using var response = await _gatewayApiFactory.Client.PostAsync(requestUri, content);

        var payload = await response.ParseToObjectAndAssert<T>();

        return payload;
    }

    private void WriteTestHeader(string testName)
    {
        _testConsole.WriteLine($"\n{new string('-', 50)}");
        _testConsole.WriteLine($"{testName} test");
        _testConsole.WriteLine($"{new string('-', 50)}");

        _testConsole.WriteLine(GenesisData.NetworkDefinition.ToString());
    }

    private async Task WaitAsync(TimeSpan? timeout)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        await Task.Delay((TimeSpan)timeout);
    }

    private void Initialize(string databaseName)
    {
        _testConsole.WriteLine("Setting up SUT");

        if (_transactionStreamStore.CommittedTransactions.Count == 0)
        {
            throw new Exception("Call MockGenesis() to initialize the SUT");
        }

        _gatewayApiFactory = TestGatewayApiFactory.Create(CoreApiStub, databaseName, _testConsole);

        _dataAggregatorFactory = TestDataAggregatorFactory.Create(CoreApiStub, databaseName, _testConsole);

        var t = WaitAsync(TimeSpan.FromSeconds(10));
        t.Wait();
    }
}
