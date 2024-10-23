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
    /// DataStruct
    /// </summary>
    [DataContract(Name = "DataStruct")]
    public partial class DataStruct : IEquatable<DataStruct>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataStruct" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected DataStruct() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DataStruct" /> class.
        /// </summary>
        /// <param name="structData">structData (required).</param>
        /// <param name="ownedEntities">ownedEntities (required).</param>
        /// <param name="referencedEntities">referencedEntities (required).</param>
        public DataStruct(SborData structData = default(SborData), List<EntityReference> ownedEntities = default(List<EntityReference>), List<EntityReference> referencedEntities = default(List<EntityReference>))
        {
            // to ensure "structData" is required (not null)
            if (structData == null)
            {
                throw new ArgumentNullException("structData is a required property for DataStruct and cannot be null");
            }
            this.StructData = structData;
            // to ensure "ownedEntities" is required (not null)
            if (ownedEntities == null)
            {
                throw new ArgumentNullException("ownedEntities is a required property for DataStruct and cannot be null");
            }
            this.OwnedEntities = ownedEntities;
            // to ensure "referencedEntities" is required (not null)
            if (referencedEntities == null)
            {
                throw new ArgumentNullException("referencedEntities is a required property for DataStruct and cannot be null");
            }
            this.ReferencedEntities = referencedEntities;
        }

        /// <summary>
        /// Gets or Sets StructData
        /// </summary>
        [DataMember(Name = "struct_data", IsRequired = true, EmitDefaultValue = true)]
        public SborData StructData { get; set; }

        /// <summary>
        /// Gets or Sets OwnedEntities
        /// </summary>
        [DataMember(Name = "owned_entities", IsRequired = true, EmitDefaultValue = true)]
        public List<EntityReference> OwnedEntities { get; set; }

        /// <summary>
        /// Gets or Sets ReferencedEntities
        /// </summary>
        [DataMember(Name = "referenced_entities", IsRequired = true, EmitDefaultValue = true)]
        public List<EntityReference> ReferencedEntities { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class DataStruct {\n");
            sb.Append("  StructData: ").Append(StructData).Append("\n");
            sb.Append("  OwnedEntities: ").Append(OwnedEntities).Append("\n");
            sb.Append("  ReferencedEntities: ").Append(ReferencedEntities).Append("\n");
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
            return this.Equals(input as DataStruct);
        }

        /// <summary>
        /// Returns true if DataStruct instances are equal
        /// </summary>
        /// <param name="input">Instance of DataStruct to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(DataStruct input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.StructData == input.StructData ||
                    (this.StructData != null &&
                    this.StructData.Equals(input.StructData))
                ) && 
                (
                    this.OwnedEntities == input.OwnedEntities ||
                    this.OwnedEntities != null &&
                    input.OwnedEntities != null &&
                    this.OwnedEntities.SequenceEqual(input.OwnedEntities)
                ) && 
                (
                    this.ReferencedEntities == input.ReferencedEntities ||
                    this.ReferencedEntities != null &&
                    input.ReferencedEntities != null &&
                    this.ReferencedEntities.SequenceEqual(input.ReferencedEntities)
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
                if (this.StructData != null)
                {
                    hashCode = (hashCode * 59) + this.StructData.GetHashCode();
                }
                if (this.OwnedEntities != null)
                {
                    hashCode = (hashCode * 59) + this.OwnedEntities.GetHashCode();
                }
                if (this.ReferencedEntities != null)
                {
                    hashCode = (hashCode * 59) + this.ReferencedEntities.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
