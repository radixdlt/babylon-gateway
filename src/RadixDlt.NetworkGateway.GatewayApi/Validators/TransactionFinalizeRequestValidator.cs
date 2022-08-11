using FluentValidation;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class TransactionFinalizeRequestValidator : AbstractValidator<TransactionFinalizeRequest>
{
    public TransactionFinalizeRequestValidator()
    {
        RuleFor(x => x.UnsignedTransaction)
            .NotEmpty();

        RuleFor(x => x.Signature)
            .NotNull()
            .SetValidator(new SignatureValidator());
    }
}
