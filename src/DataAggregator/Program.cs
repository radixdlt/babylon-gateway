using Common.Database;
using DataAggregator.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(DefaultKernel.ConfigureServices)
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
    await db.Database.MigrateAsync();
}

await host.RunAsync();
