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

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model
{
    /// <summary>
    /// CoreApiConsensusManagerFieldConfigValue
    /// </summary>
    [DataContract(Name = "CoreApiConsensusManagerFieldConfigValue")]
    public partial class CoreApiConsensusManagerFieldConfigValue : IEquatable<CoreApiConsensusManagerFieldConfigValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreApiConsensusManagerFieldConfigValue" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected CoreApiConsensusManagerFieldConfigValue() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreApiConsensusManagerFieldConfigValue" /> class.
        /// </summary>
        /// <param name="maxValidators">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the maximum number of validators in the active validator set.  (required).</param>
        /// <param name="epochChangeCondition">epochChangeCondition (required).</param>
        /// <param name="numUnstakeEpochs">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the minimum number of epochs before an unstaker can withdraw their XRD.  (required).</param>
        /// <param name="totalEmissionXrdPerEpoch">A string-encoded fixed-precision decimal to 18 decimal places. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        /// <param name="minValidatorReliability">A string-encoded fixed-precision decimal to 18 decimal places. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        /// <param name="numOwnerStakeUnitsUnlockEpochs">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the minimum number of epochs before an owner can take their stake units after attempting to withdraw them.  (required).</param>
        /// <param name="numFeeIncreaseDelayEpochs">An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the minimum number of epochs before a fee increase takes effect.  (required).</param>
        /// <param name="validatorCreationUsdEquivalentCost">The defining decimal cost of a validator in USD. This is turned into an XRD cost through the current protocol-based USD/XRD multiplier. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        /// <param name="validatorCreationXrdCost">The decimal amount of XRD required to be passed in a bucket to create a validator. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;.  (required).</param>
        public CoreApiConsensusManagerFieldConfigValue(long maxValidators = default(long), CoreApiEpochChangeCondition epochChangeCondition = default(CoreApiEpochChangeCondition), long numUnstakeEpochs = default(long), string totalEmissionXrdPerEpoch = default(string), string minValidatorReliability = default(string), long numOwnerStakeUnitsUnlockEpochs = default(long), long numFeeIncreaseDelayEpochs = default(long), string validatorCreationUsdEquivalentCost = default(string), string validatorCreationXrdCost = default(string))
        {
            this.MaxValidators = maxValidators;
            // to ensure "epochChangeCondition" is required (not null)
            if (epochChangeCondition == null)
            {
                throw new ArgumentNullException("epochChangeCondition is a required property for CoreApiConsensusManagerFieldConfigValue and cannot be null");
            }
            this.EpochChangeCondition = epochChangeCondition;
            this.NumUnstakeEpochs = numUnstakeEpochs;
            // to ensure "totalEmissionXrdPerEpoch" is required (not null)
            if (totalEmissionXrdPerEpoch == null)
            {
                throw new ArgumentNullException("totalEmissionXrdPerEpoch is a required property for CoreApiConsensusManagerFieldConfigValue and cannot be null");
            }
            this.TotalEmissionXrdPerEpoch = totalEmissionXrdPerEpoch;
            // to ensure "minValidatorReliability" is required (not null)
            if (minValidatorReliability == null)
            {
                throw new ArgumentNullException("minValidatorReliability is a required property for CoreApiConsensusManagerFieldConfigValue and cannot be null");
            }
            this.MinValidatorReliability = minValidatorReliability;
            this.NumOwnerStakeUnitsUnlockEpochs = numOwnerStakeUnitsUnlockEpochs;
            this.NumFeeIncreaseDelayEpochs = numFeeIncreaseDelayEpochs;
            // to ensure "validatorCreationUsdEquivalentCost" is required (not null)
            if (validatorCreationUsdEquivalentCost == null)
            {
                throw new ArgumentNullException("validatorCreationUsdEquivalentCost is a required property for CoreApiConsensusManagerFieldConfigValue and cannot be null");
            }
            this.ValidatorCreationUsdEquivalentCost = validatorCreationUsdEquivalentCost;
            // to ensure "validatorCreationXrdCost" is required (not null)
            if (validatorCreationXrdCost == null)
            {
                throw new ArgumentNullException("validatorCreationXrdCost is a required property for CoreApiConsensusManagerFieldConfigValue and cannot be null");
            }
            this.ValidatorCreationXrdCost = validatorCreationXrdCost;
        }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the maximum number of validators in the active validator set. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the maximum number of validators in the active validator set. </value>
        [DataMember(Name = "max_validators", IsRequired = true, EmitDefaultValue = true)]
        public long MaxValidators { get; set; }

        /// <summary>
        /// Gets or Sets EpochChangeCondition
        /// </summary>
        [DataMember(Name = "epoch_change_condition", IsRequired = true, EmitDefaultValue = true)]
        public CoreApiEpochChangeCondition EpochChangeCondition { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the minimum number of epochs before an unstaker can withdraw their XRD. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the minimum number of epochs before an unstaker can withdraw their XRD. </value>
        [DataMember(Name = "num_unstake_epochs", IsRequired = true, EmitDefaultValue = true)]
        public long NumUnstakeEpochs { get; set; }

        /// <summary>
        /// A string-encoded fixed-precision decimal to 18 decimal places. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>A string-encoded fixed-precision decimal to 18 decimal places. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "total_emission_xrd_per_epoch", IsRequired = true, EmitDefaultValue = true)]
        public string TotalEmissionXrdPerEpoch { get; set; }

        /// <summary>
        /// A string-encoded fixed-precision decimal to 18 decimal places. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>A string-encoded fixed-precision decimal to 18 decimal places. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "min_validator_reliability", IsRequired = true, EmitDefaultValue = true)]
        public string MinValidatorReliability { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the minimum number of epochs before an owner can take their stake units after attempting to withdraw them. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the minimum number of epochs before an owner can take their stake units after attempting to withdraw them. </value>
        [DataMember(Name = "num_owner_stake_units_unlock_epochs", IsRequired = true, EmitDefaultValue = true)]
        public long NumOwnerStakeUnitsUnlockEpochs { get; set; }

        /// <summary>
        /// An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the minimum number of epochs before a fee increase takes effect. 
        /// </summary>
        /// <value>An integer between &#x60;0&#x60; and &#x60;10^10&#x60;, specifying the minimum number of epochs before a fee increase takes effect. </value>
        [DataMember(Name = "num_fee_increase_delay_epochs", IsRequired = true, EmitDefaultValue = true)]
        public long NumFeeIncreaseDelayEpochs { get; set; }

        /// <summary>
        /// The defining decimal cost of a validator in USD. This is turned into an XRD cost through the current protocol-based USD/XRD multiplier. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>The defining decimal cost of a validator in USD. This is turned into an XRD cost through the current protocol-based USD/XRD multiplier. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "validator_creation_usd_equivalent_cost", IsRequired = true, EmitDefaultValue = true)]
        public string ValidatorCreationUsdEquivalentCost { get; set; }

        /// <summary>
        /// The decimal amount of XRD required to be passed in a bucket to create a validator. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. 
        /// </summary>
        /// <value>The decimal amount of XRD required to be passed in a bucket to create a validator. A decimal is formed of some signed integer &#x60;m&#x60; of attos (&#x60;10^(-18)&#x60;) units, where &#x60;-2^(192 - 1) &lt;&#x3D; m &lt; 2^(192 - 1)&#x60;. </value>
        [DataMember(Name = "validator_creation_xrd_cost", IsRequired = true, EmitDefaultValue = true)]
        public string ValidatorCreationXrdCost { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class CoreApiConsensusManagerFieldConfigValue {\n");
            sb.Append("  MaxValidators: ").Append(MaxValidators).Append("\n");
            sb.Append("  EpochChangeCondition: ").Append(EpochChangeCondition).Append("\n");
            sb.Append("  NumUnstakeEpochs: ").Append(NumUnstakeEpochs).Append("\n");
            sb.Append("  TotalEmissionXrdPerEpoch: ").Append(TotalEmissionXrdPerEpoch).Append("\n");
            sb.Append("  MinValidatorReliability: ").Append(MinValidatorReliability).Append("\n");
            sb.Append("  NumOwnerStakeUnitsUnlockEpochs: ").Append(NumOwnerStakeUnitsUnlockEpochs).Append("\n");
            sb.Append("  NumFeeIncreaseDelayEpochs: ").Append(NumFeeIncreaseDelayEpochs).Append("\n");
            sb.Append("  ValidatorCreationUsdEquivalentCost: ").Append(ValidatorCreationUsdEquivalentCost).Append("\n");
            sb.Append("  ValidatorCreationXrdCost: ").Append(ValidatorCreationXrdCost).Append("\n");
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
            return this.Equals(input as CoreApiConsensusManagerFieldConfigValue);
        }

        /// <summary>
        /// Returns true if CoreApiConsensusManagerFieldConfigValue instances are equal
        /// </summary>
        /// <param name="input">Instance of CoreApiConsensusManagerFieldConfigValue to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CoreApiConsensusManagerFieldConfigValue input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.MaxValidators == input.MaxValidators ||
                    this.MaxValidators.Equals(input.MaxValidators)
                ) && 
                (
                    this.EpochChangeCondition == input.EpochChangeCondition ||
                    (this.EpochChangeCondition != null &&
                    this.EpochChangeCondition.Equals(input.EpochChangeCondition))
                ) && 
                (
                    this.NumUnstakeEpochs == input.NumUnstakeEpochs ||
                    this.NumUnstakeEpochs.Equals(input.NumUnstakeEpochs)
                ) && 
                (
                    this.TotalEmissionXrdPerEpoch == input.TotalEmissionXrdPerEpoch ||
                    (this.TotalEmissionXrdPerEpoch != null &&
                    this.TotalEmissionXrdPerEpoch.Equals(input.TotalEmissionXrdPerEpoch))
                ) && 
                (
                    this.MinValidatorReliability == input.MinValidatorReliability ||
                    (this.MinValidatorReliability != null &&
                    this.MinValidatorReliability.Equals(input.MinValidatorReliability))
                ) && 
                (
                    this.NumOwnerStakeUnitsUnlockEpochs == input.NumOwnerStakeUnitsUnlockEpochs ||
                    this.NumOwnerStakeUnitsUnlockEpochs.Equals(input.NumOwnerStakeUnitsUnlockEpochs)
                ) && 
                (
                    this.NumFeeIncreaseDelayEpochs == input.NumFeeIncreaseDelayEpochs ||
                    this.NumFeeIncreaseDelayEpochs.Equals(input.NumFeeIncreaseDelayEpochs)
                ) && 
                (
                    this.ValidatorCreationUsdEquivalentCost == input.ValidatorCreationUsdEquivalentCost ||
                    (this.ValidatorCreationUsdEquivalentCost != null &&
                    this.ValidatorCreationUsdEquivalentCost.Equals(input.ValidatorCreationUsdEquivalentCost))
                ) && 
                (
                    this.ValidatorCreationXrdCost == input.ValidatorCreationXrdCost ||
                    (this.ValidatorCreationXrdCost != null &&
                    this.ValidatorCreationXrdCost.Equals(input.ValidatorCreationXrdCost))
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
                hashCode = (hashCode * 59) + this.MaxValidators.GetHashCode();
                if (this.EpochChangeCondition != null)
                {
                    hashCode = (hashCode * 59) + this.EpochChangeCondition.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.NumUnstakeEpochs.GetHashCode();
                if (this.TotalEmissionXrdPerEpoch != null)
                {
                    hashCode = (hashCode * 59) + this.TotalEmissionXrdPerEpoch.GetHashCode();
                }
                if (this.MinValidatorReliability != null)
                {
                    hashCode = (hashCode * 59) + this.MinValidatorReliability.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.NumOwnerStakeUnitsUnlockEpochs.GetHashCode();
                hashCode = (hashCode * 59) + this.NumFeeIncreaseDelayEpochs.GetHashCode();
                if (this.ValidatorCreationUsdEquivalentCost != null)
                {
                    hashCode = (hashCode * 59) + this.ValidatorCreationUsdEquivalentCost.GetHashCode();
                }
                if (this.ValidatorCreationXrdCost != null)
                {
                    hashCode = (hashCode * 59) + this.ValidatorCreationXrdCost.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
