using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Models;

[Table("component_resource_vault_definition")]
internal class ComponentResourceVaultDefinition
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("from_state_version")]
    public long FromStateVersion { get; set; }

    [Column("component_entity_id")]
    public long ComponentEntityId { get; set; }

    [Column("resource_entity_id")]
    public long ResourceEntityId { get; set; }

    [Column("vault_entity_id")]
    public long VaultEntityId { get; set; }
}

[Table("component_resource_vault_totals_history")]
internal class ComponentResourceVaultTotalsHistory
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("from_state_version")]
    public long FromStateVersion { get; set; }

    [Column("component_entity_id")]
    public long ComponentEntityId { get; set; }

    [Column("resource_entity_id")]
    public long ResourceEntityId { get; set; }

    [Column("total_vault_count")]
    public long TotalVaultCount { get; set; }

    [Column("total_vault_balance")]
    public long TotalVaultBalance { get; set; }
}
