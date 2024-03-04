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
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct KeyValueStoreEntryDbLookup(long KeyValueStoreEntityId, ValueBytes Key);

internal record struct KeyValueStoreChangePointerLookup(long KeyValueStoreEntityId, long StateVersion, ValueBytes Key);

internal record KeyValueStoreChangePointer(ReferencedEntity ReferencedEntity, CoreModel.GenericKeyValueStoreEntrySubstate KeyValueStoreEntry);

internal class KeyValueStoreProcessor
{
    private readonly ProcessorContext _context;

    private Dictionary<KeyValueStoreChangePointerLookup, KeyValueStoreChangePointer> _changePointers = new();
    private List<KeyValueStoreChangePointerLookup> _changeOrder = new();

    private Dictionary<long, KeyValueStoreAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryHistory> _mostRecentEntries = new();

    private List<KeyValueStoreAggregateHistory> _aggregatesToAdd = new();
    private List<KeyValueStoreEntryHistory> _entriesToAdd = new();

    public KeyValueStoreProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.GenericKeyValueStoreEntrySubstate genericKeyValueStoreEntry)
        {
            var kvStoreEntryLookup = new KeyValueStoreChangePointerLookup(
                referencedEntity.DatabaseId,
                stateVersion,
                (ValueBytes)genericKeyValueStoreEntry.Key.KeyData.GetDataBytes()
            );

            _changePointers.GetOrAdd(kvStoreEntryLookup, l =>
            {
                _changeOrder.Add(kvStoreEntryLookup);

                return new KeyValueStoreChangePointer(referencedEntity, genericKeyValueStoreEntry);
            });
        }
    }

    public void ProcessChanges()
    {
        foreach (var lookup in _changeOrder)
        {
            var change = _changePointers[lookup];

            KeyValueStoreAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.KeyValueStoreEntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new KeyValueStoreAggregateHistory
                {
                    Id = _context.Sequences.KeyValueStoreAggregateHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    KeyValueStoreEntityId = lookup.KeyValueStoreEntityId,
                    KeyValueStoreEntryIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.KeyValueStoreEntryIds.AddRange(previousAggregate.KeyValueStoreEntryIds);
                }

                _aggregatesToAdd.Add(aggregate);
                _mostRecentAggregates[lookup.KeyValueStoreEntityId] = aggregate;
            }
            else
            {
                aggregate = previousAggregate;
            }

            var entryLookup = new KeyValueStoreEntryDbLookup(lookup.KeyValueStoreEntityId, lookup.Key);

            var isDeleted = change.KeyValueStoreEntry.Value == null;
            var entry = new KeyValueStoreEntryHistory
            {
                Id = _context.Sequences.KeyValueStoreEntryHistorySequence++,
                KeyValueStoreEntityId = lookup.KeyValueStoreEntityId,
                FromStateVersion = lookup.StateVersion,
                Key = change.KeyValueStoreEntry.Key.KeyData.GetDataBytes(),
                IsLocked = change.KeyValueStoreEntry.IsLocked,
                IsDeleted = isDeleted,
                Value = isDeleted ? null : change.KeyValueStoreEntry.Value!.Data.StructData.GetDataBytes(),
            };

            _entriesToAdd.Add(entry);

            if (_mostRecentEntries.TryGetValue(entryLookup, out var previousEntry))
            {
                var currentPosition = aggregate.KeyValueStoreEntryIds.IndexOf(previousEntry.Id);

                if (currentPosition != -1)
                {
                    aggregate.KeyValueStoreEntryIds.RemoveAt(currentPosition);
                }
            }

            if (entry.Value != null)
            {
                aggregate.KeyValueStoreEntryIds.Insert(0, entry.Id);
            }

            _mostRecentEntries[entryLookup] = entry;
        }
    }

    public async Task LoadMostRecent()
    {
        _mostRecentEntries = await MostRecentKeyValueStoreEntryHistoryFor();
        _mostRecentAggregates = await MostRecentKeyValueStoreAggregateHistoryFor();
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyKeyValueStoreEntryHistory();
        rowsInserted += await CopyKeyValueStoreAggregateHistory();

        return rowsInserted;
    }

    private Task<Dictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryHistory>> MostRecentKeyValueStoreEntryHistoryFor()
    {
        var lookupSet = _changeOrder.ToHashSet();

        if (!lookupSet.Unzip(
                x => x.KeyValueStoreEntityId,
                x => (byte[])x.Key,
                out var keyValueStoreEntityIds,
                out var keys))
        {
            return Task.FromResult(new Dictionary<KeyValueStoreEntryDbLookup, KeyValueStoreEntryHistory>());
        }

        return _context.ReadHelper.MostRecent<KeyValueStoreEntryDbLookup, KeyValueStoreEntryHistory>(
            @$"
WITH variables (key_value_store_entity_id, key) AS (
    SELECT UNNEST({keyValueStoreEntityIds}), UNNEST({keys})
)
SELECT kvseh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM key_value_store_entry_history
    WHERE key_value_store_entity_id = variables.key_value_store_entity_id AND key = variables.key
    ORDER BY from_state_version DESC
    LIMIT 1
) kvseh ON true;",
            e => new KeyValueStoreEntryDbLookup(e.KeyValueStoreEntityId, (ValueBytes)e.Key));
    }

    private Task<Dictionary<long, KeyValueStoreAggregateHistory>> MostRecentKeyValueStoreAggregateHistoryFor()
    {
        var entityIds = _changeOrder.Select(x => x.KeyValueStoreEntityId).ToHashSet().ToList();

        if (!entityIds.Any())
        {
            return Task.FromResult(new Dictionary<long, KeyValueStoreAggregateHistory>());
        }

        return _context.ReadHelper.MostRecent<long, KeyValueStoreAggregateHistory>(
            @$"
WITH variables (key_value_store_entity_id) AS (
    SELECT UNNEST({entityIds})
)
SELECT kvsah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM key_value_store_aggregate_history
    WHERE key_value_store_entity_id = variables.key_value_store_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) kvsah ON true;",
            e => e.KeyValueStoreEntityId);
    }

    private Task<int> CopyKeyValueStoreEntryHistory() => _context.WriteHelper.Copy(
        _entriesToAdd,
        "COPY key_value_store_entry_history (id, from_state_version, key_value_store_entity_id, key, value, is_deleted, is_locked) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyValueStoreEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Key.ToArray(), NpgsqlDbType.Bytea, token);
            await writer.WriteNullableAsync(e.Value?.ToArray(), NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
        });

    private Task<int> CopyKeyValueStoreAggregateHistory() => _context.WriteHelper.Copy(
        _aggregatesToAdd,
        "COPY key_value_store_aggregate_history (id, from_state_version, key_value_store_entity_id, key_value_store_entry_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyValueStoreEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyValueStoreEntryIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });
}
