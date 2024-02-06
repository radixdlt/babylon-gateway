using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

internal class EntityRoleAssignmentProcessor
{
    private readonly ProcessorContext _context;

    private Dictionary<RoleAssignmentsChangePointerLookup, RoleAssignmentsChangePointer> _changePointers = new();
    private List<RoleAssignmentsChangePointerLookup> _changeOrder = new();

    private Dictionary<long, EntityRoleAssignmentsAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<RoleAssignmentEntryDbLookup, EntityRoleAssignmentsEntryHistory> _mostRecentEntries = new();

    private List<EntityRoleAssignmentsAggregateHistory> _aggregatesToAdd = new();
    private List<EntityRoleAssignmentsEntryHistory> _entriesToAdd = new();
    private List<EntityRoleAssignmentsOwnerRoleHistory> _ownersToAdd = new();

    public EntityRoleAssignmentProcessor(ProcessorContext context)
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
        _mostRecentEntries = await MostRecentEntityRoleAssignmentsEntryHistory();
        _mostRecentAggregates = await MostRecentEntityRoleAssignmentsAggregateHistory();
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyEntityRoleAssignmentsOwnerRoleHistory();
        rowsInserted += await CopyEntityRoleAssignmentsRulesEntryHistory();
        rowsInserted += await CopyEntityRoleAssignmentsAggregateHistory();

        return rowsInserted;
    }

    private Task<Dictionary<RoleAssignmentEntryDbLookup, EntityRoleAssignmentsEntryHistory>> MostRecentEntityRoleAssignmentsEntryHistory()
    {
        var lookupSet = new HashSet<RoleAssignmentEntryDbLookup>();

        foreach (var change in _changePointers.Values)
        {
            foreach (var entry in change.Entries)
            {
                lookupSet.Add(new RoleAssignmentEntryDbLookup(change.ReferencedEntity.DatabaseId, entry.Key.RoleKey, entry.Key.ObjectModuleId.ToModel()));
            }
        }

        if (!lookupSet.Unzip(x => x.EntityId, x => x.KeyRole, x => x.KeyModule, out var entityIds, out var keyRoles, out var keyModuleIds))
        {
            return Task.FromResult(new Dictionary<RoleAssignmentEntryDbLookup, EntityRoleAssignmentsEntryHistory>());
        }

        return _context.ReadHelper.MostRecent<RoleAssignmentEntryDbLookup, EntityRoleAssignmentsEntryHistory>(
            @$"
WITH variables (entity_id, key_role, module_id) AS (
    SELECT UNNEST({entityIds}), UNNEST({keyRoles}), UNNEST({keyModuleIds})
)
SELECT eareh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_role_assignments_entry_history
    WHERE entity_id = variables.entity_id AND key_role = variables.key_role AND key_module = variables.module_id
    ORDER BY from_state_version DESC
    LIMIT 1
) eareh ON true;",
            e => new RoleAssignmentEntryDbLookup(e.EntityId, e.KeyRole, e.KeyModule));
    }

    private Task<Dictionary<long, EntityRoleAssignmentsAggregateHistory>> MostRecentEntityRoleAssignmentsAggregateHistory()
    {
        var entityIds = _changeOrder.Select(x => x.EntityId).ToHashSet().ToList();

        if (!entityIds.Any())
        {
            return Task.FromResult(new Dictionary<long, EntityRoleAssignmentsAggregateHistory>());
        }

        return _context.ReadHelper.MostRecent<long, EntityRoleAssignmentsAggregateHistory>(
            @$"
WITH variables (entity_id) AS (
    SELECT UNNEST({entityIds})
)
SELECT earah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM entity_role_assignments_aggregate_history
    WHERE entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) earah ON true;",
            e => e.EntityId);
    }

    private Task<int> CopyEntityRoleAssignmentsOwnerRoleHistory() => _context.WriteHelper.Copy(
        _ownersToAdd,
        "COPY entity_role_assignments_owner_role_history (id, from_state_version, entity_id, role_assignments) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.RoleAssignments, NpgsqlDbType.Jsonb, token);
        });

    private Task<int> CopyEntityRoleAssignmentsRulesEntryHistory() => _context.WriteHelper.Copy(
        _entriesToAdd,
        "COPY entity_role_assignments_entry_history (id, from_state_version, entity_id, key_role, key_module, role_assignments, is_deleted) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyRole, NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.KeyModule, "module_id", token);
            await writer.WriteAsync(e.RoleAssignments, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
        });

    private Task<int> CopyEntityRoleAssignmentsAggregateHistory() => _context.WriteHelper.Copy(
        _aggregatesToAdd,
        "COPY entity_role_assignments_aggregate_history (id, from_state_version, entity_id, owner_role_id, entry_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.OwnerRoleId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntryIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });
}
