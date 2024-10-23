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
    /// NextEpoch
    /// </summary>
    [DataContract(Name = "NextEpoch")]
    public partial class NextEpoch : IEquatable<NextEpoch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NextEpoch" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected NextEpoch() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="NextEpoch" /> class.
        /// </summary>
        /// <param name="epoch">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the new epoch (required).</param>
        /// <param name="validators">Active validator set for the new epoch, ordered by stake descending. (required).</param>
        /// <param name="significantProtocolUpdateReadiness">significantProtocolUpdateReadiness.</param>
        public NextEpoch(long epoch = default(long), List<ActiveValidator> validators = default(List<ActiveValidator>), List<SignificantProtocolUpdateReadinessEntry> significantProtocolUpdateReadiness = default(List<SignificantProtocolUpdateReadinessEntry>))
        {
            this.Epoch = epoch;
            // to ensure "validators" is required (not null)
            if (validators == null)
            {
                throw new ArgumentNullException("validators is a required property for NextEpoch and cannot be null");
            }
            this.Validators = validators;
            this.SignificantProtocolUpdateReadiness = significantProtocolUpdateReadiness;
        }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the new epoch
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, marking the new epoch</value>
        [DataMember(Name = "epoch", IsRequired = true, EmitDefaultValue = true)]
        public long Epoch { get; set; }

        /// <summary>
        /// Active validator set for the new epoch, ordered by stake descending.
        /// </summary>
        /// <value>Active validator set for the new epoch, ordered by stake descending.</value>
        [DataMember(Name = "validators", IsRequired = true, EmitDefaultValue = true)]
        public List<ActiveValidator> Validators { get; set; }

        /// <summary>
        /// Gets or Sets SignificantProtocolUpdateReadiness
        /// </summary>
        [DataMember(Name = "significant_protocol_update_readiness", EmitDefaultValue = true)]
        public List<SignificantProtocolUpdateReadinessEntry> SignificantProtocolUpdateReadiness { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class NextEpoch {\n");
            sb.Append("  Epoch: ").Append(Epoch).Append("\n");
            sb.Append("  Validators: ").Append(Validators).Append("\n");
            sb.Append("  SignificantProtocolUpdateReadiness: ").Append(SignificantProtocolUpdateReadiness).Append("\n");
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
            return this.Equals(input as NextEpoch);
        }

        /// <summary>
        /// Returns true if NextEpoch instances are equal
        /// </summary>
        /// <param name="input">Instance of NextEpoch to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(NextEpoch input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Epoch == input.Epoch ||
                    this.Epoch.Equals(input.Epoch)
                ) && 
                (
                    this.Validators == input.Validators ||
                    this.Validators != null &&
                    input.Validators != null &&
                    this.Validators.SequenceEqual(input.Validators)
                ) && 
                (
                    this.SignificantProtocolUpdateReadiness == input.SignificantProtocolUpdateReadiness ||
                    this.SignificantProtocolUpdateReadiness != null &&
                    input.SignificantProtocolUpdateReadiness != null &&
                    this.SignificantProtocolUpdateReadiness.SequenceEqual(input.SignificantProtocolUpdateReadiness)
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
                hashCode = (hashCode * 59) + this.Epoch.GetHashCode();
                if (this.Validators != null)
                {
                    hashCode = (hashCode * 59) + this.Validators.GetHashCode();
                }
                if (this.SignificantProtocolUpdateReadiness != null)
                {
                    hashCode = (hashCode * 59) + this.SignificantProtocolUpdateReadiness.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
