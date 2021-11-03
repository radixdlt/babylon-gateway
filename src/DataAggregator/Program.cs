using DataAggregator.Configuration;
using DataAggregator.Services;
using DataAggregator.Workers;
using DataAggregator.Workers.Factory;
using Microsoft.EntityFrameworkCore;
using Shared.Database;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<AggregatorConfiguration>();
        services.AddSingleton<NodeWorkerRunnerService>();
        services.AddSingleton<NodeWorkerFactory>();
        services.AddHostedService<RootWorker>();

        #pragma warning disable SA1515 - Remove need to proceed comments by free line as it looks weird here
        services.AddDbContext<SharedDbContext>(options =>
            options
                // https://www.npgsql.org/efcore/index.html
                .UseNpgsql(
                    hostContext.Configuration.GetConnectionString("SharedDbContext"),
                    b => b.MigrationsAssembly("DataAggregator")
                )
                // Whilst we should be explicit about table names, in case we forget any, we use this tool recommended by npgsql
                // https://www.npgsql.org/efcore/modeling/table-column-naming.html
                .UseSnakeCaseNamingConvention()
        );
    })
    .Build();

await host.RunAsync();
