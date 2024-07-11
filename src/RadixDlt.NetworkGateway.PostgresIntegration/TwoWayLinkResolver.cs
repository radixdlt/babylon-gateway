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

using RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal class UnverifiedTwoWayLinks
{
    private readonly IDictionary<long, Entity> _entities;
    private readonly IDictionary<long, List<UnverifiedTwoWayLinkEntryHistory>> _unverified;

    public UnverifiedTwoWayLinks(ICollection<Entity> entities, ICollection<UnverifiedTwoWayLinkEntryHistory> unverified)
    {
        _entities = entities.ToDictionary(x => x.Id);
        _unverified = unverified.GroupBy(x => x.EntityId).ToDictionary(g => g.Key, g => g.ToList());
    }

    public bool TryGetTwoWayLinks(long entityId, [NotNullWhen(true)] out List<UnverifiedTwoWayLinkEntryHistory>? twoWayLinks)
    {
        return _unverified.TryGetValue(entityId, out twoWayLinks);
    }

    public bool TryGetTwoWayLink<T>(long entityId, [NotNullWhen(true)] out T? twoWayLink)
        where T : class
    {
        twoWayLink = default;

        if (TryGetTwoWayLinks(entityId, out var twoWayLinks))
        {
            twoWayLink = twoWayLinks.OfType<T>().FirstOrDefault();

            return twoWayLink != default;
        }

        return false;
    }

    public bool TryGetEntity(long entityId, [NotNullWhen(true)] out Entity? entity)
    {
        return _entities.TryGetValue(entityId, out entity);
    }
}

internal abstract record ResolvedTwoWayLink(bool IsValid, string? InvalidReason);

internal record DappAccountTypeResolvedTwoWayLink(string Value, bool IsValid, string? InvalidReason) : ResolvedTwoWayLink(IsValid, InvalidReason);

internal record DappClaimedEntityResolvedTwoWayLink(long EntityId, bool IsValid, string? InvalidReason) : ResolvedTwoWayLink(IsValid, InvalidReason);

internal record DappClaimedWebsiteResolvedTwoWayLink(Uri Origin, bool IsValid, string? InvalidReason) : ResolvedTwoWayLink(IsValid, InvalidReason);

internal record DappDefinitionResolvedTwoWayLink(long EntityId, bool IsValid, string? InvalidReason) : ResolvedTwoWayLink(IsValid, InvalidReason);

internal record DappDefinitionsResolvedTwoWayLink(long EntityId, bool IsValid, string? InvalidReason) : ResolvedTwoWayLink(IsValid, InvalidReason);

internal class TwoWayLinkResolver
{
    private readonly UnverifiedTwoWayLinks _unverifiedTwoWayLinks;

    public TwoWayLinkResolver(UnverifiedTwoWayLinks unverifiedTwoWayLinks)
    {
        _unverifiedTwoWayLinks = unverifiedTwoWayLinks;
    }

    public Dictionary<long, List<ResolvedTwoWayLink>> Resolve(ICollection<Entity> entities)
    {
        var results = new Dictionary<long, List<ResolvedTwoWayLink>>();

        foreach (var entity in entities)
        {
            if (!_unverifiedTwoWayLinks.TryGetTwoWayLinks(entity.Id, out var unverifiedTwoWayLinks))
            {
                continue;
            }

            foreach (var unverifiedTwoWayLink in unverifiedTwoWayLinks)
            {
                IEnumerable<ResolvedTwoWayLink> resolved = unverifiedTwoWayLink switch
                {
                    DappAccountTypeUnverifiedTwoWayLinkEntryHistory dappAccountType => Resolve(dappAccountType, entity),
                    DappClaimedEntitiesUnverifiedTwoWayLinkEntryHistory dappClaimedEntities => Resolve(dappClaimedEntities, entity),
                    DappClaimedWebsitesUnverifiedTwoWayLinkEntryHistory dappClaimedWebsites => Resolve(dappClaimedWebsites, entity),
                    DappDefinitionsUnverifiedTwoWayLinkEntryHistory dappDefinitions => Resolve(dappDefinitions, entity),
                    DappDefinitionUnverifiedTwoWayLinkEntryHistory dappDefinition => Resolve(dappDefinition, entity),
                    _ => throw new ArgumentOutOfRangeException(nameof(unverifiedTwoWayLink), unverifiedTwoWayLink, null),
                };

                results.GetOrAdd(entity.Id, _ => new List<ResolvedTwoWayLink>()).AddRange(resolved);
            }
        }

        return results;
    }

    private IEnumerable<DappAccountTypeResolvedTwoWayLink> Resolve(DappAccountTypeUnverifiedTwoWayLinkEntryHistory dappAccountType, Entity entity)
    {
        // TODO I don't like that!
        if (dappAccountType.IsDeleted)
        {
            yield break;
        }

        if (entity is not GlobalAccountEntity)
        {
            yield return new DappAccountTypeResolvedTwoWayLink(dappAccountType.Value, false, "entity is not of an account type");
        }
        else if (dappAccountType.Value != "dapp definition")
        {
            yield return new DappAccountTypeResolvedTwoWayLink(dappAccountType.Value, false, "expected 'dapp definition'");
        }
        else
        {
            yield return new DappAccountTypeResolvedTwoWayLink(dappAccountType.Value, true, null);
        }
    }

    private IEnumerable<DappClaimedEntityResolvedTwoWayLink> Resolve(DappClaimedEntitiesUnverifiedTwoWayLinkEntryHistory dappClaimedEntities, Entity entity)
    {
        // TODO I don't like that!
        if (dappClaimedEntities.IsDeleted || dappClaimedEntities.ClaimedEntityIds == null)
        {
            yield break;
        }

        foreach (var claimedEntityId in dappClaimedEntities.ClaimedEntityIds)
        {
            if (entity is not GlobalAccountEntity)
            {
                yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "entity is not of an account type");
            }
            else if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLinkEntryHistory>(entity.Id, out var dappAccountType) || dappAccountType.Value != "dapp definition")
            {
                yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "entity misses dappAccountType=dapp definition marker");
            }
            else if (!_unverifiedTwoWayLinks.TryGetEntity(claimedEntityId, out var otherEntity))
            {
                yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "other entity not found");
            }
            else
            {
                if (otherEntity is ResourceEntity)
                {
                    if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappDefinitionsUnverifiedTwoWayLinkEntryHistory>(otherEntity.Id, out var otherDappDefinitions))
                    {
                        yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "other entity two way link of type dapp_definitions not found");
                    }
                    else if (otherDappDefinitions.IsDeleted || otherDappDefinitions.DappDefinitionEntityIds == null)
                    {
                        // TODO I don't like that!
                        yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "broken, impossible");
                    }
                    else if (!otherDappDefinitions.DappDefinitionEntityIds.Contains(entity.Id))
                    {
                        yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "claimed entity id not found in other's entity dapp_definitions");
                    }
                    else
                    {
                        yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, true, null);
                    }
                }
                else if (otherEntity.IsGlobal)
                {
                    if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappDefinitionUnverifiedTwoWayLinkEntryHistory>(otherEntity.Id, out var otherDappDefinition))
                    {
                        yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "other entity two way link of type dapp_definition not found");
                    }
                    else if (otherDappDefinition.IsDeleted || otherDappDefinition.DappDefinitionEntityId == default)
                    {
                        // TODO I don't like that!
                        yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "broken, impossible");
                    }
                    else if (otherDappDefinition.DappDefinitionEntityId != entity.Id)
                    {
                        yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "claimed entity id not found in other's entity dapp_definition");
                    }
                    else
                    {
                        yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, true, null);
                    }
                }
                else
                {
                    yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntityId, false, "invalid other entity type: " + otherEntity.GetType().Name);
                }
            }
        }
    }

    private IEnumerable<DappClaimedWebsiteResolvedTwoWayLink> Resolve(DappClaimedWebsitesUnverifiedTwoWayLinkEntryHistory dappClaimedWebsites, Entity entity)
    {
        // TODO I don't like that!
        if (dappClaimedWebsites.IsDeleted || dappClaimedWebsites.ClaimedWebsites == null)
        {
            yield break;
        }

        foreach (var claimedWebsite in dappClaimedWebsites.ClaimedWebsites)
        {
            var url = new Uri(claimedWebsite);

            if (entity is not GlobalAccountEntity)
            {
                yield return new DappClaimedWebsiteResolvedTwoWayLink(url, false, "entity is not of an account type");
            }
            else if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLinkEntryHistory>(entity.Id, out var dappAccountType) || dappAccountType.Value != "dapp definition")
            {
                yield return new DappClaimedWebsiteResolvedTwoWayLink(url, false, "entity misses dappAccountType=dapp definition marker");
            }
            else
            {
                yield return new DappClaimedWebsiteResolvedTwoWayLink(url, false, "not supported yet");
            }
        }
    }

    private IEnumerable<DappDefinitionsResolvedTwoWayLink> Resolve(DappDefinitionsUnverifiedTwoWayLinkEntryHistory dappDefinitions, Entity entity)
    {
        // TODO I don't like that!
        if (dappDefinitions.IsDeleted || dappDefinitions.DappDefinitionEntityIds == null)
        {
            yield break;
        }

        foreach (var dappDefinitionEntityId in dappDefinitions.DappDefinitionEntityIds)
        {
            switch (entity)
            {
                case GlobalAccountEntity:
                {
                    if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLinkEntryHistory>(entity.Id, out var dappAccountType) || dappAccountType.Value != "dapp definition")
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "entity misses dappAccountType=dapp definition marked");
                    }
                    else if (!_unverifiedTwoWayLinks.TryGetEntity(dappDefinitionEntityId, out var otherEntity))
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "other entity not found");
                    }
                    else if (otherEntity is not GlobalAccountEntity)
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "other entity is not of type account");
                    }
                    else if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappDefinitionsUnverifiedTwoWayLinkEntryHistory>(otherEntity.Id, out var otherDappDefinitions))
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "other entity misses dapp_definitions");
                    }
                    else if (otherDappDefinitions.IsDeleted || otherDappDefinitions.DappDefinitionEntityIds == null)
                    {
                        // TODO I don't like that!
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "broken, impossible");
                    }
                    else if (!otherDappDefinitions.DappDefinitionEntityIds.Contains(entity.Id))
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "dapp definition not found in other's entity dapp_definitions");
                    }
                    else
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, true, null);
                    }

                    break;
                }

                case ResourceEntity:
                {
                    if (!_unverifiedTwoWayLinks.TryGetEntity(dappDefinitionEntityId, out var otherEntity))
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "other entity not found");
                    }
                    else if (otherEntity is not GlobalAccountEntity)
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "other entity is not of type account");
                    }
                    else if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLinkEntryHistory>(otherEntity.Id, out var otherDappAccountType) || otherDappAccountType.Value != "dapp definition")
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "other entity misses dappAccountType=dapp definition marker");
                    }
                    else if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappClaimedEntitiesUnverifiedTwoWayLinkEntryHistory>(otherEntity.Id, out var otherClaimedEntities))
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "other entity misses claimed_entities");
                    }
                    else if (otherClaimedEntities.IsDeleted || otherClaimedEntities.ClaimedEntityIds == null)
                    {
                        // TODO I don't like that!
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "broken, impossible");
                    }
                    else if (!otherClaimedEntities.ClaimedEntityIds.Contains(entity.Id))
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "dapp definition not found in other's entity claimed_entities");
                    }
                    else
                    {
                        yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, true, null);
                    }

                    break;
                }

                default:
                    yield return new DappDefinitionsResolvedTwoWayLink(dappDefinitionEntityId, false, "entity is not of an account / resource type");
                    break;
            }
        }
    }

    private IEnumerable<DappDefinitionResolvedTwoWayLink> Resolve(DappDefinitionUnverifiedTwoWayLinkEntryHistory dappDefinition, Entity entity)
    {
        // TODO I don't like that!
        if (dappDefinition.IsDeleted || dappDefinition.DappDefinitionEntityId == default)
        {
            yield break;
        }

        var claimedEntityId = dappDefinition.DappDefinitionEntityId;

        if (entity is ResourceEntity)
        {
            yield return new DappDefinitionResolvedTwoWayLink(claimedEntityId, false, "entity is a resource");
        }
        else if (!entity.IsGlobal)
        {
            yield return new DappDefinitionResolvedTwoWayLink(claimedEntityId, false, "entity is not of a global type");
        }
        else if (!_unverifiedTwoWayLinks.TryGetEntity(claimedEntityId, out var otherEntity))
        {
            yield return new DappDefinitionResolvedTwoWayLink(claimedEntityId, false, "other entity not found");
        }
        else if (otherEntity is not GlobalAccountEntity)
        {
            yield return new DappDefinitionResolvedTwoWayLink(claimedEntityId, false, "other entity is not of account type");
        }
        else if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLinkEntryHistory>(otherEntity.Id, out var otherDappAccountType) || otherDappAccountType.Value != "dapp definition")
        {
            yield return new DappDefinitionResolvedTwoWayLink(claimedEntityId, false, "other entity misses dappAccountType=dapp definition marker");
        }
        else if (!_unverifiedTwoWayLinks.TryGetTwoWayLink<DappClaimedEntitiesUnverifiedTwoWayLinkEntryHistory>(otherEntity.Id, out var otherClaimedEntities))
        {
            yield return new DappDefinitionResolvedTwoWayLink(claimedEntityId, false, "other entity two way link of type claimed_entities not found");
        }
        else if (otherClaimedEntities.IsDeleted || otherClaimedEntities.ClaimedEntityIds == null)
        {
            // TODO I don't like that!
            yield return new DappDefinitionResolvedTwoWayLink(claimedEntityId, false, "broken, impossible");
        }
        else if (!otherClaimedEntities.ClaimedEntityIds.Contains(entity.Id))
        {
            yield return new DappDefinitionResolvedTwoWayLink(claimedEntityId, false, "claimed entity id not found in other's entity dapp_definitions");
        }
        else
        {
            yield return new DappDefinitionResolvedTwoWayLink(claimedEntityId, true, null);
        }
    }
}
