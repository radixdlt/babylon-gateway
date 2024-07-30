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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
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
    private record KeyValueStoreEntryViewModel(long Id, byte[] Key, long FromStateVersion, byte[] Value, bool IsDeleted, bool IsLocked, long LastUpdatedAtStateVersion);

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

    public async Task<GatewayApiSdk.Model.StateKeyValueStoreKeysResponse> KeyValueStoreKeys(
        EntityAddress keyValueStoreAddress,
        GatewayApiSdk.Model.LedgerState ledgerState,
        GatewayModel.IdBoundaryCoursor? cursor,
        int pageSize,
        CancellationToken token = default)
    {
        var keyValueStore = await QueryHelper.GetEntity<InternalKeyValueStoreEntity>(_dbContext, keyValueStoreAddress, ledgerState, token);
        var keyValueStoreSchema = await GetKeyValueStoreSchema(keyValueStore.Id, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
SELECT d.id AS Id, d.key AS Key, d.from_state_version AS FromStateVersion, h.value AS Value, h.is_deleted AS IsDeleted, h.is_locked AS IsLocked, h.from_state_version AS LastUpdatedAtStateVersion
FROM key_value_store_entry_definition d
INNER JOIN LATERAL (
    SELECT *
    FROM key_value_store_entry_history
    WHERE key_value_store_entry_definition_id = d.id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) h ON TRUE
WHERE
    d.key_value_store_entity_id = @keyValueStoreEntityId
  AND (d.from_state_version, d.id) <= (@cursorStateVersion, @cursorId)
  AND d.from_state_version <= @stateVersion
  AND h.is_deleted = false
ORDER BY d.from_state_version DESC, d.id DESC
LIMIT @limit
;",
            parameters: new
            {
                keyValueStoreEntityId = keyValueStore.Id,
                stateVersion = ledgerState.StateVersion,
                cursorStateVersion = cursor?.StateVersionBoundary ?? long.MaxValue,
                cursorId = cursor?.IdBoundary ?? long.MaxValue,
                limit = pageSize + 1,
            },
            cancellationToken: token);

        var entriesAndOneMore = (await _dapperWrapper.QueryAsync<KeyValueStoreEntryViewModel>(_dbContext.Database.GetDbConnection(), cd)).ToList();
        var networkId = (await _networkConfigurationProvider.GetNetworkConfiguration(token)).Id;

        var items = entriesAndOneMore
            .Take(pageSize)
            .Select(e =>
            {
                var keyProgrammaticJson = ScryptoSborUtils.DataToProgrammaticJson(e.Key, keyValueStoreSchema.KeySchema, keyValueStoreSchema.KeySborTypeKind, keyValueStoreSchema.KeyTypeIndex, networkId);

                return new GatewayModel.StateKeyValueStoreKeysResponseItem(
                    key: new GatewayModel.ScryptoSborValue(e.Key.ToHex(), keyProgrammaticJson),
                    lastUpdatedAtStateVersion: e.LastUpdatedAtStateVersion
                );
            })
            .ToList();

        var nextCursor = entriesAndOneMore.Count == pageSize + 1
            ? new GatewayModel.IdBoundaryCoursor(entriesAndOneMore.Last().FromStateVersion, entriesAndOneMore.Last().Id).ToCursorString()
            : null;

        return new GatewayApiSdk.Model.StateKeyValueStoreKeysResponse(
            ledgerState: ledgerState,
            keyValueStoreAddress: keyValueStoreAddress,
            nextCursor: nextCursor,
            items: items
        );
    }

    public async Task<GatewayModel.StateKeyValueStoreDataResponse> KeyValueStoreData(
        EntityAddress keyValueStoreAddress,
        IList<ValueBytes> keys,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var keyValueStore = await QueryHelper.GetEntity<InternalKeyValueStoreEntity>(_dbContext, keyValueStoreAddress, ledgerState, token);
        var keyValueStoreSchema = await GetKeyValueStoreSchema(keyValueStore.Id, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
SELECT d.id AS Id, d.key AS Key, d.from_state_version AS FromStateVersion, h.value AS Value, h.is_deleted AS IsDeleted, h.is_locked AS IsLocked, h.from_state_version AS LastUpdatedAtStateVersion
FROM key_value_store_entry_definition d
INNER JOIN LATERAL (
    SELECT *
    FROM key_value_store_entry_history
    WHERE key_value_store_entry_definition_id = d.id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) h ON TRUE
WHERE d.key_value_store_entity_id = @keyValueStoreEntityId AND d.key = ANY(@keys) AND d.from_state_version <= @stateVersion",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                keyValueStoreEntityId = keyValueStore.Id,
                keys = keys.Distinct().Select(k => (byte[])k).ToList(),
            },
            cancellationToken: token);

        var items = new List<GatewayModel.StateKeyValueStoreDataResponseItem>();

        foreach (var e in await _dapperWrapper.QueryAsync<KeyValueStoreEntryViewModel>(_dbContext.Database.GetDbConnection(), cd))
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
                lastUpdatedAtStateVersion: e.LastUpdatedAtStateVersion,
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
INNER JOIN schema_entry_definition ksh ON ksh.schema_hash = kvssh.key_schema_hash AND ksh.entity_id = kvssh.key_schema_defining_entity_id
INNER JOIN schema_entry_definition vsh ON vsh.schema_hash = kvssh.value_schema_hash AND vsh.entity_id = kvssh.value_schema_defining_entity_id
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
}
