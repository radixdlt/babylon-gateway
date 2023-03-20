using FluentValidation;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class SborScryptoDecodeRequestValidator : AbstractValidator<GatewayModel.SborScryptoDecodeRequest>
{
    public SborScryptoDecodeRequestValidator()
    {
        RuleFor(x => x.ValueHex)
            .NotEmpty()
            .Hex();
    }
}
