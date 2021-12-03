using Common.Database.Models.Ledger.Normalization;
using Common.Numerics;
using Api = RadixGatewayApi.Generated.Model;

namespace GatewayAPI.ApiSurface;

public static class ApiIdentifiers
{
    public static Api.TokenAmount AsApiTokenAmount(this TokenAmount tokenAmount, Api.TokenIdentifier tokenIdentifier)
    {
        return new Api.TokenAmount(tokenAmount.ToSubUnitString(), tokenIdentifier);
    }

    public static Api.TokenAmount AsApiTokenAmount(this TokenAmount tokenAmount, string rri)
    {
        return new Api.TokenAmount(tokenAmount.ToSubUnitString(), rri.AsTokenIdentifier());
    }

    public static Api.ValidatorIdentifier AsValidatorIdentifier(this Validator validator)
    {
        return new Api.ValidatorIdentifier(validator.Address);
    }

    public static Api.ValidatorIdentifier AsValidatorIdentifier(this string validatorAddress)
    {
        return new Api.ValidatorIdentifier(validatorAddress);
    }

    public static Api.TokenIdentifier AsTokenIdentifier(this Resource resource)
    {
        return new Api.TokenIdentifier(resource.ResourceIdentifier);
    }

    public static Api.TokenIdentifier AsTokenIdentifier(this string rri)
    {
        return new Api.TokenIdentifier(rri);
    }

    public static Api.AccountIdentifier AsAccountIdentifier(this Account account)
    {
        return new Api.AccountIdentifier(account.Address);
    }

    public static Api.AccountIdentifier AsAccountIdentifier(this string accountAddress)
    {
        return new Api.AccountIdentifier(accountAddress);
    }

    public static Api.AccountIdentifier? AsOptionalAccountIdentifier(this Account? account)
    {
        return account == null ? null : new Api.AccountIdentifier(account.Address);
    }

    public static Api.AccountIdentifier? AsOptionalAccountIdentifier(this string? accountAddress)
    {
        return accountAddress == null ? null : new Api.AccountIdentifier(accountAddress);
    }
}
