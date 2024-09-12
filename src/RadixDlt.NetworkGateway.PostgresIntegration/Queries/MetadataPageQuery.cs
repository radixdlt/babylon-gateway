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
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries.CustomTypes;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class MetadataPageQuery
{
    internal readonly record struct QueryConfiguration(
        GatewayModel.IdBoundaryCoursor? Cursor,
        int PageSize,
        int MaxDefinitionsLookupLimit
    );

    private readonly record struct QueryResultRow(
        long EntityId,
        long TotalEntriesExcludingDeleted,
        long TotalEntriesIncludingDeleted,
        long DefinitionId,
        string Key,
        long KeyFirstSeenStateVersion,
        byte[] Value,
        bool IsLocked,
        bool IsDeleted,
        long LastUpdatedStateVersion,
        bool FilterOut,
        bool IsLastCandidate,
        IdBoundaryCursor? NextCursorInclusive
    );

    internal static async Task<IDictionary<long, GatewayModel.EntityMetadataCollection>> ReadPages(
        DbConnection dbConnection,
        IDapperWrapper dapperWrapper,
        GatewayModel.LedgerState ledgerState,
        ICollection<long> entityIds,
        QueryConfiguration queryConfiguration,
        byte networkId,
        CancellationToken token = default)
    {
        if (entityIds.Count == 0)
        {
            return ImmutableDictionary<long, GatewayModel.EntityMetadataCollection>.Empty;
        }

        var cd = new CommandDefinition(
            @"
WITH vars AS (
    SELECT
        unnest(@entityIds) AS entity_id,
        CAST(@useCursor AS bool) AS use_cursor,
        ROW(CAST(@cursorStateVersion AS bigint), CAST(@cursorDefinitionId  AS bigint)) AS start_cursor_inclusive,
        @atLedgerState AS at_ledger_state,
        @perEntityDefinitionReadLimit AS per_entity_definition_read_limit,
        @perEntityPageLimit as per_entity_page_limit
),
definitions_with_cursor AS (
     SELECT
         d.*,
         (d.from_state_version, d.id) AS cursor
     FROM entity_metadata_entry_definition d
)
SELECT
    -- entity data
    vars.entity_id AS EntityId,

    -- totals
    COALESCE(entity_totals.total_entries_excluding_deleted, 0) AS TotalEntriesExcludingDeleted,
    COALESCE(entity_totals.total_entries_including_deleted, 0) AS TotalEntriesIncludingDeleted,

    -- definitions
    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.definition_id END AS DefinitionId,
    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.key END AS Key,
    CASE WHEN COALESCE(filter_out, TRUE) THEN NULL ELSE entries_per_entity.key_first_seen_state_version END AS KeyFirstSeenStateVersion,

    -- history
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
    FROM entity_metadata_totals_history t
    WHERE t.entity_id = vars.entity_id AND t.from_state_version <= vars.at_ledger_state
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
        CASE WHEN (ROW_NUMBER() OVER (ORDER BY definitions.cursor DESC)) = vars.per_entity_page_limit OR entries.filter_out
            THEN TRUE
            ELSE FALSE
        END AS filter_out,
        CASE
            WHEN (ROW_NUMBER() OVER (ORDER BY definitions.cursor DESC)) = vars.per_entity_page_limit
                THEN definitions.cursor
            WHEN (ROW_NUMBER() OVER (ORDER BY definitions.cursor DESC)) != vars.per_entity_page_limit AND definitions.is_last_candidate
                THEN ROW(definitions.key_first_seen_state_version, definitions.id - 1)
        END AS next_cursor_inclusive
     FROM (
            SELECT
                  d.id,
                  d.key,
                  d.cursor,
                  d.from_state_version AS key_first_seen_state_version,
                  (ROW_NUMBER() OVER (ORDER BY d.cursor DESC)) = vars.per_entity_definition_read_limit AS is_last_candidate
            FROM definitions_with_cursor d
            WHERE d.entity_id = vars.entity_id AND ((NOT vars.use_cursor) OR d.cursor <= vars.start_cursor_inclusive)
            ORDER BY d.cursor DESC
            LIMIT vars.per_entity_definition_read_limit
    ) definitions
    INNER JOIN LATERAL (
        SELECT
            h.from_state_version AS last_updated_state_version,
            h.value,
            h.is_locked,
            h.is_deleted,
            h.is_deleted as filter_out
        FROM entity_metadata_entry_history h
        WHERE h.entity_metadata_entry_definition_id = definitions.id AND h.from_state_version <= vars.at_ledger_state
        ORDER BY h.from_state_version DESC
        LIMIT 1
    ) entries ON TRUE
WHERE entries.filter_out = FALSE OR definitions.is_last_candidate
ORDER BY definitions.cursor DESC
LIMIT vars.per_entity_page_limit
) entries_per_entity ON TRUE
ORDER BY entries_per_entity.cursor DESC
;",
            new
            {
                entityIds = entityIds,
                useCursor = queryConfiguration.Cursor is not null,
                atLedgerState = ledgerState.StateVersion,
                cursorStateVersion = queryConfiguration.Cursor?.StateVersionBoundary ?? 0,
                cursorDefinitionId = queryConfiguration.Cursor?.IdBoundary ?? 0,
                perEntityPageLimit = queryConfiguration.PageSize + 1,
                perEntityDefinitionReadLimit = Math.Floor(queryConfiguration.MaxDefinitionsLookupLimit / (decimal)entityIds.Count),
            },
            cancellationToken: token);
        var queryResult = (await dapperWrapper.ToListAsync<QueryResultRow>(dbConnection, cd)).ToList();

        var result = queryResult
            .GroupBy(r => r.EntityId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var rows = g.ToList();
                    var totalEntries = rows.First().TotalEntriesExcludingDeleted;
                    var elementWithCursor = rows.SingleOrDefault(x => x.NextCursorInclusive.HasValue);

                    var items = rows
                        .Where(x => !x.FilterOut)
                        .Select(
                            x =>
                            {
                                var value = ScryptoSborUtils.DecodeToGatewayMetadataItemValue(x.Value, networkId);
                                var programmaticJson = ScryptoSborUtils.DataToProgrammaticJson(x.Value, networkId);
                                var entityMetadataItemValue = new GatewayModel.EntityMetadataItemValue(x.Value.ToHex(), programmaticJson, value);
                                return new GatewayModel.EntityMetadataItem(x.Key, entityMetadataItemValue, x.IsLocked, x.LastUpdatedStateVersion);
                            })
                        .ToList();

                    var nextCursor = elementWithCursor.NextCursorInclusive != null
                        ? new GatewayModel.IdBoundaryCoursor(
                                elementWithCursor.NextCursorInclusive.Value.StateVersion,
                                elementWithCursor.NextCursorInclusive.Value.Id)
                            .ToCursorString()
                        : null;

                    return new GatewayModel.EntityMetadataCollection(
                        totalEntries,
                        nextCursor,
                        items);
                });

        return result;
    }
}
