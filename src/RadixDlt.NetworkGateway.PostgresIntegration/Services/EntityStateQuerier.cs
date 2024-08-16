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
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.Abstractions.StandardMetadata;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal partial class EntityStateQuerier : IEntityStateQuerier
{
    private record struct SchemaIdentifier(ValueBytes SchemaHash, long EntityId);

    private record MetadataViewModel(long FromStateVersion, long EntityId, string Key, byte[] Value, bool IsLocked, int TotalCount);

    private class SchemaEntryViewModel : SchemaEntryDefinition
    {
        public int TotalCount { get; set; }
    }

    private record RoyaltyVaultBalanceViewModel(long RoyaltyVaultEntityId, string Balance, long OwnerEntityId, long LastUpdatedAtStateVersion);

    private record NonFungibleIdsViewModel(long Id, long FromStateVersion, string NonFungibleId);

    private record struct ExplicitMetadataLookup(long EntityId, string MetadataKey);

    private readonly TokenAmount _tokenAmount100 = TokenAmount.FromDecimalString("100");
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IOptionsSnapshot<EndpointOptions> _endpointConfiguration;
    private readonly ReadOnlyDbContext _dbContext;
    private readonly IVirtualEntityDataProvider _virtualEntityDataProvider;
    private readonly IRoleAssignmentQuerier _roleAssignmentQuerier;
    private readonly IDapperWrapper _dapperWrapper;

    public EntityStateQuerier(
        INetworkConfigurationProvider networkConfigurationProvider,
        ReadOnlyDbContext dbContext,
        IOptionsSnapshot<EndpointOptions> endpointConfiguration,
        IVirtualEntityDataProvider virtualEntityDataProvider,
        IRoleAssignmentQuerier roleAssignmentQuerier,
        IDapperWrapper dapperWrapper)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
        _dbContext = dbContext;
        _endpointConfiguration = endpointConfiguration;
        _virtualEntityDataProvider = virtualEntityDataProvider;
        _roleAssignmentQuerier = roleAssignmentQuerier;
        _dapperWrapper = dapperWrapper;
    }

    public async Task<GatewayModel.StateEntityDetailsResponse> EntityDetails(
        List<EntityAddress> addresses,
        bool aggregatePerVault,
        GatewayModel.StateEntityDetailsOptIns optIns,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var defaultPageSize = _endpointConfiguration.Value.DefaultPageSize;
        var packagePageSize = _endpointConfiguration.Value.DefaultHeavyCollectionsPageSize;
        var networkConfiguration = await _networkConfigurationProvider.GetNetworkConfiguration(token);

        var entities = await GetEntities(addresses, ledgerState, token);
        var componentEntities = entities.OfType<ComponentEntity>().ToList();
        var resourceEntities = entities.OfType<ResourceEntity>().ToList();
        var packageEntities = entities.OfType<GlobalPackageEntity>().ToList();
        var fungibleVaultEntities = entities.OfType<InternalFungibleVaultEntity>().ToList();
        var nonFungibleVaultEntities = entities.OfType<InternalNonFungibleVaultEntity>().ToList();
        var persistedComponentEntities = componentEntities.Where(x => x.Id != default).ToList();
        var globalPersistedComponentEntities = persistedComponentEntities.Where(x => x.IsGlobal).ToList();

        // TODO ideally we'd like to run all those in parallel
        var metadata = await GetMetadataSlices(entities.Select(e => e.Id).ToArray(), 0, defaultPageSize, ledgerState, token);
        var roleAssignmentsHistory = await _roleAssignmentQuerier.GetRoleAssignmentsHistory(globalPersistedComponentEntities, ledgerState, token);
        var resourcesSupplyData = await GetResourcesSupplyData(resourceEntities.Select(x => x.Id).ToArray(), ledgerState, token);
        var packageBlueprintHistory = await GetPackageBlueprintHistory(packageEntities.Select(e => e.Id).ToArray(), 0, packagePageSize, ledgerState, token);
        var packageCodeHistory = await GetPackageCodeHistory(packageEntities.Select(e => e.Id).ToArray(), 0, packagePageSize, ledgerState, token);
        var packageSchemaHistory = await GetEntitySchemaHistory(packageEntities.Select(e => e.Id).ToArray(), 0, packagePageSize, ledgerState, token);
        // TODO var fungibleVaultsHistory = await GetFungibleVaultsHistory(fungibleVaultEntities, ledgerState, token);
        var nonFungibleVaultsHistory = await GetNonFungibleVaultsHistory(nonFungibleVaultEntities, optIns.NonFungibleIncludeNfids, ledgerState, token);
        var resolvedTwoWayLinks = optIns.DappTwoWayLinks
            ? await new StandardMetadataResolver(_dbContext, _dapperWrapper).ResolveTwoWayLinks(entities, true, ledgerState, token)
            : ImmutableDictionary<EntityAddress, ICollection<ResolvedTwoWayLink>>.Empty;
        var resolvedNativeResourceDetails = optIns.NativeResourceDetails
            ? await new NativeResourceDetailsResolver(_dbContext, _dapperWrapper, networkConfiguration).GetNativeResourceDetails(entities, ledgerState, token)
            : ImmutableDictionary<EntityAddress, GatewayModel.NativeResourceDetails>.Empty;

        var correlatedAddresses = await GetCorrelatedEntityAddresses(entities, packageBlueprintHistory, ledgerState, token);

        var qc = new EntityResourcesPageQuery.DetailsQueryConfiguration(defaultPageSize, defaultPageSize, aggregatePerVault ? defaultPageSize : 0, ledgerState.StateVersion);
        var entityResources = await EntityResourcesPageQuery.Details(_dbContext, _dapperWrapper, componentEntities.Select(e => e.Id).ToArray(), qc, token);

        // TODO the above does not support optIns.NonFungibleIncludeNfids by design (use separate query if needed)

        var entityAndResourceIds = entityResources.Values
            .SelectMany(x => x.Resources.Values.Select(r => r.ResourceEntityId))
            .Union(entities.Select(e => e.Id))
            .ToHashSet()
            .ToArray();

        var explicitMetadata = optIns.ExplicitMetadata?.Any() == true
            ? await GetExplicitMetadata(entityAndResourceIds, optIns.ExplicitMetadata.ToArray(), ledgerState, token)
            : null;

        // TODO wrap in .ToGatewayModel()?
        var mappedEntityResources = entityResources.Values.Select(x => TmpMapper.Map(x, explicitMetadata, aggregatePerVault, qc.FungibleResourcesPerEntity, qc.NonFungibleResourcesPerEntity, qc.VaultsPerResource)).ToList();

        // those collections do NOT support virtual entities, thus they cannot be used outside of entity type specific context (switch statement below and its case blocks)
        // virtual entities generate those on their own (dynamically generated information)
        var stateHistory = await GetStateHistory(persistedComponentEntities, ledgerState, token);
        var royaltyVaultsBalances = globalPersistedComponentEntities.Any() && (optIns.ComponentRoyaltyVaultBalance || optIns.PackageRoyaltyVaultBalance)
            ? await GetRoyaltyVaultBalances(globalPersistedComponentEntities.Select(x => x.Id).ToArray(), ledgerState, token)
            : null;
        var componentRoyaltyConfigs = globalPersistedComponentEntities.Any() && optIns.ComponentRoyaltyConfig
            ? await GetComponentRoyaltyConfigs(globalPersistedComponentEntities.Select(x => x.Id).ToArray(), ledgerState, token)
            : null;

        var items = new List<GatewayModel.StateEntityDetailsResponseItem>();

        foreach (var entity in entities)
        {
            GatewayModel.StateEntityDetailsResponseItemDetails? details = null;

            resolvedTwoWayLinks.TryGetValue(entity.Address, out var twoWayLinks);
            resolvedNativeResourceDetails.TryGetValue(entity.Address, out var nativeResourceDetails);

            switch (entity)
            {
                case GlobalFungibleResourceEntity frme:
                    var fungibleResourceSupplyData = resourcesSupplyData[frme.Id];
                    var fungibleTwoWayLinkedDapps = twoWayLinks?.OfType<DappDefinitionsResolvedTwoWayLink>().Select(x => x.ToGatewayModel()).ToList();

                    details = new GatewayModel.StateEntityDetailsResponseFungibleResourceDetails(
                        roleAssignments: roleAssignmentsHistory[frme.Id],
                        totalSupply: fungibleResourceSupplyData.TotalSupply.ToString(),
                        totalMinted: fungibleResourceSupplyData.TotalMinted.ToString(),
                        totalBurned: fungibleResourceSupplyData.TotalBurned.ToString(),
                        divisibility: frme.Divisibility,
                        twoWayLinkedDapps: fungibleTwoWayLinkedDapps?.Any() == true ? new GatewayModel.TwoWayLinkedDappsCollection(items: fungibleTwoWayLinkedDapps) : null,
                        nativeResourceDetails: nativeResourceDetails);

                    break;

                case GlobalNonFungibleResourceEntity nfrme:
                    var nonFungibleResourceSupplyData = resourcesSupplyData[nfrme.Id];
                    var nonFungibleTwoWayLinkedDapps = twoWayLinks?.OfType<DappDefinitionsResolvedTwoWayLink>().Select(x => x.ToGatewayModel()).ToList();

                    if (nonFungibleResourceSupplyData == null)
                    {
                        throw new ArgumentException($"Resource supply data for fungible resource with database id:{nfrme.Id} not found.");
                    }

                    details = new GatewayModel.StateEntityDetailsResponseNonFungibleResourceDetails(
                        roleAssignments: roleAssignmentsHistory[nfrme.Id],
                        totalSupply: nonFungibleResourceSupplyData.TotalSupply.ToString(),
                        totalMinted: nonFungibleResourceSupplyData.TotalMinted.ToString(),
                        totalBurned: nonFungibleResourceSupplyData.TotalBurned.ToString(),
                        nonFungibleIdType: nfrme.NonFungibleIdType.ToGatewayModel(),
                        nonFungibleDataMutableFields: nfrme.NonFungibleDataMutableFields,
                        twoWayLinkedDapps: nonFungibleTwoWayLinkedDapps?.Any() == true ? new GatewayModel.TwoWayLinkedDappsCollection(items: nonFungibleTwoWayLinkedDapps) : null,
                        nativeResourceDetails: nativeResourceDetails);
                    break;

                case GlobalPackageEntity pe:
                    var packageRoyaltyVaultBalance = royaltyVaultsBalances?.SingleOrDefault(x => x.OwnerEntityId == pe.Id)?.Balance;
                    var blueprintItems = new List<GatewayModel.PackageBlueprintCollectionItem>();
                    var codeItems = new List<GatewayModel.PackageCodeCollectionItem>();
                    var schemaItems = new List<GatewayModel.EntitySchemaCollectionItem>();
                    string? blueprintCursor = null;
                    string? codeCursor = null;
                    string? schemaCursor = null;
                    long? blueprintTotalCount = default;
                    long? codeTotalCount = default;
                    long? schemaTotalCount = default;

                    if (packageBlueprintHistory.TryGetValue(pe.Id, out var packageBlueprints))
                    {
                        blueprintTotalCount = packageBlueprints.FirstOrDefault()?.TotalCount ?? 0;
                        blueprintItems.AddRange(packageBlueprints.Select(pb => pb.ToGatewayModel(correlatedAddresses)));
                        blueprintCursor = CursorGenerator.GenerateOffsetCursor(0, packagePageSize, blueprintTotalCount.Value);
                    }

                    if (packageCodeHistory.TryGetValue(pe.Id, out var packageCodes))
                    {
                        codeTotalCount = packageCodes.FirstOrDefault()?.TotalCount ?? 0;
                        codeItems.AddRange(packageCodes.Select(pc => pc.ToGatewayModel()));
                        codeCursor = CursorGenerator.GenerateOffsetCursor(0, packagePageSize, codeTotalCount.Value);
                    }

                    if (packageSchemaHistory.TryGetValue(pe.Id, out var packageSchemas))
                    {
                        schemaTotalCount = packageSchemas.FirstOrDefault()?.TotalCount ?? 0;
                        schemaItems.AddRange(packageSchemas.Take(packagePageSize).Select(sh => sh.ToGatewayModel()));
                        schemaCursor = CursorGenerator.GenerateOffsetCursor(0, packagePageSize, schemaTotalCount.Value);
                    }

                    details = new GatewayModel.StateEntityDetailsResponsePackageDetails(
                        vmType: codeItems[0].VmType,
                        codeHashHex: codeItems[0].CodeHashHex,
                        codeHex: codeItems[0].CodeHex,
                        codes: new GatewayModel.PackageCodeCollection(codeTotalCount, codeCursor, codeItems),
                        royaltyVaultBalance: packageRoyaltyVaultBalance != null ? TokenAmount.FromSubUnitsString(packageRoyaltyVaultBalance).ToString() : null,
                        blueprints: new GatewayModel.PackageBlueprintCollection(blueprintTotalCount, blueprintCursor, blueprintItems),
                        schemas: new GatewayModel.EntitySchemaCollection(schemaTotalCount, schemaCursor, schemaItems),
                        roleAssignments: roleAssignmentsHistory[pe.Id],
                        twoWayLinkedDappAddress: twoWayLinks?.OfType<DappDefinitionResolvedTwoWayLink>().FirstOrDefault()?.EntityAddress);
                    break;

                case VirtualIdentityEntity:
                case VirtualAccountComponentEntity:
                    var virtualEntityData = await _virtualEntityDataProvider.GetVirtualEntityData(entity.Address);

                    details = virtualEntityData.Details;
                    metadata[entity.Id] = virtualEntityData.Metadata;
                    break;

                case InternalFungibleVaultEntity ifve:
                    throw new NotSupportedException("bleee");
                    // var fungibleVaultHistory = fungibleVaultsHistory[entity.Id];
                    //
                    // details = new GatewayModel.StateEntityDetailsResponseFungibleVaultDetails(
                    //     resourceAddress: correlatedAddresses[ifve.GetResourceEntityId()],
                    //     balance: new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                    //         vaultAddress: entity.Address,
                    //         amount: TokenAmount.FromSubUnitsString(fungibleVaultHistory.Balance).ToString(),
                    //         lastUpdatedAtStateVersion: fungibleVaultHistory.LastUpdatedAtStateVersion));
                case InternalNonFungibleVaultEntity infve:
                    var nonFungibleVaultHistory = nonFungibleVaultsHistory[entity.Id];

                    List<string>? nfItems = null;
                    string? nfNextCursor = null;

                    if (optIns.NonFungibleIncludeNfids && nonFungibleVaultHistory.NonFungibleIdsAndOneMore.Any())
                    {
                        nfItems = nonFungibleVaultHistory.NonFungibleIdsAndOneMore.Take(defaultPageSize).ToList();
                        nfNextCursor = CursorGenerator.GenerateOffsetCursor(0, defaultPageSize, nonFungibleVaultHistory.NonFungibleIdsCount);
                    }

                    details = new GatewayModel.StateEntityDetailsResponseNonFungibleVaultDetails(
                        resourceAddress: correlatedAddresses[infve.GetResourceEntityId()],
                        balance: new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                            totalCount: nonFungibleVaultHistory.NonFungibleIdsCount,
                            nextCursor: nfNextCursor,
                            items: nfItems,
                            vaultAddress: entity.Address,
                            lastUpdatedAtStateVersion: nonFungibleVaultHistory.LastUpdatedAtStateVersion));
                    break;

                case ComponentEntity ce:
                    ComponentMethodRoyaltyEntryHistory[]? componentRoyaltyConfig = null;
                    GatewayModel.TwoWayLinkedDappOnLedgerDetails? twoWayLinkedDappOnLedgerDetails = null;
                    var nonAccountTwoWayLinkedDapp = twoWayLinks?.OfType<DappDefinitionResolvedTwoWayLink>().FirstOrDefault()?.EntityAddress;

                    if (ce is GlobalAccountEntity)
                    {
                        var accountTwoWayLinkedDapps = twoWayLinks?.OfType<DappDefinitionsResolvedTwoWayLink>().Select(x => x.ToGatewayModel()).ToList();
                        var accountTwoWayLinkedEntities = twoWayLinks?.OfType<DappClaimedEntityResolvedTwoWayLink>().Select(x => x.ToGatewayModel()).ToList();
                        var accountTwoWayLinkedLocker = twoWayLinks?.OfType<DappAccountLockerResolvedTwoWayLink>().FirstOrDefault()?.LockerAddress;

                        if (accountTwoWayLinkedDapps?.Any() == true || accountTwoWayLinkedEntities?.Any() == true || accountTwoWayLinkedLocker != null)
                        {
                            twoWayLinkedDappOnLedgerDetails = new GatewayModel.TwoWayLinkedDappOnLedgerDetails(
                                dapps: accountTwoWayLinkedDapps?.Any() == true ? new GatewayModel.TwoWayLinkedDappsCollection(items: accountTwoWayLinkedDapps) : null,
                                entities: accountTwoWayLinkedEntities?.Any() == true ? new GatewayModel.TwoWayLinkedEntitiesCollection(items: accountTwoWayLinkedEntities) : null,
                                primaryLocker: accountTwoWayLinkedLocker);
                        }
                    }

                    stateHistory.TryGetValue(ce.Id, out var state);
                    roleAssignmentsHistory.TryGetValue(ce.Id, out var roleAssignments);
                    componentRoyaltyConfigs?.TryGetValue(ce.Id, out componentRoyaltyConfig);
                    var componentRoyaltyVaultBalance = royaltyVaultsBalances?.SingleOrDefault(x => x.OwnerEntityId == ce.Id)?.Balance;

                    details = new GatewayModel.StateEntityDetailsResponseComponentDetails(
                        packageAddress: correlatedAddresses[ce.GetInstantiatingPackageId()],
                        blueprintName: ce.BlueprintName,
                        blueprintVersion: ce.BlueprintVersion,
                        state: state != null ? new JRaw(state) : null,
                        roleAssignments: roleAssignments,
                        royaltyVaultBalance: componentRoyaltyVaultBalance != null ? TokenAmount.FromSubUnitsString(componentRoyaltyVaultBalance).ToString() : null,
                        royaltyConfig: optIns.ComponentRoyaltyConfig ? componentRoyaltyConfig.ToGatewayModel() : null,
                        twoWayLinkedDappAddress: nonAccountTwoWayLinkedDapp,
                        twoWayLinkedDappDetails: twoWayLinkedDappOnLedgerDetails,
                        nativeResourceDetails: nativeResourceDetails);
                    break;
            }

            var ancestorIdentities = optIns.AncestorIdentities && entity.HasParent
                ? new GatewayModel.StateEntityDetailsResponseItemAncestorIdentities(
                    parentAddress: correlatedAddresses[entity.ParentAncestorId.Value],
                    ownerAddress: correlatedAddresses[entity.OwnerAncestorId.Value],
                    globalAddress: correlatedAddresses[entity.GlobalAncestorId.Value])
                : null;

            var x = mappedEntityResources.FirstOrDefault(x => x.EntityId == entity.Id);

            items.Add(new GatewayModel.StateEntityDetailsResponseItem(
                address: entity.Address,
                fungibleResources: x?.Fungibles,
                nonFungibleResources: x?.NonFungibles,
                ancestorIdentities: ancestorIdentities,
                metadata: metadata[entity.Id],
                explicitMetadata: explicitMetadata?[entity.Id],
                details: details));
        }

        return new GatewayModel.StateEntityDetailsResponse(ledgerState, items);
    }

    public async Task<Dictionary<long, GatewayModel.NonFungibleResourcesCollection>> EntityNonFungibleResourcesPageSlice(
        long[] entityIds,
        bool aggregatePerVault,
        bool includeNfids,
        int offset,
        int limit,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        return aggregatePerVault
            ? await NonFungiblesAggregatedPerVaultPage(entityIds, includeNfids, offset, limit, ledgerState, token)
            : await NonFungiblesAggregatedPerResourcePage(entityIds, offset, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token);
    }

    public async Task<GatewayModel.StateEntityNonFungiblesPageResponse> EntityNonFungibleResourcesPage(
        IEntityStateQuerier.PageRequest pageRequest,
        bool aggregatePerVault,
        GatewayModel.StateEntityNonFungiblesPageRequestOptIns optIns,
        GatewayModel.LedgerState ledgerState,
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

        return new GatewayModel.StateEntityNonFungiblesPageResponse(ledgerState, result.TotalCount, result.NextCursor, result.Items, pageRequest.Address);
    }

    public async Task<GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse> EntityNonFungibleResourceVaults(
        IEntityStateQuerier.ResourceVaultsPageRequest request,
        GatewayModel.StateEntityNonFungibleResourceVaultsPageOptIns optIns,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await GetEntity<GlobalNonFungibleResourceEntity>(request.ResourceAddress, ledgerState, token);
        var nonFungibles = await GetNonFungibleResourceVaults(entity.Id, resourceEntity.Id, request.Offset, request.Limit, ledgerState, token);
        var vaultEntityIdsToQuery = nonFungibles.Select(x => x.VaultEntityId).ToArray();
        var nonFungibleIdsLimit = _endpointConfiguration.Value.DefaultPageSize;

        Dictionary<long, List<NonFungibleIdWithOwnerDataViewModel>>? nonFungibleIdsAndOneMorePerVault = null;

        if (optIns.NonFungibleIncludeNfids && vaultEntityIdsToQuery.Any())
        {
            var lookup = vaultEntityIdsToQuery.Select(vaultId => new NonFungibleIdOwnerLookup(entity.Id, resourceEntity.Id, vaultId)).ToArray();

            var nonFungibleIdsAndOneMore = await GetNonFungibleIdsFirstPageAndOneMore(
                lookup.Select(x => x.EntityId).ToArray(),
                lookup.Select(x => x.ResourceEntityId).ToArray(),
                lookup.Select(x => x.VaultEntityId).ToArray(),
                nonFungibleIdsLimit,
                ledgerState,
                token);

            nonFungibleIdsAndOneMorePerVault = nonFungibleIdsAndOneMore
                .GroupBy(x => x.VaultEntityId)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        var mapped = nonFungibles
            .Select(x =>
                {
                    List<string>? items = null;
                    string? nextCursor = null;

                    if (nonFungibleIdsAndOneMorePerVault?.TryGetValue(x.VaultEntityId, out var nfids) == true)
                    {
                        items = nfids.Take(nonFungibleIdsLimit).Select(y => y.NonFungibleId).ToList();
                        nextCursor = CursorGenerator.GenerateOffsetCursor(0, nonFungibleIdsLimit, x.NonFungibleIdsCount);
                    }

                    return new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                        totalCount: x.NonFungibleIdsCount,
                        nextCursor: nextCursor,
                        items: items,
                        vaultAddress: x.VaultAddress,
                        lastUpdatedAtStateVersion: x.LastUpdatedAtStateVersion
                    );
                }
            )
            .ToList();

        var vaultsTotalCount = nonFungibles.FirstOrDefault()?.VaultTotalCount ?? 0;
        var nextCursor = CursorGenerator.GenerateOffsetCursor(request.Offset, request.Limit, vaultsTotalCount);

        return new GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse(ledgerState, vaultsTotalCount, nextCursor, mapped, entity.Address, resourceEntity.Address);
    }

    public async Task<GatewayModel.StateEntityMetadataPageResponse> EntityMetadata(
        IEntityStateQuerier.PageRequest request,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<Entity>(request.Address, ledgerState, token);
        GatewayModel.EntityMetadataCollection metadata;

        if (entity is VirtualIdentityEntity or VirtualAccountComponentEntity)
        {
            var (_, virtualEntityMetadata) = await _virtualEntityDataProvider.GetVirtualEntityData(entity.Address);
            metadata = virtualEntityMetadata;
        }
        else
        {
            metadata = (await GetMetadataSlices(new[] { entity.Id }, request.Offset, request.Limit, ledgerState, token))[entity.Id];
        }

        return new GatewayModel.StateEntityMetadataPageResponse(ledgerState, metadata.TotalCount, metadata.NextCursor, metadata.Items, entity.Address);
    }

    public async Task<GatewayModel.StateEntitySchemaPageResponse?> EntitySchema(IEntityStateQuerier.PageRequest pageRequest, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<Entity>(pageRequest.Address, ledgerState, token);
        var packageSchemaHistory = await GetEntitySchemaHistory(new[] { entity.Id }, pageRequest.Offset, pageRequest.Limit, ledgerState, token);

        if (!packageSchemaHistory.TryGetValue(entity.Id, out var packageSchemas))
        {
            return null;
        }

        var totalCount = packageSchemas.FirstOrDefault()?.TotalCount ?? 0;

        return new GatewayModel.StateEntitySchemaPageResponse(
            ledgerState: ledgerState,
            address: entity.Address,
            totalCount: totalCount,
            nextCursor: CursorGenerator.GenerateOffsetCursor(pageRequest.Offset, pageRequest.Limit, totalCount),
            items: packageSchemas.Select(pb => pb.ToGatewayModel()).ToList());
    }

    public async Task<GatewayModel.StateEntityFungiblesPageResponse> EntityFungibleResourcesPage(
        IEntityStateQuerier.PageRequest2 pageRequest,
        bool aggregatePerVault,
        GatewayModel.StateEntityFungiblesPageRequestOptIns optIns,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(pageRequest.Address, ledgerState, token);

        var defaultPageSize = _endpointConfiguration.Value.DefaultPageSize;
        var qc = new EntityResourcesPageQuery.ResourcesPageQueryConfiguration(defaultPageSize, defaultPageSize, true, pageRequest.Cursor, ledgerState.StateVersion);
        var entityResources = await EntityResourcesPageQuery.FungibleResourcesPage(_dbContext, _dapperWrapper, entity.Id, qc, token);

        if (entityResources == null)
        {
            throw new Exception("not found");
        }

        Dictionary<long, GatewayModel.EntityMetadataCollection>? explicitMetadata = null;

        if (optIns.ExplicitMetadata?.Any() == true)
        {
            var entityAndResourceIds = entityResources.Resources.Values
                .Select(r => r.ResourceEntityId)
                .Union(new[] { entity.Id })
                .ToHashSet()
                .ToArray();

            explicitMetadata = optIns.ExplicitMetadata?.Any() == true
                ? await GetExplicitMetadata(entityAndResourceIds, optIns.ExplicitMetadata.ToArray(), ledgerState, token)
                : null;
        }

        var mapped = TmpMapper.Map(entityResources, explicitMetadata, aggregatePerVault, defaultPageSize, 0, defaultPageSize).Fungibles;

        return new GatewayModel.StateEntityFungiblesPageResponse(ledgerState, mapped.TotalCount, mapped.NextCursor, mapped.Items, pageRequest.Address);
    }

    public async Task<GatewayModel.StateEntityFungibleResourceVaultsPageResponse> EntityFungibleResourceVaults(
        IEntityStateQuerier.ResourceVaultsPageRequest2 request,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await GetEntity<GlobalFungibleResourceEntity>(request.ResourceAddress, ledgerState, token);

        var defaultPageSize = _endpointConfiguration.Value.DefaultPageSize;
        var qc = new EntityResourcesPageQuery.VaultsPageQueryConfiguration(defaultPageSize, true, request.Cursor, ledgerState.StateVersion);
        var entityResources = await EntityResourcesPageQuery.FungibleResourceVaultsPage(_dbContext, _dapperWrapper, entity.Id, resourceEntity.Id, qc, token);

        if (entityResources == null)
        {
            throw new Exception("not found");
        }

        var mapped = TmpMapper.Map(entityResources, null, true, 1, 0, defaultPageSize).Fungibles.Items.First();
        var typed = (GatewayModel.FungibleResourcesCollectionItemVaultAggregated)mapped;
        var m = typed.Vaults;

        return new GatewayModel.StateEntityFungibleResourceVaultsPageResponse(ledgerState, m.TotalCount, m.NextCursor, m.Items, entity.Address, resourceEntity.Address);
    }

    public async Task<GatewayModel.StateEntityNonFungibleIdsPageResponse> EntityNonFungibleIds(
        IEntityStateQuerier.PageRequest request,
        EntityAddress resourceAddress,
        EntityAddress vaultAddress,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await GetEntity<GlobalNonFungibleResourceEntity>(resourceAddress, ledgerState, token);
        var vaultEntity = await GetEntity<VaultEntity>(vaultAddress, ledgerState, token);
        var nonFungibleIds = await GetNonFungibleIdsSlice(entity.Id, resourceEntity.Id, vaultEntity.Id, request.Offset, request.Limit, ledgerState, token);

        return new GatewayModel.StateEntityNonFungibleIdsPageResponse(ledgerState, nonFungibleIds.TotalCount, nonFungibleIds.NextCursor, nonFungibleIds.Items, entity.Address, resourceEntity.Address);
    }

    public async Task<GatewayModel.StateNonFungibleIdsResponse> NonFungibleIds(
        EntityAddress nonFungibleResourceAddress,
        GatewayModel.LedgerState ledgerState,
        GatewayModel.IdBoundaryCoursor? cursor,
        int pageSize,
        CancellationToken token = default)
    {
        var entity = await GetEntity<GlobalNonFungibleResourceEntity>(nonFungibleResourceAddress, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
SELECT
    d.id AS Id,
    d.from_state_version AS FromStateVersion,
    d.non_fungible_id AS NonFungibleId
FROM non_fungible_id_definition d
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
ORDER BY d.from_state_version ASC, d.id ASC
LIMIT @limit
;",
            parameters: new
            {
                nonFungibleResourceEntityId = entity.Id,
                stateVersion = ledgerState.StateVersion,
                cursorStateVersion = cursor?.StateVersionBoundary ?? 1,
                cursorId = cursor?.IdBoundary ?? 1,
                limit = pageSize + 1,
            },
            cancellationToken: token);

        var entriesAndOneMore = (await _dapperWrapper.QueryAsync<NonFungibleIdsViewModel>(_dbContext.Database.GetDbConnection(), cd))
            .ToList();

        var nextCursor = entriesAndOneMore.Count == pageSize + 1
            ? new GatewayModel.IdBoundaryCoursor(entriesAndOneMore.Last().FromStateVersion, entriesAndOneMore.Last().Id).ToCursorString()
            : null;

        var resourceSupplyData = await GetResourceSupplyData(entity.Id, ledgerState, token);
        long totalCount = long.Parse(resourceSupplyData.TotalMinted.ToString());

        var items = entriesAndOneMore
            .Take(pageSize)
            .Select(vm => vm.NonFungibleId)
            .ToList();

        return new GatewayModel.StateNonFungibleIdsResponse(
            ledgerState: ledgerState,
            resourceAddress: nonFungibleResourceAddress,
            nonFungibleIds: new GatewayModel.NonFungibleIdsCollection(
                totalCount: totalCount,
                nextCursor: nextCursor,
                items: items));
    }

    public async Task<GatewayModel.StateNonFungibleDataResponse> NonFungibleIdData(
        EntityAddress resourceAddress,
        IList<string> nonFungibleIds,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await GetEntity<GlobalNonFungibleResourceEntity>(resourceAddress, ledgerState, token);

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
                entityId = entity.Id,
            },
            cancellationToken: token);

        var nonFungibleDataSchema = await _dapperWrapper.QueryFirstOrDefaultAsync<NonFungibleDataSchemaModel>(
            _dbContext.Database.GetDbConnection(), nonFungibleDataSchemaQuery, "GetNonFungibleDataSchema"
        );

        if (nonFungibleDataSchema == null)
        {
            throw new UnreachableException("No schema found for nonfungible resource: {resourceAddress}");
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
                entityId = entity.Id,
                nonFungibleIds = nonFungibleIds,
            },
            cancellationToken: token);

        var items = new List<GatewayModel.StateNonFungibleDetailsResponseItem>();

        var result = await _dapperWrapper.QueryAsync<NonFungibleIdDataViewModel>(
            _dbContext.Database.GetDbConnection(),
            cd,
            "GetNonFungibleData");

        foreach (var vm in result)
        {
            var programmaticJson = !vm.IsDeleted
                ? ScryptoSborUtils.DataToProgrammaticJson(vm.Data, nonFungibleDataSchema.Schema,
                    nonFungibleDataSchema.SborTypeKind, nonFungibleDataSchema.TypeIndex, (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id)
                : null;

            items.Add(new GatewayModel.StateNonFungibleDetailsResponseItem(
                nonFungibleId: vm.NonFungibleId,
                isBurned: vm.IsDeleted,
                data: !vm.IsDeleted ? new GatewayModel.ScryptoSborValue(vm.Data.ToHex(), programmaticJson) : null,
                lastUpdatedAtStateVersion: vm.DataLastUpdatedAtStateVersion));
        }

        return new GatewayModel.StateNonFungibleDataResponse(
            ledgerState: ledgerState,
            resourceAddress: resourceAddress.ToString(),
            nonFungibleIdType: entity.NonFungibleIdType.ToGatewayModel(),
            nonFungibleIds: items);
    }

    public async Task<GatewayModel.StateNonFungibleLocationResponse> NonFungibleIdLocation(
        EntityAddress resourceAddress,
        IList<string> nonFungibleIds,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var resourceEntity = await GetEntity<GlobalNonFungibleResourceEntity>(resourceAddress, ledgerState, token);

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

        var vaultLocationResults = (await _dapperWrapper.QueryAsync<NonFungibleIdLocationViewModel>(_dbContext.Database.GetDbConnection(), vaultLocationsCd))
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

        var vaultAncestorResults = (await _dapperWrapper.QueryAsync<NonFungibleIdLocationVaultOwnerViewModel>(_dbContext.Database.GetDbConnection(), vaultAncestorsCd))
            .ToDictionary(e => e.VaultId);

        return new GatewayModel.StateNonFungibleLocationResponse(
            ledgerState: ledgerState,
            resourceAddress: resourceAddress.ToString(),
            nonFungibleIds: vaultLocationResults
                .Select(x => new GatewayModel.StateNonFungibleLocationResponseItem(
                    nonFungibleId: x.NonFungibleId,
                    owningVaultAddress: !x.IsDeleted ? x.OwnerVaultAddress : null,
                    owningVaultParentAncestorAddress: !x.IsDeleted ? vaultAncestorResults[x.OwnerVaultId].VaultParentAncestorAddress : null,
                    owningVaultGlobalAncestorAddress: !x.IsDeleted ? vaultAncestorResults[x.OwnerVaultId].VaultGlobalAncestorAddress : null,
                    isBurned: x.IsDeleted,
                    lastUpdatedAtStateVersion: x.FromStateVersion
                ))
                .ToList());
    }

    public async Task<GatewayModel.StateValidatorsListResponse> StateValidatorsList(
        GatewayModel.StateValidatorsListCursor? cursor,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var validatorsPageSize = _endpointConfiguration.Value.ValidatorsPageSize;
        var idBoundary = cursor?.IdBoundary ?? 0;

        var validatorsAndOneMore = await _dbContext
            .Entities
            .OfType<GlobalValidatorEntity>()
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .Where(e => e.Id >= idBoundary)
            .OrderBy(e => e.Id)
            .Take(validatorsPageSize + 1)
            .AnnotateMetricName("GetValidators")
            .ToListAsync(token);

        var lastFinishedEpoch = await _dbContext
            .ValidatorActiveSetHistory
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .OrderByDescending(e => e.FromStateVersion)
            .Take(1)
            .Select(e => e.Epoch)
            .FirstOrDefaultAsync(token);

        if (lastFinishedEpoch == 0)
        {
            return new GatewayModel.StateValidatorsListResponse(ledgerState, new GatewayModel.ValidatorCollection(0, null, new List<GatewayModel.ValidatorCollectionItem>()));
        }

        var activeSetById = await _dbContext
            .ValidatorActiveSetHistory
            .Include(e => e.PublicKey)
            .Where(e => e.Epoch == lastFinishedEpoch)
            .AnnotateMetricName("GetValidatorActiveSet")
            .ToDictionaryAsync(e => e.PublicKey.ValidatorEntityId, token);

        var totalStake = activeSetById
            .Values
            .Select(asv => asv.Stake)
            .Aggregate(TokenAmount.Zero, (current, x) => current + x);

        var validatorIds = validatorsAndOneMore.Take(validatorsPageSize).Select(e => e.Id).ToArray();
        var validatorVaultIds = validatorsAndOneMore
            .Take(validatorsPageSize)
            .Aggregate(new List<long>(), (aggregated, validator) =>
            {
                aggregated.Add(validator.GetStakeVaultEntityId());
                aggregated.Add(validator.GetPendingXrdWithdrawVaultEntityId());
                aggregated.Add(validator.GetLockedOwnerStakeUnitVaultEntityId());
                aggregated.Add(validator.GetPendingOwnerStakeUnitUnlockVaultEntityId());

                return aggregated;
            })
            .ToList();

        var stateHistory = await _dbContext
            .StateHistory
            .FromSqlInterpolated($@"
WITH variables (validator_entity_id) AS (SELECT UNNEST({validatorIds}))
SELECT esh.*
FROM variables v
INNER JOIN LATERAL (
    SELECT *
    FROM state_history
    WHERE entity_id = v.validator_entity_id AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) esh ON true")
            .Cast<JsonStateHistory>()
            .ToDictionaryAsync(e => e.EntityId, token);

        var vaultHistory = await _dbContext
            .EntityVaultHistory
            .FromSqlInterpolated($@"
WITH variables (vault_entity_id) AS (SELECT UNNEST({validatorVaultIds}))
SELECT evh.*
FROM variables v
INNER JOIN LATERAL (
    SELECT *
    FROM entity_vault_history
    WHERE vault_entity_id = v.vault_entity_id AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) evh ON true")
            .Cast<EntityFungibleVaultHistory>()
            .ToDictionaryAsync(e => e.VaultEntityId, token);

        var vaultAddresses = await _dbContext
            .Entities
            .Where(e => validatorVaultIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Address })
            .AnnotateMetricName("GetVaultAddresses")
            .ToDictionaryAsync(e => e.Id, e => e.Address, token);

        var metadataById = await GetMetadataSlices(validatorIds, 0, _endpointConfiguration.Value.DefaultPageSize, ledgerState, token);

        var items = validatorsAndOneMore
            .Take(validatorsPageSize)
            .Select(v =>
            {
                GatewayModel.ValidatorCollectionItemActiveInEpoch? activeInEpoch = null;

                if (activeSetById.TryGetValue(v.Id, out var validatorActiveSetHistory))
                {
                    var stake = validatorActiveSetHistory.Stake.ToString();
                    var stakePercentage = (validatorActiveSetHistory.Stake * _tokenAmount100 / totalStake).ToString();

                    activeInEpoch = new GatewayModel.ValidatorCollectionItemActiveInEpoch(
                        stake,
                        double.Parse(stakePercentage, NumberFormatInfo.InvariantInfo),
                        validatorActiveSetHistory.PublicKey.ToGatewayPublicKey());
                }

                var stakeVault = vaultHistory[v.GetStakeVaultEntityId()];
                var pendingXrdWithdrawVaultVault = vaultHistory[v.GetPendingXrdWithdrawVaultEntityId()];
                var lockedOwnerStakeUnitVault = vaultHistory[v.GetLockedOwnerStakeUnitVaultEntityId()];
                var pendingOwnerStakeUnitUnlockVault = vaultHistory[v.GetPendingOwnerStakeUnitUnlockVaultEntityId()];
                var effectiveFeeFactor = ValidatorEffectiveFeeFactorProvider.ExtractFeeFactorFromValidatorState(stateHistory[v.Id].JsonState, ledgerState.Epoch);

                return new GatewayModel.ValidatorCollectionItem(
                    v.Address,
                    new GatewayModel.ValidatorVaultItem(stakeVault.Balance.ToString(), stakeVault.FromStateVersion, vaultAddresses[stakeVault.VaultEntityId]),
                    new GatewayModel.ValidatorVaultItem(pendingXrdWithdrawVaultVault.Balance.ToString(), pendingXrdWithdrawVaultVault.FromStateVersion, vaultAddresses[pendingXrdWithdrawVaultVault.VaultEntityId]),
                    new GatewayModel.ValidatorVaultItem(lockedOwnerStakeUnitVault.Balance.ToString(), lockedOwnerStakeUnitVault.FromStateVersion, vaultAddresses[lockedOwnerStakeUnitVault.VaultEntityId]),
                    new GatewayModel.ValidatorVaultItem(pendingOwnerStakeUnitUnlockVault.Balance.ToString(), pendingOwnerStakeUnitUnlockVault.FromStateVersion, vaultAddresses[pendingOwnerStakeUnitUnlockVault.VaultEntityId]),
                    new JRaw(stateHistory[v.Id].JsonState),
                    activeInEpoch,
                    metadataById[v.Id],
                    effectiveFeeFactor
                );
            })
            .ToList();

        var nextCursor = validatorsAndOneMore.Count == validatorsPageSize + 1
            ? new GatewayModel.StateValidatorsListCursor(validatorsAndOneMore.Last().Id).ToCursorString()
            : null;

        return new GatewayModel.StateValidatorsListResponse(ledgerState, new GatewayModel.ValidatorCollection(null, nextCursor, items));
    }

    private async Task<List<RoyaltyVaultBalanceViewModel>> GetRoyaltyVaultBalances(long[] ownerIds, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
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

        var result = await _dapperWrapper.QueryAsync<RoyaltyVaultBalanceViewModel>(_dbContext.Database.GetDbConnection(), cd);
        return result.ToList();
    }

    private async Task<Dictionary<long, GatewayModel.EntityMetadataCollection>> GetMetadataSlices(
        long[] entityIds,
        int offset,
        int limit,
        GatewayModel.LedgerState ledgerState,
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
        SELECT metadata_ids[@startIndex:@endIndex] AS metadata_slice, cardinality(metadata_ids) AS metadata_total_count
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
                startIndex = offset + 1,
                endIndex = offset + limit,
            },
            cancellationToken: token);

        foreach (var vm in await _dapperWrapper.QueryAsync<MetadataViewModel>(_dbContext.Database.GetDbConnection(), cd))
        {
            if (!result.ContainsKey(vm.EntityId))
            {
                result[vm.EntityId] = new GatewayModel.EntityMetadataCollection(vm.TotalCount, CursorGenerator.GenerateOffsetCursor(offset, limit, vm.TotalCount), new List<GatewayModel.EntityMetadataItem>());
            }

            var networkId = (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id;
            var value = ScryptoSborUtils.DecodeToGatewayMetadataItemValue(vm.Value, networkId);
            var programmaticJson = ScryptoSborUtils.DataToProgrammaticJson(vm.Value, networkId);
            var entityMetadataItemValue = new GatewayModel.EntityMetadataItemValue(vm.Value.ToHex(), programmaticJson, value);

            result[vm.EntityId].Items.Add(new GatewayModel.EntityMetadataItem(vm.Key, entityMetadataItemValue, vm.IsLocked, vm.FromStateVersion));
        }

        foreach (var missing in entityIds.Except(result.Keys))
        {
            result[missing] = GatewayModel.EntityMetadataCollection.Empty;
        }

        return result;
    }

    private async Task<Dictionary<long, GatewayModel.EntityMetadataCollection>> GetExplicitMetadata(
        long[] entityIds,
        string[] metadataKeys,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token)
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

        var metadataHistory = await _dbContext
            .EntityMetadataHistory
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
            .AnnotateMetricName()
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

            var networkId = (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id;
            var value = ScryptoSborUtils.DecodeToGatewayMetadataItemValue(mh.Value, networkId);
            var programmaticJson = ScryptoSborUtils.DataToProgrammaticJson(mh.Value, networkId);
            var entityMetadataItemValue = new GatewayModel.EntityMetadataItemValue(mh.Value.ToHex(), programmaticJson, value);

            result[mh.EntityId].Items.Add(new GatewayModel.EntityMetadataItem(mh.Key, entityMetadataItemValue, mh.IsLocked, mh.FromStateVersion));
            result[mh.EntityId].TotalCount = result[mh.EntityId].TotalCount.HasValue ? result[mh.EntityId].TotalCount + 1 : 1;
        }

        foreach (var missing in entityIds.Except(result.Keys))
        {
            result[missing] = GatewayModel.EntityMetadataCollection.Empty;
        }

        return result;
    }

    private async Task<IDictionary<long, SchemaEntryViewModel[]>> GetEntitySchemaHistory(long[] entityIds, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        if (!entityIds.Any())
        {
            return ImmutableDictionary<long, SchemaEntryViewModel[]>.Empty;
        }

        var cd = new CommandDefinition(
            commandText: @"
WITH variables (entity_id) AS (SELECT UNNEST(@entityIds)),
schema_slices AS
(
    SELECT *
    FROM variables var
    INNER JOIN LATERAL (
        SELECT entity_id, entry_ids[@startIndex:@endIndex] AS schema_slice, cardinality(entry_ids) AS total_count
        FROM schema_entry_aggregate_history
        WHERE entity_id = var.entity_id AND from_state_version <= @stateVersion
        ORDER BY from_state_version DESC
        LIMIT 1
    ) pbah ON TRUE
)
SELECT sed.*, ss.total_count
FROM schema_slices AS ss
INNER JOIN LATERAL UNNEST(schema_slice) WITH ORDINALITY AS schema_join(id, ordinality) ON TRUE
INNER JOIN schema_entry_definition sed ON sed.id = schema_join.id
ORDER BY schema_join.ordinality ASC;",
            parameters: new
            {
                entityIds = entityIds.ToList(),
                startIndex = offset + 1,
                endIndex = offset + limit,
                stateVersion = ledgerState.StateVersion,
            },
            cancellationToken: token);

        return (await _dbContext.Database.GetDbConnection().QueryAsync<SchemaEntryViewModel>(cd))
            .ToList()
            .GroupBy(b => b.EntityId)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    private async Task<ResourceEntitySupplyHistory> GetResourceSupplyData(long entityId, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var response = await GetResourcesSupplyData(new[] { entityId }, ledgerState, token);
        return response[entityId];
    }

    private async Task<Dictionary<long, ResourceEntitySupplyHistory>> GetResourcesSupplyData(long[] entityIds, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        if (!entityIds.Any())
        {
            return new Dictionary<long, ResourceEntitySupplyHistory>();
        }

        var result = await _dbContext
            .ResourceEntitySupplyHistory
            .FromSqlInterpolated($@"
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
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.ResourceEntityId, token);

        foreach (var missing in entityIds.Except(result.Keys))
        {
            result[missing] = ResourceEntitySupplyHistory.Empty;
        }

        return result;
    }

    private async Task<TEntity> GetEntity<TEntity>(EntityAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity
    {
        var entity = await _dbContext
            .Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .AnnotateMetricName()
            .FirstOrDefaultAsync(e => e.Address == address, token);

        if (entity == null)
        {
            entity = await TryResolveAsVirtualEntity(address);

            if (entity == null)
            {
                // TODO this method should return null/throw on missing, virtual component handling should be done upstream to avoid entity.Id = 0 uses, see https://github.com/radixdlt/babylon-gateway/pull/171#discussion_r1111957627
                throw new EntityNotFoundException(address.ToString());
            }
        }

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidEntityException(address.ToString());
        }

        return typedEntity;
    }

    private async Task<Entity?> TryResolveAsVirtualEntity(EntityAddress address)
    {
        if (await _virtualEntityDataProvider.IsVirtualAccountAddress(address))
        {
            return new VirtualAccountComponentEntity(address);
        }

        if (await _virtualEntityDataProvider.IsVirtualIdentityAddress(address))
        {
            return new VirtualIdentityEntity(address);
        }

        return null;
    }

    private async Task<ICollection<Entity>> GetEntities(List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var entities = await _dbContext
            .Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion && addresses.Contains(e.Address))
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Address, token);

        foreach (var address in addresses.Except(entities.Keys))
        {
            var virtualEntity = await TryResolveAsVirtualEntity(address);

            if (virtualEntity != null)
            {
                entities.Add(virtualEntity.Address, virtualEntity);
            }
        }

        return entities.Values;
    }

    private async Task<Dictionary<long, string>> GetStateHistory(ICollection<ComponentEntity> componentEntities, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var lookup = new HashSet<long>();

        foreach (var componentEntity in componentEntities)
        {
            lookup.Add(componentEntity.Id);
        }

        var result = new Dictionary<long, string>();

        if (!lookup.Any())
        {
            return result;
        }

        var entityIds = lookup.ToList();

        var states = await _dbContext
            .StateHistory
            .FromSqlInterpolated($@"
WITH variables (entity_id) AS (SELECT UNNEST({entityIds}))
SELECT esh.*
FROM variables v
INNER JOIN LATERAL (
    SELECT *
    FROM state_history
    WHERE entity_id = v.entity_id AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) esh ON TRUE;")
            .AnnotateMetricName("GetStateHistory")
            .ToListAsync(token);

        var schemasToLoad = states
            .OfType<SborStateHistory>()
            .Select(x => new SchemaIdentifier(x.SchemaHash, x.SchemaDefiningEntityId))
            .Distinct()
            .ToArray();

        var schemas = new Dictionary<SchemaIdentifier, byte[]>();

        if (schemasToLoad.Any())
        {
            var schemaEntityIds = schemasToLoad.Select(x => x.EntityId).ToArray();
            var schemaHashes = schemasToLoad.Select(x => (byte[])x.SchemaHash).ToArray();

            schemas = await _dbContext
                .SchemaEntryDefinition
                .FromSqlInterpolated($@"
SELECT *
FROM schema_entry_definition
WHERE (entity_id, schema_hash) IN (SELECT UNNEST({schemaEntityIds}), UNNEST({schemaHashes}))")
                .AnnotateMetricName("GetSchemas")
                .ToDictionaryAsync(
                    x => new SchemaIdentifier(x.SchemaHash, x.EntityId),
                    x => x.Schema,
                    token);
        }

        foreach (var state in states)
        {
            switch (state)
            {
                case JsonStateHistory jsonStateHistory:
                    result.Add(state.EntityId, jsonStateHistory.JsonState);
                    break;
                case SborStateHistory sborStateHistory:
                {
                    var schemaIdentifier = new SchemaIdentifier((ValueBytes)sborStateHistory.SchemaHash, sborStateHistory.SchemaDefiningEntityId);
                    var schemaFound = schemas.TryGetValue(schemaIdentifier, out var schemaBytes);

                    if (!schemaFound)
                    {
                        throw new UnreachableException(
                            $"schema not found for entity :{sborStateHistory.EntityId} with schema defining entity id: {sborStateHistory.SchemaDefiningEntityId} and schema hash: {sborStateHistory.SchemaHash.ToHex()}");
                    }

                    var jsonState = ScryptoSborUtils.DataToProgrammaticJsonString(
                        sborStateHistory.SborState,
                        schemaBytes!,
                        sborStateHistory.SborTypeKind,
                        sborStateHistory.TypeIndex,
                        (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id);

                    result.Add(state.EntityId, jsonState);
                    break;
                }
            }
        }

        return result;
    }

    private async Task<Dictionary<long, ComponentMethodRoyaltyEntryHistory[]>> GetComponentRoyaltyConfigs(long[] componentEntityIds, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        if (!componentEntityIds.Any())
        {
            return new Dictionary<long, ComponentMethodRoyaltyEntryHistory[]>();
        }

        return (await _dbContext
                .ComponentEntityMethodRoyaltyEntryHistory
                .FromSqlInterpolated($@"
WITH variables (component_entity_id) AS (
   SELECT UNNEST({componentEntityIds})
),
aggregates AS (
   SELECT cmrah.*
   FROM variables
   INNER JOIN LATERAL (
        SELECT *
        FROM component_method_royalty_aggregate_history
        WHERE entity_id = variables.component_entity_id AND from_state_version <= {ledgerState.StateVersion}
        ORDER BY from_state_version DESC
        LIMIT 1
   ) cmrah ON TRUE
)
SELECT cmreh.*
FROM aggregates
INNER JOIN LATERAL UNNEST(entry_ids) WITH ORDINALITY AS component_method_royalty_join(id, ordinality) ON TRUE
INNER JOIN component_method_royalty_entry_history cmreh ON cmreh.id = component_method_royalty_join.id
ORDER BY component_method_royalty_join.ordinality ASC;")
                .AnnotateMetricName()
                .ToListAsync(token))
            .GroupBy(b => b.EntityId)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    private async Task<Dictionary<long, EntityAddress>> GetCorrelatedEntityAddresses(
        ICollection<Entity> entities,
        IDictionary<long, PackageBlueprintViewModel[]> packageBlueprints,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var lookup = new HashSet<long>();

        foreach (var entity in entities)
        {
            lookup.Add(entity.Id);

            if (entity.HasParent)
            {
                lookup.Add(entity.ParentAncestorId.Value);
                lookup.Add(entity.OwnerAncestorId.Value);
                lookup.Add(entity.GlobalAncestorId.Value);
            }

            if (entity.TryGetCorrelation(EntityRelationship.ComponentToInstantiatingPackage, out var packageCorrelation))
            {
                lookup.Add(packageCorrelation.EntityId);
            }

            if (entity is VaultEntity vaultEntity)
            {
                lookup.Add(vaultEntity.GetResourceEntityId());
            }
        }

        foreach (var dependantEntityId in packageBlueprints.Values.SelectMany(x => x).SelectMany(x => x.DependantEntityIds?.ToArray() ?? Array.Empty<long>()))
        {
            lookup.Add(dependantEntityId);
        }

        var ids = lookup.ToList();

        return await _dbContext
            .Entities
            .Where(e => ids.Contains(e.Id) && e.FromStateVersion <= ledgerState.StateVersion)
            .Select(e => new { e.Id, e.Address })
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Id, e => e.Address, token);
    }

    private async Task<Dictionary<EntityAddress, long>> ResolveResourceEntityIds(
        ICollection<GatewayModel.FungibleResourcesCollection>? fungibleResources,
        ICollection<GatewayModel.NonFungibleResourcesCollection>? nonFungibleResources,
        CancellationToken token)
    {
        if (fungibleResources?.Any() != true && nonFungibleResources?.Any() != true)
        {
            return new Dictionary<EntityAddress, long>();
        }

        var lookupAddresses = new HashSet<string>();

        fungibleResources?.SelectMany(fr => fr.Items).Select(i => i.ResourceAddress).ForEach(a => lookupAddresses.Add(a));
        nonFungibleResources?.SelectMany(nfr => nfr.Items).Select(i => i.ResourceAddress).ForEach(a => lookupAddresses.Add(a));

        var addresses = lookupAddresses.ToList();

        return await _dbContext
            .Entities
            .Where(e => addresses.Contains(e.Address))
            .Select(e => new { e.Id, e.Address })
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Address, e => e.Id, token);
    }
}
