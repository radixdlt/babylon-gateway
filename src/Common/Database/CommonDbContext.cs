using Common.Database.Models;
using Common.Database.Models.Ledger;
using Common.Database.Models.Ledger.History;
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

    public DbSet<AccountResourceBalanceHistory> AccountResourceBalanceHistoryEntries => Set<AccountResourceBalanceHistory>();

#pragma warning restore CS1591

    public CommonDbContext(DbContextOptions<CommonDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerTransaction>()
            .HasCheckConstraint(
                "CK_CompleteHistory",
                "state_version = 1 OR state_version = parent_state_version + 1"
            );

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

        HookUpSubstate<AccountResourceBalanceSubstate>(modelBuilder);
        modelBuilder.Entity<AccountResourceBalanceSubstate>()
            .HasIndex(s => new { s.AccountAddress, s.ResourceIdentifier, s.Amount })
            .IncludeProperties(s => new { s.SubstateIdentifier })
            .HasFilter("down_state_version is null")
            .HasDatabaseName($"IX_{nameof(AccountResourceBalanceSubstate)}_CurrentUnspentUTXOs");

        HookUpSubstate<AccountXrdStakeBalanceSubstate>(modelBuilder);
        HookUpSubstate<AccountStakeOwnershipBalanceSubstate>(modelBuilder);
        HookUpSubstate<ValidatorStakeBalanceSubstate>(modelBuilder);

        HookUpAccountResourceBalanceHistory(modelBuilder);
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
    }

    private static void HookUpAccountResourceBalanceHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasKey(h => new { h.AccountAddress, h.ResourceIdentifier, h.FromStateVersion });

        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasIndex(h => new { h.AccountAddress, h.ResourceIdentifier })
            .HasFilter("to_state_version is null")
            .IsUnique()
            .HasDatabaseName($"ix_{nameof(AccountResourceBalanceHistory).ToSnakeCase()}_current_balance");

        // All four of these indices (these three plus the implicit index from the PK) could be useful for different queries
        // TODO:NG-39 Remove any which aren't important for the APIs we're exposing
        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasIndex(h => new { h.AccountAddress, h.FromStateVersion });
        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasIndex(h => new { h.ResourceIdentifier, h.AccountAddress, h.FromStateVersion });
        modelBuilder.Entity<AccountResourceBalanceHistory>()
            .HasIndex(h => new { h.ResourceIdentifier, h.FromStateVersion });
    }

    private static void HookUpSubstate<TSubstate>(ModelBuilder modelBuilder)
        where TSubstate : SubstateBase
    {
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
            .HasConstraintName($"fk_{nameof(TSubstate).ToSnakeCase()}_up_operation_group");

        modelBuilder.Entity<TSubstate>()
            .HasOne(s => s.DownOperationGroup)
            .WithMany()
            .HasForeignKey(s => new
            {
                ResultantStateVersion = s.DownStateVersion,
                OperationGroupIndex = s.DownOperationGroupIndex,
            })
            .OnDelete(DeleteBehavior.Restrict) // Null out FKs if OperationGroup deleted (all such dependents need to be loaded by EF Core at the time of deletion!)
            .HasConstraintName($"fk_{nameof(TSubstate).ToSnakeCase()}_down_operation_group");
    }
}
