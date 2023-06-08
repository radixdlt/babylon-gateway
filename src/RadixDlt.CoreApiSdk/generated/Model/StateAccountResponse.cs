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
    /// StateAccountResponse
    /// </summary>
    [DataContract(Name = "StateAccountResponse")]
    public partial class StateAccountResponse : IEquatable<StateAccountResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateAccountResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected StateAccountResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StateAccountResponse" /> class.
        /// </summary>
        /// <param name="info">info (required).</param>
        /// <param name="accessRules">accessRules (required).</param>
        /// <param name="state">state (required).</param>
        /// <param name="vaults">All of the account&#39;s vaults (required).</param>
        public StateAccountResponse(Substate info = default(Substate), Substate accessRules = default(Substate), Substate state = default(Substate), List<VaultBalance> vaults = default(List<VaultBalance>))
        {
            // to ensure "info" is required (not null)
            if (info == null)
            {
                throw new ArgumentNullException("info is a required property for StateAccountResponse and cannot be null");
            }
            this.Info = info;
            // to ensure "accessRules" is required (not null)
            if (accessRules == null)
            {
                throw new ArgumentNullException("accessRules is a required property for StateAccountResponse and cannot be null");
            }
            this.AccessRules = accessRules;
            // to ensure "state" is required (not null)
            if (state == null)
            {
                throw new ArgumentNullException("state is a required property for StateAccountResponse and cannot be null");
            }
            this.State = state;
            // to ensure "vaults" is required (not null)
            if (vaults == null)
            {
                throw new ArgumentNullException("vaults is a required property for StateAccountResponse and cannot be null");
            }
            this.Vaults = vaults;
        }

        /// <summary>
        /// Gets or Sets Info
        /// </summary>
        [DataMember(Name = "info", IsRequired = true, EmitDefaultValue = true)]
        public Substate Info { get; set; }

        /// <summary>
        /// Gets or Sets AccessRules
        /// </summary>
        [DataMember(Name = "access_rules", IsRequired = true, EmitDefaultValue = true)]
        public Substate AccessRules { get; set; }

        /// <summary>
        /// Gets or Sets State
        /// </summary>
        [DataMember(Name = "state", IsRequired = true, EmitDefaultValue = true)]
        public Substate State { get; set; }

        /// <summary>
        /// All of the account&#39;s vaults
        /// </summary>
        /// <value>All of the account&#39;s vaults</value>
        [DataMember(Name = "vaults", IsRequired = true, EmitDefaultValue = true)]
        public List<VaultBalance> Vaults { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StateAccountResponse {\n");
            sb.Append("  Info: ").Append(Info).Append("\n");
            sb.Append("  AccessRules: ").Append(AccessRules).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
            sb.Append("  Vaults: ").Append(Vaults).Append("\n");
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
            return this.Equals(input as StateAccountResponse);
        }

        /// <summary>
        /// Returns true if StateAccountResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of StateAccountResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StateAccountResponse input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Info == input.Info ||
                    (this.Info != null &&
                    this.Info.Equals(input.Info))
                ) && 
                (
                    this.AccessRules == input.AccessRules ||
                    (this.AccessRules != null &&
                    this.AccessRules.Equals(input.AccessRules))
                ) && 
                (
                    this.State == input.State ||
                    (this.State != null &&
                    this.State.Equals(input.State))
                ) && 
                (
                    this.Vaults == input.Vaults ||
                    this.Vaults != null &&
                    input.Vaults != null &&
                    this.Vaults.SequenceEqual(input.Vaults)
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
                if (this.Info != null)
                {
                    hashCode = (hashCode * 59) + this.Info.GetHashCode();
                }
                if (this.AccessRules != null)
                {
                    hashCode = (hashCode * 59) + this.AccessRules.GetHashCode();
                }
                if (this.State != null)
                {
                    hashCode = (hashCode * 59) + this.State.GetHashCode();
                }
                if (this.Vaults != null)
                {
                    hashCode = (hashCode * 59) + this.Vaults.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
