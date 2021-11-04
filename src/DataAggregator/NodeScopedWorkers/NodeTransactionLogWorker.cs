using DataAggregator.GlobalWorkers;
using DataAggregator.NodeScopedServices;

namespace DataAggregator.NodeScopedWorkers;

/// <summary>
/// Responsible for syncing the transaction stream from a node.
/// </summary>
public class NodeTransactionLogWorker : LoopedWorkerBase, INodeWorker
{
    private readonly ITransactionLogReader _transactionLogReader;

    public NodeTransactionLogWorker(
        ILogger<NodeTransactionLogWorker> logger,
        ITransactionLogReader transactionLogReader
    )
        : base(logger, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10))
    {
        _transactionLogReader = transactionLogReader;
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        // TODO
        await _transactionLogReader.GetTransactions(1, 1);
    }
}
