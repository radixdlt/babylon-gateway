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

using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Diagnostics;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class EntityRelationshipProcessor : ISubstateScanUpsertProcessor
{
    private readonly ReferencedEntityDictionary _referencedEntities;

    public EntityRelationshipProcessor(ReferencedEntityDictionary referencedEntities)
    {
        _referencedEntities = referencedEntities;
    }

    public void OnUpsertScan(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.IRoyaltyVaultHolder royaltyVaultHolder && royaltyVaultHolder.TryGetRoyaltyVault(out var rv))
        {
            referencedEntity.PostResolveConfigure((Entity e) =>
            {
                e.AddCorrelation(EntityRelationship.EntityToRoyaltyVault, _referencedEntities.Get((EntityAddress)rv.EntityAddress).DatabaseId);

                _referencedEntities.Get((EntityAddress)rv.EntityAddress).PostResolveConfigureLow((InternalFungibleVaultEntity ve) =>
                {
                    ve.AddCorrelation(EntityRelationship.RoyaltyVaultOfEntity, referencedEntity.DatabaseId);
                });
            });
        }

        if (substateData is CoreModel.TypeInfoModuleFieldTypeInfoSubstate typeInfoSubstate && typeInfoSubstate.Value.Details is CoreModel.ObjectTypeInfoDetails objectDetails)
        {
            referencedEntity.PostResolveConfigure((ComponentEntity e) =>
            {
                e.AddCorrelation(EntityRelationship.ComponentToInstantiatingPackage, _referencedEntities.Get((EntityAddress)objectDetails.BlueprintInfo.PackageAddress).DatabaseId);
            });

            if (objectDetails.BlueprintInfo.BlueprintName is CoreModel.NativeBlueprintNames.FungibleVault or CoreModel.NativeBlueprintNames.NonFungibleVault)
            {
                referencedEntity.PostResolveConfigure((VaultEntity e) =>
                {
                    var resourceEntityId = _referencedEntities.Get((EntityAddress)objectDetails.BlueprintInfo.OuterObject).DatabaseId;
                    e.OuterObjectEntityId = resourceEntityId;
                    e.AddCorrelation(EntityRelationship.VaultToResource, resourceEntityId);
                });
            }

            if (objectDetails.BlueprintInfo.BlueprintName is CoreModel.NativeBlueprintNames.Validator)
            {
                referencedEntity.PostResolveConfigure((GlobalValidatorEntity e) =>
                {
                    var consensusManagerEntityId = _referencedEntities.Get((EntityAddress)objectDetails.BlueprintInfo.OuterObject).DatabaseId;
                    e.OuterObjectEntityId = consensusManagerEntityId;
                });
            }
        }

        if (substateData is CoreModel.ValidatorFieldStateSubstate validator)
        {
            referencedEntity.PostResolveConfigure((GlobalValidatorEntity e) =>
            {
                e.AddCorrelation(EntityRelationship.ValidatorToStakeVault, _referencedEntities.Get((EntityAddress)validator.Value.StakeXrdVault.EntityAddress).DatabaseId);
                e.AddCorrelation(EntityRelationship.ValidatorToPendingXrdWithdrawVault, _referencedEntities.Get((EntityAddress)validator.Value.PendingXrdWithdrawVault.EntityAddress).DatabaseId);
                e.AddCorrelation(EntityRelationship.ValidatorToLockedOwnerStakeUnitVault, _referencedEntities.Get((EntityAddress)validator.Value.LockedOwnerStakeUnitVault.EntityAddress).DatabaseId);
                e.AddCorrelation(EntityRelationship.ValidatorToPendingOwnerStakeUnitUnlockVault, _referencedEntities.Get((EntityAddress)validator.Value.PendingOwnerStakeUnitUnlockVault.EntityAddress).DatabaseId);

                _referencedEntities.Get((EntityAddress)validator.Value.ClaimTokenResourceAddress).PostResolveConfigureLow((ResourceEntity cte) =>
                {
                    cte.AddCorrelation(EntityRelationship.ClaimTokenOfValidator, e.Id);
                });

                _referencedEntities.Get((EntityAddress)validator.Value.StakeUnitResourceAddress).PostResolveConfigureLow((ResourceEntity ue) =>
                {
                    ue.AddCorrelation(EntityRelationship.StakeUnitOfValidator, e.Id);
                });
            });
        }

        if (substateData is CoreModel.AccessControllerFieldStateSubstate accessController)
        {
            var recoveryBadge = (EntityAddress)accessController.Value.RecoveryBadgeResourceAddress;

            referencedEntity.PostResolveConfigure((GlobalAccessControllerEntity ac) =>
            {
                ac.AddCorrelation(EntityRelationship.AccessControllerToRecoveryBadge, _referencedEntities.Get(recoveryBadge).DatabaseId);

                _referencedEntities.Get(recoveryBadge).PostResolveConfigureLow((GlobalNonFungibleResourceEntity nf) =>
                {
                    nf.AddCorrelation(EntityRelationship.RecoveryBadgeOfAccessController, ac.Id);
                });
            });
        }

        if (substateData is CoreModel.OneResourcePoolFieldStateSubstate oneResourcePool)
        {
            referencedEntity.PostResolveConfigureLow((GlobalOneResourcePoolEntity e) =>
            {
                var poolUnitResourceEntity = _referencedEntities.Get((EntityAddress)oneResourcePool.Value.PoolUnitResourceAddress);

                e.AddCorrelation(EntityRelationship.ResourcePoolToUnitResource, poolUnitResourceEntity.DatabaseId);

                _referencedEntities.GetByDatabaseId(poolUnitResourceEntity.DatabaseId).PostResolveConfigureLow((ResourceEntity ue) =>
                {
                    ue.AddCorrelation(EntityRelationship.UnitResourceOfResourcePool, e.Id);
                });

                var vault = oneResourcePool.Value.Vault;

                _referencedEntities.Get((EntityAddress)vault.EntityAddress).PostResolveConfigureLow((VaultEntity ve) =>
                {
                    e.AddCorrelation(EntityRelationship.ResourcePoolToResourceVault, ve.Id);
                    ve.AddCorrelation(EntityRelationship.ResourceVaultOfResourcePool, referencedEntity.DatabaseId);

                    // as OneResourcePool substates do not expose Resource we need to rely on Vault's correlation to the Resource,
                    // hence the use of .PostResolveConfigureLow() to ensure this correlation is already set
                    e.AddCorrelation(EntityRelationship.ResourcePoolToResource, ve.GetResourceEntityId());
                });
            });
        }

        if (substateData is CoreModel.TwoResourcePoolFieldStateSubstate twoResourcePool)
        {
            referencedEntity.PostResolveConfigure((GlobalTwoResourcePoolEntity e) =>
            {
                var poolUnitResourceEntity = _referencedEntities.Get((EntityAddress)twoResourcePool.Value.PoolUnitResourceAddress);

                e.AddCorrelation(EntityRelationship.ResourcePoolToUnitResource, poolUnitResourceEntity.DatabaseId);

                _referencedEntities.GetByDatabaseId(poolUnitResourceEntity.DatabaseId).PostResolveConfigureLow((ResourceEntity ue) =>
                {
                    ue.AddCorrelation(EntityRelationship.UnitResourceOfResourcePool, e.Id);
                });

                foreach (var poolVault in twoResourcePool.Value.Vaults)
                {
                    e.AddCorrelation(EntityRelationship.ResourcePoolToResource, _referencedEntities.Get((EntityAddress)poolVault.ResourceAddress).DatabaseId);

                    _referencedEntities.Get((EntityAddress)poolVault.Vault.EntityAddress).PostResolveConfigureLow((VaultEntity ve) =>
                    {
                        e.AddCorrelation(EntityRelationship.ResourcePoolToResourceVault, ve.Id);
                        ve.AddCorrelation(EntityRelationship.ResourceVaultOfResourcePool, referencedEntity.DatabaseId);
                    });
                }
            });
        }

        if (substateData is CoreModel.MultiResourcePoolFieldStateSubstate multiResourcePool)
        {
            referencedEntity.PostResolveConfigure((GlobalMultiResourcePoolEntity e) =>
            {
                var poolUnitResourceEntity = _referencedEntities.Get((EntityAddress)multiResourcePool.Value.PoolUnitResourceAddress);

                e.AddCorrelation(EntityRelationship.ResourcePoolToUnitResource, poolUnitResourceEntity.DatabaseId);

                _referencedEntities.GetByDatabaseId(poolUnitResourceEntity.DatabaseId).PostResolveConfigureLow((ResourceEntity ue) =>
                {
                    ue.AddCorrelation(EntityRelationship.UnitResourceOfResourcePool, e.Id);
                });

                foreach (var vault in multiResourcePool.Value.Vaults)
                {
                    e.AddCorrelation(EntityRelationship.ResourcePoolToResource, _referencedEntities.Get((EntityAddress)vault.ResourceAddress).DatabaseId);

                    _referencedEntities.Get((EntityAddress)vault.Vault.EntityAddress).PostResolveConfigureLow((VaultEntity ve) =>
                    {
                        e.AddCorrelation(EntityRelationship.ResourcePoolToResourceVault, ve.Id);
                        ve.AddCorrelation(EntityRelationship.ResourceVaultOfResourcePool, referencedEntity.DatabaseId);
                    });
                }
            });
        }

        // we want to annotate AccountLocker-related KeyValueStores with corresponding AccountLocker+Account pair
        if (substateData is CoreModel.AccountLockerAccountClaimsEntrySubstate accountLocker)
        {
            var account = accountLocker.Key.AccountAddress;
            var keyValueStore = accountLocker.Value.ResourceVaults;

            _referencedEntities
                .GetOrAdd((EntityAddress)keyValueStore.EntityAddress, ea => new ReferencedEntity(ea, keyValueStore.EntityType, stateVersion))
                .PostResolveConfigure((InternalKeyValueStoreEntity e) =>
                {
                    e.AddCorrelation(EntityRelationship.AccountLockerOfLocker, referencedEntity.DatabaseId);
                    e.AddCorrelation(EntityRelationship.AccountLockerOfAccount, _referencedEntities.Get((EntityAddress)account).DatabaseId);
                });
        }

        // we want to annotate AccountLocker-related Vaults using AccountLocker+Account pair obtained from corresponding KeyValueStore
        if (substateData is CoreModel.TypeInfoModuleFieldTypeInfoSubstate && referencedEntity.Type is CoreModel.EntityType.InternalFungibleVault or CoreModel.EntityType.InternalNonFungibleVault)
        {
            referencedEntity.PostResolveConfigure((Entity e) =>
            {
                if (!e.ParentAncestorId.HasValue)
                {
                    throw new UnreachableException("Vault cannot exists without a parent entity.");
                }

                var parent = _referencedEntities.GetByDatabaseId(e.ParentAncestorId.Value);

                if (parent.Type == CoreModel.EntityType.InternalKeyValueStore)
                {
                    var parentKeyValueStore = parent.GetDatabaseEntity<InternalKeyValueStoreEntity>();

                    if (parentKeyValueStore.TryGetAccountLockerEntryDbLookup(out var lookup))
                    {
                        // as we're running in the context of PostResolveConfigure with regular priority we must fall back to low priority action
                        referencedEntity.PostResolveConfigureLow((VaultEntity ve) =>
                        {
                            ve.AddCorrelation(EntityRelationship.AccountLockerOfLocker, lookup.LockerEntityId);
                            ve.AddCorrelation(EntityRelationship.AccountLockerOfAccount, lookup.AccountEntityId);
                        });
                    }
                }
            });
        }
    }
}
