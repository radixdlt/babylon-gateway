using FluentValidation;
using RadixDlt.NetworkGateway.Common;
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
                const string NullMessage = "'{PropertyName}' must be null when 'state_version' is defined.";

                RuleFor(x => x.Timestamp).Null().WithMessage(NullMessage);
                RuleFor(x => x.Epoch).Null().WithMessage(NullMessage);
                RuleFor(x => x.Round).Null().WithMessage(NullMessage);
            })
            .When(x => x.StateVersion.HasValue);

        RuleFor(x => x.Timestamp)
            .LessThanOrEqualTo(systemClock.UtcNow).WithMessage("'{PropertyName}' must not be in future.")
            .DependentRules(() =>
            {
                const string NullMessage = "'{PropertyName}' must be null when 'timestamp' is defined.";

                RuleFor(x => x.StateVersion).Null().WithMessage(NullMessage);
                RuleFor(x => x.Epoch).Null().WithMessage(NullMessage);
                RuleFor(x => x.Round).Null().WithMessage(NullMessage);
            })
            .When(x => x.Timestamp.HasValue);

        RuleFor(x => x.Epoch)
            .GreaterThan(0)
            .DependentRules(() =>
            {
                const string NullMessage = "'{PropertyName}' must be null when 'epoch' is defined.";

                RuleFor(x => x.StateVersion).Null().WithMessage(NullMessage);
                RuleFor(x => x.Timestamp).Null().WithMessage(NullMessage);
            })
            .When(x => x.Epoch.HasValue);

        RuleFor(x => x.Round)
            .GreaterThan(0)
            .DependentRules(() =>
            {
                const string NullMessage = "'{PropertyName}' must be null when 'round' is defined.";

                RuleFor(x => x.StateVersion).Null().WithMessage(NullMessage);
                RuleFor(x => x.Timestamp).Null().WithMessage(NullMessage);
                RuleFor(x => x.Epoch).NotNull().WithMessage("'{PropertyName}' must not be null when 'round' is defined.");
            })
            .When(x => x.Round.HasValue);
    }
}
