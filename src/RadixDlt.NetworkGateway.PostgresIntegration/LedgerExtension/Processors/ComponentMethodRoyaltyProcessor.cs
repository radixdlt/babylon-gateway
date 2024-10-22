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
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class ComponentMethodRoyaltyProcessor : IProcessorBase, ISubstateUpsertProcessor
{
    private record struct ComponentMethodRoyaltyEntryDbLookup(long EntityId, string MethodName);

    private record struct ComponentMethodRoyaltyChangePointerLookup(long EntityId, long StateVersion);

    private record ComponentMethodRoyaltyChangePointer(ReferencedEntity ReferencedEntity)
    {
        public List<CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate> Entries { get; } = new();
    }

    private readonly ProcessorContext _context;

    private ChangeTracker<ComponentMethodRoyaltyChangePointerLookup, ComponentMethodRoyaltyChangePointer> _changes = new();

    private Dictionary<long, ComponentMethodRoyaltyAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory> _mostRecentEntries = new();

    private List<ComponentMethodRoyaltyAggregateHistory> _aggregatesToAdd = new();
    private List<ComponentMethodRoyaltyEntryHistory> _entriesToAdd = new();

    public ComponentMethodRoyaltyProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate methodRoyaltyEntry)
        {
            _changes
                .GetOrAdd(new ComponentMethodRoyaltyChangePointerLookup(referencedEntity.DatabaseId, stateVersion), _ => new ComponentMethodRoyaltyChangePointer(referencedEntity))
                .Entries
                .Add(methodRoyaltyEntry);
        }
    }

    public async Task LoadDependenciesAsync()
    {
        _mostRecentEntries.AddRange(await MostRecentComponentMethodRoyaltyEntryHistory());
        _mostRecentAggregates.AddRange(await MostRecentComponentMethodRoyaltyAggregateHistory());
    }

    public void ProcessChanges()
    {
        foreach (var (lookup, change) in _changes.AsEnumerable())
        {
            ComponentMethodRoyaltyAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.EntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new ComponentMethodRoyaltyAggregateHistory
                {
                    Id = _context.Sequences.ComponentMethodRoyaltyAggregateHistorySequence++,
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
                var entryLookup = new ComponentMethodRoyaltyEntryDbLookup(lookup.EntityId, entry.Key.MethodName);
                var entryHistory = new ComponentMethodRoyaltyEntryHistory
                {
                    Id = _context.Sequences.ComponentMethodRoyaltyEntryHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    EntityId = lookup.EntityId,
                    MethodName = entry.Key.MethodName,
                    RoyaltyAmount = entry.Value?.RoyaltyAmount?.ToJson(),
                    IsLocked = entry.IsLocked,
                };

                _entriesToAdd.Add(entryHistory);

                if (_mostRecentEntries.TryGetValue(entryLookup, out var previousEntry))
                {
                    var currentPosition = aggregate.EntryIds.IndexOf(previousEntry.Id);

                    if (currentPosition != -1)
                    {
                        aggregate.EntryIds.RemoveAt(currentPosition);
                    }
                }

                if (entry.Value != null)
                {
                    aggregate.EntryIds.Insert(0, entryHistory.Id);
                }

                _mostRecentEntries[entryLookup] = entryHistory;
            }
        }
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyComponentMethodRoyaltyEntryHistory();
        rowsInserted += await CopyComponentMethodRoyaltyAggregateHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory>> MostRecentComponentMethodRoyaltyEntryHistory()
    {
        var lookupSet = new HashSet<ComponentMethodRoyaltyEntryDbLookup>();

        foreach (var (_, change) in _changes.AsEnumerable())
        {
            foreach (var entry in change.Entries)
            {
                lookupSet.Add(new ComponentMethodRoyaltyEntryDbLookup(change.ReferencedEntity.DatabaseId, entry.Key.MethodName));
            }
        }

        if (!lookupSet.Unzip(x => x.EntityId, x => x.MethodName, out var entityIds, out var methodNames))
        {
            return ImmutableDictionary<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory>(
            @$"
WITH variables (entity_id, method_name) AS (
    SELECT UNNEST({entityIds}), UNNEST({methodNames})
)
SELECT cmreh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM component_method_royalty_entry_history
    WHERE entity_id = variables.entity_id AND method_name = variables.method_name
    ORDER BY from_state_version DESC
    LIMIT 1
) cmreh ON true;",
            e => new ComponentMethodRoyaltyEntryDbLookup(e.EntityId, e.MethodName));
    }

    private async Task<IDictionary<long, ComponentMethodRoyaltyAggregateHistory>> MostRecentComponentMethodRoyaltyAggregateHistory()
    {
        var entityIds = _changes.Keys.Select(x => x.EntityId).ToHashSet().ToList();

        if (!entityIds.Any())
        {
            return ImmutableDictionary<long, ComponentMethodRoyaltyAggregateHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<long, ComponentMethodRoyaltyAggregateHistory>(
            $@"
WITH variables (entity_id) AS (
    SELECT UNNEST({entityIds})
)
SELECT cmrah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM component_method_royalty_aggregate_history
    WHERE entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) cmrah ON true;",
            e => e.EntityId);
    }

    private Task<int> CopyComponentMethodRoyaltyEntryHistory() => _context.WriteHelper.Copy(
        _entriesToAdd,
        "COPY component_method_royalty_entry_history (id, from_state_version, entity_id, method_name, royalty_amount, is_locked) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.MethodName, NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.RoyaltyAmount, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
        });

    private Task<int> CopyComponentMethodRoyaltyAggregateHistory() => _context.WriteHelper.Copy(
        _aggregatesToAdd,
        "COPY component_method_royalty_aggregate_history (id, from_state_version, entity_id, entry_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntryIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });
}
