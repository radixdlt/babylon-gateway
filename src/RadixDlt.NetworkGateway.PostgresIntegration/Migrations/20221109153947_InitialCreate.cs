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
using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RadixDlt.NetworkGateway.PostgresIntegration.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entities",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    address = table.Column<byte[]>(type: "bytea", nullable: false),
                    global_address = table.Column<byte[]>(type: "bytea", nullable: true),
                    ancestor_ids = table.Column<long[]>(type: "bigint[]", nullable: true),
                    parent_ancestor_id = table.Column<long>(type: "bigint", nullable: true),
                    owner_ancestor_id = table.Column<long>(type: "bigint", nullable: true),
                    global_ancestor_id = table.Column<long>(type: "bigint", nullable: true),
                    discriminator = table.Column<string>(type: "text", nullable: false),
                    package_id = table.Column<long>(type: "bigint", nullable: true),
                    divisibility = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_metadata_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    keys = table.Column<string[]>(type: "text[]", nullable: false),
                    values = table.Column<string[]>(type: "text[]", nullable: false)
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
                    is_most_recent = table.Column<bool>(type: "boolean", nullable: false),
                    fungible_resource_ids = table.Column<long[]>(type: "bigint[]", nullable: false),
                    non_fungible_resource_ids = table.Column<long[]>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_resource_aggregate_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_resource_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    owner_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    global_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    discriminator = table.Column<string>(type: "text", nullable: false),
                    balance = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: true),
                    ids_count = table.Column<long>(type: "bigint", nullable: true),
                    ids = table.Column<byte[][]>(type: "bytea[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_resource_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fungible_resource_supply_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    resource_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    total_supply = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_minted = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_burnt = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fungible_resource_supply_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ledger_transactions",
                columns: table => new
                {
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    transaction_accumulator = table.Column<byte[]>(type: "bytea", nullable: false),
                    message = table.Column<byte[]>(type: "bytea", nullable: true),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    index_in_epoch = table.Column<long>(type: "bigint", nullable: false),
                    round_in_epoch = table.Column<long>(type: "bigint", nullable: false),
                    is_start_of_epoch = table.Column<bool>(type: "boolean", nullable: false),
                    is_start_of_round = table.Column<bool>(type: "boolean", nullable: false),
                    referenced_entities = table.Column<long[]>(type: "bigint[]", nullable: false),
                    fee_paid = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    tip_paid = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    round_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    normalized_round_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    discriminator = table.Column<string>(type: "text", nullable: false),
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
                    network_name = table.Column<string>(type: "text", nullable: false),
                    package_hrp = table.Column<string>(type: "text", nullable: false),
                    normal_component_hrp = table.Column<string>(type: "text", nullable: false),
                    account_component_hrp = table.Column<string>(type: "text", nullable: false),
                    system_component_hrp = table.Column<string>(type: "text", nullable: false),
                    resource_hrp = table.Column<string>(type: "text", nullable: false),
                    validator_hrp = table.Column<string>(type: "text", nullable: false),
                    node_hrp = table.Column<string>(type: "text", nullable: false),
                    xrd_address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_network_configuration", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pending_transactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payload_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    intent_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    signed_intent_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    notarized_transaction_blob = table.Column<byte[]>(type: "bytea", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    submitted_by_this_gateway = table.Column<bool>(type: "boolean", nullable: false),
                    first_submitted_to_gateway_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_submitted_to_gateway_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_submitted_to_node_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_submitted_to_node_name = table.Column<string>(type: "text", nullable: true),
                    submission_count = table.Column<int>(type: "integer", nullable: false),
                    first_seen_in_mempool_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_missing_from_mempool_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    commit_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    failure_explanation = table.Column<string>(type: "text", nullable: true),
                    failure_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pending_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "raw_transactions",
                columns: table => new
                {
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    payload_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    payload = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_transactions", x => x.state_version);
                });

            migrationBuilder.CreateTable(
                name: "ledger_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    top_of_ledger_state_version = table.Column<long>(type: "bigint", nullable: false),
                    sync_status_target_state_version = table.Column<long>(type: "bigint", nullable: false),
                    last_updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                name: "IX_entity_metadata_history_entity_id_from_state_version",
                table: "entity_metadata_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_resource_aggregate_history_entity_id_from_state_vers~",
                table: "entity_resource_aggregate_history",
                columns: new[] { "entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_resource_aggregate_history_is_most_recent_entity_id",
                table: "entity_resource_aggregate_history",
                columns: new[] { "is_most_recent", "entity_id" },
                filter: "is_most_recent IS TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_entity_resource_history_global_entity_id_from_state_version",
                table: "entity_resource_history",
                columns: new[] { "global_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_resource_history_owner_entity_id_from_state_version",
                table: "entity_resource_history",
                columns: new[] { "owner_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_fungible_resource_supply_history_resource_entity_id_from_st~",
                table: "fungible_resource_supply_history",
                columns: new[] { "resource_entity_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_ledger_status_top_of_ledger_state_version",
                table: "ledger_status",
                column: "top_of_ledger_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_epoch",
                table: "ledger_transactions",
                column: "epoch",
                unique: true,
                filter: "is_start_of_epoch = true");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_epoch_round_in_epoch",
                table: "ledger_transactions",
                columns: new[] { "epoch", "round_in_epoch" },
                unique: true,
                filter: "is_start_of_round = true");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_intent_hash",
                table: "ledger_transactions",
                column: "intent_hash",
                filter: "intent_hash IS NOT NULL")
                .Annotation("Npgsql:IndexMethod", "hash");

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
                name: "IX_pending_transactions_payload_hash",
                table: "pending_transactions",
                column: "payload_hash")
                .Annotation("Npgsql:IndexMethod", "hash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entities");

            migrationBuilder.DropTable(
                name: "entity_metadata_history");

            migrationBuilder.DropTable(
                name: "entity_resource_aggregate_history");

            migrationBuilder.DropTable(
                name: "entity_resource_history");

            migrationBuilder.DropTable(
                name: "fungible_resource_supply_history");

            migrationBuilder.DropTable(
                name: "ledger_status");

            migrationBuilder.DropTable(
                name: "network_configuration");

            migrationBuilder.DropTable(
                name: "pending_transactions");

            migrationBuilder.DropTable(
                name: "raw_transactions");

            migrationBuilder.DropTable(
                name: "ledger_transactions");
        }
    }
}
