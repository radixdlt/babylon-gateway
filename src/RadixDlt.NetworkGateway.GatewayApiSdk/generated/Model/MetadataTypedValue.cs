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
 * The version of the OpenAPI document: v1.5.0
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
    /// MetadataTypedValue
    /// </summary>
    [DataContract(Name = "MetadataTypedValue")]
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubType(typeof(MetadataBoolValue), "Bool")]
    [JsonSubtypes.KnownSubType(typeof(MetadataBoolArrayValue), "BoolArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataDecimalValue), "Decimal")]
    [JsonSubtypes.KnownSubType(typeof(MetadataDecimalArrayValue), "DecimalArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataGlobalAddressValue), "GlobalAddress")]
    [JsonSubtypes.KnownSubType(typeof(MetadataGlobalAddressArrayValue), "GlobalAddressArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataI32Value), "I32")]
    [JsonSubtypes.KnownSubType(typeof(MetadataI32ArrayValue), "I32Array")]
    [JsonSubtypes.KnownSubType(typeof(MetadataI64Value), "I64")]
    [JsonSubtypes.KnownSubType(typeof(MetadataI64ArrayValue), "I64Array")]
    [JsonSubtypes.KnownSubType(typeof(MetadataInstantValue), "Instant")]
    [JsonSubtypes.KnownSubType(typeof(MetadataInstantArrayValue), "InstantArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataBoolArrayValue), "MetadataBoolArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataBoolValue), "MetadataBoolValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataDecimalArrayValue), "MetadataDecimalArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataDecimalValue), "MetadataDecimalValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataGlobalAddressArrayValue), "MetadataGlobalAddressArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataGlobalAddressValue), "MetadataGlobalAddressValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataI32ArrayValue), "MetadataI32ArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataI32Value), "MetadataI32Value")]
    [JsonSubtypes.KnownSubType(typeof(MetadataI64ArrayValue), "MetadataI64ArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataI64Value), "MetadataI64Value")]
    [JsonSubtypes.KnownSubType(typeof(MetadataInstantArrayValue), "MetadataInstantArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataInstantValue), "MetadataInstantValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataNonFungibleGlobalIdArrayValue), "MetadataNonFungibleGlobalIdArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataNonFungibleGlobalIdValue), "MetadataNonFungibleGlobalIdValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataNonFungibleLocalIdArrayValue), "MetadataNonFungibleLocalIdArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataNonFungibleLocalIdValue), "MetadataNonFungibleLocalIdValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataOriginArrayValue), "MetadataOriginArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataOriginValue), "MetadataOriginValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataPublicKeyArrayValue), "MetadataPublicKeyArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataPublicKeyHashArrayValue), "MetadataPublicKeyHashArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataPublicKeyHashValue), "MetadataPublicKeyHashValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataPublicKeyValue), "MetadataPublicKeyValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataStringArrayValue), "MetadataStringArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataStringValue), "MetadataStringValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU32ArrayValue), "MetadataU32ArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU32Value), "MetadataU32Value")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU64ArrayValue), "MetadataU64ArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU64Value), "MetadataU64Value")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU8ArrayValue), "MetadataU8ArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU8Value), "MetadataU8Value")]
    [JsonSubtypes.KnownSubType(typeof(MetadataUrlArrayValue), "MetadataUrlArrayValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataUrlValue), "MetadataUrlValue")]
    [JsonSubtypes.KnownSubType(typeof(MetadataNonFungibleGlobalIdValue), "NonFungibleGlobalId")]
    [JsonSubtypes.KnownSubType(typeof(MetadataNonFungibleGlobalIdArrayValue), "NonFungibleGlobalIdArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataNonFungibleLocalIdValue), "NonFungibleLocalId")]
    [JsonSubtypes.KnownSubType(typeof(MetadataNonFungibleLocalIdArrayValue), "NonFungibleLocalIdArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataOriginValue), "Origin")]
    [JsonSubtypes.KnownSubType(typeof(MetadataOriginArrayValue), "OriginArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataPublicKeyValue), "PublicKey")]
    [JsonSubtypes.KnownSubType(typeof(MetadataPublicKeyArrayValue), "PublicKeyArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataPublicKeyHashValue), "PublicKeyHash")]
    [JsonSubtypes.KnownSubType(typeof(MetadataPublicKeyHashArrayValue), "PublicKeyHashArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataStringValue), "String")]
    [JsonSubtypes.KnownSubType(typeof(MetadataStringArrayValue), "StringArray")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU32Value), "U32")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU32ArrayValue), "U32Array")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU64Value), "U64")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU64ArrayValue), "U64Array")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU8Value), "U8")]
    [JsonSubtypes.KnownSubType(typeof(MetadataU8ArrayValue), "U8Array")]
    [JsonSubtypes.KnownSubType(typeof(MetadataUrlValue), "Url")]
    [JsonSubtypes.KnownSubType(typeof(MetadataUrlArrayValue), "UrlArray")]
    public partial class MetadataTypedValue : IEquatable<MetadataTypedValue>
    {

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", IsRequired = true, EmitDefaultValue = true)]
        public MetadataValueType Type { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataTypedValue" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected MetadataTypedValue() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataTypedValue" /> class.
        /// </summary>
        /// <param name="type">type (required).</param>
        public MetadataTypedValue(MetadataValueType type = default(MetadataValueType))
        {
            this.Type = type;
        }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class MetadataTypedValue {\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
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
            return this.Equals(input as MetadataTypedValue);
        }

        /// <summary>
        /// Returns true if MetadataTypedValue instances are equal
        /// </summary>
        /// <param name="input">Instance of MetadataTypedValue to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(MetadataTypedValue input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Type == input.Type ||
                    this.Type.Equals(input.Type)
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
                hashCode = (hashCode * 59) + this.Type.GetHashCode();
                return hashCode;
            }
        }

    }

}
