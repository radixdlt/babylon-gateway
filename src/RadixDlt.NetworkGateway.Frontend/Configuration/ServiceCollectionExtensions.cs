using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.Frontend.Endpoints;
using RadixDlt.NetworkGateway.Frontend.Services;

namespace RadixDlt.NetworkGateway.Frontend.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNetworkGatewayApi(this IServiceCollection services)
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

        services
            .AddScoped<GatewayEndpoint>()
            .AddScoped<TransactionEndpoint>();

        services
            .AddOptions<NetworkGatewayFrontendOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("Api"); // TODO is this how we want to Bind configuration by default?

        return services;
    }
}
