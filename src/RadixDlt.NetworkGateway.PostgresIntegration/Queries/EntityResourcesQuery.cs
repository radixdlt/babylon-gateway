using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal class EntityResourcesPageQuery
{
    // TODO add support for string -> TokenAmount in Dapper (possibly with no silly CAST AS TEXT in the SQL
    // TODO those collections MUST be ordered on C# level and optionally on SQL level!!!
    // TODO drop redundant Resource* and Vault* prefixes?
    // TODO maybe we should consider stored procedures for those queries?

    public record ResultEntity(long EntityId, long TotalFungibleResourceCount, long TotalNonFungibleResourceCount)
    {
        public IList<ResultResource> FungibleResources => Resources.Values.Where(x => x.ResourceType == ResourceType.Fungible).ToList();

        public IList<ResultResource> NonFungibleResources => Resources.Values.Where(x => x.ResourceType == ResourceType.NonFungible).ToList();

        public Dictionary<long, ResultResource> Resources { get; } = new();
    }

    public record ResultResource(long ResourceEntityId, EntityAddress ResourceEntityAddress, ResourceType ResourceType, string ResourceBalance, long ResourceFromStateVersion, long ResourceLastUpdatedAtStateVersion, long? ResourceVaultTotalCount)
    {
        public Dictionary<long, ResultVault> Vaults { get; } = new();
    }

    public record ResultVault(long VaultEntityId, EntityAddress VaultEntityAddress, string VaultBalance, long VaultFromStateVersion, long VaultLastUpdatedAtStateVersion);

    public record struct DetailsQueryConfiguration(int FungibleResourcesPerEntity, int NonFungibleResourcesPerEntity, int VaultsPerResource, long AtLedgerState);

    public record struct ResourcesPageQueryConfiguration(int ResourcesPerEntity, int VaultsPerResource, bool DescendingOrder, GatewayModel.StateVersionIdCursor? Cursor, long AtLedgerState);

    public record struct VaultsPageQueryConfiguration(int VaultsPerResource, bool DescendingOrder, GatewayModel.StateVersionIdCursor? Cursor, long AtLedgerState);

    private record struct QueryConfiguration(
        int FungibleResourcesPerEntity,
        int NonFungibleResourcesPerEntity,
        int VaultsPerResource,
        bool DescendingOrder,
        GatewayModel.StateVersionIdCursor? ResourceCursor,
        GatewayModel.StateVersionIdCursor? VaultCursor,
        long AtLedgerState);

    public static async Task<Dictionary<long, ResultEntity>> Details(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        ICollection<long> entityIds,
        DetailsQueryConfiguration detailsConfiguration,
        CancellationToken token = default)
    {
        var configuration = new QueryConfiguration(
            detailsConfiguration.FungibleResourcesPerEntity,
            detailsConfiguration.NonFungibleResourcesPerEntity,
            detailsConfiguration.VaultsPerResource,
            true,
            null,
            null,
            detailsConfiguration.AtLedgerState);

        return await ExecuteEntityResourcesPageQuery(dbContext, dapperWrapper, entityIds, null, configuration, token);
    }

    public static async Task<ResultEntity?> FungibleResourcesPage(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        long entityId,
        ResourcesPageQueryConfiguration pageConfiguration,
        CancellationToken token = default)
    {
        var configuration = new QueryConfiguration(
            pageConfiguration.ResourcesPerEntity,
            0,
            pageConfiguration.VaultsPerResource,
            pageConfiguration.DescendingOrder,
            pageConfiguration.Cursor,
            null,
            pageConfiguration.AtLedgerState);
        var results = await ExecuteEntityResourcesPageQuery(dbContext, dapperWrapper, new[] { entityId }, null, configuration, token);

        results.TryGetValue(entityId, out var result);

        return result;
    }

    public static async Task<ResultEntity?> FungibleResourceVaultsPage(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        long entityId,
        long resourceId,
        VaultsPageQueryConfiguration pageConfiguration,
        CancellationToken token = default)
    {
        var configuration = new QueryConfiguration(
            1,
            0,
            pageConfiguration.VaultsPerResource,
            pageConfiguration.DescendingOrder,
            null,
            pageConfiguration.Cursor,
            pageConfiguration.AtLedgerState);
        var results = await ExecuteEntityResourcesPageQuery(dbContext, dapperWrapper, new[] { entityId }, resourceId, configuration, token);

        results.TryGetValue(entityId, out var result);

        return result;
    }

    private static async Task<Dictionary<long, ResultEntity>> ExecuteEntityResourcesPageQuery(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        ICollection<long> entityIds,
        long? resourceEntityId,
        QueryConfiguration configuration,
        CancellationToken token = default)
    {
        // TODO throw if entityIds.Count > 1 and resourceEntityId/cursors are used ??

        var cd = dapperWrapper.CreateCommandDefinition(
            @"WITH
variables AS (
    SELECT
        unnest(@entityIds) AS entity_id,
        @resourceEntityId AS resource_entity_id,
        @fungibleResourcesPerEntity AS fungible_resources_per_entity,
        @nonFungibleResourcesPerEntity AS non_fungible_resources_per_entity,
        @vaultsPerResource AS vaults_per_resource,
        @useVaultAggregation as use_vault_aggregation,
        @descendingOrder AS descending_order,
        ROW(@resourceCursorStateVersion, @resourceCursorId) AS resource_cursor,
        ROW(@vaultCursorStateVersion, @vaultCursorId) AS vault_cursor,
        @atLedgerState AS at_ledger_state
)
SELECT
    var.entity_id,
    th.total_fungible_count AS total_fungible_resource_count,
    th.total_non_fungible_count AS total_non_fungible_resource_count,

    ed_entity.id AS resource_entity_id,
    ed_entity.address AS resource_entity_address,
    ed.resource_type AS resource_type,
    CAST(ed_balance.balance AS TEXT) AS resource_balance,
    ed.from_state_version AS resource_from_state_version,
    ed_balance.from_state_version AS resource_last_updated_at_state_version,
    ed_vault_totals.total_count AS resource_vault_total_count,

    ed_vault_ed.vault_entity_id AS vault_entity_id,
    ed_vault_ed_entity.address AS vault_entity_address,
    CAST(ed_vault_ed_balance.balance AS TEXT) AS vault_balance,
    ed_vault_ed.from_state_version AS vault_from_state_version,
    ed_vault_ed_balance.from_state_version AS vault_last_updated_at_state_version
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM entity_resource_totals_history
    WHERE entity_id = var.entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON TRUE
INNER JOIN LATERAL (
    (
        SELECT *
        FROM entity_resource_entry_definition
        WHERE
            entity_id = var.entity_id
          AND resource_type = 'fungible'
          AND from_state_version <= var.at_ledger_state
          AND CASE WHEN var.resource_entity_id IS NOT NULL THEN resource_entity_id = var.resource_entity_id ELSE TRUE END
          AND CASE WHEN var.descending_order THEN (from_state_version, id) <= var.resource_cursor ELSE (from_state_version, id) >= var.resource_cursor END
        ORDER BY
            CASE WHEN var.descending_order THEN from_state_version END DESC,
            CASE WHEN var.descending_order THEN id END DESC,
            CASE WHEN NOT var.descending_order THEN from_state_version END,
            CASE WHEN NOT var.descending_order THEN id END
        LIMIT var.fungible_resources_per_entity
    )
    UNION ALL
    (
        SELECT *
        FROM entity_resource_entry_definition
        WHERE
            entity_id = var.entity_id
          AND resource_type = 'non_fungible'
          AND from_state_version <= var.at_ledger_state
          AND CASE WHEN var.resource_entity_id IS NOT NULL THEN resource_entity_id = var.resource_entity_id ELSE TRUE END
          AND CASE WHEN var.descending_order THEN (from_state_version, id) <= var.resource_cursor ELSE (from_state_version, id) >= var.resource_cursor END
        ORDER BY
            CASE WHEN var.descending_order THEN from_state_version END DESC,
            CASE WHEN var.descending_order THEN id END DESC,
            CASE WHEN NOT var.descending_order THEN from_state_version END,
            CASE WHEN NOT var.descending_order THEN id END
        LIMIT var.non_fungible_resources_per_entity
    )
) ed ON TRUE
INNER JOIN entities ed_entity ON ed_entity.id = ed.resource_entity_id
LEFT JOIN LATERAL (
    SELECT *
    FROM entity_resource_balance_history
    WHERE entity_id = var.entity_id AND resource_entity_id = ed.resource_entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) ed_balance ON TRUE
LEFT JOIN LATERAL (
    SELECT *
    FROM entity_resource_vault_totals_history
    WHERE entity_id = var.entity_id AND resource_entity_id = ed.resource_entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) ed_vault_totals ON var.use_vault_aggregation
LEFT JOIN LATERAL (
    SELECT *
    FROM entity_resource_vault_entry_definition
    WHERE
        entity_id = var.entity_id
      AND resource_entity_id = ed.resource_entity_id
      AND from_state_version <= var.at_ledger_state
      AND CASE WHEN var.descending_order THEN (from_state_version, id) <= var.vault_cursor ELSE (from_state_version, id) >= var.vault_cursor END
    ORDER BY
        CASE WHEN var.descending_order THEN from_state_version END DESC,
        CASE WHEN var.descending_order THEN id END DESC,
        CASE WHEN NOT var.descending_order THEN from_state_version END,
        CASE WHEN NOT var.descending_order THEN id END
    LIMIT var.vaults_per_resource
) ed_vault_ed ON var.use_vault_aggregation
LEFT JOIN entities ed_vault_ed_entity ON ed_vault_ed_entity.id = ed_vault_ed.vault_entity_id
LEFT JOIN LATERAL (
    SELECT *
    FROM vault_balance_history
    WHERE vault_entity_id = ed_vault_ed.vault_entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) ed_vault_ed_balance ON var.use_vault_aggregation;",
            new
            {
                entityIds = entityIds.ToList(),
                resourceEntityId = resourceEntityId,
                fungibleResourcesPerEntity = configuration.FungibleResourcesPerEntity + 1,
                nonFungibleResourcesPerEntity = configuration.NonFungibleResourcesPerEntity + 1,
                vaultsPerResource = configuration.VaultsPerResource + 1,
                useVaultAggregation = configuration.VaultsPerResource > 0,
                descendingOrder = configuration.DescendingOrder,
                resourceCursorStateVersion = configuration.ResourceCursor?.StateVersion ?? (configuration.DescendingOrder ? long.MaxValue : long.MinValue),
                resourceCursorId = configuration.ResourceCursor?.Id ?? (configuration.DescendingOrder ? long.MaxValue : long.MinValue),
                vaultCursorStateVersion = configuration.VaultCursor?.StateVersion ?? (configuration.DescendingOrder ? long.MaxValue : long.MinValue),
                vaultCursorId = configuration.VaultCursor?.Id ?? (configuration.DescendingOrder ? long.MaxValue : long.MinValue),
                atLedgerState = configuration.AtLedgerState,
            },
            token);

        var result = new Dictionary<long, ResultEntity>();

        await dapperWrapper.QueryAsync<ResultEntity, ResultResource, ResultVault?, ResultEntity>(
            dbContext.Database.GetDbConnection(),
            cd,
            (entity, resource, vault) =>
            {
                result.TryAdd(entity.EntityId, entity);
                result[entity.EntityId].Resources.TryAdd(resource.ResourceEntityId, resource);

                if (vault != null)
                {
                    result[entity.EntityId].Resources[resource.ResourceEntityId].Vaults.TryAdd(vault.VaultEntityId, vault);
                }

                return entity;
            },
            "resource_entity_id,vault_entity_id");

        return result;
    }
}
