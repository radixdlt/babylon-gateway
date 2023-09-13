using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    List<CoreModel.CommittedTransaction> GetTransactionBatch(long fromStateVersion, int maxBatchSize);

    void StoreNodeTransactions(string nodeName, List<CoreModel.CommittedTransaction> transactions, int responseSize);

    bool ShouldFetchNewTransactions(string nodeName, long fromStateVersion);

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

    public List<CoreApiSdk.Model.CommittedTransaction> GetTransactionBatch(long fromStateVersion, int maxBatchSize)
    {
        var nodeWithMostTransactions = _transactionsByNode
            .Where(x => x.Value.ContainsKey(fromStateVersion))
            .OrderByDescending(x => x.Value.Count(y => y.Key >= fromStateVersion))
            .Select(x => x.Value)
            .FirstOrDefault();

        if (nodeWithMostTransactions == null)
        {
            return new List<CoreModel.CommittedTransaction>();
        }

        return nodeWithMostTransactions
            .Where(x => x.Key >= fromStateVersion)
            .OrderBy(x => x.Key)
            .Take(maxBatchSize)
            .Select(x => x.Value.CommittedTransaction)
            .ToList();
    }

    public bool ShouldFetchNewTransactions(string nodeName, long fromStateVersion)
    {
        var transactionStore = GetTransactionsStoreForNode(nodeName);

        var storedTransactionsSize = transactionStore
            .Where(x => x.Key >= fromStateVersion)
            .Aggregate(0, (sum, current) => sum + current.Value.EstimatedSize);

        var shouldFetchTransactions = storedTransactionsSize < Config.MaxEstimatedTransactionPipelineByteSizePerNode;
        if (!shouldFetchTransactions)
        {
            _logger.LogDebug(
                "Fetched transaction store is full. Not fetching new transactions. Store holds: {StoredTransactionsSize} max limit per node: {MaxEstimatedTransactionPipelineByteSizePerNode}",
                storedTransactionsSize, Config.MaxEstimatedTransactionPipelineByteSizePerNode);
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

        return Math.Max(lastKnownStateVersion, lastCommittedStateVersion + 1);
    }

    private ConcurrentDictionary<long, CommittedTransactionWithSize> GetTransactionsStoreForNode(string nodeName)
    {
        return _transactionsByNode.GetOrAdd(nodeName, new ConcurrentDictionary<long, CommittedTransactionWithSize>());
    }
}
