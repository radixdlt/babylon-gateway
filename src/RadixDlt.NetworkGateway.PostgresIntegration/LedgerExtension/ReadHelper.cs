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
using Npgsql;
using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class ReadHelper
{
    private readonly ReadWriteDbContext _dbContext;
    private readonly NpgsqlConnection _connection;

    public ReadHelper(ReadWriteDbContext dbContext)
    {
        _dbContext = dbContext;
        _connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
    }

    public async Task<Dictionary<long, EntityResourceAggregateHistory>> MostRecentEntityResourceAggregateHistoryFor(List<FungibleVaultChange> fungibleVaultChanges, List<NonFungibleVaultChange> nonFungibleVaultChanges, CancellationToken token)
    {
        var entityIds = new HashSet<long>();

        foreach (var change in fungibleVaultChanges)
        {
            entityIds.Add(change.ReferencedVault.DatabaseOwnerAncestorId);
            entityIds.Add(change.ReferencedVault.DatabaseGlobalAncestorId);
        }

        foreach (var change in nonFungibleVaultChanges)
        {
            entityIds.Add(change.ReferencedVault.DatabaseOwnerAncestorId);
            entityIds.Add(change.ReferencedVault.DatabaseGlobalAncestorId);
        }

        var ids = entityIds.ToList();

        return await _dbContext.EntityResourceAggregateHistory
            .FromSqlInterpolated(@$"
WITH variables (entity_id) AS (
    SELECT UNNEST({ids})
)
SELECT erah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_resource_aggregate_history
    WHERE entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) erah ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => e.EntityId, token);
    }

    public async Task<Dictionary<long, NonFungibleIdStoreHistory>> MostRecentNonFungibleIdStoreHistoryFor(List<NonFungibleIdChange> nonFungibleIdStoreChanges, CancellationToken token)
    {
        var ids = nonFungibleIdStoreChanges.Select(x => x.ReferencedStore.DatabaseGlobalAncestorId).Distinct().ToList();

        return await _dbContext.NonFungibleIdStoreHistory
            .FromSqlInterpolated(@$"
WITH variables (entity_id) AS (
    SELECT UNNEST({ids})
)
SELECT emh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM non_fungible_id_store_history
    WHERE non_fungible_resource_manager_entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) emh ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => e.NonFungibleResourceManagerEntityId, token);
    }

    public async Task<Dictionary<long, ResourceManagerEntitySupplyHistory>> MostRecentResourceManagerEntitySupplyHistoryFor(List<ResourceManagerSupplyChange> resourceManagerSupplyChanges, CancellationToken token)
    {
        var ids = resourceManagerSupplyChanges.Select(c => c.ResourceEntity.DatabaseId).Distinct().ToList();

        return await _dbContext.ResourceManagerEntitySupplyHistory
            .FromSqlInterpolated(@$"
WITH variables (resource_manager_entity_id) AS (
    SELECT UNNEST({ids})
)
SELECT rmesh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM resource_manager_entity_supply_history
    WHERE resource_manager_entity_id = variables.resource_manager_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) rmesh ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => e.ResourceManagerEntityId, token);
    }

    public async Task<Dictionary<string, Entity>> ExistingEntitiesFor(ReferencedEntityDictionary referencedEntities, CancellationToken token)
    {
        var entityAddresses = referencedEntities.Addresses.Select(x => x.ConvertFromHex()).ToList();
        var globalEntityAddresses = referencedEntities.KnownGlobalAddresses.Select(x => (string)x).ToList();
        var entityAddressesParameter = new NpgsqlParameter("@entity_addresses", NpgsqlDbType.Array | NpgsqlDbType.Bytea) { Value = entityAddresses };
        var globalEntityAddressesParameter = new NpgsqlParameter("@global_entity_addresses", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = globalEntityAddresses };

        return await _dbContext.Entities
            .FromSqlInterpolated($@"
SELECT *
FROM entities
WHERE id IN(
    SELECT DISTINCT UNNEST(id || ancestor_ids) AS id
    FROM entities
    WHERE address = ANY({entityAddressesParameter}) OR global_address = ANY({globalEntityAddressesParameter})
)")
            .AsNoTracking()
            .ToDictionaryAsync(e => ((byte[])e.Address).ToHex(), token);
    }

    public async Task<Dictionary<NonFungibleIdLookup, NonFungibleIdData>> ExistingNonFungibleIdDataFor(List<NonFungibleIdChange> nonFungibleIdStoreChanges, List<NonFungibleVaultChange> nonFungibleVaultChanges, CancellationToken token)
    {
        var nonFungibles = new HashSet<NonFungibleIdLookup>();
        var resourceManagerEntityIds = new List<long>();
        var nonFungibleIds = new List<string>();

        foreach (var nonFungibleIdChange in nonFungibleIdStoreChanges)
        {
            nonFungibles.Add(new NonFungibleIdLookup(nonFungibleIdChange.ReferencedResource.DatabaseId, nonFungibleIdChange.NonFungibleId));
        }

        foreach (var nonFungibleVaultChange in nonFungibleVaultChanges)
        {
            foreach (var nfid in nonFungibleVaultChange.NonFungibleIds)
            {
                nonFungibles.Add(new NonFungibleIdLookup(nonFungibleVaultChange.ReferencedResource.DatabaseId, nfid));
            }
        }

        foreach (var nf in nonFungibles)
        {
            resourceManagerEntityIds.Add(nf.ResourceManagerEntityId);
            nonFungibleIds.Add(nf.NonFungibleId);
        }

        return await _dbContext.NonFungibleIdData
            .FromSqlInterpolated(@$"
SELECT * FROM non_fungible_id_data WHERE (non_fungible_resource_manager_entity_id, non_fungible_id) IN (
    SELECT UNNEST({resourceManagerEntityIds}), UNNEST({nonFungibleIds})
)")
            .AsNoTracking()
            .ToDictionaryAsync(e => new NonFungibleIdLookup(e.NonFungibleResourceManagerEntityId, e.NonFungibleId), token);
    }

    public async Task<Dictionary<ValidatorKeyLookup, ValidatorPublicKeyHistory>> ExistingValidatorKeysFor(List<ValidatorSetChange> validatorKeyLookups, CancellationToken token)
    {
        var validatorEntityIds = new List<long>();
        var validatorKeyTypes = new List<PublicKeyType>();
        var validatorKeys = new List<byte[]>();

        var lookupSet = new HashSet<ValidatorKeyLookup>();

        foreach (var (lookup, _) in validatorKeyLookups.SelectMany(change => change.ValidatorSet))
        {
            lookupSet.Add(lookup);
        }

        foreach (var lookup in lookupSet)
        {
            validatorEntityIds.Add(lookup.ValidatorEntityId);
            validatorKeyTypes.Add(lookup.PublicKeyType);
            validatorKeys.Add(lookup.PublicKey);
        }

        return await _dbContext.ValidatorKeyHistory
            .FromSqlInterpolated(@$"
SELECT * FROM validator_public_key_history WHERE (validator_entity_id, key_type, key) IN (
    SELECT UNNEST({validatorEntityIds}), UNNEST({validatorKeyTypes}), UNNEST({validatorKeys})
)")
            .AsNoTracking()
            .ToDictionaryAsync(e => new ValidatorKeyLookup(e.ValidatorEntityId, e.KeyType, e.Key), token);
    }

    public async Task<SequencesHolder> LoadSequences(CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
SELECT
    nextval('component_entity_state_history_id_seq') AS ComponentEntityStateHistorySequence,
    nextval('entities_id_seq') AS EntitySequence,
    nextval('entity_access_rules_chain_history_id_seq') AS EntityAccessRulesChainHistorySequence,
    nextval('entity_metadata_history_id_seq') AS EntityMetadataHistorySequence,
    nextval('entity_resource_aggregate_history_id_seq') AS EntityResourceAggregateHistorySequence,
    nextval('entity_vault_history_id_seq') AS EntityVaultHistorySequence,
    nextval('resource_manager_entity_supply_history_id_seq') AS ResourceManagerEntitySupplyHistorySequence,
    nextval('non_fungible_id_data_id_seq') AS NonFungibleIdDataSequence,
    nextval('non_fungible_id_mutable_data_history_id_seq') AS NonFungibleIdMutableDataHistorySequence,
    nextval('non_fungible_id_store_history_id_seq') AS NonFungibleIdStoreHistorySequence,
    nextval('validator_public_key_history_id_seq') AS ValidatorPublicKeyHistorySequence,
    nextval('validator_active_set_history_id_seq') AS ValidatorActiveSetHistorySequence",
            cancellationToken: token);

        return await _connection.QueryFirstAsync<SequencesHolder>(cd);
    }
}
