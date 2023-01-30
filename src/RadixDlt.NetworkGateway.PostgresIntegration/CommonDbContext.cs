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

using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
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
    public DbSet<NetworkConfiguration> NetworkConfiguration => Set<NetworkConfiguration>();

    public DbSet<RawUserTransaction> RawUserTransactions => Set<RawUserTransaction>();

    public DbSet<LedgerStatus> LedgerStatus => Set<LedgerStatus>();

    public DbSet<LedgerTransaction> LedgerTransactions => Set<LedgerTransaction>();

    public DbSet<PendingTransaction> PendingTransactions => Set<PendingTransaction>();

    public DbSet<Entity> Entities => Set<Entity>();

    public DbSet<EntityResourceAggregateHistory> EntityResourceAggregateHistory => Set<EntityResourceAggregateHistory>();

    public DbSet<EntityResourceHistory> EntityResourceHistory => Set<EntityResourceHistory>();

    public DbSet<ResourceManagerEntitySupplyHistory> ResourceManagerEntitySupplyHistory => Set<ResourceManagerEntitySupplyHistory>();

    public DbSet<NonFungibleIdData> NonFungibleIdData => Set<NonFungibleIdData>();

    public DbSet<NonFungibleIdMutableDataHistory> NonFungibleIdMutableDataHistory => Set<NonFungibleIdMutableDataHistory>();

    public DbSet<NonFungibleIdStoreHistory> NonFungibleIdStoreHistory => Set<NonFungibleIdStoreHistory>();

    public DbSet<ComponentEntityStateHistory> ComponentEntityStateHistory => Set<ComponentEntityStateHistory>();

    public DbSet<EntityAccessRulesChainHistory> EntityAccessRulesLayersHistory => Set<EntityAccessRulesChainHistory>();

    public CommonDbContext(DbContextOptions options)
        : base(options)
    {
    }

    // Note that PostGres doesn't have clustered indexes: all indexes are non-clustered, and operate against the heap
    // So secondary indexes might benefit from the inclusion of columns for faster lookups
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<AccessRulesChainSubtype>();
        modelBuilder.HasPostgresEnum<LedgerTransactionStatus>();
        modelBuilder.HasPostgresEnum<NonFungibleIdType>();
        modelBuilder.HasPostgresEnum<PendingTransactionStatus>();

        HookupSingleEntries(modelBuilder);
        HookupTransactions(modelBuilder);
        HookupPendingTransactions(modelBuilder);
        HookupEntities(modelBuilder);
        HookupHistory(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TokenAmount>()
            .HaveConversion<TokenAmountToBigIntegerConverter>()
            .HaveColumnType("numeric")
            .HavePrecision(1000, 0);

        configurationBuilder.Properties<RadixAddress>()
            .HaveConversion<RadixAddressToByteArrayConverter>();
    }

    private static void HookupSingleEntries(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerStatus>()
            .HasOne(ls => ls.TopOfLedgerTransaction)
            .WithMany()
            .HasForeignKey(ls => ls.TopOfLedgerStateVersion)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void HookupTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerTransaction>()
            .HasDiscriminator<string>("discriminator")
            .HasValue<UserLedgerTransaction>("user")
            .HasValue<ValidatorLedgerTransaction>("validator")
            .HasValue<SystemLedgerTransaction>("system");

        modelBuilder.Entity<UserLedgerTransaction>()
            .HasIndex(lt => lt.PayloadHash)
            .HasMethod("hash")
            .HasFilter("payload_hash IS NOT NULL");
        modelBuilder.Entity<UserLedgerTransaction>()
            .HasIndex(lt => lt.IntentHash)
            .HasMethod("hash")
            .HasFilter("intent_hash IS NOT NULL");
        modelBuilder.Entity<UserLedgerTransaction>()
            .HasIndex(lt => lt.SignedIntentHash)
            .HasMethod("hash")
            .HasFilter("signed_intent_hash IS NOT NULL");

        // Because StateVersion, RoundTimestamp and (Epoch, EndOfEpochRound) are correlated with the linear
        // history of the table,  we could consider defining them as a BRIN index, using .HasMethod("brin")
        // This is a lighter (lossy) index where the indexed data is correlated with the linear order of the table
        // See also https://www.postgresql.org/docs/current/indexes-types.html

        // This index lets you quickly translate Time => StateVersion
        modelBuilder.Entity<LedgerTransaction>()
            .HasIndex(lt => lt.RoundTimestamp);

        // This index lets you quickly translate Epoch/Round => StateVersion
        modelBuilder.Entity<LedgerTransaction>()
            .HasIndex(lt => new { lt.Epoch, lt.RoundInEpoch })
            .IsUnique()
            .HasFilter("index_in_round = 0");
    }

    private static void HookupPendingTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PendingTransaction>()
            .HasIndex(pt => pt.Status);

        modelBuilder.Entity<PendingTransaction>()
            .HasIndex(pt => pt.PayloadHash)
            .HasMethod("hash");

        modelBuilder.Entity<PendingTransaction>()
            .HasIndex(pt => pt.IntentHash)
            .HasMethod("hash");
    }

    private static void HookupEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity>()
            .HasDiscriminator<string>("discriminator")
            .HasValue<EpochManagerEntity>("epoch_manager")
            .HasValue<FungibleResourceManagerEntity>("fungible_resource_manager")
            .HasValue<NonFungibleResourceManagerEntity>("non_fungible_resource_manager")
            .HasValue<NormalComponentEntity>("normal_component")
            .HasValue<AccountComponentEntity>("account_component")
            .HasValue<ValidatorComponentEntity>("validator_component")
            .HasValue<PackageEntity>("package")
            .HasValue<KeyValueStoreEntity>("key_value_store")
            .HasValue<VaultEntity>("vault")
            .HasValue<NonFungibleStoreEntity>("non_fungible_store")
            .HasValue<ClockEntity>("clock");

        modelBuilder.Entity<Entity>()
            .HasIndex(e => e.Address)
            .HasMethod("hash");

        modelBuilder.Entity<Entity>()
            .HasIndex(e => e.GlobalAddress)
            .HasMethod("hash")
            .HasFilter("global_address IS NOT NULL");
    }

    private static void HookupHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityMetadataHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder.Entity<EntityResourceAggregateHistory>()
            .HasIndex(e => new { e.EntityId, e.FromStateVersion });

        modelBuilder.Entity<EntityResourceHistory>()
            .HasDiscriminator<string>("discriminator")
            .HasValue<EntityFungibleResourceHistory>("fungible")
            .HasValue<EntityNonFungibleResourceHistory>("non_fungible");

        modelBuilder.Entity<EntityResourceHistory>()
            .HasIndex(e => new { e.OwnerEntityId, e.FromStateVersion });

        modelBuilder.Entity<EntityResourceHistory>()
            .HasIndex(e => new { e.GlobalEntityId, e.FromStateVersion });

        modelBuilder.Entity<ResourceManagerEntitySupplyHistory>()
            .HasIndex(e => new { e.ResourceManagerEntityId, e.FromStateVersion });

        modelBuilder.Entity<NonFungibleIdData>()
            .HasIndex(e => new { e.NonFungibleResourceManagerEntityId, e.FromStateVersion });

        modelBuilder.Entity<NonFungibleIdData>()
            .HasIndex(e => new { e.NonFungibleResourceManagerEntityId, e.NonFungibleId, e.FromStateVersion });

        modelBuilder.Entity<NonFungibleIdMutableDataHistory>()
            .HasIndex(e => new { e.NonFungibleIdDataId, e.FromStateVersion });

        modelBuilder.Entity<NonFungibleIdStoreHistory>()
            .HasIndex(e => new { e.NonFungibleResourceManagerEntityId, e.FromStateVersion });

        modelBuilder.Entity<ComponentEntityStateHistory>()
            .HasIndex(e => new { e.ComponentEntityId, e.FromStateVersion });

        modelBuilder.Entity<EntityAccessRulesChainHistory>()
            .HasIndex(e => new { e.EntityId, e.Subtype, e.FromStateVersion });
    }
}
