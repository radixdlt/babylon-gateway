using RadixDlt.CoreApiSdk.Model;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

public class TestGlobalEntities : List<TestGlobalEntity>
{
    private List<StateUpdates> _stateUpdatesList = new();

    public StateUpdates StateUpdates
    {
        get
        {
            return _stateUpdatesList.Combine();
        }
    }

    public void AddStateUpdates(StateUpdates stateUpdates)
    {
        _stateUpdatesList.Add(stateUpdates);
    }
}
