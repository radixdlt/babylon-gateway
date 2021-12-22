using Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GatewayAPI.Monitoring;

/// <summary>
/// We were having issues with the standard DbContextHealthCheck - because it was timing out and failing even if
/// database queries were fine - it was also seeming to synchronously block the thread. So this health check uses a
/// direct query instead of CanConnectAsync which uses a new connection for each check.
/// </summary>
/// <typeparam name="TContext">The database context.</typeparam>
public class GatewayDbContextHealthCheck<TContext> : IHealthCheck
    where TContext : CommonDbContext
{
    private readonly TContext _dbContext;
    private readonly ILogger<GatewayDbContextHealthCheck<TContext>> _logger;

    public GatewayDbContextHealthCheck(TContext dbContext, ILogger<GatewayDbContextHealthCheck<TContext>> logger)
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

        const int TimeoutMs = 500;

        return await TestQueryWithTimeout(cancellationToken, TimeoutMs)
            ? HealthCheckResult.Healthy()
            : new HealthCheckResult(context.Registration.FailureStatus);
    }

    private async Task<bool> TestQueryWithTimeout(CancellationToken token, int timeoutMs)
    {
        using var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        var queryTask = TestQuery(timeoutTokenSource.Token);

        try
        {
            return await queryTask.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs), token);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning(
                "Test query for {DbContextName} timed out after {TimeoutMs}ms",
                typeof(TContext).Name,
                timeoutMs
            );
            timeoutTokenSource.Cancel();
            return false;
        }
    }

    private async Task<bool> TestQuery(CancellationToken token)
    {
        try
        {
            await _dbContext.LedgerStatus.SingleOrDefaultAsync(token);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test query for {DbContextName} raised an exception", typeof(TContext).Name);
            return false;
        }
    }
}
