using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RadixDlt.NetworkGateway.IntegrationTests;

public partial class GatewayTestsRunner : IDisposable
{
    public void Dispose()
    {
        TearDown();
    }

    private GatewayTestsRunner PrepareEnvironment()
    {
        _testConsole.WriteLine("Preparing SUT");

        MockGenesis();

        var databaseName = _testName;
        Initialize(databaseName);

        return this;
    }

    private void Initialize(string databaseName)
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        _testConsole.WriteLine("Initializing TestGatewayApiFactory");
        _gatewayApiFactory = TestGatewayApiFactory.Create(CoreApiStub, databaseName, _testConsole);

        // allow db creation
        var t = WaitAsync(TimeSpan.FromSeconds(10));
        t.Wait();

        _testConsole.WriteLine("Initializing TestDataAggregatorFactory");
        _dataAggregatorFactory = TestDataAggregatorFactory.Create(CoreApiStub, databaseName, _testConsole);
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

    private GatewayTestsRunner MockGenesis()
    {
        _testConsole.WriteLine(MethodBase.GetCurrentMethod()!.Name);

        var networkConfiguration = CoreApiStub.CoreApiStubDefaultConfiguration.NetworkConfigurationResponse;

        _testConsole.WriteLine("XRD resource");
        var (tokenEntity, tokens) = new FungibleResourceBuilder(networkConfiguration)
            .WithResourceName("XRD")
            .Build();

        CoreApiStub.GlobalEntities.Add(tokenEntity);
        CoreApiStub.GlobalEntities.AddStateUpdates(tokens);

        _testConsole.WriteLine("SysFaucet vault");
        var (vaultEntity, vault) = new VaultBuilder(CoreApiStub.CoreApiStubDefaultConfiguration.NetworkConfigurationResponse)
            .WithVaultName("SysFaucet vault")
            .WithFungibleTokens(tokenEntity.GlobalAddress)
            .Build();

        CoreApiStub.GlobalEntities.Add(vaultEntity); // only for testing purposes, vault is not a global entity!

        CoreApiStub.GlobalEntities.AddStateUpdates(vault);

        _testConsole.WriteLine("SysFaucet package");
        var (packageEntity, package) = new PackageBuilder()
            .WithBlueprints(new List<IBlueprint> { new SysFaucetBlueprint() })
            .WithFixedAddress(GenesisData.SysFaucetPackageAddress)
            .Build();

        CoreApiStub.GlobalEntities.Add(packageEntity);
        CoreApiStub.GlobalEntities.AddStateUpdates(package);

        // TODO: KeyValueStore builder !!!
        _testConsole.WriteLine("SysFaucet component");
        var (componentEntity, component) = new ComponentBuilder(CoreApiStub.CoreApiStubDefaultConfiguration.NetworkConfigurationResponse)
            .WithComponentName(GenesisData.SysFaucetBlueprintName)
            .WithComponentInfoSubstate(GenesisData.SysFaucetInfoSubstate)
            .WithComponentStateSubstate(
                GenesisData.SysFaucetStateSubstate(vaultEntity.EntityAddressHex, "000000000000000000000000000000000000000000000000000000000000000001000000"))
            .Build();

        CoreApiStub.GlobalEntities.Add(componentEntity);
        CoreApiStub.GlobalEntities.AddStateUpdates(component);

        _testConsole.WriteLine("Transaction receipt");
        var transactionReceipt = new TransactionReceiptBuilder().WithStateUpdates(CoreApiStub.GlobalEntities.StateUpdates).Build();

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
}
