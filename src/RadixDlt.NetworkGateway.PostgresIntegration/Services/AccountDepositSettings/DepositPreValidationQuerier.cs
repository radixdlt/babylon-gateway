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
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services.AccountDepositSettings;

internal class DepositPreValidationQuerier : IDepositPreValidationQuerier
{
    private record DepositPreValidationResourcePreferenceRulesViewModel(
        long ResourceEntityId,
        AccountResourcePreferenceRule AccountResourcePreferenceRule);

    private readonly IDapperWrapper _dapperWrapper;
    private readonly ReadOnlyDbContext _dbContext;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IEntityQuerier _entityQuerier;

    public DepositPreValidationQuerier(
        IDapperWrapper dapperWrapper,
        ReadOnlyDbContext dbContext,
        INetworkConfigurationProvider networkConfigurationProvider,
        IEntityQuerier entityQuerier)
    {
        _dapperWrapper = dapperWrapper;
        _dbContext = dbContext;
        _networkConfigurationProvider = networkConfigurationProvider;
        _entityQuerier = entityQuerier;
    }

    public async Task<GatewayModel.AccountDepositPreValidationDecidingFactors> AccountTryDepositPreValidation(
        EntityAddress accountAddress,
        EntityAddress[] resourceAddresses,
        EntityAddress? badgeResourceAddress,
        string? nonFungibleBadgeNfid,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default
    )
    {
        var accountEntity = await _entityQuerier.GetNonVirtualEntity<GlobalAccountEntity>(_dbContext, accountAddress, ledgerState, token);
        var xrdResourceAddress = (await _networkConfigurationProvider.GetNetworkConfiguration(token)).WellKnownAddresses.Xrd;
        var addressesToResolve = resourceAddresses.ToList();
        if (badgeResourceAddress.HasValue)
        {
            addressesToResolve.Add(badgeResourceAddress.Value);
        }

        var entityAddressToIdDictionary = await _entityQuerier.ResolveEntityIds(_dbContext, addressesToResolve, ledgerState, token);
        var resourceEntityMap = entityAddressToIdDictionary
            .Where(x => resourceAddresses.Contains(x.Key))
            .ToDictionary();

        var accountDefaultDepositRule =
            await _dbContext
                .AccountDefaultDepositRuleHistory
                .Where(x => x.AccountEntityId == accountEntity.Id)
                .OrderByDescending(x => x.FromStateVersion)
                .FirstAsync(token);

        var vaultsExistence = await GetResourceVaultsExistence(accountEntity.Id, resourceEntityMap, ledgerState, token);

        var isBadgeAuthorizedDepositor = badgeResourceAddress.HasValue &&
                                         await IsBadgeAuthorizedDepositor(
                                             accountEntity.Id,
                                             entityAddressToIdDictionary[badgeResourceAddress.Value],
                                             nonFungibleBadgeNfid,
                                             ledgerState,
                                             token);

        var existingResourcePreferenceRules =
            await GetExistingResourcePreferenceRulesForResources(
                accountEntity.Id,
                resourceEntityMap,
                ledgerState,
                token);

        var resourcePreferencesDecidingFactors = new List<GatewayModel.AccountDepositPreValidationDecidingFactorsResourceSpecificDetailsItem>();

        foreach (var resourceAddress in resourceAddresses)
        {
            var resourcePreferenceExists = existingResourcePreferenceRules.TryGetValue(resourceAddress, out var resourcePreference);
            var vaultExists = vaultsExistence[resourceAddress];
            var isXrd = resourceAddress == xrdResourceAddress;

            resourcePreferencesDecidingFactors.Add(
                new GatewayModel.AccountDepositPreValidationDecidingFactorsResourceSpecificDetailsItem(
                    resourceAddress,
                    vaultExists,
                    isXrd,
                    resourcePreferenceExists ? resourcePreference!.AccountResourcePreferenceRule.ToGatewayModel() : null
                )
            );
        }

        var decidingFactors = new GatewayModel.AccountDepositPreValidationDecidingFactors(
            badgeResourceAddress.HasValue ? isBadgeAuthorizedDepositor : null,
            accountDefaultDepositRule.DefaultDepositRule.ToGatewayModel(),
            resourcePreferencesDecidingFactors);

        return decidingFactors;
    }

    private async Task<Dictionary<EntityAddress, DepositPreValidationResourcePreferenceRulesViewModel>> GetExistingResourcePreferenceRulesForResources(
        long accountEntityId,
        Dictionary<EntityAddress, long> resourceMap,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default
    )
    {
        var resourcePreferencesQuery = new CommandDefinition(
            commandText: @"
SELECT arpreh.resource_entity_id AS ResourceEntityId, arpreh.account_resource_preference_rule AS AccountResourcePreferenceRule
FROM account_resource_preference_rule_entry_history arpreh
INNER JOIN (
    SELECT *
    FROM account_resource_preference_rule_aggregate_history
    WHERE account_entity_id = @accountEntityId and from_state_version <= @stateVersion
    ORDER BY from_state_version desc
    limit 1
) AS arprah
ON arpreh.id = ANY(arprah.entry_ids) AND arpreh.resource_entity_id = ANY(@resourceEntityIds)
WHERE arpreh.account_entity_id = @accountEntityId AND arpreh.from_state_version <= @stateVersion",
            parameters: new
            {
                accountEntityId = accountEntityId,
                stateVersion = ledgerState.StateVersion,
                resourceEntityIds = resourceMap.Select(x => x.Value).ToList(),
            },
            cancellationToken: token);

        var resourcePreferencesQueryResult =
            (await _dapperWrapper.QueryAsync<DepositPreValidationResourcePreferenceRulesViewModel>(_dbContext.Database.GetDbConnection(), resourcePreferencesQuery)).ToList();

        var inverseResourceMap = resourceMap.ToDictionary(i => i.Value, i => i.Key);
        return resourcePreferencesQueryResult.ToDictionary(
            x => inverseResourceMap[x.ResourceEntityId],
            x => x);
    }

    private async Task<bool> IsBadgeAuthorizedDepositor(
        long accountEntityId,
        long badgeResourceEntityId,
        string? nonFungibleBadgeNfid,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var authorizedDepositorQuery = new CommandDefinition(
            commandText: @"
SELECT 1
FROM account_authorized_depositor_entry_history aadeh
INNER JOIN (
    SELECT *
    FROM account_authorized_depositor_aggregate_history
    WHERE account_entity_id = @accountEntityId and from_state_version <= @stateVersion
    ORDER BY from_state_version desc
    limit 1
) AS aadah
ON aadeh.id = ANY(aadah.entry_ids) AND
(
    (aadeh.resource_entity_id = @badgeResourceEntityId AND @nonFungibleId IS NULL AND aadeh.discriminator = 'resource')
    OR (aadeh.resource_entity_id = @badgeResourceEntityId AND aadeh.non_fungible_id = @nonFungibleId AND aadeh.discriminator = 'non_fungible')
)
WHERE aadeh.account_entity_id = @accountEntityId AND aadeh.from_state_version <= @stateVersion;",
            parameters: new
            {
                accountEntityId = accountEntityId,
                stateVersion = ledgerState.StateVersion,
                badgeResourceEntityId = badgeResourceEntityId,
                nonFungibleId = nonFungibleBadgeNfid,
            },
            cancellationToken: token);

        var authorizedDepositorEntry = await _dapperWrapper.QueryFirstOrDefaultAsync<int?>(
            _dbContext.Database.GetDbConnection(),
            authorizedDepositorQuery);

        return authorizedDepositorEntry != null;
    }

    private async Task<Dictionary<EntityAddress, bool>> GetResourceVaultsExistence(
        long accountEntityId,
        Dictionary<EntityAddress, long> resourcesDictionary,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var resourceEntityIds = resourcesDictionary.Values.ToList();

        var existingResourceDefinitions = await _dbContext
            .EntityResourceEntryDefinition
            .Where(e => e.EntityId == accountEntityId && resourceEntityIds.Contains(e.ResourceEntityId))
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .Select(e => e.ResourceEntityId)
            .ToListAsync(token);

        return resourcesDictionary.ToDictionary(resource => resource.Key, resource => existingResourceDefinitions.Contains(resource.Value));
    }
}
