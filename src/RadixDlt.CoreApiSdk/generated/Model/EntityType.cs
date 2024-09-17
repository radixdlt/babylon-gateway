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
 * Radix Core API
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.2.2
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
    /// Defines EntityType
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntityType
    {
        /// <summary>
        /// Enum GlobalPackage for value: GlobalPackage
        /// </summary>
        [EnumMember(Value = "GlobalPackage")]
        GlobalPackage = 1,

        /// <summary>
        /// Enum GlobalConsensusManager for value: GlobalConsensusManager
        /// </summary>
        [EnumMember(Value = "GlobalConsensusManager")]
        GlobalConsensusManager = 2,

        /// <summary>
        /// Enum GlobalValidator for value: GlobalValidator
        /// </summary>
        [EnumMember(Value = "GlobalValidator")]
        GlobalValidator = 3,

        /// <summary>
        /// Enum GlobalGenericComponent for value: GlobalGenericComponent
        /// </summary>
        [EnumMember(Value = "GlobalGenericComponent")]
        GlobalGenericComponent = 4,

        /// <summary>
        /// Enum GlobalAccount for value: GlobalAccount
        /// </summary>
        [EnumMember(Value = "GlobalAccount")]
        GlobalAccount = 5,

        /// <summary>
        /// Enum GlobalIdentity for value: GlobalIdentity
        /// </summary>
        [EnumMember(Value = "GlobalIdentity")]
        GlobalIdentity = 6,

        /// <summary>
        /// Enum GlobalAccessController for value: GlobalAccessController
        /// </summary>
        [EnumMember(Value = "GlobalAccessController")]
        GlobalAccessController = 7,

        /// <summary>
        /// Enum GlobalVirtualSecp256k1Account for value: GlobalVirtualSecp256k1Account
        /// </summary>
        [EnumMember(Value = "GlobalVirtualSecp256k1Account")]
        GlobalVirtualSecp256k1Account = 8,

        /// <summary>
        /// Enum GlobalVirtualSecp256k1Identity for value: GlobalVirtualSecp256k1Identity
        /// </summary>
        [EnumMember(Value = "GlobalVirtualSecp256k1Identity")]
        GlobalVirtualSecp256k1Identity = 9,

        /// <summary>
        /// Enum GlobalVirtualEd25519Account for value: GlobalVirtualEd25519Account
        /// </summary>
        [EnumMember(Value = "GlobalVirtualEd25519Account")]
        GlobalVirtualEd25519Account = 10,

        /// <summary>
        /// Enum GlobalVirtualEd25519Identity for value: GlobalVirtualEd25519Identity
        /// </summary>
        [EnumMember(Value = "GlobalVirtualEd25519Identity")]
        GlobalVirtualEd25519Identity = 11,

        /// <summary>
        /// Enum GlobalFungibleResource for value: GlobalFungibleResource
        /// </summary>
        [EnumMember(Value = "GlobalFungibleResource")]
        GlobalFungibleResource = 12,

        /// <summary>
        /// Enum InternalFungibleVault for value: InternalFungibleVault
        /// </summary>
        [EnumMember(Value = "InternalFungibleVault")]
        InternalFungibleVault = 13,

        /// <summary>
        /// Enum GlobalNonFungibleResource for value: GlobalNonFungibleResource
        /// </summary>
        [EnumMember(Value = "GlobalNonFungibleResource")]
        GlobalNonFungibleResource = 14,

        /// <summary>
        /// Enum InternalNonFungibleVault for value: InternalNonFungibleVault
        /// </summary>
        [EnumMember(Value = "InternalNonFungibleVault")]
        InternalNonFungibleVault = 15,

        /// <summary>
        /// Enum InternalGenericComponent for value: InternalGenericComponent
        /// </summary>
        [EnumMember(Value = "InternalGenericComponent")]
        InternalGenericComponent = 16,

        /// <summary>
        /// Enum InternalKeyValueStore for value: InternalKeyValueStore
        /// </summary>
        [EnumMember(Value = "InternalKeyValueStore")]
        InternalKeyValueStore = 17,

        /// <summary>
        /// Enum GlobalOneResourcePool for value: GlobalOneResourcePool
        /// </summary>
        [EnumMember(Value = "GlobalOneResourcePool")]
        GlobalOneResourcePool = 18,

        /// <summary>
        /// Enum GlobalTwoResourcePool for value: GlobalTwoResourcePool
        /// </summary>
        [EnumMember(Value = "GlobalTwoResourcePool")]
        GlobalTwoResourcePool = 19,

        /// <summary>
        /// Enum GlobalMultiResourcePool for value: GlobalMultiResourcePool
        /// </summary>
        [EnumMember(Value = "GlobalMultiResourcePool")]
        GlobalMultiResourcePool = 20,

        /// <summary>
        /// Enum GlobalTransactionTracker for value: GlobalTransactionTracker
        /// </summary>
        [EnumMember(Value = "GlobalTransactionTracker")]
        GlobalTransactionTracker = 21,

        /// <summary>
        /// Enum GlobalAccountLocker for value: GlobalAccountLocker
        /// </summary>
        [EnumMember(Value = "GlobalAccountLocker")]
        GlobalAccountLocker = 22

    }

}
