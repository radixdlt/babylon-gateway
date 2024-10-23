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
using JsonSubTypes;
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// RoundUpdateLedgerTransaction
    /// </summary>
    [DataContract(Name = "RoundUpdateLedgerTransaction")]
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubType(typeof(FlashLedgerTransaction), "Flash")]
    [JsonSubtypes.KnownSubType(typeof(GenesisLedgerTransaction), "Genesis")]
    [JsonSubtypes.KnownSubType(typeof(RoundUpdateLedgerTransaction), "RoundUpdate")]
    [JsonSubtypes.KnownSubType(typeof(UserLedgerTransaction), "User")]
    [JsonSubtypes.KnownSubType(typeof(UserLedgerTransactionV2), "UserV2")]
    public partial class RoundUpdateLedgerTransaction : LedgerTransaction, IEquatable<RoundUpdateLedgerTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoundUpdateLedgerTransaction" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected RoundUpdateLedgerTransaction() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RoundUpdateLedgerTransaction" /> class.
        /// </summary>
        /// <param name="roundUpdateTransaction">roundUpdateTransaction (required).</param>
        /// <param name="type">type (required) (default to LedgerTransactionType.RoundUpdate).</param>
        /// <param name="payloadHex">The hex-encoded full ledger transaction payload. Only returned if enabled in TransactionFormatOptions on your request..</param>
        public RoundUpdateLedgerTransaction(RoundUpdateTransaction roundUpdateTransaction = default(RoundUpdateTransaction), LedgerTransactionType type = LedgerTransactionType.RoundUpdate, string payloadHex = default(string)) : base(type, payloadHex)
        {
            // to ensure "roundUpdateTransaction" is required (not null)
            if (roundUpdateTransaction == null)
            {
                throw new ArgumentNullException("roundUpdateTransaction is a required property for RoundUpdateLedgerTransaction and cannot be null");
            }
            this.RoundUpdateTransaction = roundUpdateTransaction;
        }

        /// <summary>
        /// Gets or Sets RoundUpdateTransaction
        /// </summary>
        [DataMember(Name = "round_update_transaction", IsRequired = true, EmitDefaultValue = true)]
        public RoundUpdateTransaction RoundUpdateTransaction { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class RoundUpdateLedgerTransaction {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  RoundUpdateTransaction: ").Append(RoundUpdateTransaction).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public override string ToJson()
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
            return this.Equals(input as RoundUpdateLedgerTransaction);
        }

        /// <summary>
        /// Returns true if RoundUpdateLedgerTransaction instances are equal
        /// </summary>
        /// <param name="input">Instance of RoundUpdateLedgerTransaction to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(RoundUpdateLedgerTransaction input)
        {
            if (input == null)
            {
                return false;
            }
            return base.Equals(input) && 
                (
                    this.RoundUpdateTransaction == input.RoundUpdateTransaction ||
                    (this.RoundUpdateTransaction != null &&
                    this.RoundUpdateTransaction.Equals(input.RoundUpdateTransaction))
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
                int hashCode = base.GetHashCode();
                if (this.RoundUpdateTransaction != null)
                {
                    hashCode = (hashCode * 59) + this.RoundUpdateTransaction.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
