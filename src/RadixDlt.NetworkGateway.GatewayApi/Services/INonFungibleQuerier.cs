using RadixDlt.NetworkGateway.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface INonFungibleQuerier
{
    Task<GatewayApiSdk.Model.StateNonFungibleIdsResponse> NonFungibleIds(
        EntityAddress resourceAddress,
        GatewayApiSdk.Model.LedgerState ledgerState,
        GatewayApiSdk.Model.IdBoundaryCoursor? cursor,
        int pageSize,
        CancellationToken token = default);

    Task<GatewayApiSdk.Model.StateNonFungibleDataResponse> NonFungibleIdData(
        EntityAddress resourceAddress,
        IList<string> nonFungibleIds,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default);

    Task<GatewayApiSdk.Model.StateNonFungibleLocationResponse> NonFungibleIdLocation(
        EntityAddress resourceAddress,
        IList<string> nonFungibleIds,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default);
}
