using Prometheus;
using RadixDlt.NetworkGateway.PostgresIntegration.Interceptors;
using System;
using System.IO;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Metrics;

internal class SqlQueryMetricsHelper
{
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
}
