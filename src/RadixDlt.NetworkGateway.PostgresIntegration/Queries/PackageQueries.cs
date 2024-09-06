using Dapper;
using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class PackageQueries
{
    internal class PackageBlueprintResultRow : PackageBlueprintHistory
    {
        public int TotalCount { get; set; }
    }

    internal class PackageCodeResultRow : PackageCodeHistory
    {
        public int TotalCount { get; set; }
    }

    public static async Task<IDictionary<long, PackageBlueprintResultRow[]>> GetPackageBlueprintHistory(
        ReadOnlyDbContext dbContext,
        long[] packageEntityIds,
        int offset,
        int limit,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token)
    {
        if (!packageEntityIds.Any())
        {
            return ImmutableDictionary<long, PackageBlueprintResultRow[]>.Empty;
        }

        var cd = new CommandDefinition(
            commandText: @"
WITH variables (package_entity_id) AS (SELECT UNNEST(@packageEntityIds)),
blueprint_slices AS
(
    SELECT *
    FROM variables var
    INNER JOIN LATERAL (
        SELECT package_entity_id, package_blueprint_ids[@startIndex:@endIndex] AS blueprint_slice, cardinality(package_blueprint_ids) AS total_count
        FROM package_blueprint_aggregate_history
        WHERE package_entity_id = var.package_entity_id AND from_state_version <= @stateVersion
        ORDER BY from_state_version DESC
        LIMIT 1
    ) pbah ON TRUE
)
SELECT pbh.*, bs.total_count
FROM blueprint_slices AS bs
INNER JOIN LATERAL UNNEST(blueprint_slice) WITH ORDINALITY AS blueprint_join(id, ordinality) ON TRUE
INNER JOIN package_blueprint_history pbh ON pbh.id = blueprint_join.id
ORDER BY blueprint_join.ordinality ASC;",
            parameters: new
            {
                packageEntityIds = packageEntityIds.ToList(),
                startIndex = offset + 1,
                endIndex = offset + limit,
                stateVersion = ledgerState.StateVersion,
            },
            cancellationToken: token);

        return (await dbContext.Database.GetDbConnection().QueryAsync<PackageBlueprintResultRow>(cd))
            .ToList()
            .GroupBy(b => b.PackageEntityId)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public static async Task<IDictionary<long, PackageCodeResultRow[]>> GetPackageCodeHistory(
        ReadOnlyDbContext dbContext,
        long[] packageEntityIds,
        int offset,
        int limit,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token)
    {
        if (!packageEntityIds.Any())
        {
            return ImmutableDictionary<long, PackageCodeResultRow[]>.Empty;
        }

        var cd = new CommandDefinition(
            commandText: @"
WITH variables (package_entity_id) AS (SELECT UNNEST(@packageEntityIds)),
code_slices AS
(
    SELECT *
    FROM variables var
    INNER JOIN LATERAL (
        SELECT package_entity_id, package_code_ids[@startIndex:@endIndex] AS code_slice, cardinality(package_code_ids) AS total_count
        FROM package_code_aggregate_history
        WHERE package_entity_id = var.package_entity_id AND from_state_version <= @stateVersion
        ORDER BY from_state_version DESC
        LIMIT 1
    ) pbah ON TRUE
)
SELECT pch.*, cs.total_count
FROM code_slices AS cs
INNER JOIN LATERAL UNNEST(code_slice) WITH ORDINALITY AS code_join(id, ordinality) ON TRUE
INNER JOIN package_code_history pch ON pch.id = code_join.id
ORDER BY code_join.ordinality ASC;",
            parameters: new
            {
                packageEntityIds = packageEntityIds.ToList(),
                startIndex = offset + 1,
                endIndex = offset + limit,
                stateVersion = ledgerState.StateVersion,
            },
            cancellationToken: token);

        return (await dbContext.Database.GetDbConnection().QueryAsync<PackageCodeResultRow>(cd))
            .ToList()
            .GroupBy(b => b.PackageEntityId)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public static async Task<Dictionary<long, EntityAddress>> GetCorrelatedEntityAddresses(
        ReadOnlyDbContext dbContext,
        ICollection<Entity> entities,
        IDictionary<long, PackageBlueprintResultRow[]> packageBlueprints,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var lookup = new HashSet<long>();

        foreach (var entity in entities)
        {
            lookup.Add(entity.Id);

            if (entity.HasParent)
            {
                lookup.Add(entity.ParentAncestorId.Value);
                lookup.Add(entity.OwnerAncestorId.Value);
                lookup.Add(entity.GlobalAncestorId.Value);
            }

            if (entity.TryGetCorrelation(EntityRelationship.ComponentToInstantiatingPackage, out var packageCorrelation))
            {
                lookup.Add(packageCorrelation.EntityId);
            }

            if (entity is VaultEntity vaultEntity)
            {
                lookup.Add(vaultEntity.GetResourceEntityId());
            }
        }

        foreach (var dependantEntityId in
                 packageBlueprints.Values.SelectMany(x => x).SelectMany(x => x.DependantEntityIds?.ToArray() ?? System.Array.Empty<long>()))
        {
            lookup.Add(dependantEntityId);
        }

        var ids = lookup.ToList();

        return await dbContext
            .Entities
            .Where(e => ids.Contains(e.Id) && e.FromStateVersion <= ledgerState.StateVersion)
            .Select(e => new { e.Id, e.Address })
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Id, e => e.Address, token);
    }
}
