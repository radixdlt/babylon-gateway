﻿using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class FungibleResourceBuilder : BuilderBase<StateUpdates>
{
    private readonly StateUpdates _stateUpdates;

    private string _resourceAddress;
    private string _resourceAddressHex;
    private string _resourceName = "XRD";
    private string _totalSupplyAttos = string.Empty;
    private int _fungibleDivisibility = 18;

    public FungibleResourceBuilder(StateUpdates stateUpdates)
    {
        _stateUpdates = stateUpdates;

        // generate something like: resource_loc_1qqwknku2
        _resourceAddress = AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.ResourceHrp);
        _resourceAddressHex = AddressHelper.AddressToHex(_resourceAddress);
    }

    public override StateUpdates Build()
    {
        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var newGlobalEntities = new List<GlobalEntityId>();

        newGlobalEntities.GetOrAdd(new GlobalEntityId(
            entityType: EntityType.ResourceManager,
            entityAddressHex: _resourceAddressHex,
            globalAddressHex: _resourceAddressHex,
            globalAddress: _resourceAddress));

        var upSubstates = new List<UpSubstate>()
        {
            new(
                substateId: new SubstateId(
                    entityType: EntityType.ResourceManager,
                    entityAddressHex: _resourceAddressHex,
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
                        totalSupplyAttos: _totalSupplyAttos)
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
        _resourceAddressHex = AddressHelper.AddressToHex(_resourceAddress);
        return this;
    }

    public FungibleResourceBuilder WithFixedAddressHex(string resourceAddressHex)
    {
        _resourceAddressHex = resourceAddressHex;

        _resourceAddress = AddressHelper.AddressFromHex(
            _resourceAddressHex,
            GenesisData.NetworkDefinition.ResourceHrp);
        return this;
    }

    public FungibleResourceBuilder WithResourceName(string resourceName)
    {
        _resourceName = resourceName;

        return this;
    }

    public FungibleResourceBuilder WithTotalSupplyAttos(string totalSupplyAttos)
    {
        _totalSupplyAttos = totalSupplyAttos;

        return this;
    }

    public FungibleResourceBuilder WithFungibleDivisibility(int fungibleDivisibility)
    {
        _fungibleDivisibility = fungibleDivisibility;

        return this;
    }
}