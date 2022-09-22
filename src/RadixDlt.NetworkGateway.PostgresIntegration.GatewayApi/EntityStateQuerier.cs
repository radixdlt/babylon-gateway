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
using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.Commons.Extensions;
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

    public async Task<EntityStateResponse> TmpAccountResourcesSnapshot(byte[] address, LedgerState ledgerState, CancellationToken token = default)
    {
        // TODO just some quick and naive implementation
        // TODO we will denormalize a lot to improve performance and reduce complexity
        // TODO add proper pagination support

        var entity = await _dbContext.TmpEntities
            .Where(e => e.FromStateVersion <= ledgerState._Version)
            .FirstOrDefaultAsync(e => e.GlobalAddress == address, token);

        if (entity == null)
        {
            // TODO just not found
            throw new Exception("xxxx");
        }

        string hrp;

        // TODO we need to keep track of "what subtype" (component => account, system, validator; resource => fungible, non-fungible) we're dealing with
        if (entity is TmpResourceManagerEntity)
        {
            hrp = _networkConfigurationProvider.GetAddressHrps().ResourceHrpSuffix;
        }
        else if (entity is TmpComponentEntity component)
        {
            hrp = component.Kind switch
            {
                "account" => _networkConfigurationProvider.GetAddressHrps().AccountHrp,
                "validator" => _networkConfigurationProvider.GetAddressHrps().ValidatorHrp,
                _ => throw new Exception("fix me"), // TODO fix me
            };
        }
        else
        {
            throw new Exception("unsupported entity type"); // TODO fix me
        }

        // TODO those DISTINCT ON queries are going to be slow because we have to fetch everything first, ideally we should change it to somehow just fetch first element form index and stop immediately
        // TODO as we support "lookup by global" and "lookup by owner" we must duplicate some keys

        // TODO this one might need index, think: (owner_entity_id, from_state_version, fungible_resource_entity_id) include (balance)
        // TODO this one might benefit form "*" => "fungible_resource_entity_id, balance"
        var fungibleBalanceHistory = await _dbContext.TmpOwnerEntityFungibleResourceBalanceHistory
            .FromSqlInterpolated($@"
SELECT DISTINCT ON (owner_entity_id, fungible_resource_entity_id) *
FROM tmp_entity_fungible_resource_balance_history
WHERE owner_entity_id = {entity.Id} AND from_state_version <= {ledgerState._Version}
ORDER BY owner_entity_id, fungible_resource_entity_id, from_state_version DESC")
            .ToListAsync(token);

        // TODO this one might need index, think: (owner_entity_id, from_state_version, fungible_resource_entity_id) include(ids.length) - but no INCLUDE(ids) as its just too big
        // TODO this one might benefit form "*" => "fungible_resource_entity_id, ids" (first x elements of ids actually)
        // TODO or maybe we don't even want to return actual NF ids here?
        var nonFungibleIdsHistory = await _dbContext.TmpOwnerEntityNonFungibleResourceIdsHistory
            .FromSqlInterpolated($@"
SELECT DISTINCT ON (owner_entity_id, non_fungible_resource_entity_id) *
FROM tmp_entity_non_fungible_resource_ids_history
WHERE owner_entity_id = {entity.Id} AND from_state_version <= {ledgerState._Version}
ORDER BY owner_entity_id, non_fungible_resource_entity_id, from_state_version DESC")
            .ToListAsync(token);

        var referencedEntityIds = fungibleBalanceHistory
            .Select(h => h.FungibleResourceEntityId)
            .Concat(nonFungibleIdsHistory.Select(h => h.NonFungibleResourceEntityId))
            .Distinct()
            .ToList();

        var resources = await _dbContext.TmpEntities
            .Where(e => referencedEntityIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, token);

        var fungibles = new List<EntityStateResponseFungibleResource>();

        foreach (var fbh in fungibleBalanceHistory)
        {
            var rga = resources[fbh.FungibleResourceEntityId].GlobalAddress ?? throw new Exception("xxx"); // TODO fix me
            var ra = RadixBech32.EncodeRadixEngineAddress(RadixEngineAddressType.HASHED_KEY, _networkConfigurationProvider.GetAddressHrps().ResourceHrpSuffix, rga);

            fungibles.Add(new EntityStateResponseFungibleResource(ra, fbh.Balance.ToSubUnitString()));
        }

        var nonFungibles = new List<EntityStateResponseNonFungibleResource>();

        foreach (var nfih in nonFungibleIdsHistory)
        {
            var rga = resources[nfih.NonFungibleResourceEntityId].GlobalAddress ?? throw new Exception("xxx"); // TODO fix me
            var ra = RadixBech32.EncodeRadixEngineAddress(RadixEngineAddressType.HASHED_KEY, _networkConfigurationProvider.GetAddressHrps().ResourceHrpSuffix, rga);

            nonFungibles.Add(new EntityStateResponseNonFungibleResource(ra, nfih.IdsCount));
        }

        var adr = RadixBech32.EncodeRadixEngineAddress(RadixEngineAddressType.HASHED_KEY, hrp, address);
        var fungiblesPagination = new EntityStateResponseFungibleResources(fungibleBalanceHistory.Count, null, "TBD (currently everything is returned)", fungibles);
        var nonFungiblesPagination = new EntityStateResponseNonFungibleResources(nonFungibleIdsHistory.Count, null, "TBD (currently everything is returned)", nonFungibles);

        return new EntityStateResponse(adr, fungiblesPagination, nonFungiblesPagination);
    }
}
