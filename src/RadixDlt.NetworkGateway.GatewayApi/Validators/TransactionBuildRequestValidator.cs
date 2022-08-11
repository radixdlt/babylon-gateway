using FluentValidation;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class TransactionBuildRequestValidator : AbstractValidator<TransactionBuildRequest>
{
    public TransactionBuildRequestValidator(
        AccountIdentifierValidator accountIdentifierValidator,
        PartialLedgerStateIdentifierValidator partialLedgerStateIdentifierValidator)
    {
        RuleFor(x => x.Actions)
            .NotNull()
            // TODO add more
            ;

        RuleFor(x => x.FeePayer)
            .NotNull()
            .SetValidator(accountIdentifierValidator);

        RuleFor(x => x.AtStateIdentifier)
            .SetValidator(partialLedgerStateIdentifierValidator);

        RuleFor(x => x.Message)
            // TODO add some?
            ;

        RuleFor(x => x.DisableTokenMintAndBurn)
            // TODO add some?
            ;
    }
}
