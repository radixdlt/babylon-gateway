using DataAggregator.GlobalServices;
using DataAggregator.GlobalWorkers;
using DataAggregator.NodeScopedServices.ApiReaders;

namespace DataAggregator.NodeScopedWorkers;

/// <summary>
/// Responsible for syncing the transaction stream from a node.
/// </summary>
public class NodeTransactionLogWorker : LoopedWorkerBase, INodeWorker
{
    private readonly ILogger<NodeTransactionLogWorker> _logger;
    private readonly ITransactionLogReader _transactionLogReader;
    private readonly ITransactionCommitter _transactionCommitter;
    private int _transactionIndex;

    public NodeTransactionLogWorker(
        ILogger<NodeTransactionLogWorker> logger,
        ITransactionLogReader transactionLogReader,
        ITransactionCommitter transactionCommitter
    )
        : base(logger, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10))
    {
        _logger = logger;
        _transactionLogReader = transactionLogReader;
        _transactionCommitter = transactionCommitter;
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        // TODO - do something sensible
        var transactions = await _transactionLogReader.GetTransactions(_transactionIndex, 10);
        _logger.LogInformation(
            "Transaction {StateVersion} has {Count} operation groups",
            _transactionIndex,
            transactions[0].OperationGroups.Count
        );
        await _transactionCommitter.CommitTransactions(transactions, stoppingToken);
        _transactionIndex += 10;
    }
}
