using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class VaultBuilder : BuilderBase<StateUpdates>
{
    private string _vaultAddressHex;

    public VaultBuilder(CoreApiStubDefaultConfiguration defaultConfig)
    {
        var vaultAddress = AddressHelper.GenerateRandomAddress(defaultConfig.NetworkDefinition.NormalComponentHrp);
        _vaultAddressHex = AddressHelper.AddressToHex(vaultAddress);
    }

    private string _fungibleTokensResourceAddress = string.Empty;
    private string _fungibleTokensAmountAttos = string.Empty;

    public override StateUpdates Build()
    {
        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var newGlobalEntities = new List<GlobalEntityId>()
        {
            new(
                entityType: EntityType.Vault,
                entityAddressHex: _vaultAddressHex,
                globalAddressHex: _vaultAddressHex,
                globalAddress: AddressHelper.AddressFromHex(_vaultAddressHex)),
        };

        var upSubstates = new List<UpSubstate>()
        {
            new(
                substateId: new SubstateId(
                    entityType: EntityType.Vault,
                    entityAddressHex: _vaultAddressHex,
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
                                resourceAddress: _fungibleTokensResourceAddress,
                                amountAttos: _fungibleTokensAmountAttos)))
                ),
                substateHex: "11050000005661756c74010000001001000000110800000046756e6769626c6504000000b61b000000000000000000000000000000000000000000000000000000000004071232a10a00000000a12000000000000040eaed7446d09c2c9f0c00000000000000000000000000000000000000",
                substateDataHash: "16727d810c5684cdfe732101b8075b69964fafc8b0632a5d6d1a7c193214e991"
            ),
        };

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities);
    }

    public VaultBuilder WithFixedAddress(string vaultAddress)
    {
        _vaultAddressHex = vaultAddress;

        return this;
    }

    public VaultBuilder WithFungibleTokensResourceAddress(string fungibleTokensResourceAddress)
    {
        _fungibleTokensResourceAddress = fungibleTokensResourceAddress;

        return this;
    }

    public VaultBuilder WithFungibleResourceAmountAttos(string fungibleTokensAmountAttos)
    {
        _fungibleTokensAmountAttos = fungibleTokensAmountAttos;

        return this;
    }
}
