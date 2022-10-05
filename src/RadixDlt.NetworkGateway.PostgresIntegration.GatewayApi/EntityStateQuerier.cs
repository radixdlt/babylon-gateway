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
using RadixDlt.NetworkGateway.Commons;
using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal class EntityStateQuerier : IEntityStateQuerier
{
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly ReadOnlyDbContext _dbContext;

    public EntityStateQuerier(INetworkConfigurationProvider networkConfigurationProvider, ReadOnlyDbContext dbContext)
    {
        _networkConfigurationProvider = networkConfigurationProvider;
        _dbContext = dbContext;
    }

    public async Task<EntityResourcesResponse?> EntityResourcesSnapshot(RadixAddress address, LedgerState ledgerState, CancellationToken token = default)
    {
        // const int resourcesPerType = 5; // TODO add proper pagination support
        // const string referenceColumn = "global_entity_id"; // TODO or "owner_entity_id"

        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState._Version)
            .FirstOrDefaultAsync(e => e.GlobalAddress == address, token);

        if (entity is not ComponentEntity ce)
        {
            return null;
        }

        var hrp = GetHrpByEntity(ce);

        if (hrp == null)
        {
            return null;
        }

        // TODO this has been recently replaced with EF-based inheritance, but we might want to get back to two separate tables instead of discriminator column
        // TODO this one might need index, think: (owner_entity_id, from_state_version, fungible_resource_entity_id) include (balance)
        // TODO this one might benefit form "*" => "fungible_resource_entity_id, balance"

        var dbResources = await _dbContext.EntityResourceHistory
            .FromSqlInterpolated($@"
WITH aggregate_history AS (
    SELECT fungible_resource_ids, non_fungible_resource_ids
    FROM entity_resource_aggregate_history
    WHERE from_state_version <= {ledgerState._Version} AND entity_id = {ce.Id}
    ORDER BY from_state_version DESC
    LIMIT 1
),
unnested_aggregate_history AS (
    SELECT UNNEST(fungible_resource_ids || non_fungible_resource_ids) AS resource_id
    FROM aggregate_history
)
SELECT erh.*
FROM unnested_aggregate_history uah
INNER JOIN LATERAL (
    SELECT *
    FROM entity_resource_history
    WHERE from_state_version <= {ledgerState._Version} AND global_entity_id = {ce.Id} AND resource_entity_id = uah.resource_id
    ORDER BY from_state_version DESC
    LIMIT 1
) erh ON true;
")
            .ToListAsync(token);

        var referencedEntityIds = dbResources.Select(h => h.ResourceEntityId).ToList();

        var resources = await _dbContext.Entities
            .Where(e => referencedEntityIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, token);

        var fungibles = new List<EntityStateResponseFungibleResource>();
        var nonFungibles = new List<EntityStateResponseNonFungibleResource>();

        foreach (var dbResource in dbResources)
        {
            var rga = resources[dbResource.ResourceEntityId].GlobalAddress ?? throw new Exception("xxx"); // TODO fix me
            var ra = RadixBech32.EncodeRadixEngineAddress(RadixEngineAddressType.HASHED_KEY, _networkConfigurationProvider.GetAddressHrps().ResourceHrpSuffix, rga);

            if (dbResource is EntityFungibleResourceHistory efrh)
            {
                fungibles.Add(new EntityStateResponseFungibleResource(ra, efrh.Balance.ToSubUnitString()));
            }
            else if (dbResource is EntityNonFungibleResourceHistory enfrh)
            {
                nonFungibles.Add(new EntityStateResponseNonFungibleResource(ra, enfrh.IdsCount));
            }
            else
            {
                throw new Exception("bla bla bla"); // TODO fix me
            }
        }

        var adr = RadixBech32.EncodeRadixEngineAddress(RadixEngineAddressType.HASHED_KEY, hrp, address);
        var fungiblesPagination = new EntityResourcesResponseFungibleResources(fungibles.Count, null, "TBD (currently everything is returned)", fungibles);
        var nonFungiblesPagination = new EntityResourcesResponseNonFungibleResources(nonFungibles.Count, null, "TBD (currently everything is returned)", nonFungibles);

        return new EntityResourcesResponse(adr, fungiblesPagination, nonFungiblesPagination);
    }

    public async Task<EntityDetailsResponse?> EntityDetailsSnapshot(RadixAddress address, LedgerState ledgerState, CancellationToken token = default)
    {
        // TODO just some quick and naive implementation

        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState._Version)
            .FirstOrDefaultAsync(e => e.GlobalAddress == address, token);

        if (entity == null)
        {
            return null;
        }

        var hrp = GetHrpByEntity(entity);

        if (hrp == null)
        {
            return null;
        }

        var metadata = new Dictionary<string, string>();
        var metadataHistory = await _dbContext.EntityMetadataHistory
            .Where(e => e.EntityId == entity.Id && e.FromStateVersion <= ledgerState._Version)
            .OrderByDescending(e => e.FromStateVersion)
            .FirstOrDefaultAsync(token);

        if (metadataHistory != null)
        {
            metadata = metadataHistory.Keys.Zip(metadataHistory.Values).ToDictionary(z => z.First, z => z.Second);
        }

        var adr = RadixBech32.EncodeRadixEngineAddress(RadixEngineAddressType.HASHED_KEY, hrp, address);

        return new EntityDetailsResponse(adr, metadata);
    }

    private string? GetHrpByEntity(Entity entity)
    {
        if (entity is ResourceManagerEntity)
        {
            return _networkConfigurationProvider.GetAddressHrps().ResourceHrpSuffix;
        }

        if (entity is ComponentEntity ce)
        {
            // TODO use enum or something
            switch (ce.Kind)
            {
                case "account":
                    return _networkConfigurationProvider.GetAddressHrps().AccountHrp;
                case "validator":
                    return _networkConfigurationProvider.GetAddressHrps().ValidatorHrp;
            }
        }

        return null;
    }
}
