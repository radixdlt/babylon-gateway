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

// <auto-generated>
/*
 * Radix Core API - Babylon
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.0.4
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using RadixDlt.CoreApiSdk.GenericHost.Client;

namespace RadixDlt.CoreApiSdk.GenericHost.Model
{

    /// <summary>
    /// NonFungibleLocalId
    /// </summary>
    // moje start
    // moje end
public partial class NonFungibleLocalId
{
    /// <summary>
        /// Initializes a new instance of the <see cref="NonFungibleLocalId" /> class.
        /// </summary>
        /// <param name="idType">idType</param>
        /// <param name="sborHex">The hex-encoded SBOR-encoded bytes of its non-fungible id</param>
        /// <param name="simpleRep">The simple string representation of the non-fungible id. * For string ids, this is &#x60;&lt;the-string-id&gt;&#x60; * For integer ids, this is &#x60;#the-integer-id#&#x60; * For bytes ids, this is &#x60;[the-lower-case-hex-representation]&#x60; * For RUID ids, this is &#x60;{...-...-...-...}&#x60; where &#x60;...&#x60; are each 16 hex characters. A given non-fungible resource has a fixed &#x60;NonFungibleIdType&#x60;, so this representation uniquely identifies this non-fungible under the given resource address. </param>
        [JsonConstructor]
    public NonFungibleLocalId(NonFungibleIdType idType, string sborHex, string simpleRep)
    {
            IdType = idType;
            SborHex = sborHex;
            SimpleRep = simpleRep;
    OnCreated();
    }

partial void OnCreated();

        /// <summary>
            /// Gets or Sets IdType
            /// </summary>
        [JsonPropertyName("id_type")]
        public NonFungibleIdType IdType { get; set; }

            /// <summary>
                /// The hex-encoded SBOR-encoded bytes of its non-fungible id
                /// </summary>
                /// <value>The hex-encoded SBOR-encoded bytes of its non-fungible id</value>
            [JsonPropertyName("sbor_hex")]
            public string SborHex { get; set; }

            /// <summary>
                /// The simple string representation of the non-fungible id. * For string ids, this is &#x60;&lt;the-string-id&gt;&#x60; * For integer ids, this is &#x60;#the-integer-id#&#x60; * For bytes ids, this is &#x60;[the-lower-case-hex-representation]&#x60; * For RUID ids, this is &#x60;{...-...-...-...}&#x60; where &#x60;...&#x60; are each 16 hex characters. A given non-fungible resource has a fixed &#x60;NonFungibleIdType&#x60;, so this representation uniquely identifies this non-fungible under the given resource address. 
                /// </summary>
                /// <value>The simple string representation of the non-fungible id. * For string ids, this is &#x60;&lt;the-string-id&gt;&#x60; * For integer ids, this is &#x60;#the-integer-id#&#x60; * For bytes ids, this is &#x60;[the-lower-case-hex-representation]&#x60; * For RUID ids, this is &#x60;{...-...-...-...}&#x60; where &#x60;...&#x60; are each 16 hex characters. A given non-fungible resource has a fixed &#x60;NonFungibleIdType&#x60;, so this representation uniquely identifies this non-fungible under the given resource address. </value>
            [JsonPropertyName("simple_rep")]
            public string SimpleRep { get; set; }

/// <summary>
    /// Returns the string presentation of the object
    /// </summary>
/// <returns>String presentation of the object</returns>
public override string ToString()
{
StringBuilder sb = new StringBuilder();
sb.Append("class NonFungibleLocalId {\n");
    sb.Append("  IdType: ").Append(IdType).Append("\n");
    sb.Append("  SborHex: ").Append(SborHex).Append("\n");
    sb.Append("  SimpleRep: ").Append(SimpleRep).Append("\n");
sb.Append("}\n");
return sb.ToString();
}
}


    /// <summary>
    /// A Json converter for type <see cref="NonFungibleLocalId" />
    /// </summary>
    public class NonFungibleLocalIdJsonConverter : JsonConverter<NonFungibleLocalId>
    {
        /// <summary>
        /// Deserializes json to <see cref="NonFungibleLocalId" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override NonFungibleLocalId Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            Option<NonFungibleIdType?> idType = default;
            Option<string?> sborHex = default;
            Option<string?> simpleRep = default;

            while (utf8JsonReader.Read())
            {
                if (startingTokenType == JsonTokenType.StartObject && utf8JsonReader.TokenType == JsonTokenType.EndObject && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (startingTokenType == JsonTokenType.StartArray && utf8JsonReader.TokenType == JsonTokenType.EndArray && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (utf8JsonReader.TokenType == JsonTokenType.PropertyName && currentDepth == utf8JsonReader.CurrentDepth - 1)
                {
                    string? localVarJsonPropertyName = utf8JsonReader.GetString();
                    utf8JsonReader.Read();

                    switch (localVarJsonPropertyName)
                    {
                        case "id_type":
                            string? idTypeRawValue = utf8JsonReader.GetString();
                            if (idTypeRawValue != null)
                                idType = new Option<NonFungibleIdType?>(NonFungibleIdTypeValueConverter.FromStringOrDefault(idTypeRawValue));
                            break;
                        case "sbor_hex":
                            sborHex = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        case "simple_rep":
                            simpleRep = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!idType.IsSet)
                throw new ArgumentException("Property is required for class NonFungibleLocalId.", nameof(idType));

            if (!sborHex.IsSet)
                throw new ArgumentException("Property is required for class NonFungibleLocalId.", nameof(sborHex));

            if (!simpleRep.IsSet)
                throw new ArgumentException("Property is required for class NonFungibleLocalId.", nameof(simpleRep));

            if (idType.IsSet && idType.Value == null)
                throw new ArgumentNullException(nameof(idType), "Property is not nullable for class NonFungibleLocalId.");

            if (sborHex.IsSet && sborHex.Value == null)
                throw new ArgumentNullException(nameof(sborHex), "Property is not nullable for class NonFungibleLocalId.");

            if (simpleRep.IsSet && simpleRep.Value == null)
                throw new ArgumentNullException(nameof(simpleRep), "Property is not nullable for class NonFungibleLocalId.");

            return new NonFungibleLocalId(idType.Value!.Value!, sborHex.Value!, simpleRep.Value!);
        }

        /// <summary>
        /// Serializes a <see cref="NonFungibleLocalId" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="nonFungibleLocalId"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, NonFungibleLocalId nonFungibleLocalId, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(ref writer, nonFungibleLocalId, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="NonFungibleLocalId" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="nonFungibleLocalId"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(ref Utf8JsonWriter writer, NonFungibleLocalId nonFungibleLocalId, JsonSerializerOptions jsonSerializerOptions)
        {
            if (nonFungibleLocalId.SborHex == null)
                throw new ArgumentNullException(nameof(nonFungibleLocalId.SborHex), "Property is required for class NonFungibleLocalId.");

            if (nonFungibleLocalId.SimpleRep == null)
                throw new ArgumentNullException(nameof(nonFungibleLocalId.SimpleRep), "Property is required for class NonFungibleLocalId.");

            var idTypeRawValue = NonFungibleIdTypeValueConverter.ToJsonValue(nonFungibleLocalId.IdType);
            writer.WriteString("id_type", idTypeRawValue);

            writer.WriteString("sbor_hex", nonFungibleLocalId.SborHex);

            writer.WriteString("simple_rep", nonFungibleLocalId.SimpleRep);
        }
    }
}
