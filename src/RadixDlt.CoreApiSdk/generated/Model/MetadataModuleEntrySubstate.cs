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
using JsonSubTypes;
using FileParameter = RadixDlt.CoreApiSdk.Client.FileParameter;
using OpenAPIDateConverter = RadixDlt.CoreApiSdk.Client.OpenAPIDateConverter;

namespace RadixDlt.CoreApiSdk.Model
{
    /// <summary>
    /// MetadataModuleEntrySubstate
    /// </summary>
    [DataContract(Name = "MetadataModuleEntrySubstate")]
    [JsonConverter(typeof(JsonSubtypes), "substate_type")]
    [JsonSubtypes.KnownSubType(typeof(AccessControllerFieldStateSubstate), "AccessControllerFieldState")]
    [JsonSubtypes.KnownSubType(typeof(AccountAuthorizedDepositorEntrySubstate), "AccountAuthorizedDepositorEntry")]
    [JsonSubtypes.KnownSubType(typeof(AccountFieldStateSubstate), "AccountFieldState")]
    [JsonSubtypes.KnownSubType(typeof(AccountLockerAccountClaimsEntrySubstate), "AccountLockerAccountClaimsEntry")]
    [JsonSubtypes.KnownSubType(typeof(AccountResourcePreferenceEntrySubstate), "AccountResourcePreferenceEntry")]
    [JsonSubtypes.KnownSubType(typeof(AccountVaultEntrySubstate), "AccountVaultEntry")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldKernelBootSubstate), "BootLoaderModuleFieldKernelBoot")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldSystemBootSubstate), "BootLoaderModuleFieldSystemBoot")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldTransactionValidationConfigurationSubstate), "BootLoaderModuleFieldTransactionValidationConfiguration")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldVmBootSubstate), "BootLoaderModuleFieldVmBoot")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldConfigSubstate), "ConsensusManagerFieldConfig")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentProposalStatisticSubstate), "ConsensusManagerFieldCurrentProposalStatistic")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentTimeSubstate), "ConsensusManagerFieldCurrentTime")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentTimeRoundedToMinutesSubstate), "ConsensusManagerFieldCurrentTimeRoundedToMinutes")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentValidatorSetSubstate), "ConsensusManagerFieldCurrentValidatorSet")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldStateSubstate), "ConsensusManagerFieldState")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldValidatorRewardsSubstate), "ConsensusManagerFieldValidatorRewards")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerRegisteredValidatorsByStakeIndexEntrySubstate), "ConsensusManagerRegisteredValidatorsByStakeIndexEntry")]
    [JsonSubtypes.KnownSubType(typeof(FungibleResourceManagerFieldDivisibilitySubstate), "FungibleResourceManagerFieldDivisibility")]
    [JsonSubtypes.KnownSubType(typeof(FungibleResourceManagerFieldTotalSupplySubstate), "FungibleResourceManagerFieldTotalSupply")]
    [JsonSubtypes.KnownSubType(typeof(FungibleVaultFieldBalanceSubstate), "FungibleVaultFieldBalance")]
    [JsonSubtypes.KnownSubType(typeof(FungibleVaultFieldFrozenStatusSubstate), "FungibleVaultFieldFrozenStatus")]
    [JsonSubtypes.KnownSubType(typeof(GenericKeyValueStoreEntrySubstate), "GenericKeyValueStoreEntry")]
    [JsonSubtypes.KnownSubType(typeof(GenericScryptoComponentFieldStateSubstate), "GenericScryptoComponentFieldState")]
    [JsonSubtypes.KnownSubType(typeof(MetadataModuleEntrySubstate), "MetadataModuleEntry")]
    [JsonSubtypes.KnownSubType(typeof(MultiResourcePoolFieldStateSubstate), "MultiResourcePoolFieldState")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerDataEntrySubstate), "NonFungibleResourceManagerDataEntry")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerFieldIdTypeSubstate), "NonFungibleResourceManagerFieldIdType")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerFieldMutableFieldsSubstate), "NonFungibleResourceManagerFieldMutableFields")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerFieldTotalSupplySubstate), "NonFungibleResourceManagerFieldTotalSupply")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleVaultContentsIndexEntrySubstate), "NonFungibleVaultContentsIndexEntry")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleVaultFieldBalanceSubstate), "NonFungibleVaultFieldBalance")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleVaultFieldFrozenStatusSubstate), "NonFungibleVaultFieldFrozenStatus")]
    [JsonSubtypes.KnownSubType(typeof(OneResourcePoolFieldStateSubstate), "OneResourcePoolFieldState")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintAuthTemplateEntrySubstate), "PackageBlueprintAuthTemplateEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintDefinitionEntrySubstate), "PackageBlueprintDefinitionEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintDependenciesEntrySubstate), "PackageBlueprintDependenciesEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintRoyaltyEntrySubstate), "PackageBlueprintRoyaltyEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeInstrumentedCodeEntrySubstate), "PackageCodeInstrumentedCodeEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeOriginalCodeEntrySubstate), "PackageCodeOriginalCodeEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeVmTypeEntrySubstate), "PackageCodeVmTypeEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageFieldRoyaltyAccumulatorSubstate), "PackageFieldRoyaltyAccumulator")]
    [JsonSubtypes.KnownSubType(typeof(ProtocolUpdateStatusModuleFieldSummarySubstate), "ProtocolUpdateStatusModuleFieldSummary")]
    [JsonSubtypes.KnownSubType(typeof(RoleAssignmentModuleFieldOwnerRoleSubstate), "RoleAssignmentModuleFieldOwnerRole")]
    [JsonSubtypes.KnownSubType(typeof(RoleAssignmentModuleRuleEntrySubstate), "RoleAssignmentModuleRuleEntry")]
    [JsonSubtypes.KnownSubType(typeof(RoyaltyModuleFieldStateSubstate), "RoyaltyModuleFieldState")]
    [JsonSubtypes.KnownSubType(typeof(RoyaltyModuleMethodRoyaltyEntrySubstate), "RoyaltyModuleMethodRoyaltyEntry")]
    [JsonSubtypes.KnownSubType(typeof(SchemaEntrySubstate), "SchemaEntry")]
    [JsonSubtypes.KnownSubType(typeof(TransactionTrackerCollectionEntrySubstate), "TransactionTrackerCollectionEntry")]
    [JsonSubtypes.KnownSubType(typeof(TransactionTrackerFieldStateSubstate), "TransactionTrackerFieldState")]
    [JsonSubtypes.KnownSubType(typeof(TwoResourcePoolFieldStateSubstate), "TwoResourcePoolFieldState")]
    [JsonSubtypes.KnownSubType(typeof(TypeInfoModuleFieldTypeInfoSubstate), "TypeInfoModuleFieldTypeInfo")]
    [JsonSubtypes.KnownSubType(typeof(ValidatorFieldProtocolUpdateReadinessSignalSubstate), "ValidatorFieldProtocolUpdateReadinessSignal")]
    [JsonSubtypes.KnownSubType(typeof(ValidatorFieldStateSubstate), "ValidatorFieldState")]
    public partial class MetadataModuleEntrySubstate : Substate, IEquatable<MetadataModuleEntrySubstate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataModuleEntrySubstate" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected MetadataModuleEntrySubstate() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataModuleEntrySubstate" /> class.
        /// </summary>
        /// <param name="key">key (required).</param>
        /// <param name="value">value.</param>
        /// <param name="substateType">substateType (required) (default to SubstateType.MetadataModuleEntry).</param>
        /// <param name="isLocked">isLocked (required).</param>
        public MetadataModuleEntrySubstate(MetadataKey key = default(MetadataKey), MetadataModuleEntryValue value = default(MetadataModuleEntryValue), SubstateType substateType = SubstateType.MetadataModuleEntry, bool isLocked = default(bool)) : base(substateType, isLocked)
        {
            // to ensure "key" is required (not null)
            if (key == null)
            {
                throw new ArgumentNullException("key is a required property for MetadataModuleEntrySubstate and cannot be null");
            }
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets or Sets Key
        /// </summary>
        [DataMember(Name = "key", IsRequired = true, EmitDefaultValue = true)]
        public MetadataKey Key { get; set; }

        /// <summary>
        /// Gets or Sets Value
        /// </summary>
        [DataMember(Name = "value", EmitDefaultValue = true)]
        public MetadataModuleEntryValue Value { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class MetadataModuleEntrySubstate {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Key: ").Append(Key).Append("\n");
            sb.Append("  Value: ").Append(Value).Append("\n");
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
            return this.Equals(input as MetadataModuleEntrySubstate);
        }

        /// <summary>
        /// Returns true if MetadataModuleEntrySubstate instances are equal
        /// </summary>
        /// <param name="input">Instance of MetadataModuleEntrySubstate to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(MetadataModuleEntrySubstate input)
        {
            if (input == null)
            {
                return false;
            }
            return base.Equals(input) && 
                (
                    this.Key == input.Key ||
                    (this.Key != null &&
                    this.Key.Equals(input.Key))
                ) && base.Equals(input) && 
                (
                    this.Value == input.Value ||
                    (this.Value != null &&
                    this.Value.Equals(input.Value))
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
                if (this.Key != null)
                {
                    hashCode = (hashCode * 59) + this.Key.GetHashCode();
                }
                if (this.Value != null)
                {
                    hashCode = (hashCode * 59) + this.Value.GetHashCode();
                }
                return hashCode;
            }
        }

    }

}
