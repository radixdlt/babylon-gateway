using FluentValidation;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.GatewayApi.Validators;

public class RecentTransactionsRequestValidator : AbstractValidator<RecentTransactionsRequest>
{
    public RecentTransactionsRequestValidator(
        IOptionsSnapshot<EndpointOptions> endpointOptionsSnapshot,
        PartialLedgerStateIdentifierValidator partialLedgerStateIdentifierValidator)
    {
        var endpointOptions = endpointOptionsSnapshot.Value;

        RuleFor(x => x.AtStateIdentifier)
            .SetValidator(partialLedgerStateIdentifierValidator);

        RuleFor(x => x.FromStateIdentifier)
            .SetValidator(partialLedgerStateIdentifierValidator);

        RuleFor(x => x.Cursor);

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(endpointOptions.MaxPageSize);
    }
}
