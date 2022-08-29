using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

public static class HostExtensions
{
    public static async Task ExecutePostgresMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MigrationsDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<MigrationsDbContext>();

        logger.LogInformation("Starting database migrations if required");

        await dbContext.Database.MigrateAsync();

        logger.LogInformation("Database migrations (if required) were completed");
    }
}
