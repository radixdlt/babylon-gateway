using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Metrics;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Interceptors;

internal class MetricsInterceptor : DbCommandInterceptor
{
    private const string UnknownQueryName = "UNKNOWN";
    private readonly IOptionsMonitor<SlowQueriesLoggingOptions> _slowQueriesLoggingOptions;
    private readonly ILogger<MetricsInterceptor> _logger;
    private readonly ISqlQueryObserver _sqlQueryObserver;

    public MetricsInterceptor(ILoggerFactory loggerFactory, IOptionsMonitor<SlowQueriesLoggingOptions> slowQueriesLoggingOptions, ISqlQueryObserver sqlQueryObserver)
    {
        _slowQueriesLoggingOptions = slowQueriesLoggingOptions;
        _sqlQueryObserver = sqlQueryObserver;
        _logger = loggerFactory.CreateLogger<MetricsInterceptor>();
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
        var queryName = GetQueryName(command) ?? UnknownQueryName;

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

    private string? GetQueryName(DbCommand dbCommand)
    {
        const string QueryNameGroup = "queryName";
        var matches = Regex.Matches(dbCommand.CommandText, $"(?:{SqlQueryMetricsHelper.QueryNameTag}=)(?<{QueryNameGroup}>\\b.*?\\b);");

        switch (matches.Count)
        {
            case 0:
                _logger.LogDebug("Missing query name for: {commandText}", dbCommand.CommandText);
                return null;
            case 1:
            {
                var hasQueryName = matches.First().Groups.TryGetValue(QueryNameGroup, out var queryName);

                if (!hasQueryName || string.IsNullOrEmpty(queryName?.Value))
                {
                    _logger.LogDebug("Missing query name for: {commandText}", dbCommand.CommandText);
                }

                return queryName!.Value;
            }

            case > 1:
            {
                var foundTags = string.Join(',', matches.Select(x => x.Groups.Values.ToString()));
                _logger.LogDebug("Query name provided multiple times: {foundTags}, in query: {commandText}", foundTags, dbCommand.CommandText);
                return null;
            }

            default: return null;
        }
    }
}
