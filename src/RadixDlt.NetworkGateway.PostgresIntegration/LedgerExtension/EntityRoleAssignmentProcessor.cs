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
