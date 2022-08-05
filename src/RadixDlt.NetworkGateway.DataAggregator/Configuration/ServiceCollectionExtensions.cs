using Common.CoreCommunications;
using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using RadixDlt.NetworkGateway.DataAggregator.GlobalServices;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.DataAggregator.Workers.GlobalWorkers;
using RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;
using System.Net;

namespace RadixDlt.NetworkGateway.DataAggregator.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddNetworkGatewayDataAggregator(this IServiceCollection services)
    {
        services
            .AddOptions<NetworkGatewayDataAggregatorOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("DataAggregator"); // TODO is this how we want to Bind configuration by default?

        // Globally-Scoped services
        AddGlobalScopedServices(services);
        AddGlobalHostedServices(services);
        AddDatabaseContext(services);

        // Node-Scoped services
        AddNodeScopedServices(services);
        AddTransientApiReaders(services);
        AddNodeInitializers(services);
        AddNodeWorkers(services);
    }

    private static void AddGlobalScopedServices(IServiceCollection services)
    {
        services.AddSingleton<IAggregatorConfiguration, AggregatorConfiguration>();
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

    private static void AddDatabaseContext(IServiceCollection services)
    {
        // Useful links:
        // https://www.npgsql.org/efcore/index.html
        // https://www.npgsql.org/doc/connection-string-parameters.html

/*
        // First - Migration Context
        var migrationsDbConnectionString = hostContext.Configuration.GetConnectionString("MigrationsDbContext");
        var aggregatorDbConnectionString = hostContext.Configuration.GetConnectionString("AggregatorDbContext");

        if (migrationsDbConnectionString != null)
        {
            services.AddDbContextFactory<MigrationsDbContext>(options =>
            {
                options.UseNpgsql(
                    migrationsDbConnectionString,
                    o => o.NonBrokenUseNodaTime()
                );
            });
        }
        else
        {
            // If no MigrationsDbContext was provided, we use the default AggregatorDbContext, but
            //   overrides the default 30 second CommandTimeout on migrations (which causes long migrations to rollback)
            //   We override the timeout with 15 minutes.
            // Note that it is still not advised to use the migrate-on-startup strategy in production.
            //   We recommend following the guidance here:
            //   https://docs.radixdlt.com/main/node-and-gateway/network-gateway-releasing.html
            // If a Gateway runner wishes to add that back, they can explicitly configure their own MigrationsDbContext.
            services.AddDbContextFactory<MigrationsDbContext>(options =>
            {
                options.UseNpgsql(
                    aggregatorDbConnectionString,
                    o => o.NonBrokenUseNodaTime()
                        .CommandTimeout(900) // 15 minutes
                );
            });
        }
*/
        services.AddDbContextFactory<AggregatorDbContext>(options =>
        {
            options.UseNpgsql(
                "Host=localhost:5532;Database=babylon_stokenet;Username=db_superuser;Password=db_password;Include Error Detail=true", // hostContext.Configuration.GetConnectionString("AggregatorDbContext"),
                o => o.NonBrokenUseNodaTime()
            );
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
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => ConfigureHttpClientHandler(
                serviceProvider,
                "DisableCoreApiHttpsCertificateChecks",
                "CoreApiHttpProxyAddress"
            ));

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

    private static HttpClientHandler ConfigureHttpClientHandler(
        IServiceProvider serviceProvider,
        string disableApiChecksConfigParameterName,
        string httpProxyAddressConfigParameterName
    )
    {
        var httpClientHandler = new HttpClientHandler();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var disableCertificateChecks = configuration.GetValue<bool>(disableApiChecksConfigParameterName);
        var httpProxyAddress = configuration.GetValue<string?>(httpProxyAddressConfigParameterName);

        if (disableCertificateChecks)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        if (!string.IsNullOrWhiteSpace(httpProxyAddress))
        {
            httpClientHandler.Proxy = new WebProxy(httpProxyAddress);
        }

        return httpClientHandler;
    }
}
