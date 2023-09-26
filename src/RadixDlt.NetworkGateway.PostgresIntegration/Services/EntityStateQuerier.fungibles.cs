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
                resourceEndIndex = resourceOffset + resourceLimit,
                vaultStartIndex = vaultOffset + 1,
                vaultEndIndex = vaultOffset + vaultLimit,
            },
            cancellationToken: token);

        var resourcesTotalCount = 0;

        var resources = new Dictionary<EntityAddress, GatewayModel.FungibleResourcesCollectionItemVaultAggregated>();

        foreach (var vm in await _dapperWrapper.QueryAsync<FungibleAggregatedPerVaultViewModel>(_dbContext.Database.GetDbConnection(), cd))
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
                endIndex = offset + limit,
            },
            cancellationToken: token);

        var result = (await _dapperWrapper.QueryAsync<FungibleResourceVaultsViewModel>(_dbContext.Database.GetDbConnection(), cd)).ToList();
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
