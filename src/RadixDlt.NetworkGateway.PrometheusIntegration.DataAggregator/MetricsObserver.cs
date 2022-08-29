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

using Prometheus;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Common.Exceptions;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.DataAggregator.Workers.GlobalWorkers;
using RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PrometheusIntegration;

internal class MetricsObserver :
    IGlobalWorkerObserver,
    INodeWorkerObserver,
    ILedgerConfirmationServiceObserver,
    IMempoolPrunerServiceObserver,
    IMempoolResubmissionServiceObserver,
    IAggregatorHealthCheckObserver,
    ISystemStatusServiceObserver,
    INodeInitializerObserver,
    IMempoolTrackerServiceObserver,
    INodeTransactionLogWorkerObserver,
    INodeMempoolTransactionIdsReaderWorkerObserver,
    INodeMempoolFullTransactionReaderWorkerObserver,
    IRawTransactionWriterObserver,
    INetworkConfigurationReaderObserver,
    INetworkStatusReaderObserver,
    ITransactionLogReaderObserver
{
    private static readonly Counter _globalWorkerErrorsCount = Metrics
        .CreateCounter(
            "ng_workers_global_error_count",
            "Number of errors in global workers.",
            new CounterConfiguration { LabelNames = new[] { "worker", "error", "type" } }
        );

    private static readonly Counter _nodeWorkerErrorsCount = Metrics
        .CreateCounter(
            "ng_workers_node_error_count",
            "Number of errors in node workers.",
            new CounterConfiguration { LabelNames = new[] { "worker", "node", "error", "type" } }
        );

    /* Global Metrics - Quorum/Sync related - ie ledger_node prefix */
    private static readonly Gauge _quorumExistsStatus = Metrics
        .CreateGauge(
            "ng_ledger_sync_quorum_exists_status",
            "Whether enough nodes agree to continue committing transaction to the DB. 1 = true, 0.5 = unknown, 0 = false. (if 0, it's a critical alarm)."
        );

    private static readonly Gauge _quorumExtensionConsistentStatus = Metrics
        .CreateGauge(
            "ng_ledger_sync_quorum_extension_consistent_status",
            "If a node quorum exists for a ledger extension, whether it agrees with the existing DB accumulator and is internally consistent. 1 = true, 0.5 = unknown, 0 = false. If 0, it's a critical alarm."
        );

    private static readonly Gauge _sufficientlySyncedUpNodesTotal = Metrics
        .CreateGauge(
            "ng_ledger_sync_sufficiently_synced_up_nodes_total",
            "The number of nodes which are sufficiently synced up."
        );

    private static readonly Gauge _sufficientlySyncedUpNodesTrustWeightingTotal = Metrics
        .CreateGauge(
            "ng_ledger_sync_sufficiently_synced_up_nodes_trust_weighting_total",
            "The trust weighting of all nodes which are currently sufficiently synced up"
        );

    private static readonly Gauge _configuredNodesTotal = Metrics
        .CreateGauge(
            "ng_ledger_sync_configured_nodes_total",
            "The number of nodes which are configured for transaction syncing."
        );

    private static readonly Gauge _configuredNodesTrustWeightingTotal = Metrics
        .CreateGauge(
            "ng_ledger_sync_configured_nodes_trust_weighting_total",
            "The trust weighting of all nodes which are currently configured for transaction syncing."
        );

    private static readonly Gauge _ledgerNodeTrustWeightingRequiredForQuorum = Metrics
        .CreateGauge(
            "ng_ledger_sync_trust_weighting_required_for_quorum_total",
            "The trust weighting currently required for quorum"
        );

    private static readonly Gauge _ledgerNodeTrustWeightingRequiredForQuorumIfAllNodesSufficientlySynced = Metrics
        .CreateGauge(
            "ng_ledger_sync_trust_weighting_required_for_quorum_if_all_nodes_sufficiently_synced_total",
            "The trust weighting required for quorum, if/once all nodes are synced up"
        );

    /* Global Metrics - Quorum/Sync related - ie ledger_commit prefix */

    private static readonly Histogram _batchCommitTimeSeconds = Metrics
        .CreateHistogram(
            "ng_ledger_commit_batch_commit_time_seconds",
            "Total time to commit a batch of transactions.",
            new HistogramConfiguration { Buckets = Histogram.LinearBuckets(start: 0.2, width: 0.2, count: 100) }
        );

    private static readonly Counter _ledgerCommittedTransactionsCount = Metrics
        .CreateCounter(
            "ng_ledger_commit_committed_transactions_count",
            "Count of committed transactions."
        );

    private static readonly Gauge _ledgerLastCommitTimestamp = Metrics
        .CreateGauge(
            "ng_ledger_commit_last_commit_timestamp_seconds",
            "Unix timestamp of the last DB ledger commit (in seconds, to millisecond precision)."
        );

    private static readonly Gauge _ledgerLastExtensionAttemptStartTimestamp = Metrics
        .CreateGauge(
            "ng_ledger_commit_last_ledger_extension_attempt_start_timestamp_seconds",
            "Unix timestamp of the start of the last attempt to extend the ledger (in seconds, to millisecond precision)."
        );

    private static readonly Gauge _peakLedgerLagBeforeLastCommit = Metrics
        .CreateGauge(
            "ng_ledger_commit_peak_round_timestamp_data_aggregator_clock_delay_before_last_commit_seconds",
            "The worst delay measured between the DB and the round timestamp at last commit (in seconds, to millisecond precision)."
        );

    private static readonly Gauge _ledgerStateVersion = Metrics
        .CreateGauge(
            "ng_ledger_commit_tip_state_version",
            "The state version of the top of the DB ledger."
        );

    private static readonly Gauge _ledgerUnixRoundTimestamp = Metrics
        .CreateGauge(
            "ng_ledger_commit_tip_round_unix_timestamp_seconds",
            "Unix timestamp of the round at the top of the DB ledger (in seconds, to millisecond precision)."
        );

    /* Per-Node Metrics */
    private static readonly Gauge _nodeLedgerTipStateVersion = Metrics
        .CreateGauge(
            "ng_node_ledger_tip_state_version",
            "The state version at the tip of the node's ledger.",
            new GaugeConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Gauge _nodeLedgerTargetStateVersion = Metrics
        .CreateGauge(
            "ng_node_ledger_target_state_version",
            "The state version which the node reports as the highest seen on the network.",
            new GaugeConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Gauge _nodeLedgerTipIsConsistentWithQuorumStatus = Metrics
        .CreateGauge(
            "ng_node_ledger_tip_is_consistent_with_quorum_status",
            "If the node's ledger tip is consistent with the committed quorum. 1 = true, 0.5 = unknown, 0 = false. If 0, this is an important warning alarm - this node will need to be fixed.",
            new GaugeConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Gauge _mempoolDbSizeByStatus = Metrics
        .CreateGauge(
            "ng_db_mempool_size_by_status_total",
            "Number of transactions currently tracked in the MempoolTransaction table, by status.",
            new GaugeConfiguration { LabelNames = new[] { "status" } }
        );

    private static readonly Counter _mempoolTransactionsPrunedCount = Metrics
        .CreateCounter(
            "ng_db_mempool_pruned_transactions_count",
            "Count of mempool transactions pruned from the DB"
        );

    private static readonly Gauge _resubmissionQueueSize = Metrics
        .CreateGauge(
            "ng_db_mempool_transactions_needing_resubmission_total",
            "Current number of transactions which have dropped out of mempools and need resubmitting."
        );

    private static readonly Counter _dbMempoolTransactionsMarkedAsResolvedButUnknownStatusCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_marked_resolved_but_unknown_status_count",
            "Number of mempool transactions marked as resolved but with an as-yet-unknown status during resubmission"
        );

    private static readonly Counter _dbMempoolTransactionsMarkedAsFailedDuringResubmissionCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_marked_failed_during_resubmission_count",
            "Number of mempool transactions marked as failed due to error during resubmission"
        );

    private static readonly Counter _dbMempoolTransactionsMarkedAsAssumedInNodeMempoolAfterResubmissionCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_assumed_in_node_mempool_after_resubmission_count",
            "Number of mempool transactions marked as InNodeMempool after resubmission"
        );

    private static readonly Counter _dbTransactionsMarkedAsFailedForTimeoutCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_marked_as_failed_for_timeout_count",
            "Number of mempool transactions in the DB marked as failed due to timeout as they won't be resubmitted"
        );

    private static readonly Counter _transactionResubmissionAttemptCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_resubmission_attempt_count",
            "Number of transaction resubmission attempts"
        );

    private static readonly Counter _transactionResubmissionSuccessCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_resubmission_success_count",
            "Number of transaction resubmission successes"
        );

    private static readonly Counter _transactionResubmissionErrorCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_resubmission_error_count",
            "Number of transaction resubmission errors"
        );

    private static readonly Counter _transactionResubmissionResolutionByResultCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_resubmission_resolution_count",
            "Number of various resolutions of transaction resubmissions",
            new CounterConfiguration { LabelNames = new[] { "result" } }
        );

    private static readonly Gauge _aggregatorIsUnhealthy = Metrics
        .CreateGauge(
            "ng_aggregator_is_unhealthy_info",
            "0 if the aggregator is healthy (has committed recently or is not the primary), 1 if it's unhealthy [as of the last health check]."
        );

    private static readonly Gauge _aggregatorIsPrimary = Metrics
        .CreateGauge(
            "ng_aggregator_is_primary_info",
            "0 if the aggregator is not the write primary, 1 if it is the write primary [as of the last health check]."
        );

    private static readonly Gauge _isPrimaryStatus = Metrics
        .CreateGauge(
            "ng_aggregator_is_primary_status",
            "1 if primary, 0 if secondary."
        );

    // NB - The namespace and choice of tag "worker" is so that it fits into the same metric namespace, and
    // aligns with the metrics in NodeWorker and GlobalWorker
    private static readonly Counter _nodeInitializersErrorsCount = Metrics
        .CreateCounter(
            "ng_workers_node_initializers_error_count",
            "Number of errors in node initializers.",
            new CounterConfiguration { LabelNames = new[] { "worker", "node", "error", "type" } }
        );

    private static readonly Gauge _combinedMempoolCurrentSizeTotal = Metrics
        .CreateGauge(
            "ng_node_mempool_combined_current_size_total",
            "Number of transactions seen currently in any node mempool."
        );

    private static readonly Counter _dbTransactionsAddedDueToNodeMempoolAppearanceCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_added_from_node_mempool_count",
            "Number of mempool transactions added to the DB due to appearing in a node mempool"
        );

    private static readonly Counter _dbTransactionsMarkedAsMissingCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_marked_as_missing_count",
            "Number of mempool transactions in the DB marked as missing"
        );

    private static readonly Counter _dbTransactionsReappearedCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_reappeared_count",
            "Number of mempool transactions in the DB which were marked as missing but now appear in a mempool again"
        );

    private static readonly Counter _failedFetchLoopsUnlabeled = Metrics
        .CreateCounter(
            "ng_node_fetch_transaction_batch_loop_error_total",
            "Number of fetch loop errors that failed.",
            new CounterConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Histogram _totalFetchTimeSecondsUnlabeled = Metrics
        .CreateHistogram(
            "ng_node_fetch_transaction_batch_time_seconds",
            "Total time to fetch a batch of transactions.",
            new HistogramConfiguration
            {
                LabelNames = new[] { "node" },
            }
        );

    private static readonly Gauge _mempoolSizeUnScoped = Metrics
        .CreateGauge(
            "ng_node_mempool_size_total",
            "Current size of node mempool.",
            new GaugeConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Counter _mempoolItemsAddedUnScoped = Metrics
        .CreateCounter(
            "ng_node_mempool_added_count",
            "Transactions added to node mempool.",
            new CounterConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Counter _mempoolItemsRemovedUnScoped = Metrics
        .CreateCounter(
            "ng_node_mempool_removed_count",
            "Transactions removed from node mempool.",
            new CounterConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Counter _fullTransactionsFetchedCount = Metrics
        .CreateCounter(
            "ng_node_mempool_full_transactions_fetched_count",
            "Count of transaction contents fetched from the node.",
            new CounterConfiguration { LabelNames = new[] { "node", "is_duplicate" } }
        );

    private static readonly Counter _transactionsMarkedCommittedCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_marked_committed_count",
            "Number of mempool transactions which are marked committed"
        );

    private static readonly Counter _transactionsMarkedCommittedWhichWereFailedCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_marked_committed_which_were_failed_count",
            "Number of mempool transactions which are marked committed which were previously marked as failed"
        );

    private static readonly Counter _failedNetworkConfigurationFetchCounterUnScoped = Metrics
        .CreateCounter(
            "ng_node_fetch_network_configuration_error_count",
            "Number of errors fetching the node's network configuration.",
            new CounterConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Counter _failedNetworkStatusFetchCounterUnScoped = Metrics
        .CreateCounter(
            "ng_node_fetch_network_status_error_count",
            "Number of errors fetching network status from the node.",
            new CounterConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Counter _failedTransactionsFetchCounterUnScoped = Metrics
        .CreateCounter(
            "ng_node_fetch_transactions_error_count",
            "Number of errors fetching transactions from the node.",
            new CounterConfiguration { LabelNames = new[] { "node" } }
        );

    void IGlobalWorkerObserver.TrackNonFaultingExceptionInWorkLoop(Type worker, Exception exception)
    {
        _globalWorkerErrorsCount.WithLabels(worker.Name, exception.GetNameForMetricsOrLogging(), "non-faulting").Inc();
    }

    void IGlobalWorkerObserver.TrackWorkerFaultedException(Type worker, Exception exception, bool isStopRequested)
    {
        var errorType = isStopRequested && exception is OperationCanceledException ? "stopped" : "faulting";
        _globalWorkerErrorsCount.WithLabels(worker.Name, exception.GetNameForMetricsOrLogging(), errorType).Inc();
    }

    void INodeWorkerObserver.TrackNonFaultingExceptionInWorkLoop(Type worker, string nodeName, Exception exception)
    {
        _nodeWorkerErrorsCount.WithLabels(worker.Name, nodeName, exception.GetNameForMetricsOrLogging(), "non-faulting").Inc();
    }

    void INodeWorkerObserver.TrackWorkerFaultedException(Type worker, string nodeName, Exception exception, bool isStopRequested)
    {
        var errorType = isStopRequested && exception is OperationCanceledException ? "stopped" : "faulting";
        _nodeWorkerErrorsCount.WithLabels(worker.Name, nodeName, exception.GetNameForMetricsOrLogging(), errorType).Inc();
    }

    void ILedgerConfirmationServiceObserver.ResetQuorum()
    {
        _quorumExistsStatus.SetStatus(MetricStatus.Unknown);
        _quorumExtensionConsistentStatus.SetStatus(MetricStatus.Unknown);
    }

    void ILedgerConfirmationServiceObserver.TrustWeightingRequirementsComputed(LedgerConfirmationService.TrustWeightingReport report)
    {
        _configuredNodesTotal.Set(report.TotalTransactionNodes);
        _configuredNodesTrustWeightingTotal.Set((double)report.TrustWeightingAvailableAcrossAllNodes);

        _sufficientlySyncedUpNodesTotal.Set(report.TotalSufficientlySyncedUpNodes);
        _sufficientlySyncedUpNodesTrustWeightingTotal.Set((double)report.TrustWeightingOfSufficientlySyncedUpNodes);

        _ledgerNodeTrustWeightingRequiredForQuorum.Set((double)report.TrustWeightingRequiredForQuorumAtPresentTime);
        _ledgerNodeTrustWeightingRequiredForQuorumIfAllNodesSufficientlySynced.Set((double)report.TrustWeightingRequiredForQuorumIfAllNodesAvailableForQuorum);
    }

    ValueTask ILedgerConfirmationServiceObserver.PreHandleLedgerExtensionIfQuorum(DateTimeOffset timestamp)
    {
        _ledgerLastExtensionAttemptStartTimestamp.Set(timestamp.ToUnixTimeSecondsWithMilliPrecision());

        return ValueTask.CompletedTask;
    }

    void ILedgerConfirmationServiceObserver.PreSubmitNodeNetworkStatus(string nodeName, long ledgerTipStateVersion, long targetStateVersion)
    {
        _nodeLedgerTipStateVersion.WithLabels(nodeName).Set(ledgerTipStateVersion);
        _nodeLedgerTargetStateVersion.WithLabels(nodeName).Set(targetStateVersion);
    }

    void ILedgerConfirmationServiceObserver.SubmitNodeNetworkStatusUnknown(string nodeName, long ledgerTipStateVersion, long targetStateVersion)
    {
        _nodeLedgerTipIsConsistentWithQuorumStatus.WithLabels(nodeName).SetStatus(MetricStatus.Unknown);
    }

    void ILedgerConfirmationServiceObserver.SubmitNodeNetworkStatusUpToDate(string nodeName, long ledgerTipStateVersion, long targetStateVersion)
    {
        _nodeLedgerTipIsConsistentWithQuorumStatus.WithLabels(nodeName).SetStatus(MetricStatus.Yes);
    }

    void ILedgerConfirmationServiceObserver.SubmitNodeNetworkStatusOutOfDate(string nodeName, long ledgerTipStateVersion, long targetStateVersion)
    {
        _nodeLedgerTipIsConsistentWithQuorumStatus.WithLabels(nodeName).SetStatus(MetricStatus.No);
    }

    void ILedgerConfirmationServiceObserver.LedgerTipInconsistentWithQuorumStatus(string inconsistentNodeName)
    {
        _nodeLedgerTipIsConsistentWithQuorumStatus.WithLabels(inconsistentNodeName).SetStatus(MetricStatus.No);
    }

    void ILedgerConfirmationServiceObserver.LedgerTipConsistentWithQuorumStatus(string consistentNodeName)
    {
        _nodeLedgerTipIsConsistentWithQuorumStatus.WithLabels(consistentNodeName).SetStatus(MetricStatus.Yes);
    }

    void ILedgerConfirmationServiceObserver.UnknownQuorumStatus()
    {
        _quorumExistsStatus.SetStatus(MetricStatus.Unknown);
    }

    void ILedgerConfirmationServiceObserver.QuorumLost()
    {
        _quorumExistsStatus.SetStatus(MetricStatus.No);
    }

    void ILedgerConfirmationServiceObserver.QuorumGained()
    {
        _quorumExistsStatus.SetStatus(MetricStatus.Yes);
    }

    void ILedgerConfirmationServiceObserver.ReportOnLedgerExtensionSuccess(DateTimeOffset timestamp, TimeSpan parentSummaryRoundTimestamp, long totalCommitMs, int transactionsCommittedCount)
    {
        _peakLedgerLagBeforeLastCommit.Set(parentSummaryRoundTimestamp.TotalSeconds);
        _batchCommitTimeSeconds.Observe(totalCommitMs / 1000D);
        _ledgerCommittedTransactionsCount.Inc(transactionsCommittedCount);
        _ledgerLastCommitTimestamp.Set(timestamp.ToUnixTimeSecondsWithMilliPrecision());
    }

    void ILedgerConfirmationServiceObserver.RecordTopOfDbLedger(long stateVersion, DateTimeOffset roundTimestamp)
    {
        _ledgerStateVersion.Set(stateVersion);
        _ledgerUnixRoundTimestamp.Set(roundTimestamp.ToUnixTimeSecondsWithMilliPrecision());
    }

    void ILedgerConfirmationServiceObserver.QuorumExtensionConsistentGained()
    {
        _quorumExtensionConsistentStatus.SetStatus(MetricStatus.Yes);
    }

    void ILedgerConfirmationServiceObserver.QuorumExtensionConsistentLost()
    {
        _quorumExtensionConsistentStatus.SetStatus(MetricStatus.No);
    }

    public ValueTask PreMempoolPrune(List<MempoolStatusCount> mempoolCountByStatus)
    {
        var existingStatusLabelsNeedingUpdating = _mempoolDbSizeByStatus.GetAllLabelValues().SelectMany(x => x).ToHashSet();

        foreach (var countByStatus in mempoolCountByStatus)
        {
            _mempoolDbSizeByStatus.WithLabels(countByStatus.Status).Set(countByStatus.Count);
            existingStatusLabelsNeedingUpdating.Remove(countByStatus.Status);
        }

        // If a known status doesn't appear in the database, it should be set to 0.
        foreach (var statusName in existingStatusLabelsNeedingUpdating)
        {
            _mempoolDbSizeByStatus.WithLabels(statusName).Set(0);
        }

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolPrunerServiceObserver.PreMempoolTransactionPruned(int count)
    {
        _mempoolTransactionsPrunedCount.Inc(count);

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.TransactionsSelected(int totalTransactionsNeedingResubmission)
    {
        _resubmissionQueueSize.Set(totalTransactionsNeedingResubmission);

        return ValueTask.CompletedTask;
    }

    void IMempoolResubmissionServiceObserver.TransactionMarkedAsAssumedSuccessfullySubmittedToNode()
    {
        _dbMempoolTransactionsMarkedAsAssumedInNodeMempoolAfterResubmissionCount.Inc();
    }

    void IMempoolResubmissionServiceObserver.TransactionMarkedAsFailed()
    {
        _dbTransactionsMarkedAsFailedForTimeoutCount.Inc();
    }

    ValueTask IMempoolResubmissionServiceObserver.TransactionMarkedAsResolvedButUnknownAfterSubmittedToNode()
    {
        _dbMempoolTransactionsMarkedAsResolvedButUnknownStatusCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.TransactionMarkedAsFailedAfterSubmittedToNode()
    {
        _dbMempoolTransactionsMarkedAsFailedDuringResubmissionCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.PreResubmit(string signedTransaction)
    {
        _transactionResubmissionAttemptCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.PostResubmit(string signedTransaction)
    {
        _transactionResubmissionSuccessCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.PostResubmitDuplicate(string signedTransaction)
    {
        _transactionResubmissionResolutionByResultCount.WithLabels("node_marks_as_duplicate").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.PostResubmitSucceeded(string signedTransaction)
    {
        _transactionResubmissionResolutionByResultCount.WithLabels("success").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.ResubmitFailedSubstateNotFound(string signedTransaction, WrappedCoreApiException<SubstateDependencyNotFoundError> wrappedCoreApiException)
    {
        _transactionResubmissionErrorCount.Inc();
        _transactionResubmissionResolutionByResultCount.WithLabels("substate_missing_or_already_used").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.ResubmitFailedPermanently(string signedTransaction, WrappedCoreApiException wrappedCoreApiException)
    {
        _transactionResubmissionErrorCount.Inc();
        _transactionResubmissionResolutionByResultCount.WithLabels("unknown_permanent_error").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.ResubmitFailedTimeout(string signedTransaction, OperationCanceledException operationCanceledException)
    {
        _transactionResubmissionErrorCount.Inc();
        _transactionResubmissionResolutionByResultCount.WithLabels("request_timeout").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IMempoolResubmissionServiceObserver.ResubmitFailedUnknown(string signedTransaction, Exception exception)
    {
        _transactionResubmissionErrorCount.Inc();
        _transactionResubmissionResolutionByResultCount.WithLabels("unknown_error").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IAggregatorHealthCheckObserver.HealthReport(bool isHealthy, bool isPrimary)
    {
        _aggregatorIsUnhealthy.Set(!isHealthy ? 1 : 0);
        _aggregatorIsPrimary.Set(isPrimary ? 1 : 0);

        return ValueTask.CompletedTask;
    }

    void ISystemStatusServiceObserver.SetIsPrimary(bool isPrimary)
    {
        _isPrimaryStatus.SetStatus(isPrimary);
    }

    void INodeInitializerObserver.TrackInitializerFaultedException(Type worker, string nodeName, bool isStopRequested, Exception exception)
    {
        var errorType = isStopRequested && exception is OperationCanceledException ? "stopped" : "faulting";
        _nodeInitializersErrorsCount.WithLabels(GetType().Name, nodeName, exception.GetNameForMetricsOrLogging(), errorType).Inc();
    }

    void IMempoolTrackerServiceObserver.CombinedMempoolCurrentSizeCount(int count)
    {
        _combinedMempoolCurrentSizeTotal.Set(count);
    }

    void IMempoolTrackerServiceObserver.TransactionsReappearedCount(int count)
    {
        _dbTransactionsReappearedCount.Inc(count);
    }

    void IMempoolTrackerServiceObserver.TransactionsAddedDueToNodeMempoolAppearanceCount(int count)
    {
        _dbTransactionsAddedDueToNodeMempoolAppearanceCount.Inc(count);
    }

    void IMempoolTrackerServiceObserver.TransactionsMarkedAsMissing()
    {
        _dbTransactionsMarkedAsMissingCount.Inc();
    }

    ValueTask INodeTransactionLogWorkerObserver.DoWorkFailed(string nodeName, Exception exception)
    {
        _failedFetchLoopsUnlabeled.WithLabels(nodeName).Inc();

        return ValueTask.CompletedTask;
    }

    public ValueTask TransactionsFetched(string nodeName, List<CommittedTransaction> transactions, long fetchTransactionsMs)
    {
        _totalFetchTimeSecondsUnlabeled.WithLabels(nodeName).Observe(fetchTransactionsMs / 1000D);

        return ValueTask.CompletedTask;
    }

    ValueTask INodeMempoolTransactionIdsReaderWorkerObserver.MempoolSize(string nodeName, int transactionIdentifiersCount)
    {
        _mempoolSizeUnScoped.WithLabels(nodeName).Set(transactionIdentifiersCount);

        return ValueTask.CompletedTask;
    }

    ValueTask INodeMempoolTransactionIdsReaderWorkerObserver.MempoolItemsChange(string nodeName, int transactionIdsAddedCount, int transactionIdsRemovedCount)
    {
        _mempoolItemsAddedUnScoped.WithLabels(nodeName).Inc(transactionIdsAddedCount);
        _mempoolItemsRemovedUnScoped.WithLabels(nodeName).Inc(transactionIdsRemovedCount);

        return ValueTask.CompletedTask;
    }

    ValueTask INodeMempoolFullTransactionReaderWorkerObserver.FullTransactionsFetchedCount(string nodeName, bool wasDuplicate)
    {
        _fullTransactionsFetchedCount.WithLabels(nodeName, wasDuplicate ? "true" : "false").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IRawTransactionWriterObserver.TransactionsMarkedCommittedWhichWasFailed()
    {
        _transactionsMarkedCommittedWhichWereFailedCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IRawTransactionWriterObserver.TransactionsMarkedCommittedCount(int count)
    {
        _transactionsMarkedCommittedCount.Inc(count);

        return ValueTask.CompletedTask;
    }

    public ValueTask GetNetworkConfigurationFailed(string nodeName, Exception exception)
    {
        _failedNetworkConfigurationFetchCounterUnScoped.WithLabels(nodeName).Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask INetworkStatusReaderObserver.GetNetworkStatusFailed(string nodeName, Exception exception)
    {
        _failedNetworkStatusFetchCounterUnScoped.WithLabels(nodeName).Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask ITransactionLogReaderObserver.GetTransactionsFailed(string nodeName, Exception exception)
    {
        _failedTransactionsFetchCounterUnScoped.WithLabels(nodeName).Inc();

        return ValueTask.CompletedTask;
    }
}
