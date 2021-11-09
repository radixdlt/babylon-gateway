using Common.Addressing;

namespace DataAggregator.GlobalServices;

public interface IAddressExtractor
{
    RadixAddress? Extract(string? address);
}

public class AddressExtractor : IAddressExtractor
{
    private readonly ILogger<AddressExtractor> _logger;
    private readonly INetworkDetailsProvider _networkDetailsProvider;

    public AddressExtractor(ILogger<AddressExtractor> logger, INetworkDetailsProvider networkDetailsProvider)
    {
        _logger = logger;
        _networkDetailsProvider = networkDetailsProvider;
    }

    public RadixAddress? Extract(string? address)
    {
        if (string.IsNullOrEmpty(address))
        {
            return null;
        }

        if (RadixAddressParser.TryParse(
            _networkDetailsProvider.GetNetworkDetails().AddressHrps,
            address,
            out var radixAddress,
            out var errorMessage
        ))
        {
            return radixAddress!;
        }

        _logger.LogWarning("Address [{Address}] didn't parse correctly: {ErrorMessage}", address, errorMessage);
        return null;
    }
}
