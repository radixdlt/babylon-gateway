using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.NodeScopedServices.ApiReaders;

public interface ITransactionLogReader
{
    Task<List<CommittedTransaction>> GetTransactions(long stateVersion, int count, CancellationToken token);
}

public class TransactionLogReader : ITransactionLogReader
{
    private INodeCoreApiProvider _apiProvider;

    public TransactionLogReader(INodeCoreApiProvider apiProvider)
    {
        _apiProvider = apiProvider;
    }

    public async Task<List<CommittedTransaction>> GetTransactions(long stateVersion, int count, CancellationToken token)
    {
        var results = await _apiProvider.TransactionsApi
            .TransactionsPostAsync(
                new CommittedTransactionsRequest
                {
                    CommittedStateIdentifier = new CommittedTransactionsRequestCommittedStateIdentifier(stateVersion),
                    Limit = count,
                },
                token
            );

        return results.Transactions;
    }
}
