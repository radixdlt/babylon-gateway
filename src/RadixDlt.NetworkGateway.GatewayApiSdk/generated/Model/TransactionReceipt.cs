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
 * Radix Gateway API - Babylon
 *
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.6.0
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
        /// Gets or Sets Status
        /// </summary>
        [DataMember(Name = "status", EmitDefaultValue = true)]
        public TransactionStatus? Status { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionReceipt" /> class.
        /// </summary>
        /// <param name="status">status.</param>
        /// <param name="feeSummary">This type is defined in the Core API as &#x60;FeeSummary&#x60;. See the Core API documentation for more details. .</param>
        /// <param name="costingParameters">costingParameters.</param>
        /// <param name="feeDestination">This type is defined in the Core API as &#x60;FeeDestination&#x60;. See the Core API documentation for more details. .</param>
        /// <param name="feeSource">This type is defined in the Core API as &#x60;FeeSource&#x60;. See the Core API documentation for more details. .</param>
        /// <param name="stateUpdates">This type is defined in the Core API as &#x60;StateUpdates&#x60;. See the Core API documentation for more details. .</param>
        /// <param name="nextEpoch">Information (number and active validator list) about new epoch if occured. This type is defined in the Core API as &#x60;NextEpoch&#x60;. See the Core API documentation for more details. .</param>
        /// <param name="output">The manifest line-by-line engine return data (only present if &#x60;status&#x60; is &#x60;CommittedSuccess&#x60;). This type is defined in the Core API as &#x60;SborData&#x60;. See the Core API documentation for more details. .</param>
        /// <param name="events">Events emitted by a transaction..</param>
        /// <param name="errorMessage">Error message (only present if status is &#x60;Failed&#x60; or &#x60;Rejected&#x60;).</param>
        public TransactionReceipt(TransactionStatus? status = default(TransactionStatus?), Object feeSummary = default(Object), Object costingParameters = default(Object), Object feeDestination = default(Object), Object feeSource = default(Object), Object stateUpdates = default(Object), Object nextEpoch = default(Object), Object output = default(Object), List<EventsItem> events = default(List<EventsItem>), string errorMessage = default(string))
        {
            this.Status = status;
            this.FeeSummary = feeSummary;
            this.CostingParameters = costingParameters;
            this.FeeDestination = feeDestination;
            this.FeeSource = feeSource;
            this.StateUpdates = stateUpdates;
            this.NextEpoch = nextEpoch;
            this.Output = output;
            this.Events = events;
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// This type is defined in the Core API as &#x60;FeeSummary&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>This type is defined in the Core API as &#x60;FeeSummary&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "fee_summary", EmitDefaultValue = true)]
        public Object FeeSummary { get; set; }

        /// <summary>
        /// Gets or Sets CostingParameters
        /// </summary>
        [DataMember(Name = "costing_parameters", EmitDefaultValue = true)]
        public Object CostingParameters { get; set; }

        /// <summary>
        /// This type is defined in the Core API as &#x60;FeeDestination&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>This type is defined in the Core API as &#x60;FeeDestination&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "fee_destination", EmitDefaultValue = true)]
        public Object FeeDestination { get; set; }

        /// <summary>
        /// This type is defined in the Core API as &#x60;FeeSource&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>This type is defined in the Core API as &#x60;FeeSource&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "fee_source", EmitDefaultValue = true)]
        public Object FeeSource { get; set; }

        /// <summary>
        /// This type is defined in the Core API as &#x60;StateUpdates&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>This type is defined in the Core API as &#x60;StateUpdates&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "state_updates", EmitDefaultValue = true)]
        public Object StateUpdates { get; set; }

        /// <summary>
        /// Information (number and active validator list) about new epoch if occured. This type is defined in the Core API as &#x60;NextEpoch&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>Information (number and active validator list) about new epoch if occured. This type is defined in the Core API as &#x60;NextEpoch&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "next_epoch", EmitDefaultValue = true)]
        public Object NextEpoch { get; set; }

        /// <summary>
        /// The manifest line-by-line engine return data (only present if &#x60;status&#x60; is &#x60;CommittedSuccess&#x60;). This type is defined in the Core API as &#x60;SborData&#x60;. See the Core API documentation for more details. 
        /// </summary>
        /// <value>The manifest line-by-line engine return data (only present if &#x60;status&#x60; is &#x60;CommittedSuccess&#x60;). This type is defined in the Core API as &#x60;SborData&#x60;. See the Core API documentation for more details. </value>
        [DataMember(Name = "output", EmitDefaultValue = true)]
        public Object Output { get; set; }

        /// <summary>
        /// Events emitted by a transaction.
        /// </summary>
        /// <value>Events emitted by a transaction.</value>
        [DataMember(Name = "events", EmitDefaultValue = true)]
        public List<EventsItem> Events { get; set; }

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
            sb.Append("  FeeDestination: ").Append(FeeDestination).Append("\n");
            sb.Append("  FeeSource: ").Append(FeeSource).Append("\n");
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
                    this.FeeDestination == input.FeeDestination ||
                    (this.FeeDestination != null &&
                    this.FeeDestination.Equals(input.FeeDestination))
                ) && 
                (
                    this.FeeSource == input.FeeSource ||
                    (this.FeeSource != null &&
                    this.FeeSource.Equals(input.FeeSource))
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
                    this.Events != null &&
                    input.Events != null &&
                    this.Events.SequenceEqual(input.Events)
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
                if (this.FeeDestination != null)
                {
                    hashCode = (hashCode * 59) + this.FeeDestination.GetHashCode();
                }
                if (this.FeeSource != null)
                {
                    hashCode = (hashCode * 59) + this.FeeSource.GetHashCode();
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
