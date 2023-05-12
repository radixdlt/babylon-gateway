using RadixDlt.NetworkGateway.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal static class EntityStateQuerierMappingExtensions
{
    public static GatewayModel.NonFungibleResourcesCollection MapToNonFungibleResourcesCollection(
        List<EntityStateQuerier.NonFungibleAggregatedPerVaultViewModel> input,
        List<EntityStateQuerier.NonFungibleIdWithOwnerDataViewModel>? nonFungibleIds,
        int vaultOffset, int vaultLimit, int resourceOffset, int resourceLimit)
    {
        var resourcesTotalCount = 0;
        var resources = new Dictionary<GlobalAddress, GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregated>();

        foreach (var vm in input)
        {
            resourcesTotalCount = vm.ResourceTotalCount;

            if (!resources.TryGetValue(vm.ResourceEntityGlobalAddress, out var existingRecord))
            {
                var vaultNextCursor = vm.VaultTotalCount > vaultLimit
                    ? new GatewayApiSdk.Model.OffsetCursor(vaultLimit).ToCursorString()
                    : null;

                existingRecord = new GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregated(
                    resourceAddress: vm.ResourceEntityGlobalAddress,
                    vaults: new GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregatedVault(
                        totalCount: vm.VaultTotalCount,
                        nextCursor: vaultNextCursor,
                        items: new List<GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem>()));

                resources[vm.ResourceEntityGlobalAddress] = existingRecord;
            }

            var ids = nonFungibleIds?
                .Where(x => x.ResourceEntityId == vm.ResourceEntityId && x.VaultEntityId == vm.VaultEntityId)
                .Select(x => x.NonFungibleId).ToList();

            existingRecord.Vaults.Items.Add(new GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                totalCount: vm.NonFungibleIdsCount,
                vaultAddress: vm.VaultAddress,
                lastUpdatedAtStateVersion: vm.LastUpdatedAtStateVersion,
                items: ids));
        }

        var previousCursor = resourceOffset > 0
            ? new GatewayApiSdk.Model.OffsetCursor(Math.Max(resourceOffset - resourceLimit, 0)).ToCursorString()
            : null;

        var nextCursor = resourcesTotalCount > resourceLimit + resourceOffset
            ? new GatewayApiSdk.Model.OffsetCursor(resourceLimit).ToCursorString()
            : null;

        return new GatewayApiSdk.Model.NonFungibleResourcesCollection(resourcesTotalCount, previousCursor, nextCursor,
            resources.Values.Cast<GatewayApiSdk.Model.NonFungibleResourcesCollectionItem>().ToList());
    }

    public static GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse MapToStateEntityNonFungibleResourceVaultsPageResponse(
        List<EntityStateQuerier.NonFungibleResourceVaultsViewModel> input,
        Dictionary<long, List<EntityStateQuerier.NonFungibleIdWithOwnerDataViewModel>>? nonFungibleIds,
        GatewayModel.LedgerState ledgerState, int offset, int limit,
        string? entityGlobalAddress, string? resourceGlobalAddress)
    {
        var mapped = input.Select(x =>
            {
                List<EntityStateQuerier.NonFungibleIdWithOwnerDataViewModel>? items = null;

                var hasItems = nonFungibleIds?.TryGetValue(x.VaultEntityId, out items);
                return new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                    totalCount: x.NonFungibleIdsCount,
                    vaultAddress: x.VaultAddress,
                    lastUpdatedAtStateVersion: x.LastUpdatedAtStateVersion,
                    items: hasItems == true && items != null ? items.Select(x => x.NonFungibleId).ToList() : null
                );
            }
        ).ToList();

        var vaultsTotalCount = input.FirstOrDefault()?.VaultTotalCount ?? 0;

        var previousCursor = offset > 0
            ? new GatewayModel.OffsetCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = vaultsTotalCount > offset + limit
            ? new GatewayModel.OffsetCursor(limit).ToCursorString()
            : null;

        return new GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse(ledgerState, vaultsTotalCount, previousCursor, nextCursor, mapped, entityGlobalAddress,
            resourceGlobalAddress);
    }
}
