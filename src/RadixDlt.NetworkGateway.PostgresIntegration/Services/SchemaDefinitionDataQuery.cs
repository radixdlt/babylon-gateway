using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal static class SchemaDefinitionDataQuery
{
    internal record EventDetailsDataQueryResult(long EntityId, string EntityAddress, string PackageAddress, string BlueprintName, string? GlobalEmitterAddress, string? OuterObjectAddress);

    internal static async Task<Dictionary<long, EventDetailsDataQueryResult>> Execute(
        IDapperWrapper dapperWrapper,
        CommonDbContext dbContext,
        List<long> entityIds,
        CancellationToken token)
    {
        if (!entityIds.Any())
        {
            return new Dictionary<long, EventDetailsDataQueryResult>();
        }

        var parameters = new
        {
            entityIds = entityIds,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
            @"
WITH vars AS (
    SELECT
        unnest(@entityIds) AS entity_id
)
SELECT
    e.id AS EntityId,
    e.address AS EntityAddress,
    pe.address as PackageAddress,
    e.blueprint_name AS BlueprintName,
    ge.address AS GlobalEmitterAddress,
    ooe.address AS OuterObjectAddress
FROM vars
INNER JOIN entities e on e.id = vars.entity_id
LEFT JOIN entities ge on e.global_ancestor_id = ge.id
LEFT JOIN entities ooe on e.outer_object_entity_id = ooe.id
LEFT JOIN entities pe ON pe.id = e.correlated_entity_ids[array_position(e.correlated_entity_relationships, 'component_to_instantiating_package')]
;",
            parameters,
            cancellationToken: token);

        var result = (await dapperWrapper.QueryAsync<EventDetailsDataQueryResult>(dbContext.Database.GetDbConnection(), cd))
            .ToDictionary(x => x.EntityId, x => x);

        return result;
    }
}
