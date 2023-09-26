using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.PostgresIntegration.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

public static class QueryableExtensions
{
    public static IQueryable<T> WithQueryName<T>(
        this IQueryable<T> source,
        string operationName = "",
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "")
    {
        var queryNameTag = SqlQueryMetricsHelper.GenerateQueryNameTag(operationName, filePath, methodName);
        return source.TagWith(queryNameTag);
    }
}
