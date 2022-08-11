using FluentValidation;
using RadixDlt.NetworkGateway.Core;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class TransactionIdentifierValidator : AbstractValidator<TransactionIdentifier>
{
    public TransactionIdentifierValidator()
    {
        RuleFor(x => x.Hash)
            .NotNull()
            .Hex(NetworkGatewayConstants.Transaction.IdentifierByteLength);
    }
}
