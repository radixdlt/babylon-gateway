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
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class EntityStateQuerier : IEntityStateQuerier
{
    private const int DefaultMetadataLimit = 10; // TODO make it configurable
    private const int DefaultResourceLimit = 5; // TODO make it configurable

    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ReadOnlyDbContext _dbContext;

    public EntityStateQuerier(INetworkConfigurationProvider networkConfigurationProvider, ReadOnlyDbContext dbContext)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
        _dbContext = dbContext;
    }

    public async Task<GatewayModel.EntityResourcesResponse?> EntityResourcesSnapshot(RadixAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.GlobalAddress == address, token);

        if (entity is not ComponentEntity)
        {
            return null;
        }

        // TODO refactor so that we drop EF completely and access two connections + Task.WhenAll at the same time

        var fungibles = await GetFungiblesSlice(entity.Id, 0, DefaultResourceLimit, ledgerState, token);
        var nonFungibles = await GetNonFungiblesSlice(entity.Id, 0, DefaultResourceLimit, ledgerState, token);

        return new GatewayModel.EntityResourcesResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), fungibles, nonFungibles);
    }

    private record NonFungibleIdViewModel(byte[] NonFungibleId, bool IsDeleted, byte[] ImmutableData, byte[] MutableData);

    public async Task<GatewayModel.EntityDetailsResponse?> EntityDetailsSnapshot(RadixAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.GlobalAddress == address, token);

        if (entity == null)
        {
            return null;
        }

        GatewayModel.EntityDetailsResponseDetails details;

        switch (entity)
        {
            case FungibleResourceManagerEntity frme:
            {
                var supplyHistory = await _dbContext.FungibleResourceSupplyHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.ResourceEntityId == frme.Id)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                var tokenAddress = entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition());

                details = new GatewayModel.EntityDetailsResponseDetails(new GatewayModel.EntityDetailsResponseFungibleResourceDetails(
                    discriminator: GatewayModel.EntityDetailsResponseDetailsType.FungibleResource,
                    divisibility: frme.Divisibility,
                    totalSupply: new GatewayModel.TokenAmount(supplyHistory.TotalSupply.ToString(), tokenAddress),
                    totalMinted: new GatewayModel.TokenAmount(supplyHistory.TotalMinted.ToString(), tokenAddress),
                    totalBurnt: new GatewayModel.TokenAmount(supplyHistory.TotalBurnt.ToString(), tokenAddress)));

                break;
            }

            case NonFungibleResourceManagerEntity nfrme:
            {
                var dbConn = _dbContext.Database.GetDbConnection();

                // TODO ORDER BY nfih.from_state_version DESC might be insufficient as we might have more than one NFID created in single state_version (add ", id ASC"?)

                var nonFungibleIds = await dbConn.QueryAsync<NonFungibleIdViewModel>(new CommandDefinition(
                    commandText: @"
SELECT nfid.non_fungible_id AS NonFungibleId, nfimdh.is_deleted AS IsDeleted, nfid.immutable_data AS ImmutableData, nfimdh.mutable_data AS MutableData
FROM non_fungible_id_data nfid
INNER JOIN LATERAL (
    SELECT is_deleted, mutable_data
    FROM non_fungible_id_mutable_data_history nfimdh
    WHERE nfimdh.from_state_version <= @stateVersion AND nfid.id = nfimdh.non_fungible_id_data_id
    ORDER BY nfid.from_state_version DESC
    LIMIT 1
) nfimdh ON TRUE
WHERE nfid.from_state_version <= @stateVersion AND nfid.non_fungible_resource_manager_entity_id = @entityId
ORDER BY nfid.from_state_version DESC
OFFSET @offset LIMIT @limit",
                    parameters: new
                    {
                        stateVersion = ledgerState.StateVersion,
                        entityId = nfrme.Id,
                        offset = 0,
                        limit = DefaultResourceLimit,
                    },
                    cancellationToken: token));

                details = new GatewayModel.EntityDetailsResponseDetails(new GatewayModel.EntityDetailsResponseNonFungibleResourceDetails(
                    discriminator: GatewayModel.EntityDetailsResponseDetailsType.NonFungibleResource,
                    ids: new GatewayModel.EntityDetailsResponseNonFungibleResourceDetailsIds(
                        nextCursor: "TBD (currently first 10 NFIDs are returned)",
                        items: nonFungibleIds
                            .Select(nfid => new GatewayModel.EntityDetailsResponseNonFungibleResourceDetailsIdsItem(
                                idHex: nfid.NonFungibleId.ToHex(),
                                immutableDataHex: nfid.ImmutableData.ToHex(),
                                mutableDataHex: nfid.MutableData.ToHex()))
                            .ToList())));
                break;
            }

            case PackageEntity pe:
                details = new GatewayModel.EntityDetailsResponseDetails(new GatewayModel.EntityDetailsResponsePackageDetails(
                    discriminator: GatewayModel.EntityDetailsResponseDetailsType.Package,
                    codeHex: pe.Code.ToHex()));
                break;

            case AccountComponentEntity ace:
                var package = await _dbContext.Entities
                    .FirstAsync(e => e.Id == ace.PackageId, token);

                details = new GatewayModel.EntityDetailsResponseDetails(new GatewayModel.EntityDetailsResponseAccountComponentDetails(
                    discriminator: GatewayModel.EntityDetailsResponseDetailsType.AccountComponent,
                    packageAddress: package.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition())));
                break;
            default:
                return null;
        }

        var metadata = await GetMetadataSlice(entity.Id, 0, DefaultMetadataLimit, ledgerState, token);

        return new GatewayModel.EntityDetailsResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), metadata, details);
    }

    public async Task<GatewayModel.EntityOverviewResponse> EntityOverview(ICollection<RadixAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var addressesList = addresses.ToList();

        var entities = await _dbContext.Entities
            .Where(e => e.GlobalAddress != null && addressesList.Contains(e.GlobalAddress))
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .ToListAsync(token);

        var metadata = await GetMetadataSlices(entities.Select(e => e.Id).ToArray(), 0, DefaultMetadataLimit, ledgerState, token);

        var items = entities
            .Select(entity => new GatewayModel.EntityOverviewResponseEntityItem(entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), metadata[entity.Id]))
            .ToList();

        return new GatewayModel.EntityOverviewResponse(ledgerState, items);
    }

    public async Task<GatewayModel.EntityMetadataResponse?> EntityMetadata(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.GlobalAddress == request.Address, token);

        if (entity == null)
        {
            return null;
        }

        var metadata = await GetMetadataSlice(entity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityMetadataResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), metadata);
    }

    public async Task<GatewayModel.EntityFungiblesResponse?> EntityFungibles(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.GlobalAddress == request.Address, token);

        if (entity == null)
        {
            return null;
        }

        var fungibles = await GetFungiblesSlice(entity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityFungiblesResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), fungibles);
    }

    public async Task<GatewayModel.EntityNonFungiblesResponse?> EntityNonFungibles(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.GlobalAddress == request.Address, token);

        if (entity == null)
        {
            return null;
        }

        var nonFungibles = await GetNonFungiblesSlice(entity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityNonFungiblesResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), nonFungibles);
    }

    private record MetadataViewModel(long EntityId, string[] Keys, string[] Values, int TotalCount);

    private record FungiblesViewModel(byte[] ResourceEntityGlobalAddress, string Balance, int TotalCount);

    private record NonFungiblesViewModel(byte[] ResourceEntityGlobalAddress, long NonFungibleIdsCount, int TotalCount);

    private async Task<GatewayModel.EntityMetadataCollection> GetMetadataSlice(long entityId, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var result = await GetMetadataSlices(new[] { entityId }, offset, limit, ledgerState, token);

        return result[entityId];
    }

    private async Task<Dictionary<long, GatewayModel.EntityMetadataCollection>> GetMetadataSlices(long[] entityIds, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var result = new Dictionary<long, GatewayModel.EntityMetadataCollection>();

        var cd = new CommandDefinition(
            commandText: @"
WITH entities (id) AS (
    SELECT UNNEST(@entityIds)
)
SELECT emh.*
FROM entities
INNER JOIN LATERAL (
    SELECT entity_id AS EntityId, keys[@offset:@limit] AS Keys, values[@offset:@limit] AS Values, array_length(keys, 1) AS TotalCount
    FROM entity_metadata_history
    WHERE entity_id = entities.id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) emh ON true;",
            parameters: new
            {
                entityIds = entityIds,
                stateVersion = ledgerState.StateVersion,
                offset = offset + 1,
                limit = offset + limit,
            },
            cancellationToken: token);

        foreach (var vm in await _dbContext.Database.GetDbConnection().QueryAsync<MetadataViewModel>(cd))
        {
            var items = vm.Keys.Zip(vm.Values)
                .Select(rm => new GatewayModel.EntityMetadataItem(rm.First, rm.Second))
                .ToList();

            var previousCursor = offset > 0
                ? new GatewayModel.EntityMetadataRequestCursor(Math.Max(offset - limit, 0)).ToCursorString()
                : null;

            var nextCursor = offset + limit < vm.TotalCount
                ? new GatewayModel.EntityMetadataRequestCursor(offset + limit).ToCursorString()
                : null;

            result[vm.EntityId] = new GatewayModel.EntityMetadataCollection(vm.TotalCount, previousCursor, nextCursor, items);
        }

        foreach (var missing in entityIds.Except(result.Keys))
        {
            result[missing] = GatewayModel.EntityMetadataCollection.Empty;
        }

        return result;
    }

    private async Task<GatewayModel.FungibleResourcesCollection> GetFungiblesSlice(long entityId, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
WITH aggregate_history_resources AS (
    SELECT fungible_resource_entity_ids
    FROM entity_resource_aggregate_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId
    ORDER BY from_state_version DESC
    LIMIT 1
),
aggregate_history AS (
    SELECT UNNEST(fungible_resource_entity_ids[@offset:@limit]) AS fungible_resource_entity_id, array_length(fungible_resource_entity_ids, 1) AS TotalCount
    FROM aggregate_history_resources
)
SELECT final.global_address AS ResourceEntityGlobalAddress, final.balance::text AS Balance, ah.TotalCount
FROM aggregate_history ah
INNER JOIN LATERAL (
    SELECT e.global_address, erh.balance
    FROM entity_resource_history erh
    INNER JOIN entities e ON erh.resource_entity_id = e.id
    WHERE erh.from_state_version <= @stateVersion AND erh.global_entity_id = @entityId AND erh.resource_entity_id = ah.fungible_resource_entity_id
    ORDER BY erh.from_state_version DESC
    LIMIT 1
) final ON true;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                offset = offset + 1,
                limit = offset + 1 + limit,
            },
            cancellationToken: token);

        long? totalCount = null;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<FungiblesViewModel>(cd)).ToList()
            .Select(vm =>
            {
                var rga = vm.ResourceEntityGlobalAddress ?? throw new InvalidOperationException("Non-global entity.");
                var ra = RadixAddressCodec.Encode(_networkConfigurationProvider.GetHrpDefinition().Resource, rga);

                totalCount ??= vm.TotalCount;

                return new GatewayModel.FungibleResourcesCollectionItem(ra, new GatewayModel.TokenAmount(TokenAmount.FromSubUnitsString(vm.Balance).ToString(), ra));
            })
            .ToList();

        var previousCursor = offset > 0
            ? new GatewayModel.EntityFungiblesCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > limit
            ? new GatewayModel.EntityFungiblesCursor(offset + limit).ToCursorString()
            : null;

        return new GatewayModel.FungibleResourcesCollection(totalCount, previousCursor, nextCursor, items.Take(limit).ToList());
    }

    private async Task<GatewayModel.NonFungibleResourcesCollection> GetNonFungiblesSlice(long entityId, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
WITH aggregate_history_resources AS (
    SELECT non_fungible_resource_entity_ids
    FROM entity_resource_aggregate_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId
    ORDER BY from_state_version DESC
    LIMIT 1
),
aggregate_history AS (
    SELECT UNNEST(non_fungible_resource_entity_ids[@offset:@limit]) AS non_fungible_resource_entity_id, array_length(non_fungible_resource_entity_ids, 1) AS TotalCount
    FROM aggregate_history_resources
)
SELECT final.global_address AS ResourceEntityGlobalAddress, final.non_fungible_ids_count AS NonFungibleIdsCount, ah.TotalCount
FROM aggregate_history ah
INNER JOIN LATERAL (
    SELECT e.global_address, erh.non_fungible_ids_count
    FROM entity_resource_history erh
    INNER JOIN entities e ON erh.resource_entity_id = e.id
    WHERE erh.from_state_version <= @stateVersion AND erh.global_entity_id = @entityId AND erh.resource_entity_id = ah.non_fungible_resource_entity_id
    ORDER BY erh.from_state_version DESC
    LIMIT 1
) final ON true;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                offset = offset + 1,
                limit = offset + 1 + limit,
            },
            cancellationToken: token);

        long? totalCount = null;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungiblesViewModel>(cd)).ToList()
            .Select(vm =>
            {
                var rga = vm.ResourceEntityGlobalAddress ?? throw new InvalidOperationException("Non-global entity.");
                var ra = RadixAddressCodec.Encode(_networkConfigurationProvider.GetHrpDefinition().Resource, rga);

                totalCount ??= vm.TotalCount;

                return new GatewayModel.NonFungibleResourcesCollectionItem(ra, vm.NonFungibleIdsCount);
            })
            .ToList();

        var previousCursor = offset > 0
            ? new GatewayModel.EntityNonFungiblesCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > limit
            ? new GatewayModel.EntityNonFungiblesCursor(offset + limit).ToCursorString()
            : null;

        return new GatewayModel.NonFungibleResourcesCollection(totalCount, previousCursor, nextCursor, items.Take(limit).ToList());
    }
}
