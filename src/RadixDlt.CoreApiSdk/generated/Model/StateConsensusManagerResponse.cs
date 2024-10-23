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
    /// StateConsensusManagerResponse
    /// </summary>
    [DataContract(Name = "StateConsensusManagerResponse")]
    public partial class StateConsensusManagerResponse : IEquatable<StateConsensusManagerResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateConsensusManagerResponse" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected StateConsensusManagerResponse() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="StateConsensusManagerResponse" /> class.
        /// </summary>
        /// <param name="atLedgerState">atLedgerState (required).</param>
        /// <param name="config">config (required).</param>
        /// <param name="state">state (required).</param>
        /// <param name="currentProposalStatistic">currentProposalStatistic (required).</param>
        /// <param name="currentValidatorSet">currentValidatorSet (required).</param>
        /// <param name="currentTime">currentTime (required).</param>
        /// <param name="currentTimeRoundedToMinutes">currentTimeRoundedToMinutes (required).</param>
        /// <param name="currentValidatorReadinessSignals">Protocol versions signalled by the current validator set. Every validator from &#x60;current_validator_set&#x60; will be referenced by exactly one of the items here. Only returned if enabled by &#x60;include_readiness_signals&#x60; on your request. .</param>
        public StateConsensusManagerResponse(LedgerStateSummary atLedgerState = default(LedgerStateSummary), Substate config = default(Substate), Substate state = default(Substate), Substate currentProposalStatistic = default(Substate), Substate currentValidatorSet = default(Substate), Substate currentTime = default(Substate), Substate currentTimeRoundedToMinutes = default(Substate), List<ProtocolVersionReadiness> currentValidatorReadinessSignals = default(List<ProtocolVersionReadiness>))
        {
            // to ensure "atLedgerState" is required (not null)
            if (atLedgerState == null)
            {
                throw new ArgumentNullException("atLedgerState is a required property for StateConsensusManagerResponse and cannot be null");
            }
            this.AtLedgerState = atLedgerState;
            // to ensure "config" is required (not null)
            if (config == null)
            {
                throw new ArgumentNullException("config is a required property for StateConsensusManagerResponse and cannot be null");
            }
            this.Config = config;
            // to ensure "state" is required (not null)
            if (state == null)
            {
                throw new ArgumentNullException("state is a required property for StateConsensusManagerResponse and cannot be null");
            }
            this.State = state;
            // to ensure "currentProposalStatistic" is required (not null)
            if (currentProposalStatistic == null)
            {
                throw new ArgumentNullException("currentProposalStatistic is a required property for StateConsensusManagerResponse and cannot be null");
            }
            this.CurrentProposalStatistic = currentProposalStatistic;
            // to ensure "currentValidatorSet" is required (not null)
            if (currentValidatorSet == null)
            {
                throw new ArgumentNullException("currentValidatorSet is a required property for StateConsensusManagerResponse and cannot be null");
            }
            this.CurrentValidatorSet = currentValidatorSet;
            // to ensure "currentTime" is required (not null)
            if (currentTime == null)
            {
                throw new ArgumentNullException("currentTime is a required property for StateConsensusManagerResponse and cannot be null");
            }
            this.CurrentTime = currentTime;
            // to ensure "currentTimeRoundedToMinutes" is required (not null)
            if (currentTimeRoundedToMinutes == null)
            {
                throw new ArgumentNullException("currentTimeRoundedToMinutes is a required property for StateConsensusManagerResponse and cannot be null");
            }
            this.CurrentTimeRoundedToMinutes = currentTimeRoundedToMinutes;
            this.CurrentValidatorReadinessSignals = currentValidatorReadinessSignals;
        }

        /// <summary>
        /// Gets or Sets AtLedgerState
        /// </summary>
        [DataMember(Name = "at_ledger_state", IsRequired = true, EmitDefaultValue = true)]
        public LedgerStateSummary AtLedgerState { get; set; }

        /// <summary>
        /// Gets or Sets Config
        /// </summary>
        [DataMember(Name = "config", IsRequired = true, EmitDefaultValue = true)]
        public Substate Config { get; set; }

        /// <summary>
        /// Gets or Sets State
        /// </summary>
        [DataMember(Name = "state", IsRequired = true, EmitDefaultValue = true)]
        public Substate State { get; set; }

        /// <summary>
        /// Gets or Sets CurrentProposalStatistic
        /// </summary>
        [DataMember(Name = "current_proposal_statistic", IsRequired = true, EmitDefaultValue = true)]
        public Substate CurrentProposalStatistic { get; set; }

        /// <summary>
        /// Gets or Sets CurrentValidatorSet
        /// </summary>
        [DataMember(Name = "current_validator_set", IsRequired = true, EmitDefaultValue = true)]
        public Substate CurrentValidatorSet { get; set; }

        /// <summary>
        /// Gets or Sets CurrentTime
        /// </summary>
        [DataMember(Name = "current_time", IsRequired = true, EmitDefaultValue = true)]
        public Substate CurrentTime { get; set; }

        /// <summary>
        /// Gets or Sets CurrentTimeRoundedToMinutes
        /// </summary>
        [DataMember(Name = "current_time_rounded_to_minutes", IsRequired = true, EmitDefaultValue = true)]
        public Substate CurrentTimeRoundedToMinutes { get; set; }

        /// <summary>
        /// Protocol versions signalled by the current validator set. Every validator from &#x60;current_validator_set&#x60; will be referenced by exactly one of the items here. Only returned if enabled by &#x60;include_readiness_signals&#x60; on your request. 
        /// </summary>
        /// <value>Protocol versions signalled by the current validator set. Every validator from &#x60;current_validator_set&#x60; will be referenced by exactly one of the items here. Only returned if enabled by &#x60;include_readiness_signals&#x60; on your request. </value>
        [DataMember(Name = "current_validator_readiness_signals", EmitDefaultValue = true)]
        public List<ProtocolVersionReadiness> CurrentValidatorReadinessSignals { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class StateConsensusManagerResponse {\n");
            sb.Append("  AtLedgerState: ").Append(AtLedgerState).Append("\n");
            sb.Append("  Config: ").Append(Config).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
            sb.Append("  CurrentProposalStatistic: ").Append(CurrentProposalStatistic).Append("\n");
            sb.Append("  CurrentValidatorSet: ").Append(CurrentValidatorSet).Append("\n");
            sb.Append("  CurrentTime: ").Append(CurrentTime).Append("\n");
            sb.Append("  CurrentTimeRoundedToMinutes: ").Append(CurrentTimeRoundedToMinutes).Append("\n");
            sb.Append("  CurrentValidatorReadinessSignals: ").Append(CurrentValidatorReadinessSignals).Append("\n");
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
            return this.Equals(input as StateConsensusManagerResponse);
        }

        /// <summary>
        /// Returns true if StateConsensusManagerResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of StateConsensusManagerResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StateConsensusManagerResponse input)
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
                    this.Config == input.Config ||
                    (this.Config != null &&
                    this.Config.Equals(input.Config))
                ) && 
                (
                    this.State == input.State ||
                    (this.State != null &&
                    this.State.Equals(input.State))
                ) && 
                (
                    this.CurrentProposalStatistic == input.CurrentProposalStatistic ||
                    (this.CurrentProposalStatistic != null &&
                    this.CurrentProposalStatistic.Equals(input.CurrentProposalStatistic))
                ) && 
                (
                    this.CurrentValidatorSet == input.CurrentValidatorSet ||
                    (this.CurrentValidatorSet != null &&
                    this.CurrentValidatorSet.Equals(input.CurrentValidatorSet))
                ) && 
                (
                    this.CurrentTime == input.CurrentTime ||
                    (this.CurrentTime != null &&
                    this.CurrentTime.Equals(input.CurrentTime))
                ) && 
                (
                    this.CurrentTimeRoundedToMinutes == input.CurrentTimeRoundedToMinutes ||
                    (this.CurrentTimeRoundedToMinutes != null &&
                    this.CurrentTimeRoundedToMinutes.Equals(input.CurrentTimeRoundedToMinutes))
                ) && 
                (
                    this.CurrentValidatorReadinessSignals == input.CurrentValidatorReadinessSignals ||
                    this.CurrentValidatorReadinessSignals != null &&
                    input.CurrentValidatorReadinessSignals != null &&
                    this.CurrentValidatorReadinessSignals.SequenceEqual(input.CurrentValidatorReadinessSignals)
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
                if (this.Config != null)
                {
                    hashCode = (hashCode * 59) + this.Config.GetHashCode();
                }
                if (this.State != null)
                {
                    hashCode = (hashCode * 59) + this.State.GetHashCode();
                }
                if (this.CurrentProposalStatistic != null)
                {
                    hashCode = (hashCode * 59) + this.CurrentProposalStatistic.GetHashCode();
                }
                if (this.CurrentValidatorSet != null)
                {
                    hashCode = (hashCode * 59) + this.CurrentValidatorSet.GetHashCode();
                }
                if (this.CurrentTime != null)
                {
                    hashCode = (hashCode * 59) + this.CurrentTime.GetHashCode();
                }
                if (this.CurrentTimeRoundedToMinutes != null)
                {
                    hashCode = (hashCode * 59) + this.CurrentTimeRoundedToMinutes.GetHashCode();
                }
                if (this.CurrentValidatorReadinessSignals != null)
                {
                    hashCode = (hashCode * 59) + this.CurrentValidatorReadinessSignals.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
