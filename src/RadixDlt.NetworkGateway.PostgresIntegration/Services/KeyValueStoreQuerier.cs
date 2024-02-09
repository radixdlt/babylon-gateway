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

using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class KeyValueStoreQuerier : IKeyValueStoreQuerier
{
    private record KeyValueStoreItemsViewModel(long FromStateVersion, byte[] Key, int TotalCount);

    private record KeyValueStoreSchemaModel(byte[] KeySchema, long KeyTypeIndex, SborTypeKind KeySborTypeKind, byte[] ValueSchema, long ValueTypeIndex, SborTypeKind ValueSborTypeKind);

    private readonly IDapperWrapper _dapperWrapper;
    private readonly ReadOnlyDbContext _dbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public KeyValueStoreQuerier(IDapperWrapper dapperWrapper, ReadOnlyDbContext dbContext, INetworkConfigurationProvider networkConfigurationProvider)
    {
        _dapperWrapper = dapperWrapper;
        _dbContext = dbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<GatewayApiSdk.Model.StateKeyValueStoreKeysResponse> KeyValueStoreItems(
        EntityAddress keyValueStoreAddress,
        GatewayApiSdk.Model.LedgerState ledgerState,
        int offset,
        int limit,
        CancellationToken token = default)
    {
        var keyValueStore = await GetEntity<InternalKeyValueStoreEntity>(keyValueStoreAddress, ledgerState, token);
        var keyValueStoreSchema = await GetKeyValueStoreSchema(keyValueStore.Id, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
WITH key_value_store_items_slices AS (
    SELECT key_value_store_entry_ids[@startIndex:@endIndex] AS key_value_store_items_slice, cardinality(key_value_store_entry_ids) AS key_value_store_items_total_count
    FROM key_value_store_aggregate_history
    WHERE key_value_store_entity_id = @keyValueStoreEntityId AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
)
SELECT
    kvseh.from_state_version AS FromStateVersion,
    kvseh.key AS Key,
    kvsits.key_value_store_items_total_count AS TotalCount
FROM key_value_store_items_slices AS kvsits
INNER JOIN LATERAL UNNEST(key_value_store_items_slice) WITH ORDINALITY AS key_value_store_join(id, ordinality) ON TRUE
INNER JOIN key_value_store_entry_history kvseh ON kvseh.id = key_value_store_join.id AND kvseh.is_deleted = FALSE
ORDER BY key_value_store_join.ordinality ASC
;",
            parameters: new
            {
                keyValueStoreEntityId = keyValueStore.Id,
                stateVersion = ledgerState.StateVersion,
                startIndex = offset + 1,
                endIndex = offset + limit,
            },
            cancellationToken: token);

        var queryResult = (await _dapperWrapper.QueryAsync<KeyValueStoreItemsViewModel>(_dbContext.Database.GetDbConnection(), cd)).ToList();

        var mappedItems = new List<GatewayModel.StateKeyValueStoreKeysResponseItem>();

        int totalCount = queryResult.FirstOrDefault()?.TotalCount ?? 0;

        foreach (var item in queryResult)
        {
            var networkId = (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id;
            var keyProgrammaticJson = ScryptoSborUtils.DataToProgrammaticJson(item.Key, keyValueStoreSchema.KeySchema, keyValueStoreSchema.KeySborTypeKind,
                keyValueStoreSchema.KeyTypeIndex, networkId);

            mappedItems.Add(new GatewayModel.StateKeyValueStoreKeysResponseItem(
                    key: new GatewayModel.ScryptoSborValue(item.Key.ToHex(), keyProgrammaticJson),
                    lastUpdatedAtStateVersion: item.FromStateVersion
                )
            );
        }

        return new GatewayApiSdk.Model.StateKeyValueStoreKeysResponse(
            ledgerState: ledgerState,
            keyValueStoreAddress: keyValueStoreAddress,
            totalCount: totalCount,
            nextCursor: CursorGenerator.GenerateOffsetCursor(offset, limit, totalCount),
            items: mappedItems
        );
    }

    public async Task<GatewayModel.StateKeyValueStoreDataResponse> KeyValueStoreData(
        EntityAddress keyValueStoreAddress,
        IList<ValueBytes> keys,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var keyValueStore = await GetEntity<InternalKeyValueStoreEntity>(keyValueStoreAddress, ledgerState, token);
        var dbKeys = keys.Distinct().Select(k => (byte[])k).ToList();
        var keyValueStoreSchema = await GetKeyValueStoreSchema(keyValueStore.Id, ledgerState, token);

        var entries = await _dbContext
            .KeyValueStoreEntryHistory
            .FromSqlInterpolated(@$"
WITH variables (key) AS (
    SELECT UNNEST({dbKeys})
)
SELECT kvseh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM key_value_store_entry_history
    WHERE key_value_store_entity_id = {keyValueStore.Id} AND key = variables.key AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) kvseh ON TRUE;")
            .AnnotateMetricName("GetKeyValueStores")
            .ToListAsync(token);

        var items = new List<GatewayModel.StateKeyValueStoreDataResponseItem>();

        foreach (var e in entries)
        {
            if (e.IsDeleted)
            {
                continue;
            }

            var networkId = (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id;
            var keyProgrammaticJson = ScryptoSborUtils.DataToProgrammaticJson(e.Key, keyValueStoreSchema.KeySchema, keyValueStoreSchema.KeySborTypeKind,
                keyValueStoreSchema.KeyTypeIndex, networkId);
            var valueProgrammaticJson = ScryptoSborUtils.DataToProgrammaticJson(e.Value, keyValueStoreSchema.ValueSchema, keyValueStoreSchema.ValueSborTypeKind,
                keyValueStoreSchema.ValueTypeIndex, networkId);

            items.Add(new GatewayModel.StateKeyValueStoreDataResponseItem(
                key: new GatewayModel.ScryptoSborValue(e.Key.ToHex(), keyProgrammaticJson),
                value: new GatewayModel.ScryptoSborValue(e.Value.ToHex(), valueProgrammaticJson),
                lastUpdatedAtStateVersion: e.FromStateVersion,
                isLocked: e.IsLocked));
        }

        return new GatewayModel.StateKeyValueStoreDataResponse(ledgerState, keyValueStoreAddress, items);
    }

    private async Task<KeyValueStoreSchemaModel> GetKeyValueStoreSchema(
        long keyValueStoreId,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var keyValueStoreSchemaQuery = new CommandDefinition(
            commandText: @"
SELECT
    ksh.schema AS KeySchema,
    kvssh.key_type_index AS KeyTypeIndex,
    kvssh.key_sbor_type_kind AS KeySborTypeKind,
    vsh.schema AS ValueSchema,
    kvssh.value_type_index AS ValueTypeIndex,
    kvssh.value_sbor_type_kind AS ValueSborTypeKind
FROM key_value_store_schema_history kvssh
INNER JOIN schema_history ksh ON ksh.schema_hash = kvssh.key_schema_hash AND ksh.entity_id = kvssh.key_schema_defining_entity_id
INNER JOIN schema_history vsh ON vsh.schema_hash = kvssh.value_schema_hash AND vsh.entity_id = kvssh.value_schema_defining_entity_id
WHERE kvssh.key_value_store_entity_id = @entityId AND kvssh.from_state_version <= @stateVersion
ORDER BY kvssh.from_state_version DESC
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = keyValueStoreId,
            },
            cancellationToken: token);

        var keyValueStoreSchema = await _dapperWrapper.QueryFirstOrDefaultAsync<KeyValueStoreSchemaModel>(
            _dbContext.Database.GetDbConnection(), keyValueStoreSchemaQuery, "GetKeyValueStoreSchema"
        );

        if (keyValueStoreSchema == null)
        {
            throw new UnreachableException($"Missing key value store schema for :{keyValueStoreId}");
        }

        return keyValueStoreSchema;
    }

    private async Task<TEntity> GetEntity<TEntity>(EntityAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity
    {
        var entity = await _dbContext
            .Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .AnnotateMetricName()
            .FirstOrDefaultAsync(e => e.Address == address, token);

        if (entity == null)
        {
            throw new EntityNotFoundException(address.ToString());
        }

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidEntityException(address.ToString());
        }

        return typedEntity;
    }
}
