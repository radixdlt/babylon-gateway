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
    /// EncryptedMessageDecryptor
    /// </summary>
    [DataContract(Name = "EncryptedMessageDecryptor")]
    public partial class EncryptedMessageDecryptor : IEquatable<EncryptedMessageDecryptor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedMessageDecryptor" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected EncryptedMessageDecryptor() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedMessageDecryptor" /> class.
        /// </summary>
        /// <param name="publicKeyFingerprintHex">The last 8 bytes of the Blake2b-256 hash of the public key bytes, in their standard Radix byte-serialization. (required).</param>
        /// <param name="aesWrappedKeyHex">The hex-encoded wrapped key bytes from applying RFC 3394 (256-bit) AES-KeyWrap to the message ephemeral public key, with the secret KEK provided by static Diffie-Helman between the decryptor public key, and the &#x60;dh_ephemeral_public_key&#x60; for that curve type. The bytes are serialized (according to RFC 3394) as the concatenation &#x60;IV (first 8 bytes) || Cipher&#x60;.  The message ephemeral public key is 128-bit in V1 transactions, but 256-bit in V2 transactions. The Cipher is the wrapped ephemeral public key, and is encoded as two 64-bit blocks in V1, and four 64-bit blocks in V2.  (required).</param>
        public EncryptedMessageDecryptor(string publicKeyFingerprintHex = default(string), string aesWrappedKeyHex = default(string))
        {
            // to ensure "publicKeyFingerprintHex" is required (not null)
            if (publicKeyFingerprintHex == null)
            {
                throw new ArgumentNullException("publicKeyFingerprintHex is a required property for EncryptedMessageDecryptor and cannot be null");
            }
            this.PublicKeyFingerprintHex = publicKeyFingerprintHex;
            // to ensure "aesWrappedKeyHex" is required (not null)
            if (aesWrappedKeyHex == null)
            {
                throw new ArgumentNullException("aesWrappedKeyHex is a required property for EncryptedMessageDecryptor and cannot be null");
            }
            this.AesWrappedKeyHex = aesWrappedKeyHex;
        }

        /// <summary>
        /// The last 8 bytes of the Blake2b-256 hash of the public key bytes, in their standard Radix byte-serialization.
        /// </summary>
        /// <value>The last 8 bytes of the Blake2b-256 hash of the public key bytes, in their standard Radix byte-serialization.</value>
        [DataMember(Name = "public_key_fingerprint_hex", IsRequired = true, EmitDefaultValue = true)]
        public string PublicKeyFingerprintHex { get; set; }

        /// <summary>
        /// The hex-encoded wrapped key bytes from applying RFC 3394 (256-bit) AES-KeyWrap to the message ephemeral public key, with the secret KEK provided by static Diffie-Helman between the decryptor public key, and the &#x60;dh_ephemeral_public_key&#x60; for that curve type. The bytes are serialized (according to RFC 3394) as the concatenation &#x60;IV (first 8 bytes) || Cipher&#x60;.  The message ephemeral public key is 128-bit in V1 transactions, but 256-bit in V2 transactions. The Cipher is the wrapped ephemeral public key, and is encoded as two 64-bit blocks in V1, and four 64-bit blocks in V2. 
        /// </summary>
        /// <value>The hex-encoded wrapped key bytes from applying RFC 3394 (256-bit) AES-KeyWrap to the message ephemeral public key, with the secret KEK provided by static Diffie-Helman between the decryptor public key, and the &#x60;dh_ephemeral_public_key&#x60; for that curve type. The bytes are serialized (according to RFC 3394) as the concatenation &#x60;IV (first 8 bytes) || Cipher&#x60;.  The message ephemeral public key is 128-bit in V1 transactions, but 256-bit in V2 transactions. The Cipher is the wrapped ephemeral public key, and is encoded as two 64-bit blocks in V1, and four 64-bit blocks in V2. </value>
        [DataMember(Name = "aes_wrapped_key_hex", IsRequired = true, EmitDefaultValue = true)]
        public string AesWrappedKeyHex { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class EncryptedMessageDecryptor {\n");
            sb.Append("  PublicKeyFingerprintHex: ").Append(PublicKeyFingerprintHex).Append("\n");
            sb.Append("  AesWrappedKeyHex: ").Append(AesWrappedKeyHex).Append("\n");
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
            return this.Equals(input as EncryptedMessageDecryptor);
        }

        /// <summary>
        /// Returns true if EncryptedMessageDecryptor instances are equal
        /// </summary>
        /// <param name="input">Instance of EncryptedMessageDecryptor to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(EncryptedMessageDecryptor input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.PublicKeyFingerprintHex == input.PublicKeyFingerprintHex ||
                    (this.PublicKeyFingerprintHex != null &&
                    this.PublicKeyFingerprintHex.Equals(input.PublicKeyFingerprintHex))
                ) && 
                (
                    this.AesWrappedKeyHex == input.AesWrappedKeyHex ||
                    (this.AesWrappedKeyHex != null &&
                    this.AesWrappedKeyHex.Equals(input.AesWrappedKeyHex))
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
                if (this.PublicKeyFingerprintHex != null)
                {
                    hashCode = (hashCode * 59) + this.PublicKeyFingerprintHex.GetHashCode();
                }
                if (this.AesWrappedKeyHex != null)
                {
                    hashCode = (hashCode * 59) + this.AesWrappedKeyHex.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
