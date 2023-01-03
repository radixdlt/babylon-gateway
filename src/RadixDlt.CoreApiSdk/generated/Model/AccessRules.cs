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
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node. It is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Heavy load may impact the node's function.  If you require queries against historical ledger state, you may also wish to consider using the [Gateway API](https://betanet-gateway.redoc.ly/). 
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
    /// AccessRules
    /// </summary>
    [DataContract(Name = "AccessRules")]
    public partial class AccessRules : IEquatable<AccessRules>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessRules" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected AccessRules() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessRules" /> class.
        /// </summary>
        /// <param name="methodAuth">methodAuth (required).</param>
        /// <param name="groupedAuth">groupedAuth (required).</param>
        /// <param name="defaultAuth">defaultAuth (required).</param>
        /// <param name="methodAuthMutability">methodAuthMutability (required).</param>
        /// <param name="groupedAuthMutability">groupedAuthMutability (required).</param>
        /// <param name="defaultAuthMutability">defaultAuthMutability (required).</param>
        public AccessRules(List<MethodAuthEntry> methodAuth = default(List<MethodAuthEntry>), List<GroupedAuthEntry> groupedAuth = default(List<GroupedAuthEntry>), AccessRule defaultAuth = default(AccessRule), List<MethodAuthMutabilityEntry> methodAuthMutability = default(List<MethodAuthMutabilityEntry>), List<GroupedAuthEntry> groupedAuthMutability = default(List<GroupedAuthEntry>), AccessRule defaultAuthMutability = default(AccessRule))
        {
            // to ensure "methodAuth" is required (not null)
            if (methodAuth == null)
            {
                throw new ArgumentNullException("methodAuth is a required property for AccessRules and cannot be null");
            }
            this.MethodAuth = methodAuth;
            // to ensure "groupedAuth" is required (not null)
            if (groupedAuth == null)
            {
                throw new ArgumentNullException("groupedAuth is a required property for AccessRules and cannot be null");
            }
            this.GroupedAuth = groupedAuth;
            // to ensure "defaultAuth" is required (not null)
            if (defaultAuth == null)
            {
                throw new ArgumentNullException("defaultAuth is a required property for AccessRules and cannot be null");
            }
            this.DefaultAuth = defaultAuth;
            // to ensure "methodAuthMutability" is required (not null)
            if (methodAuthMutability == null)
            {
                throw new ArgumentNullException("methodAuthMutability is a required property for AccessRules and cannot be null");
            }
            this.MethodAuthMutability = methodAuthMutability;
            // to ensure "groupedAuthMutability" is required (not null)
            if (groupedAuthMutability == null)
            {
                throw new ArgumentNullException("groupedAuthMutability is a required property for AccessRules and cannot be null");
            }
            this.GroupedAuthMutability = groupedAuthMutability;
            // to ensure "defaultAuthMutability" is required (not null)
            if (defaultAuthMutability == null)
            {
                throw new ArgumentNullException("defaultAuthMutability is a required property for AccessRules and cannot be null");
            }
            this.DefaultAuthMutability = defaultAuthMutability;
        }

        /// <summary>
        /// Gets or Sets MethodAuth
        /// </summary>
        [DataMember(Name = "method_auth", IsRequired = true, EmitDefaultValue = true)]
        public List<MethodAuthEntry> MethodAuth { get; set; }

        /// <summary>
        /// Gets or Sets GroupedAuth
        /// </summary>
        [DataMember(Name = "grouped_auth", IsRequired = true, EmitDefaultValue = true)]
        public List<GroupedAuthEntry> GroupedAuth { get; set; }

        /// <summary>
        /// Gets or Sets DefaultAuth
        /// </summary>
        [DataMember(Name = "default_auth", IsRequired = true, EmitDefaultValue = true)]
        public AccessRule DefaultAuth { get; set; }

        /// <summary>
        /// Gets or Sets MethodAuthMutability
        /// </summary>
        [DataMember(Name = "method_auth_mutability", IsRequired = true, EmitDefaultValue = true)]
        public List<MethodAuthMutabilityEntry> MethodAuthMutability { get; set; }

        /// <summary>
        /// Gets or Sets GroupedAuthMutability
        /// </summary>
        [DataMember(Name = "grouped_auth_mutability", IsRequired = true, EmitDefaultValue = true)]
        public List<GroupedAuthEntry> GroupedAuthMutability { get; set; }

        /// <summary>
        /// Gets or Sets DefaultAuthMutability
        /// </summary>
        [DataMember(Name = "default_auth_mutability", IsRequired = true, EmitDefaultValue = true)]
        public AccessRule DefaultAuthMutability { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class AccessRules {\n");
            sb.Append("  MethodAuth: ").Append(MethodAuth).Append("\n");
            sb.Append("  GroupedAuth: ").Append(GroupedAuth).Append("\n");
            sb.Append("  DefaultAuth: ").Append(DefaultAuth).Append("\n");
            sb.Append("  MethodAuthMutability: ").Append(MethodAuthMutability).Append("\n");
            sb.Append("  GroupedAuthMutability: ").Append(GroupedAuthMutability).Append("\n");
            sb.Append("  DefaultAuthMutability: ").Append(DefaultAuthMutability).Append("\n");
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
            return this.Equals(input as AccessRules);
        }

        /// <summary>
        /// Returns true if AccessRules instances are equal
        /// </summary>
        /// <param name="input">Instance of AccessRules to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(AccessRules input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.MethodAuth == input.MethodAuth ||
                    this.MethodAuth != null &&
                    input.MethodAuth != null &&
                    this.MethodAuth.SequenceEqual(input.MethodAuth)
                ) && 
                (
                    this.GroupedAuth == input.GroupedAuth ||
                    this.GroupedAuth != null &&
                    input.GroupedAuth != null &&
                    this.GroupedAuth.SequenceEqual(input.GroupedAuth)
                ) && 
                (
                    this.DefaultAuth == input.DefaultAuth ||
                    (this.DefaultAuth != null &&
                    this.DefaultAuth.Equals(input.DefaultAuth))
                ) && 
                (
                    this.MethodAuthMutability == input.MethodAuthMutability ||
                    this.MethodAuthMutability != null &&
                    input.MethodAuthMutability != null &&
                    this.MethodAuthMutability.SequenceEqual(input.MethodAuthMutability)
                ) && 
                (
                    this.GroupedAuthMutability == input.GroupedAuthMutability ||
                    this.GroupedAuthMutability != null &&
                    input.GroupedAuthMutability != null &&
                    this.GroupedAuthMutability.SequenceEqual(input.GroupedAuthMutability)
                ) && 
                (
                    this.DefaultAuthMutability == input.DefaultAuthMutability ||
                    (this.DefaultAuthMutability != null &&
                    this.DefaultAuthMutability.Equals(input.DefaultAuthMutability))
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
                if (this.MethodAuth != null)
                {
                    hashCode = (hashCode * 59) + this.MethodAuth.GetHashCode();
                }
                if (this.GroupedAuth != null)
                {
                    hashCode = (hashCode * 59) + this.GroupedAuth.GetHashCode();
                }
                if (this.DefaultAuth != null)
                {
                    hashCode = (hashCode * 59) + this.DefaultAuth.GetHashCode();
                }
                if (this.MethodAuthMutability != null)
                {
                    hashCode = (hashCode * 59) + this.MethodAuthMutability.GetHashCode();
                }
                if (this.GroupedAuthMutability != null)
                {
                    hashCode = (hashCode * 59) + this.GroupedAuthMutability.GetHashCode();
                }
                if (this.DefaultAuthMutability != null)
                {
                    hashCode = (hashCode * 59) + this.DefaultAuthMutability.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
