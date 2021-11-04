using DataAggregator.GlobalWorkers;
using DataAggregator.NodeScopedServices;

namespace DataAggregator.NodeScopedWorkers;

/// <summary>
/// Responsible for syncing the transaction stream from a node.
/// </summary>
public class NodeTransactionLogWorker : LoopedWorkerBase, INodeWorker
{
    private readonly ILogger<NodeTransactionLogWorker> _logger;
    private readonly ITransactionLogReader _transactionLogReader;
    private int _stateVersion = 1;

    public NodeTransactionLogWorker(
        ILogger<NodeTransactionLogWorker> logger,
        ITransactionLogReader transactionLogReader
    )
        : base(logger, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10))
    {
        _logger = logger;
        _transactionLogReader = transactionLogReader;
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        // TODO - do something sensible
        var transactions = await _transactionLogReader.GetTransactions(_stateVersion, 1);
        _logger.LogInformation(
            "Transaction {StateVersion} has {Count} operation groups",
            _stateVersion,
            transactions[0].OperationGroups.Count
        );
        _stateVersion++;
    }
}
