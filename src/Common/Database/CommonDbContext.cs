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
                "CK_CompleteStateVersionHistory",
                "state_version = 1 OR state_version = parent_state_version + 1"
            )
            .Property(lt => lt.FeePaid).HasConversion(tokenAmountConverter);
    }
}
