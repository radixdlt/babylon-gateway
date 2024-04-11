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
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.Abstractions;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface IFetchedTransactionStore
{
    void RemoveProcessedTransactions(long committedStateVersion);

    GatewayModel.CommittedStateIdentifiers? GetStateIdentifiersForStateVersion(long stateVersion);

    List<CoreModel.CommittedTransaction> GetTransactionBatch(long fromStateVersion, int maxBatchSize, int minBatchSize);

    void StoreNodeTransactions(string nodeName, List<CoreModel.CommittedTransaction> transactions, int responseSize);

    bool ShouldFetchNewTransactions(string nodeName, long committedStateVersion);

    long GetFirstStateVersionToFetch(string nodeName, long lastCommittedStateVersion);
}

public sealed class FetchedTransactionStore : IFetchedTransactionStore
{
    private record CommittedTransactionWithSize(CoreModel.CommittedTransaction CommittedTransaction, int EstimatedSize);

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, CommittedTransactionWithSize>> _transactionsByNode = new();
    private readonly ILogger<IFetchedTransactionStore> _logger;

    public FetchedTransactionStore(IOptionsMonitor<LedgerConfirmationOptions> ledgerConfirmationOptionsMonitor, ILogger<IFetchedTransactionStore> logger)
    {
        _logger = logger;
        Config = ledgerConfirmationOptionsMonitor.CurrentValue;
    }

    private LedgerConfirmationOptions Config { get; set; }

    public GatewayModel.CommittedStateIdentifiers? GetStateIdentifiersForStateVersion(long stateVersion)
    {
        var storeWithGivenTransaction = _transactionsByNode
            .Where(x => x.Value.ContainsKey(stateVersion))
            .Select(x => x.Value)
            .FirstOrDefault();

        if (storeWithGivenTransaction == null || !storeWithGivenTransaction.TryGetValue(stateVersion, out var transaction))
        {
            return null;
        }

        return new GatewayModel.CommittedStateIdentifiers(
            transaction.CommittedTransaction.ResultantStateIdentifiers.StateVersion,
            transaction.CommittedTransaction.ResultantStateIdentifiers.StateTreeHash,
            transaction.CommittedTransaction.ResultantStateIdentifiers.TransactionTreeHash,
            transaction.CommittedTransaction.ResultantStateIdentifiers.ReceiptTreeHash);
    }

    public void StoreNodeTransactions(string nodeName, List<CoreApiSdk.Model.CommittedTransaction> transactions, int responseSize)
    {
        if (!transactions.Any())
        {
            return;
        }

        var transactionStoreForNode = GetTransactionsStoreForNode(nodeName);
        var averageTransactionSize = responseSize / transactions.Count;

        foreach (var transaction in transactions)
        {
            transactionStoreForNode[transaction.ResultantStateIdentifiers.StateVersion] = new CommittedTransactionWithSize(transaction, averageTransactionSize);
        }
    }

    public void RemoveProcessedTransactions(long committedStateVersion)
    {
        foreach (var (_, transactions) in _transactionsByNode)
        {
            var stateVersionsToRemove = transactions.Keys.Where(k => k <= committedStateVersion).ToList();
            foreach (var stateVersion in stateVersionsToRemove)
            {
                transactions.TryRemove(stateVersion, out _);
            }
        }
    }

    public List<CoreApiSdk.Model.CommittedTransaction> GetTransactionBatch(long fromStateVersion, int maxBatchSize, int minBatchSize)
    {
        var nodeWithMostTransactions = _transactionsByNode
            .Where(x => x.Value.ContainsKey(fromStateVersion))
            .OrderByDescending(x => x.Value.Count(y => y.Key >= fromStateVersion))
            .FirstOrDefault();

        var nodeTransactions = nodeWithMostTransactions.Value;

        if (nodeTransactions == null)
        {
            return [];
        }

        var transactions = nodeTransactions
            .Where(x => x.Key >= fromStateVersion)
            .OrderBy(x => x.Key)
            .Take(maxBatchSize)
            .Select(x => x.Value.CommittedTransaction)
            .ToList();

        if (transactions.Count < minBatchSize)
        {
            _logger.LogDebug(
                "Number of fetched transaction {transactionCount} is smaller than configured minimum batch size {minBatchSize}. Waiting for more transactions.",
                transactions.Count,
                minBatchSize);

            if (!ShouldFetchNewTransactions(nodeWithMostTransactions.Key, fromStateVersion))
            {
                throw new ConfigurationException(
                    "Current configuration doesn't allow to process transaction stream. Transaction store is full and minimum batch size requires to fetch more transaction before processing. Please increase MaxEstimatedTransactionPipelineByteSizePerNode value or set MinCommitBatchSize to smaller value.");
            }

            return [];
        }

        return transactions;
    }

    public bool ShouldFetchNewTransactions(string nodeName, long committedStateVersion)
    {
        var transactionStore = GetTransactionsStoreForNode(nodeName);

        var storedTransactionsSize = transactionStore
            .Where(x => x.Key > committedStateVersion)
            .Aggregate(0, (sum, current) => sum + current.Value.EstimatedSize);

        var shouldFetchTransactions = storedTransactionsSize < Config.MaxEstimatedTransactionPipelineByteSizePerNode;
        if (!shouldFetchTransactions)
        {
            _logger.LogDebug(
                "Fetched transaction store is full. Not fetching new transactions. Store holds: {StoredTransactionsSize} max limit per node: {MaxEstimatedTransactionPipelineByteSizePerNode}",
                storedTransactionsSize,
                Config.MaxEstimatedTransactionPipelineByteSizePerNode);
        }

        return shouldFetchTransactions;
    }

    public long GetFirstStateVersionToFetch(string nodeName, long lastCommittedStateVersion)
    {
        var transactionStore = GetTransactionsStoreForNode(nodeName);

        var lastKnownStateVersion = transactionStore
            .Where(x => x.Key >= lastCommittedStateVersion)
            .Select(x => x.Key)
            .DefaultIfEmpty(0)
            .Max(x => x);

        return Math.Max(lastKnownStateVersion + 1, lastCommittedStateVersion + 1);
    }

    private ConcurrentDictionary<long, CommittedTransactionWithSize> GetTransactionsStoreForNode(string nodeName)
    {
        return _transactionsByNode.GetOrAdd(nodeName, new ConcurrentDictionary<long, CommittedTransactionWithSize>());
    }
}
