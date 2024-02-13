using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct MetadataEntryDbLookup(long EntityId, string Key);

internal record struct MetadataChangePointerLookup(long EntityId, long StateVersion);

internal record MetadataChangePointer
{
    public List<CoreModel.MetadataModuleEntrySubstate> Entries { get; } = new();
}

internal class EntityMetadataProcessor
{
    private readonly ProcessorContext _context;

    private ChangeTracker<MetadataChangePointerLookup, MetadataChangePointer> _changes = new();

    private Dictionary<long, EntityMetadataAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<MetadataEntryDbLookup, EntityMetadataHistory> _mostRecentEntries = new();

    private List<EntityMetadataAggregateHistory> _aggregatesToAdd = new();
    private List<EntityMetadataHistory> _entriesToAdd = new();

    public EntityMetadataProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.MetadataModuleEntrySubstate metadataEntry)
        {
            _changes
                .GetOrAdd(new MetadataChangePointerLookup(referencedEntity.DatabaseId, stateVersion), _ => new MetadataChangePointer())
                .Entries.Add(metadataEntry);
        }
    }

    public void ProcessChanges()
    {
        foreach (var (lookup, change) in _changes.AsEnumerable())
        {
            EntityMetadataAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.EntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new EntityMetadataAggregateHistory
                {
                    Id = _context.Sequences.EntityMetadataAggregateHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    EntityId = lookup.EntityId,
                    MetadataIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.MetadataIds.AddRange(previousAggregate.MetadataIds);
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
                var entryLookup = new MetadataEntryDbLookup(lookup.EntityId, entry.Key.Name);
                var entryHistory = new EntityMetadataHistory
                {
                    Id = _context.Sequences.EntityMetadataHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    EntityId = lookup.EntityId,
                    Key = entry.Key.Name,
                    Value = entry.Value?.DataStruct.StructData.Hex.ConvertFromHex(),
                    IsDeleted = entry.Value == null,
                    IsLocked = entry.IsLocked,
                };

                _entriesToAdd.Add(entryHistory);

                if (_mostRecentEntries.TryGetValue(entryLookup, out var previousEntry))
                {
                    var currentPosition = aggregate.MetadataIds.IndexOf(previousEntry.Id);

                    if (currentPosition != -1)
                    {
                        aggregate.MetadataIds.RemoveAt(currentPosition);
                    }
                }

                // TODO introduce entry.IsDeleted extension method
                if (entry.Value != null)
                {
                    aggregate.MetadataIds.Insert(0, entryHistory.Id);
                }

                _mostRecentEntries[entryLookup] = entryHistory;
            }
        }
    }

    public async Task LoadMostRecent()
    {
        _mostRecentEntries = await MostRecentEntityMetadataHistory();
        _mostRecentAggregates = await MostRecentEntityAggregateMetadataHistory();
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyEntityMetadataHistory();
        rowsInserted += await CopyEntityMetadataAggregateHistory();

        return rowsInserted;
    }

    private Task<Dictionary<MetadataEntryDbLookup, EntityMetadataHistory>> MostRecentEntityMetadataHistory()
    {
        var lookupSet = new HashSet<MetadataEntryDbLookup>();

        foreach (var (lookup, change) in _changes.AsEnumerable())
        {
            foreach (var entry in change.Entries)
            {
                lookupSet.Add(new MetadataEntryDbLookup(lookup.EntityId, entry.Key.Name));
            }
        }

        if (!lookupSet.Unzip(x => x.EntityId, x => x.Key, out var entityIds, out var keys))
        {
            return Task.FromResult(EmptyDictionary<MetadataEntryDbLookup, EntityMetadataHistory>.Instance);
        }

        return _context.ReadHelper.MostRecent<MetadataEntryDbLookup, EntityMetadataHistory>(
            @$"
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
) emh ON true;",
            e => new MetadataEntryDbLookup(e.EntityId, e.Key));
    }

    private Task<Dictionary<long, EntityMetadataAggregateHistory>> MostRecentEntityAggregateMetadataHistory()
    {
        var entityIds = _changes.Keys.Select(x => x.EntityId).ToHashSet().ToList();

        if (!entityIds.Any())
        {
            return Task.FromResult(EmptyDictionary<long, EntityMetadataAggregateHistory>.Instance);
        }

        return _context.ReadHelper.MostRecent<long, EntityMetadataAggregateHistory>(
            @$"
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
) emah ON true;",
            e => e.EntityId);
    }

    private Task<int> CopyEntityMetadataHistory() => _context.WriteHelper.Copy(
        _entriesToAdd,
        "COPY entity_metadata_history (id, from_state_version, entity_id, key, value, is_deleted, is_locked) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Key, NpgsqlDbType.Text, token);
            await writer.WriteNullableAsync(e.Value, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
        });

    private Task<int> CopyEntityMetadataAggregateHistory() => _context.WriteHelper.Copy(
        _aggregatesToAdd,
        "COPY entity_metadata_aggregate_history (id, from_state_version, entity_id, metadata_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.MetadataIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });
}
