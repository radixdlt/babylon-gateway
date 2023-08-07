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
 * Babylon Core API - RCnet v3
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  It is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  This version of the Core API belongs to the second release candidate of the Radix Babylon network (\"RCnet v3\").  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` are guaranteed to be forward compatible to Babylon mainnet launch (and beyond). We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code. 
 *
 * The version of the OpenAPI document: 0.5.0
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
    /// TransactionHeader
    /// </summary>
    [DataContract(Name = "TransactionHeader")]
    public partial class TransactionHeader : IEquatable<TransactionHeader>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHeader" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TransactionHeader() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHeader" /> class.
        /// </summary>
        /// <param name="networkId">The logical id of the network (required).</param>
        /// <param name="startEpochInclusive">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction can be submitted. In the case of uncommitted transactions, a value of &#x60;10^10&#x60; indicates that the epoch was &gt;&#x3D; &#x60;10^10&#x60;.  (required).</param>
        /// <param name="endEpochExclusive">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction will no longer be valid, and be rejected. In the case of uncommitted transactions, a value of &#x60;10^10&#x60; indicates that the epoch was &gt;&#x3D; &#x60;10^10&#x60;.  (required).</param>
        /// <param name="nonce">An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, chosen to allow a unique intent to be created (to enable submitting an otherwise identical/duplicate intent).  (required).</param>
        /// <param name="notaryPublicKey">notaryPublicKey (required).</param>
        /// <param name="notaryIsSignatory">Specifies whether the notary public key should be included in the transaction signers list (required).</param>
        /// <param name="tipPercentage">An integer between &#x60;0&#x60; and &#x60;255&#x60;, giving the validator tip as a percentage amount. A value of &#x60;1&#x60; corresponds to 1% of the fee. (required).</param>
        public TransactionHeader(int networkId = default(int), long startEpochInclusive = default(long), long endEpochExclusive = default(long), long nonce = default(long), PublicKey notaryPublicKey = default(PublicKey), bool notaryIsSignatory = default(bool), int tipPercentage = default(int))
        {
            this.NetworkId = networkId;
            this.StartEpochInclusive = startEpochInclusive;
            this.EndEpochExclusive = endEpochExclusive;
            this.Nonce = nonce;
            // to ensure "notaryPublicKey" is required (not null)
            if (notaryPublicKey == null)
            {
                throw new ArgumentNullException("notaryPublicKey is a required property for TransactionHeader and cannot be null");
            }
            this.NotaryPublicKey = notaryPublicKey;
            this.NotaryIsSignatory = notaryIsSignatory;
            this.TipPercentage = tipPercentage;
        }

        /// <summary>
        /// The logical id of the network
        /// </summary>
        /// <value>The logical id of the network</value>
        [DataMember(Name = "network_id", IsRequired = true, EmitDefaultValue = true)]
        public int NetworkId { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction can be submitted. In the case of uncommitted transactions, a value of &#x60;10^10&#x60; indicates that the epoch was &gt;&#x3D; &#x60;10^10&#x60;. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction can be submitted. In the case of uncommitted transactions, a value of &#x60;10^10&#x60; indicates that the epoch was &gt;&#x3D; &#x60;10^10&#x60;. </value>
        [DataMember(Name = "start_epoch_inclusive", IsRequired = true, EmitDefaultValue = true)]
        public long StartEpochInclusive { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction will no longer be valid, and be rejected. In the case of uncommitted transactions, a value of &#x60;10^10&#x60; indicates that the epoch was &gt;&#x3D; &#x60;10^10&#x60;. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch from which the transaction will no longer be valid, and be rejected. In the case of uncommitted transactions, a value of &#x60;10^10&#x60; indicates that the epoch was &gt;&#x3D; &#x60;10^10&#x60;. </value>
        [DataMember(Name = "end_epoch_exclusive", IsRequired = true, EmitDefaultValue = true)]
        public long EndEpochExclusive { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, chosen to allow a unique intent to be created (to enable submitting an otherwise identical/duplicate intent). 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, chosen to allow a unique intent to be created (to enable submitting an otherwise identical/duplicate intent). </value>
        [DataMember(Name = "nonce", IsRequired = true, EmitDefaultValue = true)]
        public long Nonce { get; set; }

        /// <summary>
        /// Gets or Sets NotaryPublicKey
        /// </summary>
        [DataMember(Name = "notary_public_key", IsRequired = true, EmitDefaultValue = true)]
        public PublicKey NotaryPublicKey { get; set; }

        /// <summary>
        /// Specifies whether the notary public key should be included in the transaction signers list
        /// </summary>
        /// <value>Specifies whether the notary public key should be included in the transaction signers list</value>
        [DataMember(Name = "notary_is_signatory", IsRequired = true, EmitDefaultValue = true)]
        public bool NotaryIsSignatory { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;255&#x60;, giving the validator tip as a percentage amount. A value of &#x60;1&#x60; corresponds to 1% of the fee.
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;255&#x60;, giving the validator tip as a percentage amount. A value of &#x60;1&#x60; corresponds to 1% of the fee.</value>
        [DataMember(Name = "tip_percentage", IsRequired = true, EmitDefaultValue = true)]
        public int TipPercentage { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionHeader {\n");
            sb.Append("  NetworkId: ").Append(NetworkId).Append("\n");
            sb.Append("  StartEpochInclusive: ").Append(StartEpochInclusive).Append("\n");
            sb.Append("  EndEpochExclusive: ").Append(EndEpochExclusive).Append("\n");
            sb.Append("  Nonce: ").Append(Nonce).Append("\n");
            sb.Append("  NotaryPublicKey: ").Append(NotaryPublicKey).Append("\n");
            sb.Append("  NotaryIsSignatory: ").Append(NotaryIsSignatory).Append("\n");
            sb.Append("  TipPercentage: ").Append(TipPercentage).Append("\n");
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
            return this.Equals(input as TransactionHeader);
        }

        /// <summary>
        /// Returns true if TransactionHeader instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionHeader to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionHeader input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.NetworkId == input.NetworkId ||
                    this.NetworkId.Equals(input.NetworkId)
                ) && 
                (
                    this.StartEpochInclusive == input.StartEpochInclusive ||
                    this.StartEpochInclusive.Equals(input.StartEpochInclusive)
                ) && 
                (
                    this.EndEpochExclusive == input.EndEpochExclusive ||
                    this.EndEpochExclusive.Equals(input.EndEpochExclusive)
                ) && 
                (
                    this.Nonce == input.Nonce ||
                    this.Nonce.Equals(input.Nonce)
                ) && 
                (
                    this.NotaryPublicKey == input.NotaryPublicKey ||
                    (this.NotaryPublicKey != null &&
                    this.NotaryPublicKey.Equals(input.NotaryPublicKey))
                ) && 
                (
                    this.NotaryIsSignatory == input.NotaryIsSignatory ||
                    this.NotaryIsSignatory.Equals(input.NotaryIsSignatory)
                ) && 
                (
                    this.TipPercentage == input.TipPercentage ||
                    this.TipPercentage.Equals(input.TipPercentage)
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
                hashCode = (hashCode * 59) + this.NetworkId.GetHashCode();
                hashCode = (hashCode * 59) + this.StartEpochInclusive.GetHashCode();
                hashCode = (hashCode * 59) + this.EndEpochExclusive.GetHashCode();
                hashCode = (hashCode * 59) + this.Nonce.GetHashCode();
                if (this.NotaryPublicKey != null)
                {
                    hashCode = (hashCode * 59) + this.NotaryPublicKey.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.NotaryIsSignatory.GetHashCode();
                hashCode = (hashCode * 59) + this.TipPercentage.GetHashCode();
                return hashCode;
            }
        }

    }

}
