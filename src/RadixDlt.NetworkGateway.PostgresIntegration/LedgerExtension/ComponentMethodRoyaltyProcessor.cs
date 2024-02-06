using NpgsqlTypes;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct ComponentMethodRoyaltyEntryDbLookup(long EntityId, string MethodName);

internal record struct ComponentMethodRoyaltyChangePointerLookup(long EntityId, long StateVersion);

internal record ComponentMethodRoyaltyChangePointer(ReferencedEntity ReferencedEntity)
{
    public IList<CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate> Entries { get; } = new List<CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate>();
}

internal class ComponentMethodRoyaltyProcessor
{
    private readonly ProcessorContext _context;

    private Dictionary<ComponentMethodRoyaltyChangePointerLookup, ComponentMethodRoyaltyChangePointer> _changePointers = new();
    private List<ComponentMethodRoyaltyChangePointerLookup> _changeOrder = new();

    private Dictionary<long, ComponentMethodRoyaltyAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory> _mostRecentEntries = new();

    private List<ComponentMethodRoyaltyAggregateHistory> _aggregatesToAdd = new();
    private List<ComponentMethodRoyaltyEntryHistory> _entriesToAdd = new();

    public ComponentMethodRoyaltyProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate methodRoyaltyEntry)
        {
            _changePointers
                .GetOrAdd(new ComponentMethodRoyaltyChangePointerLookup(referencedEntity.DatabaseId, stateVersion), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new ComponentMethodRoyaltyChangePointer(referencedEntity);
                })
                .Entries.Add(methodRoyaltyEntry);
        }
    }

    public void ProcessChanges()
    {
        foreach (var lookup in _changeOrder)
        {
            var change = _changePointers[lookup];

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
                    RoyaltyAmount = entry.Value?.ToJson(),
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

                // TODO introduce entry.IsDeleted extension method
                if (entry.Value != null)
                {
                    aggregate.EntryIds.Insert(0, entryHistory.Id);
                }

                _mostRecentEntries[entryLookup] = entryHistory;
            }
        }
    }

    public async Task LoadMostRecent()
    {
        _mostRecentEntries = await MostRecentComponentMethodRoyaltyEntryHistory();
        _mostRecentAggregates = await MostRecentComponentMethodRoyaltyAggregateHistory();
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyComponentMethodRoyaltyEntryHistory();
        rowsInserted += await CopyComponentMethodRoyaltyAggregateHistory();

        return rowsInserted;
    }

    private Task<Dictionary<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory>> MostRecentComponentMethodRoyaltyEntryHistory()
    {
        var lookupSet = new HashSet<ComponentMethodRoyaltyEntryDbLookup>();

        foreach (var x in _changePointers.Values)
        {
            foreach (var y in x.Entries)
            {
                lookupSet.Add(new ComponentMethodRoyaltyEntryDbLookup(x.ReferencedEntity.DatabaseId, y.Key.MethodName));
            }
        }

        if (!lookupSet.Unzip(x => x.EntityId, x => x.MethodName, out var entityIds, out var methodNames))
        {
            return Task.FromResult(new Dictionary<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory>());
        }

        return _context.ReadHelper.MostRecent<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory>(
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

    private Task<Dictionary<long, ComponentMethodRoyaltyAggregateHistory>> MostRecentComponentMethodRoyaltyAggregateHistory()
    {
        var entityIds = _changeOrder.Select(x => x.EntityId).ToHashSet().ToList();

        if (!entityIds.Any())
        {
            return Task.FromResult(new Dictionary<long, ComponentMethodRoyaltyAggregateHistory>());
        }

        return _context.ReadHelper.MostRecent<long, ComponentMethodRoyaltyAggregateHistory>(
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
