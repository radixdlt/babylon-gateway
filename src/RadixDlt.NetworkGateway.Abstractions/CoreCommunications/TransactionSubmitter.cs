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

using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreApi = RadixDlt.CoreApiSdk.Api;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.Abstractions.CoreCommunications;

public class TransactionSubmitter
{
    public static async Task<NodeSubmissionResult> Submit(
        SubmitContext submitContext,
        byte[] notarizedTransaction,
        IReadOnlyCollection<ITransactionSubmitterObserver> observers,
        CancellationToken cancellationToken)
    {
        await observers.ForEachAsync(observer => observer.ObserveSubmitAttempt(submitContext));

        var result = await SubmitInternal(submitContext, notarizedTransaction, cancellationToken);

        await observers.ForEachAsync(observer => observer.ObserveSubmitResult(submitContext, result));

        return result;
    }

    private static async Task<NodeSubmissionResult> SubmitInternal(
        SubmitContext submitContext,
        byte[] notarizedTransaction,
        CancellationToken cancellationToken)
    {
        using var timeoutTokenSource = new CancellationTokenSource(submitContext.SubmissionTimeout);
        using var finalTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancellationToken);

        try
        {
            var result = await CoreApiErrorWrapper.ResultOrError<CoreModel.TransactionSubmitResponse, CoreModel.TransactionSubmitErrorResponse>(() =>
                submitContext.TransactionApi.TransactionSubmitPostAsync(
                    new CoreModel.TransactionSubmitRequest(
                        network: submitContext.NetworkName,
                        notarizedTransactionHex: notarizedTransaction.ToHex()
                    ),
                    cancellationToken
                ));

            if (result.Succeeded)
            {
                return new NodeSubmissionResult.AcceptedIntoMempool(IsDuplicate: result.SuccessResponse.Duplicate);
            }

            return result.FailureResponse.Details switch
            {
                CoreModel.TransactionSubmitIntentAlreadyCommitted alreadyCommittedDetails =>
                    new NodeSubmissionResult.IntentAlreadyCommitted(alreadyCommittedDetails.CommittedAs),
                CoreModel.TransactionSubmitPriorityThresholdNotMetErrorDetails priorityThresholdDetails =>
                    new NodeSubmissionResult.MempoolPriorityThresholdNotMet(priorityThresholdDetails),
                CoreModel.TransactionSubmitRejectedErrorDetails rejectedDetails =>
                    new NodeSubmissionResult.Rejection(rejectedDetails),
                _ => throw new ArgumentOutOfRangeException(nameof(result.FailureResponse.Details), result.FailureResponse.Details, null),
            };
        }
        catch (OperationCanceledException ex) when (timeoutTokenSource.IsCancellationRequested)
        {
            return new NodeSubmissionResult.OtherSubmissionError(ex, IsTimeout: true, $"Operation timed-out after {submitContext.SubmissionTimeout.TotalSeconds} seconds");
        }
        catch (OperationCanceledException ex)
        {
            return new NodeSubmissionResult.OtherSubmissionError(ex, IsTimeout: false, "Operation cancelled (not due to timeout)");
        }
        catch (Exception ex)
        {
            return new NodeSubmissionResult.OtherSubmissionError(ex, IsTimeout: false, "Unknown error");
        }
    }
}

public abstract record NodeSubmissionResult
{
    // ======================================================================
    // WARNING WHEN ADDING VARIANTS:
    // ======================================================================
    // C# pattern matching for variants isn't exhaustive, which is rather sad.
    // This means if you add a new variant below, you should search everywhere
    // where `NodeSubmissionResult.` is used inside a switch statement or
    // expression, to ensure these are updated.
    //
    // Ideally, prefer adding abstract/virtual methods instead.
    // ======================================================================

    public record IntentAlreadyCommitted(CoreModel.CommittedIntentMetadata CommittedAsDetails) : NodeSubmissionResult
    {
        public override bool ShouldStopSubmittingPermanently() => true;

        public override PendingTransactionPayloadLedgerStatus PayloadStatus() => CommittedAsDetails.IsSameTransaction
            ? PendingTransactionPayloadLedgerStatus.CommitPendingOutcomeUnknown
            : PendingTransactionPayloadLedgerStatus.CommitOfOtherPayloadForIntentPendingOutcomeUnknown;

        public override PendingTransactionIntentLedgerStatus IntentStatus() => PendingTransactionIntentLedgerStatus.CommitPendingOutcomeUnknown;

        public override string MetricLabel() => "already_committed";
    }

    /// <summary>
    /// If the transaction was accepted into the mempool.
    /// This suggests the node believe the transaction could be committed.
    /// </summary>
    /// <param name="IsDuplicate">True if the transaction was already in the given node's mempool.</param>
    public record AcceptedIntoMempool(bool IsDuplicate) : NodeSubmissionResult
    {
        public override bool ShouldStopSubmittingPermanently() => false;

        public override PendingTransactionPayloadLedgerStatus PayloadStatus() => PendingTransactionPayloadLedgerStatus.TransientlyAccepted;

        public override PendingTransactionIntentLedgerStatus IntentStatus() => PendingTransactionIntentLedgerStatus.PossibleToCommit;

        public override bool IsSubmissionSuccess() => true;

        public override string MetricLabel() => IsDuplicate ? "duplicate" : "success";
    }

    public record Rejection(CoreModel.TransactionSubmitRejectedErrorDetails Details) : NodeSubmissionResult
    {
        public override bool ShouldStopSubmittingPermanently() => Details.IsPayloadRejectionPermanent;

        public override PendingTransactionPayloadLedgerStatus PayloadStatus() => Details.IsPayloadRejectionPermanent
            ? PendingTransactionPayloadLedgerStatus.PermanentlyRejected
            : PendingTransactionPayloadLedgerStatus.TransientlyRejected;

        public override PendingTransactionIntentLedgerStatus IntentStatus() => Details.IsIntentRejectionPermanent
            ? PendingTransactionIntentLedgerStatus.PermanentRejection
            : PendingTransactionIntentLedgerStatus.LikelyButNotCertainRejection;

        public override string? GetExecutionFailureReason() => Details.ErrorMessage;

        public override string MetricLabel() => Details.IsPayloadRejectionPermanent ? "permanent_rejection" : "temporary_rejection";
    }

    public record MempoolPriorityThresholdNotMet(CoreModel.TransactionSubmitPriorityThresholdNotMetErrorDetails Details) : NodeSubmissionResult
    {
        public override bool ShouldStopSubmittingPermanently() => false;

        public override PendingTransactionPayloadLedgerStatus PayloadStatus() => PendingTransactionPayloadLedgerStatus.Unknown;

        public override PendingTransactionIntentLedgerStatus IntentStatus() => PendingTransactionIntentLedgerStatus.Unknown;

        public override string GetApiSubmitErrorTitle() => $"Insufficient tip percentage of {Details.TipPercentage}; min tip percentage {Details.MinTipPercentageRequired}";

        public override string MetricLabel() => "tip_too_low";
    }

    public record OtherSubmissionError(Exception Exception, bool IsTimeout, string ErrorTitle) : NodeSubmissionResult
    {
        public override bool ShouldStopSubmittingPermanently() => false;

        public override PendingTransactionPayloadLedgerStatus PayloadStatus() => PendingTransactionPayloadLedgerStatus.Unknown;

        public override PendingTransactionIntentLedgerStatus IntentStatus() => PendingTransactionIntentLedgerStatus.Unknown;

        public override string GetApiSubmitErrorTitle() => ErrorTitle;

        public override bool IsSubmissionError() => true;

        public override string MetricLabel() => IsTimeout ? "request_timeout"
            : Exception is OperationCanceledException ? "cancellation"
            : "unknown_error";
    }

    private NodeSubmissionResult()
    {
    }

    public abstract bool ShouldStopSubmittingPermanently();

    /// <summary>
    /// The most accurate payload status knowledge from the submission result.
    /// This will be combined not necessarily update the PendingTransaction, if it's of a lower ReplacementPriority.
    /// </summary>
    public abstract PendingTransactionPayloadLedgerStatus PayloadStatus();

    /// <summary>
    /// The most accurate intent status knowledge from the submission result.
    /// This will be combined not necessarily update the PendingTransaction, if it's of a lower ReplacementPriority.
    /// </summary>
    public abstract PendingTransactionIntentLedgerStatus IntentStatus();

    public virtual string? GetExecutionFailureReason() => null;

    public virtual string? GetApiSubmitErrorTitle() => null;

    public virtual bool IsSubmissionSuccess() => false;

    public virtual bool IsSubmissionError() => false;

    public abstract string MetricLabel();
}

public interface ITransactionSubmitterObserver
{
    ValueTask ObserveSubmitAttempt(SubmitContext context);

    ValueTask ObserveSubmitResult(SubmitContext context, NodeSubmissionResult nodeSubmissionResult);
}

public record SubmitContext(
    CoreApi.TransactionApi TransactionApi,
    string NetworkName,
    TimeSpan SubmissionTimeout,
    bool IsResubmission);
