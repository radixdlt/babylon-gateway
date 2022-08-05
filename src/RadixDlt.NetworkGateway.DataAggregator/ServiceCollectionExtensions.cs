using Common.CoreCommunications;
using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prometheus;
using RadixDlt.NetworkGateway.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.DataAggregator.Workers.GlobalWorkers;
using RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;
using System.Net;

namespace RadixDlt.NetworkGateway.DataAggregator;

public static class ServiceCollectionExtensions
{
    public static void AddNetworkGatewayDataAggregator(this IServiceCollection services, string connectionString)
    {
        services
            .AddValidatableOptionsAtSection<NetworkOptions, NetworkOptionsValidator>("DataAggregator")
            .AddValidatableOptionsAtSection<MonitoringOptions, MonitoringOptionsValidator>("DataAggregator:Monitoring")
            .AddValidatableOptionsAtSection<MempoolOptions, MempoolOptionsValidator>("DataAggregator:MempoolOptions")
            .AddValidatableOptionsAtSection<LedgerConfirmationOptions, LedgerConfirmationOptionsValidator>("DataAggregator:LedgerConfirmation")
            .AddValidatableOptionsAtSection<TransactionAssertionsOptions, TransactionAssertionsOptionsValidator>("DataAggregator:TransactionAssertions");

        // Globally-Scoped services
        AddGlobalScopedServices(services);
        AddGlobalHostedServices(services);
        AddDatabaseContext(services, connectionString);

        // Node-Scoped services
        AddNodeScopedServices(services);
        AddTransientApiReaders(services);
        AddNodeInitializers(services);
        AddNodeWorkers(services);
    }

    private static void AddGlobalScopedServices(IServiceCollection services)
    {
        services.AddSingleton<INodeWorkersRunnerRegistry, NodeWorkersRunnerRegistry>();
        services.AddSingleton<INodeWorkersRunnerFactory, NodeWorkersRunnerFactory>();
        services.AddSingleton<IRawTransactionWriter, RawTransactionWriter>();
        services.AddSingleton<ILedgerConfirmationService, LedgerConfirmationService>();
        services.AddSingleton<ILedgerExtenderService, LedgerExtenderService>();
        services.AddSingleton<INetworkConfigurationProvider, NetworkConfigurationProvider>();
        services.AddSingleton<INetworkAddressConfigProvider>(x => x.GetRequiredService<INetworkConfigurationProvider>());
        services.AddSingleton<IEntityDeterminer, EntityDeterminer>();
        services.AddSingleton<ISystemStatusService, SystemStatusService>();
        services.AddSingleton<IMempoolTrackerService, MempoolTrackerService>();
        services.AddSingleton<IMempoolResubmissionService, MempoolResubmissionService>();
        services.AddSingleton<IMempoolPrunerService, MempoolPrunerService>();
    }

    private static void AddGlobalHostedServices(IServiceCollection services)
    {
        services.AddHostedService<NodeConfigurationMonitorWorker>();
        services.AddHostedService<LedgerConfirmationWorker>();
        services.AddHostedService<MempoolTrackerWorker>();
        services.AddHostedService<MempoolResubmissionWorker>();
        services.AddHostedService<MempoolPrunerWorker>();
    }

    private static void AddDatabaseContext(IServiceCollection services, string connectionString)
    {
        // Useful links:
        // https://www.npgsql.org/efcore/index.html
        // https://www.npgsql.org/doc/connection-string-parameters.html
        services.AddDbContextFactory<AggregatorDbContext>(options =>
        {
            options.UseNpgsql(connectionString, o => o.NonBrokenUseNodaTime());
        });
    }

    private static void AddNodeScopedServices(IServiceCollection services)
    {
        services.AddScoped<INodeConfigProvider, NodeConfigProvider>();
    }

    private static void AddTransientApiReaders(IServiceCollection services)
    {
        // NB - AddHttpClient is essentially like AddTransient, except it provides a HttpClient from the HttpClientFactory
        // See https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services.AddHttpClient<ICoreApiProvider, CoreApiProvider>()
            .UseHttpClientMetrics()
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => ConfigureHttpClientHandler(serviceProvider.GetRequiredService<IOptions<NetworkOptions>>()));

        // We can mock these out in tests
        // These should be transient so that they don't capture a transient HttpClient
        services.AddTransient<ITransactionLogReader, TransactionLogReader>();
        services.AddTransient<INetworkConfigurationReader, NetworkConfigurationReader>();
        services.AddTransient<INetworkStatusReader, NetworkStatusReader>();
    }

    private static void AddNodeInitializers(IServiceCollection services)
    {
        // Add node initializers - these will be instantiated by the NodeWorkersRunner.cs and run before the workers start
        services.AddScoped<INodeInitializer, NodeNetworkConfigurationInitializer>();
    }

    private static void AddNodeWorkers(IServiceCollection services)
    {
        // Add node workers - these will be instantiated by the NodeWorkersRunner.cs.
        services.AddScoped<INodeWorker, NodeTransactionLogWorker>();
        services.AddScoped<INodeWorker, NodeMempoolTransactionIdsReaderWorker>();
        services.AddScoped<INodeWorker, NodeMempoolFullTransactionReaderWorker>();
    }

    private static HttpClientHandler ConfigureHttpClientHandler(IOptions<NetworkOptions> options)
    {
        var o = options.Value;
        var httpClientHandler = new HttpClientHandler();

        if (o.DisableCoreApiHttpsCertificateChecks)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        if (!string.IsNullOrWhiteSpace(o.CoreApiHttpProxyAddress))
        {
            httpClientHandler.Proxy = new WebProxy(o.CoreApiHttpProxyAddress);
        }

        return httpClientHandler;
    }
}
