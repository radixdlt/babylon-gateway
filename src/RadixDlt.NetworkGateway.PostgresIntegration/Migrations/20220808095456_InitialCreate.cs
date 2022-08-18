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
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RadixDlt.NetworkGateway.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mempool_transactions",
                columns: table => new
                {
                    payload_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    intent_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    payload = table.Column<byte[]>(type: "bytea", nullable: false),
                    transaction_contents = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    submitted_by_this_gateway = table.Column<bool>(type: "boolean", nullable: false),
                    first_submitted_to_gateway_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    last_submitted_to_gateway_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    last_submitted_to_node_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    last_submitted_to_node_name = table.Column<string>(type: "text", nullable: true),
                    submission_count = table.Column<int>(type: "integer", nullable: false),
                    first_seen_in_mempool_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    last_missing_from_mempool_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    commit_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    failure_explanation = table.Column<string>(type: "text", nullable: true),
                    failure_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mempool_transactions", x => x.payload_hash);
                    table.UniqueConstraint("AK_mempool_transactions_intent_hash", x => x.intent_hash);
                });

            migrationBuilder.CreateTable(
                name: "network_configuration",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    network_name = table.Column<string>(type: "text", nullable: false),
                    account_hrp = table.Column<string>(type: "text", nullable: false),
                    resource_hrp_suffix = table.Column<string>(type: "text", nullable: false),
                    validator_hrp = table.Column<string>(type: "text", nullable: false),
                    node_hrp = table.Column<string>(type: "text", nullable: false),
                    xrd_address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_network_configuration", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "raw_transactions",
                columns: table => new
                {
                    transaction_payload_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    payload = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_transactions", x => x.transaction_payload_hash);
                });

            migrationBuilder.CreateTable(
                name: "ledger_transactions",
                columns: table => new
                {
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    payload_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    intent_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    signed_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    transaction_accumulator = table.Column<byte[]>(type: "bytea", nullable: false),
                    message = table.Column<byte[]>(type: "bytea", nullable: true),
                    fee_paid = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    index_in_epoch = table.Column<long>(type: "bigint", nullable: false),
                    round_in_epoch = table.Column<long>(type: "bigint", nullable: false),
                    is_user_transaction = table.Column<bool>(type: "boolean", nullable: false),
                    is_start_of_epoch = table.Column<bool>(type: "boolean", nullable: false),
                    is_start_of_round = table.Column<bool>(type: "boolean", nullable: false),
                    round_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    created_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    normalized_timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_transactions", x => x.state_version);
                    table.UniqueConstraint("AK_ledger_transactions_intent_hash", x => x.intent_hash);
                    table.UniqueConstraint("AK_ledger_transactions_payload_hash", x => x.payload_hash);
                    table.UniqueConstraint("AK_ledger_transactions_signed_hash", x => x.signed_hash);
                    table.UniqueConstraint("AK_ledger_transactions_transaction_accumulator", x => x.transaction_accumulator);
                    table.ForeignKey(
                        name: "FK_ledger_transactions_raw_transactions_payload_hash",
                        column: x => x.payload_hash,
                        principalTable: "raw_transactions",
                        principalColumn: "transaction_payload_hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address = table.Column<string>(type: "text", nullable: false),
                    public_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_from_transaction",
                        column: x => x.from_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ledger_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    top_of_ledger_state_version = table.Column<long>(type: "bigint", nullable: false),
                    sync_status_target_state_version = table.Column<long>(type: "bigint", nullable: false),
                    last_updated = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_status", x => x.id);
                    table.ForeignKey(
                        name: "FK_ledger_status_top_transactions_state_version",
                        column: x => x.top_of_ledger_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version");
                });

            migrationBuilder.CreateTable(
                name: "resources",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    engine_address = table.Column<byte[]>(type: "bytea", nullable: false),
                    rri = table.Column<string>(type: "text", nullable: false),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resources", x => x.id);
                    table.ForeignKey(
                        name: "FK_resource_from_transaction",
                        column: x => x.from_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "validators",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address = table.Column<string>(type: "text", nullable: false),
                    public_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validators", x => x.id);
                    table.ForeignKey(
                        name: "FK_validator_from_transaction",
                        column: x => x.from_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_transactions",
                columns: table => new
                {
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    is_user_transaction = table.Column<bool>(type: "boolean", nullable: false),
                    is_fee_payer = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_transactions", x => new { x.account_id, x.state_version });
                    table.ForeignKey(
                        name: "FK_account_transactions_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_transactions_ledger_transactions_state_version",
                        column: x => x.state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_resource_balance_history",
                columns: table => new
                {
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    resource_id = table.Column<long>(type: "bigint", nullable: false),
                    balance = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    to_state_version = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_resource_balance_history", x => new { x.account_id, x.resource_id, x.from_state_version });
                    table.ForeignKey(
                        name: "FK_account_resource_balance_history_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_resource_balance_history_from_transaction",
                        column: x => x.from_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_resource_balance_history_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_resource_balance_history_to_transaction",
                        column: x => x.to_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resource_supply_history",
                columns: table => new
                {
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    resource_id = table.Column<long>(type: "bigint", nullable: false),
                    total_supply = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_minted = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_burnt = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    to_state_version = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_supply_history", x => new { x.resource_id, x.from_state_version });
                    table.ForeignKey(
                        name: "FK_resource_supply_history_from_transaction",
                        column: x => x.from_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resource_supply_history_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resource_supply_history_to_transaction",
                        column: x => x.to_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "account_validator_stake_history",
                columns: table => new
                {
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    validator_id = table.Column<long>(type: "bigint", nullable: false),
                    total_stake_units = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_prepared_xrd_stake = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_prepared_unstake_units = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_exiting_xrd_stake = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    to_state_version = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_validator_stake_history", x => new { x.account_id, x.validator_id, x.from_state_version });
                    table.ForeignKey(
                        name: "FK_account_validator_stake_history_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_validator_stake_history_from_transaction",
                        column: x => x.from_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_validator_stake_history_to_transaction",
                        column: x => x.to_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_validator_stake_history_validators_validator_id",
                        column: x => x.validator_id,
                        principalTable: "validators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "validator_proposal_records",
                columns: table => new
                {
                    validator_id = table.Column<long>(type: "bigint", nullable: false),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    proposals_completed = table.Column<long>(type: "bigint", nullable: false),
                    proposals_missed = table.Column<long>(type: "bigint", nullable: false),
                    last_updated_state_version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validator_proposal_records", x => new { x.epoch, x.validator_id });
                    table.ForeignKey(
                        name: "FK_validator_proposal_record_last_updated_transaction",
                        column: x => x.last_updated_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version");
                    table.ForeignKey(
                        name: "FK_validator_proposal_records_validators_validator_id",
                        column: x => x.validator_id,
                        principalTable: "validators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "validator_stake_history",
                columns: table => new
                {
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    validator_id = table.Column<long>(type: "bigint", nullable: false),
                    total_xrd_staked = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_stake_units = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_prepared_xrd_stake = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_prepared_unstake_units = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    total_exiting_xrd_stake = table.Column<BigInteger>(type: "numeric(1000,0)", precision: 1000, scale: 0, nullable: false),
                    to_state_version = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_validator_stake_history", x => new { x.validator_id, x.from_state_version });
                    table.ForeignKey(
                        name: "FK_validator_stake_history_from_transaction",
                        column: x => x.from_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_validator_stake_history_to_transaction",
                        column: x => x.to_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_validator_stake_history_validators_validator_id",
                        column: x => x.validator_id,
                        principalTable: "validators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_account_id_from_state_vers~",
                table: "account_resource_balance_history",
                columns: new[] { "account_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_current_balance",
                table: "account_resource_balance_history",
                columns: new[] { "account_id", "resource_id" },
                unique: true,
                filter: "to_state_version is null");

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_from_state_version",
                table: "account_resource_balance_history",
                column: "from_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_resource_id_account_id_fro~",
                table: "account_resource_balance_history",
                columns: new[] { "resource_id", "account_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_resource_id_from_state_ver~",
                table: "account_resource_balance_history",
                columns: new[] { "resource_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_account_resource_balance_history_to_state_version",
                table: "account_resource_balance_history",
                column: "to_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_account_transaction_user_transactions",
                table: "account_transactions",
                columns: new[] { "account_id", "state_version" },
                unique: true,
                filter: "is_user_transaction = true");

            migrationBuilder.CreateIndex(
                name: "IX_account_transactions_state_version",
                table: "account_transactions",
                column: "state_version");

            migrationBuilder.CreateIndex(
                name: "IX_account_validator_stake_history_account_id_from_state_versi~",
                table: "account_validator_stake_history",
                columns: new[] { "account_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_account_validator_stake_history_current_stake",
                table: "account_validator_stake_history",
                columns: new[] { "account_id", "validator_id" },
                unique: true,
                filter: "to_state_version is null");

            migrationBuilder.CreateIndex(
                name: "IX_account_validator_stake_history_from_state_version",
                table: "account_validator_stake_history",
                column: "from_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_account_validator_stake_history_to_state_version",
                table: "account_validator_stake_history",
                column: "to_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_account_validator_stake_history_validator_id_account_id_fro~",
                table: "account_validator_stake_history",
                columns: new[] { "validator_id", "account_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_account_validator_stake_history_validator_id_from_state_ver~",
                table: "account_validator_stake_history",
                columns: new[] { "validator_id", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_address",
                table: "accounts",
                column: "address",
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "id" });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_from_state_version",
                table: "accounts",
                column: "from_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_status_top_of_ledger_state_version",
                table: "ledger_status",
                column: "top_of_ledger_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transaction_epoch_starts",
                table: "ledger_transactions",
                column: "epoch",
                unique: true,
                filter: "is_start_of_epoch = true");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transaction_round_starts",
                table: "ledger_transactions",
                columns: new[] { "epoch", "round_in_epoch" },
                unique: true,
                filter: "is_start_of_round = true");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transaction_round_timestamp",
                table: "ledger_transactions",
                column: "round_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transaction_user_transactions",
                table: "ledger_transactions",
                column: "state_version",
                unique: true,
                filter: "is_user_transaction = true");

            migrationBuilder.CreateIndex(
                name: "IX_mempool_transactions_status",
                table: "mempool_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_resource_supply_history_current_supply",
                table: "resource_supply_history",
                column: "resource_id",
                unique: true,
                filter: "to_state_version is null");

            migrationBuilder.CreateIndex(
                name: "IX_resource_supply_history_from_state_version",
                table: "resource_supply_history",
                column: "from_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_resource_supply_history_to_state_version",
                table: "resource_supply_history",
                column: "to_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_resources_from_state_version",
                table: "resources",
                column: "from_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_resources_rri",
                table: "resources",
                column: "rri",
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "id" });

            migrationBuilder.CreateIndex(
                name: "IX_validator_proposal_records_last_updated_state_version",
                table: "validator_proposal_records",
                column: "last_updated_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_validator_proposal_records_validator_id_epoch",
                table: "validator_proposal_records",
                columns: new[] { "validator_id", "epoch" });

            migrationBuilder.CreateIndex(
                name: "IX_validator_stake_history_current_stake",
                table: "validator_stake_history",
                column: "validator_id",
                unique: true,
                filter: "to_state_version is null");

            migrationBuilder.CreateIndex(
                name: "IX_validator_stake_history_from_state_version",
                table: "validator_stake_history",
                column: "from_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_validator_stake_history_to_state_version",
                table: "validator_stake_history",
                column: "to_state_version");

            migrationBuilder.CreateIndex(
                name: "IX_validators_address",
                table: "validators",
                column: "address",
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "id" });

            migrationBuilder.CreateIndex(
                name: "IX_validators_from_state_version",
                table: "validators",
                column: "from_state_version");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_resource_balance_history");

            migrationBuilder.DropTable(
                name: "account_transactions");

            migrationBuilder.DropTable(
                name: "account_validator_stake_history");

            migrationBuilder.DropTable(
                name: "ledger_status");

            migrationBuilder.DropTable(
                name: "mempool_transactions");

            migrationBuilder.DropTable(
                name: "network_configuration");

            migrationBuilder.DropTable(
                name: "resource_supply_history");

            migrationBuilder.DropTable(
                name: "validator_proposal_records");

            migrationBuilder.DropTable(
                name: "validator_stake_history");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "resources");

            migrationBuilder.DropTable(
                name: "validators");

            migrationBuilder.DropTable(
                name: "ledger_transactions");

            migrationBuilder.DropTable(
                name: "raw_transactions");
        }
    }
}
