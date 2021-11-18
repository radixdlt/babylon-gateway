using Common.Database;
using DataAggregator.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        IHostEnvironment env = hostingContext.HostingEnvironment;

        config.AddEnvironmentVariables("RADIX_NETWORK_GATEWAY__");
        if (args is { Length: > 0 })
        {
            config.AddCommandLine(args);
        }
    })
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

var isDevelopment = host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment();

var configuration = host.Services.GetRequiredService<IConfiguration>();

// In production, provide both RADIX_NETWORK_GATEWAY__WIPE_DATABASE and RADIX_NETWORK_GATEWAY__WIPE_DATABASE_CONFIRM to wipe the ledger
var shouldWipeDatabaseInsteadOfStart =
    configuration.GetValue<bool>("WIPE_DATABASE")
    && (isDevelopment || configuration.GetValue<bool>("WIPE_DATABASE_CONFIRM"));

if (shouldWipeDatabaseInsteadOfStart)
{
    using var scope = host.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<CommonDbContext>>();
    var db = scope.ServiceProvider.GetRequiredService<CommonDbContext>();

    logger.LogInformation("Starting db wipe");

    await db.Database.EnsureDeletedAsync();

    logger.LogInformation("DB wipe completed. Now stopping...");

    // Purposefully do not allow running after wipe - have to change the above to true once
}
else
{
    // TODO:NG-14 - Change to manage migrations more safely outside service boot-up
    // TODO:NG-38 - Tweak logs so that any migration based logs still appear, but that general Microsoft.EntityFrameworkCore.Database.Command logs do not
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
