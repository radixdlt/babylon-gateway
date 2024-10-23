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
    /// LtsStreamAccountTransactionOutcomesResponse
    /// </summary>
    [DataContract(Name = "LtsStreamAccountTransactionOutcomesResponse")]
    public partial class LtsStreamAccountTransactionOutcomesResponse : IEquatable<LtsStreamAccountTransactionOutcomesResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LtsStreamAccountTransactionOutcomesResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected LtsStreamAccountTransactionOutcomesResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LtsStreamAccountTransactionOutcomesResponse" /> class.
        /// </summary>
        /// <param name="fromStateVersion">fromStateVersion (required).</param>
        /// <param name="count">An integer between &#x60;0&#x60; and &#x60;10000&#x60;, giving the total count of transactions in the returned response (required).</param>
        /// <param name="maxLedgerStateVersion">maxLedgerStateVersion (required).</param>
        /// <param name="committedTransactionOutcomes">A committed transaction outcomes list starting from the &#x60;from_state_version&#x60; (inclusive). (required).</param>
        public LtsStreamAccountTransactionOutcomesResponse(long fromStateVersion = default(long), int count = default(int), long maxLedgerStateVersion = default(long), List<LtsCommittedTransactionOutcome> committedTransactionOutcomes = default(List<LtsCommittedTransactionOutcome>))
        {
            this.FromStateVersion = fromStateVersion;
            this.Count = count;
            this.MaxLedgerStateVersion = maxLedgerStateVersion;
            // to ensure "committedTransactionOutcomes" is required (not null)
            if (committedTransactionOutcomes == null)
            {
                throw new ArgumentNullException("committedTransactionOutcomes is a required property for LtsStreamAccountTransactionOutcomesResponse and cannot be null");
            }
            this.CommittedTransactionOutcomes = committedTransactionOutcomes;
        }

        /// <summary>
        /// Gets or Sets FromStateVersion
        /// </summary>
        [DataMember(Name = "from_state_version", IsRequired = true, EmitDefaultValue = true)]
        public long FromStateVersion { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10000&#x60;, giving the total count of transactions in the returned response
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10000&#x60;, giving the total count of transactions in the returned response</value>
        [DataMember(Name = "count", IsRequired = true, EmitDefaultValue = true)]
        public int Count { get; set; }

        /// <summary>
        /// Gets or Sets MaxLedgerStateVersion
        /// </summary>
        [DataMember(Name = "max_ledger_state_version", IsRequired = true, EmitDefaultValue = true)]
        public long MaxLedgerStateVersion { get; set; }

        /// <summary>
        /// A committed transaction outcomes list starting from the &#x60;from_state_version&#x60; (inclusive).
        /// </summary>
        /// <value>A committed transaction outcomes list starting from the &#x60;from_state_version&#x60; (inclusive).</value>
        [DataMember(Name = "committed_transaction_outcomes", IsRequired = true, EmitDefaultValue = true)]
        public List<LtsCommittedTransactionOutcome> CommittedTransactionOutcomes { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class LtsStreamAccountTransactionOutcomesResponse {\n");
            sb.Append("  FromStateVersion: ").Append(FromStateVersion).Append("\n");
            sb.Append("  Count: ").Append(Count).Append("\n");
            sb.Append("  MaxLedgerStateVersion: ").Append(MaxLedgerStateVersion).Append("\n");
            sb.Append("  CommittedTransactionOutcomes: ").Append(CommittedTransactionOutcomes).Append("\n");
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
            return this.Equals(input as LtsStreamAccountTransactionOutcomesResponse);
        }

        /// <summary>
        /// Returns true if LtsStreamAccountTransactionOutcomesResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of LtsStreamAccountTransactionOutcomesResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(LtsStreamAccountTransactionOutcomesResponse input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.FromStateVersion == input.FromStateVersion ||
                    this.FromStateVersion.Equals(input.FromStateVersion)
                ) && 
                (
                    this.Count == input.Count ||
                    this.Count.Equals(input.Count)
                ) && 
                (
                    this.MaxLedgerStateVersion == input.MaxLedgerStateVersion ||
                    this.MaxLedgerStateVersion.Equals(input.MaxLedgerStateVersion)
                ) && 
                (
                    this.CommittedTransactionOutcomes == input.CommittedTransactionOutcomes ||
                    this.CommittedTransactionOutcomes != null &&
                    input.CommittedTransactionOutcomes != null &&
                    this.CommittedTransactionOutcomes.SequenceEqual(input.CommittedTransactionOutcomes)
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
                hashCode = (hashCode * 59) + this.FromStateVersion.GetHashCode();
                hashCode = (hashCode * 59) + this.Count.GetHashCode();
                hashCode = (hashCode * 59) + this.MaxLedgerStateVersion.GetHashCode();
                if (this.CommittedTransactionOutcomes != null)
                {
                    hashCode = (hashCode * 59) + this.CommittedTransactionOutcomes.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
