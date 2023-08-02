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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

[Table("entity_resource_vault_aggregate_history")]
internal class EntityResourceVaultAggregateHistory
{
    private long[]? _originalVaultEntityIds = null;

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("from_state_version")]
    public long FromStateVersion { get; set; }

    [Column("entity_id")]
    public long EntityId { get; set; }

    [Column("resource_entity_id")]
    public long ResourceEntityId { get; set; }

    [Column("vault_entity_ids")]
    public List<long> VaultEntityIds { get; set; }

    [Column("vault_entity_id_significant_update_state_versions")]
    public List<long> VaultEntityIdSignificantUpdateStateVersions { get; set; }

    public static EntityResourceVaultAggregateHistory Create(long databaseId, long entityId, long resourceEntityId, long stateVersion)
    {
        return CopyOrCreate(databaseId, null, entityId, resourceEntityId, stateVersion);
    }

    public static EntityResourceVaultAggregateHistory CopyOf(long databaseId, EntityResourceVaultAggregateHistory other, long stateVersion)
    {
        return CopyOrCreate(databaseId, other, other.EntityId, other.ResourceEntityId, stateVersion);
    }

    /// <summary>
    /// Attempts to add new or update existing vault unless it is already the most recently modified one.
    /// </summary>
    /// <returns>true if added or modified, otherwise false.</returns>
    public bool TryUpsertVault(long vaultEntityId, long stateVersion)
    {
        var currentIndex = VaultEntityIds.IndexOf(vaultEntityId);

        // we're already the most recent one and there's no point potential second resource with same state_version
        if (currentIndex == 0 && VaultEntityIds.Count == 1)
        {
            return false;
        }

        // we're already the most recent one but there might be second resource with matching state_version
        // so in order to guarantee that we'll be unambiguously earlier in the overall sequence we might potentially
        // want to update anyways
        if (currentIndex == 0 && VaultEntityIds.Count > 1 && VaultEntityIdSignificantUpdateStateVersions[1] < VaultEntityIdSignificantUpdateStateVersions[0])
        {
            return false;
        }

        if (currentIndex != -1)
        {
            VaultEntityIds.RemoveAt(currentIndex);
            VaultEntityIdSignificantUpdateStateVersions.RemoveAt(currentIndex);
        }

        VaultEntityIds.Insert(0, vaultEntityId);
        VaultEntityIdSignificantUpdateStateVersions.Insert(0, stateVersion);

        return true;
    }

    public bool ShouldBePersisted()
    {
        if (_originalVaultEntityIds == null)
        {
            return true;
        }

        if (!_originalVaultEntityIds.SequenceEqual(VaultEntityIds.ToArray()))
        {
            return true;
        }

        return false;
    }

    private static EntityResourceVaultAggregateHistory CopyOrCreate(long databaseId, EntityResourceVaultAggregateHistory? other, long entityId, long resourceEntityId, long stateVersion)
    {
        var ret = new EntityResourceVaultAggregateHistory
        {
            Id = databaseId,
            FromStateVersion = stateVersion,
            EntityId = entityId,
            ResourceEntityId = resourceEntityId,
            VaultEntityIds = new List<long>(other?.VaultEntityIds.ToArray() ?? Array.Empty<long>()),
            VaultEntityIdSignificantUpdateStateVersions = new List<long>(other?.VaultEntityIdSignificantUpdateStateVersions.ToArray() ?? Array.Empty<long>()),
        };

        if (other != null)
        {
            ret._originalVaultEntityIds = ret.VaultEntityIds.ToArray();
        }

        return ret;
    }
}
