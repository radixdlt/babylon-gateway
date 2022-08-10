/*
 * Radix Babylon Gateway API
 *
 * See https://docs.radixdlt.com/main/apis/introduction.html 
 *
 * The version of the OpenAPI document: 2.0.0
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
using System.ComponentModel.DataAnnotations;
using FileParameter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// Optional. Allows a client to request a response referencing an earlier ledger state. If defined only one of Version, Timestamp, Epoch or Epoch and Round pair MUST be defined.
    /// </summary>
    [DataContract(Name = "PartialLedgerStateIdentifier")]
    public partial class PartialLedgerStateIdentifier : IEquatable<PartialLedgerStateIdentifier>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartialLedgerStateIdentifier" /> class.
        /// </summary>
        /// <param name="stateVersion">If the version is provided, the latest ledger state &lt;&#x3D; the given version is returned..</param>
        /// <param name="timestamp">If a timestamp is provided, the latest ledger state &lt;&#x3D; the given timestamp is returned..</param>
        /// <param name="epoch">If an epoch is provided, the ledger state at the given epoch &lt;&#x3D; the given round (else round 0) is returned..</param>
        /// <param name="round">round.</param>
        public PartialLedgerStateIdentifier(long? stateVersion = default(long?), DateTimeOffset? timestamp = default(DateTimeOffset?), long? epoch = default(long?), long? round = default(long?))
        {
            this.StateVersion = stateVersion;
            this.Timestamp = timestamp;
            this.Epoch = epoch;
            this.Round = round;
        }

        /// <summary>
        /// If the version is provided, the latest ledger state &lt;&#x3D; the given version is returned.
        /// </summary>
        /// <value>If the version is provided, the latest ledger state &lt;&#x3D; the given version is returned.</value>
        [DataMember(Name = "state_version", EmitDefaultValue = true)]
        public long? StateVersion { get; set; }

        /// <summary>
        /// If a timestamp is provided, the latest ledger state &lt;&#x3D; the given timestamp is returned.
        /// </summary>
        /// <value>If a timestamp is provided, the latest ledger state &lt;&#x3D; the given timestamp is returned.</value>
        [DataMember(Name = "timestamp", EmitDefaultValue = true)]
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// If an epoch is provided, the ledger state at the given epoch &lt;&#x3D; the given round (else round 0) is returned.
        /// </summary>
        /// <value>If an epoch is provided, the ledger state at the given epoch &lt;&#x3D; the given round (else round 0) is returned.</value>
        [DataMember(Name = "epoch", EmitDefaultValue = true)]
        public long? Epoch { get; set; }

        /// <summary>
        /// Gets or Sets Round
        /// </summary>
        [DataMember(Name = "round", EmitDefaultValue = true)]
        public long? Round { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class PartialLedgerStateIdentifier {\n");
            sb.Append("  StateVersion: ").Append(StateVersion).Append("\n");
            sb.Append("  Timestamp: ").Append(Timestamp).Append("\n");
            sb.Append("  Epoch: ").Append(Epoch).Append("\n");
            sb.Append("  Round: ").Append(Round).Append("\n");
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
            return this.Equals(input as PartialLedgerStateIdentifier);
        }

        /// <summary>
        /// Returns true if PartialLedgerStateIdentifier instances are equal
        /// </summary>
        /// <param name="input">Instance of PartialLedgerStateIdentifier to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PartialLedgerStateIdentifier input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.StateVersion == input.StateVersion ||
                    (this.StateVersion != null &&
                    this.StateVersion.Equals(input.StateVersion))
                ) && 
                (
                    this.Timestamp == input.Timestamp ||
                    (this.Timestamp != null &&
                    this.Timestamp.Equals(input.Timestamp))
                ) && 
                (
                    this.Epoch == input.Epoch ||
                    (this.Epoch != null &&
                    this.Epoch.Equals(input.Epoch))
                ) && 
                (
                    this.Round == input.Round ||
                    (this.Round != null &&
                    this.Round.Equals(input.Round))
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
                if (this.StateVersion != null)
                {
                    hashCode = (hashCode * 59) + this.StateVersion.GetHashCode();
                }
                if (this.Timestamp != null)
                {
                    hashCode = (hashCode * 59) + this.Timestamp.GetHashCode();
                }
                if (this.Epoch != null)
                {
                    hashCode = (hashCode * 59) + this.Epoch.GetHashCode();
                }
                if (this.Round != null)
                {
                    hashCode = (hashCode * 59) + this.Round.GetHashCode();
                }
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
