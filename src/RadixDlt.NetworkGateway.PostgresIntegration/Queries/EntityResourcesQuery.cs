/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal class EntityResourcesQuery
{
    // TODO add support for string -> TokenAmount in Dapper (possibly with no silly CAST AS TEXT in the SQL
    // TODO drop redundant Resource* and Vault* prefixes?
    // TODO maybe we should consider stored procedures for those queries and/or their fragments that will repeat?

    public record ResultEntity(long EntityId, long TotalFungibleResourceCount, long TotalNonFungibleResourceCount)
    {
        public StateVersionIdCursor? FungibleResourcesNextCursor { get; set; } // TODO set should throw if already non-null

        public List<ResultResource> FungibleResources { get; } = new();

        public StateVersionIdCursor? NonFungibleResourcesNextCursor { get; set; } // TODO set should throw if already non-null

        public List<ResultResource> NonFungibleResources { get; } = new();

        internal Dictionary<long, ResultResource> InternalResources { get; } = new();
    }

    public record ResultResource(long ResourceEntryDefinitionId, long ResourceEntityId, ResourceType ResourceType, EntityAddress ResourceEntityAddress, string ResourceBalance, long ResourceFromStateVersion, long ResourceLastUpdatedAtStateVersion, long? ResourceVaultTotalCount)
    {
        public StateVersionIdCursor? VaultsNextCursor { get; set; } // TODO set should throw if already non-null

        public List<ResultVault> Vaults { get; } = new();
    }

    public record ResultVault(long VaultEntryDefinitionId, long VaultEntityId, EntityAddress VaultEntityAddress, string VaultBalance, long VaultFromStateVersion, long VaultLastUpdatedAtStateVersion);

    public record struct DetailsQueryConfiguration(int FungibleResourcesPerEntity, int NonFungibleResourcesPerEntity, int VaultsPerResource, long AtLedgerState);

    public record struct ResourcesPageQueryConfiguration(int ResourcesPerEntity, int VaultsPerResource, bool DescendingOrder, StateVersionIdCursor? Cursor, long AtLedgerState);

    public record struct VaultsPageQueryConfiguration(int VaultsPerResource, bool DescendingOrder, StateVersionIdCursor? Cursor, long AtLedgerState);

    private record struct QueryConfiguration(
        int FungibleResourcesPerEntity,
        int NonFungibleResourcesPerEntity,
        int VaultsPerResource,
        bool DescendingOrder,
        StateVersionIdCursor? ResourceCursor,
        StateVersionIdCursor? VaultCursor,
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

        return await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, entityIds, null, configuration, token);
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
        var results = await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, new[] { entityId }, null, configuration, token);

        results.TryGetValue(entityId, out var result);

        return result;
    }

    public static async Task<ResultEntity?> NonFungibleResourcesPage(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        long entityId,
        ResourcesPageQueryConfiguration pageConfiguration,
        CancellationToken token = default)
    {
        var configuration = new QueryConfiguration(
            0,
            pageConfiguration.ResourcesPerEntity,
            pageConfiguration.VaultsPerResource,
            pageConfiguration.DescendingOrder,
            pageConfiguration.Cursor,
            null,
            pageConfiguration.AtLedgerState);
        var results = await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, new[] { entityId }, null, configuration, token);

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
        var results = await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, new[] { entityId }, resourceId, configuration, token);

        results.TryGetValue(entityId, out var result);

        return result;
    }

    public static async Task<ResultEntity?> NonFungibleResourceVaultsPage(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        long entityId,
        long resourceId,
        VaultsPageQueryConfiguration pageConfiguration,
        CancellationToken token = default)
    {
        var configuration = new QueryConfiguration(
            0,
            1,
            pageConfiguration.VaultsPerResource,
            pageConfiguration.DescendingOrder,
            null,
            pageConfiguration.Cursor,
            pageConfiguration.AtLedgerState);
        var results = await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, new[] { entityId }, resourceId, configuration, token);

        results.TryGetValue(entityId, out var result);

        return result;
    }

    private static async Task<Dictionary<long, ResultEntity>> ExecuteEntityResourcesQuery(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        ICollection<long> entityIds,
        long? resourceEntityId,
        QueryConfiguration configuration,
        CancellationToken token)
    {
        if (entityIds.Count > 1 && (resourceEntityId.HasValue || configuration.ResourceCursor != null || configuration.VaultCursor != null))
        {
            throw new InvalidOperationException("Neither resource filter nor cursors can be used if executing against multiple entities.");
        }

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
    coalesce(th.total_fungible_count, 0) AS total_fungible_resource_count,
    coalesce(th.total_non_fungible_count, 0) AS total_non_fungible_resource_count,

    ed.id AS resource_entry_definition_id,
    ed.resource_entity_id AS resource_entity_id,
    ed.resource_type AS resource_type,
    ed_entity.address AS resource_entity_address,
    CAST(ed_balance.balance AS TEXT) AS resource_balance,
    ed.from_state_version AS resource_from_state_version,
    ed_balance.from_state_version AS resource_last_updated_at_state_version,
    ed_vault_totals.total_count AS resource_vault_total_count,

    ed_vault_ed.id AS vault_entry_definition_id,
    ed_vault_ed.vault_entity_id AS vault_entity_id,
    ed_vault_ed_entity.address AS vault_entity_address,
    CAST(ed_vault_ed_balance.balance AS TEXT) AS vault_balance,
    ed_vault_ed.from_state_version AS vault_from_state_version,
    ed_vault_ed_balance.from_state_version AS vault_last_updated_at_state_version
FROM variables var
LEFT JOIN LATERAL (
    SELECT *
    FROM entity_resource_totals_history
    WHERE entity_id = var.entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON TRUE
LEFT JOIN LATERAL (
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
            CASE WHEN var.descending_order THEN resource_entity_id END DESC,
            CASE WHEN NOT var.descending_order THEN from_state_version END,
            CASE WHEN NOT var.descending_order THEN resource_entity_id END
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
            CASE WHEN var.descending_order THEN resource_entity_id END DESC,
            CASE WHEN NOT var.descending_order THEN from_state_version END,
            CASE WHEN NOT var.descending_order THEN resource_entity_id END
        LIMIT var.non_fungible_resources_per_entity
    )
) ed ON TRUE
LEFT JOIN entities ed_entity ON ed_entity.id = ed.resource_entity_id
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
        CASE WHEN var.descending_order THEN vault_entity_id END DESC,
        CASE WHEN NOT var.descending_order THEN from_state_version END,
        CASE WHEN NOT var.descending_order THEN vault_entity_id END
    LIMIT var.vaults_per_resource
) ed_vault_ed ON var.use_vault_aggregation
LEFT JOIN entities ed_vault_ed_entity ON ed_vault_ed_entity.id = ed_vault_ed.vault_entity_id
LEFT JOIN LATERAL (
    SELECT *
    FROM vault_balance_history
    WHERE vault_entity_id = ed_vault_ed.vault_entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) ed_vault_ed_balance ON var.use_vault_aggregation
ORDER BY
    CASE WHEN var.descending_order THEN ed.from_state_version END DESC,
    CASE WHEN var.descending_order THEN ed.resource_entity_id END DESC,
    CASE WHEN var.descending_order THEN ed_vault_ed.from_state_version END DESC,
    CASE WHEN var.descending_order THEN ed_vault_ed.vault_entity_id END DESC,
    CASE WHEN NOT var.descending_order THEN ed.from_state_version END,
    CASE WHEN NOT var.descending_order THEN ed.resource_entity_id END,
    CASE WHEN NOT var.descending_order THEN ed_vault_ed.from_state_version END,
    CASE WHEN NOT var.descending_order THEN ed_vault_ed.vault_entity_id END;",
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

        await dapperWrapper.QueryAsync<ResultEntity, ResultResource?, ResultVault?, ResultEntity>(
            dbContext.Database.GetDbConnection(),
            cd,
            (entityRow, resourceRow, vaultRow) =>
            {
                var entity = result.GetOrAdd(entityRow.EntityId, _ => entityRow);

                if (resourceRow == null)
                {
                    return entityRow;
                }

                var resource = entity.InternalResources.GetOrAdd(resourceRow.ResourceEntityId, _ =>
                {
                    if (resourceRow.ResourceType == ResourceType.Fungible)
                    {
                        if (entity.FungibleResources.Count >= configuration.FungibleResourcesPerEntity)
                        {
                            entity.FungibleResourcesNextCursor = new StateVersionIdCursor(resourceRow.ResourceFromStateVersion, resourceRow.ResourceEntryDefinitionId);
                        }
                        else
                        {
                            entity.FungibleResources.Add(resourceRow);
                        }
                    }
                    else
                    {
                        if (entity.NonFungibleResources.Count >= configuration.NonFungibleResourcesPerEntity)
                        {
                            entity.NonFungibleResourcesNextCursor = new StateVersionIdCursor(resourceRow.ResourceFromStateVersion, resourceRow.ResourceEntryDefinitionId);
                        }
                        else
                        {
                            entity.NonFungibleResources.Add(resourceRow);
                        }
                    }

                    return resourceRow;
                });

                if (vaultRow == null)
                {
                    return entityRow;
                }

                if (resource.Vaults.Count >= configuration.VaultsPerResource)
                {
                    resource.VaultsNextCursor = new StateVersionIdCursor(vaultRow.VaultFromStateVersion, vaultRow.VaultEntryDefinitionId);
                }
                else
                {
                    resource.Vaults.Add(vaultRow);
                }

                return entityRow;
            },
            "resource_entry_definition_id,vault_entry_definition_id");

        return result;
    }
}
