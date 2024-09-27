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

using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

internal record TransactionReceiptEventLookup(long EntityId, ValueBytes SchemaHash);

internal record TransactionReceiptEvent(string Name, string Emitter, byte[] Data, long EntityId, byte[] SchemaHash, long TypeIndex, SborTypeKind KeyTypeKind);

/// <summary>
/// A transaction committed onto the radix ledger.
/// This table forms a shell, to which other properties are connected.
/// </summary>
[Table("ledger_transactions")]
internal abstract class LedgerTransaction
{
    private TransactionReceipt? _engineReceipt;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("state_version")]
    public long StateVersion { get; set; }

    [Column("epoch")]
    public long Epoch { get; set; }

    [Column("round_in_epoch")]
    public long RoundInEpoch { get; set; }

    [Column("index_in_epoch")]
    public long IndexInEpoch { get; set; }

    [Column("index_in_round")]
    public long IndexInRound { get; set; }

    [Column("fee_paid")]
    public TokenAmount FeePaid { get; set; }

    [Column("tip_paid")]
    public TokenAmount TipPaid { get; set; }

    [Column("affected_global_entities")]
    public long[] AffectedGlobalEntities { get; set; }

    /// <summary>
    /// The round timestamp of a round where vertex V was voted on is derived as the median of the timestamp of the
    /// votes on the vertex's QC to its parent vertex. These votes come from a subset of validators performing
    /// consensus. As a consequence of this, the round timestamp is not guaranteed to be increasing.
    /// </summary>
    [Column("round_timestamp")]
    public DateTime RoundTimestamp { get; set; }

    /// <summary>
    /// The time of the DataAggregator server when the LedgerTransaction was added to the service.
    /// </summary>
    [Column("created_timestamp")]
    public DateTime CreatedTimestamp { get; set; }

    /// <summary>
    /// This timestamp attempts to be "sensible" - ie increasing and semi-resistant to network time attacks.
    /// It calculates itself by clamping RoundTimestamp between the previous NormalizedTimestamp and CreatedTimestamp.
    /// Thus it ensures that NormalizedTimestamp is non-decreasing, and not after the ingest time.
    /// </summary>
    [Column("normalized_round_timestamp")]
    public DateTime NormalizedRoundTimestamp { get; set; }

    [Column("receipt_status")]
    public LedgerTransactionStatus ReceiptStatus { get; set; }

    [Column("receipt_fee_summary", TypeName = "jsonb")]
    public string ReceiptFeeSummary { get; set; }

    [Column("receipt_state_updates", TypeName = "jsonb")]
    public string ReceiptStateUpdates { get; set; }

    [Column("receipt_costing_parameters", TypeName = "jsonb")]
    public string ReceiptCostingParameters { get; set; }

    [Column("receipt_fee_source", TypeName = "jsonb")]
    public string? ReceiptFeeSource { get; set; }

    [Column("receipt_fee_destination", TypeName = "jsonb")]
    public string? ReceiptFeeDestination { get; set; }

    [Column("receipt_next_epoch", TypeName = "jsonb")]
    public string? ReceiptNextEpoch { get; set; }

    [Column("receipt_output", TypeName = "jsonb")]
    public string? ReceiptOutput { get; set; }

    [Column("receipt_error_message")]
    public string? ReceiptErrorMessage { get; set; }

    [Column("receipt_event_emitters", TypeName = "jsonb[]")]
    public string[] ReceiptEventEmitters { get; set; }

    [Column("receipt_event_names", TypeName = "text[]")]
    public string[] ReceiptEventNames { get; set; }

    [Column("receipt_event_sbors")]
    public byte[][] ReceiptEventSbors { get; set; }

    [Column("receipt_event_schema_entity_ids")]
    public long[] ReceiptEventSchemaEntityIds { get; set; }

    [Column("receipt_event_schema_hashes")]
    public byte[][] ReceiptEventSchemaHashes { get; set; }

    [Column("receipt_event_type_indexes")]
    public long[] ReceiptEventTypeIndexes { get; set; }

    [Column("receipt_event_sbor_type_kinds")]
    public SborTypeKind[] ReceiptEventSborTypeKinds { get; set; }

    [Column("balance_changes", TypeName = "jsonb")]
    public string? BalanceChanges { get; set; }

    [Column("transaction_tree_hash")]
    public string TransactionTreeHash { get; set; }

    [Column("receipt_tree_hash")]
    public string ReceiptTreeHash { get; set; }

    [Column("state_tree_hash")]
    public string StateTreeHash { get; set; }

    public TransactionReceipt EngineReceipt
    {
        get => _engineReceipt ??= new TransactionReceipt(this);
    }
}

internal class GenesisLedgerTransaction : LedgerTransaction
{
}

internal class UserLedgerTransaction : LedgerTransaction
{
    /// <summary>
    /// The transaction payload hash, also known as the notarized transaction hash (for user transactions).
    /// This shouldn't be used for user transaction tracking, because it could be mutated in transit.
    /// The intent hash should be used for tracking of user transactions.
    /// </summary>
    [Column("payload_hash")]
    public string PayloadHash { get; set; }

    /// <summary>
    /// The transaction intent hash. The engine ensures two transactions with the same intent hash cannot be committed.
    /// </summary>
    [Column("intent_hash")]
    public string IntentHash { get; set; }

    /// <summary>
    /// The hash of the signed transaction, which is what the notary signs.
    /// </summary>
    [Column("signed_intent_hash")]
    public string SignedIntentHash { get; set; }

    [Column("message", TypeName = "jsonb")]
    public string? Message { get; set; }

    /// <summary>
    /// The raw payload of the transaction.
    /// </summary>
    [Column("raw_payload")]
    public byte[] RawPayload { get; set; }

    [Column("manifest_instructions")]
    public string ManifestInstructions { get; set; }

    [Column("manifest_classes")]
    public LedgerTransactionManifestClass[] ManifestClasses { get; set; }
}

internal class RoundUpdateLedgerTransaction : LedgerTransaction
{
}

internal class FlashLedgerTransaction : LedgerTransaction
{
}

internal class TransactionReceipt
{
    private readonly LedgerTransaction _ledgerTransaction;

    public TransactionReceipt(LedgerTransaction ledgerTransaction)
    {
        _ledgerTransaction = ledgerTransaction;

        Events = new ReceiptEvents(ledgerTransaction);
    }

    public string? ErrorMessage => _ledgerTransaction.ReceiptErrorMessage;

    public LedgerTransactionStatus Status => _ledgerTransaction.ReceiptStatus;

    public string? Output => _ledgerTransaction.ReceiptOutput;

    public string FeeSummary => _ledgerTransaction.ReceiptFeeSummary;

    public string? FeeDestination => _ledgerTransaction.ReceiptFeeDestination;

    public string? FeeSource => _ledgerTransaction.ReceiptFeeSource;

    public string? NextEpoch => _ledgerTransaction.ReceiptNextEpoch;

    public string CostingParameters => _ledgerTransaction.ReceiptCostingParameters;

    public string StateUpdates => _ledgerTransaction.ReceiptStateUpdates;

    public ReceiptEvents Events { get; }
}

internal class ReceiptEvents
{
    private LedgerTransaction _ledgerTransaction;

    public ReceiptEvents(LedgerTransaction ledgerTransaction)
    {
        _ledgerTransaction = ledgerTransaction;
    }

    public string[] Emitters => _ledgerTransaction.ReceiptEventEmitters;

    public string[] Names => _ledgerTransaction.ReceiptEventNames;

    public byte[][] Sbors => _ledgerTransaction.ReceiptEventSbors;

    public long[] SchemaEntityIds => _ledgerTransaction.ReceiptEventSchemaEntityIds;

    public byte[][] SchemaHashes => _ledgerTransaction.ReceiptEventSchemaHashes;

    public long[] TypeIndexes => _ledgerTransaction.ReceiptEventTypeIndexes;

    public SborTypeKind[] SborTypeKinds => _ledgerTransaction.ReceiptEventSborTypeKinds;

    public List<TransactionReceiptEventLookup> GetEventLookups()
    {
        var result = new List<TransactionReceiptEventLookup>();

        for (var i = 0; i < _ledgerTransaction.ReceiptEventSbors.Length; ++i)
        {
            result.Add(new TransactionReceiptEventLookup(_ledgerTransaction.ReceiptEventSchemaEntityIds[i], _ledgerTransaction.ReceiptEventSchemaHashes[i]));
        }

        return result;
    }

    public List<TransactionReceiptEvent> GetEvents()
    {
        var result = new List<TransactionReceiptEvent>();

        for (var i = 0; i < _ledgerTransaction.ReceiptEventSbors.Length; ++i)
        {
            var eventData = _ledgerTransaction.ReceiptEventSbors[i];
            var entityId = _ledgerTransaction.ReceiptEventSchemaEntityIds[i];
            var schemaHash = _ledgerTransaction.ReceiptEventSchemaHashes[i];
            var index = _ledgerTransaction.ReceiptEventTypeIndexes[i];
            var typeKind = _ledgerTransaction.ReceiptEventSborTypeKinds[i];
            var emitter = _ledgerTransaction.ReceiptEventEmitters[i];
            var name = _ledgerTransaction.ReceiptEventNames[i];

            result.Add(new TransactionReceiptEvent(name, emitter, eventData, entityId, schemaHash, index, typeKind));
        }

        return result;
    }
}
