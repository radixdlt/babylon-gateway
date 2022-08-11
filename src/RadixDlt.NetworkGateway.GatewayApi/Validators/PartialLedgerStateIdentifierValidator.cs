using FluentValidation;
using RadixDlt.NetworkGateway.Core;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class PartialLedgerStateIdentifierValidator : AbstractValidator<PartialLedgerStateIdentifier>
{
    public PartialLedgerStateIdentifierValidator(ISystemClock systemClock)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .Must(x => x.HasStateVersion || x.HasTimestamp || x.HasEpoch).WithMessage("The property was not either (A) missing; (B) with only a State Version; (C) with only a Timestamp; (D) with only an Epoch; or (E) with only an Epoch and Round");

        RuleFor(x => x.StateVersion)
            .GreaterThan(0)
            .DependentRules(() =>
            {
                RuleFor(x => x.Timestamp).Null();
                RuleFor(x => x.Epoch).Null();
                RuleFor(x => x.Round).Null();
            })
            .When(x => x.StateVersion.HasValue);

        RuleFor(x => x.Timestamp)
            .LessThanOrEqualTo(systemClock.UtcNow)
            .DependentRules(() =>
            {
                RuleFor(x => x.StateVersion).Null();
                RuleFor(x => x.Epoch).Null();
                RuleFor(x => x.Round).Null();
            })
            .When(x => x.Timestamp.HasValue);

        RuleFor(x => x.Epoch)
            .GreaterThan(0)
            .DependentRules(() =>
            {
                RuleFor(x => x.StateVersion).Null();
                RuleFor(x => x.Timestamp).Null();
            })
            .When(x => x.Epoch.HasValue);

        RuleFor(x => x.Round)
            .GreaterThan(0)
            .DependentRules(() =>
            {
                RuleFor(x => x.StateVersion).Null();
                RuleFor(x => x.Timestamp).Null();
                RuleFor(x => x.Epoch).NotNull();
            })
            .When(x => x.Round.HasValue);
    }
}
