using FluentValidation;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class TransactionSubmitRequestValidator : AbstractValidator<TransactionSubmitRequest>
{
    public TransactionSubmitRequestValidator()
    {
        RuleFor(x => x.SignedTransaction)
            .NotEmpty();
    }
}
