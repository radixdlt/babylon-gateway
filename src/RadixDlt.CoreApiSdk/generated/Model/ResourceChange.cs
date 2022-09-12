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
    /// ResourceChange
    /// </summary>
    [DataContract(Name = "ResourceChange")]
    public partial class ResourceChange : IEquatable<ResourceChange>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceChange" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ResourceChange() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceChange" /> class.
        /// </summary>
        /// <param name="resourceAddress">The Bech32m-encoded human readable version of the resource address (required).</param>
        /// <param name="componentAddress">The Bech32m-encoded human readable version of the component address (required).</param>
        /// <param name="vaultEntityId">vaultEntityId (required).</param>
        /// <param name="amountAttos">A decimal-string-encoded integer between 0 and 2^255-1, which represents the total number of 10^(-18) subunits in the XRD amount put or taken from the vault  (required).</param>
        public ResourceChange(string resourceAddress = default(string), string componentAddress = default(string), EntityId vaultEntityId = default(EntityId), string amountAttos = default(string))
        {
            // to ensure "resourceAddress" is required (not null)
            if (resourceAddress == null)
            {
                throw new ArgumentNullException("resourceAddress is a required property for ResourceChange and cannot be null");
            }
            this.ResourceAddress = resourceAddress;
            // to ensure "componentAddress" is required (not null)
            if (componentAddress == null)
            {
                throw new ArgumentNullException("componentAddress is a required property for ResourceChange and cannot be null");
            }
            this.ComponentAddress = componentAddress;
            // to ensure "vaultEntityId" is required (not null)
            if (vaultEntityId == null)
            {
                throw new ArgumentNullException("vaultEntityId is a required property for ResourceChange and cannot be null");
            }
            this.VaultEntityId = vaultEntityId;
            // to ensure "amountAttos" is required (not null)
            if (amountAttos == null)
            {
                throw new ArgumentNullException("amountAttos is a required property for ResourceChange and cannot be null");
            }
            this.AmountAttos = amountAttos;
        }

        /// <summary>
        /// The Bech32m-encoded human readable version of the resource address
        /// </summary>
        /// <value>The Bech32m-encoded human readable version of the resource address</value>
        [DataMember(Name = "resource_address", IsRequired = true, EmitDefaultValue = true)]
        public string ResourceAddress { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable version of the component address
        /// </summary>
        /// <value>The Bech32m-encoded human readable version of the component address</value>
        [DataMember(Name = "component_address", IsRequired = true, EmitDefaultValue = true)]
        public string ComponentAddress { get; set; }

        /// <summary>
        /// Gets or Sets VaultEntityId
        /// </summary>
        [DataMember(Name = "vault_entity_id", IsRequired = true, EmitDefaultValue = true)]
        public EntityId VaultEntityId { get; set; }

        /// <summary>
        /// A decimal-string-encoded integer between 0 and 2^255-1, which represents the total number of 10^(-18) subunits in the XRD amount put or taken from the vault 
        /// </summary>
        /// <value>A decimal-string-encoded integer between 0 and 2^255-1, which represents the total number of 10^(-18) subunits in the XRD amount put or taken from the vault </value>
        [DataMember(Name = "amount_attos", IsRequired = true, EmitDefaultValue = true)]
        public string AmountAttos { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ResourceChange {\n");
            sb.Append("  ResourceAddress: ").Append(ResourceAddress).Append("\n");
            sb.Append("  ComponentAddress: ").Append(ComponentAddress).Append("\n");
            sb.Append("  VaultEntityId: ").Append(VaultEntityId).Append("\n");
            sb.Append("  AmountAttos: ").Append(AmountAttos).Append("\n");
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
            return this.Equals(input as ResourceChange);
        }

        /// <summary>
        /// Returns true if ResourceChange instances are equal
        /// </summary>
        /// <param name="input">Instance of ResourceChange to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ResourceChange input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.ResourceAddress == input.ResourceAddress ||
                    (this.ResourceAddress != null &&
                    this.ResourceAddress.Equals(input.ResourceAddress))
                ) && 
                (
                    this.ComponentAddress == input.ComponentAddress ||
                    (this.ComponentAddress != null &&
                    this.ComponentAddress.Equals(input.ComponentAddress))
                ) && 
                (
                    this.VaultEntityId == input.VaultEntityId ||
                    (this.VaultEntityId != null &&
                    this.VaultEntityId.Equals(input.VaultEntityId))
                ) && 
                (
                    this.AmountAttos == input.AmountAttos ||
                    (this.AmountAttos != null &&
                    this.AmountAttos.Equals(input.AmountAttos))
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
                if (this.ResourceAddress != null)
                {
                    hashCode = (hashCode * 59) + this.ResourceAddress.GetHashCode();
                }
                if (this.ComponentAddress != null)
                {
                    hashCode = (hashCode * 59) + this.ComponentAddress.GetHashCode();
                }
                if (this.VaultEntityId != null)
                {
                    hashCode = (hashCode * 59) + this.VaultEntityId.GetHashCode();
                }
                if (this.AmountAttos != null)
                {
                    hashCode = (hashCode * 59) + this.AmountAttos.GetHashCode();
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
