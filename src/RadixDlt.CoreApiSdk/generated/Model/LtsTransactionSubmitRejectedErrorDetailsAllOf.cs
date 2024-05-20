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
 * Radix Core API - Babylon (Bottlenose)
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.2.1
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
    /// Indicates that the transaction was executed and resulted in a rejection, therefore the transaction is not being added into the mempool. 
    /// </summary>
    [DataContract(Name = "LtsTransactionSubmitRejectedErrorDetails_allOf")]
    public partial class LtsTransactionSubmitRejectedErrorDetailsAllOf : IEquatable<LtsTransactionSubmitRejectedErrorDetailsAllOf>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LtsTransactionSubmitRejectedErrorDetailsAllOf" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected LtsTransactionSubmitRejectedErrorDetailsAllOf() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LtsTransactionSubmitRejectedErrorDetailsAllOf" /> class.
        /// </summary>
        /// <param name="errorMessage">An explanation of the error (required).</param>
        /// <param name="isFresh">Whether (true) this rejected status has just been calculated fresh, or (false) the status is from the pending transaction result cache.  (required).</param>
        /// <param name="isPayloadRejectionPermanent">Whether the rejection of this payload is known to be permanent.  (required).</param>
        /// <param name="isIntentRejectionPermanent">Whether the rejection of this intent is known to be permanent - this is a stronger statement than the payload rejection being permanent, as it implies any payloads containing the intent will also be permanently rejected.  (required).</param>
        /// <param name="retryFromTimestamp">retryFromTimestamp.</param>
        /// <param name="retryFromEpoch">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch after which the node will consider recalculating the validity of the transaction. Only present if the rejection is temporary due to a header specifying a \&quot;from epoch\&quot; in the future. .</param>
        /// <param name="invalidFromEpoch">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction will no longer be valid, and be permanently rejected. Only present if the rejection isn&#39;t permanent. .</param>
        public LtsTransactionSubmitRejectedErrorDetailsAllOf(string errorMessage = default(string), bool isFresh = default(bool), bool isPayloadRejectionPermanent = default(bool), bool isIntentRejectionPermanent = default(bool), InstantMs retryFromTimestamp = default(InstantMs), long retryFromEpoch = default(long), long invalidFromEpoch = default(long))
        {
            // to ensure "errorMessage" is required (not null)
            if (errorMessage == null)
            {
                throw new ArgumentNullException("errorMessage is a required property for LtsTransactionSubmitRejectedErrorDetailsAllOf and cannot be null");
            }
            this.ErrorMessage = errorMessage;
            this.IsFresh = isFresh;
            this.IsPayloadRejectionPermanent = isPayloadRejectionPermanent;
            this.IsIntentRejectionPermanent = isIntentRejectionPermanent;
            this.RetryFromTimestamp = retryFromTimestamp;
            this.RetryFromEpoch = retryFromEpoch;
            this.InvalidFromEpoch = invalidFromEpoch;
        }

        /// <summary>
        /// An explanation of the error
        /// </summary>
        /// <value>An explanation of the error</value>
        [DataMember(Name = "error_message", IsRequired = true, EmitDefaultValue = true)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Whether (true) this rejected status has just been calculated fresh, or (false) the status is from the pending transaction result cache. 
        /// </summary>
        /// <value>Whether (true) this rejected status has just been calculated fresh, or (false) the status is from the pending transaction result cache. </value>
        [DataMember(Name = "is_fresh", IsRequired = true, EmitDefaultValue = true)]
        public bool IsFresh { get; set; }

        /// <summary>
        /// Whether the rejection of this payload is known to be permanent. 
        /// </summary>
        /// <value>Whether the rejection of this payload is known to be permanent. </value>
        [DataMember(Name = "is_payload_rejection_permanent", IsRequired = true, EmitDefaultValue = true)]
        public bool IsPayloadRejectionPermanent { get; set; }

        /// <summary>
        /// Whether the rejection of this intent is known to be permanent - this is a stronger statement than the payload rejection being permanent, as it implies any payloads containing the intent will also be permanently rejected. 
        /// </summary>
        /// <value>Whether the rejection of this intent is known to be permanent - this is a stronger statement than the payload rejection being permanent, as it implies any payloads containing the intent will also be permanently rejected. </value>
        [DataMember(Name = "is_intent_rejection_permanent", IsRequired = true, EmitDefaultValue = true)]
        public bool IsIntentRejectionPermanent { get; set; }

        /// <summary>
        /// Gets or Sets RetryFromTimestamp
        /// </summary>
        [DataMember(Name = "retry_from_timestamp", EmitDefaultValue = true)]
        public InstantMs RetryFromTimestamp { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch after which the node will consider recalculating the validity of the transaction. Only present if the rejection is temporary due to a header specifying a \&quot;from epoch\&quot; in the future. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch after which the node will consider recalculating the validity of the transaction. Only present if the rejection is temporary due to a header specifying a \&quot;from epoch\&quot; in the future. </value>
        [DataMember(Name = "retry_from_epoch", EmitDefaultValue = true)]
        public long RetryFromEpoch { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction will no longer be valid, and be permanently rejected. Only present if the rejection isn&#39;t permanent. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction will no longer be valid, and be permanently rejected. Only present if the rejection isn&#39;t permanent. </value>
        [DataMember(Name = "invalid_from_epoch", EmitDefaultValue = true)]
        public long InvalidFromEpoch { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class LtsTransactionSubmitRejectedErrorDetailsAllOf {\n");
            sb.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
            sb.Append("  IsFresh: ").Append(IsFresh).Append("\n");
            sb.Append("  IsPayloadRejectionPermanent: ").Append(IsPayloadRejectionPermanent).Append("\n");
            sb.Append("  IsIntentRejectionPermanent: ").Append(IsIntentRejectionPermanent).Append("\n");
            sb.Append("  RetryFromTimestamp: ").Append(RetryFromTimestamp).Append("\n");
            sb.Append("  RetryFromEpoch: ").Append(RetryFromEpoch).Append("\n");
            sb.Append("  InvalidFromEpoch: ").Append(InvalidFromEpoch).Append("\n");
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
            return this.Equals(input as LtsTransactionSubmitRejectedErrorDetailsAllOf);
        }

        /// <summary>
        /// Returns true if LtsTransactionSubmitRejectedErrorDetailsAllOf instances are equal
        /// </summary>
        /// <param name="input">Instance of LtsTransactionSubmitRejectedErrorDetailsAllOf to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(LtsTransactionSubmitRejectedErrorDetailsAllOf input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.ErrorMessage == input.ErrorMessage ||
                    (this.ErrorMessage != null &&
                    this.ErrorMessage.Equals(input.ErrorMessage))
                ) && 
                (
                    this.IsFresh == input.IsFresh ||
                    this.IsFresh.Equals(input.IsFresh)
                ) && 
                (
                    this.IsPayloadRejectionPermanent == input.IsPayloadRejectionPermanent ||
                    this.IsPayloadRejectionPermanent.Equals(input.IsPayloadRejectionPermanent)
                ) && 
                (
                    this.IsIntentRejectionPermanent == input.IsIntentRejectionPermanent ||
                    this.IsIntentRejectionPermanent.Equals(input.IsIntentRejectionPermanent)
                ) && 
                (
                    this.RetryFromTimestamp == input.RetryFromTimestamp ||
                    (this.RetryFromTimestamp != null &&
                    this.RetryFromTimestamp.Equals(input.RetryFromTimestamp))
                ) && 
                (
                    this.RetryFromEpoch == input.RetryFromEpoch ||
                    this.RetryFromEpoch.Equals(input.RetryFromEpoch)
                ) && 
                (
                    this.InvalidFromEpoch == input.InvalidFromEpoch ||
                    this.InvalidFromEpoch.Equals(input.InvalidFromEpoch)
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
                if (this.ErrorMessage != null)
                {
                    hashCode = (hashCode * 59) + this.ErrorMessage.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.IsFresh.GetHashCode();
                hashCode = (hashCode * 59) + this.IsPayloadRejectionPermanent.GetHashCode();
                hashCode = (hashCode * 59) + this.IsIntentRejectionPermanent.GetHashCode();
                if (this.RetryFromTimestamp != null)
                {
                    hashCode = (hashCode * 59) + this.RetryFromTimestamp.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.RetryFromEpoch.GetHashCode();
                hashCode = (hashCode * 59) + this.InvalidFromEpoch.GetHashCode();
                return hashCode;
            }
        }

    }

}
