using RadixDlt.NetworkGateway.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface IValidatorQuerier
{
    Task<GatewayApiSdk.Model.ValidatorsUptimeResponse> ValidatorsUptimeStatistics(IList<EntityAddress> validatorAddresses, GatewayApiSdk.Model.LedgerState ledgerState,
        GatewayApiSdk.Model.LedgerState? fromLedgerState, CancellationToken token = default);
}
