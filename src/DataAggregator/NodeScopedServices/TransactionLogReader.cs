using RadixNodeApi.GeneratedApiClient.Api;
using RadixNodeApi.GeneratedApiClient.Model;

namespace DataAggregator.NodeScopedServices;

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

    public async Task<List<CommittedTransaction>> GetTransactions(int stateVersion, int count)
    {
        var client = new DefaultApi(_nodeConfig.NodeAppSettings.Address);
        var results = await client.TransactionsPostAsync(new CommittedTransactionsRequest
        {
            StateVersion = stateVersion,
            Limit = count,
        });

        return results.Transactions;
    }
}
