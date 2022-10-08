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

    public static GlobalEntityId GetGlobalEntity(this StateUpdates stateUpdates, string globalAddress)
    {
        return stateUpdates.NewGlobalEntities.Find(ge => ge.GlobalAddress == globalAddress)!;
    }

    public static UpSubstate GetLastUpSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var entityAddressHex = stateUpdates.NewGlobalEntities.FindLast(ge => ge.GlobalAddress == entityAddress)!.EntityAddressHex;

        return stateUpdates.GetLastUpSubstateByEntityAddressHex(entityAddressHex);
    }

    public static UpSubstate GetLastUpSubstateByEntityAddressHex(this StateUpdates stateUpdates, string? entityAddressHex)
    {
        return stateUpdates.UpSubstates.FindLast(us => us.SubstateId.EntityAddressHex == entityAddressHex)!;
    }

    public static DownSubstate GetLastDownSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var entityAddressHex = stateUpdates.NewGlobalEntities.FindLast(ge => ge.GlobalAddress == entityAddress)!.EntityAddressHex;

        return stateUpdates.GetLastDownSubstateByEntityAddressHex(entityAddressHex);
    }

    public static DownSubstate GetLastDownSubstateByEntityAddressHex(this StateUpdates stateUpdates, string? entityAddressHex)
    {
        return stateUpdates.DownSubstates.FindLast(us => us.SubstateId.EntityAddressHex == entityAddressHex)!;
    }

    public static UpSubstate GetLastVaultUpSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var componentUpSubstate = stateUpdates.GetLastUpSubstateByEntityAddress(entityAddress);

        var vaultEntityAddressHex = (componentUpSubstate.SubstateData.ActualInstance as ComponentStateSubstate)?.OwnedEntities.First(v => v.EntityType == EntityType.Vault).EntityAddressHex;

        return stateUpdates.GetLastUpSubstateByEntityAddressHex(vaultEntityAddressHex);
    }

    public static DownSubstate GetLastVaultDownSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var componentUpSubstate = stateUpdates.GetLastUpSubstateByEntityAddress(entityAddress);

        var vaultEntityAddressHex = (componentUpSubstate.SubstateData.ActualInstance as ComponentStateSubstate)?.OwnedEntities.First(v => v.EntityType == EntityType.Vault).EntityAddressHex;

        return stateUpdates.GetLastDownSubstateByEntityAddressHex(vaultEntityAddressHex);
    }

    public static ResourceManagerSubstate GetFungibleResourceUpSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var resourceUpSubstate = stateUpdates.GetLastUpSubstateByEntityAddress(entityAddress);

        return (resourceUpSubstate.SubstateData.ActualInstance as ResourceManagerSubstate)!;
    }

    public static int GetFungibleResourceDivisibilityEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var vaultUpSubstate = stateUpdates.GetLastVaultUpSubstateByEntityAddress(entityAddress);

        var vaultSubstate = vaultUpSubstate.SubstateData.GetVaultSubstate();

        var xrdResourceAddress = vaultSubstate.PointedResources.First();

        var resourceUpSubstate = stateUpdates.GetLastUpSubstateByEntityAddressHex(xrdResourceAddress.Address);

        var resourceManagerSubstate = resourceUpSubstate.SubstateData.ActualInstance as ResourceManagerSubstate;

        return resourceManagerSubstate!.FungibleDivisibility;
    }

    public static string GetFungibleResoureAddressHexByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var vaultUpSubstate = stateUpdates.GetLastVaultUpSubstateByEntityAddress(entityAddress);

        var vaultSubstate = vaultUpSubstate.SubstateData.GetVaultSubstate();

        var xrdResourceAddress = vaultSubstate.PointedResources.First();

        return xrdResourceAddress.Address;
    }
}
