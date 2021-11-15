using DataAggregator.GlobalServices;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.NodeScopedServices.ApiReaders;

public interface ITransactionLogReader
{
    Task<CommittedTransactionsResponse> GetTransactions(long stateVersion, int count, CancellationToken token);
}

public class TransactionLogReader : ITransactionLogReader
{
    private readonly INetworkDetailsProvider _networkDetailsProvider;
    private INodeCoreApiProvider _apiProvider;

    public TransactionLogReader(INetworkDetailsProvider networkDetailsProvider, INodeCoreApiProvider apiProvider)
    {
        _networkDetailsProvider = networkDetailsProvider;
        _apiProvider = apiProvider;
    }

    public async Task<CommittedTransactionsResponse> GetTransactions(long stateVersion, int count, CancellationToken token)
    {
        return await _apiProvider.TransactionsApi
            .TransactionsPostAsync(
                new CommittedTransactionsRequest(
                    networkIdentifier: _networkDetailsProvider.GetNetworkIdentifierForApiRequests(),
                    stateIdentifier: new PartialStateIdentifier(stateVersion),
                    limit: count
                ),
                token
            );
    }
}
