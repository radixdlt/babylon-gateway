using Common.Database;
using DataAggregator.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(new DefaultKernel().ConfigureServices)
    .ConfigureLogging(builder =>
    {
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "hh:mm:ss ";
        });
    })
    .Build();

// After changing migrations or wanting to wipe the database, change this to "true"
static bool ShouldWipeDatabaseInsteadOfStart() => false;

if (ShouldWipeDatabaseInsteadOfStart())
{
    // TODO - Change this to work safely in production!
    // TODO - Tweak logs so that any migration based logs still appear, but that
    // https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli
    using var scope = host.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<CommonDbContext>>();
    var db = scope.ServiceProvider.GetRequiredService<CommonDbContext>();

    logger.LogInformation("Starting db wipe");

    // Uncomment to wipe Database every load!
    await db.Database.EnsureDeletedAsync();

    logger.LogInformation("DB wipe completed");

    // Migrate every load
    // await db.Database.MigrateAsync();

    // Purposefully do not allow running after wipe - have to change the above to true
}
else
{
    using (var scope = host.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CommonDbContext>>();
        var db = scope.ServiceProvider.GetRequiredService<CommonDbContext>();

        logger.LogInformation("Starting db migrations if required");

        await db.Database.MigrateAsync();

        logger.LogInformation("DB migrations performed");
    }

    await host.RunAsync();
}
