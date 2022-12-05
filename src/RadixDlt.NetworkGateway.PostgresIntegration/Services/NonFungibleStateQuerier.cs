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

using Dapper;
using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions.Addressing;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class NonFungibleStateQuerier : INonFungibleStateQuerier
{
    private record NonFungibleIdViewModel(string NonFungibleId, int TotalCount);

    private record NonFungibleDataViewModel(string NonFungibleId, bool IsDeleted, byte[] ImmutableData, byte[] MutableData);

    private readonly ReadOnlyDbContext _dbContext;

    public NonFungibleStateQuerier(ReadOnlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GatewayModel.NonFungibleIdsResponse> NonFungibleIds(INonFungibleStateQuerier.PageRequest request, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<NonFungibleResourceManagerEntity>(request.Address, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
WITH store_history (nfids, total_count) AS (
    SELECT non_fungible_id_data_ids[@offset:@limit], array_length(non_fungible_id_data_ids, 1)
    FROM non_fungible_id_store_history
    WHERE non_fungible_resource_manager_entity_id = @entityId AND from_state_version < @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
),
non_fungible_data_ids (id) AS (
    SELECT UNNEST(nfids)
    FROM store_history
)
SELECT nfd.non_fungible_id AS NonFungibleId, store_history.total_count AS TotalCount
FROM non_fungible_id_data nfd, store_history
WHERE nfd.id IN(
    SELECT id FROM non_fungible_data_ids
)",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entity.Id,
                offset = request.Offset + 1,
                limit = request.Offset + request.Limit + 1,
            },
            cancellationToken: token);

        long? totalCount = null;

        var items = (await _dbContext.Database.GetDbConnection().QueryAsync<NonFungibleIdViewModel>(cd)).ToList()
            .Select(vm =>
            {
                totalCount = vm.TotalCount;

                return new GatewayModel.NonFungibleIdsCollectionItem(vm.NonFungibleId);
            })
            .ToList();

        var previousCursor = request.Offset > 0
            ? new GatewayModel.EntityFungiblesCursor(Math.Max(request.Offset - request.Limit, 0)).ToCursorString()
            : null;

        var nextCursor = items.Count > request.Limit
            ? new GatewayModel.EntityFungiblesCursor(request.Offset + request.Limit).ToCursorString()
            : null;

        return new GatewayModel.NonFungibleIdsResponse(
            ledgerState: ledgerState,
            address: request.Address.ToString(),
            nonFungibleIds: new GatewayModel.NonFungibleIdsCollection(
                totalCount: totalCount,
                previousCursor: previousCursor,
                nextCursor: nextCursor,
                items: items.Take(request.Limit).ToList()));
    }

    public async Task<GatewayModel.NonFungibleDataResponse> NonFungibleIdData(DecodedRadixAddress address, string nonFungibleId, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entity = await GetEntity<NonFungibleResourceManagerEntity>(address, ledgerState, token);

        var cd = new CommandDefinition(
            commandText: @"
SELECT nfid.non_fungible_id AS NonFungibleId, md.is_deleted AS IsDeleted, nfid.immutable_data AS ImmutableData, md.mutable_data AS MutableData
FROM non_fungible_id_data nfid
LEFT JOIN LATERAL (
    SELECT *
    FROM non_fungible_id_mutable_data_history nfidmdh
    WHERE nfidmdh.non_fungible_id_data_id = nfid.id AND nfidmdh.from_state_version <= @stateVersion
    ORDER BY nfidmdh.non_fungible_id_data_id DESC
    LIMIT 1
) md ON TRUE
WHERE nfid.non_fungible_resource_manager_entity_id = @entityId AND nfid.non_fungible_id = @nonFungibleId
ORDER BY nfid.from_state_version DESC
LIMIT 1
",
            parameters: new
            {
                stateVersion = ledgerState.StateVersion,
                entityId = entity.Id,
                nonFungibleId = nonFungibleId,
            },
            cancellationToken: token);

        var data = await _dbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync<NonFungibleDataViewModel>(cd);

        if (data == null || data.IsDeleted)
        {
            throw new EntityNotFoundException(address.ToString()); // TODO change it to some "resource not found"?
        }

        return new GatewayModel.NonFungibleDataResponse(
            ledgerState: ledgerState,
            address: address.ToString(),
            nonFungibleIdType: entity.NonFungibleIdType.ToGatewayModel(),
            nonFungibleId: data.NonFungibleId,
            mutableDataHex: data.MutableData.ToHex(),
            immutableDataHex: data.ImmutableData.ToHex());
    }

    private async Task<Entity> GetEntity(DecodedRadixAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var entity = await _dbContext.Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.GlobalAddress == address.Data, token);

        if (entity == null)
        {
            throw new EntityNotFoundException(address.ToString());
        }

        return entity;
    }

    private async Task<TEntity> GetEntity<TEntity>(DecodedRadixAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity
    {
        var entity = await GetEntity(address, ledgerState, token);

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidEntityException(address.ToString());
        }

        return typedEntity;
    }
}
