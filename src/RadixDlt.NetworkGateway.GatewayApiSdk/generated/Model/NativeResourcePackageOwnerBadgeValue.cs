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
 * The version of the OpenAPI document: v1.10.0
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
    /// NativeResourcePackageOwnerBadgeValue
    /// </summary>
    [DataContract(Name = "NativeResourcePackageOwnerBadgeValue")]
    [JsonConverter(typeof(JsonSubtypes), "kind")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceAccessControllerRecoveryBadgeValue), "AccessControllerRecoveryBadge")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceAccountOwnerBadgeValue), "AccountOwnerBadge")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceEd25519SignatureResourceValue), "Ed25519SignatureResource")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceGlobalCallerResourceValue), "GlobalCallerResource")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceIdentityOwnerBadgeValue), "IdentityOwnerBadge")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceMultiResourcePoolUnitValue), "MultiResourcePoolUnit")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceOneResourcePoolUnitValue), "OneResourcePoolUnit")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourcePackageOfDirectCallerResourceValue), "PackageOfDirectCallerResource")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourcePackageOwnerBadgeValue), "PackageOwnerBadge")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceSecp256k1SignatureResourceValue), "Secp256k1SignatureResource")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceSystemExecutionResourceValue), "SystemExecutionResource")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceTwoResourcePoolUnitValue), "TwoResourcePoolUnit")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceValidatorClaimNftValue), "ValidatorClaimNft")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceValidatorLiquidStakeUnitValue), "ValidatorLiquidStakeUnit")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceValidatorOwnerBadgeValue), "ValidatorOwnerBadge")]
    [JsonSubtypes.KnownSubType(typeof(NativeResourceXrdValue), "Xrd")]
    public partial class NativeResourcePackageOwnerBadgeValue : NativeResourceDetails, IEquatable<NativeResourcePackageOwnerBadgeValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NativeResourcePackageOwnerBadgeValue" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected NativeResourcePackageOwnerBadgeValue() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="NativeResourcePackageOwnerBadgeValue" /> class.
        /// </summary>
        /// <param name="kind">kind (required) (default to NativeResourceKind.PackageOwnerBadge).</param>
        public NativeResourcePackageOwnerBadgeValue(NativeResourceKind kind = NativeResourceKind.PackageOwnerBadge) : base(kind)
        {
        }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class NativeResourcePackageOwnerBadgeValue {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
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
            return this.Equals(input as NativeResourcePackageOwnerBadgeValue);
        }

        /// <summary>
        /// Returns true if NativeResourcePackageOwnerBadgeValue instances are equal
        /// </summary>
        /// <param name="input">Instance of NativeResourcePackageOwnerBadgeValue to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(NativeResourcePackageOwnerBadgeValue input)
        {
            if (input == null)
            {
                return false;
            }
            return base.Equals(input);
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
                return hashCode;
            }
        }

    }

}
