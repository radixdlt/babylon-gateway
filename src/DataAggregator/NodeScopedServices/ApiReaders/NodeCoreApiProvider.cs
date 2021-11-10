using RadixCoreApi.GeneratedClient.Api;

namespace DataAggregator.NodeScopedServices.ApiReaders;

public interface INodeCoreApiProvider
{
    TransactionsApi TransactionsApi { get; }

    NetworkApi NetworkApi { get; }
}

public class NodeCoreApiProvider : INodeCoreApiProvider
{
    public TransactionsApi TransactionsApi { get; }

    public NetworkApi NetworkApi { get; }

    public NodeCoreApiProvider(INodeConfigProvider nodeConfig)
    {
        // Due to this creating a HttpClient, which is quite heavy, it suggests caching this.
        // The NodeCoreApiProvider is bound in the NodeWorker context, so will be renewed if the node is
        // disabled and re-enabled.
        TransactionsApi = new TransactionsApi(nodeConfig.NodeAppSettings.Address);
        NetworkApi = new NetworkApi(nodeConfig.NodeAppSettings.Address);
    }
}
