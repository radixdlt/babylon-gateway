using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
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

    public async Task<T> RunAndWaitUntilAllTransactionsAreIngested<T>(TimeSpan? timeout = null)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.NameFromAsync());

        Initialize(_databaseName);

        // make the api call
        _testConsole.WriteLine($"Sending a POST request to '{_request.RequestUri}'");
        var payload = await ActAsync<T>(_request.RequestUri, _request.Content);

        return payload;
    }

    public GatewayTestsRunner MockGenesis()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine("XRD resource");
        var tokens = new FungibleResourceBuilder(CoreApiStub.CoreApiStubDefaultConfiguration)
            .WithResourceName("XRD")
            .WithTotalSupply(10000000000)
            .Build();

        StateUpdatesStore.AddStateUpdates(tokens);

        _testConsole.WriteLine("SysFaucet vault");
        var vault = new VaultBuilder(CoreApiStub.CoreApiStubDefaultConfiguration)
            .WithVaultName("SysFaucet vault")
            .WithFungibleTokens(tokens.NewGlobalEntities[0].GlobalAddress)
            .WithFungibleTokensTotalSupply(10000000000)
            .WithFungibleTokensDivisibility(18)
            .Build();

        StateUpdatesStore.AddStateUpdates(vault);

        _testConsole.WriteLine("SysFaucet package");
        var package = new PackageBuilder(CoreApiStub.CoreApiStubDefaultConfiguration)
            .WithBlueprints(new List<IBlueprint> { new SysFaucetBlueprint() })
            .WithFixedAddress(GenesisData.SysFaucetPackageAddress)
            .Build();

        StateUpdatesStore.AddStateUpdates(package);

        // TODO: KeyValueStore builder !!!
        _testConsole.WriteLine("SysFaucet component");
        var component = new ComponentBuilder(CoreApiStub.CoreApiStubDefaultConfiguration, ComponentHrp.SystemComponentHrp)
            .WithComponentName(GenesisData.SysFaucetBlueprintName)
            .WithComponentInfoSubstate(GenesisData.SysFaucetInfoSubstate)
            .WithComponentStateSubstate(
                GenesisData.SysFaucetStateSubstate(vault.NewGlobalEntities[0].EntityAddressHex, "000000000000000000000000000000000000000000000000000000000000000001000000"))
            .Build();

        StateUpdatesStore.AddStateUpdates(component);

        _testConsole.WriteLine("Transaction receipt");
        var transactionReceipt = new TransactionReceiptBuilder().WithStateUpdates(StateUpdatesStore.StateUpdates).Build();

        CoreApiStub.CoreApiStubDefaultConfiguration.CommittedGenesisTransactionsResponse = new CommittedTransactionsResponse(
            1L,
            1L,
            1L,
            new List<CommittedTransaction>
            {
                new(1L, null, transactionReceipt),
            }
        );

        return this;
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
    }

    private async Task WaitAsync(TimeSpan timeout)
    {
        await Task.Delay(timeout);
    }

    private void Initialize(string databaseName)
    {
        _testConsole.WriteLine("Setting up SUT");

        if (CoreApiStub.CoreApiStubDefaultConfiguration.CommittedGenesisTransactionsResponse == null)
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
