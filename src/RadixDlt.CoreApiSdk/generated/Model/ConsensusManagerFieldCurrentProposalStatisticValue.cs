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
    /// ConsensusManagerFieldCurrentProposalStatisticValue
    /// </summary>
    [DataContract(Name = "ConsensusManagerFieldCurrentProposalStatisticValue")]
    public partial class ConsensusManagerFieldCurrentProposalStatisticValue : IEquatable<ConsensusManagerFieldCurrentProposalStatisticValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsensusManagerFieldCurrentProposalStatisticValue" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ConsensusManagerFieldCurrentProposalStatisticValue() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsensusManagerFieldCurrentProposalStatisticValue" /> class.
        /// </summary>
        /// <param name="completed">The number of successfully completed proposals this epoch for each validator, indexed by the validator order in the active set. (required).</param>
        /// <param name="missed">The number of missed proposals this epoch for each validator, indexed by the validator order in the active set. (required).</param>
        public ConsensusManagerFieldCurrentProposalStatisticValue(List<long> completed = default(List<long>), List<long> missed = default(List<long>))
        {
            // to ensure "completed" is required (not null)
            if (completed == null)
            {
                throw new ArgumentNullException("completed is a required property for ConsensusManagerFieldCurrentProposalStatisticValue and cannot be null");
            }
            this.Completed = completed;
            // to ensure "missed" is required (not null)
            if (missed == null)
            {
                throw new ArgumentNullException("missed is a required property for ConsensusManagerFieldCurrentProposalStatisticValue and cannot be null");
            }
            this.Missed = missed;
        }

        /// <summary>
        /// The number of successfully completed proposals this epoch for each validator, indexed by the validator order in the active set.
        /// </summary>
        /// <value>The number of successfully completed proposals this epoch for each validator, indexed by the validator order in the active set.</value>
        [DataMember(Name = "completed", IsRequired = true, EmitDefaultValue = true)]
        public List<long> Completed { get; set; }

        /// <summary>
        /// The number of missed proposals this epoch for each validator, indexed by the validator order in the active set.
        /// </summary>
        /// <value>The number of missed proposals this epoch for each validator, indexed by the validator order in the active set.</value>
        [DataMember(Name = "missed", IsRequired = true, EmitDefaultValue = true)]
        public List<long> Missed { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ConsensusManagerFieldCurrentProposalStatisticValue {\n");
            sb.Append("  Completed: ").Append(Completed).Append("\n");
            sb.Append("  Missed: ").Append(Missed).Append("\n");
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
            return this.Equals(input as ConsensusManagerFieldCurrentProposalStatisticValue);
        }

        /// <summary>
        /// Returns true if ConsensusManagerFieldCurrentProposalStatisticValue instances are equal
        /// </summary>
        /// <param name="input">Instance of ConsensusManagerFieldCurrentProposalStatisticValue to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ConsensusManagerFieldCurrentProposalStatisticValue input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Completed == input.Completed ||
                    this.Completed != null &&
                    input.Completed != null &&
                    this.Completed.SequenceEqual(input.Completed)
                ) && 
                (
                    this.Missed == input.Missed ||
                    this.Missed != null &&
                    input.Missed != null &&
                    this.Missed.SequenceEqual(input.Missed)
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
                if (this.Completed != null)
                {
                    hashCode = (hashCode * 59) + this.Completed.GetHashCode();
                }
                if (this.Missed != null)
                {
                    hashCode = (hashCode * 59) + this.Missed.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
