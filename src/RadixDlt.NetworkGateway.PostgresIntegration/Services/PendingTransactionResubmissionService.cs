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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using RadixDlt.NetworkGateway.Abstractions.Exceptions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class PendingTransactionResubmissionService : IPendingTransactionResubmissionService
{
    private static readonly LogLimiter _emptyResubmissionQueueLogLimiter = new(TimeSpan.FromSeconds(60), LogLevel.Information, LogLevel.Debug);

    private readonly IServiceProvider _services;
    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly IOptionsMonitor<NetworkOptions> _networkOptionsMonitor;
    private readonly IOptionsMonitor<MempoolOptions> _mempoolOptionsMonitor;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ISystemStatusService _systemStatusService;
    private readonly IClock _clock;
    private readonly IEnumerable<IPendingTransactionResubmissionServiceObserver> _observers;
    private readonly ILogger _logger;

    public PendingTransactionResubmissionService(
        IServiceProvider services,
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        IOptionsMonitor<NetworkOptions> networkOptionsMonitor,
        IOptionsMonitor<MempoolOptions> mempoolOptionsMonitor,
        INetworkConfigurationProvider networkConfigurationProvider,
        ISystemStatusService systemStatusService,
        IClock clock,
        IEnumerable<IPendingTransactionResubmissionServiceObserver> observers,
        ILogger<PendingTransactionResubmissionService> logger)
    {
        _services = services;
        _dbContextFactory = dbContextFactory;
        _networkOptionsMonitor = networkOptionsMonitor;
        _mempoolOptionsMonitor = mempoolOptionsMonitor;
        _networkConfigurationProvider = networkConfigurationProvider;
        _systemStatusService = systemStatusService;
        _clock = clock;
        _observers = observers;
        _logger = logger;
    }

    public async Task RunBatchOfResubmissions(CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        const int BatchSize = 30;

        var instantForTransactionChoosing = _clock.UtcNow;
        var mempoolConfiguration = _mempoolOptionsMonitor.CurrentValue;

        var transactionsToResubmit = await SelectTransactionsToResubmit(dbContext, instantForTransactionChoosing, mempoolConfiguration, BatchSize, token);

        var submittedAt = _clock.UtcNow;

        // The timeout should be relative to the submittedAt time we save to the DB, so needs to include the time for initial db saving (which should be very quick).
        using var ctsWithSubmissionTimeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        ctsWithSubmissionTimeout.CancelAfter(mempoolConfiguration.ResubmissionNodeRequestTimeout);

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

    private async Task<List<PendingTransaction>> SelectTransactionsToResubmit(
        ReadWriteDbContext dbContext,
        DateTime instantForTransactionChoosing,
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

        await _observers.ForEachAsync(x => x.TransactionsSelected(totalTransactionsNeedingResubmission));

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

    private List<PendingTransactionWithChosenNode> MarkTransactionsAsFailedForTimeoutOrPendingResubmissionToRandomNode(
        MempoolOptions mempoolConfiguration,
        List<PendingTransaction> transactionsWantingResubmission,
        DateTime submittedAt
    )
    {
        var transactionsToResubmitWithNodes = new List<PendingTransactionWithChosenNode>();

        foreach (var transaction in transactionsWantingResubmission)
        {
            var resubmissionLimit = transaction.LastSubmittedToGatewayTimestamp!.Value + mempoolConfiguration.StopResubmittingAfter;

            var canResubmit = submittedAt <= resubmissionLimit;

            if (canResubmit)
            {
                var nodeToSubmitTo = GetRandomCoreApi();

                _observers.ForEach(x => x.TransactionMarkedAsAssumedSuccessfullySubmittedToNode());

                transaction.MarkAsAssumedSuccessfullySubmittedToNode(nodeToSubmitTo.Name, submittedAt);
                transactionsToResubmitWithNodes.Add(new PendingTransactionWithChosenNode(transaction, nodeToSubmitTo));
            }
            else
            {
                _observers.ForEach(x => x.TransactionMarkedAsFailed());

                transaction.MarkAsRejected(
                    true,
                    PendingTransactionFailureReason.Timeout,
                    "The transaction keeps dropping out of the mempool, so we're not resubmitting it",
                    submittedAt
                );
            }
        }

        return transactionsToResubmitWithNodes;
    }

    private record PendingTransactionWithChosenNode(PendingTransaction PendingTransaction, CoreApiNode Node);

    private IQueryable<PendingTransaction> GetMempoolTransactionsNeedingResubmission(DateTime currentTimestamp, MempoolOptions mempoolOptions, ReadWriteDbContext dbContext)
    {
        var allowResubmissionIfLastSubmittedBefore = currentTimestamp - mempoolOptions.MinDelayBetweenResubmissions;
        var allowResubmissionIfDroppedOutOfMempoolBefore = currentTimestamp - mempoolOptions.MinDelayBetweenMissingFromMempoolAndResubmission;
        var isEssentiallySyncedUpNow = _systemStatusService.IsTopOfDbLedgerValidatorCommitTimestampCloseToPresent(TimeSpan.FromSeconds(60));

        return dbContext.PendingTransactions
            .Where(mt =>
                mt.SubmittedByThisGateway
                && (
                    /* Transactions get marked Missing way by the MempoolTrackerService */
                    mt.Status == PendingTransactionStatus.Missing

                    /* If we're synced up now, try submitting transactions with unknown status again. They almost
                       certainly failed due to a real double spend - so we'll detect it now and can mark them failed */
                    || (isEssentiallySyncedUpNow && mt.Status == PendingTransactionStatus.ResolvedButUnknownTillSyncedUp)
                )
                && mt.LastDroppedOutOfMempoolTimestamp!.Value < allowResubmissionIfDroppedOutOfMempoolBefore
                && mt.LastSubmittedToNodeTimestamp!.Value < allowResubmissionIfLastSubmittedBefore
            );
    }

    private async Task ResubmitAllAndUpdateTransactionStatusesOnFailure(
        MempoolOptions mempoolOptions,
        List<PendingTransactionWithChosenNode> transactionsToResubmitWithNodes,
        DateTime submittedAt,
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
                failureReason!.Value == PendingTransactionFailureReason.DoubleSpend
                && !_systemStatusService.GivenClockDriftBoundIsTopOfDbLedgerValidatorCommitTimestampConfidentlyAfter(
                    mempoolOptions.AssumedBoundOnNetworkLedgerDataAggregatorClockDrift,
                    transaction.LastDroppedOutOfMempoolTimestamp!.Value
                );

            if (isDoubleSpendWhichCouldBeItself)
            {
                // If we're not synced up, we can't be sure if the double spend from the node is actually just the
                // transaction itself having already hit the ledger! Let's just assume it submitted correctly and
                // resubmit.

                await _observers.ForEachAsync(x => x.TransactionMarkedAsResolvedButUnknownAfterSubmittedToNode());

                transaction.MarkAsResolvedButUnknownAfterSubmittedToNode(nodeName, submittedAt);
            }
            else
            {
                await _observers.ForEachAsync(x => x.TransactionMarkedAsFailedAfterSubmittedToNode());

                transaction.MarkAsFailedAfterSubmittedToNode(false, nodeName, failureReason.Value, failureExplanation!, submittedAt, _clock.UtcNow);
            }
        }
    }

    private record SubmissionResult(
        PendingTransaction MempoolTransaction,
        bool TransactionInvalid,
        PendingTransactionFailureReason? FailureReason,
        string? SubmissionFailureExplanation,
        string NodeName
    );

    private async Task<SubmissionResult[]> ResubmitAll(List<PendingTransactionWithChosenNode> transactionsToResubmit, CancellationToken token)
    {
        return await Task.WhenAll(transactionsToResubmit.Select(t => Resubmit(t, token)));
    }

    // NB - The error handling here should mirror the resubmission in ConstructionAndSubmissionService
    private async Task<SubmissionResult> Resubmit(PendingTransactionWithChosenNode transactionWithNode, CancellationToken cancellationToken)
    {
        var transaction = transactionWithNode.PendingTransaction;
        var chosenNode = transactionWithNode.Node;
        var notarizedTransaction = transaction.NotarizedTransactionBlob;

        await _observers.ForEachAsync(x => x.PreResubmit(notarizedTransaction));

        using var nodeScope = _services.CreateScope();
        nodeScope.ServiceProvider.GetRequiredService<INodeConfigProvider>().CoreApiNode = chosenNode;
        var coreApiProvider = nodeScope.ServiceProvider.GetRequiredService<ICoreApiProvider>();

        var submitRequest = new CoreModel.TransactionSubmitRequest(
            network: _networkConfigurationProvider.GetNetworkName(),
            notarizedTransactionHex: notarizedTransaction.ToHex()
        );

        try
        {
            var result = await CoreApiErrorWrapper.ExtractCoreApiErrors(async () => await
                coreApiProvider.TransactionsApi.TransactionSubmitPostAsync(submitRequest, cancellationToken)
            );

            await _observers.ForEachAsync(x => x.PostResubmit(notarizedTransaction));

            if (result.Duplicate)
            {
                await _observers.ForEachAsync(x => x.PostResubmitDuplicate(notarizedTransaction));
            }
            else
            {
                await _observers.ForEachAsync(x => x.PostResubmitSucceeded(notarizedTransaction));
            }

            return new SubmissionResult(transaction, false, null, null, chosenNode.Name);
        }
        catch (WrappedCoreApiException ex) when (ex.Properties.Transience == CoreApiErrorTransience.Permanent)
        {
            await _observers.ForEachAsync(x => x.ResubmitFailedPermanently(notarizedTransaction, ex));

            var failureExplanation = $"Core API Exception: {ex.Error.GetType().Name} on resubmission";

            return new SubmissionResult(transaction, true, PendingTransactionFailureReason.Unknown, failureExplanation, chosenNode.Name);
        }
        catch (OperationCanceledException ex)
        {
            await _observers.ForEachAsync(x => x.ResubmitFailedTimeout(notarizedTransaction, ex));

            return new SubmissionResult(transaction, false, null, null, chosenNode.Name);
        }
        catch (Exception ex)
        {
            await _observers.ForEachAsync(x => x.ResubmitFailedUnknown(notarizedTransaction, ex));

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
