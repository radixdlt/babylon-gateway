// <copyright file="TransactionDetailsQuery.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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
