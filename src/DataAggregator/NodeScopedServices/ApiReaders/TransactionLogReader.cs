using RadixCoreApi.GeneratedClient.Api;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.NodeScopedServices.ApiReaders;

public interface ITransactionLogReader
{
    Task<List<CommittedTransaction>> GetTransactions(int stateVersion, int count);
}

public class TransactionLogReader : ITransactionLogReader
{
    private INodeConfigProvider _nodeConfig;

    public TransactionLogReader(INodeConfigProvider nodeConfig)
    {
        _nodeConfig = nodeConfig;
    }

    public async Task<List<CommittedTransaction>> GetTransactions(int transactionIndex, int count)
    {
        var client = new ApiApi(_nodeConfig.NodeAppSettings.Address);
        var results = await client.TransactionsPostAsync(new CommittedTransactionsRequest
        {
            Index = transactionIndex,
            Limit = count,
        });

        return results.Transactions;
    }
}
