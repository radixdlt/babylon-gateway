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
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

[Table("entities")]
internal abstract class Entity
{
    internal record CorrelatedEntity(EntityRelationship Relationship, long EntityId);

    private HashSet<CorrelatedEntity>? _correlatedEntities;

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("from_state_version")]
    public long FromStateVersion { get; set; }

    [Column("address")]
    public EntityAddress Address { get; set; }

    [Column("is_global")]
    public bool IsGlobal { get; set; }

    [Column("ancestor_ids")]
    public List<long>? AncestorIds { get; set; }

    [Column("parent_ancestor_id")]
    public long? ParentAncestorId { get; set; }

    [Column("owner_ancestor_id")]
    public long? OwnerAncestorId { get; set; }

    [Column("global_ancestor_id")]
    public long? GlobalAncestorId { get; set; }

    [Column("correlated_entity_relationships")]
    public List<EntityRelationship> CorrelatedEntityRelationships { get; set; } = new();

    [Column("correlated_entity_ids")]
    public List<long> CorrelatedEntityIds { get; set; } = new();

    [MemberNotNullWhen(true, nameof(AncestorIds), nameof(ParentAncestorId), nameof(OwnerAncestorId), nameof(GlobalAncestorId))]
    public bool HasParent => AncestorIds != null;

    [NotMapped]
    public ISet<CorrelatedEntity> Correlations
    {
        get => _correlatedEntities ??= new HashSet<CorrelatedEntity>(CorrelatedEntityRelationships.Zip(CorrelatedEntityIds).Select(x => new CorrelatedEntity(x.First, x.Second)));
    }

    public void AddCorrelation(EntityRelationship relationship, long entityId)
    {
        Correlations.Add(new CorrelatedEntity(relationship, entityId));
    }

    public bool TryGetCorrelation(EntityRelationship relationship, [NotNullWhen(true)] out CorrelatedEntity? correlation)
    {
        correlation = Correlations.SingleOrDefault(x => x.Relationship == relationship);

        return correlation != null;
    }

    public CorrelatedEntity GetCorrelation(EntityRelationship relationship)
    {
        if (TryGetCorrelation(relationship, out var definition))
        {
            return definition;
        }

        throw new ArgumentException("Relationship not found", nameof(relationship));
    }
}

internal abstract class ResourceEntity : ComponentEntity
{
}

internal class GlobalFungibleResourceEntity : ResourceEntity
{
    [Column("divisibility")]
    public int Divisibility { get; set; }
}

internal class GlobalNonFungibleResourceEntity : ResourceEntity, IAggregateHolder
{
    [Column("non_fungible_id_type")]
    public NonFungibleIdType NonFungibleIdType { get; set; }

    [Column("non_fungible_data_mutable_fields")]
    public List<string> NonFungibleDataMutableFields { get; set; }

    public IEnumerable<(string Name, int TotalCount)> AggregateCounts()
    {
        yield return (nameof(NonFungibleDataMutableFields), NonFungibleDataMutableFields.Count);
    }
}

internal abstract class ComponentEntity : Entity
{
    [Column("blueprint_name")]
    public string BlueprintName { get; set; }

    [Column("blueprint_version")]
    public string BlueprintVersion { get; set; }

    [Column("assigned_module_ids")]
    public List<ModuleId> AssignedModuleIds { get; set; }

    public long GetPackageId() => GetCorrelation(EntityRelationship.ComponentPackage).EntityId;
}

internal class GlobalValidatorEntity : ComponentEntity
{
    public long GetStakeVaultEntityId() => GetCorrelation(EntityRelationship.ValidatorStakeVault).EntityId;

    public long GetPendingXrdWithdrawVaultEntityId() => GetCorrelation(EntityRelationship.ValidatorPendingXrdWithdrawVault).EntityId;

    public long GetLockedOwnerStakeUnitVaultEntityId() => GetCorrelation(EntityRelationship.ValidatorLockedOwnerStakeUnitVault).EntityId;

    public long GetPendingOwnerStakeUnitUnlockVaultEntityId() => GetCorrelation(EntityRelationship.ValidatorPendingOwnerStakeUnitUnlockVault).EntityId;
}

internal class GlobalConsensusManager : ComponentEntity
{
}

internal abstract class VaultEntity : ComponentEntity
{
    public long GetResourceEntityId() => GetCorrelation(EntityRelationship.VaultResource).EntityId;

    public bool TryGetAccountLockerEntryDbLookup(out AccountLockerEntryDbLookup lookup)
    {
        lookup = default;

        if (TryGetCorrelation(EntityRelationship.AccountLockerLocker, out var locker) && TryGetCorrelation(EntityRelationship.AccountLockerAccount, out var account))
        {
            lookup = new AccountLockerEntryDbLookup(locker.EntityId, account.EntityId);

            return true;
        }

        return false;
    }
}

internal class InternalFungibleVaultEntity : VaultEntity
{
    public bool IsRoyaltyVault => TryGetCorrelation(EntityRelationship.VaultRoyalty, out _);
}

internal class InternalNonFungibleVaultEntity : VaultEntity
{
}

internal class GlobalGenericComponentEntity : ComponentEntity
{
}

internal class InternalGenericComponentEntity : ComponentEntity
{
}

internal class GlobalAccountEntity : ComponentEntity
{
}

internal class GlobalAccountLockerEntity : ComponentEntity
{
}

internal class GlobalIdentityEntity : ComponentEntity
{
}

internal class GlobalPackageEntity : ComponentEntity
{
}

// This is transient model, not stored in database
// "virtual" should be understood as un-instantiated, pre-allocated entity
internal class VirtualAccountComponentEntity : GlobalAccountEntity
{
    public VirtualAccountComponentEntity(EntityAddress address)
    {
        Address = address;
        IsGlobal = true;
    }
}

// This is transient model, not stored in database
// "virtual" should be understood as un-instantiated, pre-allocated entity
internal class VirtualIdentityEntity : GlobalIdentityEntity
{
    public VirtualIdentityEntity(EntityAddress address)
    {
        Address = address;
        IsGlobal = true;
    }
}

internal class InternalKeyValueStoreEntity : Entity
{
    public bool TryGetAccountLockerEntryDbLookup(out AccountLockerEntryDbLookup lookup)
    {
        lookup = default;

        if (TryGetCorrelation(EntityRelationship.AccountLockerLocker, out var locker) && TryGetCorrelation(EntityRelationship.AccountLockerAccount, out var account))
        {
            lookup = new AccountLockerEntryDbLookup(locker.EntityId, account.EntityId);

            return true;
        }

        return false;
    }
}

internal class GlobalAccessControllerEntity : ComponentEntity
{
}

internal class ResourcePoolEntity : ComponentEntity
{
}

internal class GlobalOneResourcePoolEntity : ResourcePoolEntity
{
}

internal class GlobalTwoResourcePoolEntity : ResourcePoolEntity
{
}

internal class GlobalMultiResourcePoolEntity : ResourcePoolEntity
{
}

internal class GlobalTransactionTrackerEntity : ComponentEntity
{
}
