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
    /// The fields partition of the blueprint.
    /// </summary>
    [DataContract(Name = "BlueprintSchemaCollectionPartition")]
    public partial class BlueprintSchemaCollectionPartition : IEquatable<BlueprintSchemaCollectionPartition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlueprintSchemaCollectionPartition" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected BlueprintSchemaCollectionPartition() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BlueprintSchemaCollectionPartition" /> class.
        /// </summary>
        /// <param name="partitionDescription">partitionDescription (required).</param>
        /// <param name="collectionSchema">collectionSchema (required).</param>
        public BlueprintSchemaCollectionPartition(PartitionDescription partitionDescription = default(PartitionDescription), BlueprintCollectionSchema collectionSchema = default(BlueprintCollectionSchema))
        {
            // to ensure "partitionDescription" is required (not null)
            if (partitionDescription == null)
            {
                throw new ArgumentNullException("partitionDescription is a required property for BlueprintSchemaCollectionPartition and cannot be null");
            }
            this.PartitionDescription = partitionDescription;
            // to ensure "collectionSchema" is required (not null)
            if (collectionSchema == null)
            {
                throw new ArgumentNullException("collectionSchema is a required property for BlueprintSchemaCollectionPartition and cannot be null");
            }
            this.CollectionSchema = collectionSchema;
        }

        /// <summary>
        /// Gets or Sets PartitionDescription
        /// </summary>
        [DataMember(Name = "partition_description", IsRequired = true, EmitDefaultValue = true)]
        public PartitionDescription PartitionDescription { get; set; }

        /// <summary>
        /// Gets or Sets CollectionSchema
        /// </summary>
        [DataMember(Name = "collection_schema", IsRequired = true, EmitDefaultValue = true)]
        public BlueprintCollectionSchema CollectionSchema { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class BlueprintSchemaCollectionPartition {\n");
            sb.Append("  PartitionDescription: ").Append(PartitionDescription).Append("\n");
            sb.Append("  CollectionSchema: ").Append(CollectionSchema).Append("\n");
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
            return this.Equals(input as BlueprintSchemaCollectionPartition);
        }

        /// <summary>
        /// Returns true if BlueprintSchemaCollectionPartition instances are equal
        /// </summary>
        /// <param name="input">Instance of BlueprintSchemaCollectionPartition to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(BlueprintSchemaCollectionPartition input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.PartitionDescription == input.PartitionDescription ||
                    (this.PartitionDescription != null &&
                    this.PartitionDescription.Equals(input.PartitionDescription))
                ) && 
                (
                    this.CollectionSchema == input.CollectionSchema ||
                    (this.CollectionSchema != null &&
                    this.CollectionSchema.Equals(input.CollectionSchema))
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
                if (this.PartitionDescription != null)
                {
                    hashCode = (hashCode * 59) + this.PartitionDescription.GetHashCode();
                }
                if (this.CollectionSchema != null)
                {
                    hashCode = (hashCode * 59) + this.CollectionSchema.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
