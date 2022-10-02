﻿using Newtonsoft.Json.Linq;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class AccountBuilder : BuilderBase<(TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates)>
{
    private readonly CoreApiStubDefaultConfiguration _defaultConfig;
    private readonly TestGlobalEntities _globalEntities;

    private string _accountAddress = string.Empty;

    private string _accountName = string.Empty;
    private string _accountPublicKey = string.Empty;
    private long _tokensBalance;
    private string _tokenName = string.Empty;

    public AccountBuilder(CoreApiStubDefaultConfiguration defaultConfig, TestGlobalEntities globalEntities)
    {
        _defaultConfig = defaultConfig;
        _globalEntities = globalEntities;
        _accountPublicKey = AddressHelper.GenerateRandomPublicKey();
    }

    public override (TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates) Build()
    {
        var keyValueStoreAddressHex = "000000000000000000000000000000000000000000000000000000000000000001000000";

        // TODO: create a new resource if not 'XRD'
        // if XRD: (https://github.com/0xOmarA/PTE-programmatic-interactions/blob/main/src/main.rs)
        // find the entity and get its address
        // Create a new down/up state to free 'tokenBalance' tokens from SystFaucet vault
        // update GlobalEntities.StateUpdates
        var (tokenEntity, tokens) = new FungibleResourceBuilder(_defaultConfig)
            .WithResourceName(_tokenName)
            .Build();

        _globalEntities.Add(tokenEntity);
        _globalEntities.AddStateUpdates(tokens);

        var (vaultEntity, vault) = new VaultBuilder(_defaultConfig)
            .WithVaultName(_accountName)
            .WithFungibleTokens(tokenEntity.EntityAddressHex)
            .Build();

        _globalEntities.Add(vaultEntity);
        _globalEntities.AddStateUpdates(vault);

        var componentStateSubstateData = new ComponentStateSubstate(
            entityType: EntityType.Component,
            substateType: SubstateType.ComponentState,
            dataStruct: new DataStruct(
                structData: new SborData(
                    dataHex: "1002000000b3240000000000000000000000000000000000000000000000000000000000000000000000000000008324000000000000000000000000000000000000000000000000000000000000000000000001000000",
                    dataJson: JObject.Parse($"{{\"fields\": [{{\"bytes\": \"{vaultEntity.EntityAddressHex}\", \"type\": \"Custom\", \"type_id\": 179}}, {{\"bytes\": \"{keyValueStoreAddressHex}\", \"type\": \"Custom\", \"type_id\": 131}}], \"type\": \"Struct\"}}")
                ),
                ownedEntities: new List<EntityId>()
                {
                    new(entityType: EntityType.Vault, entityAddressHex: vaultEntity.EntityAddressHex),
                    new(entityType: EntityType.KeyValueStore, entityAddressHex: keyValueStoreAddressHex),
                },
                referencedEntities: new List<EntityId>()
            )
        );

        return new ComponentBuilder(_defaultConfig, ComponentHrp.AccountComponentHrp)
            .WithComponentName($"_component_{_accountName}")
            .WithComponentStateSubstate(componentStateSubstateData)
            .WithVault(vaultEntity.EntityAddressHex) // TODO: is it used?
            .Build();
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
