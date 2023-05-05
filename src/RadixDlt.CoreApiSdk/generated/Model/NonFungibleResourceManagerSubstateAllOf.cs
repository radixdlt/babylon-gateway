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
 * Babylon Core API - RCnet V2
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  It is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  This version of the Core API belongs to the first release candidate of the Radix Babylon network (\"RCnet-V1\").  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` are guaranteed to be forward compatible to Babylon mainnet launch (and beyond). We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
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
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// NonFungibleResourceManagerSubstateAllOf
    /// </summary>
    [DataContract(Name = "NonFungibleResourceManagerSubstate_allOf")]
    public partial class NonFungibleResourceManagerSubstateAllOf : IEquatable<NonFungibleResourceManagerSubstateAllOf>
    {

        /// <summary>
        /// Gets or Sets NonFungibleIdType
        /// </summary>
        [DataMember(Name = "non_fungible_id_type", IsRequired = true, EmitDefaultValue = true)]
        public NonFungibleIdType NonFungibleIdType { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="NonFungibleResourceManagerSubstateAllOf" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected NonFungibleResourceManagerSubstateAllOf() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="NonFungibleResourceManagerSubstateAllOf" /> class.
        /// </summary>
        /// <param name="nonFungibleIdType">nonFungibleIdType (required).</param>
        /// <param name="totalSupply">The string-encoded decimal representing the total supply of this resource. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(256 - 1) &lt;&#x3D; m &lt; 2^(256 - 1)&#x60;.  (required).</param>
        /// <param name="nonFungibleDataTable">nonFungibleDataTable (required).</param>
        /// <param name="nonFungibleDataTypeIndex">nonFungibleDataTypeIndex (required).</param>
        /// <param name="nonFungibleDataMutableFields">The field names of the NF Metadata which are mutable.  (required).</param>
        public NonFungibleResourceManagerSubstateAllOf(NonFungibleIdType nonFungibleIdType = default(NonFungibleIdType), string totalSupply = default(string), EntityReference nonFungibleDataTable = default(EntityReference), LocalTypeIndex nonFungibleDataTypeIndex = default(LocalTypeIndex), List<string> nonFungibleDataMutableFields = default(List<string>))
        {
            this.NonFungibleIdType = nonFungibleIdType;
            // to ensure "totalSupply" is required (not null)
            if (totalSupply == null)
            {
                throw new ArgumentNullException("totalSupply is a required property for NonFungibleResourceManagerSubstateAllOf and cannot be null");
            }
            this.TotalSupply = totalSupply;
            // to ensure "nonFungibleDataTable" is required (not null)
            if (nonFungibleDataTable == null)
            {
                throw new ArgumentNullException("nonFungibleDataTable is a required property for NonFungibleResourceManagerSubstateAllOf and cannot be null");
            }
            this.NonFungibleDataTable = nonFungibleDataTable;
            // to ensure "nonFungibleDataTypeIndex" is required (not null)
            if (nonFungibleDataTypeIndex == null)
            {
                throw new ArgumentNullException("nonFungibleDataTypeIndex is a required property for NonFungibleResourceManagerSubstateAllOf and cannot be null");
            }
            this.NonFungibleDataTypeIndex = nonFungibleDataTypeIndex;
            // to ensure "nonFungibleDataMutableFields" is required (not null)
            if (nonFungibleDataMutableFields == null)
            {
                throw new ArgumentNullException("nonFungibleDataMutableFields is a required property for NonFungibleResourceManagerSubstateAllOf and cannot be null");
            }
            this.NonFungibleDataMutableFields = nonFungibleDataMutableFields;
        }

        /// <summary>
        /// The string-encoded decimal representing the total supply of this resource. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(256 - 1) &lt;&#x3D; m &lt; 2^(256 - 1)&#x60;. 
        /// </summary>
        /// <value>The string-encoded decimal representing the total supply of this resource. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(256 - 1) &lt;&#x3D; m &lt; 2^(256 - 1)&#x60;. </value>
        [DataMember(Name = "total_supply", IsRequired = true, EmitDefaultValue = true)]
        public string TotalSupply { get; set; }

        /// <summary>
        /// Gets or Sets NonFungibleDataTable
        /// </summary>
        [DataMember(Name = "non_fungible_data_table", IsRequired = true, EmitDefaultValue = true)]
        public EntityReference NonFungibleDataTable { get; set; }

        /// <summary>
        /// Gets or Sets NonFungibleDataTypeIndex
        /// </summary>
        [DataMember(Name = "non_fungible_data_type_index", IsRequired = true, EmitDefaultValue = true)]
        public LocalTypeIndex NonFungibleDataTypeIndex { get; set; }

        /// <summary>
        /// The field names of the NF Metadata which are mutable. 
        /// </summary>
        /// <value>The field names of the NF Metadata which are mutable. </value>
        [DataMember(Name = "non_fungible_data_mutable_fields", IsRequired = true, EmitDefaultValue = true)]
        public List<string> NonFungibleDataMutableFields { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class NonFungibleResourceManagerSubstateAllOf {\n");
            sb.Append("  NonFungibleIdType: ").Append(NonFungibleIdType).Append("\n");
            sb.Append("  TotalSupply: ").Append(TotalSupply).Append("\n");
            sb.Append("  NonFungibleDataTable: ").Append(NonFungibleDataTable).Append("\n");
            sb.Append("  NonFungibleDataTypeIndex: ").Append(NonFungibleDataTypeIndex).Append("\n");
            sb.Append("  NonFungibleDataMutableFields: ").Append(NonFungibleDataMutableFields).Append("\n");
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
            return this.Equals(input as NonFungibleResourceManagerSubstateAllOf);
        }

        /// <summary>
        /// Returns true if NonFungibleResourceManagerSubstateAllOf instances are equal
        /// </summary>
        /// <param name="input">Instance of NonFungibleResourceManagerSubstateAllOf to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(NonFungibleResourceManagerSubstateAllOf input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.NonFungibleIdType == input.NonFungibleIdType ||
                    this.NonFungibleIdType.Equals(input.NonFungibleIdType)
                ) && 
                (
                    this.TotalSupply == input.TotalSupply ||
                    (this.TotalSupply != null &&
                    this.TotalSupply.Equals(input.TotalSupply))
                ) && 
                (
                    this.NonFungibleDataTable == input.NonFungibleDataTable ||
                    (this.NonFungibleDataTable != null &&
                    this.NonFungibleDataTable.Equals(input.NonFungibleDataTable))
                ) && 
                (
                    this.NonFungibleDataTypeIndex == input.NonFungibleDataTypeIndex ||
                    (this.NonFungibleDataTypeIndex != null &&
                    this.NonFungibleDataTypeIndex.Equals(input.NonFungibleDataTypeIndex))
                ) && 
                (
                    this.NonFungibleDataMutableFields == input.NonFungibleDataMutableFields ||
                    this.NonFungibleDataMutableFields != null &&
                    input.NonFungibleDataMutableFields != null &&
                    this.NonFungibleDataMutableFields.SequenceEqual(input.NonFungibleDataMutableFields)
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
                hashCode = (hashCode * 59) + this.NonFungibleIdType.GetHashCode();
                if (this.TotalSupply != null)
                {
                    hashCode = (hashCode * 59) + this.TotalSupply.GetHashCode();
                }
                if (this.NonFungibleDataTable != null)
                {
                    hashCode = (hashCode * 59) + this.NonFungibleDataTable.GetHashCode();
                }
                if (this.NonFungibleDataTypeIndex != null)
                {
                    hashCode = (hashCode * 59) + this.NonFungibleDataTypeIndex.GetHashCode();
                }
                if (this.NonFungibleDataMutableFields != null)
                {
                    hashCode = (hashCode * 59) + this.NonFungibleDataMutableFields.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
