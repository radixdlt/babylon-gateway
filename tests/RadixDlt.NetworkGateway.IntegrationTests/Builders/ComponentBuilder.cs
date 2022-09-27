using RadixDlt.CoreApiSdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class ComponentBuilder : IBuilder<StateUpdates>
{
    private StateUpdates? _component = null;

    public StateUpdates Build()
    {
        if (_component == null)
        {
            throw new NullReferenceException("No component found.");
        }

        return new StateUpdates(_component.DownVirtualSubstates, _component.UpSubstates, _component.DownSubstates, _component.NewGlobalEntities);
    }

    public ComponentBuilder WithComponent(StateUpdates? component)
    {
        _component = component;

        return this;
    }
}
