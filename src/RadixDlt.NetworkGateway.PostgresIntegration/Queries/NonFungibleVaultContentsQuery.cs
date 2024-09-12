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
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries.CustomTypes;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class NonFungibleVaultContentsQuery
{
    private record QueryResultRow(
        long VaultEntityId,
        TokenAmount TotalEntries,
        long DefinitionId,
        long FirstSeenStateVersion,
        long LastUpdatedStateVersion,
        bool IsDeleted,
        long NonFungibleIdDefinitionId,
        string NonFungibleId,
        byte[]? Data,
        bool FilterOut,
        bool IsLastCandidate,
        IdBoundaryCursor? NextCursorInclusive
    );

    public record struct QueryConfiguration(
        IdBoundaryCursor? Cursor,
        int PageSize,
        int MaxDefinitionsLookupLimit
        );

    public static async Task<IDictionary<long, GatewayModel.NonFungibleIdsCollection>> Execute(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        GatewayModel.LedgerState ledgerState,
        ICollection<long> vaultEntityIds,
        QueryConfiguration configuration,
        CancellationToken token)
    {
        if (vaultEntityIds.Count == 0)
        {
            return ImmutableDictionary<long, GatewayModel.NonFungibleIdsCollection>.Empty;
        }

        if (vaultEntityIds.Count > 1 && configuration.Cursor != null)
        {
            throw new InvalidOperationException("Can't use cursor if executing against multiple vault entities.");
        }

        var cd = new CommandDefinition(
            @"
WITH vars AS (
    SELECT
        unnest(@vaultEntityIds) AS vault_entity_id,
        CAST(@useCursor AS bool) AS use_cursor,
        ROW(@cursorStateVersion, @cursorId) AS start_cursor_inclusive,
        @atLedgerState AS at_ledger_state,
        @perEntityDefinitionReadLimit AS per_entity_definition_read_limit,
        @perEntityPageLimit as per_entity_page_limit
),
definitions_with_cursor AS (
        SELECT
            d.*,
            (d.from_state_version, d.id) AS cursor
        FROM non_fungible_vault_entry_definition d
)
SELECT
    -- Vault data.
    vars.vault_entity_id AS VaultEntityId,
    CAST(vault_balance.balance AS TEXT) AS TotalEntries,

    -- Vault content data.
    CASE WHEN COALESCE(entries.filter_out, TRUE) THEN NULL ELSE entries.definition_id END AS DefinitionId,
    CASE WHEN COALESCE(entries.filter_out, TRUE) THEN NULL ELSE entries.definition_from_state_version END AS FirstSeenStateVersion,
    CASE WHEN COALESCE(entries.filter_out, TRUE) THEN NULL ELSE entries.last_updated_state_version END AS LastUpdatedStateVersion,
    CASE WHEN COALESCE(entries.filter_out, TRUE) THEN NULL ELSE entries.is_deleted END AS IsDeleted,
    CASE WHEN COALESCE(entries.filter_out, TRUE) THEN NULL ELSE entries.non_fungible_id_definition_id END AS NonFungibleIdDefinitionId,

    -- Non fungible id data
    CASE WHEN COALESCE(entries.filter_out, TRUE) THEN NULL ELSE nf_def.non_fungible_id END AS NonFungibleId,
    CASE WHEN COALESCE(entries.filter_out, TRUE) THEN NULL ELSE non_fungible_id_data.data END,

    -- Cursor
    COALESCE(entries.filter_out, TRUE) AS FilterOut,
    COALESCE(entries.is_last_candidate, TRUE) AS IsLastCandidate,
    entries.next_cursor_inclusive AS NextCursorInclusive
FROM vars
LEFT JOIN LATERAL (
    SELECT
        definitions.id AS definition_id,
        definitions.non_fungible_id_definition_id,
        definitions.from_state_version AS definition_from_state_version,
        definitions.is_last_candidate AS is_last_candidate,
        definitions.cursor,
        history.from_state_version AS last_updated_state_version,
        history.is_deleted,
        CASE
            WHEN (ROW_NUMBER() OVER (ORDER BY definitions.cursor DESC)) = vars.per_entity_page_limit OR history.filter_out
            THEN TRUE
            ELSE FALSE
        END AS filter_out,
        CASE
            WHEN (ROW_NUMBER() OVER (ORDER BY definitions.cursor DESC)) = vars.per_entity_page_limit
                THEN definitions.cursor
            WHEN (ROW_NUMBER() OVER (ORDER BY definitions.cursor DESC)) != vars.per_entity_page_limit AND definitions.is_last_candidate
                THEN ROW(definitions.from_state_version, definitions.id - 1)
        END AS next_cursor_inclusive
    FROM (
        SELECT
             id,
             non_fungible_id_definition_id,
             from_state_version,
             cursor,
             (ROW_NUMBER() OVER (ORDER BY d.cursor DESC)) = vars.per_entity_definition_read_limit AS is_last_candidate
         FROM definitions_with_cursor d
         WHERE
             vault_entity_id = vars.vault_entity_id
           AND from_state_version <= vars.at_ledger_state
           AND ((NOT vars.use_cursor) OR d.cursor <= vars.start_cursor_inclusive)
         ORDER BY d.cursor DESC
         LIMIT vars.per_entity_definition_read_limit
        ) definitions
        INNER JOIN LATERAL (
        SELECT
            h.from_state_version,
            h.is_deleted,
            is_deleted AS filter_out
        FROM non_fungible_vault_entry_history h
        WHERE h.non_fungible_vault_entry_definition_id = definitions.id AND h.from_state_version <= vars.at_ledger_state
        ORDER BY h.from_state_version DESC
        LIMIT 1
    ) history ON TRUE
    WHERE history.filter_out = FALSE OR definitions.is_last_candidate
    ORDER BY definitions.cursor DESC
    LIMIT vars.per_entity_page_limit
) entries ON TRUE
LEFT JOIN non_fungible_id_definition nf_def ON nf_def.id = entries.non_fungible_id_definition_id
LEFT JOIN LATERAL (
    SELECT *
    FROM non_fungible_id_data_history
    WHERE non_fungible_id_definition_id = nf_def.id AND from_state_version <= vars.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) non_fungible_id_data ON TRUE
LEFT JOIN LATERAL (
    SELECT *
    FROM vault_balance_history
    WHERE vault_entity_id = vars.vault_entity_id AND from_state_version <= vars.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) vault_balance ON TRUE
ORDER BY entries.cursor DESC
;",
            new
            {
                vaultEntityIds = vaultEntityIds.ToList(),
                useCursor = configuration.Cursor is not null,
                cursorStateVersion = configuration.Cursor?.StateVersion,
                cursorId = configuration.Cursor?.Id,
                atLedgerState = ledgerState.StateVersion,
                perEntityDefinitionReadLimit = Math.Floor(configuration.MaxDefinitionsLookupLimit / (decimal)vaultEntityIds.Count),
                perEntityPageLimit = configuration.PageSize + 1,
            },
            cancellationToken: token);

        var queryResult = (await dapperWrapper.QueryAsync<QueryResultRow>(dbContext.Database.GetDbConnection(), cd)).ToList();
        var result = queryResult
            .GroupBy(x => x.VaultEntityId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var rows = g.ToList();
                    var totalEntries = long.Parse(rows.First().TotalEntries.ToString());
                    var elementWithCursor = rows.SingleOrDefault(x => x.NextCursorInclusive.HasValue);

                    var items = rows
                        .Where(x => !x.FilterOut)
                        .Select(x => x.NonFungibleId)
                        .ToList();

                    var nextCursor = elementWithCursor?.NextCursorInclusive != null
                        ? new GatewayModel.IdBoundaryCoursor(
                                elementWithCursor.NextCursorInclusive.Value.StateVersion,
                                elementWithCursor.NextCursorInclusive.Value.Id)
                            .ToCursorString()
                        : null;

                    return new GatewayModel.NonFungibleIdsCollection(totalEntries, nextCursor, items);
                });

        return result;
    }
}
