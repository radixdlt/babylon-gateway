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

    private record NonFungibleIdLocationVaultOwnerViewModel(
        long VaultId,
        long VaultParentAncestorId,
        EntityAddress VaultParentAncestorAddress,
        long VaultGlobalAncestorId,
        EntityAddress VaultGlobalAncestorAddress);

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
        var parameters = new
        {
            nonFungibleResourceEntityId = resourceEntity.Id,
            stateVersion = ledgerState.StateVersion,
            cursorStateVersion = cursor?.StateVersionBoundary ?? 1,
            cursorId = cursor?.IdBoundary ?? 1,
            limit = pageSize + 1,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
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
            parameters: parameters,
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
        var parameters = new
        {
            stateVersion = ledgerState.StateVersion,
            entityId = resourceEntity.Id,
        };

        var nonFungibleDataSchemaQuery = DapperExtensions.CreateCommandDefinition(
            commandText: @"
SELECT
    sh.schema,
    nfsh.type_index AS TypeIndex,
    nfsh.sbor_type_kind AS SborTypeKind
FROM non_fungible_schema_history nfsh
INNER JOIN schema_entry_definition sh ON sh.schema_hash = nfsh.schema_hash AND sh.entity_id = nfsh.schema_defining_entity_id
WHERE nfsh.resource_entity_id = @entityId AND nfsh.from_state_version <= @stateVersion
ORDER BY nfsh.from_state_version DESC",
            parameters: parameters,
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

        var o = new
        {
            stateVersion = ledgerState.StateVersion,
            entityId = resourceEntity.Id,
            nonFungibleIds = nonFungibleIds,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
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
            parameters: o,
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
        var vaultLocationsParameters = new
        {
            stateVersion = ledgerState.StateVersion,
            resourceEntityId = resourceEntity.Id,
            nonFungibleIds = nonFungibleIds,
        };

        var vaultLocationsCd = DapperExtensions.CreateCommandDefinition(
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
            parameters: vaultLocationsParameters,
            cancellationToken: token);

        var vaultLocationResults = (await dapperWrapper.QueryAsync<NonFungibleIdLocationViewModel>(dbContext.Database.GetDbConnection(), vaultLocationsCd))
            .ToList();

        var vaultAncestorsParameters = new
        {
            vaultIds = vaultLocationResults.Select(x => x.OwnerVaultId).Distinct().ToList(),
        };

        var vaultAncestorsCd = DapperExtensions.CreateCommandDefinition(
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
            parameters: vaultAncestorsParameters,
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
