using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal static class SchemaDefinitionDataQuery
{
    internal record EventDetailsDataQueryResult(long EntityId, string EntityAddress, string PackageAddress, string BlueprintName);

    internal static async Task<Dictionary<long, EventDetailsDataQueryResult>> Execute(
        IDapperWrapper dapperWrapper,
        CommonDbContext dbContext,
        List<long> schemaDefiningEntityIds,
        CancellationToken token)
    {
        if (!schemaDefiningEntityIds.Any())
        {
            return new Dictionary<long, EventDetailsDataQueryResult>();
        }

        var parameters = new
        {
            schemaDefiningEntityIds = schemaDefiningEntityIds,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
            @"
WITH vars AS (
    SELECT
        unnest(@schemaDefiningEntityIds) AS schema_defining_entity_id
)
SELECT
    e.id AS EntityId,
    e.address AS EntityAddress,
    pe.address as PackageAddress,
    e.blueprint_name AS BlueprintName
FROM vars
INNER JOIN entities e on e.id = vars.schema_defining_entity_id
LEFT JOIN entities pe ON pe.id = e.correlated_entity_ids[array_position(pe.correlated_entity_relationships, 'component_to_instantiating_package')]
;",
            parameters,
            cancellationToken: token);

        var result = (await dapperWrapper.QueryAsync<EventDetailsDataQueryResult>(dbContext.Database.GetDbConnection(), cd))
            .ToDictionary(x => x.EntityId, x => x);

        return result;
    }
}
