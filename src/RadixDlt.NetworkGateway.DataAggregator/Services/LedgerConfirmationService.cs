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
using System.Diagnostics;
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
    private record CommittedTransactionWithSize(CoreModel.CommittedTransaction CommittedTransaction, int EstimatedSize);

    /* Dependencies */
    private readonly ILogger<LedgerConfirmationService> _logger;
    private readonly IOptionsMonitor<LedgerConfirmationOptions> _ledgerConfirmationOptionsMonitor;
    private readonly IOptionsMonitor<NetworkOptions> _networkOptionsMonitor;
    private readonly ISystemStatusService _systemStatusService;
    private readonly ILedgerExtenderService _ledgerExtenderService;
    private readonly IEnumerable<ILedgerConfirmationServiceObserver> _observers;
    private readonly IClock _clock;

    /* Variables */
    private readonly ConcurrentLruCache<long, byte[]> _quorumTreeHashCacheByStateVersion = new(2000);
    private readonly ConcurrentDictionary<string, long> _latestLedgerTipByNode = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, CommittedTransactionWithSize>> _transactionsByNode = new();
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

    public TransactionSummary? GetTip() => _knownTopOfCommittedLedger;

    public async Task HandleLedgerExtension(CancellationToken token)
    {
        await _observers.ForEachAsync(x => x.PreHandleLedgerExtensionIfQuorum(_clock.UtcNow));

        await LoadTopOfDbLedger(token);

        PrepareForLedgerExtensionCheck();
        var transactions = ConstructLedgerExtension();

        if (transactions.Count == 0)
        {
            return;
        }

        var consistentLedgerExtension = GenerateConsistentLedgerExtension(transactions);
        var latestSyncStatus = new SyncTargetCarrier(GetTargetStateVersion());

        var (commitReport, totalCommitMs) = await CodeStopwatch.TimeInMs(
            () => _ledgerExtenderService.CommitTransactions(consistentLedgerExtension, latestSyncStatus, token)
        );

        HandleLedgerExtensionSuccess(consistentLedgerExtension, totalCommitMs, commitReport);

        await DelayBetweenIngestionBatchesIfRequested(commitReport);
    }

    public void SubmitNodeNetworkStatus(string nodeName, long ledgerTipStateVersion, byte[] ledgerTipTreeHash)
    {
        _observers.ForEach(x => x.PreSubmitNodeNetworkStatus(nodeName, ledgerTipStateVersion));

        _latestLedgerTipByNode[nodeName] = ledgerTipStateVersion;

        if (ledgerTipStateVersion > _knownTopOfCommittedLedger?.StateVersion)
        {
            // We handle consistency checks ahead of the commit point in the ConstructQuorumLedgerExtension method
            return;
        }

        var cachedTreeHash = _quorumTreeHashCacheByStateVersion.GetOrDefault(ledgerTipStateVersion);

        if (cachedTreeHash == null)
        {
            // Ledger Tip is too far behind -- so don't report on consistency.
            // We could change this to do a database look-up in future to give a consistency check.
            _observers.ForEach(x => x.SubmitNodeNetworkStatusUnknown(nodeName, ledgerTipStateVersion));
        }
        else if (cachedTreeHash.BytesAreEqual(ledgerTipTreeHash))
        {
            _observers.ForEach(x => x.SubmitNodeNetworkStatusUpToDate(nodeName, ledgerTipStateVersion));
        }
        else
        {
            _observers.ForEach(x => x.SubmitNodeNetworkStatusOutOfDate(nodeName, ledgerTipStateVersion));
        }
    }

    public void SubmitTransactionsFromNode(string nodeName, List<CoreModel.CommittedTransaction> transactions, int responseSize)
    {
        if (!_latestLedgerTipByNode.ContainsKey(nodeName))
        {
            throw new InvalidNodeStateException("The node's ledger tip must be written first");
        }

        if (!transactions.Any())
        {
            return;
        }

        var transactionStoreForNode = GetTransactionsForNode(nodeName);
        var averageTransactionSize = responseSize / transactions.Count;

        foreach (var transaction in transactions)
        {
            transactionStoreForNode[transaction.ResultantStateIdentifiers.StateVersion] = new CommittedTransactionWithSize(transaction, averageTransactionSize);
        }
    }

    public TransactionsRequested? GetWhichTransactionsAreRequestedFromNode(string nodeName)
    {
        var currentTopOfLedger = _knownTopOfCommittedLedger;

        if (currentTopOfLedger == null)
        {
            return null;
        }

        var transactionsOnRecord = GetTransactionsForNode(nodeName);
        var exclusiveLowerBound = currentTopOfLedger.StateVersion;
        var inclusiveUpperBound = currentTopOfLedger.StateVersion + Config.MaxTransactionPipelineSizePerNode;
        long? firstMissingStateVersionGap = null;
        long accumulatedEstimatedSize = 0;

        for (var stateVersion = exclusiveLowerBound + 1; stateVersion <= inclusiveUpperBound; stateVersion++)
        {
            if (transactionsOnRecord.TryGetValue(stateVersion, out var committedTransactionWithSize))
            {
                accumulatedEstimatedSize += committedTransactionWithSize.EstimatedSize;

                if (accumulatedEstimatedSize > Config.MaxEstimatedTransactionPipelineByteSizePerNode)
                {
                    return null;
                }
            }
            else
            {
                firstMissingStateVersionGap = stateVersion;

                break;
            }
        }

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
        TransactionNodes = _networkOptionsMonitor
            .CurrentValue
            .CoreApiNodes
            .Where(n => n.Enabled && !n.DisabledForTransactionIndexing)
            .ToList();

        Config = _ledgerConfirmationOptionsMonitor.CurrentValue;
    }

    private List<CoreModel.CommittedTransaction> ConstructLedgerExtension()
    {
        var ledgerExtension = new List<CoreModel.CommittedTransaction>();

        var startStateVersion = _knownTopOfCommittedLedger!.StateVersion + 1;

        for (var stateVersion = startStateVersion; stateVersion < startStateVersion + Config.MaxCommitBatchSize; stateVersion++)
        {
            var chosenTransaction = GetTransactionFromRandomNode(stateVersion);
            ledgerExtension.Add(chosenTransaction);
        }

        return ledgerExtension;
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
        UpdateRecordsOfTopOfLedger(commitReport.FinalTransaction);
        ReportOnLedgerExtensionSuccess(ledgerExtension, totalCommitMs, commitReport);
        RemoveProcessedTransactions(commitReport.FinalTransaction.StateVersion);
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
        var currentTimestamp = _clock.UtcNow;
        var committedTransactionSummary = commitReport.FinalTransaction;

        _observers.ForEach(x =>
            x.ReportOnLedgerExtensionSuccess(currentTimestamp, currentTimestamp - ledgerExtension.LatestTransactionSummary.RoundTimestamp, totalCommitMs, commitReport.TransactionsCommittedCount));

        _logger.LogInformation(
            "Committed {TransactionCount} transactions to the DB in {TotalCommitTransactionsMs}ms [EntitiesTouched={DbEntriesWritten}]",
            ledgerExtension.CommittedTransactions.Count,
            totalCommitMs,
            commitReport.DbEntriesTouched
        );

        _logger.LogInformation(
            "[TimeSplitsInMs: TxnContentHandling={TxnContentHandlingMs},DbDependencyLoading={DbDependenciesLoadingMs},DbPersistence={DbPersistanceMs}]",
            commitReport.ContentHandlingMs,
            commitReport.DbDependenciesLoadingMs,
            commitReport.DbPersistenceMs
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
            _quorumTreeHashCacheByStateVersion.Set(
                committedTransaction.ResultantStateIdentifiers.StateVersion,
                committedTransaction.ResultantStateIdentifiers.GetTransactionTreeHashBytes()
            );
        }
    }

    private void UpdateRecordsOfTopOfLedger(TransactionSummary topOfLedger)
    {
        _knownTopOfCommittedLedger = topOfLedger;

        _observers.ForEach(x => x.RecordTopOfDbLedger(topOfLedger.StateVersion, topOfLedger.RoundTimestamp));

        _systemStatusService.SetTopOfDbLedgerNormalizedRoundTimestamp(topOfLedger.NormalizedRoundTimestamp);
    }

    private void RemoveProcessedTransactions(long committedStateVersion)
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

    private CoreModel.CommittedTransaction GetTransactionFromRandomNode(long stateVersion)
    {
        var nodeTransactions = _transactionsByNode
            .Where(x => x.Value.ContainsKey(stateVersion))
            .Select(x => x.Value)
            .FirstOrDefault();

        if (nodeTransactions == null || !nodeTransactions.TryGetValue(stateVersion, out var transaction))
        {
            throw new UnreachableException($"Transaction with state version: {stateVersion} not found.");
        }

        return transaction.CommittedTransaction;
    }

    private ConsistentLedgerExtension GenerateConsistentLedgerExtension(List<CoreModel.CommittedTransaction> transactions)
    {
        var transactionBatchParentSummary = _knownTopOfCommittedLedger!;
        var previousStateVersion = transactionBatchParentSummary.StateVersion;

        try
        {
            foreach (var transaction in transactions)
            {
                TransactionConsistency.AssertChildTransactionConsistent(previousStateVersion: previousStateVersion, stateVersion: transaction.ResultantStateIdentifiers.StateVersion);
                previousStateVersion = transaction.ResultantStateIdentifiers.StateVersion;
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

    private ConcurrentDictionary<long, CommittedTransactionWithSize> GetTransactionsForNode(string nodeName)
    {
        return _transactionsByNode.GetOrAdd(nodeName, new ConcurrentDictionary<long, CommittedTransactionWithSize>());
    }
}
