using Dapper;
using RadixDlt.NetworkGateway.Abstractions;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services.Queries;

// ReSharper disable NotAccessedPositionalProperty.Global
internal static class NonFungibleIdsInResourcePageQuery
{
    internal readonly record struct QueryConfiguration(
        GatewayModel.IdBoundaryCoursor? ExclusiveCursor,
        bool IsAscending,
        bool IncludeDeleted,
        bool IncludeValue,
        int MaxPageSize,
        int MaxDefinitionsToRead
    );

    // Raw result form the query - easiest to keep this relatively
    // standardised across different PageQuerys, but you might need
    // to change the type of Key, Value and the Totals
    private readonly record struct QueryResultRow(
        long EntityId,
        string EntityAddress,
        long TotalEntriesExcludingDeleted,
        long TotalEntriesIncludingDeleted,
        bool FilterOut,
        long? DefinitionId,
        string? Key,
        long? KeyFirstSeenStateVersion,
        byte[]? Value,
        bool? IsLocked,
        bool? IsDeleted,
        long? LastUpdatedStateVersion,
        long? NextExclusiveCursorStateVersion,
        long? NextExclusiveCursorDefinitionId
    );

    // Query-specific results model - mapping QueryResultRow back out
    internal readonly record struct PerEntityResult(
        long EntityId,
        EntityAddress NonFungibleEntityAddress,
        GatewayModel.IdBoundaryCoursor? NextCursor,
        long TotalEntriesGivenPagingParameters,
        long TotalMinted,
        long TotalSupply,
        List<PageItem> PageItems
    )
    {
        internal GatewayModel.NonFungibleIdsCollection ToNonFungibleIdsCollection()
        {
            return new GatewayModel.NonFungibleIdsCollection(
                totalCount: TotalEntriesGivenPagingParameters,
                nextCursor: NextCursor?.ToCursorString(),
                items: PageItems.Select(i => i.NonFungibleId).ToList());
        }
    }

    internal readonly record struct PageItem(
        long DefinitionId,
        string NonFungibleId,
        long KeyFirstSeenStateVersion,
        byte[]? Data,
        bool IsLocked,
        bool IsDeleted,
        long DataLastUpdatedStateVersion
    );

    internal static async Task<Dictionary<EntityAddress, PerEntityResult>> ReadPages(
        DbConnection dbConnection,
        IDapperWrapper dapperWrapper,
        GatewayModel.LedgerState ledgerState,
        List<EntityAddress> nonFungibleResources,
        QueryConfiguration queryConfiguration,
        CancellationToken token = default)
    {
        // See `query_conventions.md` for details about how this query structure works
        var queryParameters = new
        {
            rootEntityAddresses = nonFungibleResources,
            useCursor = queryConfiguration.ExclusiveCursor is not null,
            stateVersion = ledgerState.StateVersion,
            exclusiveCursorStateVersion = queryConfiguration.ExclusiveCursor?.StateVersionBoundary ?? 0,
            exclusiveCursorDefinitionId = queryConfiguration.ExclusiveCursor?.IdBoundary ?? 0,
            pageLimit = queryConfiguration.MaxPageSize,
            definitionReadLimit = queryConfiguration.MaxDefinitionsToRead,
        };

        var commandDefinition = new CommandDefinition(
            commandText: $@"
WITH vars AS (
    SELECT
        CAST(@rootEntityAddresses AS text[]) AS entity_addresses,
        -- If use_cursor is false, the cursor is ignored, so just set it to (0, 0)
        CAST(@useCursor AS bool) AS use_cursor,
        -- This cursor is (from_state_version, definition_id) exclusive
        ROW(CAST(@exclusiveCursorStateVersion AS bigint), CAST(@exclusiveCursorDefinitionId AS bigint)) AS start_cursor_exclusive,
        CAST(@stateVersion AS bigint) AS current_state_version
),
definitions_with_cursor AS (
    SELECT
        d.*,
        (d.from_state_version, d.id) AS cursor
    FROM non_fungible_id_definition d
),
entries_per_entity AS (
    SELECT
        entities.id AS EntityId,
        entities.address AS EntityAddress,
        entity_totals.total_entries_excluding_deleted AS TotalEntriesExcludingDeleted,
        entity_totals.total_entries_including_deleted AS TotalEntriesIncludingDeleted,
        COALESCE(filter_out, TRUE) AS FilterOut,
        CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE definition_id END AS DefinitionId,
        CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE key END AS Key,
        CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE key_first_seen_state_version END AS KeyFirstSeenStateVersion,
        CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE value END AS Value,
        CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE is_locked END AS IsLocked,
        CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE is_deleted END AS IsDeleted,
        CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE last_updated_state_version END AS LastUpdatedStateVersion,
        next_cursor_exclusive.f1 AS NextExclusiveCursorStateVersion,
        next_cursor_exclusive.f2 AS NextExclusiveCursorDefinitionId
    FROM vars
    INNER JOIN LATERAL (
        SELECT
            UNNEST(vars.entity_addresses) AS address
    ) addresses ON TRUE
    INNER JOIN entities
        ON e.address = addresses.address
        AND e.from_state_version <= vars.current_state_version
    -- In general, this can be replaced by some XXX_totals_history table, or removed if we don't have any relevant totals table
    INNER JOIN LATERAL (
        SELECT
            t.total_supply AS total_entries_excluding_deleted,
            t.total_minted AS total_entries_including_deleted
        FROM resource_entity_supply_history t
        WHERE
            t.resource_entity_id = entities.id
            AND t.from_state_version <= vars.current_state_version
        ORDER BY
            t.from_state_version DESC
        LIMIT 1
    ) entity_totals ON TRUE
    LEFT JOIN LATERAL ( -- LEFT JOIN so we always return a row where we can join on the totals
        SELECT
            definitions.id as definition_id,
            definitions.non_fungible_id,
            definitions.key_first_seen_state_version,
            definitions.cursor,
            entries.*,
            CASE WHEN
                -- Add cursor to last row returned only
                -- > EITHER because we have filled a page (row num = limit)
                -- > OR because we have reached the last sub-query item (definitions.is_last_subquery_item)
                --
                -- NOTE: The last row should be ignored if filter_out is TRUE - in which case it's just being returned for the cursor
                (ROW_NUMBER() OVER (ORDER BY definitions.cursor {(queryConfiguration.IsAscending ? "ASC" : "DESC")})) = @pageLimit
                OR definitions.is_last_subquery_item
            THEN definitions.cursor ELSE NULL END AS next_cursor_exclusive
         FROM (
            SELECT
                d.id AS id,
                d.non_fungible_id AS key, -- The key
                d.from_state_version AS key_first_seen_state_version,
                d.cursor,
                (ROW_NUMBER() OVER (ORDER BY d.cursor {(queryConfiguration.IsAscending ? "ASC" : "DESC")})) = @definitionReadLimit AS is_last_subquery_item
            FROM definitions_with_cursor d
            WHERE
                d.non_fungible_resource_entity_id = entities.id
                AND (
                    (NOT vars.use_cursor) OR
                    d.cursor {(queryConfiguration.IsAscending ? ">" : "<")} vars.start_cursor_exclusive
                )
            ORDER BY
                d.cursor {(queryConfiguration.IsAscending ? "ASC" : "DESC")}
            LIMIT @definitionReadLimit
        ) definitions
        INNER JOIN LATERAL (
            SELECT
                h.from_state_version AS last_updated_state_version,
                {(queryConfiguration.IncludeValue ? "NULL" : "h.data")} AS value,
                h.is_locked,
                h.is_deleted,
                {(queryConfiguration.IncludeDeleted ? "TRUE" : "h.is_deleted")} AS filter_out
            FROM non_fungible_id_data_history h
            WHERE
                h.non_fungible_id_definition_id = definitions.id
                AND h.from_state_version <= vars.current_state_version
            ORDER BY
                h.from_state_version DESC
            LIMIT 1
        ) entries ON TRUE
        WHERE
            (NOT entries.filter_out)
            OR definitions.is_last_subquery_item
        ORDER BY
            definitions.cursor {(queryConfiguration.IsAscending ? "ASC" : "DESC")}
        LIMIT @pageLimit
    ) entries_per_entity ON TRUE
)
SELECT * FROM entries_per_entity
;",
            parameters: queryParameters,
            cancellationToken: token);

        var results = await dapperWrapper.QueryAsync<QueryResultRow>(dbConnection, commandDefinition);

        // NOTE: In some other instances where we have sub-pages, we may need to find roots for sub-pages here
        // and do a call to load them as a dictionary, before creating the data models, reading off the sub-page roots.

        return results
            .GroupBy(r => r.EntityId)
            .Select(g =>
            {
                var rows = g.ToList();
                var finalRow = rows.Last();
                var nextCursor = finalRow.NextExclusiveCursorStateVersion.HasValue
                    ? new GatewayModel.IdBoundaryCoursor(finalRow.NextExclusiveCursorStateVersion, finalRow.NextExclusiveCursorDefinitionId)
                    : null;
                return new PerEntityResult(
                    EntityId: finalRow.EntityId,
                    NonFungibleEntityAddress: (EntityAddress)finalRow.EntityAddress,
                    NextCursor: nextCursor,
                    TotalEntriesGivenPagingParameters: queryConfiguration.IncludeDeleted ? finalRow.TotalEntriesIncludingDeleted : finalRow.TotalEntriesExcludingDeleted,
                    TotalMinted: finalRow.TotalEntriesIncludingDeleted,
                    TotalSupply: finalRow.TotalEntriesExcludingDeleted,
                    PageItems: rows
                        .Where(f => !f.FilterOut)
                        .Select(row => new PageItem(
                            DefinitionId: row.DefinitionId!.Value,
                            NonFungibleId: row.Key!,
                            KeyFirstSeenStateVersion: row.KeyFirstSeenStateVersion!.Value,
                            Data: row.Value, // Will be null if !IncludeValue
                            IsLocked: row.IsLocked!.Value,
                            IsDeleted: row.IsDeleted!.Value,
                            DataLastUpdatedStateVersion: row.LastUpdatedStateVersion!.Value
                        )).ToList()
                );
            })
            .ToDictionary(r => r.NonFungibleEntityAddress);
    }
}
