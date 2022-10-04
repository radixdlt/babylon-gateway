using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class PackageBuilder : BuilderBase<StateUpdates>
{
    private List<IBlueprint> _blueprints = new();
    private string _packageAddress;

    public PackageBuilder(CoreApiStubDefaultConfiguration defaultConfig)
    {
        _packageAddress = AddressHelper.GenerateRandomAddress(defaultConfig.NetworkDefinition.PackageHrp);
    }

    public override StateUpdates Build()
    {
        if (!_blueprints.Any())
        {
            throw new ArgumentException("No blueprints found.");
        }

        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var newGlobalEntities = new List<GlobalEntityId>()
        {
            new GlobalEntityId(
                entityType: EntityType.Package,
                entityAddressHex: AddressHelper.AddressToHex(_packageAddress),
                globalAddressHex: AddressHelper.AddressToHex(_packageAddress),
                globalAddress: _packageAddress),
        };

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

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities);
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
