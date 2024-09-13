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
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class ExplicitMetadataQuery
{
    private readonly record struct ExplicitMetadataLookup(long EntityId, string MetadataKey);

    private readonly record struct QueryResultRow(
        long EntityId,
        long TotalEntriesExcludingDeleted,
        long TotalEntriesIncludingDeleted,
        string Key,
        long KeyFirstSeenStateVersion,
        byte[] Value,
        bool IsLocked,
        long LastUpdatedStateVersion,
        bool FilterOut
    );

    internal static async Task<Dictionary<long, GatewayModel.EntityMetadataCollection>> Read(
        DbConnection dbConnection,
        IDapperWrapper dapperWrapper,
        long[] entityIds,
        string[] metadataKeys,
        GatewayApiSdk.Model.LedgerState ledgerState,
        byte networkId,
        CancellationToken token = default)
    {
        var lookup = new HashSet<ExplicitMetadataLookup>();
        var entityIdsParameter = new List<long>();
        var metadataKeysParameter = new List<string>();

        foreach (var entityId in entityIds)
        {
            foreach (var metadataKey in metadataKeys)
            {
                lookup.Add(new ExplicitMetadataLookup(entityId, metadataKey));
            }
        }

        foreach (var (entityId, metadataKey) in lookup)
        {
            entityIdsParameter.Add(entityId);
            metadataKeysParameter.Add(metadataKey);
        }

        var commandDefinition = new CommandDefinition(
            @"
WITH vars AS (
    SELECT
        UNNEST(@entityIds) as entity_id,
        UNNEST(@metadataKeys) as metadata_key,
        @atLedgerState AS at_ledger_state
)
SELECT
    -- entity id
    vars.entity_id as EntityId,

    -- totals
    COALESCE(entity_totals.total_entries_excluding_deleted, 0) AS TotalEntriesExcludingDeleted,
    COALESCE(entity_totals.total_entries_including_deleted, 0) AS TotalEntriesIncludingDeleted,

    -- data
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.key END,
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.KeyFirstSeenStateVersion END,
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.value END,
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.is_locked END AS IsLocked,
    CASE WHEN COALESCE(entries_with_definitions.filter_out, TRUE) THEN NULL ELSE entries_with_definitions.LastUpdatedStateVersion END,
    COALESCE(entries_with_definitions.filter_out, TRUE) AS FilterOut
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

-- entries
LEFT JOIN LATERAL (
    SELECT
        definition.entity_id,
        definition.key,
        definition.from_state_version AS KeyFirstSeenStateVersion,
        history.value,
        history.is_locked,
        history.is_deleted AS filter_out,
        history.from_state_version AS LastUpdatedStateVersion
    FROM entity_metadata_entry_definition definition
    INNER JOIN LATERAL (
        SELECT
            value,
            is_locked,
            is_deleted,
            from_state_version
        FROM entity_metadata_entry_history history
        WHERE history.entity_metadata_entry_definition_id = definition.id AND from_state_version <= vars.at_ledger_state
        ORDER BY history.from_state_version DESC
        LIMIT 1
    ) history on true
    WHERE definition.entity_id = vars.entity_id AND definition.key = vars.metadata_key
 ) entries_with_definitions on TRUE
;",
            new
            {
                entityIds = entityIdsParameter,
                metadataKeys = metadataKeysParameter,
                atLedgerState = ledgerState.StateVersion,
            },
            cancellationToken: token
        );

        var queryResult = await dapperWrapper.ToListAsync<QueryResultRow>(
            dbConnection,
            commandDefinition);

        var result = queryResult
            .GroupBy(r => r.EntityId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var totalEntries = g.First().TotalEntriesExcludingDeleted;

                    var items = g
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

                    return new GatewayModel.EntityMetadataCollection(totalEntries, items: items);
                });

        return result;
    }
}
