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
using RadixDlt.NetworkGateway.Abstractions.StandardMetadata;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class EntityStateQuerier : IEntityStateQuerier
{
    private record struct SchemaIdentifier(ValueBytes SchemaHash, long EntityId);

    private class SchemaEntryViewModel : SchemaEntryDefinition
    {
        public int TotalCount { get; set; }
    }

    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IOptionsSnapshot<EndpointOptions> _endpointConfiguration;
    private readonly ReadOnlyDbContext _dbContext;
    private readonly IPreAllocatedEntityDataProvider _preAllocatedEntityDataProvider;
    private readonly IRoleAssignmentQuerier _roleAssignmentQuerier;
    private readonly IDapperWrapper _dapperWrapper;
    private readonly IEntityQuerier _entityQuerier;

    public EntityStateQuerier(
        INetworkConfigurationProvider networkConfigurationProvider,
        ReadOnlyDbContext dbContext,
        IOptionsSnapshot<EndpointOptions> endpointConfiguration,
        IPreAllocatedEntityDataProvider preAllocatedEntityDataProvider,
        IRoleAssignmentQuerier roleAssignmentQuerier,
        IDapperWrapper dapperWrapper,
        IEntityQuerier entityQuerier)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
        _dbContext = dbContext;
        _endpointConfiguration = endpointConfiguration;
        _preAllocatedEntityDataProvider = preAllocatedEntityDataProvider;
        _roleAssignmentQuerier = roleAssignmentQuerier;
        _dapperWrapper = dapperWrapper;
        _entityQuerier = entityQuerier;
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
        var networkConfiguration = _networkConfigurationProvider.GetNetworkConfiguration();

        var entities = await _entityQuerier.GetEntities(addresses, ledgerState, token);
        var persistedEntities = entities.Where(x => x.Id != default).ToList();
        var componentEntities = entities.OfType<ComponentEntity>().ToList();
        var resourceEntities = entities.OfType<ResourceEntity>().ToList();
        var packageEntities = entities.OfType<GlobalPackageEntity>().ToList();
        var persistedComponentEntities = componentEntities.Where(x => x.Id != default).ToList();
        var globalPersistedComponentEntities = persistedComponentEntities.Where(x => x.IsGlobal).ToList();

        // TODO ideally we'd like to run all those in parallel
        var metadata = await MetadataPagedQuery.Execute(
            _dbContext.Database.GetDbConnection(),
            _dapperWrapper,
            ledgerState,
            entities.Select(e => e.Id).ToArray(),
            new MetadataPagedQuery.QueryConfiguration
            {
                Cursor = null,
                PageSize = _endpointConfiguration.Value.DefaultPageSize,
                MaxDefinitionsLookupLimit = _endpointConfiguration.Value.MaxDefinitionsLookupLimit,
            },
            _networkConfigurationProvider.GetNetworkConfiguration().Id,
            token);

        var roleAssignmentsHistory = await _roleAssignmentQuerier.GetRoleAssignmentsHistory(globalPersistedComponentEntities, ledgerState, token);
        var resourcesSupplyData = await ResourceSupplyMultiLookupQuery.Execute(_dbContext, resourceEntities.Select(x => x.Id).ToArray(), ledgerState, token);
        var packageBlueprintHistory = await PackageQueries.PackageBlueprintHistoryMultiLookup(_dbContext, packageEntities.Select(e => e.Id).ToArray(), 0, packagePageSize, ledgerState, token);
        var packageCodeHistory = await PackageQueries.PackageCodeHistoryMultiLookup(_dbContext, packageEntities.Select(e => e.Id).ToArray(), 0, packagePageSize, ledgerState, token);
        var packageSchemaHistory = await GetEntitySchemaHistory(packageEntities.Select(e => e.Id).ToArray(), 0, packagePageSize, ledgerState, token);

        var instantiatingPackageDictionary = persistedEntities
            .ToDictionary(
            x => x.Id,
            x =>
            {
                x.TryGetCorrelation(EntityRelationship.ComponentToInstantiatingPackage, out var correlation);
                return correlation?.EntityId;
            }
        );
        var instantiatingPackageIds = instantiatingPackageDictionary.Values.Where(x => x.HasValue).Select(x => x!.Value).ToHashSet().ToArray();

        var entityIdsToResolveTwoWayLinks = persistedEntities.Select(x => x.Id).Union(instantiatingPackageIds).ToHashSet().ToArray();
        var resolvedTwoWayLinks = optIns.DappTwoWayLinks
            ? await new StandardMetadataResolver(_dbContext, _dapperWrapper).ResolveTwoWayLinks(entityIdsToResolveTwoWayLinks, true, ledgerState, token)
            : ImmutableDictionary<EntityAddress, ICollection<ResolvedTwoWayLink>>.Empty;
        var resolvedNativeResourceDetails = optIns.NativeResourceDetails
            ? await new NativeResourceDetailsResolver(_dbContext, _dapperWrapper, networkConfiguration).GetNativeResourceDetails(entities, ledgerState, token)
            : ImmutableDictionary<EntityAddress, GatewayModel.NativeResourceDetails>.Empty;

        var correlatedAddresses = await PackageQueries.GetCorrelatedEntityAddresses(_dbContext, entities, packageBlueprintHistory, ledgerState, token);

        // top-level & royalty vaults (if opted-in)
        var vaultIds = entities
            .OfType<VaultEntity>()
            .Select(x => x.Id)
            .ToHashSet();

        if (optIns.ComponentRoyaltyVaultBalance || optIns.PackageRoyaltyVaultBalance)
        {
            vaultIds.UnionWith(globalPersistedComponentEntities
                .Select(x => x.TryGetCorrelation(EntityRelationship.EntityToRoyaltyVault, out var royaltyVaultCorrelation) ? royaltyVaultCorrelation.EntityId : -1)
                .Where(x => x > 0));
        }

        var vaultBalances = await GetVaultBalances(vaultIds.ToArray(), ledgerState, token);
        var fungibleResourcesConfiguration =
            new EntityFungibleResourcesAndVaultsPagedQuery.DetailsQueryConfiguration(defaultPageSize, aggregatePerVault ? defaultPageSize : 0, ledgerState.StateVersion);
        var entityFungibleResources =
            await EntityFungibleResourcesAndVaultsPagedQuery.Details(_dbContext, _dapperWrapper, componentEntities.Select(e => e.Id).ToArray(), fungibleResourcesConfiguration, token);
        var nonFungibleResourcesConfiguration =
            new EntityNonFungibleResourcesAndVaultsPagedQuery.DetailsQueryConfiguration(defaultPageSize, aggregatePerVault ? defaultPageSize : 0, ledgerState.StateVersion);
        var entityNonFungibleResources =
            await EntityNonFungibleResourcesAndVaultsPagedQuery.Details(_dbContext, _dapperWrapper, componentEntities.Select(e => e.Id).ToArray(), nonFungibleResourcesConfiguration, token);

        var allEntitiesToQueryForMetadata = entityFungibleResources
            .Values
            .SelectMany(x => x.FungibleResources.Values)
            .Select(r => r.ResourceEntityId)
            .Union(
                entityNonFungibleResources
                    .Values
                    .SelectMany(x => x.NonFungibleResources.Values)
                    .Select(x => x.ResourceEntityId))
            .Union(entities.Select(e => e.Id))
            .ToHashSet()
            .ToArray();

        var allVaultsToQueryForContent = entities
            .OfType<InternalNonFungibleVaultEntity>()
            .Select(x => x.Id)
            .ToArray()
            .Union(
                entityNonFungibleResources
                    .Values
                    .SelectMany(x => x.NonFungibleResources.Values)
                    .SelectMany(x => x.Vaults.Values)
                    .Select(x => x.VaultEntityId)
                    .ToArray())
            .ToHashSet()
            .ToArray();

        var explicitMetadata = optIns.ExplicitMetadata?.Any() == true
            ? await MetadataMultiLookupQuery.Execute(
                _dbContext.Database.GetDbConnection(),
                _dapperWrapper,
                allEntitiesToQueryForMetadata,
                optIns.ExplicitMetadata.ToArray(),
                ledgerState,
                networkConfiguration.Id,
                token)
            : null;

        var nonFungibleVaultContents = optIns.NonFungibleIncludeNfids
            ? await NonFungibleVaultContentsPagedQuery.Execute(
                _dbContext,
                _dapperWrapper,
                ledgerState,
                allVaultsToQueryForContent,
                new NonFungibleVaultContentsPagedQuery.QueryConfiguration(null, defaultPageSize, _endpointConfiguration.Value.MaxDefinitionsLookupLimit), token)
            : null;

        // those collections do NOT support virtual entities, thus they cannot be used outside of entity type specific context (switch statement below and its case blocks)
        // virtual entities generate those on their own (dynamically generated information)
        var stateHistory = await GetStateHistory(persistedComponentEntities, ledgerState, token);
        var componentRoyaltyConfigs = globalPersistedComponentEntities.Any() && optIns.ComponentRoyaltyConfig
            ? await GetComponentRoyaltyConfigs(globalPersistedComponentEntities.Select(x => x.Id).ToArray(), ledgerState, token)
            : null;

        var dappDefinitionsLinkedEntitiesPackageAddresses = resolvedTwoWayLinks
            .Values
            .SelectMany(x => x)
            .OfType<DappClaimedEntityResolvedTwoWayLink>()
            .Where(x => x.EntityAddress.IsPackage)
            .Select(x => x.EntityAddress)
            .ToList();

        var dappDefinitionsLinkedEntitiesPackageIdsDictionary = await _entityQuerier.ResolveEntityIds(dappDefinitionsLinkedEntitiesPackageAddresses, token);

        var autoLinkBlueprintDictionary = await GetBlueprintLinkMetadata(
            instantiatingPackageIds.Union(dappDefinitionsLinkedEntitiesPackageIdsDictionary.Values).ToHashSet().ToArray(),
            ledgerState,
            networkConfiguration,
            token);

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
                    string? packageRoyaltyVaultBalance = null;
                    if (optIns.PackageRoyaltyVaultBalance && pe.TryGetCorrelation(EntityRelationship.EntityToRoyaltyVault, out var packageRoyaltyRelation) &&
                        vaultBalances.TryGetValue(packageRoyaltyRelation.EntityId, out var packageRoyaltyVault))
                    {
                        packageRoyaltyVaultBalance = packageRoyaltyVault.Balance.ToString();
                    }

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
                        blueprintCursor = GatewayModelExtensions.GenerateOffsetCursor(0, packagePageSize, blueprintTotalCount.Value);
                    }

                    if (packageCodeHistory.TryGetValue(pe.Id, out var packageCodes))
                    {
                        codeTotalCount = packageCodes.FirstOrDefault()?.TotalCount ?? 0;
                        codeItems.AddRange(packageCodes.Select(pc => pc.ToGatewayModel()));
                        codeCursor = GatewayModelExtensions.GenerateOffsetCursor(0, packagePageSize, codeTotalCount.Value);
                    }

                    if (packageSchemaHistory.TryGetValue(pe.Id, out var packageSchemas))
                    {
                        schemaTotalCount = packageSchemas.FirstOrDefault()?.TotalCount ?? 0;
                        schemaItems.AddRange(packageSchemas.Take(packagePageSize).Select(sh => sh.ToGatewayModel()));
                        schemaCursor = GatewayModelExtensions.GenerateOffsetCursor(0, packagePageSize, schemaTotalCount.Value);
                    }

                    details = new GatewayModel.StateEntityDetailsResponsePackageDetails(
                        vmType: codeItems[0].VmType,
                        codeHashHex: codeItems[0].CodeHashHex,
                        codeHex: codeItems[0].CodeHex,
                        codes: new GatewayModel.PackageCodeCollection(codeTotalCount, codeCursor, codeItems),
                        royaltyVaultBalance: packageRoyaltyVaultBalance,
                        blueprints: new GatewayModel.PackageBlueprintCollection(blueprintTotalCount, blueprintCursor, blueprintItems),
                        schemas: new GatewayModel.EntitySchemaCollection(schemaTotalCount, schemaCursor, schemaItems),
                        roleAssignments: roleAssignmentsHistory[pe.Id],
                        twoWayLinkedDappAddress: twoWayLinks?.OfType<DappDefinitionResolvedTwoWayLink>().FirstOrDefault()?.EntityAddress);
                    break;

                case PreAllocatedIdentityEntity:
                case PreAllocatedAccountComponentEntity:
                    var preAllocatedEntityData = _preAllocatedEntityDataProvider.GetPreAllocatedEntityData(entity.Address);

                    details = preAllocatedEntityData.Details;
                    metadata[entity.Id] = preAllocatedEntityData.Metadata;
                    break;

                case InternalFungibleVaultEntity ifve:
                    details = new GatewayModel.StateEntityDetailsResponseFungibleVaultDetails(
                        resourceAddress: correlatedAddresses[ifve.GetResourceEntityId()],
                        balance: new GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem(
                            vaultAddress: entity.Address,
                            amount: vaultBalances[entity.Id].Balance.ToString(),
                            lastUpdatedAtStateVersion: vaultBalances[entity.Id].FromStateVersion));
                    break;
                case InternalNonFungibleVaultEntity infve:
                    List<string>? nfItems = null;
                    string? nfNextCursor = null;

                    if (optIns.NonFungibleIncludeNfids && nonFungibleVaultContents?.TryGetValue(entity.Id, out var vaultContent) == true)
                    {
                        nfNextCursor = vaultContent.NextCursor;
                        nfItems = vaultContent.Items;
                    }

                    details = new GatewayModel.StateEntityDetailsResponseNonFungibleVaultDetails(
                        resourceAddress: correlatedAddresses[infve.GetResourceEntityId()],
                        balance: new GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem(
                            totalCount: long.Parse(vaultBalances[entity.Id].Balance.ToString()),
                            nextCursor: nfNextCursor,
                            items: nfItems,
                            vaultAddress: entity.Address,
                            lastUpdatedAtStateVersion: vaultBalances[entity.Id].FromStateVersion));
                    break;

                case ComponentEntity ce:
                    string? componentRoyaltyVaultBalance = null;
                    ComponentMethodRoyaltyEntryHistory[]? componentRoyaltyConfig = null;
                    GatewayModel.TwoWayLinkedDappOnLedgerDetails? dAppDefinitionLinkDetail = null;

                    var directlyLinkedDapp = twoWayLinks?.OfType<DappDefinitionResolvedTwoWayLink>().FirstOrDefault()?.EntityAddress;

                    var instantiatingPackageId = ce.GetInstantiatingPackageId();
                    var instantiatingPackageAddress = correlatedAddresses[instantiatingPackageId];
                    resolvedTwoWayLinks.TryGetValue(instantiatingPackageAddress, out var instantiatingPackageTwoWayLinks);
                    var twoWayLinkedBlueprints = autoLinkBlueprintDictionary[instantiatingPackageId];

                    var blueprintLinkedDapp = twoWayLinkedBlueprints?.Contains(ce.BlueprintName) == true
                        ? instantiatingPackageTwoWayLinks?.OfType<DappDefinitionResolvedTwoWayLink>().FirstOrDefault()?.EntityAddress
                        : null;

                    // The dApp is represented by a GlobalAccount.
                    // We try to resolve the dApp details only if the entity is an account as it is not possible for other entity types.
                    if (ce is GlobalAccountEntity)
                    {
                        var dAppDefinitionPartnerDapps = twoWayLinks?.OfType<DappDefinitionsResolvedTwoWayLink>().Select(x => x.ToGatewayModel()).ToList();
                        var dAppDefinitionClaimedEntities = twoWayLinks?.OfType<DappClaimedEntityResolvedTwoWayLink>().Select(x =>
                        {
                            if (!x.EntityAddress.IsPackage)
                            {
                                return x.ToGatewayModel();
                            }

                            var entityId = dappDefinitionsLinkedEntitiesPackageIdsDictionary[x.EntityAddress];
                            var autoLinkBlueprints = autoLinkBlueprintDictionary[entityId];
                            return x.ToGatewayModel(autoLinkBlueprints);
                        }).ToList();
                        var accountTwoWayLinkedLocker = twoWayLinks?.OfType<DappAccountLockerResolvedTwoWayLink>().FirstOrDefault()?.LockerAddress;

                        if (dAppDefinitionPartnerDapps?.Any() == true || dAppDefinitionClaimedEntities?.Any() == true || accountTwoWayLinkedLocker != null)
                        {
                            dAppDefinitionLinkDetail = new GatewayModel.TwoWayLinkedDappOnLedgerDetails(
                                dapps: dAppDefinitionPartnerDapps?.Any() == true ? new GatewayModel.TwoWayLinkedDappsCollection(items: dAppDefinitionPartnerDapps) : null,
                                entities: dAppDefinitionClaimedEntities?.Any() == true ? new GatewayModel.TwoWayLinkedEntitiesCollection(items: dAppDefinitionClaimedEntities) : null,
                                primaryLocker: accountTwoWayLinkedLocker);
                        }
                    }

                    stateHistory.TryGetValue(ce.Id, out var state);
                    roleAssignmentsHistory.TryGetValue(ce.Id, out var roleAssignments);
                    componentRoyaltyConfigs?.TryGetValue(ce.Id, out componentRoyaltyConfig);

                    if (optIns.ComponentRoyaltyVaultBalance && ce.TryGetCorrelation(EntityRelationship.EntityToRoyaltyVault, out var componentRoyaltyRelation) &&
                        vaultBalances.TryGetValue(componentRoyaltyRelation.EntityId, out var componentRoyaltyVault))
                    {
                        componentRoyaltyVaultBalance = componentRoyaltyVault.Balance.ToString();
                    }

                    details = new GatewayModel.StateEntityDetailsResponseComponentDetails(
                        packageAddress: instantiatingPackageAddress,
                        blueprintName: ce.BlueprintName,
                        blueprintVersion: ce.BlueprintVersion,
                        state: state != null ? new JRaw(state) : null,
                        roleAssignments: roleAssignments,
                        royaltyVaultBalance: componentRoyaltyVaultBalance,
                        royaltyConfig: optIns.ComponentRoyaltyConfig ? componentRoyaltyConfig.ToGatewayModel() : null,
                        twoWayLinkedDappAddress: directlyLinkedDapp ?? blueprintLinkedDapp,
                        blueprintLinkedDappAddress: blueprintLinkedDapp,
                        directLinkedDappAddress: directlyLinkedDapp,
                        twoWayLinkedDappDetails: dAppDefinitionLinkDetail,
                        nativeResourceDetails: nativeResourceDetails);
                    break;
            }

            var ancestorIdentities = optIns.AncestorIdentities && entity.HasParent
                ? new GatewayModel.StateEntityDetailsResponseItemAncestorIdentities(
                    parentAddress: correlatedAddresses[entity.ParentAncestorId.Value],
                    ownerAddress: correlatedAddresses[entity.OwnerAncestorId.Value],
                    globalAddress: correlatedAddresses[entity.GlobalAncestorId.Value])
                : null;

            GatewayModel.FungibleResourcesCollection? fungibles = null;
            GatewayModel.NonFungibleResourcesCollection? nonFungibles = null;

            if (entityFungibleResources.TryGetValue(entity.Id, out var efr))
            {
                fungibles = efr.ToGatewayModel(aggregatePerVault, explicitMetadata);
            }

            if (entityNonFungibleResources.TryGetValue(entity.Id, out var enfr))
            {
                nonFungibles = enfr.ToGatewayModel(aggregatePerVault, explicitMetadata, nonFungibleVaultContents);
            }

            items.Add(new GatewayModel.StateEntityDetailsResponseItem(
                address: entity.Address,
                fungibleResources: fungibles,
                nonFungibleResources: nonFungibles,
                ancestorIdentities: ancestorIdentities,
                metadata: metadata[entity.Id],
                explicitMetadata: explicitMetadata?[entity.Id],
                details: details));
        }

        return new GatewayModel.StateEntityDetailsResponse(ledgerState, items);
    }

    public async Task<GatewayModel.StateEntityFungiblesPageResponse> EntityFungibleResourcesPage(
        IEntityStateQuerier.PageRequestByCursor pageRequest,
        bool aggregatePerVault,
        GatewayModel.StateEntityFungiblesPageRequestOptIns optIns,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var defaultPageSize = _endpointConfiguration.Value.DefaultPageSize;

        var entity = await _entityQuerier.GetEntity<ComponentEntity>(pageRequest.Address, ledgerState, token);
        var entityResourcesConfiguration = new EntityFungibleResourcesAndVaultsPagedQuery.ResourcesPageQueryConfiguration(defaultPageSize, aggregatePerVault ? defaultPageSize : 0,
            pageRequest.Cursor.FromGatewayModel(), ledgerState.StateVersion);
        var fungibleResourcesPage = await EntityFungibleResourcesAndVaultsPagedQuery.FungibleResourcesPage(_dbContext, _dapperWrapper, entity.Id, entityResourcesConfiguration, token);

        if (fungibleResourcesPage != null)
        {
            var entityIds = fungibleResourcesPage
                .FungibleResources
                .Values
                .Select(x => x.ResourceEntityId)
                .Union(new[] { entity.Id })
                .ToHashSet()
                .ToArray();

            var explicitMetadata = optIns.ExplicitMetadata?.Any() == true
                ? await MetadataMultiLookupQuery.Execute(
                    _dbContext.Database.GetDbConnection(),
                    _dapperWrapper,
                    entityIds,
                    optIns.ExplicitMetadata.ToArray(),
                    ledgerState,
                    _networkConfigurationProvider.GetNetworkConfiguration().Id,
                    token)
                : null;

            var mappedFungibles = fungibleResourcesPage.ToGatewayModel(aggregatePerVault, explicitMetadata);

            return new GatewayModel.StateEntityFungiblesPageResponse(ledgerState, mappedFungibles.TotalCount, mappedFungibles.NextCursor, mappedFungibles.Items, pageRequest.Address);
        }

        return new GatewayModel.StateEntityFungiblesPageResponse(ledgerState, 0, null, new List<GatewayModel.FungibleResourcesCollectionItem>(), pageRequest.Address);
    }

    public async Task<GatewayModel.StateEntityFungibleResourceVaultsPageResponse> EntityFungibleResourceVaults(
        IEntityStateQuerier.ResourceVaultsPageRequest request,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var defaultPageSize = _endpointConfiguration.Value.DefaultPageSize;

        var entity = await _entityQuerier.GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await _entityQuerier.GetEntity<GlobalFungibleResourceEntity>(request.ResourceAddress, ledgerState, token);
        var entityResourcesConfiguration = new EntityFungibleResourcesAndVaultsPagedQuery.VaultsPageQueryConfiguration(defaultPageSize, request.Cursor.FromGatewayModel(), ledgerState.StateVersion);
        var fungibleResourceVaultsPage =
            await EntityFungibleResourcesAndVaultsPagedQuery.FungibleResourceVaultsPage(_dbContext, _dapperWrapper, entity.Id, resourceEntity.Id, entityResourcesConfiguration, token);

        if (fungibleResourceVaultsPage != null)
        {
            var fungibles = fungibleResourceVaultsPage.ToGatewayModel(true, null);

            var vaults = (fungibles.Items.FirstOrDefault() as GatewayModel.FungibleResourcesCollectionItemVaultAggregated)?.Vaults;

            if (vaults != null)
            {
                return new GatewayModel.StateEntityFungibleResourceVaultsPageResponse(ledgerState, vaults.TotalCount, vaults.NextCursor, vaults.Items, entity.Address, resourceEntity.Address);
            }
        }

        return new GatewayModel.StateEntityFungibleResourceVaultsPageResponse(ledgerState, 0, null, new List<GatewayModel.FungibleResourcesCollectionItemVaultAggregatedVaultItem>(), entity.Address,
            resourceEntity.Address);
    }

    public async Task<GatewayModel.StateEntityNonFungiblesPageResponse> EntityNonFungibleResourcesPage(
        IEntityStateQuerier.PageRequestByCursor pageRequest,
        bool aggregatePerVault,
        GatewayModel.StateEntityNonFungiblesPageRequestOptIns optIns,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var defaultPageSize = _endpointConfiguration.Value.DefaultPageSize;

        var entity = await _entityQuerier.GetEntity<ComponentEntity>(pageRequest.Address, ledgerState, token);
        var entityResourcesConfiguration = new EntityNonFungibleResourcesAndVaultsPagedQuery.ResourcesPageQueryConfiguration(defaultPageSize, aggregatePerVault ? defaultPageSize : 0,
            pageRequest.Cursor.FromGatewayModel(), ledgerState.StateVersion);
        var entityResources = await EntityNonFungibleResourcesAndVaultsPagedQuery.NonFungibleResourcesPage(_dbContext, _dapperWrapper, entity.Id, entityResourcesConfiguration, token);

        if (entityResources != null)
        {
            var entityIds = entityResources
                .NonFungibleResources
                .Values
                .Select(x => x.ResourceEntityId)
                .Union(new[] { entity.Id })
                .ToHashSet()
                .ToArray();

            var explicitMetadata = optIns.ExplicitMetadata?.Any() == true
                ? await MetadataMultiLookupQuery.Execute(
                    _dbContext.Database.GetDbConnection(),
                    _dapperWrapper,
                    entityIds,
                    optIns.ExplicitMetadata.ToArray(),
                    ledgerState,
                    _networkConfigurationProvider.GetNetworkConfiguration().Id,
                    token)
                : null;

            var nonFungibleVaultContents = optIns.NonFungibleIncludeNfids
                ? await NonFungibleVaultContentsPagedQuery.Execute(
                    _dbContext,
                    _dapperWrapper,
                    ledgerState,
                    entityResources.NonFungibleResources.Values.SelectMany(x => x.Vaults.Values).Select(x => x.VaultEntityId).ToArray(),
                    new NonFungibleVaultContentsPagedQuery.QueryConfiguration(null, defaultPageSize, _endpointConfiguration.Value.MaxDefinitionsLookupLimit), token)
                : null;

            var nonFungibles = entityResources.ToGatewayModel(aggregatePerVault, explicitMetadata, nonFungibleVaultContents);

            return new GatewayModel.StateEntityNonFungiblesPageResponse(ledgerState, nonFungibles.TotalCount, nonFungibles.NextCursor, nonFungibles.Items, pageRequest.Address);
        }

        return new GatewayModel.StateEntityNonFungiblesPageResponse(ledgerState, 0, null, new List<GatewayModel.NonFungibleResourcesCollectionItem>(), pageRequest.Address);
    }

    public async Task<GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse> EntityNonFungibleResourceVaults(
        IEntityStateQuerier.ResourceVaultsPageRequest request,
        GatewayModel.StateEntityNonFungibleResourceVaultsPageOptIns optIns,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var defaultPageSize = _endpointConfiguration.Value.DefaultPageSize;

        var entity = await _entityQuerier.GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await _entityQuerier.GetEntity<GlobalNonFungibleResourceEntity>(request.ResourceAddress, ledgerState, token);
        var entityResourcesConfiguration = new EntityNonFungibleResourcesAndVaultsPagedQuery.VaultsPageQueryConfiguration(defaultPageSize, request.Cursor.FromGatewayModel(), ledgerState.StateVersion);
        var entityResources =
            await EntityNonFungibleResourcesAndVaultsPagedQuery.NonFungibleResourceVaultsPage(_dbContext, _dapperWrapper, entity.Id, resourceEntity.Id, entityResourcesConfiguration, token);

        if (entityResources != null)
        {
            var nonFungibleVaultContents = optIns.NonFungibleIncludeNfids
                ? await NonFungibleVaultContentsPagedQuery.Execute(
                    _dbContext,
                    _dapperWrapper,
                    ledgerState,
                    entityResources.NonFungibleResources.Values.SelectMany(r => r.Vaults.Values).Select(x => x.VaultEntityId).ToArray(),
                    new NonFungibleVaultContentsPagedQuery.QueryConfiguration(null, defaultPageSize, _endpointConfiguration.Value.MaxDefinitionsLookupLimit),
                    token)
                : null;

            var nonFungibles = entityResources.ToGatewayModel(true, null, nonFungibleVaultContents);

            var vaults = (nonFungibles.Items.FirstOrDefault() as GatewayModel.NonFungibleResourcesCollectionItemVaultAggregated)?.Vaults;

            if (vaults != null)
            {
                return new GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse(ledgerState, vaults.TotalCount, vaults.NextCursor, vaults.Items, entity.Address, resourceEntity.Address);
            }
        }

        return new GatewayModel.StateEntityNonFungibleResourceVaultsPageResponse(ledgerState, 0, null, new List<GatewayModel.NonFungibleResourcesCollectionItemVaultAggregatedVaultItem>(),
            entity.Address, resourceEntity.Address);
    }

    public async Task<GatewayModel.StateEntityMetadataPageResponse> EntityMetadata(
        IEntityStateQuerier.PageRequestByCursor request,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var entity = await _entityQuerier.GetEntity<Entity>(request.Address, ledgerState, token);
        GatewayModel.EntityMetadataCollection metadata;

        if (entity is PreAllocatedIdentityEntity or PreAllocatedAccountComponentEntity)
        {
            var (_, preAllocatedEntityMetadata) = _preAllocatedEntityDataProvider.GetPreAllocatedEntityData(entity.Address);
            metadata = preAllocatedEntityMetadata;
        }
        else
        {
            metadata = (await MetadataPagedQuery.Execute(
                _dbContext.Database.GetDbConnection(),
                _dapperWrapper,
                ledgerState,
                new[] { entity.Id },
                new MetadataPagedQuery.QueryConfiguration
                {
                    Cursor = request.Cursor,
                    PageSize = request.Limit,
                    MaxDefinitionsLookupLimit = _endpointConfiguration.Value.MaxDefinitionsLookupLimit,
                },
                _networkConfigurationProvider.GetNetworkConfiguration().Id,
                token))[entity.Id];
        }

        return new GatewayModel.StateEntityMetadataPageResponse(ledgerState, metadata.TotalCount, metadata.NextCursor, metadata.Items, entity.Address);
    }

    public async Task<GatewayModel.StateEntitySchemaPageResponse?> EntitySchema(IEntityStateQuerier.PageRequest pageRequest, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await _entityQuerier.GetEntity<Entity>(pageRequest.Address, ledgerState, token);
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
            nextCursor: GatewayModelExtensions.GenerateOffsetCursor(pageRequest.Offset, pageRequest.Limit, totalCount),
            items: packageSchemas.Select(pb => pb.ToGatewayModel()).ToList());
    }

    public async Task<GatewayModel.StateEntityNonFungibleIdsPageResponse> EntityNonFungibleIds(
        IEntityStateQuerier.PageRequestByCursor request,
        EntityAddress resourceAddress,
        EntityAddress vaultAddress,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var defaultPageSize = _endpointConfiguration.Value.DefaultPageSize;
        var entity = await _entityQuerier.GetEntity<ComponentEntity>(request.Address, ledgerState, token);
        var resourceEntity = await _entityQuerier.GetEntity<GlobalNonFungibleResourceEntity>(resourceAddress, ledgerState, token);
        var vaultEntity = await _entityQuerier.GetEntity<VaultEntity>(vaultAddress, ledgerState, token);

        var vaultEntityIds = new[] { vaultEntity.Id };

        var queryConfiguration = new NonFungibleVaultContentsPagedQuery.QueryConfiguration(
            request.Cursor.FromGatewayModel(),
            request.Limit,
            _endpointConfiguration.Value.MaxDefinitionsLookupLimit
        );

        var nonFungibleIdsPerVault = await NonFungibleVaultContentsPagedQuery.Execute(
            _dbContext,
            _dapperWrapper,
            ledgerState,
            vaultEntityIds,
            queryConfiguration,
            token);

        if (nonFungibleIdsPerVault.TryGetValue(vaultEntity.Id, out var vaultContent))
        {
            return new GatewayModel.StateEntityNonFungibleIdsPageResponse(
                ledgerState: ledgerState,
                totalCount: vaultContent.TotalCount,
                nextCursor: vaultContent.NextCursor,
                items: vaultContent.Items,
                address: entity.Address,
                resourceAddress: resourceEntity.Address);
        }

        return new GatewayModel.StateEntityNonFungibleIdsPageResponse(ledgerState, 0, null, new List<string>(), entity.Address, resourceEntity.Address);
    }

    private async Task<IDictionary<long, VaultBalanceHistory>> GetVaultBalances(long[] vaultEntityIds, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        if (!vaultEntityIds.Any())
        {
            return ImmutableDictionary<long, VaultBalanceHistory>.Empty;
        }

        return await _dbContext
            .VaultBalanceHistory
            .FromSqlInterpolated($@"WITH
variables AS (
    SELECT unnest({vaultEntityIds}) AS vault_entity_id
)
SELECT vbh.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM vault_balance_history
    WHERE vault_entity_id = var.vault_entity_id AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) vbh on true;")
            .ToDictionaryAsync(e => e.VaultEntityId, token);
    }

    private async Task<IDictionary<long, SchemaEntryViewModel[]>> GetEntitySchemaHistory(long[] entityIds, int offset, int limit, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        if (!entityIds.Any())
        {
            return ImmutableDictionary<long, SchemaEntryViewModel[]>.Empty;
        }

        var parameters = new
        {
            entityIds = entityIds.ToList(),
            startIndex = offset + 1,
            endIndex = offset + limit,
            stateVersion = ledgerState.StateVersion,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
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
            parameters: parameters,
            cancellationToken: token);

        return (await _dbContext.Database.GetDbConnection().QueryAsync<SchemaEntryViewModel>(cd))
            .ToList()
            .GroupBy(b => b.EntityId)
            .ToDictionary(g => g.Key, g => g.ToArray());
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
                        _networkConfigurationProvider.GetNetworkConfiguration().Id);

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

    private async Task<IDictionary<long, List<string>?>> GetBlueprintLinkMetadata(
        long[] entityIds,
        GatewayModel.LedgerState ledgerState,
        NetworkConfiguration networkConfiguration,
        CancellationToken token)
    {
        const string EnableBlueprintLinkingMetadataKey = "enable_blueprint_linking";

        if (entityIds.Length == 0)
        {
            return ImmutableDictionary<long, List<string>?>.Empty;
        }

        var blueprintLinkingMetaData = await MetadataMultiLookupQuery.Execute(
            _dbContext.Database.GetDbConnection(),
            _dapperWrapper,
            entityIds,
            new[] { EnableBlueprintLinkingMetadataKey },
            ledgerState,
            networkConfiguration.Id,
            token);

        return blueprintLinkingMetaData.ToDictionary(
            x => x.Key,
            x =>
            {
                var typed = x.Value.Items.FirstOrDefault()?.Value.Typed;
                return typed is not GatewayModel.MetadataStringArrayValue stringArrayValue ? null : stringArrayValue.Values;
            });
    }
}
