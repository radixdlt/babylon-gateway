using Common.Numerics;

namespace DataAggregator.LedgerExtension;

public static class DictionaryExtensions
{
    public static void TrackBalanceDelta<TBalanceIdentifier>(
        this Dictionary<TBalanceIdentifier, TokenAmount> balanceTracker,
        TBalanceIdentifier balanceIdentifier,
        TokenAmount balanceDelta
    )
        where TBalanceIdentifier : notnull
    {
        balanceTracker[balanceIdentifier] =
            balanceTracker.GetValueOrDefault(balanceIdentifier, TokenAmount.Zero) + balanceDelta;
    }
}
