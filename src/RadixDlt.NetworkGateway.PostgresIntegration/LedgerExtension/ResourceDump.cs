using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record FungibleVaultChange(ReferencedEntity ReferencedVault, ReferencedEntity ReferencedResource, long StateVersion, TokenAmount Balance);

internal record NonFungibleVaultChange(ReferencedEntity ReferencedVault, ReferencedEntity ReferencedResource, long StateVersion, string NonFungibleId, bool IsWithdrawal);

internal record NonFungibleIdChangePointer(ReferencedEntity ReferencedResource, string NonFungibleId, CoreModel.NonFungibleResourceManagerDataEntrySubstate Substate, long StateVersion)
{
    public byte[]? Value => Substate.Value?.DataStruct.StructData.GetDataBytes();

    public bool IsDeleted => Substate.Value == null;

    public bool IsLocked => Substate.IsLocked;
}

internal class ResourceDump
{
    public List<EntityFungibleVaultHistory> EntityFungibleVaultHistoryToAdd { get; } = new();

    public List<EntityNonFungibleVaultHistory> EntityNonFungibleVaultHistoryToAdd { get; } = new();

    public List<EntityResourceAggregateHistory> EntityResourceAggregateHistoryToAdd { get; } = new();

    public List<NonFungibleIdData> NonFungibleIdDataToAdd { get; } = new();

    public List<EntityResourceAggregatedVaultsHistory> EntityResourceAggregatedVaultsHistoryToAdd { get; } = new();

    public List<NonFungibleIdDataHistory> NonFungibleIdsMutableDataHistoryToAdd { get; } = new();

    public List<EntityResourceVaultAggregateHistory> EntityResourceVaultAggregateHistoryToAdd { get; } = new();

    // TODO should be list
    public Dictionary<NonFungibleStoreLookup, NonFungibleIdStoreHistory> NonFungibleIdStoreHistoryToAdd { get; } = new();

    private Dictionary<EntityResourceLookup, EntityResourceAggregatedVaultsHistory> MostRecentEntityResourceAggregatedVaultsHistory { get; init; } = null!;

    private Dictionary<EntityResourceVaultLookup, EntityResourceVaultAggregateHistory> MostRecentEntityResourceVaultAggregateHistory { get; init; } = null!;

    private Dictionary<long, EntityResourceAggregateHistory> MostRecentEntityResourceAggregateHistory { get; init; } = null!;

    private Dictionary<long, EntityNonFungibleVaultHistory> MostRecentEntityNonFungibleVaultHistory { get; init; } = null!;

    private Dictionary<long, NonFungibleIdStoreHistory> MostRecentNonFungibleIdStoreHistory { get; init; } = null!;

    private Dictionary<NonFungibleIdLookup, NonFungibleIdData> ExistingNonFungibleIdData { get; init; } = null!;

    private readonly List<EntityResourceAggregateHistory> _entityResourceAggregateHistoryCandidates = new();
    private readonly List<FungibleVaultChange> _fungibleVaultChanges;
    private readonly List<NonFungibleVaultChange> _nonFungibleVaultChanges;
    private readonly List<NonFungibleIdChangePointer> _nonFungibleIdChangePointers;
    private readonly SequencesHolder _sequences;

    private ResourceDump(List<FungibleVaultChange> fungibleVaultChanges, List<NonFungibleVaultChange> nonFungibleVaultChanges, List<NonFungibleIdChangePointer> nonFungibleIdChangePointers, SequencesHolder sequences)
    {
        _fungibleVaultChanges = fungibleVaultChanges;
        _nonFungibleVaultChanges = nonFungibleVaultChanges;
        _nonFungibleIdChangePointers = nonFungibleIdChangePointers;
        _sequences = sequences;
    }

    public static async Task<ResourceDump> Create(List<FungibleVaultChange> fungibleVaultChanges, List<NonFungibleVaultChange> nonFungibleVaultChanges, List<NonFungibleIdChangePointer> nonFungibleIdChangePointers, ReadHelper readHelper, SequencesHolder sequences, CancellationToken token)
    {
        return new ResourceDump(fungibleVaultChanges, nonFungibleVaultChanges, nonFungibleIdChangePointers, sequences)
        {
            MostRecentEntityResourceAggregateHistory = await readHelper.MostRecentEntityResourceAggregateHistoryFor(fungibleVaultChanges, nonFungibleVaultChanges, token),
            MostRecentEntityResourceAggregatedVaultsHistory = await readHelper.MostRecentEntityResourceAggregatedVaultsHistoryFor(fungibleVaultChanges, nonFungibleVaultChanges, token),
            MostRecentEntityResourceVaultAggregateHistory = await readHelper.MostRecentEntityResourceVaultAggregateHistoryFor(fungibleVaultChanges, nonFungibleVaultChanges, token),
            MostRecentEntityNonFungibleVaultHistory = await readHelper.MostRecentEntityNonFungibleVaultHistory(nonFungibleVaultChanges, token),
            MostRecentNonFungibleIdStoreHistory = await readHelper.MostRecentNonFungibleIdStoreHistoryFor(nonFungibleIdChangePointers, token),
            ExistingNonFungibleIdData = await readHelper.ExistingNonFungibleIdDataFor(nonFungibleIdChangePointers, nonFungibleVaultChanges, token),
        };
    }

    public void DoSth()
    {
        EntityFungibleVaultHistoryToAdd.AddRange(_fungibleVaultChanges
            .Select(e =>
            {
                AggregateEntityResource(e.ReferencedVault, e.ReferencedResource, e.StateVersion, true, e.Balance, null);

                return new EntityFungibleVaultHistory
                {
                    Id = _sequences.EntityVaultHistorySequence++,
                    FromStateVersion = e.StateVersion,
                    OwnerEntityId = e.ReferencedVault.DatabaseOwnerAncestorId,
                    GlobalEntityId = e.ReferencedVault.DatabaseGlobalAncestorId,
                    ResourceEntityId = e.ReferencedResource.DatabaseId,
                    VaultEntityId = e.ReferencedVault.DatabaseId,
                    IsRoyaltyVault = e.ReferencedVault.GetDatabaseEntity<InternalFungibleVaultEntity>().IsRoyaltyVault,
                    Balance = e.Balance,
                };
            }));

        EntityNonFungibleVaultHistoryToAdd.AddRange(_nonFungibleVaultChanges
            .GroupBy(x => new { x.StateVersion, x.ReferencedVault, x.ReferencedResource })
            .Select(e =>
            {
                var vaultExists = MostRecentEntityNonFungibleVaultHistory.TryGetValue(e.Key.ReferencedVault.DatabaseId, out var existingVaultHistory);

                var nfids = vaultExists ? existingVaultHistory!.NonFungibleIds : new List<long>();
                var addedItems = e.Where(x => !x.IsWithdrawal)
                    .Select(x => ExistingNonFungibleIdData[new NonFungibleIdLookup(e.Key.ReferencedResource.DatabaseId, x.NonFungibleId)].Id)
                    .ToList();

                var deletedItems = e.Where(x => x.IsWithdrawal)
                    .Select(x => ExistingNonFungibleIdData[new NonFungibleIdLookup(e.Key.ReferencedResource.DatabaseId, x.NonFungibleId)].Id)
                    .ToList();

                nfids.AddRange(addedItems);
                nfids.RemoveAll(x => deletedItems.Contains(x));

                AggregateEntityResource(e.Key.ReferencedVault, e.Key.ReferencedResource, e.Key.StateVersion, false, null, nfids.Count);

                return new EntityNonFungibleVaultHistory
                {
                    Id = _sequences.EntityVaultHistorySequence++,
                    FromStateVersion = e.Key.StateVersion,
                    OwnerEntityId = e.Key.ReferencedVault.DatabaseOwnerAncestorId,
                    GlobalEntityId = e.Key.ReferencedVault.DatabaseGlobalAncestorId,
                    ResourceEntityId = e.Key.ReferencedResource.DatabaseId,
                    VaultEntityId = e.Key.ReferencedVault.DatabaseId,
                    NonFungibleIds = nfids.ToList(),
                };
            }));

        foreach (var e in _nonFungibleIdChangePointers)
        {
            var nonFungibleIdData = ExistingNonFungibleIdData.GetOrAdd(new NonFungibleIdLookup(e.ReferencedResource.DatabaseId, e.NonFungibleId), _ =>
            {
                var ret = new NonFungibleIdData
                {
                    Id = _sequences.NonFungibleIdDataSequence++,
                    FromStateVersion = e.StateVersion,
                    NonFungibleResourceEntityId = e.ReferencedResource.DatabaseId,
                    NonFungibleId = e.NonFungibleId,
                };

                NonFungibleIdDataToAdd.Add(ret);

                return ret;
            });

            var nonFungibleIdStore = NonFungibleIdStoreHistoryToAdd.GetOrAdd(new NonFungibleStoreLookup(e.ReferencedResource.DatabaseId, e.StateVersion), _ =>
            {
                IEnumerable<long> previousNonFungibleIdDataIds = MostRecentNonFungibleIdStoreHistory.TryGetValue(e.ReferencedResource.DatabaseId, out var value)
                    ? value.NonFungibleIdDataIds
                    : Array.Empty<long>();

                var ret = new NonFungibleIdStoreHistory
                {
                    Id = _sequences.NonFungibleIdStoreHistorySequence++,
                    FromStateVersion = e.StateVersion,
                    NonFungibleResourceEntityId = e.ReferencedResource.DatabaseId,
                    NonFungibleIdDataIds = new List<long>(previousNonFungibleIdDataIds),
                };

                MostRecentNonFungibleIdStoreHistory[e.ReferencedResource.DatabaseId] = ret;

                return ret;
            });

            NonFungibleIdsMutableDataHistoryToAdd.Add(new NonFungibleIdDataHistory
            {
                Id = _sequences.NonFungibleIdDataHistorySequence++,
                FromStateVersion = e.StateVersion,
                NonFungibleIdDataId = nonFungibleIdData.Id,
                Data = e.Value,
                IsDeleted = e.IsDeleted,
                IsLocked = e.IsLocked,
            });

            if (!nonFungibleIdStore.NonFungibleIdDataIds.Contains(nonFungibleIdData.Id))
            {
                nonFungibleIdStore.NonFungibleIdDataIds.Add(nonFungibleIdData.Id);
            }
        }

        EntityResourceAggregateHistoryToAdd.AddRange(_entityResourceAggregateHistoryCandidates.Where(x => x.ShouldBePersisted()));
    }

    private void AggregateEntityResource(
                ReferencedEntity referencedVault,
                ReferencedEntity referencedResource,
                long stateVersion,
                bool fungibleResource,
                TokenAmount? tmpFungibleBalance,
                long? tmpNonFungibleTotalCount)
    {
        if (referencedVault.GetDatabaseEntity<VaultEntity>() is InternalFungibleVaultEntity { IsRoyaltyVault: true })
        {
            return;
        }

        if (fungibleResource)
        {
            var tmpBalance = tmpFungibleBalance ?? throw new InvalidOperationException("impossible x1"); // TODO improve

            AggregateEntityFungibleResourceVaultInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId, tmpBalance, referencedVault.DatabaseId);
            AggregateEntityFungibleResourceVaultInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId, tmpBalance, referencedVault.DatabaseId);
        }
        else
        {
            var tmpTotalCount = tmpNonFungibleTotalCount ?? throw new InvalidOperationException("impossible x2"); // TODO improve

            AggregateEntityNonFungibleResourceVaultInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId, tmpTotalCount, referencedVault.DatabaseId);
            AggregateEntityNonFungibleResourceVaultInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId, tmpTotalCount, referencedVault.DatabaseId);
        }

        AggregateEntityResourceInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId);
        AggregateEntityResourceInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId);
        AggregateEntityResourceVaultInternal(referencedVault.DatabaseOwnerAncestorId, referencedResource.DatabaseId, referencedVault.DatabaseId);
        AggregateEntityResourceVaultInternal(referencedVault.DatabaseGlobalAncestorId, referencedResource.DatabaseId, referencedVault.DatabaseId);

        // TODO rename tmpBalance->delta and drop tmpResourceVaultEntityId once TX events become available
        void AggregateEntityFungibleResourceVaultInternal(long entityId, long resourceEntityId, TokenAmount tmpBalance, long tmpResourceVaultEntityId)
        {
            var lookup = new EntityResourceLookup(entityId, resourceEntityId);

            if (!MostRecentEntityResourceAggregatedVaultsHistory.TryGetValue(lookup, out var aggregate) || aggregate.FromStateVersion != stateVersion)
            {
                var previousTmpTmp = aggregate?.TmpTmpRemoveMeOnceTxEventsBecomeAvailable ?? string.Empty;

                aggregate = new EntityFungibleResourceAggregatedVaultsHistory
                {
                    Id = _sequences.EntityResourceAggregatedVaultsHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = entityId,
                    ResourceEntityId = resourceEntityId,
                    TmpTmpRemoveMeOnceTxEventsBecomeAvailable = previousTmpTmp,
                };

                EntityResourceAggregatedVaultsHistoryToAdd.Add(aggregate);
                MostRecentEntityResourceAggregatedVaultsHistory[lookup] = aggregate;
            }

            // TODO replace with simple aggregate.Balance += delta once TX events become available
            var tmpSum = TokenAmount.Zero;
            var tmpExists = false;
            var tmpColl = aggregate.TmpTmpRemoveMeOnceTxEventsBecomeAvailable
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(e =>
                {
                    var parts = e.Split('=');
                    var vaultId = long.Parse(parts[0]);
                    var balance = TokenAmount.FromDecimalString(parts[1]);

                    if (vaultId == tmpResourceVaultEntityId)
                    {
                        tmpExists = true;
                        balance = tmpBalance;
                    }

                    tmpSum += balance;

                    return $"{vaultId}={balance}";
                })
                .ToList();

            if (tmpExists == false)
            {
                tmpColl.Add($"{tmpResourceVaultEntityId}={tmpBalance}");
                tmpSum += tmpBalance;
            }

            aggregate.TmpTmpRemoveMeOnceTxEventsBecomeAvailable = string.Join(";", tmpColl);

            ((EntityFungibleResourceAggregatedVaultsHistory)aggregate).Balance = tmpSum;
        }

        // TODO rename tmpTotalCount->delta and drop tmpResourceVaultEntityId once TX events become available
        void AggregateEntityNonFungibleResourceVaultInternal(long entityId, long resourceEntityId, long tmpTotalCount, long tmpResourceVaultEntityId)
        {
            var lookup = new EntityResourceLookup(entityId, resourceEntityId);

            if (!MostRecentEntityResourceAggregatedVaultsHistory.TryGetValue(lookup, out var aggregate) || aggregate.FromStateVersion != stateVersion)
            {
                var previousTmpTmp = aggregate?.TmpTmpRemoveMeOnceTxEventsBecomeAvailable ?? string.Empty;

                aggregate = new EntityNonFungibleResourceAggregatedVaultsHistory
                {
                    Id = _sequences.EntityResourceAggregatedVaultsHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = entityId,
                    ResourceEntityId = resourceEntityId,
                    TmpTmpRemoveMeOnceTxEventsBecomeAvailable = previousTmpTmp,
                };

                EntityResourceAggregatedVaultsHistoryToAdd.Add(aggregate);
                MostRecentEntityResourceAggregatedVaultsHistory[lookup] = aggregate;
            }

            // TODO replace with simple aggregate.TotalCount += delta once TX events become available
            var tmpSum = 0L;
            var tmpExists = false;
            var tmpColl = aggregate.TmpTmpRemoveMeOnceTxEventsBecomeAvailable
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(e =>
                {
                    var parts = e.Split('=');
                    var vaultId = long.Parse(parts[0]);
                    var totalCount = long.Parse(parts[1]);

                    if (vaultId == tmpResourceVaultEntityId)
                    {
                        tmpExists = true;
                        totalCount = tmpTotalCount;
                    }

                    tmpSum += totalCount;

                    return $"{vaultId}={totalCount}";
                })
                .ToList();

            if (tmpExists == false)
            {
                tmpColl.Add($"{tmpResourceVaultEntityId}={tmpTotalCount}");
                tmpSum += tmpTotalCount;
            }

            aggregate.TmpTmpRemoveMeOnceTxEventsBecomeAvailable = string.Join(";", tmpColl);

            ((EntityNonFungibleResourceAggregatedVaultsHistory)aggregate).TotalCount = tmpSum;
        }

        void AggregateEntityResourceInternal(long entityId, long resourceEntityId)
        {
            // we only want to create new aggregated resource history entry if
            // - given resource is seen for the very first time,
            // - given resource is already stored but has been updated and this update caused change of order (this is evaluated right before db persistence)

            if (MostRecentEntityResourceAggregateHistory.TryGetValue(entityId, out var aggregate))
            {
                var existingResourceCollection = fungibleResource
                    ? aggregate.FungibleResourceEntityIds
                    : aggregate.NonFungibleResourceEntityIds;

                // we're already the most recent one, there's nothing more to do
                if (existingResourceCollection.IndexOf(resourceEntityId) == 0)
                {
                    return;
                }
            }

            if (aggregate == null || aggregate.FromStateVersion != stateVersion)
            {
                aggregate = aggregate == null
                    ? EntityResourceAggregateHistory.Create(_sequences.EntityResourceAggregateHistorySequence++, entityId, stateVersion)
                    : EntityResourceAggregateHistory.CopyOf(_sequences.EntityResourceAggregateHistorySequence++, aggregate, stateVersion);

                _entityResourceAggregateHistoryCandidates.Add(aggregate);
                MostRecentEntityResourceAggregateHistory[entityId] = aggregate;
            }

            if (fungibleResource)
            {
                aggregate.TryUpsertFungible(resourceEntityId, stateVersion);
            }
            else
            {
                aggregate.TryUpsertNonFungible(resourceEntityId, stateVersion);
            }
        }

        void AggregateEntityResourceVaultInternal(long entityId, long resourceEntityId, long resourceVaultEntityId)
        {
            var lookup = new EntityResourceVaultLookup(entityId, resourceEntityId);

            if (MostRecentEntityResourceVaultAggregateHistory.TryGetValue(lookup, out var existingAggregate))
            {
                if (existingAggregate.VaultEntityIds.Contains(resourceVaultEntityId))
                {
                    return;
                }
            }

            var aggregate = existingAggregate;

            if (aggregate == null || aggregate.FromStateVersion != stateVersion)
            {
                aggregate = new EntityResourceVaultAggregateHistory
                {
                    Id = _sequences.EntityResourceVaultAggregateHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = entityId,
                    ResourceEntityId = resourceEntityId,
                    VaultEntityIds = new List<long>(existingAggregate?.VaultEntityIds.ToArray() ?? Array.Empty<long>()),
                };

                EntityResourceVaultAggregateHistoryToAdd.Add(aggregate);
                MostRecentEntityResourceVaultAggregateHistory[lookup] = aggregate;
            }

            aggregate.VaultEntityIds.Add(resourceVaultEntityId);
        }
    }
}
