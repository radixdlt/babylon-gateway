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

using Dapper;
using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries.CustomTypes;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class EntityResourcesQuery
{
    public record PerEntityQueryResultRow(long EntityId, long TotalFungibleResourceCount, long TotalNonFungibleResourceCount)
    {
        public List<ResourceResultRow> FungibleResources { get; } = new();

        public List<ResourceResultRow> NonFungibleResources { get; } = new();

        internal Dictionary<long, ResourceResultRow> AllResources { get; } = new();
    }

    public record ResourceResultRow(
        long ResourceEntryDefinitionId,
        long ResourceEntityId,
        ResourceType ResourceType,
        EntityAddress ResourceEntityAddress,
        TokenAmount ResourceBalance,
        long ResourceFirstSeenStateVersion,
        long ResourceLastUpdatedAtStateVersion,
        long? ResourceVaultTotalCount,
        IdBoundaryCursor? ResourceNextCursorInclusive,
        bool ResourceFilterOut)
    {
        public List<VaultResultRow> Vaults { get; } = new();
    }

    public record VaultResultRow(
        long VaultEntryDefinitionId,
        long VaultEntityId,
        EntityAddress VaultEntityAddress,
        TokenAmount VaultBalance,
        long VaultFirstSeenStateVersion,
        long VaultLastUpdatedAtStateVersion,
        IdBoundaryCursor? VaultNextCursorInclusive,
        bool VaultFilterOut);

    public record struct DetailsQueryConfiguration(int FungibleResourcesPerEntity, int NonFungibleResourcesPerEntity, int VaultsPerResource, long AtLedgerState);

    public record struct ResourcesPageQueryConfiguration(int ResourcesPerEntity, int VaultsPerResource, IdBoundaryCursor? Cursor, long AtLedgerState);

    public record struct VaultsPageQueryConfiguration(int VaultsPerResource, IdBoundaryCursor? Cursor, long AtLedgerState);

    private record struct QueryConfiguration(
        int FungibleResourcesPerEntity,
        int NonFungibleResourcesPerEntity,
        int VaultsPerResource,
        IdBoundaryCursor? ResourceCursor,
        IdBoundaryCursor? VaultCursor,
        long AtLedgerState);

    public static async Task<IDictionary<long, PerEntityQueryResultRow>> Details(
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
            null,
            null,
            detailsConfiguration.AtLedgerState);

        return await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, entityIds, null, configuration, token);
    }

    public static async Task<PerEntityQueryResultRow?> FungibleResourcesPage(
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
            pageConfiguration.Cursor,
            null,
            pageConfiguration.AtLedgerState);
        var results = await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, new[] { entityId }, null, configuration, token);

        results.TryGetValue(entityId, out var result);

        return result;
    }

    public static async Task<PerEntityQueryResultRow?> NonFungibleResourcesPage(
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
            pageConfiguration.Cursor,
            null,
            pageConfiguration.AtLedgerState);
        var results = await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, new[] { entityId }, null, configuration, token);

        results.TryGetValue(entityId, out var result);

        return result;
    }

    public static async Task<PerEntityQueryResultRow?> FungibleResourceVaultsPage(
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
            null,
            pageConfiguration.Cursor,
            pageConfiguration.AtLedgerState);

        var results = await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, new[] { entityId }, resourceId, configuration, token);

        results.TryGetValue(entityId, out var result);

        return result;
    }

    public static async Task<PerEntityQueryResultRow?> NonFungibleResourceVaultsPage(
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
            null,
            pageConfiguration.Cursor,
            pageConfiguration.AtLedgerState);
        var results = await ExecuteEntityResourcesQuery(dbContext, dapperWrapper, new[] { entityId }, resourceId, configuration, token);

        results.TryGetValue(entityId, out var result);

        return result;
    }

    public static void ToGatewayModel(
        this PerEntityQueryResultRow input,
        bool aggregatePerVault,
        IDictionary<long, GatewayModel.EntityMetadataCollection>? explicitMetadata,
        IDictionary<long, GatewayModel.NonFungibleIdsCollection>? nfVaultContents,
        out GatewayModel.FungibleResourcesCollection fungibles,
        out GatewayModel.NonFungibleResourcesCollection nonFungibles)
    {
        var fungibleResourceNextCursorInclusive = input.FungibleResources.SingleOrDefault(x => x.ResourceNextCursorInclusive.HasValue)?.ResourceNextCursorInclusive;
        string? fungibleCursor = fungibleResourceNextCursorInclusive.ToGatewayModel()?.ToCursorString();

        var fungibleItems = input
            .FungibleResources
            .Where(x => !x.ResourceFilterOut)
            .Select(
                f =>
                {
                    GatewayModel.FungibleResourcesCollectionItem val;
                    GatewayModel.EntityMetadataCollection? resourceExplicitMetadata = null;

                    explicitMetadata?.TryGetValue(f.ResourceEntityId, out resourceExplicitMetadata);

                    if (aggregatePerVault)
                    {
                        var vaultNextCursorInclusive = f.Vaults.SingleOrDefault(x => x.VaultNextCursorInclusive.HasValue)?.VaultNextCursorInclusive;
                        string? vaultCursor = vaultNextCursorInclusive.ToGatewayModel()?.ToCursorString();
                        var vaults = f
                            .Vaults
                            .Where(x => !x.VaultFilterOut)
                            .Select(
                                v => new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                                    vaultAddress: v.VaultEntityAddress,
                                    amount: v.VaultBalance.ToString(),
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
                            amount: f.ResourceBalance.ToString(),
                            lastUpdatedAtStateVersion: f.ResourceLastUpdatedAtStateVersion,
                            explicitMetadata: resourceExplicitMetadata);
                    }

                    return val;
                })
            .ToList();

        var nonFungibleResourceNextCursorInclusive = input.NonFungibleResources.SingleOrDefault(x => x.ResourceNextCursorInclusive.HasValue)?.ResourceNextCursorInclusive;
        string? nonFungibleCursor = nonFungibleResourceNextCursorInclusive.ToGatewayModel()?.ToCursorString();
        var nonFungibleItems = input
            .NonFungibleResources
            .Where(x => !x.ResourceFilterOut)
            .Select(
                nf =>
                {
                    GatewayModel.NonFungibleResourcesCollectionItem val;
                    GatewayModel.EntityMetadataCollection? resourceExplicitMetadata = null;

                    explicitMetadata?.TryGetValue(nf.ResourceEntityId, out resourceExplicitMetadata);

                    if (aggregatePerVault)
                    {
                        var vaultNextCursorInclusive = nf.Vaults.SingleOrDefault(x => x.VaultNextCursorInclusive.HasValue)?.VaultNextCursorInclusive;
                        string? vaultCursor = vaultNextCursorInclusive.ToGatewayModel()?.ToCursorString();

                        var vaults = nf
                            .Vaults
                            .Where(x => !x.VaultFilterOut)
                            .Select(
                                v =>
                                {
                                    string? nfidCursor = null;
                                    List<string>? nfids = null;

                                    if (nfVaultContents?.TryGetValue(v.VaultEntityId, out var vaultNfids) == true)
                                    {
                                        nfidCursor = vaultNfids.NextCursor;
                                        nfids = vaultNfids.Items;
                                    }

                                    return new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                                        totalCount: long.Parse(v.VaultBalance.ToString()),
                                        nextCursor: nfidCursor,
                                        items: nfids,
                                        vaultAddress: v.VaultEntityAddress,
                                        lastUpdatedAtStateVersion: v.VaultLastUpdatedAtStateVersion);
                                })
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
                            amount: long.Parse(nf.ResourceBalance.ToString()),
                            lastUpdatedAtStateVersion: nf.ResourceLastUpdatedAtStateVersion,
                            explicitMetadata: resourceExplicitMetadata);
                    }

                    return val;
                })
            .ToList();

        fungibles = new GatewayModel.FungibleResourcesCollection(input.TotalFungibleResourceCount, fungibleCursor, fungibleItems);
        nonFungibles = new GatewayModel.NonFungibleResourcesCollection(input.TotalNonFungibleResourceCount, nonFungibleCursor, nonFungibleItems);
    }

    private static async Task<IDictionary<long, PerEntityQueryResultRow>> ExecuteEntityResourcesQuery(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        ICollection<long> entityIds,
        long? resourceEntityId,
        QueryConfiguration configuration,
        CancellationToken token)
    {
        if (entityIds.Count == 0)
        {
            return ImmutableDictionary<long, PerEntityQueryResultRow>.Empty;
        }

        if (entityIds.Count > 1 && (resourceEntityId.HasValue || configuration.ResourceCursor != null || configuration.VaultCursor != null))
        {
            throw new InvalidOperationException("Neither resource filter nor cursors can be used if executing against multiple entities.");
        }

        var cd = new CommandDefinition(
            @"
WITH variables AS (
    SELECT
        unnest(@entityIds) AS entity_id,
        @resourceEntityId AS resource_entity_id,
        @fungibleResourcesPerEntity AS fungible_resources_per_entity,
        @nonFungibleResourcesPerEntity AS non_fungible_resources_per_entity,
        @vaultsPerResource AS vaults_per_resource,
        @useVaultAggregation as use_vault_aggregation,
        CAST(@useResourceCursor AS bool) AS use_resource_cursor,
        ROW(@resourceCursorStateVersion, @resourceCursorId) AS resource_cursor_inclusive,
        CAST(@useVaultCursor AS bool) AS use_vault_cursor,
        ROW(@vaultCursorStateVersion, @vaultCursorId) AS vault_cursor_inclusive,
        @atLedgerState AS at_ledger_state
),
entity_resource_definitions_with_cursor AS (
     SELECT
         d.*,
         (d.from_state_version, d.id) AS cursor
     FROM entity_resource_entry_definition d
),
entity_resource_vault_definitions_with_cursor AS (
    SELECT
        d.*,
        (d.from_state_version, d.id) AS cursor
    FROM entity_resource_vault_entry_definition d
)
SELECT
    -- entity data
    var.entity_id as EntityId,

    -- totals
    COALESCE(resource_totals.total_fungible_count, 0) AS TotalFungibleResourceCount,
    COALESCE(resource_totals.total_non_fungible_count, 0) AS TotalNonFungibleResourceCount,

    -- resources
    definitions.id AS ResourceEntryDefinitionId,
    definitions.resource_entity_id AS ResourceEntityId,
    definitions.resource_type AS ResourceType,
    CASE WHEN NOT definitions.cursor_only_row THEN resource_entity.address ELSE NULL END AS ResourceEntityAddress,
    CASE WHEN NOT definitions.cursor_only_row THEN CAST(resource_balance_history.balance AS TEXT) ELSE NULL END AS ResourceBalance,
    CASE WHEN NOT definitions.cursor_only_row THEN definitions.from_state_version ELSE NULL END AS ResourceFirstSeenStateVersion,
    CASE WHEN NOT definitions.cursor_only_row THEN resource_balance_history.from_state_version ELSE NULL END AS ResourceLastUpdatedAtStateVersion,
    CASE WHEN NOT definitions.cursor_only_row THEN resource_vault_totals.total_count ELSE NULL END AS ResourceVaultTotalCount,
    CASE WHEN definitions.cursor_only_row THEN definitions.cursor ELSE NULL END AS ResourceNextCursorInclusive,
    CASE WHEN definitions.cursor_only_row THEN TRUE ELSE FALSE END AS ResourceFilterOut,

    -- vaults
    vault_entry_definition.id AS VaultEntryDefinitionId,
    CASE WHEN NOT vault_entry_definition.cursor_only_row THEN vault_entry_definition.vault_entity_id ELSE NULL END AS VaultEntityId,
    CASE WHEN NOT vault_entry_definition.cursor_only_row THEN vault_entity.address ELSE NULL END AS VaultEntityAddress,
    CASE WHEN NOT vault_entry_definition.cursor_only_row THEN CAST(vault_balance_history.balance AS TEXT) ELSE NULL END AS VaultBalance,
    CASE WHEN NOT vault_entry_definition.cursor_only_row THEN vault_entry_definition.from_state_version ELSE NULL END AS VaultFirstSeenStateVersion,
    CASE WHEN NOT vault_entry_definition.cursor_only_row THEN vault_balance_history.from_state_version ELSE NULL END AS VaultLastUpdatedAtStateVersion,
    CASE WHEN vault_entry_definition.cursor_only_row THEN vault_entry_definition.cursor ELSE NULL END AS VaultNextCursorInclusive,
    CASE WHEN vault_entry_definition.cursor_only_row THEN TRUE ELSE FALSE END AS VaultFilterOut
FROM variables var
LEFT JOIN LATERAL (
    SELECT
        total_fungible_count,
        total_non_fungible_count
    FROM entity_resource_totals_history
    WHERE entity_id = var.entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) resource_totals ON TRUE
LEFT JOIN LATERAL (
    (
        SELECT
            d.id,
            d.resource_entity_id,
            d.resource_type,
            d.from_state_version,
            d.cursor,
            ROW_NUMBER() OVER (ORDER BY d.cursor DESC) = var.fungible_resources_per_entity as cursor_only_row
        FROM entity_resource_definitions_with_cursor d
        WHERE
            entity_id = var.entity_id
          AND resource_type = 'fungible'
          AND from_state_version <= var.at_ledger_state
          AND CASE WHEN var.resource_entity_id IS NOT NULL THEN resource_entity_id = var.resource_entity_id ELSE TRUE END
          AND ((NOT var.use_resource_cursor) OR d.cursor <= var.resource_cursor_inclusive)
        ORDER BY d.cursor DESC
        LIMIT var.fungible_resources_per_entity
    )
    UNION ALL
    (
        SELECT
            d.id,
            d.resource_entity_id,
            d.resource_type,
            d.from_state_version,
            d.cursor,
            ROW_NUMBER() OVER (ORDER BY d.cursor DESC) = var.non_fungible_resources_per_entity as cursor_only_row
        FROM entity_resource_definitions_with_cursor d
        WHERE
            entity_id = var.entity_id
          AND resource_type = 'non_fungible'
          AND from_state_version <= var.at_ledger_state
          AND CASE WHEN var.resource_entity_id IS NOT NULL THEN resource_entity_id = var.resource_entity_id ELSE TRUE END
          AND ((NOT var.use_resource_cursor) OR d.cursor <= var.resource_cursor_inclusive)
        ORDER BY d.cursor DESC
        LIMIT var.non_fungible_resources_per_entity
    )
) definitions ON TRUE
LEFT JOIN entities resource_entity ON resource_entity.id = definitions.resource_entity_id
LEFT JOIN LATERAL (
    SELECT *
    FROM entity_resource_balance_history
    WHERE entity_id = var.entity_id AND resource_entity_id = definitions.resource_entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) resource_balance_history ON TRUE
LEFT JOIN LATERAL (
    SELECT
        total_count
    FROM entity_resource_vault_totals_history
    WHERE entity_id = var.entity_id AND resource_entity_id = definitions.resource_entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) resource_vault_totals ON var.use_vault_aggregation
LEFT JOIN LATERAL (
    SELECT
        id,
        vault_entity_id,
        from_state_version,
        cursor,
        ROW_NUMBER() OVER (ORDER BY d.cursor DESC) = var.vaults_per_resource as cursor_only_row
    FROM entity_resource_vault_definitions_with_cursor d
    WHERE
        d.entity_id = var.entity_id
      AND d.resource_entity_id = definitions.resource_entity_id
      AND d.from_state_version <= var.at_ledger_state
      AND ((NOT var.use_vault_cursor) OR d.cursor <= var.vault_cursor_inclusive)
    ORDER BY d.cursor DESC
    LIMIT var.vaults_per_resource
) vault_entry_definition ON var.use_vault_aggregation
LEFT JOIN entities vault_entity ON vault_entity.id = vault_entry_definition.vault_entity_id
LEFT JOIN LATERAL (
    SELECT
        balance,
        from_state_version
    FROM vault_balance_history
    WHERE vault_entity_id = vault_entry_definition.vault_entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) vault_balance_history ON var.use_vault_aggregation
ORDER BY definitions.cursor DESC, vault_entry_definition.cursor DESC",
            new
            {
                entityIds = entityIds.ToList(),
                resourceEntityId = resourceEntityId,
                fungibleResourcesPerEntity = configuration.FungibleResourcesPerEntity == 0 ? 0 : configuration.FungibleResourcesPerEntity + 1,
                nonFungibleResourcesPerEntity = configuration.NonFungibleResourcesPerEntity == 0 ? 0 : configuration.NonFungibleResourcesPerEntity + 1,
                vaultsPerResource = configuration.VaultsPerResource == 0 ? 0 : configuration.VaultsPerResource + 1,
                useVaultAggregation = configuration.VaultsPerResource > 0,
                useResourceCursor = configuration.ResourceCursor is not null,
                resourceCursorStateVersion = configuration.ResourceCursor?.StateVersion,
                resourceCursorId = configuration.ResourceCursor?.Id,
                useVaultCursor = configuration.VaultCursor is not null,
                vaultCursorStateVersion = configuration.VaultCursor?.StateVersion,
                vaultCursorId = configuration.VaultCursor?.Id,
                atLedgerState = configuration.AtLedgerState,
            },
            cancellationToken: token);

        var result = new Dictionary<long, PerEntityQueryResultRow>();

        await dapperWrapper.QueryAsync<PerEntityQueryResultRow, ResourceResultRow?, VaultResultRow?, PerEntityQueryResultRow>(
            dbContext.Database.GetDbConnection(),
            cd,
            (entityRow, resourceRow, vaultRow) =>
            {
                var entity = result.GetOrAdd(entityRow.EntityId, _ => entityRow);

                if (resourceRow == null)
                {
                    return entityRow;
                }

                var resource = entity.AllResources.GetOrAdd(
                    resourceRow.ResourceEntityId,
                    _ =>
                    {
                        if (resourceRow.ResourceType == ResourceType.Fungible)
                        {
                            entity.FungibleResources.Add(resourceRow);
                        }
                        else
                        {
                            entity.NonFungibleResources.Add(resourceRow);
                        }

                        return resourceRow;
                    });

                if (vaultRow == null)
                {
                    return entityRow;
                }

                resource.Vaults.Add(vaultRow);

                return entityRow;
            },
            "ResourceEntryDefinitionId,VaultEntryDefinitionId");

        return result;
    }
}
