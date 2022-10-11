using RadixDlt.CoreApiSdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadixDlt.NetworkGateway.IntegrationTests.Utilities;

public static class StateUpdatesExtensions
{
    public static StateUpdates Add(this StateUpdates stateUpdates1, StateUpdates stateUpdates2)
    {
        stateUpdates1.DownVirtualSubstates.AddRange(stateUpdates2.DownVirtualSubstates);
        stateUpdates1.UpSubstates.AddRange(stateUpdates2.UpSubstates);
        stateUpdates1.DownSubstates.AddRange(stateUpdates2.DownSubstates);
        stateUpdates1.NewGlobalEntities.AddRange(stateUpdates2.NewGlobalEntities);

        return stateUpdates1;
    }

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

    public static GlobalEntityId GetOrAdd(this List<GlobalEntityId> source, GlobalEntityId item)
    {
        var globalEntityId = source.Find(ge => ge.GlobalAddress == item.GlobalAddress &&
                                            ge.GlobalAddressHex == item.GlobalAddressHex);

        if (globalEntityId != null)
        {
            return globalEntityId;
        }

        source.Add(item);

        return item;
    }

    public static StateUpdates TakeTokensFromVault(this StateUpdates stateUpdates, string componentAddress, FeeSummary feeSummary, double xrdAmount, out string newTotalAttos)
    {
        // a default value of free XRD tokens
        var tokenAmountAttos = TokenAttosConverter.Tokens2Attos(xrdAmount);

        var vaultDownSubstate = GetLastVaultDownSubstateByEntityAddress(stateUpdates, componentAddress);

        var vaultUpSubstate = GetLastVaultUpSubstateByEntityAddress(stateUpdates, componentAddress);

        var downVirtualSubstates = new List<SubstateId>();
        var downSubstates = new List<DownSubstate?>();
        var upSubstates = new List<UpSubstate>();
        var globalEntityIds = new List<GlobalEntityId>();

        // create a new down state with the 'old' balance
        // create a new up state with the new balance and increase its state version

        // new vault total

        // add new vault down substate
        var newVaultDownSubstate = vaultDownSubstate.CloneSubstate();
        if (newVaultDownSubstate == null)
        {
            newVaultDownSubstate = new DownSubstate(
                new SubstateId(
                    EntityType.Vault,
                    GetFungibleResoureAddressHexByEntityAddress(stateUpdates, componentAddress),
                    SubstateType.Vault,
                    Convert.ToHexString(Encoding.UTF8.GetBytes("substateKeyHex")).ToLowerInvariant()
                ),
                substateDataHash: "hash"
            );
        }

        newVaultDownSubstate._Version = vaultUpSubstate._Version;

        downSubstates.Add(newVaultDownSubstate);

        // add new vault up state
        var newVaultUpSubstate = vaultUpSubstate.CloneSubstate();
        newVaultUpSubstate._Version += 1;

        var newVaultSubstate = newVaultUpSubstate.SubstateData.GetVaultSubstate();

        var vaultResourceAmount = newVaultSubstate.ResourceAmount.GetFungibleResourceAmount();
        var vaultResourceAmountAttos = TokenAttosConverter.ParseAttosFromString(vaultResourceAmount.AmountAttos);

        var feesAttos = feeSummary.CostUnitConsumed
                        * TokenAttosConverter.ParseAttosFromString(feeSummary.CostUnitPriceAttos);

        // _testConsole.WriteLine($"Paid fees {TokenAttosConverter.Attos2Tokens(feesAttos)} xrd");

        var newAttosBalance = vaultResourceAmountAttos - tokenAmountAttos - feesAttos;

        vaultResourceAmount!.AmountAttos = newAttosBalance.ToString();

        newTotalAttos = tokenAmountAttos.ToString();

        upSubstates.Add(newVaultUpSubstate);

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, globalEntityIds);
    }
}
