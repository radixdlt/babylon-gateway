using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

[Table("entities")]
internal class Entity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("type")]
    public TmpEntityType Type { get; set; }

    [Column("address")]
    public TmpAddress Address { get; set; }

    [Column("global_address")]
    public TmpAddress? GlobalAddress { get; set; }

    [Column("owner_id")]
    public long? OwnerId { get; set; }

    [Column("ancestor_id")]
    public long? AncestorId { get; set; }

    [Column("global_ancestor_id")]
    public long? GlobalAncestorId { get; set; }

    [Column("from_state_version")]
    public long FromStateVersion { get; set; }
}
