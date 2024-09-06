using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class NonFungibleQuerier : INonFungibleQuerier
{
    private readonly ReadOnlyDbContext _readOnlyDbContext;
    private readonly IDapperWrapper _dapperWrapper;
    private readonly IEntityQuerier _entityQuerier;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public NonFungibleQuerier(ReadOnlyDbContext readOnlyDbContext, IDapperWrapper dapperWrapper, IEntityQuerier entityQuerier, INetworkConfigurationProvider networkConfigurationProvider)
    {
        _readOnlyDbContext = readOnlyDbContext;
        _dapperWrapper = dapperWrapper;
        _entityQuerier = entityQuerier;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<GatewayModel.StateNonFungibleIdsResponse> NonFungibleIds(
        EntityAddress resourceAddress,
        GatewayModel.LedgerState ledgerState,
        GatewayModel.IdBoundaryCoursor? cursor,
        int pageSize,
        CancellationToken token = default)
    {
        var nonFungibleResourceEntity = await _entityQuerier.GetEntity<GlobalNonFungibleResourceEntity>(resourceAddress, ledgerState, token);

        return await NonFungibleResourceQueries.NonFungibleIds(
            _readOnlyDbContext,
            _dapperWrapper,
            nonFungibleResourceEntity,
            ledgerState,
            cursor,
            pageSize,
            token);
    }

    public async Task<GatewayModel.StateNonFungibleDataResponse> NonFungibleIdData(
        EntityAddress resourceAddress,
        IList<string> nonFungibleIds,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var nonFungibleResourceEntity = await _entityQuerier.GetEntity<GlobalNonFungibleResourceEntity>(resourceAddress, ledgerState, token);

        return await NonFungibleResourceQueries.NonFungibleIdData(
            _readOnlyDbContext,
            _dapperWrapper,
            nonFungibleResourceEntity,
            nonFungibleIds,
            (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id,
            ledgerState,
            token);
    }

    public async Task<GatewayModel.StateNonFungibleLocationResponse> NonFungibleIdLocation(
        EntityAddress resourceAddress,
        IList<string> nonFungibleIds,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var nonFungibleResourceEntity = await _entityQuerier.GetEntity<GlobalNonFungibleResourceEntity>(resourceAddress, ledgerState, token);

        return await NonFungibleResourceQueries.NonFungibleIdLocation(
            _readOnlyDbContext,
            _dapperWrapper,
            nonFungibleResourceEntity,
            nonFungibleIds,
            ledgerState,
            token);
    }
}
