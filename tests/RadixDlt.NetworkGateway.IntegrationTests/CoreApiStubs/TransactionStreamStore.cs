using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit.Abstractions;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

[DataContract]
public class Transaction
{
    [DataMember(Name = "state_version")]
    public long StateVersion { get; set; }

    [DataMember(Name = "notarized_transaction")]
    public NotarizedTransaction? NotarizedTransaction { get; set; }

    [DataMember(Name = "receipt")]
    public TransactionReceipt? Receipt { get; set; }
}

[DataContract]
public class TransactionStreamStore
{
    private readonly CoreApiStub _coreApiStub;
    private readonly StateUpdatesStore _stateUpdatesStore;
    private readonly ITestOutputHelper _testConsole;

    [DataMember(Name = "from_state_version")]
    public long FromStateVersion { get; set; } = 1L;

    [DataMember(Name = "to_state_version")]
    public long ToStateVersion { get; set; } = 1L;

    [DataMember(Name = "max_state_version")]
    public long MaxStateVersion { get; set; } = 1L;

    [DataMember(Name = "transactions")]
    private List<Transaction> Transactions { get; set; } = new();

    public TransactionStreamStore(CoreApiStub coreApiStub, StateUpdatesStore stateUpdatesStore, ITestOutputHelper testConsole)
    {
        _coreApiStub = coreApiStub;
        _stateUpdatesStore = stateUpdatesStore;
        _testConsole = testConsole;
    }

    public void GenerateGenesisTransaction()
    {
        _testConsole.WriteLine("XRD resource");
        var tokens = new FungibleResourceBuilder(_coreApiStub.CoreApiStubDefaultConfiguration)
            .WithResourceName("XRD")
            .WithTotalSupply(10000000000)
            .Build();

        _stateUpdatesStore.AddStateUpdates(tokens);

        _testConsole.WriteLine("SysFaucet vault");
        var vault = new VaultBuilder(_coreApiStub.CoreApiStubDefaultConfiguration)
            .WithVaultName("SysFaucet vault")
            .WithFungibleTokens(tokens.NewGlobalEntities[0].GlobalAddress)
            .WithFungibleTokensTotalSupply(10000000000)
            .WithFungibleTokensDivisibility(18)
            .Build();

        _stateUpdatesStore.AddStateUpdates(vault);

        _testConsole.WriteLine("SysFaucet package");
        var package = new PackageBuilder(_coreApiStub.CoreApiStubDefaultConfiguration)
            .WithBlueprints(new List<IBlueprint> { new SysFaucetBlueprint() })
            .WithFixedAddress(GenesisData.SysFaucetPackageAddress)
            .Build();

        _stateUpdatesStore.AddStateUpdates(package);

        _testConsole.WriteLine("SysFaucet component");
        var componentInfo = new ComponentBuilder(_coreApiStub.CoreApiStubDefaultConfiguration, ComponentHrp.SystemComponentHrp)
            .WithComponentInfoSubstate(GenesisData.SysFaucetInfoSubstate)
            .Build();

        _stateUpdatesStore.AddStateUpdates(componentInfo);

        _testConsole.WriteLine("SysFaucet component state");
        var componentState = new ComponentBuilder(_coreApiStub.CoreApiStubDefaultConfiguration, ComponentHrp.SystemComponentHrp)
            .WithComponentStateSubstate(
                GenesisData.SysFaucetStateSubstate(vault.NewGlobalEntities[0].EntityAddressHex, "000000000000000000000000000000000000000000000000000000000000000001000000"))
            .Build();

        _stateUpdatesStore.AddStateUpdates(componentState);

        _testConsole.WriteLine("System component info");
        var systemComponentInfo = new ComponentBuilder(_coreApiStub.CoreApiStubDefaultConfiguration, ComponentHrp.SystemComponentHrp)
            .WithSystemStateSubstate(epoch: 0L)
            .Build();

        _stateUpdatesStore.AddStateUpdates(systemComponentInfo);

        _testConsole.WriteLine("Transaction receipt");
        var transactionReceipt = new TransactionReceiptBuilder()
            .WithStateUpdates(_stateUpdatesStore.StateUpdates)
            .WithFeeSummary(new FeeSummary(
                loanFullyRepaid: true,
                costUnitLimit: 10000000,
                costUnitConsumed: 0,
                costUnitPriceAttos: "1000000000000",
                tipPercentage: 0,
                xrdBurnedAttos: "0",
                xrdTippedAttos: "0"
            ))
            .Build();

        _coreApiStub.CoreApiStubDefaultConfiguration.CommittedGenesisTransactionsResponse = new CommittedTransactionsResponse(
            1L,
            1L,
            1L,
            new List<CommittedTransaction>
            {
                new(1L, null, transactionReceipt),
            }
        );
    }
}
