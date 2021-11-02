using DataAggregator.Configuration;
using DataAggregator.Services;
using DataAggregator.Workers;
using DataAggregator.Workers.Factory;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<AggregatorConfiguration>();
        services.AddSingleton<NodeWorkerService>();
        services.AddSingleton<NodeWorkerFactory>();
        services.AddHostedService<RootWorker>();
    })
    .Build();

await host.RunAsync();
