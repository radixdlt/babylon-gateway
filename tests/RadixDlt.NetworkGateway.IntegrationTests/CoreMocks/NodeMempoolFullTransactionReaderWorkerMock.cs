using Moq;
using RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;
using System.Threading;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreMocks;

public class NodeMempoolFullTransactionReaderWorkerMock : Mock<INodeMempoolFullTransactionReaderWorker>
{
    public NodeMempoolFullTransactionReaderWorkerMock()
    {
        Setup(x => x.FetchAndShareUnknownFullTransactions(It.IsAny<CancellationToken>()));
    }
}
