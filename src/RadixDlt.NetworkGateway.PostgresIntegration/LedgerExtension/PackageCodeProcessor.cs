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
