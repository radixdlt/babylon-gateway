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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Exceptions;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

/// <summary>
/// This service is responsible for controlling the NodeTransactionLogWorkers, and deciding on / committing when
/// a quorum is reached.
/// </summary>
public sealed class LedgerConfirmationService : ILedgerConfirmationService
{
    private static readonly LogLimiter _noExtensionLogLimiter = new(TimeSpan.FromSeconds(5), LogLevel.Warning, LogLevel.Debug);

    /* Dependencies */
    private readonly ILogger<LedgerConfirmationService> _logger;
    private readonly IOptionsMonitor<LedgerConfirmationOptions> _ledgerConfirmationOptionsMonitor;
    private readonly IOptionsMonitor<NetworkOptions> _networkOptionsMonitor;
    private readonly ISystemStatusService _systemStatusService;
    private readonly ILedgerExtenderService _ledgerExtenderService;
    private readonly IEnumerable<ILedgerConfirmationServiceObserver> _observers;
    private readonly IClock _clock;

    /* Variables */
    private readonly ConcurrentLruCache<long, byte[]> _quorumAccumulatorCacheByStateVersion = new(2000); // TODO not sure how this suits Babylon
    private readonly ConcurrentDictionary<string, long> _latestLedgerTipByNode = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, CoreModel.CommittedTransaction>> _transactionsByNode = new();
    private TransactionSummary? _knownTopOfCommittedLedger;

    private IList<CoreApiNode> TransactionNodes { get; set; } = new List<CoreApiNode>();

    private LedgerConfirmationOptions Config { get; set; }

    public LedgerConfirmationService(
        ILogger<LedgerConfirmationService> logger,
        IOptionsMonitor<LedgerConfirmationOptions> ledgerConfirmationOptionsMonitor,
        IOptionsMonitor<NetworkOptions> networkOptionsMonitor,
        ISystemStatusService systemStatusService,
        ILedgerExtenderService ledgerExtenderService,
        IEnumerable<ILedgerConfirmationServiceObserver> observers,
        IClock clock)
    {
        _logger = logger;
        _ledgerConfirmationOptionsMonitor = ledgerConfirmationOptionsMonitor;
        _networkOptionsMonitor = networkOptionsMonitor;
        _systemStatusService = systemStatusService;
        _ledgerExtenderService = ledgerExtenderService;
        _observers = observers;
        _clock = clock;

        _observers.ForEach(x => x.ResetQuorum());

        Config = _ledgerConfirmationOptionsMonitor.CurrentValue;
    }

    /// <summary>
    /// To be run by the (single-threaded) ledger extender worker.
    /// </summary>
    public async Task HandleLedgerExtensionIfQuorum(CancellationToken token)
    {
        await _observers.ForEachAsync(x => x.PreHandleLedgerExtensionIfQuorum(_clock.UtcNow));

        await LoadTopOfDbLedger(token);

        PrepareForLedgerExtensionCheck();
        var transactions = ConstructQuorumLedgerExtension();

        if (transactions.Count == 0)
        {
            return;
        }

        var ledgerExtension = GenerateConsistentLedgerExtension(transactions);
        var latestSyncStatus = new SyncTargetCarrier(GetTargetStateVersion());

        var (commitReport, totalCommitMs) = await CodeStopwatch.TimeInMs(
            () => _ledgerExtenderService.CommitTransactions(ledgerExtension, latestSyncStatus, token)
        );

        HandleLedgerExtensionSuccess(ledgerExtension, totalCommitMs, commitReport);

        await DelayBetweenIngestionBatchesIfRequested(commitReport);
    }

    /// <summary>
    /// To be called from the node worker.
    /// </summary>
    public void SubmitNodeNetworkStatus(string nodeName, long ledgerTipStateVersion, byte[] ledgerTipAccumulator, long targetStateVersion)
    {
        _observers.ForEach(x => x.PreSubmitNodeNetworkStatus(nodeName, ledgerTipStateVersion, targetStateVersion));

        _latestLedgerTipByNode[nodeName] = ledgerTipStateVersion;

        if (ledgerTipStateVersion > _knownTopOfCommittedLedger?.StateVersion)
        {
            // We handle consistency checks ahead of the commit point in the ConstructQuorumLedgerExtension method
            return;
        }

        var cachedAccumulator = _quorumAccumulatorCacheByStateVersion.GetOrDefault(ledgerTipStateVersion);

        if (cachedAccumulator == null)
        {
            // Ledger Tip is too far behind -- so don't report on consistency.
            // We could change this to do a database look-up in future to give a consistency check.

            _observers.ForEach(x => x.SubmitNodeNetworkStatusUnknown(nodeName, ledgerTipStateVersion, targetStateVersion));
        }
        else if (cachedAccumulator.BytesAreEqual(ledgerTipAccumulator))
        {
            _observers.ForEach(x => x.SubmitNodeNetworkStatusUpToDate(nodeName, ledgerTipStateVersion, targetStateVersion));
        }
        else
        {
            _observers.ForEach(x => x.SubmitNodeNetworkStatusOutOfDate(nodeName, ledgerTipStateVersion, targetStateVersion));
        }
    }

    /// <summary>
    /// To be called from the node worker.
    /// </summary>
    public void SubmitTransactionsFromNode(string nodeName, List<CoreModel.CommittedTransaction> transactions)
    {
        if (!_latestLedgerTipByNode.ContainsKey(nodeName))
        {
            throw new InvalidNodeStateException("The node's ledger tip must be written first");
        }

        var transactionStoreForNode = GetTransactionsForNode(nodeName);

        foreach (var transaction in transactions)
        {
            transactionStoreForNode[transaction.StateVersion] = transaction;
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

        if (firstMissingStateVersionGap == null || firstMissingStateVersionGap >= inclusiveUpperBound)
        {
            return null;
        }

        return new TransactionsRequested(firstMissingStateVersionGap.Value, inclusiveUpperBound);
    }

    private void PrepareForLedgerExtensionCheck()
    {
        // We persist these to avoid excessive config load allocations;
        // but update them at the start of each loop in case the config has changed
        TransactionNodes = _networkOptionsMonitor.CurrentValue.CoreApiNodes
            .Where(n => n.Enabled && !n.DisabledForTransactionIndexing)
            .ToList();

        Config = _ledgerConfirmationOptionsMonitor.CurrentValue;
    }

    private List<CoreModel.CommittedTransaction> ConstructQuorumLedgerExtension()
    {
        var extension = new List<CoreModel.CommittedTransaction>();

        var startStateVersion = _knownTopOfCommittedLedger!.StateVersion + 1;
        var trustRequirements = ComputeTrustWeightingRequirements();

        if (trustRequirements.TrustWeightingAvailableAcrossAllNodes == 0)
        {
            _logger.LogWarning("Total trust weighting across all nodes is zero - perhaps no nodes are configured for transaction reading?");
            return extension;
        }

        if (trustRequirements.TrustWeightingRequiredForQuorumAtPresentTime == 0)
        {
            var logLevel = _systemStatusService.IsInStartupGracePeriod() ? LogLevel.Debug : _noExtensionLogLimiter.GetLogLevel();
            _logger.Log(logLevel, "Total trust weighting required for extension is zero - likely the system is either yet to read from nodes, or none of the nodes are close enough to synced up");
            return extension;
        }

        for (var stateVersion = startStateVersion; stateVersion < startStateVersion + Config.MaxCommitBatchSize; stateVersion++)
        {
            var (chosenTransaction, trustAlreadyCommittedByNodes, inconsistentNodeNames) = FindMostTrustedTransaction(stateVersion);

            var haveQuorum = chosenTransaction != null && chosenTransaction.Trust >= trustRequirements.TrustWeightingRequiredForQuorumAtPresentTime;

            if (haveQuorum)
            {
                foreach (var inconsistentNodeName in inconsistentNodeNames)
                {
                    _observers.ForEach(x => x.LedgerTipInconsistentWithQuorumStatus(inconsistentNodeName));
                }

                foreach (var consistentNodeName in chosenTransaction!.NodeNames)
                {
                    _observers.ForEach(x => x.LedgerTipConsistentWithQuorumStatus(consistentNodeName));
                }

                extension.Add(chosenTransaction.Transaction);
                continue;
            }

            /*
             * It's likely we simply just don't have enough nodes contributing yet.
             * But, if it's not possible to reach quorum even with more nodes contributing - then we need to worry.
             * So let's try a couple of possibilities
             */
            var trustWeightingOfBestTransaction = chosenTransaction?.Trust ?? 0m;

            var remainingTrustPossibleFromSufficientlySyncedNodes =
                trustRequirements.TrustWeightingOfSufficientlySyncedUpNodes - trustAlreadyCommittedByNodes;

            if (
                trustWeightingOfBestTransaction + remainingTrustPossibleFromSufficientlySyncedNodes <
                trustRequirements.TrustWeightingRequiredForQuorumAtPresentTime
            )
            {
                // We can't make headway with all sufficiently synced up nodes - let's set this to Unknown until
                // we get more nodes synced up...
                _observers.ForEach(x => x.UnknownQuorumStatus());
            }

            var remainingTrustPossibleFromAllNodes =
                trustRequirements.TrustWeightingAvailableAcrossAllNodes - trustAlreadyCommittedByNodes;

            if (
                trustWeightingOfBestTransaction + remainingTrustPossibleFromAllNodes <
                trustRequirements.TrustWeightingRequiredForQuorumIfAllNodesAvailableForQuorum
            )
            {
                // Even with all nodes synced up, we wouldn't reach a quorum - mark this as a critical alarm!
                _observers.ForEach(x => x.QuorumLost());
            }

            break;
        }

        if (extension.Count > 0)
        {
            _observers.ForEach(x => x.QuorumGained());
        }

        return extension;
    }

    private async Task LoadTopOfDbLedger(CancellationToken token)
    {
        var (topOfLedger, readTopOfLedgerMs) = await CodeStopwatch.TimeInMs(
            () => _ledgerExtenderService.GetLatestTransactionSummary(token)
        );
        UpdateRecordsOfTopOfLedger(topOfLedger);
        _logger.LogDebug(
            "Top of DB ledger is at state version {StateVersion} (read in {ReadTopOfLedgerMs}ms)",
            topOfLedger.StateVersion,
            readTopOfLedgerMs
        );
    }

    private void HandleLedgerExtensionSuccess(ConsistentLedgerExtension ledgerExtension, long totalCommitMs, CommitTransactionsReport commitReport)
    {
        AddAccumulatorsToCache(ledgerExtension);
        ReportOnLedgerExtensionSuccess(ledgerExtension, totalCommitMs, commitReport);

        // NB - this must come after UpdateTopOfLedgerVariable so that the nodes don't try to fill the gap that's
        //      created when we remove the transactions below it
        StopTrackingTransactionsUpToStateVersion(commitReport.FinalTransaction.StateVersion);
    }

    private async Task DelayBetweenIngestionBatchesIfRequested(CommitTransactionsReport commitReport)
    {
        var isDelayEnabled = Config.DelayBetweenLargeBatches.TotalMilliseconds > 0;
        var isLargeBatch = commitReport.TransactionsCommittedCount >= Config.LargeBatchSizeToAddDelay;

        if (!isDelayEnabled || !isLargeBatch)
        {
            return;
        }

        _logger.LogInformation(
            "Enforcing delay of {DelayMs}ms due to the size of the ingestion batch",
            Config.DelayBetweenLargeBatches.TotalMilliseconds
        );
        await Task.Delay(Config.DelayBetweenLargeBatches);
    }

    private void ReportOnLedgerExtensionSuccess(ConsistentLedgerExtension ledgerExtension, long totalCommitMs, CommitTransactionsReport commitReport)
    {
        _systemStatusService.RecordTransactionsCommitted();

        UpdateRecordsOfTopOfLedger(commitReport.FinalTransaction);

        var currentTimestamp = _clock.UtcNow;
        var committedTransactionSummary = commitReport.FinalTransaction;

        _observers.ForEach(x => x.ReportOnLedgerExtensionSuccess(currentTimestamp, currentTimestamp - ledgerExtension.LatestTransactionSummary.RoundTimestamp, totalCommitMs, commitReport.TransactionsCommittedCount));

        _logger.LogInformation(
            "Committed {TransactionCount} transactions to the DB in {TotalCommitTransactionsMs}ms [EntitiesTouched={DbEntriesWritten}]",
            ledgerExtension.CommittedTransactions.Count,
            totalCommitMs,
            commitReport.DbEntriesWritten
        );

        _logger.LogInformation(
            "[TimeSplitsInMs: RawTxns={RawTxnPersistenceMs},Mempool={MempoolTransactionUpdateMs},TxnContentHandling={TxnContentHandlingMs},DbDependencyLoading={DbDependenciesLoadingMs},DbPersistence={DbPersistanceMs}]",
            commitReport.RawTxnPersistenceMs,
            commitReport.MempoolTransactionUpdateMs,
            commitReport.TransactionContentHandlingMs,
            commitReport.DbDependenciesLoadingMs,
            commitReport.DbPersistanceMs
        );

        _logger.LogInformation(
            "[NewDbLedgerTip: StateVersion={LedgerStateVersion},Epoch={LedgerEpoch},IndexInEpoch={LedgerIndexInEpoch},RoundTimestamp={RoundTimestamp}]",
            committedTransactionSummary.StateVersion,
            committedTransactionSummary.Epoch,
            committedTransactionSummary.IndexInEpoch,
            committedTransactionSummary.RoundTimestamp.AsUtcIsoDateToSecondsForLogs()
        );
    }

    private void AddAccumulatorsToCache(ConsistentLedgerExtension ledgerExtension)
    {
        foreach (var committedTransaction in ledgerExtension.CommittedTransactions)
        {
            _quorumAccumulatorCacheByStateVersion.Set(
                committedTransaction.StateVersion,
                committedTransaction.AccumulatorHash.ConvertFromHex()
            );
        }
    }

    private void UpdateRecordsOfTopOfLedger(TransactionSummary topOfLedger)
    {
        _knownTopOfCommittedLedger = topOfLedger;

        _observers.ForEach(x => x.RecordTopOfDbLedger(topOfLedger.StateVersion, topOfLedger.RoundTimestamp));

        _systemStatusService.SetTopOfDbLedgerNormalizedRoundTimestamp(topOfLedger.NormalizedRoundTimestamp);
    }

    private void StopTrackingTransactionsUpToStateVersion(long committedStateVersion)
    {
        foreach (var (_, transactions) in _transactionsByNode)
        {
            var stateVersionsToRemove = transactions.Keys.Where(k => k <= committedStateVersion).ToList();
            foreach (var stateVersion in stateVersionsToRemove)
            {
                transactions.TryRemove(stateVersion, out _);
            }
        }
    }

    private record MostTrustedTransactionReport(
        TransactionClaim? Best,
        decimal TotalTrustCommittedByNodes,
        List<string> InconsistentNodeNames
    );

    private record TransactionClaim(CoreModel.CommittedTransaction Transaction, List<string> NodeNames, decimal Trust);

    private MostTrustedTransactionReport FindMostTrustedTransaction(long stateVersion)
    {
        var transactionsWithTrust = _transactionsByNode
            .Select(n => new
            {
                NodeName = n.Key,
                Transaction = n.Value.GetValueOrDefault(stateVersion),
                Trust = GetTrustForNode(n.Key),
            })
            .Where(x => x.Transaction != null)
            .ToList();

        if (transactionsWithTrust.Count == 0)
        {
            return new MostTrustedTransactionReport(null, 0m, new List<string>());
        }

        var groupedTransactions = transactionsWithTrust
            .GroupBy(t => t.Transaction!.StateVersion)
            .Select(grouping => new TransactionClaim(
                grouping.First().Transaction!,
                grouping.Select(g => g.NodeName).ToList(),
                grouping.Sum(x => x.Trust)
            ))
            .ToList();

        var totalTrust = groupedTransactions.Sum(t => t.Trust);
        var orderedTransactionClaims = groupedTransactions.OrderByDescending(t => t.Trust).ToList();
        var topTransaction = orderedTransactionClaims.First();

        var inconsistentNodeNames = orderedTransactionClaims.Skip(1)
            .SelectMany(t => t.NodeNames)
            .ToList();

        return new MostTrustedTransactionReport(topTransaction, totalTrust, inconsistentNodeNames);
    }

    private ConsistentLedgerExtension GenerateConsistentLedgerExtension(List<CoreModel.CommittedTransaction> transactions)
    {
        var transactionBatchParentSummary = _knownTopOfCommittedLedger!;
        var previousStateVersion = transactionBatchParentSummary.StateVersion;

        try
        {
            foreach (var transaction in transactions)
            {
                TransactionConsistency.AssertChildTransactionConsistent(previousStateVersion, transaction.StateVersion);

                if (transaction.LedgerTransaction.ActualInstance is CoreModel.UserLedgerTransaction ult)
                {
                    TransactionConsistency.AssertTransactionHashCorrect(ult.NotarizedTransaction.PayloadHex.ConvertFromHex(), ult.NotarizedTransaction.Hash.ConvertFromHex());
                }

                previousStateVersion = transaction.StateVersion;
            }

            _observers.ForEach(x => x.QuorumExtensionConsistentGained());
        }
        catch (InvalidLedgerCommitException)
        {
            _observers.ForEach(x => x.QuorumExtensionConsistentLost());
            throw;
        }
        catch (InconsistentLedgerException)
        {
            _observers.ForEach(x => x.QuorumExtensionConsistentLost());
            throw;
        }

        return new ConsistentLedgerExtension(transactionBatchParentSummary, transactions);
    }

    private long GetTargetStateVersion()
    {
        var ledgerTips = _latestLedgerTipByNode.Values.ToList();

        if (ledgerTips.Count == 0)
        {
            throw new InvalidNodeStateException("At least one ledger tip must have been submitted");
        }

        return ledgerTips.Max();
    }

    public sealed record TrustWeightingReport(
        int TotalTransactionNodes,
        int TotalSufficientlySyncedUpNodes,
        decimal TrustWeightingAvailableAcrossAllNodes,
        decimal TrustWeightingOfSufficientlySyncedUpNodes,
        decimal TrustWeightingRequiredForQuorumAtPresentTime,
        decimal TrustWeightingRequiredForQuorumIfAllNodesAvailableForQuorum
    );

    private TrustWeightingReport ComputeTrustWeightingRequirements()
    {
        var sufficientlySyncedUpNodes = TransactionNodes
            .Where(node => IsSufficientlySyncedUp(node.Name))
            .ToList();

        var sufficientlySyncedUpNodesTrustWeighting = sufficientlySyncedUpNodes.Sum(node => node.TrustWeighting);
        var trustWeightingAcrossAllNodes = TransactionNodes.Sum(node => node.TrustWeighting);

        var trustWeightingTotalUsedForQuorumCalculation = Config.OnlyUseSufficientlySyncedUpNodesForQuorumCalculation
            ? sufficientlySyncedUpNodesTrustWeighting
            : trustWeightingAcrossAllNodes;

        var trustWeightingForQuorum = Config.CommitRequiresNodeQuorumTrustProportion * trustWeightingTotalUsedForQuorumCalculation;
        var trustWeightingForQuorumIfAllSyncedUp = Config.CommitRequiresNodeQuorumTrustProportion * trustWeightingAcrossAllNodes;

        var report = new TrustWeightingReport(
            TotalTransactionNodes: TransactionNodes.Count,
            TotalSufficientlySyncedUpNodes: sufficientlySyncedUpNodes.Count,
            TrustWeightingAvailableAcrossAllNodes: trustWeightingAcrossAllNodes,
            TrustWeightingOfSufficientlySyncedUpNodes: sufficientlySyncedUpNodesTrustWeighting,
            TrustWeightingRequiredForQuorumAtPresentTime: trustWeightingForQuorum,
            TrustWeightingRequiredForQuorumIfAllNodesAvailableForQuorum: trustWeightingForQuorumIfAllSyncedUp
        );

        _observers.ForEach(x => x.TrustWeightingRequirementsComputed(report));

        return report;
    }

    private decimal GetTrustForNode(string nodeName)
    {
        return TransactionNodes.SingleOrDefault(n => n.Name == nodeName)?.TrustWeighting ?? 0m;
    }

    private bool IsSufficientlySyncedUp(string nodeName)
    {
        var ledgerTip = _latestLedgerTipByNode.GetValueOrDefault(nodeName);

        return ledgerTip != 0 &&
               (ledgerTip + Config.SufficientlySyncedStateVersionThreshold) > _knownTopOfCommittedLedger!.StateVersion;
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

    private ConcurrentDictionary<long, CoreModel.CommittedTransaction> GetTransactionsForNode(string nodeName)
    {
        return _transactionsByNode.GetOrAdd(nodeName, new ConcurrentDictionary<long, CoreModel.CommittedTransaction>());
    }
}
