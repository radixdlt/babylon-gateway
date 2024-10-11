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

using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.Abstractions.StandardMetadata;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.ValueConverters;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

/// <summary>
/// Common DB Context for the Network Gateway database.
///
/// After updating this file, run ./generation/generate-migration.sh.
/// </summary>
internal abstract class CommonDbContext : DbContext
{
    internal const string DiscriminatorColumnName = "discriminator";

    public DbSet<LedgerTransaction> LedgerTransactions => Set<LedgerTransaction>();

    public DbSet<LedgerTransactionEvents> LedgerTransactionEvents => Set<LedgerTransactionEvents>();

    public DbSet<LedgerTransactionMarker> LedgerTransactionMarkers => Set<LedgerTransactionMarker>();

    public DbSet<PendingTransaction> PendingTransactions => Set<PendingTransaction>();

    public DbSet<Entity> Entities => Set<Entity>();

    public DbSet<EntityMetadataEntryDefinition> EntityMetadataEntryDefinition => Set<EntityMetadataEntryDefinition>();

    public DbSet<EntityMetadataEntryHistory> EntityMetadataEntryHistory => Set<EntityMetadataEntryHistory>();

    public DbSet<EntityMetadataTotalsHistory> EntityMetadataTotalHistory => Set<EntityMetadataTotalsHistory>();

    public DbSet<PackageBlueprintAggregateHistory> PackageBlueprintAggregateHistory => Set<PackageBlueprintAggregateHistory>();

    public DbSet<PackageCodeAggregateHistory> PackageCodeAggregateHistory => Set<PackageCodeAggregateHistory>();

    public DbSet<AccountLockerEntryDefinition> AccountLockerDefinition => Set<AccountLockerEntryDefinition>();

    public DbSet<AccountLockerTotalsHistory> AccountLockerTotalsHistory => Set<AccountLockerTotalsHistory>();

    public DbSet<AccountLockerEntryResourceVaultDefinition> AccountLockerEntryResourceVaultDefinition => Set<AccountLockerEntryResourceVaultDefinition>();

    public DbSet<AccountLockerEntryTouchHistory> AccountLockerEntryTouchHistory => Set<AccountLockerEntryTouchHistory>();

    public DbSet<AccountDefaultDepositRuleHistory> AccountDefaultDepositRuleHistory => Set<AccountDefaultDepositRuleHistory>();

    public DbSet<AccountResourcePreferenceRuleEntryHistory> AccountResourcePreferenceRuleEntryHistory => Set<AccountResourcePreferenceRuleEntryHistory>();

    public DbSet<AccountResourcePreferenceRuleAggregateHistory> AccountResourcePreferenceRuleAggregateHistory => Set<AccountResourcePreferenceRuleAggregateHistory>();

    public DbSet<ResourceEntitySupplyHistory> ResourceEntitySupplyHistory => Set<ResourceEntitySupplyHistory>();

    public DbSet<NonFungibleIdDefinition> NonFungibleIdDefinition => Set<NonFungibleIdDefinition>();

    public DbSet<NonFungibleIdDataHistory> NonFungibleIdDataHistory => Set<NonFungibleIdDataHistory>();

    public DbSet<NonFungibleIdLocationHistory> NonFungibleIdLocationHistory => Set<NonFungibleIdLocationHistory>();

    public DbSet<StateHistory> StateHistory => Set<StateHistory>();

    public DbSet<ValidatorPublicKeyHistory> ValidatorKeyHistory => Set<ValidatorPublicKeyHistory>();

    public DbSet<ValidatorActiveSetHistory> ValidatorActiveSetHistory => Set<ValidatorActiveSetHistory>();

    public DbSet<EntityRoleAssignmentsOwnerRoleHistory> EntityRoleAssignmentsOwnerHistory => Set<EntityRoleAssignmentsOwnerRoleHistory>();

    public DbSet<EntityRoleAssignmentsEntryHistory> EntityRoleAssignmentsEntryHistory => Set<EntityRoleAssignmentsEntryHistory>();

    public DbSet<EntityRoleAssignmentsAggregateHistory> EntityRoleAssignmentsAggregateHistory => Set<EntityRoleAssignmentsAggregateHistory>();

    public DbSet<ComponentMethodRoyaltyEntryHistory> ComponentEntityMethodRoyaltyEntryHistory => Set<ComponentMethodRoyaltyEntryHistory>();

    public DbSet<ComponentMethodRoyaltyAggregateHistory> ComponentEntityMethodRoyaltyAggregateHistory => Set<ComponentMethodRoyaltyAggregateHistory>();

    public DbSet<PackageBlueprintHistory> PackageBlueprintHistory => Set<PackageBlueprintHistory>();

    public DbSet<PackageCodeHistory> PackageCodeHistory => Set<PackageCodeHistory>();

    public DbSet<SchemaEntryDefinition> SchemaEntryDefinition => Set<SchemaEntryDefinition>();

    public DbSet<SchemaEntryAggregateHistory> SchemaEntryAggregateHistory => Set<SchemaEntryAggregateHistory>();

    public DbSet<KeyValueStoreEntryDefinition> KeyValueStoreEntryDefinition => Set<KeyValueStoreEntryDefinition>();

    public DbSet<KeyValueStoreEntryHistory> KeyValueStoreEntryHistory => Set<KeyValueStoreEntryHistory>();

    public DbSet<KeyValueStoreTotalsHistory> KeyValueStoreTotalsHistory => Set<KeyValueStoreTotalsHistory>();

    public DbSet<ValidatorCumulativeEmissionHistory> ValidatorCumulativeEmissionHistory => Set<ValidatorCumulativeEmissionHistory>();

    public DbSet<NonFungibleSchemaHistory> NonFungibleSchemaHistory => Set<NonFungibleSchemaHistory>();

    public DbSet<KeyValueStoreSchemaHistory> KeyValueStoreSchemaHistory => Set<KeyValueStoreSchemaHistory>();

    public DbSet<AccountAuthorizedDepositorEntryHistory> AccountAuthorizedDepositorEntryHistory => Set<AccountAuthorizedDepositorEntryHistory>();

    public DbSet<AccountAuthorizedDepositorAggregateHistory> AccountAuthorizedDepositorAggregateHistory => Set<AccountAuthorizedDepositorAggregateHistory>();

    public DbSet<UnverifiedStandardMetadataAggregateHistory> UnverifiedStandardMetadataAggregateHistory => Set<UnverifiedStandardMetadataAggregateHistory>();

    public DbSet<UnverifiedStandardMetadataEntryHistory> UnverifiedStandardMetadataEntryHistory => Set<UnverifiedStandardMetadataEntryHistory>();

    public DbSet<ResourceHolder> ResourceHolders => Set<ResourceHolder>();

    public DbSet<EntityResourceEntryDefinition> EntityResourceEntryDefinition => Set<EntityResourceEntryDefinition>();

    public DbSet<EntityResourceTotalsHistory> EntityResourceTotalsHistory => Set<EntityResourceTotalsHistory>();

    public DbSet<EntityResourceBalanceHistory> EntityResourceBalanceHistory => Set<EntityResourceBalanceHistory>();

    public DbSet<EntityResourceVaultEntryDefinition> EntityResourceVaultEntryDefinition => Set<EntityResourceVaultEntryDefinition>();

    public DbSet<EntityResourceVaultTotalsHistory> EntityResourceVaultTotalsHistory => Set<EntityResourceVaultTotalsHistory>();

    public DbSet<VaultBalanceHistory> VaultBalanceHistory => Set<VaultBalanceHistory>();

    public DbSet<NonFungibleVaultEntryDefinition> NonFungibleVaultEntryDefinition => Set<NonFungibleVaultEntryDefinition>();

    public DbSet<NonFungibleVaultEntryHistory> NonFungibleVaultEntryHistory => Set<NonFungibleVaultEntryHistory>();

    public CommonDbContext(DbContextOptions options)
        : base(options)
    {
    }

    // Note that PostGres doesn't have clustered indexes: all indexes are non-clustered, and operate against the heap
    // So secondary indexes might benefit from the inclusion of columns for faster lookups
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<AccountDefaultDepositRule>();
        modelBuilder.HasPostgresEnum<AccountResourcePreferenceRule>();
        modelBuilder.HasPostgresEnum<EntityType>();
        modelBuilder.HasPostgresEnum<EntityRelationship>();
        modelBuilder.HasPostgresEnum<LedgerTransactionStatus>();
        modelBuilder.HasPostgresEnum<LedgerTransactionType>();
        modelBuilder.HasPostgresEnum<LedgerTransactionManifestClass>();
        modelBuilder.HasPostgresEnum<LedgerTransactionMarkerType>();
        modelBuilder.HasPostgresEnum<LedgerTransactionMarkerEventType>();
        modelBuilder.HasPostgresEnum<LedgerTransactionMarkerOperationType>();
        modelBuilder.HasPostgresEnum<LedgerTransactionMarkerTransactionType>();
        modelBuilder.HasPostgresEnum<NonFungibleIdType>();
        modelBuilder.HasPostgresEnum<PackageVmType>();
        modelBuilder.HasPostgresEnum<PendingTransactionPayloadLedgerStatus>();
        modelBuilder.HasPostgresEnum<PendingTransactionIntentLedgerStatus>();
        modelBuilder.HasPostgresEnum<PublicKeyType>();
        modelBuilder.HasPostgresEnum<ResourceType>();
        modelBuilder.HasPostgresEnum<ModuleId>();
        modelBuilder.HasPostgresEnum<SborTypeKind>();
        modelBuilder.HasPostgresEnum<StateType>();
        modelBuilder.HasPostgresEnum<AuthorizedDepositorBadgeType>();
        modelBuilder.HasPostgresEnum<StandardMetadataKey>();

        HookupTransactions(modelBuilder);
        HookupPendingTransactions(modelBuilder);
        HookupDefinitions(modelBuilder);
        HookupHistory(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<TokenAmount>()
            .HaveConversion<TokenAmountToBigIntegerConverter>()
            .HaveColumnType("numeric")
            .HavePrecision(1000);

        configurationBuilder
            .Properties<RadixAddress>()
            .HaveConversion<RadixAddressToByteArrayConverter>();

        configurationBuilder
            .Properties<EntityAddress>()
            .HaveConversion<EntityAddressToStringConverter>();
    }

    private static void HookupTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<LedgerTransaction>()
            .HasDiscriminator<LedgerTransactionType>(DiscriminatorColumnName)
            .HasValue<UserLedgerTransaction>(LedgerTransactionType.User)
            .HasValue<RoundUpdateLedgerTransaction>(LedgerTransactionType.RoundUpdate)
            .HasValue<GenesisLedgerTransaction>(LedgerTransactionType.Genesis)
            .HasValue<FlashLedgerTransaction>(LedgerTransactionType.Flash);

        modelBuilder
            .Entity<UserLedgerTransaction>()
            .HasIndex(lt => lt.IntentHash)
            .HasMethod("hash")
            .HasFilter("intent_hash IS NOT NULL");

        // Because StateVersion, RoundTimestamp and (Epoch, EndOfEpochRound) are correlated with the linear
        // history of the table,  we could consider defining them as a BRIN index, using .HasMethod("brin")
        // This is a lighter (lossy) index where the indexed data is correlated with the linear order of the table
        // See also https://www.postgresql.org/docs/current/indexes-types.html

        // This index lets you quickly translate Time => StateVersion
        modelBuilder
            .Entity<LedgerTransaction>()
            .HasIndex(lt => lt.RoundTimestamp);

        // This index lets you quickly translate Epoch/Round => StateVersion
        modelBuilder
            .Entity<LedgerTransaction>()
            .HasIndex(lt => new { lt.Epoch, lt.RoundInEpoch })
            .IsUnique()
            .HasFilter("index_in_round = 0");

        modelBuilder
            .Entity<LedgerTransactionMarker>()
            .HasDiscriminator<LedgerTransactionMarkerType>(DiscriminatorColumnName)
            .HasValue<EventLedgerTransactionMarker>(LedgerTransactionMarkerType.Event)
            .HasValue<TransactionTypeLedgerTransactionMarker>(LedgerTransactionMarkerType.TransactionType)
            .HasValue<ManifestAddressLedgerTransactionMarker>(LedgerTransactionMarkerType.ManifestAddress)
            .HasValue<ManifestClassMarker>(LedgerTransactionMarkerType.ManifestClass)
            .HasValue<AffectedGlobalEntityTransactionMarker>(LedgerTransactionMarkerType.AffectedGlobalEntity)
            .HasValue<EventGlobalEmitterTransactionMarker>(LedgerTransactionMarkerType.EventGlobalEmitter);

        modelBuilder
            .Entity<LedgerTransactionMarker>()
            .HasIndex(e => e.StateVersion);

        modelBuilder
            .Entity<EventLedgerTransactionMarker>()
            .HasIndex(e => new { e.EventType, e.EntityId, e.StateVersion })
            .HasFilter("discriminator = 'event'");

        modelBuilder
            .Entity<TransactionTypeLedgerTransactionMarker>()
            .HasIndex(e => new { e.TransactionType, e.StateVersion })
            .HasFilter("discriminator = 'transaction_type'");

        modelBuilder
            .Entity<ManifestAddressLedgerTransactionMarker>()
            .HasIndex(e => new { e.OperationType, e.EntityId, e.StateVersion })
            .HasFilter("discriminator = 'manifest_address'");

        modelBuilder
            .Entity<AffectedGlobalEntityTransactionMarker>()
            .HasIndex(e => new { e.EntityId, e.StateVersion })
            .HasFilter("discriminator = 'affected_global_entity'");

        modelBuilder
            .Entity<EventGlobalEmitterTransactionMarker>()
            .HasIndex(e => new { e.EntityId, e.StateVersion })
            .HasFilter("discriminator = 'event_global_emitter'");

        modelBuilder
            .Entity<ManifestClassMarker>()
            .HasIndex(e => new { a = e.ManifestClass, b = e.StateVersion }, "IX_ledger_transaction_markers_manifest_class_is_most_specific")
            .HasFilter("discriminator = 'manifest_class' and is_most_specific = true");

        modelBuilder
            .Entity<ManifestClassMarker>()
            .HasIndex(e => new { c = e.ManifestClass, d = e.StateVersion }, "IX_ledger_transaction_markers_manifest_class")
            .HasFilter("discriminator = 'manifest_class'");
    }

    private static void HookupPendingTransactions(ModelBuilder modelBuilder)
    {
        // The following indices cover the following queries:
        // - LedgerExtenderService:
        //   > MarkCommitted: Lookup by PayloadHash + PayloadStatus
        // - ResubmissionService:
        //   > ForResubmitting: ResubmitFromTimestamp < T
        // - PrunerService:
        //   > PruneIf: FirstSubmittedToGatewayTimestamp < pruneIfLastGatewaySubmissionBefore
        // - TransactionStatusAPI:
        //   > Lookup by intent

        modelBuilder
            .Entity<PendingTransaction>()
            .OwnsOne(
                pt => pt.GatewayHandling,
                builder =>
                {
                    builder.HasIndex(gatewayHandling => gatewayHandling.ResubmitFromTimestamp);
                    builder.HasIndex(gatewayHandling => gatewayHandling.FirstSubmittedToGatewayTimestamp);
                }
            );

        modelBuilder
            .Entity<PendingTransaction>()
            .HasIndex(pt => pt.PayloadHash)
            .IsUnique();

        modelBuilder
            .Entity<PendingTransaction>()
            .HasIndex(pt => pt.IntentHash);
    }

    private static void HookupDefinitions(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Entity>()
            .HasIndex(e => e.Address)
            .IsUnique();

        modelBuilder
            .Entity<Entity>()
            .HasIndex(e => new { e.FromStateVersion })
            .HasFilter("discriminator = 'global_validator'");

        modelBuilder
            .Entity<Entity>()
            .HasDiscriminator<EntityType>(DiscriminatorColumnName)
            .HasValue<GlobalConsensusManager>(EntityType.GlobalConsensusManager)
            .HasValue<GlobalFungibleResourceEntity>(EntityType.GlobalFungibleResource)
            .HasValue<GlobalNonFungibleResourceEntity>(EntityType.GlobalNonFungibleResource)
            .HasValue<GlobalGenericComponentEntity>(EntityType.GlobalGenericComponent)
            .HasValue<InternalGenericComponentEntity>(EntityType.InternalGenericComponent)
            .HasValue<GlobalAccountEntity>(EntityType.GlobalAccountComponent)
            .HasValue<GlobalPackageEntity>(EntityType.GlobalPackage)
            .HasValue<InternalKeyValueStoreEntity>(EntityType.InternalKeyValueStore)
            .HasValue<InternalFungibleVaultEntity>(EntityType.InternalFungibleVault)
            .HasValue<InternalNonFungibleVaultEntity>(EntityType.InternalNonFungibleVault)
            .HasValue<GlobalValidatorEntity>(EntityType.GlobalValidator)
            .HasValue<GlobalAccessControllerEntity>(EntityType.GlobalAccessController)
            .HasValue<GlobalIdentityEntity>(EntityType.GlobalIdentity)
            .HasValue<GlobalOneResourcePoolEntity>(EntityType.GlobalOneResourcePool)
            .HasValue<GlobalTwoResourcePoolEntity>(EntityType.GlobalTwoResourcePool)
            .HasValue<GlobalMultiResourcePoolEntity>(EntityType.GlobalMultiResourcePool)
            .HasValue<GlobalTransactionTrackerEntity>(EntityType.GlobalTransactionTracker)
            .HasValue<GlobalAccountLockerEntity>(EntityType.GlobalAccountLocker);

        modelBuilder
            .Entity<SchemaEntryDefinition>()
            .HasIndex(e => new { e.EntityId, e.SchemaHash });

        modelBuilder
            .Entity<KeyValueStoreEntryDefinition>()
            .HasIndex(e => new { e.KeyValueStoreEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<KeyValueStoreEntryDefinition>()
            .HasIndex(e => new { e.KeyValueStoreEntityId, e.Key });

        modelBuilder
            .Entity<KeyValueStoreTotalsHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<NonFungibleIdDefinition>()
            .HasIndex(e => new { e.NonFungibleResourceEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<NonFungibleIdDefinition>()
            .HasIndex(e => new { e.NonFungibleResourceEntityId, e.NonFungibleId, e.FromStateVersion })
            .IsUnique();

        modelBuilder
            .Entity<AccountLockerEntryDefinition>()
            .HasIndex(e => new { e.AccountLockerEntityId, e.AccountEntityId })
            .IsUnique();

        modelBuilder
            .Entity<AccountLockerEntryResourceVaultDefinition>()
            .HasIndex(e => new { e.AccountLockerDefinitionId, e.FromStateVersion });

        modelBuilder
            .Entity<AccountLockerTotalsHistory>()
            .HasIndex(e => new { e.AccountLockerEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<AccountLockerEntryResourceVaultTotalsHistory>()
            .HasIndex(e => new { e.AccountLockerDefinitionId, e.FromStateVersion });

        modelBuilder
            .Entity<EntityResourceEntryDefinition>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<EntityResourceEntryDefinition>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion }, "IX_entity_resource_entry_definition_fungibles")
            .HasFilter("resource_type = 'fungible'");

        modelBuilder
            .Entity<EntityResourceEntryDefinition>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion }, "IX_entity_resource_entry_definition_non_fungibles")
            .HasFilter("resource_type = 'non_fungible'");

        modelBuilder
            .Entity<EntityResourceVaultEntryDefinition>()
            .HasIndex(e => new { e.EntityId, e.ResourceEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<NonFungibleVaultEntryDefinition>()
            .HasIndex(e => new { e.VaultEntityId, e.FromStateVersion });
    }

    private static void HookupHistory(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<AccountLockerEntryTouchHistory>()
            .HasIndex(e => new { e.AccountLockerDefinitionId, e.FromStateVersion });

        modelBuilder
            .Entity<StateHistory>()
            .HasDiscriminator<StateType>(DiscriminatorColumnName)
            .HasValue<JsonStateHistory>(StateType.Json)
            .HasValue<SborStateHistory>(StateType.Sbor);

        modelBuilder
            .Entity<AccountDefaultDepositRuleHistory>()
            .HasIndex(e => new { e.AccountEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<EntityMetadataEntryHistory>()
            .HasIndex(e => new { e.EntityMetadataEntryDefinitionId, e.FromStateVersion });

        modelBuilder
            .Entity<EntityMetadataEntryDefinition>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<EntityMetadataEntryDefinition>()
            .HasIndex(e => new { e.EntityId, e.Key });

        modelBuilder
            .Entity<EntityMetadataTotalsHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<PackageBlueprintAggregateHistory>()
            .HasIndex(e => new { e.PackageEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<PackageCodeAggregateHistory>()
            .HasIndex(e => new { e.PackageEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<EntityRoleAssignmentsOwnerRoleHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<EntityRoleAssignmentsEntryHistory>()
            .HasIndex(e => new { e.EntityId, e.KeyRole, e.KeyModule, e.FromStateVersion });

        modelBuilder
            .Entity<EntityRoleAssignmentsAggregateHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<ComponentMethodRoyaltyEntryHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<ComponentMethodRoyaltyEntryHistory>()
            .HasIndex(e => new { e.EntityId, e.MethodName, e.FromStateVersion });

        modelBuilder
            .Entity<ComponentMethodRoyaltyAggregateHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<ResourceEntitySupplyHistory>()
            .HasIndex(e => new { e.ResourceEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<NonFungibleIdDataHistory>()
            .HasIndex(e => new { e.NonFungibleIdDefinitionId, e.FromStateVersion });

        modelBuilder
            .Entity<NonFungibleIdLocationHistory>()
            .HasIndex(e => new { NonFungibleIdDataId = e.NonFungibleIdDefinitionId, e.FromStateVersion });

        modelBuilder
            .Entity<StateHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<PackageBlueprintHistory>()
            .HasIndex(e => new { e.PackageEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<PackageBlueprintHistory>()
            .HasIndex(e => new { e.PackageEntityId, e.Name, e.Version, e.FromStateVersion });

        modelBuilder
            .Entity<PackageCodeHistory>()
            .HasIndex(e => new { e.PackageEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<SchemaEntryAggregateHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<ValidatorPublicKeyHistory>()
            .HasIndex(e => new { e.ValidatorEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<ValidatorPublicKeyHistory>()
            .HasIndex(e => new { e.ValidatorEntityId, e.KeyType, e.Key });

        modelBuilder
            .Entity<ValidatorActiveSetHistory>()
            .HasIndex(e => e.FromStateVersion);

        modelBuilder
            .Entity<ValidatorActiveSetHistory>()
            .HasIndex(e => e.Epoch);

        modelBuilder
            .Entity<KeyValueStoreEntryHistory>()
            .HasIndex(e => new { e.KeyValueStoreEntryDefinitionId, e.FromStateVersion });

        modelBuilder
            .Entity<KeyValueStoreSchemaHistory>()
            .HasIndex(e => new { e.KeyValueStoreEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<NonFungibleSchemaHistory>()
            .HasIndex(e => new { e.ResourceEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<AccountAuthorizedDepositorEntryHistory>()
            .HasDiscriminator<AuthorizedDepositorBadgeType>(DiscriminatorColumnName)
            .HasValue<AccountAuthorizedNonFungibleBadgeDepositorEntryHistory>(AuthorizedDepositorBadgeType.NonFungible)
            .HasValue<AccountAuthorizedResourceBadgeDepositorEntryHistory>(AuthorizedDepositorBadgeType.Resource);

        modelBuilder
            .Entity<AccountAuthorizedDepositorEntryHistory>()
            .HasIndex(e => new { e.AccountEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<AccountAuthorizedDepositorAggregateHistory>()
            .HasIndex(e => new { e.AccountEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<AccountResourcePreferenceRuleEntryHistory>()
            .HasIndex(e => new { e.AccountEntityId, e.ResourceEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<AccountResourcePreferenceRuleAggregateHistory>()
            .HasIndex(e => new { e.AccountEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<AccountAuthorizedNonFungibleBadgeDepositorEntryHistory>()
            .HasIndex(e => new { e.AccountEntityId, e.ResourceEntityId, e.NonFungibleId, e.FromStateVersion })
            .HasFilter("discriminator = 'non_fungible'");

        modelBuilder
            .Entity<AccountAuthorizedResourceBadgeDepositorEntryHistory>()
            .HasIndex(e => new { e.AccountEntityId, e.ResourceEntityId, e.FromStateVersion })
            .HasFilter("discriminator = 'resource'");

        modelBuilder
            .Entity<ValidatorCumulativeEmissionHistory>()
            .HasIndex(e => new { e.ValidatorEntityId, e.EpochNumber });

        modelBuilder
            .Entity<ValidatorCumulativeEmissionHistory>()
            .HasIndex(e => new { e.ValidatorEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<UnverifiedStandardMetadataAggregateHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<UnverifiedStandardMetadataEntryHistory>()
            .HasDiscriminator(e => e.Discriminator)
            .HasValue<DappAccountTypeUnverifiedStandardMetadataEntryHistory>(StandardMetadataKey.DappAccountType)
            .HasValue<DappDefinitionUnverifiedStandardMetadataEntryHistory>(StandardMetadataKey.DappDefinition)
            .HasValue<DappDefinitionsUnverifiedStandardMetadataEntryHistory>(StandardMetadataKey.DappDefinitions)
            .HasValue<DappClaimedEntitiesUnverifiedStandardMetadataEntryHistory>(StandardMetadataKey.DappClaimedEntities)
            .HasValue<DappClaimedWebsitesUnverifiedStandardMetadataEntryHistory>(StandardMetadataKey.DappClaimedWebsites)
            .HasValue<DappAccountLockerUnverifiedStandardMetadataEntryHistory>(StandardMetadataKey.DappAccountLocker);

        modelBuilder
            .Entity<UnverifiedStandardMetadataEntryHistory>()
            .HasIndex(e => new { e.EntityId, e.Discriminator, e.FromStateVersion });

        modelBuilder
            .Entity<ResourceHolder>()
            .HasIndex(e => new { e.EntityId, e.ResourceEntityId })
            .IsUnique();

        modelBuilder
            .Entity<ResourceHolder>()
            .HasIndex(e => new { e.EntityId, e.ResourceEntityId, e.Balance });

        modelBuilder
            .Entity<EntityResourceTotalsHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder
            .Entity<EntityResourceBalanceHistory>()
            .HasIndex(e => new { e.EntityId, e.ResourceEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<EntityResourceVaultTotalsHistory>()
            .HasIndex(e => new { e.EntityId, e.ResourceEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<VaultBalanceHistory>()
            .HasIndex(e => new { e.VaultEntityId, e.FromStateVersion });

        modelBuilder
            .Entity<NonFungibleVaultEntryHistory>()
            .HasIndex(e => new { e.NonFungibleVaultEntryDefinitionId, e.FromStateVersion });
    }
}
