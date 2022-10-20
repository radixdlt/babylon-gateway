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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.Abstractions.Workers;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

/// <summary>
/// Responsible for syncing the mempool from a node.
/// </summary>
public class NodeMempoolTransactionHashesReaderWorker : NodeWorker
{
    private static readonly IDelayBetweenLoopsStrategy _delayBetweenLoopsStrategy =
        IDelayBetweenLoopsStrategy.ExponentialDelayStrategy(
            delayBetweenLoopTriggersIfSuccessful: TimeSpan.FromMilliseconds(200),
            baseDelayAfterError: TimeSpan.FromMilliseconds(1000),
            consecutiveErrorsAllowedBeforeExponentialBackoff: 1,
            delayAfterErrorExponentialRate: 2,
            maxDelayAfterError: TimeSpan.FromSeconds(30));

    private readonly ILogger<NodeMempoolTransactionHashesReaderWorker> _logger;
    private readonly IServiceProvider _services;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IPendingTransactionTrackerService _pendingTransactionTrackerService;
    private readonly INodeConfigProvider _nodeConfig;
    private readonly IEnumerable<INodeMempoolTransactionHashesReaderWorkerObserver> _observers;
    private readonly IClock _clock;

    private HashSet<byte[]> _latestTransactionHashes = new(ByteArrayEqualityComparer.Default);

    // NB - So that we can get new transient dependencies each iteration, we create most dependencies
    //      from the service provider.
    public NodeMempoolTransactionHashesReaderWorker(
        ILogger<NodeMempoolTransactionHashesReaderWorker> logger,
        IServiceProvider services,
        INetworkConfigurationProvider networkConfigurationProvider,
        IPendingTransactionTrackerService pendingTransactionTrackerService,
        INodeConfigProvider nodeConfig,
        IEnumerable<INodeMempoolTransactionHashesReaderWorkerObserver> observers,
        IEnumerable<INodeWorkerObserver> nodeWorkerObservers,
        IClock clock
    )
        : base(logger, nodeConfig.CoreApiNode.Name, _delayBetweenLoopsStrategy, TimeSpan.FromSeconds(60), nodeWorkerObservers, clock)
    {
        _logger = logger;
        _services = services;
        _networkConfigurationProvider = networkConfigurationProvider;
        _pendingTransactionTrackerService = pendingTransactionTrackerService;
        _nodeConfig = nodeConfig;
        _observers = observers;
        _clock = clock;
    }

    public override bool IsEnabledByNodeConfiguration()
    {
        var nodeConfig = _services.GetRequiredService<INodeConfigProvider>();
        return nodeConfig.CoreApiNode.Enabled && !nodeConfig.CoreApiNode.DisabledForMempool;
    }

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        await FetchAndShareMempoolTransactions(cancellationToken);
    }

    private async Task FetchAndShareMempoolTransactions(CancellationToken stoppingToken)
    {
        var coreApiProvider = _services.GetRequiredService<ICoreApiProvider>();

        var (mempoolListResponse, fetchMs) = await CodeStopwatch.TimeInMs(async () =>
            await CoreApiErrorWrapper.ExtractCoreApiErrors(async () => await coreApiProvider.MempoolApi.MempoolListPostAsync(
                new CoreModel.MempoolListRequest(
                    network: _networkConfigurationProvider.GetNetworkName()
                ),
                stoppingToken
            ))
        );

        await _observers.ForEachAsync(x => x.MempoolSize(_nodeConfig.CoreApiNode.Name, mempoolListResponse.Contents.Count));

        // TODO are we sure we want to operate on PayloadHash alone?
        var latestMempoolHashes = mempoolListResponse.Contents
            .Select(th => th.PayloadHash.ConvertFromHex())
            .ToHashSet(ByteArrayEqualityComparer.Default);

        var previousMempoolHashes = _latestTransactionHashes;
        _latestTransactionHashes = latestMempoolHashes;

        var transactionIdsRemovedCount = previousMempoolHashes
            .ExceptInSet(latestMempoolHashes)
            .Count();

        var transactionIdsAddedCount = latestMempoolHashes
            .ExceptInSet(previousMempoolHashes)
            .Count();

        await _observers.ForEachAsync(x => x.MempoolItemsChange(_nodeConfig.CoreApiNode.Name, transactionIdsAddedCount, transactionIdsRemovedCount));

        if (transactionIdsAddedCount > 0 || transactionIdsRemovedCount > 0)
        {
            _logger.LogInformation(
                "Node mempool hashes updated: {TransactionsAdded} txns added and {TransactionsRemoved} txns removed (fetched in {TransactionFetchMs}ms)",
                transactionIdsAddedCount,
                transactionIdsRemovedCount,
                fetchMs
            );
        }

        _pendingTransactionTrackerService.RegisterNodeMempoolHashes(_nodeConfig.CoreApiNode.Name, new NodeMempoolHashes(_latestTransactionHashes, _clock.UtcNow));
    }
}
