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
    /// TransactionValidationConfig
    /// </summary>
    [DataContract(Name = "TransactionValidationConfig")]
    public partial class TransactionValidationConfig : IEquatable<TransactionValidationConfig>
    {

        /// <summary>
        /// Gets or Sets ManifestValidation
        /// </summary>
        [DataMember(Name = "manifest_validation", IsRequired = true, EmitDefaultValue = true)]
        public ManifestValidationRuleset ManifestValidation { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionValidationConfig" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TransactionValidationConfig() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionValidationConfig" /> class.
        /// </summary>
        /// <param name="maxSignerSignaturesPerIntent">maxSignerSignaturesPerIntent (required).</param>
        /// <param name="maxReferencesPerIntent">maxReferencesPerIntent (required).</param>
        /// <param name="minTipPercentage">Only applies to V1 transactions (required).</param>
        /// <param name="maxTipPercentage">Only applies to V1 transactions (required).</param>
        /// <param name="maxEpochRange">maxEpochRange (required).</param>
        /// <param name="maxInstructions">maxInstructions (required).</param>
        /// <param name="messageValidation">messageValidation (required).</param>
        /// <param name="v1TransactionsAllowNotaryToDuplicateSigner">v1TransactionsAllowNotaryToDuplicateSigner (required).</param>
        /// <param name="preparationSettings">preparationSettings (required).</param>
        /// <param name="manifestValidation">manifestValidation (required).</param>
        /// <param name="v2TransactionsAllowed">v2TransactionsAllowed (required).</param>
        /// <param name="minTipBasisPoints">minTipBasisPoints (required).</param>
        /// <param name="maxTipBasisPoints">maxTipBasisPoints (required).</param>
        /// <param name="maxSubintentDepth">maxSubintentDepth (required).</param>
        /// <param name="maxTotalSignatureValidations">maxTotalSignatureValidations (required).</param>
        /// <param name="maxTotalReferences">maxTotalReferences (required).</param>
        public TransactionValidationConfig(string maxSignerSignaturesPerIntent = default(string), string maxReferencesPerIntent = default(string), int minTipPercentage = default(int), int maxTipPercentage = default(int), string maxEpochRange = default(string), string maxInstructions = default(string), MessageValidationConfig messageValidation = default(MessageValidationConfig), bool v1TransactionsAllowNotaryToDuplicateSigner = default(bool), PreparationSettings preparationSettings = default(PreparationSettings), ManifestValidationRuleset manifestValidation = default(ManifestValidationRuleset), bool v2TransactionsAllowed = default(bool), long minTipBasisPoints = default(long), long maxTipBasisPoints = default(long), string maxSubintentDepth = default(string), string maxTotalSignatureValidations = default(string), string maxTotalReferences = default(string))
        {
            // to ensure "maxSignerSignaturesPerIntent" is required (not null)
            if (maxSignerSignaturesPerIntent == null)
            {
                throw new ArgumentNullException("maxSignerSignaturesPerIntent is a required property for TransactionValidationConfig and cannot be null");
            }
            this.MaxSignerSignaturesPerIntent = maxSignerSignaturesPerIntent;
            // to ensure "maxReferencesPerIntent" is required (not null)
            if (maxReferencesPerIntent == null)
            {
                throw new ArgumentNullException("maxReferencesPerIntent is a required property for TransactionValidationConfig and cannot be null");
            }
            this.MaxReferencesPerIntent = maxReferencesPerIntent;
            this.MinTipPercentage = minTipPercentage;
            this.MaxTipPercentage = maxTipPercentage;
            // to ensure "maxEpochRange" is required (not null)
            if (maxEpochRange == null)
            {
                throw new ArgumentNullException("maxEpochRange is a required property for TransactionValidationConfig and cannot be null");
            }
            this.MaxEpochRange = maxEpochRange;
            // to ensure "maxInstructions" is required (not null)
            if (maxInstructions == null)
            {
                throw new ArgumentNullException("maxInstructions is a required property for TransactionValidationConfig and cannot be null");
            }
            this.MaxInstructions = maxInstructions;
            // to ensure "messageValidation" is required (not null)
            if (messageValidation == null)
            {
                throw new ArgumentNullException("messageValidation is a required property for TransactionValidationConfig and cannot be null");
            }
            this.MessageValidation = messageValidation;
            this.V1TransactionsAllowNotaryToDuplicateSigner = v1TransactionsAllowNotaryToDuplicateSigner;
            // to ensure "preparationSettings" is required (not null)
            if (preparationSettings == null)
            {
                throw new ArgumentNullException("preparationSettings is a required property for TransactionValidationConfig and cannot be null");
            }
            this.PreparationSettings = preparationSettings;
            this.ManifestValidation = manifestValidation;
            this.V2TransactionsAllowed = v2TransactionsAllowed;
            this.MinTipBasisPoints = minTipBasisPoints;
            this.MaxTipBasisPoints = maxTipBasisPoints;
            // to ensure "maxSubintentDepth" is required (not null)
            if (maxSubintentDepth == null)
            {
                throw new ArgumentNullException("maxSubintentDepth is a required property for TransactionValidationConfig and cannot be null");
            }
            this.MaxSubintentDepth = maxSubintentDepth;
            // to ensure "maxTotalSignatureValidations" is required (not null)
            if (maxTotalSignatureValidations == null)
            {
                throw new ArgumentNullException("maxTotalSignatureValidations is a required property for TransactionValidationConfig and cannot be null");
            }
            this.MaxTotalSignatureValidations = maxTotalSignatureValidations;
            // to ensure "maxTotalReferences" is required (not null)
            if (maxTotalReferences == null)
            {
                throw new ArgumentNullException("maxTotalReferences is a required property for TransactionValidationConfig and cannot be null");
            }
            this.MaxTotalReferences = maxTotalReferences;
        }

        /// <summary>
        /// Gets or Sets MaxSignerSignaturesPerIntent
        /// </summary>
        [DataMember(Name = "max_signer_signatures_per_intent", IsRequired = true, EmitDefaultValue = true)]
        public string MaxSignerSignaturesPerIntent { get; set; }

        /// <summary>
        /// Gets or Sets MaxReferencesPerIntent
        /// </summary>
        [DataMember(Name = "max_references_per_intent", IsRequired = true, EmitDefaultValue = true)]
        public string MaxReferencesPerIntent { get; set; }

        /// <summary>
        /// Only applies to V1 transactions
        /// </summary>
        /// <value>Only applies to V1 transactions</value>
        [DataMember(Name = "min_tip_percentage", IsRequired = true, EmitDefaultValue = true)]
        public int MinTipPercentage { get; set; }

        /// <summary>
        /// Only applies to V1 transactions
        /// </summary>
        /// <value>Only applies to V1 transactions</value>
        [DataMember(Name = "max_tip_percentage", IsRequired = true, EmitDefaultValue = true)]
        public int MaxTipPercentage { get; set; }

        /// <summary>
        /// Gets or Sets MaxEpochRange
        /// </summary>
        [DataMember(Name = "max_epoch_range", IsRequired = true, EmitDefaultValue = true)]
        public string MaxEpochRange { get; set; }

        /// <summary>
        /// Gets or Sets MaxInstructions
        /// </summary>
        [DataMember(Name = "max_instructions", IsRequired = true, EmitDefaultValue = true)]
        public string MaxInstructions { get; set; }

        /// <summary>
        /// Gets or Sets MessageValidation
        /// </summary>
        [DataMember(Name = "message_validation", IsRequired = true, EmitDefaultValue = true)]
        public MessageValidationConfig MessageValidation { get; set; }

        /// <summary>
        /// Gets or Sets V1TransactionsAllowNotaryToDuplicateSigner
        /// </summary>
        [DataMember(Name = "v1_transactions_allow_notary_to_duplicate_signer", IsRequired = true, EmitDefaultValue = true)]
        public bool V1TransactionsAllowNotaryToDuplicateSigner { get; set; }

        /// <summary>
        /// Gets or Sets PreparationSettings
        /// </summary>
        [DataMember(Name = "preparation_settings", IsRequired = true, EmitDefaultValue = true)]
        public PreparationSettings PreparationSettings { get; set; }

        /// <summary>
        /// Gets or Sets V2TransactionsAllowed
        /// </summary>
        [DataMember(Name = "v2_transactions_allowed", IsRequired = true, EmitDefaultValue = true)]
        public bool V2TransactionsAllowed { get; set; }

        /// <summary>
        /// Gets or Sets MinTipBasisPoints
        /// </summary>
        [DataMember(Name = "min_tip_basis_points", IsRequired = true, EmitDefaultValue = true)]
        public long MinTipBasisPoints { get; set; }

        /// <summary>
        /// Gets or Sets MaxTipBasisPoints
        /// </summary>
        [DataMember(Name = "max_tip_basis_points", IsRequired = true, EmitDefaultValue = true)]
        public long MaxTipBasisPoints { get; set; }

        /// <summary>
        /// Gets or Sets MaxSubintentDepth
        /// </summary>
        [DataMember(Name = "max_subintent_depth", IsRequired = true, EmitDefaultValue = true)]
        public string MaxSubintentDepth { get; set; }

        /// <summary>
        /// Gets or Sets MaxTotalSignatureValidations
        /// </summary>
        [DataMember(Name = "max_total_signature_validations", IsRequired = true, EmitDefaultValue = true)]
        public string MaxTotalSignatureValidations { get; set; }

        /// <summary>
        /// Gets or Sets MaxTotalReferences
        /// </summary>
        [DataMember(Name = "max_total_references", IsRequired = true, EmitDefaultValue = true)]
        public string MaxTotalReferences { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class TransactionValidationConfig {\n");
            sb.Append("  MaxSignerSignaturesPerIntent: ").Append(MaxSignerSignaturesPerIntent).Append("\n");
            sb.Append("  MaxReferencesPerIntent: ").Append(MaxReferencesPerIntent).Append("\n");
            sb.Append("  MinTipPercentage: ").Append(MinTipPercentage).Append("\n");
            sb.Append("  MaxTipPercentage: ").Append(MaxTipPercentage).Append("\n");
            sb.Append("  MaxEpochRange: ").Append(MaxEpochRange).Append("\n");
            sb.Append("  MaxInstructions: ").Append(MaxInstructions).Append("\n");
            sb.Append("  MessageValidation: ").Append(MessageValidation).Append("\n");
            sb.Append("  V1TransactionsAllowNotaryToDuplicateSigner: ").Append(V1TransactionsAllowNotaryToDuplicateSigner).Append("\n");
            sb.Append("  PreparationSettings: ").Append(PreparationSettings).Append("\n");
            sb.Append("  ManifestValidation: ").Append(ManifestValidation).Append("\n");
            sb.Append("  V2TransactionsAllowed: ").Append(V2TransactionsAllowed).Append("\n");
            sb.Append("  MinTipBasisPoints: ").Append(MinTipBasisPoints).Append("\n");
            sb.Append("  MaxTipBasisPoints: ").Append(MaxTipBasisPoints).Append("\n");
            sb.Append("  MaxSubintentDepth: ").Append(MaxSubintentDepth).Append("\n");
            sb.Append("  MaxTotalSignatureValidations: ").Append(MaxTotalSignatureValidations).Append("\n");
            sb.Append("  MaxTotalReferences: ").Append(MaxTotalReferences).Append("\n");
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
            return this.Equals(input as TransactionValidationConfig);
        }

        /// <summary>
        /// Returns true if TransactionValidationConfig instances are equal
        /// </summary>
        /// <param name="input">Instance of TransactionValidationConfig to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TransactionValidationConfig input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.MaxSignerSignaturesPerIntent == input.MaxSignerSignaturesPerIntent ||
                    (this.MaxSignerSignaturesPerIntent != null &&
                    this.MaxSignerSignaturesPerIntent.Equals(input.MaxSignerSignaturesPerIntent))
                ) && 
                (
                    this.MaxReferencesPerIntent == input.MaxReferencesPerIntent ||
                    (this.MaxReferencesPerIntent != null &&
                    this.MaxReferencesPerIntent.Equals(input.MaxReferencesPerIntent))
                ) && 
                (
                    this.MinTipPercentage == input.MinTipPercentage ||
                    this.MinTipPercentage.Equals(input.MinTipPercentage)
                ) && 
                (
                    this.MaxTipPercentage == input.MaxTipPercentage ||
                    this.MaxTipPercentage.Equals(input.MaxTipPercentage)
                ) && 
                (
                    this.MaxEpochRange == input.MaxEpochRange ||
                    (this.MaxEpochRange != null &&
                    this.MaxEpochRange.Equals(input.MaxEpochRange))
                ) && 
                (
                    this.MaxInstructions == input.MaxInstructions ||
                    (this.MaxInstructions != null &&
                    this.MaxInstructions.Equals(input.MaxInstructions))
                ) && 
                (
                    this.MessageValidation == input.MessageValidation ||
                    (this.MessageValidation != null &&
                    this.MessageValidation.Equals(input.MessageValidation))
                ) && 
                (
                    this.V1TransactionsAllowNotaryToDuplicateSigner == input.V1TransactionsAllowNotaryToDuplicateSigner ||
                    this.V1TransactionsAllowNotaryToDuplicateSigner.Equals(input.V1TransactionsAllowNotaryToDuplicateSigner)
                ) && 
                (
                    this.PreparationSettings == input.PreparationSettings ||
                    (this.PreparationSettings != null &&
                    this.PreparationSettings.Equals(input.PreparationSettings))
                ) && 
                (
                    this.ManifestValidation == input.ManifestValidation ||
                    this.ManifestValidation.Equals(input.ManifestValidation)
                ) && 
                (
                    this.V2TransactionsAllowed == input.V2TransactionsAllowed ||
                    this.V2TransactionsAllowed.Equals(input.V2TransactionsAllowed)
                ) && 
                (
                    this.MinTipBasisPoints == input.MinTipBasisPoints ||
                    this.MinTipBasisPoints.Equals(input.MinTipBasisPoints)
                ) && 
                (
                    this.MaxTipBasisPoints == input.MaxTipBasisPoints ||
                    this.MaxTipBasisPoints.Equals(input.MaxTipBasisPoints)
                ) && 
                (
                    this.MaxSubintentDepth == input.MaxSubintentDepth ||
                    (this.MaxSubintentDepth != null &&
                    this.MaxSubintentDepth.Equals(input.MaxSubintentDepth))
                ) && 
                (
                    this.MaxTotalSignatureValidations == input.MaxTotalSignatureValidations ||
                    (this.MaxTotalSignatureValidations != null &&
                    this.MaxTotalSignatureValidations.Equals(input.MaxTotalSignatureValidations))
                ) && 
                (
                    this.MaxTotalReferences == input.MaxTotalReferences ||
                    (this.MaxTotalReferences != null &&
                    this.MaxTotalReferences.Equals(input.MaxTotalReferences))
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
                if (this.MaxSignerSignaturesPerIntent != null)
                {
                    hashCode = (hashCode * 59) + this.MaxSignerSignaturesPerIntent.GetHashCode();
                }
                if (this.MaxReferencesPerIntent != null)
                {
                    hashCode = (hashCode * 59) + this.MaxReferencesPerIntent.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.MinTipPercentage.GetHashCode();
                hashCode = (hashCode * 59) + this.MaxTipPercentage.GetHashCode();
                if (this.MaxEpochRange != null)
                {
                    hashCode = (hashCode * 59) + this.MaxEpochRange.GetHashCode();
                }
                if (this.MaxInstructions != null)
                {
                    hashCode = (hashCode * 59) + this.MaxInstructions.GetHashCode();
                }
                if (this.MessageValidation != null)
                {
                    hashCode = (hashCode * 59) + this.MessageValidation.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.V1TransactionsAllowNotaryToDuplicateSigner.GetHashCode();
                if (this.PreparationSettings != null)
                {
                    hashCode = (hashCode * 59) + this.PreparationSettings.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.ManifestValidation.GetHashCode();
                hashCode = (hashCode * 59) + this.V2TransactionsAllowed.GetHashCode();
                hashCode = (hashCode * 59) + this.MinTipBasisPoints.GetHashCode();
                hashCode = (hashCode * 59) + this.MaxTipBasisPoints.GetHashCode();
                if (this.MaxSubintentDepth != null)
                {
                    hashCode = (hashCode * 59) + this.MaxSubintentDepth.GetHashCode();
                }
                if (this.MaxTotalSignatureValidations != null)
                {
                    hashCode = (hashCode * 59) + this.MaxTotalSignatureValidations.GetHashCode();
                }
                if (this.MaxTotalReferences != null)
                {
                    hashCode = (hashCode * 59) + this.MaxTotalReferences.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
