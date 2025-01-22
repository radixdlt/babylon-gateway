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

using Newtonsoft.Json;
using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Configuration;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors.LedgerTransactionMarkers;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension.Processors;

internal class LedgerTransactionProcessor : IProcessorBase, ITransactionProcessor, ISubstateUpsertProcessor
{
    private record TransactionData(
        long StateVersion,
        DateTime CreatedAt,
        CoreModel.CommittedTransaction RawCommittedTransaction
    )
    {
        public long? NewEpochIndex { get; set; }

        public long? NewRoundIndex { get; set; }

        public DateTime? RoundTimestampUpdate { get; set; }
    }

    private readonly ProcessorContext _context;
    private readonly IClock _clock;
    private readonly List<LedgerTransaction> _ledgerTransactionsToAdd = new();
    private readonly List<LedgerFinalizedSubintent> _ledgerFinalizedSubintentsToAdd = new();
    private readonly List<LedgerTransactionSubintentData> _subintentDataToAdd = new();
    private readonly List<LedgerTransactionEvents> _ledgerTransactionEventsToAdd = new();
    private readonly ReferencedEntityDictionary _referencedEntities;
    private readonly ManifestProcessor _manifestProcessor;
    private readonly AffectedGlobalEntitiesProcessor _affectedGlobalEntitiesProcessor;
    private readonly IWriteHelper _writeHelper;

    private readonly Dictionary<long, TransactionData> _transactionData = new();
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
        _transactionData.Add(stateVersion, new TransactionData(stateVersion, _clock.UtcNow, committedTransaction));

        if (committedTransaction.LedgerTransaction is CoreModel.RoundUpdateLedgerTransaction roundUpdateTransaction)
        {
            var newEpochIndex = _lastProcessedTransactionSummary.Epoch != roundUpdateTransaction.RoundUpdateTransaction.Epoch ? roundUpdateTransaction.RoundUpdateTransaction.Epoch : (long?)null;

            _transactionData.Update(
                stateVersion,
                existing =>
                {
                    existing.NewEpochIndex = newEpochIndex;
                    existing.NewRoundIndex = roundUpdateTransaction.RoundUpdateTransaction.RoundInEpoch;
                    existing.RoundTimestampUpdate = DateTimeOffset.FromUnixTimeMilliseconds(roundUpdateTransaction.RoundUpdateTransaction.ProposerTimestamp.UnixTimestampMs).UtcDateTime;
                });
        }
    }

    public void VisitUpsert(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.ConsensusManagerFieldCurrentTimeSubstate currentTime)
        {
            _transactionData.Update(
                stateVersion,
                existing => { existing.RoundTimestampUpdate = DateTimeOffset.FromUnixTimeMilliseconds(currentTime.Value.ProposerTimestamp.UnixTimestampMs).UtcDateTime; });
        }
    }

    public Task LoadDependenciesAsync()
    {
        return Task.CompletedTask;
    }

    public void ProcessChanges()
    {
        foreach (var data in _transactionData.Values)
        {
            var stateVersion = data.StateVersion;
            var committedTransaction = data.RawCommittedTransaction;

            var roundTimestamp = data.RoundTimestampUpdate ?? _lastProcessedTransactionSummary.RoundTimestamp;

            var normalizedRoundTimestamp =
                roundTimestamp < _lastProcessedTransactionSummary.NormalizedRoundTimestamp ? _lastProcessedTransactionSummary.NormalizedRoundTimestamp
                : roundTimestamp > data.CreatedAt ? data.CreatedAt
                : roundTimestamp;

            var epoch = data.NewEpochIndex ?? _lastProcessedTransactionSummary.Epoch;
            var roundInEpoch = data.NewRoundIndex ?? _lastProcessedTransactionSummary.RoundInEpoch;
            var indexInEpoch = data.NewEpochIndex.HasValue ? 0 : _lastProcessedTransactionSummary.IndexInEpoch + 1;
            var indexInRound = data.NewEpochIndex.HasValue ? 0 : _lastProcessedTransactionSummary.IndexInRound + 1;

            LedgerTransaction ledgerTransaction = committedTransaction.LedgerTransaction switch
            {
                CoreModel.GenesisLedgerTransaction => new GenesisLedgerTransaction(),
                CoreModel.UserLedgerTransaction ultv1 => new UserLedgerTransactionV1
                {
                    PayloadHash = ultv1.NotarizedTransaction.HashBech32m,
                    RawPayload = ultv1.NotarizedTransaction.GetPayloadBytes(),
                    IntentHash = ultv1.NotarizedTransaction.SignedIntent.Intent.HashBech32m,
                    SignedIntentHash = ultv1.NotarizedTransaction.SignedIntent.HashBech32m,
                    Message = ultv1.NotarizedTransaction.SignedIntent.Intent.Message?.ToJson(),
                    ManifestInstructions = ultv1.NotarizedTransaction.SignedIntent.Intent.Instructions,
                    ManifestClasses = _manifestProcessor.GetManifestClasses(stateVersion),
                },
                CoreModel.UserLedgerTransactionV2 ultv2 => new UserLedgerTransactionV2
                {
                    PayloadHash = ultv2.NotarizedTransaction.HashBech32m,
                    RawPayload = ultv2.NotarizedTransaction.GetPayloadBytes(),
                    IntentHash = ultv2.NotarizedTransaction.SignedTransactionIntent.TransactionIntent.HashBech32m,
                    SignedIntentHash = ultv2.NotarizedTransaction.SignedTransactionIntent.HashBech32m,
                    Message = ultv2.NotarizedTransaction.SignedTransactionIntent.TransactionIntent.RootIntentCore.Message?.ToJson(),
                    ManifestInstructions = ultv2.NotarizedTransaction.SignedTransactionIntent.TransactionIntent.RootIntentCore.Instructions,
                    ManifestClasses = _manifestProcessor.GetManifestClasses(stateVersion),
                },
                CoreModel.RoundUpdateLedgerTransaction => new RoundUpdateLedgerTransaction(),
                CoreModel.FlashLedgerTransaction => new FlashLedgerTransaction(),
                _ => throw new UnreachableException($"Unsupported transaction type: {committedTransaction.LedgerTransaction.GetType()}"),
            };

            ledgerTransaction.Epoch = epoch;
            ledgerTransaction.RoundInEpoch = roundInEpoch;
            ledgerTransaction.IndexInEpoch = indexInEpoch;
            ledgerTransaction.IndexInRound = indexInRound;
            ledgerTransaction.RoundTimestamp = roundTimestamp;
            ledgerTransaction.CreatedTimestamp = data.CreatedAt;
            ledgerTransaction.NormalizedRoundTimestamp = normalizedRoundTimestamp;
            ledgerTransaction.StateVersion = stateVersion;
            ledgerTransaction.TransactionTreeHash = committedTransaction.ResultantStateIdentifiers.TransactionTreeHash;
            ledgerTransaction.ReceiptTreeHash = committedTransaction.ResultantStateIdentifiers.ReceiptTreeHash;
            ledgerTransaction.StateTreeHash = committedTransaction.ResultantStateIdentifiers.StateTreeHash;
            ledgerTransaction.FeePaid = committedTransaction.Receipt.FeeSummary.TotalFee();
            ledgerTransaction.TipPaid = committedTransaction.Receipt.FeeSummary.TotalTip();
            ledgerTransaction.ReceiptStatus = committedTransaction.Receipt.Status.ToModel();
            ledgerTransaction.ReceiptFeeSource = committedTransaction.Receipt.FeeSource?.ToJson();
            ledgerTransaction.ReceiptFeeDestination = committedTransaction.Receipt.FeeDestination?.ToJson();
            ledgerTransaction.ReceiptErrorMessage = committedTransaction.Receipt.ErrorMessage;
            ledgerTransaction.ReceiptFeeSummary = committedTransaction.Receipt.FeeSummary.ToJson();
            ledgerTransaction.ReceiptOutput = committedTransaction.Receipt.Output != null ? JsonConvert.SerializeObject(committedTransaction.Receipt.Output) : null;
            ledgerTransaction.ReceiptCostingParameters = committedTransaction.Receipt.CostingParameters.ToJson();
            ledgerTransaction.ReceiptNextEpoch = committedTransaction.Receipt.NextEpoch?.ToJson();
            ledgerTransaction.BalanceChanges = committedTransaction.BalanceChanges?.ToJson();
            ledgerTransaction.AffectedGlobalEntities = _affectedGlobalEntitiesProcessor.GetAllAffectedGlobalEntities(stateVersion).ToArray();

            _ledgerTransactionsToAdd.Add(ledgerTransaction);

            if (committedTransaction.LedgerTransaction is CoreModel.UserLedgerTransactionV2 userLedgerTransactionV2)
            {
                if (committedTransaction.Receipt.Status == CoreModel.TransactionStatus.Succeeded)
                {
                    _ledgerFinalizedSubintentsToAdd.AddRange(
                        userLedgerTransactionV2.NotarizedTransaction.SignedTransactionIntent.TransactionIntent.NonRootSubintents.Select(
                            (x, index) =>
                                new LedgerFinalizedSubintent
                                {
                                    SubintentHash = x.HashBech32m,
                                    FinalizedAtStateVersion = stateVersion,
                                    FinalizedAtTransactionIntentHash = userLedgerTransactionV2.NotarizedTransaction.SignedTransactionIntent.TransactionIntent.HashBech32m,
                                }));
                }

                var subintentData = userLedgerTransactionV2
                    .NotarizedTransaction
                    .SignedTransactionIntent
                    .TransactionIntent
                    .NonRootSubintents
                    .Select(
                        x => new SubintentData
                        {
                            SubintentHash = x.HashBech32m,
                            Message = x.IntentCore.Message?.ToJson(),
                            ManifestInstructions = x.IntentCore.Instructions,
                            ChildSubintentHashes = x.IntentCore.ChildrenSpecifiers,
                        })
                    .ToList();

                _subintentDataToAdd.Add(
                    new LedgerTransactionSubintentData
                    {
                        StateVersion = stateVersion,
                        ChildSubintentHashes = userLedgerTransactionV2.NotarizedTransaction.SignedTransactionIntent.TransactionIntent.RootIntentCore.ChildrenSpecifiers,
                        SubintentData = JsonConvert.SerializeObject(subintentData),
                    });
            }

            var isUserTransaction = committedTransaction.LedgerTransaction is CoreModel.UserLedgerTransaction or CoreModel.UserLedgerTransactionV2;
            var isUserTransactionOrEpochChange = isUserTransaction || data.NewEpochIndex.HasValue;

            if (_context.StorageOptions.StoreReceiptStateUpdates == LedgerTransactionStorageOption.StoreForAllTransactions ||
                (_context.StorageOptions.StoreReceiptStateUpdates == LedgerTransactionStorageOption.StoreOnlyForUserTransactions && isUserTransaction) ||
                (_context.StorageOptions.StoreReceiptStateUpdates
                    is LedgerTransactionStorageOption.StoreOnlyForUserTransactionsAndEpochChanges
                    or LedgerTransactionStorageOption.StoryOnlyForUserTransactionsAndEpochChanges && isUserTransactionOrEpochChange)
               )
            {
                ledgerTransaction.ReceiptStateUpdates = committedTransaction.Receipt.StateUpdates.ToJson();
            }

            if (_context.StorageOptions.StoreTransactionReceiptEvents == LedgerTransactionStorageOption.StoreForAllTransactions ||
                (_context.StorageOptions.StoreTransactionReceiptEvents == LedgerTransactionStorageOption.StoreOnlyForUserTransactions && isUserTransaction) ||
                (_context.StorageOptions.StoreTransactionReceiptEvents
                     is LedgerTransactionStorageOption.StoreOnlyForUserTransactionsAndEpochChanges
                     or LedgerTransactionStorageOption.StoryOnlyForUserTransactionsAndEpochChanges
                 && isUserTransactionOrEpochChange)
               )
            {
                var events = committedTransaction.Receipt.Events ?? new List<CoreModel.Event>();

                _ledgerTransactionEventsToAdd.Add(
                    new LedgerTransactionEvents
                    {
                        StateVersion = stateVersion,
                        ReceiptEventEmitters = events.Select(e => e.Type.Emitter.ToJson()).ToArray(),
                        ReceiptEventEmitterEntityIds = events.Select(e => _referencedEntities.Get((EntityAddress)e.Type.GetEmitterAddress()).DatabaseId).ToArray(),
                        ReceiptEventNames = events.Select(e => e.Type.Name).ToArray(),
                        ReceiptEventSbors = events.Select(e => e.Data.GetDataBytes()).ToArray(),
                        ReceiptEventSchemaEntityIds = events.Select(e => _referencedEntities.Get((EntityAddress)e.Type.TypeReference.FullTypeId.EntityAddress).DatabaseId).ToArray(),
                        ReceiptEventSchemaHashes = events.Select(e => e.Type.TypeReference.FullTypeId.SchemaHash.ConvertFromHex()).ToArray(),
                        ReceiptEventTypeIndexes = events.Select(e => e.Type.TypeReference.FullTypeId.LocalTypeId.Id).ToArray(),
                        ReceiptEventSborTypeKinds = events.Select(e => e.Type.TypeReference.FullTypeId.LocalTypeId.Kind.ToModel()).ToArray(),
                    });
            }

            _lastProcessedTransactionSummary = new TransactionSummary(
                StateVersion: stateVersion,
                RoundTimestamp: roundTimestamp,
                NormalizedRoundTimestamp: normalizedRoundTimestamp,
                CreatedTimestamp: data.CreatedAt,
                Epoch: epoch,
                RoundInEpoch: roundInEpoch,
                IndexInEpoch: indexInEpoch,
                IndexInRound: indexInRound);
        }
    }

    public TransactionSummary GetSummaryOfLastProcessedTransaction()
    {
        return _lastProcessedTransactionSummary;
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyLedgerTransactions();
        rowsInserted += await CopyLedgerTransactionEvents();
        rowsInserted += await CopyLedgerSubintents();
        rowsInserted += await CopyLedgerTransactionSubintentData();

        return rowsInserted;
    }

    private Task<int> CopyLedgerTransactions() => _context.WriteHelper.Copy(
        _ledgerTransactionsToAdd,
        "COPY ledger_transactions (state_version, transaction_tree_hash, receipt_tree_hash, state_tree_hash, epoch, round_in_epoch, index_in_epoch, index_in_round, fee_paid, tip_paid, affected_global_entities, round_timestamp, created_timestamp, normalized_round_timestamp, balance_changes, receipt_state_updates, receipt_status, receipt_fee_summary, receipt_fee_source, receipt_fee_destination, receipt_costing_parameters, receipt_error_message, receipt_output, receipt_next_epoch, discriminator, payload_hash, intent_hash, signed_intent_hash, message, raw_payload, manifest_instructions, manifest_classes) FROM STDIN (FORMAT BINARY)",
        async (writer, lt, token) =>
        {
            var discriminator = _writeHelper.GetDiscriminator<LedgerTransactionType>(lt.GetType());

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
                case UserLedgerTransactionV1 ult:
                    await writer.WriteAsync(ult.PayloadHash, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ult.IntentHash, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ult.SignedIntentHash, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ult.Message, NpgsqlDbType.Jsonb, token);
                    await writer.WriteAsync(ult.RawPayload, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(ult.ManifestInstructions, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ult.ManifestClasses, "ledger_transaction_manifest_class[]", token);
                    break;
                case UserLedgerTransactionV2 ultv2:
                    await writer.WriteAsync(ultv2.PayloadHash, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ultv2.IntentHash, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ultv2.SignedIntentHash, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ultv2.Message, NpgsqlDbType.Jsonb, token);
                    await writer.WriteAsync(ultv2.RawPayload, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(ultv2.ManifestInstructions, NpgsqlDbType.Text, token);
                    await writer.WriteAsync(ultv2.ManifestClasses, "ledger_transaction_manifest_class[]", token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lt), lt, null);
            }
        });

    private Task<int> CopyLedgerTransactionEvents() => _context.WriteHelper.Copy(
        _ledgerTransactionEventsToAdd,
        "COPY ledger_transaction_events (state_version, receipt_event_emitters, receipt_event_emitter_entity_ids, receipt_event_names, receipt_event_sbors, receipt_event_schema_entity_ids, receipt_event_schema_hashes, receipt_event_type_indexes, receipt_event_sbor_type_kinds) FROM STDIN (FORMAT BINARY)",
        async (writer, lt, token) =>
        {
            await writer.WriteAsync(lt.StateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.ReceiptEventEmitters, NpgsqlDbType.Array | NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.ReceiptEventEmitterEntityIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.ReceiptEventNames, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
            await writer.WriteAsync(lt.ReceiptEventSbors, NpgsqlDbType.Array | NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(lt.ReceiptEventSchemaEntityIds, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.ReceiptEventSchemaHashes, NpgsqlDbType.Array | NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(lt.ReceiptEventTypeIndexes, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.ReceiptEventSborTypeKinds, "sbor_type_kind[]", token);
        });

    private Task<int> CopyLedgerSubintents() => _context.WriteHelper.Copy(
        _ledgerFinalizedSubintentsToAdd,
        "COPY ledger_finalized_subintents (subintent_hash, finalized_at_state_version, finalized_at_transaction_intent_hash) FROM STDIN (FORMAT BINARY)",
        async (writer, subintent, token) =>
        {
            await writer.WriteAsync(subintent.SubintentHash, NpgsqlDbType.Text, token);
            await writer.WriteAsync(subintent.FinalizedAtStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(subintent.FinalizedAtTransactionIntentHash, NpgsqlDbType.Text, token);
        });

    private Task<int> CopyLedgerTransactionSubintentData() => _context.WriteHelper.Copy(
        _subintentDataToAdd,
        "COPY ledger_transaction_subintent_data (state_version, child_subintent_hashes, subintent_data) FROM STDIN (FORMAT BINARY)",
        async (writer, subintent, token) =>
        {
            await writer.WriteAsync(subintent.StateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(subintent.ChildSubintentHashes, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
            await writer.WriteAsync(subintent.SubintentData, NpgsqlDbType.Jsonb, token);
        });
}
