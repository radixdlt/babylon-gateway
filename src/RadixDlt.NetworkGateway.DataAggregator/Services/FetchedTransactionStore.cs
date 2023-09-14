using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface IFetchedTransactionStore
{
    void RemoveProcessedTransactions(long committedStateVersion);

    CoreModel.CommittedTransaction GetTransactionFromRandomNode(long stateVersion);

    void StoreNodeTransactions(string nodeName, List<CoreModel.CommittedTransaction> transactions, int responseSize);

    bool ShouldFetchNewTransactions(string nodeName, long fromStateVersion);

    long GetFirstStateVersionToFetch(string nodeName, long fromStateVersion);
}

public sealed class FetchedTransactionStore : IFetchedTransactionStore
{
    private record CommittedTransactionWithSize(CoreModel.CommittedTransaction CommittedTransaction, int EstimatedSize);

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, CommittedTransactionWithSize>> _transactionsByNode = new();

    public FetchedTransactionStore(IOptionsMonitor<LedgerConfirmationOptions> ledgerConfirmationOptionsMonitor)
    {
        Config = ledgerConfirmationOptionsMonitor.CurrentValue;
    }

    private LedgerConfirmationOptions Config { get; set; }

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

    public CoreApiSdk.Model.CommittedTransaction GetTransactionFromRandomNode(long stateVersion)
    {
        var nodeTransactions = _transactionsByNode
            .Where(x => x.Value.ContainsKey(stateVersion))
            .Select(x => x.Value)
            .FirstOrDefault();

        if (nodeTransactions == null || !nodeTransactions.TryGetValue(stateVersion, out var transaction))
        {
            throw new UnreachableException($"Transaction with state version: {stateVersion} not found.");
        }

        return transaction.CommittedTransaction;
    }

    public bool ShouldFetchNewTransactions(string nodeName, long fromStateVersion)
    {
        var transactionStore = GetTransactionsStoreForNode(nodeName);

        var storedTransactionsSize = transactionStore
            .Where(x => x.Key >= fromStateVersion)
            .Aggregate(0, (sum, current) => sum + current.Value.EstimatedSize);

        return storedTransactionsSize > Config.MaxEstimatedTransactionPipelineByteSizePerNode;
    }

    public long GetFirstStateVersionToFetch(string nodeName, long fromStateVersion)
    {
        var transactionStore = GetTransactionsStoreForNode(nodeName);

        var lastKnownStateVersion = transactionStore
            .Where(x => x.Key >= fromStateVersion)
            .Max(x => x.Key);

        return lastKnownStateVersion + 1;
    }

    private ConcurrentDictionary<long, CommittedTransactionWithSize> GetTransactionsStoreForNode(string nodeName)
    {
        return _transactionsByNode.GetOrAdd(nodeName, new ConcurrentDictionary<long, CommittedTransactionWithSize>());
    }
}
