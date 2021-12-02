using Common.Addressing;
using GatewayAPI.Exceptions;
using GatewayAPI.Services;
using RadixGatewayApi.Generated.Model;

namespace GatewayAPI.ApiSurface;

public interface IValidations
{
    AccountAddress ExtractValidAccountAddress(AccountIdentifier accountIdentifier);

    void ValidateAccountAddress(AccountIdentifier accountIdentifier);
}

public class Validations : IValidations
{
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public Validations(INetworkConfigurationProvider networkConfigurationProvider)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public AccountAddress ExtractValidAccountAddress(AccountIdentifier accountIdentifier)
    {
        if (!RadixAddressParser.TryParseAccountAddress(
                _networkConfigurationProvider.GetAddressHrps(),
                accountIdentifier.Address,
                out var accountAddress,
                out var errorMessage
            ))
        {
            throw new InvalidAddressException("Address is invalid", errorMessage);
        }

        return accountAddress;
    }

    // TODO:NG-56 - See if errors can be added as NewtonSoft validation errors against an account identifier, so they have a nicer context
    public void ValidateAccountAddress(AccountIdentifier accountIdentifier)
    {
        ExtractValidAccountAddress(accountIdentifier);
    }
}
