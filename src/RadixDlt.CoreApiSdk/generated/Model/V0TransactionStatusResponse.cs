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
 * Babylon Core API
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: 0.1.0
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
    /// V0TransactionStatusResponse
    /// </summary>
    [DataContract(Name = "V0TransactionStatusResponse")]
    public partial class V0TransactionStatusResponse : IEquatable<V0TransactionStatusResponse>
    {
        /// <summary>
        /// The status of the transaction intent
        /// </summary>
        /// <value>The status of the transaction intent</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum IntentStatusEnum
        {
            /// <summary>
            /// Enum CommittedSuccess for value: CommittedSuccess
            /// </summary>
            [EnumMember(Value = "CommittedSuccess")]
            CommittedSuccess = 1,

            /// <summary>
            /// Enum CommittedFailure for value: CommittedFailure
            /// </summary>
            [EnumMember(Value = "CommittedFailure")]
            CommittedFailure = 2,

            /// <summary>
            /// Enum InMempool for value: InMempool
            /// </summary>
            [EnumMember(Value = "InMempool")]
            InMempool = 3,

            /// <summary>
            /// Enum Rejected for value: Rejected
            /// </summary>
            [EnumMember(Value = "Rejected")]
            Rejected = 4,

            /// <summary>
            /// Enum Unknown for value: Unknown
            /// </summary>
            [EnumMember(Value = "Unknown")]
            Unknown = 5

        }


        /// <summary>
        /// The status of the transaction intent
        /// </summary>
        /// <value>The status of the transaction intent</value>
        [DataMember(Name = "intent_status", IsRequired = true, EmitDefaultValue = true)]
        public IntentStatusEnum IntentStatus { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="V0TransactionStatusResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected V0TransactionStatusResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="V0TransactionStatusResponse" /> class.
        /// </summary>
        /// <param name="intentStatus">The status of the transaction intent (required).</param>
        /// <param name="knownPayloads">knownPayloads (required).</param>
        public V0TransactionStatusResponse(IntentStatusEnum intentStatus = default(IntentStatusEnum), List<V0TransactionPayloadStatus> knownPayloads = default(List<V0TransactionPayloadStatus>))
        {
            this.IntentStatus = intentStatus;
            // to ensure "knownPayloads" is required (not null)
            if (knownPayloads == null)
            {
                throw new ArgumentNullException("knownPayloads is a required property for V0TransactionStatusResponse and cannot be null");
            }
            this.KnownPayloads = knownPayloads;
        }

        /// <summary>
        /// Gets or Sets KnownPayloads
        /// </summary>
        [DataMember(Name = "known_payloads", IsRequired = true, EmitDefaultValue = true)]
        public List<V0TransactionPayloadStatus> KnownPayloads { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class V0TransactionStatusResponse {\n");
            sb.Append("  IntentStatus: ").Append(IntentStatus).Append("\n");
            sb.Append("  KnownPayloads: ").Append(KnownPayloads).Append("\n");
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
            return this.Equals(input as V0TransactionStatusResponse);
        }

        /// <summary>
        /// Returns true if V0TransactionStatusResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of V0TransactionStatusResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(V0TransactionStatusResponse input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.IntentStatus == input.IntentStatus ||
                    this.IntentStatus.Equals(input.IntentStatus)
                ) && 
                (
                    this.KnownPayloads == input.KnownPayloads ||
                    this.KnownPayloads != null &&
                    input.KnownPayloads != null &&
                    this.KnownPayloads.SequenceEqual(input.KnownPayloads)
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
                hashCode = (hashCode * 59) + this.IntentStatus.GetHashCode();
                if (this.KnownPayloads != null)
                {
                    hashCode = (hashCode * 59) + this.KnownPayloads.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}