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
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal interface IEntityQuerier
{
    Task<TEntity> GetEntity<TEntity>(EntityAddress address, GatewayApiSdk.Model.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity;

    Task<TEntity> GetNonPreAllocatedEntity<TEntity>(EntityAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity;

    Task<IDictionary<EntityAddress, long>> ResolveEntityIds(List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token);

    Task<IDictionary<long, EntityAddress>> ResolveEntityAddresses(List<long> entityIds, CancellationToken token = default);

    Task<ICollection<Entity>> GetEntities(List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token);
}

internal class EntityQuerier : IEntityQuerier
{
    private readonly IPreAllocatedEntityDataProvider _preAllocatedEntityDataProvider;
    private readonly ReadOnlyDbContext _dbContext;

    public EntityQuerier(IPreAllocatedEntityDataProvider preAllocatedEntityDataProvider, ReadOnlyDbContext dbContext)
    {
        _preAllocatedEntityDataProvider = preAllocatedEntityDataProvider;
        _dbContext = dbContext;
    }

    public async Task<TEntity> GetEntity<TEntity>(EntityAddress address, GatewayApiSdk.Model.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity
    {
        var entity = await _dbContext
            .Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .AnnotateMetricName()
            .FirstOrDefaultAsync(e => e.Address == address, token);

        if (entity == null)
        {
            entity = TryResolveAsPreAllocatedEntity(address);

            if (entity == null)
            {
                // TODO this method should return null/throw on missing, virtual component handling should be done upstream to avoid entity.Id = 0 uses, see https://github.com/radixdlt/babylon-gateway/pull/171#discussion_r1111957627
                throw new EntityNotFoundException(address.ToString());
            }
        }

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidEntityException(address.ToString());
        }

        return typedEntity;
    }

    public async Task<TEntity> GetNonPreAllocatedEntity<TEntity>(EntityAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity
    {
        var entity = await _dbContext
            .Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion)
            .AnnotateMetricName()
            .FirstOrDefaultAsync(e => e.Address == address, token);

        if (entity == null)
        {
            throw new EntityNotFoundException(address.ToString());
        }

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidEntityException(address.ToString());
        }

        return typedEntity;
    }

    public async Task<IDictionary<EntityAddress, long>> ResolveEntityIds(List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var entities = await _dbContext
            .Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion && addresses.Contains(e.Address))
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Address, e => e.Id, token);

        return entities;
    }

    public async Task<IDictionary<long, EntityAddress>> ResolveEntityAddresses(List<long> entityIds, CancellationToken token = default)
    {
        if (!entityIds.Any())
        {
            return ImmutableDictionary<long, EntityAddress>.Empty;
        }

        return await _dbContext
            .Entities
            .Where(e => entityIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Address })
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Id, e => e.Address, token);
    }

    public async Task<ICollection<Entity>> GetEntities(List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var entities = await _dbContext
            .Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion && addresses.Contains(e.Address))
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Address, token);

        foreach (var address in addresses.Except(entities.Keys))
        {
            var preAllocatedEntity = TryResolveAsPreAllocatedEntity(address);

            if (preAllocatedEntity != null)
            {
                entities.Add(preAllocatedEntity.Address, preAllocatedEntity);
            }
        }

        return entities.Values;
    }

    private static Entity? TryResolveAsPreAllocatedEntity(EntityAddress address)
    {
        if (address.IsAccount && address.Decode().IsPreAllocatedAccountAddress())
        {
            return new PreAllocatedAccountComponentEntity(address);
        }

        if (address.IsIdentity && address.Decode().IsPreAllocatedIdentityAddress())
        {
            return new PreAllocatedIdentityEntity(address);
        }

        return null;
    }
}
