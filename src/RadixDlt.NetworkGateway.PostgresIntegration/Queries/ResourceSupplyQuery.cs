using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal static class ResourceSupplyQuery
{
    public static async Task<ResourceEntitySupplyHistory> GetResourceSupplyData(ReadOnlyDbContext dbContext, long entityId, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var response = await GetResourcesSupplyData(dbContext, new[] { entityId }, ledgerState, token);
        return response[entityId];
    }

    public static async Task<IDictionary<long, ResourceEntitySupplyHistory>> GetResourcesSupplyData(ReadOnlyDbContext dbContext, long[] entityIds, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        if (entityIds.Length == 0)
        {
            return ImmutableDictionary<long, ResourceEntitySupplyHistory>.Empty;
        }

        var result = await dbContext
            .ResourceEntitySupplyHistory
            .FromSqlInterpolated($@"
WITH variables (entity_id) AS (SELECT UNNEST({entityIds}))
SELECT resh.*
FROM variables
INNER JOIN LATERAL(
    SELECT *
    FROM resource_entity_supply_history
    WHERE from_state_version <= {ledgerState.StateVersion} AND resource_entity_id = variables.entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) resh ON true")
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.ResourceEntityId, token);

        foreach (var missing in entityIds.Except(result.Keys))
        {
            result[missing] = ResourceEntitySupplyHistory.Empty;
        }

        return result;
    }
}
