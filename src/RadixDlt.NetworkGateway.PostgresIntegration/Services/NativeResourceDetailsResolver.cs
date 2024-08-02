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

using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    public async Task<IDictionary<EntityAddress, GatewayModel.NativeResourceDetails>> GetNativeResourceDetails(ICollection<Entity> entities, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var res = new Dictionary<EntityAddress, GatewayModel.NativeResourceDetails>(entities.Count);

        var dbLookup = new List<EntityAddress>();

        foreach (var entity in entities)
        {
            if (TryGetWellKnownNativeResourceDetails(entity.Address, out var nativeResourceDetails))
            {
                res[entity.Address] = nativeResourceDetails;
            }
            else
            {
                dbLookup.Add(entity.Address);
            }
        }

        if (dbLookup.Any())
        {
            var xxx = await Redemption_InnerImpl(dbLookup, ledgerState, token);

            foreach (var (ea, val) in xxx)
            {
                // weird spec requirement
                const int MaxToReturn = 20;

                var itemsCount = val.Items.Count;
                var items = val.Items.Select(x => new GatewayModel.NativeResourceRedemptionValueItem(x.ResourceAddress, x.Amount?.ToString())).ToList();
                var x = items.Count > MaxToReturn ? new List<GatewayModel.NativeResourceRedemptionValueItem>() : items;

                res[ea] = val.OwningEntityType switch
                {
                    EntityType.GlobalValidator => new GatewayModel.NativeResourceValidatorLiquidStakeUnitValue(val.OwningEntityAddress, itemsCount, x),
                    EntityType.GlobalOneResourcePool => new GatewayModel.NativeResourceOneResourcePoolUnitValue(val.OwningEntityAddress, itemsCount, x),
                    EntityType.GlobalTwoResourcePool => new GatewayModel.NativeResourceTwoResourcePoolUnitValue(val.OwningEntityAddress, itemsCount, x),
                    EntityType.GlobalMultiResourcePool => new GatewayModel.NativeResourceMultiResourcePoolUnitValue(val.OwningEntityAddress, itemsCount, x),
                    _ => throw new ArgumentOutOfRangeException(nameof(val.OwningEntityType), val.OwningEntityType, null),
                };
            }
        }

        return res;
    }

    private record struct XxxOutput(EntityType OwningEntityType, EntityAddress OwningEntityAddress, TokenAmount UnitTotalSupply, ICollection<UnitRedemptionValue> Items);

    private record struct UnitRedemptionValue(EntityAddress ResourceAddress, TokenAmount? Amount);

    private record XxxRes(EntityType OwningEntityType, string OwningEntityAddress, string UnitAddress, string UnitTotalSupply, string ResourceAddress, string Amount);

    private async Task<IDictionary<EntityAddress, XxxOutput>> Redemption_InnerImpl(ICollection<EntityAddress> fungibleResourceAddresses, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var addresses = fungibleResourceAddresses.ToHashSet().Select(x => x.ToString()).ToList();

        if (addresses.Count == 0)
        {
            return ImmutableDictionary<EntityAddress, XxxOutput>.Empty;
        }

        var res = await _dapperWrapper.ToList<XxxRes>(
            _dbContext,
            @"WITH
unit_resources AS (
    SELECT
        e.address AS unit_address,
        s.total_supply AS unit_total_supply,
        e.correlated_entity_ids[coalesce(array_position(e.correlated_entity_relationships, 'resource_pool_unit_resource_pool'), array_position(e.correlated_entity_relationships, 'validator_stake_unit_validator'))] AS unit_owner_id
    FROM entities e
    INNER JOIN LATERAL (
        SELECT total_supply
        FROM resource_entity_supply_history
        WHERE from_state_version <= @stateVersion AND resource_entity_id = e.id
        ORDER BY from_state_version DESC
        LIMIT 1
    ) s ON TRUE
    WHERE
        e.from_state_version <= @stateVersion
      AND e.address = ANY(@addresses)
      AND ('resource_pool_unit_resource_pool' = ANY(e.correlated_entity_relationships) OR 'validator_stake_unit_validator' = ANY(e.correlated_entity_relationships))
),
unit_resource_with_owner_correlations AS (
    SELECT ur.*, oe.discriminator AS owner_discriminator, oe.address AS owner_address, unnest(oe.correlated_entity_relationships) as owner_correlated_entity_relationship, unnest(oe.correlated_entity_ids) AS owner_correlated_entity_id
    FROM unit_resources ur
    INNER JOIN entities oe ON oe.id = ur.unit_owner_id
)
SELECT
    x.owner_discriminator AS OwningEntityType,
    x.owner_address AS OwningEntityAddress,
    x.unit_address AS UnitAddress,
    CAST(x.unit_total_supply AS TEXT) AS UnitTotalSupply,
    re.address AS ResourceAddress,
    CAST(vault.balance AS TEXT) AS Amount
FROM unit_resource_with_owner_correlations x
INNER JOIN LATERAL (
    SELECT *
    FROM entity_vault_history
    WHERE from_state_version <= @stateVersion AND vault_entity_id = x.owner_correlated_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) vault ON TRUE
INNER JOIN entities re ON re.id = vault.resource_entity_id
WHERE x.owner_correlated_entity_relationship = 'resource_pool_resource_vault' OR x.owner_correlated_entity_relationship = 'validator_stake_vault';",
            new
            {
                addresses = addresses,
                stateVersion = ledgerState.StateVersion,
            },
            token);

        var result = new Dictionary<EntityAddress, XxxOutput>();

        foreach (var row in res)
        {
            var entry = result.GetOrAdd((EntityAddress)row.UnitAddress, _ => new XxxOutput(row.OwningEntityType, (EntityAddress)row.OwningEntityAddress, TokenAmount.FromSubUnitsString(row.UnitTotalSupply), new List<UnitRedemptionValue>()));

            TokenAmount? amount = null;

            if (entry.UnitTotalSupply > TokenAmount.Zero)
            {
                amount = TokenAmount.FromSubUnitsString(row.Amount) / entry.UnitTotalSupply;
            }

            entry.Items.Add(new UnitRedemptionValue((EntityAddress)row.ResourceAddress, amount));
        }

        return result;
    }

    private bool TryGetWellKnownNativeResourceDetails(EntityAddress entityAddress, [NotNullWhen(true)] out GatewayModel.NativeResourceDetails? result)
    {
        // might get executed multiple times but it doesn't really matter
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
