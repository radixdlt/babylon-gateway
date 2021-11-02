using DataAggregator.Configuration.Models;
using DataAggregator.Workers.Factory;

namespace DataAggregator.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public class NodeWorkerService
{
    private readonly NodeWorkerFactory _nodeWorkerFactory;
    private readonly Dictionary<string, NodeWorkers> _servicesMap = new();
    private readonly object _servicesMapLock = new();

    public NodeWorkerService(NodeWorkerFactory nodeWorkerFactory)
    {
        _nodeWorkerFactory = nodeWorkerFactory;
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
        NodeWorkers nodeWorkers;
        lock (_servicesMapLock)
        {
            if (_servicesMap.ContainsKey(nodeAppSettings.Name))
            {
                return;
            }

            nodeWorkers = _nodeWorkerFactory.CreateWorkersForNode(nodeAppSettings);
            _servicesMap.Add(nodeAppSettings.Name, nodeWorkers);
        }

        await nodeWorkers.StartWorkers(cancellationToken);
    }

    private Task StopNodeWorkersForNodes(IEnumerable<string> nodeNames, CancellationToken cancellationToken)
    {
        return Task.WhenAll(nodeNames.Select(n => StopNodeWorkers(n, cancellationToken)));
    }

    private async Task StopNodeWorkers(string nodeName, CancellationToken cancellationToken)
    {
        if (!_servicesMap.TryGetValue(nodeName, out var nodeWorkers))
        {
            // It's already been stopped/removed
            return;
        }

        await nodeWorkers.StopWorkersSafe(cancellationToken);
        lock (_servicesMapLock)
        {
            _servicesMap.Remove(nodeName);
        }
    }
}
