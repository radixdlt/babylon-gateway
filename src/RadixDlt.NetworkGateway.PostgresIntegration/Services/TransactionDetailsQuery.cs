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
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal static class TransactionDetailsQuery
{
    internal record ReceiptEvent(string Name, string Emitter, byte[] Data, long EntityId, byte[] SchemaHash, long TypeIndex, SborTypeKind KeyTypeKind);

    internal record LedgerTransactionQueryResult(
        long StateVersion,
        long Epoch,
        long RoundInEpoch,
        TokenAmount FeePaid,
        long[] AffectedGlobalEntities,
        DateTime RoundTimestamp,
        LedgerTransactionStatus ReceiptStatus,
        string? ReceiptFeeSource,
        string? ReceiptFeeDestination,
        string? ReceiptErrorMessage,
        LedgerTransactionType Discriminator,
        string PayloadHash,
        string IntentHash,
        string? Message,
        LedgerTransactionManifestClass[] ManifestClasses,
        byte[]? RawPayload,
        string? ReceiptCostingParameters,
        string? ReceiptFeeSummary,
        string? ReceiptNextEpoch,
        string? ReceiptOutput,
        string? ReceiptStateUpdates,
        string? BalanceChanges,
        string? ManifestInstructions
    )
    {
        public List<ReceiptEvent> Events { get; set; } = new();
    }

    private record ReceiptEvents(
        string[] ReceiptEventEmitters,
        string[] ReceiptEventNames,
        byte[][] ReceiptEventSbors,
        long[] ReceiptEventSchemaEntityIds,
        byte[][] ReceiptEventSchemaHashes,
        long[] ReceiptEventTypeIndexes,
        SborTypeKind[] ReceiptEventSborTypeKinds);

    internal static async Task<List<LedgerTransactionQueryResult>> Execute(
        IDapperWrapper dapperWrapper,
        CommonDbContext dbContext,
        List<long> transactionStateVersions,
        GatewayApiSdk.Model.TransactionDetailsOptIns optIns,
        CancellationToken token)
    {
        var parameters = new
        {
            includeRawHex = optIns.RawHex,
            includeReceiptOutput = optIns.ReceiptOutput,
            includeReceiptStateChanges = optIns.ReceiptStateChanges,
            includeReceiptEvents = optIns.ReceiptEvents,
            includeBalanceChanges = optIns.BalanceChanges,
            includeManifestInstructions = optIns.ManifestInstructions,
            transactionStateVersions = transactionStateVersions,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
            @"
WITH vars AS (
    SELECT
        @includeRawHex AS with_raw_payload,
        true AS with_receipt_costing_parameters,
        true AS with_receipt_fee_summary,
        true AS with_receipt_next_epoch,
        @includeReceiptOutput AS with_receipt_output,
        @includeReceiptStateChanges AS with_receipt_state_updates,
        @includeReceiptEvents AS with_receipt_events,
        @includeBalanceChanges  AS with_balance_changes,
        @includeManifestInstructions AS with_manifest_instructions,
        @transactionStateVersions AS transaction_state_versions
)
SELECT
    lt.state_version,
    epoch,
    round_in_epoch,
    CAST(fee_paid AS TEXT),
    affected_global_entities,
    round_timestamp,
    receipt_status,
    receipt_fee_source,
    receipt_fee_destination,
    receipt_error_message,
    discriminator,
    payload_hash,
    intent_hash,
    message,
    manifest_classes,
    CASE WHEN vars.with_raw_payload THEN raw_payload END AS raw_payload,
    CASE WHEN vars.with_receipt_costing_parameters THEN receipt_costing_parameters END AS receipt_costing_parameters,
    CASE WHEN vars.with_receipt_fee_summary THEN receipt_fee_summary END AS receipt_fee_summary,
    CASE WHEN vars.with_receipt_next_epoch THEN receipt_next_epoch  END AS receipt_next_epoch,
    CASE WHEN vars.with_receipt_output THEN receipt_output END AS receipt_output,
    CASE WHEN vars.with_receipt_state_updates THEN receipt_state_updates END AS receipt_state_updates,
    CASE WHEN vars.with_balance_changes THEN balance_changes END AS balance_changes,
    CASE WHEN vars.with_manifest_instructions THEN manifest_instructions END AS manifest_instructions,
    CASE WHEN vars.with_receipt_events THEN lte.receipt_event_emitters END AS ReceiptEventEmitters,
    CASE WHEN vars.with_receipt_events THEN lte.receipt_event_names END AS ReceiptEventNames,
    CASE WHEN vars.with_receipt_events THEN lte.receipt_event_sbors END AS ReceiptEventSbors,
    CASE WHEN vars.with_receipt_events THEN lte.receipt_event_schema_entity_ids END AS ReceiptEventSchemaEntityIds,
    CASE WHEN vars.with_receipt_events THEN lte.receipt_event_schema_hashes END AS ReceiptEventSchemaHashes,
    CASE WHEN vars.with_receipt_events THEN lte.receipt_event_type_indexes END AS ReceiptEventTypeIndexes,
    CASE WHEN vars.with_receipt_events THEN lte.receipt_event_sbor_type_kinds END AS ReceiptEventSborTypeKinds
FROM vars
CROSS JOIN ledger_transactions lt
LEFT JOIN ledger_transaction_events lte ON vars.with_receipt_events AND lte.state_version = lt.state_version
WHERE lt.state_version = ANY(vars.transaction_state_versions)",
            parameters,
            cancellationToken: token);

        var transactions = (await dapperWrapper.QueryAsync<LedgerTransactionQueryResult, ReceiptEvents?, LedgerTransactionQueryResult>(
            dbContext.Database.GetDbConnection(),
            cd,
            (transaction, events) =>
            {
                if (events == null)
                {
                    return transaction;
                }

                var mappedEvents = events
                    .ReceiptEventEmitters
                    .Select(
                        (_, i) => new ReceiptEvent(
                            events.ReceiptEventNames[i],
                            events.ReceiptEventEmitters[i],
                            events.ReceiptEventSbors[i],
                            events.ReceiptEventSchemaEntityIds[i],
                            events.ReceiptEventSchemaHashes[i],
                            events.ReceiptEventTypeIndexes[i],
                            events.ReceiptEventSborTypeKinds[i]))
                    .ToList();

                transaction.Events = mappedEvents;

                return transaction;
            },
            "ReceiptEventEmitters")).ToList();

        return transactions;
    }
}
