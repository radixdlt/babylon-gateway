using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.NetworkGateway.Common;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

public static class ServiceCollectionExtensions
{
    public static void AddNetworkGatewayPostgresMigrations(this IServiceCollection services)
    {
        services.AddDbContextFactory<MigrationsDbContext>((serviceProvider, options) =>
        {
            // https://www.npgsql.org/efcore/index.html
            options.UseNpgsql(
                serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(NetworkGatewayConstants.Database.MigrationsConnectionStringName),
                o => o.MigrationsAssembly(typeof(MigrationsDbContext).Assembly.GetName().Name));
        });
    }
}
