/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class EntitiesByRoleRequirementQuerier : IEntitiesByRoleRequirementQuerier
{
    private readonly ReadOnlyDbContext _dbContext;
    private readonly IDapperWrapper _dapperWrapper;
    private readonly IEntityQuerier _entityQuerier;

    private record EntitiesByRoleRequirementPageResultRow(
        long Id,
        EntityAddress EntityAddress,
        long FirstSeenStateVersion,
        long TotalCount);

    private record EntitiesByRoleRequirementLookupResultRow(
        long Id,
        long ResourceEntityId,
        EntityAddress EntityAddress,
        long FirstSeenStateVersion,
        long TotalCount);

    public EntitiesByRoleRequirementQuerier(ReadOnlyDbContext dbContext, IDapperWrapper dapperWrapper, IEntityQuerier entityQuerier)
    {
        _dbContext = dbContext;
        _dapperWrapper = dapperWrapper;
        _entityQuerier = entityQuerier;
    }

    public async Task<GatewayModel.EntitiesByRoleRequirementPageResponse> EntitiesByRoleRequirementPage(
        EntityAddress resourceAddress,
        string nonFungibleLocalId,
        int limit,
        GatewayModel.IdBoundaryCoursor? cursor,
        CancellationToken token = default)
    {
        var resourceEntity = await _dbContext
            .Entities
            .AnnotateMetricName()
            .FirstOrDefaultAsync(e => e.Address == resourceAddress, token);

        if (resourceEntity == null)
        {
            throw new EntityNotFoundException(resourceAddress.ToString());
        }

        if (!resourceEntity.Address.IsResource)
        {
            throw new InvalidEntityException(resourceEntity.Address.ToString());
        }

        var parameters = new
        {
            resourceEntityId = resourceEntity.Id,
            nonFungibleLocalId = nonFungibleLocalId,
            stateVersionBoundary = cursor?.StateVersionBoundary ?? 0,
            idBoundary = cursor?.IdBoundary ?? 0,
            limit = limit + 1,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
            @"
WITH entries_with_count AS (
    SELECT
         ebrr.id
        ,ebrr.entity_id
        ,ebrr.first_seen_state_version
        ,COUNT(*) OVER () AS TotalCount
    FROM entities_by_role_requirement_entry_definition ebrr
    WHERE
    (
        (@nonFungibleLocalId IS NULL AND ebrr.discriminator = 'resource' AND ebrr.resource_entity_id = @resourceEntityId)
            OR
        (@nonFungibleLocalId IS NOT NULL AND ebrr.discriminator = 'non_fungible' AND ebrr.resource_entity_id = @resourceEntityId AND ebrr.non_fungible_local_id = @nonFungibleLocalId)
    )
)
SELECT
    ewc.id                         AS Id
    ,e.address                     AS EntityAddress
    ,ewc.first_seen_state_version  AS FirstSeenStateVersion
    ,ewc.TotalCount
FROM entries_with_count ewc
INNER JOIN entities e ON ewc.entity_id = e.id
WHERE (ewc.first_seen_state_version, ewc.id) >= (@stateVersionBoundary, @idBoundary)
ORDER BY ewc.first_seen_state_version ASC, ewc.id ASC
limit @limit;",
            parameters,
            cancellationToken: token
        );

        var entriesAndOneMore = await _dapperWrapper.ToListAsync<EntitiesByRoleRequirementPageResultRow>(_dbContext.Database.GetDbConnection(), cd);
        var lastElement = entriesAndOneMore.LastOrDefault();
        var nextPageExists = entriesAndOneMore.Count == limit + 1 && lastElement != null;

        var totalCount = entriesAndOneMore.FirstOrDefault()?.TotalCount ?? 0;

        var nextCursor = nextPageExists
            ? new GatewayModel.IdBoundaryCoursor(lastElement!.FirstSeenStateVersion, lastElement.Id).ToCursorString()
            : null;

        var castedResult = entriesAndOneMore
            .Take(limit)
            .Select(x => new GatewayModel.EntitiesByRoleRequirementItem(x.EntityAddress, x.FirstSeenStateVersion))
            .ToList();

        return new GatewayModel.EntitiesByRoleRequirementPageResponse(totalCount, nextCursor, castedResult);
    }

    public async Task<GatewayModel.EntitiesByRoleRequirementLookupResponse> EntitiesByRoleRequirementLookup(
        List<GatewayModel.EntitiesByRoleRequirementRequestRequirement> requestRequirements,
        int limit,
        CancellationToken token = default)
    {
        if (requestRequirements.Count == 0)
        {
            return new GatewayModel.EntitiesByRoleRequirementLookupResponse(new List<GatewayModel.EntitiesByRoleRequirementLookupCollection>());
        }

        var resourceEntityIds = await _entityQuerier
            .ResolveEntityIds(requestRequirements.Select(x => (EntityAddress)x.ResourceAddress).ToList(), token);

        var resourceEntityIdsParameter = new List<long>();
        var nonFungibleIdsParameter = new List<string?>();

        foreach (var item in requestRequirements)
        {
            var resourceExists = resourceEntityIds.TryGetValue((EntityAddress)item.ResourceAddress, out var resourceEntityId);
            if (resourceExists)
            {
                resourceEntityIdsParameter.Add(resourceEntityId);
                nonFungibleIdsParameter.Add(item.NonFungibleId);
            }
        }

        var parameters = new
        {
            resourceEntityIds = resourceEntityIdsParameter,
            nonFungibleLocalIds = nonFungibleIdsParameter,
            limit = limit + 1,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
            @"
WITH vars AS (
    SELECT
        UNNEST(@resourceEntityIds) AS resource_entity_id,
        UNNEST(@nonFungibleLocalIds) AS non_fungible_local_id
)
SELECT
    ebrr.Id
   ,vars.resource_entity_id          AS ResourceEntityId
   ,e.address                        AS EntityAddress
   ,ebrr.FirstSeenStateVersion
   ,ebrr.TotalCount
FROM vars
INNER JOIN LATERAL (
    SELECT
         ebrr.id                          AS Id
        ,ebrr.entity_id
        ,ebrr.first_seen_state_version    AS FirstSeenStateVersion
        ,COUNT(*) OVER ()                 AS TotalCount
    FROM entities_by_role_requirement_entry_definition ebrr
    WHERE
        (vars.non_fungible_local_id IS NULL AND ebrr.discriminator = 'resource' AND ebrr.resource_entity_id = vars.resource_entity_id)
        OR
        (vars.non_fungible_local_id IS NOT NULL AND ebrr.discriminator = 'non_fungible' AND ebrr.resource_entity_id = vars.resource_entity_id AND ebrr.non_fungible_local_id = vars.non_fungible_local_id)
    ORDER BY ebrr.first_seen_state_version ASC, ebrr.id ASC
    LIMIT @limit
) ebrr on TRUE
INNER JOIN entities e on ebrr.entity_id = e.id
ORDER BY resource_entity_id
",
            parameters,
            cancellationToken: token
        );

        var queryResult = await _dapperWrapper.ToListAsync<EntitiesByRoleRequirementLookupResultRow>(_dbContext.Database.GetDbConnection(), cd);

        var result = queryResult
            .GroupBy(r => r.ResourceEntityId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var resultItems = new List<GatewayModel.EntitiesByRoleRequirementLookupCollection>();

        foreach (var requirement in requestRequirements)
        {
            var resourceExists = resourceEntityIds.TryGetValue((EntityAddress)requirement.ResourceAddress, out var resourceEntityId);
            if (!resourceExists)
            {
                resultItems.Add(new GatewayModel.EntitiesByRoleRequirementLookupCollection(0, null, requirement, new List<GatewayModel.EntitiesByRoleRequirementItem>()));
                continue;
            }

            var entriesExists = result.TryGetValue(resourceEntityId, out var entriesAndOneMore);
            if (!entriesExists || entriesAndOneMore == null)
            {
                resultItems.Add(new GatewayModel.EntitiesByRoleRequirementLookupCollection(0, null, requirement, new List<GatewayModel.EntitiesByRoleRequirementItem>()));
                continue;
            }

            var totalCount = entriesAndOneMore.FirstOrDefault()?.TotalCount ?? 0;
            var lastElement = entriesAndOneMore.LastOrDefault();
            var nextPageExists = entriesAndOneMore.Count == limit + 1 && lastElement != null;

            var nextCursor = nextPageExists
                ? new GatewayModel.IdBoundaryCoursor(lastElement!.FirstSeenStateVersion, lastElement.Id).ToCursorString()
                : null;

            var mappedRows = entriesAndOneMore
                .Select(x => new GatewayModel.EntitiesByRoleRequirementItem(x.EntityAddress, x.FirstSeenStateVersion))
                .ToList();

            resultItems.Add(new GatewayModel.EntitiesByRoleRequirementLookupCollection(totalCount, nextCursor, requirement, mappedRows));
        }

        return new GatewayModel.EntitiesByRoleRequirementLookupResponse(resultItems);
    }
}
