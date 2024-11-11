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

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RadixDlt.NetworkGateway.PostgresIntegration.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLedgerTransactionV2Support : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_default_deposit_rule", "accept,reject,allow_existing")
                .Annotation("Npgsql:Enum:account_resource_preference_rule", "allowed,disallowed")
                .Annotation("Npgsql:Enum:authorized_depositor_badge_type", "resource,non_fungible")
                .Annotation("Npgsql:Enum:entity_relationship", "component_to_instantiating_package,vault_to_resource,validator_to_stake_vault,validator_to_pending_xrd_withdraw_vault,validator_to_locked_owner_stake_unit_vault,validator_to_pending_owner_stake_unit_unlock_vault,stake_unit_of_validator,claim_token_of_validator,entity_to_royalty_vault,royalty_vault_of_entity,account_locker_of_locker,account_locker_of_account,resource_pool_to_unit_resource,resource_pool_to_resource,resource_pool_to_resource_vault,unit_resource_of_resource_pool,resource_vault_of_resource_pool,access_controller_to_recovery_badge,recovery_badge_of_access_controller")
                .Annotation("Npgsql:Enum:entity_type", "global_consensus_manager,global_fungible_resource,global_non_fungible_resource,global_generic_component,internal_generic_component,global_account_component,global_package,internal_key_value_store,internal_fungible_vault,internal_non_fungible_vault,global_validator,global_access_controller,global_identity,global_one_resource_pool,global_two_resource_pool,global_multi_resource_pool,global_transaction_tracker,global_account_locker")
                .Annotation("Npgsql:Enum:ledger_transaction_manifest_class", "general,transfer,validator_stake,validator_unstake,validator_claim,account_deposit_settings_update,pool_contribution,pool_redemption")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_event_type", "withdrawal,deposit")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_operation_type", "resource_in_use,account_deposited_into,account_withdrawn_from,account_owner_method_call,badge_presented")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_transaction_type", "user,round_change,genesis_flash,genesis_transaction,protocol_update_flash,protocol_update_transaction")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_type", "transaction_type,event,manifest_address,affected_global_entity,manifest_class,event_global_emitter,epoch_change")
                .Annotation("Npgsql:Enum:ledger_transaction_status", "succeeded,failed")
                .Annotation("Npgsql:Enum:ledger_transaction_type", "genesis,user,user_v2,round_update,flash")
                .Annotation("Npgsql:Enum:module_id", "main,metadata,royalty,role_assignment")
                .Annotation("Npgsql:Enum:non_fungible_id_type", "string,integer,bytes,ruid")
                .Annotation("Npgsql:Enum:package_vm_type", "native,scrypto_v1")
                .Annotation("Npgsql:Enum:pending_transaction_intent_ledger_status", "unknown,committed,commit_pending,permanent_rejection,possible_to_commit,likely_but_not_certain_rejection")
                .Annotation("Npgsql:Enum:pending_transaction_payload_ledger_status", "unknown,committed,commit_pending,clashing_commit,permanently_rejected,transiently_accepted,transiently_rejected")
                .Annotation("Npgsql:Enum:public_key_type", "ecdsa_secp256k1,eddsa_ed25519")
                .Annotation("Npgsql:Enum:resource_type", "fungible,non_fungible")
                .Annotation("Npgsql:Enum:sbor_type_kind", "well_known,schema_local")
                .Annotation("Npgsql:Enum:standard_metadata_key", "dapp_account_type,dapp_definition,dapp_definitions,dapp_claimed_websites,dapp_claimed_entities,dapp_account_locker")
                .Annotation("Npgsql:Enum:state_type", "json,sbor")
                .OldAnnotation("Npgsql:Enum:account_default_deposit_rule", "accept,reject,allow_existing")
                .OldAnnotation("Npgsql:Enum:account_resource_preference_rule", "allowed,disallowed")
                .OldAnnotation("Npgsql:Enum:authorized_depositor_badge_type", "resource,non_fungible")
                .OldAnnotation("Npgsql:Enum:entity_relationship", "component_to_instantiating_package,vault_to_resource,validator_to_stake_vault,validator_to_pending_xrd_withdraw_vault,validator_to_locked_owner_stake_unit_vault,validator_to_pending_owner_stake_unit_unlock_vault,stake_unit_of_validator,claim_token_of_validator,entity_to_royalty_vault,royalty_vault_of_entity,account_locker_of_locker,account_locker_of_account,resource_pool_to_unit_resource,resource_pool_to_resource,resource_pool_to_resource_vault,unit_resource_of_resource_pool,resource_vault_of_resource_pool,access_controller_to_recovery_badge,recovery_badge_of_access_controller")
                .OldAnnotation("Npgsql:Enum:entity_type", "global_consensus_manager,global_fungible_resource,global_non_fungible_resource,global_generic_component,internal_generic_component,global_account_component,global_package,internal_key_value_store,internal_fungible_vault,internal_non_fungible_vault,global_validator,global_access_controller,global_identity,global_one_resource_pool,global_two_resource_pool,global_multi_resource_pool,global_transaction_tracker,global_account_locker")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_manifest_class", "general,transfer,validator_stake,validator_unstake,validator_claim,account_deposit_settings_update,pool_contribution,pool_redemption")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_marker_event_type", "withdrawal,deposit")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_marker_operation_type", "resource_in_use,account_deposited_into,account_withdrawn_from,account_owner_method_call,badge_presented")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_marker_transaction_type", "user,round_change,genesis_flash,genesis_transaction,protocol_update_flash,protocol_update_transaction")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_marker_type", "transaction_type,event,manifest_address,affected_global_entity,manifest_class,event_global_emitter,epoch_change")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_status", "succeeded,failed")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_type", "genesis,user,round_update,flash")
                .OldAnnotation("Npgsql:Enum:module_id", "main,metadata,royalty,role_assignment")
                .OldAnnotation("Npgsql:Enum:non_fungible_id_type", "string,integer,bytes,ruid")
                .OldAnnotation("Npgsql:Enum:package_vm_type", "native,scrypto_v1")
                .OldAnnotation("Npgsql:Enum:pending_transaction_intent_ledger_status", "unknown,committed,commit_pending,permanent_rejection,possible_to_commit,likely_but_not_certain_rejection")
                .OldAnnotation("Npgsql:Enum:pending_transaction_payload_ledger_status", "unknown,committed,commit_pending,clashing_commit,permanently_rejected,transiently_accepted,transiently_rejected")
                .OldAnnotation("Npgsql:Enum:public_key_type", "ecdsa_secp256k1,eddsa_ed25519")
                .OldAnnotation("Npgsql:Enum:resource_type", "fungible,non_fungible")
                .OldAnnotation("Npgsql:Enum:sbor_type_kind", "well_known,schema_local")
                .OldAnnotation("Npgsql:Enum:standard_metadata_key", "dapp_account_type,dapp_definition,dapp_definitions,dapp_claimed_websites,dapp_claimed_entities,dapp_account_locker")
                .OldAnnotation("Npgsql:Enum:state_type", "json,sbor");

            migrationBuilder.CreateTable(
                name: "ledger_finalized_subintents",
                columns: table => new
                {
                    subintent_hash = table.Column<string>(type: "character varying(90)", maxLength: 90, nullable: false),
                    finalized_at_state_version = table.Column<long>(type: "bigint", nullable: false),
                    finalized_at_transaction_intent_hash = table.Column<string>(type: "character varying(90)", maxLength: 90, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_finalized_subintents", x => x.subintent_hash);
                });

            migrationBuilder.CreateTable(
                name: "ledger_transaction_subintent_data",
                columns: table => new
                {
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    child_subintent_hashes = table.Column<List<string>>(type: "text[]", nullable: false),
                    subintent_data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_transaction_subintent_data", x => x.state_version);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ledger_finalized_subintents");

            migrationBuilder.DropTable(
                name: "ledger_transaction_subintent_data");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_default_deposit_rule", "accept,reject,allow_existing")
                .Annotation("Npgsql:Enum:account_resource_preference_rule", "allowed,disallowed")
                .Annotation("Npgsql:Enum:authorized_depositor_badge_type", "resource,non_fungible")
                .Annotation("Npgsql:Enum:entity_relationship", "component_to_instantiating_package,vault_to_resource,validator_to_stake_vault,validator_to_pending_xrd_withdraw_vault,validator_to_locked_owner_stake_unit_vault,validator_to_pending_owner_stake_unit_unlock_vault,stake_unit_of_validator,claim_token_of_validator,entity_to_royalty_vault,royalty_vault_of_entity,account_locker_of_locker,account_locker_of_account,resource_pool_to_unit_resource,resource_pool_to_resource,resource_pool_to_resource_vault,unit_resource_of_resource_pool,resource_vault_of_resource_pool,access_controller_to_recovery_badge,recovery_badge_of_access_controller")
                .Annotation("Npgsql:Enum:entity_type", "global_consensus_manager,global_fungible_resource,global_non_fungible_resource,global_generic_component,internal_generic_component,global_account_component,global_package,internal_key_value_store,internal_fungible_vault,internal_non_fungible_vault,global_validator,global_access_controller,global_identity,global_one_resource_pool,global_two_resource_pool,global_multi_resource_pool,global_transaction_tracker,global_account_locker")
                .Annotation("Npgsql:Enum:ledger_transaction_manifest_class", "general,transfer,validator_stake,validator_unstake,validator_claim,account_deposit_settings_update,pool_contribution,pool_redemption")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_event_type", "withdrawal,deposit")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_operation_type", "resource_in_use,account_deposited_into,account_withdrawn_from,account_owner_method_call,badge_presented")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_transaction_type", "user,round_change,genesis_flash,genesis_transaction,protocol_update_flash,protocol_update_transaction")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_type", "transaction_type,event,manifest_address,affected_global_entity,manifest_class,event_global_emitter,epoch_change")
                .Annotation("Npgsql:Enum:ledger_transaction_status", "succeeded,failed")
                .Annotation("Npgsql:Enum:ledger_transaction_type", "genesis,user,round_update,flash")
                .Annotation("Npgsql:Enum:module_id", "main,metadata,royalty,role_assignment")
                .Annotation("Npgsql:Enum:non_fungible_id_type", "string,integer,bytes,ruid")
                .Annotation("Npgsql:Enum:package_vm_type", "native,scrypto_v1")
                .Annotation("Npgsql:Enum:pending_transaction_intent_ledger_status", "unknown,committed,commit_pending,permanent_rejection,possible_to_commit,likely_but_not_certain_rejection")
                .Annotation("Npgsql:Enum:pending_transaction_payload_ledger_status", "unknown,committed,commit_pending,clashing_commit,permanently_rejected,transiently_accepted,transiently_rejected")
                .Annotation("Npgsql:Enum:public_key_type", "ecdsa_secp256k1,eddsa_ed25519")
                .Annotation("Npgsql:Enum:resource_type", "fungible,non_fungible")
                .Annotation("Npgsql:Enum:sbor_type_kind", "well_known,schema_local")
                .Annotation("Npgsql:Enum:standard_metadata_key", "dapp_account_type,dapp_definition,dapp_definitions,dapp_claimed_websites,dapp_claimed_entities,dapp_account_locker")
                .Annotation("Npgsql:Enum:state_type", "json,sbor")
                .OldAnnotation("Npgsql:Enum:account_default_deposit_rule", "accept,reject,allow_existing")
                .OldAnnotation("Npgsql:Enum:account_resource_preference_rule", "allowed,disallowed")
                .OldAnnotation("Npgsql:Enum:authorized_depositor_badge_type", "resource,non_fungible")
                .OldAnnotation("Npgsql:Enum:entity_relationship", "component_to_instantiating_package,vault_to_resource,validator_to_stake_vault,validator_to_pending_xrd_withdraw_vault,validator_to_locked_owner_stake_unit_vault,validator_to_pending_owner_stake_unit_unlock_vault,stake_unit_of_validator,claim_token_of_validator,entity_to_royalty_vault,royalty_vault_of_entity,account_locker_of_locker,account_locker_of_account,resource_pool_to_unit_resource,resource_pool_to_resource,resource_pool_to_resource_vault,unit_resource_of_resource_pool,resource_vault_of_resource_pool,access_controller_to_recovery_badge,recovery_badge_of_access_controller")
                .OldAnnotation("Npgsql:Enum:entity_type", "global_consensus_manager,global_fungible_resource,global_non_fungible_resource,global_generic_component,internal_generic_component,global_account_component,global_package,internal_key_value_store,internal_fungible_vault,internal_non_fungible_vault,global_validator,global_access_controller,global_identity,global_one_resource_pool,global_two_resource_pool,global_multi_resource_pool,global_transaction_tracker,global_account_locker")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_manifest_class", "general,transfer,validator_stake,validator_unstake,validator_claim,account_deposit_settings_update,pool_contribution,pool_redemption")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_marker_event_type", "withdrawal,deposit")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_marker_operation_type", "resource_in_use,account_deposited_into,account_withdrawn_from,account_owner_method_call,badge_presented")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_marker_transaction_type", "user,round_change,genesis_flash,genesis_transaction,protocol_update_flash,protocol_update_transaction")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_marker_type", "transaction_type,event,manifest_address,affected_global_entity,manifest_class,event_global_emitter,epoch_change")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_status", "succeeded,failed")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_type", "genesis,user,user_v2,round_update,flash")
                .OldAnnotation("Npgsql:Enum:module_id", "main,metadata,royalty,role_assignment")
                .OldAnnotation("Npgsql:Enum:non_fungible_id_type", "string,integer,bytes,ruid")
                .OldAnnotation("Npgsql:Enum:package_vm_type", "native,scrypto_v1")
                .OldAnnotation("Npgsql:Enum:pending_transaction_intent_ledger_status", "unknown,committed,commit_pending,permanent_rejection,possible_to_commit,likely_but_not_certain_rejection")
                .OldAnnotation("Npgsql:Enum:pending_transaction_payload_ledger_status", "unknown,committed,commit_pending,clashing_commit,permanently_rejected,transiently_accepted,transiently_rejected")
                .OldAnnotation("Npgsql:Enum:public_key_type", "ecdsa_secp256k1,eddsa_ed25519")
                .OldAnnotation("Npgsql:Enum:resource_type", "fungible,non_fungible")
                .OldAnnotation("Npgsql:Enum:sbor_type_kind", "well_known,schema_local")
                .OldAnnotation("Npgsql:Enum:standard_metadata_key", "dapp_account_type,dapp_definition,dapp_definitions,dapp_claimed_websites,dapp_claimed_entities,dapp_account_locker")
                .OldAnnotation("Npgsql:Enum:state_type", "json,sbor");
        }
    }
}
