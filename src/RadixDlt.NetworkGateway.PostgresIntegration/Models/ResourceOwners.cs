using RadixDlt.NetworkGateway.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

[Table("resource_owners")]
internal class ResourceOwners
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("from_state_version")]
    public long FromStateVersion { get; set; }

    [Column("resource_address")]
    public EntityAddress ResourceAddress { get; set; }
}
