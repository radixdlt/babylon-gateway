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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal partial class EntityStateQuerier
{
    private record NonFungibleViewModel(EntityAddress ResourceEntityAddress, long NonFungibleIdsCount, int ResourcesTotalCount, long LastUpdatedAtStateVersion);

    private record NonFungibleResourceVaultsViewModel(
        EntityAddress ResourceEntityAddress,
        long VaultEntityId,
        EntityAddress VaultAddress,
        long NonFungibleIdsCount,
        int VaultTotalCount,
        long LastUpdatedAtStateVersion);

    private record NonFungibleAggregatedPerVaultViewModel(
        long ResourceEntityId,
        long VaultEntityId,
        EntityAddress ResourceEntityAddress,
        EntityAddress VaultAddress,
        long NonFungibleIdsCount,
        int ResourceTotalCount,
        int VaultTotalCount,
        long LastUpdatedAtStateVersion);

    private record NonFungibleIdViewModel(string NonFungibleId, int NonFungibleIdsTotalCount);

    private record NonFungibleIdWithOwnerDataViewModel(string NonFungibleId, long EntityId, long ResourceEntityId, long VaultEntityId);

    private record NonFungibleDataSchemaModel(byte[] Schema, long TypeIndex, SborTypeKind SborTypeKind);

    private record NonFungibleIdDataViewModel(string NonFungibleId, bool IsDeleted, byte[] Data, long DataLastUpdatedAtStateVersion);

    private record NonFungibleIdLocationViewModel(string NonFungibleId, bool IsDeleted, long OwnerVaultId, EntityAddress OwnerVaultAddress, long FromStateVersion);

    private record struct NonFungibleIdOwnerLookup(long EntityId, long ResourceEntityId, long VaultEntityId);

    private async Task<GatewayModel.NonFungibleResourcesCollection> GetNonFungiblesSliceAggregatedPerResource(
        long entityId,
        int offset,
        int limit,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
WITH most_recent_entity_resource_aggregate_history_nested AS (
    SELECT non_fungible_resource_entity_ids
    FROM entity_resource_aggregate_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId
    ORDER BY from_state_version DESC
    LIMIT 1
),
most_recent_entity_resource_aggregate_history AS (
    SELECT a.val AS non_fungible_resource_entity_id, cardinality(non_fungible_resource_entity_ids) AS resources_total_count, a.ord AS ord
    FROM most_recent_entity_resource_aggregate_history_nested
    LEFT JOIN LATERAL UNNEST(non_fungible_resource_entity_ids[@startIndex:@endIndex]) WITH ORDINALITY a(val,ord)  ON true
)
SELECT
    e.address AS ResourceEntityAddress,
    eravh.total_count AS NonFungibleIdsCount,
    ah.resources_total_count AS ResourcesTotalCount,
    eravh.from_state_version AS LastUpdatedAtStateVersion
FROM most_recent_entity_resource_aggregate_history ah
INNER JOIN LATERAL (
    SELECT total_count, from_state_version
    FROM entity_resource_aggregated_vaults_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId AND resource_entity_id = ah.non_fungible_resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
    ) eravh ON TRUE
INNER JOIN entities e ON ah.non_fungible_resource_entity_id = e.id
order by ah.ord;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                startIndex = offset + 1,
                endIndex = offset + limit,
            },
            cancellationToken: token);

        var totalCount = 0;

        var items = new List<GatewayModel.NonFungibleResourcesCollectionItem>();

        foreach (var vm in await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleViewModel>(cd))
        {
            totalCount = vm.ResourcesTotalCount;

            items.Add(new GatewayModel.NonFungibleResourcesCollectionItemGloballyAggregated(
                resourceAddress: vm.ResourceEntityAddress,
                amount: vm.NonFungibleIdsCount,
                lastUpdatedAtStateVersion: vm.LastUpdatedAtStateVersion));
        }

        return new GatewayModel.NonFungibleResourcesCollection(totalCount, GenerateOffsetCursor(offset, limit, totalCount), items);
    }

    private async Task<List<NonFungibleAggregatedPerVaultViewModel>> GetNonFungiblesSliceAggregatedPerVault(
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
    SELECT non_fungible_resource_entity_ids
    FROM entity_resource_aggregate_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId
    ORDER BY from_state_version DESC
    LIMIT 1
),
most_recent_entity_resource_aggregate_history AS (
    SELECT a.val AS non_fungible_resource_entity_id, cardinality(non_fungible_resource_entity_ids) AS resource_total_count, a.ord AS resource_order
    FROM most_recent_entity_resource_aggregate_history_nested
    LEFT JOIN LATERAL UNNEST(non_fungible_resource_entity_ids[@resourceStartIndex:@resourceEndIndex]) WITH ORDINALITY a(val,ord) ON true
),
most_recent_entity_resource_vault_aggregate_history_nested AS (
    SELECT rah.non_fungible_resource_entity_id, rah.resource_total_count, vah.vault_entity_ids, rah.resource_order
    FROM most_recent_entity_resource_aggregate_history rah
    INNER JOIN LATERAL (
        SELECT vault_entity_ids
        FROM entity_resource_vault_aggregate_history
        WHERE from_state_version <= @stateVersion AND entity_id = @entityId AND resource_entity_id = rah.non_fungible_resource_entity_id
        ORDER BY from_state_version DESC
        LIMIT 1
    ) vah ON TRUE
),
most_recent_entity_resource_vault_aggregate_history AS (
    SELECT
        ahn.non_fungible_resource_entity_id,
        ahn.resource_total_count,
        a.val AS vault_entity_id,
        cardinality(vault_entity_ids) AS vault_total_count,
        ahn.resource_order,
        a.ord AS vault_order
    FROM most_recent_entity_resource_vault_aggregate_history_nested ahn
    LEFT JOIN LATERAL UNNEST(vault_entity_ids[@vaultStartIndex:@vaultEndIndex]) WITH ORDINALITY a(val,ord) ON true
)
SELECT
    vah.non_fungible_resource_entity_id as ResourceEntityId,
    vah.vault_entity_id AS VaultEntityId,
    er.address AS ResourceEntityAddress,
    ev.address AS VaultAddress,
    vh.NonFungibleIdsCount,
    vah.resource_total_count AS ResourceTotalCount,
    vah.vault_total_count AS VaultTotalCount,
    vh.from_state_version AS LastUpdatedAtStateVersion
FROM most_recent_entity_resource_vault_aggregate_history vah
INNER JOIN LATERAL (
    SELECT CAST(cardinality(non_fungible_ids) AS bigint) AS NonFungibleIdsCount, from_state_version
    FROM entity_vault_history
    WHERE from_state_version <= @stateVersion AND global_entity_id = @entityId AND resource_entity_id = vah.non_fungible_resource_entity_id AND vault_entity_id = vah.vault_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) vh ON TRUE
INNER JOIN entities er ON vah.non_fungible_resource_entity_id = er.id
INNER JOIN entities ev ON vah.vault_entity_id = ev.id
ORDER BY vah.resource_order, vah.vault_order;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                resourceStartIndex = resourceOffset + 1,
                resourceEndIndex = resourceOffset + resourceLimit,
                vaultStartIndex = vaultOffset + 1,
                vaultEndIndex = vaultOffset + vaultLimit,
            },
            cancellationToken: token);

        return (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleAggregatedPerVaultViewModel>(cd)).ToList();
    }

    private async Task<List<NonFungibleResourceVaultsViewModel>> GetNonFungibleResourceVaults(
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
    SELECT non_fungible_resource_entity_ids
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
SELECT
    er.address AS ResourceEntityAddress,
    vah.vault_entity_id AS VaultEntityId,
    ev.address AS VaultAddress,
    vh.NonFungibleIdsCount,
    vah.vault_total_count AS VaultTotalCount,
    vh.from_state_version AS LastUpdatedAtStateVersion
FROM most_recent_entity_resource_vault_aggregate_history vah
INNER JOIN LATERAL (
    SELECT CAST(cardinality(non_fungible_ids) AS bigint) AS NonFungibleIdsCount, from_state_version
    FROM entity_vault_history
    WHERE from_state_version <= @stateVersion AND global_entity_id = @entityId AND resource_entity_id = @resourceEntityId AND vault_entity_id = vah.vault_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
    ) vh ON TRUE
INNER JOIN entities er ON  er.id = @resourceEntityId
INNER JOIN entities ev ON vah.vault_entity_id = ev.id
ORDER BY vah.ord;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                resourceEntityId = resourceEntityId,
                startIndex = offset + 1,
                endIndex = offset + limit,
            },
            cancellationToken: token);

        return (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleResourceVaultsViewModel>(cd)).ToList();
    }

    private async Task<GatewayModel.NonFungibleIdsCollection> GetNonFungibleIdsSlice(
        long entityId,
        long resourceEntityId,
        long vaultEntityId,
        int offset,
        int limit,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
SELECT nfid.non_fungible_id AS NonFungibleId, final.total_count AS NonFungibleIdsTotalCount
FROM (
    SELECT a.val AS non_fungible_id_data_id, cardinality(non_fungible_ids) AS total_count, a.ord AS ord
    FROM entity_vault_history
    LEFT JOIN LATERAL UNNEST(non_fungible_ids[@startIndex:@endIndex]) WITH ORDINALITY a(val,ord) ON true
    WHERE id = (
        SELECT id
        FROM entity_vault_history
        WHERE from_state_version <= @stateVersion AND global_entity_id = @entityId AND resource_entity_id = @resourceEntityId AND vault_entity_id = @vaultEntityId
        ORDER BY from_state_version DESC
        LIMIT 1
    )
) final
INNER JOIN non_fungible_id_data nfid ON nfid.id = final.non_fungible_id_data_id
order by ord
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                vaultEntityId = vaultEntityId,
                resourceEntityId = resourceEntityId,
                startIndex = offset + 1,
                endIndex = offset + limit,
            },
            cancellationToken: token);

        var totalCount = 0;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdViewModel>(cd))
            .ToList()
            .Select(vm =>
            {
                totalCount = vm.NonFungibleIdsTotalCount;

                return vm.NonFungibleId;
            })
            .ToList();

        return new GatewayModel.NonFungibleIdsCollection(totalCount, GenerateOffsetCursor(offset, limit, totalCount), items);
    }

    private async Task<List<NonFungibleIdWithOwnerDataViewModel>> GetNonFungibleIdsFirstPage(
        long[] entityIds,
        long[] resourceEntityIds,
        long[] vaultEntityIds,
        int nonFungibleIdsLimit,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
WITH variables (entity_id, resource_entity_id, vault_entity_id) AS (
    SELECT
        UNNEST(@entityIds),
        UNNEST(@resourceEntityIds),
        UNNEST(@vaultEntityIds)
),
unnested_nfids AS (
    SELECT
        v.id,
        v.global_entity_id,
        v.resource_entity_id,
        v.vault_entity_id,
        UNNEST(v.non_fungible_ids[@startIndex:@endIndex]) as unnested_non_fungible_id
    FROM variables
    LEFT JOIN LATERAL
    (
        SELECT
            id,
            non_fungible_ids,
            global_entity_id,
            resource_entity_id,
            vault_entity_id
        FROM entity_vault_history
        WHERE from_state_version <= @stateVersion AND
                global_entity_id = variables.entity_id AND
                resource_entity_id = variables.resource_entity_id AND
                vault_entity_id = variables.vault_entity_id
        ORDER BY from_state_version DESC
        LIMIT 1
    ) v ON true
)
SELECT
    nfid.non_fungible_id as NonFungibleId,
    un.global_entity_id as EntityId,
    un.resource_entity_id as ResourceEntityId,
    un.vault_entity_id as VaultEntityId
FROM unnested_nfids un
INNER JOIN non_fungible_id_data nfid ON nfid.id = un.unnested_non_fungible_id
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityIds = entityIds,
                vaultEntityIds = vaultEntityIds,
                resourceEntityIds = resourceEntityIds,
                startIndex = 1,
                endIndex = 1 + nonFungibleIdsLimit,
            },
            cancellationToken: token);

        var result = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdWithOwnerDataViewModel>(cd)).ToList();

        return result;
    }

    private async Task<Dictionary<long, GatewayModel.NonFungibleResourcesCollection>> NonFungiblesAggregatedPerResourcePage(
        long[] entityIds,
        int offset,
        int limit,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var result = new Dictionary<long, GatewayModel.NonFungibleResourcesCollection>();

        foreach (var entityId in entityIds)
        {
            result[entityId] = await GetNonFungiblesSliceAggregatedPerResource(entityId, offset, limit, ledgerState, token);
        }

        return result;
    }

    private async Task<Dictionary<long, GatewayModel.NonFungibleResourcesCollection>> NonFungiblesAggregatedPerVaultPage(
        long[] entityIds,
        bool includeNfids,
        int offset,
        int limit,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var nonFungiblesSliceAggregatedPerVault = new Dictionary<long, List<NonFungibleAggregatedPerVaultViewModel>>();
        var nonFungibleIdOwnerLookup = new List<NonFungibleIdOwnerLookup>();

        foreach (var entityId in entityIds)
        {
            var entityResult = await GetNonFungiblesSliceAggregatedPerVault(entityId, offset, limit, 0, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token);
            nonFungiblesSliceAggregatedPerVault[entityId] = entityResult;
            nonFungibleIdOwnerLookup.AddRange(entityResult.Select(row => new NonFungibleIdOwnerLookup(entityId, row.ResourceEntityId, row.VaultEntityId)).ToArray());
        }

        IEnumerable<IGrouping<long, NonFungibleIdWithOwnerDataViewModel>>? nonFungibleIdsFirstPage = null;

        if (includeNfids && nonFungibleIdOwnerLookup.Any())
        {
            var nonFungibleIds = await GetNonFungibleIdsFirstPage(
                nonFungibleIdOwnerLookup.Select(x => x.EntityId).ToArray(),
                nonFungibleIdOwnerLookup.Select(x => x.ResourceEntityId).ToArray(),
                nonFungibleIdOwnerLookup.Select(x => x.VaultEntityId).ToArray(),
                _endpointConfiguration.Value.DefaultPageSize,
                ledgerState,
                token);

            nonFungibleIdsFirstPage = nonFungibleIds.GroupBy(x => x.EntityId);
        }

        return nonFungiblesSliceAggregatedPerVault.ToDictionary(
            x => x.Key,
            x => MapToNonFungibleResourcesCollection(x.Value, nonFungibleIdsFirstPage?.FirstOrDefault(y => y.Key == x.Key)?.ToList(), offset, limit, 0, _endpointConfiguration.Value.DefaultPageSize)
        );
    }

    private GatewayModel.NonFungibleResourcesCollection MapToNonFungibleResourcesCollection(
        List<NonFungibleAggregatedPerVaultViewModel> input,
        List<NonFungibleIdWithOwnerDataViewModel>? nonFungibleIds,
        int resourceOffset,
        int resourceLimit,
        int vaultOffset,
        int vaultLimit)
    {
        var resourcesTotalCount = 0;
        var resources = new Dictionary<EntityAddress, GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregated>();

        foreach (var vm in input)
        {
            resourcesTotalCount = vm.ResourceTotalCount;

            if (!resources.TryGetValue(vm.ResourceEntityAddress, out var existingRecord))
            {
                existingRecord = new GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregated(
                    resourceAddress: vm.ResourceEntityAddress,
                    vaults: new GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregatedVault(
                        totalCount: vm.VaultTotalCount,
                        nextCursor: GenerateOffsetCursor(vaultOffset, vaultLimit, vm.VaultTotalCount),
                        items: new List<GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem>()));

                resources[vm.ResourceEntityAddress] = existingRecord;
            }

            var ids = nonFungibleIds?
                .Where(x => x.ResourceEntityId == vm.ResourceEntityId && x.VaultEntityId == vm.VaultEntityId)
                .Select(x => x.NonFungibleId)
                .ToList();

            existingRecord.Vaults.Items.Add(new GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                totalCount: vm.NonFungibleIdsCount,
                vaultAddress: vm.VaultAddress,
                lastUpdatedAtStateVersion: vm.LastUpdatedAtStateVersion,
                items: ids));
        }

        var items = resources.Values.Cast<GatewayApiSdk.Model.NonFungibleResourcesCollectionItem>().ToList();

        return new GatewayModel.NonFungibleResourcesCollection(resourcesTotalCount, GenerateOffsetCursor(resourceOffset, resourceLimit, resourcesTotalCount), items);
    }

    private GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse MapToStateEntityNonFungibleResourceVaultsPageResponse(
        List<NonFungibleResourceVaultsViewModel> input,
        Dictionary<long, List<NonFungibleIdWithOwnerDataViewModel>>? nonFungibleIds,
        GatewayModel.LedgerState ledgerState,
        int offset,
        int limit,
        string? entityGlobalAddress,
        string? resourceGlobalAddress)
    {
        var mapped = input
            .Select(x =>
                {
                    List<EntityStateQuerier.NonFungibleIdWithOwnerDataViewModel>? items = null;

                    var hasItems = nonFungibleIds?.TryGetValue(x.VaultEntityId, out items);
                    return new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                        totalCount: x.NonFungibleIdsCount,
                        vaultAddress: x.VaultAddress,
                        lastUpdatedAtStateVersion: x.LastUpdatedAtStateVersion,
                        items: hasItems == true && items != null ? items.Select(y => y.NonFungibleId).ToList() : null
                    );
                }
            )
            .ToList();

        var vaultsTotalCount = input.FirstOrDefault()?.VaultTotalCount ?? 0;
        var nextCursor = GenerateOffsetCursor(offset, limit, vaultsTotalCount);

        return new GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse(ledgerState, vaultsTotalCount, nextCursor, mapped, entityGlobalAddress, resourceGlobalAddress);
    }
}
