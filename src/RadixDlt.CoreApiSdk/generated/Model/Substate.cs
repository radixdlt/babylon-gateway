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
 * Babylon Core API
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: 0.1.0
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
using JsonSubTypes;
using System.ComponentModel.DataAnnotations;
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;
using System.Reflection;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// Substate
    /// </summary>
    [JsonConverter(typeof(SubstateJsonConverter))]
    [DataContract(Name = "Substate")]
    public partial class Substate : AbstractOpenAPISchema, IEquatable<Substate>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class
        /// with the <see cref="SystemSubstate" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of SystemSubstate.</param>
        public Substate(SystemSubstate actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class
        /// with the <see cref="ResourceManagerSubstate" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of ResourceManagerSubstate.</param>
        public Substate(ResourceManagerSubstate actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class
        /// with the <see cref="ComponentInfoSubstate" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of ComponentInfoSubstate.</param>
        public Substate(ComponentInfoSubstate actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class
        /// with the <see cref="ComponentStateSubstate" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of ComponentStateSubstate.</param>
        public Substate(ComponentStateSubstate actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class
        /// with the <see cref="PackageSubstate" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of PackageSubstate.</param>
        public Substate(PackageSubstate actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class
        /// with the <see cref="VaultSubstate" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of VaultSubstate.</param>
        public Substate(VaultSubstate actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class
        /// with the <see cref="NonFungibleSubstate" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of NonFungibleSubstate.</param>
        public Substate(NonFungibleSubstate actualInstance)
        {
            this.IsNullable = false;
            this.SchemaType= "oneOf";
            this.ActualInstance = actualInstance ?? throw new ArgumentException("Invalid instance found. Must not be null.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class
        /// with the <see cref="KeyValueStoreEntrySubstate" /> class
        /// </summary>
        /// <param name="actualInstance">An instance of KeyValueStoreEntrySubstate.</param>
        public Substate(KeyValueStoreEntrySubstate actualInstance)
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
                if (value.GetType() == typeof(ComponentInfoSubstate))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(ComponentStateSubstate))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(KeyValueStoreEntrySubstate))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(NonFungibleSubstate))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(PackageSubstate))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(ResourceManagerSubstate))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(SystemSubstate))
                {
                    this._actualInstance = value;
                }
                else if (value.GetType() == typeof(VaultSubstate))
                {
                    this._actualInstance = value;
                }
                else
                {
                    throw new ArgumentException("Invalid instance found. Must be the following types: ComponentInfoSubstate, ComponentStateSubstate, KeyValueStoreEntrySubstate, NonFungibleSubstate, PackageSubstate, ResourceManagerSubstate, SystemSubstate, VaultSubstate");
                }
            }
        }

        /// <summary>
        /// Get the actual instance of `SystemSubstate`. If the actual instance is not `SystemSubstate`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of SystemSubstate</returns>
        public SystemSubstate GetSystemSubstate()
        {
            return (SystemSubstate)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `ResourceManagerSubstate`. If the actual instance is not `ResourceManagerSubstate`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of ResourceManagerSubstate</returns>
        public ResourceManagerSubstate GetResourceManagerSubstate()
        {
            return (ResourceManagerSubstate)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `ComponentInfoSubstate`. If the actual instance is not `ComponentInfoSubstate`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of ComponentInfoSubstate</returns>
        public ComponentInfoSubstate GetComponentInfoSubstate()
        {
            return (ComponentInfoSubstate)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `ComponentStateSubstate`. If the actual instance is not `ComponentStateSubstate`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of ComponentStateSubstate</returns>
        public ComponentStateSubstate GetComponentStateSubstate()
        {
            return (ComponentStateSubstate)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `PackageSubstate`. If the actual instance is not `PackageSubstate`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of PackageSubstate</returns>
        public PackageSubstate GetPackageSubstate()
        {
            return (PackageSubstate)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `VaultSubstate`. If the actual instance is not `VaultSubstate`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of VaultSubstate</returns>
        public VaultSubstate GetVaultSubstate()
        {
            return (VaultSubstate)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `NonFungibleSubstate`. If the actual instance is not `NonFungibleSubstate`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of NonFungibleSubstate</returns>
        public NonFungibleSubstate GetNonFungibleSubstate()
        {
            return (NonFungibleSubstate)this.ActualInstance;
        }

        /// <summary>
        /// Get the actual instance of `KeyValueStoreEntrySubstate`. If the actual instance is not `KeyValueStoreEntrySubstate`,
        /// the InvalidClassException will be thrown
        /// </summary>
        /// <returns>An instance of KeyValueStoreEntrySubstate</returns>
        public KeyValueStoreEntrySubstate GetKeyValueStoreEntrySubstate()
        {
            return (KeyValueStoreEntrySubstate)this.ActualInstance;
        }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Substate {\n");
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
            return JsonConvert.SerializeObject(this.ActualInstance, Substate.SerializerSettings);
        }

        /// <summary>
        /// Converts the JSON string into an instance of Substate
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <returns>An instance of Substate</returns>
        public static Substate FromJson(string jsonString)
        {
            Substate newSubstate = null;

            if (string.IsNullOrEmpty(jsonString))
            {
                return newSubstate;
            }
            int match = 0;
            List<string> matchedTypes = new List<string>();

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(ComponentInfoSubstate).GetProperty("AdditionalProperties") == null)
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<ComponentInfoSubstate>(jsonString, Substate.SerializerSettings));
                }
                else
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<ComponentInfoSubstate>(jsonString, Substate.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("ComponentInfoSubstate");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into ComponentInfoSubstate: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(ComponentStateSubstate).GetProperty("AdditionalProperties") == null)
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<ComponentStateSubstate>(jsonString, Substate.SerializerSettings));
                }
                else
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<ComponentStateSubstate>(jsonString, Substate.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("ComponentStateSubstate");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into ComponentStateSubstate: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(KeyValueStoreEntrySubstate).GetProperty("AdditionalProperties") == null)
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<KeyValueStoreEntrySubstate>(jsonString, Substate.SerializerSettings));
                }
                else
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<KeyValueStoreEntrySubstate>(jsonString, Substate.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("KeyValueStoreEntrySubstate");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into KeyValueStoreEntrySubstate: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(NonFungibleSubstate).GetProperty("AdditionalProperties") == null)
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<NonFungibleSubstate>(jsonString, Substate.SerializerSettings));
                }
                else
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<NonFungibleSubstate>(jsonString, Substate.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("NonFungibleSubstate");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into NonFungibleSubstate: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(PackageSubstate).GetProperty("AdditionalProperties") == null)
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<PackageSubstate>(jsonString, Substate.SerializerSettings));
                }
                else
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<PackageSubstate>(jsonString, Substate.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("PackageSubstate");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into PackageSubstate: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(ResourceManagerSubstate).GetProperty("AdditionalProperties") == null)
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<ResourceManagerSubstate>(jsonString, Substate.SerializerSettings));
                }
                else
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<ResourceManagerSubstate>(jsonString, Substate.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("ResourceManagerSubstate");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into ResourceManagerSubstate: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(SystemSubstate).GetProperty("AdditionalProperties") == null)
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<SystemSubstate>(jsonString, Substate.SerializerSettings));
                }
                else
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<SystemSubstate>(jsonString, Substate.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("SystemSubstate");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into SystemSubstate: {1}", jsonString, exception.ToString()));
            }

            try
            {
                // if it does not contains "AdditionalProperties", use SerializerSettings to deserialize
                if (typeof(VaultSubstate).GetProperty("AdditionalProperties") == null)
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<VaultSubstate>(jsonString, Substate.SerializerSettings));
                }
                else
                {
                    newSubstate = new Substate(JsonConvert.DeserializeObject<VaultSubstate>(jsonString, Substate.AdditionalPropertiesSerializerSettings));
                }
                matchedTypes.Add("VaultSubstate");
                match++;
            }
            catch (Exception exception)
            {
                // deserialization failed, try the next one
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to deserialize `{0}` into VaultSubstate: {1}", jsonString, exception.ToString()));
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
            return newSubstate;
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as Substate);
        }

        /// <summary>
        /// Returns true if Substate instances are equal
        /// </summary>
        /// <param name="input">Instance of Substate to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Substate input)
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

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

    /// <summary>
    /// Custom JSON converter for Substate
    /// </summary>
    public class SubstateJsonConverter : JsonConverter
    {
        /// <summary>
        /// To write the JSON string
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="value">Object to be converted into a JSON string</param>
        /// <param name="serializer">JSON Serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((string)(typeof(Substate).GetMethod("ToJson").Invoke(value, null)));
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
                return Substate.FromJson(JObject.Load(reader).ToString(Formatting.None));
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
