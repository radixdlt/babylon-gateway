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
    /// Defines SubstateType
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubstateType
    {
        /// <summary>
        /// Enum BootLoaderModuleFieldSystemBoot for value: BootLoaderModuleFieldSystemBoot
        /// </summary>
        [EnumMember(Value = "BootLoaderModuleFieldSystemBoot")]
        BootLoaderModuleFieldSystemBoot = 1,

        /// <summary>
        /// Enum BootLoaderModuleFieldKernelBoot for value: BootLoaderModuleFieldKernelBoot
        /// </summary>
        [EnumMember(Value = "BootLoaderModuleFieldKernelBoot")]
        BootLoaderModuleFieldKernelBoot = 2,

        /// <summary>
        /// Enum BootLoaderModuleFieldVmBoot for value: BootLoaderModuleFieldVmBoot
        /// </summary>
        [EnumMember(Value = "BootLoaderModuleFieldVmBoot")]
        BootLoaderModuleFieldVmBoot = 3,

        /// <summary>
        /// Enum BootLoaderModuleFieldTransactionValidationConfiguration for value: BootLoaderModuleFieldTransactionValidationConfiguration
        /// </summary>
        [EnumMember(Value = "BootLoaderModuleFieldTransactionValidationConfiguration")]
        BootLoaderModuleFieldTransactionValidationConfiguration = 4,

        /// <summary>
        /// Enum ProtocolUpdateStatusModuleFieldSummary for value: ProtocolUpdateStatusModuleFieldSummary
        /// </summary>
        [EnumMember(Value = "ProtocolUpdateStatusModuleFieldSummary")]
        ProtocolUpdateStatusModuleFieldSummary = 5,

        /// <summary>
        /// Enum TypeInfoModuleFieldTypeInfo for value: TypeInfoModuleFieldTypeInfo
        /// </summary>
        [EnumMember(Value = "TypeInfoModuleFieldTypeInfo")]
        TypeInfoModuleFieldTypeInfo = 6,

        /// <summary>
        /// Enum RoleAssignmentModuleFieldOwnerRole for value: RoleAssignmentModuleFieldOwnerRole
        /// </summary>
        [EnumMember(Value = "RoleAssignmentModuleFieldOwnerRole")]
        RoleAssignmentModuleFieldOwnerRole = 7,

        /// <summary>
        /// Enum RoleAssignmentModuleRuleEntry for value: RoleAssignmentModuleRuleEntry
        /// </summary>
        [EnumMember(Value = "RoleAssignmentModuleRuleEntry")]
        RoleAssignmentModuleRuleEntry = 8,

        /// <summary>
        /// Enum RoleAssignmentModuleMutabilityEntry for value: RoleAssignmentModuleMutabilityEntry
        /// </summary>
        [EnumMember(Value = "RoleAssignmentModuleMutabilityEntry")]
        RoleAssignmentModuleMutabilityEntry = 9,

        /// <summary>
        /// Enum RoyaltyModuleFieldState for value: RoyaltyModuleFieldState
        /// </summary>
        [EnumMember(Value = "RoyaltyModuleFieldState")]
        RoyaltyModuleFieldState = 10,

        /// <summary>
        /// Enum RoyaltyModuleMethodRoyaltyEntry for value: RoyaltyModuleMethodRoyaltyEntry
        /// </summary>
        [EnumMember(Value = "RoyaltyModuleMethodRoyaltyEntry")]
        RoyaltyModuleMethodRoyaltyEntry = 11,

        /// <summary>
        /// Enum MetadataModuleEntry for value: MetadataModuleEntry
        /// </summary>
        [EnumMember(Value = "MetadataModuleEntry")]
        MetadataModuleEntry = 12,

        /// <summary>
        /// Enum PackageFieldRoyaltyAccumulator for value: PackageFieldRoyaltyAccumulator
        /// </summary>
        [EnumMember(Value = "PackageFieldRoyaltyAccumulator")]
        PackageFieldRoyaltyAccumulator = 13,

        /// <summary>
        /// Enum PackageCodeVmTypeEntry for value: PackageCodeVmTypeEntry
        /// </summary>
        [EnumMember(Value = "PackageCodeVmTypeEntry")]
        PackageCodeVmTypeEntry = 14,

        /// <summary>
        /// Enum PackageCodeOriginalCodeEntry for value: PackageCodeOriginalCodeEntry
        /// </summary>
        [EnumMember(Value = "PackageCodeOriginalCodeEntry")]
        PackageCodeOriginalCodeEntry = 15,

        /// <summary>
        /// Enum PackageCodeInstrumentedCodeEntry for value: PackageCodeInstrumentedCodeEntry
        /// </summary>
        [EnumMember(Value = "PackageCodeInstrumentedCodeEntry")]
        PackageCodeInstrumentedCodeEntry = 16,

        /// <summary>
        /// Enum SchemaEntry for value: SchemaEntry
        /// </summary>
        [EnumMember(Value = "SchemaEntry")]
        SchemaEntry = 17,

        /// <summary>
        /// Enum PackageBlueprintDefinitionEntry for value: PackageBlueprintDefinitionEntry
        /// </summary>
        [EnumMember(Value = "PackageBlueprintDefinitionEntry")]
        PackageBlueprintDefinitionEntry = 18,

        /// <summary>
        /// Enum PackageBlueprintDependenciesEntry for value: PackageBlueprintDependenciesEntry
        /// </summary>
        [EnumMember(Value = "PackageBlueprintDependenciesEntry")]
        PackageBlueprintDependenciesEntry = 19,

        /// <summary>
        /// Enum PackageBlueprintRoyaltyEntry for value: PackageBlueprintRoyaltyEntry
        /// </summary>
        [EnumMember(Value = "PackageBlueprintRoyaltyEntry")]
        PackageBlueprintRoyaltyEntry = 20,

        /// <summary>
        /// Enum PackageBlueprintAuthTemplateEntry for value: PackageBlueprintAuthTemplateEntry
        /// </summary>
        [EnumMember(Value = "PackageBlueprintAuthTemplateEntry")]
        PackageBlueprintAuthTemplateEntry = 21,

        /// <summary>
        /// Enum PackageFieldFunctionAccessRules for value: PackageFieldFunctionAccessRules
        /// </summary>
        [EnumMember(Value = "PackageFieldFunctionAccessRules")]
        PackageFieldFunctionAccessRules = 22,

        /// <summary>
        /// Enum FungibleResourceManagerFieldDivisibility for value: FungibleResourceManagerFieldDivisibility
        /// </summary>
        [EnumMember(Value = "FungibleResourceManagerFieldDivisibility")]
        FungibleResourceManagerFieldDivisibility = 23,

        /// <summary>
        /// Enum FungibleResourceManagerFieldTotalSupply for value: FungibleResourceManagerFieldTotalSupply
        /// </summary>
        [EnumMember(Value = "FungibleResourceManagerFieldTotalSupply")]
        FungibleResourceManagerFieldTotalSupply = 24,

        /// <summary>
        /// Enum NonFungibleResourceManagerFieldIdType for value: NonFungibleResourceManagerFieldIdType
        /// </summary>
        [EnumMember(Value = "NonFungibleResourceManagerFieldIdType")]
        NonFungibleResourceManagerFieldIdType = 25,

        /// <summary>
        /// Enum NonFungibleResourceManagerFieldTotalSupply for value: NonFungibleResourceManagerFieldTotalSupply
        /// </summary>
        [EnumMember(Value = "NonFungibleResourceManagerFieldTotalSupply")]
        NonFungibleResourceManagerFieldTotalSupply = 26,

        /// <summary>
        /// Enum NonFungibleResourceManagerFieldMutableFields for value: NonFungibleResourceManagerFieldMutableFields
        /// </summary>
        [EnumMember(Value = "NonFungibleResourceManagerFieldMutableFields")]
        NonFungibleResourceManagerFieldMutableFields = 27,

        /// <summary>
        /// Enum NonFungibleResourceManagerDataEntry for value: NonFungibleResourceManagerDataEntry
        /// </summary>
        [EnumMember(Value = "NonFungibleResourceManagerDataEntry")]
        NonFungibleResourceManagerDataEntry = 28,

        /// <summary>
        /// Enum FungibleVaultFieldBalance for value: FungibleVaultFieldBalance
        /// </summary>
        [EnumMember(Value = "FungibleVaultFieldBalance")]
        FungibleVaultFieldBalance = 29,

        /// <summary>
        /// Enum FungibleVaultFieldFrozenStatus for value: FungibleVaultFieldFrozenStatus
        /// </summary>
        [EnumMember(Value = "FungibleVaultFieldFrozenStatus")]
        FungibleVaultFieldFrozenStatus = 30,

        /// <summary>
        /// Enum NonFungibleVaultFieldBalance for value: NonFungibleVaultFieldBalance
        /// </summary>
        [EnumMember(Value = "NonFungibleVaultFieldBalance")]
        NonFungibleVaultFieldBalance = 31,

        /// <summary>
        /// Enum NonFungibleVaultFieldFrozenStatus for value: NonFungibleVaultFieldFrozenStatus
        /// </summary>
        [EnumMember(Value = "NonFungibleVaultFieldFrozenStatus")]
        NonFungibleVaultFieldFrozenStatus = 32,

        /// <summary>
        /// Enum NonFungibleVaultContentsIndexEntry for value: NonFungibleVaultContentsIndexEntry
        /// </summary>
        [EnumMember(Value = "NonFungibleVaultContentsIndexEntry")]
        NonFungibleVaultContentsIndexEntry = 33,

        /// <summary>
        /// Enum ConsensusManagerFieldConfig for value: ConsensusManagerFieldConfig
        /// </summary>
        [EnumMember(Value = "ConsensusManagerFieldConfig")]
        ConsensusManagerFieldConfig = 34,

        /// <summary>
        /// Enum ConsensusManagerFieldState for value: ConsensusManagerFieldState
        /// </summary>
        [EnumMember(Value = "ConsensusManagerFieldState")]
        ConsensusManagerFieldState = 35,

        /// <summary>
        /// Enum ConsensusManagerFieldCurrentValidatorSet for value: ConsensusManagerFieldCurrentValidatorSet
        /// </summary>
        [EnumMember(Value = "ConsensusManagerFieldCurrentValidatorSet")]
        ConsensusManagerFieldCurrentValidatorSet = 36,

        /// <summary>
        /// Enum ConsensusManagerFieldCurrentProposalStatistic for value: ConsensusManagerFieldCurrentProposalStatistic
        /// </summary>
        [EnumMember(Value = "ConsensusManagerFieldCurrentProposalStatistic")]
        ConsensusManagerFieldCurrentProposalStatistic = 37,

        /// <summary>
        /// Enum ConsensusManagerFieldCurrentTimeRoundedToMinutes for value: ConsensusManagerFieldCurrentTimeRoundedToMinutes
        /// </summary>
        [EnumMember(Value = "ConsensusManagerFieldCurrentTimeRoundedToMinutes")]
        ConsensusManagerFieldCurrentTimeRoundedToMinutes = 38,

        /// <summary>
        /// Enum ConsensusManagerFieldCurrentTime for value: ConsensusManagerFieldCurrentTime
        /// </summary>
        [EnumMember(Value = "ConsensusManagerFieldCurrentTime")]
        ConsensusManagerFieldCurrentTime = 39,

        /// <summary>
        /// Enum ConsensusManagerFieldValidatorRewards for value: ConsensusManagerFieldValidatorRewards
        /// </summary>
        [EnumMember(Value = "ConsensusManagerFieldValidatorRewards")]
        ConsensusManagerFieldValidatorRewards = 40,

        /// <summary>
        /// Enum ConsensusManagerRegisteredValidatorsByStakeIndexEntry for value: ConsensusManagerRegisteredValidatorsByStakeIndexEntry
        /// </summary>
        [EnumMember(Value = "ConsensusManagerRegisteredValidatorsByStakeIndexEntry")]
        ConsensusManagerRegisteredValidatorsByStakeIndexEntry = 41,

        /// <summary>
        /// Enum ValidatorFieldState for value: ValidatorFieldState
        /// </summary>
        [EnumMember(Value = "ValidatorFieldState")]
        ValidatorFieldState = 42,

        /// <summary>
        /// Enum ValidatorFieldProtocolUpdateReadinessSignal for value: ValidatorFieldProtocolUpdateReadinessSignal
        /// </summary>
        [EnumMember(Value = "ValidatorFieldProtocolUpdateReadinessSignal")]
        ValidatorFieldProtocolUpdateReadinessSignal = 43,

        /// <summary>
        /// Enum AccountFieldState for value: AccountFieldState
        /// </summary>
        [EnumMember(Value = "AccountFieldState")]
        AccountFieldState = 44,

        /// <summary>
        /// Enum AccountVaultEntry for value: AccountVaultEntry
        /// </summary>
        [EnumMember(Value = "AccountVaultEntry")]
        AccountVaultEntry = 45,

        /// <summary>
        /// Enum AccountResourcePreferenceEntry for value: AccountResourcePreferenceEntry
        /// </summary>
        [EnumMember(Value = "AccountResourcePreferenceEntry")]
        AccountResourcePreferenceEntry = 46,

        /// <summary>
        /// Enum AccountAuthorizedDepositorEntry for value: AccountAuthorizedDepositorEntry
        /// </summary>
        [EnumMember(Value = "AccountAuthorizedDepositorEntry")]
        AccountAuthorizedDepositorEntry = 47,

        /// <summary>
        /// Enum AccessControllerFieldState for value: AccessControllerFieldState
        /// </summary>
        [EnumMember(Value = "AccessControllerFieldState")]
        AccessControllerFieldState = 48,

        /// <summary>
        /// Enum GenericScryptoComponentFieldState for value: GenericScryptoComponentFieldState
        /// </summary>
        [EnumMember(Value = "GenericScryptoComponentFieldState")]
        GenericScryptoComponentFieldState = 49,

        /// <summary>
        /// Enum GenericKeyValueStoreEntry for value: GenericKeyValueStoreEntry
        /// </summary>
        [EnumMember(Value = "GenericKeyValueStoreEntry")]
        GenericKeyValueStoreEntry = 50,

        /// <summary>
        /// Enum OneResourcePoolFieldState for value: OneResourcePoolFieldState
        /// </summary>
        [EnumMember(Value = "OneResourcePoolFieldState")]
        OneResourcePoolFieldState = 51,

        /// <summary>
        /// Enum TwoResourcePoolFieldState for value: TwoResourcePoolFieldState
        /// </summary>
        [EnumMember(Value = "TwoResourcePoolFieldState")]
        TwoResourcePoolFieldState = 52,

        /// <summary>
        /// Enum MultiResourcePoolFieldState for value: MultiResourcePoolFieldState
        /// </summary>
        [EnumMember(Value = "MultiResourcePoolFieldState")]
        MultiResourcePoolFieldState = 53,

        /// <summary>
        /// Enum TransactionTrackerFieldState for value: TransactionTrackerFieldState
        /// </summary>
        [EnumMember(Value = "TransactionTrackerFieldState")]
        TransactionTrackerFieldState = 54,

        /// <summary>
        /// Enum TransactionTrackerCollectionEntry for value: TransactionTrackerCollectionEntry
        /// </summary>
        [EnumMember(Value = "TransactionTrackerCollectionEntry")]
        TransactionTrackerCollectionEntry = 55,

        /// <summary>
        /// Enum AccountLockerAccountClaimsEntry for value: AccountLockerAccountClaimsEntry
        /// </summary>
        [EnumMember(Value = "AccountLockerAccountClaimsEntry")]
        AccountLockerAccountClaimsEntry = 56

    }

}
