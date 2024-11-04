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
 * The version of the OpenAPI document: v1.2.3
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
    /// TransactionIntentV2
    /// </summary>
    [DataContract(Name = "TransactionIntentV2")]
    public partial class TransactionIntentV2 : IEquatable<TransactionIntentV2>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionIntentV2" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TransactionIntentV2() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionIntentV2" /> class.
        /// </summary>
        /// <param name="hash">The hex-encoded transaction intent hash for a user transaction, also known as the transaction id. This hash identifies the core \&quot;intent\&quot; of the transaction. Each transaction intent can only be committed once. This hash gets signed by any signatories on the transaction, to create the signed intent.  (required).</param>
        /// <param name="hashBech32m">The Bech32m-encoded human readable &#x60;TransactionIntentHash&#x60;. (required).</param>
        /// <param name="transactionHeader">transactionHeader (required).</param>
        /// <param name="rootIntentCore">rootIntentCore (required).</param>
        /// <param name="nonRootSubintents">nonRootSubintents (required).</param>
        public TransactionIntentV2(string hash = default(string), string hashBech32m = default(string), TransactionHeaderV2 transactionHeader = default(TransactionHeaderV2), IntentCoreV2 rootIntentCore = default(IntentCoreV2), List<SubintentV2> nonRootSubintents = default(List<SubintentV2>))
        {
            // to ensure "hash" is required (not null)
            if (hash == null)
            {
                throw new ArgumentNullException("hash is a required property for TransactionIntentV2 and cannot be null");
            }
            this.Hash = hash;
            // to ensure "hashBech32m" is required (not null)
            if (hashBech32m == null)
            {
                throw new ArgumentNullException("hashBech32m is a required property for TransactionIntentV2 and cannot be null");
            }
            this.HashBech32m = hashBech32m;
            // to ensure "transactionHeader" is required (not null)
            if (transactionHeader == null)
            {
                throw new ArgumentNullException("transactionHeader is a required property for TransactionIntentV2 and cannot be null");
            }
            this.TransactionHeader = transactionHeader;
            // to ensure "rootIntentCore" is required (not null)
            if (rootIntentCore == null)
            {
                throw new ArgumentNullException("rootIntentCore is a required property for TransactionIntentV2 and cannot be null");
            }
            this.RootIntentCore = rootIntentCore;
            // to ensure "nonRootSubintents" is required (not null)
            if (nonRootSubintents == null)
            {
                throw new ArgumentNullException("nonRootSubintents is a required property for TransactionIntentV2 and cannot be null");
            }
            this.NonRootSubintents = nonRootSubintents;
        }

        /// <summary>
        /// The hex-encoded transaction intent hash for a user transaction, also known as the transaction id. This hash identifies the core \&quot;intent\&quot; of the transaction. Each transaction intent can only be committed once. This hash gets signed by any signatories on the transaction, to create the signed intent. 
        /// </summary>
        /// <value>The hex-encoded transaction intent hash for a user transaction, also known as the transaction id. This hash identifies the core \&quot;intent\&quot; of the transaction. Each transaction intent can only be committed once. This hash gets signed by any signatories on the transaction, to create the signed intent. </value>
        [DataMember(Name = "hash", IsRequired = true, EmitDefaultValue = true)]
        public string Hash { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable &#x60;TransactionIntentHash&#x60;.
        /// </summary>
        /// <value>The Bech32m-encoded human readable &#x60;TransactionIntentHash&#x60;.</value>
        [DataMember(Name = "hash_bech32m", IsRequired = true, EmitDefaultValue = true)]
        public string HashBech32m { get; set; }

        /// <summary>
        /// Gets or Sets TransactionHeader
        /// </summary>
        [DataMember(Name = "transaction_header", IsRequired = true, EmitDefaultValue = true)]
        public TransactionHeaderV2 TransactionHeader { get; set; }

        /// <summary>
        /// Gets or Sets RootIntentCore
        /// </summary>
        [DataMember(Name = "root_intent_core", IsRequired = true, EmitDefaultValue = true)]
        public IntentCoreV2 RootIntentCore { get; set; }

        /// <summary>
        /// Gets or Sets NonRootSubintents
        /// </summary>
        [DataMember(Name = "non_root_subintents", IsRequired = true, EmitDefaultValue = true)]
        public List<SubintentV2> NonRootSubintents { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionIntentV2 {\n");
            sb.Append("  Hash: ").Append(Hash).Append("\n");
            sb.Append("  HashBech32m: ").Append(HashBech32m).Append("\n");
            sb.Append("  TransactionHeader: ").Append(TransactionHeader).Append("\n");
            sb.Append("  RootIntentCore: ").Append(RootIntentCore).Append("\n");
            sb.Append("  NonRootSubintents: ").Append(NonRootSubintents).Append("\n");
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
            return this.Equals(input as TransactionIntentV2);
        }

        /// <summary>
        /// Returns true if TransactionIntentV2 instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionIntentV2 to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionIntentV2 input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Hash == input.Hash ||
                    (this.Hash != null &&
                    this.Hash.Equals(input.Hash))
                ) && 
                (
                    this.HashBech32m == input.HashBech32m ||
                    (this.HashBech32m != null &&
                    this.HashBech32m.Equals(input.HashBech32m))
                ) && 
                (
                    this.TransactionHeader == input.TransactionHeader ||
                    (this.TransactionHeader != null &&
                    this.TransactionHeader.Equals(input.TransactionHeader))
                ) && 
                (
                    this.RootIntentCore == input.RootIntentCore ||
                    (this.RootIntentCore != null &&
                    this.RootIntentCore.Equals(input.RootIntentCore))
                ) && 
                (
                    this.NonRootSubintents == input.NonRootSubintents ||
                    this.NonRootSubintents != null &&
                    input.NonRootSubintents != null &&
                    this.NonRootSubintents.SequenceEqual(input.NonRootSubintents)
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
                if (this.Hash != null)
                {
                    hashCode = (hashCode * 59) + this.Hash.GetHashCode();
                }
                if (this.HashBech32m != null)
                {
                    hashCode = (hashCode * 59) + this.HashBech32m.GetHashCode();
                }
                if (this.TransactionHeader != null)
                {
                    hashCode = (hashCode * 59) + this.TransactionHeader.GetHashCode();
                }
                if (this.RootIntentCore != null)
                {
                    hashCode = (hashCode * 59) + this.RootIntentCore.GetHashCode();
                }
                if (this.NonRootSubintents != null)
                {
                    hashCode = (hashCode * 59) + this.NonRootSubintents.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}