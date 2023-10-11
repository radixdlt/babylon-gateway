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
    /// ResourceSpecificDepositBehaviour
    /// </summary>
    [DataContract(Name = "ResourceSpecificDepositBehaviour")]
    public partial class ResourceSpecificDepositBehaviour : IEquatable<ResourceSpecificDepositBehaviour>
    {

        /// <summary>
        /// Gets or Sets ResourcePreference
        /// </summary>
        [DataMember(Name = "resource_preference", EmitDefaultValue = true)]
        public ResourcePreference? ResourcePreference { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceSpecificDepositBehaviour" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ResourceSpecificDepositBehaviour() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceSpecificDepositBehaviour" /> class.
        /// </summary>
        /// <param name="resourcePreference">resourcePreference.</param>
        /// <param name="vaultExists">Whether the account contains a vault for the resource (even if 0 balance). This plays a role when &#x60;DefaultDepositRule&#x60; is &#x60;AllowExisting&#x60;.  (required).</param>
        /// <param name="isXrd">Whether the resource represents the native XRD fungible. XRD is a special case which does not require &#x60;vault_exists &#x3D; true&#x60; to satisfy the &#x60;AllowExisting&#x60; rule.  (required).</param>
        /// <param name="allowsTryDeposit">The fully resolved &#x60;try_deposit_*&#x60; ability of this resource (which takes all the inputs into account, including the authorized depositor badge, the default deposit rule and the above resource-specific circumstances).  (required).</param>
        public ResourceSpecificDepositBehaviour(ResourcePreference? resourcePreference = default(ResourcePreference?), bool vaultExists = default(bool), bool isXrd = default(bool), bool allowsTryDeposit = default(bool))
        {
            this.VaultExists = vaultExists;
            this.IsXrd = isXrd;
            this.AllowsTryDeposit = allowsTryDeposit;
            this.ResourcePreference = resourcePreference;
        }

        /// <summary>
        /// Whether the account contains a vault for the resource (even if 0 balance). This plays a role when &#x60;DefaultDepositRule&#x60; is &#x60;AllowExisting&#x60;. 
        /// </summary>
        /// <value>Whether the account contains a vault for the resource (even if 0 balance). This plays a role when &#x60;DefaultDepositRule&#x60; is &#x60;AllowExisting&#x60;. </value>
        [DataMember(Name = "vault_exists", IsRequired = true, EmitDefaultValue = true)]
        public bool VaultExists { get; set; }

        /// <summary>
        /// Whether the resource represents the native XRD fungible. XRD is a special case which does not require &#x60;vault_exists &#x3D; true&#x60; to satisfy the &#x60;AllowExisting&#x60; rule. 
        /// </summary>
        /// <value>Whether the resource represents the native XRD fungible. XRD is a special case which does not require &#x60;vault_exists &#x3D; true&#x60; to satisfy the &#x60;AllowExisting&#x60; rule. </value>
        [DataMember(Name = "is_xrd", IsRequired = true, EmitDefaultValue = true)]
        public bool IsXrd { get; set; }

        /// <summary>
        /// The fully resolved &#x60;try_deposit_*&#x60; ability of this resource (which takes all the inputs into account, including the authorized depositor badge, the default deposit rule and the above resource-specific circumstances). 
        /// </summary>
        /// <value>The fully resolved &#x60;try_deposit_*&#x60; ability of this resource (which takes all the inputs into account, including the authorized depositor badge, the default deposit rule and the above resource-specific circumstances). </value>
        [DataMember(Name = "allows_try_deposit", IsRequired = true, EmitDefaultValue = true)]
        public bool AllowsTryDeposit { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ResourceSpecificDepositBehaviour {\n");
            sb.Append("  ResourcePreference: ").Append(ResourcePreference).Append("\n");
            sb.Append("  VaultExists: ").Append(VaultExists).Append("\n");
            sb.Append("  IsXrd: ").Append(IsXrd).Append("\n");
            sb.Append("  AllowsTryDeposit: ").Append(AllowsTryDeposit).Append("\n");
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
            return this.Equals(input as ResourceSpecificDepositBehaviour);
        }

        /// <summary>
        /// Returns true if ResourceSpecificDepositBehaviour instances are equal
        /// </summary>
        /// <param name="input">Instance of ResourceSpecificDepositBehaviour to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ResourceSpecificDepositBehaviour input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.ResourcePreference == input.ResourcePreference ||
                    this.ResourcePreference.Equals(input.ResourcePreference)
                ) && 
                (
                    this.VaultExists == input.VaultExists ||
                    this.VaultExists.Equals(input.VaultExists)
                ) && 
                (
                    this.IsXrd == input.IsXrd ||
                    this.IsXrd.Equals(input.IsXrd)
                ) && 
                (
                    this.AllowsTryDeposit == input.AllowsTryDeposit ||
                    this.AllowsTryDeposit.Equals(input.AllowsTryDeposit)
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
                hashCode = (hashCode * 59) + this.ResourcePreference.GetHashCode();
                hashCode = (hashCode * 59) + this.VaultExists.GetHashCode();
                hashCode = (hashCode * 59) + this.IsXrd.GetHashCode();
                hashCode = (hashCode * 59) + this.AllowsTryDeposit.GetHashCode();
                return hashCode;
            }
        }

    }

}
