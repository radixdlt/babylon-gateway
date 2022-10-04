using Newtonsoft.Json.Linq;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class AccountBuilder : BuilderBase<StateUpdates>
{
    private readonly CoreApiStubDefaultConfiguration _defaultConfig;
    private readonly StateUpdatesStore _stateUpdatesStore;

    private string _accountAddress = string.Empty;
    private string _accountName = string.Empty;
    private string _accountPublicKey;
    private long _tokensBalance;
    private string _tokenName = string.Empty;

    public AccountBuilder(CoreApiStubDefaultConfiguration defaultConfig, StateUpdatesStore stateUpdatesStore)
    {
        _defaultConfig = defaultConfig;
        _stateUpdatesStore = stateUpdatesStore;
        _accountPublicKey = AddressHelper.GenerateRandomPublicKey();
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

        var componentStateSubstateData = new ComponentStateSubstate(
            entityType: EntityType.Component,
            substateType: SubstateType.ComponentState,
            dataStruct: new DataStruct(
                structData: new SborData(
                    dataHex: "1002000000b3240000000000000000000000000000000000000000000000000000000000000000000000000000008324000000000000000000000000000000000000000000000000000000000000000000000001000000",
                    dataJson: JObject.Parse($"{{\"fields\": [{{\"bytes\": \"{vault.NewGlobalEntities[0].EntityAddressHex}\", \"type\": \"Custom\", \"type_id\": 179}}, {{\"bytes\": \"{keyValueStoreAddressHex}\", \"type\": \"Custom\", \"type_id\": 131}}], \"type\": \"Struct\"}}")
                ),
                ownedEntities: new List<EntityId>()
                {
                    new(entityType: EntityType.Vault, entityAddressHex: vault.NewGlobalEntities[0].EntityAddressHex),
                    new(entityType: EntityType.KeyValueStore, entityAddressHex: keyValueStoreAddressHex),
                },
                referencedEntities: new List<EntityId>()
            )
        );

        var account = new ComponentBuilder(_defaultConfig, ComponentHrp.AccountComponentHrp)
            .WithComponentName($"_component_{_accountName}")
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
        _accountPublicKey = publicKey;

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
