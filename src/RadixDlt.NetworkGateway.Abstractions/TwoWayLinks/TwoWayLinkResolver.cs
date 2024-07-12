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

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.Abstractions.TwoWayLinks;

public sealed class TwoWayLinkResolver
{
    private readonly IUnverifiedTwoWayLinksCollection _unverifiedTwoWayLinksCollection;
    private readonly bool _resolveValidOnly;
    private readonly bool _validateOnLedgerOnly;

    public TwoWayLinkResolver(IUnverifiedTwoWayLinksCollection unverifiedTwoWayLinksCollection, bool resolveValidOnly, bool validateOnLedgerOnly)
    {
        _unverifiedTwoWayLinksCollection = unverifiedTwoWayLinksCollection;
        _resolveValidOnly = resolveValidOnly;
        _validateOnLedgerOnly = validateOnLedgerOnly;
    }

    public async ValueTask<Dictionary<EntityAddress, List<ResolvedTwoWayLink>>> Resolve(ICollection<EntityAddress> entityAddresses)
    {
        var results = new Dictionary<EntityAddress, List<ResolvedTwoWayLink>>();

        foreach (var entityAddress in entityAddresses)
        {
            if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLinks(entityAddress, out var unverifiedTwoWayLinks))
            {
                continue;
            }

            foreach (var unverifiedTwoWayLink in unverifiedTwoWayLinks)
            {
                IEnumerable<ResolvedTwoWayLink> resolved = unverifiedTwoWayLink switch
                {
                    DappAccountTypeUnverifiedTwoWayLink dappAccountType => Resolve(entityAddress, dappAccountType),
                    DappClaimedEntitiesUnverifiedTwoWayLink dappClaimedEntities => Resolve(entityAddress, dappClaimedEntities),
                    DappClaimedWebsitesUnverifiedTwoWayLink dappClaimedWebsites => await Resolve(entityAddress, dappClaimedWebsites),
                    DappDefinitionsUnverifiedTwoWayLink dappDefinitions => Resolve(entityAddress, dappDefinitions),
                    DappDefinitionUnverifiedTwoWayLink dappDefinition => Resolve(entityAddress, dappDefinition),
                    _ => throw new ArgumentOutOfRangeException(nameof(unverifiedTwoWayLink), unverifiedTwoWayLink, null),
                };

                var list = results.TryGetValue(entityAddress, out var existing) ? existing : new List<ResolvedTwoWayLink>();

                list.AddRange(resolved);

                results[entityAddress] = list;
            }
        }

        return results;
    }

    private IEnumerable<DappAccountTypeResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappAccountTypeUnverifiedTwoWayLink dappAccountType)
    {
        var invalidReason = ValidateDappAccountType(entityAddress, dappAccountType.Value);

        if (invalidReason != null || !_resolveValidOnly)
        {
            yield return new DappAccountTypeResolvedTwoWayLink(dappAccountType.Value, invalidReason);
        }
    }

    private IEnumerable<DappClaimedEntityResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappClaimedEntitiesUnverifiedTwoWayLink dappClaimedEntities)
    {
        foreach (var claimedEntity in dappClaimedEntities.ClaimedEntities)
        {
            var invalidReason = ValidateDappClaimedEntitiesEntry(entityAddress, claimedEntity);

            if (invalidReason != null || !_resolveValidOnly)
            {
                yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntity, invalidReason);
            }
        }
    }

    private async ValueTask<IEnumerable<DappClaimedWebsiteResolvedTwoWayLink>> Resolve(EntityAddress entityAddress, DappClaimedWebsitesUnverifiedTwoWayLink dappClaimedWebsites)
    {
        var result = new ConcurrentQueue<DappClaimedWebsiteResolvedTwoWayLink>();

        await Parallel.ForEachAsync(dappClaimedWebsites.ClaimedWebsites, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (claimedWebsite, _) =>
        {
            var invalidReason = await ValidateDappClaimedWebsitesEntry(entityAddress, claimedWebsite);

            if (invalidReason != null || !_resolveValidOnly)
            {
                result.Enqueue(new DappClaimedWebsiteResolvedTwoWayLink(claimedWebsite, invalidReason));
            }
        });

        return result;
    }

    private IEnumerable<DappDefinitionsResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappDefinitionsUnverifiedTwoWayLink dappDefinitions)
    {
        foreach (var dappDefinition in dappDefinitions.DappDefinitions)
        {
            var invalidReason = ValidateDappDefinitionsEntry(entityAddress, dappDefinition);

            if (invalidReason != null || !_resolveValidOnly)
            {
                yield return new DappDefinitionsResolvedTwoWayLink(dappDefinition, invalidReason);
            }
        }
    }

    private IEnumerable<DappDefinitionResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappDefinitionUnverifiedTwoWayLink dappDefinition)
    {
        var invalidReason = ValidateDappDefinition(entityAddress, dappDefinition.DappDefinition);

        if (invalidReason != null || !_resolveValidOnly)
        {
            yield return new DappDefinitionResolvedTwoWayLink(dappDefinition.DappDefinition, invalidReason);
        }
    }

    private string? ValidateDappAccountType(EntityAddress entityAddress, string dappAccountType)
    {
        if (!entityAddress.IsAccount)
        {
            return "entity is not of an account type"; // a) entity + NOT_ACCOUNT
        }

        if (dappAccountType != "dapp definition")
        {
            return "expected 'dapp definition'"; // b) entity INVALID_ACC_TYPE
        }

        return null;
    }

    private string? ValidateDappClaimedEntitiesEntry(EntityAddress entityAddress, EntityAddress otherEntityAddress)
    {
        if (!entityAddress.IsAccount)
        {
            return "entity is not of an account type"; // a) entity + NOT_ACCOUNT
        }

        if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(entityAddress, out var dappAccountType) || dappAccountType.Value != "dapp definition")
        {
            return "entity misses dappAccountType=dapp definition marker"; // c) entity + MISSING_ACC_TYPE_DAPP_DEF
        }

        if (otherEntityAddress.IsResource)
        {
            if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappDefinitionsUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappDefinitions))
            {
                return "other entity two way link of type dapp_definitions not found"; // d) other entity + MISSING_DAPP_DEFS
            }

            if (!otherDappDefinitions.DappDefinitions.Contains(entityAddress))
            {
                return "claimed entity not found in other's entity dapp_definitions"; // e) entity + other entity +
            }
        }
        else if (otherEntityAddress.IsGlobal)
        {
            if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappDefinitionUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappDefinition))
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

    private class WellKnownRadix
    {
        public class DappEntry
        {
            [JsonProperty("dAppDefinitionAddress")]
            public EntityAddress DappDefinitionAddress { get; set; }
        }

        [JsonProperty("dApps")]
        public ICollection<DappEntry>? Dapps { get; set; }
    }

    private async Task<string?> ValidateDappClaimedWebsitesEntry(EntityAddress entityAddress, Uri claimedWebsite)
    {
        if (!entityAddress.IsAccount)
        {
            return "entity is not of an account type";
        }

        if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(entityAddress, out var dappAccountType) || dappAccountType.Value != "dapp definition")
        {
            return "entity misses dappAccountType=dapp definition marker";
        }

        if (_validateOnLedgerOnly)
        {
            return "off-ledger validation disabled";
        }

        if (!Uri.TryCreate(claimedWebsite, "/.well-known/radix.json", out var url))
        {
            return "unable to construct validation url";
        }

        // TODO super naive implementation!!! do NOT use on production!!!
        // TODO external "resolver" service in this case

        try
        {
            using var hc = new HttpClient();
            using var response = await hc.GetAsync(url);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var wellKnown = JsonConvert.DeserializeObject<WellKnownRadix>(json);

            if (wellKnown == null || wellKnown.Dapps == null)
            {
                return "invalid http response";
            }

            if (wellKnown.Dapps.All(dapp => dapp.DappDefinitionAddress != entityAddress))
            {
                return "entity not found in JSON file";
            }
        }
        catch (Exception ex)
        {
            return "general error: " + ex.Message;

            // todo log etc
        }

        return null;
    }

    private string? ValidateDappDefinitionsEntry(EntityAddress entityAddress, EntityAddress otherEntityAddress)
    {
        if (entityAddress.IsAccount)
        {
            if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(entityAddress, out var dappAccountType) || dappAccountType.Value != "dapp definition")
            {
                return "entity misses dappAccountType=dapp definition marked";
            }

            if (!otherEntityAddress.IsAccount)
            {
                return "other entity is not of type account";
            }

            if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappDefinitionsUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappDefinitions))
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

            if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappAccountType) || otherDappAccountType.Value != "dapp definition")
            {
                return "other entity misses dappAccountType=dapp definition marker";
            }

            if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappClaimedEntitiesUnverifiedTwoWayLink>(otherEntityAddress, out var otherClaimedEntities))
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
        if (!entityAddress.IsResource)
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

        if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappAccountType) || otherDappAccountType.Value != "dapp definition")
        {
            return "other entity misses dappAccountType=dapp definition marker";
        }

        if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappClaimedEntitiesUnverifiedTwoWayLink>(otherEntityAddress, out var otherClaimedEntities))
        {
            return "other entity two way link of type claimed_entities not found";
        }

        if (!otherClaimedEntities.ClaimedEntities.Contains(entityAddress))
        {
            return "claimed entity id not found in other's entity dapp_definitions";
        }

        return null;
    }
}
