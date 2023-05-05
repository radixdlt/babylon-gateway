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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

[Table("entities")]
internal abstract class Entity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("from_state_version")]
    public long FromStateVersion { get; set; }

    [Column("address")]
    public RadixAddress Address { get; set; }

    [Column("global_address")]
    public GlobalAddress? GlobalAddress { get; set; }

    [Column("ancestor_ids")]
    public List<long>? AncestorIds { get; set; }

    [Column("parent_ancestor_id")]
    public long? ParentAncestorId { get; set; }

    [Column("owner_ancestor_id")]
    public long? OwnerAncestorId { get; set; }

    [Column("global_ancestor_id")]
    public long? GlobalAncestorId { get; set; }

    [Column("correlated_entities")]
    public virtual List<long> CorrelatedEntities
    {
        get
        {
            return new List<long>(AncestorIds?.ToArray() ?? Array.Empty<long>());
        }

        private set
        {
            /* setter needed for EF Core only */
        }
    }

    [MemberNotNullWhen(true, nameof(AncestorIds), nameof(ParentAncestorId), nameof(OwnerAncestorId), nameof(GlobalAncestorId))]
    public bool HasParent => AncestorIds != null;
}

internal abstract class ResourceEntity : ComponentEntity
{
}

internal class FungibleResourceEntity : ResourceEntity
{
    [Column("divisibility")]
    public int Divisibility { get; set; }
}

internal class NonFungibleResourceEntity : ResourceEntity
{
    [Column("non_fungible_id_type")]
    public NonFungibleIdType NonFungibleIdType { get; set; }
}

internal abstract class ComponentEntity : Entity
{
    [Column("package_id")]
    public long PackageId { get; set; }

    [Column("blueprint_name")]
    public string BlueprintName { get; set; }

    public override List<long> CorrelatedEntities
    {
        get
        {
            var ce = base.CorrelatedEntities;
            ce.Add(PackageId);

            return ce;
        }
    }
}

internal class ValidatorEntity : ComponentEntity
{
    [Column("stake_vault_entity_id")]
    public long StakeVaultEntityId { get; set; }

    [Column("unstake_vault_entity_id")]
    public long UnstakeVaultEntityId { get; set; }

    [Column("epoch_manager_entity_id")]
    public long EpochManagerEntityId { get; set; }

    public override List<long> CorrelatedEntities
    {
        get
        {
            var ce = base.CorrelatedEntities;

            ce.Add(StakeVaultEntityId);
            ce.Add(UnstakeVaultEntityId);
            ce.Add(EpochManagerEntityId);

            return ce;
        }
    }
}

internal class EpochManagerEntity : ComponentEntity
{
}

internal class ClockEntity : ComponentEntity
{
}

internal class VaultEntity : ComponentEntity
{
    [Column("resource_entity_id")]
    public long ResourceEntityId { get; set; }

    [Column("royalty_vault_of_entity_id")]
    public long? RoyaltyVaultOfEntityId { get; set; }

    public bool IsRoyaltyVault => RoyaltyVaultOfEntityId != null;

    public override List<long> CorrelatedEntities
    {
        get
        {
            var ce = base.CorrelatedEntities;

            ce.Add(ResourceEntityId);

            if (RoyaltyVaultOfEntityId.HasValue)
            {
                ce.Add(RoyaltyVaultOfEntityId.Value);
            }

            return ce;
        }
    }
}

internal class NormalComponentEntity : ComponentEntity
{
}

internal class AccountComponentEntity : ComponentEntity
{
}

internal class IdentityEntity : ComponentEntity
{
}

internal class PackageEntity : ComponentEntity
{
    [Column("code")]
    public byte[] Code { get; set; }

    [Column("code_type")]
    public string CodeType { get; set; }
}

// This is transient model, not stored in database
internal class VirtualAccountComponentEntity : AccountComponentEntity
{
    public VirtualAccountComponentEntity(GlobalAddress globalAddress)
    {
        GlobalAddress = globalAddress;
    }
}

// This is transient model, not stored in database
internal class VirtualIdentityEntity : IdentityEntity
{
    public VirtualIdentityEntity(GlobalAddress globalAddress)
    {
        GlobalAddress = globalAddress;
    }
}

internal class KeyValueStoreEntity : Entity
{
    [Column("store_of_non_fungible_resource_entity_id")]
    public long? StoreOfNonFungibleResourceEntityId { get; set; }
}

internal class AccessControllerEntity : ComponentEntity
{
}
