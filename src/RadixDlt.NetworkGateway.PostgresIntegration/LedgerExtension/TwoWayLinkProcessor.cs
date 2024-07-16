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
using RadixDlt.NetworkGateway.Abstractions.StandardMetadata;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct UnverifiedTwoWayLinkEntryDbLookup(long EntityId, StandardMetadataKey Type);

internal class TwoWayLinkProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private Dictionary<long, UnverifiedTwoWayLinkAggregateHistory> _mostRecentAggregates = new();
    private Dictionary<UnverifiedTwoWayLinkEntryDbLookup, UnverifiedTwoWayLinkEntryHistory> _mostRecentEntries = new();

    private List<UnverifiedTwoWayLinkAggregateHistory> _aggregatesToAdd = new();
    private List<UnverifiedTwoWayLinkEntryHistory> _entriesToAdd = new();

    public TwoWayLinkProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is not CoreModel.MetadataModuleEntrySubstate metadataEntry)
        {
            return;
        }

        bool TryParseValue<T>([NotNullWhen(true)] out T? value)
            where T : GatewayModel.MetadataTypedValue
        {
            var parsed = metadataEntry.Value == null
                ? null
                : ScryptoSborUtils.DecodeToGatewayMetadataItemValue(metadataEntry.Value.DataStruct.StructData.GetDataBytes(), _context.NetworkConfiguration.Id);

            if (parsed is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        var key = metadataEntry.Key.Name;

        if (referencedEntity.Address.IsAccount)
        {
            if (key == StandardMetadataConstants.DappAccountType && TryParseValue<GatewayModel.MetadataStringValue>(out var accountType))
            {
                _entriesToAdd.Add(new DappAccountTypeUnverifiedTwoWayLinkEntryHistory
                {
                    Id = _context.Sequences.UnverifiedTwoWayLinkEntryHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = referencedEntity.DatabaseId,
                    IsDeleted = metadataEntry.Value == null,
                    IsLocked = substateData.IsLocked,
                    Value = accountType.Value,
                });
            }
            else if (key == StandardMetadataConstants.DappClaimedWebsites && TryParseValue<GatewayModel.MetadataOriginArrayValue>(out var claimedWebsites))
            {
                _entriesToAdd.Add(new DappClaimedWebsitesUnverifiedTwoWayLinkEntryHistory
                {
                    Id = _context.Sequences.UnverifiedTwoWayLinkEntryHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = referencedEntity.DatabaseId,
                    IsDeleted = metadataEntry.Value == null,
                    IsLocked = substateData.IsLocked,
                    ClaimedWebsites = claimedWebsites.Values.ToArray(),
                });
            }
            else if (key == StandardMetadataConstants.DappClaimedEntities && TryParseValue<GatewayModel.MetadataGlobalAddressArrayValue>(out var claimedEntities))
            {
                _entriesToAdd.Add(new DappClaimedEntitiesUnverifiedTwoWayLinkEntryHistory
                {
                    Id = _context.Sequences.UnverifiedTwoWayLinkEntryHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = referencedEntity.DatabaseId,
                    IsDeleted = metadataEntry.Value == null,
                    IsLocked = substateData.IsLocked,
                    ClaimedEntityIds = claimedEntities.Values.Select(address => _referencedEntities.Get((EntityAddress)address).DatabaseId).ToArray(),
                });
            }
            else if (key == StandardMetadataConstants.DappDefinitions && TryParseValue<GatewayModel.MetadataGlobalAddressArrayValue>(out var dappDefinitions))
            {
                _entriesToAdd.Add(new DappDefinitionsUnverifiedTwoWayLinkEntryHistory
                {
                    Id = _context.Sequences.UnverifiedTwoWayLinkEntryHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = referencedEntity.DatabaseId,
                    IsDeleted = metadataEntry.Value == null,
                    IsLocked = substateData.IsLocked,
                    DappDefinitionEntityIds = dappDefinitions.Values.Select(address => _referencedEntities.Get((EntityAddress)address).DatabaseId).ToArray(),
                });
            }
        }
        else if (referencedEntity.Address.IsResource)
        {
            if (key == StandardMetadataConstants.DappDefinitions && TryParseValue<GatewayModel.MetadataGlobalAddressArrayValue>(out var dappDefinitions))
            {
                _entriesToAdd.Add(new DappDefinitionsUnverifiedTwoWayLinkEntryHistory
                {
                    Id = _context.Sequences.UnverifiedTwoWayLinkEntryHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = referencedEntity.DatabaseId,
                    IsDeleted = metadataEntry.Value == null,
                    IsLocked = substateData.IsLocked,
                    DappDefinitionEntityIds = dappDefinitions.Values.Select(address => _referencedEntities.Get((EntityAddress)address).DatabaseId).ToArray(),
                });
            }
        }
        else if (referencedEntity.Address.IsGlobal)
        {
            if (key == StandardMetadataConstants.DappDefinition && TryParseValue<GatewayModel.MetadataGlobalAddressValue>(out var dappDefinition))
            {
                _entriesToAdd.Add(new DappDefinitionUnverifiedTwoWayLinkEntryHistory
                {
                    Id = _context.Sequences.UnverifiedTwoWayLinkEntryHistorySequence++,
                    FromStateVersion = stateVersion,
                    EntityId = referencedEntity.DatabaseId,
                    IsDeleted = metadataEntry.Value == null,
                    IsLocked = substateData.IsLocked,
                    DappDefinitionEntityId = _referencedEntities.Get((EntityAddress)dappDefinition.Value).DatabaseId,
                });
            }
        }
    }

    public async Task LoadDependencies()
    {
        _mostRecentEntries.AddRange(await MostRecentEntryHistory());
        _mostRecentAggregates.AddRange(await MostRecentAggregateHistory());
    }

    public void ProcessChanges()
    {
        foreach (var entry in _entriesToAdd)
        {
            var lookup = new UnverifiedTwoWayLinkEntryDbLookup(entry.EntityId, _context.WriteHelper.GetDiscriminator<StandardMetadataKey>(entry.GetType()));

            UnverifiedTwoWayLinkAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.EntityId, out var previousAggregate) || previousAggregate.FromStateVersion != entry.FromStateVersion)
            {
                aggregate = new UnverifiedTwoWayLinkAggregateHistory
                {
                    Id = _context.Sequences.UnverifiedTwoWayLinkAggregateHistorySequence++,
                    FromStateVersion = entry.FromStateVersion,
                    EntityId = lookup.EntityId,
                    EntryIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.EntryIds.AddRange(previousAggregate.EntryIds);
                }

                _aggregatesToAdd.Add(aggregate);
                _mostRecentAggregates[lookup.EntityId] = aggregate;
            }
            else
            {
                aggregate = previousAggregate;
            }

            if (_mostRecentEntries.TryGetValue(lookup, out var previousEntry))
            {
                var currentPosition = aggregate.EntryIds.IndexOf(previousEntry.Id);

                if (currentPosition != -1)
                {
                    aggregate.EntryIds.RemoveAt(currentPosition);
                }
            }

            if (!entry.IsDeleted)
            {
                aggregate.EntryIds.Insert(0, entry.Id);
            }

            _mostRecentEntries[lookup] = entry;
        }
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyUnverifiedTwoWayLinkEntryHistory();
        rowsInserted += await CopyUnverifiedTwoWayLinkAggregateHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<UnverifiedTwoWayLinkEntryDbLookup, UnverifiedTwoWayLinkEntryHistory>> MostRecentEntryHistory()
    {
        var lookupSet = _entriesToAdd
            .Select(e => new UnverifiedTwoWayLinkEntryDbLookup(e.EntityId, _context.WriteHelper.GetDiscriminator<StandardMetadataKey>(e.GetType())))
            .ToHashSet();

        if (!lookupSet.Unzip(x => x.EntityId, x => x.Type, out var entityIds, out var discriminators))
        {
            return ImmutableDictionary<UnverifiedTwoWayLinkEntryDbLookup, UnverifiedTwoWayLinkEntryHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<UnverifiedTwoWayLinkEntryDbLookup, UnverifiedTwoWayLinkEntryHistory>(
            @$"
WITH variables (entity_id, discriminator) AS (
    SELECT UNNEST({entityIds}), UNNEST({discriminators})
)
SELECT eh.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM unverified_two_way_link_entry_history
    WHERE entity_id = var.entity_id AND discriminator = var.discriminator
    ORDER BY from_state_version DESC
    LIMIT 1
) eh ON true;",
            e => new UnverifiedTwoWayLinkEntryDbLookup(e.EntityId, _context.WriteHelper.GetDiscriminator<StandardMetadataKey>(e.GetType())));
    }

    private async Task<IDictionary<long, UnverifiedTwoWayLinkAggregateHistory>> MostRecentAggregateHistory()
    {
        var entityIds = _entriesToAdd.Select(x => x.EntityId).ToHashSet().ToList();

        if (!entityIds.Any())
        {
            return ImmutableDictionary<long, UnverifiedTwoWayLinkAggregateHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<long, UnverifiedTwoWayLinkAggregateHistory>(
            @$"
WITH variables (entity_id) AS (
    SELECT UNNEST({entityIds})
)
SELECT ah.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM unverified_two_way_link_aggregate_history
    WHERE entity_id = var.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) ah ON true;",
            e => e.EntityId);
    }

    private Task<int> CopyUnverifiedTwoWayLinkEntryHistory() => _context.WriteHelper.Copy(
        _entriesToAdd,
        "COPY unverified_two_way_link_entry_history (id, from_state_version, entity_id, is_deleted, is_locked, discriminator, value, entity_ids, claimed_websites) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            var discriminator = _context.WriteHelper.GetDiscriminator<StandardMetadataKey>(e.GetType());

            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(discriminator, "two_way_link_type", token);

            switch (e)
            {
                case DappAccountTypeUnverifiedTwoWayLinkEntryHistory at:
                    await writer.WriteAsync(at.Value, NpgsqlDbType.Text, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case DappClaimedEntitiesUnverifiedTwoWayLinkEntryHistory ce:
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(ce.ClaimedEntityIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                    await writer.WriteNullAsync(token);
                    break;
                case DappClaimedWebsitesUnverifiedTwoWayLinkEntryHistory cw:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(cw.ClaimedWebsites, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
                    break;
                case DappDefinitionsUnverifiedTwoWayLinkEntryHistory dds:
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(dds.DappDefinitionEntityIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                    await writer.WriteNullAsync(token);
                    break;
                case DappDefinitionUnverifiedTwoWayLinkEntryHistory dd:
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(new[] { dd.DappDefinitionEntityId }, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
                    await writer.WriteNullAsync(token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        });

    private Task<int> CopyUnverifiedTwoWayLinkAggregateHistory() => _context.WriteHelper.Copy(
        _aggregatesToAdd,
        "COPY unverified_two_way_link_aggregate_history (id, from_state_version, entity_id, entry_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntryIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });
}
