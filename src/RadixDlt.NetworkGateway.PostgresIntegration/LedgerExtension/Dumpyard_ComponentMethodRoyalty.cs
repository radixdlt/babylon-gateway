using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct ComponentMethodRoyaltyEntryDbLookup(long EntityId, string MethodName);

internal record struct ComponentMethodRoyaltyChangePointerLookup(long EntityId, long StateVersion);

internal record ComponentMethodRoyaltyChangePointer(ReferencedEntity ReferencedEntity)
{
    public IList<CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate> Entries { get; } = new List<CoreModel.RoyaltyModuleMethodRoyaltyEntrySubstate>();
}

internal class Dumpyard_ComponentMethodRoyalty
{
    private readonly Dumpyard_Context _context;

    private Dictionary<ComponentMethodRoyaltyChangePointerLookup, ComponentMethodRoyaltyChangePointer> _changePointers = new();
    private List<ComponentMethodRoyaltyChangePointerLookup> _changeOrder = new();

    private Dictionary<long, ComponentMethodRoyaltyAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<ComponentMethodRoyaltyEntryDbLookup, ComponentMethodRoyaltyEntryHistory> _mostRecentEntries = new();

    private List<ComponentMethodRoyaltyAggregateHistory> _aggregatesToAdd = new();
    private List<ComponentMethodRoyaltyEntryHistory> _entriesToAdd = new();

    public Dumpyard_ComponentMethodRoyalty(Dumpyard_Context context)
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

    public async Task LoadMostRecents()
    {
        _mostRecentEntries = await _context.ReadHelper.MostRecentComponentMethodRoyaltyEntryHistoryFor(_changePointers.Values, _context.Token);
        _mostRecentAggregates = await _context.ReadHelper.MostRecentComponentMethodRoyaltyAggregateHistoryFor(_changeOrder, _context.Token);
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await _context.WriteHelper.CopyComponentMethodRoyaltyEntryHistory(_entriesToAdd, _context.Token);
        rowsInserted += await _context.WriteHelper.CopyComponentMethodRoyaltyAggregateHistory(_aggregatesToAdd, _context.Token);

        return rowsInserted;
    }
}
