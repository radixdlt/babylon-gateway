using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct PackageCodeDbLookup(long PackageEntityId, ValueBytes CodeHash);

internal record PackageCodeChangePointer(long StateVersion)
{
    public CoreModel.PackageCodeOriginalCodeEntrySubstate? PackageCodeOriginalCode { get; set; }

    public CoreModel.PackageCodeVmTypeEntrySubstate? PackageCodeVmType { get; set; }

    public bool CodeVmTypeIsDeleted { get; set; }

    public bool PackageCodeIsDeleted { get; set; }
}

internal class PackageCodeProcessor
{
    private readonly ProcessorContext _context;
    private readonly byte _networkId;

    private Dictionary<PackageCodeDbLookup, PackageCodeChangePointer> _changePointers = new();
    private List<PackageCodeDbLookup> _changeOrder = new();

    private Dictionary<long, PackageCodeAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<PackageCodeDbLookup, PackageCodeHistory> _mostRecentEntries = new();

    private List<PackageCodeAggregateHistory> _aggregatesToAdd = new();
    private List<PackageCodeHistory> _entriesToAdd = new();

    public PackageCodeProcessor(ProcessorContext context, byte networkId)
    {
        _context = context;
        _networkId = networkId;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.PackageCodeOriginalCodeEntrySubstate packageCodeOriginalCode)
        {
            _changePointers
                .GetOrAdd(new PackageCodeDbLookup(referencedEntity.DatabaseId, (ValueBytes)packageCodeOriginalCode.Key.CodeHash.ConvertFromHex()), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new PackageCodeChangePointer(stateVersion);
                })
                .PackageCodeOriginalCode = packageCodeOriginalCode;
        }

        if (substateData is CoreModel.PackageCodeVmTypeEntrySubstate packageCodeVmType)
        {
            _changePointers
                .GetOrAdd(new PackageCodeDbLookup(referencedEntity.DatabaseId, (ValueBytes)packageCodeVmType.Key.CodeHash.ConvertFromHex()), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new PackageCodeChangePointer(stateVersion);
                })
                .PackageCodeVmType = packageCodeVmType;
        }
    }

    public void VisitDelete(CoreModel.SubstateId substateId, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateId.SubstateType == CoreModel.SubstateType.PackageCodeVmTypeEntry)
        {
            var keyHex = ((CoreModel.MapSubstateKey)substateId.SubstateKey).KeyHex;
            var code_hash = ScryptoSborUtils.DataToProgrammaticScryptoSborValueBytes(keyHex.ConvertFromHex(), _networkId);

            _changePointers
                .GetOrAdd(new PackageCodeDbLookup(referencedEntity.DatabaseId, (ValueBytes)code_hash.Hex.ConvertFromHex()), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new PackageCodeChangePointer(stateVersion);
                })
                .CodeVmTypeIsDeleted = true;
        }

        if (substateId.SubstateType == CoreModel.SubstateType.PackageCodeOriginalCodeEntry)
        {
            var keyHex = ((CoreModel.MapSubstateKey)substateId.SubstateKey).KeyHex;
            var code_hash = ScryptoSborUtils.DataToProgrammaticScryptoSborValueBytes(keyHex.ConvertFromHex(), _networkId);

            _changePointers
                .GetOrAdd(new PackageCodeDbLookup(referencedEntity.DatabaseId, (ValueBytes)code_hash.Hex.ConvertFromHex()), lookup =>
                {
                    _changeOrder.Add(lookup);

                    return new PackageCodeChangePointer(stateVersion);
                })
                .PackageCodeIsDeleted = true;
        }
    }

    public void ProcessChanges()
    {
        foreach (var lookup in _changeOrder)
        {
            var change = _changePointers[lookup];

            PackageCodeAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.PackageEntityId, out var previousAggregate) || previousAggregate.FromStateVersion != change.StateVersion)
            {
                aggregate = new PackageCodeAggregateHistory
                {
                    Id = _context.Sequences.PackageCodeAggregateHistorySequence++,
                    FromStateVersion = change.StateVersion,
                    PackageEntityId = lookup.PackageEntityId,
                    PackageCodeIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.PackageCodeIds.AddRange(previousAggregate.PackageCodeIds);
                }

                _aggregatesToAdd.Add(aggregate);
                _mostRecentAggregates[lookup.PackageEntityId] = aggregate;
            }
            else
            {
                aggregate = previousAggregate;
            }

            // TODO change all the code below and follow the existing pattern
            PackageCodeHistory packageCodeHistory;

            _mostRecentEntries.TryGetValue(lookup, out var existingPackageCode);

            if (existingPackageCode != null)
            {
                var previousPackageCodeId = existingPackageCode.Id;

                packageCodeHistory = existingPackageCode;
                packageCodeHistory.Id = _context.Sequences.PackageCodeHistorySequence++;
                packageCodeHistory.FromStateVersion = change.StateVersion;

                aggregate.PackageCodeIds.Remove(previousPackageCodeId);
            }
            else
            {
                packageCodeHistory = new PackageCodeHistory
                {
                    Id = _context.Sequences.PackageCodeHistorySequence++,
                    PackageEntityId = lookup.PackageEntityId,
                    FromStateVersion = change.StateVersion,
                    CodeHash = lookup.CodeHash,
                };

                _mostRecentEntries[lookup] = packageCodeHistory;
            }

            var isDeleted = change.PackageCodeIsDeleted && change.CodeVmTypeIsDeleted;
            if (isDeleted)
            {
                packageCodeHistory.IsDeleted = true;
            }
            else if (change.PackageCodeIsDeleted != change.CodeVmTypeIsDeleted)
            {
                throw new UnreachableException($"Unexpected situation where PackageCode was deleted but VmType wasn't. PackageId: {lookup.PackageEntityId}, CodeHashHex: {lookup.CodeHash.ToHex()}, StateVersion: {change.StateVersion}");
            }
            else
            {
                aggregate.PackageCodeIds.Add(packageCodeHistory.Id);

                if (change.PackageCodeVmType != null)
                {
                    packageCodeHistory.VmType = change.PackageCodeVmType.Value.VmType.ToModel();
                }

                if (change.PackageCodeOriginalCode != null)
                {
                    packageCodeHistory.Code = change.PackageCodeOriginalCode.Value.CodeHex.ConvertFromHex();
                }
            }

            _entriesToAdd.Add(packageCodeHistory);
        }
    }

    public async Task LoadMostRecent()
    {
        _mostRecentEntries = await _context.ReadHelper.MostRecentPackageCodeHistoryFor(_changeOrder, _context.Token);
        _mostRecentAggregates = await _context.ReadHelper.MostRecentPackageCodeAggregateHistoryFor(_changeOrder, _context.Token);
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await _context.WriteHelper.CopyPackageCodeHistory(_entriesToAdd, _context.Token);
        rowsInserted += await _context.WriteHelper.CopyPackageCodeAggregateHistory(_aggregatesToAdd, _context.Token);

        return rowsInserted;
    }
}
