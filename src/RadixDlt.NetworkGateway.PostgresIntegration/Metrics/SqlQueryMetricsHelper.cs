using System;
using System.IO;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Metrics;

public static class SqlQueryMetricsHelper
{
    public const string QueryNameTag = "QueryName";

    public static string GetQueryNameValue(
        string operationName,
        string methodName)
    {
        if (!operationName.All(char.IsLetterOrDigit))
        {
            throw new ArgumentException($"{nameof(operationName)} is expected to be alphanumeric. Got: {operationName}");
        }

        if (!methodName.All(char.IsLetterOrDigit))
        {
            throw new ArgumentException($"{nameof(methodName)} is expected to be alphanumeric. Got: {operationName}");
        }

        if (!string.IsNullOrEmpty(operationName) &&
            !string.Equals(operationName, methodName, StringComparison.InvariantCultureIgnoreCase))
        {
            return $"{methodName}_{operationName}";
        }
        else
        {
            return $"{methodName}";
        }
    }

    public static string GenerateQueryNameTag(string operationName, string methodName)
    {
        var queryName = GetQueryNameValue(operationName, methodName);
        return $"{QueryNameTag}<{queryName}>;";
    }
}
