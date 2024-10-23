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
    /// ProtectedAccessRuleAllOf
    /// </summary>
    [DataContract(Name = "ProtectedAccessRule_allOf")]
    public partial class ProtectedAccessRuleAllOf : IEquatable<ProtectedAccessRuleAllOf>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedAccessRuleAllOf" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ProtectedAccessRuleAllOf() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedAccessRuleAllOf" /> class.
        /// </summary>
        /// <param name="accessRule">accessRule (required).</param>
        public ProtectedAccessRuleAllOf(AccessRuleNode accessRule = default(AccessRuleNode))
        {
            // to ensure "accessRule" is required (not null)
            if (accessRule == null)
            {
                throw new ArgumentNullException("accessRule is a required property for ProtectedAccessRuleAllOf and cannot be null");
            }
            this.AccessRule = accessRule;
        }

        /// <summary>
        /// Gets or Sets AccessRule
        /// </summary>
        [DataMember(Name = "access_rule", IsRequired = true, EmitDefaultValue = true)]
        public AccessRuleNode AccessRule { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ProtectedAccessRuleAllOf {\n");
            sb.Append("  AccessRule: ").Append(AccessRule).Append("\n");
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
            return this.Equals(input as ProtectedAccessRuleAllOf);
        }

        /// <summary>
        /// Returns true if ProtectedAccessRuleAllOf instances are equal
        /// </summary>
        /// <param name="input">Instance of ProtectedAccessRuleAllOf to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ProtectedAccessRuleAllOf input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.AccessRule == input.AccessRule ||
                    (this.AccessRule != null &&
                    this.AccessRule.Equals(input.AccessRule))
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
                if (this.AccessRule != null)
                {
                    hashCode = (hashCode * 59) + this.AccessRule.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
