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
using System.ComponentModel.DataAnnotations;
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// Fees paid
    /// </summary>
    [DataContract(Name = "FeeSummary")]
    public partial class FeeSummary : IEquatable<FeeSummary>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeeSummary" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected FeeSummary() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="FeeSummary" /> class.
        /// </summary>
        /// <param name="loanFullyRepaid">Specifies whether the transaction execution loan has been fully repaid. (required).</param>
        /// <param name="costUnitLimit">Maximum amount of cost units available for the transaction execution. A decimal 32-bit unsigned integer. (required).</param>
        /// <param name="costUnitConsumed">The amount of cost units consumed by the transaction execution. A decimal 32-bit unsigned integer. (required).</param>
        /// <param name="costUnitPrice">The XRD price of a single cost unit. A fixed-scale 256-bit signed decimal number. (required).</param>
        /// <param name="tipPercentage">The validator tip. A decimal 32-bit unsigned integer, representing the percentage amount (a value of \&quot;1\&quot; corresponds to 1%). (required).</param>
        /// <param name="xrdBurned">The total amount of XRD burned. A fixed-scale 256-bit signed decimal number. (required).</param>
        /// <param name="xrdTipped">The total amount of XRD tipped to validators. A fixed-scale 256-bit signed decimal number. (required).</param>
        public FeeSummary(bool loanFullyRepaid = default(bool), string costUnitLimit = default(string), string costUnitConsumed = default(string), string costUnitPrice = default(string), string tipPercentage = default(string), string xrdBurned = default(string), string xrdTipped = default(string))
        {
            this.LoanFullyRepaid = loanFullyRepaid;
            // to ensure "costUnitLimit" is required (not null)
            if (costUnitLimit == null)
            {
                throw new ArgumentNullException("costUnitLimit is a required property for FeeSummary and cannot be null");
            }
            this.CostUnitLimit = costUnitLimit;
            // to ensure "costUnitConsumed" is required (not null)
            if (costUnitConsumed == null)
            {
                throw new ArgumentNullException("costUnitConsumed is a required property for FeeSummary and cannot be null");
            }
            this.CostUnitConsumed = costUnitConsumed;
            // to ensure "costUnitPrice" is required (not null)
            if (costUnitPrice == null)
            {
                throw new ArgumentNullException("costUnitPrice is a required property for FeeSummary and cannot be null");
            }
            this.CostUnitPrice = costUnitPrice;
            // to ensure "tipPercentage" is required (not null)
            if (tipPercentage == null)
            {
                throw new ArgumentNullException("tipPercentage is a required property for FeeSummary and cannot be null");
            }
            this.TipPercentage = tipPercentage;
            // to ensure "xrdBurned" is required (not null)
            if (xrdBurned == null)
            {
                throw new ArgumentNullException("xrdBurned is a required property for FeeSummary and cannot be null");
            }
            this.XrdBurned = xrdBurned;
            // to ensure "xrdTipped" is required (not null)
            if (xrdTipped == null)
            {
                throw new ArgumentNullException("xrdTipped is a required property for FeeSummary and cannot be null");
            }
            this.XrdTipped = xrdTipped;
        }

        /// <summary>
        /// Specifies whether the transaction execution loan has been fully repaid.
        /// </summary>
        /// <value>Specifies whether the transaction execution loan has been fully repaid.</value>
        [DataMember(Name = "loan_fully_repaid", IsRequired = true, EmitDefaultValue = true)]
        public bool LoanFullyRepaid { get; set; }

        /// <summary>
        /// Maximum amount of cost units available for the transaction execution. A decimal 32-bit unsigned integer.
        /// </summary>
        /// <value>Maximum amount of cost units available for the transaction execution. A decimal 32-bit unsigned integer.</value>
        [DataMember(Name = "cost_unit_limit", IsRequired = true, EmitDefaultValue = true)]
        public string CostUnitLimit { get; set; }

        /// <summary>
        /// The amount of cost units consumed by the transaction execution. A decimal 32-bit unsigned integer.
        /// </summary>
        /// <value>The amount of cost units consumed by the transaction execution. A decimal 32-bit unsigned integer.</value>
        [DataMember(Name = "cost_unit_consumed", IsRequired = true, EmitDefaultValue = true)]
        public string CostUnitConsumed { get; set; }

        /// <summary>
        /// The XRD price of a single cost unit. A fixed-scale 256-bit signed decimal number.
        /// </summary>
        /// <value>The XRD price of a single cost unit. A fixed-scale 256-bit signed decimal number.</value>
        [DataMember(Name = "cost_unit_price", IsRequired = true, EmitDefaultValue = true)]
        public string CostUnitPrice { get; set; }

        /// <summary>
        /// The validator tip. A decimal 32-bit unsigned integer, representing the percentage amount (a value of \&quot;1\&quot; corresponds to 1%).
        /// </summary>
        /// <value>The validator tip. A decimal 32-bit unsigned integer, representing the percentage amount (a value of \&quot;1\&quot; corresponds to 1%).</value>
        [DataMember(Name = "tip_percentage", IsRequired = true, EmitDefaultValue = true)]
        public string TipPercentage { get; set; }

        /// <summary>
        /// The total amount of XRD burned. A fixed-scale 256-bit signed decimal number.
        /// </summary>
        /// <value>The total amount of XRD burned. A fixed-scale 256-bit signed decimal number.</value>
        [DataMember(Name = "xrd_burned", IsRequired = true, EmitDefaultValue = true)]
        public string XrdBurned { get; set; }

        /// <summary>
        /// The total amount of XRD tipped to validators. A fixed-scale 256-bit signed decimal number.
        /// </summary>
        /// <value>The total amount of XRD tipped to validators. A fixed-scale 256-bit signed decimal number.</value>
        [DataMember(Name = "xrd_tipped", IsRequired = true, EmitDefaultValue = true)]
        public string XrdTipped { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class FeeSummary {\n");
            sb.Append("  LoanFullyRepaid: ").Append(LoanFullyRepaid).Append("\n");
            sb.Append("  CostUnitLimit: ").Append(CostUnitLimit).Append("\n");
            sb.Append("  CostUnitConsumed: ").Append(CostUnitConsumed).Append("\n");
            sb.Append("  CostUnitPrice: ").Append(CostUnitPrice).Append("\n");
            sb.Append("  TipPercentage: ").Append(TipPercentage).Append("\n");
            sb.Append("  XrdBurned: ").Append(XrdBurned).Append("\n");
            sb.Append("  XrdTipped: ").Append(XrdTipped).Append("\n");
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
            return this.Equals(input as FeeSummary);
        }

        /// <summary>
        /// Returns true if FeeSummary instances are equal
        /// </summary>
        /// <param name="input">Instance of FeeSummary to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(FeeSummary input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.LoanFullyRepaid == input.LoanFullyRepaid ||
                    this.LoanFullyRepaid.Equals(input.LoanFullyRepaid)
                ) && 
                (
                    this.CostUnitLimit == input.CostUnitLimit ||
                    (this.CostUnitLimit != null &&
                    this.CostUnitLimit.Equals(input.CostUnitLimit))
                ) && 
                (
                    this.CostUnitConsumed == input.CostUnitConsumed ||
                    (this.CostUnitConsumed != null &&
                    this.CostUnitConsumed.Equals(input.CostUnitConsumed))
                ) && 
                (
                    this.CostUnitPrice == input.CostUnitPrice ||
                    (this.CostUnitPrice != null &&
                    this.CostUnitPrice.Equals(input.CostUnitPrice))
                ) && 
                (
                    this.TipPercentage == input.TipPercentage ||
                    (this.TipPercentage != null &&
                    this.TipPercentage.Equals(input.TipPercentage))
                ) && 
                (
                    this.XrdBurned == input.XrdBurned ||
                    (this.XrdBurned != null &&
                    this.XrdBurned.Equals(input.XrdBurned))
                ) && 
                (
                    this.XrdTipped == input.XrdTipped ||
                    (this.XrdTipped != null &&
                    this.XrdTipped.Equals(input.XrdTipped))
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
                hashCode = (hashCode * 59) + this.LoanFullyRepaid.GetHashCode();
                if (this.CostUnitLimit != null)
                {
                    hashCode = (hashCode * 59) + this.CostUnitLimit.GetHashCode();
                }
                if (this.CostUnitConsumed != null)
                {
                    hashCode = (hashCode * 59) + this.CostUnitConsumed.GetHashCode();
                }
                if (this.CostUnitPrice != null)
                {
                    hashCode = (hashCode * 59) + this.CostUnitPrice.GetHashCode();
                }
                if (this.TipPercentage != null)
                {
                    hashCode = (hashCode * 59) + this.TipPercentage.GetHashCode();
                }
                if (this.XrdBurned != null)
                {
                    hashCode = (hashCode * 59) + this.XrdBurned.GetHashCode();
                }
                if (this.XrdTipped != null)
                {
                    hashCode = (hashCode * 59) + this.XrdTipped.GetHashCode();
                }
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
