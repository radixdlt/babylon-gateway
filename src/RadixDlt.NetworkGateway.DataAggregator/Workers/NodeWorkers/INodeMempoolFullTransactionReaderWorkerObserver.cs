using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

public interface INodeMempoolFullTransactionReaderWorkerObserver
{
    ValueTask FullTransactionsFetchedCount(string nodeName, bool wasDuplicate);
}
