using Microsoft.EntityFrameworkCore;
using Shared.Database.Models;

namespace Shared.Database;

/// <summary>
/// Shared DB Context for the radixdlt-network-gateway database
/// </summary>
public class SharedDbContext : DbContext
{
#pragma warning disable CS1591 // Remove need for public docs - instead refer to the Model docs

    public DbSet<Node> Nodes { get; set; }

#pragma warning restore CS1591

    public SharedDbContext(DbContextOptions<SharedDbContext> options)
        : base(options)
    {
    }
}
