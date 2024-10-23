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
    /// UserLedgerTransactionV2
    /// </summary>
    [DataContract(Name = "UserLedgerTransactionV2")]
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubType(typeof(FlashLedgerTransaction), "Flash")]
    [JsonSubtypes.KnownSubType(typeof(GenesisLedgerTransaction), "Genesis")]
    [JsonSubtypes.KnownSubType(typeof(RoundUpdateLedgerTransaction), "RoundUpdate")]
    [JsonSubtypes.KnownSubType(typeof(UserLedgerTransaction), "User")]
    [JsonSubtypes.KnownSubType(typeof(UserLedgerTransactionV2), "UserV2")]
    public partial class UserLedgerTransactionV2 : LedgerTransaction, IEquatable<UserLedgerTransactionV2>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLedgerTransactionV2" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected UserLedgerTransactionV2() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLedgerTransactionV2" /> class.
        /// </summary>
        /// <param name="notarizedTransaction">notarizedTransaction (required).</param>
        /// <param name="type">type (required) (default to LedgerTransactionType.UserV2).</param>
        /// <param name="payloadHex">The hex-encoded full ledger transaction payload. Only returned if enabled in TransactionFormatOptions on your request..</param>
        public UserLedgerTransactionV2(NotarizedTransactionV2 notarizedTransaction = default(NotarizedTransactionV2), LedgerTransactionType type = LedgerTransactionType.UserV2, string payloadHex = default(string)) : base(type, payloadHex)
        {
            // to ensure "notarizedTransaction" is required (not null)
            if (notarizedTransaction == null)
            {
                throw new ArgumentNullException("notarizedTransaction is a required property for UserLedgerTransactionV2 and cannot be null");
            }
            this.NotarizedTransaction = notarizedTransaction;
        }

        /// <summary>
        /// Gets or Sets NotarizedTransaction
        /// </summary>
        [DataMember(Name = "notarized_transaction", IsRequired = true, EmitDefaultValue = true)]
        public NotarizedTransactionV2 NotarizedTransaction { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class UserLedgerTransactionV2 {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  NotarizedTransaction: ").Append(NotarizedTransaction).Append("\n");
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
            return this.Equals(input as UserLedgerTransactionV2);
        }

        /// <summary>
        /// Returns true if UserLedgerTransactionV2 instances are equal
        /// </summary>
        /// <param name="input">Instance of UserLedgerTransactionV2 to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(UserLedgerTransactionV2 input)
        {
            if (input == null)
            {
                return false;
            }
            return base.Equals(input) && 
                (
                    this.NotarizedTransaction == input.NotarizedTransaction ||
                    (this.NotarizedTransaction != null &&
                    this.NotarizedTransaction.Equals(input.NotarizedTransaction))
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
                if (this.NotarizedTransaction != null)
                {
                    hashCode = (hashCode * 59) + this.NotarizedTransaction.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
