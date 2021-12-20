namespace Common.Utilities;

/// <summary>
/// Provides a sized-cache, with a max capacity, which evicts old entries from the cache.
/// Thread safe.
/// </summary>
/// <typeparam name="TKey">The key for the cache.</typeparam>
/// <typeparam name="TValue">The value for the cache.</typeparam>
public class LruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly int _maxCapacity;
    private readonly Dictionary<TKey, TValue> _cache;
    private readonly Dictionary<TKey, LinkedListNode<TKey>> _keyToQueueMap;
    private readonly LinkedList<TKey> _recentlySeenQueue = new();
    private readonly object _lock = new();

    public LruCache(int maxCapacity, IEqualityComparer<TKey>? equalityComparer = null)
    {
        if (maxCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCapacity), "The maximum capacity must be >= 1");
        }

        _maxCapacity = maxCapacity;
        _cache = new Dictionary<TKey, TValue>(equalityComparer);
        _keyToQueueMap = new Dictionary<TKey, LinkedListNode<TKey>>(equalityComparer);
    }

    public TValue? GetOrDefault(TKey key, TValue? defaultValue = default)
    {
        lock (_lock)
        {
            if (!_cache.ContainsKey(key))
            {
                return defaultValue;
            }

            MarkExistingValueAsRecentlySeen(key);
            return _cache[key];
        }
    }

    public void Set(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_keyToQueueMap.ContainsKey(key))
            {
                _cache[key] = value;
                MarkExistingValueAsRecentlySeen(key);
            }
            else
            {
                while (_recentlySeenQueue.Count >= _maxCapacity)
                {
                    DropOldestCachedItem();
                }

                AddItem(key, value);
            }
        }
    }

    private void MarkExistingValueAsRecentlySeen(TKey key)
    {
        _recentlySeenQueue.Remove(_keyToQueueMap[key]);
        _recentlySeenQueue.AddFirst(_keyToQueueMap[key]);
    }

    private void AddItem(TKey key, TValue value)
    {
        var queueNode = new LinkedListNode<TKey>(key);
        _recentlySeenQueue.AddFirst(queueNode);
        _keyToQueueMap.Add(key, queueNode);
        _cache.Add(key, value);
    }

    private void DropOldestCachedItem()
    {
        var oldestCachedItem = _recentlySeenQueue.Last;
        var oldestCachedKey = oldestCachedItem!.Value;
        _recentlySeenQueue.Remove(oldestCachedItem);
        _keyToQueueMap.Remove(oldestCachedKey);
        _cache.Remove(oldestCachedKey);
    }
}
