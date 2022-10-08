using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
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

    public FeeSummary LockFee()
    {
        // calculate fees
        // state updates when complete

        // find token divisibility
        var divisibility = StateUpdates.GetFungibleResourceDivisibilityEntityAddress(GenesisData.SysFaucetComponentAddress);

        var feeSummary = CalculateFeeSummary();

        var paidFeeAttos = feeSummary.CostUnitConsumed * TokenAttosConverter.ParseAttosFromString(feeSummary.CostUnitPriceAttos);
        var feeAmount = TokenAttosConverter.Attos2Tokens(paidFeeAttos);

        _testConsole.WriteLine($"Locking fee: {feeAmount} xrd");

        return feeSummary;
    }

    public StateUpdates TakeTokensFromVault(string componentAddress, FeeSummary feeSummary, double xrdAmount, out string newTotalAttos)
    {
        // a default value of free XRD tokens
        var tokenAmountAttos = TokenAttosConverter.Tokens2Attos(xrdAmount);

        var vaultDownSubstate = StateUpdates.GetLastVaultDownSubstateByEntityAddress(componentAddress);

        var vaultUpSubstate = StateUpdates.GetLastVaultUpSubstateByEntityAddress(componentAddress);

        var downVirtualSubstates = new List<SubstateId>();
        var downSubstates = new List<DownSubstate?>();
        var upSubstates = new List<UpSubstate>();
        var globalEntityIds = new List<GlobalEntityId>();

        // create a new down state with the 'old' balance
        // create a new up state with the new balance and increase its state version

        // new vault total

        // add new vault down substate
        var newVaultDownSubstate = vaultDownSubstate.CloneSubstate();
        if (newVaultDownSubstate != null)
        {
            newVaultDownSubstate._Version = vaultUpSubstate._Version;
        }
        else
        {
            newVaultDownSubstate = new DownSubstate(
                new SubstateId(
                    EntityType.Vault,
                    StateUpdates.GetFungibleResoureAddressHexByEntityAddress(componentAddress),
                    SubstateType.Vault,
                    Convert.ToHexString(Encoding.UTF8.GetBytes("substateKeyHex")).ToLowerInvariant()
                ), substateDataHash: "hash", vaultUpSubstate._Version
            );
        }

        downSubstates.Add(newVaultDownSubstate);

        // add new vault up state
        var newVaultUpSubstate = vaultUpSubstate.CloneSubstate();
        newVaultUpSubstate._Version += 1;

        var newVaultSubstate = newVaultUpSubstate.SubstateData.GetVaultSubstate();

        var vaultResourceAmount = newVaultSubstate.ResourceAmount.GetFungibleResourceAmount();
        var vaultResourceAmountAttos = TokenAttosConverter.ParseAttosFromString(vaultResourceAmount.AmountAttos);

        var feesAttos = feeSummary.CostUnitConsumed
                        * TokenAttosConverter.ParseAttosFromString(feeSummary.CostUnitPriceAttos);

        _testConsole.WriteLine($"Paid fees {TokenAttosConverter.Attos2Tokens(feesAttos)} xrd");

        var newAttosBalance = vaultResourceAmountAttos - tokenAmountAttos - feesAttos;

        vaultResourceAmount!.AmountAttos = newAttosBalance.ToString();

        newTotalAttos = tokenAmountAttos.ToString();

        upSubstates.Add(newVaultUpSubstate);

        return new StateUpdates(downVirtualSubstates, upSubstates, downSubstates, globalEntityIds);
    }

    public FeeSummary CalculateFeeSummary()
    {
        Random rnd = new Random();

        var tipPercentage = rnd.Next(0, 5); // percents
        var costUnitConsumed = (BigInteger)(rnd.NextDouble() * 1000000);

        var xrdBurnedAttos = costUnitConsumed * TokenAttosConverter.ParseAttosFromString(GenesisData.GenesisFeeSummary.CostUnitPriceAttos);

        var xrdTippedAttos = costUnitConsumed *
            TokenAttosConverter.ParseAttosFromString(GenesisData.GenesisFeeSummary.CostUnitPriceAttos) * tipPercentage / 100;

        return new FeeSummary(
            loanFullyRepaid: true,
            costUnitLimit: GenesisData.GenesisFeeSummary.CostUnitLimit,
            costUnitConsumed: (long)costUnitConsumed,
            costUnitPriceAttos: GenesisData.GenesisFeeSummary.CostUnitPriceAttos,
            tipPercentage: tipPercentage,
            xrdBurnedAttos: xrdBurnedAttos.ToString(),
            xrdTippedAttos: xrdTippedAttos.ToString()
        );
    }

    public void UpdateAccountBalance(string accountAddress, long amountToTransfer, FeeSummary? feeSummary)
    {
        var vaultUpSubstate = StateUpdates.GetLastVaultUpSubstateByEntityAddress(accountAddress);

        var vaultResourceAmount = vaultUpSubstate.SubstateData.GetVaultSubstate().ResourceAmount.GetFungibleResourceAmount();

        // find token divisibility
        var divisibility = StateUpdates.GetFungibleResourceDivisibilityEntityAddress(accountAddress);

        var paidFeeAttos = feeSummary == null ? 0 : feeSummary.CostUnitConsumed * long.Parse(feeSummary.CostUnitPriceAttos);

        var attos = double.Parse(vaultResourceAmount!.AmountAttos);
        var newTokenBalance = attos + (amountToTransfer * Math.Pow(10, divisibility)) - paidFeeAttos;
        var newAttos = Convert.ToDecimal(newTokenBalance, CultureInfo.InvariantCulture);
        vaultResourceAmount!.AmountAttos = newAttos.ToString(CultureInfo.InvariantCulture);
    }
}
