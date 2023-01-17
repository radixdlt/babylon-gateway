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
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.Abstractions.Workers;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

/// <summary>
/// Responsible for syncing the transaction stream from a node.
/// </summary>
public sealed class NodeTransactionLogWorker : NodeWorker
{
    private static readonly IDelayBetweenLoopsStrategy _delayBetweenLoopsStrategy =
        IDelayBetweenLoopsStrategy.ExponentialDelayStrategy(
            delayBetweenLoopTriggersIfSuccessful: TimeSpan.FromMilliseconds(200),
            baseDelayAfterError: TimeSpan.FromMilliseconds(1000),
            consecutiveErrorsAllowedBeforeExponentialBackoff: 1,
            delayAfterErrorExponentialRate: 2,
            maxDelayAfterError: TimeSpan.FromSeconds(30));

    /* Dependencies */
    private readonly ILogger<NodeTransactionLogWorker> _logger;
    private readonly ILedgerConfirmationService _ledgerConfirmationService;
    private readonly INodeConfigProvider _nodeConfigProvider;
    private readonly IServiceProvider _services;
    private readonly IEnumerable<INodeTransactionLogWorkerObserver> _observers;

    /* Properties */
    private string NodeName => _nodeConfigProvider.CoreApiNode.Name;

    // NB - So that we can get new transient dependencies (such as HttpClients) each iteration,
    //      we create these dependencies directly from the service provider each loop.
    public NodeTransactionLogWorker(
        ILogger<NodeTransactionLogWorker> logger,
        ILedgerConfirmationService ledgerConfirmationService,
        INodeConfigProvider nodeConfigProvider,
        IServiceProvider services,
        IEnumerable<INodeTransactionLogWorkerObserver> observers,
        IEnumerable<INodeWorkerObserver> nodeWorkerObservers,
        IClock clock
    )
        : base(logger, nodeConfigProvider.CoreApiNode.Name, _delayBetweenLoopsStrategy, TimeSpan.FromSeconds(60), nodeWorkerObservers, clock)
    {
        _logger = logger;
        _ledgerConfirmationService = ledgerConfirmationService;
        _nodeConfigProvider = nodeConfigProvider;
        _services = services;
        _observers = observers;
    }

    public override bool IsEnabledByNodeConfiguration()
    {
        return _nodeConfigProvider.CoreApiNode.Enabled && !_nodeConfigProvider.CoreApiNode.DisabledForTransactionIndexing;
    }

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            await FetchAndSubmitTransactions(cancellationToken);
        }
        catch (Exception ex)
        {
            await _observers.ForEachAsync(x => x.DoWorkFailed(NodeName, ex));

            throw;
        }
    }

    private async Task FetchAndSubmitTransactions(CancellationToken cancellationToken)
    {
        const int FetchMaxBatchSize = 1000;

        var networkStatus = await _services.GetRequiredService<INetworkStatusReader>().GetNetworkStatus(cancellationToken);
        var nodeLedgerTip = networkStatus.CurrentStateIdentifier.StateVersion;

        _ledgerConfirmationService.SubmitNodeNetworkStatus(NodeName, nodeLedgerTip, networkStatus.CurrentStateIdentifier.GetAccumulatorHashBytes());

        var toFetch = _ledgerConfirmationService.GetWhichTransactionsAreRequestedFromNode(NodeName);

        if (toFetch == null)
        {
            _logger.LogDebug(
                "No new transactions to fetch, sleeping for {DelayMs}ms",
                _delayBetweenLoopsStrategy.DelayAfterSuccess(ElapsedSinceLoopBeginning())
            );
            return;
        }

        var batchSize = Math.Min(
            (int)(toFetch.StateVersionInclusiveUpperBound - toFetch.StateVersionInclusiveLowerBound),
            FetchMaxBatchSize
        );

        var transactions = await FetchTransactionsFromCoreApiWithLogging(
            toFetch.StateVersionInclusiveLowerBound,
            batchSize,
            cancellationToken
        );

        if (transactions.Count == 0)
        {
            _logger.LogDebug(
                "No new transactions found, sleeping for {DelayMs}ms",
                _delayBetweenLoopsStrategy.DelayAfterSuccess(ElapsedSinceLoopBeginning())
            );
        }

        _ledgerConfirmationService.SubmitTransactionsFromNode(
            NodeName,
            transactions
        );
    }

    private async Task<List<CoreModel.CommittedTransaction>> FetchTransactionsFromCoreApiWithLogging(long fromStateVersion, int count, CancellationToken token)
    {
        _logger.LogDebug(
            "Fetching up to {TransactionCount} transactions from version {FromStateVersion} from the core api",
            count,
            fromStateVersion
        );

        var transactionStreamReader = _services.GetRequiredService<ITransactionStreamReader>();

        var (transactions, fetchTransactionsMs) = await CodeStopwatch.TimeInMs(async () =>
        {
            var response = await transactionStreamReader.GetTransactionStream(fromStateVersion, count, token);

            return response.Transactions;
        });

        await _observers.ForEachAsync(x => x.TransactionsFetched(NodeName, transactions, fetchTransactionsMs));

        if (transactions.Count > 0)
        {
            _logger.LogInformation(
                "Fetched {TransactionCount} transactions from version {FromStateVersion} from the core api in {FetchTransactionsMs}ms",
                transactions.Count,
                fromStateVersion,
                fetchTransactionsMs
            );
        }

        return transactions;
    }
}
