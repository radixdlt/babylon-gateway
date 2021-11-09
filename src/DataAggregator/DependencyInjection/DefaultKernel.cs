using Common.Database;
using DataAggregator.Configuration;
using DataAggregator.GlobalServices;
using DataAggregator.GlobalWorkers;
using DataAggregator.NodeScopedServices;
using DataAggregator.NodeScopedServices.ApiReaders;
using DataAggregator.NodeScopedWorkers;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.DependencyInjection;

public class DefaultKernel
{
    public void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        AddGlobalScopedServices(services);
        AddGlobalHostedServices(services);
        AddDatabaseContext(hostBuilderContext, services);
        AddNodeScopedServices(services);
        AddNodeInitializers(services);
        AddNodeWorkers(services);
    }

    private void AddGlobalScopedServices(IServiceCollection services)
    {
        services.AddSingleton<IAggregatorConfiguration, AggregatorConfiguration>();
        services.AddSingleton<INodeWorkersRunnerRegistry, NodeWorkersRunnerRegistry>();
        services.AddSingleton<INodeWorkersRunnerFactory, NodeWorkersRunnerFactory>();
        services.AddSingleton<IRawTransactionWriter, RawTransactionWriter>();
        services.AddSingleton<ITransactionCommitter, TransactionCommitter>();
        services.AddSingleton<INetworkDetailsProvider, NetworkDetailsProvider>();
        services.AddSingleton<IAddressExtractor, AddressExtractor>();
    }

    private void AddGlobalHostedServices(IServiceCollection services)
    {
        services.AddHostedService<NodeConfigurationMonitorWorker>();
    }

    private void AddDatabaseContext(HostBuilderContext hostContext, IServiceCollection services)
    {
        #pragma warning disable SA1515 // Remove need to proceed comments by free line as it looks weird here
        services.AddDbContextFactory<CommonDbContext>(options =>
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

    private void AddNodeScopedServices(IServiceCollection services)
    {
        services.AddScoped<INodeConfigProvider, NodeConfigProvider>();
        services.AddScoped<INodeCoreApiProvider, NodeCoreApiProvider>();
        services.AddScoped<ITransactionLogReader, TransactionLogReader>();
    }

    private void AddNodeInitializers(IServiceCollection services)
    {
        // Add node initializers - these will be instantiated by the NodeWorkersRunner.cs and run before the workers start
        services.AddScoped<INodeInitializer, NodeNetworkConfigurationInitializer>();
    }

    private void AddNodeWorkers(IServiceCollection services)
    {
        // Add node workers - these will be instantiated by the NodeWorkersRunner.cs.
        services.AddScoped<INodeWorker, NodeTransactionLogWorker>();
    }
}
