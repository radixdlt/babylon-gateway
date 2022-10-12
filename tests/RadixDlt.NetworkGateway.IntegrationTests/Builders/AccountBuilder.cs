using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class AccountBuilder : BuilderBase<StateUpdates>
{
    private readonly StateUpdates _stateUpdates;

    private string _accountAddress = string.Empty;
    private string _totalAmountAttos = string.Empty;
    private ComponentInfoSubstate? _componentInfoSubstate;
    private string _tokenName = string.Empty;

    public AccountBuilder(StateUpdates stateUpdates)
    {
        _stateUpdates = stateUpdates;
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

        // var tokens = new FungibleResourceBuilder()
        //     .WithResourceName(_tokenName)
        //     .WithTotalSupplyAttos(_totalAmountAttos)
        //     .Build();

        var tokens = _stateUpdates.GetGlobalEntity(GenesisData.GenesisResourceManagerAddress);

        var vaultAddressHex = AddressHelper.AddressToHex(AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.ResourceHrp));

        var vault = new VaultBuilder()
            .WithFixedAddressHex(vaultAddressHex)
            .WithFungibleTokensResourceAddress(tokens.GlobalAddress)
            .WithFungibleResourceAmountAttos(_totalAmountAttos)
            .Build();

        var dataStruct = new KeyValueStoreBuilder()
            .WithDataStructField(vaultAddressHex, "Custom", ScryptoType.Vault)
            .WithDataStructField(keyValueStoreAddressHex, "Custom", ScryptoType.KeyValueStore)
            .WithOwnedEntity(EntityType.Vault, vaultAddressHex)
            .WithOwnedEntity(EntityType.KeyValueStore, keyValueStoreAddressHex)
            .Build();

        var componentStateSubstateData = new ComponentStateSubstate(
            entityType: EntityType.Component,
            substateType: SubstateType.ComponentState,
            dataStruct: dataStruct
        );

        var account = new ComponentBuilder(ComponentHrp.AccountComponentHrp)
            .WithComponentInfoSubstate(_componentInfoSubstate!)
            .WithComponentStateSubstate(componentStateSubstateData)
            .WithFixedAddress(_accountAddress)
            .Build();

        return account.Add(vault);
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
