using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.NodeScopedServices.ApiReaders;

public interface ITransactionLogReader
{
    Task<CommittedTransactionsResponse> GetTransactions(long stateVersion, int count, CancellationToken token);
}

public class TransactionLogReader : ITransactionLogReader
{
    private INodeCoreApiProvider _apiProvider;

    public TransactionLogReader(INodeCoreApiProvider apiProvider)
    {
        _apiProvider = apiProvider;
    }

    public async Task<CommittedTransactionsResponse> GetTransactions(long stateVersion, int count, CancellationToken token)
    {
        return await _apiProvider.TransactionsApi
            .TransactionsPostAsync(
                new CommittedTransactionsRequest
                {
                    CommittedStateIdentifier = new PartialCommittedStateIdentifier(stateVersion),
                    Limit = count,
                },
                token
            );
    }
}
