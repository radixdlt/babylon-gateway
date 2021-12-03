using Common.Addressing;
using GatewayAPI.Exceptions;
using GatewayAPI.Services;
using RadixGatewayApi.Generated.Model;

namespace GatewayAPI.ApiSurface;

public interface IValidations
{
    AccountAddress ExtractValidAccountAddress(AccountIdentifier accountIdentifier);

    void ValidateAccountAddress(AccountIdentifier accountIdentifier);

    ValidatorAddress ExtractValidValidatorAddress(ValidatorIdentifier validatorIdentifier);

    void ValidateValidatorAddress(ValidatorIdentifier validatorIdentifier);
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
            throw new InvalidAddressException("Account address is invalid", errorMessage);
        }

        return accountAddress;
    }

    // TODO:NG-56 - See if errors can be added as NewtonSoft validation errors against an account identifier, so they have a nicer context
    public void ValidateAccountAddress(AccountIdentifier accountIdentifier)
    {
        ExtractValidAccountAddress(accountIdentifier);
    }

    public ValidatorAddress ExtractValidValidatorAddress(ValidatorIdentifier validatorIdentifier)
    {
        if (!RadixAddressParser.TryParseValidatorAddress(
                _networkConfigurationProvider.GetAddressHrps(),
                validatorIdentifier.Address,
                out var validatorAddress,
                out var errorMessage
            ))
        {
            throw new InvalidAddressException("Validator address is invalid", errorMessage);
        }

        return validatorAddress;
    }

    // TODO:NG-56 - See if errors can be added as NewtonSoft validation errors against a validator identifier, so they have a nicer context
    public void ValidateValidatorAddress(ValidatorIdentifier validatorIdentifier)
    {
        ExtractValidValidatorAddress(validatorIdentifier);
    }
}
