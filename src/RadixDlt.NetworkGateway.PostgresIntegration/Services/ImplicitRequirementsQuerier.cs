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
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class ImplicitRequirementsQuerier : IImplicitRequirementsQuerier
{
    private record struct ImplicitRequirementLookup(QueriedImplicitRequirementType Type, string Hash);

    private record struct ImplicitRequirementQueryResult(
        QueriedImplicitRequirementType QueriedType,
        string QueriedHash,
        long? FirstSeenStateVersion,
        ImplicitRequirementType ResolvedType,
        EntityAddress? EntityAddress,
        string? BlueprintName,
        byte[] PublicKeyBytes);

    private readonly ReadOnlyDbContext _dbContext;
    private readonly IDapperWrapper _dapperWrapper;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public ImplicitRequirementsQuerier(
        ReadOnlyDbContext dbContext,
        IDapperWrapper dapperWrapper,
        INetworkConfigurationProvider networkConfigurationProvider)
    {
        _dbContext = dbContext;
        _dapperWrapper = dapperWrapper;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<GatewayModel.ImplicitRequirementsLookupResponse> ImplicitRequirementsLookup(
        List<GatewayModel.NonFungibleGlobalId> nonFungibleGlobalIds,
        CancellationToken token = default)
    {
        if (nonFungibleGlobalIds.Count == 0)
        {
            return new GatewayModel.ImplicitRequirementsLookupResponse(new List<GatewayModel.ImplicitRequirementsLookupCollectionItem>());
        }

        var wellKnownAddresses = _networkConfigurationProvider.GetNetworkConfiguration().WellKnownAddresses;

        var implicitRequirementsToResolve = new Dictionary<ImplicitRequirementLookup, GatewayModel.NonFungibleGlobalId>();
        var resolvedSystemExecutionRequirements = new Dictionary<string, GatewayModel.ImplicitRequirementsLookupCollectionItem>();

        foreach (var item in nonFungibleGlobalIds)
        {
            if (item.ResourceAddress == wellKnownAddresses.SystemTransactionBadge)
            {
                var mapped = MapSystemExecutionImplicitRequirements(item);
                resolvedSystemExecutionRequirements.TryAdd(item.NonFungibleId, mapped);
            }
            else
            {
                var lookup = MapToLookup(item, wellKnownAddresses);
                implicitRequirementsToResolve.TryAdd(lookup, item);
            }
        }

        Dictionary<ImplicitRequirementLookup, ImplicitRequirementQueryResult> resolvedFromDb = new Dictionary<ImplicitRequirementLookup, ImplicitRequirementQueryResult>();

        if (implicitRequirementsToResolve.Count > 0)
        {
            implicitRequirementsToResolve.Keys.Unzip(
                x => x.Type,
                x => x.Hash,
                out var implicitRequirementTypes,
                out var implicitRequirementHashes);

            var parameters = new
            {
                implicitRequirementTypes = implicitRequirementTypes,
                implicitRequirementHashes = implicitRequirementHashes,
            };

            var cd = DapperExtensions.CreateCommandDefinition(
                @"
WITH variables(queried_type, hash) AS
(
    SELECT
        UNNEST(@implicitRequirementTypes),
        UNNEST(@implicitRequirementHashes)
)
SELECT
    variables.queried_type       AS QueriedType
     , variables.hash               AS QueriedHash
     , ir.first_seen_state_version  AS FirstSeenStateVersion
     , ir.discriminator             AS ResolvedType
     , e.address                    AS EntityAddress
     , ir.blueprint_name            AS BlueprintName
     , ir.public_key_bytes          AS PublicKeyBytes
FROM variables
LEFT JOIN LATERAL (
        SELECT
            first_seen_state_version
             ,discriminator
             ,entity_id
             ,blueprint_name
             ,public_key_bytes
        FROM implicit_requirements ir
        WHERE
            variables.queried_type = 'package_of_direct_caller'::queried_implicit_requirement_type AND
            ir.discriminator = 'package_of_direct_caller' AND
            ir.hash = variables.hash
    UNION
        SELECT
            first_seen_state_version
             ,discriminator
             ,entity_id
             ,blueprint_name
             ,public_key_bytes
        FROM implicit_requirements ir
        WHERE
            variables.queried_type = 'ed25519public_key'::queried_implicit_requirement_type AND
            ir.discriminator = 'ed25519public_key' AND
            ir.hash = variables.hash
    UNION
        SELECT
            first_seen_state_version
             ,discriminator
             ,entity_id
             ,blueprint_name
             ,public_key_bytes
        FROM implicit_requirements ir
        WHERE
            variables.queried_type = 'secp256k1public_key'::queried_implicit_requirement_type AND
            ir.discriminator = 'secp256k1public_key' AND
            ir.hash = variables.hash
    UNION
        SELECT
            first_seen_state_version
             ,discriminator
             ,entity_id
             ,blueprint_name
             ,public_key_bytes
        FROM implicit_requirements ir
        WHERE
            variables.queried_type = 'global_caller'::queried_implicit_requirement_type AND
            (ir.discriminator = 'global_caller_entity' OR ir.discriminator = 'global_caller_blueprint') AND
            ir.hash = variables.hash
) ir ON true
LEFT JOIN entities e ON ir.entity_id = e.id;",
                parameters,
                cancellationToken: token
            );

            resolvedFromDb = (await _dapperWrapper.ToListAsync<ImplicitRequirementQueryResult>(_dbContext.Database.GetDbConnection(), cd))
                .ToDictionary(x => new ImplicitRequirementLookup(x.QueriedType, x.QueriedHash), y => y);
        }

        var result = new List<GatewayModel.ImplicitRequirementsLookupCollectionItem>();

        foreach (var requestNfid in nonFungibleGlobalIds)
        {
            if (requestNfid.ResourceAddress == wellKnownAddresses.SystemTransactionBadge)
            {
                var mappedResultItem = resolvedSystemExecutionRequirements[requestNfid.NonFungibleId];
                result.Add(mappedResultItem);
            }
            else
            {
                var lookup = MapToLookup(requestNfid, wellKnownAddresses);
                var dbResolvedItem = resolvedFromDb[lookup];
                var dbResolvedMappedItem = MapDbResolvedItem(dbResolvedItem, requestNfid);
                result.Add(dbResolvedMappedItem);
            }
        }

        return new GatewayModel.ImplicitRequirementsLookupResponse(result);
    }

    private ImplicitRequirementLookup MapToLookup(GatewayModel.NonFungibleGlobalId item, WellKnownAddresses wellKnownAddresses)
    {
        if (item.ResourceAddress == wellKnownAddresses.Secp256k1SignatureVirtualBadge)
        {
            return new ImplicitRequirementLookup(QueriedImplicitRequirementType.Secp256k1PublicKey, item.NonFungibleId);
        }

        if (item.ResourceAddress == wellKnownAddresses.Ed25519SignatureVirtualBadge)
        {
            return new ImplicitRequirementLookup(QueriedImplicitRequirementType.Ed25519PublicKey, item.NonFungibleId);
        }

        if (item.ResourceAddress == wellKnownAddresses.GlobalCallerVirtualBadge)
        {
            return new ImplicitRequirementLookup(QueriedImplicitRequirementType.GlobalCaller, item.NonFungibleId);
        }

        if (item.ResourceAddress == wellKnownAddresses.PackageOfDirectCallerVirtualBadge)
        {
            return new ImplicitRequirementLookup(QueriedImplicitRequirementType.PackageOfDirectCaller, item.NonFungibleId);
        }

        throw new NotSupportedException($"Not supported implicit requirement type: {item.ResourceAddress}");
    }

    private GatewayModel.ImplicitRequirementsLookupCollectionItem MapSystemExecutionImplicitRequirements(GatewayModel.NonFungibleGlobalId item)
    {
        const string ProtocolExecutionNfid = "#0#";
        const string ValidatorExecutionNfid = "#1#";

        return item.NonFungibleId switch
        {
            ProtocolExecutionNfid => new GatewayModel.ImplicitRequirementsLookupCollectionItem(requirement: item, resolved: new GatewayModel.ResolvedProtocolExecutionImplicitRequirement()),
            ValidatorExecutionNfid => new GatewayModel.ImplicitRequirementsLookupCollectionItem(requirement: item, resolved: new GatewayModel.ResolvedValidatorExecutionImplicitRequirement()),
            _ => new GatewayModel.ImplicitRequirementsLookupCollectionItem(requirement: item, resolved: null),
        };
    }

    private GatewayModel.ImplicitRequirementsLookupCollectionItem MapDbResolvedItem(ImplicitRequirementQueryResult resultRow, GatewayModel.NonFungibleGlobalId requestItem)
    {
        if (!resultRow.FirstSeenStateVersion.HasValue)
        {
            return
                new GatewayModel.ImplicitRequirementsLookupCollectionItem(
                    requirement: requestItem,
                    resolved: null
                );
        }

        GatewayModel.ResolvedImplicitRequirement resolved = resultRow.ResolvedType switch
        {
            ImplicitRequirementType.PackageOfDirectCaller =>
                new GatewayModel.ResolvedPackageOfDirectCallerImplicitRequirement(resultRow.FirstSeenStateVersion.Value, resultRow.EntityAddress),
            ImplicitRequirementType.GlobalCallerEntity =>
                new GatewayModel.ResolvedGlobalCallerEntityImplicitRequirement(resultRow.FirstSeenStateVersion.Value, resultRow.EntityAddress),
            ImplicitRequirementType.GlobalCallerBlueprint =>
                new GatewayModel.ResolvedGlobalCallerBlueprintImplicitRequirement(resultRow.FirstSeenStateVersion.Value, resultRow.EntityAddress, resultRow.BlueprintName),
            ImplicitRequirementType.Ed25519PublicKey =>
                new GatewayModel.ResolvedEd25519PublicKeyImplicitRequirement(resultRow.FirstSeenStateVersion.Value, resultRow.PublicKeyBytes.ToHex()),
            ImplicitRequirementType.Secp256k1PublicKey =>
                new GatewayModel.ResolvedSecp256k1PublicKeyImplicitRequirement(resultRow.FirstSeenStateVersion.Value, resultRow.PublicKeyBytes.ToHex()),
            _ => throw new NotSupportedException($"Not supported implicit requirement type: {resultRow.ResolvedType}"),
        };

        return new GatewayModel.ImplicitRequirementsLookupCollectionItem(
            requirement: requestItem,
            resolved: resolved);
    }
}
