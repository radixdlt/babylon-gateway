using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAggregator.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    fee_paid = table.Column<string>(type: "text", nullable: false),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    index_in_epoch = table.Column<int>(type: "integer", nullable: false),
                    is_end_of_epoch = table.Column<bool>(type: "boolean", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_of_round = table.Column<int>(type: "integer", nullable: true)
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
                    inferred_action_amount = table.Column<string>(type: "text", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transactions_epoch_end_of_round",
                table: "ledger_transactions",
                columns: new[] { "epoch", "end_of_round" },
                unique: true,
                filter: "end_of_round IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transactions_parent_state_version",
                table: "ledger_transactions",
                column: "parent_state_version");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transactions_timestamp",
                table: "ledger_transactions",
                column: "timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nodes");

            migrationBuilder.DropTable(
                name: "operation_groups");

            migrationBuilder.DropTable(
                name: "ledger_transactions");

            migrationBuilder.DropTable(
                name: "raw_transactions");
        }
    }
}
