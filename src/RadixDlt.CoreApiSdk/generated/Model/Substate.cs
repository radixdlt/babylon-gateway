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
    /// Substate
    /// </summary>
    [DataContract(Name = "Substate")]
    [JsonConverter(typeof(JsonSubtypes), "substate_type")]
    [JsonSubtypes.KnownSubType(typeof(AccessControllerFieldStateSubstate), "AccessControllerFieldState")]
    [JsonSubtypes.KnownSubType(typeof(AccessControllerFieldStateSubstate), "AccessControllerFieldStateSubstate")]
    [JsonSubtypes.KnownSubType(typeof(AccountAuthorizedDepositorEntrySubstate), "AccountAuthorizedDepositorEntry")]
    [JsonSubtypes.KnownSubType(typeof(AccountAuthorizedDepositorEntrySubstate), "AccountAuthorizedDepositorEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(AccountFieldStateSubstate), "AccountFieldState")]
    [JsonSubtypes.KnownSubType(typeof(AccountFieldStateSubstate), "AccountFieldStateSubstate")]
    [JsonSubtypes.KnownSubType(typeof(AccountLockerAccountClaimsEntrySubstate), "AccountLockerAccountClaimsEntry")]
    [JsonSubtypes.KnownSubType(typeof(AccountLockerAccountClaimsEntrySubstate), "AccountLockerAccountClaimsEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(AccountResourcePreferenceEntrySubstate), "AccountResourcePreferenceEntry")]
    [JsonSubtypes.KnownSubType(typeof(AccountResourcePreferenceEntrySubstate), "AccountResourcePreferenceEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(AccountVaultEntrySubstate), "AccountVaultEntry")]
    [JsonSubtypes.KnownSubType(typeof(AccountVaultEntrySubstate), "AccountVaultEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldKernelBootSubstate), "BootLoaderModuleFieldKernelBoot")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldKernelBootSubstate), "BootLoaderModuleFieldKernelBootSubstate")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldSystemBootSubstate), "BootLoaderModuleFieldSystemBoot")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldSystemBootSubstate), "BootLoaderModuleFieldSystemBootSubstate")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldTransactionValidationConfigurationSubstate), "BootLoaderModuleFieldTransactionValidationConfiguration")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldTransactionValidationConfigurationSubstate), "BootLoaderModuleFieldTransactionValidationConfigurationSubstate")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldVmBootSubstate), "BootLoaderModuleFieldVmBoot")]
    [JsonSubtypes.KnownSubType(typeof(BootLoaderModuleFieldVmBootSubstate), "BootLoaderModuleFieldVmBootSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldConfigSubstate), "ConsensusManagerFieldConfig")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldConfigSubstate), "ConsensusManagerFieldConfigSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentProposalStatisticSubstate), "ConsensusManagerFieldCurrentProposalStatistic")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentProposalStatisticSubstate), "ConsensusManagerFieldCurrentProposalStatisticSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentTimeSubstate), "ConsensusManagerFieldCurrentTime")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentTimeRoundedToMinutesSubstate), "ConsensusManagerFieldCurrentTimeRoundedToMinutes")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentTimeRoundedToMinutesSubstate), "ConsensusManagerFieldCurrentTimeRoundedToMinutesSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentTimeSubstate), "ConsensusManagerFieldCurrentTimeSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentValidatorSetSubstate), "ConsensusManagerFieldCurrentValidatorSet")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldCurrentValidatorSetSubstate), "ConsensusManagerFieldCurrentValidatorSetSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldStateSubstate), "ConsensusManagerFieldState")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldStateSubstate), "ConsensusManagerFieldStateSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldValidatorRewardsSubstate), "ConsensusManagerFieldValidatorRewards")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerFieldValidatorRewardsSubstate), "ConsensusManagerFieldValidatorRewardsSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerRegisteredValidatorsByStakeIndexEntrySubstate), "ConsensusManagerRegisteredValidatorsByStakeIndexEntry")]
    [JsonSubtypes.KnownSubType(typeof(ConsensusManagerRegisteredValidatorsByStakeIndexEntrySubstate), "ConsensusManagerRegisteredValidatorsByStakeIndexEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(FungibleResourceManagerFieldDivisibilitySubstate), "FungibleResourceManagerFieldDivisibility")]
    [JsonSubtypes.KnownSubType(typeof(FungibleResourceManagerFieldDivisibilitySubstate), "FungibleResourceManagerFieldDivisibilitySubstate")]
    [JsonSubtypes.KnownSubType(typeof(FungibleResourceManagerFieldTotalSupplySubstate), "FungibleResourceManagerFieldTotalSupply")]
    [JsonSubtypes.KnownSubType(typeof(FungibleResourceManagerFieldTotalSupplySubstate), "FungibleResourceManagerFieldTotalSupplySubstate")]
    [JsonSubtypes.KnownSubType(typeof(FungibleVaultFieldBalanceSubstate), "FungibleVaultFieldBalance")]
    [JsonSubtypes.KnownSubType(typeof(FungibleVaultFieldBalanceSubstate), "FungibleVaultFieldBalanceSubstate")]
    [JsonSubtypes.KnownSubType(typeof(FungibleVaultFieldFrozenStatusSubstate), "FungibleVaultFieldFrozenStatus")]
    [JsonSubtypes.KnownSubType(typeof(FungibleVaultFieldFrozenStatusSubstate), "FungibleVaultFieldFrozenStatusSubstate")]
    [JsonSubtypes.KnownSubType(typeof(GenericKeyValueStoreEntrySubstate), "GenericKeyValueStoreEntry")]
    [JsonSubtypes.KnownSubType(typeof(GenericKeyValueStoreEntrySubstate), "GenericKeyValueStoreEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(GenericScryptoComponentFieldStateSubstate), "GenericScryptoComponentFieldState")]
    [JsonSubtypes.KnownSubType(typeof(GenericScryptoComponentFieldStateSubstate), "GenericScryptoComponentFieldStateSubstate")]
    [JsonSubtypes.KnownSubType(typeof(MetadataModuleEntrySubstate), "MetadataModuleEntry")]
    [JsonSubtypes.KnownSubType(typeof(MetadataModuleEntrySubstate), "MetadataModuleEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(MultiResourcePoolFieldStateSubstate), "MultiResourcePoolFieldState")]
    [JsonSubtypes.KnownSubType(typeof(MultiResourcePoolFieldStateSubstate), "MultiResourcePoolFieldStateSubstate")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerDataEntrySubstate), "NonFungibleResourceManagerDataEntry")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerDataEntrySubstate), "NonFungibleResourceManagerDataEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerFieldIdTypeSubstate), "NonFungibleResourceManagerFieldIdType")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerFieldIdTypeSubstate), "NonFungibleResourceManagerFieldIdTypeSubstate")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerFieldMutableFieldsSubstate), "NonFungibleResourceManagerFieldMutableFields")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerFieldMutableFieldsSubstate), "NonFungibleResourceManagerFieldMutableFieldsSubstate")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerFieldTotalSupplySubstate), "NonFungibleResourceManagerFieldTotalSupply")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleResourceManagerFieldTotalSupplySubstate), "NonFungibleResourceManagerFieldTotalSupplySubstate")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleVaultContentsIndexEntrySubstate), "NonFungibleVaultContentsIndexEntry")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleVaultContentsIndexEntrySubstate), "NonFungibleVaultContentsIndexEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleVaultFieldBalanceSubstate), "NonFungibleVaultFieldBalance")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleVaultFieldBalanceSubstate), "NonFungibleVaultFieldBalanceSubstate")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleVaultFieldFrozenStatusSubstate), "NonFungibleVaultFieldFrozenStatus")]
    [JsonSubtypes.KnownSubType(typeof(NonFungibleVaultFieldFrozenStatusSubstate), "NonFungibleVaultFieldFrozenStatusSubstate")]
    [JsonSubtypes.KnownSubType(typeof(OneResourcePoolFieldStateSubstate), "OneResourcePoolFieldState")]
    [JsonSubtypes.KnownSubType(typeof(OneResourcePoolFieldStateSubstate), "OneResourcePoolFieldStateSubstate")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintAuthTemplateEntrySubstate), "PackageBlueprintAuthTemplateEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintAuthTemplateEntrySubstate), "PackageBlueprintAuthTemplateEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintDefinitionEntrySubstate), "PackageBlueprintDefinitionEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintDefinitionEntrySubstate), "PackageBlueprintDefinitionEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintDependenciesEntrySubstate), "PackageBlueprintDependenciesEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintDependenciesEntrySubstate), "PackageBlueprintDependenciesEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintRoyaltyEntrySubstate), "PackageBlueprintRoyaltyEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageBlueprintRoyaltyEntrySubstate), "PackageBlueprintRoyaltyEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeInstrumentedCodeEntrySubstate), "PackageCodeInstrumentedCodeEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeInstrumentedCodeEntrySubstate), "PackageCodeInstrumentedCodeEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeOriginalCodeEntrySubstate), "PackageCodeOriginalCodeEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeOriginalCodeEntrySubstate), "PackageCodeOriginalCodeEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeVmTypeEntrySubstate), "PackageCodeVmTypeEntry")]
    [JsonSubtypes.KnownSubType(typeof(PackageCodeVmTypeEntrySubstate), "PackageCodeVmTypeEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(PackageFieldRoyaltyAccumulatorSubstate), "PackageFieldRoyaltyAccumulator")]
    [JsonSubtypes.KnownSubType(typeof(PackageFieldRoyaltyAccumulatorSubstate), "PackageFieldRoyaltyAccumulatorSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ProtocolUpdateStatusModuleFieldSummarySubstate), "ProtocolUpdateStatusModuleFieldSummary")]
    [JsonSubtypes.KnownSubType(typeof(ProtocolUpdateStatusModuleFieldSummarySubstate), "ProtocolUpdateStatusModuleFieldSummarySubstate")]
    [JsonSubtypes.KnownSubType(typeof(RoleAssignmentModuleFieldOwnerRoleSubstate), "RoleAssignmentModuleFieldOwnerRole")]
    [JsonSubtypes.KnownSubType(typeof(RoleAssignmentModuleFieldOwnerRoleSubstate), "RoleAssignmentModuleFieldOwnerRoleSubstate")]
    [JsonSubtypes.KnownSubType(typeof(RoleAssignmentModuleRuleEntrySubstate), "RoleAssignmentModuleRuleEntry")]
    [JsonSubtypes.KnownSubType(typeof(RoleAssignmentModuleRuleEntrySubstate), "RoleAssignmentModuleRuleEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(RoyaltyModuleFieldStateSubstate), "RoyaltyModuleFieldState")]
    [JsonSubtypes.KnownSubType(typeof(RoyaltyModuleFieldStateSubstate), "RoyaltyModuleFieldStateSubstate")]
    [JsonSubtypes.KnownSubType(typeof(RoyaltyModuleMethodRoyaltyEntrySubstate), "RoyaltyModuleMethodRoyaltyEntry")]
    [JsonSubtypes.KnownSubType(typeof(RoyaltyModuleMethodRoyaltyEntrySubstate), "RoyaltyModuleMethodRoyaltyEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(SchemaEntrySubstate), "SchemaEntry")]
    [JsonSubtypes.KnownSubType(typeof(SchemaEntrySubstate), "SchemaEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(TransactionTrackerCollectionEntrySubstate), "TransactionTrackerCollectionEntry")]
    [JsonSubtypes.KnownSubType(typeof(TransactionTrackerCollectionEntrySubstate), "TransactionTrackerCollectionEntrySubstate")]
    [JsonSubtypes.KnownSubType(typeof(TransactionTrackerFieldStateSubstate), "TransactionTrackerFieldState")]
    [JsonSubtypes.KnownSubType(typeof(TransactionTrackerFieldStateSubstate), "TransactionTrackerFieldStateSubstate")]
    [JsonSubtypes.KnownSubType(typeof(TwoResourcePoolFieldStateSubstate), "TwoResourcePoolFieldState")]
    [JsonSubtypes.KnownSubType(typeof(TwoResourcePoolFieldStateSubstate), "TwoResourcePoolFieldStateSubstate")]
    [JsonSubtypes.KnownSubType(typeof(TypeInfoModuleFieldTypeInfoSubstate), "TypeInfoModuleFieldTypeInfo")]
    [JsonSubtypes.KnownSubType(typeof(TypeInfoModuleFieldTypeInfoSubstate), "TypeInfoModuleFieldTypeInfoSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ValidatorFieldProtocolUpdateReadinessSignalSubstate), "ValidatorFieldProtocolUpdateReadinessSignal")]
    [JsonSubtypes.KnownSubType(typeof(ValidatorFieldProtocolUpdateReadinessSignalSubstate), "ValidatorFieldProtocolUpdateReadinessSignalSubstate")]
    [JsonSubtypes.KnownSubType(typeof(ValidatorFieldStateSubstate), "ValidatorFieldState")]
    [JsonSubtypes.KnownSubType(typeof(ValidatorFieldStateSubstate), "ValidatorFieldStateSubstate")]
    public partial class Substate : IEquatable<Substate>
    {

        /// <summary>
        /// Gets or Sets SubstateType
        /// </summary>
        [DataMember(Name = "substate_type", IsRequired = true, EmitDefaultValue = true)]
        public SubstateType SubstateType { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected Substate() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Substate" /> class.
        /// </summary>
        /// <param name="substateType">substateType (required).</param>
        /// <param name="isLocked">isLocked (required).</param>
        public Substate(SubstateType substateType = default(SubstateType), bool isLocked = default(bool))
        {
            this.SubstateType = substateType;
            this.IsLocked = isLocked;
        }

        /// <summary>
        /// Gets or Sets IsLocked
        /// </summary>
        [DataMember(Name = "is_locked", IsRequired = true, EmitDefaultValue = true)]
        public bool IsLocked { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Substate {\n");
            sb.Append("  SubstateType: ").Append(SubstateType).Append("\n");
            sb.Append("  IsLocked: ").Append(IsLocked).Append("\n");
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
            {
                return false;
            }
            return 
                (
                    this.SubstateType == input.SubstateType ||
                    this.SubstateType.Equals(input.SubstateType)
                ) && 
                (
                    this.IsLocked == input.IsLocked ||
                    this.IsLocked.Equals(input.IsLocked)
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
                hashCode = (hashCode * 59) + this.SubstateType.GetHashCode();
                hashCode = (hashCode * 59) + this.IsLocked.GetHashCode();
                return hashCode;
            }
        }

    }

}
