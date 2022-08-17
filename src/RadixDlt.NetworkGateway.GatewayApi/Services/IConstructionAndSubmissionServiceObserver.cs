using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Common.Exceptions;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface IConstructionAndSubmissionServiceObserver
{
    ValueTask PreHandleBuildRequest(TransactionBuildRequest request, LedgerState ledgerState);

    ValueTask PostHandleBuildRequest(TransactionBuildRequest request, LedgerState ledgerState, TransactionBuild response);

    ValueTask HandleBuildRequestFailed(TransactionBuildRequest request, LedgerState ledgerState, Exception exception);

    ValueTask PreHandleFinalizeRequest(TransactionFinalizeRequest request);

    ValueTask PostHandleFinalizeRequest(TransactionFinalizeRequest request, TransactionFinalizeResponse response);

    ValueTask HandleFinalizeRequestFailed(TransactionFinalizeRequest request, Exception exception);

    ValueTask PreHandleSubmitRequest(TransactionSubmitRequest request);

    ValueTask PostHandleSubmitRequest(TransactionSubmitRequest request, TransactionSubmitResponse response);

    ValueTask HandleSubmitRequestFailed(TransactionSubmitRequest request, Exception exception);

    ValueTask ParseTransactionFailedSubstateNotFound(ValidatedHex signedTransaction, WrappedCoreApiException<SubstateDependencyNotFoundError> wrappedCoreApiException);

    ValueTask ParseTransactionFailedInvalidTransaction(ValidatedHex signedTransaction, WrappedCoreApiException wrappedCoreApiException);

    ValueTask ParseTransactionFailedUnknown(ValidatedHex signedTransaction, Exception exception);

    ValueTask SubmissionAlreadyFailed(ValidatedHex signedTransaction, MempoolTrackGuidance mempoolTrackGuidance);

    ValueTask SubmissionAlreadySubmitted(ValidatedHex signedTransaction, MempoolTrackGuidance mempoolTrackGuidance);

    ValueTask SubmissionDuplicate(ValidatedHex signedTransaction, ConstructionSubmitResponse result);

    ValueTask SubmissionSucceeded(ValidatedHex signedTransaction, ConstructionSubmitResponse result);

    ValueTask HandleSubmissionFailedSubstateNotFound(ValidatedHex signedTransaction, WrappedCoreApiException<SubstateDependencyNotFoundError> wrappedCoreApiException);

    ValueTask HandleSubmissionFailedInvalidTransaction(ValidatedHex signedTransaction, WrappedCoreApiException wrappedCoreApiException);

    ValueTask HandleSubmissionFailedPermanently(ValidatedHex signedTransaction, WrappedCoreApiException wrappedCoreApiException);

    ValueTask HandleSubmissionFailedTimeout(ValidatedHex signedTransaction, OperationCanceledException operationCanceledException);

    ValueTask HandleSubmissionFailedUnknown(ValidatedHex signedTransaction, Exception exception);
}
