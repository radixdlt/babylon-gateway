using System;
using System.IO;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Metrics;

public static class SqlQueryMetricsHelper
{
    public const string QueryNameTag = "QueryName";

    public static string GetQueryNameValue(
        string operationName = "",
        string filePath = "",
        string methodName = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        if (!string.IsNullOrEmpty(operationName) &&
            !string.Equals(operationName, methodName, StringComparison.InvariantCultureIgnoreCase))
        {
            return $"{fileName}_{methodName}_{operationName}";
        }
        else
        {
            return $"{fileName}_{methodName}";
        }
    }

    public static string GenerateQueryNameTag(string operationName = "", string filePath = "", string methodName = "")
    {
        var queryName = GetQueryNameValue(operationName, filePath, methodName);
        return $"{QueryNameTag}={queryName};";
    }
}
