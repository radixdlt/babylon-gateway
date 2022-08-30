using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

public static class ServiceCollectionExtensions
{
    public static void AddNetworkGatewayPostgresMigrations(this IServiceCollection services)
    {
        services
            .AddDbContextFactory<MigrationsDbContext>((serviceProvider, options) =>
            {
                // https://www.npgsql.org/efcore/index.html
                options.UseNpgsql(
                    serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(PostgresIntegrationConstants.Configuration.MigrationsConnectionStringName),
                    o => o.MigrationsAssembly(typeof(MigrationsDbContext).Assembly.GetName().Name));
            });
    }
}
