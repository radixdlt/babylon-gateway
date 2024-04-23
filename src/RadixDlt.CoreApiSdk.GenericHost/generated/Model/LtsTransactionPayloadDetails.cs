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
    /// LtsTransactionPayloadDetails
    /// </summary>
    // moje start
    // moje end
public partial class LtsTransactionPayloadDetails
{
    /// <summary>
        /// Initializes a new instance of the <see cref="LtsTransactionPayloadDetails" /> class.
        /// </summary>
        /// <param name="payloadHash">The hex-encoded notarized transaction hash for a user transaction. This hash identifies the full submittable notarized transaction - ie the signed intent, plus the notary signature. </param>
        /// <param name="payloadHashBech32m">The Bech32m-encoded human readable &#x60;NotarizedTransactionHash&#x60;.</param>
        /// <param name="status">status</param>
        /// <param name="errorMessage">An explanation for the error, if failed or rejected</param>
        /// <param name="stateVersion">stateVersion</param>
        [JsonConstructor]
    public LtsTransactionPayloadDetails(string payloadHash, string payloadHashBech32m, LtsTransactionPayloadStatus status, Option<string?> errorMessage = default, Option<long?> stateVersion = default)
    {
            PayloadHash = payloadHash;
            PayloadHashBech32m = payloadHashBech32m;
            Status = status;
            ErrorMessageOption = errorMessage;
            StateVersionOption = stateVersion;
    OnCreated();
    }

partial void OnCreated();

        /// <summary>
            /// Gets or Sets Status
            /// </summary>
        [JsonPropertyName("status")]
        public LtsTransactionPayloadStatus Status { get; set; }

            /// <summary>
                /// The hex-encoded notarized transaction hash for a user transaction. This hash identifies the full submittable notarized transaction - ie the signed intent, plus the notary signature. 
                /// </summary>
                /// <value>The hex-encoded notarized transaction hash for a user transaction. This hash identifies the full submittable notarized transaction - ie the signed intent, plus the notary signature. </value>
            [JsonPropertyName("payload_hash")]
            public string PayloadHash { get; set; }

            /// <summary>
                /// The Bech32m-encoded human readable &#x60;NotarizedTransactionHash&#x60;.
                /// </summary>
                /// <value>The Bech32m-encoded human readable &#x60;NotarizedTransactionHash&#x60;.</value>
            [JsonPropertyName("payload_hash_bech32m")]
            public string PayloadHashBech32m { get; set; }

                /// <summary>
                    /// Used to track the state of ErrorMessage
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<string?> ErrorMessageOption { get; private set; }

            /// <summary>
                /// An explanation for the error, if failed or rejected
                /// </summary>
                /// <value>An explanation for the error, if failed or rejected</value>
            [JsonPropertyName("error_message")]
            public string? ErrorMessage { get { return this.ErrorMessageOption; } set { this.ErrorMessageOption = new(value); } }

                /// <summary>
                    /// Used to track the state of StateVersion
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<long?> StateVersionOption { get; private set; }

            /// <summary>
                /// Gets or Sets StateVersion
                /// </summary>
            [JsonPropertyName("state_version")]
            public long? StateVersion { get { return this.StateVersionOption; } set { this.StateVersionOption = new(value); } }

/// <summary>
    /// Returns the string presentation of the object
    /// </summary>
/// <returns>String presentation of the object</returns>
public override string ToString()
{
StringBuilder sb = new StringBuilder();
sb.Append("class LtsTransactionPayloadDetails {\n");
    sb.Append("  PayloadHash: ").Append(PayloadHash).Append("\n");
    sb.Append("  PayloadHashBech32m: ").Append(PayloadHashBech32m).Append("\n");
    sb.Append("  Status: ").Append(Status).Append("\n");
    sb.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
    sb.Append("  StateVersion: ").Append(StateVersion).Append("\n");
sb.Append("}\n");
return sb.ToString();
}
}


    /// <summary>
    /// A Json converter for type <see cref="LtsTransactionPayloadDetails" />
    /// </summary>
    public class LtsTransactionPayloadDetailsJsonConverter : JsonConverter<LtsTransactionPayloadDetails>
    {
        /// <summary>
        /// Deserializes json to <see cref="LtsTransactionPayloadDetails" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override LtsTransactionPayloadDetails Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            Option<string?> payloadHash = default;
            Option<string?> payloadHashBech32m = default;
            Option<LtsTransactionPayloadStatus?> status = default;
            Option<string?> errorMessage = default;
            Option<long?> stateVersion = default;

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
                        case "payload_hash":
                            payloadHash = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        case "payload_hash_bech32m":
                            payloadHashBech32m = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        case "status":
                            string? statusRawValue = utf8JsonReader.GetString();
                            if (statusRawValue != null)
                                status = new Option<LtsTransactionPayloadStatus?>(LtsTransactionPayloadStatusValueConverter.FromStringOrDefault(statusRawValue));
                            break;
                        case "error_message":
                            errorMessage = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        case "state_version":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                stateVersion = new Option<long?>(utf8JsonReader.GetInt64());
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!payloadHash.IsSet)
                throw new ArgumentException("Property is required for class LtsTransactionPayloadDetails.", nameof(payloadHash));

            if (!payloadHashBech32m.IsSet)
                throw new ArgumentException("Property is required for class LtsTransactionPayloadDetails.", nameof(payloadHashBech32m));

            if (!status.IsSet)
                throw new ArgumentException("Property is required for class LtsTransactionPayloadDetails.", nameof(status));

            if (payloadHash.IsSet && payloadHash.Value == null)
                throw new ArgumentNullException(nameof(payloadHash), "Property is not nullable for class LtsTransactionPayloadDetails.");

            if (payloadHashBech32m.IsSet && payloadHashBech32m.Value == null)
                throw new ArgumentNullException(nameof(payloadHashBech32m), "Property is not nullable for class LtsTransactionPayloadDetails.");

            if (status.IsSet && status.Value == null)
                throw new ArgumentNullException(nameof(status), "Property is not nullable for class LtsTransactionPayloadDetails.");

            if (errorMessage.IsSet && errorMessage.Value == null)
                throw new ArgumentNullException(nameof(errorMessage), "Property is not nullable for class LtsTransactionPayloadDetails.");

            if (stateVersion.IsSet && stateVersion.Value == null)
                throw new ArgumentNullException(nameof(stateVersion), "Property is not nullable for class LtsTransactionPayloadDetails.");

            return new LtsTransactionPayloadDetails(payloadHash.Value!, payloadHashBech32m.Value!, status.Value!.Value!, errorMessage, stateVersion);
        }

        /// <summary>
        /// Serializes a <see cref="LtsTransactionPayloadDetails" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="ltsTransactionPayloadDetails"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, LtsTransactionPayloadDetails ltsTransactionPayloadDetails, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(ref writer, ltsTransactionPayloadDetails, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="LtsTransactionPayloadDetails" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="ltsTransactionPayloadDetails"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(ref Utf8JsonWriter writer, LtsTransactionPayloadDetails ltsTransactionPayloadDetails, JsonSerializerOptions jsonSerializerOptions)
        {
            if (ltsTransactionPayloadDetails.PayloadHash == null)
                throw new ArgumentNullException(nameof(ltsTransactionPayloadDetails.PayloadHash), "Property is required for class LtsTransactionPayloadDetails.");

            if (ltsTransactionPayloadDetails.PayloadHashBech32m == null)
                throw new ArgumentNullException(nameof(ltsTransactionPayloadDetails.PayloadHashBech32m), "Property is required for class LtsTransactionPayloadDetails.");

            if (ltsTransactionPayloadDetails.ErrorMessageOption.IsSet && ltsTransactionPayloadDetails.ErrorMessage == null)
                throw new ArgumentNullException(nameof(ltsTransactionPayloadDetails.ErrorMessage), "Property is required for class LtsTransactionPayloadDetails.");

            writer.WriteString("payload_hash", ltsTransactionPayloadDetails.PayloadHash);

            writer.WriteString("payload_hash_bech32m", ltsTransactionPayloadDetails.PayloadHashBech32m);

            var statusRawValue = LtsTransactionPayloadStatusValueConverter.ToJsonValue(ltsTransactionPayloadDetails.Status);
            writer.WriteString("status", statusRawValue);

            if (ltsTransactionPayloadDetails.ErrorMessageOption.IsSet)
                writer.WriteString("error_message", ltsTransactionPayloadDetails.ErrorMessage);

            if (ltsTransactionPayloadDetails.StateVersionOption.IsSet)
                writer.WriteNumber("state_version", ltsTransactionPayloadDetails.StateVersionOption.Value!.Value);
        }
    }
}
