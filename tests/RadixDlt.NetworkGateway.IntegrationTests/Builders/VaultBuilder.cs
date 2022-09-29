using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class VaultBuilder : IBuilder<(TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates)>
{
    private string _vaultAddressHex = string.Empty;

    public VaultBuilder(NetworkConfigurationResponse networkConfiguration)
    {
        var vaultAddress = AddressHelper.GenerateRandomAddress("component_" + networkConfiguration.NetworkHrpSuffix);
        _vaultAddressHex = AddressHelper.AddressToHex(vaultAddress);
    }

    private string _resourceAddress = string.Empty;
    private string _vaultName = string.Empty;

    public (TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates) Build()
    {
        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var newGlobalEntities = new List<GlobalEntityId>();

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
                                resourceAddress: _resourceAddress,
                                amountAttos: "1000000000000000000000000000000")))
                ),
                substateHex: "11050000005661756c74010000001001000000110800000046756e6769626c6504000000b61b000000000000000000000000000000000000000000000000000000000004071232a10a00000000a12000000000000040eaed7446d09c2c9f0c00000000000000000000000000000000000000",
                substateDataHash: "16727d810c5684cdfe732101b8075b69964fafc8b0632a5d6d1a7c193214e991"
            ),
        };

        var globalEntity = new TestGlobalEntity()
        {
            Name = _vaultName,
            EntityType = EntityType.Vault,
            GlobalAddress = AddressHelper.AddressFromHex(_vaultAddressHex),
            EntityAddressHex = _vaultAddressHex,
            GlobalAddressHex = _vaultAddressHex,
        };

        return (globalEntity, new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities));
    }

    public VaultBuilder WithFixedAddress(string vaultAddress)
    {
        _vaultAddressHex = vaultAddress;

        return this;
    }

    public VaultBuilder WithFungibleTokens(string resourceAddress)
    {
        _resourceAddress = resourceAddress;

        return this;
    }

    public VaultBuilder WithVaultName(string vaultName)
    {
        _vaultName = vaultName;

        return this;
    }
}
