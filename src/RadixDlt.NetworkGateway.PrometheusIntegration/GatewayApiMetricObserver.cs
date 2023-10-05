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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Prometheus;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PrometheusIntegration;

internal class GatewayApiMetricObserver :
    IExceptionObserver,
    ICoreNodeHealthCheckerObserver,
    ISubmissionServiceObserver,
    IPreviewServiceObserver,
    ILedgerStateQuerierObserver,
    ISubmissionTrackingServiceObserver,
    ISqlQueryObserver
{
    private static readonly Histogram _sqlQueryDuration = Metrics
        .CreateHistogram("sql_query_duration", "The duration of SQL queries processed by this app.",
            new HistogramConfiguration { LabelNames = new[] { "query_name" } });

    private static readonly Counter _apiResponseErrorCount = Metrics
        .CreateCounter(
            "ng_gateway_response_error_count",
            "Count of response errors from the gateway.",
            new CounterConfiguration { LabelNames = new[] { "method", "controller", "action", "exception", "gateway_error", "status_code" } }
        );

    private static readonly Gauge _healthCheckStatusByNode = Metrics
        .CreateGauge(
            "ng_node_gateway_health_check_status",
            "The health check status of an individual node. 1 if healthy and synced, 0.5 if health but lagging, 0 if unhealthy",
            new GaugeConfiguration { LabelNames = new[] { "node" } }
        );

    private static readonly Gauge _healthCheckCountsAcrossAllNodes = Metrics
        .CreateGauge(
            "ng_nodes_gateway_health_check_node_statuses",
            "The health check status of all nodes. Statuses are HEALTHY_AND_SYNCED, HEALTHY_BUT_LAGGING, UNHEALTHY",
            new GaugeConfiguration { LabelNames = new[] { "status" } }
        );

    private static readonly Counter _transactionSubmitRequestCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_request_count",
            "Number of transaction submission requests",
            new CounterConfiguration { LabelNames = new[] { "target_node" } }
        );

    private static readonly Counter _transactionSubmitSuccessCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_success_count",
            "Number of transaction submission successes",
            new CounterConfiguration { LabelNames = new[] { "target_node" } }
        );

    private static readonly Counter _transactionSubmitErrorCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_error_count",
            "Number of transaction submission errors",
            new CounterConfiguration { LabelNames = new[] { "target_node" } }
        );

    private static readonly Counter _transactionPreviewRequestCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_preview_request_count",
            "Number of transaction preview requests",
            new CounterConfiguration { LabelNames = new[] { "target_node" } }
        );

    private static readonly Counter _transactionPreviewSuccessCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_preview_success_count",
            "Number of transaction preview successes",
            new CounterConfiguration { LabelNames = new[] { "target_node" } }
        );

    private static readonly Counter _transactionPreviewErrorCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_preview_error_count",
            "Number of transaction preview errors",
            new CounterConfiguration { LabelNames = new[] { "target_node" } }
        );

    private static readonly Counter _transactionSubmitResolutionByResultCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_resolution_count",
            "Number of various resolutions at transaction submission time",
            new CounterConfiguration { LabelNames = new[] { "result", "target_node" } }
        );

    private static readonly Gauge _ledgerTipRoundTimestampVsGatewayApiClockLagAtLastRequestSeconds = Metrics
        .CreateGauge(
            "ng_gateway_ledger_tip_round_timestamp_gateway_api_clock_lag_at_last_request_seconds",
            "The delay measured between the Gateway API clock and the round timestamp at last request to the top of the ledger (in seconds, to millisecond precision)."
        );

    private static readonly Counter _dbMempoolTransactionsAddedDueToSubmissionCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_added_from_gateway_submission_count",
            "Number of mempool transactions added to the DB due to being submitted to the gateway"
        );

    private static readonly Counter _dbMempoolTransactionsDuplicateSubmissionsCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_duplicate_submission_count",
            "Number of mempool transactions submitted to the gateway which were already being tracked"
        );

    public void OnSqlQueryExecuted(string queryName, TimeSpan duration)
    {
        _sqlQueryDuration.WithLabels(queryName).Observe(duration.TotalSeconds);
    }

    void IExceptionObserver.OnException(ActionContext actionContext, Exception exception, KnownGatewayErrorException gatewayErrorException)
    {
        // actionContext.HttpContext.Request.Method - GET or POST
        var routeValueDictionary = actionContext.RouteData.Values;

        // This is a lot of labels, but the rest depend on the action and exception, so the cardinality isn't massive / worrying
        // Method/Controller/Action align with the prometheus-net http metrics
        // https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.AspNetCore/HttpMetrics/HttpRequestMiddlewareBase.cs
        _apiResponseErrorCount
            .WithLabels(
                actionContext.HttpContext.Request.Method, // method (GET or POST)
                routeValueDictionary.GetValueOrDefault("Controller") as string ?? string.Empty, // controller
                routeValueDictionary.GetValueOrDefault("Action") as string ?? string.Empty, // action
                exception.GetNameForMetricsOrLogging(), // exception
                gatewayErrorException.GatewayError.GetType().Name, // gateway_error
                gatewayErrorException.StatusCode.ToString(CultureInfo.InvariantCulture) // status_code
            )
            .Inc();
    }

    ValueTask ICoreNodeHealthCheckerObserver.CountByStatus(int healthyAndSyncedCount, int healthyButLaggingCount, int unhealthyCount)
    {
        _healthCheckCountsAcrossAllNodes.WithLabels("HEALTHY_AND_SYNCED").Set(healthyAndSyncedCount);
        _healthCheckCountsAcrossAllNodes.WithLabels("HEALTHY_BUT_LAGGING").Set(healthyButLaggingCount);
        _healthCheckCountsAcrossAllNodes.WithLabels("UNHEALTHY").Set(unhealthyCount);

        return ValueTask.CompletedTask;
    }

    void ICoreNodeHealthCheckerObserver.NodeUnhealthy((CoreApiNode CoreApiNode, long? NodeStateVersion, Exception? Exception) healthCheckData)
    {
        _healthCheckStatusByNode.WithLabels(healthCheckData.CoreApiNode.Name).Set(0);
    }

    void ICoreNodeHealthCheckerObserver.NodeHealthyButLagging((CoreApiNode CoreApiNode, long? NodeStateVersion, Exception? Exception) healthCheckData)
    {
        _healthCheckStatusByNode.WithLabels(healthCheckData.CoreApiNode.Name).Set(0.5);
    }

    void ICoreNodeHealthCheckerObserver.NodeHealthyAndSynced((CoreApiNode CoreApiNode, long? NodeStateVersion, Exception? Exception) healthCheckData)
    {
        _healthCheckStatusByNode.WithLabels(healthCheckData.CoreApiNode.Name).Set(1);
    }

    ValueTask ILedgerStateQuerierObserver.LedgerRoundTimestampClockSkew(TimeSpan difference)
    {
        _ledgerTipRoundTimestampVsGatewayApiClockLagAtLastRequestSeconds.Set(difference.TotalSeconds);

        return ValueTask.CompletedTask;
    }

    ValueTask IPreviewServiceObserver.PreHandlePreviewRequest(GatewayModel.TransactionPreviewRequest request, string targetNode)
    {
        _transactionPreviewRequestCount.WithLabels(targetNode).Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IPreviewServiceObserver.PostHandlePreviewRequest(GatewayModel.TransactionPreviewRequest request, string targetNode, GatewayModel.TransactionPreviewResponse response)
    {
        _transactionPreviewSuccessCount.WithLabels(targetNode).Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IPreviewServiceObserver.HandlePreviewRequestFailed(GatewayModel.TransactionPreviewRequest request, string targetNode, Exception exception)
    {
        _transactionPreviewErrorCount.WithLabels(targetNode).Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask ISubmissionTrackingServiceObserver.OnSubmissionTrackedInDatabase(bool isDuplicate)
    {
        if (isDuplicate)
        {
            _dbMempoolTransactionsDuplicateSubmissionsCount.Inc();
        }
        else
        {
            _dbMempoolTransactionsAddedDueToSubmissionCount.Inc();
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask ObserveTransactionSubmissionToGatewayOutcome(TransactionSubmissionOutcome outcome)
    {
        var label = outcome switch
        {
            TransactionSubmissionOutcome.SubmittedToNetwork => "submitted_to_network",
            TransactionSubmissionOutcome.PermanentlyRejected => "permanently_rejected",
            TransactionSubmissionOutcome.StoppedSubmittingToNetwork => "stopped_submitting",
            TransactionSubmissionOutcome.DuplicateSubmission => "duplicate",
            TransactionSubmissionOutcome.ParseFailedIncorrectFormat => "parse_failed_incorrect_format",
            TransactionSubmissionOutcome.ParseFailedStaticallyInvalid => "parse_failed_invalid_transaction",
            TransactionSubmissionOutcome.ParseFailedOtherError => "parse_failed_other_error",
            TransactionSubmissionOutcome.StartEpochInFuture => "start_epoch_in_future",
            TransactionSubmissionOutcome.EndEpochInPast => "end_epoch_in_past",
        };
        _transactionSubmitResolutionByResultCount.WithLabels(label).Inc();

        return ValueTask.CompletedTask;
    }

    public ValueTask ObserveSubmitAttempt(SubmitContext context)
    {
        if (context.IsResubmission)
        {
            throw new Exception("The Gateway API is only supposed to handle initial transaction submissions");
        }

        _transactionSubmitRequestCount.WithLabels(context.TargetNode).Inc();

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Note - this parallels ObserveSubmitResult in DataAggregatorMetricsObserver.
    /// </summary>
    public ValueTask ObserveSubmitResult(SubmitContext context, NodeSubmissionResult nodeSubmissionResult)
    {
        if (context.IsResubmission)
        {
            throw new Exception("The Gateway API is only supposed to handle initial transaction submissions");
        }

        if (nodeSubmissionResult.IsSubmissionSuccess())
        {
            _transactionSubmitSuccessCount.WithLabels(context.TargetNode).Inc();
        }

        if (nodeSubmissionResult.IsSubmissionError())
        {
            _transactionSubmitErrorCount.WithLabels(context.TargetNode).Inc();
        }

        _transactionSubmitResolutionByResultCount.WithLabels(nodeSubmissionResult.MetricLabel(), context.TargetNode).Inc();

        return ValueTask.CompletedTask;
    }
}
