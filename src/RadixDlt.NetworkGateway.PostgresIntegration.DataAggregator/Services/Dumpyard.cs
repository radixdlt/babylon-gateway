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

using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons;
using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.Commons.Extensions;
using RadixDlt.NetworkGateway.Commons.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal record ReferencedEntity(string Address, EntityType Type, long StateVersion)
{
    private Entity? _databaseEntity;
    private ReferencedEntity? _parent;

    public RadixAddress? GlobalAddress { get; private set; }

    public long DatabaseId => GetDatabaseEntity().Id;

    public long DatabaseOwnerAncestorId => GetDatabaseEntity().OwnerAncestorId ?? throw new Exception("impossible bla bla bla");

    public long DatabaseGlobalAncestorId => GetDatabaseEntity().GlobalAncestorId ?? throw new Exception("impossible bla bla bla");

    public string? ComponentKind { get; private set; }

    // TODO not sure if this logic is valid?
    public bool IsOwner => Type is EntityType.Component or EntityType.ResourceManager;

    [MemberNotNullWhen(true, nameof(Parent))]
    public bool HasParent => _parent != null;

    public ReferencedEntity Parent => _parent ?? throw new InvalidOperationException("bla bla bal bla x8");

    public void Globalize(string addressHex, string address, AddressHrps networkHrps)
    {
        // TODO we probably want to differentiate all possible HRPs

        var kind = "normal";

        if (address.StartsWith(networkHrps.AccountHrp))
        {
            kind = "account";
        }
        else if (address.StartsWith(networkHrps.ValidatorHrp))
        {
            kind = "validator";
        }

        GlobalAddress = addressHex.ConvertFromHex();
        ComponentKind = kind;
    }

    public void Resolve(Entity entity)
    {
        _databaseEntity = entity;
    }

    public void ResolveParentalIds(long parentId, long ownerId, long globalId)
    {
        GetDatabaseEntity().ParentId = parentId;
        GetDatabaseEntity().OwnerAncestorId = ownerId;
        GetDatabaseEntity().GlobalAncestorId = globalId;
    }

    public void IsChildOf(ReferencedEntity parent)
    {
        _parent = parent;
    }

    private Entity GetDatabaseEntity()
    {
        var de = _databaseEntity ?? throw new Exception("bla bla"); // TODO fix me

        if (de.Id == 0)
        {
            throw new Exception("bla bla bla bla x6"); // TODO fix me
        }

        return de;
    }
}

internal record UppedSubstate(ReferencedEntity ReferencedEntity, string Key, SubstateType Type, long Version, byte[] DataHash, long StateVersion, CoreApiSdk.Model.Substate Data);

internal record FungibleResourceChange(ReferencedEntity SubstateEntity, ReferencedEntity ResourceEntity, TokenAmount Balance, long StateVersion);

internal record NonFungibleResourceChange(ReferencedEntity SubstateEntity, ReferencedEntity ResourceEntity, List<string> Ids, long StateVersion);

internal record MetadataChange(ReferencedEntity ResourceEntity, Dictionary<string, string> Metadata, long StateVersion);

internal record FungibleResourceSupply(ReferencedEntity ResourceEntity, TokenAmount TotalSupply, TokenAmount TotalMinted, TokenAmount TotalBurnt, long StateVersion);

internal record AggregateChange
{
    public long StateVersion { get; }

    public List<long> FungibleIds { get; } = new();

    public List<long> NonFungibleIds { get; } = new();

    public List<long> RemovedNonFungibleIds { get; } = new();

    public bool Persistable { get; }

    public AggregateChange(long stateVersion)
        : this(stateVersion, Array.Empty<long>(), Array.Empty<long>())
    {
        Persistable = true;
    }

    public AggregateChange(long stateVersion, ICollection<long> fungibleIds, ICollection<long> nonFungibleIds)
    {
        StateVersion = stateVersion;
        FungibleIds = new List<long>(fungibleIds);
        NonFungibleIds = new List<long>(nonFungibleIds);
    }

    public void AppendFungible(long id)
    {
        if (!FungibleIds.Contains(id))
        {
            FungibleIds.Add(id);
        }
    }

    public void AppendNonFungible(long id)
    {
        if (!NonFungibleIds.Contains(id))
        {
            NonFungibleIds.Add(id);
        }
    }

    public void RemoveNonFungible(long id)
    {
        if (!RemovedNonFungibleIds.Contains(id))
        {
            RemovedNonFungibleIds.Add(id);
        }
    }

    public void Merge(AggregateChange other)
    {
        foreach (var id in other.FungibleIds)
        {
            AppendFungible(id);
        }

        foreach (var id in other.NonFungibleIds)
        {
            AppendNonFungible(id);
        }

        foreach (var id in other.RemovedNonFungibleIds)
        {
            RemoveNonFungible(id);
        }
    }

    public void Resolve()
    {
        foreach (var id in RemovedNonFungibleIds)
        {
            NonFungibleIds.Remove(id);
        }
    }
}

internal static class DictionaryExtensions
{
    public static TVal GetOrAdd<TKey, TVal>(this IDictionary<TKey, TVal> dictionary, TKey key, Func<TKey, TVal> factory)
        where TKey : notnull
    {
        if (dictionary.ContainsKey(key))
        {
            return dictionary[key];
        }

        var value = factory(key);

        dictionary[key] = value;

        return value;
    }
}

internal class SequencesHolder
{
    public long EntitySequence { get; set; }

    public long EntityMetadataHistorySequence { get; set; }

    public long EntityResourceAggregateHistorySequence { get; set; }

    public long EntityResourceHistorySequence { get; set; }

    public long FungibleResourceSupplyHistorySequence { get; set; }

    public long NextEntity => EntitySequence++;

    public long NextEntityMetadataHistory => EntityMetadataHistorySequence++;

    public long NextEntityResourceAggregateHistory => EntityResourceAggregateHistorySequence++;

    public long NextEntityResourceHistory => EntityResourceHistorySequence++;

    public long NextFungibleResourceSupplyHistory => FungibleResourceSupplyHistorySequence++;
}
