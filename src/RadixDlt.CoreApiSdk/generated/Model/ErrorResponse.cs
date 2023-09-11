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
 * Babylon Core API - RCnet v3.1
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  It is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  This version of the Core API belongs to the fourth release candidate of the Radix Babylon network (\"RCnet v3.1\").  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` are guaranteed to be forward compatible to Babylon mainnet launch (and beyond). We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code. 
 *
 * The version of the OpenAPI document: 0.5.0
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
using JsonSubTypes;
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// ErrorResponse
    /// </summary>
    [DataContract(Name = "ErrorResponse")]
    [JsonConverter(typeof(JsonSubtypes), "error_type")]
    [JsonSubtypes.KnownSubType(typeof(BasicErrorResponse), "Basic")]
    [JsonSubtypes.KnownSubType(typeof(BasicErrorResponse), "BasicErrorResponse")]
    [JsonSubtypes.KnownSubType(typeof(LtsTransactionSubmitErrorResponse), "LtsTransactionSubmit")]
    [JsonSubtypes.KnownSubType(typeof(LtsTransactionSubmitErrorResponse), "LtsTransactionSubmitErrorResponse")]
    [JsonSubtypes.KnownSubType(typeof(TransactionSubmitErrorResponse), "TransactionSubmit")]
    [JsonSubtypes.KnownSubType(typeof(TransactionSubmitErrorResponse), "TransactionSubmitErrorResponse")]
    public partial class ErrorResponse : IEquatable<ErrorResponse>
    {

        /// <summary>
        /// Gets or Sets ErrorType
        /// </summary>
        [DataMember(Name = "error_type", IsRequired = true, EmitDefaultValue = true)]
        public ErrorResponseType ErrorType { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ErrorResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponse" /> class.
        /// </summary>
        /// <param name="errorType">errorType (required).</param>
        /// <param name="code">A numeric code corresponding to the given HTTP error code. (required).</param>
        /// <param name="message">A human-readable error message. (required).</param>
        /// <param name="traceId">A GUID to be used when reporting errors, to allow correlation with the Core API&#39;s error logs, in the case where the Core API details are hidden..</param>
        public ErrorResponse(ErrorResponseType errorType = default(ErrorResponseType), int code = default(int), string message = default(string), string traceId = default(string))
        {
            this.ErrorType = errorType;
            this.Code = code;
            // to ensure "message" is required (not null)
            if (message == null)
            {
                throw new ArgumentNullException("message is a required property for ErrorResponse and cannot be null");
            }
            this.Message = message;
            this.TraceId = traceId;
        }

        /// <summary>
        /// A numeric code corresponding to the given HTTP error code.
        /// </summary>
        /// <value>A numeric code corresponding to the given HTTP error code.</value>
        [DataMember(Name = "code", IsRequired = true, EmitDefaultValue = true)]
        public int Code { get; set; }

        /// <summary>
        /// A human-readable error message.
        /// </summary>
        /// <value>A human-readable error message.</value>
        [DataMember(Name = "message", IsRequired = true, EmitDefaultValue = true)]
        public string Message { get; set; }

        /// <summary>
        /// A GUID to be used when reporting errors, to allow correlation with the Core API&#39;s error logs, in the case where the Core API details are hidden.
        /// </summary>
        /// <value>A GUID to be used when reporting errors, to allow correlation with the Core API&#39;s error logs, in the case where the Core API details are hidden.</value>
        [DataMember(Name = "trace_id", EmitDefaultValue = true)]
        public string TraceId { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ErrorResponse {\n");
            sb.Append("  ErrorType: ").Append(ErrorType).Append("\n");
            sb.Append("  Code: ").Append(Code).Append("\n");
            sb.Append("  Message: ").Append(Message).Append("\n");
            sb.Append("  TraceId: ").Append(TraceId).Append("\n");
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
            return this.Equals(input as ErrorResponse);
        }

        /// <summary>
        /// Returns true if ErrorResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of ErrorResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ErrorResponse input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.ErrorType == input.ErrorType ||
                    this.ErrorType.Equals(input.ErrorType)
                ) && 
                (
                    this.Code == input.Code ||
                    this.Code.Equals(input.Code)
                ) && 
                (
                    this.Message == input.Message ||
                    (this.Message != null &&
                    this.Message.Equals(input.Message))
                ) && 
                (
                    this.TraceId == input.TraceId ||
                    (this.TraceId != null &&
                    this.TraceId.Equals(input.TraceId))
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
                hashCode = (hashCode * 59) + this.ErrorType.GetHashCode();
                hashCode = (hashCode * 59) + this.Code.GetHashCode();
                if (this.Message != null)
                {
                    hashCode = (hashCode * 59) + this.Message.GetHashCode();
                }
                if (this.TraceId != null)
                {
                    hashCode = (hashCode * 59) + this.TraceId.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
