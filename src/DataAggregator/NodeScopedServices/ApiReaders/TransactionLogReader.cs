using RadixCoreApi.GeneratedClient.Api;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.NodeScopedServices.ApiReaders;

public interface ITransactionLogReader
{
    Task<List<CommittedTransaction>> GetTransactions(int stateVersion, int count);
}

public class TransactionLogReader : ITransactionLogReader
{
    private INodeCoreApiProvider _apiProvider;

    public TransactionLogReader(INodeCoreApiProvider apiProvider)
    {
        _apiProvider = apiProvider;
    }

    public async Task<List<CommittedTransaction>> GetTransactions(int transactionIndex, int count)
    {
        var results = await _apiProvider.GetCoreApiClient()
            .TransactionsPostAsync(new CommittedTransactionsRequest
            {
                Index = transactionIndex,
                Limit = count,
            });

        return results.Transactions;
    }
}
