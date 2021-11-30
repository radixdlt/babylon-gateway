namespace Common.Extensions;

public static class EnumerableExtensions
{
    public static TItem GetRandomBy<TItem>(this IEnumerable<TItem> items, Func<TItem, double> weightingSelector)
    {
        var allItems = items.ToList();
        if (allItems.Count == 0)
        {
            throw new ArgumentException("enumerable cannot be empty", nameof(items));
        }

        var totalWeighting = allItems.Sum(weightingSelector);
        var randWeighting = Random.Shared.NextDouble() * totalWeighting;
        double trackedWeighting = 0;
        foreach (var item in allItems)
        {
            trackedWeighting += weightingSelector(item);
            if (trackedWeighting >= randWeighting)
            {
                return item;
            }
        }

        // Shouldn't happen - but let's do something sensible anyway
        return allItems[0];
    }
}
