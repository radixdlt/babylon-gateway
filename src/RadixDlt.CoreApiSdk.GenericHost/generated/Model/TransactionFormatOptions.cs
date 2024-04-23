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

// <auto-generated>
/*
 * Radix Core API - Babylon
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.0.4
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using RadixDlt.CoreApiSdk.GenericHost.Client;

namespace RadixDlt.CoreApiSdk.GenericHost.Model
{

    /// <summary>
    /// Requested transaction formats to include in the response
    /// </summary>
    // moje start
    // moje end
public partial class TransactionFormatOptions
{
    /// <summary>
        /// Initializes a new instance of the <see cref="TransactionFormatOptions" /> class.
        /// </summary>
        /// <param name="balanceChanges">Whether to return the transaction balance changes (default false)</param>
        /// <param name="blobs">Whether to return the hex-encoded blobs (default false)</param>
        /// <param name="manifest">Whether to return the raw manifest (default true)</param>
        /// <param name="message">Whether to return the transaction message (default true)</param>
        /// <param name="rawLedgerTransaction">Whether to return the raw hex-encoded ledger transaction bytes (default false)</param>
        /// <param name="rawNotarizedTransaction">Whether to return the raw hex-encoded notarized transaction bytes (default true)</param>
        /// <param name="rawSystemTransaction">Whether to return the raw hex-encoded system transaction bytes (default false)</param>
        [JsonConstructor]
    public TransactionFormatOptions(Option<bool?> balanceChanges = default, Option<bool?> blobs = default, Option<bool?> manifest = default, Option<bool?> message = default, Option<bool?> rawLedgerTransaction = default, Option<bool?> rawNotarizedTransaction = default, Option<bool?> rawSystemTransaction = default)
    {
            BalanceChangesOption = balanceChanges;
            BlobsOption = blobs;
            ManifestOption = manifest;
            MessageOption = message;
            RawLedgerTransactionOption = rawLedgerTransaction;
            RawNotarizedTransactionOption = rawNotarizedTransaction;
            RawSystemTransactionOption = rawSystemTransaction;
    OnCreated();
    }

partial void OnCreated();

                /// <summary>
                    /// Used to track the state of BalanceChanges
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<bool?> BalanceChangesOption { get; private set; }

            /// <summary>
                /// Whether to return the transaction balance changes (default false)
                /// </summary>
                /// <value>Whether to return the transaction balance changes (default false)</value>
            [JsonPropertyName("balance_changes")]
            public bool? BalanceChanges { get { return this.BalanceChangesOption; } set { this.BalanceChangesOption = new(value); } }

                /// <summary>
                    /// Used to track the state of Blobs
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<bool?> BlobsOption { get; private set; }

            /// <summary>
                /// Whether to return the hex-encoded blobs (default false)
                /// </summary>
                /// <value>Whether to return the hex-encoded blobs (default false)</value>
            [JsonPropertyName("blobs")]
            public bool? Blobs { get { return this.BlobsOption; } set { this.BlobsOption = new(value); } }

                /// <summary>
                    /// Used to track the state of Manifest
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<bool?> ManifestOption { get; private set; }

            /// <summary>
                /// Whether to return the raw manifest (default true)
                /// </summary>
                /// <value>Whether to return the raw manifest (default true)</value>
            [JsonPropertyName("manifest")]
            public bool? Manifest { get { return this.ManifestOption; } set { this.ManifestOption = new(value); } }

                /// <summary>
                    /// Used to track the state of Message
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<bool?> MessageOption { get; private set; }

            /// <summary>
                /// Whether to return the transaction message (default true)
                /// </summary>
                /// <value>Whether to return the transaction message (default true)</value>
            [JsonPropertyName("message")]
            public bool? Message { get { return this.MessageOption; } set { this.MessageOption = new(value); } }

                /// <summary>
                    /// Used to track the state of RawLedgerTransaction
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<bool?> RawLedgerTransactionOption { get; private set; }

            /// <summary>
                /// Whether to return the raw hex-encoded ledger transaction bytes (default false)
                /// </summary>
                /// <value>Whether to return the raw hex-encoded ledger transaction bytes (default false)</value>
            [JsonPropertyName("raw_ledger_transaction")]
            public bool? RawLedgerTransaction { get { return this.RawLedgerTransactionOption; } set { this.RawLedgerTransactionOption = new(value); } }

                /// <summary>
                    /// Used to track the state of RawNotarizedTransaction
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<bool?> RawNotarizedTransactionOption { get; private set; }

            /// <summary>
                /// Whether to return the raw hex-encoded notarized transaction bytes (default true)
                /// </summary>
                /// <value>Whether to return the raw hex-encoded notarized transaction bytes (default true)</value>
            [JsonPropertyName("raw_notarized_transaction")]
            public bool? RawNotarizedTransaction { get { return this.RawNotarizedTransactionOption; } set { this.RawNotarizedTransactionOption = new(value); } }

                /// <summary>
                    /// Used to track the state of RawSystemTransaction
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<bool?> RawSystemTransactionOption { get; private set; }

            /// <summary>
                /// Whether to return the raw hex-encoded system transaction bytes (default false)
                /// </summary>
                /// <value>Whether to return the raw hex-encoded system transaction bytes (default false)</value>
            [JsonPropertyName("raw_system_transaction")]
            public bool? RawSystemTransaction { get { return this.RawSystemTransactionOption; } set { this.RawSystemTransactionOption = new(value); } }

/// <summary>
    /// Returns the string presentation of the object
    /// </summary>
/// <returns>String presentation of the object</returns>
public override string ToString()
{
StringBuilder sb = new StringBuilder();
sb.Append("class TransactionFormatOptions {\n");
    sb.Append("  BalanceChanges: ").Append(BalanceChanges).Append("\n");
    sb.Append("  Blobs: ").Append(Blobs).Append("\n");
    sb.Append("  Manifest: ").Append(Manifest).Append("\n");
    sb.Append("  Message: ").Append(Message).Append("\n");
    sb.Append("  RawLedgerTransaction: ").Append(RawLedgerTransaction).Append("\n");
    sb.Append("  RawNotarizedTransaction: ").Append(RawNotarizedTransaction).Append("\n");
    sb.Append("  RawSystemTransaction: ").Append(RawSystemTransaction).Append("\n");
sb.Append("}\n");
return sb.ToString();
}
}


    /// <summary>
    /// A Json converter for type <see cref="TransactionFormatOptions" />
    /// </summary>
    public class TransactionFormatOptionsJsonConverter : JsonConverter<TransactionFormatOptions>
    {
        /// <summary>
        /// Deserializes json to <see cref="TransactionFormatOptions" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override TransactionFormatOptions Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            Option<bool?> balanceChanges = default;
            Option<bool?> blobs = default;
            Option<bool?> manifest = default;
            Option<bool?> message = default;
            Option<bool?> rawLedgerTransaction = default;
            Option<bool?> rawNotarizedTransaction = default;
            Option<bool?> rawSystemTransaction = default;

            while (utf8JsonReader.Read())
            {
                if (startingTokenType == JsonTokenType.StartObject && utf8JsonReader.TokenType == JsonTokenType.EndObject && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (startingTokenType == JsonTokenType.StartArray && utf8JsonReader.TokenType == JsonTokenType.EndArray && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (utf8JsonReader.TokenType == JsonTokenType.PropertyName && currentDepth == utf8JsonReader.CurrentDepth - 1)
                {
                    string? localVarJsonPropertyName = utf8JsonReader.GetString();
                    utf8JsonReader.Read();

                    switch (localVarJsonPropertyName)
                    {
                        case "balance_changes":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                balanceChanges = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "blobs":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                blobs = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "manifest":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                manifest = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "message":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                message = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "raw_ledger_transaction":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                rawLedgerTransaction = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "raw_notarized_transaction":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                rawNotarizedTransaction = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "raw_system_transaction":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                rawSystemTransaction = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        default:
                            break;
                    }
                }
            }

            if (balanceChanges.IsSet && balanceChanges.Value == null)
                throw new ArgumentNullException(nameof(balanceChanges), "Property is not nullable for class TransactionFormatOptions.");

            if (blobs.IsSet && blobs.Value == null)
                throw new ArgumentNullException(nameof(blobs), "Property is not nullable for class TransactionFormatOptions.");

            if (manifest.IsSet && manifest.Value == null)
                throw new ArgumentNullException(nameof(manifest), "Property is not nullable for class TransactionFormatOptions.");

            if (message.IsSet && message.Value == null)
                throw new ArgumentNullException(nameof(message), "Property is not nullable for class TransactionFormatOptions.");

            if (rawLedgerTransaction.IsSet && rawLedgerTransaction.Value == null)
                throw new ArgumentNullException(nameof(rawLedgerTransaction), "Property is not nullable for class TransactionFormatOptions.");

            if (rawNotarizedTransaction.IsSet && rawNotarizedTransaction.Value == null)
                throw new ArgumentNullException(nameof(rawNotarizedTransaction), "Property is not nullable for class TransactionFormatOptions.");

            if (rawSystemTransaction.IsSet && rawSystemTransaction.Value == null)
                throw new ArgumentNullException(nameof(rawSystemTransaction), "Property is not nullable for class TransactionFormatOptions.");

            return new TransactionFormatOptions(balanceChanges, blobs, manifest, message, rawLedgerTransaction, rawNotarizedTransaction, rawSystemTransaction);
        }

        /// <summary>
        /// Serializes a <see cref="TransactionFormatOptions" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="transactionFormatOptions"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, TransactionFormatOptions transactionFormatOptions, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(ref writer, transactionFormatOptions, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="TransactionFormatOptions" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="transactionFormatOptions"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(ref Utf8JsonWriter writer, TransactionFormatOptions transactionFormatOptions, JsonSerializerOptions jsonSerializerOptions)
        {
            if (transactionFormatOptions.BalanceChangesOption.IsSet)
                writer.WriteBoolean("balance_changes", transactionFormatOptions.BalanceChangesOption.Value!.Value);

            if (transactionFormatOptions.BlobsOption.IsSet)
                writer.WriteBoolean("blobs", transactionFormatOptions.BlobsOption.Value!.Value);

            if (transactionFormatOptions.ManifestOption.IsSet)
                writer.WriteBoolean("manifest", transactionFormatOptions.ManifestOption.Value!.Value);

            if (transactionFormatOptions.MessageOption.IsSet)
                writer.WriteBoolean("message", transactionFormatOptions.MessageOption.Value!.Value);

            if (transactionFormatOptions.RawLedgerTransactionOption.IsSet)
                writer.WriteBoolean("raw_ledger_transaction", transactionFormatOptions.RawLedgerTransactionOption.Value!.Value);

            if (transactionFormatOptions.RawNotarizedTransactionOption.IsSet)
                writer.WriteBoolean("raw_notarized_transaction", transactionFormatOptions.RawNotarizedTransactionOption.Value!.Value);

            if (transactionFormatOptions.RawSystemTransactionOption.IsSet)
                writer.WriteBoolean("raw_system_transaction", transactionFormatOptions.RawSystemTransactionOption.Value!.Value);
        }
    }
}
