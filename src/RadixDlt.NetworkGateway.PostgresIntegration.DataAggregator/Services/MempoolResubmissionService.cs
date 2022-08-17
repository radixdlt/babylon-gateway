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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Common.CoreCommunications;
using RadixDlt.NetworkGateway.Common.Database;
using RadixDlt.NetworkGateway.Common.Database.Models.Mempool;
using RadixDlt.NetworkGateway.Common.Exceptions;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.Common.Model;
using RadixDlt.NetworkGateway.Common.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public class MempoolResubmissionService : IMempoolResubmissionService
{
    private static readonly LogLimiter _emptyResubmissionQueueLogLimiter = new(TimeSpan.FromSeconds(60), LogLevel.Information, LogLevel.Debug);

    private readonly IServiceProvider _services;
    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly IOptionsMonitor<MempoolOptions> _mempoolOptionsMonitor;
    private readonly IOptionsMonitor<NetworkOptions> _networkOptionsMonitor;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ISystemStatusService _systemStatusService;
    private readonly ILogger<MempoolResubmissionService> _logger;
    private readonly IMempoolResubmissionServiceObserver? _observer;

    public MempoolResubmissionService(
        IServiceProvider services,
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        IOptionsMonitor<MempoolOptions> mempoolOptionsMonitor,
        IOptionsMonitor<NetworkOptions> networkOptionsMonitor,
        INetworkConfigurationProvider networkConfigurationProvider,
        ISystemStatusService systemStatusService,
        ILogger<MempoolResubmissionService> logger,
        IMempoolResubmissionServiceObserver? observer)
    {
        _services = services;
        _dbContextFactory = dbContextFactory;
        _mempoolOptionsMonitor = mempoolOptionsMonitor;
        _networkOptionsMonitor = networkOptionsMonitor;
        _networkConfigurationProvider = networkConfigurationProvider;
        _systemStatusService = systemStatusService;
        _logger = logger;
        _observer = observer;
    }

    public async Task RunBatchOfResubmissions(CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        const int BatchSize = 30;

        var instantForTransactionChoosing = SystemClock.Instance.GetCurrentInstant();
        var mempoolConfiguration = _mempoolOptionsMonitor.CurrentValue;

        var transactionsToResubmit = await SelectTransactionsToResubmit(dbContext, instantForTransactionChoosing, mempoolConfiguration, BatchSize, token);

        var submittedAt = SystemClock.Instance.GetCurrentInstant();

        // The timeout should be relative to the submittedAt time we save to the DB, so needs to include the time for initial db saving (which should be very quick).
        using var ctsWithSubmissionTimeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        ctsWithSubmissionTimeout.CancelAfter(mempoolConfiguration.ResubmissionNodeRequestTimeout.ToTimeSpan());

        var transactionsToResubmitWithNodes = MarkTransactionsAsFailedForTimeoutOrPendingResubmissionToRandomNode(
            mempoolConfiguration,
            transactionsToResubmit,
            submittedAt
        );

        // We save as assumed successfully submitted here to lock in the fact we're resubmitting - in case we hit an
        // exception below and fail to resubmit. By virtue of the ConcurrencyToken on the Status, this protects us
        // against resubmitting the same transaction multiple times - even if (eg) two data aggregators are running.
        await dbContext.SaveChangesAsync(token);

        await ResubmitAllAndUpdateTransactionStatusesOnFailure(mempoolConfiguration, transactionsToResubmitWithNodes, submittedAt, ctsWithSubmissionTimeout.Token);

        await dbContext.SaveChangesAsync(token);
    }

    private async Task<List<MempoolTransaction>> SelectTransactionsToResubmit(
        ReadWriteDbContext dbContext,
        Instant instantForTransactionChoosing,
        MempoolOptions mempoolOptions,
        int batchSize,
        CancellationToken token
    )
    {
        var transactionsToResubmit =
            await GetMempoolTransactionsNeedingResubmission(instantForTransactionChoosing, mempoolOptions, dbContext)
                .OrderBy(mt => mt.LastSubmittedToNodeTimestamp)
                .Take(batchSize)
                .ToListAsync(token);

        var totalTransactionsNeedingResubmission = transactionsToResubmit.Count < batchSize
            ? transactionsToResubmit.Count
            : await GetMempoolTransactionsNeedingResubmission(instantForTransactionChoosing, mempoolOptions, dbContext)
                .CountAsync(token);

        if (_observer != null)
        {
            await _observer.TransactionsSelected(totalTransactionsNeedingResubmission);
        }

        if (totalTransactionsNeedingResubmission == 0)
        {
            _logger.Log(_emptyResubmissionQueueLogLimiter.GetLogLevel(), "There are no transactions needing resubmission");
        }
        else if (totalTransactionsNeedingResubmission <= batchSize)
        {
            _logger.LogInformation(
                "Preparing to resubmit all {TransactionsCount} transactions needing resubmission",
                totalTransactionsNeedingResubmission
            );
        }
        else
        {
            _logger.LogWarning(
                "There are {TotalCount} transactions needing resubmission, but we are only resubmitting {BatchSize} this batch",
                totalTransactionsNeedingResubmission,
                transactionsToResubmit.Count
            );
        }

        return transactionsToResubmit;
    }

    private List<MempoolTransactionWithChosenNode> MarkTransactionsAsFailedForTimeoutOrPendingResubmissionToRandomNode(
        MempoolOptions mempoolOptions,
        List<MempoolTransaction> transactionsWantingResubmission,
        Instant submittedAt
    )
    {
        var transactionsToResubmitWithNodes = new List<MempoolTransactionWithChosenNode>();

        foreach (var transaction in transactionsWantingResubmission)
        {
            var resubmissionLimit = transaction.LastSubmittedToGatewayTimestamp!.Value + mempoolOptions.StopResubmittingAfter;

            var canResubmit = submittedAt <= resubmissionLimit;

            if (canResubmit)
            {
                var nodeToSubmitTo = GetRandomCoreApi();

                _observer?.TransactionMarkedAsAssumedSuccessfullySubmittedToNode();

                transaction.MarkAsAssumedSuccessfullySubmittedToNode(nodeToSubmitTo.Name, submittedAt);
                transactionsToResubmitWithNodes.Add(new MempoolTransactionWithChosenNode(transaction, nodeToSubmitTo));
            }
            else
            {
                _observer?.TransactionMarkedAsFailed();

                transaction.MarkAsFailed(
                    MempoolTransactionFailureReason.Timeout,
                    "The transaction keeps dropping out of the mempool, so we're not resubmitting it"
                );
            }
        }

        return transactionsToResubmitWithNodes;
    }

    private record MempoolTransactionWithChosenNode(MempoolTransaction MempoolTransaction, CoreApiNode CoreApiNode);

    private IQueryable<MempoolTransaction> GetMempoolTransactionsNeedingResubmission(
        Instant currentTimestamp,
        MempoolOptions mempoolOptions,
        ReadWriteDbContext dbContext
    )
    {
        var allowResubmissionIfLastSubmittedBefore = currentTimestamp - mempoolOptions.MinDelayBetweenResubmissions;

        var allowResubmissionIfDroppedOutOfMempoolBefore = currentTimestamp - mempoolOptions.MinDelayBetweenMissingFromMempoolAndResubmission;

        var isEssentiallySyncedUpNow = _systemStatusService.IsTopOfDbLedgerValidatorCommitTimestampCloseToPresent(Duration.FromSeconds(60));

        return dbContext.MempoolTransactions
            .Where(mt =>
                mt.SubmittedByThisGateway
                && (
                    /* Transactions get marked Missing way by the MempoolTrackerService */
                    mt.Status == MempoolTransactionStatus.Missing

                    /* If we're synced up now, try submitting transactions with unknown status again. They almost
                       certainly failed due to a real double spend - so we'll detect it now and can mark them failed */
                    || (isEssentiallySyncedUpNow && mt.Status == MempoolTransactionStatus.ResolvedButUnknownTillSyncedUp)
                )
                && mt.LastDroppedOutOfMempoolTimestamp!.Value < allowResubmissionIfDroppedOutOfMempoolBefore
                && mt.LastSubmittedToNodeTimestamp!.Value < allowResubmissionIfLastSubmittedBefore
            );
    }

    private async Task ResubmitAllAndUpdateTransactionStatusesOnFailure(
        MempoolOptions mempoolOptions,
        List<MempoolTransactionWithChosenNode> transactionsToResubmitWithNodes,
        Instant submittedAt,
        CancellationToken token
    )
    {
        var submissionResults = await ResubmitAll(transactionsToResubmitWithNodes, token);

        foreach (var (transaction, failed, failureReason, failureExplanation, nodeName) in submissionResults)
        {
            if (!failed)
            {
                continue;
            }

            var isDoubleSpendWhichCouldBeItself =
                failureReason!.Value == MempoolTransactionFailureReason.DoubleSpend
                && !_systemStatusService.GivenClockDriftBoundIsTopOfDbLedgerValidatorCommitTimestampConfidentlyAfter(
                    mempoolOptions.AssumedBoundOnNetworkLedgerDataAggregatorClockDrift,
                    transaction.LastDroppedOutOfMempoolTimestamp!.Value
                );

            if (isDoubleSpendWhichCouldBeItself)
            {
                // If we're not synced up, we can't be sure if the double spend from the node is actually just the
                // transaction itself having already hit the ledger! Let's just assume it submitted correctly and
                // resubmit.

                if (_observer != null)
                {
                    await _observer.TransactionMarkedAsResolvedButUnknownAfterSubmittedToNode();
                }

                transaction.MarkAsResolvedButUnknownAfterSubmittedToNode(nodeName, submittedAt);
            }
            else
            {
                if (_observer != null)
                {
                    await _observer.TransactionMarkedAsFailedAfterSubmittedToNode();
                }

                transaction.MarkAsFailedAfterSubmittedToNode(nodeName, failureReason.Value, failureExplanation!, submittedAt);
            }
        }
    }

    private record SubmissionResult(
        MempoolTransaction MempoolTransaction,
        bool TransactionInvalid,
        MempoolTransactionFailureReason? FailureReason,
        string? SubmissionFailureExplanation,
        string NodeName
    );

    private async Task<SubmissionResult[]> ResubmitAll(List<MempoolTransactionWithChosenNode> transactionsToResubmit, CancellationToken token)
    {
        return await Task.WhenAll(transactionsToResubmit.Select(t => Resubmit(t, token)));
    }

    // NB - The error handling here should mirror the resubmission in ConstructionAndSubmissionService
    private async Task<SubmissionResult> Resubmit(MempoolTransactionWithChosenNode transactionWithNode, CancellationToken cancellationToken)
    {
        var transaction = transactionWithNode.MempoolTransaction;
        var chosenNode = transactionWithNode.CoreApiNode;
        var signedTransaction = transaction.Payload.ToHex();

        if (_observer != null)
        {
            await _observer.PreResubmit(signedTransaction);
        }

        using var nodeScope = _services.CreateScope();
        nodeScope.ServiceProvider.GetRequiredService<INodeConfigProvider>().CoreApiNode = chosenNode;
        var coreApiProvider = nodeScope.ServiceProvider.GetRequiredService<ICoreApiProvider>();

        var submitRequest = new ConstructionSubmitRequest(
            _networkConfigurationProvider.GetNetworkIdentifierForApiRequests(),
            signedTransaction
        );

        try
        {
            var result = await CoreApiErrorWrapper.ExtractCoreApiErrors(async () => await
                coreApiProvider.ConstructionApi.ConstructionSubmitPostAsync(submitRequest, cancellationToken)
            );

            if (_observer != null)
            {
                await _observer.PostResubmit(signedTransaction);
            }

            if (result.Duplicate)
            {
                if (_observer != null)
                {
                    await _observer.PostResubmitDuplicate(signedTransaction);
                }
            }
            else
            {
                if (_observer != null)
                {
                    await _observer.PostResubmitSucceeded(signedTransaction);
                }
            }

            return new SubmissionResult(transaction, false, null, null, chosenNode.Name);
        }
        catch (WrappedCoreApiException<SubstateDependencyNotFoundError> ex)
        {
            if (_observer != null)
            {
                await _observer.ResubmitFailedSubstateNotFound(signedTransaction, ex);
            }

            _logger.LogDebug(
                "Dropping transaction because a substate identifier it used is missing or already downed - possibly it's already been committed. Substate Identifier: {Substate}",
                ex.Error.SubstateIdentifierNotFound.Identifier
            );

            return new SubmissionResult(transaction, true, MempoolTransactionFailureReason.DoubleSpend, "Double spend on resubmission", chosenNode.Name);
        }
        catch (WrappedCoreApiException ex) when (ex.Properties.Transience == Transience.Permanent)
        {
            if (_observer != null)
            {
                await _observer.ResubmitFailedPermanently(signedTransaction, ex);
            }

            return new SubmissionResult(transaction, true, MempoolTransactionFailureReason.Unknown, $"Core API Exception: {ex.Error.GetType().Name} on resubmission", chosenNode.Name);
        }
        catch (OperationCanceledException ex)
        {
            if (_observer != null)
            {
                await _observer.ResubmitFailedTimeout(signedTransaction, ex);
            }

            return new SubmissionResult(transaction, false, null, null, chosenNode.Name);
        }
        catch (Exception ex)
        {
            // Unsure of what the problem is -- it could be that the connection died or there was an internal server error
            // We have to assume the submission may have succeeded and wait for resubmission if not.

            if (_observer != null)
            {
                await _observer.ResubmitFailedUnknown(signedTransaction, ex);
            }

            return new SubmissionResult(transaction, false, null, null, chosenNode.Name);
        }
    }

    private CoreApiNode GetRandomCoreApi()
    {
        return _networkOptionsMonitor.CurrentValue.CoreApiNodes
            .Where(n => n.Enabled && !n.DisabledForConstruction)
            .GetRandomBy(n => (double)n.RequestWeighting);
    }
}
