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
    /// StateAccessControllerRequest
    /// </summary>
    [DataContract(Name = "StateAccessControllerRequest")]
    public partial class StateAccessControllerRequest : IEquatable<StateAccessControllerRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateAccessControllerRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected StateAccessControllerRequest() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StateAccessControllerRequest" /> class.
        /// </summary>
        /// <param name="network">The logical name of the network (required).</param>
        /// <param name="controllerAddress">The Bech32m-encoded human readable version of the component address (required).</param>
        public StateAccessControllerRequest(string network = default(string), string controllerAddress = default(string))
        {
            // to ensure "network" is required (not null)
            if (network == null)
            {
                throw new ArgumentNullException("network is a required property for StateAccessControllerRequest and cannot be null");
            }
            this.Network = network;
            // to ensure "controllerAddress" is required (not null)
            if (controllerAddress == null)
            {
                throw new ArgumentNullException("controllerAddress is a required property for StateAccessControllerRequest and cannot be null");
            }
            this.ControllerAddress = controllerAddress;
        }

        /// <summary>
        /// The logical name of the network
        /// </summary>
        /// <value>The logical name of the network</value>
        [DataMember(Name = "network", IsRequired = true, EmitDefaultValue = true)]
        public string Network { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable version of the component address
        /// </summary>
        /// <value>The Bech32m-encoded human readable version of the component address</value>
        [DataMember(Name = "controller_address", IsRequired = true, EmitDefaultValue = true)]
        public string ControllerAddress { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StateAccessControllerRequest {\n");
            sb.Append("  Network: ").Append(Network).Append("\n");
            sb.Append("  ControllerAddress: ").Append(ControllerAddress).Append("\n");
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
            return this.Equals(input as StateAccessControllerRequest);
        }

        /// <summary>
        /// Returns true if StateAccessControllerRequest instances are equal
        /// </summary>
        /// <param name="input">Instance of StateAccessControllerRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StateAccessControllerRequest input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Network == input.Network ||
                    (this.Network != null &&
                    this.Network.Equals(input.Network))
                ) && 
                (
                    this.ControllerAddress == input.ControllerAddress ||
                    (this.ControllerAddress != null &&
                    this.ControllerAddress.Equals(input.ControllerAddress))
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
                if (this.Network != null)
                {
                    hashCode = (hashCode * 59) + this.Network.GetHashCode();
                }
                if (this.ControllerAddress != null)
                {
                    hashCode = (hashCode * 59) + this.ControllerAddress.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
