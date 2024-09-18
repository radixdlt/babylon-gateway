﻿// <copyright file="KeyValueStoreQueries.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Dapper;
using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries.CustomTypes;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class KeyValueStoreQueries
{
    internal readonly record struct KeyValueStoreKeysQueryConfiguration(
        GatewayModel.IdBoundaryCoursor? Cursor,
        int PageSize,
        int MaxDefinitionsLookupLimit
    );

    internal record KeyValueStoreSchemaResultRow(
        byte[] KeySchema,
        long KeyTypeIndex,
        SborTypeKind KeySborTypeKind,
        byte[] ValueSchema,
        long ValueTypeIndex,
        SborTypeKind ValueSborTypeKind);

    private readonly record struct KeyValueStoreKeysResultRow(
        long KeyValueStoreEntityId,
        long TotalEntriesExcludingDeleted,
        long TotalEntriesIncludingDeleted,
        long DefinitionId,
        byte[] Key,
        long KeyFirstSeenStateVersion,
        byte[] Value,
        bool IsLocked,
        bool IsDeleted,
        long LastUpdatedStateVersion,
        bool FilterOut,
        bool IsLastCandidate,
        IdBoundaryCursor? NextCursorInclusive
    );

    private readonly record struct KeyValueStoreDataResultRow(
        long KeyValueStoreEntityId,
        byte[] Key,
        long KeyFirstSeenStateVersion,
        byte[] Value,
        bool IsDeleted,
        bool IsLocked,
        long LastUpdatedStateVersion,
        bool FilterOut);

    internal static async Task<GatewayModel.StateKeyValueStoreKeysResponse> KeyValueStoreKeys(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        Entity keyValueStoreEntity,
        KeyValueStoreSchemaResultRow schema,
        GatewayModel.LedgerState ledgerState,
        byte networkId,
        KeyValueStoreKeysQueryConfiguration queryConfiguration,
        CancellationToken token = default)
    {
        var cd = new CommandDefinition(
            commandText: @"
WITH vars AS (
    SELECT
        @keyValueStoreEntityId AS key_value_store_entity_id,
        CAST(@useCursor AS bool) AS use_cursor,
        ROW(CAST(@cursorStateVersion AS bigint), CAST(@cursorDefinitionId  AS bigint)) AS start_cursor_inclusive,
        @atLedgerState AS at_ledger_state,
        @definitionReadLimit AS definition_read_limit,
        @pageLimit as page_limit
),
definitions_with_cursor AS (
     SELECT
         d.*,
         (d.from_state_version, d.id) AS cursor
     FROM key_value_store_entry_definition d
)
SELECT
    -- entity data
    vars.key_value_store_entity_id AS KeyValueStoreEntityId,

    -- totals
    COALESCE(entity_totals.total_entries_excluding_deleted, 0) AS TotalEntriesExcludingDeleted,
    COALESCE(entity_totals.total_entries_including_deleted, 0) AS TotalEntriesIncludingDeleted,

    -- entries
    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.definition_id END AS DefinitionId,
    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.key END AS Key,
    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.key_first_seen_state_version END AS KeyFirstSeenStateVersion,

    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.value END AS Value,
    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.is_locked END AS IsLocked,
    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.is_deleted END AS IsDeleted,
    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.last_updated_state_version END AS LastUpdatedStateVersion,

    -- cursor
    COALESCE(entries_per_entity.filter_out, TRUE) AS FilterOut,
    COALESCE(entries_per_entity.is_last_candidate, TRUE) AS IsLastCandidate,
    next_cursor_inclusive AS NextCursorInclusive
FROM vars

 -- Totals
 LEFT JOIN LATERAL (
    SELECT
        t.total_entries_excluding_deleted AS total_entries_excluding_deleted,
        t.total_entries_including_deleted AS total_entries_including_deleted
    FROM key_value_store_totals_history t
    WHERE t.entity_id = vars.key_value_store_entity_id AND t.from_state_version <= vars.at_ledger_state
    ORDER BY t.from_state_version DESC
    LIMIT 1
) entity_totals ON TRUE

-- entries_per_entity
LEFT JOIN LATERAL (
    SELECT
        definitions.id as definition_id,
        definitions.key,
        definitions.key_first_seen_state_version,
        definitions.is_last_candidate,
        definitions.cursor,
        entries.last_updated_state_version,
        entries.value,
        entries.is_locked,
        entries.is_deleted,
        CASE WHEN (ROW_NUMBER() OVER (ORDER BY definitions.cursor DESC)) = vars.page_limit OR entries.filter_out
                THEN TRUE
                ELSE FALSE
            END AS filter_out,
        CASE
            WHEN (ROW_NUMBER() OVER (ORDER BY definitions.cursor DESC)) = vars.page_limit
                THEN definitions.cursor
            WHEN (ROW_NUMBER() OVER (ORDER BY definitions.cursor DESC)) != vars.page_limit AND definitions.is_last_candidate
                THEN ROW(definitions.key_first_seen_state_version, definitions.id - 1)
            END AS next_cursor_inclusive
    FROM (
             SELECT
                 d.id,
                 d.key,
                 d.cursor,
                 d.from_state_version AS key_first_seen_state_version,
                 (ROW_NUMBER() OVER (ORDER BY d.cursor DESC)) = vars.definition_read_limit AS is_last_candidate
             FROM definitions_with_cursor d
             WHERE d.key_value_store_entity_id = vars.key_value_store_entity_id AND ((NOT vars.use_cursor) OR d.cursor <= vars.start_cursor_inclusive)
             ORDER BY d.cursor DESC
             LIMIT vars.definition_read_limit
    ) definitions
    INNER JOIN LATERAL (
        SELECT
            h.from_state_version AS last_updated_state_version,
            h.value,
            h.is_locked,
            h.is_deleted,
            h.is_deleted as filter_out
        FROM key_value_store_entry_history h
        WHERE h.key_value_store_entry_definition_id = definitions.id AND h.from_state_version <= vars.at_ledger_state
        ORDER BY h.from_state_version DESC
        LIMIT 1
        ) entries ON TRUE
    WHERE entries.filter_out = FALSE OR definitions.is_last_candidate
    ORDER BY definitions.cursor DESC
    LIMIT vars.page_limit
) entries_per_entity ON TRUE
ORDER BY entries_per_entity.cursor DESC
;",
            parameters: new
            {
                keyValueStoreEntityId = keyValueStoreEntity.Id,
                atLedgerState = ledgerState.StateVersion,
                useCursor = queryConfiguration.Cursor is not null,
                cursorStateVersion = queryConfiguration.Cursor?.StateVersionBoundary ?? 0,
                cursorDefinitionId = queryConfiguration.Cursor?.IdBoundary ?? 0,
                definitionReadLimit = queryConfiguration.MaxDefinitionsLookupLimit,
                pageLimit = queryConfiguration.PageSize + 1,
            },
            cancellationToken: token);

        var queryResult = (await dapperWrapper.QueryAsync<KeyValueStoreKeysResultRow>(dbContext.Database.GetDbConnection(), cd)).ToList();

        var totalEntries = queryResult.First().TotalEntriesExcludingDeleted;
        var elementWithCursor = queryResult.SingleOrDefault(x => x.NextCursorInclusive.HasValue);

        var nextCursor = elementWithCursor.NextCursorInclusive != null
            ? new GatewayModel.IdBoundaryCoursor(
                    elementWithCursor.NextCursorInclusive.Value.StateVersion,
                    elementWithCursor.NextCursorInclusive.Value.Id)
                .ToCursorString()
            : null;

        var items = queryResult
            .Where(x => !x.FilterOut)
            .Select(
                e =>
                {
                    var keyProgrammaticJson = ScryptoSborUtils.DataToProgrammaticJson(
                        e.Key,
                        schema.KeySchema,
                        schema.KeySborTypeKind,
                        schema.KeyTypeIndex,
                        networkId);

                    return new GatewayModel.StateKeyValueStoreKeysResponseItem(
                        key: new GatewayModel.ScryptoSborValue(e.Key.ToHex(), keyProgrammaticJson),
                        lastUpdatedAtStateVersion: e.LastUpdatedStateVersion
                    );
                })
            .ToList();

        return new GatewayModel.StateKeyValueStoreKeysResponse(
            ledgerState: ledgerState,
            keyValueStoreAddress: keyValueStoreEntity.Address,
            nextCursor: nextCursor,
            items: items,
            totalCount: totalEntries
        );
    }

    internal static async Task<GatewayModel.StateKeyValueStoreDataResponse> KeyValueStoreData(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        Entity keyValueStoreEntity,
        KeyValueStoreSchemaResultRow schema,
        IList<ValueBytes> keys,
        byte networkId,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var cd = new CommandDefinition(
            commandText: @"
WITH vars AS (
    SELECT
        @entityId as entity_id,
        UNNEST(@keys) as key,
        @atLedgerState AS at_ledger_state
)
SELECT
    -- entity id
    vars.entity_id as EntityId,

    -- data
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.key END,
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.KeyFirstSeenStateVersion END,
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.value END,
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.is_locked END AS IsLocked,
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.LastUpdatedStateVersion END,
    COALESCE(entries_with_definitions.filter_out, TRUE) AS FilterOut
FROM vars

-- entries
LEFT JOIN LATERAL (
    SELECT
        definition.key,
        definition.from_state_version AS KeyFirstSeenStateVersion,
        history.value,
        history.is_locked,
        history.is_deleted AS filter_out,
        history.from_state_version AS LastUpdatedStateVersion
    FROM key_value_store_entry_definition definition
    INNER JOIN LATERAL (
        SELECT
            value,
            is_locked,
            is_deleted,
            from_state_version
        FROM key_value_store_entry_history history
        WHERE history.key_value_store_entry_definition_id = definition.id AND from_state_version <= vars.at_ledger_state
        ORDER BY history.from_state_version DESC
        LIMIT 1
    ) history on true
    WHERE definition.key_value_store_entity_id = vars.entity_id AND definition.key = vars.key
) entries_with_definitions on TRUE",
            parameters: new
            {
                entityId = keyValueStoreEntity.Id,
                atLedgerState = ledgerState.StateVersion,
                keys = keys.Distinct().Select(k => (byte[])k).ToList(),
            },
            cancellationToken: token);

        var queryResult = await dapperWrapper.QueryAsync<KeyValueStoreDataResultRow>(dbContext.Database.GetDbConnection(), cd);

        var items = queryResult
            .Where(x => !x.FilterOut)
            .Select(
                x =>
                {
                    var keyProgrammaticJson = ScryptoSborUtils.DataToProgrammaticJson(
                        x.Key,
                        schema.KeySchema,
                        schema.KeySborTypeKind,
                        schema.KeyTypeIndex,
                        networkId);
                    var valueProgrammaticJson = ScryptoSborUtils.DataToProgrammaticJson(
                        x.Value,
                        schema.ValueSchema,
                        schema.ValueSborTypeKind,
                        schema.ValueTypeIndex,
                        networkId);

                    return new GatewayModel.StateKeyValueStoreDataResponseItem(
                        key: new GatewayModel.ScryptoSborValue(x.Key.ToHex(), keyProgrammaticJson),
                        value: new GatewayModel.ScryptoSborValue(x.Value.ToHex(), valueProgrammaticJson),
                        lastUpdatedAtStateVersion: x.LastUpdatedStateVersion,
                        isLocked: x.IsLocked);
                })
            .ToList();

        return new GatewayModel.StateKeyValueStoreDataResponse(ledgerState, keyValueStoreEntity.Address, items);
    }

    internal static async Task<KeyValueStoreSchemaResultRow> KeyValueStoreSchema(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
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

        var keyValueStoreSchema = await dapperWrapper.QueryFirstOrDefaultAsync<KeyValueStoreSchemaResultRow>(
            dbContext.Database.GetDbConnection(),
            keyValueStoreSchemaQuery,
            "GetKeyValueStoreSchema"
        );

        if (keyValueStoreSchema == null)
        {
            throw new UnreachableException($"Missing key value store schema for :{keyValueStoreId}");
        }

        return keyValueStoreSchema;
    }
}
