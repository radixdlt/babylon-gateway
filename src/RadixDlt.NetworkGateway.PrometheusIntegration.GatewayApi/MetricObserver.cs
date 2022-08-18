using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Prometheus;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Common.Exceptions;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Endpoints;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PrometheusIntegration.GatewayApi;

public class MetricObserver :
    IExceptionObserver,
    ICoreNodeHealthCheckerObserver,
    IConstructionAndSubmissionServiceObserver,
    ILedgerStateQuerierObserver,
    ISubmissionTrackingServiceObserver
{
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

    private static readonly Counter _transactionBuildRequestCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_build_request_count",
            "Number of transaction build requests"
        );

    private static readonly Counter _transactionBuildSuccessCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_build_success_count",
            "Number of transaction build successes"
        );

    private static readonly Counter _transactionBuildErrorCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_build_error_count",
            "Number of transaction build errors"
        );

    private static readonly Counter _transactionFinalizeRequestCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_finalize_request_count",
            "Number of transaction finalize requests"
        );

    private static readonly Counter _transactionFinalizeSuccessCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_finalize_success_count",
            "Number of transaction finalize successes"
        );

    private static readonly Counter _transactionFinalizeErrorCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_finalize_error_count",
            "Number of transaction finalize errors"
        );

    private static readonly Counter _transactionSubmitRequestCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_request_count",
            "Number of transaction submission requests (including as part of a finalize request)"
        );

    private static readonly Counter _transactionSubmitSuccessCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_success_count",
            "Number of transaction submission successes (including as part of a finalize request)"
        );

    private static readonly Counter _transactionSubmitErrorCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_error_count",
            "Number of transaction submission errors (including as part of a finalize request)"
        );

    private static readonly Counter _transactionSubmitResolutionByResultCount = Metrics
        .CreateCounter(
            "ng_construction_transaction_submission_resolution_count",
            "Number of various resolutions at transaction submission time",
            new CounterConfiguration { LabelNames = new[] { "result" } }
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

    private static readonly Counter _dbMempoolTransactionsMarkedAsFailedDuringInitialSubmissionCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_marked_failed_from_initial_submission_count",
            "Number of mempool transactions marked as failed during initial submission to a node"
        );

    void IExceptionObserver.OnException(ActionContext actionContext, Exception exception, KnownGatewayErrorException gatewayErrorException)
    {
        // actionContext.HttpContext.Request.Method - GET or POST
        var routeValueDictionary = actionContext.RouteData.Values;

        // This is a lot of labels, but the rest depend on the action and exception, so the cardinality isn't massive / worrying
        // Method/Controller/Action align with the prometheus-net http metrics
        // https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.AspNetCore/HttpMetrics/HttpRequestMiddlewareBase.cs
        _apiResponseErrorCount.WithLabels(
            actionContext.HttpContext.Request.Method, // method (GET or POST)
            routeValueDictionary.GetValueOrDefault("Controller") as string ?? string.Empty, // controller
            routeValueDictionary.GetValueOrDefault("Action") as string ?? string.Empty, // action
            exception.GetNameForMetricsOrLogging(), // exception
            gatewayErrorException.GatewayError.GetType().Name, // gateway_error
            gatewayErrorException.StatusCode.ToString(CultureInfo.InvariantCulture) // status_code
        ).Inc();
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

    ValueTask IConstructionAndSubmissionServiceObserver.PreHandleBuildRequest(TransactionBuildRequest request, LedgerState ledgerState)
    {
        _transactionBuildRequestCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.PostHandleBuildRequest(TransactionBuildRequest request, LedgerState ledgerState, TransactionBuild response)
    {
        _transactionBuildSuccessCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.HandleBuildRequestFailed(TransactionBuildRequest request, LedgerState ledgerState, Exception exception)
    {
        _transactionBuildErrorCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.PreHandleFinalizeRequest(TransactionFinalizeRequest request)
    {
        _transactionFinalizeRequestCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.PostHandleFinalizeRequest(TransactionFinalizeRequest request, TransactionFinalizeResponse response)
    {
        _transactionFinalizeSuccessCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.HandleFinalizeRequestFailed(TransactionFinalizeRequest request, Exception exception)
    {
        _transactionFinalizeErrorCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.PreHandleSubmitRequest(TransactionSubmitRequest request)
    {
        _transactionSubmitRequestCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.PostHandleSubmitRequest(TransactionSubmitRequest request, TransactionSubmitResponse response)
    {
        _transactionSubmitSuccessCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.HandleSubmitRequestFailed(TransactionSubmitRequest request, Exception exception)
    {
        _transactionSubmitErrorCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.ParseTransactionFailedSubstateNotFound(ValidatedHex signedTransaction, WrappedCoreApiException<SubstateDependencyNotFoundError> wrappedCoreApiException)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("parse_failed_substate_missing_or_already_used").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.ParseTransactionFailedInvalidTransaction(ValidatedHex signedTransaction, WrappedCoreApiException wrappedCoreApiException)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("parse_failed_invalid_transaction").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.ParseTransactionFailedUnknown(ValidatedHex signedTransaction, Exception exception)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("parse_failed_unknown_error").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.SubmissionAlreadyFailed(ValidatedHex signedTransaction, MempoolTrackGuidance mempoolTrackGuidance)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("already_failed").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.SubmissionAlreadySubmitted(ValidatedHex signedTransaction, MempoolTrackGuidance mempoolTrackGuidance)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("already_submitted").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.SubmissionDuplicate(ValidatedHex signedTransaction, ConstructionSubmitResponse result)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("node_marks_as_duplicate").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.SubmissionSucceeded(ValidatedHex signedTransaction, ConstructionSubmitResponse result)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("success").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.HandleSubmissionFailedSubstateNotFound(ValidatedHex signedTransaction, WrappedCoreApiException<SubstateDependencyNotFoundError> wrappedCoreApiException)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("substate_missing_or_already_used").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.HandleSubmissionFailedInvalidTransaction(ValidatedHex signedTransaction, WrappedCoreApiException wrappedCoreApiException)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("invalid_transaction").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.HandleSubmissionFailedPermanently(ValidatedHex signedTransaction, WrappedCoreApiException wrappedCoreApiException)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("unknown_permanent_error").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.HandleSubmissionFailedTimeout(ValidatedHex signedTransaction, OperationCanceledException operationCanceledException)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("request_timeout").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask IConstructionAndSubmissionServiceObserver.HandleSubmissionFailedUnknown(ValidatedHex signedTransaction, Exception exception)
    {
        _transactionSubmitResolutionByResultCount.WithLabels("unknown_error").Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask ILedgerStateQuerierObserver.LedgerRoundTimestampClockSkew(Duration difference)
    {
        _ledgerTipRoundTimestampVsGatewayApiClockLagAtLastRequestSeconds.Set(difference.TotalSeconds);

        return ValueTask.CompletedTask;
    }

    ValueTask ISubmissionTrackingServiceObserver.PostMempoolTransactionAdded()
    {
        _dbMempoolTransactionsAddedDueToSubmissionCount.Inc();

        return ValueTask.CompletedTask;
    }

    ValueTask ISubmissionTrackingServiceObserver.PostMempoolTransactionMarkedAsFailed()
    {
        _dbMempoolTransactionsMarkedAsFailedDuringInitialSubmissionCount.Inc();

        return ValueTask.CompletedTask;
    }
}
