using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class ComponentResourceVaultProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    public ComponentResourceVaultProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.FungibleVaultFieldBalanceSubstate fungibleVaultFieldBalanceSubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalFungibleVaultEntity>();
            var resourceEntity = _referencedEntities.GetByDatabaseId(vaultEntity.GetResourceEntityId());
            var amount = TokenAmount.FromDecimalString(fungibleVaultFieldBalanceSubstate.Value.Amount);

            // vaultSnapshots.Add(new FungibleVaultSnapshot(referencedEntity, resourceEntity, amount, stateVersion));

            if (!vaultEntity.IsRoyaltyVault)
            {
                var previousAmountRaw = (string?)null; // (substate.PreviousValue?.SubstateData as CoreModel.FungibleVaultFieldBalanceSubstate)?.Value.Amount;
                var previousAmount = previousAmountRaw == null ? TokenAmount.Zero : TokenAmount.FromDecimalString(previousAmountRaw);
                var delta = amount - previousAmount;

                vaultChanges.Add(new EntityFungibleResourceBalanceChangeEvent(referencedEntity.DatabaseGlobalAncestorId, resourceEntity.DatabaseId, delta, stateVersion));

                if (referencedEntity.DatabaseGlobalAncestorId != referencedEntity.DatabaseOwnerAncestorId)
                {
                    vaultChanges.Add(new EntityFungibleResourceBalanceChangeEvent(referencedEntity.DatabaseOwnerAncestorId, resourceEntity.DatabaseId, delta, stateVersion));
                }
            }
        }
    }

    public async Task LoadDependencies()
    {
        _existingFungibleDefinitions.AddRange(await ExistingComponentFungibleResourceDefinitions());
        _existingNonFungibleDefinitions.AddRange(await ExistingComponentNonFungibleResourceDefinitions());
        _mostRecentFungibleTotalsHistory.AddRange(await MostRecentComponentFungibleResourceTotalsHistory());
        _mostRecentNonFungibleTotalsHistory.AddRange(await MostRecentComponentNonFungibleResourceTotalsHistory());
    }

    public void ProcessChanges()
    {
        var fungibleDefinitionsToAdd = _observedFungibleDefinitions.Where(kvp => !_existingFungibleDefinitions.ContainsKey(kvp.Key)).OrderBy(kvp => kvp.Value);
        var nonFungibleDefinitionsToAdd = _observedNonFungibleDefinitions.Where(kvp => !_existingNonFungibleDefinitions.ContainsKey(kvp.Key)).OrderBy(kvp => kvp.Value);

        foreach (var (lookup, stateVersion) in fungibleDefinitionsToAdd)
        {
            var definition = new ComponentFungibleResourceDefinition
            {
                Id = _context.Sequences.ComponentFungibleResourceDefinitionSequence++,
                FromStateVersion = stateVersion,
                ComponentEntityId = lookup.ComponentEntityId,
                ResourceEntityId = lookup.ResourceEntityId,
            };

            _fungibleDefinitionsToAdd.Add(definition);
            _existingFungibleDefinitions[lookup] = definition;

            ComponentFungibleResourceTotalsHistory totalsHistory;

            if (!_mostRecentFungibleTotalsHistory.TryGetValue(new ComponentResourceTotalsHistoryDbLookup(lookup.ComponentEntityId), out var previousTotalsHistory) || previousTotalsHistory.FromStateVersion != stateVersion)
            {
                totalsHistory = new ComponentFungibleResourceTotalsHistory
                {
                    Id = _context.Sequences.ComponentFungibleResourceTotalsHistorySequence++,
                    FromStateVersion = stateVersion,
                    ComponentEntityId = lookup.ComponentEntityId,
                    TotalCount = previousTotalsHistory?.TotalCount ?? 0,
                };

                _mostRecentFungibleTotalsHistory[new ComponentResourceTotalsHistoryDbLookup(lookup.ComponentEntityId)] = totalsHistory;
                _fungibleTotalsHistoryToAdd.Add(totalsHistory);
            }
            else
            {
                totalsHistory = previousTotalsHistory;
            }

            totalsHistory.TotalCount += 1;
        }

        foreach (var (lookup, stateVersion) in nonFungibleDefinitionsToAdd)
        {
            var definition = new ComponentNonFungibleResourceDefinition
            {
                Id = _context.Sequences.ComponentNonFungibleResourceDefinitionSequence++,
                FromStateVersion = stateVersion,
                ComponentEntityId = lookup.ComponentEntityId,
                ResourceEntityId = lookup.ResourceEntityId,
            };

            _nonFungibleDefinitionsToAdd.Add(definition);
            _existingNonFungibleDefinitions[lookup] = definition;

            ComponentNonFungibleResourceTotalsHistory totalsHistory;

            if (!_mostRecentNonFungibleTotalsHistory.TryGetValue(new ComponentResourceTotalsHistoryDbLookup(lookup.ComponentEntityId), out var previousTotalsHistory) || previousTotalsHistory.FromStateVersion != stateVersion)
            {
                totalsHistory = new ComponentNonFungibleResourceTotalsHistory
                {
                    Id = _context.Sequences.ComponentNonFungibleResourceTotalsHistorySequence++,
                    FromStateVersion = stateVersion,
                    ComponentEntityId = lookup.ComponentEntityId,
                    TotalCount = previousTotalsHistory?.TotalCount ?? 0,
                };

                _mostRecentNonFungibleTotalsHistory[new ComponentResourceTotalsHistoryDbLookup(lookup.ComponentEntityId)] = totalsHistory;
                _nonFungibleTotalsHistoryToAdd.Add(totalsHistory);
            }
            else
            {
                totalsHistory = previousTotalsHistory;
            }

            totalsHistory.TotalCount += 1;
        }
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyComponentFungibleResourceDefinitions();
        rowsInserted += await CopyComponentNonFungibleResourceDefinitions();
        rowsInserted += await CopyComponentFungibleResourceTotalsHistory();
        rowsInserted += await CopyComponentNonFungibleResourceTotalsHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<ComponentResourceDefinitionDbLookup, ComponentFungibleResourceDefinition>> ExistingComponentFungibleResourceDefinitions()
    {
        if (!_observedFungibleDefinitions.Keys.Unzip(x => x.ComponentEntityId, x => x.ResourceEntityId, out var componentEntityIds, out var resourceEntityIds))
        {
            return ImmutableDictionary<ComponentResourceDefinitionDbLookup, ComponentFungibleResourceDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ComponentResourceDefinitionDbLookup, ComponentFungibleResourceDefinition>(
            @$"
WITH variables (component_entity_id, resource_entity_id) AS (
    SELECT UNNEST({componentEntityIds}), UNNEST({resourceEntityIds})
)
SELECT *
FROM component_fungible_resource_definition
WHERE (component_entity_id, resource_entity_id) IN (SELECT * FROM variables);",
            e => new ComponentResourceDefinitionDbLookup(e.ComponentEntityId, e.ResourceEntityId));
    }

    private async Task<IDictionary<ComponentResourceDefinitionDbLookup, ComponentNonFungibleResourceDefinition>> ExistingComponentNonFungibleResourceDefinitions()
    {
        if (!_observedNonFungibleDefinitions.Keys.Unzip(x => x.ComponentEntityId, x => x.ResourceEntityId, out var componentEntityIds, out var resourceEntityIds))
        {
            return ImmutableDictionary<ComponentResourceDefinitionDbLookup, ComponentNonFungibleResourceDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ComponentResourceDefinitionDbLookup, ComponentNonFungibleResourceDefinition>(
            @$"
WITH variables (component_entity_id, resource_entity_id) AS (
    SELECT UNNEST({componentEntityIds}), UNNEST({resourceEntityIds})
)
SELECT *
FROM component_non_fungible_resource_definition
WHERE (component_entity_id, resource_entity_id) IN (SELECT * FROM variables);",
            e => new ComponentResourceDefinitionDbLookup(e.ComponentEntityId, e.ResourceEntityId));
    }

    private async Task<IDictionary<ComponentResourceTotalsHistoryDbLookup, ComponentFungibleResourceTotalsHistory>> MostRecentComponentFungibleResourceTotalsHistory()
    {
        var componentEntityIds = _observedFungibleDefinitions.Keys.Select(x => x.ComponentEntityId).ToHashSet().ToList();

        if (!componentEntityIds.Any())
        {
            return ImmutableDictionary<ComponentResourceTotalsHistoryDbLookup, ComponentFungibleResourceTotalsHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ComponentResourceTotalsHistoryDbLookup, ComponentFungibleResourceTotalsHistory>(
            @$"
WITH variables (component_entity_id) AS (
    SELECT UNNEST({componentEntityIds})
)
SELECT th.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM component_fungible_resource_totals_history
    WHERE component_entity_id = var.component_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON true;",
            e => new ComponentResourceTotalsHistoryDbLookup(e.ComponentEntityId));
    }

    private async Task<IDictionary<ComponentResourceTotalsHistoryDbLookup, ComponentNonFungibleResourceTotalsHistory>> MostRecentComponentNonFungibleResourceTotalsHistory()
    {
        var componentEntityIds = _observedNonFungibleDefinitions.Keys.Select(x => x.ComponentEntityId).ToHashSet().ToList();

        if (!componentEntityIds.Any())
        {
            return ImmutableDictionary<ComponentResourceTotalsHistoryDbLookup, ComponentNonFungibleResourceTotalsHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<ComponentResourceTotalsHistoryDbLookup, ComponentNonFungibleResourceTotalsHistory>(
            @$"
WITH variables (component_entity_id) AS (
    SELECT UNNEST({componentEntityIds})
)
SELECT th.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM component_non_fungible_resource_totals_history
    WHERE component_entity_id = var.component_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON true;",
            e => new ComponentResourceTotalsHistoryDbLookup(e.ComponentEntityId));
    }

    private Task<int> CopyComponentFungibleResourceDefinitions() => _context.WriteHelper.Copy(
        _fungibleDefinitionsToAdd,
        "COPY component_fungible_resource_definition (id, from_state_version, component_entity_id, resource_entity_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ComponentEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyComponentNonFungibleResourceDefinitions() => _context.WriteHelper.Copy(
        _nonFungibleDefinitionsToAdd,
        "COPY component_non_fungible_resource_definition (id, from_state_version, component_entity_id, resource_entity_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ComponentEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyComponentFungibleResourceTotalsHistory() => _context.WriteHelper.Copy(
        _fungibleTotalsHistoryToAdd,
        "COPY component_fungible_resource_totals_history (id, from_state_version, component_entity_id, total_count) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ComponentEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalCount, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyComponentNonFungibleResourceTotalsHistory() => _context.WriteHelper.Copy(
        _nonFungibleTotalsHistoryToAdd,
        "COPY component_non_fungible_resource_totals_history (id, from_state_version, component_entity_id, total_count) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ComponentEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalCount, NpgsqlDbType.Bigint, token);
        });
}
