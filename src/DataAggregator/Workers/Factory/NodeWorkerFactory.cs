using DataAggregator.Configuration.Models;

namespace DataAggregator.Workers.Factory;

public class NodeWorkerFactory
{
    private readonly ILogger<NodeWorkerFactory> _ownLogger;
    private readonly ILogger<NodeTransactionLogWorker> _nodeTransactionLogWorkerLogger;

    public NodeWorkerFactory(ILogger<NodeWorkerFactory> ownLogger, ILogger<NodeTransactionLogWorker> nodeTransactionLogWorkerLogger)
    {
        _ownLogger = ownLogger;
        _nodeTransactionLogWorkerLogger = nodeTransactionLogWorkerLogger;
    }

    public NodeWorkers CreateWorkersForNode(NodeAppSettings nodeAppSettings)
    {
        var nodeCancellationTokenSource = new CancellationTokenSource();

        // Create workers
        var nodeTransactionLogWorker = new NodeTransactionLogWorker(_nodeTransactionLogWorkerLogger, nodeAppSettings);

        // Capture workers together
        var nodeWorkers = new NodeWorkers(
            nodeCancellationTokenSource,
            new List<IHostedService>
            {
                nodeTransactionLogWorker
            }
        );

        return nodeWorkers;
    }
}
