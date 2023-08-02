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

using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record FungibleVaultSnapshot(ReferencedEntity ReferencedVault, ReferencedEntity ReferencedResource, long StateVersion, TokenAmount Balance);

internal record NonFungibleVaultChange(ReferencedEntity ReferencedVault, ReferencedEntity ReferencedResource, long StateVersion, string NonFungibleId, bool IsDeleted);

internal record NonFungibleIdDataChangePointer(ReferencedEntity ReferencedResource, string NonFungibleId, CoreModel.NonFungibleResourceManagerDataEntrySubstate Substate, long StateVersion)
{
    public byte[]? Value => Substate.Value?.DataStruct.StructData.GetDataBytes();

    public bool IsDeleted => Substate.Value == null;

    public bool IsLocked => Substate.IsLocked;
}

internal record struct NonFungibleIdLookup(long ResourceEntityId, string NonFungibleId);

internal class ResourceProcessor
{
    public List<VaultHistory> VaultHistoryToAdd { get; } = new();

    public List<NonFungibleId> NonFungibleIdsToAdd { get; } = new();

    public List<NonFungibleIdDataHistory> NonFungibleIdDataHistoryToAdd { get; } = new();

    public List<NonFungibleIdStoreHistory> NonFungibleIdStoreHistoryToAdd { get; } = new();

    private Dictionary<long, NonFungibleVaultHistory> MostRecentNonFungibleVaultHistory { get; init; } = null!;

    private Dictionary<long, NonFungibleIdStoreHistory> MostRecentNonFungibleIdStoreHistory { get; init; } = null!;

    private Dictionary<NonFungibleIdLookup, NonFungibleId> ExistingNonFungibleIds { get; init; } = null!;

    private readonly List<FungibleVaultSnapshot> _fungibleVaultSnapshots;
    private readonly List<NonFungibleVaultChange> _nonFungibleVaultChanges;
    private readonly List<NonFungibleIdDataChangePointer> _nonFungibleIdDataChanges;
    private readonly SequencesHolder _sequences;

    private ResourceProcessor(List<FungibleVaultSnapshot> fungibleVaultSnapshots, List<NonFungibleVaultChange> nonFungibleVaultChanges, List<NonFungibleIdDataChangePointer> nonFungibleIdDataChanges, SequencesHolder sequences)
    {
        _fungibleVaultSnapshots = fungibleVaultSnapshots;
        _nonFungibleVaultChanges = nonFungibleVaultChanges;
        _nonFungibleIdDataChanges = nonFungibleIdDataChanges;
        _sequences = sequences;
    }

    public static async Task<ResourceProcessor> Create(List<FungibleVaultSnapshot> fungibleVaultSnapshots, List<NonFungibleVaultChange> nonFungibleVaultChanges, List<NonFungibleIdDataChangePointer> nonFungibleIdChangePointers, ReadHelper readHelper, SequencesHolder sequences, CancellationToken token)
    {
        return new ResourceProcessor(fungibleVaultSnapshots, nonFungibleVaultChanges, nonFungibleIdChangePointers, sequences)
        {
            MostRecentNonFungibleVaultHistory = await readHelper.MostRecentNonFungibleVaultHistory(nonFungibleVaultChanges, token),
            MostRecentNonFungibleIdStoreHistory = await readHelper.MostRecentNonFungibleIdStoreHistoryFor(nonFungibleIdChangePointers, token),
            ExistingNonFungibleIds = await readHelper.ExistingNonFungibleIdsFor(nonFungibleIdChangePointers, nonFungibleVaultChanges, token),
        };
    }

    public void Process()
    {
        foreach (var e in _fungibleVaultSnapshots)
        {
            VaultHistoryToAdd.Add(new FungibleVaultHistory
            {
                Id = _sequences.VaultHistorySequence++,
                FromStateVersion = e.StateVersion,
                OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                VaultEntityId = e.ReferencedVault.DatabaseId,
                ResourceEntityId = e.ReferencedResource.DatabaseId,
                IsRoyaltyVault = e.ReferencedVault.GetDatabaseEntity<InternalFungibleVaultEntity>().IsRoyaltyVault,
                Balance = e.Balance,
            });
        }

        foreach (var e in _nonFungibleVaultChanges)
        {
            var nonFungibleIdLookup = new NonFungibleIdLookup(e.ReferencedResource.DatabaseId, e.NonFungibleId);

            NonFungibleVaultHistory vaultHistory;
            NonFungibleId nonFungibleId;

            if (!ExistingNonFungibleIds.TryGetValue(nonFungibleIdLookup, out var existingIdData))
            {
                nonFungibleId = new NonFungibleId
                {
                    Id = _sequences.NonFungibleIdsSequence++,
                    FromStateVersion = e.StateVersion,
                    NonFungibleResourceEntityId = e.ReferencedResource.DatabaseId,
                    SimpleRepresentation = e.NonFungibleId,
                };

                NonFungibleIdsToAdd.Add(nonFungibleId);
                ExistingNonFungibleIds[nonFungibleIdLookup] = nonFungibleId;
            }
            else
            {
                nonFungibleId = existingIdData;
            }

            if (!MostRecentNonFungibleVaultHistory.TryGetValue(e.ReferencedVault.DatabaseId, out var previousVaultHistory) || previousVaultHistory.FromStateVersion != e.StateVersion)
            {
                vaultHistory = new NonFungibleVaultHistory
                {
                    Id = _sequences.VaultHistorySequence++,
                    FromStateVersion = e.StateVersion,
                    OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                    GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                    VaultEntityId = e.ReferencedVault.DatabaseId,
                    ResourceEntityId = e.ReferencedResource.DatabaseId,
                    NonFungibleIdIds = new List<long>(),
                };

                if (previousVaultHistory != null)
                {
                    vaultHistory.NonFungibleIdIds.AddRange(previousVaultHistory.NonFungibleIdIds);
                }

                VaultHistoryToAdd.Add(vaultHistory);
                MostRecentNonFungibleVaultHistory[e.ReferencedVault.DatabaseId] = vaultHistory;
            }
            else
            {
                vaultHistory = previousVaultHistory;
            }

            var position = vaultHistory.NonFungibleIdIds.IndexOf(nonFungibleId.Id);

            if (position != -1)
            {
                vaultHistory.NonFungibleIdIds.RemoveAt(position);
            }

            if (!e.IsDeleted)
            {
                vaultHistory.NonFungibleIdIds.Insert(0, nonFungibleId.Id);
            }
        }

        foreach (var e in _nonFungibleIdDataChanges)
        {
            var nonFungibleIdLookup = new NonFungibleIdLookup(e.ReferencedResource.DatabaseId, e.NonFungibleId);

            NonFungibleIdStoreHistory storeHistory;
            NonFungibleId nonFungibleId;

            if (!ExistingNonFungibleIds.TryGetValue(nonFungibleIdLookup, out var existingIdData))
            {
                nonFungibleId = new NonFungibleId
                {
                    Id = _sequences.NonFungibleIdsSequence++,
                    FromStateVersion = e.StateVersion,
                    NonFungibleResourceEntityId = e.ReferencedResource.DatabaseId,
                    SimpleRepresentation = e.NonFungibleId,
                };

                NonFungibleIdsToAdd.Add(nonFungibleId);
                ExistingNonFungibleIds[nonFungibleIdLookup] = nonFungibleId;
            }
            else
            {
                nonFungibleId = existingIdData;
            }

            if (!MostRecentNonFungibleIdStoreHistory.TryGetValue(e.ReferencedResource.DatabaseId, out var previousStoreHistory) || previousStoreHistory.FromStateVersion != e.StateVersion)
            {
                storeHistory = new NonFungibleIdStoreHistory
                {
                    Id = _sequences.NonFungibleIdStoreHistorySequence++,
                    FromStateVersion = e.StateVersion,
                    NonFungibleResourceEntityId = e.ReferencedResource.DatabaseId,
                    NonFungibleIdIds = new List<long>(),
                };

                if (previousStoreHistory != null)
                {
                    storeHistory.NonFungibleIdIds.AddRange(previousStoreHistory.NonFungibleIdIds);
                }

                NonFungibleIdStoreHistoryToAdd.Add(storeHistory);
                MostRecentNonFungibleIdStoreHistory[e.ReferencedResource.DatabaseId] = storeHistory;
            }
            else
            {
                storeHistory = previousStoreHistory;
            }

            var position = storeHistory.NonFungibleIdIds.IndexOf(nonFungibleId.Id);

            if (position != -1)
            {
                storeHistory.NonFungibleIdIds.RemoveAt(position);
            }

            if (!e.IsDeleted)
            {
                storeHistory.NonFungibleIdIds.Insert(0, nonFungibleId.Id);
            }

            NonFungibleIdDataHistoryToAdd.Add(new NonFungibleIdDataHistory
            {
                Id = _sequences.NonFungibleIdDataHistorySequence++,
                FromStateVersion = e.StateVersion,
                NonFungibleIdDataId = nonFungibleId.Id,
                Data = e.Value,
                IsDeleted = e.IsDeleted,
                IsLocked = e.IsLocked,
            });
        }
    }
}
