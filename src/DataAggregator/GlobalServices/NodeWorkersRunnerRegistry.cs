using DataAggregator.Configuration.Models;
using DataAggregator.DependencyInjection;
using DataAggregator.NodeScopedServices;

namespace DataAggregator.GlobalServices;

public interface INodeWorkersRunnerRegistry
{
    Task EnsureCorrectNodeServicesRunning(List<NodeAppSettings> nodes, CancellationToken cancellationToken);

    Task StopAllWorkers(CancellationToken cancellationToken);
}

public class NodeWorkersRunnerRegistry : INodeWorkersRunnerRegistry
{
    private readonly ILogger<INodeWorkersRunnerRegistry> _logger;
    private readonly INodeWorkersRunnerFactory _nodeWorkersRunnerFactory;
    private readonly Dictionary<string, NodeWorkersRunner> _servicesMap = new();
    private readonly object _servicesMapLock = new();

    public NodeWorkersRunnerRegistry(ILogger<INodeWorkersRunnerRegistry> logger, INodeWorkersRunnerFactory nodeWorkersRunnerFactory)
    {
        _logger = logger;
        _nodeWorkersRunnerFactory = nodeWorkersRunnerFactory;
    }

    public async Task EnsureCorrectNodeServicesRunning(List<NodeAppSettings> nodes, CancellationToken cancellationToken)
    {
        var enabledNodesSettingsMap = nodes
            .Where(n => n.EnabledForIndexing)
            .ToDictionary(n => n.Name);

        // Find nodes to start
        var nodeWorkersToStart = enabledNodesSettingsMap
            .Where(kvp => !_servicesMap.ContainsKey(kvp.Key))
            .Select(kvp => kvp.Value);

        var startTask = StartNodeWorkersForNodes(nodeWorkersToStart, cancellationToken);

        // Find nodes to shutdown
        var nodeNamesToStop = _servicesMap.Keys.Except(enabledNodesSettingsMap.Keys);
        var endTask = StopNodeWorkersForNodes(nodeNamesToStop, cancellationToken);

        await Task.WhenAll(startTask, endTask);
    }

    public async Task StopAllWorkers(CancellationToken cancellationToken)
    {
        await StopNodeWorkersForNodes(_servicesMap.Keys, cancellationToken);
    }

    private Task StartNodeWorkersForNodes(IEnumerable<NodeAppSettings> nodes, CancellationToken cancellationToken)
    {
        return Task.WhenAll(nodes.Select(n => CreateAndStartNodeWorkersIfNotExists(n, cancellationToken)));
    }

    private async Task CreateAndStartNodeWorkersIfNotExists(NodeAppSettings nodeAppSettings, CancellationToken cancellationToken)
    {
        NodeWorkersRunner nodeWorkersRunner;
        lock (_servicesMapLock)
        {
            if (_servicesMap.ContainsKey(nodeAppSettings.Name))
            {
                return;
            }

            nodeWorkersRunner = _nodeWorkersRunnerFactory.CreateWorkersForNode(nodeAppSettings);
            _servicesMap.Add(nodeAppSettings.Name, nodeWorkersRunner);
        }

        try
        {
            _logger.LogInformation("Starting workers for node: {NodeName}", nodeAppSettings.Name);
            await nodeWorkersRunner.StartWorkers(cancellationToken);
            _logger.LogInformation("Workers for node started successfully: {NodeName}", nodeAppSettings.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting up services for node: {NodeName}. Now clearing up", nodeAppSettings.Name);
            await StopNodeWorkers(nodeAppSettings.Name, cancellationToken);
        }
    }

    private Task StopNodeWorkersForNodes(IEnumerable<string> nodeNames, CancellationToken cancellationToken)
    {
        return Task.WhenAll(nodeNames.Select(n => StopNodeWorkers(n, cancellationToken)));
    }

    private async Task StopNodeWorkers(string nodeName, CancellationToken cancellationToken)
    {
        if (!_servicesMap.TryGetValue(nodeName, out var nodeWorkersRunner))
        {
            // It's already been stopped/removed
            return;
        }

        _logger.LogInformation("Sending instruction to stop workers for node {NodeName}", nodeName);

        try
        {
            await nodeWorkersRunner.StopWorkersSafe(cancellationToken);
            _logger.LogInformation("Node workers stopped successfully for node {NodeName}", nodeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping services for node: {NodeName}. Now clearing up regardless", nodeName);
        }
        finally
        {
            // If Disposal fails, panic
            nodeWorkersRunner.Dispose();
        }

        lock (_servicesMapLock)
        {
            _servicesMap.Remove(nodeName);
        }
    }
}
