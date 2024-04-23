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
    /// BlueprintInterface
    /// </summary>
    // moje start
    // moje end
public partial class BlueprintInterface
{
    /// <summary>
        /// Initializes a new instance of the <see cref="BlueprintInterface" /> class.
        /// </summary>
        /// <param name="events">A map from the event name to the event payload type reference.</param>
        /// <param name="features">features</param>
        /// <param name="functions">A map from the function name to the FunctionSchema</param>
        /// <param name="genericTypeParameters">Generic (SBOR) type parameters which need to be filled by a concrete instance of this blueprint. </param>
        /// <param name="isTransient">If true, an instantiation of this blueprint cannot be persisted. EG buckets and proofs are transient.</param>
        /// <param name="state">state</param>
        /// <param name="types">A map from the registered type name to the concrete type, resolved against a schema from the package&#39;s schema partition. </param>
        /// <param name="outerBlueprint">outerBlueprint</param>
        [JsonConstructor]
    public BlueprintInterface(Dictionary<string, BlueprintPayloadDef> events, List<string> features, Dictionary<string, FunctionSchema> functions, List<GenericTypeParameter> genericTypeParameters, bool isTransient, IndexedStateSchema state, Dictionary<string, ScopedTypeId> types, Option<string?> outerBlueprint = default)
    {
            Events = events;
            Features = features;
            Functions = functions;
            GenericTypeParameters = genericTypeParameters;
            IsTransient = isTransient;
            State = state;
            Types = types;
            OuterBlueprintOption = outerBlueprint;
    OnCreated();
    }

partial void OnCreated();

            /// <summary>
                /// A map from the event name to the event payload type reference.
                /// </summary>
                /// <value>A map from the event name to the event payload type reference.</value>
            [JsonPropertyName("events")]
            public Dictionary<string, BlueprintPayloadDef> Events { get; set; }

            /// <summary>
                /// Gets or Sets Features
                /// </summary>
            [JsonPropertyName("features")]
            public List<string> Features { get; set; }

            /// <summary>
                /// A map from the function name to the FunctionSchema
                /// </summary>
                /// <value>A map from the function name to the FunctionSchema</value>
            [JsonPropertyName("functions")]
            public Dictionary<string, FunctionSchema> Functions { get; set; }

            /// <summary>
                /// Generic (SBOR) type parameters which need to be filled by a concrete instance of this blueprint. 
                /// </summary>
                /// <value>Generic (SBOR) type parameters which need to be filled by a concrete instance of this blueprint. </value>
            [JsonPropertyName("generic_type_parameters")]
            public List<GenericTypeParameter> GenericTypeParameters { get; set; }

            /// <summary>
                /// If true, an instantiation of this blueprint cannot be persisted. EG buckets and proofs are transient.
                /// </summary>
                /// <value>If true, an instantiation of this blueprint cannot be persisted. EG buckets and proofs are transient.</value>
            [JsonPropertyName("is_transient")]
            public bool IsTransient { get; set; }

            /// <summary>
                /// Gets or Sets State
                /// </summary>
            [JsonPropertyName("state")]
            public IndexedStateSchema State { get; set; }

            /// <summary>
                /// A map from the registered type name to the concrete type, resolved against a schema from the package&#39;s schema partition. 
                /// </summary>
                /// <value>A map from the registered type name to the concrete type, resolved against a schema from the package&#39;s schema partition. </value>
            [JsonPropertyName("types")]
            public Dictionary<string, ScopedTypeId> Types { get; set; }

                /// <summary>
                    /// Used to track the state of OuterBlueprint
                    /// </summary>
                [JsonIgnore]
                [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
                public Option<string?> OuterBlueprintOption { get; private set; }

            /// <summary>
                /// Gets or Sets OuterBlueprint
                /// </summary>
            [JsonPropertyName("outer_blueprint")]
            public string? OuterBlueprint { get { return this.OuterBlueprintOption; } set { this.OuterBlueprintOption = new(value); } }

/// <summary>
    /// Returns the string presentation of the object
    /// </summary>
/// <returns>String presentation of the object</returns>
public override string ToString()
{
StringBuilder sb = new StringBuilder();
sb.Append("class BlueprintInterface {\n");
    sb.Append("  Events: ").Append(Events).Append("\n");
    sb.Append("  Features: ").Append(Features).Append("\n");
    sb.Append("  Functions: ").Append(Functions).Append("\n");
    sb.Append("  GenericTypeParameters: ").Append(GenericTypeParameters).Append("\n");
    sb.Append("  IsTransient: ").Append(IsTransient).Append("\n");
    sb.Append("  State: ").Append(State).Append("\n");
    sb.Append("  Types: ").Append(Types).Append("\n");
    sb.Append("  OuterBlueprint: ").Append(OuterBlueprint).Append("\n");
sb.Append("}\n");
return sb.ToString();
}
}


    /// <summary>
    /// A Json converter for type <see cref="BlueprintInterface" />
    /// </summary>
    public class BlueprintInterfaceJsonConverter : JsonConverter<BlueprintInterface>
    {
        /// <summary>
        /// Deserializes json to <see cref="BlueprintInterface" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override BlueprintInterface Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            Option<Dictionary<string, BlueprintPayloadDef>?> events = default;
            Option<List<string>?> features = default;
            Option<Dictionary<string, FunctionSchema>?> functions = default;
            Option<List<GenericTypeParameter>?> genericTypeParameters = default;
            Option<bool?> isTransient = default;
            Option<IndexedStateSchema?> state = default;
            Option<Dictionary<string, ScopedTypeId>?> types = default;
            Option<string?> outerBlueprint = default;

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
                        case "events":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                events = new Option<Dictionary<string, BlueprintPayloadDef>?>(JsonSerializer.Deserialize<Dictionary<string, BlueprintPayloadDef>>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "features":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                features = new Option<List<string>?>(JsonSerializer.Deserialize<List<string>>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "functions":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                functions = new Option<Dictionary<string, FunctionSchema>?>(JsonSerializer.Deserialize<Dictionary<string, FunctionSchema>>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "generic_type_parameters":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                genericTypeParameters = new Option<List<GenericTypeParameter>?>(JsonSerializer.Deserialize<List<GenericTypeParameter>>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "is_transient":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                isTransient = new Option<bool?>(utf8JsonReader.GetBoolean());
                            break;
                        case "state":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                state = new Option<IndexedStateSchema?>(JsonSerializer.Deserialize<IndexedStateSchema>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "types":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                types = new Option<Dictionary<string, ScopedTypeId>?>(JsonSerializer.Deserialize<Dictionary<string, ScopedTypeId>>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "outer_blueprint":
                            outerBlueprint = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!events.IsSet)
                throw new ArgumentException("Property is required for class BlueprintInterface.", nameof(events));

            if (!features.IsSet)
                throw new ArgumentException("Property is required for class BlueprintInterface.", nameof(features));

            if (!functions.IsSet)
                throw new ArgumentException("Property is required for class BlueprintInterface.", nameof(functions));

            if (!genericTypeParameters.IsSet)
                throw new ArgumentException("Property is required for class BlueprintInterface.", nameof(genericTypeParameters));

            if (!isTransient.IsSet)
                throw new ArgumentException("Property is required for class BlueprintInterface.", nameof(isTransient));

            if (!state.IsSet)
                throw new ArgumentException("Property is required for class BlueprintInterface.", nameof(state));

            if (!types.IsSet)
                throw new ArgumentException("Property is required for class BlueprintInterface.", nameof(types));

            if (events.IsSet && events.Value == null)
                throw new ArgumentNullException(nameof(events), "Property is not nullable for class BlueprintInterface.");

            if (features.IsSet && features.Value == null)
                throw new ArgumentNullException(nameof(features), "Property is not nullable for class BlueprintInterface.");

            if (functions.IsSet && functions.Value == null)
                throw new ArgumentNullException(nameof(functions), "Property is not nullable for class BlueprintInterface.");

            if (genericTypeParameters.IsSet && genericTypeParameters.Value == null)
                throw new ArgumentNullException(nameof(genericTypeParameters), "Property is not nullable for class BlueprintInterface.");

            if (isTransient.IsSet && isTransient.Value == null)
                throw new ArgumentNullException(nameof(isTransient), "Property is not nullable for class BlueprintInterface.");

            if (state.IsSet && state.Value == null)
                throw new ArgumentNullException(nameof(state), "Property is not nullable for class BlueprintInterface.");

            if (types.IsSet && types.Value == null)
                throw new ArgumentNullException(nameof(types), "Property is not nullable for class BlueprintInterface.");

            if (outerBlueprint.IsSet && outerBlueprint.Value == null)
                throw new ArgumentNullException(nameof(outerBlueprint), "Property is not nullable for class BlueprintInterface.");

            return new BlueprintInterface(events.Value!, features.Value!, functions.Value!, genericTypeParameters.Value!, isTransient.Value!.Value!, state.Value!, types.Value!, outerBlueprint);
        }

        /// <summary>
        /// Serializes a <see cref="BlueprintInterface" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="blueprintInterface"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, BlueprintInterface blueprintInterface, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(ref writer, blueprintInterface, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="BlueprintInterface" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="blueprintInterface"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(ref Utf8JsonWriter writer, BlueprintInterface blueprintInterface, JsonSerializerOptions jsonSerializerOptions)
        {
            if (blueprintInterface.Events == null)
                throw new ArgumentNullException(nameof(blueprintInterface.Events), "Property is required for class BlueprintInterface.");

            if (blueprintInterface.Features == null)
                throw new ArgumentNullException(nameof(blueprintInterface.Features), "Property is required for class BlueprintInterface.");

            if (blueprintInterface.Functions == null)
                throw new ArgumentNullException(nameof(blueprintInterface.Functions), "Property is required for class BlueprintInterface.");

            if (blueprintInterface.GenericTypeParameters == null)
                throw new ArgumentNullException(nameof(blueprintInterface.GenericTypeParameters), "Property is required for class BlueprintInterface.");

            if (blueprintInterface.State == null)
                throw new ArgumentNullException(nameof(blueprintInterface.State), "Property is required for class BlueprintInterface.");

            if (blueprintInterface.Types == null)
                throw new ArgumentNullException(nameof(blueprintInterface.Types), "Property is required for class BlueprintInterface.");

            if (blueprintInterface.OuterBlueprintOption.IsSet && blueprintInterface.OuterBlueprint == null)
                throw new ArgumentNullException(nameof(blueprintInterface.OuterBlueprint), "Property is required for class BlueprintInterface.");

            writer.WritePropertyName("events");
            JsonSerializer.Serialize(writer, blueprintInterface.Events, jsonSerializerOptions);
            writer.WritePropertyName("features");
            JsonSerializer.Serialize(writer, blueprintInterface.Features, jsonSerializerOptions);
            writer.WritePropertyName("functions");
            JsonSerializer.Serialize(writer, blueprintInterface.Functions, jsonSerializerOptions);
            writer.WritePropertyName("generic_type_parameters");
            JsonSerializer.Serialize(writer, blueprintInterface.GenericTypeParameters, jsonSerializerOptions);
            writer.WriteBoolean("is_transient", blueprintInterface.IsTransient);

            writer.WritePropertyName("state");
            JsonSerializer.Serialize(writer, blueprintInterface.State, jsonSerializerOptions);
            writer.WritePropertyName("types");
            JsonSerializer.Serialize(writer, blueprintInterface.Types, jsonSerializerOptions);
            if (blueprintInterface.OuterBlueprintOption.IsSet)
                writer.WriteString("outer_blueprint", blueprintInterface.OuterBlueprint);
        }
    }
}
