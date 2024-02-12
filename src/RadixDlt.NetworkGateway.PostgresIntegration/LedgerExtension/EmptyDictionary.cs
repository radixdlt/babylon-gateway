using System.Collections.Generic;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal static class EmptyDictionary<TKey, TValue>
    where TKey : notnull
{
    public static Dictionary<TKey, TValue> Instance { get; } = new();
}
