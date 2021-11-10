using Common.Addressing;
using DataAggregator.Configuration;
using DataAggregator.Exceptions;
using DataAggregator.GlobalServices;
using DataAggregator.NodeScopedServices.ApiReaders;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.NodeScopedServices;

public class NodeNetworkConfigurationInitializer : INodeInitializer
{
    private readonly IAggregatorConfiguration _configuration;
    private readonly INetworkConfigurationReader _networkConfigurationReader;
    private readonly INetworkDetailsProvider _networkDetailsProvider;

    public NodeNetworkConfigurationInitializer(
        IAggregatorConfiguration configuration,
        INetworkConfigurationReader networkConfigurationReader,
        INetworkDetailsProvider networkDetailsProvider
    )
    {
        _configuration = configuration;
        _networkConfigurationReader = networkConfigurationReader;
        _networkDetailsProvider = networkDetailsProvider;
    }

    public async Task Initialize(CancellationToken token)
    {
        var networkConfiguration = await ReadNetworkConfigurationFromNode(token);

        if (_configuration.GetNetworkId() != networkConfiguration.NetworkIdentifier.Id)
        {
            throw new NodeInitializationException(
            $"The node's network id is {networkConfiguration.NetworkIdentifier.Id}, not {_configuration.GetNetworkId()}"
            );
        }

        _networkDetailsProvider.SetNetworkDetails(MapNetworkConfigurationToNetworkDetails(networkConfiguration));
    }

    private static NetworkDetails MapNetworkConfigurationToNetworkDetails(
        NetworkConfigurationResponse networkConfiguration
    )
    {
        var hrps = networkConfiguration.Bech32HumanReadableParts;
        return new NetworkDetails(
            networkConfiguration.NetworkIdentifier.Id,
            networkConfiguration.NetworkIdentifier.Name,
            new AddressHrps(hrps.AccountHrp, hrps.ResourceHrpSuffix, hrps.ValidatorHrp, hrps.NodeHrp)
        );
    }

    private async Task<NetworkConfigurationResponse> ReadNetworkConfigurationFromNode(CancellationToken token)
    {
        try
        {
            // Check we can connect to the node, and get the network configuration
            return await _networkConfigurationReader.GetNetworkConfiguration(token);
        }
        catch (Exception innerException)
        {
            throw new NodeInitializationException(
                $"Failed to connect / read details from node",
                innerException
            );
        }
    }
}
