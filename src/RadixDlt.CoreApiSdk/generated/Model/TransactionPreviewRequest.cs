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
    /// TransactionPreviewRequest
    /// </summary>
    [DataContract(Name = "TransactionPreviewRequest")]
    public partial class TransactionPreviewRequest : IEquatable<TransactionPreviewRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionPreviewRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TransactionPreviewRequest() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionPreviewRequest" /> class.
        /// </summary>
        /// <param name="network">The logical name of the network (required).</param>
        /// <param name="atLedgerState">atLedgerState.</param>
        /// <param name="manifest">A text representation of a transaction manifest. (required).</param>
        /// <param name="blobsHex">An array of hex-encoded blob data, if referenced by the manifest..</param>
        /// <param name="startEpochInclusive">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch at which the transaction starts being valid. If not provided, the current epoch will be used (taking into account the &#x60;at_ledger_state&#x60;, if specified). .</param>
        /// <param name="endEpochExclusive">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch at which the transaction is no longer valid. If not provided, a maximum epoch (relative to the &#x60;start_epoch_inclusive&#x60;) will be used. .</param>
        /// <param name="notaryPublicKey">notaryPublicKey.</param>
        /// <param name="notaryIsSignatory">Whether the notary should be used as a signer (optional). If not provided, this defaults to false. .</param>
        /// <param name="tipPercentage">An integer between &#x60;0&#x60; and &#x60;65535&#x60;, giving the validator tip as a percentage amount. A value of &#x60;1&#x60; corresponds to a 1% fee. If not provided, this defaults to 0. .</param>
        /// <param name="nonce">An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, chosen to allow a unique intent to be created (to enable submitting an otherwise identical/duplicate intent). If not provided, this defaults to 0. .</param>
        /// <param name="signerPublicKeys">A list of public keys to be used as transaction signers. If not provided, this defaults to an empty array. .</param>
        /// <param name="message">message.</param>
        /// <param name="options">options.</param>
        /// <param name="flags">flags.</param>
        public TransactionPreviewRequest(string network = default(string), LedgerStateSelector atLedgerState = default(LedgerStateSelector), string manifest = default(string), List<string> blobsHex = default(List<string>), long? startEpochInclusive = default(long?), long? endEpochExclusive = default(long?), PublicKey notaryPublicKey = default(PublicKey), bool? notaryIsSignatory = default(bool?), int? tipPercentage = default(int?), long? nonce = default(long?), List<PublicKey> signerPublicKeys = default(List<PublicKey>), TransactionMessage message = default(TransactionMessage), TransactionPreviewResponseOptions options = default(TransactionPreviewResponseOptions), PreviewFlags flags = default(PreviewFlags))
        {
            // to ensure "network" is required (not null)
            if (network == null)
            {
                throw new ArgumentNullException("network is a required property for TransactionPreviewRequest and cannot be null");
            }
            this.Network = network;
            // to ensure "manifest" is required (not null)
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest is a required property for TransactionPreviewRequest and cannot be null");
            }
            this.Manifest = manifest;
            this.AtLedgerState = atLedgerState;
            this.BlobsHex = blobsHex;
            this.StartEpochInclusive = startEpochInclusive;
            this.EndEpochExclusive = endEpochExclusive;
            this.NotaryPublicKey = notaryPublicKey;
            this.NotaryIsSignatory = notaryIsSignatory;
            this.TipPercentage = tipPercentage;
            this.Nonce = nonce;
            this.SignerPublicKeys = signerPublicKeys;
            this.Message = message;
            this.Options = options;
            this.Flags = flags;
        }

        /// <summary>
        /// The logical name of the network
        /// </summary>
        /// <value>The logical name of the network</value>
        [DataMember(Name = "network", IsRequired = true, EmitDefaultValue = true)]
        public string Network { get; set; }

        /// <summary>
        /// Gets or Sets AtLedgerState
        /// </summary>
        [DataMember(Name = "at_ledger_state", EmitDefaultValue = true)]
        public LedgerStateSelector AtLedgerState { get; set; }

        /// <summary>
        /// A text representation of a transaction manifest.
        /// </summary>
        /// <value>A text representation of a transaction manifest.</value>
        [DataMember(Name = "manifest", IsRequired = true, EmitDefaultValue = true)]
        public string Manifest { get; set; }

        /// <summary>
        /// An array of hex-encoded blob data, if referenced by the manifest.
        /// </summary>
        /// <value>An array of hex-encoded blob data, if referenced by the manifest.</value>
        [DataMember(Name = "blobs_hex", EmitDefaultValue = true)]
        public List<string> BlobsHex { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch at which the transaction starts being valid. If not provided, the current epoch will be used (taking into account the &#x60;at_ledger_state&#x60;, if specified). 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch at which the transaction starts being valid. If not provided, the current epoch will be used (taking into account the &#x60;at_ledger_state&#x60;, if specified). </value>
        [DataMember(Name = "start_epoch_inclusive", EmitDefaultValue = false)]
        public long? StartEpochInclusive { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch at which the transaction is no longer valid. If not provided, a maximum epoch (relative to the &#x60;start_epoch_inclusive&#x60;) will be used. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the epoch at which the transaction is no longer valid. If not provided, a maximum epoch (relative to the &#x60;start_epoch_inclusive&#x60;) will be used. </value>
        [DataMember(Name = "end_epoch_exclusive", EmitDefaultValue = false)]
        public long? EndEpochExclusive { get; set; }

        /// <summary>
        /// Gets or Sets NotaryPublicKey
        /// </summary>
        [DataMember(Name = "notary_public_key", EmitDefaultValue = true)]
        public PublicKey NotaryPublicKey { get; set; }

        /// <summary>
        /// Whether the notary should be used as a signer (optional). If not provided, this defaults to false. 
        /// </summary>
        /// <value>Whether the notary should be used as a signer (optional). If not provided, this defaults to false. </value>
        [DataMember(Name = "notary_is_signatory", EmitDefaultValue = false)]
        public bool? NotaryIsSignatory { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;65535&#x60;, giving the validator tip as a percentage amount. A value of &#x60;1&#x60; corresponds to a 1% fee. If not provided, this defaults to 0. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;65535&#x60;, giving the validator tip as a percentage amount. A value of &#x60;1&#x60; corresponds to a 1% fee. If not provided, this defaults to 0. </value>
        [DataMember(Name = "tip_percentage", EmitDefaultValue = false)]
        public int? TipPercentage { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, chosen to allow a unique intent to be created (to enable submitting an otherwise identical/duplicate intent). If not provided, this defaults to 0. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;2^32 - 1&#x60;, chosen to allow a unique intent to be created (to enable submitting an otherwise identical/duplicate intent). If not provided, this defaults to 0. </value>
        [DataMember(Name = "nonce", EmitDefaultValue = false)]
        public long? Nonce { get; set; }

        /// <summary>
        /// A list of public keys to be used as transaction signers. If not provided, this defaults to an empty array. 
        /// </summary>
        /// <value>A list of public keys to be used as transaction signers. If not provided, this defaults to an empty array. </value>
        [DataMember(Name = "signer_public_keys", EmitDefaultValue = true)]
        public List<PublicKey> SignerPublicKeys { get; set; }

        /// <summary>
        /// Gets or Sets Message
        /// </summary>
        [DataMember(Name = "message", EmitDefaultValue = true)]
        public TransactionMessage Message { get; set; }

        /// <summary>
        /// Gets or Sets Options
        /// </summary>
        [DataMember(Name = "options", EmitDefaultValue = true)]
        public TransactionPreviewResponseOptions Options { get; set; }

        /// <summary>
        /// Gets or Sets Flags
        /// </summary>
        [DataMember(Name = "flags", EmitDefaultValue = true)]
        public PreviewFlags Flags { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionPreviewRequest {\n");
            sb.Append("  Network: ").Append(Network).Append("\n");
            sb.Append("  AtLedgerState: ").Append(AtLedgerState).Append("\n");
            sb.Append("  Manifest: ").Append(Manifest).Append("\n");
            sb.Append("  BlobsHex: ").Append(BlobsHex).Append("\n");
            sb.Append("  StartEpochInclusive: ").Append(StartEpochInclusive).Append("\n");
            sb.Append("  EndEpochExclusive: ").Append(EndEpochExclusive).Append("\n");
            sb.Append("  NotaryPublicKey: ").Append(NotaryPublicKey).Append("\n");
            sb.Append("  NotaryIsSignatory: ").Append(NotaryIsSignatory).Append("\n");
            sb.Append("  TipPercentage: ").Append(TipPercentage).Append("\n");
            sb.Append("  Nonce: ").Append(Nonce).Append("\n");
            sb.Append("  SignerPublicKeys: ").Append(SignerPublicKeys).Append("\n");
            sb.Append("  Message: ").Append(Message).Append("\n");
            sb.Append("  Options: ").Append(Options).Append("\n");
            sb.Append("  Flags: ").Append(Flags).Append("\n");
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
            return this.Equals(input as TransactionPreviewRequest);
        }

        /// <summary>
        /// Returns true if TransactionPreviewRequest instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionPreviewRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionPreviewRequest input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Network == input.Network ||
                    (this.Network != null &&
                    this.Network.Equals(input.Network))
                ) && 
                (
                    this.AtLedgerState == input.AtLedgerState ||
                    (this.AtLedgerState != null &&
                    this.AtLedgerState.Equals(input.AtLedgerState))
                ) && 
                (
                    this.Manifest == input.Manifest ||
                    (this.Manifest != null &&
                    this.Manifest.Equals(input.Manifest))
                ) && 
                (
                    this.BlobsHex == input.BlobsHex ||
                    this.BlobsHex != null &&
                    input.BlobsHex != null &&
                    this.BlobsHex.SequenceEqual(input.BlobsHex)
                ) && 
                (
                    this.StartEpochInclusive == input.StartEpochInclusive ||
                    (this.StartEpochInclusive != null &&
                    this.StartEpochInclusive.Equals(input.StartEpochInclusive))
                ) && 
                (
                    this.EndEpochExclusive == input.EndEpochExclusive ||
                    (this.EndEpochExclusive != null &&
                    this.EndEpochExclusive.Equals(input.EndEpochExclusive))
                ) && 
                (
                    this.NotaryPublicKey == input.NotaryPublicKey ||
                    (this.NotaryPublicKey != null &&
                    this.NotaryPublicKey.Equals(input.NotaryPublicKey))
                ) && 
                (
                    this.NotaryIsSignatory == input.NotaryIsSignatory ||
                    (this.NotaryIsSignatory != null &&
                    this.NotaryIsSignatory.Equals(input.NotaryIsSignatory))
                ) && 
                (
                    this.TipPercentage == input.TipPercentage ||
                    (this.TipPercentage != null &&
                    this.TipPercentage.Equals(input.TipPercentage))
                ) && 
                (
                    this.Nonce == input.Nonce ||
                    (this.Nonce != null &&
                    this.Nonce.Equals(input.Nonce))
                ) && 
                (
                    this.SignerPublicKeys == input.SignerPublicKeys ||
                    this.SignerPublicKeys != null &&
                    input.SignerPublicKeys != null &&
                    this.SignerPublicKeys.SequenceEqual(input.SignerPublicKeys)
                ) && 
                (
                    this.Message == input.Message ||
                    (this.Message != null &&
                    this.Message.Equals(input.Message))
                ) && 
                (
                    this.Options == input.Options ||
                    (this.Options != null &&
                    this.Options.Equals(input.Options))
                ) && 
                (
                    this.Flags == input.Flags ||
                    (this.Flags != null &&
                    this.Flags.Equals(input.Flags))
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
                if (this.Network != null)
                {
                    hashCode = (hashCode * 59) + this.Network.GetHashCode();
                }
                if (this.AtLedgerState != null)
                {
                    hashCode = (hashCode * 59) + this.AtLedgerState.GetHashCode();
                }
                if (this.Manifest != null)
                {
                    hashCode = (hashCode * 59) + this.Manifest.GetHashCode();
                }
                if (this.BlobsHex != null)
                {
                    hashCode = (hashCode * 59) + this.BlobsHex.GetHashCode();
                }
                if (this.StartEpochInclusive != null)
                {
                    hashCode = (hashCode * 59) + this.StartEpochInclusive.GetHashCode();
                }
                if (this.EndEpochExclusive != null)
                {
                    hashCode = (hashCode * 59) + this.EndEpochExclusive.GetHashCode();
                }
                if (this.NotaryPublicKey != null)
                {
                    hashCode = (hashCode * 59) + this.NotaryPublicKey.GetHashCode();
                }
                if (this.NotaryIsSignatory != null)
                {
                    hashCode = (hashCode * 59) + this.NotaryIsSignatory.GetHashCode();
                }
                if (this.TipPercentage != null)
                {
                    hashCode = (hashCode * 59) + this.TipPercentage.GetHashCode();
                }
                if (this.Nonce != null)
                {
                    hashCode = (hashCode * 59) + this.Nonce.GetHashCode();
                }
                if (this.SignerPublicKeys != null)
                {
                    hashCode = (hashCode * 59) + this.SignerPublicKeys.GetHashCode();
                }
                if (this.Message != null)
                {
                    hashCode = (hashCode * 59) + this.Message.GetHashCode();
                }
                if (this.Options != null)
                {
                    hashCode = (hashCode * 59) + this.Options.GetHashCode();
                }
                if (this.Flags != null)
                {
                    hashCode = (hashCode * 59) + this.Flags.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
