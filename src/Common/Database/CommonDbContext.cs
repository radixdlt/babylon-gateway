using Common.Database.Models;
using Common.Database.Models.Ledger;
using Common.Database.Models.Ledger.Operations;
using Common.Database.ValueConverters;
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

        modelBuilder.Entity<LedgerTransaction>()
            .HasIndex(lt => new { lt.Epoch, lt.EndOfEpochRound })
            .IsUnique()
            .HasFilter("end_of_round IS NOT NULL");

        modelBuilder.Entity<OperationGroup>()
            .HasKey(og => new { og.ResultantStateVersion, og.OperationGroupIndex });

        modelBuilder.Entity<OperationGroup>()
            .OwnsOne(og => og.InferredAction);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TokenAmount>()
            .HaveConversion<TokenAmountToBigIntegerConverter>()
            .HaveColumnType("numeric")
            .HavePrecision(1000);

        configurationBuilder.Properties<SubstateOperationType>()
            .HaveConversion<SubstateOperationTypeValueConverter>();
    }
}
