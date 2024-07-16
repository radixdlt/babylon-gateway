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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.Abstractions.StandardMetadata;

public sealed class TwoWayLinkResolver
{
    private readonly IUnverifiedTwoWayLinksCollection _unverified;
    private readonly bool _resolveValidOnly;
    private readonly bool _validateOnLedgerOnly;

    public TwoWayLinkResolver(IUnverifiedTwoWayLinksCollection unverified, bool resolveValidOnly, bool validateOnLedgerOnly)
    {
        _unverified = unverified;
        _resolveValidOnly = resolveValidOnly;
        _validateOnLedgerOnly = validateOnLedgerOnly;
    }

    public async ValueTask<IDictionary<EntityAddress, List<ResolvedTwoWayLink>>> Resolve(ICollection<EntityAddress> entityAddresses)
    {
        var results = new ConcurrentDictionary<EntityAddress, List<ResolvedTwoWayLink>>();

        foreach (var entityAddress in entityAddresses)
        {
            if (!_unverified.TryGetTwoWayLinks(entityAddress, out var unverifiedTwoWayLinks))
            {
                continue;
            }

            // first round
            foreach (var unverifiedTwoWayLink in unverifiedTwoWayLinks)
            {
                if (unverifiedTwoWayLink is DappAccountLockerUnverifiedTwoWayLink)
                {
                    continue;
                }

                IEnumerable<ResolvedTwoWayLink> candidates = unverifiedTwoWayLink switch
                {
                    DappAccountTypeUnverifiedTwoWayLink dappAccountType => Resolve(entityAddress, dappAccountType),
                    DappClaimedEntitiesUnverifiedTwoWayLink dappClaimedEntities => Resolve(entityAddress, dappClaimedEntities),
                    DappClaimedWebsitesUnverifiedTwoWayLink dappClaimedWebsites => await Resolve(entityAddress, dappClaimedWebsites),
                    DappDefinitionsUnverifiedTwoWayLink dappDefinitions => Resolve(entityAddress, dappDefinitions),
                    DappDefinitionUnverifiedTwoWayLink dappDefinition => Resolve(entityAddress, dappDefinition),
                    _ => throw new ArgumentOutOfRangeException(nameof(unverifiedTwoWayLink), unverifiedTwoWayLink, null),
                };

                if (_resolveValidOnly)
                {
                    candidates = candidates.Where(x => x.IsValid);
                }

                results.GetOrAdd(entityAddress, _ => []).AddRange(candidates);
            }

            // second round
            foreach (var unverifiedTwoWayLink in unverifiedTwoWayLinks)
            {
                if (unverifiedTwoWayLink is not DappAccountLockerUnverifiedTwoWayLink dappAccountLocker)
                {
                    continue;
                }

                var candidates = Resolve(entityAddress, dappAccountLocker, results);

                if (_resolveValidOnly)
                {
                    candidates = candidates.Where(x => x.IsValid);
                }

                results.GetOrAdd(entityAddress, _ => []).AddRange(candidates);
            }
        }

        return results;
    }

    private IEnumerable<DappAccountMarkerResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappAccountTypeUnverifiedTwoWayLink dappAccountType)
    {
        var validationFailure = ValidateDappAccountType(entityAddress, dappAccountType.Value);

        yield return new DappAccountMarkerResolvedTwoWayLink(validationFailure);
    }

    private IEnumerable<DappClaimedEntityResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappClaimedEntitiesUnverifiedTwoWayLink dappClaimedEntities)
    {
        foreach (var claimedEntity in dappClaimedEntities.ClaimedEntities)
        {
            var validationFailure = ValidateDappClaimedEntitiesEntry(entityAddress, claimedEntity);

            yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntity, validationFailure);
        }
    }

    private async ValueTask<IEnumerable<DappClaimedWebsiteResolvedTwoWayLink>> Resolve(EntityAddress entityAddress, DappClaimedWebsitesUnverifiedTwoWayLink dappClaimedWebsites)
    {
        var result = new ConcurrentQueue<DappClaimedWebsiteResolvedTwoWayLink>();

        await Parallel.ForEachAsync(dappClaimedWebsites.ClaimedWebsites, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (claimedWebsite, _) =>
        {
            var validationFailure = await ValidateDappClaimedWebsitesEntry(entityAddress, claimedWebsite);

            result.Enqueue(new DappClaimedWebsiteResolvedTwoWayLink(claimedWebsite, validationFailure));
        });

        return result;
    }

    private IEnumerable<DappDefinitionsResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappDefinitionsUnverifiedTwoWayLink dappDefinitions)
    {
        foreach (var dappDefinition in dappDefinitions.DappDefinitions)
        {
            var validationFailure = ValidateDappDefinitionsEntry(entityAddress, dappDefinition);

            yield return new DappDefinitionsResolvedTwoWayLink(dappDefinition, validationFailure);
        }
    }

    private IEnumerable<DappDefinitionResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappDefinitionUnverifiedTwoWayLink dappDefinition)
    {
        var validationFailure = ValidateDappDefinition(entityAddress, dappDefinition.DappDefinition);

        yield return new DappDefinitionResolvedTwoWayLink(dappDefinition.DappDefinition, validationFailure);
    }

    private IEnumerable<DappAccountLockerResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappAccountLockerUnverifiedTwoWayLink dappAccountLocker, IDictionary<EntityAddress, List<ResolvedTwoWayLink>> alreadyResolved)
    {
        var validationFailure = ValidateDappAccountLocker(entityAddress, dappAccountLocker.LockerAddress, alreadyResolved);

        yield return new DappAccountLockerResolvedTwoWayLink(dappAccountLocker.LockerAddress, validationFailure);
    }

    private string? ValidateDappAccountType(EntityAddress entityAddress, string dappAccountType)
    {
        if (!entityAddress.IsAccount)
        {
            return "entity is not of an account type";
        }

        if (dappAccountType != StandardMetadataConstants.DappAccountTypeDappDefinition)
        {
            return "expected 'dapp definition'";
        }

        return null;
    }

    private string? ValidateDappClaimedEntitiesEntry(EntityAddress entityAddress, EntityAddress otherEntityAddress)
    {
        if (!entityAddress.IsAccount)
        {
            return "entity is not of an account type";
        }

        if (!_unverified.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(entityAddress, out var dappAccountType) || dappAccountType.Value != StandardMetadataConstants.DappAccountTypeDappDefinition)
        {
            return "entity misses dappAccountType=dapp definition marker";
        }

        if (otherEntityAddress.IsResource)
        {
            if (!_unverified.TryGetTwoWayLink<DappDefinitionsUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappDefinitions))
            {
                return "other entity two way link of type dapp_definitions not found";
            }

            if (!otherDappDefinitions.DappDefinitions.Contains(entityAddress))
            {
                return "claimed entity not found in other's entity dapp_definitions";
            }
        }
        else if (otherEntityAddress.IsGlobal)
        {
            if (!_unverified.TryGetTwoWayLink<DappDefinitionUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappDefinition))
            {
                return "other entity two way link of type dapp_definition not found";
            }

            if (otherDappDefinition.DappDefinition != entityAddress)
            {
                return "claimed entity id not found in other's entity dapp_definition";
            }
        }
        else
        {
            return "invalid other entity type";
        }

        return null;
    }

    private async ValueTask<string?> ValidateDappClaimedWebsitesEntry(EntityAddress entityAddress, Uri claimedWebsite)
    {
        if (!entityAddress.IsAccount)
        {
            return "entity is not of an account type";
        }

        if (!_unverified.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(entityAddress, out var dappAccountType) || dappAccountType.Value != StandardMetadataConstants.DappAccountTypeDappDefinition)
        {
            return "entity misses dappAccountType=dapp definition marker";
        }

        if (_validateOnLedgerOnly)
        {
            return "off-ledger validation disabled";
        }

        if (!Uri.TryCreate(claimedWebsite, "/.well-known/radix.json", out _))
        {
            return "unable to construct validation url";
        }

        await ValueTask.CompletedTask;

        return "not implemented yet";
    }

    private string? ValidateDappDefinitionsEntry(EntityAddress entityAddress, EntityAddress otherEntityAddress)
    {
        if (entityAddress.IsAccount)
        {
            if (!_unverified.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(entityAddress, out var dappAccountType) || dappAccountType.Value != StandardMetadataConstants.DappAccountTypeDappDefinition)
            {
                return "entity misses dappAccountType=dapp definition marked";
            }

            if (!otherEntityAddress.IsAccount)
            {
                return "other entity is not of type account";
            }

            if (!_unverified.TryGetTwoWayLink<DappDefinitionsUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappDefinitions))
            {
                return "other entity misses dapp_definitions";
            }

            if (!otherDappDefinitions.DappDefinitions.Contains(entityAddress))
            {
                return "dapp definition not found in other's entity dapp_definitions";
            }
        }
        else if (entityAddress.IsResource)
        {
            if (!otherEntityAddress.IsAccount)
            {
                return "other entity is not of type account";
            }

            if (!_unverified.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappAccountType) || otherDappAccountType.Value != StandardMetadataConstants.DappAccountTypeDappDefinition)
            {
                return "other entity misses dappAccountType=dapp definition marker";
            }

            if (!_unverified.TryGetTwoWayLink<DappClaimedEntitiesUnverifiedTwoWayLink>(otherEntityAddress, out var otherClaimedEntities))
            {
                return "other entity misses claimed_entities";
            }

            if (!otherClaimedEntities.ClaimedEntities.Contains(entityAddress))
            {
                return "dapp definition not found in other's entity claimed_entities";
            }
        }
        else
        {
            return "entity is not of an account / resource type";
        }

        return null;
    }

    private string? ValidateDappDefinition(EntityAddress entityAddress, EntityAddress otherEntityAddress)
    {
        if (entityAddress.IsResource)
        {
            return "entity is a resource";
        }

        if (!entityAddress.IsGlobal)
        {
            return "entity is not of a global type";
        }

        if (!otherEntityAddress.IsAccount)
        {
            return "other entity is not of account type";
        }

        if (!_unverified.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappAccountType) || otherDappAccountType.Value != StandardMetadataConstants.DappAccountTypeDappDefinition)
        {
            return "other entity misses account_type marker";
        }

        if (!_unverified.TryGetTwoWayLink<DappClaimedEntitiesUnverifiedTwoWayLink>(otherEntityAddress, out var otherClaimedEntities))
        {
            return "other entity two-way link of type claimed_entities not found";
        }

        if (!otherClaimedEntities.ClaimedEntities.Contains(entityAddress))
        {
            return "claimed entity id not found in dapp_definitions of the other entity";
        }

        return null;
    }

    private string? ValidateDappAccountLocker(EntityAddress entityAddress, EntityAddress lockerEntityAddress, IDictionary<EntityAddress, List<ResolvedTwoWayLink>> alreadyResolved)
    {
        if (!entityAddress.IsAccount)
        {
            return "entity is not of an account type";
        }

        if (!lockerEntityAddress.IsLocker)
        {
            return "entity is not of a locker type";
        }

        if (!alreadyResolved.TryGetValue(entityAddress, out var entityAlreadyResolved))
        {
            return "missing already resolved on entity";
        }

        if (!entityAlreadyResolved.OfType<DappAccountMarkerResolvedTwoWayLink>().Any(def => def.IsValid))
        {
            return "entity misses account_type=dapp definition marker";
        }

        if (!entityAlreadyResolved.OfType<DappClaimedEntityResolvedTwoWayLink>().Any(def => def.IsValid && def.EntityAddress == lockerEntityAddress))
        {
            return "entity misses valid(!) claimed_entities pointing to " + lockerEntityAddress;
        }

        return null;
    }
}
