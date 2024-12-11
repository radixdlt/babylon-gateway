using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RadixDlt.NetworkGateway.PostgresIntegration.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoundUpdateTransactionMarkers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM ledger_transaction_markers WHERE transaction_type = 'round_change';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
