using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.PostgresIntegration.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

public static class QueryableExtensions
{
    public static IQueryable<T> AnnotateMetricName<T>(
        this IQueryable<T> source,
        string operationName = "",
        [CallerMemberName] string methodName = "")
    {
        var queryNameTag = SqlQueryMetricsHelper.GenerateQueryNameTag(operationName, methodName);
        return source.TagWith(queryNameTag);
    }
}
