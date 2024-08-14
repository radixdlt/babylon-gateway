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
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal readonly record struct ByEntityDbLookup(long EntityId);

internal readonly record struct ByEntityResourceDbLookup(long EntityId, long ResourceEntityId);

internal readonly record struct ByEntityResourceVaultDbLookup(long EntityId, long ResourceEntityId, long VaultEntityId);

internal class EntityResourceProcessor
{
    internal readonly record struct VaultChange(ResourceType ResourceType, VaultEntity VaultEntity, TokenAmount Delta, long StateVersion)
    {
        public ByEntityDbLookup ByGlobalEntityDbLookup() => new(VaultEntity.GlobalAncestorId!.Value);

        public ByEntityDbLookup ByOwnerEntityDbLookup() => new(VaultEntity.OwnerAncestorId!.Value);

        public ByEntityResourceDbLookup ByGlobalEntityResourceDbLookup() => new(VaultEntity.GlobalAncestorId!.Value, VaultEntity.GetResourceEntityId());

        public ByEntityResourceDbLookup ByOwnerEntityResourceDbLookup() => new(VaultEntity.OwnerAncestorId!.Value, VaultEntity.GetResourceEntityId());

        public ByEntityResourceVaultDbLookup ByGlobalEntityResourceVaultDbLookup() => new(VaultEntity.GlobalAncestorId!.Value, VaultEntity.GetResourceEntityId(), VaultEntity.Id);

        public ByEntityResourceVaultDbLookup ByOwnerEntityResourceVaultDbLookup() => new(VaultEntity.OwnerAncestorId!.Value, VaultEntity.GetResourceEntityId(), VaultEntity.Id);
    }

    private readonly ProcessorContext _context;

    private readonly List<VaultChange> _observedVaultChanges = new();
    private readonly Dictionary<ByEntityResourceDbLookup, EntityResourceEntryDefinition> _existingResourceDefinitions = new();
    private readonly Dictionary<ByEntityDbLookup, EntityResourceTotalsHistory> _mostRecentResourceTotalsHistory = new();
    private readonly Dictionary<ByEntityResourceDbLookup, EntityResourceBalanceHistory> _mostRecentResourceBalanceHistory = new();
    private readonly Dictionary<ByEntityResourceVaultDbLookup, EntityResourceVaultEntryDefinition> _existingResourceVaultDefinitions = new();
    private readonly Dictionary<ByEntityResourceDbLookup, EntityResourceVaultTotalsHistory> _mostRecentResourceVaultTotalsHistory = new();
    private readonly List<EntityResourceEntryDefinition> _resourceEntryDefinitionsToAdd = new();
    private readonly List<EntityResourceTotalsHistory> _resourceTotalsHistoryToAdd = new();
    private readonly List<EntityResourceBalanceHistory> _resourceBalanceHistoryToAdd = new();
    private readonly List<EntityResourceVaultEntryDefinition> _resourceVaultEntryDefinitionsToAdd = new();
    private readonly List<EntityResourceVaultTotalsHistory> _resourceVaultTotalsHistoryToAdd = new();

    public EntityResourceProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion, CoreModel.IUpsertedSubstate substate)
    {
        if (substateData is CoreModel.FungibleVaultFieldBalanceSubstate fungibleBalanceSubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalFungibleVaultEntity>();
            var amount = TokenAmount.FromDecimalString(fungibleBalanceSubstate.Value.Amount);
            var previousAmountRaw = (substate.PreviousValue?.SubstateData as CoreModel.FungibleVaultFieldBalanceSubstate)?.Value.Amount;
            var previousAmount = previousAmountRaw == null ? TokenAmount.Zero : TokenAmount.FromDecimalString(previousAmountRaw);
            var delta = amount - previousAmount;

            if (!vaultEntity.IsRoyaltyVault)
            {
                _observedVaultChanges.Add(new VaultChange(ResourceType.Fungible, vaultEntity, delta, stateVersion));
            }
        }

        if (substateData is CoreModel.NonFungibleVaultFieldBalanceSubstate nonFungibleBalanceSubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
            var amount = TokenAmount.FromDecimalString(nonFungibleBalanceSubstate.Value.Amount);
            var previousAmountRaw = (substate.PreviousValue?.SubstateData as CoreModel.NonFungibleVaultFieldBalanceSubstate)?.Value.Amount;
            var previousAmount = previousAmountRaw == null ? TokenAmount.Zero : TokenAmount.FromDecimalString(previousAmountRaw);
            var delta = amount - previousAmount;

            _observedVaultChanges.Add(new VaultChange(ResourceType.NonFungible, vaultEntity, delta, stateVersion));
        }
    }

    public async Task LoadDependencies()
    {
        _existingResourceDefinitions.AddRange(await ExistingEntityResourceEntryDefinitions());
        _mostRecentResourceTotalsHistory.AddRange(await MostRecentEntityResourceTotalsHistory());
        _mostRecentResourceBalanceHistory.AddRange(await MostRecentResourceBalanceHistory());
        _existingResourceVaultDefinitions.AddRange(await ExistingEntityResourceVaultEntryDefinitions());
        _mostRecentResourceVaultTotalsHistory.AddRange(await MostRecentEntityResourceVaultTotalsHistory());
    }

    public void ProcessChanges()
    {
        foreach (var vaultChange in _observedVaultChanges)
        {
            var vaultEntity = vaultChange.VaultEntity;

            if (!vaultEntity.HasParent)
            {
                throw new UnreachableException("Vault entity cannot be global.");
            }

            foreach (var lookup in new[] { vaultChange.ByGlobalEntityResourceDbLookup(), vaultChange.ByOwnerEntityResourceDbLookup() })
            {
                if (!_existingResourceDefinitions.ContainsKey(lookup))
                {
                    var definition = new EntityResourceEntryDefinition
                    {
                        Id = _context.Sequences.EntityResourceEntryDefinitionSequence++,
                        FromStateVersion = vaultChange.StateVersion,
                        EntityId = lookup.EntityId,
                        ResourceEntityId = lookup.ResourceEntityId,
                        ResourceType = vaultChange.ResourceType,
                    };

                    _resourceEntryDefinitionsToAdd.Add(definition);
                    _existingResourceDefinitions[lookup] = definition;

                    EntityResourceTotalsHistory totalsHistory;

                    if (!_mostRecentResourceTotalsHistory.TryGetValue(new ByEntityDbLookup(lookup.EntityId), out var previousTotalsHistory) || previousTotalsHistory.FromStateVersion != vaultChange.StateVersion)
                    {
                        totalsHistory = new EntityResourceTotalsHistory
                        {
                            Id = _context.Sequences.EntityResourceTotalsHistorySequence++,
                            FromStateVersion = vaultChange.StateVersion,
                            EntityId = lookup.EntityId,
                            TotalCount = previousTotalsHistory?.TotalCount ?? 0,
                            TotalFungibleCount = previousTotalsHistory?.TotalFungibleCount ?? 0,
                            TotalNonFungibleCount = previousTotalsHistory?.TotalNonFungibleCount ?? 0,
                        };

                        _mostRecentResourceTotalsHistory[new ByEntityDbLookup(lookup.EntityId)] = totalsHistory;
                        _resourceTotalsHistoryToAdd.Add(totalsHistory);
                    }
                    else
                    {
                        totalsHistory = previousTotalsHistory;
                    }

                    totalsHistory.TotalCount += 1;
                    totalsHistory.TotalFungibleCount += vaultChange.ResourceType == ResourceType.Fungible ? 1 : 0;
                    totalsHistory.TotalNonFungibleCount += vaultChange.ResourceType == ResourceType.NonFungible ? 1 : 0;
                }
            }

            foreach (var lookup in new[] { vaultChange.ByGlobalEntityResourceVaultDbLookup(), vaultChange.ByOwnerEntityResourceVaultDbLookup() })
            {
                if (!_existingResourceVaultDefinitions.ContainsKey(lookup))
                {
                    var definition = new EntityResourceVaultEntryDefinition
                    {
                        Id = _context.Sequences.EntityResourceVaultEntryDefinitionSequence++,
                        FromStateVersion = vaultChange.StateVersion,
                        EntityId = lookup.EntityId,
                        ResourceEntityId = lookup.ResourceEntityId,
                        VaultEntityId = lookup.VaultEntityId,
                    };

                    _resourceVaultEntryDefinitionsToAdd.Add(definition);
                    _existingResourceVaultDefinitions[lookup] = definition;

                    EntityResourceVaultTotalsHistory totalsHistory;

                    if (!_mostRecentResourceVaultTotalsHistory.TryGetValue(new ByEntityResourceDbLookup(lookup.EntityId, lookup.ResourceEntityId), out var previousTotalsHistory) || previousTotalsHistory.FromStateVersion != vaultChange.StateVersion)
                    {
                        totalsHistory = new EntityResourceVaultTotalsHistory
                        {
                            Id = _context.Sequences.EntityResourceVaultTotalsHistorySequence++,
                            FromStateVersion = vaultChange.StateVersion,
                            EntityId = lookup.EntityId,
                            ResourceEntityId = lookup.ResourceEntityId,
                            TotalCount = previousTotalsHistory?.TotalCount ?? 0,
                        };

                        _mostRecentResourceVaultTotalsHistory[new ByEntityResourceDbLookup(lookup.EntityId, lookup.ResourceEntityId)] = totalsHistory;
                        _resourceVaultTotalsHistoryToAdd.Add(totalsHistory);
                    }
                    else
                    {
                        totalsHistory = previousTotalsHistory;
                    }

                    totalsHistory.TotalCount += 1;
                }

                EntityResourceBalanceHistory balanceHistory;

                if (!_mostRecentResourceBalanceHistory.TryGetValue(new ByEntityResourceDbLookup(lookup.EntityId, lookup.ResourceEntityId), out var previousBalanceHistory) || previousBalanceHistory.FromStateVersion != vaultChange.StateVersion)
                {
                    balanceHistory = new EntityResourceBalanceHistory
                    {
                        Id = _context.Sequences.EntityResourceBalanceHistorySequence++,
                        FromStateVersion = vaultChange.StateVersion,
                        EntityId = lookup.EntityId,
                        ResourceEntityId = lookup.ResourceEntityId,
                        Balance = previousBalanceHistory?.Balance ?? TokenAmount.Zero,
                    };

                    _mostRecentResourceBalanceHistory[new ByEntityResourceDbLookup(lookup.EntityId, lookup.ResourceEntityId)] = balanceHistory;
                    _resourceBalanceHistoryToAdd.Add(balanceHistory);
                }
                else
                {
                    balanceHistory = previousBalanceHistory;
                }

                balanceHistory.Balance += vaultChange.Delta;
            }
        }
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyEntityResourceEntryDefinitions();
        rowsInserted += await CopyEntityResourceTotalsHistory();
        rowsInserted += await CopyEntityResourceBalanceHistory();
        rowsInserted += await CopyEntityResourceVaultEntryDefinitions();
        rowsInserted += await CopyEntityResourceVaultTotalsHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<ByEntityResourceDbLookup, EntityResourceEntryDefinition>> ExistingEntityResourceEntryDefinitions()
    {
        var observedResourceDefinitions = _observedVaultChanges
            .SelectMany(x => new[] { x.ByGlobalEntityResourceDbLookup(), x.ByOwnerEntityResourceDbLookup() })
            .ToHashSet();

        if (!observedResourceDefinitions.Unzip(x => x.EntityId, x => x.ResourceEntityId, out var entityIds, out var resourceEntityIds))
        {
            return ImmutableDictionary<ByEntityResourceDbLookup, EntityResourceEntryDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ByEntityResourceDbLookup, EntityResourceEntryDefinition>(
            @$"
WITH variables (entity_id, resource_entity_id) AS (
    SELECT unnest({entityIds}), unnest({resourceEntityIds})
)
SELECT *
FROM entity_resource_entry_definition
WHERE (entity_id, resource_entity_id) IN (SELECT * FROM variables);",
            e => new ByEntityResourceDbLookup(e.EntityId, e.ResourceEntityId));
    }

    private async Task<IDictionary<ByEntityDbLookup, EntityResourceTotalsHistory>> MostRecentEntityResourceTotalsHistory()
    {
        var entityIds = _observedVaultChanges
            .SelectMany(x => new[] { x.ByGlobalEntityDbLookup(), x.ByOwnerEntityDbLookup() })
            .ToHashSet()
            .Select(x => x.EntityId)
            .ToList();

        if (!entityIds.Any())
        {
            return ImmutableDictionary<ByEntityDbLookup, EntityResourceTotalsHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ByEntityDbLookup, EntityResourceTotalsHistory>(
            @$"
WITH variables (entity_id) AS (
    SELECT unnest({entityIds})
)
SELECT th.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM entity_resource_totals_history
    WHERE entity_id = var.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON true;",
            e => new ByEntityDbLookup(e.EntityId));
    }

    private async Task<IDictionary<ByEntityResourceDbLookup, EntityResourceBalanceHistory>> MostRecentResourceBalanceHistory()
    {
        var observedResourceDefinitions = _observedVaultChanges
            .SelectMany(x => new[] { x.ByGlobalEntityResourceDbLookup(), x.ByOwnerEntityResourceDbLookup() })
            .ToHashSet()
            .ToList();

        if (!observedResourceDefinitions.Unzip(x => x.EntityId, x => x.ResourceEntityId, out var entityIds, out var resourceEntityIds))
        {
            return ImmutableDictionary<ByEntityResourceDbLookup, EntityResourceBalanceHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ByEntityResourceDbLookup, EntityResourceBalanceHistory>(
            @$"
WITH variables (entity_id, resource_entity_id) AS (
    SELECT unnest({entityIds}), unnest({resourceEntityIds})
)
SELECT th.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM entity_resource_balance_history
    WHERE entity_id = var.entity_id AND resource_entity_id = var.resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON true;",
            e => new ByEntityResourceDbLookup(e.EntityId, e.ResourceEntityId));
    }

    private async Task<IDictionary<ByEntityResourceVaultDbLookup, EntityResourceVaultEntryDefinition>> ExistingEntityResourceVaultEntryDefinitions()
    {
        var observedResourceVaultDefinitions = _observedVaultChanges
            .SelectMany(x => new[] { x.ByGlobalEntityResourceVaultDbLookup(), x.ByOwnerEntityResourceVaultDbLookup() })
            .ToHashSet();

        if (!observedResourceVaultDefinitions.Unzip(x => x.EntityId, x => x.ResourceEntityId, x => x.VaultEntityId, out var entityIds, out var resourceEntityIds, out var vaultEntityIds))
        {
            return ImmutableDictionary<ByEntityResourceVaultDbLookup, EntityResourceVaultEntryDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ByEntityResourceVaultDbLookup, EntityResourceVaultEntryDefinition>(
            @$"
WITH variables (entity_id, resource_entity_id, vault_entity_id) AS (
    SELECT unnest({entityIds}), unnest({resourceEntityIds}), unnest({vaultEntityIds})
)
SELECT *
FROM entity_resource_vault_entry_definition
WHERE (entity_id, resource_entity_id, vault_entity_id) IN (SELECT * FROM variables);",
            e => new ByEntityResourceVaultDbLookup(e.EntityId, e.ResourceEntityId, e.VaultEntityId));
    }

    private async Task<IDictionary<ByEntityResourceDbLookup, EntityResourceVaultTotalsHistory>> MostRecentEntityResourceVaultTotalsHistory()
    {
        var observedResourceDefinitions = _observedVaultChanges
            .SelectMany(x => new[] { x.ByGlobalEntityResourceDbLookup(), x.ByOwnerEntityResourceDbLookup() })
            .ToHashSet()
            .ToList();

        if (!observedResourceDefinitions.Unzip(x => x.EntityId, x => x.ResourceEntityId, out var entityIds, out var resourceEntityIds))
        {
            return ImmutableDictionary<ByEntityResourceDbLookup, EntityResourceVaultTotalsHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ByEntityResourceDbLookup, EntityResourceVaultTotalsHistory>(
            @$"
WITH variables (entity_id, resource_entity_id) AS (
    SELECT unnest({entityIds}), unnest({resourceEntityIds})
)
SELECT th.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM entity_resource_vault_totals_history
    WHERE entity_id = var.entity_id AND resource_entity_id = var.resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON true;",
            e => new ByEntityResourceDbLookup(e.EntityId, e.ResourceEntityId));
    }

    private Task<int> CopyEntityResourceEntryDefinitions() => _context.WriteHelper.Copy(
        _resourceEntryDefinitionsToAdd,
        "COPY entity_resource_entry_definition (id, from_state_version, entity_id, resource_entity_id, resource_type) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceType, "resource_type", token);
        });

    private Task<int> CopyEntityResourceTotalsHistory() => _context.WriteHelper.Copy(
        _resourceTotalsHistoryToAdd,
        "COPY entity_resource_totals_history (id, from_state_version, entity_id, total_count, total_fungible_count, total_non_fungible_count) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalCount, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalFungibleCount, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalNonFungibleCount, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyEntityResourceBalanceHistory() => _context.WriteHelper.Copy(
        _resourceBalanceHistoryToAdd,
        "COPY entity_resource_balance_history (id, from_state_version, entity_id, resource_entity_id, balance) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Balance.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
        });

    private Task<int> CopyEntityResourceVaultEntryDefinitions() => _context.WriteHelper.Copy(
        _resourceVaultEntryDefinitionsToAdd,
        "COPY entity_resource_vault_entry_definition (id, from_state_version, entity_id, resource_entity_id, vault_entity_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityId, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyEntityResourceVaultTotalsHistory() => _context.WriteHelper.Copy(
        _resourceVaultTotalsHistoryToAdd,
        "COPY entity_resource_vault_totals_history (id, from_state_version, entity_id, resource_entity_id, total_count) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalCount, NpgsqlDbType.Bigint, token);
        });
}
