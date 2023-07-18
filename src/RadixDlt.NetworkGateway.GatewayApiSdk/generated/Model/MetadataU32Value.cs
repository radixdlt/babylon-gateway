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
 * Babylon Gateway API - RCnet V2
 *
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers. For simple use cases, you can typically use the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs-babylon.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Integration and forward compatibility guarantees  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
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
using JsonSubTypes;
using FileParameter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// MetadataU32Value
    /// </summary>
    [DataContract(Name = "MetadataU32Value")]
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
    public partial class MetadataU32Value : MetadataTypedValue, IEquatable<MetadataU32Value>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataU32Value" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected MetadataU32Value() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataU32Value" /> class.
        /// </summary>
        /// <param name="value">value (required).</param>
        /// <param name="type">type (required) (default to MetadataValueType.U32).</param>
        public MetadataU32Value(string value = default(string), MetadataValueType type = MetadataValueType.U32) : base(type)
        {
            // to ensure "value" is required (not null)
            if (value == null)
            {
                throw new ArgumentNullException("value is a required property for MetadataU32Value and cannot be null");
            }
            this.Value = value;
        }

        /// <summary>
        /// Gets or Sets Value
        /// </summary>
        [DataMember(Name = "value", IsRequired = true, EmitDefaultValue = true)]
        public string Value { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class MetadataU32Value {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Value: ").Append(Value).Append("\n");
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
            return this.Equals(input as MetadataU32Value);
        }

        /// <summary>
        /// Returns true if MetadataU32Value instances are equal
        /// </summary>
        /// <param name="input">Instance of MetadataU32Value to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(MetadataU32Value input)
        {
            if (input == null)
            {
                return false;
            }
            return base.Equals(input) && 
                (
                    this.Value == input.Value ||
                    (this.Value != null &&
                    this.Value.Equals(input.Value))
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
                if (this.Value != null)
                {
                    hashCode = (hashCode * 59) + this.Value.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}