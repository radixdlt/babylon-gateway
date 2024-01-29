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

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RadixDlt.NetworkGateway.Abstractions.Model;

#nullable disable

namespace RadixDlt.NetworkGateway.PostgresIntegration.Migrations
{
    /// <inheritdoc />
    public partial class SupportProtocolUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add transaction_trakcer to well known addresses.
            migrationBuilder.Sql(@"UPDATE network_configuration SET well_known_addresses = well_known_addresses || '{""TransactionTracker"": ""transactiontracker_tdx_2_1stxxxxxxxxxxtxtrakxxxxxxxxx006844685494xxxxxxxxxxzw7jp""}'::jsonb  WHERE network_name = 'stokenet'");
            migrationBuilder.Sql(@"UPDATE network_configuration SET well_known_addresses = well_known_addresses || '{""TransactionTracker"": ""transactiontracker_rdx1stxxxxxxxxxxtxtrakxxxxxxxxx006844685494xxxxxxxxxtxtrak""}'::jsonb WHERE network_name = 'mainnet'");

            // Support flash transaction type.
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:ledger_transaction_type", "genesis,user,round_update,flash")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_type", "genesis,user,round_update");

            // Store package vm type on package_code_history
            migrationBuilder.AddColumn<PackageVmType>(
                name: "vm_type",
                table: "package_code_history",
                type: "package_vm_type",
                nullable: false,
                defaultValue: PackageVmType.Native);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "package_code_history",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("update package_code_history pch set vm_type = (select vm_type from entities e where e.id = pch.package_entity_id)");

            migrationBuilder.AlterColumn<PackageVmType>(
                name: "vm_type",
                table: "package_code_history",
                oldDefaultValue: PackageVmType.Native,
                defaultValue: null);

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                table: "package_code_history",
                oldDefaultValue: false,
                defaultValue: null);

            migrationBuilder.DropColumn(
                name: "vm_type",
                table: "entities");

            // Create aggregate for package blueprint.
            migrationBuilder.CreateTable(
                name: "package_blueprint_aggregate_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    package_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    package_blueprint_ids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_package_blueprint_aggregate_history", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_package_blueprint_aggregate_history_package_entity_id_from_~",
                table: "package_blueprint_aggregate_history",
                columns: new[] { "package_entity_id", "from_state_version" });

            migrationBuilder.Sql(@"
     INSERT INTO package_blueprint_aggregate_history (from_state_version, package_entity_id, package_blueprint_ids)
     SELECT MIN(from_state_version) from_state_version, package_entity_id, array_agg(id order by id asc) package_blueprint_ids
     FROM package_blueprint_history
     GROUP BY package_entity_id");

            // Create aggregate for package code.
            migrationBuilder.CreateTable(
                name: "package_code_aggregate_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_state_version = table.Column<long>(type: "bigint", nullable: false),
                    package_entity_id = table.Column<long>(type: "bigint", nullable: false),
                    package_code_ids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_package_code_aggregate_history", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_package_code_aggregate_history_package_entity_id_from_state~",
                table: "package_code_aggregate_history",
                columns: new[] { "package_entity_id", "from_state_version" });

            migrationBuilder.Sql(@"
INSERT INTO package_code_aggregate_history (from_state_version, package_entity_id, package_code_ids)
SELECT MIN(from_state_version) from_state_version, package_entity_id, array_agg(id order by id asc) package_code_ids
FROM package_code_history
GROUP BY package_entity_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<PackageVmType>(
                name: "vm_type",
                table: "entities",
                type: "package_vm_type",
                nullable: true);

            migrationBuilder.Sql("update entities e set vm_type = (select vm_type from package_code_history pch where pch.package_entity_id = e.id)");

            migrationBuilder.AlterColumn<PackageVmType>(
                name: "vm_type",
                table: "entities",
                nullable: false);

            migrationBuilder.DropTable(
                name: "package_blueprint_aggregate_history");

            migrationBuilder.DropTable(
                name: "package_code_aggregate_history");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "package_code_history");

            migrationBuilder.DropColumn(
                name: "vm_type",
                table: "package_code_history");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:ledger_transaction_type", "genesis,user,round_update")
                .OldAnnotation("Npgsql:Enum:ledger_transaction_type", "genesis,user,round_update,flash");
        }
    }
}
