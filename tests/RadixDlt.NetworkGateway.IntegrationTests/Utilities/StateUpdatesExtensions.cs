using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Builders;
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

    public static string? GetVaultAddressHexByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var componentUpSubstate = stateUpdates.GetLastUpSubstateByEntityAddress(entityAddress);

        return (componentUpSubstate.SubstateData.ActualInstance as ComponentStateSubstate)?.OwnedEntities.First(v => v.EntityType == EntityType.Vault).EntityAddressHex;
    }

    public static DownSubstate? GetLastVaultDownSubstateByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
    {
        var vaultEntityAddressHex = GetVaultAddressHexByEntityAddress(stateUpdates, entityAddress);

        return stateUpdates.DownSubstates.FirstOrDefault(ds => ds.SubstateId.EntityAddressHex == vaultEntityAddressHex);
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

    public static string GetFungibleResoureAddressByEntityAddress(this StateUpdates stateUpdates, string entityAddress)
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

    /// <summary>
    /// Withdraws a given amount of tokens form the vault, creates new vault's down and up  substates, and updates vault balance (ResourceAmount.AmountAttos).
    /// </summary>
    /// <param name="stateUpdates">state updates.</param>
    /// <param name="componentAddress">vault owned by component address.</param>
    /// <param name="feeSummary">fee summary.</param>
    /// <param name="xrdAmount">amount of tokens to withdraw.</param>
    /// <param name="newVaultBalanceAttos">new vault balance in attos.</param>
    /// <returns>updated state updates.</returns>
    public static StateUpdates TakeTokensFromVault(this StateUpdates stateUpdates, string componentAddress, FeeSummary feeSummary, double xrdAmount, out string newVaultBalanceAttos)
    {
        // a default value of free XRD tokens
        var tokenAmountAttos = TokenAttosConverter.Tokens2Attos(xrdAmount);

        var resourceAddress = GetFungibleResoureAddressByEntityAddress(stateUpdates, componentAddress);

        var vaultUpSubstate = GetLastVaultUpSubstateByEntityAddress(stateUpdates, componentAddress);

        var vaultAddressHex = vaultUpSubstate.SubstateId.EntityAddressHex;

        var vaultSubstate = vaultUpSubstate.SubstateData.GetVaultSubstate();

        var vaultResourceAmount = vaultSubstate.ResourceAmount.GetFungibleResourceAmount();
        var vaultResourceAmountAttos = TokenAttosConverter.ParseAttosFromString(vaultResourceAmount.AmountAttos);

        var feesAttos = feeSummary.CostUnitConsumed
                        * TokenAttosConverter.ParseAttosFromString(feeSummary.CostUnitPriceAttos);

        // _testConsole.WriteLine($"Paid fees {TokenAttosConverter.Attos2Tokens(feesAttos)} xrd");

        newVaultBalanceAttos = (vaultResourceAmountAttos - tokenAmountAttos - feesAttos).ToString();

        var vault = new VaultBuilder()
            .WithFixedAddressHex(vaultAddressHex)
            .WithFungibleTokensResourceAddress(resourceAddress)
            .WithFungibleResourceAmountAttos(newVaultBalanceAttos)
            .WithDownState(new DownSubstate(
                new SubstateId(
                    EntityType.Vault,
                    vaultAddressHex,
                    SubstateType.Vault,
                    Convert.ToHexString(Encoding.UTF8.GetBytes("substateKeyHex")).ToLowerInvariant()
                ), substateDataHash: "hash", vaultUpSubstate._Version)
            ).Build();

        return vault;
    }
}
