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

﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;

#nullable disable

namespace RadixDlt.NetworkGateway.PostgresIntegration.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_default_deposit_rule", "accept,reject,allow_existing")
                .Annotation("Npgsql:Enum:account_resource_preference_rule", "allowed,disallowed")
                .Annotation("Npgsql:Enum:entity_type", "global_consensus_manager,global_fungible_resource,global_non_fungible_resource,global_generic_component,internal_generic_component,global_account_component,global_package,internal_key_value_store,internal_fungible_vault,internal_non_fungible_vault,global_validator,global_access_controller,global_identity,global_one_resource_pool,global_two_resource_pool,global_multi_resource_pool,global_transaction_tracker")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_event_type", "withdrawal,deposit")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_operation_type", "resource_in_use,account_deposited_into,account_withdrawn_from")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_origin_type", "user,epoch_change")
                .Annotation("Npgsql:Enum:ledger_transaction_marker_type", "origin,event,manifest_address,affected_global_entity")
                .Annotation("Npgsql:Enum:ledger_transaction_status", "succeeded,failed")
                .Annotation("Npgsql:Enum:ledger_transaction_type", "genesis,user,round_update")
                .Annotation("Npgsql:Enum:non_fungible_id_type", "string,integer,bytes,ruid")
                .Annotation("Npgsql:Enum:object_module_id", "main,metadata,royalty,role_assignment")
                .Annotation("Npgsql:Enum:package_vm_type", "native,scrypto_v1")
                .Annotation("Npgsql:Enum:pending_transaction_status", "submitted_or_known_in_node_mempool,missing,rejected_temporarily,rejected_permanently,committed_success,committed_failure")
                .Annotation("Npgsql:Enum:public_key_type", "ecdsa_secp256k1,eddsa_ed25519")
                .Annotation("Npgsql:Enum:resource_type", "fungible,non_fungible")
                .Annotation("Npgsql:Enum:sbor_type_kind", "well_known,schema_local")
                .Annotation("Npgsql:Enum:state_type", "json,sbor");

            migrationBuilder.CreateTable(
                name: "account_default_deposit_rule_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    account_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    default_deposit_rule = table.Column<AccountDefaultDepositRule>(type: "account_default_deposit_rule", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_default_deposit_rule_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account_resource_preference_rule_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    account_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    account_resource_preference_rule = table.Column<AccountResourcePreferenceRule>(type: "account_resource_preference_rule", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_resource_preference_rule_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "component_method_royalty_entry_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    method_name = table.Column<string>(type: "text", nullable: false),
                    royalty_amount = table.Column<string>(type: "jsonb", nullable: true),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_method_royalty_entry_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entities",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    is_global = table.Column<bool>(type: "boolean", nullable: false),
                    ancestor_ids = table.Column<List<long>>(type: "bigint[]", nullable: true),
                    parent_ancestor_id = table.Column<long>(type: "bigint", nullable: true),
                    owner_ancestor_id = table.Column<long>(type: "bigint", nullable: true),
                    global_ancestor_id = table.Column<long>(type: "bigint", nullable: true),
                    correlated_entities = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    discriminator = table.Column<EntityType>(type: "entity_type", nullable: false),
                    package_id = table.Column<long>(type: "bigint", nullable: true),
                    blueprint_name = table.Column<string>(type: "text", nullable: true),
                    blueprint_version = table.Column<string>(type: "text", nullable: true),
                    divisibility = table.Column<int>(type: "integer", nullable: true),
                    non_fungible_id_type = table.Column<NonFungibleIdType>(type: "non_fungible_id_type", nullable: true),
                    vm_type = table.Column<PackageVmType>(type: "package_vm_type", nullable: true),
                    stake_vault_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    pending_xrd_withdraw_vault_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    locked_owner_stake_unit_vault_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    pending_owner_stake_unit_unlock_vault_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    royalty_vault_of_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_metadata_aggregate_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    metadata_ids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_metadata_aggregate_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_metadata_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<byte[]>(type: "bytea", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_metadata_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_resource_aggregate_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    fungible_resource_entity_ids = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    fungible_resource_significant_update_state_versions = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    non_fungible_resource_entity_ids = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    non_fungible_resource_significant_update_state_versions = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_resource_aggregate_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_resource_aggregated_vaults_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    discriminator = table.Column<ResourceType>(type: "resource_type", nullable: false),
                    balance = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: true),
                    total_count = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_resource_aggregated_vaults_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_resource_vault_aggregate_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    vault_entity_ids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_resource_vault_aggregate_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_role_assignments_aggregate_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    owner_role_id = table.Column<long>(type: "bigint", nullable: false),
                    entry_ids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_role_assignments_aggregate_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_role_assignments_entry_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    key_role = table.Column<string>(type: "text", nullable: false),
                    key_module = table.Column<ObjectModuleId>(type: "object_module_id", nullable: false),
                    role_assignments = table.Column<string>(type: "jsonb", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_role_assignments_entry_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_role_assignments_owner_role_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    role_assignments = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_role_assignments_owner_role_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_vault_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    owner_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    global_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    vault_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    discriminator = table.Column<ResourceType>(type: "resource_type", nullable: false),
                    balance = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: true),
                    is_royalty_vault = table.Column<bool>(type: "boolean", nullable: true),
                    non_fungible_ids = table.Column<List<long>>(type: "bigint[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_vault_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "key_value_store_entry_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    key_value_store_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    key = table.Column<byte[]>(type: "bytea", nullable: false),
                    value = table.Column<byte[]>(type: "bytea", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_key_value_store_entry_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "key_value_store_schema_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    key_value_store_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    key_schema_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    key_sbor_type_kind = table.Column<SborTypeKind>(type: "sbor_type_kind", nullable: false),
                    key_type_index = table.Column<long>(type: "bigint", nullable: false),
                    value_schema_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    value_sbor_type_kind = table.Column<SborTypeKind>(type: "sbor_type_kind", nullable: false),
                    value_type_index = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_key_value_store_schema_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ledger_transaction_markers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    discriminator = table.Column<LedgerTransactionMarkerType>(type: "ledger_transaction_marker_type", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    event_type = table.Column<LedgerTransactionMarkerEventType>(type: "ledger_transaction_marker_event_type", nullable: true),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    quantity = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: true),
                    operation_type = table.Column<LedgerTransactionMarkerOperationType>(type: "ledger_transaction_marker_operation_type", nullable: true),
                    origin_type = table.Column<LedgerTransactionMarkerOriginType>(type: "ledger_transaction_marker_origin_type", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_transaction_markers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ledger_transactions",
                columns: table => new
                {
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    round_in_epoch = table.Column<long>(type: "bigint", nullable: false),
                    index_in_epoch = table.Column<long>(type: "bigint", nullable: false),
                    index_in_round = table.Column<long>(type: "bigint", nullable: false),
                    fee_paid = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    tip_paid = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    affected_global_entities = table.Column<long[]>(type: "bigint[]", nullable: false),
                    round_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    normalized_round_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    raw_payload = table.Column<byte[]>(type: "bytea", nullable: false),
                    receipt_status = table.Column<LedgerTransactionStatus>(type: "ledger_transaction_status", nullable: false),
                    receipt_fee_summary = table.Column<string>(type: "jsonb", nullable: false),
                    receipt_state_updates = table.Column<string>(type: "jsonb", nullable: false),
                    receipt_costing_parameters = table.Column<string>(type: "jsonb", nullable: false),
                    receipt_fee_source = table.Column<string>(type: "jsonb", nullable: true),
                    receipt_fee_destination = table.Column<string>(type: "jsonb", nullable: true),
                    receipt_next_epoch = table.Column<string>(type: "jsonb", nullable: true),
                    receipt_output = table.Column<string>(type: "jsonb", nullable: true),
                    receipt_error_message = table.Column<string>(type: "text", nullable: true),
                    receipt_event_sbors = table.Column<byte[][]>(type: "bytea[]", nullable: false),
                    receipt_event_schema_hashes = table.Column<byte[][]>(type: "bytea[]", nullable: false),
                    receipt_event_type_indexes = table.Column<long[]>(type: "bigint[]", nullable: false),
                    receipt_event_sbor_type_kinds = table.Column<SborTypeKind[]>(type: "sbor_type_kind[]", nullable: false),
                    discriminator = table.Column<LedgerTransactionType>(type: "ledger_transaction_type", nullable: false),
                    payload_hash = table.Column<string>(type: "text", nullable: true),
                    intent_hash = table.Column<string>(type: "text", nullable: true),
                    signed_intent_hash = table.Column<string>(type: "text", nullable: true),
                    message = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_transactions", x => x.state_version);
                });

            migrationBuilder.CreateTable(
                name: "network_configuration",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    network_id = table.Column<byte>(type: "smallint", nullable: false),
                    network_name = table.Column<string>(type: "text", nullable: false),
                    hrp_definition = table.Column<HrpDefinition>(type: "jsonb", nullable: false),
                    well_known_addresses = table.Column<WellKnownAddresses>(type: "jsonb", nullable: false),
                    address_type_definitions = table.Column<AddressTypeDefinition[]>(type: "jsonb", nullable: false),
                    genesis_epoch = table.Column<long>(type: "bigint", nullable: false),
                    genesis_round = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_network_configuration", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "non_fungible_id_data",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_non_fungible_id_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "non_fungible_id_data_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_id_data_id = table.Column<long>(type: "bigint", nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_non_fungible_id_data_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "non_fungible_id_store_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_id_data_ids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_non_fungible_id_store_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "non_fungible_schema_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    schema_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    sbor_type_kind = table.Column<SborTypeKind>(type: "sbor_type_kind", nullable: false),
                    type_index = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_non_fungible_schema_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "package_blueprint_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    package_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    definition = table.Column<string>(type: "jsonb", nullable: false),
                    dependant_entity_ids = table.Column<List<long>>(type: "bigint[]", nullable: true),
                    auth_template = table.Column<string>(type: "jsonb", nullable: true),
                    auth_template_is_locked = table.Column<bool>(type: "boolean", nullable: true),
                    royalty_config = table.Column<string>(type: "jsonb", nullable: true),
                    royalty_config_is_locked = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_package_blueprint_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "package_code_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    package_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    code_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    code = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_package_code_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pending_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payload_hash = table.Column<string>(type: "text", nullable: false),
                    intent_hash = table.Column<string>(type: "text", nullable: false),
                    notarized_transaction_blob = table.Column<byte[]>(type: "bytea", nullable: false),
                    status = table.Column<PendingTransactionStatus>(type: "pending_transaction_status", nullable: false),
                    submitted_by_this_gateway = table.Column<bool>(type: "boolean", nullable: false),
                    first_submitted_to_gateway_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_submitted_to_gateway_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_submitted_to_node_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_submitted_to_node_name = table.Column<string>(type: "text", nullable: true),
                    submission_count = table.Column<int>(type: "integer", nullable: false),
                    first_seen_in_mempool_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_missing_from_mempool_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    commit_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_failure_reason = table.Column<string>(type: "text", nullable: true),
                    last_failure_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pending_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resource_entity_supply_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    total_supply = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_minted = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_burned = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_entity_supply_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "schema_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    schema_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    schema = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schema_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "state_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    discriminator = table.Column<StateType>(type: "state_type", nullable: false),
                    json_state = table.Column<string>(type: "jsonb", nullable: true),
                    sbor_state = table.Column<byte[]>(type: "bytea", nullable: true),
                    schema_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                    sbor_type_kind = table.Column<SborTypeKind>(type: "sbor_type_kind", nullable: true),
                    type_index = table.Column<long>(type: "bigint", nullable: true),
                    schema_defining_entity_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_state_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "validator_emission_statistics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    validator_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    epoch_number = table.Column<long>(type: "bigint", nullable: false),
                    proposals_made = table.Column<long>(type: "bigint", nullable: false),
                    proposals_missed = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validator_emission_statistics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "validator_public_key_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    validator_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    key_type = table.Column<PublicKeyType>(type: "public_key_type", nullable: false),
                    key = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validator_public_key_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "validator_active_set_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    validator_public_key_history_id = table.Column<long>(type: "bigint", nullable: false),
                    stake = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validator_active_set_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_validator_active_set_history_validator_public_key_history_v~",
                        column: x => x.validator_public_key_history_id,
                        principalTable: "validator_public_key_history",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_default_deposit_rule_history_account_entity_id_from~",
                table: "account_default_deposit_rule_history",
                columns: new[] { "account_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_preference_rule_history_account_entity_id_~",
                table: "account_resource_preference_rule_history",
                columns: new[] { "account_entity_id", "resource_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_component_method_royalty_entry_history_entity_id_from_state~",
                table: "component_method_royalty_entry_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_component_method_royalty_entry_history_entity_id_method_nam~",
                table: "component_method_royalty_entry_history",
                columns: new[] { "entity_id", "method_name", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entities_address",
                table: "entities",
                column: "address");

            migrationBuilder.CreateIndex(
                name: "IX_entity_metadata_aggregate_history_entity_id_from_state_vers~",
                table: "entity_metadata_aggregate_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_metadata_history_entity_id_key_from_state_version",
                table: "entity_metadata_history",
                columns: new[] { "entity_id", "key", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_resource_aggregate_history_entity_id_from_state_vers~",
                table: "entity_resource_aggregate_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_resource_aggregated_vaults_history_entity_id_resourc~",
                table: "entity_resource_aggregated_vaults_history",
                columns: new[] { "entity_id", "resource_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_resource_vault_aggregate_history_entity_id_resource_~",
                table: "entity_resource_vault_aggregate_history",
                columns: new[] { "entity_id", "resource_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_role_assignments_aggregate_history_entity_id_from_st~",
                table: "entity_role_assignments_aggregate_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_role_assignments_entry_history_entity_id_key_role_ke~",
                table: "entity_role_assignments_entry_history",
                columns: new[] { "entity_id", "key_role", "key_module", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_role_assignments_owner_role_history_entity_id_from_s~",
                table: "entity_role_assignments_owner_role_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_vault_history_global_entity_id_vault_entity_id_from_~",
                table: "entity_vault_history",
                columns: new[] { "global_entity_id", "vault_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_vault_history_id_resource_entity_id_from_state_versi~",
                table: "entity_vault_history",
                columns: new[] { "id", "resource_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_vault_history_owner_entity_id_vault_entity_id_from_s~",
                table: "entity_vault_history",
                columns: new[] { "owner_entity_id", "vault_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_key_value_store_entry_history_key_value_store_entity_id_key~",
                table: "key_value_store_entry_history",
                columns: new[] { "key_value_store_entity_id", "key", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_key_value_store_schema_history_key_value_store_entity_id_fr~",
                table: "key_value_store_schema_history",
                columns: new[] { "key_value_store_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transaction_markers_entity_id_state_version",
                table: "ledger_transaction_markers",
                columns: new[] { "entity_id", "state_version" },
                filter: "discriminator = 'affected_global_entity'");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transaction_markers_event_type_entity_id_state_versi~",
                table: "ledger_transaction_markers",
                columns: new[] { "event_type", "entity_id", "state_version" },
                filter: "discriminator = 'event'");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transaction_markers_operation_type_entity_id_state_v~",
                table: "ledger_transaction_markers",
                columns: new[] { "operation_type", "entity_id", "state_version" },
                filter: "discriminator = 'manifest_address'");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transaction_markers_origin_type_state_version",
                table: "ledger_transaction_markers",
                columns: new[] { "origin_type", "state_version" },
                filter: "discriminator = 'origin'");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transaction_markers_state_version",
                table: "ledger_transaction_markers",
                column: "state_version");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_epoch_round_in_epoch",
                table: "ledger_transactions",
                columns: new[] { "epoch", "round_in_epoch" },
                unique: true,
                filter: "index_in_round = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_intent_hash",
                table: "ledger_transactions",
                column: "intent_hash",
                filter: "intent_hash IS NOT NULL")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_round_timestamp",
                table: "ledger_transactions",
                column: "round_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_non_fungible_id_data_non_fungible_resource_entity_id_from_s~",
                table: "non_fungible_id_data",
                columns: new[] { "non_fungible_resource_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_non_fungible_id_data_non_fungible_resource_entity_id_non_fu~",
                table: "non_fungible_id_data",
                columns: new[] { "non_fungible_resource_entity_id", "non_fungible_id", "from_state_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_non_fungible_id_data_history_non_fungible_id_data_id_from_s~",
                table: "non_fungible_id_data_history",
                columns: new[] { "non_fungible_id_data_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_non_fungible_id_store_history_non_fungible_resource_entity_~",
                table: "non_fungible_id_store_history",
                columns: new[] { "non_fungible_resource_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_non_fungible_schema_history_resource_entity_id_from_state_v~",
                table: "non_fungible_schema_history",
                columns: new[] { "resource_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_package_blueprint_history_package_entity_id_from_state_vers~",
                table: "package_blueprint_history",
                columns: new[] { "package_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_package_code_history_package_entity_id_from_state_version",
                table: "package_code_history",
                columns: new[] { "package_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_pending_transactions_intent_hash",
                table: "pending_transactions",
                column: "intent_hash");

            migrationBuilder.CreateIndex(
                name: "IX_pending_transactions_payload_hash",
                table: "pending_transactions",
                column: "payload_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pending_transactions_status",
                table: "pending_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_resource_entity_supply_history_resource_entity_id_from_stat~",
                table: "resource_entity_supply_history",
                columns: new[] { "resource_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_schema_history_entity_id_from_state_version",
                table: "schema_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_schema_history_schema_hash_from_state_version",
                table: "schema_history",
                columns: new[] { "schema_hash", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_state_history_entity_id_from_state_version",
                table: "state_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_validator_active_set_history_epoch",
                table: "validator_active_set_history",
                column: "epoch");

            migrationBuilder.CreateIndex(
                name: "IX_validator_active_set_history_from_state_version",
                table: "validator_active_set_history",
                column: "from_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_validator_active_set_history_validator_public_key_history_id",
                table: "validator_active_set_history",
                column: "validator_public_key_history_id");

            migrationBuilder.CreateIndex(
                name: "IX_validator_public_key_history_validator_entity_id_from_state~",
                table: "validator_public_key_history",
                columns: new[] { "validator_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_validator_public_key_history_validator_entity_id_key_type_k~",
                table: "validator_public_key_history",
                columns: new[] { "validator_entity_id", "key_type", "key" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_default_deposit_rule_history");

            migrationBuilder.DropTable(
                name: "account_resource_preference_rule_history");

            migrationBuilder.DropTable(
                name: "component_method_royalty_entry_history");

            migrationBuilder.DropTable(
                name: "entities");

            migrationBuilder.DropTable(
                name: "entity_metadata_aggregate_history");

            migrationBuilder.DropTable(
                name: "entity_metadata_history");

            migrationBuilder.DropTable(
                name: "entity_resource_aggregate_history");

            migrationBuilder.DropTable(
                name: "entity_resource_aggregated_vaults_history");

            migrationBuilder.DropTable(
                name: "entity_resource_vault_aggregate_history");

            migrationBuilder.DropTable(
                name: "entity_role_assignments_aggregate_history");

            migrationBuilder.DropTable(
                name: "entity_role_assignments_entry_history");

            migrationBuilder.DropTable(
                name: "entity_role_assignments_owner_role_history");

            migrationBuilder.DropTable(
                name: "entity_vault_history");

            migrationBuilder.DropTable(
                name: "key_value_store_entry_history");

            migrationBuilder.DropTable(
                name: "key_value_store_schema_history");

            migrationBuilder.DropTable(
                name: "ledger_transaction_markers");

            migrationBuilder.DropTable(
                name: "ledger_transactions");

            migrationBuilder.DropTable(
                name: "network_configuration");

            migrationBuilder.DropTable(
                name: "non_fungible_id_data");

            migrationBuilder.DropTable(
                name: "non_fungible_id_data_history");

            migrationBuilder.DropTable(
                name: "non_fungible_id_store_history");

            migrationBuilder.DropTable(
                name: "non_fungible_schema_history");

            migrationBuilder.DropTable(
                name: "package_blueprint_history");

            migrationBuilder.DropTable(
                name: "package_code_history");

            migrationBuilder.DropTable(
                name: "pending_transactions");

            migrationBuilder.DropTable(
                name: "resource_entity_supply_history");

            migrationBuilder.DropTable(
                name: "schema_history");

            migrationBuilder.DropTable(
                name: "state_history");

            migrationBuilder.DropTable(
                name: "validator_active_set_history");

            migrationBuilder.DropTable(
                name: "validator_emission_statistics");

            migrationBuilder.DropTable(
                name: "validator_public_key_history");
        }
    }
}
