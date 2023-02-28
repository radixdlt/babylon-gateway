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
                .Annotation("Npgsql:Enum:access_rules_chain_subtype", "none,resource_manager_vault_access_rules_chain")
                .Annotation("Npgsql:Enum:entity_type", "epoch_manager,fungible_resource_manager,non_fungible_resource_manager,normal_component,account_component,package,key_value_store,vault,non_fungible_store,clock,validator,access_controller,identity")
                .Annotation("Npgsql:Enum:ledger_transaction_kind_filter_constraint", "user,epoch_change")
                .Annotation("Npgsql:Enum:ledger_transaction_status", "succeeded,failed")
                .Annotation("Npgsql:Enum:ledger_transaction_type", "user,validator,system")
                .Annotation("Npgsql:Enum:non_fungible_id_type", "string,number,bytes,uuid")
                .Annotation("Npgsql:Enum:pending_transaction_status", "submitted_or_known_in_node_mempool,missing,rejected_temporarily,rejected_permanently,committed_success,committed_failure")
                .Annotation("Npgsql:Enum:public_key_type", "ecdsa_secp256k1,eddsa_ed25519")
                .Annotation("Npgsql:Enum:resource_type", "fungible,non_fungible");

            migrationBuilder.CreateTable(
                name: "entities",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    address = table.Column<byte[]>(type: "bytea", nullable: false),
                    global_address = table.Column<string>(type: "text", nullable: true),
                    ancestor_ids = table.Column<List<long>>(type: "bigint[]", nullable: true),
                    parent_ancestor_id = table.Column<long>(type: "bigint", nullable: true),
                    owner_ancestor_id = table.Column<long>(type: "bigint", nullable: true),
                    global_ancestor_id = table.Column<long>(type: "bigint", nullable: true),
                    discriminator = table.Column<EntityType>(type: "entity_type", nullable: false),
                    package_id = table.Column<long>(type: "bigint", nullable: true),
                    blueprint_name = table.Column<string>(type: "text", nullable: true),
                    royalty_vault_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    divisibility = table.Column<int>(type: "integer", nullable: true),
                    non_fungible_id_type = table.Column<NonFungibleIdType>(type: "non_fungible_id_type", nullable: true),
                    code = table.Column<byte[]>(type: "bytea", nullable: true),
                    stake_vault_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    unstake_vault_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    epoch_manager_entity_id = table.Column<long>(type: "bigint", nullable: true),
                    royalty_vault_of_entity_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_access_rules_chain_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    subtype = table.Column<AccessRulesChainSubtype>(type: "access_rules_chain_subtype", nullable: false),
                    access_rules_chain = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_access_rules_chain_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_metadata_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    keys = table.Column<List<string>>(type: "text[]", nullable: false),
                    values = table.Column<List<string>>(type: "text[]", nullable: false)
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
                    tmp_tmp_remove_me_once_tx_events_become_available = table.Column<string>(type: "text", nullable: false),
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
                name: "entity_state_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    state = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_state_history", x => x.id);
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
                name: "ledger_transactions",
                columns: table => new
                {
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<LedgerTransactionStatus>(type: "ledger_transaction_status", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    transaction_accumulator = table.Column<byte[]>(type: "bytea", nullable: false),
                    message = table.Column<byte[]>(type: "bytea", nullable: true),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    round_in_epoch = table.Column<long>(type: "bigint", nullable: false),
                    index_in_epoch = table.Column<long>(type: "bigint", nullable: false),
                    index_in_round = table.Column<long>(type: "bigint", nullable: false),
                    is_end_of_epoch = table.Column<bool>(type: "boolean", nullable: false),
                    referenced_entities = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    fee_paid = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    tip_paid = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    round_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    normalized_round_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    kind_filter_constraint = table.Column<LedgerTransactionKindFilterConstraint>(type: "ledger_transaction_kind_filter_constraint", nullable: true),
                    raw_payload = table.Column<byte[]>(type: "bytea", nullable: false),
                    engine_receipt = table.Column<string>(type: "jsonb", nullable: false),
                    discriminator = table.Column<LedgerTransactionType>(type: "ledger_transaction_type", nullable: false),
                    payload_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                    intent_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                    signed_intent_hash = table.Column<byte[]>(type: "bytea", nullable: true)
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
                    address_type_definitions = table.Column<AddressTypeDefinition[]>(type: "jsonb", nullable: false)
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
                    non_fungible_store_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_resource_manager_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_id = table.Column<string>(type: "text", nullable: false),
                    immutable_data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_non_fungible_id_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "non_fungible_id_mutable_data_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_id_data_id = table.Column<long>(type: "bigint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    mutable_data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_non_fungible_id_mutable_data_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "non_fungible_id_store_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_store_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_resource_manager_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    non_fungible_id_data_ids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_non_fungible_id_store_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pending_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payload_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    intent_hash = table.Column<byte[]>(type: "bytea", nullable: false),
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
                    version_control = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pending_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resource_manager_entity_supply_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    resource_manager_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    total_supply = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_minted = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_burnt = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_manager_entity_supply_history", x => x.id);
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
                name: "ledger_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    top_of_ledger_state_version = table.Column<long>(type: "bigint", nullable: false),
                    sync_status_target_state_version = table.Column<long>(type: "bigint", nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_status", x => x.id);
                    table.ForeignKey(
                        name: "FK_ledger_status_ledger_transactions_top_of_ledger_state_versi~",
                        column: x => x.top_of_ledger_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version");
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
                name: "IX_entities_address",
                table: "entities",
                column: "address")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_entities_global_address",
                table: "entities",
                column: "global_address",
                filter: "global_address IS NOT NULL")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_entity_access_rules_chain_history_entity_id_subtype_from_st~",
                table: "entity_access_rules_chain_history",
                columns: new[] { "entity_id", "subtype", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_metadata_history_entity_id_from_state_version",
                table: "entity_metadata_history",
                columns: new[] { "entity_id", "from_state_version" });

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
                name: "IX_entity_state_history_entity_id_from_state_version",
                table: "entity_state_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_vault_history_global_entity_id_vault_entity_id_from_~",
                table: "entity_vault_history",
                columns: new[] { "global_entity_id", "vault_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_vault_history_owner_entity_id_vault_entity_id_from_s~",
                table: "entity_vault_history",
                columns: new[] { "owner_entity_id", "vault_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_ledger_status_top_of_ledger_state_version",
                table: "ledger_status",
                column: "top_of_ledger_state_version");

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
                name: "IX_ledger_transactions_kind_filter_constraint_state_version",
                table: "ledger_transactions",
                columns: new[] { "kind_filter_constraint", "state_version" },
                filter: "kind_filter_constraint IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_payload_hash",
                table: "ledger_transactions",
                column: "payload_hash",
                filter: "payload_hash IS NOT NULL")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_round_timestamp",
                table: "ledger_transactions",
                column: "round_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_signed_intent_hash",
                table: "ledger_transactions",
                column: "signed_intent_hash",
                filter: "signed_intent_hash IS NOT NULL")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_non_fungible_id_data_non_fungible_resource_manager_entity_~1",
                table: "non_fungible_id_data",
                columns: new[] { "non_fungible_resource_manager_entity_id", "non_fungible_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_non_fungible_id_data_non_fungible_resource_manager_entity_i~",
                table: "non_fungible_id_data",
                columns: new[] { "non_fungible_resource_manager_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_non_fungible_id_mutable_data_history_non_fungible_id_data_i~",
                table: "non_fungible_id_mutable_data_history",
                columns: new[] { "non_fungible_id_data_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_non_fungible_id_store_history_non_fungible_resource_manager~",
                table: "non_fungible_id_store_history",
                columns: new[] { "non_fungible_resource_manager_entity_id", "from_state_version" });

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
                name: "IX_resource_manager_entity_supply_history_resource_manager_ent~",
                table: "resource_manager_entity_supply_history",
                columns: new[] { "resource_manager_entity_id", "from_state_version" });

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
                name: "entities");

            migrationBuilder.DropTable(
                name: "entity_access_rules_chain_history");

            migrationBuilder.DropTable(
                name: "entity_metadata_history");

            migrationBuilder.DropTable(
                name: "entity_resource_aggregate_history");

            migrationBuilder.DropTable(
                name: "entity_resource_aggregated_vaults_history");

            migrationBuilder.DropTable(
                name: "entity_resource_vault_aggregate_history");

            migrationBuilder.DropTable(
                name: "entity_state_history");

            migrationBuilder.DropTable(
                name: "entity_vault_history");

            migrationBuilder.DropTable(
                name: "ledger_status");

            migrationBuilder.DropTable(
                name: "network_configuration");

            migrationBuilder.DropTable(
                name: "non_fungible_id_data");

            migrationBuilder.DropTable(
                name: "non_fungible_id_mutable_data_history");

            migrationBuilder.DropTable(
                name: "non_fungible_id_store_history");

            migrationBuilder.DropTable(
                name: "pending_transactions");

            migrationBuilder.DropTable(
                name: "resource_manager_entity_supply_history");

            migrationBuilder.DropTable(
                name: "validator_active_set_history");

            migrationBuilder.DropTable(
                name: "ledger_transactions");

            migrationBuilder.DropTable(
                name: "validator_public_key_history");
        }
    }
}
