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

#nullable disable

namespace DataAggregator.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_resource_balance_history",
                columns: table => new
                {
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    account_address = table.Column<string>(type: "text", nullable: false),
                    rri = table.Column<string>(type: "text", nullable: false),
                    balance = table.Column<BigInteger>(type: "numeric(1000)", precision: 1000, nullable: false),
                    to_state_version = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_resource_balance_history", x => new { x.account_address, x.rri, x.from_state_version });
                });

            migrationBuilder.CreateTable(
                name: "nodes",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    trust_weighting = table.Column<decimal>(type: "numeric", nullable: false),
                    enabled_for_indexing = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nodes", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "raw_transactions",
                columns: table => new
                {
                    transaction_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    submitted_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payload = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_transactions", x => x.transaction_id);
                });

            migrationBuilder.CreateTable(
                name: "ledger_transactions",
                columns: table => new
                {
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    parent_state_version = table.Column<long>(type: "bigint", nullable: true),
                    transaction_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    transaction_accumulator = table.Column<byte[]>(type: "bytea", nullable: false),
                    message = table.Column<byte[]>(type: "bytea", nullable: true),
                    fee_paid = table.Column<BigInteger>(type: "numeric(1000)", precision: 1000, nullable: false),
                    signed_by = table.Column<byte[]>(type: "bytea", nullable: true),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    index_in_epoch = table.Column<long>(type: "bigint", nullable: false),
                    is_only_round_change = table.Column<bool>(type: "boolean", nullable: false),
                    is_end_of_epoch = table.Column<bool>(type: "boolean", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_of_round = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_transactions", x => x.state_version);
                    table.UniqueConstraint("AK_ledger_transactions_transaction_accumulator", x => x.transaction_accumulator);
                    table.UniqueConstraint("AK_ledger_transactions_transaction_id", x => x.transaction_id);
                    table.CheckConstraint("CK_ledger_transactions_complete_history", "state_version = 1 OR state_version = parent_state_version + 1");
                    table.ForeignKey(
                        name: "FK_ledger_transactions_ledger_transactions_parent_state_version",
                        column: x => x.parent_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version");
                    table.ForeignKey(
                        name: "FK_ledger_transactions_raw_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "raw_transactions",
                        principalColumn: "transaction_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "operation_groups",
                columns: table => new
                {
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    operation_group_index = table.Column<int>(type: "integer", nullable: false),
                    inferred_action_type = table.Column<string>(type: "text", nullable: true),
                    inferred_action_from = table.Column<string>(type: "text", nullable: true),
                    inferred_action_to = table.Column<string>(type: "text", nullable: true),
                    inferred_action_amount = table.Column<BigInteger>(type: "numeric(1000)", precision: 1000, nullable: true),
                    inferred_action_rri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_groups", x => new { x.state_version, x.operation_group_index });
                    table.ForeignKey(
                        name: "FK_operation_groups_ledger_transactions_state_version",
                        column: x => x.state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_resource_balance_substates",
                columns: table => new
                {
                    up_state_version = table.Column<long>(type: "bigint", nullable: false),
                    up_operation_group_index = table.Column<int>(type: "integer", nullable: false),
                    up_operation_index_in_group = table.Column<int>(type: "integer", nullable: false),
                    account_address = table.Column<string>(type: "text", nullable: false),
                    rri = table.Column<string>(type: "text", nullable: true),
                    down_state_version = table.Column<long>(type: "bigint", nullable: true),
                    down_operation_group_index = table.Column<int>(type: "integer", nullable: true),
                    down_operation_index_in_group = table.Column<int>(type: "integer", nullable: true),
                    substate_identifier = table.Column<byte[]>(type: "bytea", nullable: false),
                    amount = table.Column<BigInteger>(type: "numeric(1000)", precision: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_resource_balance_substates", x => new { x.up_state_version, x.up_operation_group_index, x.up_operation_index_in_group });
                    table.UniqueConstraint("AK_account_resource_balance_substates_substate_identifier", x => x.substate_identifier);
                    table.ForeignKey(
                        name: "FK_account_resource_balance_substate_down_operation_group",
                        columns: x => new { x.down_state_version, x.down_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_resource_balance_substate_up_operation_group",
                        columns: x => new { x.up_state_version, x.up_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_stake_ownership_balance_substates",
                columns: table => new
                {
                    up_state_version = table.Column<long>(type: "bigint", nullable: false),
                    up_operation_group_index = table.Column<int>(type: "integer", nullable: false),
                    up_operation_index_in_group = table.Column<int>(type: "integer", nullable: false),
                    account_address = table.Column<string>(type: "text", nullable: false),
                    validator_address = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    down_state_version = table.Column<long>(type: "bigint", nullable: true),
                    down_operation_group_index = table.Column<int>(type: "integer", nullable: true),
                    down_operation_index_in_group = table.Column<int>(type: "integer", nullable: true),
                    substate_identifier = table.Column<byte[]>(type: "bytea", nullable: false),
                    amount = table.Column<BigInteger>(type: "numeric(1000)", precision: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_stake_ownership_balance_substates", x => new { x.up_state_version, x.up_operation_group_index, x.up_operation_index_in_group });
                    table.UniqueConstraint("AK_account_stake_ownership_balance_substates_substate_identifi~", x => x.substate_identifier);
                    table.ForeignKey(
                        name: "FK_account_stake_ownership_balance_substate_down_operation_group",
                        columns: x => new { x.down_state_version, x.down_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_stake_ownership_balance_substate_up_operation_group",
                        columns: x => new { x.up_state_version, x.up_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_xrd_stake_balance_substates",
                columns: table => new
                {
                    up_state_version = table.Column<long>(type: "bigint", nullable: false),
                    up_operation_group_index = table.Column<int>(type: "integer", nullable: false),
                    up_operation_index_in_group = table.Column<int>(type: "integer", nullable: false),
                    account_address = table.Column<string>(type: "text", nullable: false),
                    validator_address = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    unlock_epoch = table.Column<long>(type: "bigint", nullable: true),
                    down_state_version = table.Column<long>(type: "bigint", nullable: true),
                    down_operation_group_index = table.Column<int>(type: "integer", nullable: true),
                    down_operation_index_in_group = table.Column<int>(type: "integer", nullable: true),
                    substate_identifier = table.Column<byte[]>(type: "bytea", nullable: false),
                    amount = table.Column<BigInteger>(type: "numeric(1000)", precision: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_xrd_stake_balance_substates", x => new { x.up_state_version, x.up_operation_group_index, x.up_operation_index_in_group });
                    table.UniqueConstraint("AK_account_xrd_stake_balance_substates_substate_identifier", x => x.substate_identifier);
                    table.ForeignKey(
                        name: "FK_account_xrd_stake_balance_substate_down_operation_group",
                        columns: x => new { x.down_state_version, x.down_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_xrd_stake_balance_substate_up_operation_group",
                        columns: x => new { x.up_state_version, x.up_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "validator_stake_balance_substates",
                columns: table => new
                {
                    up_state_version = table.Column<long>(type: "bigint", nullable: false),
                    up_operation_group_index = table.Column<int>(type: "integer", nullable: false),
                    up_operation_index_in_group = table.Column<int>(type: "integer", nullable: false),
                    validator_address = table.Column<string>(type: "text", nullable: false),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    down_state_version = table.Column<long>(type: "bigint", nullable: true),
                    down_operation_group_index = table.Column<int>(type: "integer", nullable: true),
                    down_operation_index_in_group = table.Column<int>(type: "integer", nullable: true),
                    substate_identifier = table.Column<byte[]>(type: "bytea", nullable: false),
                    amount = table.Column<BigInteger>(type: "numeric(1000)", precision: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validator_stake_balance_substates", x => new { x.up_state_version, x.up_operation_group_index, x.up_operation_index_in_group });
                    table.UniqueConstraint("AK_validator_stake_balance_substates_substate_identifier", x => x.substate_identifier);
                    table.ForeignKey(
                        name: "FK_validator_stake_balance_substate_down_operation_group",
                        columns: x => new { x.down_state_version, x.down_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_validator_stake_balance_substate_up_operation_group",
                        columns: x => new { x.up_state_version, x.up_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_account_address_from_state~",
                table: "account_resource_balance_history",
                columns: new[] { "account_address", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_current_balance",
                table: "account_resource_balance_history",
                columns: new[] { "account_address", "rri" },
                unique: true,
                filter: "to_state_version is null");

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_rri_account_address_from_s~",
                table: "account_resource_balance_history",
                columns: new[] { "rri", "account_address", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_rri_from_state_version",
                table: "account_resource_balance_history",
                columns: new[] { "rri", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_substates_account_address_rri",
                table: "account_resource_balance_substates",
                columns: new[] { "account_address", "rri" });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_substates_down_state_version_down_~",
                table: "account_resource_balance_substates",
                columns: new[] { "down_state_version", "down_operation_group_index" });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_substates_rri_account_address",
                table: "account_resource_balance_substates",
                columns: new[] { "rri", "account_address" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountResourceBalanceSubstate_CurrentUnspentUTXOs",
                table: "account_resource_balance_substates",
                columns: new[] { "account_address", "rri", "amount" },
                filter: "down_state_version is null")
                .Annotation("Npgsql:IndexInclude", new[] { "substate_identifier" });

            migrationBuilder.CreateIndex(
                name: "IX_account_stake_ownership_balance_substates_account_address_v~",
                table: "account_stake_ownership_balance_substates",
                columns: new[] { "account_address", "validator_address" });

            migrationBuilder.CreateIndex(
                name: "IX_account_stake_ownership_balance_substates_down_state_versio~",
                table: "account_stake_ownership_balance_substates",
                columns: new[] { "down_state_version", "down_operation_group_index" });

            migrationBuilder.CreateIndex(
                name: "IX_account_stake_ownership_balance_substates_validator_address~",
                table: "account_stake_ownership_balance_substates",
                columns: new[] { "validator_address", "account_address" });

            migrationBuilder.CreateIndex(
                name: "IX_account_xrd_stake_balance_substates_account_address_validat~",
                table: "account_xrd_stake_balance_substates",
                columns: new[] { "account_address", "validator_address" });

            migrationBuilder.CreateIndex(
                name: "IX_account_xrd_stake_balance_substates_down_state_version_down~",
                table: "account_xrd_stake_balance_substates",
                columns: new[] { "down_state_version", "down_operation_group_index" });

            migrationBuilder.CreateIndex(
                name: "IX_account_xrd_stake_balance_substates_validator_address_accou~",
                table: "account_xrd_stake_balance_substates",
                columns: new[] { "validator_address", "account_address" });

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_epoch_end_of_round",
                table: "ledger_transactions",
                columns: new[] { "epoch", "end_of_round" },
                unique: true,
                filter: "end_of_round IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_parent_state_version",
                table: "ledger_transactions",
                column: "parent_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_timestamp",
                table: "ledger_transactions",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_validator_stake_balance_substates_down_state_version_down_o~",
                table: "validator_stake_balance_substates",
                columns: new[] { "down_state_version", "down_operation_group_index" });

            migrationBuilder.CreateIndex(
                name: "IX_validator_stake_balance_substates_epoch_validator_address",
                table: "validator_stake_balance_substates",
                columns: new[] { "epoch", "validator_address" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_validator_stake_balance_substates_validator_address",
                table: "validator_stake_balance_substates",
                column: "validator_address");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_resource_balance_history");

            migrationBuilder.DropTable(
                name: "account_resource_balance_substates");

            migrationBuilder.DropTable(
                name: "account_stake_ownership_balance_substates");

            migrationBuilder.DropTable(
                name: "account_xrd_stake_balance_substates");

            migrationBuilder.DropTable(
                name: "nodes");

            migrationBuilder.DropTable(
                name: "validator_stake_balance_substates");

            migrationBuilder.DropTable(
                name: "operation_groups");

            migrationBuilder.DropTable(
                name: "ledger_transactions");

            migrationBuilder.DropTable(
                name: "raw_transactions");
        }
    }
}
