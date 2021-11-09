using Common.Addressing;

namespace DataAggregator.GlobalServices;

public record NetworkDetails(int NetworkId, string NetworkName, AddressHrps AddressHrps);

public interface INetworkDetailsProvider
{
    void SetNetworkDetails(NetworkDetails networkDetails);

    NetworkDetails GetNetworkDetails();
}

public class NetworkDetailsProvider : INetworkDetailsProvider
{
    private readonly object _lock = new();
    private NetworkDetails? _networkDetails;

    public void SetNetworkDetails(NetworkDetails networkDetails)
    {
        lock (_lock)
        {
            _networkDetails = networkDetails;
        }
    }

    public NetworkDetails GetNetworkDetails()
    {
        lock (_lock)
        {
            var networkDetails = _networkDetails;
            if (networkDetails == null)
            {
                throw new Exception("Network Details have been read before they have been written.");
            }

            return networkDetails;
        }
    }
}
