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
 * The version of the OpenAPI document: v1.0.0
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
    /// The transaction execution receipt
    /// </summary>
    [DataContract(Name = "TransactionReceipt")]
    public partial class TransactionReceipt : IEquatable<TransactionReceipt>
    {

        /// <summary>
        /// Gets or Sets Status
        /// </summary>
        [DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
        public TransactionStatus Status { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionReceipt" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TransactionReceipt() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionReceipt" /> class.
        /// </summary>
        /// <param name="status">status (required).</param>
        /// <param name="feeSummary">feeSummary (required).</param>
        /// <param name="costingParameters">costingParameters (required).</param>
        /// <param name="feeSource">feeSource.</param>
        /// <param name="feeDestination">feeDestination.</param>
        /// <param name="stateUpdates">stateUpdates (required).</param>
        /// <param name="events">events.</param>
        /// <param name="nextEpoch">nextEpoch.</param>
        /// <param name="output">The manifest line-by-line engine return data (only present if &#x60;status&#x60; is &#x60;Succeeded&#x60;).</param>
        /// <param name="errorMessage">Error message (only present if status is &#x60;Failed&#x60; or &#x60;Rejected&#x60;).</param>
        public TransactionReceipt(TransactionStatus status = default(TransactionStatus), FeeSummary feeSummary = default(FeeSummary), CostingParameters costingParameters = default(CostingParameters), FeeSource feeSource = default(FeeSource), FeeDestination feeDestination = default(FeeDestination), StateUpdates stateUpdates = default(StateUpdates), List<Event> events = default(List<Event>), NextEpoch nextEpoch = default(NextEpoch), List<SborData> output = default(List<SborData>), string errorMessage = default(string))
        {
            this.Status = status;
            // to ensure "feeSummary" is required (not null)
            if (feeSummary == null)
            {
                throw new ArgumentNullException("feeSummary is a required property for TransactionReceipt and cannot be null");
            }
            this.FeeSummary = feeSummary;
            // to ensure "costingParameters" is required (not null)
            if (costingParameters == null)
            {
                throw new ArgumentNullException("costingParameters is a required property for TransactionReceipt and cannot be null");
            }
            this.CostingParameters = costingParameters;
            // to ensure "stateUpdates" is required (not null)
            if (stateUpdates == null)
            {
                throw new ArgumentNullException("stateUpdates is a required property for TransactionReceipt and cannot be null");
            }
            this.StateUpdates = stateUpdates;
            this.FeeSource = feeSource;
            this.FeeDestination = feeDestination;
            this.Events = events;
            this.NextEpoch = nextEpoch;
            this.Output = output;
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Gets or Sets FeeSummary
        /// </summary>
        [DataMember(Name = "fee_summary", IsRequired = true, EmitDefaultValue = true)]
        public FeeSummary FeeSummary { get; set; }

        /// <summary>
        /// Gets or Sets CostingParameters
        /// </summary>
        [DataMember(Name = "costing_parameters", IsRequired = true, EmitDefaultValue = true)]
        public CostingParameters CostingParameters { get; set; }

        /// <summary>
        /// Gets or Sets FeeSource
        /// </summary>
        [DataMember(Name = "fee_source", EmitDefaultValue = true)]
        public FeeSource FeeSource { get; set; }

        /// <summary>
        /// Gets or Sets FeeDestination
        /// </summary>
        [DataMember(Name = "fee_destination", EmitDefaultValue = true)]
        public FeeDestination FeeDestination { get; set; }

        /// <summary>
        /// Gets or Sets StateUpdates
        /// </summary>
        [DataMember(Name = "state_updates", IsRequired = true, EmitDefaultValue = true)]
        public StateUpdates StateUpdates { get; set; }

        /// <summary>
        /// Gets or Sets Events
        /// </summary>
        [DataMember(Name = "events", EmitDefaultValue = true)]
        public List<Event> Events { get; set; }

        /// <summary>
        /// Gets or Sets NextEpoch
        /// </summary>
        [DataMember(Name = "next_epoch", EmitDefaultValue = true)]
        public NextEpoch NextEpoch { get; set; }

        /// <summary>
        /// The manifest line-by-line engine return data (only present if &#x60;status&#x60; is &#x60;Succeeded&#x60;)
        /// </summary>
        /// <value>The manifest line-by-line engine return data (only present if &#x60;status&#x60; is &#x60;Succeeded&#x60;)</value>
        [DataMember(Name = "output", EmitDefaultValue = true)]
        public List<SborData> Output { get; set; }

        /// <summary>
        /// Error message (only present if status is &#x60;Failed&#x60; or &#x60;Rejected&#x60;)
        /// </summary>
        /// <value>Error message (only present if status is &#x60;Failed&#x60; or &#x60;Rejected&#x60;)</value>
        [DataMember(Name = "error_message", EmitDefaultValue = true)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionReceipt {\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  FeeSummary: ").Append(FeeSummary).Append("\n");
            sb.Append("  CostingParameters: ").Append(CostingParameters).Append("\n");
            sb.Append("  FeeSource: ").Append(FeeSource).Append("\n");
            sb.Append("  FeeDestination: ").Append(FeeDestination).Append("\n");
            sb.Append("  StateUpdates: ").Append(StateUpdates).Append("\n");
            sb.Append("  Events: ").Append(Events).Append("\n");
            sb.Append("  NextEpoch: ").Append(NextEpoch).Append("\n");
            sb.Append("  Output: ").Append(Output).Append("\n");
            sb.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
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
            return this.Equals(input as TransactionReceipt);
        }

        /// <summary>
        /// Returns true if TransactionReceipt instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionReceipt to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionReceipt input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Status == input.Status ||
                    this.Status.Equals(input.Status)
                ) && 
                (
                    this.FeeSummary == input.FeeSummary ||
                    (this.FeeSummary != null &&
                    this.FeeSummary.Equals(input.FeeSummary))
                ) && 
                (
                    this.CostingParameters == input.CostingParameters ||
                    (this.CostingParameters != null &&
                    this.CostingParameters.Equals(input.CostingParameters))
                ) && 
                (
                    this.FeeSource == input.FeeSource ||
                    (this.FeeSource != null &&
                    this.FeeSource.Equals(input.FeeSource))
                ) && 
                (
                    this.FeeDestination == input.FeeDestination ||
                    (this.FeeDestination != null &&
                    this.FeeDestination.Equals(input.FeeDestination))
                ) && 
                (
                    this.StateUpdates == input.StateUpdates ||
                    (this.StateUpdates != null &&
                    this.StateUpdates.Equals(input.StateUpdates))
                ) && 
                (
                    this.Events == input.Events ||
                    this.Events != null &&
                    input.Events != null &&
                    this.Events.SequenceEqual(input.Events)
                ) && 
                (
                    this.NextEpoch == input.NextEpoch ||
                    (this.NextEpoch != null &&
                    this.NextEpoch.Equals(input.NextEpoch))
                ) && 
                (
                    this.Output == input.Output ||
                    this.Output != null &&
                    input.Output != null &&
                    this.Output.SequenceEqual(input.Output)
                ) && 
                (
                    this.ErrorMessage == input.ErrorMessage ||
                    (this.ErrorMessage != null &&
                    this.ErrorMessage.Equals(input.ErrorMessage))
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
                hashCode = (hashCode * 59) + this.Status.GetHashCode();
                if (this.FeeSummary != null)
                {
                    hashCode = (hashCode * 59) + this.FeeSummary.GetHashCode();
                }
                if (this.CostingParameters != null)
                {
                    hashCode = (hashCode * 59) + this.CostingParameters.GetHashCode();
                }
                if (this.FeeSource != null)
                {
                    hashCode = (hashCode * 59) + this.FeeSource.GetHashCode();
                }
                if (this.FeeDestination != null)
                {
                    hashCode = (hashCode * 59) + this.FeeDestination.GetHashCode();
                }
                if (this.StateUpdates != null)
                {
                    hashCode = (hashCode * 59) + this.StateUpdates.GetHashCode();
                }
                if (this.Events != null)
                {
                    hashCode = (hashCode * 59) + this.Events.GetHashCode();
                }
                if (this.NextEpoch != null)
                {
                    hashCode = (hashCode * 59) + this.NextEpoch.GetHashCode();
                }
                if (this.Output != null)
                {
                    hashCode = (hashCode * 59) + this.Output.GetHashCode();
                }
                if (this.ErrorMessage != null)
                {
                    hashCode = (hashCode * 59) + this.ErrorMessage.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
