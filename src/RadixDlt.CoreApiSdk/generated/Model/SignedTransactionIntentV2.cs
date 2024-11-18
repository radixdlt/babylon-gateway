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
    /// SignedTransactionIntentV2
    /// </summary>
    [DataContract(Name = "SignedTransactionIntentV2")]
    public partial class SignedTransactionIntentV2 : IEquatable<SignedTransactionIntentV2>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignedTransactionIntentV2" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected SignedTransactionIntentV2() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SignedTransactionIntentV2" /> class.
        /// </summary>
        /// <param name="hash">The hex-encoded signed intent hash for a user transaction. This hash identifies the transaction intent, plus additional signatures. This hash is signed by the notary, to create the submittable &#x60;NotarizedTransaction&#x60;.  (required).</param>
        /// <param name="hashBech32m">The Bech32m-encoded human readable &#x60;SignedTransactionIntentHash&#x60;. (required).</param>
        /// <param name="transactionIntent">transactionIntent (required).</param>
        /// <param name="transactionIntentSignatures">transactionIntentSignatures (required).</param>
        /// <param name="nonRootSubintentSignatures">This gives the signatures for each subintent in &#x60;non_root_subintents&#x60; in &#x60;TransactionIntentV2&#x60;. For committed transactions, these arrays are of equal length and correspond one-to-one in order.  (required).</param>
        public SignedTransactionIntentV2(string hash = default(string), string hashBech32m = default(string), TransactionIntentV2 transactionIntent = default(TransactionIntentV2), IntentSignatures transactionIntentSignatures = default(IntentSignatures), List<IntentSignatures> nonRootSubintentSignatures = default(List<IntentSignatures>))
        {
            // to ensure "hash" is required (not null)
            if (hash == null)
            {
                throw new ArgumentNullException("hash is a required property for SignedTransactionIntentV2 and cannot be null");
            }
            this.Hash = hash;
            // to ensure "hashBech32m" is required (not null)
            if (hashBech32m == null)
            {
                throw new ArgumentNullException("hashBech32m is a required property for SignedTransactionIntentV2 and cannot be null");
            }
            this.HashBech32m = hashBech32m;
            // to ensure "transactionIntent" is required (not null)
            if (transactionIntent == null)
            {
                throw new ArgumentNullException("transactionIntent is a required property for SignedTransactionIntentV2 and cannot be null");
            }
            this.TransactionIntent = transactionIntent;
            // to ensure "transactionIntentSignatures" is required (not null)
            if (transactionIntentSignatures == null)
            {
                throw new ArgumentNullException("transactionIntentSignatures is a required property for SignedTransactionIntentV2 and cannot be null");
            }
            this.TransactionIntentSignatures = transactionIntentSignatures;
            // to ensure "nonRootSubintentSignatures" is required (not null)
            if (nonRootSubintentSignatures == null)
            {
                throw new ArgumentNullException("nonRootSubintentSignatures is a required property for SignedTransactionIntentV2 and cannot be null");
            }
            this.NonRootSubintentSignatures = nonRootSubintentSignatures;
        }

        /// <summary>
        /// The hex-encoded signed intent hash for a user transaction. This hash identifies the transaction intent, plus additional signatures. This hash is signed by the notary, to create the submittable &#x60;NotarizedTransaction&#x60;. 
        /// </summary>
        /// <value>The hex-encoded signed intent hash for a user transaction. This hash identifies the transaction intent, plus additional signatures. This hash is signed by the notary, to create the submittable &#x60;NotarizedTransaction&#x60;. </value>
        [DataMember(Name = "hash", IsRequired = true, EmitDefaultValue = true)]
        public string Hash { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable &#x60;SignedTransactionIntentHash&#x60;.
        /// </summary>
        /// <value>The Bech32m-encoded human readable &#x60;SignedTransactionIntentHash&#x60;.</value>
        [DataMember(Name = "hash_bech32m", IsRequired = true, EmitDefaultValue = true)]
        public string HashBech32m { get; set; }

        /// <summary>
        /// Gets or Sets TransactionIntent
        /// </summary>
        [DataMember(Name = "transaction_intent", IsRequired = true, EmitDefaultValue = true)]
        public TransactionIntentV2 TransactionIntent { get; set; }

        /// <summary>
        /// Gets or Sets TransactionIntentSignatures
        /// </summary>
        [DataMember(Name = "transaction_intent_signatures", IsRequired = true, EmitDefaultValue = true)]
        public IntentSignatures TransactionIntentSignatures { get; set; }

        /// <summary>
        /// This gives the signatures for each subintent in &#x60;non_root_subintents&#x60; in &#x60;TransactionIntentV2&#x60;. For committed transactions, these arrays are of equal length and correspond one-to-one in order. 
        /// </summary>
        /// <value>This gives the signatures for each subintent in &#x60;non_root_subintents&#x60; in &#x60;TransactionIntentV2&#x60;. For committed transactions, these arrays are of equal length and correspond one-to-one in order. </value>
        [DataMember(Name = "non_root_subintent_signatures", IsRequired = true, EmitDefaultValue = true)]
        public List<IntentSignatures> NonRootSubintentSignatures { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class SignedTransactionIntentV2 {\n");
            sb.Append("  Hash: ").Append(Hash).Append("\n");
            sb.Append("  HashBech32m: ").Append(HashBech32m).Append("\n");
            sb.Append("  TransactionIntent: ").Append(TransactionIntent).Append("\n");
            sb.Append("  TransactionIntentSignatures: ").Append(TransactionIntentSignatures).Append("\n");
            sb.Append("  NonRootSubintentSignatures: ").Append(NonRootSubintentSignatures).Append("\n");
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
            return this.Equals(input as SignedTransactionIntentV2);
        }

        /// <summary>
        /// Returns true if SignedTransactionIntentV2 instances are equal
        /// </summary>
        /// <param name="input">Instance of SignedTransactionIntentV2 to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(SignedTransactionIntentV2 input)
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
                    this.TransactionIntent == input.TransactionIntent ||
                    (this.TransactionIntent != null &&
                    this.TransactionIntent.Equals(input.TransactionIntent))
                ) && 
                (
                    this.TransactionIntentSignatures == input.TransactionIntentSignatures ||
                    (this.TransactionIntentSignatures != null &&
                    this.TransactionIntentSignatures.Equals(input.TransactionIntentSignatures))
                ) && 
                (
                    this.NonRootSubintentSignatures == input.NonRootSubintentSignatures ||
                    this.NonRootSubintentSignatures != null &&
                    input.NonRootSubintentSignatures != null &&
                    this.NonRootSubintentSignatures.SequenceEqual(input.NonRootSubintentSignatures)
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
                if (this.TransactionIntent != null)
                {
                    hashCode = (hashCode * 59) + this.TransactionIntent.GetHashCode();
                }
                if (this.TransactionIntentSignatures != null)
                {
                    hashCode = (hashCode * 59) + this.TransactionIntentSignatures.GetHashCode();
                }
                if (this.NonRootSubintentSignatures != null)
                {
                    hashCode = (hashCode * 59) + this.NonRootSubintentSignatures.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
