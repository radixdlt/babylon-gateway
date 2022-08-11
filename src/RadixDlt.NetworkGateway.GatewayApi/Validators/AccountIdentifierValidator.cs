using FluentValidation;
using RadixDlt.NetworkGateway.Core.Addressing;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class AccountIdentifierValidator : AbstractValidator<AccountIdentifier>
{
    public AccountIdentifierValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Address)
            .NotNull()
            .Length(8, 90)
            .Must((_, value, context) =>
            {
                if (!Bech32.IsBech32StringValid(value, out var error))
                {
                    context.MessageFormatter.AppendArgument("Error", error);

                    return false;
                }

                return true;
            }).WithMessage("{Error}");
    }
}
