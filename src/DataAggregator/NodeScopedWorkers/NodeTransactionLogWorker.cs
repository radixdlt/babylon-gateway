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
    private int _stateVersion = 1;

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
        // TODO - record where we've got up to; implement syncing state machine
        var transactions = await _transactionLogReader.GetTransactions(_stateVersion, 100, stoppingToken);
        _logger.LogInformation(
            "Transaction {StateVersion} has {Count} operation groups",
            _stateVersion,
            transactions[0].OperationGroups.Count
        );

        // For now - just commit all transactions > this should actually be a global service, not a node-scoped service
        // But I'm focusing on CommitTransactions for the purposes of the example, and will come back and fix this later
        await _transactionCommitter.CommitTransactions(transactions, stoppingToken);
        _stateVersion += 100;
    }
}
