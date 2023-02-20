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
using RadixDlt.NetworkGateway.Abstractions;
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

    private record FungibleViewModel(GlobalAddress ResourceEntityGlobalAddress, string Balance, int ResourcesTotalCount);

    private record FungibleVaultViewModel(GlobalAddress ResourceEntityGlobalAddress, string VaultAddress, string Balance, int ResourceTotalCount, int VaultTotalCount);

    private record NonFungibleViewModel(GlobalAddress ResourceEntityGlobalAddress, long NonFungibleIdsCount, int ResourcesTotalCount);

    private record NonFungibleVaultViewModel(GlobalAddress ResourceEntityGlobalAddress, string VaultAddress, long NonFungibleIdsCount, int ResourceTotalCount, int VaultTotalCount);

    private record NonFungibleIdViewModel(string NonFungibleId, int NonFungibleIdsTotalCount);

    private record NonFungibleIdDataViewModel(string NonFungibleId, bool IsDeleted, byte[] ImmutableData, byte[] MutableData);

    private const int DefaultMetadataLimit = 100; // TODO make it configurable
    private const int DefaultResourceLimit = 20; // TODO make it configurable
    private const int ValidatorsLimit = 1000; // TODO make it configurable

    private readonly TokenAmount _tokenAmount100 = TokenAmount.FromDecimalString("100");
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ReadOnlyDbContext _dbContext;
    private readonly byte _ecdsaSecp256k1VirtualAccountAddressPrefix;
    private readonly byte _eddsaEd25519VirtualAccountAddressPrefix;
    private readonly byte _ecdsaSecp256k1VirtualIdentityAddressPrefix;
    private readonly byte _eddsaEd25519VirtualIdentityAddressPrefix;

    public EntityStateQuerier(INetworkConfigurationProvider networkConfigurationProvider, ReadOnlyDbContext dbContext)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
        _dbContext = dbContext;

        _ecdsaSecp256k1VirtualAccountAddressPrefix =
            (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressSubtype.EcdsaSecp256k1VirtualAccountComponent).AddressBytePrefix;
        _eddsaEd25519VirtualAccountAddressPrefix =
            (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressSubtype.EddsaEd25519VirtualAccountComponent).AddressBytePrefix;
        _ecdsaSecp256k1VirtualIdentityAddressPrefix =
            (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressSubtype.EcdsaSecp256k1VirtualIdentityComponent).AddressBytePrefix;
        _eddsaEd25519VirtualIdentityAddressPrefix =
            (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressSubtype.EddsaEd25519VirtualIdentityComponent).AddressBytePrefix;
    }

    public async Task<GatewayModel.EntityResourcesResponse> EntityResourcesSnapshot(GlobalAddress address, bool aggregatePerVault, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(address, ledgerState, token);

        // TODO ideally we'd like to run those as either single query or separate ones but without await between them

        var fungibles = aggregatePerVault
            ? await GetFungiblesSlicePerVault(entity.Id, DefaultResourceLimit, DefaultResourceLimit, ledgerState, token)
            : await GetFungiblesSlicePerResource(entity.Id, 0, DefaultResourceLimit, ledgerState, token);
        var nonFungibles = aggregatePerVault
            ? await GetNonFungiblesSlicePerVault(entity.Id, DefaultResourceLimit, DefaultResourceLimit, ledgerState, token)
            : await GetNonFungiblesSlicePerResource(entity.Id, 0, DefaultResourceLimit, ledgerState, token);

        return new GatewayModel.EntityResourcesResponse(ledgerState, entity.GlobalAddress, fungibles, nonFungibles);
    }

    public async Task<GatewayModel.StateEntityDetailsResponseItem> EntityDetailsItem(GlobalAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity(address, ledgerState, token);

        GatewayModel.StateEntityDetailsResponseItemDetails? details = null;

        switch (entity)
        {
            case FungibleResourceManagerEntity frme:
            {
                // TODO ideally we'd like to run those as either single query or separate ones but without await between them

                var accessRulesChain = await _dbContext.EntityAccessRulesLayersHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.EntityId == frme.Id && e.Subtype == AccessRulesChainSubtype.None)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                var vaultAccessRulesChain = await _dbContext.EntityAccessRulesLayersHistory
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.EntityId == frme.Id &&
                                e.Subtype == AccessRulesChainSubtype.ResourceManagerVaultAccessRulesChain)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                details = new GatewayModel.StateEntityDetailsResponseFungibleResourceDetails(
                    accessRulesChain: new JRaw(accessRulesChain.AccessRulesChain),
                    vaultAccessRulesChain: new JRaw(vaultAccessRulesChain.AccessRulesChain),
                    divisibility: frme.Divisibility);

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
                    .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.EntityId == nfrme.Id &&
                                e.Subtype == AccessRulesChainSubtype.ResourceManagerVaultAccessRulesChain)
                    .OrderByDescending(e => e.FromStateVersion)
                    .FirstAsync(token);

                details = new GatewayModel.StateEntityDetailsResponseNonFungibleResourceDetails(
                    accessRulesChain: new JRaw(accessRulesChain.AccessRulesChain),
                    vaultAccessRulesChain: new JRaw(vaultAccessRulesChain.AccessRulesChain),
                    nonFungibleIdType: nfrme.NonFungibleIdType.ToGatewayModel());
                break;
            }

            case PackageEntity pe:
                details = new GatewayModel.StateEntityDetailsResponsePackageDetails(
                    codeHex: pe.Code.ToHex());
                break;

            case VirtualAccountComponentEntity:
                // TODO - we should better fake the data - eg accessRulesChain when this is possible
                details = new GatewayModel.StateEntityDetailsResponseComponentDetails(
                    packageAddress: _networkConfigurationProvider.GetWellKnownAddresses().AccountPackage,
                    blueprintName: "Account",
                    state: new JObject(),
                    accessRulesChain: new JArray()
                );
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

                details = new GatewayModel.StateEntityDetailsResponseComponentDetails(
                    packageAddress: package.GlobalAddress,
                    blueprintName: ce.BlueprintName,
                    state: new JRaw(state.State),
                    accessRulesChain: new JRaw(accessRulesLayers.AccessRulesChain));
                break;
        }

        var metadata = await GetMetadataSlice(entity.Id, 0, DefaultMetadataLimit, ledgerState, token);

        return new GatewayModel.StateEntityDetailsResponseItem(entity.Address.ToHex(), entity.GlobalAddress, metadata, details);
    }

    public async Task<GatewayModel.EntityMetadataResponse> EntityMetadata(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity(request.Address, ledgerState, token);
        var metadata = await GetMetadataSlice(entity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityMetadataResponse(ledgerState, entity.GlobalAddress, metadata);
    }

    public async Task<GatewayModel.EntityFungiblesResponse> EntityFungibles(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity(request.Address, ledgerState, token);
        var fungibles = await GetFungiblesSlicePerResource(entity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityFungiblesResponse(ledgerState, entity.GlobalAddress, fungibles);
    }

    public async Task<GatewayModel.EntityNonFungiblesResponse> EntityNonFungibles(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity(request.Address, ledgerState, token);
        var nonFungibles = await GetNonFungiblesSlicePerResource(entity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityNonFungiblesResponse(ledgerState, entity.GlobalAddress, nonFungibles);
    }

    public async Task<GatewayModel.EntityNonFungibleIdsResponse> EntityNonFungibleIds(IEntityStateQuerier.PageRequest request, GlobalAddress resourceAddress,
        GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await GetEntity<NonFungibleResourceManagerEntity>(resourceAddress, ledgerState, token);
        var nonFungibleIds = await GetNonFungibleIdsSlice(entity.Id, resourceEntity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.EntityNonFungibleIdsResponse(ledgerState, request.Address.ToString(), resourceAddress.ToString(), nonFungibleIds);
    }

    public async Task<GatewayModel.NonFungibleIdsResponse> NonFungibleIds(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<NonFungibleResourceManagerEntity>(request.Address, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
WITH most_recent_non_fungible_id_store_history_slice (non_fungible_id_data_ids, non_fungible_ids_total_count) AS (
    SELECT non_fungible_id_data_ids[@offset:@limit], cardinality(non_fungible_id_data_ids)
    FROM non_fungible_id_store_history
    WHERE from_state_version <= @stateVersion AND non_fungible_resource_manager_entity_id = @entityId
    ORDER BY from_state_version DESC
    LIMIT 1
)
SELECT nfid.non_fungible_id AS NonFungibleId, hs.non_fungible_ids_total_count AS NonFungibleIdsTotalCount
FROM most_recent_non_fungible_id_store_history_slice hs
INNER JOIN non_fungible_id_data nfid ON nfid.id = ANY(hs.non_fungible_id_data_ids)
ORDER BY array_position(hs.non_fungible_id_data_ids, nfid.id);
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entity.Id,
                offset = request.Offset + 1,
                limit = request.Offset + request.Limit + 1,
            },
            cancellationToken: token);

        long? totalCount = 0;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdViewModel>(cd)).ToList()
            .Select(vm =>
            {
                totalCount = vm.NonFungibleIdsTotalCount;

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

    public async Task<GatewayModel.NonFungibleDataResponse> NonFungibleIdData(GlobalAddress address, string nonFungibleId, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
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
    ORDER BY nfidmdh.from_state_version DESC
    LIMIT 1
) md ON TRUE
WHERE nfid.from_state_version <= @stateVersion AND nfid.non_fungible_resource_manager_entity_id = @entityId AND nfid.non_fungible_id = @nonFungibleId
ORDER BY nfid.from_state_version DESC
LIMIT 1
",
            parameters: new { stateVersion = ledgerState.StateVersion, entityId = entity.Id, nonFungibleId = nonFungibleId, },
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

    public async Task<GatewayModel.StateValidatorsListResponse> StateValidatorsList(GatewayModel.StateValidatorsListCursor? cursor, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var fromStateVersion = cursor?.StateVersionBoundary ?? 0;

        var validatorsAndOneMore = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.GetType() == typeof(ValidatorEntity))
            .Where(e => e.FromStateVersion > fromStateVersion)
            .OrderBy(e => e.FromStateVersion)
            .Take(ValidatorsLimit + 1)
            .ToListAsync(token);

        var findEpochSubquery = _dbContext.ValidatorActiveSetHistory
            .AsQueryable()
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .OrderByDescending(e => e.FromStateVersion)
            .Take(1)
            .Select(e => e.Epoch);

        var activeSetById = await _dbContext.ValidatorActiveSetHistory
            .Include(e => e.PublicKey)
            .Where(e => e.Epoch == findEpochSubquery.First())
            .ToDictionaryAsync(e => e.PublicKey.ValidatorEntityId, token);

        var totalStake = activeSetById.Values
            .Select(asv => asv.Stake)
            .Aggregate(TokenAmount.Zero, (current, x) => current + x);

        var validatorIds = validatorsAndOneMore.Take(ValidatorsLimit).Select(e => e.Id).ToArray();

        // TODO Validators are currently not a derived type of ComponentEntity but this is only temporary solution, remove this comment once they regain their Component-status
        var stateById = await _dbContext.ComponentEntityStateHistory
            .FromSqlInterpolated($@"
WITH variables (validator_entity_id) AS (
    SELECT UNNEST({validatorIds})
)
SELECT cesh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM component_entity_state_history
    WHERE component_entity_id = variables.validator_entity_id AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) cesh ON true;
")
            .ToDictionaryAsync(e => e.ComponentEntityId, token);

        var metadataById = await GetMetadataSlices(validatorIds, 0, DefaultMetadataLimit, ledgerState, token);

        var items = validatorsAndOneMore
            .Take(ValidatorsLimit)
            .Select(v =>
            {
                GatewayModel.ValidatorCollectionItemActiveInEpoch? activeInEpoch = null;

                if (activeSetById.TryGetValue(v.Id, out var asv))
                {
                    var stake = new GatewayModel.TokenAmount(asv.Stake.ToString(), _networkConfigurationProvider.GetWellKnownAddresses().Xrd);
                    var stakePercentage = double.Parse((asv.Stake * _tokenAmount100 / totalStake).ToString());

                    activeInEpoch = new GatewayModel.ValidatorCollectionItemActiveInEpoch(
                        new GatewayModel.ValidatorCollectionItemActiveInEpochStake(stake, stakePercentage),
                        asv.PublicKey.ToGatewayPublicKey());
                }

                return new GatewayModel.ValidatorCollectionItem(v.GlobalAddress, new JRaw(stateById[v.Id].State), activeInEpoch, metadataById[v.Id]);
            })
            .ToList();

        var nextCursor = validatorsAndOneMore.Count == ValidatorsLimit + 1
            ? new GatewayModel.StateValidatorsListCursor(validatorsAndOneMore.Last().Id).ToCursorString()
            : null;

        return new GatewayModel.StateValidatorsListResponse(ledgerState, new GatewayModel.ValidatorCollection(null, null, nextCursor, items));
    }

    private async Task<GatewayModel.EntityMetadataCollection> GetMetadataSlice(long entityId, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var result = await GetMetadataSlices(new[] { entityId }, offset, limit, ledgerState, token);

        return result[entityId];
    }

    private async Task<Dictionary<long, GatewayModel.EntityMetadataCollection>> GetMetadataSlices(long[] entityIds, int offset, int limit, GatewayModel.LedgerState ledgerState,
        CancellationToken token)
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
    SELECT entity_id AS EntityId, keys[@offset:@limit] AS Keys, values[@offset:@limit] AS Values, cardinality(keys) AS TotalCount
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
                limit = offset + limit + 1,
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

            var nextCursor = items.Count > limit
                ? new GatewayModel.EntityMetadataRequestCursor(offset + limit).ToCursorString()
                : null;

            result[vm.EntityId] = new GatewayModel.EntityMetadataCollection(vm.TotalCount, previousCursor, nextCursor, items.Take(limit).ToList());
        }

        foreach (var missing in entityIds.Except(result.Keys))
        {
            result[missing] = GatewayModel.EntityMetadataCollection.Empty;
        }

        return result;
    }

    /// <summary>
    /// Returns a paginable resource collection with total (aggregated) balance per resource.
    /// </summary>
    private async Task<GatewayModel.FungibleResourcesCollection> GetFungiblesSlicePerResource(long entityId, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
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
    SELECT UNNEST(fungible_resource_entity_ids[@offset:@limit]) AS fungible_resource_entity_id, cardinality(fungible_resource_entity_ids) AS resources_total_count
    FROM most_recent_entity_resource_aggregate_history_nested
)
SELECT e.global_address AS ResourceEntityGlobalAddress, CAST(eravh.balance AS text) AS Balance, ah.resources_total_count AS ResourcesTotalCount
FROM most_recent_entity_resource_aggregate_history ah
INNER JOIN LATERAL (
    SELECT balance
    FROM entity_resource_aggregated_vaults_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId AND resource_entity_id = ah.fungible_resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) eravh ON TRUE
INNER JOIN entities e ON ah.fungible_resource_entity_id = e.id
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                offset = offset + 1,
                limit = offset + limit + 1,
            },
            cancellationToken: token);

        var totalCount = 0;

        var items = new List<GatewayModel.FungibleResourcesCollectionItem>();

        foreach (var vm in await _dbContext.Database.GetDbConnection().QueryAsync<FungibleViewModel>(cd))
        {
            totalCount = vm.ResourcesTotalCount;

            items.Add(new GatewayModel.FungibleResourcesCollectionItemGloballyAggregated(
                resourceAddress: vm.ResourceEntityGlobalAddress,
                amount: TokenAmount.FromSubUnitsString(vm.Balance).ToString()));
        }

        var previousCursor = offset > 0
            ? new GatewayModel.EntityFungiblesCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > limit
            ? new GatewayModel.EntityFungiblesCursor(offset + limit).ToCursorString()
            : null;

        return new GatewayModel.FungibleResourcesCollection(totalCount, previousCursor, nextCursor, items.Take(limit).ToList());
    }

    /// <summary>
    /// Returns a first page of paginable resource collection with nested paginable collection of vaults with their balances per resource for a given entity.
    /// Not suitable for "give me next page" requests.
    /// </summary>
    private async Task<GatewayModel.FungibleResourcesCollection> GetFungiblesSlicePerVault(long entityId, int resourceLimit, int vaultLimit, GatewayModel.LedgerState ledgerState, CancellationToken token)
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
    SELECT UNNEST(fungible_resource_entity_ids[1:@resourceLimit]) AS fungible_resource_entity_id, cardinality(fungible_resource_entity_ids) AS resource_total_count
    FROM most_recent_entity_resource_aggregate_history_nested
),
most_recent_entity_resource_vault_aggregate_history_nested AS (
    SELECT rah.fungible_resource_entity_id, rah.resource_total_count, vah.vault_entity_ids
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
    SELECT ahn.fungible_resource_entity_id, ahn.resource_total_count, UNNEST(vault_entity_ids[1:@vaultLimit]) AS vault_entity_id, cardinality(vault_entity_ids) AS vault_total_count
    FROM most_recent_entity_resource_vault_aggregate_history_nested ahn
)
SELECT er.global_address AS ResourceEntityGlobalAddress, ENCODE(ev.address, 'hex') AS VaultAddress, CAST(vh.balance AS text) AS Balance, vah.resource_total_count AS ResourceTotalCount, vah.vault_total_count AS VaultTotalCount
FROM most_recent_entity_resource_vault_aggregate_history vah
INNER JOIN LATERAL (
    SELECT balance
    FROM entity_vault_history
    WHERE from_state_version <= @stateVersion AND global_entity_id = @entityId AND resource_entity_id = vah.fungible_resource_entity_id AND vault_entity_id = vah.vault_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) vh ON TRUE
INNER JOIN entities er ON vah.fungible_resource_entity_id = er.id
INNER JOIN entities ev ON vah.vault_entity_id = ev.id;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                resourceLimit = resourceLimit,
                vaultLimit = vaultLimit,
            },
            cancellationToken: token);

        var resourcesTotalCount = 0;

        var resources = new Dictionary<GlobalAddress, GatewayModel.FungibleResourcesCollectionItemVaultAggregated>();

        foreach (var vm in await _dbContext.Database.GetDbConnection().QueryAsync<FungibleVaultViewModel>(cd))
        {
            resourcesTotalCount = vm.ResourceTotalCount;

            if (!resources.TryGetValue(vm.ResourceEntityGlobalAddress, out var existingRecord))
            {
                var vaultNextCursor = vm.VaultTotalCount > vaultLimit
                    ? new GatewayModel.EntityFungiblesCursor(vaultLimit).ToCursorString()
                    : null;

                existingRecord = new GatewayModel.FungibleResourcesCollectionItemVaultAggregated(
                    resourceAddress: vm.ResourceEntityGlobalAddress,
                    vaults: new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVault(
                        totalCount: vm.VaultTotalCount,
                        nextCursor: vaultNextCursor,
                        items: new List<GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem>()));

                resources[vm.ResourceEntityGlobalAddress] = existingRecord;
            }

            existingRecord.Vaults.Items.Add(new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                vaultAddress: vm.VaultAddress,
                amount: TokenAmount.FromSubUnitsString(vm.Balance).ToString()));
        }

        var nextCursor = resourcesTotalCount > resourceLimit
            ? new GatewayModel.EntityFungiblesCursor(resourceLimit).ToCursorString()
            : null;

        return new GatewayModel.FungibleResourcesCollection(resourcesTotalCount, null, nextCursor, resources.Values.Cast<GatewayModel.FungibleResourcesCollectionItem>().ToList());
    }

    /// <summary>
    /// Returns a paginable resource collection with total (aggregated) count per resource.
    /// </summary>
    private async Task<GatewayModel.NonFungibleResourcesCollection> GetNonFungiblesSlicePerResource(long entityId, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
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
    SELECT UNNEST(non_fungible_resource_entity_ids[@offset:@limit]) AS non_fungible_resource_entity_id, cardinality(non_fungible_resource_entity_ids) AS resources_total_count
    FROM most_recent_entity_resource_aggregate_history_nested
)
SELECT e.global_address AS ResourceEntityGlobalAddress, eravh.total_count AS NonFungibleIdsCount, ah.resources_total_count AS ResourcesTotalCount
FROM most_recent_entity_resource_aggregate_history ah
INNER JOIN LATERAL (
    SELECT total_count
    FROM entity_resource_aggregated_vaults_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId AND resource_entity_id = ah.non_fungible_resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) eravh ON TRUE
INNER JOIN entities e ON ah.non_fungible_resource_entity_id = e.id;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                offset = offset + 1,
                limit = offset + limit + 1,
            },
            cancellationToken: token);

        var totalCount = 0;

        var items = new List<GatewayModel.NonFungibleResourcesCollectionItem>();

        foreach (var vm in await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleViewModel>(cd))
        {
            totalCount = vm.ResourcesTotalCount;

            items.Add(new GatewayModel.NonFungibleResourcesCollectionItemGloballyAggregated(
                resourceAddress: vm.ResourceEntityGlobalAddress,
                amount: vm.NonFungibleIdsCount));
        }

        var previousCursor = offset > 0
            ? new GatewayModel.EntityNonFungiblesCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > limit
            ? new GatewayModel.EntityNonFungiblesCursor(offset + limit).ToCursorString()
            : null;

        return new GatewayModel.NonFungibleResourcesCollection(totalCount, previousCursor, nextCursor, items.Take(limit).ToList());
    }

    /// <summary>
    /// Returns a first page of paginable resource collection with nested paginable collection of vaults with their total count per resource for a given entity.
    /// Not suitable for "give me next page" requests.
    /// </summary>
    private async Task<GatewayModel.NonFungibleResourcesCollection> GetNonFungiblesSlicePerVault(long entityId, int resourceLimit, int vaultLimit, GatewayModel.LedgerState ledgerState, CancellationToken token)
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
    SELECT UNNEST(non_fungible_resource_entity_ids[1:@resourceLimit]) AS non_fungible_resource_entity_id, cardinality(non_fungible_resource_entity_ids) AS resource_total_count
    FROM most_recent_entity_resource_aggregate_history_nested
),
most_recent_entity_resource_vault_aggregate_history_nested AS (
    SELECT rah.non_fungible_resource_entity_id, rah.resource_total_count, vah.vault_entity_ids
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
    SELECT ahn.non_fungible_resource_entity_id, ahn.resource_total_count, UNNEST(vault_entity_ids[1:@vaultLimit]) AS vault_entity_id, cardinality(vault_entity_ids) AS vault_total_count
    FROM most_recent_entity_resource_vault_aggregate_history_nested ahn
)
SELECT er.global_address AS ResourceEntityGlobalAddress, ENCODE(ev.address, 'hex') AS VaultAddress, vh.NonFungibleIdsCount, vah.resource_total_count AS ResourceTotalCount, vah.vault_total_count AS VaultTotalCount
FROM most_recent_entity_resource_vault_aggregate_history vah
INNER JOIN LATERAL (
    SELECT CAST(cardinality(non_fungible_ids) AS bigint) AS NonFungibleIdsCount
    FROM entity_vault_history
    WHERE from_state_version <= @stateVersion AND global_entity_id = @entityId AND resource_entity_id = vah.non_fungible_resource_entity_id AND vault_entity_id = vah.vault_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) vh ON TRUE
INNER JOIN entities er ON vah.non_fungible_resource_entity_id = er.id
INNER JOIN entities ev ON vah.vault_entity_id = ev.id;
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                resourceLimit = resourceLimit,
                vaultLimit = vaultLimit,
            },
            cancellationToken: token);

        var resourcesTotalCount = 0;

        var resources = new Dictionary<GlobalAddress, GatewayModel.NonFungibleResourcesCollectionItemVaultAggregated>();

        foreach (var vm in await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleVaultViewModel>(cd))
        {
            resourcesTotalCount = vm.ResourceTotalCount;

            if (!resources.TryGetValue(vm.ResourceEntityGlobalAddress, out var existingRecord))
            {
                var vaultNextCursor = vm.VaultTotalCount > vaultLimit
                    ? new GatewayModel.EntityNonFungiblesCursor(vaultLimit).ToCursorString()
                    : null;

                existingRecord = new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregated(
                    resourceAddress: vm.ResourceEntityGlobalAddress,
                    vaults: new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVault(
                        totalCount: vm.VaultTotalCount,
                        nextCursor: vaultNextCursor,
                        items: new List<GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem>()));

                resources[vm.ResourceEntityGlobalAddress] = existingRecord;
            }

            existingRecord.Vaults.Items.Add(new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                vaultAddress: vm.VaultAddress,
                totalCount: vm.NonFungibleIdsCount));
        }

        var nextCursor = resourcesTotalCount > resourceLimit
            ? new GatewayModel.EntityFungiblesCursor(resourceLimit).ToCursorString()
            : null;

        return new GatewayModel.NonFungibleResourcesCollection(resourcesTotalCount, null, nextCursor, resources.Values.Cast<GatewayModel.NonFungibleResourcesCollectionItem>().ToList());
    }

    private async Task<GatewayModel.NonFungibleIdsCollection> GetNonFungibleIdsSlice(long entityId, long resourceEntityId, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        // TODO NG-283 this query does not account for multiple Vaults that could store NFIDs

        var cd = new CommandDefinition(
            commandText: @"
SELECT nfid.non_fungible_id AS NonFungibleId, final.total_count AS TotalCount
FROM (
    SELECT UNNEST(non_fungible_ids[@offset:@limit]) AS non_fungible_id_data_id, cardinality(non_fungible_ids) AS total_count
    FROM entity_vault_history
    WHERE id = (
        SELECT id
        FROM entity_vault_history
        WHERE from_state_version <= @stateVersion AND global_entity_id = @entityId AND resource_entity_id = @resourceEntityId
        ORDER BY from_state_version DESC
        LIMIT 1
    )
) final
INNER JOIN non_fungible_id_data nfid ON nfid.id = final.non_fungible_id_data_id
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entityId,
                resourceEntityId = resourceEntityId,
                offset = offset + 1,
                limit = offset + limit + 1,
            },
            cancellationToken: token);

        var totalCount = 0;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdViewModel>(cd)).ToList()
            .Select(vm =>
            {
                totalCount = vm.NonFungibleIdsTotalCount;

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

    private async Task<Entity> GetEntity(GlobalAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.GlobalAddress == address, token);

        if (entity != null)
        {
            return entity;
        }

        // TODO this method should return null/throw on missing, virtual component handling should be done upstream to avoid entity.Id = 0 uses, see https://github.com/radixdlt/babylon-gateway/pull/171#discussion_r1111957627

        var firstAddressByte = RadixAddressCodec.Decode(address).Data[0];

        if (firstAddressByte == _ecdsaSecp256k1VirtualAccountAddressPrefix || firstAddressByte == _eddsaEd25519VirtualAccountAddressPrefix)
        {
            return new VirtualAccountComponentEntity(address);
        }

        if (firstAddressByte == _ecdsaSecp256k1VirtualIdentityAddressPrefix || firstAddressByte == _eddsaEd25519VirtualIdentityAddressPrefix)
        {
            return new VirtualIdentityEntity(address);
        }

        throw new EntityNotFoundException(address.ToString());
    }

    private async Task<TEntity> GetEntity<TEntity>(GlobalAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
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
