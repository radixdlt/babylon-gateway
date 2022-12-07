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
    /// V0StatePackageResponse
    /// </summary>
    [DataContract(Name = "V0StatePackageResponse")]
    public partial class V0StatePackageResponse : IEquatable<V0StatePackageResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="V0StatePackageResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected V0StatePackageResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="V0StatePackageResponse" /> class.
        /// </summary>
        /// <param name="info">info (required).</param>
        /// <param name="royaltyConfig">royaltyConfig (required).</param>
        /// <param name="royaltyAccumulator">royaltyAccumulator (required).</param>
        /// <param name="metadata">metadata (required).</param>
        /// <param name="accessRules">accessRules (required).</param>
        public V0StatePackageResponse(Substate info = default(Substate), Substate royaltyConfig = default(Substate), Substate royaltyAccumulator = default(Substate), Substate metadata = default(Substate), Substate accessRules = default(Substate))
        {
            // to ensure "info" is required (not null)
            if (info == null)
            {
                throw new ArgumentNullException("info is a required property for V0StatePackageResponse and cannot be null");
            }
            this.Info = info;
            // to ensure "royaltyConfig" is required (not null)
            if (royaltyConfig == null)
            {
                throw new ArgumentNullException("royaltyConfig is a required property for V0StatePackageResponse and cannot be null");
            }
            this.RoyaltyConfig = royaltyConfig;
            // to ensure "royaltyAccumulator" is required (not null)
            if (royaltyAccumulator == null)
            {
                throw new ArgumentNullException("royaltyAccumulator is a required property for V0StatePackageResponse and cannot be null");
            }
            this.RoyaltyAccumulator = royaltyAccumulator;
            // to ensure "metadata" is required (not null)
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata is a required property for V0StatePackageResponse and cannot be null");
            }
            this.Metadata = metadata;
            // to ensure "accessRules" is required (not null)
            if (accessRules == null)
            {
                throw new ArgumentNullException("accessRules is a required property for V0StatePackageResponse and cannot be null");
            }
            this.AccessRules = accessRules;
        }

        /// <summary>
        /// Gets or Sets Info
        /// </summary>
        [DataMember(Name = "info", IsRequired = true, EmitDefaultValue = true)]
        public Substate Info { get; set; }

        /// <summary>
        /// Gets or Sets RoyaltyConfig
        /// </summary>
        [DataMember(Name = "royalty_config", IsRequired = true, EmitDefaultValue = true)]
        public Substate RoyaltyConfig { get; set; }

        /// <summary>
        /// Gets or Sets RoyaltyAccumulator
        /// </summary>
        [DataMember(Name = "royalty_accumulator", IsRequired = true, EmitDefaultValue = true)]
        public Substate RoyaltyAccumulator { get; set; }

        /// <summary>
        /// Gets or Sets Metadata
        /// </summary>
        [DataMember(Name = "metadata", IsRequired = true, EmitDefaultValue = true)]
        public Substate Metadata { get; set; }

        /// <summary>
        /// Gets or Sets AccessRules
        /// </summary>
        [DataMember(Name = "access_rules", IsRequired = true, EmitDefaultValue = true)]
        public Substate AccessRules { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class V0StatePackageResponse {\n");
            sb.Append("  Info: ").Append(Info).Append("\n");
            sb.Append("  RoyaltyConfig: ").Append(RoyaltyConfig).Append("\n");
            sb.Append("  RoyaltyAccumulator: ").Append(RoyaltyAccumulator).Append("\n");
            sb.Append("  Metadata: ").Append(Metadata).Append("\n");
            sb.Append("  AccessRules: ").Append(AccessRules).Append("\n");
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
            return this.Equals(input as V0StatePackageResponse);
        }

        /// <summary>
        /// Returns true if V0StatePackageResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of V0StatePackageResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(V0StatePackageResponse input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Info == input.Info ||
                    (this.Info != null &&
                    this.Info.Equals(input.Info))
                ) && 
                (
                    this.RoyaltyConfig == input.RoyaltyConfig ||
                    (this.RoyaltyConfig != null &&
                    this.RoyaltyConfig.Equals(input.RoyaltyConfig))
                ) && 
                (
                    this.RoyaltyAccumulator == input.RoyaltyAccumulator ||
                    (this.RoyaltyAccumulator != null &&
                    this.RoyaltyAccumulator.Equals(input.RoyaltyAccumulator))
                ) && 
                (
                    this.Metadata == input.Metadata ||
                    (this.Metadata != null &&
                    this.Metadata.Equals(input.Metadata))
                ) && 
                (
                    this.AccessRules == input.AccessRules ||
                    (this.AccessRules != null &&
                    this.AccessRules.Equals(input.AccessRules))
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
                if (this.Info != null)
                {
                    hashCode = (hashCode * 59) + this.Info.GetHashCode();
                }
                if (this.RoyaltyConfig != null)
                {
                    hashCode = (hashCode * 59) + this.RoyaltyConfig.GetHashCode();
                }
                if (this.RoyaltyAccumulator != null)
                {
                    hashCode = (hashCode * 59) + this.RoyaltyAccumulator.GetHashCode();
                }
                if (this.Metadata != null)
                {
                    hashCode = (hashCode * 59) + this.Metadata.GetHashCode();
                }
                if (this.AccessRules != null)
                {
                    hashCode = (hashCode * 59) + this.AccessRules.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
