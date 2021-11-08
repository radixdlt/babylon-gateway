using Common.Database.Models;
using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
        var tokenAmountConverter = new ValueConverter<TokenAmount, string>(
            v => v.ToPostgresDecimal(),
            v => TokenAmount.FromStringOrNaN(v)
        );

        modelBuilder
            .Entity<LedgerTransaction>()
            .HasCheckConstraint(
                "CK_CompleteHistory",
                "transaction_index = 0 OR transaction_index = parent_transaction_index + 1"
            )
            .Property(lt => lt.FeePaid).HasConversion(tokenAmountConverter);
    }
}
