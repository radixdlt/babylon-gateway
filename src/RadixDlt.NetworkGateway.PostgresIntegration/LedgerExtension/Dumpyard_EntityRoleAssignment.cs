using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct RoleAssignmentEntryDbLookup(long EntityId, string KeyRole, ModuleId KeyModule);

internal record struct RoleAssignmentsChangePointerLookup(long EntityId, long StateVersion);

internal record RoleAssignmentsChangePointer(ReferencedEntity ReferencedEntity)
{
    public CoreModel.RoleAssignmentModuleFieldOwnerRoleSubstate? OwnerRole { get; set; }

    public List<CoreModel.RoleAssignmentModuleRuleEntrySubstate> Entries { get; } = new();
}

internal class Dumpyard_EntityRoleAssignment
{
    private readonly Dumpyard_Context _context;

    private Dictionary<RoleAssignmentsChangePointerLookup, RoleAssignmentsChangePointer> _changePointers = new();
    private List<RoleAssignmentsChangePointerLookup> _changeOrder = new();

    private Dictionary<long, EntityRoleAssignmentsAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<RoleAssignmentEntryDbLookup, EntityRoleAssignmentsEntryHistory> _mostRecentEntries = new();

    private List<EntityRoleAssignmentsAggregateHistory> _aggregatesToAdd = new();
    private List<EntityRoleAssignmentsEntryHistory> _entriesToAdd = new();
    private List<EntityRoleAssignmentsOwnerRoleHistory> _ownersToAdd = new();

    public Dumpyard_EntityRoleAssignment(Dumpyard_Context context)
    {
        _context = context;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.RoleAssignmentModuleFieldOwnerRoleSubstate accessRulesFieldOwnerRole)
        {
            _changePointers
                .GetOrAdd(new RoleAssignmentsChangePointerLookup(referencedEntity.DatabaseId, stateVersion), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new RoleAssignmentsChangePointer(referencedEntity);
                })
                .OwnerRole = accessRulesFieldOwnerRole;
        }

        if (substateData is CoreModel.RoleAssignmentModuleRuleEntrySubstate roleAssignmentEntry)
        {
            _changePointers
                .GetOrAdd(new RoleAssignmentsChangePointerLookup(referencedEntity.DatabaseId, stateVersion), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new RoleAssignmentsChangePointer(referencedEntity);
                })
                .Entries.Add(roleAssignmentEntry);
        }
    }

    public void ProcessChanges()
    {
        foreach (var lookup in _changeOrder)
        {
            var change = _changePointers[lookup];

            EntityRoleAssignmentsOwnerRoleHistory? ownerRole = null;

            if (change.OwnerRole != null)
            {
                ownerRole = new EntityRoleAssignmentsOwnerRoleHistory
                {
                    Id = _context.Sequences.EntityRoleAssignmentsOwnerRoleHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    EntityId = lookup.EntityId,
                    RoleAssignments = change.OwnerRole.Value.OwnerRole.ToJson(),
                };

                _ownersToAdd.Add(ownerRole);
            }

            EntityRoleAssignmentsAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.EntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new EntityRoleAssignmentsAggregateHistory
                {
                    Id = _context.Sequences.EntityRoleAssignmentsAggregateHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    EntityId = lookup.EntityId,
                    OwnerRoleId = ownerRole?.Id ?? previousAggregate?.OwnerRoleId ?? throw new InvalidOperationException("Unable to determine OwnerRoleId"),
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
                var entryLookup = new RoleAssignmentEntryDbLookup(lookup.EntityId, entry.Key.RoleKey, entry.Key.ObjectModuleId.ToModel());
                var entryHistory = new EntityRoleAssignmentsEntryHistory
                {
                    Id = _context.Sequences.EntityRoleAssignmentsEntryHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    EntityId = lookup.EntityId,
                    KeyRole = entry.Key.RoleKey,
                    KeyModule = entry.Key.ObjectModuleId.ToModel(),
                    RoleAssignments = entry.Value?.AccessRule.ToJson(),
                    IsDeleted = entry.Value == null,
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
        _mostRecentEntries = await _context.ReadHelper.MostRecentEntityRoleAssignmentsEntryHistoryFor(_changePointers.Values, _context.Token);
        _mostRecentAggregates = await _context.ReadHelper.MostRecentEntityRoleAssignmentsAggregateHistoryFor(_changeOrder, _context.Token);
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await _context.WriteHelper.CopyEntityRoleAssignmentsOwnerRoleHistory(_ownersToAdd, _context.Token);
        rowsInserted += await _context.WriteHelper.CopyEntityRoleAssignmentsRulesEntryHistory(_entriesToAdd, _context.Token);
        rowsInserted += await _context.WriteHelper.CopyEntityRoleAssignmentsAggregateHistory(_aggregatesToAdd, _context.Token);

        return rowsInserted;
    }
}
