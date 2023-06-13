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
using RadixDlt.NetworkGateway.Abstractions;
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

    public async Task<Dictionary<MetadataLookup, EntityMetadataHistory>> MostRecentEntityMetadataHistoryFor(List<MetadataChange> metadataChanges, CancellationToken token)
    {
        var entityIds = new List<long>();
        var keys = new List<string>();
        var lookupSet = new HashSet<MetadataLookup>();

        foreach (var metadataChange in metadataChanges)
        {
            lookupSet.Add(new MetadataLookup(metadataChange.ReferencedEntity.DatabaseId, metadataChange.Key));
        }

        foreach (var lookup in lookupSet)
        {
            entityIds.Add(lookup.EntityId);
            keys.Add(lookup.Key);
        }

        return await _dbContext.EntityMetadataHistory
            .FromSqlInterpolated(@$"
WITH variables (entity_id, key) AS (
    SELECT UNNEST({entityIds}), UNNEST({keys})
)
SELECT emh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_metadata_history
    WHERE entity_id = variables.entity_id AND key = variables.key
    ORDER BY from_state_version DESC
    LIMIT 1
) emh ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => new MetadataLookup(e.EntityId, e.Key), token);
    }

    public async Task<Dictionary<long, EntityMetadataAggregateHistory>> MostRecentEntityAggregateMetadataHistoryFor(List<MetadataChange> metadataChanges, CancellationToken token)
    {
        var entityIds = metadataChanges.Select(x => x.ReferencedEntity.DatabaseId).Distinct().ToList();

        return await _dbContext.EntityMetadataAggregateHistory
            .FromSqlInterpolated(@$"
WITH variables (entity_id) AS (
    SELECT UNNEST({entityIds})
)
SELECT emah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_metadata_aggregate_history
    WHERE entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) emah ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => e.EntityId, token);
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

    public async Task<Dictionary<EntityResourceLookup, EntityResourceAggregatedVaultsHistory>> MostRecentEntityResourceAggregatedVaultsHistoryFor(List<FungibleVaultChange> fungibleVaultChanges, List<NonFungibleVaultChange> nonFungibleVaultChanges, CancellationToken token)
    {
        var data = new HashSet<EntityResourceLookup>();

        foreach (var change in fungibleVaultChanges)
        {
            data.Add(new EntityResourceLookup(change.ReferencedVault.DatabaseOwnerAncestorId, change.ReferencedResource.DatabaseId));
            data.Add(new EntityResourceLookup(change.ReferencedVault.DatabaseGlobalAncestorId, change.ReferencedResource.DatabaseId));
        }

        foreach (var change in nonFungibleVaultChanges)
        {
            data.Add(new EntityResourceLookup(change.ReferencedVault.DatabaseOwnerAncestorId, change.ReferencedResource.DatabaseId));
            data.Add(new EntityResourceLookup(change.ReferencedVault.DatabaseGlobalAncestorId, change.ReferencedResource.DatabaseId));
        }

        var entityIds = new List<long>();
        var resourceEntityIds = new List<long>();

        foreach (var d in data)
        {
            entityIds.Add(d.EntityId);
            resourceEntityIds.Add(d.ResourceEntityId);
        }

        return await _dbContext.EntityResourceAggregatedVaultsHistory
            .FromSqlInterpolated(@$"
WITH variables (entity_id, resource_entity_id) AS (
    SELECT UNNEST({entityIds}), UNNEST({resourceEntityIds})
)
SELECT eravh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_resource_aggregated_vaults_history
    WHERE entity_id = variables.entity_id AND resource_entity_id = variables.resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) eravh ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => new EntityResourceLookup(e.EntityId, e.ResourceEntityId), token);
    }

    public async Task<Dictionary<EntityResourceVaultLookup, EntityResourceVaultAggregateHistory>> MostRecentEntityResourceVaultAggregateHistoryFor(List<FungibleVaultChange> fungibleVaultChanges, List<NonFungibleVaultChange> nonFungibleVaultChanges, CancellationToken token)
    {
        var data = new HashSet<EntityResourceVaultLookup>();

        foreach (var change in fungibleVaultChanges)
        {
            data.Add(new EntityResourceVaultLookup(change.ReferencedVault.DatabaseOwnerAncestorId, change.ReferencedResource.DatabaseId));
            data.Add(new EntityResourceVaultLookup(change.ReferencedVault.DatabaseGlobalAncestorId, change.ReferencedResource.DatabaseId));
        }

        foreach (var change in nonFungibleVaultChanges)
        {
            data.Add(new EntityResourceVaultLookup(change.ReferencedVault.DatabaseOwnerAncestorId, change.ReferencedResource.DatabaseId));
            data.Add(new EntityResourceVaultLookup(change.ReferencedVault.DatabaseGlobalAncestorId, change.ReferencedResource.DatabaseId));
        }

        var entityIds = new List<long>();
        var resourceEntityIds = new List<long>();

        foreach (var d in data)
        {
            entityIds.Add(d.EntityId);
            resourceEntityIds.Add(d.ResourceEntityId);
        }

        return await _dbContext.EntityResourceVaultAggregateHistory
            .FromSqlInterpolated(@$"
WITH variables (entity_id, resource_entity_id) AS (
    SELECT UNNEST({entityIds}), UNNEST({resourceEntityIds})
)
SELECT ervah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_resource_vault_aggregate_history
    WHERE entity_id = variables.entity_id AND resource_entity_id = variables.resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) ervah ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => new EntityResourceVaultLookup(e.EntityId, e.ResourceEntityId), token);
    }

    public async Task<Dictionary<long, EntityNonFungibleVaultHistory>> MostRecentEntityNonFungibleVaultHistory(List<NonFungibleVaultChange> nonFungibleVaultChanges, CancellationToken token)
    {
        var vaultIds = nonFungibleVaultChanges.Select(x => x.ReferencedVault.DatabaseId).Distinct().ToList();

        return await _dbContext.EntityVaultHistory
            .FromSqlInterpolated(@$"
WITH variables (vault_entity_id) AS (
    SELECT UNNEST({vaultIds})
)
SELECT evh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_vault_history
    WHERE vault_entity_id = variables.vault_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) evh ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => e.VaultEntityId, e => (EntityNonFungibleVaultHistory)e, token);
    }

    public async Task<Dictionary<long, NonFungibleIdStoreHistory>> MostRecentNonFungibleIdStoreHistoryFor(List<NonFungibleIdChange> nonFungibleIdStoreChanges, CancellationToken token)
    {
        var ids = nonFungibleIdStoreChanges.Select(x => x.ReferencedResource.DatabaseId).Distinct().ToList();

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
    WHERE non_fungible_resource_entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) emh ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => e.NonFungibleResourceEntityId, token);
    }

    public async Task<Dictionary<long, ResourceEntitySupplyHistory>> MostRecentResourceEntitySupplyHistoryFor(List<ResourceSupplyChange> resourceSupplyChanges, CancellationToken token)
    {
        var ids = resourceSupplyChanges.Select(c => c.ResourceEntityId).Distinct().ToList();

        return await _dbContext.ResourceEntitySupplyHistory
            .FromSqlInterpolated(@$"
WITH variables (resource_entity_id) AS (
    SELECT UNNEST({ids})
)
SELECT rmesh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM resource_entity_supply_history
    WHERE resource_entity_id = variables.resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) rmesh ON true;")
            .AsNoTracking()
            .ToDictionaryAsync(e => e.ResourceEntityId, token);
    }

    public async Task<Dictionary<EntityAddress, Entity>> ExistingEntitiesFor(ReferencedEntityDictionary referencedEntities, CancellationToken token)
    {
        var entityAddressesToLoad = referencedEntities.Addresses.Select(x => (string)x).ToList();
        var knownAddressesToLoad = referencedEntities.KnownAddresses.Select(x => (string)x).ToList();
        var entityAddressesParameter = new NpgsqlParameter("@entity_addresses", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = entityAddressesToLoad.Concat(knownAddressesToLoad).ToArray(),
        };

        return await _dbContext.Entities
            .FromSqlInterpolated($@"
SELECT *
FROM entities
WHERE id IN(
    SELECT UNNEST(id || correlated_entities) AS id
    FROM entities
    WHERE address = ANY({entityAddressesParameter})
)")
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Address, token);
    }

    public async Task<Dictionary<NonFungibleIdLookup, NonFungibleIdData>> ExistingNonFungibleIdDataFor(List<NonFungibleIdChange> nonFungibleIdStoreChanges, List<NonFungibleVaultChange> nonFungibleVaultChanges, CancellationToken token)
    {
        var nonFungibles = new HashSet<NonFungibleIdLookup>();
        var resourceEntityIds = new List<long>();
        var nonFungibleIds = new List<string>();

        foreach (var nonFungibleIdChange in nonFungibleIdStoreChanges)
        {
            nonFungibles.Add(new NonFungibleIdLookup(nonFungibleIdChange.ReferencedResource.DatabaseId, nonFungibleIdChange.NonFungibleId));
        }

        foreach (var nonFungibleVaultChange in nonFungibleVaultChanges)
        {
            nonFungibles.Add(new NonFungibleIdLookup(nonFungibleVaultChange.ReferencedResource.DatabaseId, nonFungibleVaultChange.NonFungibleId));
        }

        foreach (var nf in nonFungibles)
        {
            resourceEntityIds.Add(nf.ResourceEntityId);
            nonFungibleIds.Add(nf.NonFungibleId);
        }

        return await _dbContext.NonFungibleIdData
            .FromSqlInterpolated(@$"
SELECT * FROM non_fungible_id_data WHERE (non_fungible_resource_entity_id, non_fungible_id) IN (
    SELECT UNNEST({resourceEntityIds}), UNNEST({nonFungibleIds})
)")
            .AsNoTracking()
            .ToDictionaryAsync(e => new NonFungibleIdLookup(e.NonFungibleResourceEntityId, e.NonFungibleId), token);
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
WITH variables (validator_entity_id, key_type, key) AS (
    SELECT UNNEST({validatorEntityIds}), UNNEST({validatorKeyTypes}), UNNEST({validatorKeys})
)
SELECT vpkh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM validator_public_key_history
    WHERE validator_entity_id = variables.validator_entity_id AND key_type = variables.key_type AND key = variables.key
    ORDER BY from_state_version DESC
    LIMIT 1
) vpkh ON true;
")
            .AsNoTracking()
            .ToDictionaryAsync(e => new ValidatorKeyLookup(e.ValidatorEntityId, e.KeyType, e.Key), token);
    }

    public async Task<SequencesHolder> LoadSequences(CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
SELECT
    nextval('account_default_deposit_rule_history_id_seq') AS AccountDefaultDepositRuleHistorySequence,
    nextval('account_resource_deposit_rule_history_id_seq') AS AccountResourceDepositRuleHistorySequence,
    nextval('entity_state_history_id_seq') AS EntityStateHistorySequence,
    nextval('entities_id_seq') AS EntitySequence,
    nextval('entity_access_rules_chain_history_id_seq') AS EntityAccessRulesChainHistorySequence,
    nextval('entity_metadata_history_id_seq') AS EntityMetadataHistorySequence,
    nextval('entity_metadata_aggregate_history_id_seq') AS EntityMetadataAggregateHistorySequence,
    nextval('entity_resource_aggregated_vaults_history_id_seq') AS EntityResourceAggregatedVaultsHistorySequence,
    nextval('entity_resource_aggregate_history_id_seq') AS EntityResourceAggregateHistorySequence,
    nextval('entity_resource_vault_aggregate_history_id_seq') AS EntityResourceVaultAggregateHistorySequence,
    nextval('entity_vault_history_id_seq') AS EntityVaultHistorySequence,
    nextval('resource_entity_supply_history_id_seq') AS ResourceEntitySupplyHistorySequence,
    nextval('non_fungible_id_data_id_seq') AS NonFungibleIdDataSequence,
    nextval('non_fungible_id_data_history_id_seq') AS NonFungibleIdDataHistorySequence,
    nextval('non_fungible_id_store_history_id_seq') AS NonFungibleIdStoreHistorySequence,
    nextval('validator_public_key_history_id_seq') AS ValidatorPublicKeyHistorySequence,
    nextval('validator_active_set_history_id_seq') AS ValidatorActiveSetHistorySequence,
    nextval('ledger_transaction_markers_id_seq') AS LedgerTransactionMarkerSequence",
            cancellationToken: token);

        return await _connection.QueryFirstAsync<SequencesHolder>(cd);
    }
}
