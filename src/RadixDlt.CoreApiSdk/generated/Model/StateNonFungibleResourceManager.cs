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
    /// StateNonFungibleResourceManager
    /// </summary>
    [DataContract(Name = "StateNonFungibleResourceManager")]
    [JsonConverter(typeof(JsonSubtypes), "resource_type")]
    [JsonSubtypes.KnownSubType(typeof(StateFungibleResourceManager), "Fungible")]
    [JsonSubtypes.KnownSubType(typeof(StateNonFungibleResourceManager), "NonFungible")]
    public partial class StateNonFungibleResourceManager : StateResourceManager, IEquatable<StateNonFungibleResourceManager>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateNonFungibleResourceManager" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected StateNonFungibleResourceManager() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StateNonFungibleResourceManager" /> class.
        /// </summary>
        /// <param name="idType">idType (required).</param>
        /// <param name="totalSupply">totalSupply.</param>
        /// <param name="mutableFields">mutableFields (required).</param>
        /// <param name="resourceType">resourceType (required) (default to ResourceType.NonFungible).</param>
        public StateNonFungibleResourceManager(Substate idType = default(Substate), Substate totalSupply = default(Substate), Substate mutableFields = default(Substate), ResourceType resourceType = ResourceType.NonFungible) : base(resourceType)
        {
            // to ensure "idType" is required (not null)
            if (idType == null)
            {
                throw new ArgumentNullException("idType is a required property for StateNonFungibleResourceManager and cannot be null");
            }
            this.IdType = idType;
            // to ensure "mutableFields" is required (not null)
            if (mutableFields == null)
            {
                throw new ArgumentNullException("mutableFields is a required property for StateNonFungibleResourceManager and cannot be null");
            }
            this.MutableFields = mutableFields;
            this.TotalSupply = totalSupply;
        }

        /// <summary>
        /// Gets or Sets IdType
        /// </summary>
        [DataMember(Name = "id_type", IsRequired = true, EmitDefaultValue = true)]
        public Substate IdType { get; set; }

        /// <summary>
        /// Gets or Sets TotalSupply
        /// </summary>
        [DataMember(Name = "total_supply", EmitDefaultValue = true)]
        public Substate TotalSupply { get; set; }

        /// <summary>
        /// Gets or Sets MutableFields
        /// </summary>
        [DataMember(Name = "mutable_fields", IsRequired = true, EmitDefaultValue = true)]
        public Substate MutableFields { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StateNonFungibleResourceManager {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  IdType: ").Append(IdType).Append("\n");
            sb.Append("  TotalSupply: ").Append(TotalSupply).Append("\n");
            sb.Append("  MutableFields: ").Append(MutableFields).Append("\n");
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
            return this.Equals(input as StateNonFungibleResourceManager);
        }

        /// <summary>
        /// Returns true if StateNonFungibleResourceManager instances are equal
        /// </summary>
        /// <param name="input">Instance of StateNonFungibleResourceManager to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StateNonFungibleResourceManager input)
        {
            if (input == null)
            {
                return false;
            }
            return base.Equals(input) && 
                (
                    this.IdType == input.IdType ||
                    (this.IdType != null &&
                    this.IdType.Equals(input.IdType))
                ) && base.Equals(input) && 
                (
                    this.TotalSupply == input.TotalSupply ||
                    (this.TotalSupply != null &&
                    this.TotalSupply.Equals(input.TotalSupply))
                ) && base.Equals(input) && 
                (
                    this.MutableFields == input.MutableFields ||
                    (this.MutableFields != null &&
                    this.MutableFields.Equals(input.MutableFields))
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
                if (this.IdType != null)
                {
                    hashCode = (hashCode * 59) + this.IdType.GetHashCode();
                }
                if (this.TotalSupply != null)
                {
                    hashCode = (hashCode * 59) + this.TotalSupply.GetHashCode();
                }
                if (this.MutableFields != null)
                {
                    hashCode = (hashCode * 59) + this.MutableFields.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
