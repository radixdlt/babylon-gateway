using Common.Database;
using Common.Utilities;
using GatewayAPI.Database;
using Microsoft.EntityFrameworkCore;
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
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiagnosticDbContextHealthCheck<TContext>> _logger;

    public DiagnosticDbContextHealthCheck(IConfiguration configuration, ILogger<DiagnosticDbContextHealthCheck<TContext>> logger)
    {
        _configuration = configuration;
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
        // We run it in a separate thread because; based on our tests, it appears that this operation might block
        var queryTask = Task.Run(TestQuery); // We purposefully run it without a cancellation token so that it can outlast the health check

        try
        {
            await queryTask.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs), token);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning(
                "Test query for {DbContextName} timed out after {TimeoutMs}ms - but I'll try to let it carry on anyway...",
                typeof(TContext).Name,
                timeoutMs
            );
        }
    }

    private async Task TestQuery()
    {
        // We create a DbContext here so it's managed separately and isn't disposed by the DI framework
        // This means we can run it after the HealthCheck gets disposed to still check on long-running thread issues
        await using var dbContext = CreateDbContext();
        try
        {
            var (canConnect, timeInMs) = await CodeStopwatch.TimeInMs(
                async () => await dbContext.Database.CanConnectAsync()
            );
            _logger.LogInformation(
                "CanConnectAsync for {DbContextName} returned {CanConnect} in {TimeInMs}ms",
                dbContext.GetType().Name,
                canConnect,
                timeInMs
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "CanConnectAsync for {DbContextName} raised an error",
                dbContext.GetType().Name
            );
        }
    }

    private CommonDbContext CreateDbContext()
    {
        if (typeof(TContext) == typeof(GatewayReadOnlyDbContext))
        {
            return new GatewayReadOnlyDbContext(
                new DbContextOptionsBuilder<GatewayReadOnlyDbContext>()
                .UseNpgsql(
                    _configuration.GetConnectionString("ReadOnlyDbContext"),
                    o => o.UseNodaTime()
                )
                .Options
            );
        }

        return new GatewayReadWriteDbContext(
            new DbContextOptionsBuilder<GatewayReadWriteDbContext>()
                .UseNpgsql(
                    _configuration.GetConnectionString("ReadWriteDbContext"),
                    o => o.UseNodaTime()
                )
                .Options
        );
    }
}
