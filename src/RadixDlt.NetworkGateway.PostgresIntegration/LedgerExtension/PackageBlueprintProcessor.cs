using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct PackageBlueprintDbLookup(long PackageEntityId, string Name, string Version);

internal record PackageBlueprintChangePointer(long StateVersion)
{
    public CoreModel.PackageBlueprintDefinitionEntrySubstate? PackageBlueprintDefinition { get; set; }

    public CoreModel.PackageBlueprintDependenciesEntrySubstate? PackageBlueprintDependencies { get; set; }

    public CoreModel.PackageBlueprintRoyaltyEntrySubstate? PackageBlueprintRoyalty { get; set; }

    public CoreModel.PackageBlueprintAuthTemplateEntrySubstate? PackageBlueprintAuthTemplate { get; set; }
}

internal class PackageBlueprintProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private Dictionary<PackageBlueprintDbLookup, PackageBlueprintChangePointer> _changePointers = new();
    private List<PackageBlueprintDbLookup> _changeOrder = new();

    private Dictionary<long, PackageBlueprintAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<PackageBlueprintDbLookup, PackageBlueprintHistory> _mostRecentEntries = new();

    private List<PackageBlueprintAggregateHistory> _aggregatesToAdd = new();
    private List<PackageBlueprintHistory> _entriesToAdd = new();

    public PackageBlueprintProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.PackageBlueprintDefinitionEntrySubstate packageBlueprintDefinition)
        {
            _changePointers
                .GetOrAdd(new PackageBlueprintDbLookup(referencedEntity.DatabaseId, packageBlueprintDefinition.Key.BlueprintName, packageBlueprintDefinition.Key.BlueprintVersion), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new PackageBlueprintChangePointer(stateVersion);
                })
                .PackageBlueprintDefinition = packageBlueprintDefinition;
        }

        if (substateData is CoreModel.PackageBlueprintDependenciesEntrySubstate packageBlueprintDependencies)
        {
            _changePointers
                .GetOrAdd(new PackageBlueprintDbLookup(referencedEntity.DatabaseId, packageBlueprintDependencies.Key.BlueprintName, packageBlueprintDependencies.Key.BlueprintVersion), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new PackageBlueprintChangePointer(stateVersion);
                })
                .PackageBlueprintDependencies = packageBlueprintDependencies;
        }

        if (substateData is CoreModel.PackageBlueprintRoyaltyEntrySubstate packageBlueprintRoyalty)
        {
            _changePointers
                .GetOrAdd(new PackageBlueprintDbLookup(referencedEntity.DatabaseId, packageBlueprintRoyalty.Key.BlueprintName, packageBlueprintRoyalty.Key.BlueprintVersion), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new PackageBlueprintChangePointer(stateVersion);
                })
                .PackageBlueprintRoyalty = packageBlueprintRoyalty;
        }

        if (substateData is CoreModel.PackageBlueprintAuthTemplateEntrySubstate packageBlueprintAuthTemplate)
        {
            _changePointers
                .GetOrAdd(new PackageBlueprintDbLookup(referencedEntity.DatabaseId, packageBlueprintAuthTemplate.Key.BlueprintName, packageBlueprintAuthTemplate.Key.BlueprintVersion), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new PackageBlueprintChangePointer(stateVersion);
                })
                .PackageBlueprintAuthTemplate = packageBlueprintAuthTemplate;
        }
    }

    public void ProcessChanges()
    {
        foreach (var lookup in _changeOrder)
        {
            var change = _changePointers[lookup];

            PackageBlueprintAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.PackageEntityId, out var previousAggregate) || previousAggregate.FromStateVersion != change.StateVersion)
            {
                aggregate = new PackageBlueprintAggregateHistory
                {
                    Id = _context.Sequences.PackageBlueprintAggregateHistorySequence++,
                    FromStateVersion = change.StateVersion,
                    PackageEntityId = lookup.PackageEntityId,
                    PackageBlueprintIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.PackageBlueprintIds.AddRange(previousAggregate.PackageBlueprintIds);
                }

                _mostRecentAggregates[lookup.PackageEntityId] = aggregate;
                _aggregatesToAdd.Add(aggregate);
            }
            else
            {
                aggregate = previousAggregate;
            }

            // TODO change all the code below and follow the existing pattern
            PackageBlueprintHistory packageBlueprintHistory;

            _mostRecentEntries.TryGetValue(lookup, out var existingPackageBlueprint);

            if (existingPackageBlueprint != null)
            {
                var previousPackageBlueprintHistoryId = existingPackageBlueprint.Id;

                packageBlueprintHistory = existingPackageBlueprint;
                packageBlueprintHistory.Id = _context.Sequences.PackageBlueprintHistorySequence++;
                packageBlueprintHistory.FromStateVersion = change.StateVersion;

                aggregate.PackageBlueprintIds.Remove(previousPackageBlueprintHistoryId);
                aggregate.PackageBlueprintIds.Add(packageBlueprintHistory.Id);
            }
            else
            {
                packageBlueprintHistory = new PackageBlueprintHistory
                {
                    Id = _context.Sequences.PackageBlueprintHistorySequence++,
                    PackageEntityId = lookup.PackageEntityId,
                    FromStateVersion = change.StateVersion,
                    Name = lookup.Name,
                    Version = lookup.Version,
                };

                _mostRecentEntries[lookup] = packageBlueprintHistory;
                aggregate.PackageBlueprintIds.Add(packageBlueprintHistory.Id);
            }

            if (change.PackageBlueprintDefinition != null)
            {
                packageBlueprintHistory.Definition = change.PackageBlueprintDefinition.Value.Definition.ToJson();
            }

            if (change.PackageBlueprintDependencies != null)
            {
                packageBlueprintHistory.DependantEntityIds = change.PackageBlueprintDependencies.Value.Dependencies.Dependencies.Select(address => _referencedEntities.Get((EntityAddress)address).DatabaseId).ToList();
            }

            if (change.PackageBlueprintRoyalty != null)
            {
                packageBlueprintHistory.RoyaltyConfig = change.PackageBlueprintRoyalty.Value.RoyaltyConfig.ToJson();
                packageBlueprintHistory.RoyaltyConfigIsLocked = change.PackageBlueprintRoyalty.IsLocked;
            }

            if (change.PackageBlueprintAuthTemplate != null)
            {
                packageBlueprintHistory.AuthTemplate = change.PackageBlueprintAuthTemplate.Value.AuthConfig.ToJson();
                packageBlueprintHistory.AuthTemplateIsLocked = change.PackageBlueprintAuthTemplate.IsLocked;
            }

            _entriesToAdd.Add(packageBlueprintHistory);
        }
    }

    public async Task LoadMostRecent()
    {
        _mostRecentEntries = await _context.ReadHelper.MostRecentPackageBlueprintHistoryFor(_changePointers.Keys, _context.Token);
        _mostRecentAggregates = await _context.ReadHelper.MostRecentPackageBlueprintAggregateHistoryFor(_changePointers.Keys, _context.Token);
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyPackageBlueprintHistory();
        rowsInserted += await CopyPackageBlueprintAggregateHistory();

        return rowsInserted;
    }

    private Task<int> CopyPackageBlueprintHistory() => _context.WriteHelper.Copy(
        _entriesToAdd,
        "COPY package_blueprint_history (id, from_state_version, package_entity_id, name, version, definition, dependant_entity_ids, auth_template, auth_template_is_locked, royalty_config, royalty_config_is_locked) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.PackageEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Name, NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.Version, NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.Definition, NpgsqlDbType.Jsonb, token);
            await writer.WriteNullableAsync(e.DependantEntityIds?.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AuthTemplate, NpgsqlDbType.Jsonb, token);
            await writer.WriteNullableAsync(e.AuthTemplateIsLocked, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.RoyaltyConfig, NpgsqlDbType.Jsonb, token);
            await writer.WriteNullableAsync(e.RoyaltyConfigIsLocked, NpgsqlDbType.Boolean, token);
        });

    private Task<int> CopyPackageBlueprintAggregateHistory() => _context.WriteHelper.Copy(
        _aggregatesToAdd,
        "COPY package_blueprint_aggregate_history (id, from_state_version, package_entity_id, package_blueprint_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.PackageEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.PackageBlueprintIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });
}
