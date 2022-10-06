using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit.Abstractions;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class StateUpdatesStore
{
    public StateUpdatesStore(ITestOutputHelper testConsole)
    {
        _testConsole = testConsole;
    }

    private readonly List<StateUpdates> _stateUpdatesList = new();
    private readonly ITestOutputHelper _testConsole;

    public StateUpdates StateUpdates
    {
        get
        {
            return _stateUpdatesList.Combine();
        }
    }

    public string ToJson()
    {
        return _stateUpdatesList.ToJson();
    }

    public void AddStateUpdates(StateUpdates stateUpdates)
    {
        _stateUpdatesList.Add(stateUpdates);
    }

    public GlobalEntityId GetGlobalEntity(string globalAddress)
    {
        return StateUpdates.NewGlobalEntities.Find(ge => ge.GlobalAddress == globalAddress)!;
    }

    public UpSubstate GetLastUpSubstateByEntityAddress(string entityAddress)
    {
        var entityAddressHex = StateUpdates.NewGlobalEntities.FindLast(ge => ge.GlobalAddress == entityAddress)!.EntityAddressHex;

        return GetLastUpSubstateByEntityAddressHex(entityAddressHex);
    }

    public UpSubstate GetLastUpSubstateByEntityAddressHex(string entityAddressHex)
    {
        return StateUpdates.UpSubstates.FindLast(us => us.SubstateId.EntityAddressHex == entityAddressHex)!;
    }

    public DownSubstate GetLastDownSubstateByEntityAddress(string entityAddress)
    {
        var entityAddressHex = StateUpdates.NewGlobalEntities.FindLast(ge => ge.GlobalAddress == entityAddress)!.EntityAddressHex;

        return GetLastDownSubstateByEntityAddressHex(entityAddressHex);
    }

    public DownSubstate GetLastDownSubstateByEntityAddressHex(string entityAddressHex)
    {
        return StateUpdates.DownSubstates.FindLast(us => us.SubstateId.EntityAddressHex == entityAddressHex)!;
    }

    public UpSubstate GetComponentVaultUpSubstateByEntityAddress(string entityAddress)
    {
        var componentUpSubstate = GetLastUpSubstateByEntityAddress(entityAddress);

        var vaultEntityAddressHex = ((componentUpSubstate!.SubstateData.ActualInstance as ComponentStateSubstate)!).OwnedEntities.First(v => v.EntityType == EntityType.Vault).EntityAddressHex;

        return GetLastUpSubstateByEntityAddressHex(vaultEntityAddressHex);
    }

    public DownSubstate GetComponentVaultDownSubstateByEntityAddress(string entityAddress)
    {
        var componentUpSubstate = GetLastUpSubstateByEntityAddress(entityAddress);

        var vaultEntityAddressHex = ((componentUpSubstate!.SubstateData.ActualInstance as ComponentStateSubstate)!).OwnedEntities.First(v => v.EntityType == EntityType.Vault).EntityAddressHex;

        return GetLastDownSubstateByEntityAddressHex(vaultEntityAddressHex);
    }

    public ResourceManagerSubstate GetFungibleResourceUpSubstateByEntityAddress(string entityAddress)
    {
        var resourceUpSubstate = GetLastUpSubstateByEntityAddress(entityAddress);

        return (resourceUpSubstate.SubstateData.ActualInstance as ResourceManagerSubstate)!;
    }

    public int GetFungibleResourceDivisibilityEntityAddress(string entityAddress)
    {
        var vaultUpSubstate = GetComponentVaultUpSubstateByEntityAddress(entityAddress);

        var vaultSubstate = vaultUpSubstate.SubstateData.GetVaultSubstate();

        var xrdResourceAddress = vaultSubstate.PointedResources.First();

        var resourceUpSubstate = GetLastUpSubstateByEntityAddressHex(xrdResourceAddress.Address);

        var resourceManagerSubstate = resourceUpSubstate.SubstateData.ActualInstance as ResourceManagerSubstate;

        return resourceManagerSubstate!.FungibleDivisibility;
    }

    public FeeSummary LockFee()
    {
        // calculate fees
        // state updates when complete

        // find token divisibility
        var divisibility = GetFungibleResourceDivisibilityEntityAddress(GenesisData.SysFaucetComponentAddress);

        var feeSummary = CalculateFeeSummary();
        var feeAmount = Math.Round(feeSummary.CostUnitConsumed / Math.Pow(10, divisibility), 4);

        _testConsole.WriteLine($"Locking fee: {feeAmount} xrd");

        return feeSummary;
    }

    public StateUpdates GetFreeTokens(FeeSummary feeSummary, long amount, out string totalAttos)
    {
        _testConsole.WriteLine($"Getting free tokens from the faucet");

        var vaultDownSubstate = GetComponentVaultDownSubstateByEntityAddress(GenesisData.SysFaucetComponentAddress);

        var vaultUpSubstate = GetComponentVaultUpSubstateByEntityAddress(GenesisData.SysFaucetComponentAddress);

        var vaultSubstate = vaultUpSubstate.SubstateData.GetVaultSubstate();

        var xrdResourceAddress = vaultSubstate.PointedResources.First();

        var resourceDownSubstate = GetLastDownSubstateByEntityAddressHex(xrdResourceAddress.Address);

        var resourceUpSubstate = GetLastUpSubstateByEntityAddressHex(xrdResourceAddress.Address);

        var resourceManagerSubstate = resourceUpSubstate.SubstateData.ActualInstance as ResourceManagerSubstate;

        int divisibility = resourceManagerSubstate!.FungibleDivisibility;

        var downVirtualSubstates = new List<SubstateId>();
        var downSubstates = new List<DownSubstate?>();
        var upSubstates = new List<UpSubstate>();
        var globalEntityIds = new List<GlobalEntityId>();

        // create a new down state with the 'old' balance
        // create a new up state with the new balance and increase its state version

        // new resource total

        var downSubstate = resourceDownSubstate.CloneSubstate();
        if (downSubstate != null)
        {
            downSubstate._Version += 1;
            downSubstates.Add(downSubstate);
        }

        var fees = feeSummary.CostUnitConsumed * long.Parse(feeSummary.CostUnitPriceAttos);
        var total = amount * Math.Pow(10, divisibility) /*+ fees */; // TODO account is debited without fees?
        totalAttos = Convert.ToDecimal(total).ToString(CultureInfo.InvariantCulture);

        var attos = double.Parse(resourceManagerSubstate.TotalSupplyAttos);
        var newAttosBalance = attos - (amount * Math.Pow(10, divisibility)) - fees;

        var upSubstate = resourceUpSubstate.CloneSubstate();
        upSubstate.SubstateData.GetResourceManagerSubstate().TotalSupplyAttos = Convert.ToDecimal(newAttosBalance).ToString(CultureInfo.InvariantCulture);
        upSubstate._Version += 1;
        upSubstates.Add(upSubstate);

        // new vault total

        downSubstate = vaultDownSubstate.CloneSubstate();
        if (downSubstate != null)
        {
            downSubstate._Version += 1;
            downSubstates.Add(downSubstate);
        }

        var vaultResourceAmount = vaultSubstate.ResourceAmount.GetFungibleResourceAmount();
        attos = double.Parse(vaultResourceAmount.AmountAttos);
        newAttosBalance = attos - (amount * Math.Pow(10, divisibility)) - fees;
        vaultResourceAmount!.AmountAttos = Convert.ToDecimal(newAttosBalance).ToString(CultureInfo.InvariantCulture);

        upSubstate = resourceUpSubstate.CloneSubstate();
        upSubstate._Version = upSubstate._Version + 1;
        upSubstates.Add(upSubstate);

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, globalEntityIds);
    }

    public FeeSummary CalculateFeeSummary()
    {
        Random rnd = new Random();

        var tipPercentage = rnd.Next(0, 5); // percents
        var costXrdConsumed = Math.Round(rnd.NextDouble(), 2) * 100000;

        var costUnitConsumed = Convert.ToInt64(costXrdConsumed);
        var xrdBurnedAttos = Convert.ToDecimal(costUnitConsumed * long.Parse(GenesisData.GenesisFeeSummary.CostUnitPriceAttos));
        var xrdTippedAttos = Convert.ToDecimal(costUnitConsumed * long.Parse(GenesisData.GenesisFeeSummary.CostUnitPriceAttos) * tipPercentage / 100);

        return new FeeSummary(
            loanFullyRepaid: true,
            costUnitLimit: GenesisData.GenesisFeeSummary.CostUnitLimit,
            costUnitConsumed: costUnitConsumed,
            costUnitPriceAttos: GenesisData.GenesisFeeSummary.CostUnitPriceAttos,
            tipPercentage: tipPercentage,
            xrdBurnedAttos: xrdBurnedAttos.ToString(CultureInfo.InvariantCulture),
            xrdTippedAttos: xrdTippedAttos.ToString(CultureInfo.InvariantCulture)
        );
    }

    public void UpdateAccountBalance(string accountAddress, long amountToTransfer, FeeSummary? feeSummary)
    {
        var vaultUpSubstate = GetComponentVaultUpSubstateByEntityAddress(accountAddress);

        var vaultResourceAmount = vaultUpSubstate.SubstateData.GetVaultSubstate().ResourceAmount.GetFungibleResourceAmount();

        // find token divisibility
        var divisibility = GetFungibleResourceDivisibilityEntityAddress(accountAddress);

        var paidFeeAttos = feeSummary == null ? 0 : feeSummary.CostUnitConsumed * long.Parse(feeSummary.CostUnitPriceAttos);

        var attos = double.Parse(vaultResourceAmount!.AmountAttos);
        var newTokenBalance = attos + (amountToTransfer * Math.Pow(10, divisibility)) - paidFeeAttos;
        var newAttos = Convert.ToDecimal(newTokenBalance, CultureInfo.InvariantCulture);
        vaultResourceAmount!.AmountAttos = newAttos.ToString(CultureInfo.InvariantCulture);
    }
}
