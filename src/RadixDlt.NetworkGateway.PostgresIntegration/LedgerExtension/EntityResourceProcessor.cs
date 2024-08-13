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

internal record struct EntityResourceDefinitionDbLookup(long EntityId, long ResourceEntityId);

internal record struct EntityResourceVaultDefinitionDbLookup(long EntityId, long ResourceEntityId, long VaultEntityId);

internal record struct EntityResourceTotalsHistoryDbLookup(long EntityId);

internal record struct EntityResourceVaultTotalsHistoryDbLookup(long EntityId, long ResourceEntityId);

internal record struct EntityResourceBalanceHistoryDbLookup(long EntityId, long ResourceEntityId);

internal record struct VaultChange(ResourceType ResourceType, VaultEntity VaultEntity, TokenAmount Delta, long StateVersion)
{
    public long ToGlobalEntityId() => VaultEntity.GlobalAncestorId!.Value;

    public long ToOwnerEntityId() => VaultEntity.OwnerAncestorId!.Value;

    public (long EntityId, long ResourceEntityId) ToGlobalXxx() => (EntityId: VaultEntity.GlobalAncestorId!.Value, ResourceEntityId: VaultEntity.GetResourceEntityId());

    public (long EntityId, long ResourceEntityId) ToOwnerXxx() => (EntityId: VaultEntity.GlobalAncestorId!.Value, ResourceEntityId: VaultEntity.GetResourceEntityId());

    public (long EntityId, long ResourceEntityId, long VaultEntityId) ToGlobalYyy() => (EntityId: VaultEntity.GlobalAncestorId!.Value, ResourceEntityId: VaultEntity.GetResourceEntityId(), VaultEntityId: VaultEntity.Id);

    public (long EntityId, long ResourceEntityId, long VaultEntityId) ToOwnerYyy() => (EntityId: VaultEntity.GlobalAncestorId!.Value, ResourceEntityId: VaultEntity.GetResourceEntityId(), VaultEntityId: VaultEntity.Id);
}

internal class EntityResourceProcessor
{
    private readonly ProcessorContext _context;

    private List<VaultChange> _vaultChanges = new();

    private Dictionary<EntityResourceDefinitionDbLookup, EntityResourceDefinition> _existingResourceDefinitions = new();
    private Dictionary<EntityResourceVaultDefinitionDbLookup, EntityResourceVaultDefinition> _existingResourceVaultDefinitions = new();
    private Dictionary<EntityResourceTotalsHistoryDbLookup, EntityResourceTotalsHistory> _mostRecentResourceTotalsHistory = new();
    private Dictionary<EntityResourceVaultTotalsHistoryDbLookup, EntityResourceVaultTotalsHistory> _mostRecentResourceVaultTotalsHistory = new();
    private Dictionary<EntityResourceBalanceHistoryDbLookup, EntityResourceBalanceHistory> _mostRecentResourceBalanceHistory = new();
    private List<EntityResourceDefinition> _resourceDefinitionsToAdd = new();
    private List<EntityResourceVaultDefinition> _resourceVaultDefinitionsToAdd = new();
    private List<EntityResourceTotalsHistory> _resourceTotalsHistoryToAdd = new();
    private List<EntityResourceVaultTotalsHistory> _resourceVaultTotalsHistoryToAdd = new();
    private List<EntityResourceBalanceHistory> _resourceBalanceHistoryToAdd = new();

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
                _vaultChanges.Add(new VaultChange(ResourceType.Fungible, vaultEntity, delta, stateVersion));
            }
        }

        if (substateData is CoreModel.NonFungibleVaultFieldBalanceSubstate nonFungibleBalanceSubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
            var amount = TokenAmount.FromDecimalString(nonFungibleBalanceSubstate.Value.Amount);
            var previousAmountRaw = (substate.PreviousValue?.SubstateData as CoreModel.NonFungibleVaultFieldBalanceSubstate)?.Value.Amount;
            var previousAmount = previousAmountRaw == null ? TokenAmount.Zero : TokenAmount.FromDecimalString(previousAmountRaw);
            var delta = amount - previousAmount;

            _vaultChanges.Add(new VaultChange(ResourceType.NonFungible, vaultEntity, delta, stateVersion));
        }
    }

    public async Task LoadDependencies()
    {
        _existingResourceDefinitions.AddRange(await ExistingEntityResourceDefinition());
        _existingResourceVaultDefinitions.AddRange(await ExistingEntityResourceVaultDefinition());
        _mostRecentResourceTotalsHistory.AddRange(await MostRecentEntityResourceTotalsHistory());
        _mostRecentResourceVaultTotalsHistory.AddRange(await MostRecentEntityResourceVaultTotalsHistory());
        _mostRecentResourceBalanceHistory.AddRange(await MostRecentResourceBalanceHistory());
    }

    public void ProcessChanges()
    {
        foreach (var vaultChange in _vaultChanges)
        {
            var vaultEntity = vaultChange.VaultEntity;

            if (!vaultEntity.HasParent)
            {
                throw new UnreachableException("Vault entity cannot be global.");
            }

            var resDefLookups = new[]
            {
                new EntityResourceDefinitionDbLookup(vaultEntity.GlobalAncestorId.Value, vaultEntity.GetResourceEntityId()),
                new EntityResourceDefinitionDbLookup(vaultEntity.OwnerAncestorId.Value, vaultEntity.GetResourceEntityId()),
            };

            foreach (var lookup in resDefLookups)
            {
                if (!_existingResourceDefinitions.ContainsKey(lookup))
                {
                    var definition = new EntityResourceDefinition
                    {
                        Id = _context.Sequences.EntityResourceDefinitionSequence++,
                        FromStateVersion = vaultChange.StateVersion,
                        EntityId = lookup.EntityId,
                        ResourceEntityId = lookup.ResourceEntityId,
                        ResourceType = vaultChange.ResourceType,
                    };

                    _resourceDefinitionsToAdd.Add(definition);
                    _existingResourceDefinitions[lookup] = definition;

                    EntityResourceTotalsHistory totalsHistory;

                    if (!_mostRecentResourceTotalsHistory.TryGetValue(new EntityResourceTotalsHistoryDbLookup(lookup.EntityId), out var previousTotalsHistory) || previousTotalsHistory.FromStateVersion != vaultChange.StateVersion)
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

                        _mostRecentResourceTotalsHistory[new EntityResourceTotalsHistoryDbLookup(lookup.EntityId)] = totalsHistory;
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

            var resVaultDefLookups = new[]
            {
                new EntityResourceVaultDefinitionDbLookup(vaultEntity.GlobalAncestorId!.Value, vaultEntity.GetResourceEntityId(), vaultEntity.Id),
                new EntityResourceVaultDefinitionDbLookup(vaultEntity.OwnerAncestorId!.Value, vaultEntity.GetResourceEntityId(), vaultEntity.Id),
            };

            foreach (var lookup in resVaultDefLookups)
            {
                if (!_existingResourceVaultDefinitions.ContainsKey(lookup))
                {
                    var definition = new EntityResourceVaultDefinition
                    {
                        Id = _context.Sequences.EntityResourceVaultDefinitionSequence++,
                        FromStateVersion = vaultChange.StateVersion,
                        EntityId = lookup.EntityId,
                        ResourceEntityId = lookup.ResourceEntityId,
                        VaultEntityId = lookup.VaultEntityId,
                    };

                    _resourceVaultDefinitionsToAdd.Add(definition);
                    _existingResourceVaultDefinitions[lookup] = definition;

                    EntityResourceVaultTotalsHistory totalsHistory;

                    if (!_mostRecentResourceVaultTotalsHistory.TryGetValue(new EntityResourceVaultTotalsHistoryDbLookup(lookup.EntityId, lookup.ResourceEntityId), out var previousTotalsHistory) || previousTotalsHistory.FromStateVersion != vaultChange.StateVersion)
                    {
                        totalsHistory = new EntityResourceVaultTotalsHistory
                        {
                            Id = _context.Sequences.EntityResourceVaultTotalsHistorySequence++,
                            FromStateVersion = vaultChange.StateVersion,
                            EntityId = lookup.EntityId,
                            ResourceEntityId = lookup.ResourceEntityId,
                            TotalCount = previousTotalsHistory?.TotalCount ?? 0,
                        };

                        _mostRecentResourceVaultTotalsHistory[new EntityResourceVaultTotalsHistoryDbLookup(lookup.EntityId, lookup.ResourceEntityId)] = totalsHistory;
                        _resourceVaultTotalsHistoryToAdd.Add(totalsHistory);
                    }
                    else
                    {
                        totalsHistory = previousTotalsHistory;
                    }

                    totalsHistory.TotalCount += 1;
                }

                EntityResourceBalanceHistory balanceHistory;

                if (!_mostRecentResourceBalanceHistory.TryGetValue(new EntityResourceBalanceHistoryDbLookup(lookup.EntityId, lookup.ResourceEntityId), out var previousBalanceHistory) || previousBalanceHistory.FromStateVersion != vaultChange.StateVersion)
                {
                    balanceHistory = new EntityResourceBalanceHistory
                    {
                        Id = _context.Sequences.EntityResourceBalanceHistorySequence++,
                        FromStateVersion = vaultChange.StateVersion,
                        EntityId = lookup.EntityId,
                        ResourceEntityId = lookup.ResourceEntityId,
                        Balance = previousBalanceHistory?.Balance ?? TokenAmount.Zero,
                    };

                    _mostRecentResourceBalanceHistory[new EntityResourceBalanceHistoryDbLookup(lookup.EntityId, lookup.ResourceEntityId)] = balanceHistory;
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

        rowsInserted += await CopyEntityResourceDefinitions();
        rowsInserted += await CopyEntityResourceVaultDefinitions();
        rowsInserted += await CopyEntityResourceTotalsHistory();
        rowsInserted += await CopyEntityResourceVaultTotalsHistory();
        rowsInserted += await CopyEntityResourceBalanceHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<EntityResourceDefinitionDbLookup, EntityResourceDefinition>> ExistingEntityResourceDefinition()
    {
        var observedResourceDefinitions = _vaultChanges
            .SelectMany(x => new[] { x.ToGlobalXxx(), x.ToOwnerXxx() })
            .ToHashSet();

        if (!observedResourceDefinitions.Unzip(x => x.EntityId, x => x.ResourceEntityId, out var entityIds, out var resourceEntityIds))
        {
            return ImmutableDictionary<EntityResourceDefinitionDbLookup, EntityResourceDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<EntityResourceDefinitionDbLookup, EntityResourceDefinition>(
            @$"
WITH variables (entity_id, resource_entity_id) AS (
    SELECT unnest({entityIds}), unnest({resourceEntityIds})
)
SELECT *
FROM entity_resource_definition
WHERE (entity_id, resource_entity_id) IN (SELECT * FROM variables);",
            e => new EntityResourceDefinitionDbLookup(e.EntityId, e.ResourceEntityId));
    }

    private async Task<IDictionary<EntityResourceVaultDefinitionDbLookup, EntityResourceVaultDefinition>> ExistingEntityResourceVaultDefinition()
    {
        var observedResourceVaultDefinitions = _vaultChanges
            .SelectMany(x => new[] { x.ToGlobalYyy(), x.ToOwnerYyy() })
            .ToHashSet();

        if (!observedResourceVaultDefinitions.Unzip(x => x.EntityId, x => x.ResourceEntityId, x => x.VaultEntityId, out var entityIds, out var resourceEntityIds, out var vaultEntityIds))
        {
            return ImmutableDictionary<EntityResourceVaultDefinitionDbLookup, EntityResourceVaultDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<EntityResourceVaultDefinitionDbLookup, EntityResourceVaultDefinition>(
            @$"
WITH variables (entity_id, resource_entity_id, vault_entity_id) AS (
    SELECT unnest({entityIds}), unnest({resourceEntityIds}), unnest({vaultEntityIds})
)
SELECT *
FROM entity_resource_vault_definition
WHERE (entity_id, resource_entity_id, vault_entity_id) IN (SELECT * FROM variables);",
            e => new EntityResourceVaultDefinitionDbLookup(e.EntityId, e.ResourceEntityId, e.VaultEntityId));
    }

    private async Task<IDictionary<EntityResourceTotalsHistoryDbLookup, EntityResourceTotalsHistory>> MostRecentEntityResourceTotalsHistory()
    {
        var entityIds = _vaultChanges
            .SelectMany(x => new[] { x.ToGlobalEntityId(), x.ToOwnerEntityId() })
            .ToHashSet()
            .ToList();

        if (!entityIds.Any())
        {
            return ImmutableDictionary<EntityResourceTotalsHistoryDbLookup, EntityResourceTotalsHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<EntityResourceTotalsHistoryDbLookup, EntityResourceTotalsHistory>(
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
            e => new EntityResourceTotalsHistoryDbLookup(e.EntityId));
    }

    private async Task<IDictionary<EntityResourceVaultTotalsHistoryDbLookup, EntityResourceVaultTotalsHistory>> MostRecentEntityResourceVaultTotalsHistory()
    {
        var observedResourceDefinitions = _vaultChanges
            .SelectMany(x => new[] { x.ToGlobalXxx(), x.ToOwnerXxx() })
            .ToHashSet()
            .ToList();

        if (!observedResourceDefinitions.Unzip(x => x.EntityId, x => x.ResourceEntityId, out var entityIds, out var resourceEntityIds))
        {
            return ImmutableDictionary<EntityResourceVaultTotalsHistoryDbLookup, EntityResourceVaultTotalsHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<EntityResourceVaultTotalsHistoryDbLookup, EntityResourceVaultTotalsHistory>(
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
            e => new EntityResourceVaultTotalsHistoryDbLookup(e.EntityId, e.ResourceEntityId));
    }

    private async Task<IDictionary<EntityResourceBalanceHistoryDbLookup, EntityResourceBalanceHistory>> MostRecentResourceBalanceHistory()
    {
        var observedResourceDefinitions = _vaultChanges
            .SelectMany(x => new[] { x.ToGlobalXxx(), x.ToOwnerXxx() })
            .ToHashSet()
            .ToList();

        if (!observedResourceDefinitions.Unzip(x => x.EntityId, x => x.ResourceEntityId, out var entityIds, out var resourceEntityIds))
        {
            return ImmutableDictionary<EntityResourceBalanceHistoryDbLookup, EntityResourceBalanceHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<EntityResourceBalanceHistoryDbLookup, EntityResourceBalanceHistory>(
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
            e => new EntityResourceBalanceHistoryDbLookup(e.EntityId, e.ResourceEntityId));
    }

    private Task<int> CopyEntityResourceDefinitions() => _context.WriteHelper.Copy(
        _resourceDefinitionsToAdd,
        "COPY entity_resource_definition (id, from_state_version, entity_id, resource_entity_id, resource_type) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceType, "resource_type", token);
        });

    private Task<int> CopyEntityResourceVaultDefinitions() => _context.WriteHelper.Copy(
        _resourceVaultDefinitionsToAdd,
        "COPY entity_resource_vault_definition (id, from_state_version, entity_id, resource_entity_id, vault_entity_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityId, NpgsqlDbType.Bigint, token);
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
