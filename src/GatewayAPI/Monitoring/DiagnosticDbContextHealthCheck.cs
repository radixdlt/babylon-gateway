using Common.Database;
using Common.Utilities;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GatewayAPI.Monitoring;

/// <summary>
/// We were having issues with the standard DbContextHealthCheck - because it was timing out and failing even if
/// database queries were fine - it was also seeming to synchronously block the thread.
///
/// This health check is a (temporary) diagnostic to help us dig into what's going on and see if CanConnectAsync does
/// get linearly longer with time.
/// </summary>
/// <typeparam name="TContext">The database context.</typeparam>
public class DiagnosticDbContextHealthCheck<TContext> : IHealthCheck
    where TContext : CommonDbContext
{
    private readonly TContext _dbContext;
    private readonly ILogger<DiagnosticDbContextHealthCheck<TContext>> _logger;

    public DiagnosticDbContextHealthCheck(TContext dbContext, ILogger<DiagnosticDbContextHealthCheck<TContext>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        const int TimeoutMs = 900;

        await RunTestQueryOnSeparateThread(cancellationToken, TimeoutMs);

        return HealthCheckResult.Healthy();
    }

    private async Task RunTestQueryOnSeparateThread(CancellationToken token, int timeoutMs)
    {
        var queryTask = Task.Run(TestQuery); // Without a cancellation token

        try
        {
            await queryTask.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs), token);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning(
                "Test query for {DbContextName} timed out after {TimeoutMs}ms - but I'll try to let it carry on anyway (it might hit a Dispose exception though, because DI will dispose of the DbContext)...",
                typeof(TContext).Name,
                timeoutMs
            );
        }
    }

    private async Task TestQuery()
    {
        try
        {
            var (canConnect, timeInMs) = await CodeStopwatch.TimeInMs(
                async () => await _dbContext.Database.CanConnectAsync()
            );
            _logger.LogInformation(
                "CanConnectAsync for {DbContextName} returned {CanConnect} in {TimeInMs}ms",
                typeof(TContext).Name,
                canConnect,
                timeInMs
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "CanConnectAsync for {DbContextName} raised an error",
                typeof(TContext).Name
            );
        }
    }
}
