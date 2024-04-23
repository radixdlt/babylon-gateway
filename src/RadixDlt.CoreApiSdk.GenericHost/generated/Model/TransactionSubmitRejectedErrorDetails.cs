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
    /// TransactionSubmitRejectedErrorDetails
    /// </summary>
    // moje start
    // moje end
public partial class TransactionSubmitRejectedErrorDetails : TransactionSubmitErrorDetails
{
    /// <summary>
        /// Initializes a new instance of the <see cref="TransactionSubmitRejectedErrorDetails" /> class.
        /// </summary>
        /// <param name="errorMessage">An explanation of the error</param>
        /// <param name="isFresh">Whether (true) this rejected status has just been calculated fresh, or (false) the status is from the pending transaction result cache. </param>
        /// <param name="isIntentRejectionPermanent">Whether the rejection of this intent is known to be permanent - this is a stronger statement than the payload rejection being permanent, as it implies any payloads containing the intent will also be permanently rejected. </param>
        /// <param name="isPayloadRejectionPermanent">Whether the rejection of this payload is known to be permanent. </param>
        /// <param name="type">type</param>
        /// <param name="invalidFromEpoch">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction will no longer be valid, and be permanently rejected. Only present if the rejection isn&#39;t permanent. </param>
        /// <param name="retryFromEpoch">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch after which the node will consider recalculating the validity of the transaction. Only present if the rejection is temporary due to a header specifying a \&quot;from epoch\&quot; in the future. </param>
        /// <param name="retryFromTimestamp">retryFromTimestamp</param>
        [JsonConstructor]
    public TransactionSubmitRejectedErrorDetails(string errorMessage, bool isFresh, bool isIntentRejectionPermanent, bool isPayloadRejectionPermanent, TransactionSubmitErrorDetailsType type, Option<long?> invalidFromEpoch = default, Option<long?> retryFromEpoch = default, Option<Instant?> retryFromTimestamp = default) : base(type)
    {
            ErrorMessage = errorMessage;
            IsFresh = isFresh;
            IsIntentRejectionPermanent = isIntentRejectionPermanent;
            IsPayloadRejectionPermanent = isPayloadRejectionPermanent;
            InvalidFromEpochOption = invalidFromEpoch;
            RetryFromEpochOption = retryFromEpoch;
            RetryFromTimestampOption = retryFromTimestamp;
    OnCreated();
    }

partial void OnCreated();

            /// <summary>
                /// An explanation of the error
                /// </summary>
                /// <value>An explanation of the error</value>
            [JsonPropertyName("error_message")]
            public string ErrorMessage { get; set; }

            /// <summary>
                /// Whether (true) this rejected status has just been calculated fresh, or (false) the status is from the pending transaction result cache. 
                /// </summary>
                /// <value>Whether (true) this rejected status has just been calculated fresh, or (false) the status is from the pending transaction result cache. </value>
            [JsonPropertyName("is_fresh")]
            public bool IsFresh { get; set; }

            /// <summary>
                /// Whether the rejection of this intent is known to be permanent - this is a stronger statement than the payload rejection being permanent, as it implies any payloads containing the intent will also be permanently rejected. 
                /// </summary>
                /// <value>Whether the rejection of this intent is known to be permanent - this is a stronger statement than the payload rejection being permanent, as it implies any payloads containing the intent will also be permanently rejected. </value>
            [JsonPropertyName("is_intent_rejection_permanent")]
            public bool IsIntentRejectionPermanent { get; set; }

            /// <summary>
                /// Whether the rejection of this payload is known to be permanent. 
                /// </summary>
                /// <value>Whether the rejection of this payload is known to be permanent. </value>
            [JsonPropertyName("is_payload_rejection_permanent")]
            public bool IsPayloadRejectionPermanent { get; set; }

                /// <summary>
                    /// Used to track the state of InvalidFromEpoch
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<long?> InvalidFromEpochOption { get; private set; }

            /// <summary>
                /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction will no longer be valid, and be permanently rejected. Only present if the rejection isn&#39;t permanent. 
                /// </summary>
                /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction will no longer be valid, and be permanently rejected. Only present if the rejection isn&#39;t permanent. </value>
            [JsonPropertyName("invalid_from_epoch")]
            public long? InvalidFromEpoch { get { return this.InvalidFromEpochOption; } set { this.InvalidFromEpochOption = new(value); } }

                /// <summary>
                    /// Used to track the state of RetryFromEpoch
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<long?> RetryFromEpochOption { get; private set; }

            /// <summary>
                /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch after which the node will consider recalculating the validity of the transaction. Only present if the rejection is temporary due to a header specifying a \&quot;from epoch\&quot; in the future. 
                /// </summary>
                /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch after which the node will consider recalculating the validity of the transaction. Only present if the rejection is temporary due to a header specifying a \&quot;from epoch\&quot; in the future. </value>
            [JsonPropertyName("retry_from_epoch")]
            public long? RetryFromEpoch { get { return this.RetryFromEpochOption; } set { this.RetryFromEpochOption = new(value); } }

                /// <summary>
                    /// Used to track the state of RetryFromTimestamp
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<Instant?> RetryFromTimestampOption { get; private set; }

            /// <summary>
                /// Gets or Sets RetryFromTimestamp
                /// </summary>
            [JsonPropertyName("retry_from_timestamp")]
            public Instant? RetryFromTimestamp { get { return this.RetryFromTimestampOption; } set { this.RetryFromTimestampOption = new(value); } }

/// <summary>
    /// Returns the string presentation of the object
    /// </summary>
/// <returns>String presentation of the object</returns>
public override string ToString()
{
StringBuilder sb = new StringBuilder();
sb.Append("class TransactionSubmitRejectedErrorDetails {\n");
    sb.Append("  ").Append(base.ToString()?.Replace("\n", "\n  ")).Append("\n");
    sb.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
    sb.Append("  IsFresh: ").Append(IsFresh).Append("\n");
    sb.Append("  IsIntentRejectionPermanent: ").Append(IsIntentRejectionPermanent).Append("\n");
    sb.Append("  IsPayloadRejectionPermanent: ").Append(IsPayloadRejectionPermanent).Append("\n");
    sb.Append("  InvalidFromEpoch: ").Append(InvalidFromEpoch).Append("\n");
    sb.Append("  RetryFromEpoch: ").Append(RetryFromEpoch).Append("\n");
    sb.Append("  RetryFromTimestamp: ").Append(RetryFromTimestamp).Append("\n");
sb.Append("}\n");
return sb.ToString();
}
}


    /// <summary>
    /// A Json converter for type <see cref="TransactionSubmitRejectedErrorDetails" />
    /// </summary>
    public class TransactionSubmitRejectedErrorDetailsJsonConverter : JsonConverter<TransactionSubmitRejectedErrorDetails>
    {
        /// <summary>
        /// Deserializes json to <see cref="TransactionSubmitRejectedErrorDetails" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override TransactionSubmitRejectedErrorDetails Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            Option<string?> errorMessage = default;
            Option<bool?> isFresh = default;
            Option<bool?> isIntentRejectionPermanent = default;
            Option<bool?> isPayloadRejectionPermanent = default;
            Option<TransactionSubmitErrorDetailsType?> type = default;
            Option<long?> invalidFromEpoch = default;
            Option<long?> retryFromEpoch = default;
            Option<Instant?> retryFromTimestamp = default;

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
                        case "error_message":
                            errorMessage = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        case "is_fresh":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                isFresh = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "is_intent_rejection_permanent":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                isIntentRejectionPermanent = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "is_payload_rejection_permanent":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                isPayloadRejectionPermanent = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "type":
                            string? typeRawValue = utf8JsonReader.GetString();
                            if (typeRawValue != null)
                                type = new Option<TransactionSubmitErrorDetailsType?>(TransactionSubmitErrorDetailsTypeValueConverter.FromStringOrDefault(typeRawValue));
                            break;
                        case "invalid_from_epoch":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                invalidFromEpoch = new Option<long?>(utf8JsonReader.GetInt64());
                            break;
                        case "retry_from_epoch":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                retryFromEpoch = new Option<long?>(utf8JsonReader.GetInt64());
                            break;
                        case "retry_from_timestamp":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                retryFromTimestamp = new Option<Instant?>(JsonSerializer.Deserialize<Instant>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!errorMessage.IsSet)
                throw new ArgumentException("Property is required for class TransactionSubmitRejectedErrorDetails.", nameof(errorMessage));

            if (!isFresh.IsSet)
                throw new ArgumentException("Property is required for class TransactionSubmitRejectedErrorDetails.", nameof(isFresh));

            if (!isIntentRejectionPermanent.IsSet)
                throw new ArgumentException("Property is required for class TransactionSubmitRejectedErrorDetails.", nameof(isIntentRejectionPermanent));

            if (!isPayloadRejectionPermanent.IsSet)
                throw new ArgumentException("Property is required for class TransactionSubmitRejectedErrorDetails.", nameof(isPayloadRejectionPermanent));

            if (!type.IsSet)
                throw new ArgumentException("Property is required for class TransactionSubmitRejectedErrorDetails.", nameof(type));

            if (errorMessage.IsSet && errorMessage.Value == null)
                throw new ArgumentNullException(nameof(errorMessage), "Property is not nullable for class TransactionSubmitRejectedErrorDetails.");

            if (isFresh.IsSet && isFresh.Value == null)
                throw new ArgumentNullException(nameof(isFresh), "Property is not nullable for class TransactionSubmitRejectedErrorDetails.");

            if (isIntentRejectionPermanent.IsSet && isIntentRejectionPermanent.Value == null)
                throw new ArgumentNullException(nameof(isIntentRejectionPermanent), "Property is not nullable for class TransactionSubmitRejectedErrorDetails.");

            if (isPayloadRejectionPermanent.IsSet && isPayloadRejectionPermanent.Value == null)
                throw new ArgumentNullException(nameof(isPayloadRejectionPermanent), "Property is not nullable for class TransactionSubmitRejectedErrorDetails.");

            if (type.IsSet && type.Value == null)
                throw new ArgumentNullException(nameof(type), "Property is not nullable for class TransactionSubmitRejectedErrorDetails.");

            if (invalidFromEpoch.IsSet && invalidFromEpoch.Value == null)
                throw new ArgumentNullException(nameof(invalidFromEpoch), "Property is not nullable for class TransactionSubmitRejectedErrorDetails.");

            if (retryFromEpoch.IsSet && retryFromEpoch.Value == null)
                throw new ArgumentNullException(nameof(retryFromEpoch), "Property is not nullable for class TransactionSubmitRejectedErrorDetails.");

            if (retryFromTimestamp.IsSet && retryFromTimestamp.Value == null)
                throw new ArgumentNullException(nameof(retryFromTimestamp), "Property is not nullable for class TransactionSubmitRejectedErrorDetails.");

            return new TransactionSubmitRejectedErrorDetails(errorMessage.Value!, isFresh.Value!.Value!, isIntentRejectionPermanent.Value!.Value!, isPayloadRejectionPermanent.Value!.Value!, type.Value!.Value!, invalidFromEpoch, retryFromEpoch, retryFromTimestamp);
        }

        /// <summary>
        /// Serializes a <see cref="TransactionSubmitRejectedErrorDetails" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="transactionSubmitRejectedErrorDetails"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, TransactionSubmitRejectedErrorDetails transactionSubmitRejectedErrorDetails, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(ref writer, transactionSubmitRejectedErrorDetails, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="TransactionSubmitRejectedErrorDetails" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="transactionSubmitRejectedErrorDetails"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(ref Utf8JsonWriter writer, TransactionSubmitRejectedErrorDetails transactionSubmitRejectedErrorDetails, JsonSerializerOptions jsonSerializerOptions)
        {
            if (transactionSubmitRejectedErrorDetails.ErrorMessage == null)
                throw new ArgumentNullException(nameof(transactionSubmitRejectedErrorDetails.ErrorMessage), "Property is required for class TransactionSubmitRejectedErrorDetails.");

            if (transactionSubmitRejectedErrorDetails.RetryFromTimestampOption.IsSet && transactionSubmitRejectedErrorDetails.RetryFromTimestamp == null)
                throw new ArgumentNullException(nameof(transactionSubmitRejectedErrorDetails.RetryFromTimestamp), "Property is required for class TransactionSubmitRejectedErrorDetails.");

            writer.WriteString("error_message", transactionSubmitRejectedErrorDetails.ErrorMessage);

            writer.WriteBoolean("is_fresh", transactionSubmitRejectedErrorDetails.IsFresh);

            writer.WriteBoolean("is_intent_rejection_permanent", transactionSubmitRejectedErrorDetails.IsIntentRejectionPermanent);

            writer.WriteBoolean("is_payload_rejection_permanent", transactionSubmitRejectedErrorDetails.IsPayloadRejectionPermanent);

            var typeRawValue = TransactionSubmitErrorDetailsTypeValueConverter.ToJsonValue(transactionSubmitRejectedErrorDetails.Type);
            writer.WriteString("type", typeRawValue);

            if (transactionSubmitRejectedErrorDetails.InvalidFromEpochOption.IsSet)
                writer.WriteNumber("invalid_from_epoch", transactionSubmitRejectedErrorDetails.InvalidFromEpochOption.Value!.Value);

            if (transactionSubmitRejectedErrorDetails.RetryFromEpochOption.IsSet)
                writer.WriteNumber("retry_from_epoch", transactionSubmitRejectedErrorDetails.RetryFromEpochOption.Value!.Value);

            if (transactionSubmitRejectedErrorDetails.RetryFromTimestampOption.IsSet)
            {
                writer.WritePropertyName("retry_from_timestamp");
                JsonSerializer.Serialize(writer, transactionSubmitRejectedErrorDetails.RetryFromTimestamp, jsonSerializerOptions);
            }
        }
    }
}
