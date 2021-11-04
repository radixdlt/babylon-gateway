using DataAggregator.Configuration;
using DataAggregator.GlobalServices;

namespace DataAggregator.GlobalWorkers;

/// <summary>
/// Responsible for reading the config, and ensuring workers are running for each node.
/// </summary>
public class NodeConfigurationMonitorWorker : LoopedWorkerBase
{
    private readonly ILogger<NodeConfigurationMonitorWorker> _logger;
    private readonly INodeWorkersRunnerRegistry _nodeWorkersRunnerRegistry;
    private readonly IAggregatorConfiguration _configuration;

    public NodeConfigurationMonitorWorker(
        ILogger<NodeConfigurationMonitorWorker> logger,
        IAggregatorConfiguration configuration,
        INodeWorkersRunnerRegistry nodeWorkersRunnerRegistry
    )
        : base(logger, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10))
    {
        _logger = logger;
        _configuration = configuration;
        _nodeWorkersRunnerRegistry = nodeWorkersRunnerRegistry;
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        await HandleNodeConfiguration(stoppingToken);
    }

    protected override async Task OnStop(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Received cancellation at: {Time} - instructing all node workers to stop", DateTimeOffset.Now);
        await _nodeWorkersRunnerRegistry.StopAllWorkers(stoppingToken);
        _logger.LogInformation("Instructed all workers to stop");
    }

    private async Task HandleNodeConfiguration(CancellationToken stoppingToken)
    {
        var nodeConfiguration = _configuration.GetNodes();

        await Task.WhenAll(
            UpdateNodeConfigurationInDatabaseIfNeeded(),
            _nodeWorkersRunnerRegistry.EnsureCorrectNodeServicesRunning(nodeConfiguration, stoppingToken)
        );
    }

    private Task UpdateNodeConfigurationInDatabaseIfNeeded()
    {
        // TODO
        return Task.CompletedTask;
    }
}
