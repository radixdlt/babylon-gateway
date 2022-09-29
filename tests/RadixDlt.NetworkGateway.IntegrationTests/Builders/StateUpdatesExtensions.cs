using RadixDlt.CoreApiSdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders
{
    public static class StateUpdatesExtensions
    {
        public static string ToJson(this List<StateUpdates> stateUpdatesList)
        {
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
}
