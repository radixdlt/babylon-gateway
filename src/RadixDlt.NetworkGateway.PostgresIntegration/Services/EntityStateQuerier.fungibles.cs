using Dapper;
using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal partial class EntityStateQuerier
{
    private record FungibleViewModel(EntityAddress ResourceEntityAddress, string Balance, int ResourcesTotalCount, long LastUpdatedAtStateVersion);

    private record FungibleResourceVaultsViewModel(EntityAddress ResourceEntityAddress, EntityAddress VaultAddress, string Balance, int VaultTotalCount, long LastUpdatedAtStateVersion);

    private record FungibleAggregatedPerVaultViewModel(
        EntityAddress ResourceEntityAddress,
        EntityAddress VaultAddress,
        string Balance,
        int ResourceTotalCount,
        int VaultTotalCount,
        long LastUpdatedAtStateVersion);

    private async Task<GatewayModel.FungibleResourcesCollection> GetFungiblesSliceAggregatedPerVault(
        long entityId,
        int resourceOffset,
        int resourceLimit,
        int vaultOffset,
        int vaultLimit,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
WITH most_recent_entity_resource_aggregate_history_nested AS (
    SELECT fungible_resource_entity_ids
    FROM entity_resource_aggregate_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId
    ORDER BY from_state_version DESC
    LIMIT 1
),
most_recent_entity_resource_aggregate_history AS (
    SELECT a.val AS fungible_resource_entity_id, cardinality(fungible_resource_entity_ids) AS resource_total_count, a.ord AS resource_order
    FROM most_recent_entity_resource_aggregate_history_nested
    LEFT JOIN LATERAL UNNEST(fungible_resource_entity_ids[@resourceStartIndex:@resourceEndIndex]) WITH ORDINALITY AS a(val,ord) ON true
),
most_recent_entity_resource_vault_aggregate_history_nested AS (
    SELECT rah.fungible_resource_entity_id, rah.resource_total_count, vah.vault_entity_ids, rah.resource_order
    FROM most_recent_entity_resource_aggregate_history rah
    INNER JOIN LATERAL (
        SELECT vault_entity_ids
        FROM entity_resource_vault_aggregate_history
        WHERE from_state_version <= @stateVersion AND entity_id = @entityId AND resource_entity_id = rah.fungible_resource_entity_id
        ORDER BY from_state_version DESC
        LIMIT 1
    ) vah ON TRUE
),
most_recent_entity_resource_vault_aggregate_history AS (
    SELECT
        ahn.fungible_resource_entity_id,
        ahn.resource_total_count,
        a.val AS vault_entity_id,
        cardinality(vault_entity_ids) AS vault_total_count,
        ahn.resource_order,
        a.ord AS vault_order
    FROM most_recent_entity_resource_vault_aggregate_history_nested ahn
    LEFT JOIN LATERAL UNNEST(vault_entity_ids[@vaultStartIndex:@vaultEndIndex]) WITH ORDINALITY AS a(val,ord) ON true
)
SELECT
    er.address AS ResourceEntityAddress,
    ev.address AS VaultAddress,
    CAST(vh.balance AS text) AS Balance,
    vah.resource_total_count AS ResourceTotalCount,
    vah.vault_total_count AS VaultTotalCount,
    vh.from_state_version AS LastUpdatedAtStateVersion
FROM most_recent_entity_resource_vault_aggregate_history vah
INNER JOIN LATERAL (
    SELECT balance, from_state_version
    FROM entity_vault_history
    WHERE from_state_version <= @stateVersion AND global_entity_id = @entityId AND resource_entity_id = vah.fungible_resource_entity_id AND vault_entity_id = vah.vault_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) vh ON TRUE
INNER JOIN entities er ON vah.fungible_resource_entity_id = er.id
INNER JOIN entities ev ON vah.vault_entity_id = ev.id
ORDER BY vah.resource_order, vah.vault_order;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                resourceStartIndex = resourceOffset + 1,
                resourceEndIndex = resourceOffset + 1 + resourceLimit,
                vaultStartIndex = vaultOffset + 1,
                vaultEndIndex = vaultOffset + 1 + vaultLimit,
            },
            cancellationToken: token);

        var resourcesTotalCount = 0;

        var resources = new Dictionary<EntityAddress, GatewayModel.FungibleResourcesCollectionItemVaultAggregated>();

        foreach (var vm in await _dbContext.Database.GetDbConnection().QueryAsync<FungibleAggregatedPerVaultViewModel>(cd))
        {
            resourcesTotalCount = vm.ResourceTotalCount;

            if (!resources.TryGetValue(vm.ResourceEntityAddress, out var existingRecord))
            {
                existingRecord = new GatewayModel.FungibleResourcesCollectionItemVaultAggregated(
                    resourceAddress: vm.ResourceEntityAddress,
                    vaults: new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVault(
                        totalCount: vm.VaultTotalCount,
                        nextCursor: GenerateOffsetCursor(vaultOffset, vaultLimit, vm.VaultTotalCount),
                        items: new List<GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem>()));

                resources[vm.ResourceEntityAddress] = existingRecord;
            }

            existingRecord.Vaults.Items.Add(new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                amount: TokenAmount.FromSubUnitsString(vm.Balance).ToString(),
                vaultAddress: vm.VaultAddress,
                lastUpdatedAtStateVersion: vm.LastUpdatedAtStateVersion));
        }

        var items = resources.Values.Cast<GatewayModel.FungibleResourcesCollectionItem>().ToList();

        return new GatewayModel.FungibleResourcesCollection(resourcesTotalCount, GenerateOffsetCursor(resourceOffset, resourceLimit, resourcesTotalCount), items);
    }

    private async Task<GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVault> GetFungibleResourceVaults(
        long entityId,
        long resourceEntityId,
        int offset,
        int limit,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
WITH most_recent_entity_resource_aggregate_history_nested AS (
    SELECT fungible_resource_entity_ids
    FROM entity_resource_aggregate_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId
    ORDER BY from_state_version DESC
    LIMIT 1
),
most_recent_entity_resource_vault_aggregate_history_nested AS (
    SELECT vah.vault_entity_ids
    FROM most_recent_entity_resource_aggregate_history_nested rah
    INNER JOIN LATERAL (
        SELECT vault_entity_ids
        FROM entity_resource_vault_aggregate_history
        WHERE from_state_version <= @stateVersion AND entity_id = @entityId AND resource_entity_id = @resourceEntityId
        ORDER BY from_state_version DESC
        LIMIT 1
        ) vah ON TRUE
),
most_recent_entity_resource_vault_aggregate_history AS (
    SELECT a.val AS vault_entity_id, cardinality(vault_entity_ids) AS vault_total_count, a.ord AS ord
    FROM most_recent_entity_resource_vault_aggregate_history_nested ahn
    LEFT JOIN LATERAL UNNEST(vault_entity_ids[@startIndex:@endIndex]) WITH ORDINALITY a(val,ord) ON true
)
SELECT er.address AS ResourceEntityAddress, ev.address AS VaultAddress, CAST(vh.balance AS text) AS Balance, vah.vault_total_count AS VaultTotalCount, vh.from_state_version AS LastUpdatedAtStateVersion
FROM most_recent_entity_resource_vault_aggregate_history vah
INNER JOIN LATERAL (
    SELECT balance, from_state_version
    FROM entity_vault_history
    WHERE from_state_version <= @stateVersion AND global_entity_id = @entityId AND resource_entity_id = @resourceEntityId AND vault_entity_id = vah.vault_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
    ) vh ON TRUE
INNER JOIN entities er ON er.id = @resourceEntityId
INNER JOIN entities ev ON vah.vault_entity_id = ev.id
ORDER BY vah.ord;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                resourceEntityId = resourceEntityId,
                startIndex = offset + 1,
                endIndex = offset + 1 + limit,
            },
            cancellationToken: token);

        var result = (await _dbContext.Database.GetDbConnection().QueryAsync<FungibleResourceVaultsViewModel>(cd)).ToList();
        var vaultsTotalCount = result.FirstOrDefault()?.VaultTotalCount ?? 0;
        var castedResult = result
            .Select(x =>
                new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                    amount: TokenAmount.FromSubUnitsString(x.Balance).ToString(),
                    vaultAddress: x.VaultAddress,
                    lastUpdatedAtStateVersion: x.LastUpdatedAtStateVersion))
            .ToList();

        return new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVault(vaultsTotalCount, GenerateOffsetCursor(offset, limit, vaultsTotalCount), castedResult);
    }
}
