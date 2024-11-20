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
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// ExecutedScenarioTransaction
    /// </summary>
    [DataContract(Name = "ExecutedScenarioTransaction")]
    public partial class ExecutedScenarioTransaction : IEquatable<ExecutedScenarioTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutedScenarioTransaction" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ExecutedScenarioTransaction() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutedScenarioTransaction" /> class.
        /// </summary>
        /// <param name="logicalName">logicalName (required).</param>
        /// <param name="stateVersion">stateVersion (required).</param>
        /// <param name="intentHash">The hex-encoded transaction intent hash for a user transaction, also known as the transaction id. This hash identifies the core \&quot;intent\&quot; of the transaction. Each transaction intent can only be committed once. This hash gets signed by any signatories on the transaction, to create the signed intent.  (required).</param>
        /// <param name="intentHashBech32m">The Bech32m-encoded human readable &#x60;TransactionIntentHash&#x60;. (required).</param>
        public ExecutedScenarioTransaction(string logicalName = default(string), long stateVersion = default(long), string intentHash = default(string), string intentHashBech32m = default(string))
        {
            // to ensure "logicalName" is required (not null)
            if (logicalName == null)
            {
                throw new ArgumentNullException("logicalName is a required property for ExecutedScenarioTransaction and cannot be null");
            }
            this.LogicalName = logicalName;
            this.StateVersion = stateVersion;
            // to ensure "intentHash" is required (not null)
            if (intentHash == null)
            {
                throw new ArgumentNullException("intentHash is a required property for ExecutedScenarioTransaction and cannot be null");
            }
            this.IntentHash = intentHash;
            // to ensure "intentHashBech32m" is required (not null)
            if (intentHashBech32m == null)
            {
                throw new ArgumentNullException("intentHashBech32m is a required property for ExecutedScenarioTransaction and cannot be null");
            }
            this.IntentHashBech32m = intentHashBech32m;
        }

        /// <summary>
        /// Gets or Sets LogicalName
        /// </summary>
        [DataMember(Name = "logical_name", IsRequired = true, EmitDefaultValue = true)]
        public string LogicalName { get; set; }

        /// <summary>
        /// Gets or Sets StateVersion
        /// </summary>
        [DataMember(Name = "state_version", IsRequired = true, EmitDefaultValue = true)]
        public long StateVersion { get; set; }

        /// <summary>
        /// The hex-encoded transaction intent hash for a user transaction, also known as the transaction id. This hash identifies the core \&quot;intent\&quot; of the transaction. Each transaction intent can only be committed once. This hash gets signed by any signatories on the transaction, to create the signed intent. 
        /// </summary>
        /// <value>The hex-encoded transaction intent hash for a user transaction, also known as the transaction id. This hash identifies the core \&quot;intent\&quot; of the transaction. Each transaction intent can only be committed once. This hash gets signed by any signatories on the transaction, to create the signed intent. </value>
        [DataMember(Name = "intent_hash", IsRequired = true, EmitDefaultValue = true)]
        public string IntentHash { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable &#x60;TransactionIntentHash&#x60;.
        /// </summary>
        /// <value>The Bech32m-encoded human readable &#x60;TransactionIntentHash&#x60;.</value>
        [DataMember(Name = "intent_hash_bech32m", IsRequired = true, EmitDefaultValue = true)]
        public string IntentHashBech32m { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ExecutedScenarioTransaction {\n");
            sb.Append("  LogicalName: ").Append(LogicalName).Append("\n");
            sb.Append("  StateVersion: ").Append(StateVersion).Append("\n");
            sb.Append("  IntentHash: ").Append(IntentHash).Append("\n");
            sb.Append("  IntentHashBech32m: ").Append(IntentHashBech32m).Append("\n");
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
            return this.Equals(input as ExecutedScenarioTransaction);
        }

        /// <summary>
        /// Returns true if ExecutedScenarioTransaction instances are equal
        /// </summary>
        /// <param name="input">Instance of ExecutedScenarioTransaction to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ExecutedScenarioTransaction input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.LogicalName == input.LogicalName ||
                    (this.LogicalName != null &&
                    this.LogicalName.Equals(input.LogicalName))
                ) && 
                (
                    this.StateVersion == input.StateVersion ||
                    this.StateVersion.Equals(input.StateVersion)
                ) && 
                (
                    this.IntentHash == input.IntentHash ||
                    (this.IntentHash != null &&
                    this.IntentHash.Equals(input.IntentHash))
                ) && 
                (
                    this.IntentHashBech32m == input.IntentHashBech32m ||
                    (this.IntentHashBech32m != null &&
                    this.IntentHashBech32m.Equals(input.IntentHashBech32m))
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
                if (this.LogicalName != null)
                {
                    hashCode = (hashCode * 59) + this.LogicalName.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.StateVersion.GetHashCode();
                if (this.IntentHash != null)
                {
                    hashCode = (hashCode * 59) + this.IntentHash.GetHashCode();
                }
                if (this.IntentHashBech32m != null)
                {
                    hashCode = (hashCode * 59) + this.IntentHashBech32m.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
