using DataAggregator.Configuration.Models;
using Shared.Utilities;

namespace DataAggregator.Workers;

/// <summary>
/// Responsible for syncing the transaction stream from a node
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
        _logger.Log(loopLogLimiter.GetLogLevel(), "Node {node}: Starting at: {time}", _nodeAppSettings.GetNodeNiceName(), DateTimeOffset.Now);
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.Log(
                loopLogLimiter.GetLogLevel(),
                "Node {node}: still running at {time}",
                _nodeAppSettings.GetNodeNiceName(),
                DateTimeOffset.Now
            );
            await Task.Delay(1000, stoppingToken);
        }
        _logger.LogInformation("Node {node}: Stopping at: {time}", _nodeAppSettings.GetNodeNiceName(), DateTimeOffset.Now);
    }
}
