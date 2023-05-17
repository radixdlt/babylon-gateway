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
    /// NotarizedTransaction
    /// </summary>
    [DataContract(Name = "NotarizedTransaction")]
    public partial class NotarizedTransaction : IEquatable<NotarizedTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotarizedTransaction" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected NotarizedTransaction() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="NotarizedTransaction" /> class.
        /// </summary>
        /// <param name="hash">The hex-encoded notarized transaction hash. This is known as the Notarized Transaction Hash, Payload Hash or User Payload Hash. This hash is &#x60;Blake2b-256(compiled_notarized_transaction)&#x60; (required).</param>
        /// <param name="payloadHex">The hex-encoded full notarized transaction payload. Returning this can be disabled in TransactionFormatOptions on your request (default true)..</param>
        /// <param name="signedIntent">signedIntent (required).</param>
        /// <param name="notarySignature">notarySignature (required).</param>
        public NotarizedTransaction(string hash = default(string), string payloadHex = default(string), SignedTransactionIntent signedIntent = default(SignedTransactionIntent), Signature notarySignature = default(Signature))
        {
            // to ensure "hash" is required (not null)
            if (hash == null)
            {
                throw new ArgumentNullException("hash is a required property for NotarizedTransaction and cannot be null");
            }
            this.Hash = hash;
            // to ensure "signedIntent" is required (not null)
            if (signedIntent == null)
            {
                throw new ArgumentNullException("signedIntent is a required property for NotarizedTransaction and cannot be null");
            }
            this.SignedIntent = signedIntent;
            // to ensure "notarySignature" is required (not null)
            if (notarySignature == null)
            {
                throw new ArgumentNullException("notarySignature is a required property for NotarizedTransaction and cannot be null");
            }
            this.NotarySignature = notarySignature;
            this.PayloadHex = payloadHex;
        }

        /// <summary>
        /// The hex-encoded notarized transaction hash. This is known as the Notarized Transaction Hash, Payload Hash or User Payload Hash. This hash is &#x60;Blake2b-256(compiled_notarized_transaction)&#x60;
        /// </summary>
        /// <value>The hex-encoded notarized transaction hash. This is known as the Notarized Transaction Hash, Payload Hash or User Payload Hash. This hash is &#x60;Blake2b-256(compiled_notarized_transaction)&#x60;</value>
        [DataMember(Name = "hash", IsRequired = true, EmitDefaultValue = true)]
        public string Hash { get; set; }

        /// <summary>
        /// The hex-encoded full notarized transaction payload. Returning this can be disabled in TransactionFormatOptions on your request (default true).
        /// </summary>
        /// <value>The hex-encoded full notarized transaction payload. Returning this can be disabled in TransactionFormatOptions on your request (default true).</value>
        [DataMember(Name = "payload_hex", EmitDefaultValue = true)]
        public string PayloadHex { get; set; }

        /// <summary>
        /// Gets or Sets SignedIntent
        /// </summary>
        [DataMember(Name = "signed_intent", IsRequired = true, EmitDefaultValue = true)]
        public SignedTransactionIntent SignedIntent { get; set; }

        /// <summary>
        /// Gets or Sets NotarySignature
        /// </summary>
        [DataMember(Name = "notary_signature", IsRequired = true, EmitDefaultValue = true)]
        public Signature NotarySignature { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class NotarizedTransaction {\n");
            sb.Append("  Hash: ").Append(Hash).Append("\n");
            sb.Append("  PayloadHex: ").Append(PayloadHex).Append("\n");
            sb.Append("  SignedIntent: ").Append(SignedIntent).Append("\n");
            sb.Append("  NotarySignature: ").Append(NotarySignature).Append("\n");
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
            return this.Equals(input as NotarizedTransaction);
        }

        /// <summary>
        /// Returns true if NotarizedTransaction instances are equal
        /// </summary>
        /// <param name="input">Instance of NotarizedTransaction to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(NotarizedTransaction input)
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
                    this.PayloadHex == input.PayloadHex ||
                    (this.PayloadHex != null &&
                    this.PayloadHex.Equals(input.PayloadHex))
                ) && 
                (
                    this.SignedIntent == input.SignedIntent ||
                    (this.SignedIntent != null &&
                    this.SignedIntent.Equals(input.SignedIntent))
                ) && 
                (
                    this.NotarySignature == input.NotarySignature ||
                    (this.NotarySignature != null &&
                    this.NotarySignature.Equals(input.NotarySignature))
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
                if (this.PayloadHex != null)
                {
                    hashCode = (hashCode * 59) + this.PayloadHex.GetHashCode();
                }
                if (this.SignedIntent != null)
                {
                    hashCode = (hashCode * 59) + this.SignedIntent.GetHashCode();
                }
                if (this.NotarySignature != null)
                {
                    hashCode = (hashCode * 59) + this.NotarySignature.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
