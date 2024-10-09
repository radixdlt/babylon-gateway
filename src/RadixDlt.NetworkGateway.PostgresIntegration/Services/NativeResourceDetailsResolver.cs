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
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class NativeResourceDetailsResolver
{
    private static IDictionary<string, GatewayModel.NativeResourceDetails>? _wellKnownStaticCache;

    private readonly ReadOnlyDbContext _dbContext;
    private readonly IDapperWrapper _dapperWrapper;
    private readonly NetworkConfiguration _networkConfiguration;

    public NativeResourceDetailsResolver(ReadOnlyDbContext dbContext, IDapperWrapper dapperWrapper, NetworkConfiguration networkConfiguration)
    {
        _dbContext = dbContext;
        _dapperWrapper = dapperWrapper;
        _networkConfiguration = networkConfiguration;
    }

    public async Task<IDictionary<EntityAddress, GatewayModel.NativeResourceDetails>> GetNativeResourceDetails(
        ICollection<Entity> entities,
        GatewayModel.LedgerState ledgerState,
        CancellationToken token)
    {
        var result = new Dictionary<EntityAddress, GatewayModel.NativeResourceDetails>(entities.Count);
        var missingEntities = new List<EntityAddress>();

        foreach (var entity in entities)
        {
            if (TryGetWellKnown(entity.Address, out var nativeResourceDetails))
            {
                result[entity.Address] = nativeResourceDetails;
            }
            else
            {
                missingEntities.Add(entity.Address);
            }
        }

        if (missingEntities.Any())
        {
            result.AddRange(await GetFromDatabase(missingEntities, ledgerState, token));
        }

        return result;
    }

    private record struct DbRow(
        EntityRelationship BaseRelationship,
        string BaseEntityAddress,
        TokenAmount? BaseTotalSupply,
        EntityType RootEntityType,
        string RootEntityAddress,
        string? ResourceEntityAddress,
        string? ResourceBalance);

    private async Task<IDictionary<EntityAddress, GatewayModel.NativeResourceDetails>> GetFromDatabase(List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var parameters = new
        {
            addresses = addresses.Select(e => (string)e).ToList(),
            stateVersion = ledgerState.StateVersion,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
            @"WITH
variables AS (
    SELECT
        @stateVersion AS state_version,
        @addresses AS addresses
),
base_entities AS (
    SELECT
        e.address AS base_address,
        s.total_supply AS base_total_supply,
        unnest(e.correlated_entity_relationships) AS base_correlated_entity_relationship,
        unnest(e.correlated_entity_ids) AS base_correlated_entity_id
    FROM variables var, entities e
    LEFT JOIN LATERAL (
        SELECT total_supply
        FROM resource_entity_supply_history
        WHERE from_state_version <= var.state_version AND resource_entity_id = e.id
        ORDER BY from_state_version DESC
        LIMIT 1
    ) s ON TRUE
    WHERE
        e.from_state_version <= var.state_version
      AND e.address = ANY(var.addresses)
      AND e.correlated_entity_relationships && '{unit_resource_of_resource_pool, stake_unit_of_validator, claim_token_of_validator, recovery_badge_of_access_controller}'::entity_relationship[]
),
base_with_root AS (
    SELECT
        base.*,
        root.discriminator AS root_discriminator,
        root.address AS root_address,
        unnest(root.correlated_entity_relationships) as root_correlated_entity_relationship,
        unnest(root.correlated_entity_ids) AS root_correlated_entity_id
    FROM base_entities base
    INNER JOIN entities root ON root.id = base.base_correlated_entity_id
    WHERE base.base_correlated_entity_relationship = ANY('{unit_resource_of_resource_pool, stake_unit_of_validator, claim_token_of_validator, recovery_badge_of_access_controller}'::entity_relationship[])
)
SELECT
    bwr.base_correlated_entity_relationship AS BaseRelationship,
    bwr.base_address AS BaseEntityAddress,
    CAST(bwr.base_total_supply AS TEXT) AS BaseTotalSupply,
    bwr.root_discriminator AS RootEntityType,
    bwr.root_address AS RootEntityAddress,
    re.address AS ResourceEntityAddress,
    CAST(vault.balance AS TEXT) AS ResourceBalance
FROM variables var, base_with_root bwr
LEFT JOIN LATERAL (
    SELECT *
    FROM vault_balance_history
    WHERE from_state_version <= var.state_version AND vault_entity_id = bwr.root_correlated_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) vault ON TRUE
LEFT JOIN entities ve ON ve.id = vault.vault_entity_id
LEFT JOIN entities re ON re.id = ve.correlated_entity_ids[array_position(ve.correlated_entity_relationships, 'vault_to_resource')]
WHERE bwr.root_correlated_entity_relationship = ANY('{resource_pool_to_resource_vault, validator_to_stake_vault, access_controller_to_recovery_badge}'::entity_relationship[]);",
            parameters,
            cancellationToken: token
        );

        var rows = await _dapperWrapper.ToListAsync<DbRow>(_dbContext.Database.GetDbConnection(), cd);

        return rows
            .GroupBy(r => (EntityAddress)r.BaseEntityAddress)
            .ToDictionary(g => g.Key, grouping =>
            {
                var rootEntityType = grouping.First().RootEntityType;
                var rootEntityAddress = grouping.First().RootEntityAddress;
                var baseRelationship = grouping.First().BaseRelationship;

                if (rootEntityType == EntityType.GlobalAccessController)
                {
                    return new GatewayModel.NativeResourceAccessControllerRecoveryBadgeValue(rootEntityAddress);
                }

                if (rootEntityType == EntityType.GlobalValidator && baseRelationship == EntityRelationship.ClaimTokenOfValidator)
                {
                    return new GatewayModel.NativeResourceValidatorClaimNftValue(rootEntityAddress);
                }

                var baseTotalSupply = grouping.First().BaseTotalSupply ?? throw new InvalidOperationException($"BaseTotalSupply cannot be empty on {grouping.Key}");
                var redemptionValues = new List<GatewayModel.NativeResourceRedemptionValueItem>();

                foreach (var entry in grouping)
                {
                    var resourceAddress = entry.ResourceEntityAddress ?? throw new InvalidOperationException($"ResourceEntityAddress cannot be empty on {grouping.Key}");
                    TokenAmount? amount = null;

                    if (baseTotalSupply > TokenAmount.Zero)
                    {
                        var resourceBalance = entry.ResourceBalance ?? throw new InvalidOperationException($"ResourceBalance cannot be empty on {grouping.Key}");
                        amount = TokenAmount.FromSubUnitsString(resourceBalance) / baseTotalSupply;
                    }

                    redemptionValues.Add(new GatewayModel.NativeResourceRedemptionValueItem(resourceAddress, amount?.ToString()));
                }

                GatewayModel.NativeResourceDetails result = rootEntityType switch
                {
                    EntityType.GlobalValidator => new GatewayModel.NativeResourceValidatorLiquidStakeUnitValue(rootEntityAddress, redemptionValues.Count, redemptionValues),
                    EntityType.GlobalOneResourcePool => new GatewayModel.NativeResourceOneResourcePoolUnitValue(rootEntityAddress, redemptionValues.Count, redemptionValues),
                    EntityType.GlobalTwoResourcePool => new GatewayModel.NativeResourceTwoResourcePoolUnitValue(rootEntityAddress, redemptionValues.Count, redemptionValues),
                    EntityType.GlobalMultiResourcePool => new GatewayModel.NativeResourceMultiResourcePoolUnitValue(rootEntityAddress, redemptionValues.Count, redemptionValues),
                    _ => throw new ArgumentOutOfRangeException(nameof(rootEntityType), rootEntityType, null),
                };

                return result;
            });
    }

    private bool TryGetWellKnown(EntityAddress entityAddress, [NotNullWhen(true)] out GatewayModel.NativeResourceDetails? result)
    {
        // might get executed multiple times, but it doesn't really matter
        if (_wellKnownStaticCache == null)
        {
            var wka = _networkConfiguration.WellKnownAddresses;

            _wellKnownStaticCache = new Dictionary<string, GatewayModel.NativeResourceDetails>
            {
                [wka.Xrd] = new GatewayModel.NativeResourceXrdValue(),
                [wka.PackageOwnerBadge] = new GatewayModel.NativeResourcePackageOwnerBadgeValue(),
                [wka.AccountOwnerBadge] = new GatewayModel.NativeResourceAccountOwnerBadgeValue(),
                [wka.IdentityOwnerBadge] = new GatewayModel.NativeResourceIdentityOwnerBadgeValue(),
                [wka.ValidatorOwnerBadge] = new GatewayModel.NativeResourceValidatorOwnerBadgeValue(),
                [wka.Secp256k1SignatureVirtualBadge] = new GatewayModel.NativeResourceSecp256k1SignatureResourceValue(),
                [wka.Ed25519SignatureVirtualBadge] = new GatewayModel.NativeResourceEd25519SignatureResourceValue(),
                [wka.GlobalCallerVirtualBadge] = new GatewayModel.NativeResourceGlobalCallerResourceValue(),
                [wka.PackageOfDirectCallerVirtualBadge] = new GatewayModel.NativeResourcePackageOfDirectCallerResourceValue(),
                [wka.SystemTransactionBadge] = new GatewayModel.NativeResourceSystemExecutionResourceValue(), // TODO is it even valid?
            };
        }

        return _wellKnownStaticCache.TryGetValue(entityAddress, out result);
    }
}
