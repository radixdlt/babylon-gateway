using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record MetadataChangePointer(ReferencedEntity ReferencedEntity, CoreModel.MetadataModuleEntrySubstate Substate, long StateVersion)
{
    public string Key => Substate.Key.Name;

    public byte[]? Value => Substate.Value?.DataStruct.StructData.Hex.ConvertFromHex();

    public bool IsDeleted => Substate.Value == null;

    public bool IsLocked => Substate.IsLocked;
}

internal record struct MetadataLookup(long EntityId, string Key);

internal class MetadataDump
{
    public List<EntityMetadataHistory> EntityMetadataHistoryToAdd { get; } = new();

    public List<EntityMetadataAggregateHistory> EntityMetadataAggregateHistoryToAdd { get; } = new();

    private Dictionary<MetadataLookup, EntityMetadataHistory> MostRecentMetadataHistory { get; init; } = null!;

    private Dictionary<long, EntityMetadataAggregateHistory> MostRecentAggregatedMetadataHistory { get; init; } = null!;

    private readonly List<MetadataChangePointer> _metadataChangePointers;
    private readonly SequencesHolder _sequences;

    private MetadataDump(List<MetadataChangePointer> metadataChangePointers, SequencesHolder sequences)
    {
        _metadataChangePointers = metadataChangePointers;
        _sequences = sequences;
    }

    public static async Task<MetadataDump> Create(List<MetadataChangePointer> metadataChangePointers, ReadHelper readHelper, SequencesHolder sequences, CancellationToken token)
    {
        return new MetadataDump(metadataChangePointers, sequences)
        {
            MostRecentMetadataHistory = await readHelper.MostRecentEntityMetadataHistoryFor(metadataChangePointers, token),
            MostRecentAggregatedMetadataHistory = await readHelper.MostRecentEntityAggregateMetadataHistoryFor(metadataChangePointers, token),
        };
    }

    public void DoSth()
    {
        foreach (var metadataChangePointer in _metadataChangePointers)
        {
            var lookup = new MetadataLookup(metadataChangePointer.ReferencedEntity.DatabaseId, metadataChangePointer.Key);
            var metadataHistory = new EntityMetadataHistory
            {
                Id = _sequences.EntityMetadataHistorySequence++,
                FromStateVersion = metadataChangePointer.StateVersion,
                EntityId = metadataChangePointer.ReferencedEntity.DatabaseId,
                Key = metadataChangePointer.Key,
                Value = metadataChangePointer.Value,
                IsDeleted = metadataChangePointer.IsDeleted,
                IsLocked = metadataChangePointer.IsLocked,
            };

            EntityMetadataHistoryToAdd.Add(metadataHistory);

            EntityMetadataAggregateHistory aggregate;

            if (!MostRecentAggregatedMetadataHistory.TryGetValue(metadataChangePointer.ReferencedEntity.DatabaseId, out var previousAggregate) || previousAggregate.FromStateVersion != metadataChangePointer.StateVersion)
            {
                aggregate = new EntityMetadataAggregateHistory
                {
                    Id = _sequences.EntityMetadataAggregateHistorySequence++,
                    FromStateVersion = metadataChangePointer.StateVersion,
                    EntityId = metadataChangePointer.ReferencedEntity.DatabaseId,
                    MetadataIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.MetadataIds.AddRange(previousAggregate.MetadataIds);
                }

                EntityMetadataAggregateHistoryToAdd.Add(aggregate);
                MostRecentAggregatedMetadataHistory[metadataChangePointer.ReferencedEntity.DatabaseId] = aggregate;
            }
            else
            {
                aggregate = previousAggregate;
            }

            if (MostRecentMetadataHistory.TryGetValue(lookup, out var previous))
            {
                var currentPosition = aggregate.MetadataIds.IndexOf(previous.Id);

                if (currentPosition != -1)
                {
                    aggregate.MetadataIds.RemoveAt(currentPosition);
                }
            }

            if (!metadataChangePointer.IsDeleted)
            {
                aggregate.MetadataIds.Insert(0, metadataHistory.Id);
            }

            MostRecentMetadataHistory[lookup] = metadataHistory;
        }
    }
}
