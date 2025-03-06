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
 * The version of the OpenAPI document: v1.10.1
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
    /// TransactionDetailsOptIns
    /// </summary>
    [DataContract(Name = "TransactionDetailsOptIns")]
    public partial class TransactionDetailsOptIns : IEquatable<TransactionDetailsOptIns>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDetailsOptIns" /> class.
        /// </summary>
        /// <param name="rawHex">if set to &#x60;true&#x60;, raw transaction hex is returned. (default to false).</param>
        /// <param name="receiptStateChanges">if set to &#x60;true&#x60;, state changes inside receipt object are returned. (default to false).</param>
        /// <param name="receiptFeeSummary">if set to &#x60;true&#x60;, fee summary inside receipt object is returned. (default to false).</param>
        /// <param name="receiptFeeSource">if set to &#x60;true&#x60;, fee source inside receipt object is returned. (default to false).</param>
        /// <param name="receiptFeeDestination">if set to &#x60;true&#x60;, fee destination inside receipt object is returned. (default to false).</param>
        /// <param name="receiptCostingParameters">if set to &#x60;true&#x60;, costing parameters inside receipt object is returned. (default to false).</param>
        /// <param name="receiptEvents">if set to &#x60;true&#x60;, events inside receipt object is returned. Please use the &#x60;detailed_events&#x60; instead, as it provides an enriched model with context and additional data. (default to false).</param>
        /// <param name="detailedEvents">if set to &#x60;true&#x60;, detailed events object is returned.  For more information please visit the [Detailed Events docs](#section/Detailed-Events-Explained).  (default to false).</param>
        /// <param name="receiptOutput">(true by default) if set to &#x60;true&#x60;, transaction receipt output is returned. (default to true).</param>
        /// <param name="affectedGlobalEntities">if set to &#x60;true&#x60;, all affected global entities by given transaction are returned. (default to false).</param>
        /// <param name="manifestInstructions">if set to &#x60;true&#x60;, manifest instructions for user transactions are returned. (default to false).</param>
        /// <param name="balanceChanges">if set to &#x60;true&#x60;, returns the fungible and non-fungible balance changes.  **Warning!** This opt-in might be missing for recently committed transactions, in that case a &#x60;null&#x60; value will be returned. Retry the request until non-null value is returned.  (default to false).</param>
        public TransactionDetailsOptIns(bool rawHex = false, bool receiptStateChanges = false, bool receiptFeeSummary = false, bool receiptFeeSource = false, bool receiptFeeDestination = false, bool receiptCostingParameters = false, bool receiptEvents = false, bool detailedEvents = false, bool receiptOutput = true, bool affectedGlobalEntities = false, bool manifestInstructions = false, bool balanceChanges = false)
        {
            this.RawHex = rawHex;
            this.ReceiptStateChanges = receiptStateChanges;
            this.ReceiptFeeSummary = receiptFeeSummary;
            this.ReceiptFeeSource = receiptFeeSource;
            this.ReceiptFeeDestination = receiptFeeDestination;
            this.ReceiptCostingParameters = receiptCostingParameters;
            this.ReceiptEvents = receiptEvents;
            this.DetailedEvents = detailedEvents;
            this.ReceiptOutput = receiptOutput;
            this.AffectedGlobalEntities = affectedGlobalEntities;
            this.ManifestInstructions = manifestInstructions;
            this.BalanceChanges = balanceChanges;
        }

        /// <summary>
        /// if set to &#x60;true&#x60;, raw transaction hex is returned.
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, raw transaction hex is returned.</value>
        [DataMember(Name = "raw_hex", EmitDefaultValue = true)]
        public bool RawHex { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, state changes inside receipt object are returned.
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, state changes inside receipt object are returned.</value>
        [DataMember(Name = "receipt_state_changes", EmitDefaultValue = true)]
        public bool ReceiptStateChanges { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, fee summary inside receipt object is returned.
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, fee summary inside receipt object is returned.</value>
        [DataMember(Name = "receipt_fee_summary", EmitDefaultValue = true)]
        public bool ReceiptFeeSummary { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, fee source inside receipt object is returned.
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, fee source inside receipt object is returned.</value>
        [DataMember(Name = "receipt_fee_source", EmitDefaultValue = true)]
        public bool ReceiptFeeSource { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, fee destination inside receipt object is returned.
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, fee destination inside receipt object is returned.</value>
        [DataMember(Name = "receipt_fee_destination", EmitDefaultValue = true)]
        public bool ReceiptFeeDestination { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, costing parameters inside receipt object is returned.
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, costing parameters inside receipt object is returned.</value>
        [DataMember(Name = "receipt_costing_parameters", EmitDefaultValue = true)]
        public bool ReceiptCostingParameters { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, events inside receipt object is returned. Please use the &#x60;detailed_events&#x60; instead, as it provides an enriched model with context and additional data.
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, events inside receipt object is returned. Please use the &#x60;detailed_events&#x60; instead, as it provides an enriched model with context and additional data.</value>
        [DataMember(Name = "receipt_events", EmitDefaultValue = true)]
        [Obsolete]
        public bool ReceiptEvents { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, detailed events object is returned.  For more information please visit the [Detailed Events docs](#section/Detailed-Events-Explained). 
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, detailed events object is returned.  For more information please visit the [Detailed Events docs](#section/Detailed-Events-Explained). </value>
        [DataMember(Name = "detailed_events", EmitDefaultValue = true)]
        public bool DetailedEvents { get; set; }

        /// <summary>
        /// (true by default) if set to &#x60;true&#x60;, transaction receipt output is returned.
        /// </summary>
        /// <value>(true by default) if set to &#x60;true&#x60;, transaction receipt output is returned.</value>
        [DataMember(Name = "receipt_output", EmitDefaultValue = true)]
        public bool ReceiptOutput { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, all affected global entities by given transaction are returned.
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, all affected global entities by given transaction are returned.</value>
        [DataMember(Name = "affected_global_entities", EmitDefaultValue = true)]
        public bool AffectedGlobalEntities { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, manifest instructions for user transactions are returned.
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, manifest instructions for user transactions are returned.</value>
        [DataMember(Name = "manifest_instructions", EmitDefaultValue = true)]
        public bool ManifestInstructions { get; set; }

        /// <summary>
        /// if set to &#x60;true&#x60;, returns the fungible and non-fungible balance changes.  **Warning!** This opt-in might be missing for recently committed transactions, in that case a &#x60;null&#x60; value will be returned. Retry the request until non-null value is returned. 
        /// </summary>
        /// <value>if set to &#x60;true&#x60;, returns the fungible and non-fungible balance changes.  **Warning!** This opt-in might be missing for recently committed transactions, in that case a &#x60;null&#x60; value will be returned. Retry the request until non-null value is returned. </value>
        [DataMember(Name = "balance_changes", EmitDefaultValue = true)]
        public bool BalanceChanges { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionDetailsOptIns {\n");
            sb.Append("  RawHex: ").Append(RawHex).Append("\n");
            sb.Append("  ReceiptStateChanges: ").Append(ReceiptStateChanges).Append("\n");
            sb.Append("  ReceiptFeeSummary: ").Append(ReceiptFeeSummary).Append("\n");
            sb.Append("  ReceiptFeeSource: ").Append(ReceiptFeeSource).Append("\n");
            sb.Append("  ReceiptFeeDestination: ").Append(ReceiptFeeDestination).Append("\n");
            sb.Append("  ReceiptCostingParameters: ").Append(ReceiptCostingParameters).Append("\n");
            sb.Append("  ReceiptEvents: ").Append(ReceiptEvents).Append("\n");
            sb.Append("  DetailedEvents: ").Append(DetailedEvents).Append("\n");
            sb.Append("  ReceiptOutput: ").Append(ReceiptOutput).Append("\n");
            sb.Append("  AffectedGlobalEntities: ").Append(AffectedGlobalEntities).Append("\n");
            sb.Append("  ManifestInstructions: ").Append(ManifestInstructions).Append("\n");
            sb.Append("  BalanceChanges: ").Append(BalanceChanges).Append("\n");
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
            return this.Equals(input as TransactionDetailsOptIns);
        }

        /// <summary>
        /// Returns true if TransactionDetailsOptIns instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionDetailsOptIns to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionDetailsOptIns input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.RawHex == input.RawHex ||
                    this.RawHex.Equals(input.RawHex)
                ) && 
                (
                    this.ReceiptStateChanges == input.ReceiptStateChanges ||
                    this.ReceiptStateChanges.Equals(input.ReceiptStateChanges)
                ) && 
                (
                    this.ReceiptFeeSummary == input.ReceiptFeeSummary ||
                    this.ReceiptFeeSummary.Equals(input.ReceiptFeeSummary)
                ) && 
                (
                    this.ReceiptFeeSource == input.ReceiptFeeSource ||
                    this.ReceiptFeeSource.Equals(input.ReceiptFeeSource)
                ) && 
                (
                    this.ReceiptFeeDestination == input.ReceiptFeeDestination ||
                    this.ReceiptFeeDestination.Equals(input.ReceiptFeeDestination)
                ) && 
                (
                    this.ReceiptCostingParameters == input.ReceiptCostingParameters ||
                    this.ReceiptCostingParameters.Equals(input.ReceiptCostingParameters)
                ) && 
                (
                    this.ReceiptEvents == input.ReceiptEvents ||
                    this.ReceiptEvents.Equals(input.ReceiptEvents)
                ) && 
                (
                    this.DetailedEvents == input.DetailedEvents ||
                    this.DetailedEvents.Equals(input.DetailedEvents)
                ) && 
                (
                    this.ReceiptOutput == input.ReceiptOutput ||
                    this.ReceiptOutput.Equals(input.ReceiptOutput)
                ) && 
                (
                    this.AffectedGlobalEntities == input.AffectedGlobalEntities ||
                    this.AffectedGlobalEntities.Equals(input.AffectedGlobalEntities)
                ) && 
                (
                    this.ManifestInstructions == input.ManifestInstructions ||
                    this.ManifestInstructions.Equals(input.ManifestInstructions)
                ) && 
                (
                    this.BalanceChanges == input.BalanceChanges ||
                    this.BalanceChanges.Equals(input.BalanceChanges)
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
                hashCode = (hashCode * 59) + this.RawHex.GetHashCode();
                hashCode = (hashCode * 59) + this.ReceiptStateChanges.GetHashCode();
                hashCode = (hashCode * 59) + this.ReceiptFeeSummary.GetHashCode();
                hashCode = (hashCode * 59) + this.ReceiptFeeSource.GetHashCode();
                hashCode = (hashCode * 59) + this.ReceiptFeeDestination.GetHashCode();
                hashCode = (hashCode * 59) + this.ReceiptCostingParameters.GetHashCode();
                hashCode = (hashCode * 59) + this.ReceiptEvents.GetHashCode();
                hashCode = (hashCode * 59) + this.DetailedEvents.GetHashCode();
                hashCode = (hashCode * 59) + this.ReceiptOutput.GetHashCode();
                hashCode = (hashCode * 59) + this.AffectedGlobalEntities.GetHashCode();
                hashCode = (hashCode * 59) + this.ManifestInstructions.GetHashCode();
                hashCode = (hashCode * 59) + this.BalanceChanges.GetHashCode();
                return hashCode;
            }
        }

    }

}
