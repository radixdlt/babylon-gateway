using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prometheus;
using RadixDlt.NetworkGateway.Configuration;
using RadixDlt.NetworkGateway.CoreCommunications;
using RadixDlt.NetworkGateway.Extensions;
using RadixDlt.NetworkGateway.Frontend.Configuration;
using RadixDlt.NetworkGateway.Frontend.CoreCommunications;
using RadixDlt.NetworkGateway.Frontend.Endpoints;
using RadixDlt.NetworkGateway.Frontend.Initializers;
using RadixDlt.NetworkGateway.Frontend.Services;
using RadixDlt.NetworkGateway.Frontend.Workers;
using System.Net;

namespace RadixDlt.NetworkGateway.Frontend;

public static class ServiceCollectionExtensions
{
    public static void AddNetworkGatewayFrontend(this IServiceCollection services, string roConnectionString, string rwConnectionString)
    {
        services
            .AddValidatableOptionsAtSection<EndpointOptions, EndpointOptionsValidator>("GatewayApi:Endpoint")
            .AddValidatableOptionsAtSection<NetworkOptions, NetworkOptionsValidator>("GatewayApi:Network")
            .AddValidatableOptionsAtSection<AcceptableLedgerLagOptions, AcceptableLedgerLagOptionsValidator>("GatewayApi:AcceptableLedgerLag");

        services
            .AddOptions<NetworkOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("Api"); // TODO is this how we want to Bind configuration by default?

        // Initializers
        AddInitializers(services);

        // Singleton-Scoped services
        AddSingletonServices(services);
        AddHostedServices(services);

        // Request scoped services
        AddRequestScopedServices(services);
        AddReadOnlyDatabaseContext(services, roConnectionString);
        AddReadWriteDatabaseContext(services, rwConnectionString);

        // Other scoped services
        AddWorkerScopedServices(services);

        // Transient (pooled) services
        AddCoreApiHttpClient(services);
    }

    private static void AddInitializers(IServiceCollection services)
    {
        services
            .AddHostedService<NetworkConfigurationInitializer>();
    }

    private static void AddSingletonServices(IServiceCollection services)
    {
        // Should only contain services without any DBContext or HttpClient - as these both need to be recycled
        // semi-regularly
        services.AddSingleton<INetworkConfigurationProvider, NetworkConfigurationProvider>();
        services.AddSingleton<INetworkAddressConfigProvider>(x => x.GetRequiredService<INetworkConfigurationProvider>());
        services.AddSingleton<IValidations, Validations>();
        services.AddSingleton<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<IValidationErrorHandler, ValidationErrorHandler>();
        services.AddSingleton<IEntityDeterminer, EntityDeterminer>();
        services.AddSingleton<ICoreNodesSelectorService, CoreNodesSelectorService>();
    }

    private static void AddHostedServices(IServiceCollection services)
    {
        services.AddHostedService<CoreNodesSupervisorStatusReviseWorker>();
    }

    private static void AddRequestScopedServices(IServiceCollection services)
    {
        services.AddScoped<ILedgerStateQuerier, LedgerStateQuerier>();
        services.AddScoped<ITransactionQuerier, TransactionQuerier>();
        services.AddScoped<IConstructionAndSubmissionService, ConstructionAndSubmissionService>();
        services.AddScoped<ISubmissionTrackingService, SubmissionTrackingService>();
    }

    private static void AddWorkerScopedServices(IServiceCollection services)
    {
        services.AddScoped<ICoreNodeHealthChecker, CoreNodeHealthChecker>();
    }

    private static void AddCoreApiHttpClient(IServiceCollection services)
    {
        // NB - AddHttpClient is essentially like AddTransient, except it provides a HttpClient from the HttpClientFactory
        // See https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services
            .AddHttpClient<ICoreApiHandler, CoreApiHandler>()
            .UseHttpClientMetrics()
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => ConfigureHttpClientHandler(serviceProvider.GetRequiredService<IOptions<NetworkOptions>>()));

        services
            .AddHttpClient<ICoreNodeHealthChecker, CoreNodeHealthChecker>()
            .UseHttpClientMetrics()
            .ConfigurePrimaryHttpMessageHandler(serviceProvider => ConfigureHttpClientHandler(serviceProvider.GetRequiredService<IOptions<NetworkOptions>>()));
    }

    private static void AddReadOnlyDatabaseContext(IServiceCollection services, string roConnectionString)
    {
        services.AddDbContext<GatewayReadOnlyDbContext>(options =>
        {
            // https://www.npgsql.org/efcore/index.html
            options.UseNpgsql(roConnectionString, o => o.NonBrokenUseNodaTime());
        });
    }

    private static void AddReadWriteDatabaseContext(IServiceCollection services, string rwConnectionString)
    {
        services.AddDbContext<GatewayReadWriteDbContext>(options =>
        {
            // https://www.npgsql.org/efcore/index.html
            options.UseNpgsql(rwConnectionString, o => o.NonBrokenUseNodaTime());
        });
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
