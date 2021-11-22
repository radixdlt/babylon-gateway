using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Common.Extensions;

public static class DbSetExtensions
{
    public static IQueryable<TEntity> FromSqlRawWithDimensionalIn<TEntity, TKey>(
        this DbSet<TEntity> dbSet,
        string sqlBeforeIn,
        IReadOnlyCollection<TKey> keys,
        Func<TKey, IEnumerable<object>> mapToKey
    )
        where TEntity : class
    {
        if (!keys.Any())
        {
            throw new ArgumentException("Needs to have at least some keys", nameof(keys));
        }

        var tupleDimension = mapToKey(keys.First()).Count();

        var placeholders = CreateArrayOfTuplesPlaceholder(keys.Count, tupleDimension);

        object[] values = keys.SelectMany(mapToKey).ToArray()!;

        return dbSet.FromSqlRaw($"{sqlBeforeIn} IN ({placeholders})", values);
    }

    /// <summary>
    /// Outputs a string like (({0},{1},{2}),({3},{4},{5})) for arrayLength=2, tupleLength=3.
    /// </summary>
    private static string CreateArrayOfTuplesPlaceholder(int arrayLength, int tupleLength)
    {
        var placeholders = new StringBuilder();
        for (int i = 0; i < arrayLength; i++)
        {
            if (i > 0)
            {
                placeholders.Append(',');
            }

            placeholders.Append('(');

            for (int j = 0; j < tupleLength; j++)
            {
                if (j > 0)
                {
                    placeholders.Append(',');
                }

                placeholders.Append('{');
                placeholders.Append((2 * i) + j);
                placeholders.Append('}');
            }

            placeholders.Append(')');
        }

        return placeholders.ToString();
    }
}
