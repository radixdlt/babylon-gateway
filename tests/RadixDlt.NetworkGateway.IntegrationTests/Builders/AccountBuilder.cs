using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class AccountBuilder : BuilderBase<StateUpdates>
{
    private readonly CoreApiStubDefaultConfiguration _defaultConfig;
    private readonly List<StateUpdates> _stateUpdatesList;
    private readonly StateUpdatesStore _stateUpdatesStore;

    private string _accountAddress = string.Empty;
    private string _tokenName = string.Empty;
    private string _totalAmountAttos = string.Empty;
    private ComponentInfoSubstate? _componentInfoSubstate;

    public AccountBuilder(CoreApiStubDefaultConfiguration defaultConfig, List<StateUpdates> stateUpdatesList, StateUpdatesStore stateUpdatesStore)
    {
        _defaultConfig = defaultConfig;
        _stateUpdatesList = stateUpdatesList;
        _stateUpdatesStore = stateUpdatesStore;
        AddressHelper.GenerateRandomPublicKey();
    }

    public override StateUpdates Build()
    {
        var keyValueStoreAddressHex = "000000000000000000000000000000000000000000000000000000000000000001000000";

        // TODO: create a new resource if not 'XRD'
        // if XRD: (https://github.com/0xOmarA/PTE-programmatic-interactions/blob/main/src/main.rs)
        // find the entity and get its address
        // Create a new down/up state to free 'tokenBalance' tokens from SystFaucet vault
        // update GlobalEntities.StateUpdates

        // var tokens = new FungibleResourceBuilder(_defaultConfig)
        //     .WithResourceName(_tokenName)
        //     .WithTotalSupplyAttos(_totalAmountAttos)
        //     .Build();

        var tokens = _stateUpdatesStore.GetGlobalEntity(GenesisData.GenesisResourceManagerAddress);

        var vault = new VaultBuilder(_defaultConfig)
            .WithFungibleTokensResourceAddress(tokens.EntityAddressHex)
            .WithFungibleResourceAmountAttos(_totalAmountAttos)
            .Build();

        _stateUpdatesList.Add(vault);

        var dataStruct = new KeyValueStoreBuilder()
            .WithDataStructField(vault.NewGlobalEntities[0].EntityAddressHex, "Custom", ScryptoType.Vault)
            .WithDataStructField(keyValueStoreAddressHex, "Custom", ScryptoType.KeyValueStore)
            .WithOwnedEntity(EntityType.Vault, vault.NewGlobalEntities[0].EntityAddressHex)
            .WithOwnedEntity(EntityType.KeyValueStore, keyValueStoreAddressHex)
            .Build();

        var componentStateSubstateData = new ComponentStateSubstate(
            entityType: EntityType.Component,
            substateType: SubstateType.ComponentState,
            dataStruct: dataStruct
        );

        var account = new ComponentBuilder(_defaultConfig, ComponentHrp.AccountComponentHrp)
            .WithComponentInfoSubstate(_componentInfoSubstate!)
            .WithComponentStateSubstate(componentStateSubstateData)
            .WithFixedAddress(_accountAddress)
            .Build();

        return account;
    }

    public AccountBuilder WithFixedAddress(string accountAddress)
    {
        _accountAddress = accountAddress;

        return this;
    }

    public AccountBuilder WithPublicKey(string publicKey)
    {
        return this;
    }

    public AccountBuilder WithTokenName(string tokenName)
    {
        _tokenName = tokenName;

        return this;
    }

    public AccountBuilder WithTotalAmountAttos(string totalAmountAttos)
    {
        _totalAmountAttos = totalAmountAttos;

        return this;
    }

    public AccountBuilder WithComponentInfoSubstate(ComponentInfoSubstate componentInfoSubstate)
    {
        _componentInfoSubstate = componentInfoSubstate;

        return this;
    }
}
