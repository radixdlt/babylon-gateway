using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

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
