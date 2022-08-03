using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.Endpoints;
using RadixDlt.NetworkGateway.Services;
using RadixDlt.NetworkGateway.Workers;

namespace RadixDlt.NetworkGateway.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNetworkGatewayDataAggregator(this IServiceCollection services)
    {
        AddSharedServices(services);

        services
            .AddSingleton<DistributedLockService>();

        services
            .AddHostedService<LedgerSynchronizationWorker>();

        services
            .AddOptions<DataAggregatorOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("DataAggregator"); // TODO is this how we want to Bind configuration by default?

        return services;
    }

    public static IServiceCollection AddNetworkGatewayApi(this IServiceCollection services)
    {
        AddSharedServices(services);

        services
            .AddScoped<GatewayEndpoint>()
            .AddScoped<TransactionEndpoint>();

        services
            .AddOptions<ApiOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("Api"); // TODO is this how we want to Bind configuration by default?

        return services;
    }

    private static void AddSharedServices(IServiceCollection services)
    {
        services.AddDbContext<GatewayReadOnlyDbContext>(options =>
        {
            options.UseNpgsql("Host=localhost:5532;Database=babylon_stokenet;Username=db_superuser;Password=db_password;Include Error Detail=true", o => o.NonBrokenUseNodaTime());

            // // https://www.npgsql.org/efcore/index.html
            // options.UseNpgsql(
            //     hostContext.Configuration.GetConnectionString("ReadOnlyDbContext"),
            //
            // );
        });

        services
            .AddScoped<ILedgerStateQuerier, LedgerStateQuerier>()
            .AddScoped<ITransactionQuerier, TransactionQuerier>();
    }
}
