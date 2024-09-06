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
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class AccountLockerQuerier : IAccountLockerQuerier
{
    private readonly IEntityQuerier _entityQuerier;
    private readonly ReadOnlyDbContext _dbContext;
    private readonly IDapperWrapper _dapperWrapper;

    private record AccountLockerVaultsResultRow(long Id, long FromStateVersion, EntityType ResourceDiscriminator, string ResourceAddress, string VaultAddress, long LastUpdatedAtStateVersion, string? Balance);

    private record AccountLockerTouchedAtResultRow(long AccountLockerEntityId, long AccountEntityId, long LastUpdatedAt);

    public AccountLockerQuerier(IEntityQuerier entityQuerier, ReadOnlyDbContext readOnlyDbContext, IDapperWrapper dapperWrapper)
    {
        _entityQuerier = entityQuerier;
        _dbContext = readOnlyDbContext;
        _dapperWrapper = dapperWrapper;
    }

    public async Task<GatewayModel.StateAccountLockerPageVaultsResponse> AccountLockerVaultsPage(
        IEntityStateQuerier.AccountLockerPageRequest pageRequest,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var accountLocker = await _entityQuerier.GetEntity<GlobalAccountLockerEntity>(pageRequest.AccountLockerAddress.LockerAddress, ledgerState, token);
        var account = await _entityQuerier.GetEntity<GlobalAccountEntity>(pageRequest.AccountLockerAddress.AccountAddress, ledgerState, token);
        var accountLockerEntryDefinition = await _dbContext
            .AccountLockerDefinition
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .AnnotateMetricName("AccountLockerEntryDefinition")
            .FirstOrDefaultAsync(e => e.AccountLockerEntityId == accountLocker.Id && e.AccountEntityId == account.Id, token);

        if (accountLockerEntryDefinition == null)
        {
            throw new AccountLockerNotFoundException(pageRequest.AccountLockerAddress.LockerAddress, pageRequest.AccountLockerAddress.AccountAddress);
        }

        var cd = new CommandDefinition(
            commandText: @"
SELECT d.id AS Id, d.from_state_version AS FromStateVersion, re.discriminator AS ResourceDiscriminator, re.address AS ResourceAddress, ve.address AS VaultAddress, vh.from_state_version AS LastUpdatedAtStateVersion, vh.balance AS Balance
FROM account_locker_entry_resource_vault_definition d
INNER JOIN entities re ON re.id = d.resource_entity_id
INNER JOIN entities ve ON ve.id = d.vault_entity_id
INNER JOIN LATERAL (
    SELECT balance::text AS balance, from_state_version
    FROM vault_balance_history
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

        var vaultsAndOneMore = (await _dapperWrapper.QueryAsync<AccountLockerVaultsResultRow>(_dbContext.Database.GetDbConnection(), cd, "vaultsAndOneMore")).ToList();

        var items = vaultsAndOneMore
            .Take(pageRequest.Limit)
            .Select(k =>
            {
                GatewayModel.AccountLockerVaultCollectionItem result;

                if (k.ResourceDiscriminator == EntityType.GlobalFungibleResource && k.Balance != null)
                {
                    result = new GatewayModel.AccountLockerVaultCollectionItemFungible(
                        amount: TokenAmount.FromSubUnitsString(k.Balance).ToString(),
                        resourceAddress: k.ResourceAddress,
                        vaultAddress: k.VaultAddress,
                        lastUpdatedAtStateVersion: k.LastUpdatedAtStateVersion);
                }
                else if (k.ResourceDiscriminator == EntityType.GlobalNonFungibleResource && k.Balance != null)
                {
                    result = new GatewayModel.AccountLockerVaultCollectionItemNonFungible(
                        totalCount: long.Parse(TokenAmount.FromSubUnitsString(k.Balance).ToString()),
                        resourceAddress: k.ResourceAddress,
                        vaultAddress: k.VaultAddress,
                        lastUpdatedAtStateVersion: k.LastUpdatedAtStateVersion);
                }
                else
                {
                    throw new UnreachableException("AccountLockerVaultViewModel must return either fungible vault balance or non-fungible vault total NFIDs count");
                }

                return result;
            })
            .ToList();

        var nextCursor = vaultsAndOneMore.Count == pageRequest.Limit + 1
            ? new GatewayModel.StateAccountLockerAccountResourcesCursor(vaultsAndOneMore.Last().FromStateVersion, vaultsAndOneMore.Last().Id).ToCursorString()
            : null;

        return new GatewayModel.StateAccountLockerPageVaultsResponse(
            ledgerState: ledgerState,
            lockerAddress: accountLocker.Address,
            accountAddress: account.Address,
            nextCursor: nextCursor,
            items: items
        );
    }

    public async Task<GatewayModel.StateAccountLockersTouchedAtResponse> AccountLockersTouchedAt(IList<AccountLockerAddress> accountLockers, GatewayModel.LedgerState atLedgerState, CancellationToken token = default)
    {
        var entityAddresses = accountLockers.SelectMany(l => new[] { l.LockerAddress, l.AccountAddress }).ToHashSet().ToList();
        var entities = await _entityQuerier.GetEntities(entityAddresses, atLedgerState, token);
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

        accountLockers.Unzip(l => GetAddressOrThrow(l.LockerAddress), l => GetAddressOrThrow(l.AccountAddress), out var accountLockerEntityIds, out var accountEntityIds);

        var cd = new CommandDefinition(
            commandText: @"
WITH variables AS (
    SELECT UNNEST(@accountLockerEntityIds) AS account_locker_entity_id, UNNEST(@accountEntityIds) AS account_entity_id
),
entry_definitions AS (
    SELECT d.*
    FROM account_locker_entry_definition d, variables v
    WHERE d.account_locker_entity_id = v.account_locker_entity_id AND d.account_entity_id = v.account_entity_id AND d.from_state_version <= @stateVersion
)
SELECT ed.account_locker_entity_id AS AccountLockerEntityId, ed.account_entity_id AS AccountEntityId, th.from_state_version AS LastUpdatedAt
FROM entry_definitions ed
INNER JOIN LATERAL (
    SELECT from_state_version
    FROM account_locker_entry_touch_history
    WHERE account_locker_definition_id = ed.id AND from_state_version <= @stateVersion
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON TRUE;",
            parameters: new
            {
                accountLockerEntityIds = accountLockerEntityIds,
                accountEntityIds = accountEntityIds,
                stateVersion = atLedgerState.StateVersion,
            },
            cancellationToken: token);

        var items = new List<GatewayModel.StateAccountLockersTouchedAtResponseItem>();

        foreach (var row in await _dapperWrapper.QueryAsync<AccountLockerTouchedAtResultRow>(_dbContext.Database.GetDbConnection(), cd))
        {
            items.Add(new GatewayModel.StateAccountLockersTouchedAtResponseItem(
                lockerAddress: entitiesById[row.AccountLockerEntityId].Address,
                accountAddress: entitiesById[row.AccountEntityId].Address,
                lastTouchedAtStateVersion: row.LastUpdatedAt));
        }

        return new GatewayModel.StateAccountLockersTouchedAtResponse(atLedgerState, items);
    }
}
