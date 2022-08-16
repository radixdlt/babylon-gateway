using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.Common;
using RadixDlt.NetworkGateway.Common.Database;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.DataAggregator.Services;

namespace RadixDlt.NetworkGateway.PostgresIntegration.DataAggregator;

public static class ServiceCollectionExtensions
{
    public static void TmpAddNetworkGatewayDataAggregator(this IServiceCollection services)
    {
        services
            .AddSingleton<IRawTransactionWriter, RawTransactionWriter>()
            .AddSingleton<ILedgerExtenderService, LedgerExtenderService>()
            .AddSingleton<INetworkConfigurationProvider, NetworkConfigurationProvider>()
            .AddSingleton<IMempoolTrackerService, MempoolTrackerService>()
            .AddSingleton<IMempoolResubmissionService, MempoolResubmissionService>()
            .AddSingleton<IMempoolPrunerService, MempoolPrunerService>();

        services
            .AddHealthChecks()
            .AddDbContextCheck<ReadWriteDbContext>("network_gateway_data_aggregator_database_readwrite_connection");

        // Useful links:
        // https://www.npgsql.org/efcore/index.html
        // https://www.npgsql.org/doc/connection-string-parameters.html
        services
            .AddDbContextFactory<ReadWriteDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(NetworkGatewayConstants.Database.ReadWriteConnectionStringName), o => o.NonBrokenUseNodaTime());
            });
    }
}
