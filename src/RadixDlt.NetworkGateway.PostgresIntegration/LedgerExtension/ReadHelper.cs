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
using Microsoft.EntityFrameworkCore.Query;
using Npgsql;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PublicKeyType = RadixDlt.NetworkGateway.Abstractions.Model.PublicKeyType;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record NonFungibleMap(EntityAddress ResourceAddress, string SimpleRepresentation, long ResourceEntityId, long NonFungibleIdDataId);

internal class ReadHelper : IReadHelper
{
    private readonly ReadWriteDbContext _dbContext;
    private readonly NpgsqlConnection _connection;
    private readonly IEnumerable<ILedgerExtenderServiceObserver> _observers;
    private readonly CancellationToken _token;
    private readonly IDapperWrapper _dapperWrapper;

    public ReadHelper(ReadWriteDbContext dbContext, IEnumerable<ILedgerExtenderServiceObserver> observers, IDapperWrapper dapperWrapper, CancellationToken token = default)
    {
        _dbContext = dbContext;
        _connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        _observers = observers;
        _dapperWrapper = dapperWrapper;
        _token = token;
    }

    public async Task<Dictionary<TKey, TValue>> LoadDependencies<TKey, TValue>([NotParameterized] FormattableString sql, Func<TValue, TKey> keySelector, [CallerMemberName] string stageName = "")
        where TKey : notnull
        where TValue : class
    {
        var sw = Stopwatch.GetTimestamp();

        var result = await _dbContext
            .Set<TValue>()
            .FromSqlInterpolated(sql)
            .AsNoTracking()
            .AnnotateMetricName(stageName)
            .ToDictionaryAsync(keySelector, _token);

        await _observers.ForEachAsync(x => x.StageCompleted(stageName, Stopwatch.GetElapsedTime(sw), result.Count));

        return result;
    }

    public async Task<Dictionary<long, EntityResourceAggregateHistory>> MostRecentEntityResourceAggregateHistoryFor(List<IVaultSnapshot> vaultSnapshots, CancellationToken token)
    {
        if (!vaultSnapshots.Any())
        {
            return new Dictionary<long, EntityResourceAggregateHistory>();
        }

        var sw = Stopwatch.GetTimestamp();
        var entityIds = new HashSet<long>();

        foreach (var change in vaultSnapshots)
        {
            entityIds.Add(change.ReferencedVault.DatabaseOwnerAncestorId);
            entityIds.Add(change.ReferencedVault.DatabaseGlobalAncestorId);
        }

        var ids = entityIds.ToList();

        var result = await _dbContext
            .EntityResourceAggregateHistory
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
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.EntityId, token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(MostRecentEntityResourceAggregateHistoryFor), Stopwatch.GetElapsedTime(sw), result.Count));

        return result;
    }

    public async Task<Dictionary<EntityResourceLookup, EntityResourceAggregatedVaultsHistory>> MostRecentEntityResourceAggregatedVaultsHistoryFor(
        List<IVaultChange> vaultChanges,
        CancellationToken token)
    {
        if (!vaultChanges.Any())
        {
            return new Dictionary<EntityResourceLookup, EntityResourceAggregatedVaultsHistory>();
        }

        var sw = Stopwatch.GetTimestamp();
        var entityIds = new List<long>();
        var resourceEntityIds = new List<long>();

        foreach (var d in vaultChanges.Select(e => new EntityResourceLookup(e.EntityId, e.ResourceEntityId)).ToHashSet())
        {
            entityIds.Add(d.EntityId);
            resourceEntityIds.Add(d.ResourceEntityId);
        }

        var result = await _dbContext
            .EntityResourceAggregatedVaultsHistory
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
            .AnnotateMetricName()
            .ToDictionaryAsync(e => new EntityResourceLookup(e.EntityId, e.ResourceEntityId), token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(MostRecentEntityResourceAggregatedVaultsHistoryFor), Stopwatch.GetElapsedTime(sw), result.Count));

        return result;
    }

    public async Task<Dictionary<EntityResourceVaultLookup, EntityResourceVaultAggregateHistory>> MostRecentEntityResourceVaultAggregateHistoryFor(
        List<IVaultSnapshot> vaultSnapshots,
        CancellationToken token)
    {
        if (!vaultSnapshots.Any())
        {
            return new Dictionary<EntityResourceVaultLookup, EntityResourceVaultAggregateHistory>();
        }

        var sw = Stopwatch.GetTimestamp();
        var data = new HashSet<EntityResourceVaultLookup>();

        foreach (var change in vaultSnapshots)
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

        var result = await _dbContext
            .EntityResourceVaultAggregateHistory
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
            .AnnotateMetricName()
            .ToDictionaryAsync(e => new EntityResourceVaultLookup(e.EntityId, e.ResourceEntityId), token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(MostRecentEntityResourceVaultAggregateHistoryFor), Stopwatch.GetElapsedTime(sw), result.Count));

        return result;
    }

    public async Task<Dictionary<long, EntityNonFungibleVaultHistory>> MostRecentEntityNonFungibleVaultHistory(List<NonFungibleVaultSnapshot> nonFungibleVaultSnapshots, CancellationToken token)
    {
        if (!nonFungibleVaultSnapshots.Any())
        {
            return new Dictionary<long, EntityNonFungibleVaultHistory>();
        }

        var sw = Stopwatch.GetTimestamp();
        var vaultIds = nonFungibleVaultSnapshots.Select(x => x.ReferencedVault.DatabaseId).ToHashSet().ToList();

        var result = await _dbContext
            .EntityVaultHistory
            .FromSqlInterpolated(@$"
WITH variables (vault_entity_id) AS (
    SELECT UNNEST({vaultIds})
)
SELECT evh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_vault_history
    WHERE vault_entity_id = variables.vault_entity_id AND discriminator = 'non_fungible'
    ORDER BY from_state_version DESC
    LIMIT 1
) evh ON true;")
            .AsNoTracking()
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.VaultEntityId, e => (EntityNonFungibleVaultHistory)e, token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(MostRecentEntityNonFungibleVaultHistory), Stopwatch.GetElapsedTime(sw), result.Count));

        return result;
    }

    public async Task<Dictionary<long, NonFungibleIdStoreHistory>> MostRecentNonFungibleIdStoreHistoryFor(List<NonFungibleIdChange> nonFungibleIdStoreChanges, CancellationToken token)
    {
        if (!nonFungibleIdStoreChanges.Any())
        {
            return new Dictionary<long, NonFungibleIdStoreHistory>();
        }

        var sw = Stopwatch.GetTimestamp();
        var ids = nonFungibleIdStoreChanges.Select(x => x.ReferencedResource.DatabaseId).Distinct().ToList();

        var result = await _dbContext
            .NonFungibleIdStoreHistory
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
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.NonFungibleResourceEntityId, token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(MostRecentNonFungibleIdStoreHistoryFor), Stopwatch.GetElapsedTime(sw), result.Count));

        return result;
    }

    public async Task<Dictionary<long, ResourceEntitySupplyHistory>> MostRecentResourceEntitySupplyHistoryFor(List<ResourceSupplyChange> resourceSupplyChanges, CancellationToken token)
    {
        if (!resourceSupplyChanges.Any())
        {
            return new Dictionary<long, ResourceEntitySupplyHistory>();
        }

        var sw = Stopwatch.GetTimestamp();
        var ids = resourceSupplyChanges.Select(c => c.ResourceEntityId).Distinct().ToList();

        var result = await _dbContext
            .ResourceEntitySupplyHistory
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
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.ResourceEntityId, token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(MostRecentResourceEntitySupplyHistoryFor), Stopwatch.GetElapsedTime(sw), result.Count));

        return result;
    }

    public async Task<Dictionary<EntityAddress, Entity>> ExistingEntitiesFor(ReferencedEntityDictionary referencedEntities, CancellationToken token)
    {
        var sw = Stopwatch.GetTimestamp();
        var entityAddressesToLoad = referencedEntities.Addresses.Select(x => (string)x).ToList();
        var knownAddressesToLoad = referencedEntities.KnownAddresses.Select(x => (string)x).ToList();
        var addressesToLoad = entityAddressesToLoad.Concat(knownAddressesToLoad).ToList();

        var result = await _dbContext
            .Entities
            .FromSqlInterpolated($@"
SELECT *
FROM entities
WHERE id IN(
    SELECT UNNEST(id || correlated_entities) AS id
    FROM entities
    WHERE address = ANY({addressesToLoad})
)")
            .AsNoTracking()
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Address, token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(ExistingEntitiesFor), Stopwatch.GetElapsedTime(sw), result.Count));

        return result;
    }

    public async Task<Dictionary<NonFungibleGlobalIdLookup, NonFungibleIdGlobalIdDatabaseId>> ReadNonFungibleData(
        HashSet<NonFungibleGlobalIdLookup> referencedNonFungibleIds,
        CancellationToken token)
    {
        if (!referencedNonFungibleIds.Any())
        {
            return new Dictionary<NonFungibleGlobalIdLookup, NonFungibleIdGlobalIdDatabaseId>();
        }

        var sw = Stopwatch.GetTimestamp();

        var resourceAddresses = new List<string>();
        var nonFungibleIds = new List<string>();

        foreach (var nf in referencedNonFungibleIds)
        {
            resourceAddresses.Add(nf.ResourceAddress);
            nonFungibleIds.Add(nf.SimpleRepresentation);
        }

        var cd = new CommandDefinition(
            commandText: $@"
WITH var (non_fungible_resource_address, non_fungible_id) AS (
    SELECT UNNEST(@resourceAddresses), UNNEST(@nonFungibleIds)
)
SELECT
    var.non_fungible_resource_address AS ResourceAddress,
    var.non_fungible_id AS SimpleRepresentation,
    nfid.non_fungible_resource_entity_id AS ResourceEntityId,
    nfid.id AS NonFungibleIdDataId
FROM var
INNER JOIN entities e ON e.address = var.non_fungible_resource_address
INNER JOIN non_fungible_id_data nfid ON nfid.non_fungible_resource_entity_id = e.id AND nfid.non_fungible_id = var.non_fungible_id",
            parameters: new
            {
                resourceAddresses = resourceAddresses,
                nonFungibleIds = nonFungibleIds,
            },
            cancellationToken: token);

        var result = (await _dapperWrapper.QueryAsync<NonFungibleMap>(_connection, cd)).ToList();

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(ReadNonFungibleData), Stopwatch.GetElapsedTime(sw), result.Count));

        return result.ToDictionary(x => new NonFungibleGlobalIdLookup(x.ResourceAddress, x.SimpleRepresentation), x => new NonFungibleIdGlobalIdDatabaseId(x.ResourceEntityId, x.NonFungibleIdDataId));
    }

    public async Task<Dictionary<NonFungibleIdLookup, NonFungibleIdData>> ExistingNonFungibleIdDataFor(
        List<NonFungibleIdChange> nonFungibleIdStoreChanges,
        List<NonFungibleVaultSnapshot> nonFungibleVaultSnapshots,
        CancellationToken token)
    {
        if (!nonFungibleIdStoreChanges.Any() && !nonFungibleVaultSnapshots.Any())
        {
            return new Dictionary<NonFungibleIdLookup, NonFungibleIdData>();
        }

        var sw = Stopwatch.GetTimestamp();
        var nonFungibles = new HashSet<NonFungibleIdLookup>();
        var resourceEntityIds = new List<long>();
        var nonFungibleIds = new List<string>();

        foreach (var nonFungibleIdChange in nonFungibleIdStoreChanges)
        {
            nonFungibles.Add(new NonFungibleIdLookup(nonFungibleIdChange.ReferencedResource.DatabaseId, nonFungibleIdChange.NonFungibleId));
        }

        foreach (var nonFungibleVaultChange in nonFungibleVaultSnapshots)
        {
            nonFungibles.Add(new NonFungibleIdLookup(nonFungibleVaultChange.ReferencedResource.DatabaseId, nonFungibleVaultChange.NonFungibleId));
        }

        foreach (var nf in nonFungibles)
        {
            resourceEntityIds.Add(nf.ResourceEntityId);
            nonFungibleIds.Add(nf.NonFungibleId);
        }

        var result = await _dbContext
            .NonFungibleIdData
            .FromSqlInterpolated(@$"
SELECT * FROM non_fungible_id_data WHERE (non_fungible_resource_entity_id, non_fungible_id) IN (
    SELECT UNNEST({resourceEntityIds}), UNNEST({nonFungibleIds})
)")
            .AsNoTracking()
            .AnnotateMetricName()
            .ToDictionaryAsync(e => new NonFungibleIdLookup(e.NonFungibleResourceEntityId, e.NonFungibleId), token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(ExistingNonFungibleIdDataFor), Stopwatch.GetElapsedTime(sw), result.Count));

        return result;
    }

    public async Task<SequencesHolder> LoadSequences(CancellationToken token)
    {
        var sw = Stopwatch.GetTimestamp();
        var cd = new CommandDefinition(
            commandText: @"
SELECT
    nextval('account_default_deposit_rule_history_id_seq') AS AccountDefaultDepositRuleHistorySequence,
    nextval('account_resource_preference_rule_entry_history_id_seq') AS AccountResourcePreferenceRuleEntryHistorySequence,
    nextval('account_resource_preference_rule_aggregate_history_id_seq') AS AccountResourcePreferenceRuleAggregateHistorySequence,
    nextval('state_history_id_seq') AS StateHistorySequence,
    nextval('entities_id_seq') AS EntitySequence,
    nextval('entity_metadata_history_id_seq') AS EntityMetadataHistorySequence,
    nextval('entity_metadata_aggregate_history_id_seq') AS EntityMetadataAggregateHistorySequence,
    nextval('entity_resource_aggregated_vaults_history_id_seq') AS EntityResourceAggregatedVaultsHistorySequence,
    nextval('entity_resource_aggregate_history_id_seq') AS EntityResourceAggregateHistorySequence,
    nextval('entity_resource_vault_aggregate_history_id_seq') AS EntityResourceVaultAggregateHistorySequence,
    nextval('entity_vault_history_id_seq') AS EntityVaultHistorySequence,
    nextval('entity_role_assignments_aggregate_history_id_seq') AS EntityRoleAssignmentsAggregateHistorySequence,
    nextval('entity_role_assignments_entry_history_id_seq') AS EntityRoleAssignmentsEntryHistorySequence,
    nextval('entity_role_assignments_owner_role_history_id_seq') AS EntityRoleAssignmentsOwnerRoleHistorySequence,
    nextval('component_method_royalty_entry_history_id_seq') AS ComponentMethodRoyaltyEntryHistorySequence,
    nextval('component_method_royalty_aggregate_history_id_seq') AS ComponentMethodRoyaltyAggregateHistorySequence,
    nextval('resource_entity_supply_history_id_seq') AS ResourceEntitySupplyHistorySequence,
    nextval('non_fungible_id_data_id_seq') AS NonFungibleIdDataSequence,
    nextval('non_fungible_id_data_history_id_seq') AS NonFungibleIdDataHistorySequence,
    nextval('non_fungible_id_store_history_id_seq') AS NonFungibleIdStoreHistorySequence,
    nextval('non_fungible_id_location_history_id_seq') AS NonFungibleIdLocationHistorySequence,
    nextval('validator_public_key_history_id_seq') AS ValidatorPublicKeyHistorySequence,
    nextval('validator_active_set_history_id_seq') AS ValidatorActiveSetHistorySequence,
    nextval('ledger_transaction_markers_id_seq') AS LedgerTransactionMarkerSequence,
    nextval('package_blueprint_history_id_seq') AS PackageBlueprintHistorySequence,
    nextval('package_code_history_id_seq') AS PackageCodeHistorySequence,
    nextval('schema_history_id_seq') AS SchemaHistorySequence,
    nextval('key_value_store_entry_definition_id_seq') AS KeyValueStoreEntryDefinitionSequence,
    nextval('key_value_store_entry_history_id_seq') AS KeyValueStoreEntryHistorySequence,
    nextval('validator_emission_statistics_id_seq') AS ValidatorEmissionStatisticsSequence,
    nextval('non_fungible_schema_history_id_seq') AS NonFungibleSchemaHistorySequence,
    nextval('key_value_store_schema_history_id_seq') AS KeyValueSchemaHistorySequence,
    nextval('package_blueprint_aggregate_history_id_seq') AS PackageBlueprintAggregateHistorySequence,
    nextval('package_code_aggregate_history_id_seq') AS PackageCodeAggregateHistorySequence,
    nextval('account_authorized_depositor_entry_history_id_seq') AS AccountAuthorizedDepositorEntryHistorySequence,
    nextval('account_authorized_depositor_aggregate_history_id_seq') AS AccountAuthorizedDepositorAggregateHistorySequence
",
            cancellationToken: token);

        var result = await _connection.QueryFirstAsync<SequencesHolder>(cd);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(LoadSequences), Stopwatch.GetElapsedTime(sw), null));

        return result;
    }
}
