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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using ToolkitModel = RadixEngineToolkit;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class EntityStateQuerier : IEntityStateQuerier
{
    private record MetadataViewModel(long FromStateVersion, long EntityId, string Key, byte[] Value, bool IsLocked, int TotalCount);

    private record ValidatorCurrentStakeViewModel(long ValidatorId, string Balance, string State, long BalanceLastUpdatedAtStateVersion, long StateLastUpdatedAtStateVersion);

    private record FungibleViewModel(EntityAddress ResourceEntityAddress, string Balance, int ResourcesTotalCount, long LastUpdatedAtStateVersion);

    private record RoyaltyVaultBalanceViewModel(long RoyaltyVaultEntityId, string Balance, long OwnerEntityId, long LastUpdatedAtStateVersion);

    private record FungibleResourceVaultsViewModel(EntityAddress ResourceEntityAddress, EntityAddress VaultAddress, string Balance, int VaultTotalCount, long LastUpdatedAtStateVersion);

    private record FungibleAggregatedPerVaultViewModel(EntityAddress ResourceEntityAddress, EntityAddress VaultAddress, string Balance, int ResourceTotalCount, int VaultTotalCount, long LastUpdatedAtStateVersion);

    private record NonFungibleViewModel(EntityAddress ResourceEntityAddress, long NonFungibleIdsCount, int ResourcesTotalCount, long LastUpdatedAtStateVersion);

    private record NonFungibleResourceVaultsViewModel(EntityAddress ResourceEntityAddress, long VaultEntityId, EntityAddress VaultAddress, long NonFungibleIdsCount, int VaultTotalCount, long LastUpdatedAtStateVersion);

    private record NonFungibleAggregatedPerVaultViewModel(long ResourceEntityId, long VaultEntityId, EntityAddress ResourceEntityAddress, EntityAddress VaultAddress, long NonFungibleIdsCount, int ResourceTotalCount, int VaultTotalCount, long LastUpdatedAtStateVersion);

    private record NonFungibleIdViewModel(string NonFungibleId, int NonFungibleIdsTotalCount);

    private record NonFungibleIdWithOwnerDataViewModel(string NonFungibleId, long EntityId, long ResourceEntityId, long VaultEntityId);

    private record NonFungibleIdDataViewModel(string NonFungibleId, bool IsDeleted, byte[] Data, long DataLastUpdatedAtStateVersion);

    private record struct NonFungibleIdOwnerLookup(long EntityId, long ResourceEntityId, long VaultEntityId);

    private record struct ExplicitMetadataLookup(long EntityId, string MetadataKey);

    private readonly TokenAmount _tokenAmount100 = TokenAmount.FromDecimalString("100");
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IOptionsSnapshot<EndpointOptions> _endpointConfiguration;
    private readonly ReadOnlyDbContext _dbContext;
    private readonly IVirtualEntityMetadataProvider _virtualEntityMetadataProvider;
    private readonly ILogger<EntityStateQuerier> _logger;
    private readonly byte _ecdsaSecp256k1VirtualAccountAddressPrefix;
    private readonly byte _eddsaEd25519VirtualAccountAddressPrefix;
    private readonly byte _ecdsaSecp256k1VirtualIdentityAddressPrefix;
    private readonly byte _eddsaEd25519VirtualIdentityAddressPrefix;

    public EntityStateQuerier(INetworkConfigurationProvider networkConfigurationProvider, ReadOnlyDbContext dbContext,
        IOptionsSnapshot<EndpointOptions> endpointConfiguration, ILogger<EntityStateQuerier> logger, IVirtualEntityMetadataProvider virtualEntityMetadataProvider)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
        _dbContext = dbContext;
        _endpointConfiguration = endpointConfiguration;
        _logger = logger;
        _virtualEntityMetadataProvider = virtualEntityMetadataProvider;

        _ecdsaSecp256k1VirtualAccountAddressPrefix = (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressEntityType.GlobalVirtualSecp256k1Account).AddressBytePrefix;
        _eddsaEd25519VirtualAccountAddressPrefix = (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressEntityType.GlobalVirtualEd25519Account).AddressBytePrefix;
        _ecdsaSecp256k1VirtualIdentityAddressPrefix = (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressEntityType.GlobalVirtualSecp256k1Identity).AddressBytePrefix;
        _eddsaEd25519VirtualIdentityAddressPrefix = (byte)_networkConfigurationProvider.GetAddressTypeDefinition(AddressEntityType.GlobalVirtualEd25519Identity).AddressBytePrefix;
    }

    public async Task<GatewayModel.StateEntityDetailsResponse> EntityDetails(List<EntityAddress> addresses, bool aggregatePerVault, GatewayModel.StateEntityDetailsOptIns optIns, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entities = await GetEntities(addresses, ledgerState, token);
        var componentEntities = entities.OfType<ComponentEntity>().ToList();
        var resourceEntities = entities.OfType<ResourceEntity>().ToList();
        var packageEntities = entities.OfType<GlobalPackageEntity>().ToList();

        // TODO ideally we'd like to run those in parallel
        var metadata = await GetMetadataSlices(entities.Select(e => e.Id).ToArray(), 0, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token);
        var accessRulesHistory = await GetAccessRulesHistory(resourceEntities, componentEntities, ledgerState, token);
        var stateHistory = await GetStateHistory(componentEntities, ledgerState, token);
        var correlatedAddresses = await GetCorrelatedEntityAddresses(entities, componentEntities, ledgerState, token);
        var resourcesSupplyData = await GetResourcesSupplyData(resourceEntities.Select(x => x.Id).ToArray(), ledgerState, token);
        var packageBlueprints = await GetPackageBlueprints(packageEntities.Select(e => e.Id).ToArray(), ledgerState, token);

        var royaltyVaultsBalance = componentEntities.Any() && (optIns.ComponentRoyaltyVaultBalance || optIns.PackageRoyaltyVaultBalance)
            ? await RoyaltyVaultBalance(componentEntities.Select(x => x.Id).ToArray(), ledgerState, token)
            : null;

        var fungibleResources = await EntityFungibleResourcesPageSlice(componentEntities.Select(e => e.Id).ToArray(), aggregatePerVault, 0, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token);
        var nonFungibleResources = await EntityNonFungibleResourcesPageSlice(componentEntities.Select(e => e.Id).ToArray(), aggregatePerVault, optIns.NonFungibleIncludeNfids, 0, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token);
        var resourceAddressToEntityId = await ResolveResourceEntityIds(fungibleResources.Values, nonFungibleResources.Values, token);

        var explicitMetadata = optIns.ExplicitMetadata?.Any() == true
            ? await GetExplicitMetadata(entities.Select(e => e.Id).Concat(resourceAddressToEntityId.Values).ToArray(), optIns.ExplicitMetadata.ToArray(), ledgerState, token)
            : null;

        var items = new List<GatewayModel.StateEntityDetailsResponseItem>();

        foreach (var entity in entities)
        {
            GatewayModel.StateEntityDetailsResponseItemDetails? details = null;

            switch (entity)
            {
                case GlobalFungibleResourceEntity frme:
                    var fungibleResourceSupplyData = resourcesSupplyData[frme.Id];
                    details = new GatewayModel.StateEntityDetailsResponseFungibleResourceDetails(
                        totalSupply: fungibleResourceSupplyData.TotalSupply.ToString(),
                        totalMinted: fungibleResourceSupplyData.TotalMinted.ToString(),
                        totalBurned: fungibleResourceSupplyData.TotalBurned.ToString(),
                        accessRulesChain: new JRaw(accessRulesHistory[frme.Id]),
                        vaultAccessRulesChain: new JRaw("{}"), // TODO restore?
                        divisibility: frme.Divisibility);

                    break;

                case GlobalNonFungibleResourceEntity nfrme:
                    var nonFungibleResourceSupplyData = resourcesSupplyData[nfrme.Id];
                    if (nonFungibleResourceSupplyData == null)
                    {
                        throw new ArgumentException($"Resource supply data for fungible resource with database id:{nfrme.Id} not found.");
                    }

                    details = new GatewayModel.StateEntityDetailsResponseNonFungibleResourceDetails(
                        totalSupply: nonFungibleResourceSupplyData.TotalSupply.ToString(),
                        totalMinted: nonFungibleResourceSupplyData.TotalMinted.ToString(),
                        totalBurned: nonFungibleResourceSupplyData.TotalBurned.ToString(),
                        accessRulesChain: new JRaw(accessRulesHistory[nfrme.Id]),
                        vaultAccessRulesChain: new JRaw("{}"), // TODO restore?
                        nonFungibleIdType: nfrme.NonFungibleIdType.ToGatewayModel());
                    break;

                case GlobalPackageEntity pe:
                    var packageRoyaltyVaultBalance = royaltyVaultsBalance?.SingleOrDefault(x => x.OwnerEntityId == pe.Id)?.Balance;

                    details = new GatewayModel.StateEntityDetailsResponsePackageDetails(
                        codeHex: pe.Code.ToHex(),
                        royaltyVaultBalance: packageRoyaltyVaultBalance != null ? TokenAmount.FromSubUnitsString(packageRoyaltyVaultBalance).ToString() : null
                        );
                    break;

                case VirtualIdentityEntity:
                    var virtualIdentityMetadata = _virtualEntityMetadataProvider.GetVirtualEntityMetadata(entity.Address);
                    metadata[entity.Id] = virtualIdentityMetadata;

                    // TODO - we should better fake the data - eg accessRulesChain when this is possible
                    details = new GatewayModel.StateEntityDetailsResponseComponentDetails(
                        blueprintName: "Account",
                        state: new JObject(),
                        accessRulesChain: new JArray()
                    );
                    break;

                case VirtualAccountComponentEntity:
                    var virtualAccountMetadata = _virtualEntityMetadataProvider.GetVirtualEntityMetadata(entity.Address);
                    metadata[entity.Id] = virtualAccountMetadata;

                    // TODO - we should better fake the data - eg accessRulesChain when this is possible
                    details = new GatewayModel.StateEntityDetailsResponseComponentDetails(
                        blueprintName: "Account",
                        state: new JObject(),
                        accessRulesChain: new JArray()
                    );
                    break;

                case ComponentEntity ce:
                    stateHistory.TryGetValue(ce.Id, out var state);
                    accessRulesHistory.TryGetValue(ce.Id, out var accessRules);

                    var componentRoyaltyVaultBalance = royaltyVaultsBalance?.SingleOrDefault(x => x.OwnerEntityId == ce.Id)?.Balance;

                    details = new GatewayModel.StateEntityDetailsResponseComponentDetails(
                        packageAddress: correlatedAddresses[ce.PackageId],
                        blueprintName: ce.BlueprintName,
                        state: state != null ? new JRaw(state.State) : null,
                        accessRulesChain: accessRules != null ? new JRaw(accessRules) : null,
                        royaltyVaultBalance: componentRoyaltyVaultBalance != null ? TokenAmount.FromSubUnitsString(componentRoyaltyVaultBalance).ToString() : null
                    );
                    break;
            }

            var ancestorIdentities = optIns.AncestorIdentities && entity.HasParent
                ? new GatewayModel.StateEntityDetailsResponseItemAncestorIdentities(
                    parentAddress: correlatedAddresses[entity.ParentAncestorId.Value],
                    ownerAddress: correlatedAddresses[entity.OwnerAncestorId.Value],
                    globalAddress: correlatedAddresses[entity.GlobalAncestorId.Value])
                : null;

            var haveFungibles = fungibleResources.TryGetValue(entity.Id, out var fungibles);
            var haveNonFungibles = nonFungibleResources.TryGetValue(entity.Id, out var nonFungibles);

            if (explicitMetadata != null)
            {
                if (haveFungibles && fungibles != null)
                {
                    fungibles.Items.ForEach(c => c.ExplicitMetadata = explicitMetadata[resourceAddressToEntityId[(EntityAddress)c.ResourceAddress]]);
                }

                if (haveNonFungibles && nonFungibles != null)
                {
                    nonFungibles.Items.ForEach(c => c.ExplicitMetadata = explicitMetadata[resourceAddressToEntityId[(EntityAddress)c.ResourceAddress]]);
                }
            }

            items.Add(new GatewayModel.StateEntityDetailsResponseItem(
                address: entity.Address,
                fungibleResources: haveFungibles ? fungibles : null,
                nonFungibleResources: haveNonFungibles ? nonFungibles : null,
                ancestorIdentities: ancestorIdentities,
                metadata: metadata[entity.Id],
                explicitMetadata: explicitMetadata?[entity.Id],
                details: details));
        }

        return new GatewayModel.StateEntityDetailsResponse(ledgerState, items);
    }

    public async Task<Dictionary<long, GatewayModel.FungibleResourcesCollection>> EntityFungibleResourcesPageSlice(long[] entityIds, bool aggregatePerVault, int offset, int limit, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var result = new Dictionary<long, GatewayModel.FungibleResourcesCollection>();

        foreach (var entityId in entityIds)
        {
            result[entityId] = await EntityFungibleResourcesPageSlice(entityId, aggregatePerVault, offset, limit, ledgerState, token);
        }

        return result;
    }

    public async Task<Dictionary<long, GatewayModel.NonFungibleResourcesCollection>> EntityNonFungibleResourcesPageSlice(long[] entityIds, bool aggregatePerVault, bool includeNfids, int offset, int limit, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        return aggregatePerVault
            ? await NonFungiblesAggregatedPerVaultPage(entityIds, includeNfids, offset, limit, ledgerState, token)
            : await NonFungiblesAggregatedPerResourcePage(entityIds, offset, ledgerState, token);
    }

    public async Task<Dictionary<long, GatewayModel.NonFungibleResourcesCollection>> NonFungiblesAggregatedPerResourcePage(long[] entityIds, int offset, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var result = new Dictionary<long, GatewayModel.NonFungibleResourcesCollection>();

        foreach (var entityId in entityIds)
        {
            result[entityId] = await GetNonFungiblesSliceAggregatedPerResource(entityId, offset, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token);
        }

        return result;
    }

    public async Task<Dictionary<long, GatewayModel.NonFungibleResourcesCollection>> NonFungiblesAggregatedPerVaultPage(
        long[] entityIds, bool includeNfids, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var nonFungiblesSliceAggregatedPerVault = new Dictionary<long, List<NonFungibleAggregatedPerVaultViewModel>>();
        var nonFungibleIdOwnerLookup = new List<NonFungibleIdOwnerLookup>();

        foreach (var entityId in entityIds)
        {
            var entityResult = await GetNonFungiblesSliceAggregatedPerVault(entityId, offset, limit, 0, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token);
            nonFungiblesSliceAggregatedPerVault[entityId] = entityResult;
            nonFungibleIdOwnerLookup.AddRange(
                entityResult.Select(row => new NonFungibleIdOwnerLookup(entityId, row.ResourceEntityId, row.VaultEntityId)).ToArray()
                );
        }

        if (!includeNfids || !nonFungibleIdOwnerLookup.Any())
        {
            return nonFungiblesSliceAggregatedPerVault.ToDictionary(
                x => x.Key,
                x => MapToNonFungibleResourcesCollection(x.Value, null, offset, limit, offset, limit)
            );
        }

        var nonFungibleIds = await GetNonFungibleIdsFirstPage(
            nonFungibleIdOwnerLookup.Select(x => x.EntityId).ToArray(),
            nonFungibleIdOwnerLookup.Select(x => x.ResourceEntityId).ToArray(),
            nonFungibleIdOwnerLookup.Select(x => x.VaultEntityId).ToArray(),
            _endpointConfiguration.Value.DefaultPageSize,
            ledgerState,
            token);

        var nonFungibleIdsFirstPage = nonFungibleIds
            .GroupBy(x => x.EntityId)
            .ToDictionary(x => x.Key, x => x.ToList());

        return nonFungiblesSliceAggregatedPerVault.ToDictionary(
            x => x.Key,
            x => MapToNonFungibleResourcesCollection(x.Value, nonFungibleIdsFirstPage[x.Key], offset, limit, offset, limit)
        );
    }

    public async Task<GatewayModel.StateEntityNonFungiblesPageResponse> EntityNonFungibleResourcesPage(IEntityStateQuerier.PageRequest pageRequest, bool aggregatePerVault, GatewayModel.StateEntityNonFungiblesPageRequestOptIns optIns, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(pageRequest.Address, ledgerState, token);
        var result = (await EntityNonFungibleResourcesPageSlice(
            new[] { entity.Id }, aggregatePerVault, optIns.NonFungibleIncludeNfids, pageRequest.Offset, pageRequest.Limit, ledgerState, token))[entity.Id];

        if (optIns.ExplicitMetadata?.Any() == true)
        {
            var resourceAddressToEntityId = await ResolveResourceEntityIds(null, new[] { result }, token);
            var explicitMetadata = await GetExplicitMetadata(resourceAddressToEntityId.Values.ToArray(), optIns.ExplicitMetadata.ToArray(), ledgerState, token);

            result.Items.ForEach(nfr => nfr.ExplicitMetadata = explicitMetadata[resourceAddressToEntityId[(EntityAddress)nfr.ResourceAddress]]);
        }

        return new GatewayModel.StateEntityNonFungiblesPageResponse(ledgerState, result.TotalCount, result.PreviousCursor, result.NextCursor, result.Items, pageRequest.Address);
    }

    public async Task<GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse> EntityNonFungibleResourceVaults(
        IEntityStateQuerier.ResourceVaultsPageRequest request, GatewayModel.StateEntityNonFungibleResourceVaultsPageOptIns optIns,
        GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await GetEntity<GlobalNonFungibleResourceEntity>(request.ResourceAddress, ledgerState, token);
        var nonFungibles = await GetNonFungibleResourceVaults(entity.Id, resourceEntity.Id, request.Offset, request.Limit, ledgerState, token);
        var vaultEntityIdsToQuery = nonFungibles.Select(x => x.VaultEntityId).ToArray();

        if (!optIns.NonFungibleIncludeNfids || !vaultEntityIdsToQuery.Any())
        {
            return MapToStateEntityNonFungibleResourceVaultsPageResponse(nonFungibles, null, ledgerState, request.Offset, request.Limit, entity.Address, resourceEntity.Address);
        }

        var nonFungibleIds = await GetNonFungibleIdsFirstPage(
            new[] { entity.Id },
            new[] { resourceEntity.Id },
            vaultEntityIdsToQuery,
            _endpointConfiguration.Value.DefaultPageSize,
            ledgerState,
            token);

        var nonFungibleIdsPerVault = nonFungibleIds
            .GroupBy(x => x.VaultEntityId)
            .ToDictionary(
                x => x.Key,
                x => x.ToList());

        return MapToStateEntityNonFungibleResourceVaultsPageResponse(nonFungibles, nonFungibleIdsPerVault, ledgerState, request.Offset, request.Limit, entity.Address, resourceEntity.Address);
    }

    public async Task<GatewayModel.StateEntityFungiblesPageResponse> EntityFungibleResourcesPage(IEntityStateQuerier.PageRequest pageRequest, bool aggregatePerVault, GatewayModel.StateEntityFungiblesPageRequestOptIns optIns, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(pageRequest.Address, ledgerState, token);
        var result = await EntityFungibleResourcesPageSlice(entity.Id, aggregatePerVault, pageRequest.Offset, pageRequest.Limit, ledgerState, token);

        if (optIns.ExplicitMetadata?.Any() == true)
        {
            var resourceAddressToEntityId = await ResolveResourceEntityIds(new[] { result }, null, token);
            var explicitMetadata = await GetExplicitMetadata(resourceAddressToEntityId.Values.ToArray(), optIns.ExplicitMetadata.ToArray(), ledgerState, token);

            result.Items.ForEach(fr => fr.ExplicitMetadata = explicitMetadata[resourceAddressToEntityId[(EntityAddress)fr.ResourceAddress]]);
        }

        return new GatewayModel.StateEntityFungiblesPageResponse(ledgerState, result.TotalCount, result.PreviousCursor, result.NextCursor, result.Items, pageRequest.Address);
    }

    public async Task<GatewayModel.FungibleResourcesCollection> EntityFungibleResourcesPageSlice(long entityId, bool aggregatePerVault, int offset, int limit, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var fungibles = aggregatePerVault
            ? await GetFungiblesSliceAggregatedPerVault(entityId, offset, limit, 0, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token)
            : await GetFungiblesSliceAggregatedPerResource(entityId, offset, limit, ledgerState, token);

        return fungibles;
    }

    public async Task<GatewayModel.StateEntityMetadataPageResponse> EntityMetadata(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<Entity>(request.Address, ledgerState, token);
        var metadata = (await GetMetadataSlices(new[] { entity.Id }, request.Offset, request.Limit, ledgerState, token))[entity.Id];

        return new GatewayModel.StateEntityMetadataPageResponse(
            ledgerState, metadata.TotalCount, metadata.PreviousCursor,
            metadata.NextCursor, metadata.Items, entity.Address);
    }

    public async Task<GatewayModel.StateEntityFungibleResourceVaultsPageResponse> EntityFungibleResourceVaults(IEntityStateQuerier.ResourceVaultsPageRequest request, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await GetEntity<GlobalFungibleResourceEntity>(request.ResourceAddress, ledgerState, token);
        var fungibles = await GetFungibleResourceVaults(entity.Id, resourceEntity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.StateEntityFungibleResourceVaultsPageResponse(
            ledgerState, fungibles.TotalCount, fungibles.PreviousCursor, fungibles.NextCursor, fungibles.Items, entity.Address, resourceEntity.Address);
    }

    public async Task<GatewayModel.StateEntityNonFungibleIdsPageResponse> EntityNonFungibleIds(IEntityStateQuerier.PageRequest request, EntityAddress resourceAddress,
        EntityAddress vaultAddress, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await GetEntity<GlobalNonFungibleResourceEntity>(resourceAddress, ledgerState, token);
        var vaultEntity = await GetEntity<VaultEntity>(vaultAddress, ledgerState, token);
        var nonFungibleIds = await GetNonFungibleIdsSlice(entity.Id, resourceEntity.Id, vaultEntity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.StateEntityNonFungibleIdsPageResponse(
            ledgerState, nonFungibleIds.TotalCount, nonFungibleIds.PreviousCursor, nonFungibleIds.NextCursor,
            nonFungibleIds.Items, entity.Address, resourceEntity.Address);
    }

    public async Task<GatewayModel.StateNonFungibleIdsResponse> NonFungibleIds(IEntityStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<GlobalNonFungibleResourceEntity>(request.Address, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
WITH most_recent_non_fungible_id_store_history_slice (non_fungible_id_data_ids, non_fungible_ids_total_count) AS (
    SELECT non_fungible_id_data_ids[@offset:@limit], cardinality(non_fungible_id_data_ids)
    FROM non_fungible_id_store_history
    WHERE from_state_version <= @stateVersion AND non_fungible_resource_entity_id = @entityId
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

                return vm.NonFungibleId;
            })
            .ToList();

        var previousCursor = request.Offset > 0
            ? new GatewayModel.OffsetCursor(Math.Max(request.Offset - request.Limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > request.Limit
            ? new GatewayModel.OffsetCursor(request.Offset + request.Limit).ToCursorString()
            : null;

        return new GatewayModel.StateNonFungibleIdsResponse(
            ledgerState: ledgerState,
            resourceAddress: request.Address.ToString(),
            nonFungibleIds: new GatewayModel.NonFungibleIdsCollection(
                totalCount: totalCount,
                previousCursor: previousCursor,
                nextCursor: nextCursor,
                items: items.Take(request.Limit).ToList()));
    }

    public async Task<GatewayModel.StateNonFungibleDataResponse> NonFungibleIdData(EntityAddress resourceAddress, IList<string> nonFungibleIds, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<GlobalNonFungibleResourceEntity>(resourceAddress, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
SELECT nfid.non_fungible_id AS NonFungibleId, md.is_deleted AS IsDeleted, md.data AS Data, md.from_state_version AS DataLastUpdatedAtStateVersion
FROM non_fungible_id_data nfid
LEFT JOIN LATERAL (
    SELECT data, is_deleted, from_state_version
    FROM non_fungible_id_data_history nfiddh
    WHERE nfiddh.non_fungible_id_data_id = nfid.id AND nfiddh.from_state_version <= @stateVersion
    ORDER BY nfiddh.from_state_version DESC
    LIMIT 1
) md ON TRUE
WHERE nfid.from_state_version <= @stateVersion AND nfid.non_fungible_resource_entity_id = @entityId AND nfid.non_fungible_id = ANY(@nonFungibleIds)
ORDER BY nfid.from_state_version DESC
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entity.Id,
                nonFungibleIds = nonFungibleIds,
            },
            cancellationToken: token);

        var items = new List<GatewayModel.StateNonFungibleDetailsResponseItem>();

        foreach (var vm in await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdDataViewModel>(cd))
        {
            if (vm.IsDeleted)
            {
                continue;
            }

            items.Add(new GatewayModel.StateNonFungibleDetailsResponseItem(
                nonFungibleId: vm.NonFungibleId,
                data: ScryptoSborUtils.NonFungibleDataToGatewayScryptoSbor(vm.Data, _networkConfigurationProvider.GetNetworkId()),
                lastUpdatedAtStateVersion: vm.DataLastUpdatedAtStateVersion));
        }

        return new GatewayModel.StateNonFungibleDataResponse(
            ledgerState: ledgerState,
            resourceAddress: resourceAddress.ToString(),
            nonFungibleIdType: entity.NonFungibleIdType.ToGatewayModel(),
            nonFungibleIds: items);
    }

    public async Task<GatewayModel.StateValidatorsListResponse> StateValidatorsList(GatewayModel.StateValidatorsListCursor? cursor, GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var validatorsPageSize = _endpointConfiguration.Value.ValidatorsPageSize;
        var fromStateVersion = cursor?.StateVersionBoundary ?? 0;

        var validatorsAndOneMore = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion && e.GetType() == typeof(GlobalValidatorEntity))
            .Where(e => e.FromStateVersion > fromStateVersion)
            .OrderBy(e => e.FromStateVersion)
            .ThenBy(e => e.Id)
            .Take(validatorsPageSize + 1)
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

        var validatorIds = validatorsAndOneMore.Take(validatorsPageSize).Select(e => e.Id).ToArray();

        var cd = new CommandDefinition(
            commandText: @"
WITH variables (validator_entity_id) AS (SELECT UNNEST(@validatorIds))
SELECT e.id AS ValidatorId, CAST(evh.balance AS text) AS Balance, esh.state AS State, evh.from_state_version AS BalanceLastUpdatedAtStateVersion, esh.from_state_version AS StateLastUpdatedAtStateVersion
FROM variables
INNER JOIN entities e ON e.id = variables.validator_entity_id AND from_state_version <= @stateVersion
INNER JOIN LATERAL (
    SELECT balance, from_state_version
    FROM entity_vault_history
    WHERE vault_entity_id = e.stake_vault_entity_id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) evh ON TRUE
INNER JOIN LATERAL (
    SELECT state, from_state_version
    FROM entity_state_history
    WHERE entity_id = variables.validator_entity_id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) esh ON true
;",
            parameters: new
            {
                validatorIds = validatorIds, stateVersion = ledgerState.StateVersion,
            },
            cancellationToken: token);

        var validatorsDetails = (await _dbContext.Database.GetDbConnection().QueryAsync<ValidatorCurrentStakeViewModel>(cd)).ToList();

        var metadataById = await GetMetadataSlices(validatorIds, 0, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token);

        var items = validatorsAndOneMore
            .Take(validatorsPageSize)
            .Select(v =>
            {
                GatewayModel.ValidatorCollectionItemActiveInEpoch? activeInEpoch = null;

                if (activeSetById.TryGetValue(v.Id, out var validatorActiveSetHistory))
                {
                    activeInEpoch = new GatewayModel.ValidatorCollectionItemActiveInEpoch(
                        validatorActiveSetHistory.Stake.ToString(),
                        double.Parse((validatorActiveSetHistory.Stake * _tokenAmount100 / totalStake).ToString()),
                        validatorActiveSetHistory.PublicKey.ToGatewayPublicKey());
                }

                var details = validatorsDetails.Single(x => x.ValidatorId == v.Id);

                return new GatewayModel.ValidatorCollectionItem(
                    v.Address,
                    new JRaw(details.State),
                    TokenAmount.FromSubUnitsString(details.Balance).ToString(),
                    activeInEpoch,
                    metadataById[v.Id]);
            })
            .ToList();

        var nextCursor = validatorsAndOneMore.Count == validatorsPageSize + 1
            ? new GatewayModel.StateValidatorsListCursor(validatorsAndOneMore.Last().Id).ToCursorString()
            : null;

        return new GatewayModel.StateValidatorsListResponse(ledgerState, new GatewayModel.ValidatorCollection(null, null, nextCursor, items));
    }

    private static GatewayModel.NonFungibleResourcesCollection MapToNonFungibleResourcesCollection(
        List<NonFungibleAggregatedPerVaultViewModel> input,
        List<NonFungibleIdWithOwnerDataViewModel>? nonFungibleIds,
        int vaultOffset, int vaultLimit, int resourceOffset, int resourceLimit)
    {
        var resourcesTotalCount = 0;
        var resources = new Dictionary<EntityAddress, GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregated>();

        foreach (var vm in input)
        {
            resourcesTotalCount = vm.ResourceTotalCount;

            if (!resources.TryGetValue(vm.ResourceEntityAddress, out var existingRecord))
            {
                var vaultNextCursor = vm.VaultTotalCount > vaultLimit
                    ? new GatewayApiSdk.Model.OffsetCursor(vaultLimit).ToCursorString()
                    : null;

                existingRecord = new GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregated(
                    resourceAddress: vm.ResourceEntityAddress,
                    vaults: new GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregatedVault(
                        totalCount: vm.VaultTotalCount,
                        nextCursor: vaultNextCursor,
                        items: new List<GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem>()));

                resources[vm.ResourceEntityAddress] = existingRecord;
            }

            var ids = nonFungibleIds?
                .Where(x => x.ResourceEntityId == vm.ResourceEntityId && x.VaultEntityId == vm.VaultEntityId)
                .Select(x => x.NonFungibleId).ToList();

            existingRecord.Vaults.Items.Add(new GatewayApiSdk.Model.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                totalCount: vm.NonFungibleIdsCount,
                vaultAddress: vm.VaultAddress,
                lastUpdatedAtStateVersion: vm.LastUpdatedAtStateVersion,
                items: ids));
        }

        var previousCursor = resourceOffset > 0
            ? new GatewayApiSdk.Model.OffsetCursor(Math.Max(resourceOffset - resourceLimit, 0)).ToCursorString()
            : null;

        var nextCursor = resourcesTotalCount > resourceLimit + resourceOffset
            ? new GatewayApiSdk.Model.OffsetCursor(resourceLimit).ToCursorString()
            : null;

        return new GatewayApiSdk.Model.NonFungibleResourcesCollection(resourcesTotalCount, previousCursor, nextCursor,
            resources.Values.Cast<GatewayApiSdk.Model.NonFungibleResourcesCollectionItem>().ToList());
    }

    private static GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse MapToStateEntityNonFungibleResourceVaultsPageResponse(
        List<NonFungibleResourceVaultsViewModel> input,
        Dictionary<long, List<NonFungibleIdWithOwnerDataViewModel>>? nonFungibleIds,
        GatewayModel.LedgerState ledgerState, int offset, int limit,
        string? entityGlobalAddress, string? resourceGlobalAddress)
    {
        var mapped = input.Select(x =>
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
        ).ToList();

        var vaultsTotalCount = input.FirstOrDefault()?.VaultTotalCount ?? 0;

        var previousCursor = offset > 0
            ? new GatewayModel.OffsetCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = vaultsTotalCount > offset + limit
            ? new GatewayModel.OffsetCursor(limit).ToCursorString()
            : null;

        return new GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse(ledgerState, vaultsTotalCount, previousCursor, nextCursor, mapped, entityGlobalAddress,
            resourceGlobalAddress);
    }

    private async Task<List<RoyaltyVaultBalanceViewModel>> RoyaltyVaultBalance(long[] ownerIds, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var cd = new CommandDefinition(
            commandText: @"
WITH owner_ids (entity_id) AS (SELECT UNNEST(@ownerIds))
SELECT evh.*
from owner_ids
INNER JOIN LATERAL (
    SELECT
        vault_entity_id as royaltyVaultEntityId,
        CAST(balance AS text) AS Balance,
        owner_entity_id AS OwnerEntityId,
        from_state_version AS LastUpdatedAtStateVersion
    FROM entity_vault_history
    WHERE owner_entity_id = owner_ids.entity_id AND from_state_version <= @stateVersion AND is_royalty_vault = true
    ORDER BY from_state_version DESC
    LIMIT 1
) evh on true
;",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                ownerIds = ownerIds,
            },
            cancellationToken: token);

        var result = await _dbContext.Database.GetDbConnection().QueryAsync<RoyaltyVaultBalanceViewModel>(cd);

        return result.ToList();
    }

    private async Task<Dictionary<long, GatewayModel.EntityMetadataCollection>> GetMetadataSlices(long[] entityIds, int offset, int limit, GatewayModel.LedgerState ledgerState,
        CancellationToken token)
    {
        var result = new Dictionary<long, GatewayModel.EntityMetadataCollection>();

        var cd = new CommandDefinition(
            commandText: @"
WITH variables (entity_id) AS (
    SELECT UNNEST(@entityIds)
),
metadata_slices AS (
    SELECT variables.entity_id, emah.metadata_slice, emah.metadata_total_count
    FROM variables
    INNER JOIN LATERAL (
        SELECT metadata_ids[@offset:@limit] AS metadata_slice, cardinality(metadata_ids) AS metadata_total_count
        FROM entity_metadata_aggregate_history
        WHERE entity_id = variables.entity_id AND from_state_version <= @stateVersion
        ORDER BY from_state_version DESC
        LIMIT 1
    ) emah ON TRUE
)
SELECT emh.from_state_version AS FromStateVersion, emh.entity_id AS EntityId, emh.key AS Key, emh.value AS Value, emh.is_locked AS IsLocked, ms.metadata_total_count AS TotalCount
FROM metadata_slices AS ms
INNER JOIN LATERAL UNNEST(metadata_slice) WITH ORDINALITY AS metadata_join(id, ordinality) ON TRUE
INNER JOIN entity_metadata_history emh ON emh.id = metadata_join.id AND emh.is_deleted = FALSE
ORDER BY metadata_join.ordinality ASC;",
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
            if (!result.ContainsKey(vm.EntityId))
            {
                var previousCursor = offset > 0
                    ? new GatewayModel.OffsetCursor(Math.Max(offset - limit, 0)).ToCursorString()
                    : null;

                var nextCursor = vm.TotalCount > limit
                    ? new GatewayModel.OffsetCursor(offset + limit).ToCursorString()
                    : null;

                result[vm.EntityId] = new GatewayModel.EntityMetadataCollection(vm.TotalCount, previousCursor, nextCursor, new List<GatewayModel.EntityMetadataItem>());
            }

            var value = ScryptoSborUtils.DecodeToGatewayMetadataItemValue(vm.Value, _networkConfigurationProvider.GetNetworkId());
            var stringRepresentation = ToolkitModel.RadixEngineToolkitUniffiMethods.ScryptoSborDecodeToStringRepresentation(vm.Value.ToList(), ToolkitModel.SerializationMode.PROGRAMMATIC, _networkConfigurationProvider.GetNetworkId(), null);
            var entityMetadataItemValue = new GatewayModel.EntityMetadataItemValue(vm.Value.ToHex(), JObject.Parse(stringRepresentation), value);

            result[vm.EntityId].Items.Add(new GatewayModel.EntityMetadataItem(vm.Key, entityMetadataItemValue, vm.IsLocked, vm.FromStateVersion));
        }

        foreach (var missing in entityIds.Except(result.Keys))
        {
            result[missing] = GatewayModel.EntityMetadataCollection.Empty;
        }

        return result;
    }

    private async Task<Dictionary<long, GatewayModel.EntityMetadataCollection>> GetExplicitMetadata(long[] entityIds, string[] metadataKeys, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var lookup = new HashSet<ExplicitMetadataLookup>();
        var entityIdsParameter = new List<long>();
        var metadataKeysParameter = new List<string>();

        foreach (var entityId in entityIds)
        {
            foreach (var metadataKey in metadataKeys)
            {
                lookup.Add(new ExplicitMetadataLookup(entityId, metadataKey));
            }
        }

        foreach (var (entityId, metadataKey) in lookup)
        {
            entityIdsParameter.Add(entityId);
            metadataKeysParameter.Add(metadataKey);
        }

        var metadataHistory = await _dbContext.EntityMetadataHistory
            .FromSqlInterpolated(@$"
WITH variables (entity_id, metadata_key) AS (
    SELECT UNNEST({entityIdsParameter}), UNNEST({metadataKeysParameter})
)
SELECT emh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_metadata_history
    WHERE entity_id = variables.entity_id AND key = variables.metadata_key AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) emh ON TRUE;")
            .ToListAsync(token);

        var result = new Dictionary<long, GatewayModel.EntityMetadataCollection>();

        foreach (var mh in metadataHistory)
        {
            if (mh.IsDeleted)
            {
                continue;
            }

            if (!result.ContainsKey(mh.EntityId))
            {
                result[mh.EntityId] = new GatewayModel.EntityMetadataCollection(items: new List<GatewayModel.EntityMetadataItem>());
            }

            var value = ScryptoSborUtils.DecodeToGatewayMetadataItemValue(mh.Value, _networkConfigurationProvider.GetNetworkId());
            var stringRepresentation = ToolkitModel.RadixEngineToolkitUniffiMethods.ScryptoSborDecodeToStringRepresentation(mh.Value.ToList(), ToolkitModel.SerializationMode.PROGRAMMATIC, _networkConfigurationProvider.GetNetworkId(), null);
            var entityMetadataItemValue = new GatewayModel.EntityMetadataItemValue(mh.Value.ToHex(), JObject.Parse(stringRepresentation), value);

            result[mh.EntityId].Items.Add(new GatewayModel.EntityMetadataItem(mh.Key, entityMetadataItemValue, mh.IsLocked, mh.FromStateVersion));
        }

        foreach (var missing in entityIds.Except(result.Keys))
        {
            result[missing] = GatewayModel.EntityMetadataCollection.Empty;
        }

        return result;
    }

    private async Task<GatewayModel.FungibleResourcesCollection> GetFungiblesSliceAggregatedPerResource(long entityId, int offset, int limit, GatewayModel.LedgerState ledgerState,
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
    SELECT a.val AS fungible_resource_entity_id, cardinality(fungible_resource_entity_ids) AS resources_total_count, a.ord AS ord
    FROM most_recent_entity_resource_aggregate_history_nested
    LEFT JOIN LATERAL UNNEST(fungible_resource_entity_ids[@offset:@limit]) WITH ORDINALITY AS a(val,ord) ON true
)
SELECT
    e.address AS ResourceEntityAddress,
    CAST(eravh.balance AS text) AS Balance,
    ah.resources_total_count AS ResourcesTotalCount,
    eravh.from_state_version AS LastUpdatedAtStateVersion
FROM most_recent_entity_resource_aggregate_history ah
INNER JOIN LATERAL (
    SELECT balance, from_state_version
    FROM entity_resource_aggregated_vaults_history
    WHERE from_state_version <= @stateVersion AND entity_id = @entityId AND resource_entity_id = ah.fungible_resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) eravh ON TRUE
INNER JOIN entities e ON ah.fungible_resource_entity_id = e.id
order by ah.ord
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
                resourceAddress: vm.ResourceEntityAddress,
                amount: TokenAmount.FromSubUnitsString(vm.Balance).ToString(),
                lastUpdatedAtStateVersion: vm.LastUpdatedAtStateVersion));
        }

        var previousCursor = offset > 0
            ? new GatewayModel.OffsetCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > limit
            ? new GatewayModel.OffsetCursor(offset + limit).ToCursorString()
            : null;

        return new GatewayModel.FungibleResourcesCollection(totalCount, previousCursor, nextCursor, items.Take(limit).ToList());
    }

    private async Task<Dictionary<long, ResourceEntitySupplyHistory>> GetResourcesSupplyData(long[] entityIds, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        if (!entityIds.Any())
        {
            return new Dictionary<long, ResourceEntitySupplyHistory>();
        }

        var result = await _dbContext.ResourceEntitySupplyHistory.FromSqlInterpolated($@"
WITH variables (entity_id) AS (SELECT UNNEST({entityIds}))
SELECT resh.*
FROM variables
INNER JOIN LATERAL(
    SELECT *
    FROM resource_entity_supply_history
    WHERE from_state_version <= {ledgerState.StateVersion} AND resource_entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) resh ON true")
            .ToDictionaryAsync(e => e.ResourceEntityId, token);

        return result;
    }

    private async Task<GatewayModel.FungibleResourcesCollection> GetFungiblesSliceAggregatedPerVault(long entityId, int resourceOffset, int resourceLimit, int vaultOffset, int vaultLimit, GatewayModel.LedgerState ledgerState,
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
    LEFT JOIN LATERAL UNNEST(fungible_resource_entity_ids[@resourceOffset:@resourceLimit]) WITH ORDINALITY AS a(val,ord) ON true
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
    LEFT JOIN LATERAL UNNEST(vault_entity_ids[@vaultOffset:@vaultLimit]) WITH ORDINALITY AS a(val,ord) ON true
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
                resourceOffset = resourceOffset + 1,
                resourceLimit = resourceLimit + resourceOffset,
                vaultOffset = vaultOffset + 1,
                vaultLimit = vaultLimit + vaultOffset,
            },
            cancellationToken: token);

        var resourcesTotalCount = 0;

        var resources = new Dictionary<EntityAddress, GatewayModel.FungibleResourcesCollectionItemVaultAggregated>();

        foreach (var vm in await _dbContext.Database.GetDbConnection().QueryAsync<FungibleAggregatedPerVaultViewModel>(cd))
        {
            resourcesTotalCount = vm.ResourceTotalCount;

            if (!resources.TryGetValue(vm.ResourceEntityAddress, out var existingRecord))
            {
                var vaultNextCursor = vm.VaultTotalCount > vaultLimit
                    ? new GatewayModel.OffsetCursor(vaultLimit).ToCursorString()
                    : null;

                existingRecord = new GatewayModel.FungibleResourcesCollectionItemVaultAggregated(
                    resourceAddress: vm.ResourceEntityAddress,
                    vaults: new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVault(
                        totalCount: vm.VaultTotalCount,
                        nextCursor: vaultNextCursor,
                        items: new List<GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem>()));

                resources[vm.ResourceEntityAddress] = existingRecord;
            }

            existingRecord.Vaults.Items.Add(new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                amount: TokenAmount.FromSubUnitsString(vm.Balance).ToString(),
                vaultAddress: vm.VaultAddress,
                lastUpdatedAtStateVersion: vm.LastUpdatedAtStateVersion));
        }

        var previousCursor = resourceOffset > 0
            ? new GatewayModel.OffsetCursor(Math.Max(resourceOffset - resourceLimit, 0)).ToCursorString()
            : null;

        var nextCursor = resourcesTotalCount > resourceLimit + resourceOffset
            ? new GatewayModel.OffsetCursor(resourceLimit).ToCursorString()
            : null;

        return new GatewayModel.FungibleResourcesCollection(resourcesTotalCount, previousCursor, nextCursor, resources.Values.Cast<GatewayModel.FungibleResourcesCollectionItem>().ToList());
    }

    private async Task<GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVault> GetFungibleResourceVaults(long entityId, long resourceEntityId, int vaultOffset, int vaultLimit, GatewayModel.LedgerState ledgerState,
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
    LEFT JOIN LATERAL UNNEST(vault_entity_ids[@vaultOffset:@vaultLimit]) WITH ORDINALITY a(val,ord) ON true
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
                vaultOffset = vaultOffset + 1,
                vaultLimit = vaultLimit + vaultOffset,
            },
            cancellationToken: token);

        var result = (await _dbContext.Database.GetDbConnection().QueryAsync<FungibleResourceVaultsViewModel>(cd)).ToList();
        var vaultsTotalCount = result.FirstOrDefault()?.VaultTotalCount ?? 0;
        var castedResult = result.Select(x =>
                new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                    amount: TokenAmount.FromSubUnitsString(x.Balance).ToString(),
                    vaultAddress: x.VaultAddress,
                    lastUpdatedAtStateVersion: x.LastUpdatedAtStateVersion))
               .ToList();

        var previousCursor = vaultOffset > 0
            ? new GatewayModel.OffsetCursor(Math.Max(vaultOffset - vaultLimit, 0)).ToCursorString()
            : null;

        var nextCursor = vaultsTotalCount > vaultOffset + vaultLimit
            ? new GatewayModel.OffsetCursor(vaultLimit).ToCursorString()
            : null;

        return new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVault(vaultsTotalCount, previousCursor, nextCursor, castedResult);
    }

    private async Task<GatewayModel.NonFungibleResourcesCollection> GetNonFungiblesSliceAggregatedPerResource(long entityId, int offset, int limit, GatewayModel.LedgerState ledgerState,
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
    LEFT JOIN LATERAL UNNEST(non_fungible_resource_entity_ids[@offset:@limit]) WITH ORDINALITY a(val,ord)  ON true
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
                resourceAddress: vm.ResourceEntityAddress,
                amount: vm.NonFungibleIdsCount,
                lastUpdatedAtStateVersion: vm.LastUpdatedAtStateVersion));
        }

        var previousCursor = offset > 0
            ? new GatewayModel.OffsetCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > limit
            ? new GatewayModel.OffsetCursor(offset + limit).ToCursorString()
            : null;

        return new GatewayModel.NonFungibleResourcesCollection(totalCount, previousCursor, nextCursor, items.Take(limit).ToList());
    }

    private async Task<List<NonFungibleAggregatedPerVaultViewModel>> GetNonFungiblesSliceAggregatedPerVault(long entityId, int resourceOffset, int resourceLimit, int vaultOffset, int vaultLimit,
        GatewayModel.LedgerState ledgerState, CancellationToken token)
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
    LEFT JOIN LATERAL UNNEST(non_fungible_resource_entity_ids[@resourceOffset:@resourceLimit]) WITH ORDINALITY a(val,ord) ON true
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
    LEFT JOIN LATERAL UNNEST(vault_entity_ids[@vaultOffset:@vaultLimit]) WITH ORDINALITY a(val,ord) ON true
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
                resourceOffset = resourceOffset + 1,
                resourceLimit = resourceLimit + resourceOffset,
                vaultOffset = vaultOffset + 1,
                vaultLimit = vaultLimit + vaultOffset,
            },
            cancellationToken: token);

        return (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleAggregatedPerVaultViewModel>(cd)).ToList();
    }

    private async Task<List<NonFungibleResourceVaultsViewModel>> GetNonFungibleResourceVaults(long entityId, long resourceEntityId, int vaultOffset, int vaultLimit,
        GatewayModel.LedgerState ledgerState, CancellationToken token)
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
    LEFT JOIN LATERAL UNNEST(vault_entity_ids[@vaultOffset:@vaultLimit]) WITH ORDINALITY a(val,ord) ON true
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
                vaultOffset = vaultOffset + 1,
                vaultLimit = vaultOffset + vaultLimit,
            },
            cancellationToken: token);

        var result = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleResourceVaultsViewModel>(cd)).ToList();
        return result;
    }

    private async Task<List<NonFungibleIdWithOwnerDataViewModel>> GetNonFungibleIdsFirstPage(long[] entityIds, long[] resourceEntityIds, long[] vaultEntityIds, int nonFungibleIdsLimit,
            GatewayModel.LedgerState ledgerState, CancellationToken token)
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
        UNNEST(v.non_fungible_ids[0:@nfidsLimit]) as unnested_non_fungible_id
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
                nfidsLimit = nonFungibleIdsLimit,
            },
            cancellationToken: token);

        var result = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdWithOwnerDataViewModel>(cd)).ToList();

        return result;
    }

    private async Task<GatewayModel.NonFungibleIdsCollection> GetNonFungibleIdsSlice(long entityId, long resourceEntityId, long vaultEntityId, int offset, int limit,
        GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
SELECT nfid.non_fungible_id AS NonFungibleId, final.total_count AS NonFungibleIdsTotalCount
FROM (
    SELECT a.val AS non_fungible_id_data_id, cardinality(non_fungible_ids) AS total_count, a.ord AS ord
    FROM entity_vault_history
    LEFT JOIN LATERAL UNNEST(non_fungible_ids[@offset:@limit]) WITH ORDINALITY a(val,ord) ON true
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
                offset = offset + 1,
                limit = offset + limit + 1,
            },
            cancellationToken: token);

        var totalCount = 0;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdViewModel>(cd)).ToList()
            .Select(vm =>
            {
                totalCount = vm.NonFungibleIdsTotalCount;

                return vm.NonFungibleId;
            })
            .ToList();

        var previousCursor = offset > 0
            ? new GatewayModel.OffsetCursor(Math.Max(offset - limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > limit
            ? new GatewayModel.OffsetCursor(offset + limit).ToCursorString()
            : null;

        return new GatewayModel.NonFungibleIdsCollection(totalCount, previousCursor, nextCursor, items.Take(limit).ToList());
    }

    private async Task<TEntity> GetEntity<TEntity>(EntityAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity
    {
        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.Address == address, token);

        if (entity == null)
        {
            // TODO this method should return null/throw on missing, virtual component handling should be done upstream to avoid entity.Id = 0 uses, see https://github.com/radixdlt/babylon-gateway/pull/171#discussion_r1111957627
            if (!TryGetVirtualEntity(address, out entity))
            {
                throw new EntityNotFoundException(address.ToString());
            }
        }

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidEntityException(address.ToString());
        }

        return typedEntity;
    }

    private bool TryGetVirtualEntity(EntityAddress address, [NotNullWhen(true)] out Entity? entity)
    {
        var firstAddressByte = RadixAddressCodec.Decode(address).Data[0];

        if (firstAddressByte == _ecdsaSecp256k1VirtualAccountAddressPrefix || firstAddressByte == _eddsaEd25519VirtualAccountAddressPrefix)
        {
            entity = new VirtualAccountComponentEntity(address);

            return true;
        }

        if (firstAddressByte == _ecdsaSecp256k1VirtualIdentityAddressPrefix || firstAddressByte == _eddsaEd25519VirtualIdentityAddressPrefix)
        {
            entity = new VirtualIdentityEntity(address);

            return true;
        }

        entity = default;

        return false;
    }

    private async Task<ICollection<Entity>> GetEntities(List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var entities = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion && addresses.Contains(e.Address))
            .ToListAsync(token);

        foreach (var address in addresses)
        {
            if (entities.All(e => e.Address != address) && TryGetVirtualEntity(address, out var virtualEntity))
            {
                entities.Add(virtualEntity);
            }
        }

        return entities;
    }

    private async Task<Dictionary<long, string>> GetAccessRulesHistory(ICollection<ResourceEntity> resourceEntities, ICollection<ComponentEntity> componentEntities, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var lookup = new HashSet<long>();

        foreach (var resourceEntity in resourceEntities)
        {
            lookup.Add(resourceEntity.Id);
        }

        foreach (var componentEntity in componentEntities)
        {
            lookup.Add(componentEntity.Id);
        }

        if (!lookup.Any())
        {
            return new Dictionary<long, string>();
        }

        var entityIds = lookup.ToList();

        // TODO just a prototype
        return await _dbContext.EntityAccessRulesAggregateHistory
            .FromSqlInterpolated($@"
WITH variables (entity_id) AS (SELECT UNNEST({entityIds}))
SELECT earah.*
FROM variables v
INNER JOIN LATERAL (
    SELECT *
    FROM entity_access_rules_aggregate_history
    WHERE entity_id = v.entity_id AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) earah ON TRUE;")
            .ToDictionaryAsync(e => e.EntityId, e => $$"""{"_todo_":"just a prototype","owner_role_id":{{e.OwnerRoleId}},"entry_ids":[{{string.Join(",", e.EntryIds)}}]}""", token);
    }

    private async Task<Dictionary<long, EntityStateHistory>> GetStateHistory(ICollection<ComponentEntity> componentEntities, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var lookup = new HashSet<long>();

        foreach (var componentEntity in componentEntities)
        {
            lookup.Add(componentEntity.Id);
        }

        if (!lookup.Any())
        {
            return new Dictionary<long, EntityStateHistory>();
        }

        var entityIds = lookup.ToList();

        return await _dbContext.EntityStateHistory
            .FromSqlInterpolated($@"
WITH variables (entity_id) AS (SELECT UNNEST({entityIds}))
SELECT esh.*
FROM variables v
INNER JOIN LATERAL (
    SELECT *
    FROM entity_state_history
    WHERE entity_id = v.entity_id AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) esh ON TRUE;")
            .ToDictionaryAsync(e => e.EntityId, token);
    }

    private async Task<Dictionary<long, PackageBlueprint>> GetPackageBlueprints(long[] entityIds, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        if (!entityIds.Any())
        {
            return new Dictionary<long, PackageBlueprint>();
        }

        // TODO we do not want to select package's code unless explicitly requested as it may be a truly massive payload
        var result = await _dbContext.PackageBlueprints.FromSqlInterpolated($@"
WITH variables (entity_id) AS (SELECT UNNEST({entityIds}))
SELECT pb.*
FROM variables
INNER JOIN LATERAL(
    SELECT *
    FROM package_blueprints
    WHERE package_entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) pb ON true")
            .ToDictionaryAsync(e => e.PackageEntityId, token);

        return result;
    }

    private async Task<Dictionary<long, EntityAddress>> GetCorrelatedEntityAddresses(ICollection<Entity> entities, ICollection<ComponentEntity> componentEntities, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var lookup = new HashSet<long>();

        foreach (var entity in entities)
        {
            if (entity.HasParent)
            {
                lookup.Add(entity.ParentAncestorId.Value);
                lookup.Add(entity.OwnerAncestorId.Value);
                lookup.Add(entity.GlobalAncestorId.Value);
            }
        }

        foreach (var componentEntity in componentEntities)
        {
            lookup.Add(componentEntity.PackageId);
        }

        var ids = lookup.ToList();

        return await _dbContext.Entities
            .Where(e => ids.Contains(e.Id))
            .Select(e => new { e.Id, GlobalAddress = e.Address })
            .ToDictionaryAsync(e => e.Id, e => e.GlobalAddress, token);
    }

    private async Task<Dictionary<EntityAddress, long>> ResolveResourceEntityIds(ICollection<GatewayModel.FungibleResourcesCollection>? fungibleResources, ICollection<GatewayModel.NonFungibleResourcesCollection>? nonFungibleResources, CancellationToken token)
    {
        if (fungibleResources?.Any() != true && nonFungibleResources?.Any() != true)
        {
            return new Dictionary<EntityAddress, long>();
        }

        var lookupAddresses = new HashSet<string>();

        fungibleResources?.SelectMany(fr => fr.Items).Select(i => i.ResourceAddress).ForEach(a => lookupAddresses.Add(a));
        nonFungibleResources?.SelectMany(nfr => nfr.Items).Select(i => i.ResourceAddress).ForEach(a => lookupAddresses.Add(a));

        var addresses = lookupAddresses.ToList();

        return await _dbContext.Entities
            .Where(e => addresses.Contains(e.Address))
            .Select(e => new { e.Id, e.Address })
            .ToDictionaryAsync(e => e.Address, e => e.Id, token);
    }
}
