using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RadixDlt.NetworkGateway.Frontend.Services;

namespace RadixDlt.NetworkGateway.Frontend.Initializers;

public class NetworkConfigurationInitializer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public NetworkConfigurationInitializer(IServiceProvider serviceProvider, ILogger<NetworkConfigurationInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var networkConfigurationProvider = scope.ServiceProvider.GetRequiredService<INetworkConfigurationProvider>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await networkConfigurationProvider
                    .LoadNetworkConfigurationFromDatabase(
                        scope.ServiceProvider.GetRequiredService<GatewayReadOnlyDbContext>(),
                        stoppingToken
                    );
                break;
            }
            catch (Exception exception)
            {
                if (exception.ShouldBeConsideredAppFatal())
                {
                    throw;
                }

                _logger.LogWarning(exception, "Error fetching network configuration - perhaps the data aggregator hasn't committed yet? Will try again in 2 seconds");

                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
