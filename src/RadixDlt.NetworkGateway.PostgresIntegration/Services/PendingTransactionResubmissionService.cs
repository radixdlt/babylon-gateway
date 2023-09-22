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
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
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
    private readonly IClock _clock;
    private readonly IReadOnlyCollection<IPendingTransactionResubmissionServiceObserver> _observers;
    private readonly ILogger<PendingTransactionResubmissionService> _logger;
    private readonly ITopOfLedgerProvider _topOfLedgerProvider;

    public PendingTransactionResubmissionService(
        IServiceProvider services,
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        IOptionsMonitor<NetworkOptions> networkOptionsMonitor,
        IOptionsMonitor<MempoolOptions> mempoolOptionsMonitor,
        INetworkConfigurationProvider networkConfigurationProvider,
        IClock clock,
        IEnumerable<IPendingTransactionResubmissionServiceObserver> observers,
        ILogger<PendingTransactionResubmissionService> logger,
        ITopOfLedgerProvider topOfLedgerProvider)
    {
        _services = services;
        _dbContextFactory = dbContextFactory;
        _networkOptionsMonitor = networkOptionsMonitor;
        _mempoolOptionsMonitor = mempoolOptionsMonitor;
        _networkConfigurationProvider = networkConfigurationProvider;
        _clock = clock;
        _observers = observers.ToArray();
        _logger = logger;
        _topOfLedgerProvider = topOfLedgerProvider;
    }

    public async Task RunBatchOfResubmissions(CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var instantForTransactionChoosing = _clock.UtcNow;
        var mempoolConfiguration = _mempoolOptionsMonitor.CurrentValue;
        var handlingConfig = new PendingTransactionHandlingConfig(mempoolConfiguration.MaxSubmissionAttempts, mempoolConfiguration.StopResubmittingAfter, mempoolConfiguration.BaseDelayBetweenResubmissions, mempoolConfiguration.ResubmissionDelayBackoffExponent);

        var transactionsToResubmit = await SelectTransactionsToResubmit(dbContext, instantForTransactionChoosing, mempoolConfiguration.ResubmissionBatchSize, token);

        var submittedAt = _clock.UtcNow;
        var currentEpoch = await GetCurrentEpoch(token);

        var transactionsToResubmitWithNodes = UpdateTransactionsForPotentialSubmissionOrRetirement(
            handlingConfig,
            transactionsToResubmit,
            currentEpoch,
            submittedAt
        );

        // We save as pending submission here to lock in the fact we're resubmitting - in case we hit an exception below and fail to resubmit.
        // By virtue of the ConcurrencyToken, this protects us against resubmitting the same transaction multiple times
        // - even if (eg) two data aggregators are running.
        await dbContext.SaveChangesAsync(token);

        await ResubmitAllAndUpdateTransactionStatusesOnFailure(
            transactionsToResubmitWithNodes,
            handlingConfig,
            currentEpoch,
            token);

        await dbContext.SaveChangesAsync(token);
    }

    private async Task<List<PendingTransaction>> SelectTransactionsToResubmit(
        ReadWriteDbContext dbContext,
        DateTime instantForTransactionChoosing,
        int batchSize,
        CancellationToken token
    )
    {
        var transactionsToResubmit =
            await GetPendingTransactionsNeedingResubmission(instantForTransactionChoosing, dbContext)
                .OrderBy(mt => mt.GatewayHandling.ResubmitFromTimestamp)
                .Include(t => t.Payload)
                .Take(batchSize)
                .ToListAsync(token);

        var totalTransactionsNeedingResubmission = transactionsToResubmit.Count < batchSize
            ? transactionsToResubmit.Count
            : await GetPendingTransactionsNeedingResubmission(instantForTransactionChoosing, dbContext)
                .CountAsync(token);

        await _observers.ForEachAsync(x => x.ObserveResubmissionQueueSize(totalTransactionsNeedingResubmission));

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

    private List<PendingTransactionWithChosenNode> UpdateTransactionsForPotentialSubmissionOrRetirement(
        PendingTransactionHandlingConfig handlingConfig,
        List<PendingTransaction> transactionsWantingResubmission,
        ulong currentEpoch,
        DateTime currentTime
    )
    {
        var transactionsToResubmitWithNodes = new List<PendingTransactionWithChosenNode>();

        foreach (var transaction in transactionsWantingResubmission)
        {
            var canResubmit = transaction.UpdateForPendingSubmissionOrRetirement(handlingConfig, currentTime, currentEpoch);

            if (canResubmit)
            {
                _observers.ForEach(x => x.TransactionMarkedAsSubmissionPending());
                transactionsToResubmitWithNodes.Add(new PendingTransactionWithChosenNode(transaction, GetRandomCoreApi()));
            }
            else
            {
                _observers.ForEach(x => x.TransactionMarkedAsNoLongerSubmitting());
            }
        }

        return transactionsToResubmitWithNodes;
    }

    private record PendingTransactionWithChosenNode(PendingTransaction PendingTransaction, CoreApiNode Node);

    private IQueryable<PendingTransaction> GetPendingTransactionsNeedingResubmission(DateTime currentTimestamp, ReadWriteDbContext dbContext)
    {
        return dbContext.PendingTransactions
            .Where(mt =>
                mt.GatewayHandling.ResubmitFromTimestamp != null && mt.GatewayHandling.ResubmitFromTimestamp < currentTimestamp
            );
    }

    private async Task ResubmitAllAndUpdateTransactionStatusesOnFailure(
        List<PendingTransactionWithChosenNode> transactionsToResubmitWithNodes,
        PendingTransactionHandlingConfig handlingConfig,
        ulong currentEpoch,
        CancellationToken token
    )
    {
        var submissionResults = await ResubmitAll(transactionsToResubmitWithNodes, token);
        var handledAt = _clock.UtcNow;

        foreach (var (transaction, nodeName, result) in submissionResults)
        {
            transaction.HandleNodeSubmissionResult(handlingConfig, nodeName, result, handledAt, currentEpoch);
        }
    }

    private record ContextualSubmissionResult(
        PendingTransaction PendingTransaction,
        string NodeName,
        NodeSubmissionResult NodeSubmissionResult
    );

    private async Task<ContextualSubmissionResult[]> ResubmitAll(List<PendingTransactionWithChosenNode> transactionsToResubmit, CancellationToken token)
    {
        return await Task.WhenAll(transactionsToResubmit.Select(t => Resubmit(t, token)));
    }

    // NB - The error handling here should mirror the resubmission in ConstructionAndSubmissionService
    private async Task<ContextualSubmissionResult> Resubmit(PendingTransactionWithChosenNode transactionWithNode, CancellationToken cancellationToken)
    {
        var transaction = transactionWithNode.PendingTransaction;
        var chosenNode = transactionWithNode.Node;
        var notarizedTransaction = transaction.Payload.NotarizedTransactionBlob;

        using var nodeScope = _services.CreateScope();
        nodeScope.ServiceProvider.GetRequiredService<INodeConfigProvider>().CoreApiNode = chosenNode;
        var coreApiProvider = nodeScope.ServiceProvider.GetRequiredService<ICoreApiProvider>();

        var result = await TransactionSubmitter.Submit(
            new SubmitContext(
                TransactionApi: coreApiProvider.TransactionsApi,
                NetworkName: _networkConfigurationProvider.GetNetworkName(),
                SubmissionTimeout: _mempoolOptionsMonitor.CurrentValue.ResubmissionNodeRequestTimeout,
                IsResubmission: true,
                ForceNodeToRecalculateResult: false),
            notarizedTransaction,
            _observers,
            cancellationToken
        );

        return new ContextualSubmissionResult(transaction, chosenNode.Name, result);
    }

    private CoreApiNode GetRandomCoreApi()
    {
        return _networkOptionsMonitor
            .CurrentValue
            .CoreApiNodes
            .Where(n => n.Enabled && !n.DisabledForConstruction)
            .GetRandomBy(n => (double)n.RequestWeighting);
    }

    private async Task<ulong> GetCurrentEpoch(CancellationToken cancellationToken)
    {
        var topOfLedger = await _topOfLedgerProvider.GetTopOfLedger(cancellationToken);
        var signedEpoch = topOfLedger.Epoch;
        return (signedEpoch >= 0) ? (ulong)signedEpoch : throw new InvalidStateException($"Epoch was negative: {signedEpoch}");
    }
}
