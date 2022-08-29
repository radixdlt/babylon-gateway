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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons;
using RadixDlt.NetworkGateway.Commons.CoreCommunications;
using RadixDlt.NetworkGateway.Commons.Exceptions;
using RadixDlt.NetworkGateway.Commons.Extensions;
using RadixDlt.NetworkGateway.Commons.Utilities;
using RadixDlt.NetworkGateway.Commons.Workers;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

/// <summary>
/// Responsible for syncing unknown transaction contents from the mempool of a node.
///
/// Only needed when we care about tracking transactions which weren't submitted by the network.
/// </summary>
public class NodeMempoolFullTransactionReaderWorker : NodeWorker
{
    private static readonly IDelayBetweenLoopsStrategy _delayBetweenLoopsStrategy =
        IDelayBetweenLoopsStrategy.ExponentialDelayStrategy(
            delayBetweenLoopTriggersIfSuccessful: TimeSpan.FromMilliseconds(500),
            baseDelayAfterError: TimeSpan.FromMilliseconds(500),
            consecutiveErrorsAllowedBeforeExponentialBackoff: 1,
            delayAfterErrorExponentialRate: 2,
            maxDelayAfterError: TimeSpan.FromSeconds(30));

    private readonly ILogger<NodeMempoolFullTransactionReaderWorker> _logger;
    private readonly IServiceProvider _services;
    private readonly IOptionsMonitor<MempoolOptions> _mempoolOptionsMonitor;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IMempoolTrackerService _mempoolTrackerService;
    private readonly INodeConfigProvider _nodeConfig;
    private readonly IEnumerable<INodeMempoolFullTransactionReaderWorkerObserver> _observers;
    private readonly IClock _clock;

    // NB - So that we can get new transient dependencies each iteration (such as the HttpClients)
    //      we create such dependencies from the service provider.
    public NodeMempoolFullTransactionReaderWorker(
        ILogger<NodeMempoolFullTransactionReaderWorker> logger,
        IServiceProvider services,
        IOptionsMonitor<MempoolOptions> mempoolOptionsMonitor,
        INetworkConfigurationProvider networkConfigurationProvider,
        IMempoolTrackerService mempoolTrackerService,
        INodeConfigProvider nodeConfig,
        IEnumerable<INodeMempoolFullTransactionReaderWorkerObserver> observers,
        IEnumerable<INodeWorkerObserver> nodeWorkerObservers,
        IClock clock)
        : base(logger, nodeConfig.CoreApiNode.Name, _delayBetweenLoopsStrategy, TimeSpan.FromSeconds(60), nodeWorkerObservers, clock)
    {
        _logger = logger;
        _services = services;
        _mempoolOptionsMonitor = mempoolOptionsMonitor;
        _networkConfigurationProvider = networkConfigurationProvider;
        _mempoolTrackerService = mempoolTrackerService;
        _nodeConfig = nodeConfig;
        _observers = observers;
        _clock = clock;
    }

    public override bool IsEnabledByNodeConfiguration()
    {
        return _nodeConfig.CoreApiNode.Enabled
               && !_nodeConfig.CoreApiNode.DisabledForMempool
               && !_nodeConfig.CoreApiNode.DisabledForMempoolUnknownTransactionFetching
               && _mempoolOptionsMonitor.CurrentValue.TrackTransactionsNotSubmittedByThisGateway;
    }

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        await FetchAndShareUnknownFullTransactions(cancellationToken);
    }

    private async Task FetchAndShareUnknownFullTransactions(CancellationToken cancellationToken)
    {
        var mempoolConfiguration = _mempoolOptionsMonitor.CurrentValue;
        var coreApiProvider = _services.GetRequiredService<ICoreApiProvider>();

        // We duplicate this call from the TransactionIdsReader for simplicity, to mean the workers don't need
        // to sync up. This should be a cheap call to the Core API.
        var transactionIdentifiersInMempool = await CoreApiErrorWrapper.ExtractCoreApiErrors(async () => await coreApiProvider.MempoolApi.MempoolPostAsync(
            new MempoolRequest(
                _networkConfigurationProvider.GetNetworkIdentifierForApiRequests()
            ),
            cancellationToken
        ));

        var idsInMempool = transactionIdentifiersInMempool.TransactionIdentifiers
            .Select(ti => ti.Hash.ConvertFromHex())
            .ToList();

        if (idsInMempool.Count == 0)
        {
            return;
        }

        var transactionIdsToFetch = await _mempoolTrackerService.WhichTransactionsNeedContentFetching(idsInMempool, cancellationToken);

        if (transactionIdsToFetch.Count == 0)
        {
            return;
        }

        var (fetchAndSubmissionReport, transactionFetchMs) = await CodeStopwatch.TimeInMs(
            async () => await FetchAndSubmitEachTransactionContents(mempoolConfiguration, coreApiProvider, transactionIdsToFetch, cancellationToken)
        );

        _logger.LogInformation(
            "Full transaction contents fetched from node: {NonDuplicateCount} were new and {DuplicateCount} were already fetched by another node in the mean-time (fetched in {TransactionFetchMs}ms)",
            fetchAndSubmissionReport.NonDuplicateCount,
            fetchAndSubmissionReport.DuplicateCount,
            transactionFetchMs
        );
    }

    private record FetchAndSubmissionReport(int NonDuplicateCount, int DuplicateCount);

    private async Task<FetchAndSubmissionReport> FetchAndSubmitEachTransactionContents(
        MempoolOptions mempoolOptions,
        ICoreApiProvider coreApiProvider,
        HashSet<byte[]> transactionsToFetch,
        CancellationToken cancellationToken
    )
    {
        var fetchedNonDuplicateCount = 0;
        var fetchedDuplicateCount = 0;

        await Parallel.ForEachAsync(
            transactionsToFetch,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = mempoolOptions.FetchUnknownTransactionFromMempoolDegreeOfParallelizationPerNode,
                CancellationToken = cancellationToken,
            },
            async (transactionId, token) =>
            {
                if (!_mempoolTrackerService.TransactionContentsStillNeedFetching(transactionId))
                {
                    return;
                }

                var transactionData = await FetchTransactionContents(coreApiProvider, transactionId, token);

                if (transactionData != null)
                {
                    var wasDuplicate = !_mempoolTrackerService.SubmitTransactionContents(transactionData);

                    await _observers.ForEachAsync(x => x.FullTransactionsFetchedCount(_nodeConfig.CoreApiNode.Name, wasDuplicate));

                    if (wasDuplicate)
                    {
                        Interlocked.Increment(ref fetchedDuplicateCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref fetchedNonDuplicateCount);
                    }
                }
            });

        return new FetchAndSubmissionReport(fetchedNonDuplicateCount, fetchedDuplicateCount);
    }

    private async Task<FullTransactionData?> FetchTransactionContents(
        ICoreApiProvider coreApiProvider,
        byte[] transactionId,
        CancellationToken stoppingToken
    )
    {
        try
        {
            var response = await CoreApiErrorWrapper.ExtractCoreApiErrors(async () =>
                await coreApiProvider.MempoolApi.MempoolTransactionPostAsync(
                    new MempoolTransactionRequest(
                        _networkConfigurationProvider.GetNetworkIdentifierForApiRequests(),
                        new TransactionIdentifier(transactionId.ToHex())
                    ),
                    stoppingToken
                ));

            return new FullTransactionData(
                transactionId,
                _clock.UtcNow,
                response.Transaction.Metadata.Hex.ConvertFromHex(),
                response.Transaction
            );
        }
        catch (WrappedCoreApiException<TransactionNotFoundError>)
        {
            // It's likely dropped out of the mempool, so we can't fetch it
            return null;
        }
    }
}
