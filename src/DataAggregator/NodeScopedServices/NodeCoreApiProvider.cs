using RadixCoreApi.GeneratedClient.Api;

namespace DataAggregator.NodeScopedServices;

public interface INodeCoreApiProvider
{
    ApiApi GetCoreApiClient();
}

public class NodeCoreApiProvider : INodeCoreApiProvider
{
    private readonly ApiApi _coreApi;

    public NodeCoreApiProvider(INodeConfigProvider nodeConfig)
    {
        // Due to this creating a HttpClient, which is quite heavy, it suggests caching this.
        // The NodeCoreApiProvider is bound in the NodeWorker context, so will be renewed if the node is
        // disabled and re-enabled.
        _coreApi = new ApiApi(nodeConfig.NodeAppSettings.Address);
    }

    public ApiApi GetCoreApiClient()
    {
        return _coreApi;
    }
}
