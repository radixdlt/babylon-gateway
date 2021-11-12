using DataAggregator.GlobalServices;
using DataAggregator.GlobalWorkers;
using DataAggregator.NodeScopedServices.ApiReaders;
using System.Diagnostics;

namespace DataAggregator.NodeScopedWorkers;

/// <summary>
/// Responsible for syncing the transaction stream from a node.
/// </summary>
public class NodeTransactionLogWorker : LoopedWorkerBase, INodeWorker
{
    private readonly ILogger<NodeTransactionLogWorker> _logger;
    private readonly ITransactionLogReader _transactionLogReader;
    private readonly ITransactionCommitter _transactionCommitter;

    public NodeTransactionLogWorker(
        ILogger<NodeTransactionLogWorker> logger,
        ITransactionLogReader transactionLogReader,
        ITransactionCommitter transactionCommitter
    )
        : base(logger, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10))
    {
        _logger = logger;
        _transactionLogReader = transactionLogReader;
        _transactionCommitter = transactionCommitter;
    }

    // TODO implement syncing state machine, and separate committing into a global worker...
    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        const int TransactionsToPull = 1000;

        _logger.LogInformation("Getting top of ledger...");

        var topOfLedgerStateVersion = await _transactionCommitter.GetTopOfLedgerStateVersion(stoppingToken);

        _logger.LogInformation(
            "Reading {TransactionCount} transactions from state version {StateVersion} from the node api",
            TransactionsToPull,
            topOfLedgerStateVersion
        );

        var getTransactionsStopwatch = new Stopwatch();
        getTransactionsStopwatch.Start();

        var transactionsResponse = await _transactionLogReader.GetTransactions(topOfLedgerStateVersion, TransactionsToPull, stoppingToken);

        _logger.LogInformation(
            "Reading {TransactionCount} transactions from state version {StateVersion} from the node api took {MillisecondsElapsed}ms",
            TransactionsToPull,
            topOfLedgerStateVersion,
            getTransactionsStopwatch.ElapsedMilliseconds
        );

        var commitTransactionsStopwatch = new Stopwatch();
        commitTransactionsStopwatch.Start();

        _logger.LogInformation(
            "Preparing to commit {TransactionCount} transactions after state version {StateVersion}",
            TransactionsToPull,
            topOfLedgerStateVersion
        );

        // For now - just commit all transactions > this should actually be a global service, not a node-scoped service
        // But I'm focusing on CommitTransactions for the purposes of the example, and will come back and fix this later
        await _transactionCommitter.CommitTransactions(
            transactionsResponse.CommittedStateIdentifier,
            transactionsResponse.Transactions,
            stoppingToken
        );

        _logger.LogInformation(
            "Committing {TransactionCount} transactions after state version {StateVersion} took {MillisecondsElapsed}ms",
            TransactionsToPull,
            topOfLedgerStateVersion,
            commitTransactionsStopwatch.ElapsedMilliseconds
        );
    }
}
