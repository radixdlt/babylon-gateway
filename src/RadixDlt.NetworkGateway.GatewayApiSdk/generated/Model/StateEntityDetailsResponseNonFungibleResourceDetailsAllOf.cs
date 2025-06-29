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
 * The version of the OpenAPI document: v1.10.3
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
    /// StateEntityDetailsResponseNonFungibleResourceDetailsAllOf
    /// </summary>
    [DataContract(Name = "StateEntityDetailsResponseNonFungibleResourceDetails_allOf")]
    public partial class StateEntityDetailsResponseNonFungibleResourceDetailsAllOf
    {

        /// <summary>
        /// Gets or Sets NonFungibleIdType
        /// </summary>
        [DataMember(Name = "non_fungible_id_type", IsRequired = true, EmitDefaultValue = true)]
        public NonFungibleIdType NonFungibleIdType { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseNonFungibleResourceDetailsAllOf" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected StateEntityDetailsResponseNonFungibleResourceDetailsAllOf() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseNonFungibleResourceDetailsAllOf" /> class.
        /// </summary>
        /// <param name="roleAssignments">roleAssignments (required).</param>
        /// <param name="nonFungibleIdType">nonFungibleIdType (required).</param>
        /// <param name="totalSupply">String-encoded decimal representing the amount of a related fungible resource. (required).</param>
        /// <param name="totalMinted">String-encoded decimal representing the amount of a related fungible resource. (required).</param>
        /// <param name="totalBurned">String-encoded decimal representing the amount of a related fungible resource. (required).</param>
        /// <param name="nonFungibleDataMutableFields">nonFungibleDataMutableFields (required).</param>
        /// <param name="twoWayLinkedDapps">twoWayLinkedDapps.</param>
        /// <param name="nativeResourceDetails">nativeResourceDetails.</param>
        public StateEntityDetailsResponseNonFungibleResourceDetailsAllOf(ComponentEntityRoleAssignments roleAssignments = default(ComponentEntityRoleAssignments), NonFungibleIdType nonFungibleIdType = default(NonFungibleIdType), string totalSupply = default(string), string totalMinted = default(string), string totalBurned = default(string), List<string> nonFungibleDataMutableFields = default(List<string>), TwoWayLinkedDappsCollection twoWayLinkedDapps = default(TwoWayLinkedDappsCollection), NativeResourceDetails nativeResourceDetails = default(NativeResourceDetails))
        {
            // to ensure "roleAssignments" is required (not null)
            if (roleAssignments == null)
            {
                throw new ArgumentNullException("roleAssignments is a required property for StateEntityDetailsResponseNonFungibleResourceDetailsAllOf and cannot be null");
            }
            this.RoleAssignments = roleAssignments;
            this.NonFungibleIdType = nonFungibleIdType;
            // to ensure "totalSupply" is required (not null)
            if (totalSupply == null)
            {
                throw new ArgumentNullException("totalSupply is a required property for StateEntityDetailsResponseNonFungibleResourceDetailsAllOf and cannot be null");
            }
            this.TotalSupply = totalSupply;
            // to ensure "totalMinted" is required (not null)
            if (totalMinted == null)
            {
                throw new ArgumentNullException("totalMinted is a required property for StateEntityDetailsResponseNonFungibleResourceDetailsAllOf and cannot be null");
            }
            this.TotalMinted = totalMinted;
            // to ensure "totalBurned" is required (not null)
            if (totalBurned == null)
            {
                throw new ArgumentNullException("totalBurned is a required property for StateEntityDetailsResponseNonFungibleResourceDetailsAllOf and cannot be null");
            }
            this.TotalBurned = totalBurned;
            // to ensure "nonFungibleDataMutableFields" is required (not null)
            if (nonFungibleDataMutableFields == null)
            {
                throw new ArgumentNullException("nonFungibleDataMutableFields is a required property for StateEntityDetailsResponseNonFungibleResourceDetailsAllOf and cannot be null");
            }
            this.NonFungibleDataMutableFields = nonFungibleDataMutableFields;
            this.TwoWayLinkedDapps = twoWayLinkedDapps;
            this.NativeResourceDetails = nativeResourceDetails;
        }

        /// <summary>
        /// Gets or Sets RoleAssignments
        /// </summary>
        [DataMember(Name = "role_assignments", IsRequired = true, EmitDefaultValue = true)]
        public ComponentEntityRoleAssignments RoleAssignments { get; set; }

        /// <summary>
        /// String-encoded decimal representing the amount of a related fungible resource.
        /// </summary>
        /// <value>String-encoded decimal representing the amount of a related fungible resource.</value>
        [DataMember(Name = "total_supply", IsRequired = true, EmitDefaultValue = true)]
        public string TotalSupply { get; set; }

        /// <summary>
        /// String-encoded decimal representing the amount of a related fungible resource.
        /// </summary>
        /// <value>String-encoded decimal representing the amount of a related fungible resource.</value>
        [DataMember(Name = "total_minted", IsRequired = true, EmitDefaultValue = true)]
        public string TotalMinted { get; set; }

        /// <summary>
        /// String-encoded decimal representing the amount of a related fungible resource.
        /// </summary>
        /// <value>String-encoded decimal representing the amount of a related fungible resource.</value>
        [DataMember(Name = "total_burned", IsRequired = true, EmitDefaultValue = true)]
        public string TotalBurned { get; set; }

        /// <summary>
        /// Gets or Sets NonFungibleDataMutableFields
        /// </summary>
        [DataMember(Name = "non_fungible_data_mutable_fields", IsRequired = true, EmitDefaultValue = true)]
        public List<string> NonFungibleDataMutableFields { get; set; }

        /// <summary>
        /// Gets or Sets TwoWayLinkedDapps
        /// </summary>
        [DataMember(Name = "two_way_linked_dapps", EmitDefaultValue = true)]
        public TwoWayLinkedDappsCollection TwoWayLinkedDapps { get; set; }

        /// <summary>
        /// Gets or Sets NativeResourceDetails
        /// </summary>
        [DataMember(Name = "native_resource_details", EmitDefaultValue = true)]
        public NativeResourceDetails NativeResourceDetails { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StateEntityDetailsResponseNonFungibleResourceDetailsAllOf {\n");
            sb.Append("  RoleAssignments: ").Append(RoleAssignments).Append("\n");
            sb.Append("  NonFungibleIdType: ").Append(NonFungibleIdType).Append("\n");
            sb.Append("  TotalSupply: ").Append(TotalSupply).Append("\n");
            sb.Append("  TotalMinted: ").Append(TotalMinted).Append("\n");
            sb.Append("  TotalBurned: ").Append(TotalBurned).Append("\n");
            sb.Append("  NonFungibleDataMutableFields: ").Append(NonFungibleDataMutableFields).Append("\n");
            sb.Append("  TwoWayLinkedDapps: ").Append(TwoWayLinkedDapps).Append("\n");
            sb.Append("  NativeResourceDetails: ").Append(NativeResourceDetails).Append("\n");
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
            return this.Equals(input as StateEntityDetailsResponseNonFungibleResourceDetailsAllOf);
        }

        /// <summary>
        /// Returns true if StateEntityDetailsResponseNonFungibleResourceDetailsAllOf instances are equal
        /// </summary>
        /// <param name="input">Instance of StateEntityDetailsResponseNonFungibleResourceDetailsAllOf to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StateEntityDetailsResponseNonFungibleResourceDetailsAllOf input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.RoleAssignments == input.RoleAssignments ||
                    (this.RoleAssignments != null &&
                    this.RoleAssignments.Equals(input.RoleAssignments))
                ) && 
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
                    this.TotalMinted == input.TotalMinted ||
                    (this.TotalMinted != null &&
                    this.TotalMinted.Equals(input.TotalMinted))
                ) && 
                (
                    this.TotalBurned == input.TotalBurned ||
                    (this.TotalBurned != null &&
                    this.TotalBurned.Equals(input.TotalBurned))
                ) && 
                (
                    this.NonFungibleDataMutableFields == input.NonFungibleDataMutableFields ||
                    this.NonFungibleDataMutableFields != null &&
                    input.NonFungibleDataMutableFields != null &&
                    this.NonFungibleDataMutableFields.SequenceEqual(input.NonFungibleDataMutableFields)
                ) && 
                (
                    this.TwoWayLinkedDapps == input.TwoWayLinkedDapps ||
                    (this.TwoWayLinkedDapps != null &&
                    this.TwoWayLinkedDapps.Equals(input.TwoWayLinkedDapps))
                ) && 
                (
                    this.NativeResourceDetails == input.NativeResourceDetails ||
                    (this.NativeResourceDetails != null &&
                    this.NativeResourceDetails.Equals(input.NativeResourceDetails))
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
                if (this.RoleAssignments != null)
                {
                    hashCode = (hashCode * 59) + this.RoleAssignments.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.NonFungibleIdType.GetHashCode();
                if (this.TotalSupply != null)
                {
                    hashCode = (hashCode * 59) + this.TotalSupply.GetHashCode();
                }
                if (this.TotalMinted != null)
                {
                    hashCode = (hashCode * 59) + this.TotalMinted.GetHashCode();
                }
                if (this.TotalBurned != null)
                {
                    hashCode = (hashCode * 59) + this.TotalBurned.GetHashCode();
                }
                if (this.NonFungibleDataMutableFields != null)
                {
                    hashCode = (hashCode * 59) + this.NonFungibleDataMutableFields.GetHashCode();
                }
                if (this.TwoWayLinkedDapps != null)
                {
                    hashCode = (hashCode * 59) + this.TwoWayLinkedDapps.GetHashCode();
                }
                if (this.NativeResourceDetails != null)
                {
                    hashCode = (hashCode * 59) + this.NativeResourceDetails.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
