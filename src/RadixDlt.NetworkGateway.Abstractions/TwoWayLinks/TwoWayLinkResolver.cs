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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public Dictionary<EntityAddress, List<ResolvedTwoWayLink>> Resolve(ICollection<EntityAddress> entityAddresses)
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
                    DappClaimedWebsitesUnverifiedTwoWayLink dappClaimedWebsites => Resolve(entityAddress, dappClaimedWebsites),
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
        var isValid = ValidateDappAccountType(entityAddress, dappAccountType.Value, out var invalidReason);

        if (isValid || !_resolveValidOnly)
        {
            yield return new DappAccountTypeResolvedTwoWayLink(dappAccountType.Value, invalidReason);
        }
    }

    private IEnumerable<DappClaimedEntityResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappClaimedEntitiesUnverifiedTwoWayLink dappClaimedEntities)
    {
        foreach (var claimedEntity in dappClaimedEntities.ClaimedEntities)
        {
            var isValid = ValidateDappClaimedEntitiesEntry(entityAddress, claimedEntity, out var invalidReason);

            if (isValid || !_resolveValidOnly)
            {
                yield return new DappClaimedEntityResolvedTwoWayLink(claimedEntity, invalidReason);
            }
        }
    }

    private IEnumerable<DappClaimedWebsiteResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappClaimedWebsitesUnverifiedTwoWayLink dappClaimedWebsites)
    {
        foreach (var claimedWebsite in dappClaimedWebsites.ClaimedWebsites)
        {
            var isValid = ValidateDappClaimedWebsitesEntry(entityAddress, claimedWebsite, out var invalidReason);

            if (isValid || !_resolveValidOnly)
            {
                yield return new DappClaimedWebsiteResolvedTwoWayLink(claimedWebsite, invalidReason);
            }
        }
    }

    private IEnumerable<DappDefinitionsResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappDefinitionsUnverifiedTwoWayLink dappDefinitions)
    {
        foreach (var dappDefinition in dappDefinitions.DappDefinitions)
        {
            var isValid = ValidateDappDefinitionsEntry(entityAddress, dappDefinition, out var invalidReason);

            if (isValid || !_resolveValidOnly)
            {
                yield return new DappDefinitionsResolvedTwoWayLink(dappDefinition, invalidReason);
            }
        }
    }

    private IEnumerable<DappDefinitionResolvedTwoWayLink> Resolve(EntityAddress entityAddress, DappDefinitionUnverifiedTwoWayLink dappDefinition)
    {
        var isValid = ValidateDappDefinition(entityAddress, dappDefinition.DappDefinition, out var invalidReason);

        if (isValid || !_resolveValidOnly)
        {
            yield return new DappDefinitionResolvedTwoWayLink(dappDefinition.DappDefinition, invalidReason);
        }
    }

    private bool ValidateDappAccountType(EntityAddress entityAddress, string dappAccountType, [NotNullWhen(false)] out string? error)
    {
        error = default;

        if (!entityAddress.IsAccount)
        {
            error = "entity is not of an account type";
        }
        else if (dappAccountType != "dapp definition")
        {
            error = "expected 'dapp definition'";
        }

        return error == default;
    }

    private bool ValidateDappClaimedEntitiesEntry(EntityAddress entityAddress, EntityAddress otherEntityAddress, [NotNullWhen(false)] out string? error)
    {
        error = default;

        if (!entityAddress.IsAccount)
        {
            error = "entity is not of an account type";
        }
        else if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(entityAddress, out var dappAccountType) || dappAccountType.Value != "dapp definition")
        {
            error = "entity misses dappAccountType=dapp definition marker";
        }
        else
        {
            if (otherEntityAddress.IsResource)
            {
                if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappDefinitionsUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappDefinitions))
                {
                    error = "other entity two way link of type dapp_definitions not found";
                }
                else if (!otherDappDefinitions.DappDefinitions.Contains(entityAddress))
                {
                    error = "claimed entity id not found in other's entity dapp_definitions";
                }
            }
            else if (otherEntityAddress.IsGlobal)
            {
                if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappDefinitionUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappDefinition))
                {
                    error = "other entity two way link of type dapp_definition not found";
                }
                else if (otherDappDefinition.DappDefinition != entityAddress)
                {
                    error = "claimed entity id not found in other's entity dapp_definition";
                }
            }
            else
            {
                error = "invalid other entity type";
            }
        }

        return error == default;
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

    private bool ValidateDappClaimedWebsitesEntry(EntityAddress entityAddress, Uri claimedWebsite, [NotNullWhen(false)] out string? error)
    {
        error = default;

        if (!entityAddress.IsAccount)
        {
            error = "entity is not of an account type";
        }
        else if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(entityAddress, out var dappAccountType) || dappAccountType.Value != "dapp definition")
        {
            error = "entity misses dappAccountType=dapp definition marker";
        }
        else if (_validateOnLedgerOnly)
        {
            error = "off-ledger validation disabled";
        }
        else if (!Uri.TryCreate(claimedWebsite, "/.well-known/radix.json", out var url))
        {
            error = "unable to construct validation url";
        }
        else
        {
            // TODO super naive implementation!!! do NOT use on production!!!
            // TODO use async

            try
            {
                using var hc = new HttpClient();
                using var response = hc.GetAsync(url).GetAwaiter().GetResult();

                response.EnsureSuccessStatusCode();
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var wellKnown = JsonConvert.DeserializeObject<WellKnownRadix>(json);

                if (wellKnown == null || wellKnown.Dapps == null)
                {
                    error = "invalid http response";
                }
                else if (wellKnown.Dapps.All(dapp => dapp.DappDefinitionAddress != entityAddress))
                {
                    error = "entity not found in JSON file";
                }
            }
            catch (Exception ex)
            {
                error = "general error: " + ex.Message;

                // todo log etc
            }
        }

        return error == default;
    }

    private bool ValidateDappDefinitionsEntry(EntityAddress entityAddress, EntityAddress otherEntityAddress, [NotNullWhen(false)] out string? error)
    {
        error = default;

        if (entityAddress.IsAccount)
        {
            if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(entityAddress, out var dappAccountType) || dappAccountType.Value != "dapp definition")
            {
                error = "entity misses dappAccountType=dapp definition marked";
            }
            else if (!otherEntityAddress.IsAccount)
            {
                error = "other entity is not of type account";
            }
            else if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappDefinitionsUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappDefinitions))
            {
                error = "other entity misses dapp_definitions";
            }
            else if (!otherDappDefinitions.DappDefinitions.Contains(entityAddress))
            {
                error = "dapp definition not found in other's entity dapp_definitions";
            }
        }
        else if (entityAddress.IsResource)
        {
            if (!otherEntityAddress.IsAccount)
            {
                error = "other entity is not of type account";
            }
            else if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappAccountType) || otherDappAccountType.Value != "dapp definition")
            {
                error = "other entity misses dappAccountType=dapp definition marker";
            }
            else if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappClaimedEntitiesUnverifiedTwoWayLink>(otherEntityAddress, out var otherClaimedEntities))
            {
                error = "other entity misses claimed_entities";
            }
            else if (!otherClaimedEntities.ClaimedEntities.Contains(entityAddress))
            {
                error = "dapp definition not found in other's entity claimed_entities";
            }
        }
        else
        {
            error = "entity is not of an account / resource type";
        }

        return error == default;
    }

    private bool ValidateDappDefinition(EntityAddress entityAddress, EntityAddress otherEntityAddress, [NotNullWhen(false)] out string? error)
    {
        error = default;

        if (!entityAddress.IsResource)
        {
            error = "entity is a resource";
        }
        else if (!entityAddress.IsGlobal)
        {
            error = "entity is not of a global type";
        }
        else if (!otherEntityAddress.IsAccount)
        {
            error = "other entity is not of account type";
        }
        else if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappAccountTypeUnverifiedTwoWayLink>(otherEntityAddress, out var otherDappAccountType) || otherDappAccountType.Value != "dapp definition")
        {
            error = "other entity misses dappAccountType=dapp definition marker";
        }
        else if (!_unverifiedTwoWayLinksCollection.TryGetTwoWayLink<DappClaimedEntitiesUnverifiedTwoWayLink>(otherEntityAddress, out var otherClaimedEntities))
        {
            error = "other entity two way link of type claimed_entities not found";
        }
        else if (!otherClaimedEntities.ClaimedEntities.Contains(entityAddress))
        {
            error = "claimed entity id not found in other's entity dapp_definitions";
        }

        return error == default;
    }
}
