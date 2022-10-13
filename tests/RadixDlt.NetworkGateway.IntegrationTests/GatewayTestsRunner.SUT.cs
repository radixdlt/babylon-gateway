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

    public async Task RunAndWaitUntilAllTransactionsIngested<T>(Action<T, string>? callback = null)
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
                _transactionStreamStore.MarkPendingTransactionAsCommitted(pendingTransaction);

                var t = WaitAsync(TimeSpan.FromSeconds(5));
                t.Wait();
            }

            _transactionStreamStore.MarkPendingTransactionAsCompleted(pendingTransaction);

            var canParse = await response.TryParse<T>();

            if (canParse)
            {
                if (pendingTransaction.IntentHash != null)
                {
                    callback?.Invoke(await response.ParseToObjectAndAssert<T>(), pendingTransaction.IntentHash);
                }
            }
        }
    }

    public void SaveStateUpdatesToFile()
    {
        var statesDump = JsonConvert.SerializeObject(
            _transactionStreamStore,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        File.WriteAllText(_databaseName + ".log", statesDump);
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

    // Tear down
    private void TearDown()
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
}
