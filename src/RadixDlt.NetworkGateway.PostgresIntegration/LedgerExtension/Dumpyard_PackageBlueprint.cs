using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct PackageBlueprintDbLookup(long PackageEntityId, string Name, string Version);

internal record PackageBlueprintChange(long StateVersion)
{
    public CoreModel.PackageBlueprintDefinitionEntrySubstate? PackageBlueprintDefinition { get; set; }

    public CoreModel.PackageBlueprintDependenciesEntrySubstate? PackageBlueprintDependencies { get; set; }

    public CoreModel.PackageBlueprintRoyaltyEntrySubstate? PackageBlueprintRoyalty { get; set; }

    public CoreModel.PackageBlueprintAuthTemplateEntrySubstate? PackageBlueprintAuthTemplate { get; set; }
}

internal class Dumpyard_PackageBlueprint
{
    private readonly ReferencedEntityDictionary _referencedEntities;

    private Dictionary<PackageBlueprintDbLookup, PackageBlueprintChange> _changePointers = new();
    private List<PackageBlueprintDbLookup> _changes = new();

    private Dictionary<PackageBlueprintDbLookup, PackageBlueprintHistory> _mostRecentEntries = new();
    private Dictionary<long, PackageBlueprintAggregateHistory> _mostRecentAggregates = new();

    private List<PackageBlueprintHistory> _entriesToAdd = new();
    private List<PackageBlueprintAggregateHistory> _aggregatesToAdd = new();

    public Dumpyard_PackageBlueprint(ReferencedEntityDictionary referencedEntities)
    {
        _referencedEntities = referencedEntities;
    }

    public void AcceptUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.PackageBlueprintDefinitionEntrySubstate packageBlueprintDefinition)
        {
            _changePointers
                .GetOrAdd(new PackageBlueprintDbLookup(referencedEntity.DatabaseId, packageBlueprintDefinition.Key.BlueprintName, packageBlueprintDefinition.Key.BlueprintVersion), lookup =>
                {
                    _changes.Add(lookup);

                    return new PackageBlueprintChange(stateVersion);
                })
                .PackageBlueprintDefinition = packageBlueprintDefinition;
        }

        if (substateData is CoreModel.PackageBlueprintDependenciesEntrySubstate packageBlueprintDependencies)
        {
            _changePointers
                .GetOrAdd(new PackageBlueprintDbLookup(referencedEntity.DatabaseId, packageBlueprintDependencies.Key.BlueprintName, packageBlueprintDependencies.Key.BlueprintVersion), lookup =>
                {
                    _changes.Add(lookup);

                    return new PackageBlueprintChange(stateVersion);
                })
                .PackageBlueprintDependencies = packageBlueprintDependencies;
        }

        if (substateData is CoreModel.PackageBlueprintRoyaltyEntrySubstate packageBlueprintRoyalty)
        {
            _changePointers
                .GetOrAdd(new PackageBlueprintDbLookup(referencedEntity.DatabaseId, packageBlueprintRoyalty.Key.BlueprintName, packageBlueprintRoyalty.Key.BlueprintVersion), lookup =>
                {
                    _changes.Add(lookup);

                    return new PackageBlueprintChange(stateVersion);
                })
                .PackageBlueprintRoyalty = packageBlueprintRoyalty;
        }

        if (substateData is CoreModel.PackageBlueprintAuthTemplateEntrySubstate packageBlueprintAuthTemplate)
        {
            _changePointers
                .GetOrAdd(new PackageBlueprintDbLookup(referencedEntity.DatabaseId, packageBlueprintAuthTemplate.Key.BlueprintName, packageBlueprintAuthTemplate.Key.BlueprintVersion), lookup =>
                {
                    _changes.Add(lookup);

                    return new PackageBlueprintChange(stateVersion);
                })
                .PackageBlueprintAuthTemplate = packageBlueprintAuthTemplate;
        }
    }

    public async Task LoadMostRecents(ReadHelper readHelper, CancellationToken token = default)
    {
        _mostRecentEntries = await readHelper.MostRecentPackageBlueprintHistoryFor(_changePointers.Keys, token);
        _mostRecentAggregates = await readHelper.MostRecentPackageBlueprintAggregateHistoryFor(_changePointers.Keys, token);
    }

    public void PrepareAdd(SequencesHolder sequences)
    {
        foreach (var lookup in _changes)
        {
            var change = _changePointers[lookup];

            var packageEntityId = lookup.PackageEntityId;
            var stateVersion = change.StateVersion;

            _mostRecentAggregates.TryGetValue(packageEntityId, out var existingPackageBlueprintAggregate);

            PackageBlueprintAggregateHistory packageBlueprintAggregate;

            if (existingPackageBlueprintAggregate == null || existingPackageBlueprintAggregate.FromStateVersion != change.StateVersion)
            {
                packageBlueprintAggregate = new PackageBlueprintAggregateHistory
                {
                    Id = sequences.PackageBlueprintAggregateHistorySequence++,
                    FromStateVersion = stateVersion,
                    PackageEntityId = packageEntityId,
                    PackageBlueprintIds = existingPackageBlueprintAggregate?.PackageBlueprintIds ?? new List<long>(),
                };

                _mostRecentAggregates[packageEntityId] = packageBlueprintAggregate;
                _aggregatesToAdd.Add(packageBlueprintAggregate);
            }
            else
            {
                packageBlueprintAggregate = existingPackageBlueprintAggregate;
            }

            _mostRecentEntries.TryGetValue(lookup, out var existingPackageBlueprint);

            PackageBlueprintHistory packageBlueprintHistory;

            if (existingPackageBlueprint != null)
            {
                var previousPackageBlueprintHistoryId = existingPackageBlueprint.Id;

                packageBlueprintHistory = existingPackageBlueprint;
                packageBlueprintHistory.Id = sequences.PackageBlueprintHistorySequence++;
                packageBlueprintHistory.FromStateVersion = change.StateVersion;

                packageBlueprintAggregate.PackageBlueprintIds.Remove(previousPackageBlueprintHistoryId);
                packageBlueprintAggregate.PackageBlueprintIds.Add(packageBlueprintHistory.Id);
            }
            else
            {
                packageBlueprintHistory = new PackageBlueprintHistory
                {
                    Id = sequences.PackageBlueprintHistorySequence++,
                    PackageEntityId = packageEntityId,
                    FromStateVersion = stateVersion,
                    Name = lookup.Name,
                    Version = lookup.Version,
                };

                _mostRecentEntries[lookup] = packageBlueprintHistory;
                packageBlueprintAggregate.PackageBlueprintIds.Add(packageBlueprintHistory.Id);
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

    public async Task<int> WriteNew(WriteHelper writeHelper, CancellationToken token)
    {
        var rowsInserted = 0;

        rowsInserted += await writeHelper.CopyPackageBlueprintHistory(_entriesToAdd, token);
        rowsInserted += await writeHelper.CopyPackageBlueprintAggregateHistory(_aggregatesToAdd, token);

        return rowsInserted;
    }
}
