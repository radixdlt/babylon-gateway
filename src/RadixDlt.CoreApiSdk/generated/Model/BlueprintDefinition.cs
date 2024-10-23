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
    /// BlueprintDefinition
    /// </summary>
    [DataContract(Name = "BlueprintDefinition")]
    public partial class BlueprintDefinition : IEquatable<BlueprintDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlueprintDefinition" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected BlueprintDefinition() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BlueprintDefinition" /> class.
        /// </summary>
        /// <param name="_interface">_interface (required).</param>
        /// <param name="functionExports">A map from the function name to its export (required).</param>
        /// <param name="hookExports">A map from certain object lifecycle hooks to a callback \&quot;package export\&quot;. There is at most one callback registered for each &#x60;ObjectHook&#x60;.  (required).</param>
        public BlueprintDefinition(BlueprintInterface _interface = default(BlueprintInterface), Dictionary<string, PackageExport> functionExports = default(Dictionary<string, PackageExport>), List<HookExport> hookExports = default(List<HookExport>))
        {
            // to ensure "_interface" is required (not null)
            if (_interface == null)
            {
                throw new ArgumentNullException("_interface is a required property for BlueprintDefinition and cannot be null");
            }
            this.Interface = _interface;
            // to ensure "functionExports" is required (not null)
            if (functionExports == null)
            {
                throw new ArgumentNullException("functionExports is a required property for BlueprintDefinition and cannot be null");
            }
            this.FunctionExports = functionExports;
            // to ensure "hookExports" is required (not null)
            if (hookExports == null)
            {
                throw new ArgumentNullException("hookExports is a required property for BlueprintDefinition and cannot be null");
            }
            this.HookExports = hookExports;
        }

        /// <summary>
        /// Gets or Sets Interface
        /// </summary>
        [DataMember(Name = "interface", IsRequired = true, EmitDefaultValue = true)]
        public BlueprintInterface Interface { get; set; }

        /// <summary>
        /// A map from the function name to its export
        /// </summary>
        /// <value>A map from the function name to its export</value>
        [DataMember(Name = "function_exports", IsRequired = true, EmitDefaultValue = true)]
        public Dictionary<string, PackageExport> FunctionExports { get; set; }

        /// <summary>
        /// A map from certain object lifecycle hooks to a callback \&quot;package export\&quot;. There is at most one callback registered for each &#x60;ObjectHook&#x60;. 
        /// </summary>
        /// <value>A map from certain object lifecycle hooks to a callback \&quot;package export\&quot;. There is at most one callback registered for each &#x60;ObjectHook&#x60;. </value>
        [DataMember(Name = "hook_exports", IsRequired = true, EmitDefaultValue = true)]
        public List<HookExport> HookExports { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class BlueprintDefinition {\n");
            sb.Append("  Interface: ").Append(Interface).Append("\n");
            sb.Append("  FunctionExports: ").Append(FunctionExports).Append("\n");
            sb.Append("  HookExports: ").Append(HookExports).Append("\n");
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
            return this.Equals(input as BlueprintDefinition);
        }

        /// <summary>
        /// Returns true if BlueprintDefinition instances are equal
        /// </summary>
        /// <param name="input">Instance of BlueprintDefinition to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(BlueprintDefinition input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Interface == input.Interface ||
                    (this.Interface != null &&
                    this.Interface.Equals(input.Interface))
                ) && 
                (
                    this.FunctionExports == input.FunctionExports ||
                    this.FunctionExports != null &&
                    input.FunctionExports != null &&
                    this.FunctionExports.SequenceEqual(input.FunctionExports)
                ) && 
                (
                    this.HookExports == input.HookExports ||
                    this.HookExports != null &&
                    input.HookExports != null &&
                    this.HookExports.SequenceEqual(input.HookExports)
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
                if (this.Interface != null)
                {
                    hashCode = (hashCode * 59) + this.Interface.GetHashCode();
                }
                if (this.FunctionExports != null)
                {
                    hashCode = (hashCode * 59) + this.FunctionExports.GetHashCode();
                }
                if (this.HookExports != null)
                {
                    hashCode = (hashCode * 59) + this.HookExports.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
