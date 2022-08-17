using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

public interface INodeMempoolTransactionIdsReaderWorkerObserver
{
    ValueTask MempoolSize(string nodeName, int transactionIdentifiersCount);

    ValueTask MempoolItemsChange(string nodeName, int transactionIdsAddedCount, int transactionIdsRemovedCount);
}
