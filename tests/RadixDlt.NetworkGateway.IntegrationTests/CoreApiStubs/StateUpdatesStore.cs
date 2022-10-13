using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit.Abstractions;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class StateUpdatesStore
{
    private readonly List<StateUpdates> _stateUpdatesList = new();
    private readonly ITestOutputHelper _testConsole;

    public StateUpdatesStore(ITestOutputHelper testConsole)
    {
        _testConsole = testConsole;
    }

    public StateUpdates StateUpdates
    {
        get => _stateUpdatesList.Combine();

        set
        {
            _stateUpdatesList.Clear();
            _stateUpdatesList.Add(value);
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

        var feeSummary = CalculateFeeSummary();

        var paidFeeAttos = feeSummary.CostUnitConsumed * TokenAttosConverter.ParseAttosFromString(feeSummary.CostUnitPriceAttos);
        var feeAmount = TokenAttosConverter.Attos2Tokens(paidFeeAttos);

        _testConsole.WriteLine($"Locking fee: {feeAmount} xrd");

        return feeSummary;
    }

    public FeeSummary CalculateFeeSummary()
    {
        var rnd = new Random();

        var tipPercentage = rnd.Next(0, 5); // percents
        var costUnitConsumed = (BigInteger)(rnd.NextDouble() * 1000000);

        var xrdBurnedAttos = costUnitConsumed * TokenAttosConverter.ParseAttosFromString(GenesisData.GenesisFeeSummary.CostUnitPriceAttos);

        var xrdTippedAttos = costUnitConsumed *
            TokenAttosConverter.ParseAttosFromString(GenesisData.GenesisFeeSummary.CostUnitPriceAttos) * tipPercentage / 100;

        var feeSummary = new FeeSummary(
            true,
            GenesisData.GenesisFeeSummary.CostUnitLimit,
            (long)costUnitConsumed,
            GenesisData.GenesisFeeSummary.CostUnitPriceAttos,
            tipPercentage,
            xrdBurnedAttos.ToString(),
            xrdTippedAttos.ToString()
        );

        _testConsole.WriteLine($"Calculated fee summary:\n{feeSummary}");

        return feeSummary;
    }
}
