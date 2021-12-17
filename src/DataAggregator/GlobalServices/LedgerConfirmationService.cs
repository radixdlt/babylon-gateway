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
using DataAggregator.Configuration;
using DataAggregator.Configuration.Models;
using DataAggregator.LedgerExtension;
using DataAggregator.Monitoring;
using Prometheus;
using RadixCoreApi.Generated.Model;
using System.Collections.Concurrent;

namespace DataAggregator.GlobalServices;

public interface ILedgerConfirmationService
{
    // This method is to be called from the global LedgerExtensionWorker
    Task HandleLedgerExtensionIfQuorum(CancellationToken token);

    // Below are to be called from the node transaction log workers - to communicate with the LedgerConfirmationService
    void SubmitLedgerTipFromNode(string nodeName, long stateVersion, byte[] accumulator);

    void SubmitTransactionsFromNode(string nodeName, List<CommittedTransaction> transactions);

    TransactionsRequested? GetWhichTransactionsAreRequestedFromNode(string nodeName);
}

public record TransactionsRequested(long StateVersionExclusiveLowerBound, long StateVersionInclusiveUpperBound);

/// <summary>
/// This service is responsible for controlling the NodeTransactionLogWorkers, and deciding on / committing when
/// a quorum is reached.
/// </summary>
public class LedgerConfirmationService : ILedgerConfirmationService
{
    /* Metrics */
    private static readonly Histogram _batchCommitTimeSeconds = Metrics
        .CreateHistogram(
            "ledger_batch_commit_time_seconds",
            "Total time to commit a batch of transactions.",
            new HistogramConfiguration { Buckets = Histogram.LinearBuckets(start: 0.2, width: 0.2, count: 100) }
        );

    /* Dependencies */
    private readonly ILogger<LedgerConfirmationService> _logger;
    private readonly IAggregatorConfiguration _aggregatorConfiguration;
    private readonly ISystemStatusService _systemStatusService;
    private readonly ILedgerExtenderService _ledgerExtenderService;

    /* Variables */

    // TODO - this is to work out if outdated nodes are consistent - Maybe implement an LRU Cache for this?
    // See comment: https://radixdlt.atlassian.net/browse/NG-62?focusedCommentId=13423
    private readonly LruCache<long, byte[]> _quorumAccumulatorCacheByStateVersion = new(2000);
    private readonly ConcurrentDictionary<string, long> _latestLedgerTipByNode = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, CommittedTransaction>> _transactionsByNode = new();
    private TransactionSummary? _knownTopOfCommittedLedger;

    /* Properties */
    private LedgerConfirmationConfiguration Config => _aggregatorConfiguration.GetLedgerConfirmationConfiguration();

    private IEnumerable<NodeAppSettings> TransactionNodes => _aggregatorConfiguration.GetNodes()
        .Where(n => n.Enabled && !n.DisabledForTransactionIndexing);

    public LedgerConfirmationService(
        ILogger<LedgerConfirmationService> logger,
        IAggregatorConfiguration aggregatorConfiguration,
        ISystemStatusService systemStatusService,
        ILedgerExtenderService ledgerExtenderService
    )
    {
        _logger = logger;
        _aggregatorConfiguration = aggregatorConfiguration;
        _systemStatusService = systemStatusService;
        _ledgerExtenderService = ledgerExtenderService;
    }

    /// <summary>
    /// To be run by the (single-threaded) ledger extender worker.
    /// </summary>
    public async Task HandleLedgerExtensionIfQuorum(CancellationToken token)
    {
        await LoadTopOfDbLedger(token);

        var transactions = ConstructQuorumLedgerExtension();

        if (transactions.Count == 0)
        {
            return;
        }

        var ledgerExtension = GenerateConsistentLedgerExtension(transactions);

        var (commitReport, totalCommitMs) = await CodeStopwatch.TimeInMs(
            () => _ledgerExtenderService.CommitTransactions(ledgerExtension, token)
        );

        ReportOnLedgerExtensionSuccess(ledgerExtension, totalCommitMs, commitReport);

        StopTrackingTransactionsUpToStateVersion(commitReport.FinalTransaction.StateVersion);
    }

    /// <summary>
    /// To be called from the node worker.
    /// </summary>
    public void SubmitLedgerTipFromNode(string nodeName, long stateVersion, byte[] accumulator)
    {
        _latestLedgerTipByNode[nodeName] = stateVersion;
        // TODO - check accumulator consistency
    }

    /// <summary>
    /// To be called from the node worker.
    /// </summary>
    public void SubmitTransactionsFromNode(
        string nodeName,
        List<CommittedTransaction> transactions
    )
    {
        if (!_latestLedgerTipByNode.ContainsKey(nodeName))
        {
            throw new Exception("The node's ledger tip must be written first");
        }

        var transactionStoreForNode = GetTransactionsForNode(nodeName);
        foreach (var transaction in transactions)
        {
            transactionStoreForNode[transaction.CommittedStateIdentifier.StateVersion] = transaction;
        }
    }

    /// <summary>
    /// To be called from the node worker.
    /// </summary>
    public TransactionsRequested? GetWhichTransactionsAreRequestedFromNode(string nodeName)
    {
        var currentTopOfLedger = _knownTopOfCommittedLedger;
        if (currentTopOfLedger == null)
        {
            return null;
        }

        var exclusiveLowerBound = currentTopOfLedger.StateVersion;
        var inclusiveUpperBound = currentTopOfLedger.StateVersion + Config.MaxTransactionPipelineSizePerNode;

        var firstMissingStateVersionGap = GetFirstStateVersionGapForNode(nodeName, exclusiveLowerBound, inclusiveUpperBound);

        return (firstMissingStateVersionGap == null || firstMissingStateVersionGap > inclusiveUpperBound)
            ? null
            : new TransactionsRequested(firstMissingStateVersionGap.Value - 1, inclusiveUpperBound);
    }

    private List<CommittedTransaction> ConstructQuorumLedgerExtension()
    {
        var extension = new List<CommittedTransaction>();

        var startStateVersion = _knownTopOfCommittedLedger!.StateVersion + 1;
        var totalTrustWeighting = GetTotalTrustWeighting();

        if (totalTrustWeighting == 0)
        {
            _logger.LogWarning("Total trust weighting computed as zero - perhaps no nodes are synced up?");
            return extension;
        }

        var trustThresholdForQuorum = Config.QuorumRequiresTrustProportion * totalTrustWeighting;

        for (var stateVersion = startStateVersion; stateVersion < startStateVersion + Config.MaxCommitBatchSize; stateVersion++)
        {
            var (bestTransaction, totalTrustAcrossAllTransactions) = FindBestTransactionAtStateVersion(stateVersion);

            if (bestTransaction == null || bestTransaction.Trust < trustThresholdForQuorum)
            {
                // It's likely we just don't have enough nodes contributing yet.
                // But, if it's not possible to reach quorum even with more nodes contributing - then we need to worry.
                var remainingTrustPossible = totalTrustWeighting - totalTrustAcrossAllTransactions;
                if ((bestTransaction?.Trust ?? 0m) + remainingTrustPossible < trustThresholdForQuorum)
                {
                    // TODO
                    // Raise the alarm! It's impossible to reach consensus...
                }

                break;
            }

            extension.Add(bestTransaction.Transaction);
        }

        return extension;
    }

    private void StopTrackingTransactionsUpToStateVersion(long committedStateVersion)
    {
        foreach (var (nodeName, transactions) in _transactionsByNode)
        {
            var stateVersionsToRemove = transactions.Keys.Where(k => k <= committedStateVersion).ToList();
            foreach (var stateVersion in stateVersionsToRemove)
            {
                transactions.TryRemove(stateVersion, out _);
            }
        }
    }

    private async Task LoadTopOfDbLedger(CancellationToken token)
    {
        var (topOfLedger, readTopOfLedgerMs) = await CodeStopwatch.TimeInMs(
            () => _ledgerExtenderService.GetTopOfLedger(token)
        );

        _knownTopOfCommittedLedger = topOfLedger;
        _systemStatusService.RecordTopOfDbLedger(topOfLedger);
        _logger.LogDebug(
            "Top of DB ledger is at state version {StateVersion} (read in {ReadTopOfLedgerMs}ms)",
            topOfLedger.StateVersion,
            readTopOfLedgerMs
        );
    }

    private void ReportOnLedgerExtensionSuccess(ConsistentLedgerExtension ledgerExtension, long totalCommitMs, CommitTransactionsReport commitReport)
    {
        var isSyncedUp = ledgerExtension.TransactionData.Count < Config.MaxCommitBatchSize;

        _systemStatusService.RecordTransactionsCommitted(commitReport, isSyncedUp);
        _batchCommitTimeSeconds.Observe(totalCommitMs / 1000D);

        _logger.LogInformation(
            "Committed {TransactionCount} transactions to the DB in {TotalCommitTransactionsMs}ms [EntitiesTouched={DbEntriesWritten},TxnContentDbActions={TransactionContentDbActionsCount}]",
            ledgerExtension.TransactionData.Count,
            totalCommitMs,
            commitReport.DbEntriesWritten,
            commitReport.TransactionContentDbActionsCount
        );

        _logger.LogInformation(
            "[TimeSplitsInMs: RawTxns={RawTxnPersistenceMs},Mempool={MempoolTransactionUpdateMs},TxnContentHandling={TxnContentHandlingMs},DbDependencyLoading={DbDependenciesLoadingMs},LocalActionPlanning={LocalDbContextActionsMs},DbPersistence={DbPersistanceMs}]",
            commitReport.RawTxnPersistenceMs,
            commitReport.MempoolTransactionUpdateMs,
            commitReport.TransactionContentHandlingMs,
            commitReport.DbDependenciesLoadingMs,
            commitReport.LocalDbContextActionsMs,
            commitReport.DbPersistanceMs
        );

        var committedTransactionSummary = commitReport.FinalTransaction;
        _logger.LogInformation(
            "[NewDbLedgerTip: StateVersion={LedgerStateVersion},Epoch={LedgerEpoch},IndexInEpoch={LedgerIndexInEpoch},RoundTimestamp={RoundTimestamp}]",
            committedTransactionSummary.StateVersion,
            committedTransactionSummary.Epoch,
            committedTransactionSummary.IndexInEpoch,
            committedTransactionSummary.RoundTimestamp.AsUtcIsoDateToSecondsForLogs()
        );
    }

    private record BestTransactionReport(TransactionWithTrust? Best, decimal TotalTrustAcrossAllTransactions);

    private record TransactionWithTrust(CommittedTransaction Transaction, decimal Trust);

    private BestTransactionReport FindBestTransactionAtStateVersion(long stateVersion)
    {
        var transactionsWithTrust = _transactionsByNode
            .Select(n => new
            {
                Transaction = n.Value.GetValueOrDefault(stateVersion),
                Trust = GetTrustForNode(n.Key),
            })
            .Where(x => x.Transaction != null)
            .ToList();

        if (transactionsWithTrust.Count == 0)
        {
            return new BestTransactionReport(null, 0m);
        }

        var groupedTransactions = transactionsWithTrust
            .GroupBy(t => t.Transaction!.CommittedStateIdentifier.TransactionAccumulator)
            .Select(grouping => new TransactionWithTrust(
                grouping.First().Transaction!,
                grouping.Sum(x => x.Trust)
            ))
            .ToList();

        var totalTrust = groupedTransactions.Sum(t => t.Trust);
        var topTransaction = groupedTransactions.MaxBy(t => t.Trust);

        return new BestTransactionReport(topTransaction, totalTrust);
    }

    private ConsistentLedgerExtension GenerateConsistentLedgerExtension(
        List<CommittedTransaction> transactions
    )
    {
        var transactionData = new List<CommittedTransactionData>();
        var transactionBatchParentSummary = _knownTopOfCommittedLedger!;
        var currentParentSummary = transactionBatchParentSummary;

        foreach (var transaction in transactions)
        {
            var summary = TransactionSummarisation.GenerateSummary(currentParentSummary, transaction);
            var contents = transaction.Metadata.Hex.ConvertFromHex();

            TransactionConsistency.AssertTransactionHashCorrect(contents, summary.TransactionIdentifierHash);
            TransactionConsistency.AssertChildTransactionConsistent(currentParentSummary, summary);

            transactionData.Add(new CommittedTransactionData(transaction, summary, contents));
            currentParentSummary = summary;
        }

        return new ConsistentLedgerExtension(transactionBatchParentSummary, transactionData);
    }

    private decimal GetTotalTrustWeighting()
    {
        var nodesForCalculation = Config.OnlyUseSufficientlySyncedUpNodesForQuorumCalculation
            ? TransactionNodes.Where(node => IsSufficientlySyncedUp(node.Name))
            : TransactionNodes;

        return nodesForCalculation.Sum(node => node.TrustWeighting);
    }

    private decimal GetTrustForNode(string nodeName)
    {
        return TransactionNodes.SingleOrDefault(n => n.Name == nodeName)?.TrustWeighting ?? 0m;
    }

    private bool IsSufficientlySyncedUp(string nodeName)
    {
        var ledgerTip = _latestLedgerTipByNode.GetValueOrDefault(nodeName);

        return ledgerTip != 0 &&
               (ledgerTip + Config.SufficientlySyncedThreshold) > _knownTopOfCommittedLedger!.StateVersion;
    }

    private long? GetFirstStateVersionGapForNode(string nodeName, long stateVersionExclusiveLowerBound, long stateVersionInclusiveUpperBound)
    {
        var transactionsOnRecord = GetTransactionsForNode(nodeName);

        for (var stateVersion = stateVersionExclusiveLowerBound + 1; stateVersion <= stateVersionInclusiveUpperBound; stateVersion++)
        {
            if (!transactionsOnRecord.ContainsKey(stateVersion))
            {
                return stateVersion;
            }
        }

        return null;
    }

    private ConcurrentDictionary<long, CommittedTransaction> GetTransactionsForNode(string nodeName)
    {
        return _transactionsByNode.GetOrAdd(nodeName, new ConcurrentDictionary<long, CommittedTransaction>());
    }
}
