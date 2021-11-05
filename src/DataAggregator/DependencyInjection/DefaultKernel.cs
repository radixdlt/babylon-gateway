using Common.Database;
using DataAggregator.Configuration;
using DataAggregator.GlobalServices;
using DataAggregator.GlobalWorkers;
using DataAggregator.NodeScopedServices;
using DataAggregator.NodeScopedWorkers;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.DependencyInjection;

public static class DefaultKernel
{
    public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        AddGlobalScopedServices(services);
        AddDatabaseContext(hostBuilderContext, services);
        AddNodeScopedServices(services);
    }

    private static void AddGlobalScopedServices(IServiceCollection services)
    {
        services.AddSingleton<IAggregatorConfiguration, AggregatorConfiguration>();
        services.AddSingleton<INodeWorkersRunnerRegistry, NodeWorkersRunnerRegistry>();
        services.AddSingleton<INodeWorkersRunnerFactory, NodeWorkersRunnerFactory>();
        services.AddHostedService<NodeConfigurationMonitorWorker>();
    }

    private static void AddDatabaseContext(HostBuilderContext hostContext, IServiceCollection services)
    {
        #pragma warning disable SA1515 // Remove need to proceed comments by free line as it looks weird here
        services.AddDbContext<CommonDbContext>(options =>
            options
                // https://www.npgsql.org/efcore/index.html
                .UseNpgsql(
                    hostContext.Configuration.GetConnectionString("CommonDbContext"),
                    b => b.MigrationsAssembly("DataAggregator")
                )
                // Whilst we should be explicit about table names, in case we forget any, we use this tool recommended by npgsql
                // https://www.npgsql.org/efcore/modeling/table-column-naming.html
                .UseSnakeCaseNamingConvention()
        );
        #pragma warning restore SA1515
    }

    private static void AddNodeScopedServices(IServiceCollection services)
    {
        services.AddScoped<INodeConfigProvider, NodeConfigProvider>();
        services.AddScoped<ITransactionLogReader, TransactionLogReader>();

        // Add node workers - these will be instantiated by the NodeWorkersRunner.cs.
        services.AddScoped<INodeWorker, NodeTransactionLogWorker>();
    }
}
