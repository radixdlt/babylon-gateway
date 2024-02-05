using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct RoleAssignmentsChangePointerLookup(long EntityId, long StateVersion);

internal record struct RoleAssignmentEntryDbLookup(long EntityId, string KeyRole, ModuleId KeyModule);

internal record RoleAssignmentsChangePointer(ReferencedEntity ReferencedEntity, long StateVersion)
{
    public CoreModel.RoleAssignmentModuleFieldOwnerRoleSubstate? OwnerRole { get; set; }

    public IList<CoreModel.RoleAssignmentModuleRuleEntrySubstate> Entries { get; } = new List<CoreModel.RoleAssignmentModuleRuleEntrySubstate>();
}

internal class Dumpyard_EntityRoleAssignment
{
    private Dictionary<RoleAssignmentsChangePointerLookup, RoleAssignmentsChangePointer> _changePointers = new();
    private List<RoleAssignmentsChangePointerLookup> _changes = new();

    private Dictionary<RoleAssignmentEntryDbLookup, EntityRoleAssignmentsEntryHistory> _mostRecentEntries = new();
    private Dictionary<long, EntityRoleAssignmentsAggregateHistory> _mostRecentAggregates = new();

    private List<EntityRoleAssignmentsOwnerRoleHistory> _ownersToAdd = new();
    private List<EntityRoleAssignmentsEntryHistory> _entriesToAdd = new();
    private List<EntityRoleAssignmentsAggregateHistory> _aggregatesToAdd = new();

    public void AcceptUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.RoleAssignmentModuleFieldOwnerRoleSubstate accessRulesFieldOwnerRole)
        {
            _changePointers
                .GetOrAdd(new RoleAssignmentsChangePointerLookup(referencedEntity.DatabaseId, stateVersion), lookup =>
                {
                    _changes.Add(lookup);

                    return new RoleAssignmentsChangePointer(referencedEntity, stateVersion);
                })
                .OwnerRole = accessRulesFieldOwnerRole;
        }

        if (substateData is CoreModel.RoleAssignmentModuleRuleEntrySubstate roleAssignmentEntry)
        {
            _changePointers
                .GetOrAdd(new RoleAssignmentsChangePointerLookup(referencedEntity.DatabaseId, stateVersion), lookup =>
                {
                    _changes.Add(lookup);

                    return new RoleAssignmentsChangePointer(referencedEntity, stateVersion);
                })
                .Entries
                .Add(roleAssignmentEntry);
        }
    }

    public async Task LoadMostRecents(ReadHelper readHelper, CancellationToken token = default)
    {
        _mostRecentEntries = await readHelper.MostRecentEntityRoleAssignmentsEntryHistoryFor(_changePointers.Values, token);
        _mostRecentAggregates = await readHelper.MostRecentEntityRoleAssignmentsAggregateHistoryFor(_changes, token);
    }

    public void PrepareAdd(SequencesHolder sequences)
    {
        foreach (var lookup in _changes)
        {
            var accessRuleChange = _changePointers[lookup];

            EntityRoleAssignmentsOwnerRoleHistory? ownerRole = null;

            if (accessRuleChange.OwnerRole != null)
            {
                ownerRole = new EntityRoleAssignmentsOwnerRoleHistory
                {
                    Id = sequences.EntityRoleAssignmentsOwnerRoleHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    EntityId = lookup.EntityId,
                    RoleAssignments = accessRuleChange.OwnerRole.Value.OwnerRole.ToJson(),
                };

                _ownersToAdd.Add(ownerRole);
            }

            EntityRoleAssignmentsAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.EntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new EntityRoleAssignmentsAggregateHistory
                {
                    Id = sequences.EntityRoleAssignmentsAggregateHistorySequence++,
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

            foreach (var entry in accessRuleChange.Entries)
            {
                var entryLookup = new RoleAssignmentEntryDbLookup(lookup.EntityId, entry.Key.RoleKey, entry.Key.ObjectModuleId.ToModel());
                var entryHistory = new EntityRoleAssignmentsEntryHistory
                {
                    Id = sequences.EntityRoleAssignmentsEntryHistorySequence++,
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

                // !entry.IsDeleted
                if (entry.Value != null)
                {
                    aggregate.EntryIds.Insert(0, entryHistory.Id);
                }

                _mostRecentEntries[entryLookup] = entryHistory;
            }
        }
    }

    public async Task<int> WriteNew(WriteHelper writeHelper, CancellationToken token)
    {
        var rowsInserted = 0;

        rowsInserted += await writeHelper.CopyEntityRoleAssignmentsOwnerRoleHistory(_ownersToAdd, token);
        rowsInserted += await writeHelper.CopyEntityRoleAssignmentsRulesEntryHistory(_entriesToAdd, token);
        rowsInserted += await writeHelper.CopyEntityRoleAssignmentsAggregateHistory(_aggregatesToAdd, token);

        return rowsInserted;
    }
}
