using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Queries;

internal interface IEntityQuerier
{
    Task<TEntity> GetEntity<TEntity>(EntityAddress address, GatewayApiSdk.Model.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity;

    Task<TEntity> GetNonVirtualEntity<TEntity>(ReadOnlyDbContext dbContext, EntityAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity;

    Task<Dictionary<EntityAddress, long>> ResolveEntityIds(ReadOnlyDbContext dbContext, List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token);

    Task<ICollection<Entity>> GetEntities(List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token);
}

internal class EntityQuerier : IEntityQuerier
{
    private readonly IVirtualEntityDataProvider _virtualEntityDataProvider;
    private readonly ReadOnlyDbContext _dbContext;

    public EntityQuerier(IVirtualEntityDataProvider virtualEntityDataProvider, ReadOnlyDbContext dbContext)
    {
        _virtualEntityDataProvider = virtualEntityDataProvider;
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
            entity = await TryResolveAsVirtualEntity(address);

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

    public async Task<TEntity> GetNonVirtualEntity<TEntity>(ReadOnlyDbContext dbContext, EntityAddress address, GatewayModel.LedgerState ledgerState, CancellationToken token)
        where TEntity : Entity
    {
        var entity = await dbContext
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

    public async Task<Dictionary<EntityAddress, long>> ResolveEntityIds(ReadOnlyDbContext dbContext, List<EntityAddress> addresses, GatewayModel.LedgerState ledgerState, CancellationToken token)
    {
        var entities = await dbContext
            .Entities
            .Where(e => e.FromStateVersion <= ledgerState.StateVersion && addresses.Contains(e.Address))
            .AnnotateMetricName()
            .ToDictionaryAsync(e => e.Address, e => e.Id, token);

        return entities;
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
            var virtualEntity = await TryResolveAsVirtualEntity(address);

            if (virtualEntity != null)
            {
                entities.Add(virtualEntity.Address, virtualEntity);
            }
        }

        return entities.Values;
    }

    private async Task<Entity?> TryResolveAsVirtualEntity(EntityAddress address)
    {
        if (await _virtualEntityDataProvider.IsVirtualAccountAddress(address))
        {
            return new VirtualAccountComponentEntity(address);
        }

        if (await _virtualEntityDataProvider.IsVirtualIdentityAddress(address))
        {
            return new VirtualIdentityEntity(address);
        }

        return null;
    }
}
