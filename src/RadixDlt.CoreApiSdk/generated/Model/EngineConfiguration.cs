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
    /// EngineConfiguration
    /// </summary>
    [DataContract(Name = "EngineConfiguration")]
    public partial class EngineConfiguration : IEquatable<EngineConfiguration>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineConfiguration" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected EngineConfiguration() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineConfiguration" /> class.
        /// </summary>
        /// <param name="nativeToken">nativeToken (required).</param>
        /// <param name="maximumMessageLength">maximumMessageLength (required).</param>
        /// <param name="maximumValidators">maximumValidators (required).</param>
        /// <param name="tokenSymbolPattern">tokenSymbolPattern (required).</param>
        /// <param name="unstakingDelayEpochLength">unstakingDelayEpochLength (required).</param>
        /// <param name="minimumCompletedProposalsPercentage">minimumCompletedProposalsPercentage (required).</param>
        /// <param name="maximumTransactionSize">maximumTransactionSize (required).</param>
        /// <param name="maximumTransactionsPerRound">maximumTransactionsPerRound (required).</param>
        /// <param name="validatorFeeIncreaseDebouncerEpochLength">validatorFeeIncreaseDebouncerEpochLength (required).</param>
        /// <param name="maximumRoundsPerEpoch">maximumRoundsPerEpoch (required).</param>
        /// <param name="maximumValidatorFeeIncrease">maximumValidatorFeeIncrease (required).</param>
        /// <param name="minimumStake">minimumStake (required).</param>
        /// <param name="rewardsPerProposal">rewardsPerProposal (required).</param>
        /// <param name="reservedSymbols">reservedSymbols (required).</param>
        /// <param name="feeTable">feeTable (required).</param>
        public EngineConfiguration(TokenResourceIdentifier nativeToken = default(TokenResourceIdentifier), int maximumMessageLength = default(int), int maximumValidators = default(int), string tokenSymbolPattern = default(string), long unstakingDelayEpochLength = default(long), int minimumCompletedProposalsPercentage = default(int), long maximumTransactionSize = default(long), int maximumTransactionsPerRound = default(int), long validatorFeeIncreaseDebouncerEpochLength = default(long), long maximumRoundsPerEpoch = default(long), int maximumValidatorFeeIncrease = default(int), ResourceAmount minimumStake = default(ResourceAmount), ResourceAmount rewardsPerProposal = default(ResourceAmount), List<string> reservedSymbols = default(List<string>), FeeTable feeTable = default(FeeTable))
        {
            // to ensure "nativeToken" is required (not null)
            if (nativeToken == null)
            {
                throw new ArgumentNullException("nativeToken is a required property for EngineConfiguration and cannot be null");
            }
            this.NativeToken = nativeToken;
            this.MaximumMessageLength = maximumMessageLength;
            this.MaximumValidators = maximumValidators;
            // to ensure "tokenSymbolPattern" is required (not null)
            if (tokenSymbolPattern == null)
            {
                throw new ArgumentNullException("tokenSymbolPattern is a required property for EngineConfiguration and cannot be null");
            }
            this.TokenSymbolPattern = tokenSymbolPattern;
            this.UnstakingDelayEpochLength = unstakingDelayEpochLength;
            this.MinimumCompletedProposalsPercentage = minimumCompletedProposalsPercentage;
            this.MaximumTransactionSize = maximumTransactionSize;
            this.MaximumTransactionsPerRound = maximumTransactionsPerRound;
            this.ValidatorFeeIncreaseDebouncerEpochLength = validatorFeeIncreaseDebouncerEpochLength;
            this.MaximumRoundsPerEpoch = maximumRoundsPerEpoch;
            this.MaximumValidatorFeeIncrease = maximumValidatorFeeIncrease;
            // to ensure "minimumStake" is required (not null)
            if (minimumStake == null)
            {
                throw new ArgumentNullException("minimumStake is a required property for EngineConfiguration and cannot be null");
            }
            this.MinimumStake = minimumStake;
            // to ensure "rewardsPerProposal" is required (not null)
            if (rewardsPerProposal == null)
            {
                throw new ArgumentNullException("rewardsPerProposal is a required property for EngineConfiguration and cannot be null");
            }
            this.RewardsPerProposal = rewardsPerProposal;
            // to ensure "reservedSymbols" is required (not null)
            if (reservedSymbols == null)
            {
                throw new ArgumentNullException("reservedSymbols is a required property for EngineConfiguration and cannot be null");
            }
            this.ReservedSymbols = reservedSymbols;
            // to ensure "feeTable" is required (not null)
            if (feeTable == null)
            {
                throw new ArgumentNullException("feeTable is a required property for EngineConfiguration and cannot be null");
            }
            this.FeeTable = feeTable;
        }

        /// <summary>
        /// Gets or Sets NativeToken
        /// </summary>
        [DataMember(Name = "native_token", IsRequired = true, EmitDefaultValue = true)]
        public TokenResourceIdentifier NativeToken { get; set; }

        /// <summary>
        /// Gets or Sets MaximumMessageLength
        /// </summary>
        [DataMember(Name = "maximum_message_length", IsRequired = true, EmitDefaultValue = true)]
        public int MaximumMessageLength { get; set; }

        /// <summary>
        /// Gets or Sets MaximumValidators
        /// </summary>
        [DataMember(Name = "maximum_validators", IsRequired = true, EmitDefaultValue = true)]
        public int MaximumValidators { get; set; }

        /// <summary>
        /// Gets or Sets TokenSymbolPattern
        /// </summary>
        [DataMember(Name = "token_symbol_pattern", IsRequired = true, EmitDefaultValue = true)]
        public string TokenSymbolPattern { get; set; }

        /// <summary>
        /// Gets or Sets UnstakingDelayEpochLength
        /// </summary>
        [DataMember(Name = "unstaking_delay_epoch_length", IsRequired = true, EmitDefaultValue = true)]
        public long UnstakingDelayEpochLength { get; set; }

        /// <summary>
        /// Gets or Sets MinimumCompletedProposalsPercentage
        /// </summary>
        [DataMember(Name = "minimum_completed_proposals_percentage", IsRequired = true, EmitDefaultValue = true)]
        public int MinimumCompletedProposalsPercentage { get; set; }

        /// <summary>
        /// Gets or Sets MaximumTransactionSize
        /// </summary>
        [DataMember(Name = "maximum_transaction_size", IsRequired = true, EmitDefaultValue = true)]
        public long MaximumTransactionSize { get; set; }

        /// <summary>
        /// Gets or Sets MaximumTransactionsPerRound
        /// </summary>
        [DataMember(Name = "maximum_transactions_per_round", IsRequired = true, EmitDefaultValue = true)]
        public int MaximumTransactionsPerRound { get; set; }

        /// <summary>
        /// Gets or Sets ValidatorFeeIncreaseDebouncerEpochLength
        /// </summary>
        [DataMember(Name = "validator_fee_increase_debouncer_epoch_length", IsRequired = true, EmitDefaultValue = true)]
        public long ValidatorFeeIncreaseDebouncerEpochLength { get; set; }

        /// <summary>
        /// Gets or Sets MaximumRoundsPerEpoch
        /// </summary>
        [DataMember(Name = "maximum_rounds_per_epoch", IsRequired = true, EmitDefaultValue = true)]
        public long MaximumRoundsPerEpoch { get; set; }

        /// <summary>
        /// Gets or Sets MaximumValidatorFeeIncrease
        /// </summary>
        [DataMember(Name = "maximum_validator_fee_increase", IsRequired = true, EmitDefaultValue = true)]
        public int MaximumValidatorFeeIncrease { get; set; }

        /// <summary>
        /// Gets or Sets MinimumStake
        /// </summary>
        [DataMember(Name = "minimum_stake", IsRequired = true, EmitDefaultValue = true)]
        public ResourceAmount MinimumStake { get; set; }

        /// <summary>
        /// Gets or Sets RewardsPerProposal
        /// </summary>
        [DataMember(Name = "rewards_per_proposal", IsRequired = true, EmitDefaultValue = true)]
        public ResourceAmount RewardsPerProposal { get; set; }

        /// <summary>
        /// Gets or Sets ReservedSymbols
        /// </summary>
        [DataMember(Name = "reserved_symbols", IsRequired = true, EmitDefaultValue = true)]
        public List<string> ReservedSymbols { get; set; }

        /// <summary>
        /// Gets or Sets FeeTable
        /// </summary>
        [DataMember(Name = "fee_table", IsRequired = true, EmitDefaultValue = true)]
        public FeeTable FeeTable { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class EngineConfiguration {\n");
            sb.Append("  NativeToken: ").Append(NativeToken).Append("\n");
            sb.Append("  MaximumMessageLength: ").Append(MaximumMessageLength).Append("\n");
            sb.Append("  MaximumValidators: ").Append(MaximumValidators).Append("\n");
            sb.Append("  TokenSymbolPattern: ").Append(TokenSymbolPattern).Append("\n");
            sb.Append("  UnstakingDelayEpochLength: ").Append(UnstakingDelayEpochLength).Append("\n");
            sb.Append("  MinimumCompletedProposalsPercentage: ").Append(MinimumCompletedProposalsPercentage).Append("\n");
            sb.Append("  MaximumTransactionSize: ").Append(MaximumTransactionSize).Append("\n");
            sb.Append("  MaximumTransactionsPerRound: ").Append(MaximumTransactionsPerRound).Append("\n");
            sb.Append("  ValidatorFeeIncreaseDebouncerEpochLength: ").Append(ValidatorFeeIncreaseDebouncerEpochLength).Append("\n");
            sb.Append("  MaximumRoundsPerEpoch: ").Append(MaximumRoundsPerEpoch).Append("\n");
            sb.Append("  MaximumValidatorFeeIncrease: ").Append(MaximumValidatorFeeIncrease).Append("\n");
            sb.Append("  MinimumStake: ").Append(MinimumStake).Append("\n");
            sb.Append("  RewardsPerProposal: ").Append(RewardsPerProposal).Append("\n");
            sb.Append("  ReservedSymbols: ").Append(ReservedSymbols).Append("\n");
            sb.Append("  FeeTable: ").Append(FeeTable).Append("\n");
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
            return this.Equals(input as EngineConfiguration);
        }

        /// <summary>
        /// Returns true if EngineConfiguration instances are equal
        /// </summary>
        /// <param name="input">Instance of EngineConfiguration to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(EngineConfiguration input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.NativeToken == input.NativeToken ||
                    (this.NativeToken != null &&
                    this.NativeToken.Equals(input.NativeToken))
                ) && 
                (
                    this.MaximumMessageLength == input.MaximumMessageLength ||
                    this.MaximumMessageLength.Equals(input.MaximumMessageLength)
                ) && 
                (
                    this.MaximumValidators == input.MaximumValidators ||
                    this.MaximumValidators.Equals(input.MaximumValidators)
                ) && 
                (
                    this.TokenSymbolPattern == input.TokenSymbolPattern ||
                    (this.TokenSymbolPattern != null &&
                    this.TokenSymbolPattern.Equals(input.TokenSymbolPattern))
                ) && 
                (
                    this.UnstakingDelayEpochLength == input.UnstakingDelayEpochLength ||
                    this.UnstakingDelayEpochLength.Equals(input.UnstakingDelayEpochLength)
                ) && 
                (
                    this.MinimumCompletedProposalsPercentage == input.MinimumCompletedProposalsPercentage ||
                    this.MinimumCompletedProposalsPercentage.Equals(input.MinimumCompletedProposalsPercentage)
                ) && 
                (
                    this.MaximumTransactionSize == input.MaximumTransactionSize ||
                    this.MaximumTransactionSize.Equals(input.MaximumTransactionSize)
                ) && 
                (
                    this.MaximumTransactionsPerRound == input.MaximumTransactionsPerRound ||
                    this.MaximumTransactionsPerRound.Equals(input.MaximumTransactionsPerRound)
                ) && 
                (
                    this.ValidatorFeeIncreaseDebouncerEpochLength == input.ValidatorFeeIncreaseDebouncerEpochLength ||
                    this.ValidatorFeeIncreaseDebouncerEpochLength.Equals(input.ValidatorFeeIncreaseDebouncerEpochLength)
                ) && 
                (
                    this.MaximumRoundsPerEpoch == input.MaximumRoundsPerEpoch ||
                    this.MaximumRoundsPerEpoch.Equals(input.MaximumRoundsPerEpoch)
                ) && 
                (
                    this.MaximumValidatorFeeIncrease == input.MaximumValidatorFeeIncrease ||
                    this.MaximumValidatorFeeIncrease.Equals(input.MaximumValidatorFeeIncrease)
                ) && 
                (
                    this.MinimumStake == input.MinimumStake ||
                    (this.MinimumStake != null &&
                    this.MinimumStake.Equals(input.MinimumStake))
                ) && 
                (
                    this.RewardsPerProposal == input.RewardsPerProposal ||
                    (this.RewardsPerProposal != null &&
                    this.RewardsPerProposal.Equals(input.RewardsPerProposal))
                ) && 
                (
                    this.ReservedSymbols == input.ReservedSymbols ||
                    this.ReservedSymbols != null &&
                    input.ReservedSymbols != null &&
                    this.ReservedSymbols.SequenceEqual(input.ReservedSymbols)
                ) && 
                (
                    this.FeeTable == input.FeeTable ||
                    (this.FeeTable != null &&
                    this.FeeTable.Equals(input.FeeTable))
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
                if (this.NativeToken != null)
                {
                    hashCode = (hashCode * 59) + this.NativeToken.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.MaximumMessageLength.GetHashCode();
                hashCode = (hashCode * 59) + this.MaximumValidators.GetHashCode();
                if (this.TokenSymbolPattern != null)
                {
                    hashCode = (hashCode * 59) + this.TokenSymbolPattern.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.UnstakingDelayEpochLength.GetHashCode();
                hashCode = (hashCode * 59) + this.MinimumCompletedProposalsPercentage.GetHashCode();
                hashCode = (hashCode * 59) + this.MaximumTransactionSize.GetHashCode();
                hashCode = (hashCode * 59) + this.MaximumTransactionsPerRound.GetHashCode();
                hashCode = (hashCode * 59) + this.ValidatorFeeIncreaseDebouncerEpochLength.GetHashCode();
                hashCode = (hashCode * 59) + this.MaximumRoundsPerEpoch.GetHashCode();
                hashCode = (hashCode * 59) + this.MaximumValidatorFeeIncrease.GetHashCode();
                if (this.MinimumStake != null)
                {
                    hashCode = (hashCode * 59) + this.MinimumStake.GetHashCode();
                }
                if (this.RewardsPerProposal != null)
                {
                    hashCode = (hashCode * 59) + this.RewardsPerProposal.GetHashCode();
                }
                if (this.ReservedSymbols != null)
                {
                    hashCode = (hashCode * 59) + this.ReservedSymbols.GetHashCode();
                }
                if (this.FeeTable != null)
                {
                    hashCode = (hashCode * 59) + this.FeeTable.GetHashCode();
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
