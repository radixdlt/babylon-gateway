using RadixDlt.CoreApiSdk.Model;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.Utilities;

public static class StateUpdatesExtensions
{
    public static StateUpdates Combine(this List<StateUpdates> stateUpdatesList)
    {
        return new StateUpdates(
            stateUpdatesList.SelectMany(s => s.DownVirtualSubstates).ToList(),
            stateUpdatesList.SelectMany(s => s.UpSubstates).ToList(),
            stateUpdatesList.SelectMany(s => s.DownSubstates).ToList(),
            stateUpdatesList.SelectMany(s => s.NewGlobalEntities).ToList());
    }

    public static string ToJson(this List<StateUpdates> stateUpdatesList)
    {
        return Combine(stateUpdatesList).ToJson();
    }
}
