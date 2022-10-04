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

    public GlobalEntityId GetGlobalEntity(string globalAddress)
    {
        return StateUpdates.NewGlobalEntities.Find(ge => ge.GlobalAddress == globalAddress)!;
    }

    public UpSubstate GetLastUpstateByGlobalAddress(string globalAddress)
    {
        var entityAddressHex = StateUpdates.NewGlobalEntities.FindLast(ge => ge.GlobalAddress == globalAddress)!.EntityAddressHex;

        return GetLastUpstateByEntityAddressHex(entityAddressHex);
    }

    public UpSubstate GetLastUpstateByEntityAddressHex(string entityAddressHex)
    {
        return StateUpdates.UpSubstates.FindLast(us => us.SubstateId.EntityAddressHex == entityAddressHex)!;
    }
}
