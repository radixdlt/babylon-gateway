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
 * The version of the OpenAPI document: v1.6.1
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
    /// CaCostingParameters
    /// </summary>
    [DataContract(Name = "CaCostingParameters")]
    public partial class CaCostingParameters : IEquatable<CaCostingParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaCostingParameters" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected CaCostingParameters() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="CaCostingParameters" /> class.
        /// </summary>
        /// <param name="executionCostUnitPrice">The string-encoded decimal representing the XRD price of a single cost unit of transaction execution. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        /// <param name="executionCostUnitLimit">An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, representing the maximum amount of cost units available for the transaction execution. (required).</param>
        /// <param name="executionCostUnitLoan">An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, representing the maximum number of cost units which can be used before fee is locked from a vault. (required).</param>
        /// <param name="finalizationCostUnitPrice">The string-encoded decimal representing the XRD price of a single cost unit of transaction finalization. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        /// <param name="finalizationCostUnitLimit">An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, representing the maximum amount of cost units available for the transaction finalization. (required).</param>
        /// <param name="xrdUsdPrice">The string-encoded decimal representing what amount of XRD is consumed by a Royalty of 1 USD. This is fixed for a given protocol version, so is not an accurate representation of the XRD price. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        /// <param name="xrdStoragePrice">The string-encoded decimal representing the price of 1 byte of state storage, expressed in XRD. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        /// <param name="xrdArchiveStoragePrice">The string-encoded decimal representing the price of 1 byte of archive storage, expressed in XRD. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        /// <param name="tipPercentage">An integer between &#x60;0&#x60; and &#x60;65535&#x60;, giving the validator tip as a percentage amount. A value of &#x60;1&#x60; corresponds to 1% of the fee. (required).</param>
        public CaCostingParameters(string executionCostUnitPrice = default(string), long executionCostUnitLimit = default(long), long executionCostUnitLoan = default(long), string finalizationCostUnitPrice = default(string), long finalizationCostUnitLimit = default(long), string xrdUsdPrice = default(string), string xrdStoragePrice = default(string), string xrdArchiveStoragePrice = default(string), int tipPercentage = default(int))
        {
            // to ensure "executionCostUnitPrice" is required (not null)
            if (executionCostUnitPrice == null)
            {
                throw new ArgumentNullException("executionCostUnitPrice is a required property for CaCostingParameters and cannot be null");
            }
            this.ExecutionCostUnitPrice = executionCostUnitPrice;
            this.ExecutionCostUnitLimit = executionCostUnitLimit;
            this.ExecutionCostUnitLoan = executionCostUnitLoan;
            // to ensure "finalizationCostUnitPrice" is required (not null)
            if (finalizationCostUnitPrice == null)
            {
                throw new ArgumentNullException("finalizationCostUnitPrice is a required property for CaCostingParameters and cannot be null");
            }
            this.FinalizationCostUnitPrice = finalizationCostUnitPrice;
            this.FinalizationCostUnitLimit = finalizationCostUnitLimit;
            // to ensure "xrdUsdPrice" is required (not null)
            if (xrdUsdPrice == null)
            {
                throw new ArgumentNullException("xrdUsdPrice is a required property for CaCostingParameters and cannot be null");
            }
            this.XrdUsdPrice = xrdUsdPrice;
            // to ensure "xrdStoragePrice" is required (not null)
            if (xrdStoragePrice == null)
            {
                throw new ArgumentNullException("xrdStoragePrice is a required property for CaCostingParameters and cannot be null");
            }
            this.XrdStoragePrice = xrdStoragePrice;
            // to ensure "xrdArchiveStoragePrice" is required (not null)
            if (xrdArchiveStoragePrice == null)
            {
                throw new ArgumentNullException("xrdArchiveStoragePrice is a required property for CaCostingParameters and cannot be null");
            }
            this.XrdArchiveStoragePrice = xrdArchiveStoragePrice;
            this.TipPercentage = tipPercentage;
        }

        /// <summary>
        /// The string-encoded decimal representing the XRD price of a single cost unit of transaction execution. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>The string-encoded decimal representing the XRD price of a single cost unit of transaction execution. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "execution_cost_unit_price", IsRequired = true, EmitDefaultValue = true)]
        public string ExecutionCostUnitPrice { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, representing the maximum amount of cost units available for the transaction execution.
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, representing the maximum amount of cost units available for the transaction execution.</value>
        [DataMember(Name = "execution_cost_unit_limit", IsRequired = true, EmitDefaultValue = true)]
        public long ExecutionCostUnitLimit { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, representing the maximum number of cost units which can be used before fee is locked from a vault.
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, representing the maximum number of cost units which can be used before fee is locked from a vault.</value>
        [DataMember(Name = "execution_cost_unit_loan", IsRequired = true, EmitDefaultValue = true)]
        public long ExecutionCostUnitLoan { get; set; }

        /// <summary>
        /// The string-encoded decimal representing the XRD price of a single cost unit of transaction finalization. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>The string-encoded decimal representing the XRD price of a single cost unit of transaction finalization. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "finalization_cost_unit_price", IsRequired = true, EmitDefaultValue = true)]
        public string FinalizationCostUnitPrice { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, representing the maximum amount of cost units available for the transaction finalization.
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, representing the maximum amount of cost units available for the transaction finalization.</value>
        [DataMember(Name = "finalization_cost_unit_limit", IsRequired = true, EmitDefaultValue = true)]
        public long FinalizationCostUnitLimit { get; set; }

        /// <summary>
        /// The string-encoded decimal representing what amount of XRD is consumed by a Royalty of 1 USD. This is fixed for a given protocol version, so is not an accurate representation of the XRD price. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>The string-encoded decimal representing what amount of XRD is consumed by a Royalty of 1 USD. This is fixed for a given protocol version, so is not an accurate representation of the XRD price. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "xrd_usd_price", IsRequired = true, EmitDefaultValue = true)]
        public string XrdUsdPrice { get; set; }

        /// <summary>
        /// The string-encoded decimal representing the price of 1 byte of state storage, expressed in XRD. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>The string-encoded decimal representing the price of 1 byte of state storage, expressed in XRD. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "xrd_storage_price", IsRequired = true, EmitDefaultValue = true)]
        public string XrdStoragePrice { get; set; }

        /// <summary>
        /// The string-encoded decimal representing the price of 1 byte of archive storage, expressed in XRD. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>The string-encoded decimal representing the price of 1 byte of archive storage, expressed in XRD. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "xrd_archive_storage_price", IsRequired = true, EmitDefaultValue = true)]
        public string XrdArchiveStoragePrice { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;65535&#x60;, giving the validator tip as a percentage amount. A value of &#x60;1&#x60; corresponds to 1% of the fee.
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;65535&#x60;, giving the validator tip as a percentage amount. A value of &#x60;1&#x60; corresponds to 1% of the fee.</value>
        [DataMember(Name = "tip_percentage", IsRequired = true, EmitDefaultValue = true)]
        public int TipPercentage { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class CaCostingParameters {\n");
            sb.Append("  ExecutionCostUnitPrice: ").Append(ExecutionCostUnitPrice).Append("\n");
            sb.Append("  ExecutionCostUnitLimit: ").Append(ExecutionCostUnitLimit).Append("\n");
            sb.Append("  ExecutionCostUnitLoan: ").Append(ExecutionCostUnitLoan).Append("\n");
            sb.Append("  FinalizationCostUnitPrice: ").Append(FinalizationCostUnitPrice).Append("\n");
            sb.Append("  FinalizationCostUnitLimit: ").Append(FinalizationCostUnitLimit).Append("\n");
            sb.Append("  XrdUsdPrice: ").Append(XrdUsdPrice).Append("\n");
            sb.Append("  XrdStoragePrice: ").Append(XrdStoragePrice).Append("\n");
            sb.Append("  XrdArchiveStoragePrice: ").Append(XrdArchiveStoragePrice).Append("\n");
            sb.Append("  TipPercentage: ").Append(TipPercentage).Append("\n");
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
            return this.Equals(input as CaCostingParameters);
        }

        /// <summary>
        /// Returns true if CaCostingParameters instances are equal
        /// </summary>
        /// <param name="input">Instance of CaCostingParameters to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CaCostingParameters input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.ExecutionCostUnitPrice == input.ExecutionCostUnitPrice ||
                    (this.ExecutionCostUnitPrice != null &&
                    this.ExecutionCostUnitPrice.Equals(input.ExecutionCostUnitPrice))
                ) && 
                (
                    this.ExecutionCostUnitLimit == input.ExecutionCostUnitLimit ||
                    this.ExecutionCostUnitLimit.Equals(input.ExecutionCostUnitLimit)
                ) && 
                (
                    this.ExecutionCostUnitLoan == input.ExecutionCostUnitLoan ||
                    this.ExecutionCostUnitLoan.Equals(input.ExecutionCostUnitLoan)
                ) && 
                (
                    this.FinalizationCostUnitPrice == input.FinalizationCostUnitPrice ||
                    (this.FinalizationCostUnitPrice != null &&
                    this.FinalizationCostUnitPrice.Equals(input.FinalizationCostUnitPrice))
                ) && 
                (
                    this.FinalizationCostUnitLimit == input.FinalizationCostUnitLimit ||
                    this.FinalizationCostUnitLimit.Equals(input.FinalizationCostUnitLimit)
                ) && 
                (
                    this.XrdUsdPrice == input.XrdUsdPrice ||
                    (this.XrdUsdPrice != null &&
                    this.XrdUsdPrice.Equals(input.XrdUsdPrice))
                ) && 
                (
                    this.XrdStoragePrice == input.XrdStoragePrice ||
                    (this.XrdStoragePrice != null &&
                    this.XrdStoragePrice.Equals(input.XrdStoragePrice))
                ) && 
                (
                    this.XrdArchiveStoragePrice == input.XrdArchiveStoragePrice ||
                    (this.XrdArchiveStoragePrice != null &&
                    this.XrdArchiveStoragePrice.Equals(input.XrdArchiveStoragePrice))
                ) && 
                (
                    this.TipPercentage == input.TipPercentage ||
                    this.TipPercentage.Equals(input.TipPercentage)
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
                if (this.ExecutionCostUnitPrice != null)
                {
                    hashCode = (hashCode * 59) + this.ExecutionCostUnitPrice.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.ExecutionCostUnitLimit.GetHashCode();
                hashCode = (hashCode * 59) + this.ExecutionCostUnitLoan.GetHashCode();
                if (this.FinalizationCostUnitPrice != null)
                {
                    hashCode = (hashCode * 59) + this.FinalizationCostUnitPrice.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.FinalizationCostUnitLimit.GetHashCode();
                if (this.XrdUsdPrice != null)
                {
                    hashCode = (hashCode * 59) + this.XrdUsdPrice.GetHashCode();
                }
                if (this.XrdStoragePrice != null)
                {
                    hashCode = (hashCode * 59) + this.XrdStoragePrice.GetHashCode();
                }
                if (this.XrdArchiveStoragePrice != null)
                {
                    hashCode = (hashCode * 59) + this.XrdArchiveStoragePrice.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.TipPercentage.GetHashCode();
                return hashCode;
            }
        }

    }

}
