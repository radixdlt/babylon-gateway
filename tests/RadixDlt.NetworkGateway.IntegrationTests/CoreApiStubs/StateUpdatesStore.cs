using RadixDlt.CoreApiSdk.Model;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class StateUpdatesStore
{
    private readonly List<StateUpdates> _stateUpdatesList = new();

    public StateUpdates StateUpdates
    {
        get
        {
            return Combine(_stateUpdatesList);
        }
    }

    public static StateUpdates Combine(List<StateUpdates> stateUpdatesList)
    {
        return new StateUpdates(
            stateUpdatesList.SelectMany(s => s.DownVirtualSubstates).ToList(),
            stateUpdatesList.SelectMany(s => s.UpSubstates).ToList(),
            stateUpdatesList.SelectMany(s => s.DownSubstates).ToList(),
            stateUpdatesList.SelectMany(s => s.NewGlobalEntities).ToList());
    }

    public void AddStateUpdates(StateUpdates stateUpdates)
    {
        _stateUpdatesList.Add(stateUpdates);
    }
}
