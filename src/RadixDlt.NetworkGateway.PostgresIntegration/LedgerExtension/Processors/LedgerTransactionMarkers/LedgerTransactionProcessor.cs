// <copyright file="LedgerTransactionProcessor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Newtonsoft.Json;
using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.LedgerTransactionMarkers;

internal class LedgerTransactionProcessor : IProcessorBase, ITransactionProcessor, ISubstateUpsertProcessor
{
    private readonly ProcessorContext _context;
    private readonly IClock _clock;
    private readonly List<LedgerTransaction> _ledgerTransactionsToAdd = new();
    private readonly ReferencedEntityDictionary _referencedEntities;
    private readonly ManifestProcessor _manifestProcessor;
    private readonly AffectedGlobalEntitiesProcessor _affectedGlobalEntitiesProcessor;
    private readonly IWriteHelper _writeHelper;

    private TransactionSummary _lastProcessedTransactionSummary;

    public LedgerTransactionProcessor(
        ProcessorContext context,
        IClock clock,
        ReferencedEntityDictionary referencedEntities,
        ManifestProcessor manifestProcessor,
        AffectedGlobalEntitiesProcessor affectedGlobalEntitiesProcessor,
        IWriteHelper writeHelper,
        TransactionSummary lastTransactionSummary)
    {
        _context = context;
        _clock = clock;
        _referencedEntities = referencedEntities;
        _manifestProcessor = manifestProcessor;
        _affectedGlobalEntitiesProcessor = affectedGlobalEntitiesProcessor;
        _writeHelper = writeHelper;
        _lastProcessedTransactionSummary = lastTransactionSummary;
    }

    public void VisitTransaction(CoreModel.CommittedTransaction committedTransaction, long stateVersion)
    {
        var events = committedTransaction.Receipt.Events ?? new List<CoreModel.Event>();

        long? epochUpdate = null;
        long? roundInEpochUpdate = null;
        DateTime? roundTimestampUpdate = null;

        if (committedTransaction.LedgerTransaction is CoreModel.RoundUpdateLedgerTransaction roundUpdateTransaction)
        {
            epochUpdate = _lastProcessedTransactionSummary.Epoch != roundUpdateTransaction.RoundUpdateTransaction.Epoch ? roundUpdateTransaction.RoundUpdateTransaction.Epoch : null;
            roundInEpochUpdate = roundUpdateTransaction.RoundUpdateTransaction.RoundInEpoch;
            roundTimestampUpdate = DateTimeOffset.FromUnixTimeMilliseconds(roundUpdateTransaction.RoundUpdateTransaction.ProposerTimestamp.UnixTimestampMs).UtcDateTime;
        }

        var isStartOfEpoch = epochUpdate.HasValue;
        var isStartOfRound = roundInEpochUpdate.HasValue;
        var createdTimestamp = _clock.UtcNow;

        var roundTimestamp = roundTimestampUpdate ?? _lastProcessedTransactionSummary.RoundTimestamp;
        var normalizedRoundTimestamp =
            roundTimestamp < _lastProcessedTransactionSummary.NormalizedRoundTimestamp ? _lastProcessedTransactionSummary.NormalizedRoundTimestamp
            : roundTimestamp > createdTimestamp ? createdTimestamp
            : roundTimestamp;

        var epoch = epochUpdate ?? _lastProcessedTransactionSummary.Epoch;
        var roundInEpoch = roundInEpochUpdate ?? _lastProcessedTransactionSummary.RoundInEpoch;
        var indexInEpoch = isStartOfEpoch ? 0 : _lastProcessedTransactionSummary.IndexInEpoch + 1;
        var indexInRound = isStartOfRound ? 0 : _lastProcessedTransactionSummary.IndexInRound + 1;

        LedgerTransaction ledgerTransaction = committedTransaction.LedgerTransaction switch
        {
            CoreModel.GenesisLedgerTransaction => new GenesisLedgerTransaction(),
            CoreModel.UserLedgerTransaction ult => new UserLedgerTransaction
            {
                PayloadHash = ult.NotarizedTransaction.HashBech32m,
                IntentHash = ult.NotarizedTransaction.SignedIntent.Intent.HashBech32m,
                SignedIntentHash = ult.NotarizedTransaction.SignedIntent.HashBech32m,
                Message = ult.NotarizedTransaction.SignedIntent.Intent.Message?.ToJson(),
                RawPayload = ult.NotarizedTransaction.GetPayloadBytes(),
                ManifestInstructions = ult.NotarizedTransaction.SignedIntent.Intent.Instructions,
                ManifestClasses = _manifestProcessor.GetManifestClasses(stateVersion),
            },
            CoreModel.RoundUpdateLedgerTransaction => new RoundUpdateLedgerTransaction(),
            CoreModel.FlashLedgerTransaction => new FlashLedgerTransaction(),
            _ => throw new UnreachableException($"Unsupported transaction type: {committedTransaction.LedgerTransaction.GetType()}"),
        };


        ledgerTransaction.StateVersion = stateVersion;
        ledgerTransaction.TransactionTreeHash = committedTransaction.ResultantStateIdentifiers.TransactionTreeHash;
        ledgerTransaction.ReceiptTreeHash = committedTransaction.ResultantStateIdentifiers.ReceiptTreeHash;
        ledgerTransaction.StateTreeHash = committedTransaction.ResultantStateIdentifiers.StateTreeHash;
        ledgerTransaction.Epoch = epoch;
        ledgerTransaction.RoundInEpoch = roundInEpoch;
        ledgerTransaction.IndexInEpoch = indexInEpoch;
        ledgerTransaction.IndexInRound = indexInRound;
        ledgerTransaction.FeePaid = committedTransaction.Receipt.FeeSummary.TotalFee();
        ledgerTransaction.TipPaid = committedTransaction.Receipt.FeeSummary.TotalTip();
        ledgerTransaction.RoundTimestamp = roundTimestamp;
        ledgerTransaction.CreatedTimestamp = createdTimestamp;
        ledgerTransaction.NormalizedRoundTimestamp = normalizedRoundTimestamp;
        ledgerTransaction.ReceiptStateUpdates = committedTransaction.Receipt.StateUpdates.ToJson();
        ledgerTransaction.ReceiptStatus = committedTransaction.Receipt.Status.ToModel();
        ledgerTransaction.ReceiptFeeSummary = committedTransaction.Receipt.FeeSummary.ToJson();
        ledgerTransaction.ReceiptErrorMessage = committedTransaction.Receipt.ErrorMessage;
        ledgerTransaction.ReceiptOutput = committedTransaction.Receipt.Output != null ? JsonConvert.SerializeObject(committedTransaction.Receipt.Output) : null;
        ledgerTransaction.ReceiptNextEpoch = committedTransaction.Receipt.NextEpoch?.ToJson();
        ledgerTransaction.ReceiptCostingParameters = committedTransaction.Receipt.CostingParameters.ToJson();
        ledgerTransaction.ReceiptFeeSource = committedTransaction.Receipt.FeeSource?.ToJson();
        ledgerTransaction.ReceiptFeeDestination = committedTransaction.Receipt.FeeDestination?.ToJson();
        ledgerTransaction.BalanceChanges = committedTransaction.BalanceChanges?.ToJson();
        ledgerTransaction.AffectedGlobalEntities = _affectedGlobalEntitiesProcessor.GetAllAffectedGlobalEntities(stateVersion).ToArray();
        ledgerTransaction.ReceiptEventEmitters = events.Select(e => e.Type.Emitter.ToJson()).ToArray();
        ledgerTransaction.ReceiptEventNames = events.Select(e => e.Type.Name).ToArray();
        ledgerTransaction.ReceiptEventSbors = events.Select(e => e.Data.GetDataBytes()).ToArray();
        ledgerTransaction.ReceiptEventSchemaEntityIds = events.Select(e => _referencedEntities.Get((EntityAddress)e.Type.TypeReference.FullTypeId.EntityAddress).DatabaseId).ToArray();
        ledgerTransaction.ReceiptEventSchemaHashes = events.Select(e => e.Type.TypeReference.FullTypeId.SchemaHash.ConvertFromHex()).ToArray();
        ledgerTransaction.ReceiptEventTypeIndexes = events.Select(e => e.Type.TypeReference.FullTypeId.LocalTypeId.Id).ToArray();
        ledgerTransaction.ReceiptEventSborTypeKinds = events.Select(e => e.Type.TypeReference.FullTypeId.LocalTypeId.Kind.ToModel()).ToArray();

        _ledgerTransactionsToAdd.Add(ledgerTransaction);

        _lastProcessedTransactionSummary = new TransactionSummary(
            StateVersion: stateVersion,
            RoundTimestamp: roundTimestamp,
            NormalizedRoundTimestamp: normalizedRoundTimestamp,
            // TODO PP: i'm not sure it's correct.
            TransactionTreeHash: _lastProcessedTransactionSummary.TransactionTreeHash,
            ReceiptTreeHash: _lastProcessedTransactionSummary.ReceiptTreeHash,
            StateTreeHash: _lastProcessedTransactionSummary.StateTreeHash,
            CreatedTimestamp: createdTimestamp,
            Epoch: epoch,
            RoundInEpoch: roundInEpoch,
            IndexInEpoch: indexInEpoch,
            IndexInRound: indexInRound);
    }

    public void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.ConsensusManagerFieldCurrentTimeSubstate currentTime)
        {
            // TODO PP: we have to update it here.
            // TODO PP: that never worked?
            roundTimestampUpdate = DateTimeOffset.FromUnixTimeMilliseconds(currentTime.Value.ProposerTimestamp.UnixTimestampMs).UtcDateTime;
        }
    }

    public Task LoadDependenciesAsync()
    {
        return Task.CompletedTask;
    }

    public void ProcessChanges()
    {
    }

    public TransactionSummary GetLastProcessedTransactionSummary()
    {
        return _lastProcessedTransactionSummary;
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyLedgerTransactionMarkers();

        return rowsInserted;
    }

    private Task<int> CopyLedgerTransactionMarkers() => _context.WriteHelper.Copy(
        _ledgerTransactionsToAdd,
        "COPY ledger_transactions (state_version, transaction_tree_hash, receipt_tree_hash, state_tree_hash, epoch, round_in_epoch, index_in_epoch, index_in_round, fee_paid, tip_paid, affected_global_entities, round_timestamp, created_timestamp, normalized_round_timestamp, balance_changes, receipt_state_updates, receipt_status, receipt_fee_summary, receipt_fee_source, receipt_fee_destination, receipt_costing_parameters, receipt_error_message, receipt_output, receipt_next_epoch, receipt_event_emitters, receipt_event_names, receipt_event_sbors, receipt_event_schema_entity_ids, receipt_event_schema_hashes, receipt_event_type_indexes, receipt_event_sbor_type_kinds, discriminator, payload_hash, intent_hash, signed_intent_hash, message, raw_payload, manifest_instructions, manifest_classes) FROM STDIN (FORMAT BINARY)",
        async (writer, lt, token) =>
        {
            var discriminator = _writeHelper.GetDiscriminator<LedgerTransactionType>(lt.GetType());

            await _writeHelper.HandleMaxAggregateCounts(lt);
            await writer.StartRowAsync(token);
            await writer.WriteAsync(lt.StateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.TransactionTreeHash, NpgsqlDbType.Text, token);
            await writer.WriteAsync(lt.ReceiptTreeHash, NpgsqlDbType.Text, token);
            await writer.WriteAsync(lt.StateTreeHash, NpgsqlDbType.Text, token);
            await writer.WriteAsync(lt.Epoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.RoundInEpoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.IndexInEpoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.IndexInRound, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.FeePaid.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(lt.TipPaid.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(lt.AffectedGlobalEntities, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.RoundTimestamp, NpgsqlDbType.TimestampTz, token);
            await writer.WriteAsync(lt.CreatedTimestamp, NpgsqlDbType.TimestampTz, token);
            await writer.WriteAsync(lt.NormalizedRoundTimestamp, NpgsqlDbType.TimestampTz, token);
            await writer.WriteAsync(lt.BalanceChanges, NpgsqlDbType.Jsonb, token);

            await writer.WriteAsync(lt.EngineReceipt.StateUpdates, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.Status, "ledger_transaction_status", token);
            await writer.WriteAsync(lt.EngineReceipt.FeeSummary, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.FeeSource, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.FeeDestination, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.CostingParameters, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.ErrorMessage, NpgsqlDbType.Text, token);
            await writer.WriteAsync(lt.EngineReceipt.Output, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.NextEpoch, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.Events.Emitters, NpgsqlDbType.Array | NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.Events.Names, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
            await writer.WriteAsync(lt.EngineReceipt.Events.Sbors, NpgsqlDbType.Array | NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(lt.EngineReceipt.Events.SchemaEntityIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.EngineReceipt.Events.SchemaHashes, NpgsqlDbType.Array | NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(lt.EngineReceipt.Events.TypeIndexes, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.EngineReceipt.Events.SborTypeKinds, "sbor_type_kind[]", token);
            await writer.WriteAsync(discriminator, "ledger_transaction_type", token);

            switch (lt)
            {
                case GenesisLedgerTransaction:
                case RoundUpdateLedgerTransaction:
                case FlashLedgerTransaction:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case UserLedgerTransaction ult:
                    await writer.WriteAsync(ult.PayloadHash, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ult.IntentHash, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ult.SignedIntentHash, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ult.Message, NpgsqlDbType.Jsonb, token);
                    await writer.WriteAsync(ult.RawPayload, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(ult.ManifestInstructions, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ult.ManifestClasses, "ledger_transaction_manifest_class[]", token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lt), lt, null);
            }
        });
}
