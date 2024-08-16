using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using System.Collections.Generic;
using System.Linq;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal static class TmpMapper
{
    public record MapResult(long EntityId, GatewayModel.FungibleResourcesCollection Fungibles, GatewayModel.NonFungibleResourcesCollection NonFungibles);

    public static MapResult Map(
        EntityResourcesPageQuery.ResultEntity x,
        Dictionary<long, GatewayModel.EntityMetadataCollection>? explicitMetadata,
        bool aggregatePerVault,
        int fungibleResourcesPerEntity,
        int nonFungibleResourcesPerEntity,
        int vaultsPerResource)
    {
        string? funCursor = null;
        var fun = x.FungibleResources
            .TakeWhile((f, idx) => CursorGenerator.TakeWhileStateVersionId(f, idx, fungibleResourcesPerEntity, x => new GatewayModel.StateVersionIdCursor(x.ResourceFromStateVersion, x.ResourceEntityId), out funCursor))
            .Select(f =>
            {
                GatewayModel.FungibleResourcesCollectionItem val;
                GatewayModel.EntityMetadataCollection? resourceExplicitMetadata = null;

                explicitMetadata?.TryGetValue(f.ResourceEntityId, out resourceExplicitMetadata);

                if (aggregatePerVault)
                {
                    string? vaultCursor = null;
                    var vaults = f.Vaults.Values
                        .TakeWhile((v, idx) => CursorGenerator.TakeWhileStateVersionId(v, idx, vaultsPerResource, x => new GatewayModel.StateVersionIdCursor(x.VaultFromStateVersion, x.VaultEntityId), out vaultCursor))
                        .Select(v => new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                            vaultAddress: v.VaultEntityAddress,
                            amount: TokenAmount.FromSubUnitsString(v.VaultBalance).ToString(),
                            lastUpdatedAtStateVersion: v.VaultLastUpdatedAtStateVersion))
                        .ToList();

                    val = new GatewayModel.FungibleResourcesCollectionItemVaultAggregated(
                        resourceAddress: f.ResourceEntityAddress,
                        vaults: new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVault(
                            totalCount: f.ResourceVaultTotalCount,
                            nextCursor: vaultCursor,
                            items: vaults),
                        explicitMetadata: resourceExplicitMetadata);
                }
                else
                {
                    val = new GatewayModel.FungibleResourcesCollectionItemGloballyAggregated(
                        resourceAddress: f.ResourceEntityAddress,
                        amount: TokenAmount.FromSubUnitsString(f.ResourceBalance).ToString(),
                        lastUpdatedAtStateVersion: f.ResourceLastUpdatedAtStateVersion,
                        explicitMetadata: resourceExplicitMetadata);
                }

                return val;
            })
            .ToList();

        string? nonFunCursor = null;
        var nonFun = x.NonFungibleResources
            .TakeWhile((nf, idx) => CursorGenerator.TakeWhileStateVersionId(nf, idx, nonFungibleResourcesPerEntity, x => new GatewayModel.StateVersionIdCursor(x.ResourceFromStateVersion, x.ResourceEntityId), out nonFunCursor))
            .Select(nf =>
            {
                GatewayModel.NonFungibleResourcesCollectionItem val;
                GatewayModel.EntityMetadataCollection? resourceExplicitMetadata = null;

                explicitMetadata?.TryGetValue(nf.ResourceEntityId, out resourceExplicitMetadata);

                if (aggregatePerVault)
                {
                    string? vaultCursor = null;
                    var vaults = nf.Vaults.Values
                        .TakeWhile((v, idx) => CursorGenerator.TakeWhileStateVersionId(v, idx, vaultsPerResource, x => new GatewayModel.StateVersionIdCursor(x.VaultFromStateVersion, x.VaultEntityId), out vaultCursor))
                        .Select(v => new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                            totalCount: long.Parse(TokenAmount.FromSubUnitsString(v.VaultBalance).ToString()),
                            nextCursor: "tbd", // TODO implement
                            items: null, // TODO implement
                            vaultAddress: v.VaultEntityAddress,
                            lastUpdatedAtStateVersion: v.VaultLastUpdatedAtStateVersion))
                        .ToList();

                    val = new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregated(
                        resourceAddress: nf.ResourceEntityAddress,
                        vaults: new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVault(
                            totalCount: nf.ResourceVaultTotalCount,
                            nextCursor: vaultCursor,
                            items: vaults),
                        explicitMetadata: resourceExplicitMetadata);
                }
                else
                {
                    val = new GatewayModel.NonFungibleResourcesCollectionItemGloballyAggregated(
                        resourceAddress: nf.ResourceEntityAddress,
                        amount: long.Parse(TokenAmount.FromSubUnitsString(nf.ResourceBalance).ToString()),
                        lastUpdatedAtStateVersion: nf.ResourceLastUpdatedAtStateVersion,
                        explicitMetadata: resourceExplicitMetadata);
                }

                return val;
            })
            .ToList();

        return new MapResult(
            EntityId: x.EntityId,
            Fungibles: new GatewayModel.FungibleResourcesCollection(
                totalCount: x.TotalFungibleResourceCount,
                nextCursor: funCursor,
                items: fun),
            NonFungibles: new GatewayModel.NonFungibleResourcesCollection(
                totalCount: x.TotalNonFungibleResourceCount,
                nextCursor: nonFunCursor,
                items: nonFun));
    }
}
