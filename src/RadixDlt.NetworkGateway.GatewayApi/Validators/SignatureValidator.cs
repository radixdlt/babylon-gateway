using FluentValidation;
using RadixDlt.NetworkGateway.Core;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class SignatureValidator : AbstractValidator<Signature>
{
    public SignatureValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PublicKey)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(x => x.PublicKey.Hex)
                    .NotNull()
                    .Hex(NetworkGatewayConstants.Transaction.CompressedPublicKeyBytesLength);
            });

        RuleFor(x => x.Bytes)
            .NotEmpty();
    }
}
