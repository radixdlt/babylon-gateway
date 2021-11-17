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

// Easy switch for development - after changing migrations or wanting to wipe the database, change this to "true"
static bool ShouldWipeDatabaseInsteadOfStart() => false;

if (ShouldWipeDatabaseInsteadOfStart())
{
    // TODO:NG-14 - Change to manage migrations more safely outside service boot-up
    // TODO:NG-38 - Tweak logs so that any migration based logs still appear, but that general Microsoft.EntityFrameworkCore.Database.Command logs do not
    // https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli
    using var scope = host.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<CommonDbContext>>();
    var db = scope.ServiceProvider.GetRequiredService<CommonDbContext>();

    logger.LogInformation("Starting db wipe");

    await db.Database.EnsureDeletedAsync();

    logger.LogInformation("DB wipe completed");

    // Purposefully do not allow running after wipe - have to change the above to true once
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
