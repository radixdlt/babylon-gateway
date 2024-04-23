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
 * Radix Core API - Babylon
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.0.4
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using RadixDlt.CoreApiSdk.GenericHost.Api;
using RadixDlt.CoreApiSdk.GenericHost.Model;

namespace RadixDlt.CoreApiSdk.GenericHost.Client
{
    /// <summary>
    /// Provides hosting configuration for RadixDlt.CoreApiSdk.GenericHost
    /// </summary>
    public class HostConfiguration
    {
        private readonly IServiceCollection _services;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

        internal bool HttpClientsAdded { get; private set; }

        /// <summary>
        /// Instantiates the class 
        /// </summary>
        /// <param name="services"></param>
        public HostConfiguration(IServiceCollection services)
        {
            _services = services;
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonOptions.Converters.Add(new DateTimeJsonConverter());
            _jsonOptions.Converters.Add(new DateTimeNullableJsonConverter());
            _jsonOptions.Converters.Add(new DateOnlyJsonConverter());
            _jsonOptions.Converters.Add(new DateOnlyNullableJsonConverter());
            _jsonOptions.Converters.Add(new AccessControllerFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new AccessControllerFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new AccessRuleJsonConverter());
            _jsonOptions.Converters.Add(new AccessRuleNodeJsonConverter());
            _jsonOptions.Converters.Add(new AccessRuleNodeTypeJsonConverter());
            _jsonOptions.Converters.Add(new AccessRuleNodeTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new AccessRuleTypeJsonConverter());
            _jsonOptions.Converters.Add(new AccessRuleTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new AccountAuthorizedDepositorEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new AccountAuthorizedDepositorEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new AccountFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new AccountFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new AccountResourcePreferenceEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new AccountResourcePreferenceEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new AccountVaultEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new AccountVaultEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new ActiveValidatorJsonConverter());
            _jsonOptions.Converters.Add(new ActiveValidatorIndexJsonConverter());
            _jsonOptions.Converters.Add(new ActiveValidatorKeyJsonConverter());
            _jsonOptions.Converters.Add(new AddressTypeJsonConverter());
            _jsonOptions.Converters.Add(new AllOfAccessRuleNodeJsonConverter());
            _jsonOptions.Converters.Add(new AllOfProofRuleJsonConverter());
            _jsonOptions.Converters.Add(new AllowAllAccessRuleJsonConverter());
            _jsonOptions.Converters.Add(new AmountOfProofRuleJsonConverter());
            _jsonOptions.Converters.Add(new AnyOfAccessRuleNodeJsonConverter());
            _jsonOptions.Converters.Add(new AnyOfProofRuleJsonConverter());
            _jsonOptions.Converters.Add(new AttachedModuleIdJsonConverter());
            _jsonOptions.Converters.Add(new AttachedModuleIdNullableJsonConverter());
            _jsonOptions.Converters.Add(new AuthConfigJsonConverter());
            _jsonOptions.Converters.Add(new AuthorizedDepositorBadgeJsonConverter());
            _jsonOptions.Converters.Add(new AuthorizedDepositorBadgeTypeJsonConverter());
            _jsonOptions.Converters.Add(new AuthorizedDepositorBadgeTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new AuthorizedDepositorKeyJsonConverter());
            _jsonOptions.Converters.Add(new BasicErrorResponseJsonConverter());
            _jsonOptions.Converters.Add(new BinaryPlaintextMessageContentJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintCollectionSchemaJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintCollectionSchemaTypeJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintCollectionSchemaTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintDefinitionJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintDependenciesJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintFunctionTargetIdentifierJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintInfoJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintInterfaceJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintMethodRoyaltyJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintPayloadDefJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintPayloadDefTypeJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintPayloadDefTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintRoyaltyConfigJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintSchemaBlueprintTypeReferenceJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintSchemaCollectionPartitionJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintSchemaFieldPartitionJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintTypeIdentifierJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintTypeReferenceJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintTypeReferenceKindJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintTypeReferenceKindNullableJsonConverter());
            _jsonOptions.Converters.Add(new BlueprintVersionKeyJsonConverter());
            _jsonOptions.Converters.Add(new BootLoaderModuleFieldVmBootSubstateJsonConverter());
            _jsonOptions.Converters.Add(new BootLoaderModuleFieldVmBootValueJsonConverter());
            _jsonOptions.Converters.Add(new CommittedIntentMetadataJsonConverter());
            _jsonOptions.Converters.Add(new CommittedStateIdentifierJsonConverter());
            _jsonOptions.Converters.Add(new CommittedTransactionJsonConverter());
            _jsonOptions.Converters.Add(new CommittedTransactionBalanceChangesJsonConverter());
            _jsonOptions.Converters.Add(new ComponentMethodTargetIdentifierJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusLedgerProofOriginJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldConfigSubstateJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldConfigValueJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldCurrentProposalStatisticSubstateJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldCurrentProposalStatisticValueJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldCurrentTimeRoundedToMinutesSubstateJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldCurrentTimeRoundedToMinutesValueJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldCurrentTimeSubstateJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldCurrentTimeValueJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldCurrentValidatorSetSubstateJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldCurrentValidatorSetValueJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldValidatorRewardsSubstateJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerFieldValidatorRewardsValueJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerRegisteredValidatorsByStakeIndexEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new ConsensusManagerRegisteredValidatorsByStakeIndexEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new CostingParametersJsonConverter());
            _jsonOptions.Converters.Add(new CountOfProofRuleJsonConverter());
            _jsonOptions.Converters.Add(new CreatedSubstateJsonConverter());
            _jsonOptions.Converters.Add(new DataStructJsonConverter());
            _jsonOptions.Converters.Add(new DefaultDepositRuleJsonConverter());
            _jsonOptions.Converters.Add(new DefaultDepositRuleNullableJsonConverter());
            _jsonOptions.Converters.Add(new DeletedSubstateJsonConverter());
            _jsonOptions.Converters.Add(new DenyAllAccessRuleJsonConverter());
            _jsonOptions.Converters.Add(new EcdsaSecp256k1PublicKeyJsonConverter());
            _jsonOptions.Converters.Add(new EcdsaSecp256k1SignatureJsonConverter());
            _jsonOptions.Converters.Add(new EcdsaSecp256k1SignatureWithPublicKeyJsonConverter());
            _jsonOptions.Converters.Add(new EddsaEd25519PublicKeyJsonConverter());
            _jsonOptions.Converters.Add(new EddsaEd25519SignatureJsonConverter());
            _jsonOptions.Converters.Add(new EddsaEd25519SignatureWithPublicKeyJsonConverter());
            _jsonOptions.Converters.Add(new EncryptedMessageCurveDecryptorSetJsonConverter());
            _jsonOptions.Converters.Add(new EncryptedMessageDecryptorJsonConverter());
            _jsonOptions.Converters.Add(new EncryptedTransactionMessageJsonConverter());
            _jsonOptions.Converters.Add(new EntityModuleJsonConverter());
            _jsonOptions.Converters.Add(new EntityModuleNullableJsonConverter());
            _jsonOptions.Converters.Add(new EntityReferenceJsonConverter());
            _jsonOptions.Converters.Add(new EntityTypeJsonConverter());
            _jsonOptions.Converters.Add(new EntityTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new EpochChangeConditionJsonConverter());
            _jsonOptions.Converters.Add(new EpochRoundJsonConverter());
            _jsonOptions.Converters.Add(new ErrorResponseJsonConverter());
            _jsonOptions.Converters.Add(new ErrorResponseTypeJsonConverter());
            _jsonOptions.Converters.Add(new ErrorResponseTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new EventJsonConverter());
            _jsonOptions.Converters.Add(new EventEmitterIdentifierJsonConverter());
            _jsonOptions.Converters.Add(new EventEmitterIdentifierTypeJsonConverter());
            _jsonOptions.Converters.Add(new EventEmitterIdentifierTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new EventTypeIdentifierJsonConverter());
            _jsonOptions.Converters.Add(new ExecutedGenesisScenarioJsonConverter());
            _jsonOptions.Converters.Add(new ExecutedScenarioTransactionJsonConverter());
            _jsonOptions.Converters.Add(new FeeDestinationJsonConverter());
            _jsonOptions.Converters.Add(new FeeSourceJsonConverter());
            _jsonOptions.Converters.Add(new FeeSummaryJsonConverter());
            _jsonOptions.Converters.Add(new FieldSchemaJsonConverter());
            _jsonOptions.Converters.Add(new FieldSchemaFeatureConditionJsonConverter());
            _jsonOptions.Converters.Add(new FieldSchemaFeatureConditionAlwaysJsonConverter());
            _jsonOptions.Converters.Add(new FieldSchemaFeatureConditionIfOuterObjectFeatureJsonConverter());
            _jsonOptions.Converters.Add(new FieldSchemaFeatureConditionIfOwnFeatureJsonConverter());
            _jsonOptions.Converters.Add(new FieldSchemaFeatureConditionTypeJsonConverter());
            _jsonOptions.Converters.Add(new FieldSchemaFeatureConditionTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new FieldSubstateKeyJsonConverter());
            _jsonOptions.Converters.Add(new FlashLedgerTransactionJsonConverter());
            _jsonOptions.Converters.Add(new FlashSetSubstateJsonConverter());
            _jsonOptions.Converters.Add(new FlashedStateUpdatesJsonConverter());
            _jsonOptions.Converters.Add(new FrozenStatusJsonConverter());
            _jsonOptions.Converters.Add(new FullyScopedTypeIdJsonConverter());
            _jsonOptions.Converters.Add(new FunctionAuthTypeJsonConverter());
            _jsonOptions.Converters.Add(new FunctionAuthTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new FunctionEventEmitterIdentifierJsonConverter());
            _jsonOptions.Converters.Add(new FunctionSchemaJsonConverter());
            _jsonOptions.Converters.Add(new FungibleResourceAmountJsonConverter());
            _jsonOptions.Converters.Add(new FungibleResourceManagerFieldDivisibilitySubstateJsonConverter());
            _jsonOptions.Converters.Add(new FungibleResourceManagerFieldDivisibilityValueJsonConverter());
            _jsonOptions.Converters.Add(new FungibleResourceManagerFieldTotalSupplySubstateJsonConverter());
            _jsonOptions.Converters.Add(new FungibleResourceManagerFieldTotalSupplyValueJsonConverter());
            _jsonOptions.Converters.Add(new FungibleVaultFieldBalanceSubstateJsonConverter());
            _jsonOptions.Converters.Add(new FungibleVaultFieldBalanceValueJsonConverter());
            _jsonOptions.Converters.Add(new FungibleVaultFieldFrozenStatusSubstateJsonConverter());
            _jsonOptions.Converters.Add(new FungibleVaultFieldFrozenStatusValueJsonConverter());
            _jsonOptions.Converters.Add(new GenericBlueprintPayloadDefJsonConverter());
            _jsonOptions.Converters.Add(new GenericKeyJsonConverter());
            _jsonOptions.Converters.Add(new GenericKeyValueStoreEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new GenericKeyValueStoreEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new GenericScryptoComponentFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new GenericScryptoComponentFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new GenericSubstitutionJsonConverter());
            _jsonOptions.Converters.Add(new GenericSubstitutionTypeJsonConverter());
            _jsonOptions.Converters.Add(new GenericSubstitutionTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new GenericTypeParameterJsonConverter());
            _jsonOptions.Converters.Add(new GenericTypeParameterConstraintsJsonConverter());
            _jsonOptions.Converters.Add(new GenericTypeParameterConstraintsNullableJsonConverter());
            _jsonOptions.Converters.Add(new GenesisLedgerProofOriginJsonConverter());
            _jsonOptions.Converters.Add(new GenesisLedgerTransactionJsonConverter());
            _jsonOptions.Converters.Add(new HookExportJsonConverter());
            _jsonOptions.Converters.Add(new IndexBlueprintCollectionSchemaJsonConverter());
            _jsonOptions.Converters.Add(new IndexedStateSchemaJsonConverter());
            _jsonOptions.Converters.Add(new InstanceSchemaBlueprintTypeReferenceJsonConverter());
            _jsonOptions.Converters.Add(new InstantJsonConverter());
            _jsonOptions.Converters.Add(new InstructionResourceChangesJsonConverter());
            _jsonOptions.Converters.Add(new KeyValueBasedStructureJsonConverter());
            _jsonOptions.Converters.Add(new KeyValueBlueprintCollectionSchemaJsonConverter());
            _jsonOptions.Converters.Add(new KeyValueStoreEntryStructureJsonConverter());
            _jsonOptions.Converters.Add(new KeyValueStoreInfoJsonConverter());
            _jsonOptions.Converters.Add(new KeyValueStoreTypeInfoDetailsJsonConverter());
            _jsonOptions.Converters.Add(new LeaderProposalHistoryJsonConverter());
            _jsonOptions.Converters.Add(new LedgerHashesJsonConverter());
            _jsonOptions.Converters.Add(new LedgerHeaderJsonConverter());
            _jsonOptions.Converters.Add(new LedgerHeaderSummaryJsonConverter());
            _jsonOptions.Converters.Add(new LedgerProofJsonConverter());
            _jsonOptions.Converters.Add(new LedgerProofOriginJsonConverter());
            _jsonOptions.Converters.Add(new LedgerProofOriginTypeJsonConverter());
            _jsonOptions.Converters.Add(new LedgerProofOriginTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new LedgerStateSummaryJsonConverter());
            _jsonOptions.Converters.Add(new LedgerTransactionJsonConverter());
            _jsonOptions.Converters.Add(new LedgerTransactionTypeJsonConverter());
            _jsonOptions.Converters.Add(new LedgerTransactionTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new LocalGenericSubstitutionJsonConverter());
            _jsonOptions.Converters.Add(new LocalNonFungibleKeyJsonConverter());
            _jsonOptions.Converters.Add(new LocalTypeIdJsonConverter());
            _jsonOptions.Converters.Add(new LtsCommittedTransactionOutcomeJsonConverter());
            _jsonOptions.Converters.Add(new LtsCommittedTransactionStatusJsonConverter());
            _jsonOptions.Converters.Add(new LtsCommittedTransactionStatusNullableJsonConverter());
            _jsonOptions.Converters.Add(new LtsEntityFungibleBalanceChangesJsonConverter());
            _jsonOptions.Converters.Add(new LtsEntityNonFungibleBalanceChangesJsonConverter());
            _jsonOptions.Converters.Add(new LtsFeeFungibleResourceBalanceChangeJsonConverter());
            _jsonOptions.Converters.Add(new LtsFeeFungibleResourceBalanceChangeTypeJsonConverter());
            _jsonOptions.Converters.Add(new LtsFeeFungibleResourceBalanceChangeTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new LtsFungibleResourceBalanceJsonConverter());
            _jsonOptions.Converters.Add(new LtsFungibleResourceBalanceChangeJsonConverter());
            _jsonOptions.Converters.Add(new LtsResultantAccountFungibleBalancesJsonConverter());
            _jsonOptions.Converters.Add(new LtsResultantFungibleBalanceJsonConverter());
            _jsonOptions.Converters.Add(new LtsStateAccountAllFungibleResourceBalancesRequestJsonConverter());
            _jsonOptions.Converters.Add(new LtsStateAccountAllFungibleResourceBalancesResponseJsonConverter());
            _jsonOptions.Converters.Add(new LtsStateAccountDepositBehaviourRequestJsonConverter());
            _jsonOptions.Converters.Add(new LtsStateAccountDepositBehaviourResponseJsonConverter());
            _jsonOptions.Converters.Add(new LtsStateAccountFungibleResourceBalanceRequestJsonConverter());
            _jsonOptions.Converters.Add(new LtsStateAccountFungibleResourceBalanceResponseJsonConverter());
            _jsonOptions.Converters.Add(new LtsStreamAccountTransactionOutcomesRequestJsonConverter());
            _jsonOptions.Converters.Add(new LtsStreamAccountTransactionOutcomesResponseJsonConverter());
            _jsonOptions.Converters.Add(new LtsStreamTransactionOutcomesRequestJsonConverter());
            _jsonOptions.Converters.Add(new LtsStreamTransactionOutcomesResponseJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionConstructionRequestJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionConstructionResponseJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionIntentStatusJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionIntentStatusNullableJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionPayloadDetailsJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionPayloadStatusJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionPayloadStatusNullableJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionStatusRequestJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionStatusResponseJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionSubmitErrorDetailsJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionSubmitErrorDetailsTypeJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionSubmitErrorDetailsTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionSubmitErrorResponseJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionSubmitIntentAlreadyCommittedJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionSubmitPriorityThresholdNotMetErrorDetailsJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionSubmitRejectedErrorDetailsJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionSubmitRequestJsonConverter());
            _jsonOptions.Converters.Add(new LtsTransactionSubmitResponseJsonConverter());
            _jsonOptions.Converters.Add(new MainMethodKeyJsonConverter());
            _jsonOptions.Converters.Add(new MapSubstateKeyJsonConverter());
            _jsonOptions.Converters.Add(new MempoolListRequestJsonConverter());
            _jsonOptions.Converters.Add(new MempoolListResponseJsonConverter());
            _jsonOptions.Converters.Add(new MempoolTransactionHashesJsonConverter());
            _jsonOptions.Converters.Add(new MempoolTransactionRequestJsonConverter());
            _jsonOptions.Converters.Add(new MempoolTransactionResponseJsonConverter());
            _jsonOptions.Converters.Add(new MempoolTransactionResponsePayloadsInnerJsonConverter());
            _jsonOptions.Converters.Add(new MetadataKeyJsonConverter());
            _jsonOptions.Converters.Add(new MetadataModuleEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new MetadataModuleEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new MethodAccessibilityJsonConverter());
            _jsonOptions.Converters.Add(new MethodAccessibilityTypeJsonConverter());
            _jsonOptions.Converters.Add(new MethodAccessibilityTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new MethodAuthTypeJsonConverter());
            _jsonOptions.Converters.Add(new MethodAuthTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new MethodEventEmitterIdentifierJsonConverter());
            _jsonOptions.Converters.Add(new ModuleIdJsonConverter());
            _jsonOptions.Converters.Add(new ModuleIdNullableJsonConverter());
            _jsonOptions.Converters.Add(new ModuleVersionJsonConverter());
            _jsonOptions.Converters.Add(new MultiResourcePoolFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new MultiResourcePoolFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new MutableFieldJsonConverter());
            _jsonOptions.Converters.Add(new NetworkConfigurationResponseJsonConverter());
            _jsonOptions.Converters.Add(new NetworkConfigurationResponseVersionJsonConverter());
            _jsonOptions.Converters.Add(new NetworkConfigurationResponseWellKnownAddressesJsonConverter());
            _jsonOptions.Converters.Add(new NetworkStatusRequestJsonConverter());
            _jsonOptions.Converters.Add(new NetworkStatusResponseJsonConverter());
            _jsonOptions.Converters.Add(new NextEpochJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleAuthorizedDepositorBadgeJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleGlobalIdJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleIdTypeJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleIdTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleLocalIdJsonConverter());
            _jsonOptions.Converters.Add(new NonFungiblePresentedBadgeJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleRequirementJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleResourceAmountJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleResourceManagerDataEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleResourceManagerDataEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleResourceManagerFieldIdTypeSubstateJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleResourceManagerFieldIdTypeValueJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleResourceManagerFieldMutableFieldsSubstateJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleResourceManagerFieldMutableFieldsValueJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleResourceManagerFieldTotalSupplySubstateJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleResourceManagerFieldTotalSupplyValueJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleVaultContentsIndexEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleVaultContentsIndexEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleVaultFieldBalanceSubstateJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleVaultFieldBalanceValueJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleVaultFieldFrozenStatusSubstateJsonConverter());
            _jsonOptions.Converters.Add(new NonFungibleVaultFieldFrozenStatusValueJsonConverter());
            _jsonOptions.Converters.Add(new NotarizedTransactionJsonConverter());
            _jsonOptions.Converters.Add(new ObjectFieldStructureJsonConverter());
            _jsonOptions.Converters.Add(new ObjectHookJsonConverter());
            _jsonOptions.Converters.Add(new ObjectHookNullableJsonConverter());
            _jsonOptions.Converters.Add(new ObjectIndexPartitionEntryStructureJsonConverter());
            _jsonOptions.Converters.Add(new ObjectInstanceTypeReferenceJsonConverter());
            _jsonOptions.Converters.Add(new ObjectKeyValuePartitionEntryStructureJsonConverter());
            _jsonOptions.Converters.Add(new ObjectRoleKeyJsonConverter());
            _jsonOptions.Converters.Add(new ObjectSortedIndexPartitionEntryStructureJsonConverter());
            _jsonOptions.Converters.Add(new ObjectSubstateTypeReferenceJsonConverter());
            _jsonOptions.Converters.Add(new ObjectSubstateTypeReferenceTypeJsonConverter());
            _jsonOptions.Converters.Add(new ObjectSubstateTypeReferenceTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new ObjectTypeInfoDetailsJsonConverter());
            _jsonOptions.Converters.Add(new OneResourcePoolFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new OneResourcePoolFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new OuterObjectOnlyMethodAccessibilityJsonConverter());
            _jsonOptions.Converters.Add(new OwnPackageOnlyMethodAccessibilityJsonConverter());
            _jsonOptions.Converters.Add(new OwnerRoleJsonConverter());
            _jsonOptions.Converters.Add(new OwnerRoleUpdaterJsonConverter());
            _jsonOptions.Converters.Add(new OwnerRoleUpdaterNullableJsonConverter());
            _jsonOptions.Converters.Add(new PackageBlueprintAuthTemplateEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new PackageBlueprintAuthTemplateEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new PackageBlueprintDefinitionEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new PackageBlueprintDefinitionEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new PackageBlueprintDependenciesEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new PackageBlueprintDependenciesEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new PackageBlueprintRoyaltyEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new PackageBlueprintRoyaltyEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new PackageCodeInstrumentedCodeEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new PackageCodeInstrumentedCodeEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new PackageCodeKeyJsonConverter());
            _jsonOptions.Converters.Add(new PackageCodeOriginalCodeEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new PackageCodeOriginalCodeEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new PackageCodeVmTypeEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new PackageCodeVmTypeEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new PackageExportJsonConverter());
            _jsonOptions.Converters.Add(new PackageFieldRoyaltyAccumulatorSubstateJsonConverter());
            _jsonOptions.Converters.Add(new PackageFieldRoyaltyAccumulatorValueJsonConverter());
            _jsonOptions.Converters.Add(new PackageObjectSubstateTypeReferenceJsonConverter());
            _jsonOptions.Converters.Add(new PackageTypeReferenceJsonConverter());
            _jsonOptions.Converters.Add(new ParsedLedgerTransactionJsonConverter());
            _jsonOptions.Converters.Add(new ParsedLedgerTransactionIdentifiersJsonConverter());
            _jsonOptions.Converters.Add(new ParsedNotarizedTransactionJsonConverter());
            _jsonOptions.Converters.Add(new ParsedNotarizedTransactionAllOfValidationErrorJsonConverter());
            _jsonOptions.Converters.Add(new ParsedNotarizedTransactionIdentifiersJsonConverter());
            _jsonOptions.Converters.Add(new ParsedSignedTransactionIntentJsonConverter());
            _jsonOptions.Converters.Add(new ParsedSignedTransactionIntentIdentifiersJsonConverter());
            _jsonOptions.Converters.Add(new ParsedTransactionJsonConverter());
            _jsonOptions.Converters.Add(new ParsedTransactionIntentJsonConverter());
            _jsonOptions.Converters.Add(new ParsedTransactionIntentIdentifiersJsonConverter());
            _jsonOptions.Converters.Add(new ParsedTransactionTypeJsonConverter());
            _jsonOptions.Converters.Add(new ParsedTransactionTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new PartitionDescriptionJsonConverter());
            _jsonOptions.Converters.Add(new PartitionDescriptionTypeJsonConverter());
            _jsonOptions.Converters.Add(new PartitionDescriptionTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new PartitionIdJsonConverter());
            _jsonOptions.Converters.Add(new PartitionKindJsonConverter());
            _jsonOptions.Converters.Add(new PartitionKindNullableJsonConverter());
            _jsonOptions.Converters.Add(new PaymentFromVaultJsonConverter());
            _jsonOptions.Converters.Add(new PaymentToRoyaltyRecipientJsonConverter());
            _jsonOptions.Converters.Add(new PendingOwnerStakeWithdrawalJsonConverter());
            _jsonOptions.Converters.Add(new PlaintextMessageContentJsonConverter());
            _jsonOptions.Converters.Add(new PlaintextMessageContentTypeJsonConverter());
            _jsonOptions.Converters.Add(new PlaintextMessageContentTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new PlaintextTransactionMessageJsonConverter());
            _jsonOptions.Converters.Add(new PoolVaultJsonConverter());
            _jsonOptions.Converters.Add(new PresentedBadgeJsonConverter());
            _jsonOptions.Converters.Add(new PresentedBadgeTypeJsonConverter());
            _jsonOptions.Converters.Add(new PresentedBadgeTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new PrimaryRoleRecoveryAttemptJsonConverter());
            _jsonOptions.Converters.Add(new ProofAccessRuleNodeJsonConverter());
            _jsonOptions.Converters.Add(new ProofRuleJsonConverter());
            _jsonOptions.Converters.Add(new ProofRuleTypeJsonConverter());
            _jsonOptions.Converters.Add(new ProofRuleTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new ProposerRewardJsonConverter());
            _jsonOptions.Converters.Add(new ProtectedAccessRuleJsonConverter());
            _jsonOptions.Converters.Add(new ProtocolUpdateLedgerProofOriginJsonConverter());
            _jsonOptions.Converters.Add(new PublicKeyJsonConverter());
            _jsonOptions.Converters.Add(new PublicKeyTypeJsonConverter());
            _jsonOptions.Converters.Add(new PublicKeyTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new PublicMethodAccessibilityJsonConverter());
            _jsonOptions.Converters.Add(new ReceiverInfoJsonConverter());
            _jsonOptions.Converters.Add(new RecoveryProposalJsonConverter());
            _jsonOptions.Converters.Add(new RecoveryRoleRecoveryAttemptJsonConverter());
            _jsonOptions.Converters.Add(new ReferenceTypeJsonConverter());
            _jsonOptions.Converters.Add(new RemoteGenericSubstitutionJsonConverter());
            _jsonOptions.Converters.Add(new RequestedStateVersionOutOfBoundsErrorDetailsJsonConverter());
            _jsonOptions.Converters.Add(new RequireProofRuleJsonConverter());
            _jsonOptions.Converters.Add(new RequirementJsonConverter());
            _jsonOptions.Converters.Add(new RequirementTypeJsonConverter());
            _jsonOptions.Converters.Add(new RequirementTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new ResourceAmountJsonConverter());
            _jsonOptions.Converters.Add(new ResourceAuthorizedDepositorBadgeJsonConverter());
            _jsonOptions.Converters.Add(new ResourceChangeJsonConverter());
            _jsonOptions.Converters.Add(new ResourceKeyJsonConverter());
            _jsonOptions.Converters.Add(new ResourcePreferenceJsonConverter());
            _jsonOptions.Converters.Add(new ResourcePreferenceNullableJsonConverter());
            _jsonOptions.Converters.Add(new ResourcePresentedBadgeJsonConverter());
            _jsonOptions.Converters.Add(new ResourceRequirementJsonConverter());
            _jsonOptions.Converters.Add(new ResourceSpecificDepositBehaviourJsonConverter());
            _jsonOptions.Converters.Add(new ResourceTypeJsonConverter());
            _jsonOptions.Converters.Add(new ResourceTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new RoleAssignmentModuleFieldOwnerRoleSubstateJsonConverter());
            _jsonOptions.Converters.Add(new RoleAssignmentModuleFieldOwnerRoleValueJsonConverter());
            _jsonOptions.Converters.Add(new RoleAssignmentModuleRuleEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new RoleAssignmentModuleRuleEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new RoleDetailsJsonConverter());
            _jsonOptions.Converters.Add(new RoleProtectedMethodAccessibilityJsonConverter());
            _jsonOptions.Converters.Add(new RoleSpecificationJsonConverter());
            _jsonOptions.Converters.Add(new RoleSpecificationNullableJsonConverter());
            _jsonOptions.Converters.Add(new RoundUpdateLedgerTransactionJsonConverter());
            _jsonOptions.Converters.Add(new RoundUpdateTransactionJsonConverter());
            _jsonOptions.Converters.Add(new RoyaltyAmountJsonConverter());
            _jsonOptions.Converters.Add(new RoyaltyModuleFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new RoyaltyModuleFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new RoyaltyModuleMethodRoyaltyEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new RoyaltyModuleMethodRoyaltyEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new SborDataJsonConverter());
            _jsonOptions.Converters.Add(new SborFormatOptionsJsonConverter());
            _jsonOptions.Converters.Add(new ScenariosRequestJsonConverter());
            _jsonOptions.Converters.Add(new ScenariosResponseJsonConverter());
            _jsonOptions.Converters.Add(new SchemaEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new SchemaEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new SchemaKeyJsonConverter());
            _jsonOptions.Converters.Add(new ScopedTypeIdJsonConverter());
            _jsonOptions.Converters.Add(new ScryptoSchemaJsonConverter());
            _jsonOptions.Converters.Add(new SignatureJsonConverter());
            _jsonOptions.Converters.Add(new SignatureWithPublicKeyJsonConverter());
            _jsonOptions.Converters.Add(new SignedTransactionIntentJsonConverter());
            _jsonOptions.Converters.Add(new SignificantProtocolUpdateReadinessEntryJsonConverter());
            _jsonOptions.Converters.Add(new SortedIndexBlueprintCollectionSchemaJsonConverter());
            _jsonOptions.Converters.Add(new SortedSubstateKeyJsonConverter());
            _jsonOptions.Converters.Add(new StateAccessControllerRequestJsonConverter());
            _jsonOptions.Converters.Add(new StateAccessControllerResponseJsonConverter());
            _jsonOptions.Converters.Add(new StateAccountRequestJsonConverter());
            _jsonOptions.Converters.Add(new StateAccountResponseJsonConverter());
            _jsonOptions.Converters.Add(new StateComponentDescendentNodeJsonConverter());
            _jsonOptions.Converters.Add(new StateComponentRequestJsonConverter());
            _jsonOptions.Converters.Add(new StateComponentResponseJsonConverter());
            _jsonOptions.Converters.Add(new StateConsensusManagerRequestJsonConverter());
            _jsonOptions.Converters.Add(new StateConsensusManagerResponseJsonConverter());
            _jsonOptions.Converters.Add(new StateFungibleResourceManagerJsonConverter());
            _jsonOptions.Converters.Add(new StateNonFungibleRequestJsonConverter());
            _jsonOptions.Converters.Add(new StateNonFungibleResourceManagerJsonConverter());
            _jsonOptions.Converters.Add(new StateNonFungibleResponseJsonConverter());
            _jsonOptions.Converters.Add(new StatePackageRequestJsonConverter());
            _jsonOptions.Converters.Add(new StatePackageResponseJsonConverter());
            _jsonOptions.Converters.Add(new StateResourceManagerJsonConverter());
            _jsonOptions.Converters.Add(new StateResourceRequestJsonConverter());
            _jsonOptions.Converters.Add(new StateResourceResponseJsonConverter());
            _jsonOptions.Converters.Add(new StateUpdatesJsonConverter());
            _jsonOptions.Converters.Add(new StateValidatorRequestJsonConverter());
            _jsonOptions.Converters.Add(new StateValidatorResponseJsonConverter());
            _jsonOptions.Converters.Add(new StaticBlueprintPayloadDefJsonConverter());
            _jsonOptions.Converters.Add(new StaticRoleDefinitionAuthTemplateJsonConverter());
            _jsonOptions.Converters.Add(new StreamTransactionsErrorDetailsJsonConverter());
            _jsonOptions.Converters.Add(new StreamTransactionsErrorDetailsTypeJsonConverter());
            _jsonOptions.Converters.Add(new StreamTransactionsErrorDetailsTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new StreamTransactionsErrorResponseJsonConverter());
            _jsonOptions.Converters.Add(new StreamTransactionsRequestJsonConverter());
            _jsonOptions.Converters.Add(new StreamTransactionsResponseJsonConverter());
            _jsonOptions.Converters.Add(new StringPlaintextMessageContentJsonConverter());
            _jsonOptions.Converters.Add(new SubstateJsonConverter());
            _jsonOptions.Converters.Add(new SubstateFormatOptionsJsonConverter());
            _jsonOptions.Converters.Add(new SubstateIdJsonConverter());
            _jsonOptions.Converters.Add(new SubstateKeyJsonConverter());
            _jsonOptions.Converters.Add(new SubstateKeyTypeJsonConverter());
            _jsonOptions.Converters.Add(new SubstateKeyTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new SubstateSystemStructureJsonConverter());
            _jsonOptions.Converters.Add(new SubstateSystemStructureTypeJsonConverter());
            _jsonOptions.Converters.Add(new SubstateSystemStructureTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new SubstateTypeJsonConverter());
            _jsonOptions.Converters.Add(new SubstateTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new SubstateValueJsonConverter());
            _jsonOptions.Converters.Add(new SystemFieldKindJsonConverter());
            _jsonOptions.Converters.Add(new SystemFieldKindNullableJsonConverter());
            _jsonOptions.Converters.Add(new SystemFieldStructureJsonConverter());
            _jsonOptions.Converters.Add(new SystemSchemaStructureJsonConverter());
            _jsonOptions.Converters.Add(new SystemTransactionJsonConverter());
            _jsonOptions.Converters.Add(new TargetIdentifierJsonConverter());
            _jsonOptions.Converters.Add(new TargetIdentifierTypeJsonConverter());
            _jsonOptions.Converters.Add(new TargetIdentifierTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new TimestampedValidatorSignatureJsonConverter());
            _jsonOptions.Converters.Add(new TransactionCallPreviewRequestJsonConverter());
            _jsonOptions.Converters.Add(new TransactionCallPreviewResponseJsonConverter());
            _jsonOptions.Converters.Add(new TransactionFormatOptionsJsonConverter());
            _jsonOptions.Converters.Add(new TransactionHeaderJsonConverter());
            _jsonOptions.Converters.Add(new TransactionIdKeyJsonConverter());
            _jsonOptions.Converters.Add(new TransactionIdentifiersJsonConverter());
            _jsonOptions.Converters.Add(new TransactionIntentJsonConverter());
            _jsonOptions.Converters.Add(new TransactionIntentStatusJsonConverter());
            _jsonOptions.Converters.Add(new TransactionIntentStatusNullableJsonConverter());
            _jsonOptions.Converters.Add(new TransactionMessageJsonConverter());
            _jsonOptions.Converters.Add(new TransactionMessageTypeJsonConverter());
            _jsonOptions.Converters.Add(new TransactionMessageTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new TransactionParseRequestJsonConverter());
            _jsonOptions.Converters.Add(new TransactionParseResponseJsonConverter());
            _jsonOptions.Converters.Add(new TransactionPayloadDetailsJsonConverter());
            _jsonOptions.Converters.Add(new TransactionPayloadStatusJsonConverter());
            _jsonOptions.Converters.Add(new TransactionPayloadStatusNullableJsonConverter());
            _jsonOptions.Converters.Add(new TransactionPreviewRequestJsonConverter());
            _jsonOptions.Converters.Add(new TransactionPreviewRequestFlagsJsonConverter());
            _jsonOptions.Converters.Add(new TransactionPreviewResponseJsonConverter());
            _jsonOptions.Converters.Add(new TransactionPreviewResponseLogsInnerJsonConverter());
            _jsonOptions.Converters.Add(new TransactionReceiptJsonConverter());
            _jsonOptions.Converters.Add(new TransactionReceiptRequestJsonConverter());
            _jsonOptions.Converters.Add(new TransactionReceiptResponseJsonConverter());
            _jsonOptions.Converters.Add(new TransactionStatusJsonConverter());
            _jsonOptions.Converters.Add(new TransactionStatusNullableJsonConverter());
            _jsonOptions.Converters.Add(new TransactionStatusRequestJsonConverter());
            _jsonOptions.Converters.Add(new TransactionStatusResponseJsonConverter());
            _jsonOptions.Converters.Add(new TransactionSubmitErrorDetailsJsonConverter());
            _jsonOptions.Converters.Add(new TransactionSubmitErrorDetailsTypeJsonConverter());
            _jsonOptions.Converters.Add(new TransactionSubmitErrorDetailsTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new TransactionSubmitErrorResponseJsonConverter());
            _jsonOptions.Converters.Add(new TransactionSubmitIntentAlreadyCommittedJsonConverter());
            _jsonOptions.Converters.Add(new TransactionSubmitPriorityThresholdNotMetErrorDetailsJsonConverter());
            _jsonOptions.Converters.Add(new TransactionSubmitRejectedErrorDetailsJsonConverter());
            _jsonOptions.Converters.Add(new TransactionSubmitRequestJsonConverter());
            _jsonOptions.Converters.Add(new TransactionSubmitResponseJsonConverter());
            _jsonOptions.Converters.Add(new TransactionTrackerCollectionEntrySubstateJsonConverter());
            _jsonOptions.Converters.Add(new TransactionTrackerCollectionEntryValueJsonConverter());
            _jsonOptions.Converters.Add(new TransactionTrackerFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new TransactionTrackerFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new TransactionTrackerTransactionStatusJsonConverter());
            _jsonOptions.Converters.Add(new TransactionTrackerTransactionStatusNullableJsonConverter());
            _jsonOptions.Converters.Add(new TwoResourcePoolFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new TwoResourcePoolFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new TypeInfoDetailsJsonConverter());
            _jsonOptions.Converters.Add(new TypeInfoModuleFieldTypeInfoSubstateJsonConverter());
            _jsonOptions.Converters.Add(new TypeInfoModuleFieldTypeInfoValueJsonConverter());
            _jsonOptions.Converters.Add(new TypeInfoTypeJsonConverter());
            _jsonOptions.Converters.Add(new TypeInfoTypeNullableJsonConverter());
            _jsonOptions.Converters.Add(new UpdatedSubstateJsonConverter());
            _jsonOptions.Converters.Add(new UserLedgerTransactionJsonConverter());
            _jsonOptions.Converters.Add(new ValidatorFeeChangeRequestJsonConverter());
            _jsonOptions.Converters.Add(new ValidatorFieldProtocolUpdateReadinessSignalSubstateJsonConverter());
            _jsonOptions.Converters.Add(new ValidatorFieldProtocolUpdateReadinessSignalValueJsonConverter());
            _jsonOptions.Converters.Add(new ValidatorFieldStateSubstateJsonConverter());
            _jsonOptions.Converters.Add(new ValidatorFieldStateValueJsonConverter());
            _jsonOptions.Converters.Add(new VaultBalanceJsonConverter());
            _jsonOptions.Converters.Add(new VirtualLazyLoadSchemaJsonConverter());
            _jsonOptions.Converters.Add(new VmTypeJsonConverter());
            _jsonOptions.Converters.Add(new VmTypeNullableJsonConverter());
            JsonSerializerOptionsProvider jsonSerializerOptionsProvider = new(_jsonOptions);
            _services.AddSingleton(jsonSerializerOptionsProvider);
            _services.AddSingleton<IApiFactory, ApiFactory>();
            _services.AddSingleton<LTSApiEvents>();
            _services.AddTransient<ILTSApi, LTSApi>();
            _services.AddSingleton<MempoolApiEvents>();
            _services.AddTransient<IMempoolApi, MempoolApi>();
            _services.AddSingleton<StateApiEvents>();
            _services.AddTransient<IStateApi, StateApi>();
            _services.AddSingleton<StatusApiEvents>();
            _services.AddTransient<IStatusApi, StatusApi>();
            _services.AddSingleton<StreamApiEvents>();
            _services.AddTransient<IStreamApi, StreamApi>();
            _services.AddSingleton<TransactionApiEvents>();
            _services.AddTransient<ITransactionApi, TransactionApi>();
        }

        /// <summary>
        /// Configures the HttpClients.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public HostConfiguration AddApiHttpClients
        (
            Action<HttpClient>? client = null, Action<IHttpClientBuilder>? builder = null)
        {
            if (client == null)
                client = c => c.BaseAddress = new Uri(ClientUtils.BASE_ADDRESS);

            List<IHttpClientBuilder> builders = new List<IHttpClientBuilder>();

            builders.Add(_services.AddHttpClient<ILTSApi, LTSApi>(client));
            builders.Add(_services.AddHttpClient<IMempoolApi, MempoolApi>(client));
            builders.Add(_services.AddHttpClient<IStateApi, StateApi>(client));
            builders.Add(_services.AddHttpClient<IStatusApi, StatusApi>(client));
            builders.Add(_services.AddHttpClient<IStreamApi, StreamApi>(client));
            builders.Add(_services.AddHttpClient<ITransactionApi, TransactionApi>(client));
            
            if (builder != null)
                foreach (IHttpClientBuilder instance in builders)
                    builder(instance);

            HttpClientsAdded = true;

            return this;
        }

        /// <summary>
        /// Configures the JsonSerializerSettings
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public HostConfiguration ConfigureJsonOptions(Action<JsonSerializerOptions> options)
        {
            options(_jsonOptions);

            return this;
        }

        /// <summary>
        /// Adds tokens to your IServiceCollection
        /// </summary>
        /// <typeparam name="TTokenBase"></typeparam>
        /// <param name="token"></param>
        /// <returns></returns>
        public HostConfiguration AddTokens<TTokenBase>(TTokenBase token) where TTokenBase : TokenBase
        {
            return AddTokens(new TTokenBase[]{ token });
        }

        /// <summary>
        /// Adds tokens to your IServiceCollection
        /// </summary>
        /// <typeparam name="TTokenBase"></typeparam>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public HostConfiguration AddTokens<TTokenBase>(IEnumerable<TTokenBase> tokens) where TTokenBase : TokenBase
        {
            TokenContainer<TTokenBase> container = new TokenContainer<TTokenBase>(tokens);
            _services.AddSingleton(services => container);

            return this;
        }

        /// <summary>
        /// Adds a token provider to your IServiceCollection
        /// </summary>
        /// <typeparam name="TTokenProvider"></typeparam>
        /// <typeparam name="TTokenBase"></typeparam>
        /// <returns></returns>
        public HostConfiguration UseProvider<TTokenProvider, TTokenBase>() 
            where TTokenProvider : TokenProvider<TTokenBase>
            where TTokenBase : TokenBase
        {
            _services.AddSingleton<TTokenProvider>();
            _services.AddSingleton<TokenProvider<TTokenBase>>(services => services.GetRequiredService<TTokenProvider>());

            return this;
        }
    }
}
