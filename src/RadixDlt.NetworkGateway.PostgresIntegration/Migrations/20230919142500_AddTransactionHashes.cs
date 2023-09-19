using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RadixDlt.NetworkGateway.PostgresIntegration.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "raw_payload",
                table: "ledger_transactions",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea");

            migrationBuilder.AddColumn<string>(
                name: "receipt_tree_hash",
                table: "ledger_transactions",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "state_tree_hash",
                table: "ledger_transactions",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "transaction_tree_hash",
                table: "ledger_transactions",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "receipt_tree_hash",
                table: "ledger_transactions");

            migrationBuilder.DropColumn(
                name: "state_tree_hash",
                table: "ledger_transactions");

            migrationBuilder.DropColumn(
                name: "transaction_tree_hash",
                table: "ledger_transactions");

            migrationBuilder.AlterColumn<byte[]>(
                name: "raw_payload",
                table: "ledger_transactions",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);
        }
    }
}
