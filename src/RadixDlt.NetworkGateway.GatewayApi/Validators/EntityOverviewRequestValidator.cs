using FluentValidation;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

internal class EntityOverviewRequestValidator : AbstractValidator<EntityOverviewRequest>
{
    public EntityOverviewRequestValidator(PartialLedgerStateIdentifierValidator partialLedgerStateIdentifierValidator)
    {
        RuleFor(x => x.AtStateIdentifier)
            .SetValidator(partialLedgerStateIdentifierValidator);

        RuleFor(x => x.Addresses)
            .NotEmpty()
            .DependentRules(() =>
            {
                RuleFor(x => x.Addresses.Count)
                    .GreaterThan(0)
                    .LessThan(20);

                RuleForEach(x => x.Addresses)
                    .Must((_, value, context) =>
                    {
                        if (!Bech32.IsBech32StringValid(value, out var error))
                        {
                            context.MessageFormatter.AppendArgument("Error", error);

                            return false;
                        }

                        return true;
                    }).WithMessage("{Error}");
            });
    }
}
