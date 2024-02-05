using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct ComponentMethodRoyaltyChangePointerLookup(long EntityId, long StateVersion);

internal record struct ComponentMethodRoyaltyEntryDbLookup(long EntityId, string MethodName);

internal record ComponentMethodRoyaltyChangePointer(ReferencedEntity ReferencedEntity, long StateVersion)
{
    public IList<CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate> Entries { get; } = new List<CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate>();
}

internal class Dumpyard_ComponentMethodRoyalty
{
    private Dictionary<ComponentMethodRoyaltyChangePointerLookup, ComponentMethodRoyaltyChangePointer> _changePointers = new();
    private List<ComponentMethodRoyaltyChangePointerLookup> _changes = new();

    private Dictionary<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory> _mostRecentEntries = new();
    private Dictionary<long, ComponentMethodRoyaltyAggregateHistory> _mostRecentAggregates = new();

    private List<ComponentMethodRoyaltyEntryHistory> _entriesToAdd = new();
    private List<ComponentMethodRoyaltyAggregateHistory> _aggregatesToAdd = new();

    public void AcceptUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate methodRoyaltyEntry)
        {
            _changePointers
                .GetOrAdd(new ComponentMethodRoyaltyChangePointerLookup(referencedEntity.DatabaseId, stateVersion), lookup =>
                {
                    _changes.Add(lookup);

                    return new ComponentMethodRoyaltyChangePointer(referencedEntity, stateVersion);
                })
                .Entries.Add(methodRoyaltyEntry);
        }
    }

    public async Task LoadMostRecents(ReadHelper readHelper, CancellationToken token = default)
    {
        _mostRecentEntries = await readHelper.MostRecentComponentMethodRoyaltyEntryHistoryFor(_changePointers.Values, token);
        _mostRecentAggregates = await readHelper.MostRecentComponentMethodRoyaltyAggregateHistoryFor(_changes, token);
    }

    public void PrepareAdd(SequencesHolder sequences)
    {
        foreach (var lookup in _changes)
        {
            var componentMethodRoyaltyChange = _changePointers[lookup];

            ComponentMethodRoyaltyAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.EntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new ComponentMethodRoyaltyAggregateHistory
                {
                    Id = sequences.ComponentMethodRoyaltyAggregateHistorySequence++,
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

            foreach (var entry in componentMethodRoyaltyChange.Entries)
            {
                var entryLookup = new ComponentMethodRoyaltyEntryDbLookup(lookup.EntityId, entry.Key.MethodName);
                var entryHistory = new ComponentMethodRoyaltyEntryHistory
                {
                    Id = sequences.ComponentMethodRoyaltyEntryHistorySequence++,
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

                aggregate.EntryIds.Insert(0, entryHistory.Id);

                _mostRecentEntries[entryLookup] = entryHistory;
            }
        }
    }

    public async Task<int> WriteNew(WriteHelper writeHelper, CancellationToken token)
    {
        var rowsInserted = 0;

        rowsInserted += await writeHelper.CopyComponentMethodRoyaltyEntryHistory(_entriesToAdd, token);
        rowsInserted += await writeHelper.CopyComponentMethodRoyaltyAggregateHistory(_aggregatesToAdd, token);

        return rowsInserted;
    }
}
