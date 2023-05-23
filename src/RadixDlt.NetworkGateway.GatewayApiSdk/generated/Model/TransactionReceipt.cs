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
 * Babylon Gateway API - RCnet V2
 *
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers. For simple use cases, you can typically use the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs-babylon.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Integration and forward compatibility guarantees  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
 *
 * The version of the OpenAPI document: 0.4.0
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
using FileParameter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// TransactionReceipt
    /// </summary>
    [DataContract(Name = "TransactionReceipt")]
    public partial class TransactionReceipt : IEquatable<TransactionReceipt>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionReceipt" /> class.
        /// </summary>
        /// <param name="status">The status of the transaction..</param>
        /// <param name="feeSummary">Fees paid, Only present if the &#x60;status&#x60; is not &#x60;Rejected&#x60;..</param>
        /// <param name="stateUpdates">Transaction state updates (only present if status is Succeeded or Failed)..</param>
        /// <param name="nextEpoch">Information (number and active validator list) about new epoch if occured..</param>
        /// <param name="output">The manifest line-by-line engine return data (only present if &#x60;status&#x60; is &#x60;Succeeded&#x60;)..</param>
        /// <param name="events">Events emitted by a transaction..</param>
        /// <param name="errorMessage">Error message (only present if status is &#x60;Failed&#x60; or &#x60;Rejected&#x60;).</param>
        public TransactionReceipt(Object status = default(Object), Object feeSummary = default(Object), Object stateUpdates = default(Object), Object nextEpoch = default(Object), Object output = default(Object), Object events = default(Object), string errorMessage = default(string))
        {
            this.Status = status;
            this.FeeSummary = feeSummary;
            this.StateUpdates = stateUpdates;
            this.NextEpoch = nextEpoch;
            this.Output = output;
            this.Events = events;
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// The status of the transaction.
        /// </summary>
        /// <value>The status of the transaction.</value>
        [DataMember(Name = "status", EmitDefaultValue = true)]
        public Object Status { get; set; }

        /// <summary>
        /// Fees paid, Only present if the &#x60;status&#x60; is not &#x60;Rejected&#x60;.
        /// </summary>
        /// <value>Fees paid, Only present if the &#x60;status&#x60; is not &#x60;Rejected&#x60;.</value>
        [DataMember(Name = "fee_summary", EmitDefaultValue = true)]
        public Object FeeSummary { get; set; }

        /// <summary>
        /// Transaction state updates (only present if status is Succeeded or Failed).
        /// </summary>
        /// <value>Transaction state updates (only present if status is Succeeded or Failed).</value>
        [DataMember(Name = "state_updates", EmitDefaultValue = true)]
        public Object StateUpdates { get; set; }

        /// <summary>
        /// Information (number and active validator list) about new epoch if occured.
        /// </summary>
        /// <value>Information (number and active validator list) about new epoch if occured.</value>
        [DataMember(Name = "next_epoch", EmitDefaultValue = true)]
        public Object NextEpoch { get; set; }

        /// <summary>
        /// The manifest line-by-line engine return data (only present if &#x60;status&#x60; is &#x60;Succeeded&#x60;).
        /// </summary>
        /// <value>The manifest line-by-line engine return data (only present if &#x60;status&#x60; is &#x60;Succeeded&#x60;).</value>
        [DataMember(Name = "output", EmitDefaultValue = true)]
        public Object Output { get; set; }

        /// <summary>
        /// Events emitted by a transaction.
        /// </summary>
        /// <value>Events emitted by a transaction.</value>
        [DataMember(Name = "events", EmitDefaultValue = true)]
        public Object Events { get; set; }

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
            sb.Append("  StateUpdates: ").Append(StateUpdates).Append("\n");
            sb.Append("  NextEpoch: ").Append(NextEpoch).Append("\n");
            sb.Append("  Output: ").Append(Output).Append("\n");
            sb.Append("  Events: ").Append(Events).Append("\n");
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
                    (this.Status != null &&
                    this.Status.Equals(input.Status))
                ) && 
                (
                    this.FeeSummary == input.FeeSummary ||
                    (this.FeeSummary != null &&
                    this.FeeSummary.Equals(input.FeeSummary))
                ) && 
                (
                    this.StateUpdates == input.StateUpdates ||
                    (this.StateUpdates != null &&
                    this.StateUpdates.Equals(input.StateUpdates))
                ) && 
                (
                    this.NextEpoch == input.NextEpoch ||
                    (this.NextEpoch != null &&
                    this.NextEpoch.Equals(input.NextEpoch))
                ) && 
                (
                    this.Output == input.Output ||
                    (this.Output != null &&
                    this.Output.Equals(input.Output))
                ) && 
                (
                    this.Events == input.Events ||
                    (this.Events != null &&
                    this.Events.Equals(input.Events))
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
                if (this.Status != null)
                {
                    hashCode = (hashCode * 59) + this.Status.GetHashCode();
                }
                if (this.FeeSummary != null)
                {
                    hashCode = (hashCode * 59) + this.FeeSummary.GetHashCode();
                }
                if (this.StateUpdates != null)
                {
                    hashCode = (hashCode * 59) + this.StateUpdates.GetHashCode();
                }
                if (this.NextEpoch != null)
                {
                    hashCode = (hashCode * 59) + this.NextEpoch.GetHashCode();
                }
                if (this.Output != null)
                {
                    hashCode = (hashCode * 59) + this.Output.GetHashCode();
                }
                if (this.Events != null)
                {
                    hashCode = (hashCode * 59) + this.Events.GetHashCode();
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
