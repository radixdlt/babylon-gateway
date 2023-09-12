using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RadixDlt.NetworkGateway.PostgresIntegration.Migrations
{
    /// <inheritdoc />
    public partial class MarkSchemaDefiningColumnsAsRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE key_value_store_schema_history SET value_schema_defining_entity_id = key_value_store_entity_id");
            migrationBuilder.Sql("UPDATE key_value_store_schema_history SET key_schema_defining_entity_id = key_value_store_entity_id");
            migrationBuilder.Sql("UPDATE non_fungible_schema_history SET schema_defining_entity_id = resource_entity_id");

            migrationBuilder.AlterColumn<long>(
                name: "schema_defining_entity_id",
                table: "non_fungible_schema_history",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "value_schema_defining_entity_id",
                table: "key_value_store_schema_history",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "key_schema_defining_entity_id",
                table: "key_value_store_schema_history",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "schema_defining_entity_id",
                table: "non_fungible_schema_history",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "value_schema_defining_entity_id",
                table: "key_value_store_schema_history",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "key_schema_defining_entity_id",
                table: "key_value_store_schema_history",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
