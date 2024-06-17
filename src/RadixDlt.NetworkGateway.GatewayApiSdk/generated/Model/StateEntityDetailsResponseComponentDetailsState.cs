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
 * Radix Gateway API - Babylon
 *
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers, and for light queries from front-end dApps. For exchange/asset integrations, back-end dApp integrations, or simple use cases, you should consider using the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  All responses may have additional fields added at any release, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects.  When the Radix protocol is updated, new functionality may be added, and so discriminated unions returned by the API may need to be updated to have new variants added, corresponding to the updated data. Clients may need to update in advance to be able to handle these new variants when a protocol update comes out.  On the very rare occasions we need to make breaking changes to the API, these will be warned in advance with deprecation notices on previous versions. These deprecation notices will include a safe migration path. Deprecation notes or breaking changes will be flagged clearly in release notes for new versions of the Gateway.  The Gateway DB schema is not subject to any compatibility guarantees, and may be changed at any release. DB changes will be flagged in the release notes so clients doing custom DB integrations can prepare. 
 *
 * The version of the OpenAPI document: v1.6.1
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
using FileParameter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.NetworkGateway.GatewayApiSdk.Client.OpenAPIDateConverter;
using System.Reflection;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// StateEntityDetailsResponseComponentDetailsState
    /// </summary>
    [JsonConverter(typeof(StateEntityDetailsResponseComponentDetailsStateJsonConverter))]
    [DataContract(Name = "StateEntityDetailsResponseComponentDetailsState")]
    public partial class StateEntityDetailsResponseComponentDetailsState : AbstractOpenAPISchema, IEquatable<StateEntityDetailsResponseComponentDetailsState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseComponentDetailsState" /> class
        /// with the <see cref="CoreApiGenericScryptoComponentFieldStateValue" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of CoreApiGenericScryptoComponentFieldStateValue.</param>
        public StateEntityDetailsResponseComponentDetailsState(CoreApiGenericScryptoComponentFieldStateValue actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseComponentDetailsState" /> class
        /// with the <see cref="CoreApiAccountFieldStateValue" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of CoreApiAccountFieldStateValue.</param>
        public StateEntityDetailsResponseComponentDetailsState(CoreApiAccountFieldStateValue actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseComponentDetailsState" /> class
        /// with the <see cref="CoreApiValidatorFieldStateValue" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of CoreApiValidatorFieldStateValue.</param>
        public StateEntityDetailsResponseComponentDetailsState(CoreApiValidatorFieldStateValue actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseComponentDetailsState" /> class
        /// with the <see cref="CoreApiAccessControllerFieldStateValue" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of CoreApiAccessControllerFieldStateValue.</param>
        public StateEntityDetailsResponseComponentDetailsState(CoreApiAccessControllerFieldStateValue actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseComponentDetailsState" /> class
        /// with the <see cref="CoreApiOneResourcePoolFieldStateValue" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of CoreApiOneResourcePoolFieldStateValue.</param>
        public StateEntityDetailsResponseComponentDetailsState(CoreApiOneResourcePoolFieldStateValue actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseComponentDetailsState" /> class
        /// with the <see cref="CoreApiTwoResourcePoolFieldStateValue" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of CoreApiTwoResourcePoolFieldStateValue.</param>
        public StateEntityDetailsResponseComponentDetailsState(CoreApiTwoResourcePoolFieldStateValue actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateEntityDetailsResponseComponentDetailsState" /> class
        /// with the <see cref="CoreApiMultiResourcePoolFieldStateValue" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of CoreApiMultiResourcePoolFieldStateValue.</param>
        public StateEntityDetailsResponseComponentDetailsState(CoreApiMultiResourcePoolFieldStateValue actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }


        private Object _actualInstance;

        /// <summary>
        /// Gets or Sets ActualInstance
        /// </summary>
        public override Object ActualInstance
        {
            get
            {
                return _actualInstance;
            }
            set
            {
                if (value.GetType() == typeof(CoreApiAccessControllerFieldStateValue))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(CoreApiAccountFieldStateValue))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(CoreApiGenericScryptoComponentFieldStateValue))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(CoreApiMultiResourcePoolFieldStateValue))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(CoreApiOneResourcePoolFieldStateValue))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(CoreApiTwoResourcePoolFieldStateValue))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(CoreApiValidatorFieldStateValue))
                {
                    this._actualInstance = value;
                }
                else
                {
                    throw new ArgumentException("Invalid instance found. Must be the following types: CoreApiAccessControllerFieldStateValue, CoreApiAccountFieldStateValue, CoreApiGenericScryptoComponentFieldStateValue, CoreApiMultiResourcePoolFieldStateValue, CoreApiOneResourcePoolFieldStateValue, CoreApiTwoResourcePoolFieldStateValue, CoreApiValidatorFieldStateValue");
                }
            }
        }

        /// <summary>
        /// Get the actual instance of `CoreApiGenericScryptoComponentFieldStateValue`. If the actual instance is not `CoreApiGenericScryptoComponentFieldStateValue`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of CoreApiGenericScryptoComponentFieldStateValue</returns>
        public CoreApiGenericScryptoComponentFieldStateValue GetCoreApiGenericScryptoComponentFieldStateValue()
        {
            return (CoreApiGenericScryptoComponentFieldStateValue)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `CoreApiAccountFieldStateValue`. If the actual instance is not `CoreApiAccountFieldStateValue`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of CoreApiAccountFieldStateValue</returns>
        public CoreApiAccountFieldStateValue GetCoreApiAccountFieldStateValue()
        {
            return (CoreApiAccountFieldStateValue)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `CoreApiValidatorFieldStateValue`. If the actual instance is not `CoreApiValidatorFieldStateValue`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of CoreApiValidatorFieldStateValue</returns>
        public CoreApiValidatorFieldStateValue GetCoreApiValidatorFieldStateValue()
        {
            return (CoreApiValidatorFieldStateValue)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `CoreApiAccessControllerFieldStateValue`. If the actual instance is not `CoreApiAccessControllerFieldStateValue`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of CoreApiAccessControllerFieldStateValue</returns>
        public CoreApiAccessControllerFieldStateValue GetCoreApiAccessControllerFieldStateValue()
        {
            return (CoreApiAccessControllerFieldStateValue)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `CoreApiOneResourcePoolFieldStateValue`. If the actual instance is not `CoreApiOneResourcePoolFieldStateValue`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of CoreApiOneResourcePoolFieldStateValue</returns>
        public CoreApiOneResourcePoolFieldStateValue GetCoreApiOneResourcePoolFieldStateValue()
        {
            return (CoreApiOneResourcePoolFieldStateValue)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `CoreApiTwoResourcePoolFieldStateValue`. If the actual instance is not `CoreApiTwoResourcePoolFieldStateValue`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of CoreApiTwoResourcePoolFieldStateValue</returns>
        public CoreApiTwoResourcePoolFieldStateValue GetCoreApiTwoResourcePoolFieldStateValue()
        {
            return (CoreApiTwoResourcePoolFieldStateValue)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `CoreApiMultiResourcePoolFieldStateValue`. If the actual instance is not `CoreApiMultiResourcePoolFieldStateValue`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of CoreApiMultiResourcePoolFieldStateValue</returns>
        public CoreApiMultiResourcePoolFieldStateValue GetCoreApiMultiResourcePoolFieldStateValue()
        {
            return (CoreApiMultiResourcePoolFieldStateValue)this.ActualInstance;
        }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class StateEntityDetailsResponseComponentDetailsState {\n");
            sb.Append("  ActualInstance: ").Append(this.ActualInstance).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public override string ToJson()
        {
            return JsonConvert.SerializeObject(this.ActualInstance, StateEntityDetailsResponseComponentDetailsState.SerializerSettings);
        }

        /// <summary>
        /// Converts the JSON string into an instance of StateEntityDetailsResponseComponentDetailsState
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <returns>An instance of StateEntityDetailsResponseComponentDetailsState</returns>
        public static StateEntityDetailsResponseComponentDetailsState FromJson(string jsonString)
        {
            StateEntityDetailsResponseComponentDetailsState newStateEntityDetailsResponseComponentDetailsState = null;

            if (string.IsNullOrEmpty(jsonString))
            {
                return newStateEntityDetailsResponseComponentDetailsState;
            }
            int match = 0;
            List<string> matchedTypes = new List<string>();

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(CoreApiAccessControllerFieldStateValue).GetProperty("AdditionalProperties") == null)
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiAccessControllerFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.SerializerSettings));
                }
                else
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiAccessControllerFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("CoreApiAccessControllerFieldStateValue");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into CoreApiAccessControllerFieldStateValue: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(CoreApiAccountFieldStateValue).GetProperty("AdditionalProperties") == null)
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiAccountFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.SerializerSettings));
                }
                else
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiAccountFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("CoreApiAccountFieldStateValue");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into CoreApiAccountFieldStateValue: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(CoreApiGenericScryptoComponentFieldStateValue).GetProperty("AdditionalProperties") == null)
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiGenericScryptoComponentFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.SerializerSettings));
                }
                else
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiGenericScryptoComponentFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("CoreApiGenericScryptoComponentFieldStateValue");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into CoreApiGenericScryptoComponentFieldStateValue: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(CoreApiMultiResourcePoolFieldStateValue).GetProperty("AdditionalProperties") == null)
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiMultiResourcePoolFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.SerializerSettings));
                }
                else
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiMultiResourcePoolFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("CoreApiMultiResourcePoolFieldStateValue");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into CoreApiMultiResourcePoolFieldStateValue: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(CoreApiOneResourcePoolFieldStateValue).GetProperty("AdditionalProperties") == null)
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiOneResourcePoolFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.SerializerSettings));
                }
                else
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiOneResourcePoolFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("CoreApiOneResourcePoolFieldStateValue");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into CoreApiOneResourcePoolFieldStateValue: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(CoreApiTwoResourcePoolFieldStateValue).GetProperty("AdditionalProperties") == null)
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiTwoResourcePoolFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.SerializerSettings));
                }
                else
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiTwoResourcePoolFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("CoreApiTwoResourcePoolFieldStateValue");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into CoreApiTwoResourcePoolFieldStateValue: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(CoreApiValidatorFieldStateValue).GetProperty("AdditionalProperties") == null)
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiValidatorFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.SerializerSettings));
                }
                else
                {
                    newStateEntityDetailsResponseComponentDetailsState = new StateEntityDetailsResponseComponentDetailsState(JsonConvert.DeserializeObject<CoreApiValidatorFieldStateValue>(jsonString, StateEntityDetailsResponseComponentDetailsState.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("CoreApiValidatorFieldStateValue");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into CoreApiValidatorFieldStateValue: {1}", jsonString, exception.ToString()));
            }

            if (match == 0)
            {
                throw new InvalidDataException("The JSON string `" + jsonString + "` cannot be deserialized into any schema defined.");
            }
            else if (match > 1)
            {
                throw new InvalidDataException("The JSON string `" + jsonString + "` incorrectly matches more than one schema (should be exactly one match): " + matchedTypes);
            }

            // deserialization is considered successful at this point if no exception has been thrown.
            return newStateEntityDetailsResponseComponentDetailsState;
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as StateEntityDetailsResponseComponentDetailsState);
        }

        /// <summary>
        /// Returns true if StateEntityDetailsResponseComponentDetailsState instances are equal
        /// </summary>
        /// <param name="input">Instance of StateEntityDetailsResponseComponentDetailsState to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StateEntityDetailsResponseComponentDetailsState input)
        {
            if (input == null)
                return false;

            return this.ActualInstance.Equals(input.ActualInstance);
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
                if (this.ActualInstance != null)
                    hashCode = hashCode * 59 + this.ActualInstance.GetHashCode();
                return hashCode;
            }
        }

    }

    /// <summary>
    /// Custom JSON converter for StateEntityDetailsResponseComponentDetailsState
    /// </summary>
    public class StateEntityDetailsResponseComponentDetailsStateJsonConverter : JsonConverter
    {
        /// <summary>
        /// To write the JSON string
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="value">Object to be converted into a JSON string</param>
        /// <param name="serializer">JSON Serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((string)(typeof(StateEntityDetailsResponseComponentDetailsState).GetMethod("ToJson").Invoke(value, null)));
        }

        /// <summary>
        /// To convert a JSON string into an object
        /// </summary>
        /// <param name="reader">JSON reader</param>
        /// <param name="objectType">Object type</param>
        /// <param name="existingValue">Existing value</param>
        /// <param name="serializer">JSON Serializer</param>
        /// <returns>The object converted from the JSON string</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType != JsonToken.Null)
            {
                return StateEntityDetailsResponseComponentDetailsState.FromJson(JObject.Load(reader).ToString(Formatting.None));
            }
            return null;
        }

        /// <summary>
        /// Check if the object can be converted
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <returns>True if the object can be converted</returns>
        public override bool CanConvert(Type objectType)
        {
            return false;
        }
    }

}
