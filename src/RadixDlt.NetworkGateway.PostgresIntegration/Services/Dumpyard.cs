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

using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

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

internal record ReferencedEntity(string Address, CoreModel.EntityType Type, long StateVersion)
{
    private Entity? _databaseEntity;
    private ReferencedEntity? _immediateParentReference;

    public string? GlobalAddress { get; private set; }

    public Type? TypeHint { get; private set; }

    public long DatabaseId => GetDatabaseEntity().Id;

    public long? DatabaseParentAncestorId => GetDatabaseEntity().ParentAncestorId;

    public long DatabaseOwnerAncestorId => GetDatabaseEntity().OwnerAncestorId ?? throw new InvalidOperationException("OwnerAncestorId not set, probably global entity or incorrectly configured one.");

    public long DatabaseGlobalAncestorId => GetDatabaseEntity().GlobalAncestorId ?? throw new InvalidOperationException("GlobalAncestorId not set, probably global entity or incorrectly configured one.");

    public bool CanBeOwner => Type is CoreModel.EntityType.Component or CoreModel.EntityType.ResourceManager or CoreModel.EntityType.KeyValueStore;

    public bool IsGlobal => GlobalAddress != null || GetDatabaseEntity().GlobalAddress != null;

    [MemberNotNullWhen(true, nameof(ImmediateParentReference))]
    public bool HasImmediateParentReference => _immediateParentReference != null;

    public ReferencedEntity ImmediateParentReference => _immediateParentReference ?? throw new InvalidOperationException("Parent not set, probably global entity or incorrectly configured one.");

    public void Globalize(string globalAddressHex)
    {
        GlobalAddress = globalAddressHex;
    }

    public void Resolve(Entity entity)
    {
        _databaseEntity = entity;

        if (entity.GlobalAddress != null)
        {
            GlobalAddress = entity.GlobalAddress.ToHex();
        }
    }

    public void IsImmediateChildOf(ReferencedEntity parent)
    {
        _immediateParentReference = parent;
    }

    public void WithTypeHint(Type type)
    {
        if (TypeHint != null && TypeHint != type)
        {
            throw new ArgumentException("Already annotated with different type hint", nameof(type));
        }

        TypeHint = type;
    }

    public TEntity CreateUsingTypeHint<TEntity>()
    {
        if (TypeHint == null)
        {
            throw new InvalidOperationException("No TypeHint specified");
        }

        if (!TypeHint.IsAssignableTo(typeof(TEntity)))
        {
            throw new InvalidOperationException("Conflicting hint type");
        }

        if (Activator.CreateInstance(TypeHint) is not TEntity instance)
        {
            throw new InvalidOperationException("Unable to create instance");
        }

        return instance;
    }

    public void ConfigureDatabaseEntity<T>(Action<T> action)
        where T : Entity
    {
        var dbEntity = GetDatabaseEntity();

        if (dbEntity is not T typedDbEntity)
        {
            throw new ArgumentException("Action argument type does not match underlying entity type.", nameof(action));
        }

        action.Invoke(typedDbEntity);
    }

    private Entity GetDatabaseEntity()
    {
        var de = _databaseEntity ?? throw new InvalidOperationException("Database entity not loaded yet.");

        if (de.Id == 0)
        {
            throw new InvalidOperationException("Database entity not ready yet.");
        }

        return de;
    }
}

internal record FungibleVaultChange(ReferencedEntity ReferencedVault, ReferencedEntity ReferencedResource, TokenAmount Balance, long StateVersion);

internal record NonFungibleVaultChange(ReferencedEntity ReferencedVault, ReferencedEntity ReferencedResource, List<byte[]> NonFungibleIds, long StateVersion);

internal record NonFungibleIdChange(ReferencedEntity ReferencedStore, byte[] NonFungibleId, bool IsDeleted, CoreModel.NonFungibleData Data, long StateVersion);

internal record MetadataChange(ReferencedEntity ResourceEntity, Dictionary<string, string> Metadata, long StateVersion);

internal record FungibleResourceSupply(ReferencedEntity ResourceEntity, TokenAmount TotalSupply, TokenAmount TotalMinted, TokenAmount TotalBurnt, long StateVersion);

internal record AggregateChange
{
    public long StateVersion { get; }

    public List<long> FungibleIds { get; } = new();

    public List<long> NonFungibleIds { get; } = new();

    public bool Persistable { get; }

    public AggregateChange(long stateVersion)
        : this(stateVersion, Array.Empty<long>(), Array.Empty<long>())
    {
        Persistable = true;
    }

    public AggregateChange(long stateVersion, long[] fungibleIds, long[] nonFungibleIds)
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

    public void Apply(AggregateChange? previous)
    {
        if (previous == null)
        {
            return;
        }

        var finalFungibleIds = new List<long>(previous.FungibleIds);
        var finalNonFungibleIds = new List<long>(previous.NonFungibleIds);

        foreach (var id in FungibleIds.Where(id => !finalFungibleIds.Contains(id)))
        {
            finalFungibleIds.Add(id);
        }

        foreach (var id in NonFungibleIds.Where(id => !finalNonFungibleIds.Contains(id)))
        {
            finalNonFungibleIds.Add(id);
        }

        // TODO add support for NonFungibleIds removal (separate collections + foreach + finalNonFungibleIds.Remove(id)

        FungibleIds.Clear();
        FungibleIds.AddRange(finalFungibleIds);
        NonFungibleIds.Clear();
        NonFungibleIds.AddRange(finalNonFungibleIds);
    }

    public bool ShouldBePersisted(AggregateChange? previous)
    {
        if (!Persistable)
        {
            return false;
        }

        if (previous == null)
        {
            return true;
        }

        if (FungibleIds.SequenceEqual(previous.FungibleIds) && NonFungibleIds.SequenceEqual(previous.NonFungibleIds))
        {
            return false;
        }

        return true;
    }
}

internal class ReferencedEntityDictionary
{
    private readonly Dictionary<string, ReferencedEntity> _storage = new();
    private readonly Dictionary<long, List<ReferencedEntity>> _inversed = new();
    private readonly Dictionary<string, ReferencedEntity> _globalsCache = new();
    private readonly Dictionary<long, ReferencedEntity> _dbIdCache = new();

    public ICollection<string> Addresses => _storage.Keys;

    public ICollection<ReferencedEntity> All => _storage.Values;

    public ReferencedEntity GetOrAdd(string addressHex, Func<string, ReferencedEntity> factory)
    {
        if (_storage.ContainsKey(addressHex))
        {
            return _storage[addressHex];
        }

        var value = factory(addressHex);

        if (!_inversed.ContainsKey(value.StateVersion))
        {
            _inversed[value.StateVersion] = new List<ReferencedEntity>();
        }

        _storage[addressHex] = value;
        _inversed[value.StateVersion].Add(value);

        return value;
    }

    public ReferencedEntity Get(string addressHex)
    {
        return _storage[addressHex];
    }

    public ReferencedEntity GetByGlobal(string globalAddressHex)
    {
        return _globalsCache.GetOrAdd(globalAddressHex, _ => _storage.Values.First(re => re.GlobalAddress == globalAddressHex));
    }

    public ReferencedEntity GetByDatabaseId(long id)
    {
        return _dbIdCache.GetOrAdd(id, _ => _storage.Values.First(re => re.DatabaseId == id));
    }

    public IEnumerable<ReferencedEntity> OfStateVersion(long stateVersion)
    {
        if (_inversed.ContainsKey(stateVersion))
        {
            return _inversed[stateVersion];
        }

        return Array.Empty<ReferencedEntity>();
    }
}

internal class SequencesHolder
{
    public long EntitySequence { get; set; }

    public long EntityMetadataHistorySequence { get; set; }

    public long EntityResourceAggregateHistorySequence { get; set; }

    public long EntityResourceHistorySequence { get; set; }

    public long FungibleResourceSupplyHistorySequence { get; set; }

    public long NonFungibleIdDataSequence { get; set; }

    public long NonFungibleIdMutableDataHistorySequence { get; set; }

    public long NextEntity => EntitySequence++;

    public long NextEntityMetadataHistory => EntityMetadataHistorySequence++;

    public long NextEntityResourceAggregateHistory => EntityResourceAggregateHistorySequence++;

    public long NextEntityResourceHistory => EntityResourceHistorySequence++;

    public long NextFungibleResourceSupplyHistory => FungibleResourceSupplyHistorySequence++;

    public long NextNonFungibleIdData => NonFungibleIdDataSequence++;

    public long NextNonFungibleIdMutableDataHistory => NonFungibleIdMutableDataHistorySequence++;
}
