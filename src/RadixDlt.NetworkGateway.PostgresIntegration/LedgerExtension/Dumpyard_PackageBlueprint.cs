using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
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

internal class Dumpyard_PackageBlueprint
{
    private readonly Dumpyard_Context _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private Dictionary<PackageBlueprintDbLookup, PackageBlueprintChangePointer> _changePointers = new();
    private List<PackageBlueprintDbLookup> _changeOrder = new();

    private Dictionary<long, PackageBlueprintAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<PackageBlueprintDbLookup, PackageBlueprintHistory> _mostRecentEntries = new();

    private List<PackageBlueprintAggregateHistory> _aggregatesToAdd = new();
    private List<PackageBlueprintHistory> _entriesToAdd = new();

    public Dumpyard_PackageBlueprint(Dumpyard_Context context, ReferencedEntityDictionary referencedEntities)
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

    public async Task LoadMostRecents()
    {
        _mostRecentEntries = await _context.ReadHelper.MostRecentPackageBlueprintHistoryFor(_changePointers.Keys, _context.Token);
        _mostRecentAggregates = await _context.ReadHelper.MostRecentPackageBlueprintAggregateHistoryFor(_changePointers.Keys, _context.Token);
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await _context.WriteHelper.CopyPackageBlueprintHistory(_entriesToAdd, _context.Token);
        rowsInserted += await _context.WriteHelper.CopyPackageBlueprintAggregateHistory(_aggregatesToAdd, _context.Token);

        return rowsInserted;
    }
}
