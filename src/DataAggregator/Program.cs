using DataAggregator.DependencyInjection;

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

await host.RunAsync();
