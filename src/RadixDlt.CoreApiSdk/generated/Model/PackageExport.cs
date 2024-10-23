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
    /// PackageExport
    /// </summary>
    [DataContract(Name = "PackageExport")]
    public partial class PackageExport : IEquatable<PackageExport>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageExport" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected PackageExport() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageExport" /> class.
        /// </summary>
        /// <param name="codeHash">The hex-encoded code hash, capturing the vm-type and the code itself. (required).</param>
        /// <param name="exportName">exportName (required).</param>
        public PackageExport(string codeHash = default(string), string exportName = default(string))
        {
            // to ensure "codeHash" is required (not null)
            if (codeHash == null)
            {
                throw new ArgumentNullException("codeHash is a required property for PackageExport and cannot be null");
            }
            this.CodeHash = codeHash;
            // to ensure "exportName" is required (not null)
            if (exportName == null)
            {
                throw new ArgumentNullException("exportName is a required property for PackageExport and cannot be null");
            }
            this.ExportName = exportName;
        }

        /// <summary>
        /// The hex-encoded code hash, capturing the vm-type and the code itself.
        /// </summary>
        /// <value>The hex-encoded code hash, capturing the vm-type and the code itself.</value>
        [DataMember(Name = "code_hash", IsRequired = true, EmitDefaultValue = true)]
        public string CodeHash { get; set; }

        /// <summary>
        /// Gets or Sets ExportName
        /// </summary>
        [DataMember(Name = "export_name", IsRequired = true, EmitDefaultValue = true)]
        public string ExportName { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class PackageExport {\n");
            sb.Append("  CodeHash: ").Append(CodeHash).Append("\n");
            sb.Append("  ExportName: ").Append(ExportName).Append("\n");
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
            return this.Equals(input as PackageExport);
        }

        /// <summary>
        /// Returns true if PackageExport instances are equal
        /// </summary>
        /// <param name="input">Instance of PackageExport to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PackageExport input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.CodeHash == input.CodeHash ||
                    (this.CodeHash != null &&
                    this.CodeHash.Equals(input.CodeHash))
                ) && 
                (
                    this.ExportName == input.ExportName ||
                    (this.ExportName != null &&
                    this.ExportName.Equals(input.ExportName))
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
                if (this.CodeHash != null)
                {
                    hashCode = (hashCode * 59) + this.CodeHash.GetHashCode();
                }
                if (this.ExportName != null)
                {
                    hashCode = (hashCode * 59) + this.ExportName.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
