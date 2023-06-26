/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

/*
 * Babylon Core API - RCnet V2
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  It is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  This version of the Core API belongs to the first release candidate of the Radix Babylon network (\"RCnet-V1\").  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` are guaranteed to be forward compatible to Babylon mainnet launch (and beyond). We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
 *
 * The version of the OpenAPI document: 0.4.0
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
    /// BlueprintSchema
    /// </summary>
    [DataContract(Name = "BlueprintSchema")]
    public partial class BlueprintSchema : IEquatable<BlueprintSchema>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlueprintSchema" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected BlueprintSchema() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BlueprintSchema" /> class.
        /// </summary>
        /// <param name="outerBlueprint">outerBlueprint.</param>
        /// <param name="schema">schema (required).</param>
        /// <param name="functionSchemas">A map from the function name to the FunctionSchema (required).</param>
        /// <param name="virtualLazyLoadFunctionSchemas">A map from the system function ID to the VirtualLazyLoadSchema (required).</param>
        /// <param name="eventSchemas">A map from the event name to the local type index for the event payload under the blueprint schema. (required).</param>
        /// <param name="fieldPartition">fieldPartition.</param>
        /// <param name="collectionPartitions">The collection partitions for this blueprint. (required).</param>
        /// <param name="dependencies">dependencies (required).</param>
        /// <param name="features">features (required).</param>
        public BlueprintSchema(string outerBlueprint = default(string), ScryptoSchema schema = default(ScryptoSchema), Dictionary<string, FunctionSchema> functionSchemas = default(Dictionary<string, FunctionSchema>), Dictionary<string, VirtualLazyLoadSchema> virtualLazyLoadFunctionSchemas = default(Dictionary<string, VirtualLazyLoadSchema>), Dictionary<string, LocalTypeIndex> eventSchemas = default(Dictionary<string, LocalTypeIndex>), BlueprintSchemaFieldPartition fieldPartition = default(BlueprintSchemaFieldPartition), List<BlueprintSchemaCollectionPartition> collectionPartitions = default(List<BlueprintSchemaCollectionPartition>), List<string> dependencies = default(List<string>), List<string> features = default(List<string>))
        {
            // to ensure "schema" is required (not null)
            if (schema == null)
            {
                throw new ArgumentNullException("schema is a required property for BlueprintSchema and cannot be null");
            }
            this.Schema = schema;
            // to ensure "functionSchemas" is required (not null)
            if (functionSchemas == null)
            {
                throw new ArgumentNullException("functionSchemas is a required property for BlueprintSchema and cannot be null");
            }
            this.FunctionSchemas = functionSchemas;
            // to ensure "virtualLazyLoadFunctionSchemas" is required (not null)
            if (virtualLazyLoadFunctionSchemas == null)
            {
                throw new ArgumentNullException("virtualLazyLoadFunctionSchemas is a required property for BlueprintSchema and cannot be null");
            }
            this.VirtualLazyLoadFunctionSchemas = virtualLazyLoadFunctionSchemas;
            // to ensure "eventSchemas" is required (not null)
            if (eventSchemas == null)
            {
                throw new ArgumentNullException("eventSchemas is a required property for BlueprintSchema and cannot be null");
            }
            this.EventSchemas = eventSchemas;
            // to ensure "collectionPartitions" is required (not null)
            if (collectionPartitions == null)
            {
                throw new ArgumentNullException("collectionPartitions is a required property for BlueprintSchema and cannot be null");
            }
            this.CollectionPartitions = collectionPartitions;
            // to ensure "dependencies" is required (not null)
            if (dependencies == null)
            {
                throw new ArgumentNullException("dependencies is a required property for BlueprintSchema and cannot be null");
            }
            this.Dependencies = dependencies;
            // to ensure "features" is required (not null)
            if (features == null)
            {
                throw new ArgumentNullException("features is a required property for BlueprintSchema and cannot be null");
            }
            this.Features = features;
            this.OuterBlueprint = outerBlueprint;
            this.FieldPartition = fieldPartition;
        }

        /// <summary>
        /// Gets or Sets OuterBlueprint
        /// </summary>
        [DataMember(Name = "outer_blueprint", EmitDefaultValue = true)]
        public string OuterBlueprint { get; set; }

        /// <summary>
        /// Gets or Sets Schema
        /// </summary>
        [DataMember(Name = "schema", IsRequired = true, EmitDefaultValue = true)]
        public ScryptoSchema Schema { get; set; }

        /// <summary>
        /// A map from the function name to the FunctionSchema
        /// </summary>
        /// <value>A map from the function name to the FunctionSchema</value>
        [DataMember(Name = "function_schemas", IsRequired = true, EmitDefaultValue = true)]
        public Dictionary<string, FunctionSchema> FunctionSchemas { get; set; }

        /// <summary>
        /// A map from the system function ID to the VirtualLazyLoadSchema
        /// </summary>
        /// <value>A map from the system function ID to the VirtualLazyLoadSchema</value>
        [DataMember(Name = "virtual_lazy_load_function_schemas", IsRequired = true, EmitDefaultValue = true)]
        public Dictionary<string, VirtualLazyLoadSchema> VirtualLazyLoadFunctionSchemas { get; set; }

        /// <summary>
        /// A map from the event name to the local type index for the event payload under the blueprint schema.
        /// </summary>
        /// <value>A map from the event name to the local type index for the event payload under the blueprint schema.</value>
        [DataMember(Name = "event_schemas", IsRequired = true, EmitDefaultValue = true)]
        public Dictionary<string, LocalTypeIndex> EventSchemas { get; set; }

        /// <summary>
        /// Gets or Sets FieldPartition
        /// </summary>
        [DataMember(Name = "field_partition", EmitDefaultValue = true)]
        public BlueprintSchemaFieldPartition FieldPartition { get; set; }

        /// <summary>
        /// The collection partitions for this blueprint.
        /// </summary>
        /// <value>The collection partitions for this blueprint.</value>
        [DataMember(Name = "collection_partitions", IsRequired = true, EmitDefaultValue = true)]
        public List<BlueprintSchemaCollectionPartition> CollectionPartitions { get; set; }

        /// <summary>
        /// Gets or Sets Dependencies
        /// </summary>
        [DataMember(Name = "dependencies", IsRequired = true, EmitDefaultValue = true)]
        public List<string> Dependencies { get; set; }

        /// <summary>
        /// Gets or Sets Features
        /// </summary>
        [DataMember(Name = "features", IsRequired = true, EmitDefaultValue = true)]
        public List<string> Features { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class BlueprintSchema {\n");
            sb.Append("  OuterBlueprint: ").Append(OuterBlueprint).Append("\n");
            sb.Append("  Schema: ").Append(Schema).Append("\n");
            sb.Append("  FunctionSchemas: ").Append(FunctionSchemas).Append("\n");
            sb.Append("  VirtualLazyLoadFunctionSchemas: ").Append(VirtualLazyLoadFunctionSchemas).Append("\n");
            sb.Append("  EventSchemas: ").Append(EventSchemas).Append("\n");
            sb.Append("  FieldPartition: ").Append(FieldPartition).Append("\n");
            sb.Append("  CollectionPartitions: ").Append(CollectionPartitions).Append("\n");
            sb.Append("  Dependencies: ").Append(Dependencies).Append("\n");
            sb.Append("  Features: ").Append(Features).Append("\n");
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
            return this.Equals(input as BlueprintSchema);
        }

        /// <summary>
        /// Returns true if BlueprintSchema instances are equal
        /// </summary>
        /// <param name="input">Instance of BlueprintSchema to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(BlueprintSchema input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.OuterBlueprint == input.OuterBlueprint ||
                    (this.OuterBlueprint != null &&
                    this.OuterBlueprint.Equals(input.OuterBlueprint))
                ) && 
                (
                    this.Schema == input.Schema ||
                    (this.Schema != null &&
                    this.Schema.Equals(input.Schema))
                ) && 
                (
                    this.FunctionSchemas == input.FunctionSchemas ||
                    this.FunctionSchemas != null &&
                    input.FunctionSchemas != null &&
                    this.FunctionSchemas.SequenceEqual(input.FunctionSchemas)
                ) && 
                (
                    this.VirtualLazyLoadFunctionSchemas == input.VirtualLazyLoadFunctionSchemas ||
                    this.VirtualLazyLoadFunctionSchemas != null &&
                    input.VirtualLazyLoadFunctionSchemas != null &&
                    this.VirtualLazyLoadFunctionSchemas.SequenceEqual(input.VirtualLazyLoadFunctionSchemas)
                ) && 
                (
                    this.EventSchemas == input.EventSchemas ||
                    this.EventSchemas != null &&
                    input.EventSchemas != null &&
                    this.EventSchemas.SequenceEqual(input.EventSchemas)
                ) && 
                (
                    this.FieldPartition == input.FieldPartition ||
                    (this.FieldPartition != null &&
                    this.FieldPartition.Equals(input.FieldPartition))
                ) && 
                (
                    this.CollectionPartitions == input.CollectionPartitions ||
                    this.CollectionPartitions != null &&
                    input.CollectionPartitions != null &&
                    this.CollectionPartitions.SequenceEqual(input.CollectionPartitions)
                ) && 
                (
                    this.Dependencies == input.Dependencies ||
                    this.Dependencies != null &&
                    input.Dependencies != null &&
                    this.Dependencies.SequenceEqual(input.Dependencies)
                ) && 
                (
                    this.Features == input.Features ||
                    this.Features != null &&
                    input.Features != null &&
                    this.Features.SequenceEqual(input.Features)
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
                if (this.OuterBlueprint != null)
                {
                    hashCode = (hashCode * 59) + this.OuterBlueprint.GetHashCode();
                }
                if (this.Schema != null)
                {
                    hashCode = (hashCode * 59) + this.Schema.GetHashCode();
                }
                if (this.FunctionSchemas != null)
                {
                    hashCode = (hashCode * 59) + this.FunctionSchemas.GetHashCode();
                }
                if (this.VirtualLazyLoadFunctionSchemas != null)
                {
                    hashCode = (hashCode * 59) + this.VirtualLazyLoadFunctionSchemas.GetHashCode();
                }
                if (this.EventSchemas != null)
                {
                    hashCode = (hashCode * 59) + this.EventSchemas.GetHashCode();
                }
                if (this.FieldPartition != null)
                {
                    hashCode = (hashCode * 59) + this.FieldPartition.GetHashCode();
                }
                if (this.CollectionPartitions != null)
                {
                    hashCode = (hashCode * 59) + this.CollectionPartitions.GetHashCode();
                }
                if (this.Dependencies != null)
                {
                    hashCode = (hashCode * 59) + this.Dependencies.GetHashCode();
                }
                if (this.Features != null)
                {
                    hashCode = (hashCode * 59) + this.Features.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
