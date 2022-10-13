using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class PackageBuilder : BuilderBase<StateUpdates>
{
    private List<IBlueprint> _blueprints = new();
    private string _packageAddress;
    private string _packageAddressHex;

    public PackageBuilder()
    {
        _packageAddress = AddressHelper.GenerateRandomAddress(GenesisData.NetworkDefinition.PackageHrp);

        _packageAddressHex = AddressHelper.AddressToHex(_packageAddress);
    }

    public override StateUpdates Build()
    {
        var downSubstates = new List<DownSubstate>();

        var downVirtualSubstates = new List<SubstateId>();

        var newGlobalEntities = new List<GlobalEntityId>
        {
            new(
                EntityType.Package,
                _packageAddressHex,
                _packageAddressHex,
                _packageAddress),
        };

        var upSubstates = new List<UpSubstate>
        {
            new(
                new SubstateId(
                    EntityType.Package,
                    _packageAddressHex,
                    SubstateType.Package,
                    "00"
                ),
                0L,
                substateData: new Substate(
                    new PackageSubstate(
                        EntityType.Package,
                        SubstateType.Package,
                        GenesisData.SysFaucetCodeHex)
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

        _packageAddressHex = AddressHelper.AddressToHex(_packageAddress);

        return this;
    }

    public PackageBuilder WithFixedAddressHex(string packageAddressHex)
    {
        _packageAddressHex = packageAddressHex;

        _packageAddress = AddressHelper.AddressFromHex(
            _packageAddressHex,
            GenesisData.NetworkDefinition.PackageHrp);

        return this;
    }
}
