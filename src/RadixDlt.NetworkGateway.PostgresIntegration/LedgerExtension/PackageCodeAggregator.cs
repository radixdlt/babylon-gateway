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

using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct PackageCodeLookup(long PackageEntityId, ValueBytes CodeHash);

internal abstract record PackageCodeChange(long StateVersion, PackageCodeLookup Lookup);

internal record PackageCodeByteChange(long StateVersion,  PackageCodeLookup Lookup, byte[] Code) : PackageCodeChange(StateVersion, Lookup);

internal record PackageCodeVmChange(long StateVersion,  PackageCodeLookup Lookup, PackageVmType VmType) : PackageCodeChange(StateVersion, Lookup);

internal static class PackageCodeAggregator
{
    public static (List<PackageCodeHistory> PackageCodeHistoryToAdd, List<PackageCodeAggregateHistory> PackageCodeAggregateHistoryToAdd) AggregatePackageCode(
        List<PackageCodeChange> packageCodeChanges,
        Dictionary<PackageCodeLookup, PackageCodeHistory> mostRecentPackageCodeHistory,
        Dictionary<long, PackageCodeAggregateHistory> mostRecentPackageCodeAggregateHistory,
        SequencesHolder sequences)
    {
        var packageCodeHistoryToAdd = new List<PackageCodeHistory>();
        var packageCodeAggregateHistoryToAdd = new List<PackageCodeAggregateHistory>();

        var packageGroups = packageCodeChanges.GroupBy(x => new { x.Lookup.PackageEntityId, x.StateVersion });

        foreach (var packageGroup in packageGroups)
        {
            var packageEntityId = packageGroup.Key.PackageEntityId;
            var stateVersion = packageGroup.Key.StateVersion;

            mostRecentPackageCodeAggregateHistory.TryGetValue(packageEntityId, out var existingPackageCodeAggregate);

            PackageCodeAggregateHistory packageCodeAggregate;

            if (existingPackageCodeAggregate == null)
            {
                packageCodeAggregate = new PackageCodeAggregateHistory
                {
                    Id = sequences.PackageCodeAggregateHistorySequence++,
                    FromStateVersion = stateVersion,
                    PackageEntityId = packageEntityId,
                    PackageCodeIds = new List<long>(),
                };

                mostRecentPackageCodeAggregateHistory[packageEntityId] = packageCodeAggregate;
            }
            else
            {
                packageCodeAggregate = existingPackageCodeAggregate;
                packageCodeAggregate.Id = sequences.PackageCodeAggregateHistorySequence++;
                packageCodeAggregate.FromStateVersion = stateVersion;
            }

            var packageCodeGroups = packageGroup
                .GroupBy(x => new { x.Lookup.PackageEntityId, x.Lookup.CodeHash, x.StateVersion });

            foreach (var packageCodeGroup in packageCodeGroups)
            {
                var lookup = new PackageCodeLookup(packageEntityId, packageCodeGroup.Key.CodeHash);
                mostRecentPackageCodeHistory.TryGetValue(lookup, out var existingPackageCode);

                PackageCodeHistory packageCodeHistory;

                if (existingPackageCode != null)
                {
                    var previousPackageCodeId = existingPackageCode.Id;

                    packageCodeHistory = existingPackageCode;
                    packageCodeHistory.Id = sequences.PackageCodeHistorySequence++;
                    packageCodeHistory.FromStateVersion = packageCodeGroup.Key.StateVersion;

                    packageCodeAggregate.PackageCodeIds.Remove(previousPackageCodeId);
                    packageCodeAggregate.PackageCodeIds.Add(packageCodeHistory.Id);
                }
                else
                {
                    packageCodeHistory = new PackageCodeHistory
                    {
                        Id = sequences.PackageCodeHistorySequence++,
                        PackageEntityId = packageEntityId,
                        FromStateVersion = stateVersion,
                        CodeHash = lookup.CodeHash,
                    };

                    mostRecentPackageCodeHistory[lookup] = packageCodeHistory;

                    packageCodeAggregate.PackageCodeIds.Add(packageCodeHistory.Id);
                }

                foreach (var change in packageCodeGroup)
                {
                    switch (change)
                    {
                        case PackageCodeByteChange codeByteChange:
                        {
                            packageCodeHistory.Code = codeByteChange.Code;
                            break;
                        }

                        case PackageCodeVmChange vmChange:
                        {
                            packageCodeHistory.VmType = vmChange.VmType;
                            break;
                        }

                        default: throw new UnreachableException($"Unexpected type of package code change: {change.GetType()}");
                    }
                }

                packageCodeHistoryToAdd.Add(packageCodeHistory);
            }

            packageCodeAggregateHistoryToAdd.Add(packageCodeAggregate);
        }

        return (packageCodeHistoryToAdd, packageCodeAggregateHistoryToAdd);
    }
}
