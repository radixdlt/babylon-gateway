using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class FungibleResourceBuilder : BuilderBase<(TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates)>
{
    private string _resourceAddress;

    private string _resourceName = string.Empty;
    private long _totalTokensSupply = 1000000;

    public FungibleResourceBuilder(CoreApiStubDefaultConfiguration defaultConfig)
    {
        // generate something like: resource_loc_1qqwknku2
        _resourceAddress = AddressHelper.GenerateRandomAddress(defaultConfig.NetworkDefinition.ResourceHrp);
    }

    public override (TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates) Build()
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
                            new("name", "Radix"),
                            new("symbol", "XRD"),
                            new("url", "https://tokens.radixdlt.com"),
                            new("description", "The Radix Public Network's native token, used to pay the network's required transaction fees and to secure the network through staking to its validator nodes."),
                        },
                        totalSupplyAttos: Convert.ToString(Convert.ToDecimal(_totalTokensSupply * Math.Pow(10, 18)), CultureInfo.InvariantCulture))
                ),
                substateHex: GenesisData.FungibleResourceCodeHex,
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

    public FungibleResourceBuilder WithTotalSupply(long totalTokensSupply)
    {
        _totalTokensSupply = totalTokensSupply;

        return this;
    }
}
