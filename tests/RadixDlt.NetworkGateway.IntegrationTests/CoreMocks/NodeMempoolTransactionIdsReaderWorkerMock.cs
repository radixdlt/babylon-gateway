using Moq;
using RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;
using System.Threading;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreMocks;

public class NodeMempoolTransactionIdsReaderWorkerMock : Mock<INodeMempoolTransactionIdsReaderWorker>
{
    public NodeMempoolTransactionIdsReaderWorkerMock()
    {
        Setup(x => x.FetchAndShareMempoolTransactions(It.IsAny<CancellationToken>()));
    }
}
