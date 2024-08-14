using RadixDlt.NetworkGateway.Abstractions.Numerics;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

[Table("vault_balance_history")]
internal class VaultBalanceHistory
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("from_state_version")]
    public long FromStateVersion { get; set; }

    [Column("vault_entity_id")]
    public long VaultEntityId { get; set; }

    [Column("balance")]
    public TokenAmount Balance { get; set; }
}
