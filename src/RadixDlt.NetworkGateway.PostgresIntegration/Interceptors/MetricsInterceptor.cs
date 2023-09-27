using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Metrics;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Interceptors;

internal class MetricsInterceptor : DbCommandInterceptor
{
    private readonly IOptionsMonitor<SlowQueryLoggingOptions> _slowQueriesLoggingOptions;
    private readonly ILogger<MetricsInterceptor> _logger;
    private readonly ISqlQueryObserver _sqlQueryObserver;

    public MetricsInterceptor(ILogger<MetricsInterceptor> logger, IOptionsMonitor<SlowQueryLoggingOptions> slowQueriesLoggingOptions, ISqlQueryObserver sqlQueryObserver)
    {
        _slowQueriesLoggingOptions = slowQueriesLoggingOptions;
        _sqlQueryObserver = sqlQueryObserver;
        _logger = logger;
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        Observe(command, eventData);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        Observe(command, eventData);
        return new ValueTask<DbDataReader>(result);
    }

    private void Observe(
        DbCommand command,
        CommandExecutedEventData eventData)
    {
        var queryName = GetQueryName(command);

        _sqlQueryObserver.OnSqlQueryExecuted(queryName, eventData.Duration);

        var logQueriesLongerThan = _slowQueriesLoggingOptions.CurrentValue.SlowQueriesThreshold;
        if (eventData.Duration > logQueriesLongerThan)
        {
#pragma warning disable EF1001
            var parameters = command.Parameters.FormatParameters(true);
#pragma warning restore EF1001

            _logger.LogWarning(
                "Long running query: {query}, parameters: {queryParameters} duration: {queryDuration} milliseconds",
                command.CommandText, parameters, eventData.Duration.TotalMilliseconds);
        }
    }

    private string GetQueryName(DbCommand dbCommand)
    {
        const string UnknownQueryName = "UNKNOWN";

        var startOfTag = dbCommand.CommandText.IndexOf($"{SqlQueryMetricsHelper.QueryNameTag}<", StringComparison.InvariantCultureIgnoreCase);
        var endOfTag = dbCommand.CommandText.IndexOf(">", StringComparison.InvariantCultureIgnoreCase);

        if (startOfTag < 0 || endOfTag < 0)
        {
            _logger.LogDebug("Missing query name for: {commandText}", dbCommand.CommandText);
            return UnknownQueryName;
        }

        var queryName = dbCommand.CommandText.Substring(startOfTag, endOfTag);

        if (string.IsNullOrEmpty(queryName))
        {
            throw new ArgumentNullException(queryName, "Query name extracted from query tag is empty.");
        }

        return queryName;
    }
}
