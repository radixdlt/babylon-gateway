using Common.Database;
using DataAggregator.DependencyInjection;

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

// TODO - Change this
// https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommonDbContext>();

    // Uncomment to wipe Database every load!
    // await db.Database.EnsureDeletedAsync();
    // await db.Database.EnsureCreatedAsync();

    // Migrate every load
    // await db.Database.MigrateAsync();
}

await host.RunAsync();
