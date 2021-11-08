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
                    transaction_index = table.Column<long>(type: "bigint", nullable: false),
                    parent_transaction_index = table.Column<long>(type: "bigint", nullable: true),
                    transaction_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    transaction_accumulator = table.Column<byte[]>(type: "bytea", nullable: false),
                    state_version = table.Column<long>(type: "bigint", nullable: false),
                    message = table.Column<byte[]>(type: "bytea", nullable: true),
                    fee_paid = table.Column<string>(type: "text", precision: 1000, scale: 18, nullable: false),
                    epoch = table.Column<long>(type: "bigint", nullable: false),
                    index_in_epoch = table.Column<int>(type: "integer", nullable: false),
                    is_end_of_epoch = table.Column<bool>(type: "boolean", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ledger_transactions", x => x.transaction_index);
                    table.CheckConstraint("CK_ledger_transactions_CK_CompleteHistory", "transaction_index = 0 OR transaction_index = parent_transaction_index + 1");
                    table.ForeignKey(
                        name: "fk_ledger_transactions_ledger_transactions_parent_transaction_",
                        column: x => x.parent_transaction_index,
                        principalTable: "ledger_transactions",
                        principalColumn: "transaction_index");
                    table.ForeignKey(
                        name: "fk_ledger_transactions_raw_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "raw_transactions",
                        principalColumn: "transaction_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transactions_parent_transaction_index",
                table: "ledger_transactions",
                column: "parent_transaction_index");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transactions_transaction_id",
                table: "ledger_transactions",
                column: "transaction_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ledger_transactions");

            migrationBuilder.DropTable(
                name: "nodes");

            migrationBuilder.DropTable(
                name: "raw_transactions");
        }
    }
}
