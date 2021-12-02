using Common.Addressing;
using Common.Database;
using Common.Database.Models.SingleEntries;
using GatewayAPI.Database;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;
using RadixGatewayApi.Generated.Model;

namespace GatewayAPI.Services;

public interface INetworkConfigurationProvider
{
    Task LoadNetworkConfigurationFromDatabase(GatewayReadOnlyDbContext dbContext, CancellationToken token);

    string GetNetworkName();

    NetworkIdentifier GetNetworkIdentifierForApiRequests();

    AddressHrps GetAddressHrps();

    string GetXrdAddress();

    TokenIdentifier GetXrdTokenIdentifier();
}

public class NetworkConfigurationProvider : INetworkConfigurationProvider
{
    private readonly object _writeLock = new();
    private CapturedConfig? _capturedConfig;

    private record CapturedConfig(
        NetworkConfiguration NetworkConfiguration,
        AddressHrps AddressHrps,
        NetworkIdentifier NetworkIdentifier,
        TokenIdentifier XrdTokenIdentifier
    );

    public async Task LoadNetworkConfigurationFromDatabase(GatewayReadOnlyDbContext dbContext, CancellationToken token)
    {
        var networkConfiguration = await GetCurrentLedgerNetworkConfigurationFromDb(dbContext, token);
        if (networkConfiguration == null)
        {
            throw new Exception("Can't set current configuration from database as it's not there");
        }

        EnsureNetworkConfigurationCaptured(networkConfiguration);
    }

    public string GetNetworkName()
    {
        return GetCapturedConfig().NetworkConfiguration.NetworkDefinition.NetworkName;
    }

    public NetworkIdentifier GetNetworkIdentifierForApiRequests()
    {
        return GetCapturedConfig().NetworkIdentifier;
    }

    public AddressHrps GetAddressHrps()
    {
        return GetCapturedConfig().AddressHrps;
    }

    public string GetXrdAddress()
    {
        return GetCapturedConfig().NetworkConfiguration.WellKnownAddresses.XrdAddress;
    }

    public TokenIdentifier GetXrdTokenIdentifier()
    {
        return GetCapturedConfig().XrdTokenIdentifier;
    }

    private CapturedConfig GetCapturedConfig()
    {
        return _capturedConfig ?? throw new Exception("Config hasn't been captured from a Node or from the Database yet.");
    }

    private void EnsureNetworkConfigurationCaptured(NetworkConfiguration inputNetworkConfiguration)
    {
        lock (_writeLock)
        {
            if (_capturedConfig != null)
            {
                return;
            }

            _capturedConfig = new CapturedConfig(
                inputNetworkConfiguration,
                inputNetworkConfiguration.NetworkAddressHrps.ToAddressHrps(),
                new NetworkIdentifier(inputNetworkConfiguration.NetworkDefinition.NetworkName),
                new TokenIdentifier(inputNetworkConfiguration.WellKnownAddresses.XrdAddress)
            );
        }
    }

    private async Task<NetworkConfiguration?> GetCurrentLedgerNetworkConfigurationFromDb(CommonDbContext dbContext, CancellationToken token)
    {
        return await dbContext.NetworkConfiguration.AsNoTracking().SingleOrDefaultAsync(token);
    }
}
