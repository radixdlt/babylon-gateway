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
 * The version of the OpenAPI document: v1.7.2
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
    /// Defines MetadataValueType
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MetadataValueType
    {
        /// <summary>
        /// Enum String for value: String
        /// </summary>
        [EnumMember(Value = "String")]
        String = 1,

        /// <summary>
        /// Enum Bool for value: Bool
        /// </summary>
        [EnumMember(Value = "Bool")]
        Bool = 2,

        /// <summary>
        /// Enum U8 for value: U8
        /// </summary>
        [EnumMember(Value = "U8")]
        U8 = 3,

        /// <summary>
        /// Enum U32 for value: U32
        /// </summary>
        [EnumMember(Value = "U32")]
        U32 = 4,

        /// <summary>
        /// Enum U64 for value: U64
        /// </summary>
        [EnumMember(Value = "U64")]
        U64 = 5,

        /// <summary>
        /// Enum I32 for value: I32
        /// </summary>
        [EnumMember(Value = "I32")]
        I32 = 6,

        /// <summary>
        /// Enum I64 for value: I64
        /// </summary>
        [EnumMember(Value = "I64")]
        I64 = 7,

        /// <summary>
        /// Enum Decimal for value: Decimal
        /// </summary>
        [EnumMember(Value = "Decimal")]
        Decimal = 8,

        /// <summary>
        /// Enum GlobalAddress for value: GlobalAddress
        /// </summary>
        [EnumMember(Value = "GlobalAddress")]
        GlobalAddress = 9,

        /// <summary>
        /// Enum PublicKey for value: PublicKey
        /// </summary>
        [EnumMember(Value = "PublicKey")]
        PublicKey = 10,

        /// <summary>
        /// Enum NonFungibleGlobalId for value: NonFungibleGlobalId
        /// </summary>
        [EnumMember(Value = "NonFungibleGlobalId")]
        NonFungibleGlobalId = 11,

        /// <summary>
        /// Enum NonFungibleLocalId for value: NonFungibleLocalId
        /// </summary>
        [EnumMember(Value = "NonFungibleLocalId")]
        NonFungibleLocalId = 12,

        /// <summary>
        /// Enum Instant for value: Instant
        /// </summary>
        [EnumMember(Value = "Instant")]
        Instant = 13,

        /// <summary>
        /// Enum Url for value: Url
        /// </summary>
        [EnumMember(Value = "Url")]
        Url = 14,

        /// <summary>
        /// Enum Origin for value: Origin
        /// </summary>
        [EnumMember(Value = "Origin")]
        Origin = 15,

        /// <summary>
        /// Enum PublicKeyHash for value: PublicKeyHash
        /// </summary>
        [EnumMember(Value = "PublicKeyHash")]
        PublicKeyHash = 16,

        /// <summary>
        /// Enum StringArray for value: StringArray
        /// </summary>
        [EnumMember(Value = "StringArray")]
        StringArray = 17,

        /// <summary>
        /// Enum BoolArray for value: BoolArray
        /// </summary>
        [EnumMember(Value = "BoolArray")]
        BoolArray = 18,

        /// <summary>
        /// Enum U8Array for value: U8Array
        /// </summary>
        [EnumMember(Value = "U8Array")]
        U8Array = 19,

        /// <summary>
        /// Enum U32Array for value: U32Array
        /// </summary>
        [EnumMember(Value = "U32Array")]
        U32Array = 20,

        /// <summary>
        /// Enum U64Array for value: U64Array
        /// </summary>
        [EnumMember(Value = "U64Array")]
        U64Array = 21,

        /// <summary>
        /// Enum I32Array for value: I32Array
        /// </summary>
        [EnumMember(Value = "I32Array")]
        I32Array = 22,

        /// <summary>
        /// Enum I64Array for value: I64Array
        /// </summary>
        [EnumMember(Value = "I64Array")]
        I64Array = 23,

        /// <summary>
        /// Enum DecimalArray for value: DecimalArray
        /// </summary>
        [EnumMember(Value = "DecimalArray")]
        DecimalArray = 24,

        /// <summary>
        /// Enum GlobalAddressArray for value: GlobalAddressArray
        /// </summary>
        [EnumMember(Value = "GlobalAddressArray")]
        GlobalAddressArray = 25,

        /// <summary>
        /// Enum PublicKeyArray for value: PublicKeyArray
        /// </summary>
        [EnumMember(Value = "PublicKeyArray")]
        PublicKeyArray = 26,

        /// <summary>
        /// Enum NonFungibleGlobalIdArray for value: NonFungibleGlobalIdArray
        /// </summary>
        [EnumMember(Value = "NonFungibleGlobalIdArray")]
        NonFungibleGlobalIdArray = 27,

        /// <summary>
        /// Enum NonFungibleLocalIdArray for value: NonFungibleLocalIdArray
        /// </summary>
        [EnumMember(Value = "NonFungibleLocalIdArray")]
        NonFungibleLocalIdArray = 28,

        /// <summary>
        /// Enum InstantArray for value: InstantArray
        /// </summary>
        [EnumMember(Value = "InstantArray")]
        InstantArray = 29,

        /// <summary>
        /// Enum UrlArray for value: UrlArray
        /// </summary>
        [EnumMember(Value = "UrlArray")]
        UrlArray = 30,

        /// <summary>
        /// Enum OriginArray for value: OriginArray
        /// </summary>
        [EnumMember(Value = "OriginArray")]
        OriginArray = 31,

        /// <summary>
        /// Enum PublicKeyHashArray for value: PublicKeyHashArray
        /// </summary>
        [EnumMember(Value = "PublicKeyHashArray")]
        PublicKeyHashArray = 32

    }

}
