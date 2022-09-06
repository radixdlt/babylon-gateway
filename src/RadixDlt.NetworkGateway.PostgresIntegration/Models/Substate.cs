using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

internal abstract class BaseSubstate
{
    [Key]
    [Column("key")]
    public TmpKey Key { get; set; }

    public Entity Entity { get; set; }
    public long EntityId { get; set; }
}
