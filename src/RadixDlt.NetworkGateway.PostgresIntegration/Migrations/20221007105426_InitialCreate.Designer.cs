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

﻿// <auto-generated />
using System;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RadixDlt.NetworkGateway.PostgresIntegration;

#nullable disable

namespace RadixDlt.NetworkGateway.PostgresIntegration.Migrations
{
    [DbContext(typeof(MigrationsDbContext))]
    [Migration("20221007105426_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.Entity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<byte[]>("Address")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("address");

                    b.Property<long>("FromStateVersion")
                        .HasColumnType("bigint")
                        .HasColumnName("from_state_version");

                    b.Property<byte[]>("GlobalAddress")
                        .HasColumnType("bytea")
                        .HasColumnName("global_address");

                    b.Property<long?>("GlobalAncestorId")
                        .HasColumnType("bigint")
                        .HasColumnName("global_ancestor_id");

                    b.Property<long?>("OwnerAncestorId")
                        .HasColumnType("bigint")
                        .HasColumnName("owner_ancestor_id");

                    b.Property<long?>("ParentId")
                        .HasColumnType("bigint")
                        .HasColumnName("parent_id");

                    b.Property<string>("type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Address");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Address"), "hash");

                    b.HasIndex("GlobalAddress");

                    b.ToTable("entities");

                    b.HasDiscriminator<string>("type").HasValue("Entity");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.EntityMetadataHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("EntityId")
                        .HasColumnType("bigint")
                        .HasColumnName("entity_id");

                    b.Property<long>("FromStateVersion")
                        .HasColumnType("bigint")
                        .HasColumnName("from_state_version");

                    b.Property<string[]>("Keys")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("keys");

                    b.Property<string[]>("Values")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasColumnName("values");

                    b.HasKey("Id");

                    b.ToTable("entity_metadata_history");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.EntityResourceAggregateHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("EntityId")
                        .HasColumnType("bigint")
                        .HasColumnName("entity_id");

                    b.Property<long>("FromStateVersion")
                        .HasColumnType("bigint")
                        .HasColumnName("from_state_version");

                    b.Property<long[]>("FungibleResourceIds")
                        .IsRequired()
                        .HasColumnType("bigint[]")
                        .HasColumnName("fungible_resource_ids");

                    b.Property<bool>("IsMostRecent")
                        .HasColumnType("boolean")
                        .HasColumnName("is_most_recent");

                    b.Property<long[]>("NonFungibleResourceIds")
                        .IsRequired()
                        .HasColumnType("bigint[]")
                        .HasColumnName("non_fungible_resource_ids");

                    b.HasKey("Id");

                    b.HasIndex("EntityId", "FromStateVersion");

                    b.HasIndex("IsMostRecent", "EntityId");

                    b.ToTable("entity_resource_aggregate_history");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.EntityResourceHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("FromStateVersion")
                        .HasColumnType("bigint")
                        .HasColumnName("from_state_version");

                    b.Property<long>("GlobalEntityId")
                        .HasColumnType("bigint")
                        .HasColumnName("global_entity_id");

                    b.Property<long>("OwnerEntityId")
                        .HasColumnType("bigint")
                        .HasColumnName("owner_entity_id");

                    b.Property<long>("ResourceEntityId")
                        .HasColumnType("bigint")
                        .HasColumnName("resource_entity_id");

                    b.Property<string>("type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GlobalEntityId", "FromStateVersion");

                    b.HasIndex("OwnerEntityId", "FromStateVersion");

                    b.ToTable("entity_resource_history");

                    b.HasDiscriminator<string>("type").HasValue("EntityResourceHistory");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.FungibleResourceSupplyHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("FromStateVersion")
                        .HasColumnType("bigint")
                        .HasColumnName("from_state_version");

                    b.Property<long>("ResourceEntityId")
                        .HasColumnType("bigint")
                        .HasColumnName("resource_entity_id");

                    b.Property<BigInteger>("TotalBurnt")
                        .HasPrecision(1000)
                        .HasColumnType("numeric(1000,0)")
                        .HasColumnName("total_burnt");

                    b.Property<BigInteger>("TotalMinted")
                        .HasPrecision(1000)
                        .HasColumnType("numeric(1000,0)")
                        .HasColumnName("total_minted");

                    b.Property<BigInteger>("TotalSupply")
                        .HasPrecision(1000)
                        .HasColumnType("numeric(1000,0)")
                        .HasColumnName("total_supply");

                    b.HasKey("Id");

                    b.HasIndex("ResourceEntityId", "FromStateVersion");

                    b.ToTable("fungible_resource_supply_history");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.LedgerStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_updated");

                    b.Property<long>("TopOfLedgerStateVersion")
                        .HasColumnType("bigint")
                        .HasColumnName("top_of_ledger_state_version");

                    b.HasKey("Id");

                    b.HasIndex("TopOfLedgerStateVersion");

                    b.ToTable("ledger_status");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.LedgerTransaction", b =>
                {
                    b.Property<long>("StateVersion")
                        .HasColumnType("bigint")
                        .HasColumnName("state_version");

                    b.Property<DateTimeOffset>("CreatedTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_timestamp");

                    b.Property<long>("Epoch")
                        .HasColumnType("bigint")
                        .HasColumnName("epoch");

                    b.Property<BigInteger>("FeePaid")
                        .HasPrecision(1000)
                        .HasColumnType("numeric(1000,0)")
                        .HasColumnName("fee_paid");

                    b.Property<long>("IndexInEpoch")
                        .HasColumnType("bigint")
                        .HasColumnName("index_in_epoch");

                    b.Property<byte[]>("IntentHash")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("intent_hash");

                    b.Property<bool>("IsStartOfEpoch")
                        .HasColumnType("boolean")
                        .HasColumnName("is_start_of_epoch");

                    b.Property<bool>("IsStartOfRound")
                        .HasColumnType("boolean")
                        .HasColumnName("is_start_of_round");

                    b.Property<bool>("IsUserTransaction")
                        .HasColumnType("boolean")
                        .HasColumnName("is_user_transaction");

                    b.Property<byte[]>("Message")
                        .HasColumnType("bytea")
                        .HasColumnName("message");

                    b.Property<DateTimeOffset>("NormalizedRoundTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("normalized_timestamp");

                    b.Property<byte[]>("PayloadHash")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("payload_hash");

                    b.Property<long[]>("ReferencedEntities")
                        .IsRequired()
                        .HasColumnType("bigint[]")
                        .HasColumnName("referenced_entities");

                    b.Property<long>("RoundInEpoch")
                        .HasColumnType("bigint")
                        .HasColumnName("round_in_epoch");

                    b.Property<DateTimeOffset>("RoundTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("round_timestamp");

                    b.Property<byte[]>("SignedTransactionHash")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("signed_hash");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<BigInteger>("TipPaid")
                        .HasPrecision(1000)
                        .HasColumnType("numeric(1000,0)")
                        .HasColumnName("tip_paid");

                    b.Property<byte[]>("TransactionAccumulator")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("transaction_accumulator");

                    b.HasKey("StateVersion");

                    b.HasAlternateKey("IntentHash");

                    b.HasAlternateKey("PayloadHash");

                    b.HasAlternateKey("SignedTransactionHash");

                    b.HasAlternateKey("TransactionAccumulator");

                    b.HasIndex("Epoch")
                        .IsUnique()
                        .HasDatabaseName("IX_ledger_transaction_epoch_starts")
                        .HasFilter("is_start_of_epoch = true");

                    b.HasIndex("RoundTimestamp")
                        .HasDatabaseName("IX_ledger_transaction_round_timestamp");

                    b.HasIndex("StateVersion")
                        .IsUnique()
                        .HasDatabaseName("IX_ledger_transaction_user_transactions")
                        .HasFilter("is_user_transaction = true");

                    b.HasIndex("Epoch", "RoundInEpoch")
                        .IsUnique()
                        .HasDatabaseName("IX_ledger_transaction_round_starts")
                        .HasFilter("is_start_of_round = true");

                    b.ToTable("ledger_transactions");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.MempoolTransaction", b =>
                {
                    b.Property<byte[]>("PayloadHash")
                        .HasColumnType("bytea")
                        .HasColumnName("payload_hash");

                    b.Property<DateTimeOffset?>("CommitTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("commit_timestamp");

                    b.Property<string>("FailureExplanation")
                        .HasColumnType("text")
                        .HasColumnName("failure_explanation");

                    b.Property<string>("FailureReason")
                        .HasColumnType("text")
                        .HasColumnName("failure_reason");

                    b.Property<DateTimeOffset?>("FailureTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("failure_timestamp");

                    b.Property<DateTimeOffset?>("FirstSeenInMempoolTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("first_seen_in_mempool_timestamp");

                    b.Property<DateTimeOffset?>("FirstSubmittedToGatewayTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("first_submitted_to_gateway_timestamp");

                    b.Property<byte[]>("IntentHash")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("intent_hash");

                    b.Property<DateTimeOffset?>("LastDroppedOutOfMempoolTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_missing_from_mempool_timestamp");

                    b.Property<DateTimeOffset?>("LastSubmittedToGatewayTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_submitted_to_gateway_timestamp");

                    b.Property<string>("LastSubmittedToNodeName")
                        .HasColumnType("text")
                        .HasColumnName("last_submitted_to_node_name");

                    b.Property<DateTimeOffset?>("LastSubmittedToNodeTimestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_submitted_to_node_timestamp");

                    b.Property<byte[]>("Payload")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("payload");

                    b.Property<string>("Status")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<int>("SubmissionToNodesCount")
                        .HasColumnType("integer")
                        .HasColumnName("submission_count");

                    b.Property<bool>("SubmittedByThisGateway")
                        .HasColumnType("boolean")
                        .HasColumnName("submitted_by_this_gateway");

                    b.Property<string>("TransactionContents")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("transaction_contents");

                    b.HasKey("PayloadHash");

                    b.HasAlternateKey("IntentHash");

                    b.HasIndex("Status");

                    b.ToTable("mempool_transactions");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.NetworkConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.HasKey("Id");

                    b.ToTable("network_configuration");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.RawTransaction", b =>
                {
                    b.Property<long>("StateVersion")
                        .HasColumnType("bigint")
                        .HasColumnName("state_version");

                    b.Property<byte[]>("Payload")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("payload");

                    b.Property<byte[]>("TransactionPayloadHash")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("transaction_payload_hash");

                    b.HasKey("StateVersion");

                    b.ToTable("raw_transactions");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.ComponentEntity", b =>
                {
                    b.HasBaseType("RadixDlt.NetworkGateway.PostgresIntegration.Models.Entity");

                    b.Property<string>("Kind")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("kind");

                    b.ToTable("entities");

                    b.HasDiscriminator().HasValue("component");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.EntityFungibleResourceHistory", b =>
                {
                    b.HasBaseType("RadixDlt.NetworkGateway.PostgresIntegration.Models.EntityResourceHistory");

                    b.Property<BigInteger>("Balance")
                        .HasPrecision(1000)
                        .HasColumnType("numeric(1000,0)")
                        .HasColumnName("balance");

                    b.ToTable("entity_resource_history");

                    b.HasDiscriminator().HasValue("fungible");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.EntityNonFungibleResourceHistory", b =>
                {
                    b.HasBaseType("RadixDlt.NetworkGateway.PostgresIntegration.Models.EntityResourceHistory");

                    b.Property<long[]>("Ids")
                        .IsRequired()
                        .HasColumnType("bigint[]")
                        .HasColumnName("ids");

                    b.Property<long>("IdsCount")
                        .HasColumnType("bigint")
                        .HasColumnName("ids_count");

                    b.ToTable("entity_resource_history");

                    b.HasDiscriminator().HasValue("non_fungible");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.PackageEntity", b =>
                {
                    b.HasBaseType("RadixDlt.NetworkGateway.PostgresIntegration.Models.Entity");

                    b.ToTable("entities");

                    b.HasDiscriminator().HasValue("package");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.ResourceManagerEntity", b =>
                {
                    b.HasBaseType("RadixDlt.NetworkGateway.PostgresIntegration.Models.Entity");

                    b.ToTable("entities");

                    b.HasDiscriminator().HasValue("resource_manager");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.SystemEntity", b =>
                {
                    b.HasBaseType("RadixDlt.NetworkGateway.PostgresIntegration.Models.Entity");

                    b.ToTable("entities");

                    b.HasDiscriminator().HasValue("system");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.ValueStoreEntity", b =>
                {
                    b.HasBaseType("RadixDlt.NetworkGateway.PostgresIntegration.Models.Entity");

                    b.ToTable("entities");

                    b.HasDiscriminator().HasValue("value_store");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.VaultEntity", b =>
                {
                    b.HasBaseType("RadixDlt.NetworkGateway.PostgresIntegration.Models.Entity");

                    b.ToTable("entities");

                    b.HasDiscriminator().HasValue("vault");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.LedgerStatus", b =>
                {
                    b.HasOne("RadixDlt.NetworkGateway.PostgresIntegration.Models.LedgerTransaction", "TopOfLedgerTransaction")
                        .WithMany()
                        .HasForeignKey("TopOfLedgerStateVersion")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("FK_ledger_status_top_transactions_state_version");

                    b.OwnsOne("RadixDlt.NetworkGateway.PostgresIntegration.Models.SyncTarget", "SyncTarget", b1 =>
                        {
                            b1.Property<int>("LedgerStatusId")
                                .HasColumnType("integer");

                            b1.Property<long>("TargetStateVersion")
                                .HasColumnType("bigint")
                                .HasColumnName("sync_status_target_state_version");

                            b1.HasKey("LedgerStatusId");

                            b1.ToTable("ledger_status");

                            b1.WithOwner()
                                .HasForeignKey("LedgerStatusId");
                        });

                    b.Navigation("SyncTarget")
                        .IsRequired();

                    b.Navigation("TopOfLedgerTransaction");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.LedgerTransaction", b =>
                {
                    b.HasOne("RadixDlt.NetworkGateway.PostgresIntegration.Models.RawTransaction", "RawTransaction")
                        .WithMany()
                        .HasForeignKey("StateVersion")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RawTransaction");
                });

            modelBuilder.Entity("RadixDlt.NetworkGateway.PostgresIntegration.Models.NetworkConfiguration", b =>
                {
                    b.OwnsOne("RadixDlt.NetworkGateway.PostgresIntegration.Models.NetworkAddressHrps", "NetworkAddressHrps", b1 =>
                        {
                            b1.Property<int>("NetworkConfigurationId")
                                .HasColumnType("integer");

                            b1.Property<string>("AccountHrp")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("account_hrp");

                            b1.Property<string>("NodeHrp")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("node_hrp");

                            b1.Property<string>("ResourceHrpSuffix")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("resource_hrp_suffix");

                            b1.Property<string>("ValidatorHrp")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("validator_hrp");

                            b1.HasKey("NetworkConfigurationId");

                            b1.ToTable("network_configuration");

                            b1.WithOwner()
                                .HasForeignKey("NetworkConfigurationId");
                        });

                    b.OwnsOne("RadixDlt.NetworkGateway.PostgresIntegration.Models.NetworkDefinition", "NetworkDefinition", b1 =>
                        {
                            b1.Property<int>("NetworkConfigurationId")
                                .HasColumnType("integer");

                            b1.Property<string>("NetworkName")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("network_name");

                            b1.HasKey("NetworkConfigurationId");

                            b1.ToTable("network_configuration");

                            b1.WithOwner()
                                .HasForeignKey("NetworkConfigurationId");
                        });

                    b.OwnsOne("RadixDlt.NetworkGateway.PostgresIntegration.Models.WellKnownAddresses", "WellKnownAddresses", b1 =>
                        {
                            b1.Property<int>("NetworkConfigurationId")
                                .HasColumnType("integer");

                            b1.Property<string>("XrdAddress")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("xrd_address");

                            b1.HasKey("NetworkConfigurationId");

                            b1.ToTable("network_configuration");

                            b1.WithOwner()
                                .HasForeignKey("NetworkConfigurationId");
                        });

                    b.Navigation("NetworkAddressHrps")
                        .IsRequired();

                    b.Navigation("NetworkDefinition")
                        .IsRequired();

                    b.Navigation("WellKnownAddresses")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}