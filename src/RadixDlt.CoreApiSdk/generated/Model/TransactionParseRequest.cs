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

/*
 * Radix Core API - Babylon
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.0.4
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// TransactionParseRequest
    /// </summary>
    [DataContract(Name = "TransactionParseRequest")]
    public partial class TransactionParseRequest : IEquatable<TransactionParseRequest>
    {
        /// <summary>
        /// The type of transaction payload that should be assumed. If omitted, \&quot;Any\&quot; is used - where the payload is attempted to be parsed as each of the following in turn: Notarized, Signed, Unsigned, Ledger. 
        /// </summary>
        /// <value>The type of transaction payload that should be assumed. If omitted, \&quot;Any\&quot; is used - where the payload is attempted to be parsed as each of the following in turn: Notarized, Signed, Unsigned, Ledger. </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ParseModeEnum
        {
            /// <summary>
            /// Enum Any for value: Any
            /// </summary>
            [EnumMember(Value = "Any")]
            Any = 1,

            /// <summary>
            /// Enum Notarized for value: Notarized
            /// </summary>
            [EnumMember(Value = "Notarized")]
            Notarized = 2,

            /// <summary>
            /// Enum Signed for value: Signed
            /// </summary>
            [EnumMember(Value = "Signed")]
            Signed = 3,

            /// <summary>
            /// Enum Unsigned for value: Unsigned
            /// </summary>
            [EnumMember(Value = "Unsigned")]
            Unsigned = 4,

            /// <summary>
            /// Enum Ledger for value: Ledger
            /// </summary>
            [EnumMember(Value = "Ledger")]
            Ledger = 5

        }


        /// <summary>
        /// The type of transaction payload that should be assumed. If omitted, \&quot;Any\&quot; is used - where the payload is attempted to be parsed as each of the following in turn: Notarized, Signed, Unsigned, Ledger. 
        /// </summary>
        /// <value>The type of transaction payload that should be assumed. If omitted, \&quot;Any\&quot; is used - where the payload is attempted to be parsed as each of the following in turn: Notarized, Signed, Unsigned, Ledger. </value>
        [DataMember(Name = "parse_mode", EmitDefaultValue = true)]
        public ParseModeEnum? ParseMode { get; set; }
        /// <summary>
        /// The type of validation that should be performed, if the payload correctly decompiles as a Notarized Transaction. This is only relevant for Notarized payloads. If omitted, \&quot;Static\&quot; is used. 
        /// </summary>
        /// <value>The type of validation that should be performed, if the payload correctly decompiles as a Notarized Transaction. This is only relevant for Notarized payloads. If omitted, \&quot;Static\&quot; is used. </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ValidationModeEnum
        {
            /// <summary>
            /// Enum None for value: None
            /// </summary>
            [EnumMember(Value = "None")]
            None = 1,

            /// <summary>
            /// Enum Static for value: Static
            /// </summary>
            [EnumMember(Value = "Static")]
            Static = 2,

            /// <summary>
            /// Enum Full for value: Full
            /// </summary>
            [EnumMember(Value = "Full")]
            Full = 3

        }


        /// <summary>
        /// The type of validation that should be performed, if the payload correctly decompiles as a Notarized Transaction. This is only relevant for Notarized payloads. If omitted, \&quot;Static\&quot; is used. 
        /// </summary>
        /// <value>The type of validation that should be performed, if the payload correctly decompiles as a Notarized Transaction. This is only relevant for Notarized payloads. If omitted, \&quot;Static\&quot; is used. </value>
        [DataMember(Name = "validation_mode", EmitDefaultValue = true)]
        public ValidationModeEnum? ValidationMode { get; set; }
        /// <summary>
        /// The amount of information to return in the response. \&quot;Basic\&quot; includes the type, validity information, and any relevant identifiers. \&quot;Full\&quot; also includes the fully parsed information. If omitted, \&quot;Full\&quot; is used. 
        /// </summary>
        /// <value>The amount of information to return in the response. \&quot;Basic\&quot; includes the type, validity information, and any relevant identifiers. \&quot;Full\&quot; also includes the fully parsed information. If omitted, \&quot;Full\&quot; is used. </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ResponseModeEnum
        {
            /// <summary>
            /// Enum Basic for value: Basic
            /// </summary>
            [EnumMember(Value = "Basic")]
            Basic = 1,

            /// <summary>
            /// Enum Full for value: Full
            /// </summary>
            [EnumMember(Value = "Full")]
            Full = 2

        }


        /// <summary>
        /// The amount of information to return in the response. \&quot;Basic\&quot; includes the type, validity information, and any relevant identifiers. \&quot;Full\&quot; also includes the fully parsed information. If omitted, \&quot;Full\&quot; is used. 
        /// </summary>
        /// <value>The amount of information to return in the response. \&quot;Basic\&quot; includes the type, validity information, and any relevant identifiers. \&quot;Full\&quot; also includes the fully parsed information. If omitted, \&quot;Full\&quot; is used. </value>
        [DataMember(Name = "response_mode", EmitDefaultValue = true)]
        public ResponseModeEnum? ResponseMode { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionParseRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TransactionParseRequest() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionParseRequest" /> class.
        /// </summary>
        /// <param name="network">The logical name of the network (required).</param>
        /// <param name="payloadHex">A hex-encoded payload of a full transaction or a partial transaction - either a notarized transaction, a signed transaction intent an unsigned transaction intent, or a ledger payload.  (required).</param>
        /// <param name="parseMode">The type of transaction payload that should be assumed. If omitted, \&quot;Any\&quot; is used - where the payload is attempted to be parsed as each of the following in turn: Notarized, Signed, Unsigned, Ledger. .</param>
        /// <param name="validationMode">The type of validation that should be performed, if the payload correctly decompiles as a Notarized Transaction. This is only relevant for Notarized payloads. If omitted, \&quot;Static\&quot; is used. .</param>
        /// <param name="responseMode">The amount of information to return in the response. \&quot;Basic\&quot; includes the type, validity information, and any relevant identifiers. \&quot;Full\&quot; also includes the fully parsed information. If omitted, \&quot;Full\&quot; is used. .</param>
        /// <param name="transactionFormatOptions">transactionFormatOptions.</param>
        public TransactionParseRequest(string network = default(string), string payloadHex = default(string), ParseModeEnum? parseMode = default(ParseModeEnum?), ValidationModeEnum? validationMode = default(ValidationModeEnum?), ResponseModeEnum? responseMode = default(ResponseModeEnum?), TransactionFormatOptions transactionFormatOptions = default(TransactionFormatOptions))
        {
            // to ensure "network" is required (not null)
            if (network == null)
            {
                throw new ArgumentNullException("network is a required property for TransactionParseRequest and cannot be null");
            }
            this.Network = network;
            // to ensure "payloadHex" is required (not null)
            if (payloadHex == null)
            {
                throw new ArgumentNullException("payloadHex is a required property for TransactionParseRequest and cannot be null");
            }
            this.PayloadHex = payloadHex;
            this.ParseMode = parseMode;
            this.ValidationMode = validationMode;
            this.ResponseMode = responseMode;
            this.TransactionFormatOptions = transactionFormatOptions;
        }

        /// <summary>
        /// The logical name of the network
        /// </summary>
        /// <value>The logical name of the network</value>
        [DataMember(Name = "network", IsRequired = true, EmitDefaultValue = true)]
        public string Network { get; set; }

        /// <summary>
        /// A hex-encoded payload of a full transaction or a partial transaction - either a notarized transaction, a signed transaction intent an unsigned transaction intent, or a ledger payload. 
        /// </summary>
        /// <value>A hex-encoded payload of a full transaction or a partial transaction - either a notarized transaction, a signed transaction intent an unsigned transaction intent, or a ledger payload. </value>
        [DataMember(Name = "payload_hex", IsRequired = true, EmitDefaultValue = true)]
        public string PayloadHex { get; set; }

        /// <summary>
        /// Gets or Sets TransactionFormatOptions
        /// </summary>
        [DataMember(Name = "transaction_format_options", EmitDefaultValue = true)]
        public TransactionFormatOptions TransactionFormatOptions { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionParseRequest {\n");
            sb.Append("  Network: ").Append(Network).Append("\n");
            sb.Append("  PayloadHex: ").Append(PayloadHex).Append("\n");
            sb.Append("  ParseMode: ").Append(ParseMode).Append("\n");
            sb.Append("  ValidationMode: ").Append(ValidationMode).Append("\n");
            sb.Append("  ResponseMode: ").Append(ResponseMode).Append("\n");
            sb.Append("  TransactionFormatOptions: ").Append(TransactionFormatOptions).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as TransactionParseRequest);
        }

        /// <summary>
        /// Returns true if TransactionParseRequest instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionParseRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionParseRequest input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Network == input.Network ||
                    (this.Network != null &&
                    this.Network.Equals(input.Network))
                ) && 
                (
                    this.PayloadHex == input.PayloadHex ||
                    (this.PayloadHex != null &&
                    this.PayloadHex.Equals(input.PayloadHex))
                ) && 
                (
                    this.ParseMode == input.ParseMode ||
                    this.ParseMode.Equals(input.ParseMode)
                ) && 
                (
                    this.ValidationMode == input.ValidationMode ||
                    this.ValidationMode.Equals(input.ValidationMode)
                ) && 
                (
                    this.ResponseMode == input.ResponseMode ||
                    this.ResponseMode.Equals(input.ResponseMode)
                ) && 
                (
                    this.TransactionFormatOptions == input.TransactionFormatOptions ||
                    (this.TransactionFormatOptions != null &&
                    this.TransactionFormatOptions.Equals(input.TransactionFormatOptions))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Network != null)
                {
                    hashCode = (hashCode * 59) + this.Network.GetHashCode();
                }
                if (this.PayloadHex != null)
                {
                    hashCode = (hashCode * 59) + this.PayloadHex.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.ParseMode.GetHashCode();
                hashCode = (hashCode * 59) + this.ValidationMode.GetHashCode();
                hashCode = (hashCode * 59) + this.ResponseMode.GetHashCode();
                if (this.TransactionFormatOptions != null)
                {
                    hashCode = (hashCode * 59) + this.TransactionFormatOptions.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
