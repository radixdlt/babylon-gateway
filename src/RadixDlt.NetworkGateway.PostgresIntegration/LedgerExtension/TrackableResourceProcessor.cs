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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal interface ITheStruct
{
    long EntityId { get; }

    long VaultEntityId { get; }

    long ResourceEntityId { get; }

    long StateVersion { get; }
}

internal sealed record FungibleTheStruct(long EntityId, long VaultEntityId, long ResourceEntityId, long StateVersion, TokenAmount Delta) : ITheStruct;

internal sealed record NonFungibleTheStruct(long EntityId, long VaultEntityId, long ResourceEntityId, long StateVersion, long Delta) : ITheStruct;

internal record struct EntityResourceLookup(long EntityId, long ResourceEntityId);

internal class TrackableResourceProcessor
{
    public List<EntityResourceAggregateHistory> EntityResourceAggregateHistoryToAdd { get; } = new();

    public List<EntityResourceVaultAggregateHistory> EntityResourceVaultAggregateHistoryToAdd { get; } = new();

    public List<EntityResourceAggregatedVaultsHistory> EntityResourceAggregatedVaultsHistoryToAdd { get; } = new();

    private Dictionary<EntityResourceLookup, EntityResourceAggregatedVaultsHistory> MostRecentEntityResourceAggregatedVaultsHistory { get; set; } = null!;

    private Dictionary<EntityResourceLookup, EntityResourceVaultAggregateHistory> MostRecentEntityResourceVaultAggregateHistory { get; set; } = null!;

    private Dictionary<long, EntityResourceAggregateHistory> MostRecentEntityResourceAggregateHistory { get; set; } = null!;

    private readonly List<EntityResourceAggregateHistory> _entityResourceAggregateHistoryCandidates = new();
    private readonly List<EntityResourceVaultAggregateHistory> _entityResourceVaultAggregateHistoryCandidates = new();

    private readonly List<ITheStruct> _trackedEvents = new();
    private SequencesHolder _sequences = null!;

    public bool TryTrackVault(InternalFungibleVaultEntity vaultEntity, GlobalFungibleResourceEntity resourceEntity, TokenAmount delta, long stateVersion)
    {
        if (vaultEntity.IsRoyaltyVault)
        {
            return false;
        }

        _trackedEvents.Add(new FungibleTheStruct(vaultEntity.GlobalAncestorId!.Value, vaultEntity.Id, resourceEntity.Id, stateVersion, delta));

        if (vaultEntity.ParentAncestorId.HasValue && vaultEntity.ParentAncestorId.Value != vaultEntity.GlobalAncestorId)
        {
            _trackedEvents.Add(new FungibleTheStruct(vaultEntity.ParentAncestorId.Value, vaultEntity.Id, resourceEntity.Id, stateVersion, delta));
        }

        return true;
    }

    public bool TryTrackVault(InternalNonFungibleVaultEntity vaultEntity, GlobalNonFungibleResourceEntity resourceEntity, long delta, long stateVersion)
    {
        _trackedEvents.Add(new NonFungibleTheStruct(vaultEntity.GlobalAncestorId!.Value, vaultEntity.Id, resourceEntity.Id, stateVersion, delta));

        if (vaultEntity.ParentAncestorId.HasValue && vaultEntity.ParentAncestorId.Value != vaultEntity.GlobalAncestorId)
        {
            _trackedEvents.Add(new NonFungibleTheStruct(vaultEntity.ParentAncestorId.Value, vaultEntity.Id, resourceEntity.Id, stateVersion, delta));
        }

        return true;
    }

    public async Task LoadDependencies(ReadHelper readHelper, SequencesHolder sequences, CancellationToken token)
    {
        _sequences = sequences;
        MostRecentEntityResourceAggregateHistory = await readHelper.MostRecentEntityResourceAggregateHistoryFor(_trackedEvents, token);
        MostRecentEntityResourceVaultAggregateHistory = await readHelper.MostRecentEntityResourceVaultAggregateHistoryFor(_trackedEvents, token);
        MostRecentEntityResourceAggregatedVaultsHistory = await readHelper.MostRecentEntityResourceAggregatedVaultsHistoryFor(_trackedEvents, token);
    }

    public void DoSth()
    {
        foreach (var e in _trackedEvents)
        {
            var lookup = new EntityResourceLookup(e.EntityId, e.ResourceEntityId);

            // SECTION 1: old AggregateEntityResourceInternal
            EntityResourceAggregateHistory a1;

            if (!MostRecentEntityResourceAggregateHistory.TryGetValue(e.EntityId, out var previousA1) || previousA1.FromStateVersion != e.StateVersion)
            {
                a1 = previousA1 == null
                    ? EntityResourceAggregateHistory.Create(_sequences.EntityResourceAggregateHistorySequence++, e.EntityId, e.StateVersion)
                    : EntityResourceAggregateHistory.CopyOf(_sequences.EntityResourceAggregateHistorySequence++, previousA1, e.StateVersion);

                _entityResourceAggregateHistoryCandidates.Add(a1);
                MostRecentEntityResourceAggregateHistory[e.EntityId] = a1;
            }
            else
            {
                a1 = previousA1;
            }

            switch (e)
            {
                case FungibleTheStruct:
                    a1.TryUpsertFungible(e.ResourceEntityId, e.StateVersion);
                    break;
                case NonFungibleTheStruct:
                    a1.TryUpsertNonFungible(e.ResourceEntityId, e.StateVersion);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }

            // SECTION 2: old AggregateEntityResourceVaultInternal
            EntityResourceVaultAggregateHistory a2;

            if (!MostRecentEntityResourceVaultAggregateHistory.TryGetValue(lookup, out var previousA2) || previousA2.FromStateVersion != e.StateVersion)
            {
                a2 = previousA2 == null
                    ? EntityResourceVaultAggregateHistory.Create(_sequences.EntityResourceAggregateHistorySequence++, e.EntityId, e.ResourceEntityId, e.StateVersion)
                    : EntityResourceVaultAggregateHistory.CopyOf(_sequences.EntityResourceAggregateHistorySequence++, previousA2, e.StateVersion);

                _entityResourceVaultAggregateHistoryCandidates.Add(a2);
                MostRecentEntityResourceVaultAggregateHistory[lookup] = a2;
            }
            else
            {
                a2 = previousA2;
            }

            a2.TryUpsertVault(e.VaultEntityId, e.StateVersion);

            // SECTION 3: old AggregateEntityFungibleResourceVaultInternal

            switch (e)
            {
                case FungibleTheStruct fe:
                {
                    EntityFungibleResourceAggregatedVaultsHistory a3;

                    if (!MostRecentEntityResourceAggregatedVaultsHistory.TryGetValue(lookup, out var previousA3) || previousA3.FromStateVersion != e.StateVersion)
                    {
                        var previousBalance = (previousA3 as EntityFungibleResourceAggregatedVaultsHistory)?.Balance ?? TokenAmount.Zero;

                        a3 = new EntityFungibleResourceAggregatedVaultsHistory
                        {
                            Id = _sequences.EntityResourceAggregatedVaultsHistorySequence++,
                            FromStateVersion = fe.StateVersion,
                            EntityId = fe.EntityId,
                            ResourceEntityId = fe.ResourceEntityId,
                            Balance = previousBalance,
                        };

                        EntityResourceAggregatedVaultsHistoryToAdd.Add(a3);
                        MostRecentEntityResourceAggregatedVaultsHistory[lookup] = a3;
                    }
                    else
                    {
                        a3 = (EntityFungibleResourceAggregatedVaultsHistory)previousA3;
                    }

                    a3.Balance += fe.Delta;

                    break;
                }

                case NonFungibleTheStruct nfe:
                {
                    EntityNonFungibleResourceAggregatedVaultsHistory a4;

                    if (!MostRecentEntityResourceAggregatedVaultsHistory.TryGetValue(lookup, out var previousA4) || previousA4.FromStateVersion != e.StateVersion)
                    {
                        var previousTotalCount = (previousA4 as EntityNonFungibleResourceAggregatedVaultsHistory)?.TotalCount ?? 0;

                        a4 = new EntityNonFungibleResourceAggregatedVaultsHistory
                        {
                            Id = _sequences.EntityResourceAggregatedVaultsHistorySequence++,
                            FromStateVersion = nfe.StateVersion,
                            EntityId = nfe.EntityId,
                            ResourceEntityId = nfe.ResourceEntityId,
                            TotalCount = previousTotalCount,
                        };

                        EntityResourceAggregatedVaultsHistoryToAdd.Add(a4);
                        MostRecentEntityResourceAggregatedVaultsHistory[lookup] = a4;
                    }
                    else
                    {
                        a4 = (EntityNonFungibleResourceAggregatedVaultsHistory)previousA4;
                    }

                    a4.TotalCount += nfe.Delta;

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }

        EntityResourceAggregateHistoryToAdd.AddRange(_entityResourceAggregateHistoryCandidates.Where(x => x.ShouldBePersisted()));
        EntityResourceVaultAggregateHistoryToAdd.AddRange(_entityResourceVaultAggregateHistoryCandidates.Where(x => x.ShouldBePersisted()));
    }
}
