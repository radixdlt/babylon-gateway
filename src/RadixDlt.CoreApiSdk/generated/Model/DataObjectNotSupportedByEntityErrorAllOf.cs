/*
 * Radix Core API
 *
 * This API provides endpoints from a node for integration with the Radix ledger.  # Overview  > WARNING > > The Core API is __NOT__ intended to be available on the public web. It is > designed to be accessed in a private network.  The Core API is separated into three: * The **Data API** is a read-only api which allows you to view and sync to the state of the ledger. * The **Construction API** allows you to construct and submit a transaction to the network. * The **Key API** allows you to use the keys managed by the node to sign transactions.  The Core API is a low level API primarily designed for network integrations such as exchanges, ledger analytics providers, or hosted ledger data dashboards where detailed ledger data is required and the integrator can be expected to run their node to provide the Core API for their own consumption.  For a higher level API, see the [Gateway API](https://redocly.github.io/redoc/?url=https://raw.githubusercontent.com/radixdlt/radixdlt-network-gateway/main/generation/gateway-api-spec.yaml).  For node monitoring, see the [System API](https://redocly.github.io/redoc/?url=https://raw.githubusercontent.com/radixdlt/radixdlt/main/radixdlt-core/radixdlt/src/main/java/com/radixdlt/api/system/api.yaml).  ## Rosetta  The Data API and Construction API is inspired from [Rosetta API](https://www.rosetta-api.org/) most notably:   * Use of a JSON-Based RPC protocol on top of HTTP Post requests   * Use of Operations, Amounts, and Identifiers as universal language to   express asset movement for reading and writing  There are a few notable exceptions to note:   * Fetching of ledger data is through a Transaction stream rather than a   Block stream   * Use of `EntityIdentifier` rather than `AccountIdentifier`   * Use of `OperationGroup` rather than `related_operations` to express related   operations   * Construction endpoints perform coin selection on behalf of the caller.   This has the unfortunate effect of not being able to support high frequency   transactions from a single account. This will be addressed in future updates.   * Construction endpoints are online rather than offline as required by Rosetta  Future versions of the api will aim towards a fully-compliant Rosetta API.  ## Enabling Endpoints  All endpoints are enabled when running a node with the exception of two endpoints, each of which need to be manually configured to access: * `/transactions` endpoint must be enabled with configuration `api.transaction.enable=true`. This is because the transactions endpoint requires additional database storage which may not be needed for users who aren't using this endpoint * `/key/sign` endpoint must be enable with configuration `api.sign.enable=true`. This is a potentially dangerous endpoint if accessible publicly so it must be enabled manually.  ## Client Code Generation  We have found success with generating clients against the [api.yaml specification](https://raw.githubusercontent.com/radixdlt/radixdlt/main/radixdlt-core/radixdlt/src/main/java/com/radixdlt/api/core/api.yaml). See https://openapi-generator.tech/ for more details.  The OpenAPI generator only supports openapi version 3.0.0 at present, but you can change 3.1.0 to 3.0.0 in the first line of the spec without affecting generation.  # Data API Flow  The Data API can be used to synchronize a full or partial view of the ledger, transaction by transaction.  ![Data API Flow](https://raw.githubusercontent.com/radixdlt/radixdlt/feature/update-documentation/radixdlt-core/radixdlt/src/main/java/com/radixdlt/api/core/documentation/data_sequence_flow.png)  # Construction API Flow  The Construction API can be used to construct and submit transactions to the network.  ![Construction API Flow](https://raw.githubusercontent.com/radixdlt/radixdlt/feature/open-api/radixdlt-core/radixdlt/src/main/java/com/radixdlt/api/core/documentation/construction_sequence_flow.png)  Unlike the Rosetta Construction API [specification](https://www.rosetta-api.org/docs/construction_api_introduction.html), this Construction API selects UTXOs on behalf of the caller. This has the unfortunate side effect of not being able to support high frequency transactions from a single account due to UTXO conflicts. This will be addressed in a future release. 
 *
 * The version of the OpenAPI document: 1.0.0
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
using System.ComponentModel.DataAnnotations;
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// DataObjectNotSupportedByEntityErrorAllOf
    /// </summary>
    [DataContract(Name = "DataObjectNotSupportedByEntityError_allOf")]
    public partial class DataObjectNotSupportedByEntityErrorAllOf : IEquatable<DataObjectNotSupportedByEntityErrorAllOf>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectNotSupportedByEntityErrorAllOf" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected DataObjectNotSupportedByEntityErrorAllOf() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DataObjectNotSupportedByEntityErrorAllOf" /> class.
        /// </summary>
        /// <param name="entityIdentifier">entityIdentifier (required).</param>
        /// <param name="dataObjectNotSupported">dataObjectNotSupported (required).</param>
        public DataObjectNotSupportedByEntityErrorAllOf(EntityIdentifier entityIdentifier = default(EntityIdentifier), DataObject dataObjectNotSupported = default(DataObject))
        {
            // to ensure "entityIdentifier" is required (not null)
            if (entityIdentifier == null)
            {
                throw new ArgumentNullException("entityIdentifier is a required property for DataObjectNotSupportedByEntityErrorAllOf and cannot be null");
            }
            this.EntityIdentifier = entityIdentifier;
            // to ensure "dataObjectNotSupported" is required (not null)
            if (dataObjectNotSupported == null)
            {
                throw new ArgumentNullException("dataObjectNotSupported is a required property for DataObjectNotSupportedByEntityErrorAllOf and cannot be null");
            }
            this.DataObjectNotSupported = dataObjectNotSupported;
        }

        /// <summary>
        /// Gets or Sets EntityIdentifier
        /// </summary>
        [DataMember(Name = "entity_identifier", IsRequired = true, EmitDefaultValue = true)]
        public EntityIdentifier EntityIdentifier { get; set; }

        /// <summary>
        /// Gets or Sets DataObjectNotSupported
        /// </summary>
        [DataMember(Name = "data_object_not_supported", IsRequired = true, EmitDefaultValue = true)]
        public DataObject DataObjectNotSupported { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class DataObjectNotSupportedByEntityErrorAllOf {\n");
            sb.Append("  EntityIdentifier: ").Append(EntityIdentifier).Append("\n");
            sb.Append("  DataObjectNotSupported: ").Append(DataObjectNotSupported).Append("\n");
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
            return this.Equals(input as DataObjectNotSupportedByEntityErrorAllOf);
        }

        /// <summary>
        /// Returns true if DataObjectNotSupportedByEntityErrorAllOf instances are equal
        /// </summary>
        /// <param name="input">Instance of DataObjectNotSupportedByEntityErrorAllOf to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(DataObjectNotSupportedByEntityErrorAllOf input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.EntityIdentifier == input.EntityIdentifier ||
                    (this.EntityIdentifier != null &&
                    this.EntityIdentifier.Equals(input.EntityIdentifier))
                ) && 
                (
                    this.DataObjectNotSupported == input.DataObjectNotSupported ||
                    (this.DataObjectNotSupported != null &&
                    this.DataObjectNotSupported.Equals(input.DataObjectNotSupported))
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
                if (this.EntityIdentifier != null)
                {
                    hashCode = (hashCode * 59) + this.EntityIdentifier.GetHashCode();
                }
                if (this.DataObjectNotSupported != null)
                {
                    hashCode = (hashCode * 59) + this.DataObjectNotSupported.GetHashCode();
                }
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
