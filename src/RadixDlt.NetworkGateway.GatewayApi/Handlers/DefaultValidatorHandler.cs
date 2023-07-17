using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Handlers;

internal class DefaultValidatorHandler : IValidatorHandler
{
    private readonly ILedgerStateQuerier _ledgerStateQuerier;
    private readonly IValidatorQuerier _validatorQuerier;

    public DefaultValidatorHandler(ILedgerStateQuerier ledgerStateQuerier, IValidatorQuerier validatorQuerier)
    {
        _ledgerStateQuerier = ledgerStateQuerier;
        _validatorQuerier = validatorQuerier;
    }

    public async Task<GatewayApiSdk.Model.ValidatorsUptimeResponse> Uptime(GatewayApiSdk.Model.ValidatorsUptimeRequest request, CancellationToken token)
    {
        var ledgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadRequest(request.AtLedgerState, token);
        var fromLedgerState = await _ledgerStateQuerier.GetValidLedgerStateForReadForwardRequest(request.FromLedgerState, token);

        return await _validatorQuerier.ValidatorsUptimeStatistics(request.ValidatorAddresses.Select(x => (EntityAddress)x).ToList(), ledgerState, fromLedgerState, token);
    }
}
