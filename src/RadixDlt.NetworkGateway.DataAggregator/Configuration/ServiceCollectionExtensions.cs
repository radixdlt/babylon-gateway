using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.DataAggregator.Workers;

namespace RadixDlt.NetworkGateway.DataAggregator.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNetworkGatewayDataAggregator(this IServiceCollection services)
    {
        services.AddDbContext<GatewayReadWriteDbContext>(options =>
        {
            options.UseNpgsql("Host=localhost:5532;Database=babylon_stokenet;Username=db_superuser;Password=db_password;Include Error Detail=true", o => o.NonBrokenUseNodaTime());

            // // https://www.npgsql.org/efcore/index.html
            // options.UseNpgsql(
            //     hostContext.Configuration.GetConnectionString("ReadOnlyDbContext"),
            //
            // );
        });

        services
            .AddSingleton<DistributedLockService>();

        services
            .AddHostedService<LedgerSynchronizationWorker>();

        services
            .AddOptions<NetworkGatewayDataAggregatorOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("DataAggregator"); // TODO is this how we want to Bind configuration by default?

        return services;
    }
}
