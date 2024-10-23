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
    /// StateAccessControllerResponse
    /// </summary>
    [DataContract(Name = "StateAccessControllerResponse")]
    public partial class StateAccessControllerResponse : IEquatable<StateAccessControllerResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateAccessControllerResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected StateAccessControllerResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StateAccessControllerResponse" /> class.
        /// </summary>
        /// <param name="atLedgerState">atLedgerState (required).</param>
        /// <param name="state">state (required).</param>
        /// <param name="ownerRole">ownerRole (required).</param>
        /// <param name="vaults">Any vaults owned directly or indirectly by the component (required).</param>
        /// <param name="descendentNodes">Any descendent nodes owned directly or indirectly by the component (required).</param>
        public StateAccessControllerResponse(LedgerStateSummary atLedgerState = default(LedgerStateSummary), Substate state = default(Substate), Substate ownerRole = default(Substate), List<VaultBalance> vaults = default(List<VaultBalance>), List<StateComponentDescendentNode> descendentNodes = default(List<StateComponentDescendentNode>))
        {
            // to ensure "atLedgerState" is required (not null)
            if (atLedgerState == null)
            {
                throw new ArgumentNullException("atLedgerState is a required property for StateAccessControllerResponse and cannot be null");
            }
            this.AtLedgerState = atLedgerState;
            // to ensure "state" is required (not null)
            if (state == null)
            {
                throw new ArgumentNullException("state is a required property for StateAccessControllerResponse and cannot be null");
            }
            this.State = state;
            // to ensure "ownerRole" is required (not null)
            if (ownerRole == null)
            {
                throw new ArgumentNullException("ownerRole is a required property for StateAccessControllerResponse and cannot be null");
            }
            this.OwnerRole = ownerRole;
            // to ensure "vaults" is required (not null)
            if (vaults == null)
            {
                throw new ArgumentNullException("vaults is a required property for StateAccessControllerResponse and cannot be null");
            }
            this.Vaults = vaults;
            // to ensure "descendentNodes" is required (not null)
            if (descendentNodes == null)
            {
                throw new ArgumentNullException("descendentNodes is a required property for StateAccessControllerResponse and cannot be null");
            }
            this.DescendentNodes = descendentNodes;
        }

        /// <summary>
        /// Gets or Sets AtLedgerState
        /// </summary>
        [DataMember(Name = "at_ledger_state", IsRequired = true, EmitDefaultValue = true)]
        public LedgerStateSummary AtLedgerState { get; set; }

        /// <summary>
        /// Gets or Sets State
        /// </summary>
        [DataMember(Name = "state", IsRequired = true, EmitDefaultValue = true)]
        public Substate State { get; set; }

        /// <summary>
        /// Gets or Sets OwnerRole
        /// </summary>
        [DataMember(Name = "owner_role", IsRequired = true, EmitDefaultValue = true)]
        public Substate OwnerRole { get; set; }

        /// <summary>
        /// Any vaults owned directly or indirectly by the component
        /// </summary>
        /// <value>Any vaults owned directly or indirectly by the component</value>
        [DataMember(Name = "vaults", IsRequired = true, EmitDefaultValue = true)]
        public List<VaultBalance> Vaults { get; set; }

        /// <summary>
        /// Any descendent nodes owned directly or indirectly by the component
        /// </summary>
        /// <value>Any descendent nodes owned directly or indirectly by the component</value>
        [DataMember(Name = "descendent_nodes", IsRequired = true, EmitDefaultValue = true)]
        public List<StateComponentDescendentNode> DescendentNodes { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StateAccessControllerResponse {\n");
            sb.Append("  AtLedgerState: ").Append(AtLedgerState).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
            sb.Append("  OwnerRole: ").Append(OwnerRole).Append("\n");
            sb.Append("  Vaults: ").Append(Vaults).Append("\n");
            sb.Append("  DescendentNodes: ").Append(DescendentNodes).Append("\n");
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
            return this.Equals(input as StateAccessControllerResponse);
        }

        /// <summary>
        /// Returns true if StateAccessControllerResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of StateAccessControllerResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StateAccessControllerResponse input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.AtLedgerState == input.AtLedgerState ||
                    (this.AtLedgerState != null &&
                    this.AtLedgerState.Equals(input.AtLedgerState))
                ) && 
                (
                    this.State == input.State ||
                    (this.State != null &&
                    this.State.Equals(input.State))
                ) && 
                (
                    this.OwnerRole == input.OwnerRole ||
                    (this.OwnerRole != null &&
                    this.OwnerRole.Equals(input.OwnerRole))
                ) && 
                (
                    this.Vaults == input.Vaults ||
                    this.Vaults != null &&
                    input.Vaults != null &&
                    this.Vaults.SequenceEqual(input.Vaults)
                ) && 
                (
                    this.DescendentNodes == input.DescendentNodes ||
                    this.DescendentNodes != null &&
                    input.DescendentNodes != null &&
                    this.DescendentNodes.SequenceEqual(input.DescendentNodes)
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
                if (this.AtLedgerState != null)
                {
                    hashCode = (hashCode * 59) + this.AtLedgerState.GetHashCode();
                }
                if (this.State != null)
                {
                    hashCode = (hashCode * 59) + this.State.GetHashCode();
                }
                if (this.OwnerRole != null)
                {
                    hashCode = (hashCode * 59) + this.OwnerRole.GetHashCode();
                }
                if (this.Vaults != null)
                {
                    hashCode = (hashCode * 59) + this.Vaults.GetHashCode();
                }
                if (this.DescendentNodes != null)
                {
                    hashCode = (hashCode * 59) + this.DescendentNodes.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
