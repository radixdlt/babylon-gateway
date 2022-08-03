namespace GatewayApi;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();

        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddEnvironmentVariables("RADIX_NG_API__");
                config.AddEnvironmentVariables("RADIX_NG_API:"); // Remove this line once https://github.com/dotnet/runtime/issues/61577#issuecomment-1044959384 is fixed
                if (args is { Length: > 0 })
                {
                    config.AddCommandLine(args);
                }

                if (context.HostingEnvironment.IsDevelopment())
                {
                    // As an easier alternative to developer secrets -- this file is in .gitignore to prevent source controlling
                    config.AddJsonFile("appsettings.DevelopmentOverrides.json", optional: true, reloadOnChange: true);
                }
                else
                {
                    config.AddJsonFile("appsettings.ProductionOverrides.json", optional: true, reloadOnChange: true);
                }

                var customConfigurationPath = config.Build()
                    .GetValue<string?>("CustomJsonConfigurationFilePath", null);

                if (customConfigurationPath != null)
                {
                    config.AddJsonFile(customConfigurationPath, false, true);
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .ConfigureKestrel(o =>
                    {
                        o.AddServerHeader = false;
                    })
                    .UseStartup<GatewayApiStartup>();
            });
}
