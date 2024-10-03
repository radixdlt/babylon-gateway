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
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct KeyValueStoreEntryDbLookup(long KeyValueStoreEntityId, ValueBytes Key);

internal record struct KeyValueStoreChangePointerLookup(long KeyValueStoreEntityId, long StateVersion, ValueBytes Key);

internal record KeyValueStoreChangePointer(CoreModel.GenericKeyValueStoreEntrySubstate NewSubstate, CoreModel.GenericKeyValueStoreEntrySubstate? PreviousSubstate);

internal class KeyValueStoreProcessor
{
    private readonly ProcessorContext _context;

    private readonly Dictionary<KeyValueStoreEntryDbLookup, long> _observedEntryDefinitions = new();
    private readonly Dictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryDefinition> _existingEntryDefinitions = new();
    private readonly Dictionary<long, KeyValueStoreTotalsHistory> _existingTotalsHistory = new();

    private readonly List<KeyValueStoreEntryDefinition> _entryDefinitionsToAdd = new();
    private readonly List<KeyValueStoreEntryHistory> _entryHistoryToAdd = new();
    private readonly List<KeyValueStoreTotalsHistory> _totalsHistoryToAdd = new();

    private ChangeTracker<KeyValueStoreChangePointerLookup, KeyValueStoreChangePointer> _changes = new();

    public KeyValueStoreProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.GenericKeyValueStoreEntrySubstate genericKeyValueStoreEntry)
        {
            var key = (ValueBytes)genericKeyValueStoreEntry.Key.KeyData.GetDataBytes();
            var kvStoreEntryLookup = new KeyValueStoreChangePointerLookup(referencedEntity.DatabaseId, stateVersion, key);

            _changes.Add(kvStoreEntryLookup, new KeyValueStoreChangePointer(genericKeyValueStoreEntry, substate.PreviousValue?.SubstateData as CoreModel.GenericKeyValueStoreEntrySubstate));
            _observedEntryDefinitions.TryAdd(new KeyValueStoreEntryDbLookup(referencedEntity.DatabaseId, key), stateVersion);
        }
    }

    public async Task LoadDependencies()
    {
        _existingEntryDefinitions.AddRange(await ExistingKeyValueStoreEntryDefinitions());
        _existingTotalsHistory.AddRange(await ExistingKeyValueStoreTotalsHistory());
    }

    public void ProcessChanges()
    {
        foreach (var lookup in _observedEntryDefinitions.Keys.Except(_existingEntryDefinitions.Keys))
        {
            var entryDefinition = new KeyValueStoreEntryDefinition
            {
                Id = _context.Sequences.KeyValueStoreEntryDefinitionSequence++,
                FromStateVersion = _observedEntryDefinitions[lookup],
                KeyValueStoreEntityId = lookup.KeyValueStoreEntityId,
                Key = lookup.Key,
            };

            _entryDefinitionsToAdd.Add(entryDefinition);
            _existingEntryDefinitions[lookup] = entryDefinition;
        }

        foreach (var change in _changes.AsEnumerable())
        {
            var isDeleted = change.Value.NewSubstate.Value == null;

            _entryHistoryToAdd.Add(new KeyValueStoreEntryHistory
            {
                Id = _context.Sequences.KeyValueStoreEntryHistorySequence++,
                FromStateVersion = change.Key.StateVersion,
                KeyValueStoreEntryDefinitionId = _existingEntryDefinitions[new KeyValueStoreEntryDbLookup(change.Key.KeyValueStoreEntityId, change.Key.Key)].Id,
                Value = isDeleted ? null : change.Value.NewSubstate.Value!.Data.StructData.GetDataBytes(),
                IsDeleted = isDeleted,
                IsLocked = change.Value.NewSubstate.IsLocked,
            });

            var totalsExists = _existingTotalsHistory.TryGetValue(change.Key.KeyValueStoreEntityId, out var previousTotals);

            var newTotals = new KeyValueStoreTotalsHistory
            {
                Id = _context.Sequences.EntityMetadataTotalsHistorySequence++,
                FromStateVersion = change.Key.StateVersion,
                EntityId = change.Key.KeyValueStoreEntityId,
                TotalEntriesExcludingDeleted = totalsExists ? previousTotals!.TotalEntriesExcludingDeleted : 0,
                TotalEntriesIncludingDeleted = totalsExists ? previousTotals!.TotalEntriesIncludingDeleted : 0,
            };

            switch (change.Value)
            {
                case { PreviousSubstate: null, NewSubstate.Value: null }:
                    newTotals.TotalEntriesIncludingDeleted++;
                    break;
                case { PreviousSubstate: null, NewSubstate.Value: not null }:
                    newTotals.TotalEntriesIncludingDeleted++;
                    newTotals.TotalEntriesExcludingDeleted++;
                    break;
                case { PreviousSubstate.Value: not null, NewSubstate.Value: null }:
                    newTotals.TotalEntriesExcludingDeleted--;
                    break;
                case { PreviousSubstate.Value: null, NewSubstate.Value: not null }:
                    newTotals.TotalEntriesExcludingDeleted++;
                    break;
            }

            _existingTotalsHistory[change.Key.KeyValueStoreEntityId] = newTotals;
            _totalsHistoryToAdd.Add(newTotals);
        }
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyKeyValueStoreEntryDefinition();
        rowsInserted += await CopyKeyValueStoreEntryHistory();
        rowsInserted += await CopyKeyValueStoreTotalsHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryDefinition>> ExistingKeyValueStoreEntryDefinitions()
    {
        if (!_observedEntryDefinitions.Keys.ToHashSet().Unzip(x => x.KeyValueStoreEntityId, x => (byte[])x.Key, out var keyValueStoreEntityIds, out var keys))
        {
            return ImmutableDictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<KeyValueStoreEntryDbLookup, KeyValueStoreEntryDefinition>(
            @$"
WITH variables (key_value_store_entity_id, key) AS (
    SELECT UNNEST({keyValueStoreEntityIds}), UNNEST({keys})
)
SELECT *
FROM key_value_store_entry_definition
WHERE (key_value_store_entity_id, key) IN (SELECT * FROM variables)",
            e => new KeyValueStoreEntryDbLookup(e.KeyValueStoreEntityId, (ValueBytes)e.Key));
    }

    private async Task<IDictionary<long, KeyValueStoreTotalsHistory>> ExistingKeyValueStoreTotalsHistory()
    {
        var entityIds = _observedEntryDefinitions
            .Keys
            .Select(x => x.KeyValueStoreEntityId)
            .ToHashSet();

        if (entityIds.Count == 0)
        {
            return ImmutableDictionary<long, KeyValueStoreTotalsHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<long, KeyValueStoreTotalsHistory>(
            @$"
WITH variables (entity_id) AS (
    SELECT UNNEST({entityIds})
)
SELECT kvsth.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM key_value_store_totals_history
    WHERE entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) kvsth ON true;",
            e => e.EntityId);
    }

    private Task<int> CopyKeyValueStoreEntryDefinition() => _context.WriteHelper.Copy(
        _entryDefinitionsToAdd,
        "COPY key_value_store_entry_definition (id, from_state_version, key_value_store_entity_id, key) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyValueStoreEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Key, NpgsqlDbType.Bytea, token);
        });

    private Task<int> CopyKeyValueStoreEntryHistory() => _context.WriteHelper.Copy(
        _entryHistoryToAdd,
        "COPY key_value_store_entry_history (id, from_state_version, key_value_store_entry_definition_id, value, is_deleted, is_locked) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyValueStoreEntryDefinitionId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Value?.ToArray(), NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
        });

    private Task<int> CopyKeyValueStoreTotalsHistory() => _context.WriteHelper.Copy(
        _totalsHistoryToAdd,
        "COPY key_value_store_totals_history (id, from_state_version, entity_id, total_entries_including_deleted, total_entries_excluding_deleted) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalEntriesIncludingDeleted, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalEntriesExcludingDeleted, NpgsqlDbType.Bigint, token);
        });
}
