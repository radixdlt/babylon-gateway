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

using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class EntitySchemaProcessor : IProcessorBase, ISubstateUpsertProcessor, ISubstateDeleteProcessor
{
    private record struct SchemaDefinitionEntryDbLookup(long EntityId, ValueBytes SchemaHash);

    private record struct SchemaChangePointerLookup(long EntityId, long StateVersion);

    private record SchemaChangePointer
    {
        public List<CoreModel.SchemaEntrySubstate> Entries { get; } = new();

        public List<string> DeletedSchemaHashes { get; } = new();
    }

    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private readonly ChangeTracker<SchemaChangePointerLookup, SchemaChangePointer> _changes = new();

    private readonly Dictionary<long, SchemaEntryAggregateHistory> _mostRecentAggregates = new();
    private readonly Dictionary<SchemaDefinitionEntryDbLookup, SchemaEntryDefinition> _existingSchemas = new();

    private readonly List<SchemaEntryAggregateHistory> _aggregatesToAdd = new();
    private readonly List<SchemaEntryDefinition> _definitionsToAdd = new();
    private readonly List<NonFungibleSchemaHistory> _nonFungibleSchemaHistoryToAdd = new();
    private readonly List<KeyValueStoreSchemaHistory> _keyValueStoreSchemaHistoryToAdd = new();

    public EntitySchemaProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.SchemaEntrySubstate schemaEntry)
        {
            _changes
                .GetOrAdd(new SchemaChangePointerLookup(referencedEntity.DatabaseId, stateVersion), _ => new SchemaChangePointer())
                .Entries
                .Add(schemaEntry);
        }

        if (substateData is CoreModel.TypeInfoModuleFieldTypeInfoSubstate typeInfoSubstate)
        {
            if (typeInfoSubstate.TryGetNonFungibleDataSchemaDetails(out var nonFungibleDataSchemaDetails))
            {
                var schemaDefiningEntityId = !string.IsNullOrEmpty(nonFungibleDataSchemaDetails.Value.SchemaDefiningEntityAddress)
                    ? _referencedEntities.Get((EntityAddress)nonFungibleDataSchemaDetails.Value.SchemaDefiningEntityAddress).DatabaseId
                    : referencedEntity.DatabaseId;

                _nonFungibleSchemaHistoryToAdd.Add(
                    new NonFungibleSchemaHistory
                    {
                        Id = _context.Sequences.NonFungibleSchemaHistorySequence++,
                        ResourceEntityId = referencedEntity.DatabaseId,
                        SchemaHash = nonFungibleDataSchemaDetails.Value.SchemaHash.ConvertFromHex(),
                        SborTypeKind = nonFungibleDataSchemaDetails.Value.SborTypeKind.ToModel(),
                        TypeIndex = nonFungibleDataSchemaDetails.Value.TypeIndex,
                        SchemaDefiningEntityId = schemaDefiningEntityId,
                        FromStateVersion = stateVersion,
                    });
            }

            if (typeInfoSubstate.Value.Details is CoreModel.KeyValueStoreTypeInfoDetails
                && typeInfoSubstate.TryGetKeyValueStoreKeySchemaDetails(out var keySchemaDetails)
                && typeInfoSubstate.TryGetKeyValueStoreValueSchemaDetails(out var valueSchemaDetails))
            {
                var keySchemaDefiningEntityId = !string.IsNullOrEmpty(keySchemaDetails.Value.SchemaDefiningEntityAddress)
                    ? _referencedEntities.Get((EntityAddress)keySchemaDetails.Value.SchemaDefiningEntityAddress).DatabaseId
                    : referencedEntity.DatabaseId;

                var valueSchemaDefiningEntityId = !string.IsNullOrEmpty(valueSchemaDetails.Value.SchemaDefiningEntityAddress)
                    ? _referencedEntities.Get((EntityAddress)valueSchemaDetails.Value.SchemaDefiningEntityAddress).DatabaseId
                    : referencedEntity.DatabaseId;

                _keyValueStoreSchemaHistoryToAdd.Add(
                    new KeyValueStoreSchemaHistory
                    {
                        Id = _context.Sequences.KeyValueSchemaHistorySequence++,
                        KeyValueStoreEntityId = referencedEntity.DatabaseId,
                        KeySchemaDefiningEntityId = keySchemaDefiningEntityId,
                        KeySchemaHash = keySchemaDetails.Value.SchemaHash.ConvertFromHex(),
                        KeySborTypeKind = keySchemaDetails.Value.SborTypeKind.ToModel(),
                        KeyTypeIndex = keySchemaDetails.Value.TypeIndex,
                        ValueSchemaDefiningEntityId = valueSchemaDefiningEntityId,
                        ValueSchemaHash = valueSchemaDetails.Value.SchemaHash.ConvertFromHex(),
                        ValueSborTypeKind = valueSchemaDetails.Value.SborTypeKind.ToModel(),
                        ValueTypeIndex = valueSchemaDetails.Value.TypeIndex,
                        FromStateVersion = stateVersion,
                    });
            }
        }
    }

    public void VisitDelete(CoreModel.SubstateId substateId, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateId.SubstateType == CoreModel.SubstateType.SchemaEntry)
        {
            var keyHex = ((CoreModel.MapSubstateKey)substateId.SubstateKey).KeyHex;
            var schemaHash = ScryptoSborUtils.DataToProgrammaticScryptoSborValueBytes(keyHex.ConvertFromHex(), _context.NetworkConfiguration.Id);

            _changes
                .GetOrAdd(new SchemaChangePointerLookup(referencedEntity.DatabaseId, stateVersion), _ => new SchemaChangePointer())
                .DeletedSchemaHashes
                .Add(schemaHash.Hex);
        }
    }

    public async Task LoadDependenciesAsync()
    {
        _mostRecentAggregates.AddRange(await MostRecentSchemaEntryAggregateHistory());
        _existingSchemas.AddRange(await LoadExistingSchemas());
    }

    public void ProcessChanges()
    {
        foreach (var (lookup, change) in _changes.AsEnumerable())
        {
            SchemaEntryAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.EntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new SchemaEntryAggregateHistory
                {
                    Id = _context.Sequences.SchemaEntryAggregateHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    EntityId = lookup.EntityId,
                    EntryIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.EntryIds.AddRange(previousAggregate.EntryIds);
                }

                _aggregatesToAdd.Add(aggregate);
                _mostRecentAggregates[lookup.EntityId] = aggregate;
            }
            else
            {
                aggregate = previousAggregate;
            }

            foreach (var entry in change.Entries)
            {
                var entryDefinition = new SchemaEntryDefinition
                {
                    Id = _context.Sequences.SchemaEntryDefinitionSequence++,
                    FromStateVersion = lookup.StateVersion,
                    EntityId = lookup.EntityId,
                    SchemaHash = entry.Key.SchemaHash.ConvertFromHex(),
                    Schema = entry.Value.Schema.SborData.Hex.ConvertFromHex(),
                };

                _definitionsToAdd.Add(entryDefinition);

                aggregate.EntryIds.Insert(0, entryDefinition.Id);
            }

            foreach (var deletedSchemaHash in change.DeletedSchemaHashes)
            {
                var entryLookup = new SchemaDefinitionEntryDbLookup(lookup.EntityId, deletedSchemaHash.ConvertFromHex());

                if (_existingSchemas.TryGetValue(entryLookup, out var previousEntry))
                {
                    var currentPosition = aggregate.EntryIds.IndexOf(previousEntry.Id);

                    if (currentPosition != -1)
                    {
                        aggregate.EntryIds.RemoveAt(currentPosition);
                    }
                    else
                    {
                        throw new UnreachableException(
                            $"Unexpected situation where SchemaEntryDefinition with EntityId:{entryLookup.EntityId}, SchemaHash:{deletedSchemaHash} got deleted but wasn't found in aggregate table.");
                    }
                }
                else
                {
                    throw new UnreachableException(
                        $"Unexpected situation where SchemaEntryDefinition with EntityId:{entryLookup.EntityId}, SchemaHash:{deletedSchemaHash} got deleted but wasn't found in gateway database.");
                }
            }
        }
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;

        rowsInserted += await CopySchemaEntryDefinitions();
        rowsInserted += await CopySchemaEntryAggregateHistory();
        rowsInserted += await CopyNonFungibleDataSchemaHistory();
        rowsInserted += await CopyKeyValueStoreSchemaHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<long, SchemaEntryAggregateHistory>> MostRecentSchemaEntryAggregateHistory()
    {
        var entityIds = _changes.Keys.Select(x => x.EntityId).ToHashSet().ToList();

        if (!entityIds.Any())
        {
            return ImmutableDictionary<long, SchemaEntryAggregateHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<long, SchemaEntryAggregateHistory>(
            @$"
WITH variables (entity_id) AS (
    SELECT UNNEST({entityIds})
)
SELECT seah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM schema_entry_aggregate_history
    WHERE entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) seah ON true;",
            e => e.EntityId);
    }

    private async Task<IDictionary<SchemaDefinitionEntryDbLookup, SchemaEntryDefinition>> LoadExistingSchemas()
    {
        var lookupSet = new HashSet<SchemaDefinitionEntryDbLookup>();

        foreach (var (lookup, change) in _changes.AsEnumerable())
        {
            foreach (var deletedSchemaHash in change.DeletedSchemaHashes)
            {
                lookupSet.Add(new SchemaDefinitionEntryDbLookup(lookup.EntityId, deletedSchemaHash.ConvertFromHex()));
            }
        }

        if (!lookupSet.Unzip(x => x.EntityId, x => (byte[])x.SchemaHash, out var entityIds, out var schemaHashes))
        {
            return ImmutableDictionary<SchemaDefinitionEntryDbLookup, SchemaEntryDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<SchemaDefinitionEntryDbLookup, SchemaEntryDefinition>(
            @$"
SELECT *
FROM schema_entry_definition
WHERE (entity_id, schema_hash) IN (SELECT UNNEST({entityIds}), UNNEST({schemaHashes}));",
            e => new SchemaDefinitionEntryDbLookup(e.EntityId, e.SchemaHash));
    }

    private Task<int> CopySchemaEntryDefinitions() => _context.WriteHelper.Copy(
        _definitionsToAdd,
        "COPY schema_entry_definition (id, from_state_version, entity_id, schema_hash, schema) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.SchemaHash, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.Schema, NpgsqlDbType.Bytea, token);
        });

    private Task<int> CopySchemaEntryAggregateHistory() => _context.WriteHelper.Copy(
        _aggregatesToAdd,
        "COPY schema_entry_aggregate_history (id, from_state_version, entity_id, entry_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntryIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyNonFungibleDataSchemaHistory() => _context.WriteHelper.Copy(
        _nonFungibleSchemaHistoryToAdd,
        "COPY non_fungible_schema_history (id, from_state_version, resource_entity_id, schema_defining_entity_id, schema_hash, sbor_type_kind, type_index) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.SchemaDefiningEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.SchemaHash, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.SborTypeKind, "sbor_type_kind", token);
            await writer.WriteAsync(e.TypeIndex, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyKeyValueStoreSchemaHistory() => _context.WriteHelper.Copy(
        _keyValueStoreSchemaHistoryToAdd,
        "COPY key_value_store_schema_history (id, from_state_version, key_value_store_entity_id, key_schema_defining_entity_id, key_schema_hash, key_sbor_type_kind, key_type_index, value_schema_defining_entity_id, value_schema_hash, value_sbor_type_kind, value_type_index) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyValueStoreEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeySchemaDefiningEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeySchemaHash, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.KeySborTypeKind, "sbor_type_kind", token);
            await writer.WriteAsync(e.KeyTypeIndex, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ValueSchemaDefiningEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ValueSchemaHash, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.ValueSborTypeKind, "sbor_type_kind", token);
            await writer.WriteAsync(e.ValueTypeIndex, NpgsqlDbType.Bigint, token);
        });
}
