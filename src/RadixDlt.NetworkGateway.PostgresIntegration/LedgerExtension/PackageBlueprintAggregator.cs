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

using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal abstract record PackageBlueprintChange(long StateVersion, long PackageEntityId, string Name, string Version);

internal record PackageBlueprintDefinitionChange(long StateVersion, long PackageEntityId, string Name, string Version, string Definition)
    : PackageBlueprintChange(StateVersion, PackageEntityId, Name, Version);

internal record PackageBlueprintDependantEntityIdsChange(long StateVersion, long PackageEntityId, string Name, string Version, List<long>? DependantEntityIds = null)
    : PackageBlueprintChange(StateVersion, PackageEntityId, Name, Version);

internal record PackageBlueprintAuthTemplateChange(long StateVersion, long PackageEntityId, string Name, string Version, string? AuthTemplate = null, bool? AuthTemplateIsLocked = null)
    : PackageBlueprintChange(StateVersion, PackageEntityId, Name, Version);

internal record PackageBlueprintRoyaltyConfigChange(long StateVersion, long PackageEntityId, string Name, string Version, string? RoyaltyConfig = null, bool? RoyaltyConfigIsLocked = null)
    : PackageBlueprintChange(StateVersion, PackageEntityId, Name, Version);

internal static class PackageBlueprintAggregator
{
    public static (List<PackageBlueprintHistory> PackageBlueprintHistoryToAdd, List<PackageBlueprintAggregateHistory> PackageBlueprintAggregateHistoryToAdd) AggregatePackageBlueprint(
        List<PackageBlueprintChange> packageBlueprintChanges,
        Dictionary<PackageBlueprintLookup, PackageBlueprintHistory> mostRecentPackageBlueprintHistory,
        Dictionary<long, PackageBlueprintAggregateHistory> mostRecentPackageBlueprintAggregateHistory,
        SequencesHolder sequences)
    {
        var packageBlueprintHistoryToAdd = new List<PackageBlueprintHistory>();
        var packageBlueprintAggregateHistoryToAdd = new List<PackageBlueprintAggregateHistory>();

        var packageGroups = packageBlueprintChanges.GroupBy(x => new { x.PackageEntityId, x.StateVersion });

        foreach (var packageGroup in packageGroups)
        {
            var packageEntityId = packageGroup.Key.PackageEntityId;
            var stateVersion = packageGroup.Key.StateVersion;
            mostRecentPackageBlueprintAggregateHistory.TryGetValue(packageEntityId, out var existingPackageBlueprintAggregate);

            PackageBlueprintAggregateHistory packageBlueprintAggregate;

            if (existingPackageBlueprintAggregate == null)
            {
                packageBlueprintAggregate = new PackageBlueprintAggregateHistory
                {
                    Id = sequences.PackageBlueprintAggregateHistorySequence++,
                    FromStateVersion = stateVersion,
                    PackageEntityId = packageEntityId,
                    PackageBlueprintIds = new List<long>(),
                };

                mostRecentPackageBlueprintAggregateHistory[packageEntityId] = packageBlueprintAggregate;
            }
            else
            {
                packageBlueprintAggregate = existingPackageBlueprintAggregate;
                packageBlueprintAggregate.Id = sequences.PackageBlueprintAggregateHistorySequence++;
                packageBlueprintAggregate.FromStateVersion = stateVersion;
            }

            var packageBlueprintGroups = packageGroup
                .GroupBy(x => new { x.PackageEntityId, x.Name, x.Version, x.StateVersion });

            foreach (var packageBlueprintGroup in packageBlueprintGroups)
            {
                var lookup = new PackageBlueprintLookup(packageEntityId, packageBlueprintGroup.Key.Name, packageBlueprintGroup.Key.Version);
                mostRecentPackageBlueprintHistory.TryGetValue(lookup, out var existingPackageBlueprint);

                PackageBlueprintHistory packageBlueprintHistory;

                if (existingPackageBlueprint != null)
                {
                    var previousPackageBlueprintHistoryId = existingPackageBlueprint.Id;

                    packageBlueprintHistory = existingPackageBlueprint;
                    packageBlueprintHistory.Id = sequences.PackageBlueprintHistorySequence++;
                    packageBlueprintHistory.FromStateVersion = packageBlueprintGroup.Key.StateVersion;

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
                    mostRecentPackageBlueprintHistory[lookup] = packageBlueprintHistory;

                    packageBlueprintAggregate.PackageBlueprintIds.Add(packageBlueprintHistory.Id);
                }

                foreach (var change in packageBlueprintGroup)
                {
                    switch (change)
                    {
                        case PackageBlueprintDefinitionChange definitionChange:
                        {
                            packageBlueprintHistory.Definition = definitionChange.Definition;
                            break;
                        }

                        case PackageBlueprintAuthTemplateChange authTemplateChange:
                        {
                            packageBlueprintHistory.AuthTemplate = authTemplateChange.AuthTemplate;
                            packageBlueprintHistory.AuthTemplateIsLocked = authTemplateChange.AuthTemplateIsLocked;
                            break;
                        }

                        case PackageBlueprintRoyaltyConfigChange royaltyConfigChange:
                        {
                            packageBlueprintHistory.RoyaltyConfig = royaltyConfigChange.RoyaltyConfig;
                            packageBlueprintHistory.RoyaltyConfigIsLocked = royaltyConfigChange.RoyaltyConfigIsLocked;
                            break;
                        }

                        case PackageBlueprintDependantEntityIdsChange dependantEntityIdsChange:
                        {
                            packageBlueprintHistory.DependantEntityIds = dependantEntityIdsChange.DependantEntityIds;
                            break;
                        }

                        default: throw new UnreachableException($"Unexpected type of package blueprint change: {change.GetType()}");
                    }
                }

                packageBlueprintHistoryToAdd.Add(packageBlueprintHistory);
            }

            packageBlueprintAggregateHistoryToAdd.Add(packageBlueprintAggregate);
        }

        return (packageBlueprintHistoryToAdd, packageBlueprintAggregateHistoryToAdd);
    }
}
