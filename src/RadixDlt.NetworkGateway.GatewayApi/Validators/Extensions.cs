using FluentValidation;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public static class Extensions
{
    public static IRuleBuilderOptions<T, string?> Base64<T>(this IRuleBuilder<T, string?> ruleBuilder, int? expectedLength = null)
    {
        return ruleBuilder.SetValidator(new Base64Validator<T>(expectedLength));
    }

    public static IRuleBuilderOptions<T, string?> Hex<T>(this IRuleBuilder<T, string?> ruleBuilder, int? expectedLength = null)
    {
        return ruleBuilder.SetValidator(new HexValidator<T>(expectedLength));
    }
}
