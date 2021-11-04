using Microsoft.EntityFrameworkCore;
using Shared.Database.Models;

namespace Shared.Database;

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
}
