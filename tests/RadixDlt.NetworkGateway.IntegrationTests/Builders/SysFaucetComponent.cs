using RadixDlt.CoreApiSdk.Model;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class SysFaucetComponent : StateUpdates
{
    private readonly string _package_address = "package_loc1qyqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqsac8r79";
    private readonly string _entity_address_hex = "040000000000000000000000000000000000000000000000000001";
    private readonly string _global_address_hex = "system_loc1qsqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqstnrp49";

    public SysFaucetComponent()
    {
        DownSubstates = new List<DownSubstate>();

        DownVirtualSubstates = new List<SubstateId>();

        NewGlobalEntities = new List<GlobalEntityId>()
        {
            new GlobalEntityId(
                entityType: EntityType.Package,
                entityAddressHex: _entity_address_hex,
                globalAddressHex: _entity_address_hex,
                globalAddress: _global_address_hex
            ),
        };

        UpSubstates = new List<UpSubstate>()
        {
            new UpSubstate(
                substateId: new SubstateId(
                    entityType: EntityType.Component,
                    entityAddressHex: _entity_address_hex,
                    substateType: SubstateType.ComponentInfo,
                    substateKeyHex: "00"),
                substateData: new Substate(
                    actualInstance: new ComponentInfoSubstate(
                        entityType: EntityType.Component,
                        substateType: SubstateType.ComponentInfo,
                        packageAddress: _package_address,
                        blueprintName: "SysFaucet"
                    )
                ),
                substateHex: "110d000000436f6d706f6e656e74496e666f010000001003000000801b0000000100000000000000000000000000000000000000000000000000010c09000000537973466175636574301000000000",
                substateDataHash: "bdd0d7acd8e2436ba42f830f6e732ad287576abc6770df397cc356f780bfe9f2",
                version: 0L
            ),
        };
    }
}
