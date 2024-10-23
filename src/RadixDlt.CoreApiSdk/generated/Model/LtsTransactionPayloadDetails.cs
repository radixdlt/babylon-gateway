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
    /// LtsTransactionPayloadDetails
    /// </summary>
    [DataContract(Name = "LtsTransactionPayloadDetails")]
    public partial class LtsTransactionPayloadDetails : IEquatable<LtsTransactionPayloadDetails>
    {

        /// <summary>
        /// Gets or Sets Status
        /// </summary>
        [DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
        public LtsTransactionPayloadStatus Status { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="LtsTransactionPayloadDetails" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected LtsTransactionPayloadDetails() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LtsTransactionPayloadDetails" /> class.
        /// </summary>
        /// <param name="payloadHash">The hex-encoded notarized transaction hash for a user transaction. This hash identifies the full submittable notarized transaction - ie the signed intent, plus the notary signature.  (required).</param>
        /// <param name="payloadHashBech32m">The Bech32m-encoded human readable &#x60;NotarizedTransactionHash&#x60;. (required).</param>
        /// <param name="stateVersion">stateVersion.</param>
        /// <param name="status">status (required).</param>
        /// <param name="errorMessage">An explanation for the error, if failed or rejected.</param>
        public LtsTransactionPayloadDetails(string payloadHash = default(string), string payloadHashBech32m = default(string), long stateVersion = default(long), LtsTransactionPayloadStatus status = default(LtsTransactionPayloadStatus), string errorMessage = default(string))
        {
            // to ensure "payloadHash" is required (not null)
            if (payloadHash == null)
            {
                throw new ArgumentNullException("payloadHash is a required property for LtsTransactionPayloadDetails and cannot be null");
            }
            this.PayloadHash = payloadHash;
            // to ensure "payloadHashBech32m" is required (not null)
            if (payloadHashBech32m == null)
            {
                throw new ArgumentNullException("payloadHashBech32m is a required property for LtsTransactionPayloadDetails and cannot be null");
            }
            this.PayloadHashBech32m = payloadHashBech32m;
            this.Status = status;
            this.StateVersion = stateVersion;
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// The hex-encoded notarized transaction hash for a user transaction. This hash identifies the full submittable notarized transaction - ie the signed intent, plus the notary signature. 
        /// </summary>
        /// <value>The hex-encoded notarized transaction hash for a user transaction. This hash identifies the full submittable notarized transaction - ie the signed intent, plus the notary signature. </value>
        [DataMember(Name = "payload_hash", IsRequired = true, EmitDefaultValue = true)]
        public string PayloadHash { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable &#x60;NotarizedTransactionHash&#x60;.
        /// </summary>
        /// <value>The Bech32m-encoded human readable &#x60;NotarizedTransactionHash&#x60;.</value>
        [DataMember(Name = "payload_hash_bech32m", IsRequired = true, EmitDefaultValue = true)]
        public string PayloadHashBech32m { get; set; }

        /// <summary>
        /// Gets or Sets StateVersion
        /// </summary>
        [DataMember(Name = "state_version", EmitDefaultValue = true)]
        public long StateVersion { get; set; }

        /// <summary>
        /// An explanation for the error, if failed or rejected
        /// </summary>
        /// <value>An explanation for the error, if failed or rejected</value>
        [DataMember(Name = "error_message", EmitDefaultValue = true)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class LtsTransactionPayloadDetails {\n");
            sb.Append("  PayloadHash: ").Append(PayloadHash).Append("\n");
            sb.Append("  PayloadHashBech32m: ").Append(PayloadHashBech32m).Append("\n");
            sb.Append("  StateVersion: ").Append(StateVersion).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
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
            return this.Equals(input as LtsTransactionPayloadDetails);
        }

        /// <summary>
        /// Returns true if LtsTransactionPayloadDetails instances are equal
        /// </summary>
        /// <param name="input">Instance of LtsTransactionPayloadDetails to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(LtsTransactionPayloadDetails input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.PayloadHash == input.PayloadHash ||
                    (this.PayloadHash != null &&
                    this.PayloadHash.Equals(input.PayloadHash))
                ) && 
                (
                    this.PayloadHashBech32m == input.PayloadHashBech32m ||
                    (this.PayloadHashBech32m != null &&
                    this.PayloadHashBech32m.Equals(input.PayloadHashBech32m))
                ) && 
                (
                    this.StateVersion == input.StateVersion ||
                    this.StateVersion.Equals(input.StateVersion)
                ) && 
                (
                    this.Status == input.Status ||
                    this.Status.Equals(input.Status)
                ) && 
                (
                    this.ErrorMessage == input.ErrorMessage ||
                    (this.ErrorMessage != null &&
                    this.ErrorMessage.Equals(input.ErrorMessage))
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
                if (this.PayloadHash != null)
                {
                    hashCode = (hashCode * 59) + this.PayloadHash.GetHashCode();
                }
                if (this.PayloadHashBech32m != null)
                {
                    hashCode = (hashCode * 59) + this.PayloadHashBech32m.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.StateVersion.GetHashCode();
                hashCode = (hashCode * 59) + this.Status.GetHashCode();
                if (this.ErrorMessage != null)
                {
                    hashCode = (hashCode * 59) + this.ErrorMessage.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
