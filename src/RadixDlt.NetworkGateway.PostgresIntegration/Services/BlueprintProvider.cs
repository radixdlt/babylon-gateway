using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal interface IBlueprintProvider
{
    Task<Dictionary<BlueprintDefinitionIdentifier, PackageBlueprintHistory>> GetBlueprints(
        IReadOnlyCollection<BlueprintDefinitionIdentifier> blueprintDefinitions,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default
    );
}

internal class BlueprintProvider : IBlueprintProvider
{
    private readonly ReadOnlyDbContext _dbContext;

    public BlueprintProvider(ReadOnlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dictionary<BlueprintDefinitionIdentifier, PackageBlueprintHistory>> GetBlueprints(
        IReadOnlyCollection<BlueprintDefinitionIdentifier> blueprintDefinitions,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default
    )
    {
        var blueprintNames = blueprintDefinitions.Select(x => x.Name).ToList();
        var blueprintVersions = blueprintDefinitions.Select(x => x.Version).ToList();
        var packageEntityIds = blueprintDefinitions.Select(x => x.PackageEntityId).ToList();

        var result = await _dbContext
            .PackageBlueprintHistory
            .FromSqlInterpolated($@"
WITH blueprints (blueprint_name, blueprint_version, package_entity_id) AS (SELECT UNNEST({blueprintNames}), UNNEST({blueprintVersions}), UNNEST({packageEntityIds}))
SELECT pbh.*
FROM blueprints b
INNER JOIN package_blueprint_history pbh
ON pbh.name = b.blueprint_name AND pbh.version = b.blueprint_version and pbh.package_entity_id = b.package_entity_id
WHERE from_state_version <= {ledgerState.StateVersion}
")
            .ToDictionaryAsync(
                x => new BlueprintDefinitionIdentifier(x.Name, x.Version, x.PackageEntityId),
                x => x,
                token);

        return result;
    }
}
