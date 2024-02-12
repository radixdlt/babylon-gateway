using System;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class ChangeTracker<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _store = new();
    private readonly List<TKey> _insertionOrder = new();

    public IList<TKey> Keys => _insertionOrder;

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        return _store.GetOrAdd(key, _ =>
        {
            _insertionOrder.Add(key);

            return factory(key);
        });
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> AsEnumerable()
    {
        return _insertionOrder.Select(key => new KeyValuePair<TKey, TValue>(key, _store[key]));
    }
}
