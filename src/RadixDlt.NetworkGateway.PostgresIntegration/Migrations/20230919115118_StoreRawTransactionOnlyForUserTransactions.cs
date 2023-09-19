using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RadixDlt.NetworkGateway.PostgresIntegration.Migrations
{
    /// <inheritdoc />
    public partial class StoreRawTransactionOnlyForUserTransactions : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
