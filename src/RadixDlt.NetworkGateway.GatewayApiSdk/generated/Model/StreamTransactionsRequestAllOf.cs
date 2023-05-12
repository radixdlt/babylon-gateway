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
using FileParameter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// StreamTransactionsRequestAllOf
    /// </summary>
    [DataContract(Name = "StreamTransactionsRequest_allOf")]
    public partial class StreamTransactionsRequestAllOf : IEquatable<StreamTransactionsRequestAllOf>
    {
        /// <summary>
        /// Limit returned transactions by their kind. Defaults to &#x60;user&#x60;.
        /// </summary>
        /// <value>Limit returned transactions by their kind. Defaults to &#x60;user&#x60;.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum KindFilterEnum
        {
            /// <summary>
            /// Enum User for value: User
            /// </summary>
            [EnumMember(Value = "User")]
            User = 1,

            /// <summary>
            /// Enum EpochChange for value: EpochChange
            /// </summary>
            [EnumMember(Value = "EpochChange")]
            EpochChange = 2,

            /// <summary>
            /// Enum All for value: All
            /// </summary>
            [EnumMember(Value = "All")]
            All = 3

        }


        /// <summary>
        /// Limit returned transactions by their kind. Defaults to &#x60;user&#x60;.
        /// </summary>
        /// <value>Limit returned transactions by their kind. Defaults to &#x60;user&#x60;.</value>
        [DataMember(Name = "kind_filter", EmitDefaultValue = true)]
        public KindFilterEnum? KindFilter { get; set; }
        /// <summary>
        /// Defines TypeFilter
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeFilterEnum
        {
            /// <summary>
            /// Enum Deposit for value: Deposit
            /// </summary>
            [EnumMember(Value = "Deposit")]
            Deposit = 1,

            /// <summary>
            /// Enum Withdrawal for value: Withdrawal
            /// </summary>
            [EnumMember(Value = "Withdrawal")]
            Withdrawal = 2

        }


        /// <summary>
        /// Gets or Sets TypeFilter
        /// </summary>
        [DataMember(Name = "type_filter", EmitDefaultValue = true)]
        public TypeFilterEnum? TypeFilter { get; set; }
        /// <summary>
        /// Configures the order of returned result set. Defaults to &#x60;desc&#x60;.
        /// </summary>
        /// <value>Configures the order of returned result set. Defaults to &#x60;desc&#x60;.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum OrderEnum
        {
            /// <summary>
            /// Enum Asc for value: Asc
            /// </summary>
            [EnumMember(Value = "Asc")]
            Asc = 1,

            /// <summary>
            /// Enum Desc for value: Desc
            /// </summary>
            [EnumMember(Value = "Desc")]
            Desc = 2

        }


        /// <summary>
        /// Configures the order of returned result set. Defaults to &#x60;desc&#x60;.
        /// </summary>
        /// <value>Configures the order of returned result set. Defaults to &#x60;desc&#x60;.</value>
        [DataMember(Name = "order", EmitDefaultValue = true)]
        public OrderEnum? Order { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTransactionsRequestAllOf" /> class.
        /// </summary>
        /// <param name="fromLedgerState">fromLedgerState.</param>
        /// <param name="kindFilter">Limit returned transactions by their kind. Defaults to &#x60;user&#x60;..</param>
        /// <param name="entityId">entityId.</param>
        /// <param name="resourceEntityId">resourceEntityId.</param>
        /// <param name="typeFilter">typeFilter.</param>
        /// <param name="order">Configures the order of returned result set. Defaults to &#x60;desc&#x60;..</param>
        public StreamTransactionsRequestAllOf(LedgerStateSelector fromLedgerState = default(LedgerStateSelector), KindFilterEnum? kindFilter = default(KindFilterEnum?), long? entityId = default(long?), long? resourceEntityId = default(long?), TypeFilterEnum? typeFilter = default(TypeFilterEnum?), OrderEnum? order = default(OrderEnum?))
        {
            this.FromLedgerState = fromLedgerState;
            this.KindFilter = kindFilter;
            this.EntityId = entityId;
            this.ResourceEntityId = resourceEntityId;
            this.TypeFilter = typeFilter;
            this.Order = order;
        }

        /// <summary>
        /// Gets or Sets FromLedgerState
        /// </summary>
        [DataMember(Name = "from_ledger_state", EmitDefaultValue = true)]
        public LedgerStateSelector FromLedgerState { get; set; }

        /// <summary>
        /// Gets or Sets EntityId
        /// </summary>
        [DataMember(Name = "entity_id", EmitDefaultValue = true)]
        public long? EntityId { get; set; }

        /// <summary>
        /// Gets or Sets ResourceEntityId
        /// </summary>
        [DataMember(Name = "resource_entity_id", EmitDefaultValue = true)]
        public long? ResourceEntityId { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StreamTransactionsRequestAllOf {\n");
            sb.Append("  FromLedgerState: ").Append(FromLedgerState).Append("\n");
            sb.Append("  KindFilter: ").Append(KindFilter).Append("\n");
            sb.Append("  EntityId: ").Append(EntityId).Append("\n");
            sb.Append("  ResourceEntityId: ").Append(ResourceEntityId).Append("\n");
            sb.Append("  TypeFilter: ").Append(TypeFilter).Append("\n");
            sb.Append("  Order: ").Append(Order).Append("\n");
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
            return this.Equals(input as StreamTransactionsRequestAllOf);
        }

        /// <summary>
        /// Returns true if StreamTransactionsRequestAllOf instances are equal
        /// </summary>
        /// <param name="input">Instance of StreamTransactionsRequestAllOf to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StreamTransactionsRequestAllOf input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.FromLedgerState == input.FromLedgerState ||
                    (this.FromLedgerState != null &&
                    this.FromLedgerState.Equals(input.FromLedgerState))
                ) && 
                (
                    this.KindFilter == input.KindFilter ||
                    this.KindFilter.Equals(input.KindFilter)
                ) && 
                (
                    this.EntityId == input.EntityId ||
                    (this.EntityId != null &&
                    this.EntityId.Equals(input.EntityId))
                ) && 
                (
                    this.ResourceEntityId == input.ResourceEntityId ||
                    (this.ResourceEntityId != null &&
                    this.ResourceEntityId.Equals(input.ResourceEntityId))
                ) && 
                (
                    this.TypeFilter == input.TypeFilter ||
                    this.TypeFilter.Equals(input.TypeFilter)
                ) && 
                (
                    this.Order == input.Order ||
                    this.Order.Equals(input.Order)
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
                if (this.FromLedgerState != null)
                {
                    hashCode = (hashCode * 59) + this.FromLedgerState.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.KindFilter.GetHashCode();
                if (this.EntityId != null)
                {
                    hashCode = (hashCode * 59) + this.EntityId.GetHashCode();
                }
                if (this.ResourceEntityId != null)
                {
                    hashCode = (hashCode * 59) + this.ResourceEntityId.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.TypeFilter.GetHashCode();
                hashCode = (hashCode * 59) + this.Order.GetHashCode();
                return hashCode;
            }
        }

    }

}
