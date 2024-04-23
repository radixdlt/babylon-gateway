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

// <auto-generated>
/*
 * Radix Core API - Babylon
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.0.4
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using RadixDlt.CoreApiSdk.GenericHost.Client;

namespace RadixDlt.CoreApiSdk.GenericHost.Model
{

    /// <summary>
    /// StateConsensusManagerResponse
    /// </summary>
    // moje start
    // moje end
public partial class StateConsensusManagerResponse
{
    /// <summary>
        /// Initializes a new instance of the <see cref="StateConsensusManagerResponse" /> class.
        /// </summary>
        /// <param name="atLedgerState">atLedgerState</param>
        /// <param name="config">config</param>
        /// <param name="currentProposalStatistic">currentProposalStatistic</param>
        /// <param name="currentTime">currentTime</param>
        /// <param name="currentTimeRoundedToMinutes">currentTimeRoundedToMinutes</param>
        /// <param name="currentValidatorSet">currentValidatorSet</param>
        /// <param name="state">state</param>
        [JsonConstructor]
    public StateConsensusManagerResponse(LedgerStateSummary atLedgerState, Substate config, Substate currentProposalStatistic, Substate currentTime, Substate currentTimeRoundedToMinutes, Substate currentValidatorSet, Substate state)
    {
            AtLedgerState = atLedgerState;
            Config = config;
            CurrentProposalStatistic = currentProposalStatistic;
            CurrentTime = currentTime;
            CurrentTimeRoundedToMinutes = currentTimeRoundedToMinutes;
            CurrentValidatorSet = currentValidatorSet;
            State = state;
    OnCreated();
    }

partial void OnCreated();

            /// <summary>
                /// Gets or Sets AtLedgerState
                /// </summary>
            [JsonPropertyName("at_ledger_state")]
            public LedgerStateSummary AtLedgerState { get; set; }

            /// <summary>
                /// Gets or Sets Config
                /// </summary>
            [JsonPropertyName("config")]
            public Substate Config { get; set; }

            /// <summary>
                /// Gets or Sets CurrentProposalStatistic
                /// </summary>
            [JsonPropertyName("current_proposal_statistic")]
            public Substate CurrentProposalStatistic { get; set; }

            /// <summary>
                /// Gets or Sets CurrentTime
                /// </summary>
            [JsonPropertyName("current_time")]
            public Substate CurrentTime { get; set; }

            /// <summary>
                /// Gets or Sets CurrentTimeRoundedToMinutes
                /// </summary>
            [JsonPropertyName("current_time_rounded_to_minutes")]
            public Substate CurrentTimeRoundedToMinutes { get; set; }

            /// <summary>
                /// Gets or Sets CurrentValidatorSet
                /// </summary>
            [JsonPropertyName("current_validator_set")]
            public Substate CurrentValidatorSet { get; set; }

            /// <summary>
                /// Gets or Sets State
                /// </summary>
            [JsonPropertyName("state")]
            public Substate State { get; set; }

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
    sb.Append("  CurrentProposalStatistic: ").Append(CurrentProposalStatistic).Append("\n");
    sb.Append("  CurrentTime: ").Append(CurrentTime).Append("\n");
    sb.Append("  CurrentTimeRoundedToMinutes: ").Append(CurrentTimeRoundedToMinutes).Append("\n");
    sb.Append("  CurrentValidatorSet: ").Append(CurrentValidatorSet).Append("\n");
    sb.Append("  State: ").Append(State).Append("\n");
sb.Append("}\n");
return sb.ToString();
}
}


    /// <summary>
    /// A Json converter for type <see cref="StateConsensusManagerResponse" />
    /// </summary>
    public class StateConsensusManagerResponseJsonConverter : JsonConverter<StateConsensusManagerResponse>
    {
        /// <summary>
        /// Deserializes json to <see cref="StateConsensusManagerResponse" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override StateConsensusManagerResponse Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            Option<LedgerStateSummary?> atLedgerState = default;
            Option<Substate?> config = default;
            Option<Substate?> currentProposalStatistic = default;
            Option<Substate?> currentTime = default;
            Option<Substate?> currentTimeRoundedToMinutes = default;
            Option<Substate?> currentValidatorSet = default;
            Option<Substate?> state = default;

            while (utf8JsonReader.Read())
            {
                if (startingTokenType == JsonTokenType.StartObject && utf8JsonReader.TokenType == JsonTokenType.EndObject && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (startingTokenType == JsonTokenType.StartArray && utf8JsonReader.TokenType == JsonTokenType.EndArray && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (utf8JsonReader.TokenType == JsonTokenType.PropertyName && currentDepth == utf8JsonReader.CurrentDepth - 1)
                {
                    string? localVarJsonPropertyName = utf8JsonReader.GetString();
                    utf8JsonReader.Read();

                    switch (localVarJsonPropertyName)
                    {
                        case "at_ledger_state":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                atLedgerState = new Option<LedgerStateSummary?>(JsonSerializer.Deserialize<LedgerStateSummary>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "config":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                config = new Option<Substate?>(JsonSerializer.Deserialize<Substate>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "current_proposal_statistic":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                currentProposalStatistic = new Option<Substate?>(JsonSerializer.Deserialize<Substate>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "current_time":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                currentTime = new Option<Substate?>(JsonSerializer.Deserialize<Substate>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "current_time_rounded_to_minutes":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                currentTimeRoundedToMinutes = new Option<Substate?>(JsonSerializer.Deserialize<Substate>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "current_validator_set":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                currentValidatorSet = new Option<Substate?>(JsonSerializer.Deserialize<Substate>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "state":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                state = new Option<Substate?>(JsonSerializer.Deserialize<Substate>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!atLedgerState.IsSet)
                throw new ArgumentException("Property is required for class StateConsensusManagerResponse.", nameof(atLedgerState));

            if (!config.IsSet)
                throw new ArgumentException("Property is required for class StateConsensusManagerResponse.", nameof(config));

            if (!currentProposalStatistic.IsSet)
                throw new ArgumentException("Property is required for class StateConsensusManagerResponse.", nameof(currentProposalStatistic));

            if (!currentTime.IsSet)
                throw new ArgumentException("Property is required for class StateConsensusManagerResponse.", nameof(currentTime));

            if (!currentTimeRoundedToMinutes.IsSet)
                throw new ArgumentException("Property is required for class StateConsensusManagerResponse.", nameof(currentTimeRoundedToMinutes));

            if (!currentValidatorSet.IsSet)
                throw new ArgumentException("Property is required for class StateConsensusManagerResponse.", nameof(currentValidatorSet));

            if (!state.IsSet)
                throw new ArgumentException("Property is required for class StateConsensusManagerResponse.", nameof(state));

            if (atLedgerState.IsSet && atLedgerState.Value == null)
                throw new ArgumentNullException(nameof(atLedgerState), "Property is not nullable for class StateConsensusManagerResponse.");

            if (config.IsSet && config.Value == null)
                throw new ArgumentNullException(nameof(config), "Property is not nullable for class StateConsensusManagerResponse.");

            if (currentProposalStatistic.IsSet && currentProposalStatistic.Value == null)
                throw new ArgumentNullException(nameof(currentProposalStatistic), "Property is not nullable for class StateConsensusManagerResponse.");

            if (currentTime.IsSet && currentTime.Value == null)
                throw new ArgumentNullException(nameof(currentTime), "Property is not nullable for class StateConsensusManagerResponse.");

            if (currentTimeRoundedToMinutes.IsSet && currentTimeRoundedToMinutes.Value == null)
                throw new ArgumentNullException(nameof(currentTimeRoundedToMinutes), "Property is not nullable for class StateConsensusManagerResponse.");

            if (currentValidatorSet.IsSet && currentValidatorSet.Value == null)
                throw new ArgumentNullException(nameof(currentValidatorSet), "Property is not nullable for class StateConsensusManagerResponse.");

            if (state.IsSet && state.Value == null)
                throw new ArgumentNullException(nameof(state), "Property is not nullable for class StateConsensusManagerResponse.");

            return new StateConsensusManagerResponse(atLedgerState.Value!, config.Value!, currentProposalStatistic.Value!, currentTime.Value!, currentTimeRoundedToMinutes.Value!, currentValidatorSet.Value!, state.Value!);
        }

        /// <summary>
        /// Serializes a <see cref="StateConsensusManagerResponse" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="stateConsensusManagerResponse"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, StateConsensusManagerResponse stateConsensusManagerResponse, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(ref writer, stateConsensusManagerResponse, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="StateConsensusManagerResponse" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="stateConsensusManagerResponse"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(ref Utf8JsonWriter writer, StateConsensusManagerResponse stateConsensusManagerResponse, JsonSerializerOptions jsonSerializerOptions)
        {
            if (stateConsensusManagerResponse.AtLedgerState == null)
                throw new ArgumentNullException(nameof(stateConsensusManagerResponse.AtLedgerState), "Property is required for class StateConsensusManagerResponse.");

            if (stateConsensusManagerResponse.Config == null)
                throw new ArgumentNullException(nameof(stateConsensusManagerResponse.Config), "Property is required for class StateConsensusManagerResponse.");

            if (stateConsensusManagerResponse.CurrentProposalStatistic == null)
                throw new ArgumentNullException(nameof(stateConsensusManagerResponse.CurrentProposalStatistic), "Property is required for class StateConsensusManagerResponse.");

            if (stateConsensusManagerResponse.CurrentTime == null)
                throw new ArgumentNullException(nameof(stateConsensusManagerResponse.CurrentTime), "Property is required for class StateConsensusManagerResponse.");

            if (stateConsensusManagerResponse.CurrentTimeRoundedToMinutes == null)
                throw new ArgumentNullException(nameof(stateConsensusManagerResponse.CurrentTimeRoundedToMinutes), "Property is required for class StateConsensusManagerResponse.");

            if (stateConsensusManagerResponse.CurrentValidatorSet == null)
                throw new ArgumentNullException(nameof(stateConsensusManagerResponse.CurrentValidatorSet), "Property is required for class StateConsensusManagerResponse.");

            if (stateConsensusManagerResponse.State == null)
                throw new ArgumentNullException(nameof(stateConsensusManagerResponse.State), "Property is required for class StateConsensusManagerResponse.");

            writer.WritePropertyName("at_ledger_state");
            JsonSerializer.Serialize(writer, stateConsensusManagerResponse.AtLedgerState, jsonSerializerOptions);
            writer.WritePropertyName("config");
            JsonSerializer.Serialize(writer, stateConsensusManagerResponse.Config, jsonSerializerOptions);
            writer.WritePropertyName("current_proposal_statistic");
            JsonSerializer.Serialize(writer, stateConsensusManagerResponse.CurrentProposalStatistic, jsonSerializerOptions);
            writer.WritePropertyName("current_time");
            JsonSerializer.Serialize(writer, stateConsensusManagerResponse.CurrentTime, jsonSerializerOptions);
            writer.WritePropertyName("current_time_rounded_to_minutes");
            JsonSerializer.Serialize(writer, stateConsensusManagerResponse.CurrentTimeRoundedToMinutes, jsonSerializerOptions);
            writer.WritePropertyName("current_validator_set");
            JsonSerializer.Serialize(writer, stateConsensusManagerResponse.CurrentValidatorSet, jsonSerializerOptions);
            writer.WritePropertyName("state");
            JsonSerializer.Serialize(writer, stateConsensusManagerResponse.State, jsonSerializerOptions);
        }
    }
}
