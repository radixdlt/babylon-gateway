using RadixCoreApi.GeneratedClient.Api;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.NodeScopedServices.ApiReaders;

public interface INetworkConfigurationReader
{
    Task<NetworkConfigurationResponse> GetNetworkConfiguration(CancellationToken token);
}

public class NetworkConfigurationReader : INetworkConfigurationReader
{
    private INodeCoreApiProvider _apiProvider;

    public NetworkConfigurationReader(INodeCoreApiProvider apiProvider)
    {
        _apiProvider = apiProvider;
    }

    public async Task<NetworkConfigurationResponse> GetNetworkConfiguration(CancellationToken token)
    {
        return await _apiProvider.NetworkApi
            .NetworkConfigurationPostAsync(new object(), token);
    }
}
