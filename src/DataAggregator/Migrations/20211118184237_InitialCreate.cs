using System;
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
                    table.PrimaryKey("pk_account_resource_balance_history", x => new { x.account_address, x.rri, x.from_state_version });
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
                    table.PrimaryKey("pk_nodes", x => x.name);
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
                    table.PrimaryKey("pk_raw_transactions", x => x.transaction_id);
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
                    table.PrimaryKey("pk_ledger_transactions", x => x.state_version);
                    table.UniqueConstraint("ak_ledger_transactions_transaction_accumulator", x => x.transaction_accumulator);
                    table.UniqueConstraint("ak_ledger_transactions_transaction_id", x => x.transaction_id);
                    table.CheckConstraint("CK_ledger_transactions_CK_CompleteHistory", "state_version = 1 OR state_version = parent_state_version + 1");
                    table.ForeignKey(
                        name: "fk_ledger_transactions_ledger_transactions_parent_state_version",
                        column: x => x.parent_state_version,
                        principalTable: "ledger_transactions",
                        principalColumn: "state_version");
                    table.ForeignKey(
                        name: "fk_ledger_transactions_raw_transactions_transaction_id",
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
                    table.PrimaryKey("pk_operation_groups", x => new { x.state_version, x.operation_group_index });
                    table.ForeignKey(
                        name: "fk_operation_groups_ledger_transactions_state_version",
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
                    table.PrimaryKey("pk_account_resource_balance_substates", x => new { x.up_state_version, x.up_operation_group_index, x.up_operation_index_in_group });
                    table.UniqueConstraint("ak_account_resource_balance_substates_substate_identifier", x => x.substate_identifier);
                    table.ForeignKey(
                        name: "fk_t_substate_down_operation_group",
                        columns: x => new { x.down_state_version, x.down_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_t_substate_up_operation_group",
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
                    table.PrimaryKey("pk_account_stake_ownership_balance_substates", x => new { x.up_state_version, x.up_operation_group_index, x.up_operation_index_in_group });
                    table.UniqueConstraint("ak_account_stake_ownership_balance_substates_substate_identifi", x => x.substate_identifier);
                    table.ForeignKey(
                        name: "fk_t_substate_down_operation_group",
                        columns: x => new { x.down_state_version, x.down_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_t_substate_up_operation_group",
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
                    table.PrimaryKey("pk_account_xrd_stake_balance_substates", x => new { x.up_state_version, x.up_operation_group_index, x.up_operation_index_in_group });
                    table.UniqueConstraint("ak_account_xrd_stake_balance_substates_substate_identifier", x => x.substate_identifier);
                    table.ForeignKey(
                        name: "fk_t_substate_down_operation_group",
                        columns: x => new { x.down_state_version, x.down_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_t_substate_up_operation_group",
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
                    table.PrimaryKey("pk_validator_stake_balance_substates", x => new { x.up_state_version, x.up_operation_group_index, x.up_operation_index_in_group });
                    table.UniqueConstraint("ak_validator_stake_balance_substates_substate_identifier", x => x.substate_identifier);
                    table.ForeignKey(
                        name: "fk_t_substate_down_operation_group",
                        columns: x => new { x.down_state_version, x.down_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_t_substate_up_operation_group",
                        columns: x => new { x.up_state_version, x.up_operation_group_index },
                        principalTable: "operation_groups",
                        principalColumns: new[] { "state_version", "operation_group_index" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_resource_balance_history_account_address_from_state",
                table: "account_resource_balance_history",
                columns: new[] { "account_address", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "ix_account_resource_balance_history_current_balance",
                table: "account_resource_balance_history",
                columns: new[] { "account_address", "rri" },
                unique: true,
                filter: "to_state_version is null");

            migrationBuilder.CreateIndex(
                name: "ix_account_resource_balance_history_rri_account_address_from_s",
                table: "account_resource_balance_history",
                columns: new[] { "rri", "account_address", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "ix_account_resource_balance_history_rri_from_state_version",
                table: "account_resource_balance_history",
                columns: new[] { "rri", "from_state_version" });

            migrationBuilder.CreateIndex(
                name: "ix_account_resource_balance_substates_account_address_rri",
                table: "account_resource_balance_substates",
                columns: new[] { "account_address", "rri" });

            migrationBuilder.CreateIndex(
                name: "ix_account_resource_balance_substates_down_state_version_down_",
                table: "account_resource_balance_substates",
                columns: new[] { "down_state_version", "down_operation_group_index" });

            migrationBuilder.CreateIndex(
                name: "ix_account_resource_balance_substates_rri_account_address",
                table: "account_resource_balance_substates",
                columns: new[] { "rri", "account_address" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountResourceBalanceSubstate_CurrentUnspentUTXOs",
                table: "account_resource_balance_substates",
                columns: new[] { "account_address", "rri", "amount" },
                filter: "down_state_version is null")
                .Annotation("Npgsql:IndexInclude", new[] { "substate_identifier" });

            migrationBuilder.CreateIndex(
                name: "ix_account_stake_ownership_balance_substates_account_address_v",
                table: "account_stake_ownership_balance_substates",
                columns: new[] { "account_address", "validator_address" });

            migrationBuilder.CreateIndex(
                name: "ix_account_stake_ownership_balance_substates_down_state_versio",
                table: "account_stake_ownership_balance_substates",
                columns: new[] { "down_state_version", "down_operation_group_index" });

            migrationBuilder.CreateIndex(
                name: "ix_account_stake_ownership_balance_substates_validator_address",
                table: "account_stake_ownership_balance_substates",
                columns: new[] { "validator_address", "account_address" });

            migrationBuilder.CreateIndex(
                name: "ix_account_xrd_stake_balance_substates_account_address_validat",
                table: "account_xrd_stake_balance_substates",
                columns: new[] { "account_address", "validator_address" });

            migrationBuilder.CreateIndex(
                name: "ix_account_xrd_stake_balance_substates_down_state_version_down",
                table: "account_xrd_stake_balance_substates",
                columns: new[] { "down_state_version", "down_operation_group_index" });

            migrationBuilder.CreateIndex(
                name: "ix_account_xrd_stake_balance_substates_validator_address_accou",
                table: "account_xrd_stake_balance_substates",
                columns: new[] { "validator_address", "account_address" });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transactions_epoch_end_of_round",
                table: "ledger_transactions",
                columns: new[] { "epoch", "end_of_round" },
                unique: true,
                filter: "end_of_round IS NOT NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transactions_parent_state_version",
                table: "ledger_transactions",
                column: "parent_state_version");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transactions_timestamp",
                table: "ledger_transactions",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_validator_stake_balance_substates_down_state_version_down_o",
                table: "validator_stake_balance_substates",
                columns: new[] { "down_state_version", "down_operation_group_index" });

            migrationBuilder.CreateIndex(
                name: "ix_validator_stake_balance_substates_epoch_validator_address",
                table: "validator_stake_balance_substates",
                columns: new[] { "epoch", "validator_address" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_validator_stake_balance_substates_validator_address",
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
