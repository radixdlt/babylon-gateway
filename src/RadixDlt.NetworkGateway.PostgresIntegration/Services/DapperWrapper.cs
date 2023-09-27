using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Metrics;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

public interface IDapperWrapper
{
    Task<IEnumerable<T>> QueryAsync<T>(
        IDbConnection connection,
        CommandDefinition command,
        string operationName = "",
        [CallerMemberName] string methodName = "");

    Task<T> QueryFirstOrDefaultAsync<T>(
        IDbConnection connection,
        CommandDefinition command,
        string operationName = "",
        [CallerMemberName] string methodName = "");
}

public class DapperWrapper : IDapperWrapper
{
    private readonly IOptionsMonitor<SlowQueryLoggingOptions> _slowQueriesLoggingOptions;
    private readonly ILogger<DapperWrapper> _logger;
    private readonly ISqlQueryObserver _sqlQueryObserver;

    public DapperWrapper(
        IOptionsMonitor<SlowQueryLoggingOptions> slowQueriesLoggingOptions,
        ILogger<DapperWrapper> logger,
        ISqlQueryObserver sqlQueryObserver)
    {
        _slowQueriesLoggingOptions = slowQueriesLoggingOptions;
        _logger = logger;
        _sqlQueryObserver = sqlQueryObserver;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(
        IDbConnection connection,
        CommandDefinition command,
        string operationName = "",
        [CallerMemberName] string methodName = "")
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await connection.QueryAsync<T>(command);

        var elapsed = stopwatch.Elapsed;
        var queryName = SqlQueryMetricsHelper.GetQueryNameValue(operationName, methodName);

        _sqlQueryObserver.OnSqlQueryExecuted(queryName, elapsed);

        var logQueriesLongerThan = _slowQueriesLoggingOptions.CurrentValue.SlowQueryThreshold;
        if (elapsed > logQueriesLongerThan)
        {
            var parameters = JsonConvert.SerializeObject(command.Parameters);
            _logger.LogWarning(
                "Long running query: {query}, parameters: {queryParameters} duration: {queryDuration} seconds",
                command.CommandText, parameters, elapsed);
        }

        return result;
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(
        IDbConnection connection,
        CommandDefinition command,
        string operationName = "",
        [CallerMemberName] string methodName = "")
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await connection.QueryFirstOrDefaultAsync<T>(command);

        var elapsed = stopwatch.Elapsed;
        var queryName = SqlQueryMetricsHelper.GetQueryNameValue(operationName, methodName);
        _sqlQueryObserver.OnSqlQueryExecuted(queryName, elapsed);

        var logQueriesLongerThan = _slowQueriesLoggingOptions.CurrentValue.SlowQueryThreshold;
        if (elapsed > logQueriesLongerThan)
        {
            var parameters = JsonConvert.SerializeObject(command.Parameters);
            _logger.LogWarning(
                "Long running query: {query}, parameters: {queryParameters} duration: {queryDuration} milliseconds",
                command.CommandText, parameters, elapsed);
        }

        return result;
    }
}
