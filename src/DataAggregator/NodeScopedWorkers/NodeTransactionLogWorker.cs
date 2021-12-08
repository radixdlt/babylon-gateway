/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using Common.Extensions;
using Common.Utilities;
using DataAggregator.GlobalServices;
using DataAggregator.GlobalWorkers;
using DataAggregator.Monitoring;
using DataAggregator.NodeScopedServices;
using DataAggregator.NodeScopedServices.ApiReaders;
using Prometheus;
using RadixCoreApi.Generated.Model;

namespace DataAggregator.NodeScopedWorkers;

/// <summary>
/// Responsible for syncing the transaction stream from a node.
/// </summary>
public class NodeTransactionLogWorker : LoopedWorkerBase, INodeWorker
{
    private static readonly Counter _failedFetchAndCommitLoops = Metrics
        .CreateCounter("ledger_batch_fetch_commit_loop_error_total", "Number of commit loop errors that failed.");

    private static readonly Histogram _totalFetchTimeSeconds = Metrics
        .CreateHistogram(
            "ledger_batch_fetch_time_seconds",
            "Total time to fetch a batch of transactions.",
            new HistogramConfiguration { Buckets = Histogram.LinearBuckets(start: 0.1, width: 0.1, count: 50) }
        );

    private static readonly Histogram _totalCommitTimeSeconds = Metrics
        .CreateHistogram(
            "ledger_batch_commit_time_seconds",
            "Total time to commit a batch of transactions.",
            new HistogramConfiguration { Buckets = Histogram.LinearBuckets(start: 0.2, width: 0.2, count: 100) }
        );

    /* Dependencies */
    private readonly ILogger<NodeTransactionLogWorker> _logger;
    private readonly IServiceProvider _services;
    private readonly ISystemStatusService _systemStatusService;

    /* Properties for simple fetch pipelining */
    private record FetchPipeline(
        long TopOfLedgerStateVersion,
        int TransactionsToPull,
        Task<CommittedTransactionsResponse> FetchTask
    );

    private FetchPipeline? _pipelinedFetch;


    // NB - So that we can get new transient dependencies each iteration, we create most dependencies
    //      from the service provider.
    public NodeTransactionLogWorker(
        ILogger<NodeTransactionLogWorker> logger,
        IServiceProvider services,
        ISystemStatusService systemStatusService
    )
        : base(logger, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60))
    {
        _logger = logger;
        _services = services;
        _systemStatusService = systemStatusService;
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        await _failedFetchAndCommitLoops.CountExceptionsAsync(() => FetchAndCommitTransactions(stoppingToken));
    }

    // TODO:NG-12 - Implement node-specific syncing state machine, and separate committing into a global worker...
    // TODO:NG-40 - Do special actions when we start the ledger: Save the network of the ledger, and check this against our configuration before we commit.
    // TODO:NG-13 - Ensure we still maintain the primary aggregator lock in the database before we commit
    private async Task FetchAndCommitTransactions(CancellationToken stoppingToken)
    {
        const int TransactionsToPull = 1000;
        var ledgerExtenderService = _services.GetRequiredService<ILedgerExtenderService>();

        _logger.LogInformation("Starting sync loop by looking up the top of the committed ledger");

        var (topOfLedger, readTopOfLedgerMs) = await CodeStopwatch.TimeInMs(
            () => ledgerExtenderService.GetTopOfLedger(stoppingToken)
        );

        _systemStatusService.RecordTopOfLedger(topOfLedger);

        _logger.LogInformation(
            "Top of DB ledger is at state version {StateVersion} (read in {ReadTopOfLedgerMs}ms)",
            topOfLedger.StateVersion,
            readTopOfLedgerMs
        );

        // TODO:NG-12 - turn on pipelining if we can speed up the result parsing by the client
        var transactionsResponse = await FetchTransactionsFromCoreApiAndPipelineNextFetch(
            topOfLedger.StateVersion, TransactionsToPull, false, stoppingToken
        );

        if (transactionsResponse.Transactions.Count == 0)
        {
            _logger.LogInformation(
                "No new transactions found, sleeping for {DelayMs}ms",
                GetRemainingRestartDelay().Milliseconds
            );
        }

        var (committedTransactionReport, totalCommitTransactionsMs) = await CodeStopwatch.TimeInMs(
            () => ledgerExtenderService.CommitTransactions(
                transactionsResponse.StateIdentifier,
                transactionsResponse.Transactions,
                stoppingToken
            )
        );

        _systemStatusService.RecordTransactionsCommitted(committedTransactionReport);
        _totalCommitTimeSeconds.Observe(totalCommitTransactionsMs / 1000D);

        _logger.LogInformation(
            "Committed {TransactionCount} transactions to the DB in {TotalCommitTransactionsMs}ms [EntitiesTouched={DbEntriesWritten},TxnContentDbActions={TransactionContentDbActionsCount}]",
            transactionsResponse.Transactions.Count,
            totalCommitTransactionsMs,
            committedTransactionReport.DbEntriesWritten,
            committedTransactionReport.TransactionContentDbActionsCount
        );
        _logger.LogInformation(
            "[TimeSplitsInMs: RawTxnPersistence={RawTxnPersistenceMs},TxnContentHandling={TxnContentHandlingMs},DbDependencyLoading={DbDependenciesLoadingMs},LocalDbContextActions={LocalDbContextActionsMs},DbPersistence={DbPersistanceMs}]",
            committedTransactionReport.RawTxnPersistenceMs,
            committedTransactionReport.TransactionContentHandlingMs,
            committedTransactionReport.DbDependenciesLoadingMs,
            committedTransactionReport.LocalDbContextActionsMs,
            committedTransactionReport.DbPersistanceMs
        );

        var committedTransactionSummary = committedTransactionReport.FinalTransaction;
        _logger.LogInformation(
            "[NewDbLedgerTip: StateVersion={LedgerStateVersion},Epoch={LedgerEpoch},IndexInEpoch={LedgerIndexInEpoch},NormalizedTimestamp={NormalizedTimestamp}]",
            committedTransactionSummary.StateVersion,
            committedTransactionSummary.Epoch,
            committedTransactionSummary.IndexInEpoch,
            committedTransactionSummary.NormalizedTimestamp.AsUtcIsoDateToSecondsForLogs()
        );
    }

    private Task<CommittedTransactionsResponse> FetchTransactionsFromCoreApiAndPipelineNextFetch(
        long topOfLedgerStateVersion,
        int transactionsToPull,
        bool shouldPipeline,
        CancellationToken stoppingToken
    )
    {
        var currentPipelinedFetch = _pipelinedFetch;
        var currentPipelinedFetchMatches =
            currentPipelinedFetch != null
            && currentPipelinedFetch.TopOfLedgerStateVersion == topOfLedgerStateVersion
            && currentPipelinedFetch.TransactionsToPull == transactionsToPull;

        var currentFetch = currentPipelinedFetchMatches
            ? currentPipelinedFetch!.FetchTask
            : FetchTransactionsFromCoreApi(topOfLedgerStateVersion, transactionsToPull, stoppingToken);

        if (shouldPipeline)
        {
            var nextFromVersion = topOfLedgerStateVersion + transactionsToPull;
            _pipelinedFetch = new FetchPipeline(
                nextFromVersion,
                transactionsToPull,
                FetchTransactionsFromCoreApi(nextFromVersion, transactionsToPull, stoppingToken)
            );
        }
        else
        {
            _pipelinedFetch = null;
        }

        return currentFetch;
    }

    private async Task<CommittedTransactionsResponse> FetchTransactionsFromCoreApi(
        long topOfLedgerStateVersion,
        int transactionsToPull,
        CancellationToken stoppingToken
    )
    {
        _logger.LogInformation(
            "Fetching up to {TransactionCount} transactions from version {FromStateVersion} from the core api",
            transactionsToPull,
            topOfLedgerStateVersion
        );

        var (transactionsResponse, fetchTransactionsMs) = await CodeStopwatch.TimeInMs(
            () => _services.GetRequiredService<ITransactionLogReader>().GetTransactions(topOfLedgerStateVersion, transactionsToPull, stoppingToken)
        );

        _totalFetchTimeSeconds.Observe(fetchTransactionsMs / 1000D);

        _logger.LogInformation(
            "Fetched {TransactionCount} transactions from version {FromStateVersion} from the core api in {FetchTransactionsMs}ms",
            transactionsResponse.Transactions.Count,
            topOfLedgerStateVersion,
            fetchTransactionsMs
        );
        return transactionsResponse;
    }
}
