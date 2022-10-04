using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class FungibleResourceBuilder : BuilderBase<StateUpdates>
{
    private string _resourceAddress;

    private string _resourceName = string.Empty;
    private long _totalTokensSupply = 1000000;
    private int _fungibleDivisibility = 18;

    public FungibleResourceBuilder(CoreApiStubDefaultConfiguration defaultConfig)
    {
        // generate something like: resource_loc_1qqwknku2
        _resourceAddress = AddressHelper.GenerateRandomAddress(defaultConfig.NetworkDefinition.ResourceHrp);
    }

    public override StateUpdates Build()
    {
        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var newGlobalEntities = new List<GlobalEntityId>()
        {
            new GlobalEntityId(
                entityType: EntityType.ResourceManager,
                entityAddressHex: AddressHelper.AddressToHex(_resourceAddress),
                globalAddressHex: AddressHelper.AddressToHex(_resourceAddress),
                globalAddress: _resourceAddress),
        };

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
                        fungibleDivisibility: _fungibleDivisibility,
                        metadata: new List<ResourceManagerSubstateAllOfMetadata>()
                        {
                            new("name", "Radix"),
                            new("symbol", "XRD"),
                            new("url", "https://tokens.radixdlt.com"),
                            new("description", "The Radix Public Network's native token, used to pay the network's required transaction fees and to secure the network through staking to its validator nodes."),
                        },
                        totalSupplyAttos: Convert.ToString(Convert.ToDecimal(_totalTokensSupply * Math.Pow(10, _fungibleDivisibility)), CultureInfo.InvariantCulture))
                ),
                substateHex: GenesisData.FungibleResourceCodeHex,
                substateDataHash: "3dc43a58c5cc27bba7d9a96966c8d66a230c781ec04f936bf10130688ed887cf"
            ),
        };

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities);
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

    public FungibleResourceBuilder WithFungibleDivisibility(int fungibleDivisibility)
    {
        _fungibleDivisibility = fungibleDivisibility;

        return this;
    }
}
