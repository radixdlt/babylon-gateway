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
    private readonly ILedgerExtenderService _ledgerExtenderService;

    public NodeTransactionLogWorker(
        ILogger<NodeTransactionLogWorker> logger,
        ITransactionLogReader transactionLogReader,
        ILedgerExtenderService ledgerExtenderService
    )
        : base(logger, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10))
    {
        _logger = logger;
        _transactionLogReader = transactionLogReader;
        _ledgerExtenderService = ledgerExtenderService;
    }

    // TODO:NG-12 - Implement node-specific syncing state machine, and separate committing into a global worker...
    // TODO:NG-40 - Do special actions when we start the ledger: Save the network of the ledger, and check this against our configuration before we commit.
    // TODO:NG-13 - Ensure we still maintain the primary aggregator lock in the database before we commit
    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        const int TransactionsToPull = 1000;

        _logger.LogInformation("Starting sync loop by looking up the top of the committed ledger");

        var topOfLedgerStateVersion = await _ledgerExtenderService.GetTopOfLedgerStateVersion(stoppingToken);

        _logger.LogInformation(
            "Last commit at top of DB ledger is at resultant state version {StateVersion}",
            topOfLedgerStateVersion
        );

        var getTransactionsStopwatch = new Stopwatch();
        getTransactionsStopwatch.Start();

        var transactionsResponse = await _transactionLogReader.GetTransactions(topOfLedgerStateVersion, TransactionsToPull, stoppingToken);

        _logger.LogInformation(
            "Read {TransactionCount} transactions from the core api in {MillisecondsElapsed}ms",
            TransactionsToPull,
            getTransactionsStopwatch.ElapsedMilliseconds
        );

        var commitTransactionsStopwatch = new Stopwatch();
        commitTransactionsStopwatch.Start();

        var commitedTransactionSummary = await _ledgerExtenderService.CommitTransactions(
            transactionsResponse.StateIdentifier,
            transactionsResponse.Transactions,
            stoppingToken
        );

        _logger.LogInformation(
            "Committed {TransactionCount} transactions to the DB in {MillisecondsElapsed}ms (ledger is up to StateVersion={LedgerStateVersion}, Epoch={LedgerEpoch}, IndexInEpoch={LedgerIndexInEpoch}, Timestamp={LedgerTimestamp})",
            TransactionsToPull,
            commitTransactionsStopwatch.ElapsedMilliseconds,
            commitedTransactionSummary.StateVersion,
            commitedTransactionSummary.Epoch,
            commitedTransactionSummary.IndexInEpoch,
            commitedTransactionSummary.CurrentViewTimestamp
        );
    }
}
