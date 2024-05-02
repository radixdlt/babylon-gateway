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
 * The version of the OpenAPI document: v1.2.0
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
    /// SystemParameters
    /// </summary>
    [DataContract(Name = "SystemParameters")]
    public partial class SystemParameters : IEquatable<SystemParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemParameters" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected SystemParameters() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemParameters" /> class.
        /// </summary>
        /// <param name="networkDefinition">networkDefinition (required).</param>
        /// <param name="costingParameters">costingParameters (required).</param>
        /// <param name="limitParameters">limitParameters (required).</param>
        /// <param name="xrdMaxPerFunctionRoyalty">The string-encoded decimal representing the maximum amount of XRD configurable for a single function&#39;s royalty. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        /// <param name="applyAdditionalCosting">Whether to adjust costs for querying fee reserve state and encoding bech32 addresses. (required).</param>
        public SystemParameters(NetworkDefinition networkDefinition = default(NetworkDefinition), SystemCostingParameters costingParameters = default(SystemCostingParameters), LimitParameters limitParameters = default(LimitParameters), string xrdMaxPerFunctionRoyalty = default(string), bool applyAdditionalCosting = default(bool))
        {
            // to ensure "networkDefinition" is required (not null)
            if (networkDefinition == null)
            {
                throw new ArgumentNullException("networkDefinition is a required property for SystemParameters and cannot be null");
            }
            this.NetworkDefinition = networkDefinition;
            // to ensure "costingParameters" is required (not null)
            if (costingParameters == null)
            {
                throw new ArgumentNullException("costingParameters is a required property for SystemParameters and cannot be null");
            }
            this.CostingParameters = costingParameters;
            // to ensure "limitParameters" is required (not null)
            if (limitParameters == null)
            {
                throw new ArgumentNullException("limitParameters is a required property for SystemParameters and cannot be null");
            }
            this.LimitParameters = limitParameters;
            // to ensure "xrdMaxPerFunctionRoyalty" is required (not null)
            if (xrdMaxPerFunctionRoyalty == null)
            {
                throw new ArgumentNullException("xrdMaxPerFunctionRoyalty is a required property for SystemParameters and cannot be null");
            }
            this.XrdMaxPerFunctionRoyalty = xrdMaxPerFunctionRoyalty;
            this.ApplyAdditionalCosting = applyAdditionalCosting;
        }

        /// <summary>
        /// Gets or Sets NetworkDefinition
        /// </summary>
        [DataMember(Name = "network_definition", IsRequired = true, EmitDefaultValue = true)]
        public NetworkDefinition NetworkDefinition { get; set; }

        /// <summary>
        /// Gets or Sets CostingParameters
        /// </summary>
        [DataMember(Name = "costing_parameters", IsRequired = true, EmitDefaultValue = true)]
        public SystemCostingParameters CostingParameters { get; set; }

        /// <summary>
        /// Gets or Sets LimitParameters
        /// </summary>
        [DataMember(Name = "limit_parameters", IsRequired = true, EmitDefaultValue = true)]
        public LimitParameters LimitParameters { get; set; }

        /// <summary>
        /// The string-encoded decimal representing the maximum amount of XRD configurable for a single function&#39;s royalty. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>The string-encoded decimal representing the maximum amount of XRD configurable for a single function&#39;s royalty. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "xrd_max_per_function_royalty", IsRequired = true, EmitDefaultValue = true)]
        public string XrdMaxPerFunctionRoyalty { get; set; }

        /// <summary>
        /// Whether to adjust costs for querying fee reserve state and encoding bech32 addresses.
        /// </summary>
        /// <value>Whether to adjust costs for querying fee reserve state and encoding bech32 addresses.</value>
        [DataMember(Name = "apply_additional_costing", IsRequired = true, EmitDefaultValue = true)]
        public bool ApplyAdditionalCosting { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class SystemParameters {\n");
            sb.Append("  NetworkDefinition: ").Append(NetworkDefinition).Append("\n");
            sb.Append("  CostingParameters: ").Append(CostingParameters).Append("\n");
            sb.Append("  LimitParameters: ").Append(LimitParameters).Append("\n");
            sb.Append("  XrdMaxPerFunctionRoyalty: ").Append(XrdMaxPerFunctionRoyalty).Append("\n");
            sb.Append("  ApplyAdditionalCosting: ").Append(ApplyAdditionalCosting).Append("\n");
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
            return this.Equals(input as SystemParameters);
        }

        /// <summary>
        /// Returns true if SystemParameters instances are equal
        /// </summary>
        /// <param name="input">Instance of SystemParameters to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(SystemParameters input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.NetworkDefinition == input.NetworkDefinition ||
                    (this.NetworkDefinition != null &&
                    this.NetworkDefinition.Equals(input.NetworkDefinition))
                ) && 
                (
                    this.CostingParameters == input.CostingParameters ||
                    (this.CostingParameters != null &&
                    this.CostingParameters.Equals(input.CostingParameters))
                ) && 
                (
                    this.LimitParameters == input.LimitParameters ||
                    (this.LimitParameters != null &&
                    this.LimitParameters.Equals(input.LimitParameters))
                ) && 
                (
                    this.XrdMaxPerFunctionRoyalty == input.XrdMaxPerFunctionRoyalty ||
                    (this.XrdMaxPerFunctionRoyalty != null &&
                    this.XrdMaxPerFunctionRoyalty.Equals(input.XrdMaxPerFunctionRoyalty))
                ) && 
                (
                    this.ApplyAdditionalCosting == input.ApplyAdditionalCosting ||
                    this.ApplyAdditionalCosting.Equals(input.ApplyAdditionalCosting)
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
                if (this.NetworkDefinition != null)
                {
                    hashCode = (hashCode * 59) + this.NetworkDefinition.GetHashCode();
                }
                if (this.CostingParameters != null)
                {
                    hashCode = (hashCode * 59) + this.CostingParameters.GetHashCode();
                }
                if (this.LimitParameters != null)
                {
                    hashCode = (hashCode * 59) + this.LimitParameters.GetHashCode();
                }
                if (this.XrdMaxPerFunctionRoyalty != null)
                {
                    hashCode = (hashCode * 59) + this.XrdMaxPerFunctionRoyalty.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.ApplyAdditionalCosting.GetHashCode();
                return hashCode;
            }
        }

    }

}
