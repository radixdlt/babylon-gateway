using FluentValidation;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class SborScryptoEncodeRequestValidator : AbstractValidator<GatewayModel.SborScryptoEncodeRequest>
{
    public SborScryptoEncodeRequestValidator()
    {
        RuleFor(x => x.Value)
            .NotNull();
    }
}
