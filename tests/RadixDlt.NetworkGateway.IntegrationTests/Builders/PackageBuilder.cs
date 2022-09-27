using RadixDlt.CoreApiSdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class PackageBuilder : IBuilder<StateUpdates>
{
    private List<StateUpdates> _blueprints = new();

    public StateUpdates Build()
    {
        if (!_blueprints.Any())
        {
            throw new ArgumentException("No blueprints found.");
        }

        var downSubstates = new List<DownSubstate>();
        var upSubstates = new List<UpSubstate>();
        var downVirtualSubstates = new List<SubstateId>();
        var newGlobalEntities = new List<GlobalEntityId>();

        foreach (var bluePrint in _blueprints)
        {
            downSubstates.AddRange(bluePrint.DownSubstates);
            upSubstates.AddRange(bluePrint.UpSubstates);
            downVirtualSubstates.AddRange(bluePrint.DownVirtualSubstates);
            newGlobalEntities.AddRange(bluePrint.NewGlobalEntities);
        }

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities);
    }

    public PackageBuilder WithBlueprints(List<StateUpdates> blueprints)
    {
        _blueprints = blueprints;

        return this;
    }
}
