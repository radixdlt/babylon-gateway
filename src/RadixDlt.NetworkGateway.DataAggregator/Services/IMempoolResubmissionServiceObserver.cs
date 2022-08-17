using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Common.Exceptions;
using System;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface IMempoolResubmissionServiceObserver
{
    ValueTask TransactionsSelected(int totalTransactionsNeedingResubmission);

    void TransactionMarkedAsAssumedSuccessfullySubmittedToNode();

    void TransactionMarkedAsFailed();

    ValueTask TransactionMarkedAsResolvedButUnknownAfterSubmittedToNode();

    ValueTask TransactionMarkedAsFailedAfterSubmittedToNode();

    ValueTask PreResubmit(string signedTransaction);

    ValueTask PostResubmit(string signedTransaction);

    ValueTask PostResubmitDuplicate(string signedTransaction);

    ValueTask PostResubmitSucceeded(string signedTransaction);

    ValueTask ResubmitFailedSubstateNotFound(string signedTransaction, WrappedCoreApiException<SubstateDependencyNotFoundError> wrappedCoreApiException);

    ValueTask ResubmitFailedPermanently(string signedTransaction, WrappedCoreApiException wrappedCoreApiException);

    ValueTask ResubmitFailedTimeout(string signedTransaction, OperationCanceledException operationCanceledException);

    ValueTask ResubmitFailedUnknown(string signedTransaction, Exception exception);
}
