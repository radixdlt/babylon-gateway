using Common.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RadixDlt.NetworkGateway.Frontend.Services;

namespace RadixDlt.NetworkGateway.Frontend.Initializers;

public class DatabaseInitializer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitializer(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var maxWaitForDbMs = _configuration.GetValue("MaxWaitForDbOnStartupMs", 0);

        await ConnectionHelpers.TryWaitForExistingDb<GatewayReadOnlyDbContext>(_serviceProvider, maxWaitForDbMs);
    }
}
