using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class FungibleResourceBuilder : IBuilder<(TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates)>
{
    private string _resourceAddress = "resource_tdx_21_1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqzqvmpphj";

    private string _resourceName = string.Empty;

    public (TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates) Build()
    {
        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var globalEntityId = new TestGlobalEntity()
        {
            EntityType = EntityType.ResourceManager,
            EntityAddressHex = AddressHelper.AddressToHex(_resourceAddress),
            GlobalAddressHex = AddressHelper.AddressToHex(_resourceAddress),
            GlobalAddress = _resourceAddress,
            Name = _resourceName,
        };

        var newGlobalEntities = new List<GlobalEntityId>() { globalEntityId, };

        var upSubstates = new List<UpSubstate>()
        {
            new(
                substateId: new SubstateId(
                    entityType: EntityType.ResourceManager,
                    entityAddressHex: AddressHelper.AddressToHex(_resourceAddress),
                    substateType: SubstateType.ResourceManager,
                    substateKeyHex: "00"
                ),
                version: 0L,
                substateData: new Substate(
                    actualInstance: new ResourceManagerSubstate(
                        entityType: EntityType.ResourceManager,
                        substateType: SubstateType.ResourceManager,
                        resourceType: ResourceType.Fungible,
                        fungibleDivisibility: 18,
                        metadata: new List<ResourceManagerSubstateAllOfMetadata>()
                        {
                            new ResourceManagerSubstateAllOfMetadata("name", "Radix"),
                            new ResourceManagerSubstateAllOfMetadata("symbol", "XRD"),
                            new ResourceManagerSubstateAllOfMetadata("url", "https://tokens.radixdlt.com"),
                            new ResourceManagerSubstateAllOfMetadata("description", "The Radix Public Network's native token, used to pay the network's required transaction fees and to secure the network through staking to its validator nodes."),
                        },
                        totalSupplyAttos: "1000000000000000000000000000000")
                ),
                substateHex: GenesisBinaryData.FungibleResourceCodeHex,
                substateDataHash: "3dc43a58c5cc27bba7d9a96966c8d66a230c781ec04f936bf10130688ed887cf"
            ),
        };

        return (globalEntityId, new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities));
    }

    public FungibleResourceBuilder WithFixedAddress(string resourceAddress)
    {
        _resourceAddress = resourceAddress;

        return this;
    }

    public FungibleResourceBuilder WithResourceName(string resourceName)
    {
        _resourceName = resourceName;

        return this;
    }
}
