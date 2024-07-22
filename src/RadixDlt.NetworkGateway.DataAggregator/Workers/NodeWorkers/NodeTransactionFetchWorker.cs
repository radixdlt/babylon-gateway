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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions.CoreCommunications;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.Abstractions.Workers;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreClient = RadixDlt.CoreApiSdk.Client;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.Abstractions;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

public sealed class NodeTransactionFetchWorker : BaseNodeWorker
{
    private record struct FetchedTransactions(List<CoreModel.CommittedTransaction> Transactions, CoreModel.CommittedStateIdentifier PreviousStateIdentifiers, int ResponseSize);

    private static readonly IDelayBetweenLoopsStrategy _delayBetweenLoopsStrategy =
        IDelayBetweenLoopsStrategy.ExponentialDelayStrategy(
            delayBetweenLoopTriggersIfSuccessful: TimeSpan.FromMilliseconds(200),
            baseDelayAfterError: TimeSpan.FromMilliseconds(1000),
            consecutiveErrorsAllowedBeforeExponentialBackoff: 1,
            delayAfterErrorExponentialRate: 2,
            maxDelayAfterError: TimeSpan.FromSeconds(30));

    /* Dependencies */
    private readonly ILogger<NodeTransactionFetchWorker> _logger;
    private readonly IFetchedTransactionStore _fetchedTransactionStore;
    private readonly ICoreApiNodeProvider _coreApiNodeProvider;
    private readonly INodeStatusProvider _nodeStatusProvider;
    private readonly IEnumerable<INodeTransactionLogWorkerObserver> _observers;
    private readonly IOptionsMonitor<LedgerConfirmationOptions> _ledgerConfirmationOptionsMonitor;
    private readonly ICommittedStateIdentifiersReader _committedStateIdentifiersReader;
    private readonly Func<ITransactionStreamReader> _transactionStreamReaderFactory;
    private readonly Func<INetworkStatusReader> _networkStatusReaderFactory;
    private readonly ITopOfLedgerProvider _topOfLedgerProvider;

    /* Properties */
    private string NodeName => _coreApiNodeProvider.CoreApiNode.Name;

    public NodeTransactionFetchWorker(
        ILogger<NodeTransactionFetchWorker> logger,
        IFetchedTransactionStore fetchedTransactionStore,
        ICoreApiNodeProvider coreApiNodeProvider,
        IEnumerable<INodeTransactionLogWorkerObserver> observers,
        IEnumerable<INodeWorkerObserver> nodeWorkerObservers,
        GatewayModel.IClock clock,
        IOptionsMonitor<LedgerConfirmationOptions> ledgerConfirmationOptionsMonitor,
        INodeStatusProvider nodeStatusProvider,
        ICommittedStateIdentifiersReader committedStateIdentifiersReader,
        Func<ITransactionStreamReader> transactionStreamReaderFactory,
        Func<INetworkStatusReader> networkStatusReaderFactory,
        ITopOfLedgerProvider topOfLedgerProvider)
        : base(logger, coreApiNodeProvider.CoreApiNode.Name, _delayBetweenLoopsStrategy, TimeSpan.FromSeconds(60), nodeWorkerObservers, clock)
    {
        _logger = logger;
        _fetchedTransactionStore = fetchedTransactionStore;
        _coreApiNodeProvider = coreApiNodeProvider;
        _observers = observers;
        _ledgerConfirmationOptionsMonitor = ledgerConfirmationOptionsMonitor;
        _nodeStatusProvider = nodeStatusProvider;
        _committedStateIdentifiersReader = committedStateIdentifiersReader;
        _transactionStreamReaderFactory = transactionStreamReaderFactory;
        _networkStatusReaderFactory = networkStatusReaderFactory;
        _topOfLedgerProvider = topOfLedgerProvider;
    }

    public override bool IsEnabledByNodeConfiguration()
    {
        return _coreApiNodeProvider.CoreApiNode is { Enabled: true, DisabledForTransactionIndexing: false };
    }

    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            await FetchTransactions(cancellationToken);
        }
        catch (Exception ex)
        {
            await _observers.ForEachAsync(x => x.DoWorkFailed(NodeName, ex));
            throw;
        }
    }

    private async Task FetchTransactions(CancellationToken cancellationToken)
    {
        var fetchMaxBatchSize = _ledgerConfirmationOptionsMonitor.CurrentValue.MaxCoreApiTransactionBatchSize;
        var nodeStatus = await _networkStatusReaderFactory().GetNetworkStatus(cancellationToken);
        _nodeStatusProvider.UpdateNodeStatus(NodeName, nodeStatus);

        var transactionsToFetch = await GetTransactionsToFetch(NodeName, cancellationToken);

        if (transactionsToFetch == null)
        {
            _logger.LogDebug(
                "No new transactions to fetch, sleeping for {DelayMs}ms",
                _delayBetweenLoopsStrategy.DelayAfterSuccess(ElapsedSinceLoopBeginning())
            );
            return;
        }

        var batchSize = Math.Min(
            (int)(transactionsToFetch.Value.StateVersionInclusiveUpperBound - transactionsToFetch.Value.StateVersionInclusiveLowerBound),
            fetchMaxBatchSize
        );

        if (batchSize == 0)
        {
            _logger.LogDebug(
                "No transactions need fetching, sleeping for {DelayMs}ms",
                _delayBetweenLoopsStrategy.DelayAfterSuccess(ElapsedSinceLoopBeginning())
            );
            return;
        }

        var batch = await FetchTransactions(
            transactionsToFetch.Value.StateVersionInclusiveLowerBound,
            batchSize,
            cancellationToken
        );

        if (batch.Transactions.Count == 0)
        {
            _logger.LogDebug(
                "No new transactions found, sleeping for {DelayMs}ms",
                _delayBetweenLoopsStrategy.DelayAfterSuccess(ElapsedSinceLoopBeginning())
            );
            return;
        }

        var previousStateVersion = batch.Transactions[0].ResultantStateIdentifiers.StateVersion - 1;
        var knownPreviousStateIdentifiers = await GetKnownPreviousStateIdentifiers(previousStateVersion, cancellationToken);

        TransactionConsistencyValidator.ValidateHashes(previousStateVersion, knownPreviousStateIdentifiers, batch.PreviousStateIdentifiers);

        _fetchedTransactionStore.StoreNodeTransactions(
            NodeName,
            batch.Transactions,
            batch.ResponseSize
        );
    }

    private async Task<GatewayModel.CommittedStateIdentifiers?> GetKnownPreviousStateIdentifiers(long previousStateVersion, CancellationToken cancellationToken)
    {
        var fetchedStateIdentifiers = _fetchedTransactionStore.GetStateIdentifiersForStateVersion(previousStateVersion);
        if (fetchedStateIdentifiers != null)
        {
            return fetchedStateIdentifiers;
        }

        var storedStateIdentifiers = await _committedStateIdentifiersReader.GetStateIdentifiersForStateVersion(previousStateVersion, cancellationToken);
        if (storedStateIdentifiers != null)
        {
            return storedStateIdentifiers;
        }

        return null;
    }

    private async Task<FetchedTransactions> FetchTransactions(long fromStateVersion, int count, CancellationToken token)
    {
        _logger.LogDebug(
            "Fetching up to {TransactionCount} transactions from version {FromStateVersion} from the core api",
            count,
            fromStateVersion
        );

        var (apiResponse, fetchTransactionsMs) = await CodeStopwatch.TimeInMs(async () =>
        {
            return await CoreApiErrorWrapper.ResultOrError<CoreClient.ApiResponse<CoreModel.StreamTransactionsResponse>, CoreModel.StreamTransactionsErrorResponse>(() =>
            {
                return _transactionStreamReaderFactory().GetTransactionStream(fromStateVersion, count, token);
            });
        });

        if (apiResponse.Failed)
        {
            var ex = apiResponse.FailureResponse.OriginalApiException;

            if (apiResponse.FailureResponse.Details is CoreModel.RequestedStateVersionOutOfBoundsErrorDetails oob)
            {
                _logger.LogDebug(ex, "Requested state version past the maximal ledger state version of {MaxLedgerStateVersion}", oob.MaxLedgerStateVersion);

                return new FetchedTransactions(new List<CoreModel.CommittedTransaction>(), null!, 0);
            }

            throw apiResponse.FailureResponse.OriginalApiException;
        }

        var transactions = apiResponse.SuccessResponse.Data.Transactions;

        if (transactions.Count > 0)
        {
            _logger.LogInformation(
                "Fetched {TransactionCount} transactions from version {FromStateVersion} from the core api in {FetchTransactionsMs}ms",
                transactions.Count,
                fromStateVersion,
                fetchTransactionsMs
            );
        }

        await _observers.ForEachAsync(x => x.TransactionsFetched(NodeName, transactions, fetchTransactionsMs));

        return new FetchedTransactions(transactions, apiResponse.SuccessResponse.Data.PreviousStateIdentifiers, Encoding.UTF8.GetByteCount(apiResponse.SuccessResponse.RawContent));
    }

    private async Task<(long StateVersionInclusiveLowerBound, long StateVersionInclusiveUpperBound)?> GetTransactionsToFetch(string nodeName, CancellationToken cancellationToken)
    {
        var lastCommittedTransactionSummary = await _topOfLedgerProvider.GetTopOfLedger(cancellationToken);
        var processTransactionFromStateVersion = lastCommittedTransactionSummary.StateVersion;
        var maxUpperLimit = processTransactionFromStateVersion + _ledgerConfirmationOptionsMonitor.CurrentValue.MaxTransactionPipelineSizePerNode + 1;

        var shouldFetchNewTransactions = _fetchedTransactionStore.ShouldFetchNewTransactions(nodeName, processTransactionFromStateVersion);
        if (!shouldFetchNewTransactions)
        {
            return null;
        }

        var firstMissingTransaction = _fetchedTransactionStore.GetFirstStateVersionToFetch(nodeName, processTransactionFromStateVersion);
        return firstMissingTransaction > maxUpperLimit ? null : (firstMissingTransaction, maxUpperLimit);
    }
}
