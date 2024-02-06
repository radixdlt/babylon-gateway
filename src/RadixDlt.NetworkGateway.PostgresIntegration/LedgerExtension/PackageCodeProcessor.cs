using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        _mostRecentEntries = await MostRecentPackageCodeHistory();
        _mostRecentAggregates = await MostRecentPackageCodeAggregateHistory();
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyPackageCodeHistory();
        rowsInserted += await CopyPackageCodeAggregateHistory();

        return rowsInserted;
    }

    private Task<Dictionary<PackageCodeDbLookup, PackageCodeHistory>> MostRecentPackageCodeHistory()
    {
        var lookupSet = _changeOrder.ToHashSet();

        if (!lookupSet.Unzip(x => x.PackageEntityId, x => (byte[])x.CodeHash, out var packageEntityIds, out var codeHashes))
        {
            return Task.FromResult(new Dictionary<PackageCodeDbLookup, PackageCodeHistory>());
        }

        return _context.ReadHelper.MostRecent<PackageCodeDbLookup, PackageCodeHistory>(
            @$"
WITH variables (package_entity_id, code_hash) AS (
    SELECT UNNEST({packageEntityIds}), UNNEST({codeHashes})
)
SELECT pbh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM package_code_history
    WHERE package_entity_id = variables.package_entity_id AND code_hash = variables.code_hash
    ORDER BY from_state_version DESC
    LIMIT 1
) pbh ON true;",
            e => new PackageCodeDbLookup(e.PackageEntityId, e.CodeHash));
    }

    private Task<Dictionary<long, PackageCodeAggregateHistory>> MostRecentPackageCodeAggregateHistory()
    {
        var packageEntityIds = _changeOrder.Select(x => x.PackageEntityId).ToHashSet().ToList();

        if (!packageEntityIds.Any())
        {
            return Task.FromResult(new Dictionary<long, PackageCodeAggregateHistory>());
        }

        return _context.ReadHelper.MostRecent<long, PackageCodeAggregateHistory>(
            $@"
WITH variables (package_entity_id) AS (
    SELECT UNNEST({packageEntityIds})
)
SELECT pbah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM package_code_aggregate_history
    WHERE package_entity_id = variables.package_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) pbah ON true;",
            e => e.PackageEntityId);
    }

    private Task<int> CopyPackageCodeHistory() => _context.WriteHelper.Copy(
        _entriesToAdd,
        "COPY package_code_history (id, from_state_version, package_entity_id, code_hash, code, vm_type, is_deleted) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.PackageEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.CodeHash, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.Code, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.VmType, "package_vm_type", token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
        });

    private Task<int> CopyPackageCodeAggregateHistory() => _context.WriteHelper.Copy(
        _aggregatesToAdd,
        "COPY package_code_aggregate_history (id, from_state_version, package_entity_id, package_code_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.PackageEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.PackageCodeIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });
}
