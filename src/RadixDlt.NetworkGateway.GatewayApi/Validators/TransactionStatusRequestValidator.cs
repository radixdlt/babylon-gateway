using FluentValidation;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class TransactionStatusRequestValidator : AbstractValidator<TransactionStatusRequest>
{
    public TransactionStatusRequestValidator(
        TransactionIdentifierValidator transactionIdentifierValidator,
        PartialLedgerStateIdentifierValidator partialLedgerStateIdentifierValidator)
    {
        RuleFor(x => x.TransactionIdentifier)
            .NotNull()
            .SetValidator(transactionIdentifierValidator);

        RuleFor(x => x.AtStateIdentifier)
            .SetValidator(partialLedgerStateIdentifierValidator);
    }
}
