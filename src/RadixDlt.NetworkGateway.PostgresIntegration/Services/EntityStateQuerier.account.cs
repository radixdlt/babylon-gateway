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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal partial class EntityStateQuerier
{
    private record AccountLockerVaultViewModel(long Id, long FromStateVersion, string ResourceAddress, string VaultAddress, long LastUpdatedAtStateVersion, string? FungibleBalance, int? NonFungibleCount);

    private record AccountLockerTbdViewModel(long AccountLockerEntityId, long AccountEntityId, long? LastUpdatedAt);

    public async Task<GatewayModel.StateAccountLockerPageAccountResourcesResponse> AccountLockerPage(
        IEntityStateQuerier.AccountLockerPageRequest pageRequest,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var accountLocker = await GetEntity<GlobalAccountLockerEntity>(pageRequest.AccountLockerAddress, ledgerState, token);
        var account = await GetEntity<GlobalAccountEntity>(pageRequest.AccountAddress, ledgerState, token);
        var accountLockerEntryDefinition = await _dbContext
            .AccountLockerDefinition
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .FirstOrDefaultAsync(e => e.AccountLockerEntityId == accountLocker.Id && e.AccountEntityId == account.Id, token);

        if (accountLockerEntryDefinition == null)
        {
            throw new EntityNotFoundException("should not use this excetpion"); // TODO should not use this excetpion
        }

        var cd = new CommandDefinition(
            commandText: @"
SELECT d.id AS Id, d.from_state_version AS FromStateVersion, re.address AS ResourceAddress, ve.address AS VaultAddress, vh.from_state_version AS LastUpdatedAtStateVersion, vh.fungible_balance AS FungibleBalance, vh.non_fungible_count AS NonFungibleCount
FROM account_locker_entry_resource_vault_definition d
INNER JOIN entities re ON re.id = d.resource_entity_id
INNER JOIN entities ve ON ve.id = d.vault_entity_id
INNER JOIN LATERAL (
    SELECT balance::text AS fungible_balance, cardinality(non_fungible_ids) AS non_fungible_count, from_state_version
    FROM entity_vault_history
    WHERE vault_entity_id = ve.id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) vh ON true
WHERE
    d.account_locker_definition_id = @accountLockerDefinitionId
  AND d.from_state_version <= @stateVersion
  AND (d.from_state_version, d.id) <= (@cursorStateVersion, @cursorId)
ORDER BY d.from_state_version DESC, d.id DESC
LIMIT @limit;",
            parameters: new
            {
                accountLockerDefinitionId = accountLockerEntryDefinition.Id,
                stateVersion = ledgerState.StateVersion,
                cursorStateVersion = pageRequest.Cursor?.StateVersionBoundary ?? long.MaxValue,
                cursorId = pageRequest.Cursor?.IdBoundary ?? long.MaxValue,
                limit = pageRequest.Limit + 1,
            },
            cancellationToken: token);

        var keysAndOneMore = (await _dapperWrapper.QueryAsync<AccountLockerVaultViewModel>(_dbContext.Database.GetDbConnection(), cd)).ToList();

        var items = keysAndOneMore
            .Take(pageRequest.Limit)
            .Select(k =>
            {
                // TODO maybe we want to differienciate between fungible and non-fungible from day 1?
                var fungibleAmount = k.FungibleBalance != null ? TokenAmount.FromSubUnitsString(k.FungibleBalance).ToString() : null;
                var nonFungibleAmount = k.NonFungibleCount.HasValue ? k.NonFungibleCount.ToString() : null;
                var amount = fungibleAmount ?? nonFungibleAmount ?? throw new UnreachableException("bleee"); // TODO do something

                return new GatewayModel.AccountLockerResourceVault(
                    resourceAddress: k.ResourceAddress,
                    vaultAddress: k.VaultAddress,
                    amount: amount,
                    lastUpdatedAtStateVersion: k.LastUpdatedAtStateVersion
                );
            })
            .ToList();

        var nextCursor = keysAndOneMore.Count == pageRequest.Limit + 1
            ? new GatewayModel.StateAccountLockerAccountResourcesCursor(keysAndOneMore.Last().FromStateVersion, keysAndOneMore.Last().Id).ToCursorString()
            : null;

        return new GatewayApiSdk.Model.StateAccountLockerPageAccountResourcesResponse(
            ledgerState: ledgerState,
            accountLockerAddress: accountLocker.Address,
            accountAddress: account.Address,
            nextCursor: nextCursor,
            items: items
        );
    }

    public async Task<GatewayModel.StateAccountLockerTbdResponse> AccountLockerTbd(IList<IEntityStateQuerier.AccountLockerLookup> lookup, GatewayModel.LedgerState ledgerState, CancellationToken token = default)
    {
        var entityAddresses = lookup.SelectMany(l => new[] { l.AccountLockerAddress, l.AccountAddress }).ToHashSet().ToList();
        var entities = await GetEntities(entityAddresses, ledgerState, token);
        var entitiesByAddress = entities.ToDictionary(e => e.Address);
        var entitiesById = entities.ToDictionary(e => e.Id);

        long GetAddressOrThrow(EntityAddress input)
        {
            if (!entitiesByAddress.TryGetValue(input, out var e))
            {
                throw new EntityNotFoundException(input);
            }

            return e.Id;
        }

        lookup.Unzip(l => GetAddressOrThrow(l.AccountLockerAddress), l => GetAddressOrThrow(l.AccountAddress), l => l.FromStateVersion ?? 0, out var accountLockerEntityIds, out var accountEntityIds, out var fromStateVersions);

        var cd = new CommandDefinition(
            commandText: @"
WITH variables AS (
    SELECT UNNEST(@accountLockerEntityIds) AS account_locker_entity_id, UNNEST(@accountEntityIds) AS account_entity_id, UNNEST(@fromLedgerState) AS from_state_version
),
lookup AS (
    SELECT d.*, v.from_state_version AS rename_me_from_state_version
    FROM account_locker_entry_definition d, variables v
    WHERE d.account_locker_entity_id = v.account_locker_entity_id AND d.account_entity_id = v.account_entity_id AND d.from_state_version <= @stateVersion
)
SELECT l.account_locker_entity_id AS AccountLockerEntityId, l.account_entity_id AS AccountEntityId, th.from_state_version AS LastUpdatedAt
FROM lookup l
LEFT JOIN LATERAL (
    SELECT from_state_version
    FROM account_locker_entry_touch_history
    WHERE account_locker_definition_id = l.id AND from_state_version BETWEEN l.rename_me_from_state_version AND @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON TRUE;",
            parameters: new
            {
                accountLockerEntityIds = accountLockerEntityIds,
                accountEntityIds = accountEntityIds,
                fromLedgerState = fromStateVersions,
                stateVersion = ledgerState.StateVersion,
            },
            cancellationToken: token);

        var items = new List<GatewayModel.StateAccountLockerTbdResponseItem>();

        foreach (var row in await _dapperWrapper.QueryAsync<AccountLockerTbdViewModel>(_dbContext.Database.GetDbConnection(), cd))
        {
            items.Add(new GatewayModel.StateAccountLockerTbdResponseItem(
                accountLockerAddress: entitiesById[row.AccountLockerEntityId].Address,
                accountAddress: entitiesById[row.AccountEntityId].Address,
                resourceLastStoredAtStateVersion: row.LastUpdatedAt ?? -123));
        }

        return new GatewayModel.StateAccountLockerTbdResponse(ledgerState, items);
    }
}
