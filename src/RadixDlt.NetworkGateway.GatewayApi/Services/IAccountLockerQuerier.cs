using RadixDlt.NetworkGateway.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface IAccountLockerQuerier
{
    Task<GatewayApiSdk.Model.StateAccountLockerPageVaultsResponse> AccountLockerVaultsPage(
        IEntityStateQuerier.AccountLockerPageRequest pageRequest,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default);

    Task<GatewayApiSdk.Model.StateAccountLockersTouchedAtResponse> AccountLockersTouchedAt(
        IList<AccountLockerAddress> accountLockers,
        GatewayApiSdk.Model.LedgerState atLedgerState,
        CancellationToken token = default);
}
