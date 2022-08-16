using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.Common;
using RadixDlt.NetworkGateway.Common.Database;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.GatewayApi;
using RadixDlt.NetworkGateway.GatewayApi.Initializers;
using RadixDlt.NetworkGateway.GatewayApi.Services;

namespace RadixDlt.NetworkGateway.PostgresIntegration.GatewayApi;

public static class GatewayApiBuilderExtensions
{
    public static GatewayApiBuilder UsePostgresPersistence(this GatewayApiBuilder builder)
    {
        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<ReadOnlyDbContext>("network_gateway_api_database_readonly_connection")
            .AddDbContextCheck<ReadWriteDbContext>("network_gateway_api_database_readwrite_connection");

        builder.Services
            .AddHostedService<NetworkConfigurationInitializer>();

        builder.Services
            .AddScoped<ILedgerStateQuerier, LedgerStateQuerier>()
            .AddScoped<ITransactionQuerier, TransactionQuerier>()
            .AddScoped<SubmissionTrackingService>()
            .AddScoped<ISubmissionTrackingService>(provider => provider.GetRequiredService<SubmissionTrackingService>())
            .AddScoped<IMempoolQuerier>(provider => provider.GetRequiredService<SubmissionTrackingService>())
            .AddScoped<ICapturedConfigProvider, CapturedConfigProvider>();

        builder.Services
            .AddDbContext<ReadOnlyDbContext>((serviceProvider, options) =>
            {
                // https://www.npgsql.org/efcore/index.html
                options.UseNpgsql(serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(NetworkGatewayConstants.Database.ReadOnlyConnectionStringName), o => o.NonBrokenUseNodaTime());
            })
            .AddDbContext<ReadWriteDbContext>((serviceProvider, options) =>
            {
                // https://www.npgsql.org/efcore/index.html
                options.UseNpgsql(serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(NetworkGatewayConstants.Database.ReadWriteConnectionStringName), o => o.NonBrokenUseNodaTime());
            });

        return builder;
    }
}
