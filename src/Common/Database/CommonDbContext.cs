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

using Common.Database.Models;
using Common.Database.Models.Ledger;
using Common.Database.Models.Ledger.History;
using Common.Database.Models.Ledger.Normalization;
using Common.Database.Models.Ledger.Substates;
using Common.Database.ValueConverters;
using Common.Extensions;
using Common.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Common.Database;

/// <summary>
/// Common DB Context for the radixdlt-network-gateway database.
/// </summary>
public class CommonDbContext : DbContext
{
#pragma warning disable CS1591 // Remove need for public docs - instead refer to the Model docs

    public DbSet<Node> Nodes => Set<Node>();

    public DbSet<RawTransaction> RawTransactions => Set<RawTransaction>();

    public DbSet<LedgerTransaction> LedgerTransactions => Set<LedgerTransaction>();

    public DbSet<LedgerOperationGroup> OperationGroups => Set<LedgerOperationGroup>();

    public DbSet<AccountResourceBalanceSubstate> AccountResourceBalanceSubstates => Set<AccountResourceBalanceSubstate>();

    public DbSet<AccountXrdStakeBalanceSubstate> AccountXrdStakeBalanceSubstates => Set<AccountXrdStakeBalanceSubstate>();

    public DbSet<AccountStakeOwnershipBalanceSubstate> AccountStakeOwnershipBalanceSubstates => Set<AccountStakeOwnershipBalanceSubstate>();

    public DbSet<ValidatorStakeBalanceSubstate> ValidatorStakeBalanceSubstates => Set<ValidatorStakeBalanceSubstate>();

    public DbSet<ResourceDataSubstate> ResourceDataSubstates => Set<ResourceDataSubstate>();

    public DbSet<ValidatorDataSubstate> ValidatorDataSubstate => Set<ValidatorDataSubstate>();

    public DbSet<AccountResourceBalanceHistory> AccountResourceBalanceHistoryEntries => Set<AccountResourceBalanceHistory>();

    public DbSet<ResourceSupplyHistory> ResourceSupplyHistoryEntries => Set<ResourceSupplyHistory>();

    public DbSet<ValidatorStakeHistory> ValidatorStakeHistoryEntries => Set<ValidatorStakeHistory>();

    public DbSet<AccountValidatorStakeHistory> AccountValidatorStakeHistoryEntries => Set<AccountValidatorStakeHistory>();

#pragma warning restore CS1591

    public CommonDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerTransaction>()
            .HasAlternateKey(lt => lt.TransactionIdentifierHash);

        modelBuilder.Entity<LedgerTransaction>()
            .HasAlternateKey(lt => lt.TransactionAccumulator);

        // Because Timestamp and (Epoch, EndOfEpochRound) are correlated with the linear history of the table,
        // we could consider defining them as a BRIN index, using .HasMethod("brin")
        // This is a lighter (lossy) index where the indexed data is correlated with the linear order of the table
        // See also https://www.postgresql.org/docs/current/indexes-types.html
        modelBuilder.Entity<LedgerTransaction>()
            .HasIndex(lt => lt.Timestamp);

        modelBuilder.Entity<LedgerTransaction>()
            .HasIndex(lt => new { lt.Epoch, lt.EndOfEpochRound })
            .IsUnique()
            .IncludeProperties(s => new { s.Timestamp })
            .HasFilter("end_of_round IS NOT NULL");

        modelBuilder.Entity<LedgerOperationGroup>()
            .HasKey(og => new { og.ResultantStateVersion, og.OperationGroupIndex });

        modelBuilder.Entity<LedgerOperationGroup>()
            .OwnsOne(og => og.InferredAction);

        modelBuilder.Entity<Resource>()
            .HasIndex(r => r.ResourceIdentifier)
            .IsUnique();

        modelBuilder.Entity<Account>()
            .HasIndex(r => r.Address)
            .IsUnique();

        modelBuilder.Entity<Validator>()
            .HasIndex(r => r.Address)
            .IsUnique();

        HookUpSubstate<AccountResourceBalanceSubstate>(modelBuilder);
        modelBuilder.Entity<AccountResourceBalanceSubstate>()
            .HasIndex(s => new { s.AccountId, s.ResourceId, s.Amount })
            .IncludeProperties(s => new { s.SubstateIdentifier })
            .HasFilter("down_state_version is null")
            .HasDatabaseName($"IX_{nameof(AccountResourceBalanceSubstate).ToSnakeCase()}_current_unspent_utxos");

        HookUpSubstate<AccountXrdStakeBalanceSubstate>(modelBuilder);
        HookUpSubstate<AccountStakeOwnershipBalanceSubstate>(modelBuilder);
        HookUpSubstate<ValidatorStakeBalanceSubstate>(modelBuilder);

        HookUpSubstate<ResourceDataSubstate>(modelBuilder);
        HookUpSubstate<ValidatorDataSubstate>(modelBuilder);

        HookUpAccountResourceBalanceHistory(modelBuilder);
        HookUpResourceSupplyHistory(modelBuilder);
        HookUpValidatorStakeHistory(modelBuilder);
        HookUpAccountValidatorStakeHistory(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TokenAmount>()
            .HaveConversion<TokenAmountToBigIntegerConverter>()
            .HaveColumnType("numeric")
            .HavePrecision(1000);

        configurationBuilder.Properties<AccountStakeOwnershipBalanceSubstateType>()
            .HaveConversion<AccountStakeOwnershipBalanceSubstateTypeValueConverter>();

        configurationBuilder.Properties<AccountXrdStakeBalanceSubstateType>()
            .HaveConversion<AccountXrdStakeBalanceSubstateTypeValueConverter>();

        configurationBuilder.Properties<ResourceDataSubstateType>()
            .HaveConversion<ResourceDataSubstateTypeValueConverter>();

        configurationBuilder.Properties<ValidatorDataSubstateType>()
            .HaveConversion<ValidatorDataSubstateTypeValueConverter>();
    }

    private static void HookUpAccountResourceBalanceHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasKey(h => new { h.AccountId, h.ResourceId, h.FromStateVersion });

        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasIndex(h => new { h.AccountId, h.ResourceId })
            .HasFilter("to_state_version is null")
            .IsUnique()
            .HasDatabaseName($"IX_{nameof(AccountResourceBalanceHistory).ToSnakeCase()}_current_balance");

        // All four of these indices (these three plus the implicit index from the PK) could be useful for different queries
        // TODO:NG-39 Remove any which aren't important for the APIs we're exposing
        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasIndex(h => new { h.AccountId, h.FromStateVersion });
        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasIndex(h => new { h.ResourceId, h.AccountId, h.FromStateVersion });
        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasIndex(h => new { h.ResourceId, h.FromStateVersion });
    }

    private static void HookUpResourceSupplyHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResourceSupplyHistory>()
            .HasKey(h => new { h.ResourceId, h.FromStateVersion });

        modelBuilder.Entity<ResourceSupplyHistory>()
            .HasIndex(h => new { h.ResourceId })
            .HasFilter("to_state_version is null")
            .IsUnique()
            .HasDatabaseName($"IX_{nameof(ResourceSupplyHistory).ToSnakeCase()}_current_supply");
    }

    private static void HookUpValidatorStakeHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ValidatorStakeHistory>()
            .HasKey(h => new { h.ValidatorId, h.FromStateVersion });

        modelBuilder.Entity<ValidatorStakeHistory>()
            .HasIndex(h => new { h.ValidatorId })
            .HasFilter("to_state_version is null")
            .IsUnique()
            .HasDatabaseName($"IX_{nameof(ValidatorStakeHistory).ToSnakeCase()}_current_stake");
    }

    private static void HookUpAccountValidatorStakeHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountValidatorStakeHistory>()
            .HasKey(h => new { h.AccountId, h.ValidatorId, h.FromStateVersion });

        modelBuilder.Entity<AccountValidatorStakeHistory>()
            .HasIndex(h => new { h.AccountId, h.ValidatorId })
            .HasFilter("to_state_version is null")
            .IsUnique()
            .HasDatabaseName($"IX_{nameof(AccountValidatorStakeHistory).ToSnakeCase()}_current_stake");

        // All four of these indices (these three plus the implicit index from the PK) could be useful for different queries
        // TODO:NG-39 Remove any which aren't important for the APIs we're exposing
        modelBuilder.Entity<AccountValidatorStakeHistory>()
            .HasIndex(h => new { h.AccountId, h.FromStateVersion });
        modelBuilder.Entity<AccountValidatorStakeHistory>()
            .HasIndex(h => new { h.ValidatorId, h.AccountId, h.FromStateVersion });
        modelBuilder.Entity<AccountValidatorStakeHistory>()
            .HasIndex(h => new { h.ValidatorId, h.FromStateVersion });
    }

    private static void HookUpSubstate<TSubstate>(ModelBuilder modelBuilder)
        where TSubstate : SubstateBase
    {
        var substateNameSnakeCase = typeof(TSubstate).Name.ToSnakeCase();

        modelBuilder.Entity<TSubstate>()
            .HasKey(s => new { s.UpStateVersion, s.UpOperationGroupIndex, s.UpOperationIndexInGroup });

        modelBuilder.Entity<TSubstate>()
            .HasAlternateKey(s => s.SubstateIdentifier);

        modelBuilder.Entity<TSubstate>()
            .HasOne(s => s.UpOperationGroup)
            .WithMany()
            .HasForeignKey(s => new
            {
                ResultantStateVersion = s.UpStateVersion,
                OperationGroupIndex = s.UpOperationGroupIndex,
            })
            .OnDelete(DeleteBehavior.Cascade) // Deletes Substate if OperationGroup deleted
            .HasConstraintName($"FK_{substateNameSnakeCase}_up_operation_group");

        modelBuilder.Entity<TSubstate>()
            .HasOne(s => s.DownOperationGroup)
            .WithMany()
            .HasForeignKey(s => new
            {
                ResultantStateVersion = s.DownStateVersion,
                OperationGroupIndex = s.DownOperationGroupIndex,
            })
            .OnDelete(DeleteBehavior.Restrict) // Null out FKs if OperationGroup deleted (all such dependents need to be loaded by EF Core at the time of deletion!)
            .HasConstraintName($"FK_{substateNameSnakeCase}_down_operation_group");
    }
}
