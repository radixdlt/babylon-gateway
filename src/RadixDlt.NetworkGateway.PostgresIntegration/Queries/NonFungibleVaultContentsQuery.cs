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

using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class NonFungibleVaultContentsQuery
{
    public record ResultVault(long VaultEntityId, string VaultBalance)
    {
        public StateVersionIdCursor? NonFungibleIdsNextCursor { get; set; } // TODO set should throw if already non-null

        public List<ResultNonFungibleId> NonFungibleIds { get; } = new();
    }

    public record ResultNonFungibleId(
        long NonFungibleIdDefinitionId,
        long DefinitionFromStateVersion,
        bool DefinitionIsLastCandidate,
        long LastUpdatedAtStateVersion,
        bool IsDeleted,
        string NonFungibleId,
        byte[]? Data);

    public record struct QueryConfiguration(
        int NonFungibleIdsPerVault,
        bool DescendingOrder,
        StateVersionIdCursor? Cursor,
        long AtLedgerState);

    public static async Task<Dictionary<long, ResultVault>> Execute(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        ICollection<long> vaultIds,
        QueryConfiguration configuration,
        CancellationToken token = default)
    {
        // TODO throw if vaultEntityIds.Count > 1 and cursor is used ??
        // TODO add support for remaining features

        var cd = dapperWrapper.CreateCommandDefinition(
            @"WITH
variables AS (
    SELECT
        unnest(@vaultEntityIds) AS vault_entity_id,
        @scanLimit AS scan_limit,
        @pageLimit AS page_limit,
        FALSE AS ignore_deleted,
        TRUE AS include_data,
        @descendingOrder AS descending_order,
        ROW(@vaultCursorStateVersion, @vaultCursorId) AS vault_cursor,
        @atLedgerState AS at_ledger_state
)
SELECT
    var.vault_entity_id,
    CAST(balance.balance AS TEXT) AS vault_balance,

    entries.non_fungible_id_definition_id,
    entries.definition_from_state_version,
    entries.definition_is_last_candidate,
    entries.last_updated_at_state_version,
    entries.is_deleted,
    nf_def.non_fungible_id,
    nf_data.data
    -- , nf_data.is_deleted -- TODO how it relates to the entries.is_deleted? is it even needed?
FROM variables var
INNER JOIN LATERAL (
    SELECT
        ed.non_fungible_id_definition_id,
        ed.from_state_version AS definition_from_state_version,
        ed.is_last_candidate AS definition_is_last_candidate,
        eh.from_state_version AS last_updated_at_state_version,
        eh.is_deleted
    FROM (
        SELECT
            *,
            row_number() over (ORDER BY from_state_version DESC, id DESC) = var.scan_limit AS is_last_candidate
        FROM non_fungible_vault_entry_definition
        WHERE
            vault_entity_id = var.vault_entity_id
          AND from_state_version <= var.at_ledger_state
          AND CASE WHEN var.descending_order THEN (from_state_version, non_fungible_id_definition_id) <= var.vault_cursor ELSE (from_state_version, non_fungible_id_definition_id) >= var.vault_cursor END
        ORDER BY
            CASE WHEN var.descending_order THEN from_state_version END DESC,
            CASE WHEN var.descending_order THEN non_fungible_id_definition_id END DESC,
            CASE WHEN NOT var.descending_order THEN from_state_version END,
            CASE WHEN NOT var.descending_order THEN non_fungible_id_definition_id END
        LIMIT var.scan_limit
    ) ed
    INNER JOIN LATERAL (
        SELECT
            *,
            CASE WHEN var.ignore_deleted THEN NOT is_deleted ELSE TRUE END AS should_be_returned
        FROM non_fungible_vault_entry_history
        WHERE non_fungible_vault_entry_definition_id = ed.id AND ed.from_state_version <= var.at_ledger_state
        ORDER BY from_state_version DESC
        LIMIT 1
    ) eh ON TRUE
    WHERE ed.is_last_candidate OR eh.should_be_returned
    ORDER BY
        CASE WHEN var.descending_order THEN ed.from_state_version END DESC,
        CASE WHEN var.descending_order THEN ed.id END DESC,
        CASE WHEN NOT var.descending_order THEN ed.from_state_version END,
        CASE WHEN NOT var.descending_order THEN ed.id END
    LIMIT var.page_limit
) entries ON TRUE
INNER JOIN non_fungible_id_definition nf_def ON nf_def.id = entries.non_fungible_id_definition_id
LEFT JOIN LATERAL (
    SELECT *
    FROM non_fungible_id_data_history
    WHERE non_fungible_id_definition_id = nf_def.id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) nf_data ON var.include_data
LEFT JOIN LATERAL (
    SELECT *
    FROM vault_balance_history
    WHERE vault_entity_id = var.vault_entity_id AND from_state_version <= var.at_ledger_state
    ORDER BY from_state_version DESC
    LIMIT 1
) balance ON TRUE
ORDER BY
    CASE WHEN var.descending_order THEN entries.definition_from_state_version END DESC,
    CASE WHEN var.descending_order THEN entries.non_fungible_id_definition_id END DESC,
    CASE WHEN NOT var.descending_order THEN entries.definition_from_state_version END,
    CASE WHEN NOT var.descending_order THEN entries.non_fungible_id_definition_id END;",
            new
            {
                vaultEntityIds = vaultIds.ToList(),
                scanLimit = 5000 + 1, // TODO configurable / use const
                pageLimit = configuration.NonFungibleIdsPerVault + 1,
                descendingOrder = configuration.DescendingOrder,
                vaultCursorStateVersion = configuration.Cursor?.StateVersion ?? (configuration.DescendingOrder ? long.MaxValue : long.MinValue),
                vaultCursorId = configuration.Cursor?.Id ?? (configuration.DescendingOrder ? long.MaxValue : long.MinValue),
                atLedgerState = configuration.AtLedgerState,
            },
            token);

        var result = new Dictionary<long, ResultVault>();

        await dapperWrapper.QueryAsync<ResultVault, ResultNonFungibleId, ResultVault>(
            dbContext.Database.GetDbConnection(),
            cd,
            (vaultRow, entryRow) =>
            {
                var vault = result.GetOrAdd(vaultRow.VaultEntityId, _ => vaultRow);

                if (entryRow.DefinitionIsLastCandidate || vault.NonFungibleIds.Count >= configuration.NonFungibleIdsPerVault)
                {
                    vault.NonFungibleIdsNextCursor = new StateVersionIdCursor(entryRow.DefinitionFromStateVersion, entryRow.NonFungibleIdDefinitionId);
                }
                else
                {
                    vault.NonFungibleIds.Add(entryRow);
                }

                return vault;
            },
            "non_fungible_id_definition_id");

        return result;
    }
}
