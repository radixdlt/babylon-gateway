using Dapper;
using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class NonFungibleResourceQueries
{
    private record NonFungibleDataResultRow(byte[] Schema, long TypeIndex, SborTypeKind SborTypeKind);

    private record NonFungibleIdDataViewModel(string NonFungibleId, bool IsDeleted, byte[] Data, long DataLastUpdatedAtStateVersion);

    private record NonFungibleIdLocationViewModel(string NonFungibleId, bool IsDeleted, long OwnerVaultId, EntityAddress OwnerVaultAddress, long FromStateVersion);

    private record NonFungibleIdLocationVaultOwnerViewModel(long VaultId, long VaultParentAncestorId, EntityAddress VaultParentAncestorAddress, long VaultGlobalAncestorId, EntityAddress VaultGlobalAncestorAddress);

    private record NonFungibleIdsQueryResult(long Id, long FromStateVersion, string NonFungibleId, TokenAmount TotalMinted);

    public static async Task<GatewayModel.StateNonFungibleIdsResponse> NonFungibleIds(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        GlobalNonFungibleResourceEntity resourceEntity,
        GatewayModel.LedgerState ledgerState,
        GatewayModel.IdBoundaryCoursor? cursor,
        int pageSize,
        CancellationToken token = default)
    {
        var cd = new CommandDefinition(
            commandText: $@"
SELECT
    d.id AS Id,
    d.from_state_version AS FromStateVersion,
    d.non_fungible_id AS NonFungibleId,
    CAST(totals.total_minted AS TEXT) AS TotalMinted
FROM non_fungible_id_definition d
INNER JOIN LATERAL(
    SELECT *
    FROM resource_entity_supply_history
    WHERE from_state_version <= {ledgerState.StateVersion} AND resource_entity_id = @nonFungibleResourceEntityId
    ORDER BY from_state_version DESC
    LIMIT 1
) totals ON TRUE
INNER JOIN LATERAL (
    SELECT *
    FROM non_fungible_id_data_history
    WHERE non_fungible_id_definition_id = d.id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) h ON TRUE
WHERE
    d.non_fungible_resource_entity_id = @nonFungibleResourceEntityId
  AND (d.from_state_version, d.id) >= (@cursorStateVersion, @cursorId)
  AND d.from_state_version <= @stateVersion
ORDER BY (d.from_state_version, d.id) ASC
LIMIT @limit
;",
            parameters: new
            {
                nonFungibleResourceEntityId = resourceEntity.Id,
                stateVersion = ledgerState.StateVersion,
                cursorStateVersion = cursor?.StateVersionBoundary ?? 1,
                cursorId = cursor?.IdBoundary ?? 1,
                limit = pageSize + 1,
            },
            cancellationToken: token);

        var entriesAndOneMore = (await dapperWrapper.QueryAsync<NonFungibleIdsQueryResult>(dbContext.Database.GetDbConnection(), cd))
            .ToList();

        var nextCursor = entriesAndOneMore.Count == pageSize + 1
            ? new GatewayModel.IdBoundaryCoursor(entriesAndOneMore.Last().FromStateVersion, entriesAndOneMore.Last().Id).ToCursorString()
            : null;

        long totalCount = entriesAndOneMore.Count != 0 ? long.Parse(entriesAndOneMore.First().TotalMinted.ToString()) : 0;

        var items = entriesAndOneMore
            .Take(pageSize)
            .Select(vm => vm.NonFungibleId)
            .ToList();

        return new GatewayModel.StateNonFungibleIdsResponse(
            ledgerState: ledgerState,
            resourceAddress: resourceEntity.Address,
            nonFungibleIds: new GatewayModel.NonFungibleIdsCollection(
                totalCount: totalCount,
                nextCursor: nextCursor,
                items: items));
    }

    public static async Task<GatewayModel.StateNonFungibleDataResponse> NonFungibleIdData(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        GlobalNonFungibleResourceEntity resourceEntity,
        IList<string> nonFungibleIds,
        byte networkId,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var nonFungibleDataSchemaQuery = new CommandDefinition(
            commandText: @"
SELECT
    sh.schema,
    nfsh.type_index AS TypeIndex,
    nfsh.sbor_type_kind AS SborTypeKind
FROM non_fungible_schema_history nfsh
INNER JOIN schema_entry_definition sh ON sh.schema_hash = nfsh.schema_hash AND sh.entity_id = nfsh.schema_defining_entity_id
WHERE nfsh.resource_entity_id = @entityId AND nfsh.from_state_version <= @stateVersion
ORDER BY nfsh.from_state_version DESC",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = resourceEntity.Id,
            },
            cancellationToken: token);

        var nonFungibleDataSchema = await dapperWrapper.QueryFirstOrDefaultAsync<NonFungibleDataResultRow>(
            dbContext.Database.GetDbConnection(),
            nonFungibleDataSchemaQuery,
            "GetNonFungibleDataSchema"
        );

        if (nonFungibleDataSchema == null)
        {
            throw new UnreachableException($"No schema found for nonfungible resource: {resourceEntity.Address}");
        }

        var cd = new CommandDefinition(
            commandText: @"
SELECT nfid.non_fungible_id AS NonFungibleId, md.is_deleted AS IsDeleted, md.data AS Data, md.from_state_version AS DataLastUpdatedAtStateVersion
FROM non_fungible_id_definition nfid
LEFT JOIN LATERAL (
    SELECT data, is_deleted, from_state_version
    FROM non_fungible_id_data_history nfiddh
    WHERE nfiddh.non_fungible_id_definition_id = nfid.id AND nfiddh.from_state_version <= @stateVersion
    ORDER BY nfiddh.from_state_version DESC
    LIMIT 1
) md ON TRUE
WHERE nfid.from_state_version <= @stateVersion AND nfid.non_fungible_resource_entity_id = @entityId AND nfid.non_fungible_id = ANY(@nonFungibleIds)
ORDER BY nfid.from_state_version DESC
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = resourceEntity.Id,
                nonFungibleIds = nonFungibleIds,
            },
            cancellationToken: token);

        var items = new List<GatewayModel.StateNonFungibleDetailsResponseItem>();

        var result = await dapperWrapper.QueryAsync<NonFungibleIdDataViewModel>(
            dbContext.Database.GetDbConnection(),
            cd,
            "GetNonFungibleData");

        foreach (var vm in result)
        {
            var programmaticJson = !vm.IsDeleted
                ? ScryptoSborUtils.DataToProgrammaticJson(
                    vm.Data,
                    nonFungibleDataSchema.Schema,
                    nonFungibleDataSchema.SborTypeKind,
                    nonFungibleDataSchema.TypeIndex,
                    networkId)
                : null;

            items.Add(
                new GatewayModel.StateNonFungibleDetailsResponseItem(
                    nonFungibleId: vm.NonFungibleId,
                    isBurned: vm.IsDeleted,
                    data: !vm.IsDeleted ? new GatewayModel.ScryptoSborValue(vm.Data.ToHex(), programmaticJson) : null,
                    lastUpdatedAtStateVersion: vm.DataLastUpdatedAtStateVersion));
        }

        return new GatewayModel.StateNonFungibleDataResponse(
            ledgerState: ledgerState,
            resourceAddress: resourceEntity.Address.ToString(),
            nonFungibleIdType: resourceEntity.NonFungibleIdType.ToGatewayModel(),
            nonFungibleIds: items);
    }

    public static async Task<GatewayModel.StateNonFungibleLocationResponse> NonFungibleIdLocation(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        GlobalNonFungibleResourceEntity resourceEntity,
        IList<string> nonFungibleIds,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var vaultLocationsCd = new CommandDefinition(
            commandText: @"
WITH variables (non_fungible_id) AS (
    SELECT UNNEST(@nonFungibleIds)
)
SELECT
    nfid.non_fungible_id AS NonFungibleId,
    md.is_deleted AS IsDeleted,
    lh.vault_entity_id AS OwnerVaultId,
    e.address AS OwnerVaultAddress,
    (CASE WHEN md.is_deleted THEN md.from_state_version ELSE lh.from_state_version END) AS FromStateVersion
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM non_fungible_id_definition
    WHERE non_fungible_resource_entity_id = @resourceEntityId AND non_fungible_id = var.non_fungible_id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) nfid ON TRUE
INNER JOIN LATERAL (
    SELECT is_deleted, from_state_version
    FROM non_fungible_id_data_history
    WHERE non_fungible_id_definition_id = nfid.id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) md ON TRUE
INNER JOIN LATERAL (
    SELECT *
    FROM non_fungible_id_location_history
    WHERE non_fungible_id_definition_id = nfid.id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) lh ON TRUE
INNER JOIN entities e ON e.id = lh.vault_entity_id AND e.from_state_version <= @stateVersion",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                resourceEntityId = resourceEntity.Id,
                nonFungibleIds = nonFungibleIds,
            },
            cancellationToken: token);

        var vaultLocationResults = (await dapperWrapper.QueryAsync<NonFungibleIdLocationViewModel>(dbContext.Database.GetDbConnection(), vaultLocationsCd))
            .ToList();

        var vaultAncestorsCd = new CommandDefinition(
            commandText: @"
SELECT
    e.id AS VaultId,
    pae.id AS VaultParentAncestorId,
    pae.address AS VaultParentAncestorAddress,
    gae.id AS VaultGlobalAncestorId,
    gae.address AS VaultGlobalAncestorAddress
FROM entities e
INNER JOIN entities pae ON e.parent_ancestor_id = pae.id
INNER JOIN entities gae ON e.global_ancestor_id = gae.id
WHERE e.id = ANY(@vaultIds)",
            parameters: new
            {
                vaultIds = vaultLocationResults.Select(x => x.OwnerVaultId).Distinct().ToList(),
            },
            cancellationToken: token);

        var vaultAncestorResults = (await dapperWrapper.QueryAsync<NonFungibleIdLocationVaultOwnerViewModel>(dbContext.Database.GetDbConnection(), vaultAncestorsCd))
            .ToDictionary(e => e.VaultId);

        return new GatewayModel.StateNonFungibleLocationResponse(
            ledgerState: ledgerState,
            resourceAddress: resourceEntity.Address.ToString(),
            nonFungibleIds: vaultLocationResults
                .Select(
                    x => new GatewayModel.StateNonFungibleLocationResponseItem(
                        nonFungibleId: x.NonFungibleId,
                        owningVaultAddress: !x.IsDeleted ? x.OwnerVaultAddress : null,
                        owningVaultParentAncestorAddress: !x.IsDeleted ? vaultAncestorResults[x.OwnerVaultId].VaultParentAncestorAddress : null,
                        owningVaultGlobalAncestorAddress: !x.IsDeleted ? vaultAncestorResults[x.OwnerVaultId].VaultGlobalAncestorAddress : null,
                        isBurned: x.IsDeleted,
                        lastUpdatedAtStateVersion: x.FromStateVersion
                    ))
                .ToList());
    }
}
