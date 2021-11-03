using DataAggregator.Configuration.Models;
using Shared.Utilities;

namespace DataAggregator.Workers;

/// <summary>
/// Responsible for syncing the transaction stream from a node.
/// </summary>
public class NodeTransactionLogWorker : BackgroundService
{
    private readonly ILogger<NodeTransactionLogWorker> _logger;
    private readonly NodeAppSettings _nodeAppSettings;

    public NodeTransactionLogWorker(ILogger<NodeTransactionLogWorker> logger, NodeAppSettings nodeAppSettings)
    {
        _logger = logger;
        _nodeAppSettings = nodeAppSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var loopLogLimiter = new LogLimiter(TimeSpan.FromSeconds(10), LogLevel.Information, LogLevel.Debug);
        _logger.Log(
            loopLogLimiter.GetLogLevel(),
            "{NodeName} ({NodeAddress}) - Starting at: {Time}",
            _nodeAppSettings.Name, _nodeAppSettings.Address, DateTimeOffset.Now
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            var minDelayBetweenLoops = Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            _logger.Log(
                loopLogLimiter.GetLogLevel(),
                "{NodeName} ({NodeAddress}) - still running at {Time}",
                _nodeAppSettings.Name, _nodeAppSettings.Address, DateTimeOffset.Now
            );

            // Do something
            await minDelayBetweenLoops;
        }

        _logger.LogInformation(
            "{NodeName} ({NodeAddress}) - Stopping at: {Time}",
            _nodeAppSettings.Name, _nodeAppSettings.Address, DateTimeOffset.Now
        );
    }
}
