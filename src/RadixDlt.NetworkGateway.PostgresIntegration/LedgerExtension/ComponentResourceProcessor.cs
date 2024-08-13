using NpgsqlTypes;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct ComponentResourceDefinitionDbLookup(long ComponentEntityId, long ResourceEntityId);

internal record struct ComponentResourceTotalsHistoryDbLookup(long ComponentEntityId);

internal class ComponentResourceProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private Dictionary<ComponentResourceDefinitionDbLookup, long> _observedFungibleDefinitions = new();
    private Dictionary<ComponentResourceDefinitionDbLookup, long> _observedNonFungibleDefinitions = new();
    private Dictionary<ComponentResourceDefinitionDbLookup, ComponentFungibleResourceDefinition> _existingFungibleDefinitions = new();
    private Dictionary<ComponentResourceDefinitionDbLookup, ComponentNonFungibleResourceDefinition> _existingNonFungibleDefinitions = new();
    private Dictionary<ComponentResourceTotalsHistoryDbLookup, ComponentFungibleResourceTotalsHistory> _mostRecentFungibleTotalsHistory = new();
    private Dictionary<ComponentResourceTotalsHistoryDbLookup, ComponentNonFungibleResourceTotalsHistory> _mostRecentNonFungibleTotalsHistory = new();
    private List<ComponentFungibleResourceDefinition> _fungibleDefinitionsToAdd = new();
    private List<ComponentNonFungibleResourceDefinition> _nonFungibleDefinitionsToAdd = new();
    private List<ComponentFungibleResourceTotalsHistory> _fungibleTotalsHistoryToAdd = new();
    private List<ComponentNonFungibleResourceTotalsHistory> _nonFungibleTotalsHistoryToAdd = new();

    public ComponentResourceProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.FungibleVaultFieldBalanceSubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalFungibleVaultEntity>();
            var resourceEntity = _referencedEntities.GetByDatabaseId(vaultEntity.GetResourceEntityId());

            var globalId = referencedEntity.IsGlobal ? referencedEntity.DatabaseId : referencedEntity.DatabaseGlobalAncestorId;
            var ownerId = referencedEntity.IsGlobal ? referencedEntity.DatabaseId : referencedEntity.DatabaseOwnerAncestorId;

            _observedFungibleDefinitions.TryAdd(new ComponentResourceDefinitionDbLookup(globalId, resourceEntity.DatabaseId), stateVersion);
            _observedFungibleDefinitions.TryAdd(new ComponentResourceDefinitionDbLookup(ownerId, resourceEntity.DatabaseId), stateVersion);
        }

        if (substateData is CoreModel.NonFungibleVaultFieldBalanceSubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
            var resourceEntity = _referencedEntities.GetByDatabaseId(vaultEntity.GetResourceEntityId());

            var globalId = referencedEntity.IsGlobal ? referencedEntity.DatabaseId : referencedEntity.DatabaseGlobalAncestorId;
            var ownerId = referencedEntity.IsGlobal ? referencedEntity.DatabaseId : referencedEntity.DatabaseOwnerAncestorId;

            _observedNonFungibleDefinitions.TryAdd(new ComponentResourceDefinitionDbLookup(globalId, resourceEntity.DatabaseId), stateVersion);
            _observedNonFungibleDefinitions.TryAdd(new ComponentResourceDefinitionDbLookup(ownerId, resourceEntity.DatabaseId), stateVersion);
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
