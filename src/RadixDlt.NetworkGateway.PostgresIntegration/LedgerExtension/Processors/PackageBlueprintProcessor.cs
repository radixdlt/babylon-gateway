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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct PackageBlueprintChangePointerLookup(long PackageEntityId, string Name, string Version, long StateVersion);

internal record struct PackageBlueprintDbLookup(long PackageEntityId, string Name, string Version);

internal record PackageBlueprintChangePointer
{
    public CoreModel.PackageBlueprintDefinitionEntrySubstate? PackageBlueprintDefinition { get; set; }

    public CoreModel.PackageBlueprintDependenciesEntrySubstate? PackageBlueprintDependencies { get; set; }

    public CoreModel.PackageBlueprintRoyaltyEntrySubstate? PackageBlueprintRoyalty { get; set; }

    public CoreModel.PackageBlueprintAuthTemplateEntrySubstate? PackageBlueprintAuthTemplate { get; set; }
}

internal class PackageBlueprintProcessor : IProcessorBase, ISubstateUpsertProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private readonly ChangeTracker<PackageBlueprintChangePointerLookup, PackageBlueprintChangePointer> _changes = new();

    private readonly Dictionary<long, PackageBlueprintAggregateHistory> _mostRecentAggregates = new();
    private readonly Dictionary<PackageBlueprintDbLookup, PackageBlueprintHistory> _mostRecentEntries = new();

    private readonly List<PackageBlueprintAggregateHistory> _aggregatesToAdd = new();
    private readonly List<PackageBlueprintHistory> _entriesToAdd = new();

    public PackageBlueprintProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.PackageBlueprintDefinitionEntrySubstate packageBlueprintDefinition)
        {
            _changes
                .GetOrAdd(
                    new PackageBlueprintChangePointerLookup(referencedEntity.DatabaseId, packageBlueprintDefinition.Key.BlueprintName, packageBlueprintDefinition.Key.BlueprintVersion, stateVersion),
                    _ => new PackageBlueprintChangePointer())
                .PackageBlueprintDefinition = packageBlueprintDefinition;
        }

        if (substateData is CoreModel.PackageBlueprintDependenciesEntrySubstate packageBlueprintDependencies)
        {
            _changes
                .GetOrAdd(
                    new PackageBlueprintChangePointerLookup(
                        referencedEntity.DatabaseId,
                        packageBlueprintDependencies.Key.BlueprintName,
                        packageBlueprintDependencies.Key.BlueprintVersion,
                        stateVersion),
                    _ => new PackageBlueprintChangePointer())
                .PackageBlueprintDependencies = packageBlueprintDependencies;
        }

        if (substateData is CoreModel.PackageBlueprintRoyaltyEntrySubstate packageBlueprintRoyalty)
        {
            _changes
                .GetOrAdd(
                    new PackageBlueprintChangePointerLookup(referencedEntity.DatabaseId, packageBlueprintRoyalty.Key.BlueprintName, packageBlueprintRoyalty.Key.BlueprintVersion, stateVersion),
                    _ => new PackageBlueprintChangePointer())
                .PackageBlueprintRoyalty = packageBlueprintRoyalty;
        }

        if (substateData is CoreModel.PackageBlueprintAuthTemplateEntrySubstate packageBlueprintAuthTemplate)
        {
            _changes
                .GetOrAdd(
                    new PackageBlueprintChangePointerLookup(
                        referencedEntity.DatabaseId,
                        packageBlueprintAuthTemplate.Key.BlueprintName,
                        packageBlueprintAuthTemplate.Key.BlueprintVersion,
                        stateVersion),
                    _ => new PackageBlueprintChangePointer())
                .PackageBlueprintAuthTemplate = packageBlueprintAuthTemplate;
        }
    }

    public async Task LoadDependenciesAsync()
    {
        _mostRecentEntries.AddRange(await MostRecentPackageBlueprintHistory());
        _mostRecentAggregates.AddRange(await MostRecentPackageBlueprintAggregateHistory());
    }

    public void ProcessChanges()
    {
        foreach (var (lookup, change) in _changes.AsEnumerable())
        {
            PackageBlueprintAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.PackageEntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new PackageBlueprintAggregateHistory
                {
                    Id = _context.Sequences.PackageBlueprintAggregateHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    PackageEntityId = lookup.PackageEntityId,
                    PackageBlueprintIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.PackageBlueprintIds.AddRange(previousAggregate.PackageBlueprintIds);
                }

                _aggregatesToAdd.Add(aggregate);
                _mostRecentAggregates[lookup.PackageEntityId] = aggregate;
            }
            else
            {
                aggregate = previousAggregate;
            }

            // WARNING! PackageBlueprint[Entry]History can be partially updated, therefore we must optionally clone previous entity similarly to how we handle aggregates
            var entryLookup = new PackageBlueprintDbLookup(lookup.PackageEntityId, lookup.Name, lookup.Version);
            PackageBlueprintHistory entryHistory;

            if (!_mostRecentEntries.TryGetValue(entryLookup, out var previousEntryHistory) || previousEntryHistory.FromStateVersion != lookup.StateVersion)
            {
                entryHistory = new PackageBlueprintHistory
                {
                    Id = _context.Sequences.PackageBlueprintHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    PackageEntityId = lookup.PackageEntityId,
                    Name = lookup.Name,
                    Version = lookup.Version,
                };

                _entriesToAdd.Add(entryHistory);

                if (previousEntryHistory != null)
                {
                    entryHistory.Definition = previousEntryHistory.Definition;
                    entryHistory.DependantEntityIds = previousEntryHistory.DependantEntityIds?.ToList();
                    entryHistory.AuthTemplate = previousEntryHistory.AuthTemplate;
                    entryHistory.AuthTemplateIsLocked = previousEntryHistory.AuthTemplateIsLocked;
                    entryHistory.RoyaltyConfig = previousEntryHistory.RoyaltyConfig;
                    entryHistory.RoyaltyConfigIsLocked = previousEntryHistory.RoyaltyConfigIsLocked;

                    var currentPosition = aggregate.PackageBlueprintIds.IndexOf(previousEntryHistory.Id);

                    if (currentPosition != -1)
                    {
                        aggregate.PackageBlueprintIds.RemoveAt(currentPosition);
                    }
                }

                aggregate.PackageBlueprintIds.Insert(0, entryHistory.Id);

                _mostRecentEntries[entryLookup] = entryHistory;
            }
            else
            {
                entryHistory = previousEntryHistory;
            }

            if (change.PackageBlueprintDefinition != null)
            {
                entryHistory.Definition = change.PackageBlueprintDefinition.Value.Definition.ToJson();
            }

            if (change.PackageBlueprintDependencies != null)
            {
                entryHistory.DependantEntityIds =
                    change.PackageBlueprintDependencies.Value.Dependencies.Dependencies.Select(address => _referencedEntities.Get((EntityAddress)address).DatabaseId).ToList();
            }

            if (change.PackageBlueprintRoyalty != null)
            {
                entryHistory.RoyaltyConfig = change.PackageBlueprintRoyalty.Value.RoyaltyConfig.ToJson();
                entryHistory.RoyaltyConfigIsLocked = change.PackageBlueprintRoyalty.IsLocked;
            }

            if (change.PackageBlueprintAuthTemplate != null)
            {
                entryHistory.AuthTemplate = change.PackageBlueprintAuthTemplate.Value.AuthConfig.ToJson();
                entryHistory.AuthTemplateIsLocked = change.PackageBlueprintAuthTemplate.IsLocked;
            }
        }
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyPackageBlueprintHistory();
        rowsInserted += await CopyPackageBlueprintAggregateHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<PackageBlueprintDbLookup, PackageBlueprintHistory>> MostRecentPackageBlueprintHistory()
    {
        var lookupSet = _changes.Keys.ToHashSet();

        if (!lookupSet.Unzip(x => x.PackageEntityId, x => x.Name, x => x.Version, out var packageEntityIds, out var names, out var versions))
        {
            return ImmutableDictionary<PackageBlueprintDbLookup, PackageBlueprintHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<PackageBlueprintDbLookup, PackageBlueprintHistory>(
            @$"
WITH variables (package_entity_id, name, version) AS (
    SELECT UNNEST({packageEntityIds}), UNNEST({names}), UNNEST({versions})
)
SELECT pbh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM package_blueprint_history
    WHERE package_entity_id = variables.package_entity_id AND name = variables.name AND version = variables.version
    ORDER BY from_state_version DESC
    LIMIT 1
) pbh ON true;",
            e => new PackageBlueprintDbLookup(e.PackageEntityId, e.Name, e.Version));
    }

    private async Task<IDictionary<long, PackageBlueprintAggregateHistory>> MostRecentPackageBlueprintAggregateHistory()
    {
        var packageEntityIds = _changes.Keys.Select(x => x.PackageEntityId).ToHashSet().ToList();

        if (!packageEntityIds.Any())
        {
            return ImmutableDictionary<long, PackageBlueprintAggregateHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<long, PackageBlueprintAggregateHistory>(
            $@"
WITH variables (package_entity_id) AS (
    SELECT UNNEST({packageEntityIds})
)
SELECT pbah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM package_blueprint_aggregate_history
    WHERE package_entity_id = variables.package_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) pbah ON true;",
            e => e.PackageEntityId);
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
            await writer.WriteAsync(e.DependantEntityIds?.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AuthTemplate, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(e.AuthTemplateIsLocked, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.RoyaltyConfig, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(e.RoyaltyConfigIsLocked, NpgsqlDbType.Boolean, token);
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
