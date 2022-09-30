using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class PackageBuilder : IBuilder<(TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates)>
{
    private List<IBlueprint> _blueprints = new();
    private string _packageAddress = "package_tdx_21_1qyqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqsc9ekjt";

    public (TestGlobalEntity TestGlobalEntity, StateUpdates StateUpdates) Build()
    {
        if (!_blueprints.Any())
        {
            throw new ArgumentException("No blueprints found.");
        }

        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var globalEntityId = new TestGlobalEntity()
        {
            EntityType = EntityType.Package,
            EntityAddressHex = AddressHelper.AddressToHex(_packageAddress),
            GlobalAddressHex = AddressHelper.AddressToHex(_packageAddress),
            GlobalAddress = _packageAddress,
            Name = string.Join(",", _blueprints.Select(b => b.Name)),
        };

        var newGlobalEntities = new List<GlobalEntityId>() { globalEntityId, };

        var upSubstates = new List<UpSubstate>()
        {
            new(
                substateId: new SubstateId(
                    entityType: EntityType.Package,
                    entityAddressHex: AddressHelper.AddressToHex(_packageAddress),
                    substateType: SubstateType.Package,
                    substateKeyHex: "00"
                ),
                version: 0L,
                substateData: new Substate(
                    actualInstance: new PackageSubstate(
                        entityType: EntityType.Package,
                        substateType: SubstateType.Package,
                        codeHex: GenesisData.SysFaucetCodeHex)
                ),
                substateHex: GenesisData.SysFaucetSubstateHex,
                substateDataHash: "ccceea5952ca631dcf65146141884a6fbeb0b3b0535fee3d58124a5e9823cb53"
            ),
        };

        return (globalEntityId, new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities));
    }

    public PackageBuilder WithBlueprints(List<IBlueprint> blueprints)
    {
        _blueprints = blueprints;

        return this;
    }

    public PackageBuilder WithFixedAddress(string packageAddress)
    {
        _packageAddress = packageAddress;

        return this;
    }
}
