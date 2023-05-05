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
 * Babylon Core API - RCnet V2
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  It is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  This version of the Core API belongs to the first release candidate of the Radix Babylon network (\"RCnet-V1\").  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` are guaranteed to be forward compatible to Babylon mainnet launch (and beyond). We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
 *
 * The version of the OpenAPI document: 0.4.0
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
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// ValidatorSubstate
    /// </summary>
    [DataContract(Name = "ValidatorSubstate")]
    [JsonConverter(typeof(JsonSubtypes), "substate_type")]
    [JsonSubtypes.KnownSubType(typeof(AccessControllerSubstate), "AccessController")]
    [JsonSubtypes.KnownSubType(typeof(AccessRulesSubstate), "AccessRules")]
    [JsonSubtypes.KnownSubType(typeof(AccountSubstate), "Account")]
    [JsonSubtypes.KnownSubType(typeof(ClockSubstate), "Clock")]
    [JsonSubtypes.KnownSubType(typeof(ComponentRoyaltyAccumulatorSubstate), "ComponentRoyaltyAccumulator")]
    [JsonSubtypes.KnownSubType(typeof(ComponentRoyaltyConfigSubstate), "ComponentRoyaltyConfig")]
    [JsonSubtypes.KnownSubType(typeof(ComponentStateSubstate), "ComponentState")]
    [JsonSubtypes.KnownSubType(typeof(EpochManagerSubstate), "EpochManager")]
    [JsonSubtypes.KnownSubType(typeof(FungibleResourceManagerSubstate), "FungibleResourceManager")]
    [JsonSubtypes.KnownSubType(typeof(KeyValueStoreEntrySubstate), "KeyValueStoreEntry")]
    [JsonSubtypes.KnownSubType(typeof(MetadataEntrySubstate), "MetadataEntry")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerSubstate), "NonFungibleResourceManager")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeSubstate), "PackageCode")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeTypeSubstate), "PackageCodeType")]
    [JsonSubtypes.KnownSubType(typeof(PackageFunctionAccessRulesSubstate), "PackageFunctionAccessRules")]
    [JsonSubtypes.KnownSubType(typeof(PackageInfoSubstate), "PackageInfo")]
    [JsonSubtypes.KnownSubType(typeof(PackageRoyaltySubstate), "PackageRoyalty")]
    [JsonSubtypes.KnownSubType(typeof(TypeInfoSubstate), "TypeInfo")]
    [JsonSubtypes.KnownSubType(typeof(ValidatorSubstate), "Validator")]
    [JsonSubtypes.KnownSubType(typeof(ValidatorSetSubstate), "ValidatorSet")]
    [JsonSubtypes.KnownSubType(typeof(VaultFungibleSubstate), "VaultFungible")]
    [JsonSubtypes.KnownSubType(typeof(VaultInfoSubstate), "VaultInfo")]
    [JsonSubtypes.KnownSubType(typeof(VaultNonFungibleSubstate), "VaultNonFungible")]
    public partial class ValidatorSubstate : Substate, IEquatable<ValidatorSubstate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorSubstate" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ValidatorSubstate() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorSubstate" /> class.
        /// </summary>
        /// <param name="epochManagerAddress">The Bech32m-encoded human readable version of the component address (required).</param>
        /// <param name="validatorAddress">The Bech32m-encoded human readable version of the component address (required).</param>
        /// <param name="publicKey">publicKey (required).</param>
        /// <param name="stakeVault">stakeVault (required).</param>
        /// <param name="unstakeVault">unstakeVault (required).</param>
        /// <param name="liquidStakeUnitResourceAddress">The Bech32m-encoded human readable version of the resource address (required).</param>
        /// <param name="unstakeClaimTokenResourceAddress">The Bech32m-encoded human readable version of the resource address (required).</param>
        /// <param name="isRegistered">isRegistered (required).</param>
        /// <param name="substateType">substateType (required) (default to SubstateType.Validator).</param>
        public ValidatorSubstate(string epochManagerAddress = default(string), string validatorAddress = default(string), EcdsaSecp256k1PublicKey publicKey = default(EcdsaSecp256k1PublicKey), EntityReference stakeVault = default(EntityReference), EntityReference unstakeVault = default(EntityReference), string liquidStakeUnitResourceAddress = default(string), string unstakeClaimTokenResourceAddress = default(string), bool isRegistered = default(bool), SubstateType substateType = SubstateType.Validator) : base(substateType)
        {
            // to ensure "epochManagerAddress" is required (not null)
            if (epochManagerAddress == null)
            {
                throw new ArgumentNullException("epochManagerAddress is a required property for ValidatorSubstate and cannot be null");
            }
            this.EpochManagerAddress = epochManagerAddress;
            // to ensure "validatorAddress" is required (not null)
            if (validatorAddress == null)
            {
                throw new ArgumentNullException("validatorAddress is a required property for ValidatorSubstate and cannot be null");
            }
            this.ValidatorAddress = validatorAddress;
            // to ensure "publicKey" is required (not null)
            if (publicKey == null)
            {
                throw new ArgumentNullException("publicKey is a required property for ValidatorSubstate and cannot be null");
            }
            this.PublicKey = publicKey;
            // to ensure "stakeVault" is required (not null)
            if (stakeVault == null)
            {
                throw new ArgumentNullException("stakeVault is a required property for ValidatorSubstate and cannot be null");
            }
            this.StakeVault = stakeVault;
            // to ensure "unstakeVault" is required (not null)
            if (unstakeVault == null)
            {
                throw new ArgumentNullException("unstakeVault is a required property for ValidatorSubstate and cannot be null");
            }
            this.UnstakeVault = unstakeVault;
            // to ensure "liquidStakeUnitResourceAddress" is required (not null)
            if (liquidStakeUnitResourceAddress == null)
            {
                throw new ArgumentNullException("liquidStakeUnitResourceAddress is a required property for ValidatorSubstate and cannot be null");
            }
            this.LiquidStakeUnitResourceAddress = liquidStakeUnitResourceAddress;
            // to ensure "unstakeClaimTokenResourceAddress" is required (not null)
            if (unstakeClaimTokenResourceAddress == null)
            {
                throw new ArgumentNullException("unstakeClaimTokenResourceAddress is a required property for ValidatorSubstate and cannot be null");
            }
            this.UnstakeClaimTokenResourceAddress = unstakeClaimTokenResourceAddress;
            this.IsRegistered = isRegistered;
        }

        /// <summary>
        /// The Bech32m-encoded human readable version of the component address
        /// </summary>
        /// <value>The Bech32m-encoded human readable version of the component address</value>
        [DataMember(Name = "epoch_manager_address", IsRequired = true, EmitDefaultValue = true)]
        public string EpochManagerAddress { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable version of the component address
        /// </summary>
        /// <value>The Bech32m-encoded human readable version of the component address</value>
        [DataMember(Name = "validator_address", IsRequired = true, EmitDefaultValue = true)]
        public string ValidatorAddress { get; set; }

        /// <summary>
        /// Gets or Sets PublicKey
        /// </summary>
        [DataMember(Name = "public_key", IsRequired = true, EmitDefaultValue = true)]
        public EcdsaSecp256k1PublicKey PublicKey { get; set; }

        /// <summary>
        /// Gets or Sets StakeVault
        /// </summary>
        [DataMember(Name = "stake_vault", IsRequired = true, EmitDefaultValue = true)]
        public EntityReference StakeVault { get; set; }

        /// <summary>
        /// Gets or Sets UnstakeVault
        /// </summary>
        [DataMember(Name = "unstake_vault", IsRequired = true, EmitDefaultValue = true)]
        public EntityReference UnstakeVault { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable version of the resource address
        /// </summary>
        /// <value>The Bech32m-encoded human readable version of the resource address</value>
        [DataMember(Name = "liquid_stake_unit_resource_address", IsRequired = true, EmitDefaultValue = true)]
        public string LiquidStakeUnitResourceAddress { get; set; }

        /// <summary>
        /// The Bech32m-encoded human readable version of the resource address
        /// </summary>
        /// <value>The Bech32m-encoded human readable version of the resource address</value>
        [DataMember(Name = "unstake_claim_token_resource_address", IsRequired = true, EmitDefaultValue = true)]
        public string UnstakeClaimTokenResourceAddress { get; set; }

        /// <summary>
        /// Gets or Sets IsRegistered
        /// </summary>
        [DataMember(Name = "is_registered", IsRequired = true, EmitDefaultValue = true)]
        public bool IsRegistered { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ValidatorSubstate {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  EpochManagerAddress: ").Append(EpochManagerAddress).Append("\n");
            sb.Append("  ValidatorAddress: ").Append(ValidatorAddress).Append("\n");
            sb.Append("  PublicKey: ").Append(PublicKey).Append("\n");
            sb.Append("  StakeVault: ").Append(StakeVault).Append("\n");
            sb.Append("  UnstakeVault: ").Append(UnstakeVault).Append("\n");
            sb.Append("  LiquidStakeUnitResourceAddress: ").Append(LiquidStakeUnitResourceAddress).Append("\n");
            sb.Append("  UnstakeClaimTokenResourceAddress: ").Append(UnstakeClaimTokenResourceAddress).Append("\n");
            sb.Append("  IsRegistered: ").Append(IsRegistered).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public override string ToJson()
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
            return this.Equals(input as ValidatorSubstate);
        }

        /// <summary>
        /// Returns true if ValidatorSubstate instances are equal
        /// </summary>
        /// <param name="input">Instance of ValidatorSubstate to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ValidatorSubstate input)
        {
            if (input == null)
            {
                return false;
            }
            return base.Equals(input) && 
                (
                    this.EpochManagerAddress == input.EpochManagerAddress ||
                    (this.EpochManagerAddress != null &&
                    this.EpochManagerAddress.Equals(input.EpochManagerAddress))
                ) && base.Equals(input) && 
                (
                    this.ValidatorAddress == input.ValidatorAddress ||
                    (this.ValidatorAddress != null &&
                    this.ValidatorAddress.Equals(input.ValidatorAddress))
                ) && base.Equals(input) && 
                (
                    this.PublicKey == input.PublicKey ||
                    (this.PublicKey != null &&
                    this.PublicKey.Equals(input.PublicKey))
                ) && base.Equals(input) && 
                (
                    this.StakeVault == input.StakeVault ||
                    (this.StakeVault != null &&
                    this.StakeVault.Equals(input.StakeVault))
                ) && base.Equals(input) && 
                (
                    this.UnstakeVault == input.UnstakeVault ||
                    (this.UnstakeVault != null &&
                    this.UnstakeVault.Equals(input.UnstakeVault))
                ) && base.Equals(input) && 
                (
                    this.LiquidStakeUnitResourceAddress == input.LiquidStakeUnitResourceAddress ||
                    (this.LiquidStakeUnitResourceAddress != null &&
                    this.LiquidStakeUnitResourceAddress.Equals(input.LiquidStakeUnitResourceAddress))
                ) && base.Equals(input) && 
                (
                    this.UnstakeClaimTokenResourceAddress == input.UnstakeClaimTokenResourceAddress ||
                    (this.UnstakeClaimTokenResourceAddress != null &&
                    this.UnstakeClaimTokenResourceAddress.Equals(input.UnstakeClaimTokenResourceAddress))
                ) && base.Equals(input) && 
                (
                    this.IsRegistered == input.IsRegistered ||
                    this.IsRegistered.Equals(input.IsRegistered)
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
                int hashCode = base.GetHashCode();
                if (this.EpochManagerAddress != null)
                {
                    hashCode = (hashCode * 59) + this.EpochManagerAddress.GetHashCode();
                }
                if (this.ValidatorAddress != null)
                {
                    hashCode = (hashCode * 59) + this.ValidatorAddress.GetHashCode();
                }
                if (this.PublicKey != null)
                {
                    hashCode = (hashCode * 59) + this.PublicKey.GetHashCode();
                }
                if (this.StakeVault != null)
                {
                    hashCode = (hashCode * 59) + this.StakeVault.GetHashCode();
                }
                if (this.UnstakeVault != null)
                {
                    hashCode = (hashCode * 59) + this.UnstakeVault.GetHashCode();
                }
                if (this.LiquidStakeUnitResourceAddress != null)
                {
                    hashCode = (hashCode * 59) + this.LiquidStakeUnitResourceAddress.GetHashCode();
                }
                if (this.UnstakeClaimTokenResourceAddress != null)
                {
                    hashCode = (hashCode * 59) + this.UnstakeClaimTokenResourceAddress.GetHashCode();
                }
                hashCode = (hashCode * 59) + this.IsRegistered.GetHashCode();
                return hashCode;
            }
        }

    }

}
