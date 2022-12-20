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
using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
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
    private record MetadataViewModel(long EntityId, string[] Keys, string[] Values, int TotalCount);

    private record FungiblesViewModel(byte[] ResourceEntityGlobalAddress, string Balance, int TotalCount);

    private record NonFungiblesViewModel(byte[] ResourceEntityGlobalAddress, long NonFungibleIdsCount, int TotalCount);

    private record NonFungibleIdsViewModel(string NonFungibleId, int TotalCount);

    private record NonFungibleIdDataViewModel(string NonFungibleId, bool IsDeleted, byte[] ImmutableData, byte[] MutableData);

    private const int DefaultMetadataLimit = 100; // TODO make it configurable
    private const int DefaultResourceLimit = 20; // TODO make it configurable

    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ReadOnlyDbContext _dbContext;
    private readonly byte _ecdsaSecp256k1VirtualAccountAddressPrefix;
    private readonly byte _eddsaEd25519VirtualAccountAddressPrefix;

    public EntityStateQuerier(INetworkConfigurationProvider networkConfigurationProvider, ReadOnlyDbContext dbContext)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
        _dbContext = dbContext;

        _ecdsaSecp256k1VirtualAccountAddressPrefix = (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressSubtype.EcdsaSecp256k1VirtualAccountComponent).AddressBytePrefix;
        _eddsaEd25519VirtualAccountAddressPrefix = (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressSubtype.EddsaEd25519VirtualAccountComponent).AddressBytePrefix;
    }

    public async Task<GatewayModel.EntityResourcesResponse> EntityResourcesSnapshot(DecodedRadixAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(address, ledgerState, token);

        // TODO ideally we'd like to run those as either single query or separate ones but without await between them

        var fungibles = await GetFungiblesSlice(entity.Id, 0, DefaultResourceLimit, ledgerState, token);
        var nonFungibles = await GetNonFungiblesSlice(entity.Id, 0, DefaultResourceLimit, ledgerState, token);

        return new GatewayModel.EntityResourcesResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), fungibles, nonFungibles);
    }

    public async Task<GatewayModel.EntityDetailsResponse> EntityDetailsSnapshot(DecodedRadixAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity(address, ledgerState, token);

        GatewayModel.EntityDetailsResponseDetails details;

        switch (entity)
        {
            case FungibleResourceManagerEntity frme:
            {
                // TODO ideally we'd like to run those as either single query or separate ones but without await between them

                var supplyHistory = await _dbContext.ResourceManagerEntitySupplyHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.ResourceManagerEntityId == frme.Id)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                var accessRulesChain = await _dbContext.EntityAccessRulesLayersHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.EntityId == frme.Id && e.Subtype == AccessRulesChainSubtype.None)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                var vaultAccessRulesChain = await _dbContext.EntityAccessRulesLayersHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.EntityId == frme.Id && e.Subtype == AccessRulesChainSubtype.ResourceManagerVaultAccessRulesChain)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                details = new GatewayModel.EntityDetailsResponseDetails(new GatewayModel.EntityDetailsResponseFungibleResourceDetails(
                    discriminator: GatewayModel.EntityDetailsResponseDetailsType.FungibleResource,
                    accessRulesChain: new JRaw(accessRulesChain.AccessRulesChain),
                    vaultAccessRulesChain: new JRaw(vaultAccessRulesChain.AccessRulesChain),
                    divisibility: frme.Divisibility,
                    totalSupply: supplyHistory.TotalSupply.ToString(),
                    totalMinted: supplyHistory.TotalMinted.ToString(),
                    totalBurnt: supplyHistory.TotalBurnt.ToString()));

                break;
            }

            case NonFungibleResourceManagerEntity nfrme:
            {
                // TODO ideally we'd like to run those as either single query or separate ones but without await between them

                var accessRulesChain = await _dbContext.EntityAccessRulesLayersHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.EntityId == nfrme.Id && e.Subtype == AccessRulesChainSubtype.None)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                var vaultAccessRulesChain = await _dbContext.EntityAccessRulesLayersHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.EntityId == nfrme.Id && e.Subtype == AccessRulesChainSubtype.ResourceManagerVaultAccessRulesChain)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                details = new GatewayModel.EntityDetailsResponseDetails(new GatewayModel.EntityDetailsResponseNonFungibleResourceDetails(
                    discriminator: GatewayModel.EntityDetailsResponseDetailsType.NonFungibleResource,
                    accessRulesChain: new JRaw(accessRulesChain.AccessRulesChain),
                    vaultAccessRulesChain: new JRaw(vaultAccessRulesChain.AccessRulesChain),
                    nonFungibleIdType: nfrme.NonFungibleIdType.ToGatewayModel()));
                break;
            }

            case PackageEntity pe:
                details = new GatewayModel.EntityDetailsResponseDetails(new GatewayModel.EntityDetailsResponsePackageDetails(
                    discriminator: GatewayModel.EntityDetailsResponseDetailsType.Package,
                    codeHex: pe.Code.ToHex()));
                break;

            case VirtualAccountComponentEntity:
                // TODO - we should better fake the data - eg accessRulesChain when this is possible
                details = new GatewayModel.EntityDetailsResponseDetails(new GatewayModel.EntityDetailsResponseComponentDetails(
                    discriminator: GatewayModel.EntityDetailsResponseDetailsType.Component,
                    packageAddress: _networkConfigurationProvider.GetWellKnownAddresses().AccountPackage,
                    blueprintName: "Account",
                    state: new JObject(),
                    accessRulesChain: new JArray()
                ));
                break;

            case ComponentEntity ce:
                var package = await _dbContext.Entities
                    .FirstAsync(e => e.Id == ce.PackageId, token);

                var state = await _dbContext.ComponentEntityStateHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.ComponentEntityId == ce.Id)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                var accessRulesLayers = await _dbContext.EntityAccessRulesLayersHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.EntityId == ce.Id)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                details = new GatewayModel.EntityDetailsResponseDetails(new GatewayModel.EntityDetailsResponseComponentDetails(
                    discriminator: GatewayModel.EntityDetailsResponseDetailsType.Component,
                    packageAddress: package.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()),
                    blueprintName: ce.BlueprintName,
                    state: new JRaw(state.State),
                    accessRulesChain: new JRaw(accessRulesLayers.AccessRulesChain)));
                break;

            default:
                throw new InvalidEntityException(address.ToString());
        }

        var metadata = await GetMetadataSlice(entity.Id, 0, DefaultMetadataLimit, ledgerState, token);

        return new GatewayModel.EntityDetailsResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), metadata, details);
    }

    public async Task<GatewayModel.EntityOverviewResponse> EntityOverview(ICollection<DecodedRadixAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var addressesList = addresses.Select(x => x.Data).ToList();

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

    public async Task<GatewayModel.EntityMetadataResponse> EntityMetadata(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity(request.Address, ledgerState, token);
        var metadata = await GetMetadataSlice(entity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityMetadataResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), metadata);
    }

    public async Task<GatewayModel.EntityFungiblesResponse> EntityFungibles(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity(request.Address, ledgerState, token);
        var fungibles = await GetFungiblesSlice(entity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityFungiblesResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), fungibles);
    }

    public async Task<GatewayModel.EntityNonFungiblesResponse> EntityNonFungibles(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity(request.Address, ledgerState, token);
        var nonFungibles = await GetNonFungiblesSlice(entity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityNonFungiblesResponse(ledgerState, entity.BuildHrpGlobalAddress(_networkConfigurationProvider.GetHrpDefinition()), nonFungibles);
    }

    public async Task<GatewayModel.EntityNonFungibleIdsResponse> EntityNonFungibleIds(IEntityStateQuerier.PageRequest request, DecodedRadixAddress resourceAddress, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await GetEntity<NonFungibleResourceManagerEntity>(resourceAddress, ledgerState, token);
        var nonFungibleIds = await GetNonFungibleIdsSlice(entity.Id, resourceEntity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityNonFungibleIdsResponse(ledgerState, request.Address.ToString(), resourceAddress.ToString(), nonFungibleIds);
    }

    public async Task<GatewayModel.NonFungibleIdsResponse> NonFungibleIds(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<NonFungibleResourceManagerEntity>(request.Address, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
WITH store_history (nfids, total_count) AS (
    SELECT non_fungible_id_data_ids[@offset:@limit], array_length(non_fungible_id_data_ids, 1)
    FROM non_fungible_id_store_history
    WHERE non_fungible_resource_manager_entity_id = @entityId AND from_state_version < @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
),
non_fungible_data_ids (id) AS (
    SELECT UNNEST(nfids)
    FROM store_history
)
SELECT nfd.non_fungible_id AS NonFungibleId, store_history.total_count AS TotalCount
FROM non_fungible_id_data nfd, store_history
WHERE nfd.id IN(
    SELECT id FROM non_fungible_data_ids
)",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entity.Id,
                offset = request.Offset + 1,
                limit = request.Offset + request.Limit + 1,
            },
            cancellationToken: token);

        long? totalCount = 0;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdsViewModel>(cd)).ToList()
            .Select(vm =>
            {
                totalCount = vm.TotalCount;

                return new GatewayModel.NonFungibleIdsCollectionItem(vm.NonFungibleId);
            })
            .ToList();

        var previousCursor = request.Offset > 0
            ? new GatewayModel.EntityFungiblesCursor(Math.Max(request.Offset - request.Limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > request.Limit
            ? new GatewayModel.EntityFungiblesCursor(request.Offset + request.Limit).ToCursorString()
            : null;

        return new GatewayModel.NonFungibleIdsResponse(
            ledgerState: ledgerState,
            address: request.Address.ToString(),
            nonFungibleIds: new GatewayModel.NonFungibleIdsCollection(
                totalCount: totalCount,
                previousCursor: previousCursor,
                nextCursor: nextCursor,
                items: items.Take(request.Limit).ToList()));
    }

    public async Task<GatewayModel.NonFungibleDataResponse> NonFungibleIdData(DecodedRadixAddress address, string nonFungibleId, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<NonFungibleResourceManagerEntity>(address, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
SELECT nfid.non_fungible_id AS NonFungibleId, md.is_deleted AS IsDeleted, nfid.immutable_data AS ImmutableData, md.mutable_data AS MutableData
FROM non_fungible_id_data nfid
LEFT JOIN LATERAL (
    SELECT *
    FROM non_fungible_id_mutable_data_history nfidmdh
    WHERE nfidmdh.non_fungible_id_data_id = nfid.id AND nfidmdh.from_state_version <= @stateVersion
    ORDER BY nfidmdh.non_fungible_id_data_id DESC
    LIMIT 1
) md ON TRUE
WHERE nfid.from_state_version <= @stateVersion AND nfid.non_fungible_resource_manager_entity_id = @entityId AND nfid.non_fungible_id = @nonFungibleId
ORDER BY nfid.from_state_version DESC
LIMIT 1
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entity.Id,
                nonFungibleId = nonFungibleId,
            },
            cancellationToken: token);

        var data = await _dbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync<NonFungibleIdDataViewModel>(cd);

        if (data == null || data.IsDeleted)
        {
            throw new EntityNotFoundException(address.ToString()); // TODO change it to some "resource not found"?
        }

        return new GatewayModel.NonFungibleDataResponse(
            ledgerState: ledgerState,
            address: address.ToString(),
            nonFungibleIdType: entity.NonFungibleIdType.ToGatewayModel(),
            nonFungibleId: data.NonFungibleId,
            mutableDataHex: data.MutableData.ToHex(),
            immutableDataHex: data.ImmutableData.ToHex());
    }

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

        long? totalCount = 0;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<FungiblesViewModel>(cd)).ToList()
            .Select(vm =>
            {
                var rga = vm.ResourceEntityGlobalAddress ?? throw new InvalidOperationException("Non-global entity.");
                var ra = RadixAddressCodec.Encode(_networkConfigurationProvider.GetHrpDefinition().Resource, rga);

                totalCount = vm.TotalCount;

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

        long? totalCount = 0;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungiblesViewModel>(cd)).ToList()
            .Select(vm =>
            {
                var rga = vm.ResourceEntityGlobalAddress ?? throw new InvalidOperationException("Non-global entity.");
                var ra = RadixAddressCodec.Encode(_networkConfigurationProvider.GetHrpDefinition().Resource, rga);

                totalCount = vm.TotalCount;

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

    private async Task<GatewayModel.NonFungibleIdsCollection> GetNonFungibleIdsSlice(long entityId, long resourceEntityId, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
SELECT UNNEST(non_fungible_ids[@offset:@limit]) AS NonFungibleId, array_length(non_fungible_ids, 1) AS TotalCount
FROM entity_resource_history
WHERE id = (
    SELECT id
    FROM entity_resource_history
    WHERE from_state_version <= @stateVersion AND global_entity_id = @entityId AND resource_entity_id = @resourceEntityId
    ORDER BY from_state_version DESC
    LIMIT 1
)
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                resourceEntityId = resourceEntityId,
                offset = offset + 1,
                limit = offset + 1 + limit,
            },
            cancellationToken: token);

        long? totalCount = 0;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdsViewModel>(cd)).ToList()
            .Select(vm =>
            {
                totalCount = vm.TotalCount;

                return new GatewayModel.NonFungibleIdsCollectionItem(vm.NonFungibleId);
            })
            .ToList();

        var previousCursor = offset > 0
            ? new GatewayModel.EntityNonFungiblesCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > limit
            ? new GatewayModel.EntityNonFungiblesCursor(offset + limit).ToCursorString()
            : null;

        return new GatewayModel.NonFungibleIdsCollection(totalCount, previousCursor, nextCursor, items.Take(limit).ToList());
    }

    private async Task<Entity> GetEntity(DecodedRadixAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.GlobalAddress == address.Data, token);

        if (entity == null)
        {
            if (address.Data[0] == _ecdsaSecp256k1VirtualAccountAddressPrefix || address.Data[0] == _eddsaEd25519VirtualAccountAddressPrefix)
            {
                return new VirtualAccountComponentEntity(address.Data);
            }

            throw new EntityNotFoundException(address.ToString());
        }

        return entity;
    }

    private async Task<TEntity> GetEntity<TEntity>(DecodedRadixAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity
    {
        var entity = await GetEntity(address, ledgerState, token);

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidEntityException(address.ToString());
        }

        return typedEntity;
    }
}
