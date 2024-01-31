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
 * The version of the OpenAPI document: v1.3.0
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
using JsonSubTypes;
using FileParameter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// StateEntityDetailsResponseComponentDetails
    /// </summary>
    [DataContract(Name = "StateEntityDetailsResponseComponentDetails")]
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubType(typeof(StateEntityDetailsResponseComponentDetails), "Component")]
    [JsonSubtypes.KnownSubType(typeof(StateEntityDetailsResponseFungibleResourceDetails), "FungibleResource")]
    [JsonSubtypes.KnownSubType(typeof(StateEntityDetailsResponseFungibleVaultDetails), "FungibleVault")]
    [JsonSubtypes.KnownSubType(typeof(StateEntityDetailsResponseNonFungibleResourceDetails), "NonFungibleResource")]
    [JsonSubtypes.KnownSubType(typeof(StateEntityDetailsResponseNonFungibleVaultDetails), "NonFungibleVault")]
    [JsonSubtypes.KnownSubType(typeof(StateEntityDetailsResponsePackageDetails), "Package")]
    public partial class StateEntityDetailsResponseComponentDetails : StateEntityDetailsResponseItemDetails, IEquatable<StateEntityDetailsResponseComponentDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseComponentDetails" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected StateEntityDetailsResponseComponentDetails() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseComponentDetails" /> class.
        /// </summary>
        /// <param name="packageAddress">Bech32m-encoded human readable version of the address..</param>
        /// <param name="blueprintName">blueprintName (required).</param>
        /// <param name="blueprintVersion">blueprintVersion (required).</param>
        /// <param name="state">A representation of a component&#39;s inner state. If this entity is a &#x60;GenericComponent&#x60;, this field will be in a programmatic JSON structure (you can deserialize it as a &#x60;ProgrammaticScryptoSborValue&#x60;). Otherwise, for \&quot;native\&quot; components such as &#x60;Account&#x60;, &#x60;Validator&#x60;, &#x60;AccessController&#x60;, &#x60;OneResourcePool&#x60;, &#x60;TwoResourcePool&#x60;, and &#x60;MultiResourcePool&#x60;, this field will be a custom JSON model defined in the Core API schema. .</param>
        /// <param name="roleAssignments">roleAssignments.</param>
        /// <param name="royaltyVaultBalance">String-encoded decimal representing the amount of a related fungible resource..</param>
        /// <param name="type">type (required) (default to StateEntityDetailsResponseItemDetailsType.Component).</param>
        public StateEntityDetailsResponseComponentDetails(string packageAddress = default(string), string blueprintName = default(string), string blueprintVersion = default(string), Object state = default(Object), ComponentEntityRoleAssignments roleAssignments = default(ComponentEntityRoleAssignments), string royaltyVaultBalance = default(string), StateEntityDetailsResponseItemDetailsType type = StateEntityDetailsResponseItemDetailsType.Component) : base(type)
        {
            // to ensure "blueprintName" is required (not null)
            if (blueprintName == null)
            {
                throw new ArgumentNullException("blueprintName is a required property for StateEntityDetailsResponseComponentDetails and cannot be null");
            }
            this.BlueprintName = blueprintName;
            // to ensure "blueprintVersion" is required (not null)
            if (blueprintVersion == null)
            {
                throw new ArgumentNullException("blueprintVersion is a required property for StateEntityDetailsResponseComponentDetails and cannot be null");
            }
            this.BlueprintVersion = blueprintVersion;
            this.PackageAddress = packageAddress;
            this.State = state;
            this.RoleAssignments = roleAssignments;
            this.RoyaltyVaultBalance = royaltyVaultBalance;
        }

        /// <summary>
        /// Bech32m-encoded human readable version of the address.
        /// </summary>
        /// <value>Bech32m-encoded human readable version of the address.</value>
        [DataMember(Name = "package_address", EmitDefaultValue = true)]
        public string PackageAddress { get; set; }

        /// <summary>
        /// Gets or Sets BlueprintName
        /// </summary>
        [DataMember(Name = "blueprint_name", IsRequired = true, EmitDefaultValue = true)]
        public string BlueprintName { get; set; }

        /// <summary>
        /// Gets or Sets BlueprintVersion
        /// </summary>
        [DataMember(Name = "blueprint_version", IsRequired = true, EmitDefaultValue = true)]
        public string BlueprintVersion { get; set; }

        /// <summary>
        /// A representation of a component&#39;s inner state. If this entity is a &#x60;GenericComponent&#x60;, this field will be in a programmatic JSON structure (you can deserialize it as a &#x60;ProgrammaticScryptoSborValue&#x60;). Otherwise, for \&quot;native\&quot; components such as &#x60;Account&#x60;, &#x60;Validator&#x60;, &#x60;AccessController&#x60;, &#x60;OneResourcePool&#x60;, &#x60;TwoResourcePool&#x60;, and &#x60;MultiResourcePool&#x60;, this field will be a custom JSON model defined in the Core API schema. 
        /// </summary>
        /// <value>A representation of a component&#39;s inner state. If this entity is a &#x60;GenericComponent&#x60;, this field will be in a programmatic JSON structure (you can deserialize it as a &#x60;ProgrammaticScryptoSborValue&#x60;). Otherwise, for \&quot;native\&quot; components such as &#x60;Account&#x60;, &#x60;Validator&#x60;, &#x60;AccessController&#x60;, &#x60;OneResourcePool&#x60;, &#x60;TwoResourcePool&#x60;, and &#x60;MultiResourcePool&#x60;, this field will be a custom JSON model defined in the Core API schema. </value>
        [DataMember(Name = "state", EmitDefaultValue = true)]
        public Object State { get; set; }

        /// <summary>
        /// Gets or Sets RoleAssignments
        /// </summary>
        [DataMember(Name = "role_assignments", EmitDefaultValue = true)]
        public ComponentEntityRoleAssignments RoleAssignments { get; set; }

        /// <summary>
        /// String-encoded decimal representing the amount of a related fungible resource.
        /// </summary>
        /// <value>String-encoded decimal representing the amount of a related fungible resource.</value>
        [DataMember(Name = "royalty_vault_balance", EmitDefaultValue = true)]
        public string RoyaltyVaultBalance { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StateEntityDetailsResponseComponentDetails {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  PackageAddress: ").Append(PackageAddress).Append("\n");
            sb.Append("  BlueprintName: ").Append(BlueprintName).Append("\n");
            sb.Append("  BlueprintVersion: ").Append(BlueprintVersion).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
            sb.Append("  RoleAssignments: ").Append(RoleAssignments).Append("\n");
            sb.Append("  RoyaltyVaultBalance: ").Append(RoyaltyVaultBalance).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public override string ToJson()
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
            return this.Equals(input as StateEntityDetailsResponseComponentDetails);
        }

        /// <summary>
        /// Returns true if StateEntityDetailsResponseComponentDetails instances are equal
        /// </summary>
        /// <param name="input">Instance of StateEntityDetailsResponseComponentDetails to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StateEntityDetailsResponseComponentDetails input)
        {
            if (input == null)
            {
                return false;
            }
            return base.Equals(input) && 
                (
                    this.PackageAddress == input.PackageAddress ||
                    (this.PackageAddress != null &&
                    this.PackageAddress.Equals(input.PackageAddress))
                ) && base.Equals(input) && 
                (
                    this.BlueprintName == input.BlueprintName ||
                    (this.BlueprintName != null &&
                    this.BlueprintName.Equals(input.BlueprintName))
                ) && base.Equals(input) && 
                (
                    this.BlueprintVersion == input.BlueprintVersion ||
                    (this.BlueprintVersion != null &&
                    this.BlueprintVersion.Equals(input.BlueprintVersion))
                ) && base.Equals(input) && 
                (
                    this.State == input.State ||
                    (this.State != null &&
                    this.State.Equals(input.State))
                ) && base.Equals(input) && 
                (
                    this.RoleAssignments == input.RoleAssignments ||
                    (this.RoleAssignments != null &&
                    this.RoleAssignments.Equals(input.RoleAssignments))
                ) && base.Equals(input) && 
                (
                    this.RoyaltyVaultBalance == input.RoyaltyVaultBalance ||
                    (this.RoyaltyVaultBalance != null &&
                    this.RoyaltyVaultBalance.Equals(input.RoyaltyVaultBalance))
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
                int hashCode = base.GetHashCode();
                if (this.PackageAddress != null)
                {
                    hashCode = (hashCode * 59) + this.PackageAddress.GetHashCode();
                }
                if (this.BlueprintName != null)
                {
                    hashCode = (hashCode * 59) + this.BlueprintName.GetHashCode();
                }
                if (this.BlueprintVersion != null)
                {
                    hashCode = (hashCode * 59) + this.BlueprintVersion.GetHashCode();
                }
                if (this.State != null)
                {
                    hashCode = (hashCode * 59) + this.State.GetHashCode();
                }
                if (this.RoleAssignments != null)
                {
                    hashCode = (hashCode * 59) + this.RoleAssignments.GetHashCode();
                }
                if (this.RoyaltyVaultBalance != null)
                {
                    hashCode = (hashCode * 59) + this.RoyaltyVaultBalance.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
