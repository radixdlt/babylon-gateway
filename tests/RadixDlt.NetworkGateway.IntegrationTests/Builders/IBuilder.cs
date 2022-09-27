using RadixDlt.CoreApiSdk.Model;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public interface IBuilder<T>
{
    public T Build();
}

public static class StateUpdatesExtensions
{
    public static string ToJson(this List<StateUpdates> stateUpdatesList)
    {
        var downSubstates = new List<DownSubstate>();
        var upSubstates = new List<UpSubstate>();
        var downVirtualSubstates = new List<SubstateId>();
        var newGlobalEntities = new List<GlobalEntityId>();

        foreach (var stateUpdates in stateUpdatesList)
        {
            downSubstates.AddRange(stateUpdates.DownSubstates);
            upSubstates.AddRange(stateUpdates.UpSubstates);
            downVirtualSubstates.AddRange(stateUpdates.DownVirtualSubstates);
            newGlobalEntities.AddRange(stateUpdates.NewGlobalEntities);
        }

        return Combine(stateUpdatesList).ToJson();
    }

    public static StateUpdates Combine(this List<StateUpdates> stateUpdatesList)
    {
        var downSubstates = new List<DownSubstate>();
        var upSubstates = new List<UpSubstate>();
        var downVirtualSubstates = new List<SubstateId>();
        var newGlobalEntities = new List<GlobalEntityId>();

        foreach (var stateUpdates in stateUpdatesList)
        {
            downSubstates.AddRange(stateUpdates.DownSubstates);
            upSubstates.AddRange(stateUpdates.UpSubstates);
            downVirtualSubstates.AddRange(stateUpdates.DownVirtualSubstates);
            newGlobalEntities.AddRange(stateUpdates.NewGlobalEntities);
        }

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, newGlobalEntities);
    }
}
