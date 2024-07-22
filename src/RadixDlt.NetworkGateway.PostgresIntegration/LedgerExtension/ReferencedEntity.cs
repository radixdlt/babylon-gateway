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
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record ReferencedEntity(EntityAddress Address, CoreModel.EntityType Type, long StateVersion)
{
    private enum PostResolvePriority
    {
        High,
        Normal,
        Low,
    }

    private readonly IList<Action> _postResolveActionsHighPriority = new List<Action>();
    private readonly IList<Action> _postResolveActionsNormalPriority = new List<Action>();
    private readonly IList<Action> _postResolveActionsLowPriority = new List<Action>();
    private Entity? _databaseEntity;
    private ReferencedEntity? _immediateParentReference;
    private bool _resolved;
    private bool _postResolveConfigurationInvoked;

    public Type? TypeHint { get; private set; }

    public bool IsGlobal => Address.IsGlobal;

    public long DatabaseId => GetDatabaseEntityInternal().Id;

    public long? DatabaseParentAncestorId => GetDatabaseEntityInternal().ParentAncestorId;

    public long DatabaseOwnerAncestorId => GetDatabaseEntityInternal().OwnerAncestorId ?? throw new InvalidOperationException("OwnerAncestorId not set, probably global entity or incorrectly configured one.");

    public long DatabaseGlobalAncestorId => GetDatabaseEntityInternal().GlobalAncestorId ?? throw new InvalidOperationException("GlobalAncestorId not set, probably global entity or incorrectly configured one.");

    public long AffectedGlobalEntityId => IsGlobal ? DatabaseId : DatabaseGlobalAncestorId;

    public long GlobalEventEmitterEntityId => IsGlobal ? DatabaseId : DatabaseGlobalAncestorId;

    public bool CanBeOwnerAncestor => Type is not CoreModel.EntityType.InternalKeyValueStore;

    [MemberNotNullWhen(true, nameof(ImmediateParentReference))]
    public bool HasImmediateParentReference => _immediateParentReference != null;

    public ReferencedEntity ImmediateParentReference => _immediateParentReference ?? throw new InvalidOperationException("Parent not set, probably global entity or incorrectly configured one.");

    public void Resolve(Entity entity)
    {
        _databaseEntity = entity;
        _resolved = true;
    }

    public void IsImmediateChildOf(ReferencedEntity parent)
    {
        _immediateParentReference = parent;
    }

    public void WithTypeHint(Type type)
    {
        if (TypeHint != null && TypeHint != type)
        {
            throw new ArgumentException($"{this} already annotated with different type hint", nameof(type));
        }

        TypeHint = type;
    }

    public TEntity CreateUsingTypeHintOrDefault<TEntity>(Type defaultType)
    {
        TypeHint ??= defaultType;

        return CreateUsingTypeHint<TEntity>();
    }

    public TEntity CreateUsingTypeHint<TEntity>()
    {
        if (TypeHint == null)
        {
            throw new InvalidOperationException($"No TypeHint specified for: {this}");
        }

        if (!TypeHint.IsAssignableTo(typeof(TEntity)))
        {
            throw new InvalidOperationException($"Conflicting hint type for: {this}");
        }

        if (Activator.CreateInstance(TypeHint) is not TEntity instance)
        {
            throw new InvalidOperationException($"Unable to create instance for: {this}");
        }

        return instance;
    }

    public void PostResolveConfigureHigh<T>(Action<T> action) => PostResolveConfigure(action, PostResolvePriority.High);

    public void PostResolveConfigure<T>(Action<T> action) => PostResolveConfigure(action, PostResolvePriority.Normal);

    public void PostResolveConfigureLow<T>(Action<T> action) => PostResolveConfigure(action, PostResolvePriority.Low);

    public void InvokePostResolveConfiguration()
    {
        if (!_resolved)
        {
            throw new InvalidOperationException("Not resolved yet");
        }

        if (_postResolveConfigurationInvoked)
        {
            throw new InvalidOperationException("Already configured");
        }

        foreach (var action in _postResolveActionsHighPriority)
        {
            action.Invoke();
        }

        foreach (var action in _postResolveActionsNormalPriority)
        {
            action.Invoke();
        }

        foreach (var action in _postResolveActionsLowPriority)
        {
            action.Invoke();
        }

        _postResolveActionsHighPriority.Clear();
        _postResolveActionsNormalPriority.Clear();
        _postResolveActionsLowPriority.Clear();
        _postResolveConfigurationInvoked = true;
    }

    public T GetDatabaseEntity<T>()
    {
        var dbEntity = GetDatabaseEntityInternal();

        if (dbEntity is not T typedDbEntity)
        {
            throw new ArgumentException("Action argument type does not match underlying entity type.");
        }

        return typedDbEntity;
    }

    /// <summary>
    /// Define custom ToString to avoid infinitely recursive ToString calls, and calls to the database, etc.
    /// </summary>
    /// <returns>The string representation of the type for debugging.</returns>
    public override string ToString()
    {
        return $"{nameof(ReferencedEntity)} {{ {nameof(Address)}: {Address}, {nameof(IsGlobal)}: {IsGlobal}, {nameof(Type)}: {Type}, {nameof(StateVersion)}: {StateVersion}, {nameof(_databaseEntity)}: {_databaseEntity?.ToString() ?? "null"} }}";
    }

    private Entity GetDatabaseEntityInternal()
    {
        var de = _databaseEntity ?? throw new InvalidOperationException($"Database entity not loaded yet for {this}.");

        if (de.Id == 0)
        {
            throw new InvalidOperationException($"Database entity not ready yet for {this}.");
        }

        return de;
    }

    private void PostResolveConfigure<T>(Action<T> action, PostResolvePriority priority)
    {
        var list = priority switch
        {
            PostResolvePriority.High => _postResolveActionsHighPriority,
            PostResolvePriority.Normal => _postResolveActionsNormalPriority,
            PostResolvePriority.Low => _postResolveActionsLowPriority,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
        };

        list.Add(() =>
        {
            action.Invoke(GetDatabaseEntity<T>());
        });
    }
}
