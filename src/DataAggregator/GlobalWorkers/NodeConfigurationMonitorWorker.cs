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

        var enabledNodes = nodeConfiguration
            .Where(n => n.EnabledForIndexing)
            /* TODO:NG-12 - Enable syncing from more than one node! */
            .Take(1)
            .ToList();

        await Task.WhenAll(
            UpdateNodeConfigurationInDatabaseIfNeeded(),
            _nodeWorkersRunnerRegistry.EnsureCorrectNodeServicesRunning(enabledNodes, stoppingToken)
        );
    }

    private Task UpdateNodeConfigurationInDatabaseIfNeeded()
    {
        // TODO:NG-12 - Ensure we write this if it's needed, or we get rid of this if it's not
        return Task.CompletedTask;
    }
}
