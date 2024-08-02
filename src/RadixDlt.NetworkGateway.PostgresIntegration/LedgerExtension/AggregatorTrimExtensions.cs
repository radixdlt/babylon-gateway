using Microsoft.Extensions.Logging;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

public static class AggregatorTrimExtensions
{
    private const int DefaultTrim = 50_000;

    public static ILogger? Logger { get; set; }

    public static T[]? DaTrim<T>(this T[]? array, int trimLength = DefaultTrim, [CallerMemberName] string callerMemberName = "", [CallerArgumentExpression(nameof(array))] string callerArgumentExpression = "")
    {
        if (array == null)
        {
            return null;
        }

        if (array.Length > trimLength)
        {
            Logger?.LogWarning("Trimming an array of length {Length} down to {TrimLength}: {CallerArgumentExpression} of {CallerMemberName}", array.Length, trimLength, callerArgumentExpression, callerMemberName);

            return array.Take(trimLength).ToArray();
        }

        return array;
    }
}
