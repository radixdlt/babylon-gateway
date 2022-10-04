using Newtonsoft.Json.Linq;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class AccountBuilder : BuilderBase<StateUpdates>
{
    private readonly CoreApiStubDefaultConfiguration _defaultConfig;
    private readonly StateUpdatesStore _stateUpdatesStore;

    private string _accountAddress = string.Empty;
    private string _accountName = string.Empty;
    private long _tokensBalance;
    private string _tokenName = string.Empty;

    public AccountBuilder(CoreApiStubDefaultConfiguration defaultConfig, StateUpdatesStore stateUpdatesStore)
    {
        _defaultConfig = defaultConfig;
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
        var tokens = new FungibleResourceBuilder(_defaultConfig)
            .WithResourceName(_tokenName)
            .WithTotalSupply(_tokensBalance)
            .Build();

        _stateUpdatesStore.AddStateUpdates(tokens);

        var vault = new VaultBuilder(_defaultConfig)
            .WithVaultName(_accountName)
            .WithFungibleTokens(tokens.NewGlobalEntities[0].EntityAddressHex)
            .WithFungibleTokensTotalSupply(_tokensBalance)
            .Build();

        _stateUpdatesStore.AddStateUpdates(vault);

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
            .WithComponentStateSubstate(componentStateSubstateData)
            .WithFixedAddress(_accountAddress)
            .WithVault(vault.NewGlobalEntities[0].EntityAddressHex) // TODO: is it used?
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

    public AccountBuilder WithAccountName(string accountName)
    {
        _accountName = accountName;

        return this;
    }

    public AccountBuilder WithTokenName(string tokenName)
    {
        _tokenName = tokenName;

        return this;
    }

    public AccountBuilder WithBalance(long tokensBalance)
    {
     _tokensBalance = tokensBalance;

     return this;
    }
}
