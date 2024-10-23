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
    /// PackageBlueprintAuthTemplateEntryValue
    /// </summary>
    [DataContract(Name = "PackageBlueprintAuthTemplateEntryValue")]
    public partial class PackageBlueprintAuthTemplateEntryValue : IEquatable<PackageBlueprintAuthTemplateEntryValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageBlueprintAuthTemplateEntryValue" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected PackageBlueprintAuthTemplateEntryValue() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageBlueprintAuthTemplateEntryValue" /> class.
        /// </summary>
        /// <param name="authConfig">authConfig (required).</param>
        public PackageBlueprintAuthTemplateEntryValue(AuthConfig authConfig = default(AuthConfig))
        {
            // to ensure "authConfig" is required (not null)
            if (authConfig == null)
            {
                throw new ArgumentNullException("authConfig is a required property for PackageBlueprintAuthTemplateEntryValue and cannot be null");
            }
            this.AuthConfig = authConfig;
        }

        /// <summary>
        /// Gets or Sets AuthConfig
        /// </summary>
        [DataMember(Name = "auth_config", IsRequired = true, EmitDefaultValue = true)]
        public AuthConfig AuthConfig { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class PackageBlueprintAuthTemplateEntryValue {\n");
            sb.Append("  AuthConfig: ").Append(AuthConfig).Append("\n");
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
            return this.Equals(input as PackageBlueprintAuthTemplateEntryValue);
        }

        /// <summary>
        /// Returns true if PackageBlueprintAuthTemplateEntryValue instances are equal
        /// </summary>
        /// <param name="input">Instance of PackageBlueprintAuthTemplateEntryValue to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PackageBlueprintAuthTemplateEntryValue input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.AuthConfig == input.AuthConfig ||
                    (this.AuthConfig != null &&
                    this.AuthConfig.Equals(input.AuthConfig))
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
                if (this.AuthConfig != null)
                {
                    hashCode = (hashCode * 59) + this.AuthConfig.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
