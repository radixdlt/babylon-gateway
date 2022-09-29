using Newtonsoft.Json.Linq;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class ComponentBuilder : IBuilder<(TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates)>
{
    private string _componentAddress = string.Empty;

    private string _keyValueStoreAddressHex = "000000000000000000000000000000000000000000000000000000000000000001000000";

    private Substate? _substateData;
    private string _componentName = string.Empty;
    private string _vaultAddressHex = string.Empty;

    public ComponentBuilder(NetworkConfigurationResponse networkConfiguration)
    {
        _componentAddress = AddressHelper.GenerateRandomAddress("component_" + networkConfiguration.NetworkHrpSuffix);
    }

    public (TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates) Build()
    {
        if (_substateData == null)
        {
            throw new NullReferenceException("No subState found.");
        }

        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var globalEntityId = new TestGlobalEntity()
        {
            EntityType = EntityType.Package,
            EntityAddressHex = AddressHelper.AddressToHex(_componentAddress),
            GlobalAddressHex = AddressHelper.AddressToHex(_componentAddress),
            GlobalAddress = _componentAddress,
            Name = _componentName,
        };

        var newGlobalEntities = new List<GlobalEntityId>() { globalEntityId };

        var upSubstates = new List<UpSubstate>()
        {
            new UpSubstate(
                substateId: new SubstateId(
                    entityType: EntityType.Component,
                    entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                    substateType: SubstateType.ComponentInfo,
                    substateKeyHex: "00"),
                substateData: _substateData,
                substateHex: "110d000000436f6d706f6e656e74496e666f010000001003000000801b0000000100000000000000000000000000000000000000000000000000010c09000000537973466175636574301000000000",
                substateDataHash: "bdd0d7acd8e2436ba42f830f6e732ad287576abc6770df397cc356f780bfe9f2",
                version: 0L
            ), // owned entities
            new UpSubstate(
                substateId: new SubstateId(
                    entityType: EntityType.Component,
                    entityAddressHex: AddressHelper.AddressToHex(_componentAddress),
                    substateType: SubstateType.ComponentState,
                    substateKeyHex: "01"
                ),
                version: 0L,
                substateData: new Substate(
                    actualInstance: new ComponentStateSubstate(
                        entityType: EntityType.Component,
                        substateType: SubstateType.ComponentState,
                        dataStruct: new DataStruct(
                            structData: new SborData(
                                dataHex: "1002000000b3240000000000000000000000000000000000000000000000000000000000000000000000000000008324000000000000000000000000000000000000000000000000000000000000000000000001000000",
                                dataJson: JObject.Parse($"{{\"fields\": [{{\"bytes\": \"{_vaultAddressHex}\", \"type\": \"Custom\", \"type_id\": 179}}, {{\"bytes\": \"{_keyValueStoreAddressHex}\", \"type\": \"Custom\", \"type_id\": 131}}], \"type\": \"Struct\"}}")
                            ),
                            ownedEntities: new List<EntityId>()
                            {
                                new(entityType: EntityType.Vault, entityAddressHex: _vaultAddressHex),
                                new(entityType: EntityType.KeyValueStore, entityAddressHex: _keyValueStoreAddressHex),
                            },
                            referencedEntities: new List<EntityId>()
                        )
                    )
                ),
                substateHex: "110e000000436f6d706f6e656e7453746174650100000010010000003007570000001002000000b3240000000000000000000000000000000000000000000000000000000000000000000000000000008324000000000000000000000000000000000000000000000000000000000000000000000001000000",
                substateDataHash: "810ea50c21903f64f85bce050f7feb679e4df85a657fc20c21bb7ce341b6959d"
            ),
        };

        return (globalEntityId, new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities));
    }

    public ComponentBuilder WithFixedAddress(string componentAddress)
    {
        _componentAddress = componentAddress;

        return this;
    }

    public ComponentBuilder WithComponentName(string componentName)
    {
        _componentName = componentName;

        return this;
    }

    public ComponentBuilder WithComponentInfoSubstate(string packageAddress, string blueprintName)
    {
        _substateData = new Substate(
            actualInstance: new ComponentInfoSubstate(
                entityType: EntityType.Component,
                substateType: SubstateType.ComponentInfo,
                packageAddress: packageAddress,
                blueprintName: blueprintName
            )
        );

        return this;
    }

    public ComponentBuilder WithVault(string vaultAddressHex)
    {
        _vaultAddressHex = vaultAddressHex;

        return this;
    }
}
