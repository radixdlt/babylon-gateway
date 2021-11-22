using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Common.Database;

public static class ConnectionHelpers
{
    public static async Task PerformScopedDbAction<TContext>(IServiceProvider services, Func<ILogger, DbContext, Task> dbAction)
        where TContext : DbContext
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await dbAction(logger, dbContext);
    }

    public static async Task TryWaitForDb<TContext>(IServiceProvider services, int maxWaitForDbMs)
        where TContext : DbContext
    {
        await PerformScopedDbAction<TContext>(services, async (logger, dbContext) =>
        {
            await TryWaitForDbInternal(logger, dbContext, maxWaitForDbMs);
        });
    }

    private static async Task TryWaitForDbInternal(ILogger logger, DbContext dbContext, int maxWaitForDbMs)
    {
        if (maxWaitForDbMs <= 0)
        {
            return;
        }

        if (await dbContext.Database.CanConnectAsync())
        {
            logger.LogInformation("Database appears to be accepting connections");
            return;
        }

        logger.LogInformation("Database does not appear to be accepting connections. Waiting up to {MaxWaitForDbMs}ms to connect... ", maxWaitForDbMs);

        var timer = new Stopwatch();
        timer.Start();
        while (timer.ElapsedMilliseconds <= maxWaitForDbMs)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            if (await dbContext.Database.CanConnectAsync())
            {
                logger.LogInformation("Connected to database after {WaitForDbMs}ms", timer.ElapsedMilliseconds);
                return;
            }
        }

        logger.LogWarning("Could not connect to database after waiting {MaxWaitForDbMs}ms", timer.ElapsedMilliseconds);

        // Try to throw a useful exception explaining that it can't connect
        await dbContext.Database.OpenConnectionAsync();
    }
}
