using System;
using System.IO;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Metrics;

public static class SqlQueryMetricsHelper
{
    public const string QueryNameTag = "gcmKFRVsi3IDgKuTQT2z";

    public static string GetQueryNameValue(
        string operationName,
        string methodName)
    {
        if (!IsValidTagPart(operationName))
        {
            throw new ArgumentException("Expected alphanumeric, got: {operationName}", nameof(operationName));
        }

        if (!IsValidTagPart(methodName))
        {
            throw new ArgumentException("Expected alphanumeric, got: {operationName}", nameof(methodName));
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

    private static bool IsValidTagPart(string value)
    {
        return value.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '_');
    }
}
