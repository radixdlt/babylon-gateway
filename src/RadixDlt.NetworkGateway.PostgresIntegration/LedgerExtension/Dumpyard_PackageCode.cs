using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct PackageCodeDbLookup(long PackageEntityId, ValueBytes CodeHash);

internal record PackageCodeChange(long StateVersion)
{
    public CoreModel.PackageCodeOriginalCodeEntrySubstate? PackageCodeOriginalCode { get; set; }

    public CoreModel.PackageCodeVmTypeEntrySubstate? PackageCodeVmType { get; set; }

    public bool CodeVmTypeIsDeleted { get; set; }

    public bool PackageCodeIsDeleted { get; set; }
}

internal class Dumpyard_PackageCode
{
    private readonly byte _networkId;

    private Dictionary<PackageCodeDbLookup, PackageCodeChange> _changePointers = new();
    private List<PackageCodeDbLookup> _changes = new();

    private Dictionary<PackageCodeDbLookup, PackageCodeHistory> _mostRecentEntries = new();
    private Dictionary<long, PackageCodeAggregateHistory> _mostRecentAggregates = new();

    private List<PackageCodeHistory> _entriesToAdd = new();
    private List<PackageCodeAggregateHistory> _aggregatesToAdd = new();

    public Dumpyard_PackageCode(byte networkId)
    {
        _networkId = networkId;
    }

    public void AcceptUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.PackageCodeOriginalCodeEntrySubstate packageCodeOriginalCode)
        {
            _changePointers
                .GetOrAdd(new PackageCodeDbLookup(referencedEntity.DatabaseId, (ValueBytes)packageCodeOriginalCode.Key.CodeHash.ConvertFromHex()), lookup =>
                {
                    _changes.Add(lookup);

                    return new PackageCodeChange(stateVersion);
                })
                .PackageCodeOriginalCode = packageCodeOriginalCode;
        }

        if (substateData is CoreModel.PackageCodeVmTypeEntrySubstate packageCodeVmType)
        {
            _changePointers
                .GetOrAdd(new PackageCodeDbLookup(referencedEntity.DatabaseId, (ValueBytes)packageCodeVmType.Key.CodeHash.ConvertFromHex()), lookup =>
                {
                    _changes.Add(lookup);

                    return new PackageCodeChange(stateVersion);
                })
                .PackageCodeVmType = packageCodeVmType;
        }
    }

    public void AcceptDeleted(CoreModel.SubstateId substateId, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateId.SubstateType == CoreModel.SubstateType.PackageCodeVmTypeEntry)
        {
            var keyHex = ((CoreModel.MapSubstateKey)substateId.SubstateKey).KeyHex;
            var code_hash = ScryptoSborUtils.DataToProgrammaticScryptoSborValueBytes(keyHex.ConvertFromHex(), _networkId);

            _changePointers
                .GetOrAdd(new PackageCodeDbLookup(referencedEntity.DatabaseId, (ValueBytes)code_hash.Hex.ConvertFromHex()), lookup =>
                {
                    _changes.Add(lookup);

                    return new PackageCodeChange(stateVersion);
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
                    _changes.Add(lookup);

                    return new PackageCodeChange(stateVersion);
                })
                .PackageCodeIsDeleted = true;
        }
    }

    public async Task LoadMostRecents(ReadHelper readHelper, CancellationToken token = default)
    {
        _mostRecentEntries = await readHelper.MostRecentPackageCodeHistoryFor(_changes, token);
        _mostRecentAggregates = await readHelper.MostRecentPackageCodeAggregateHistoryFor(_changes, token);
    }

    public void PrepareAdd(SequencesHolder sequences)
    {
        foreach (var lookup in _changes)
        {
            var change = _changePointers[lookup];

            var packageEntityId = lookup.PackageEntityId;
            var stateVersion = change.StateVersion;

            _mostRecentAggregates.TryGetValue(packageEntityId, out var existingPackageCodeAggregate);

            PackageCodeAggregateHistory packageCodeAggregate;

            if (existingPackageCodeAggregate == null || existingPackageCodeAggregate.FromStateVersion != change.StateVersion)
            {
                packageCodeAggregate = new PackageCodeAggregateHistory
                {
                    Id = sequences.PackageCodeAggregateHistorySequence++,
                    FromStateVersion = stateVersion,
                    PackageEntityId = packageEntityId,
                    PackageCodeIds = existingPackageCodeAggregate?.PackageCodeIds ?? new List<long>(),
                };

                _mostRecentAggregates[packageEntityId] = packageCodeAggregate;
                _aggregatesToAdd.Add(packageCodeAggregate);
            }
            else
            {
                packageCodeAggregate = existingPackageCodeAggregate;
            }

            _mostRecentEntries.TryGetValue(lookup, out var existingPackageCode);

            PackageCodeHistory packageCodeHistory;

            if (existingPackageCode != null)
            {
                var previousPackageCodeId = existingPackageCode.Id;

                packageCodeHistory = existingPackageCode;
                packageCodeHistory.Id = sequences.PackageCodeHistorySequence++;
                packageCodeHistory.FromStateVersion = change.StateVersion;
                packageCodeAggregate.PackageCodeIds.Remove(previousPackageCodeId);
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

                _mostRecentEntries[lookup] = packageCodeHistory;
            }

            var isDeleted = change.PackageCodeIsDeleted && change.CodeVmTypeIsDeleted;
            if (isDeleted)
            {
                packageCodeHistory.IsDeleted = true;
            }
            else if (change.PackageCodeIsDeleted != change.CodeVmTypeIsDeleted)
            {
                throw new UnreachableException(
                    $"Unexpected situation where PackageCode was deleted but VmType wasn't. PackageId: {lookup.PackageEntityId}, CodeHashHex: {lookup.CodeHash.ToHex()}, StateVersion: {change.StateVersion}");
            }
            else
            {
                packageCodeAggregate.PackageCodeIds.Add(packageCodeHistory.Id);

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

    public async Task<int> WriteNew(WriteHelper writeHelper, CancellationToken token)
    {
        var rowsInserted = 0;

        rowsInserted += await writeHelper.CopyPackageCodeHistory(_entriesToAdd, token);
        rowsInserted += await writeHelper.CopyPackageCodeAggregateHistory(_aggregatesToAdd, token);

        return rowsInserted;
    }
}
