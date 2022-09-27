using Newtonsoft.Json.Linq;
using RadixDlt.CoreApiSdk.Model;
using System;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class VaultBuilder : IBuilder<StateUpdates>
{
    private readonly string _entity_address_hex = "000000000000000000000000000000000000000000000000000000000000000000000000";
    private readonly string _resource_address_hex = "000000000000000000000000000000000000000000000000000004";
    private readonly string _resource_address = "resource_loc1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqjw23a7";

    public StateUpdates Build()
    {
        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var newGlobalEntities = new List<GlobalEntityId>()
        {
            new GlobalEntityId(
                entityType: EntityType.ResourceManager,
                entityAddressHex: _resource_address_hex,
                globalAddressHex: _resource_address_hex,
                globalAddress: _resource_address),
        };

        var upSubstates = new List<UpSubstate>()
        {
            new UpSubstate(
                substateId: new SubstateId(
                    entityType: EntityType.Vault,
                    entityAddressHex: _entity_address_hex,
                    substateType: SubstateType.Vault,
                    substateKeyHex: "00"
                ),
                version: 0L,
                substateData: new Substate(
                    actualInstance: new VaultSubstate(
                        entityType: EntityType.Vault,
                        substateType: SubstateType.Vault,
                        resourceAmount: new ResourceAmount(
                            new FungibleResourceAmount(
                                resourceType: ResourceType.Fungible,
                                resourceAddress: _resource_address,
                                amountAttos: "1000000000000000000000000000000")))
                ),
                substateHex: "11050000005661756c74010000001001000000110800000046756e6769626c6504000000b61b000000000000000000000000000000000000000000000000000000000004071232a10a00000000a12000000000000040eaed7446d09c2c9f0c00000000000000000000000000000000000000",
                substateDataHash: "16727d810c5684cdfe732101b8075b69964fafc8b0632a5d6d1a7c193214e991"
            ),
            new UpSubstate(
                substateId: new SubstateId(
                    entityType: EntityType.Component,
                    entityAddressHex: "040000000000000000000000000000000000000000000000000001",
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
                                dataJson: JObject.Parse("{\"fields\": [{\"bytes\": \"000000000000000000000000000000000000000000000000000000000000000000000000\", \"type\": \"Custom\", \"type_id\": 179}, {\"bytes\": \"000000000000000000000000000000000000000000000000000000000000000001000000\", \"type\": \"Custom\", \"type_id\": 131}], \"type\": \"Struct\"}")
                            ),
                            ownedEntities: new List<EntityId>()
                            {
                                new EntityId(entityType: EntityType.Vault, entityAddressHex: "000000000000000000000000000000000000000000000000000000000000000000000000"),
                                new EntityId(entityType: EntityType.KeyValueStore, entityAddressHex: "000000000000000000000000000000000000000000000000000000000000000001000000"),
                            },
                            referencedEntities: new List<EntityId>()
                        )
                    )
                ),
                substateHex: "110e000000436f6d706f6e656e7453746174650100000010010000003007570000001002000000b3240000000000000000000000000000000000000000000000000000000000000000000000000000008324000000000000000000000000000000000000000000000000000000000000000000000001000000",
                substateDataHash: "810ea50c21903f64f85bce050f7feb679e4df85a657fc20c21bb7ce341b6959d"
            ),
        };

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities);
    }
}
