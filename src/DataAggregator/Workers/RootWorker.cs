using DataAggregator.Configuration;
using DataAggregator.Services;
using Shared.Utilities;

namespace DataAggregator.Workers;

/// <summary>
/// Responsible for reading the config, and ensuring workers are running for each node.
/// </summary>
public class RootWorker : BackgroundService
{
    private readonly ILogger<RootWorker> _logger;
    private readonly NodeWorkerRunnerService _nodeWorkerRunnerService;
    private readonly AggregatorConfiguration _configuration;

    public RootWorker(ILogger<RootWorker> logger, AggregatorConfiguration configuration, NodeWorkerRunnerService nodeWorkerRunnerService)
    {
        _logger = logger;
        _configuration = configuration;
        _nodeWorkerRunnerService = nodeWorkerRunnerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var loopLogLimiter = new LogLimiter(TimeSpan.FromSeconds(10), LogLevel.Information, LogLevel.Debug);
        _logger.Log(loopLogLimiter.GetLogLevel(), "Starting at: {Time}", DateTimeOffset.Now);
        while (!stoppingToken.IsCancellationRequested)
        {
            var minDelayBetweenLoops = Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            _logger.Log(loopLogLimiter.GetLogLevel(), "Still running at: {Time}", DateTimeOffset.Now);
            await HandleNodeConfiguration(stoppingToken);
            await minDelayBetweenLoops;
        }

        _logger.LogInformation("Received cancellation at: {Time} - instructing all node workers to stop", DateTimeOffset.Now);
        await _nodeWorkerRunnerService.StopAllWorkers(stoppingToken);
    }

    private async Task HandleNodeConfiguration(CancellationToken stoppingToken)
    {
        var nodeConfiguration = _configuration.GetNodes();

        await Task.WhenAll(
            UpdateNodeConfigurationInDatabaseIfNeeded(),
            _nodeWorkerRunnerService.EnsureCorrectNodeServicesRunning(nodeConfiguration, stoppingToken)
        );
    }

    private Task UpdateNodeConfigurationInDatabaseIfNeeded()
    {
        // TODO
        return Task.CompletedTask;
    }
}
