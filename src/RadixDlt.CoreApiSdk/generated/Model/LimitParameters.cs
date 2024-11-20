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
 * Radix Core API
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.3.0
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
    /// LimitParameters
    /// </summary>
    [DataContract(Name = "LimitParameters")]
    public partial class LimitParameters : IEquatable<LimitParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LimitParameters" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected LimitParameters() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LimitParameters" /> class.
        /// </summary>
        /// <param name="maxCallDepth">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum call depth allowed during transaction execution.  (required).</param>
        /// <param name="maxHeapSubstateTotalBytes">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of all substates kept on the heap during a single transaction&#39;s execution.  (required).</param>
        /// <param name="maxTrackSubstateTotalBytes">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of all substates kept in the track during a single transaction&#39;s execution.  (required).</param>
        /// <param name="maxSubstateKeySize">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a Substate&#39;s key in the low-level Substate database.  (required).</param>
        /// <param name="maxSubstateValueSize">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a Substate&#39;s value in the low-level Substate database.  (required).</param>
        /// <param name="maxInvokeInputSize">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single call&#39;s input parameters.  (required).</param>
        /// <param name="maxEventSize">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single emitted event.  (required).</param>
        /// <param name="maxLogSize">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single logged line.  (required).</param>
        /// <param name="maxPanicMessageSize">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single panic message.  (required).</param>
        /// <param name="maxNumberOfLogs">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum count of log lines emitted during a single transaction&#39;s execution.  (required).</param>
        /// <param name="maxNumberOfEvents">A decimal string-encoded 64-bit unsigned integer, representing the configured maximum count of events emitted during a single transaction&#39;s execution.  (required).</param>
        public LimitParameters(string maxCallDepth = default(string), string maxHeapSubstateTotalBytes = default(string), string maxTrackSubstateTotalBytes = default(string), string maxSubstateKeySize = default(string), string maxSubstateValueSize = default(string), string maxInvokeInputSize = default(string), string maxEventSize = default(string), string maxLogSize = default(string), string maxPanicMessageSize = default(string), string maxNumberOfLogs = default(string), string maxNumberOfEvents = default(string))
        {
            // to ensure "maxCallDepth" is required (not null)
            if (maxCallDepth == null)
            {
                throw new ArgumentNullException("maxCallDepth is a required property for LimitParameters and cannot be null");
            }
            this.MaxCallDepth = maxCallDepth;
            // to ensure "maxHeapSubstateTotalBytes" is required (not null)
            if (maxHeapSubstateTotalBytes == null)
            {
                throw new ArgumentNullException("maxHeapSubstateTotalBytes is a required property for LimitParameters and cannot be null");
            }
            this.MaxHeapSubstateTotalBytes = maxHeapSubstateTotalBytes;
            // to ensure "maxTrackSubstateTotalBytes" is required (not null)
            if (maxTrackSubstateTotalBytes == null)
            {
                throw new ArgumentNullException("maxTrackSubstateTotalBytes is a required property for LimitParameters and cannot be null");
            }
            this.MaxTrackSubstateTotalBytes = maxTrackSubstateTotalBytes;
            // to ensure "maxSubstateKeySize" is required (not null)
            if (maxSubstateKeySize == null)
            {
                throw new ArgumentNullException("maxSubstateKeySize is a required property for LimitParameters and cannot be null");
            }
            this.MaxSubstateKeySize = maxSubstateKeySize;
            // to ensure "maxSubstateValueSize" is required (not null)
            if (maxSubstateValueSize == null)
            {
                throw new ArgumentNullException("maxSubstateValueSize is a required property for LimitParameters and cannot be null");
            }
            this.MaxSubstateValueSize = maxSubstateValueSize;
            // to ensure "maxInvokeInputSize" is required (not null)
            if (maxInvokeInputSize == null)
            {
                throw new ArgumentNullException("maxInvokeInputSize is a required property for LimitParameters and cannot be null");
            }
            this.MaxInvokeInputSize = maxInvokeInputSize;
            // to ensure "maxEventSize" is required (not null)
            if (maxEventSize == null)
            {
                throw new ArgumentNullException("maxEventSize is a required property for LimitParameters and cannot be null");
            }
            this.MaxEventSize = maxEventSize;
            // to ensure "maxLogSize" is required (not null)
            if (maxLogSize == null)
            {
                throw new ArgumentNullException("maxLogSize is a required property for LimitParameters and cannot be null");
            }
            this.MaxLogSize = maxLogSize;
            // to ensure "maxPanicMessageSize" is required (not null)
            if (maxPanicMessageSize == null)
            {
                throw new ArgumentNullException("maxPanicMessageSize is a required property for LimitParameters and cannot be null");
            }
            this.MaxPanicMessageSize = maxPanicMessageSize;
            // to ensure "maxNumberOfLogs" is required (not null)
            if (maxNumberOfLogs == null)
            {
                throw new ArgumentNullException("maxNumberOfLogs is a required property for LimitParameters and cannot be null");
            }
            this.MaxNumberOfLogs = maxNumberOfLogs;
            // to ensure "maxNumberOfEvents" is required (not null)
            if (maxNumberOfEvents == null)
            {
                throw new ArgumentNullException("maxNumberOfEvents is a required property for LimitParameters and cannot be null");
            }
            this.MaxNumberOfEvents = maxNumberOfEvents;
        }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum call depth allowed during transaction execution. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum call depth allowed during transaction execution. </value>
        [DataMember(Name = "max_call_depth", IsRequired = true, EmitDefaultValue = true)]
        public string MaxCallDepth { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of all substates kept on the heap during a single transaction&#39;s execution. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of all substates kept on the heap during a single transaction&#39;s execution. </value>
        [DataMember(Name = "max_heap_substate_total_bytes", IsRequired = true, EmitDefaultValue = true)]
        public string MaxHeapSubstateTotalBytes { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of all substates kept in the track during a single transaction&#39;s execution. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of all substates kept in the track during a single transaction&#39;s execution. </value>
        [DataMember(Name = "max_track_substate_total_bytes", IsRequired = true, EmitDefaultValue = true)]
        public string MaxTrackSubstateTotalBytes { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a Substate&#39;s key in the low-level Substate database. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a Substate&#39;s key in the low-level Substate database. </value>
        [DataMember(Name = "max_substate_key_size", IsRequired = true, EmitDefaultValue = true)]
        public string MaxSubstateKeySize { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a Substate&#39;s value in the low-level Substate database. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a Substate&#39;s value in the low-level Substate database. </value>
        [DataMember(Name = "max_substate_value_size", IsRequired = true, EmitDefaultValue = true)]
        public string MaxSubstateValueSize { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single call&#39;s input parameters. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single call&#39;s input parameters. </value>
        [DataMember(Name = "max_invoke_input_size", IsRequired = true, EmitDefaultValue = true)]
        public string MaxInvokeInputSize { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single emitted event. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single emitted event. </value>
        [DataMember(Name = "max_event_size", IsRequired = true, EmitDefaultValue = true)]
        public string MaxEventSize { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single logged line. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single logged line. </value>
        [DataMember(Name = "max_log_size", IsRequired = true, EmitDefaultValue = true)]
        public string MaxLogSize { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single panic message. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum byte size of a single panic message. </value>
        [DataMember(Name = "max_panic_message_size", IsRequired = true, EmitDefaultValue = true)]
        public string MaxPanicMessageSize { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum count of log lines emitted during a single transaction&#39;s execution. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum count of log lines emitted during a single transaction&#39;s execution. </value>
        [DataMember(Name = "max_number_of_logs", IsRequired = true, EmitDefaultValue = true)]
        public string MaxNumberOfLogs { get; set; }

        /// <summary>
        /// A decimal string-encoded 64-bit unsigned integer, representing the configured maximum count of events emitted during a single transaction&#39;s execution. 
        /// </summary>
        /// <value>A decimal string-encoded 64-bit unsigned integer, representing the configured maximum count of events emitted during a single transaction&#39;s execution. </value>
        [DataMember(Name = "max_number_of_events", IsRequired = true, EmitDefaultValue = true)]
        public string MaxNumberOfEvents { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class LimitParameters {\n");
            sb.Append("  MaxCallDepth: ").Append(MaxCallDepth).Append("\n");
            sb.Append("  MaxHeapSubstateTotalBytes: ").Append(MaxHeapSubstateTotalBytes).Append("\n");
            sb.Append("  MaxTrackSubstateTotalBytes: ").Append(MaxTrackSubstateTotalBytes).Append("\n");
            sb.Append("  MaxSubstateKeySize: ").Append(MaxSubstateKeySize).Append("\n");
            sb.Append("  MaxSubstateValueSize: ").Append(MaxSubstateValueSize).Append("\n");
            sb.Append("  MaxInvokeInputSize: ").Append(MaxInvokeInputSize).Append("\n");
            sb.Append("  MaxEventSize: ").Append(MaxEventSize).Append("\n");
            sb.Append("  MaxLogSize: ").Append(MaxLogSize).Append("\n");
            sb.Append("  MaxPanicMessageSize: ").Append(MaxPanicMessageSize).Append("\n");
            sb.Append("  MaxNumberOfLogs: ").Append(MaxNumberOfLogs).Append("\n");
            sb.Append("  MaxNumberOfEvents: ").Append(MaxNumberOfEvents).Append("\n");
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
            return this.Equals(input as LimitParameters);
        }

        /// <summary>
        /// Returns true if LimitParameters instances are equal
        /// </summary>
        /// <param name="input">Instance of LimitParameters to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(LimitParameters input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.MaxCallDepth == input.MaxCallDepth ||
                    (this.MaxCallDepth != null &&
                    this.MaxCallDepth.Equals(input.MaxCallDepth))
                ) && 
                (
                    this.MaxHeapSubstateTotalBytes == input.MaxHeapSubstateTotalBytes ||
                    (this.MaxHeapSubstateTotalBytes != null &&
                    this.MaxHeapSubstateTotalBytes.Equals(input.MaxHeapSubstateTotalBytes))
                ) && 
                (
                    this.MaxTrackSubstateTotalBytes == input.MaxTrackSubstateTotalBytes ||
                    (this.MaxTrackSubstateTotalBytes != null &&
                    this.MaxTrackSubstateTotalBytes.Equals(input.MaxTrackSubstateTotalBytes))
                ) && 
                (
                    this.MaxSubstateKeySize == input.MaxSubstateKeySize ||
                    (this.MaxSubstateKeySize != null &&
                    this.MaxSubstateKeySize.Equals(input.MaxSubstateKeySize))
                ) && 
                (
                    this.MaxSubstateValueSize == input.MaxSubstateValueSize ||
                    (this.MaxSubstateValueSize != null &&
                    this.MaxSubstateValueSize.Equals(input.MaxSubstateValueSize))
                ) && 
                (
                    this.MaxInvokeInputSize == input.MaxInvokeInputSize ||
                    (this.MaxInvokeInputSize != null &&
                    this.MaxInvokeInputSize.Equals(input.MaxInvokeInputSize))
                ) && 
                (
                    this.MaxEventSize == input.MaxEventSize ||
                    (this.MaxEventSize != null &&
                    this.MaxEventSize.Equals(input.MaxEventSize))
                ) && 
                (
                    this.MaxLogSize == input.MaxLogSize ||
                    (this.MaxLogSize != null &&
                    this.MaxLogSize.Equals(input.MaxLogSize))
                ) && 
                (
                    this.MaxPanicMessageSize == input.MaxPanicMessageSize ||
                    (this.MaxPanicMessageSize != null &&
                    this.MaxPanicMessageSize.Equals(input.MaxPanicMessageSize))
                ) && 
                (
                    this.MaxNumberOfLogs == input.MaxNumberOfLogs ||
                    (this.MaxNumberOfLogs != null &&
                    this.MaxNumberOfLogs.Equals(input.MaxNumberOfLogs))
                ) && 
                (
                    this.MaxNumberOfEvents == input.MaxNumberOfEvents ||
                    (this.MaxNumberOfEvents != null &&
                    this.MaxNumberOfEvents.Equals(input.MaxNumberOfEvents))
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
                if (this.MaxCallDepth != null)
                {
                    hashCode = (hashCode * 59) + this.MaxCallDepth.GetHashCode();
                }
                if (this.MaxHeapSubstateTotalBytes != null)
                {
                    hashCode = (hashCode * 59) + this.MaxHeapSubstateTotalBytes.GetHashCode();
                }
                if (this.MaxTrackSubstateTotalBytes != null)
                {
                    hashCode = (hashCode * 59) + this.MaxTrackSubstateTotalBytes.GetHashCode();
                }
                if (this.MaxSubstateKeySize != null)
                {
                    hashCode = (hashCode * 59) + this.MaxSubstateKeySize.GetHashCode();
                }
                if (this.MaxSubstateValueSize != null)
                {
                    hashCode = (hashCode * 59) + this.MaxSubstateValueSize.GetHashCode();
                }
                if (this.MaxInvokeInputSize != null)
                {
                    hashCode = (hashCode * 59) + this.MaxInvokeInputSize.GetHashCode();
                }
                if (this.MaxEventSize != null)
                {
                    hashCode = (hashCode * 59) + this.MaxEventSize.GetHashCode();
                }
                if (this.MaxLogSize != null)
                {
                    hashCode = (hashCode * 59) + this.MaxLogSize.GetHashCode();
                }
                if (this.MaxPanicMessageSize != null)
                {
                    hashCode = (hashCode * 59) + this.MaxPanicMessageSize.GetHashCode();
                }
                if (this.MaxNumberOfLogs != null)
                {
                    hashCode = (hashCode * 59) + this.MaxNumberOfLogs.GetHashCode();
                }
                if (this.MaxNumberOfEvents != null)
                {
                    hashCode = (hashCode * 59) + this.MaxNumberOfEvents.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
