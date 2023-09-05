using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreApiModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal interface IRoleAssignmentQuerier
{
    Task<Dictionary<long, GatewayApiSdk.Model.ComponentEntityRoleAssignments>> GetRoleAssignmentsHistory(
        List<ComponentEntity> componentEntities,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default);
}

internal class RoleAssignmentQuerier : IRoleAssignmentQuerier
{
    private readonly ReadOnlyDbContext _dbContext;
    private readonly IRoleAssignmentsMapper _roleAssignmentsMapper;
    private readonly IBlueprintProvider _blueprintProvider;

    public RoleAssignmentQuerier(
        ReadOnlyDbContext dbContext,
        IRoleAssignmentsMapper roleAssignmentsMapper,
        IBlueprintProvider blueprintProvider)
    {
        _dbContext = dbContext;
        _roleAssignmentsMapper = roleAssignmentsMapper;
        _blueprintProvider = blueprintProvider;
    }

    public async Task<Dictionary<long, GatewayApiSdk.Model.ComponentEntityRoleAssignments>> GetRoleAssignmentsHistory(
        List<ComponentEntity> componentEntities,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default)
    {
        var componentLookup = componentEntities.Select(x => x.Id).ToHashSet();
        var aggregates = await GetEntityRoleAssignmentsAggregateHistory(componentLookup, ledgerState, token);

        var blueprintLookup = componentEntities.Select(x => new BlueprintDefinitionIdentifier(x.BlueprintName, x.BlueprintVersion, x.PackageId)).ToHashSet();
        var blueprintDefinitions = await _blueprintProvider.GetBlueprints(blueprintLookup, ledgerState, token);
        var blueprintAuthConfigs = ExtractAuthConfigurationFromBlueprint(blueprintDefinitions);

        var ownerRoleIds = aggregates.Select(a => a.OwnerRoleId).Distinct().ToList();
        var roleAssignmentsHistory = aggregates.SelectMany(a => a.EntryIds).Distinct().ToList();

        var ownerRoles = await _dbContext
            .EntityRoleAssignmentsOwnerHistory
            .Where(e => ownerRoleIds.Contains(e.Id))
            .ToListAsync(token);

        var entries = await _dbContext
            .EntityRoleAssignmentsEntryHistory
            .Where(e => roleAssignmentsHistory.Contains(e.Id))
            .Where(e => !e.IsDeleted)
            .ToListAsync(token);

        return _roleAssignmentsMapper.GetEffectiveRoleAssignments(componentEntities, blueprintAuthConfigs, ownerRoles, entries);
    }

    private Dictionary<BlueprintDefinitionIdentifier, CoreApiModel.AuthConfig> ExtractAuthConfigurationFromBlueprint(Dictionary<BlueprintDefinitionIdentifier, PackageBlueprintHistory> blueprints)
    {
        return blueprints.ToDictionary(x => x.Key, x =>
        {
            if (string.IsNullOrEmpty(x.Value.AuthTemplate))
            {
                throw new UnreachableException($"Auth template configuration not found in blueprint:{x.Value.Name} version:{x.Value.Version}, packageId: {x.Value.PackageEntityId}");
            }

            var authConfig = JsonConvert.DeserializeObject<CoreApiModel.AuthConfig>(x.Value.AuthTemplate);

            if (authConfig == null)
            {
                throw new UnreachableException($"Unable to parse auth config to coreAPI model. Value: {x.Value.AuthTemplate}");
            }

            return authConfig;
        });
    }

    private async Task<List<EntityRoleAssignmentsAggregateHistory>> GetEntityRoleAssignmentsAggregateHistory(
        IReadOnlyCollection<long> componentIds,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default)
    {
        if (!componentIds.Any())
        {
            return new List<EntityRoleAssignmentsAggregateHistory>();
        }

        var entityIds = componentIds.ToList();

        var aggregates = await _dbContext
            .EntityRoleAssignmentsAggregateHistory
            .FromSqlInterpolated($@"
WITH variables (entity_id) AS (SELECT UNNEST({entityIds}))
SELECT earah.*
FROM variables v
INNER JOIN LATERAL (
    SELECT *
    FROM entity_role_assignments_aggregate_history
    WHERE entity_id = v.entity_id AND from_state_version <= {ledgerState.StateVersion}
    ORDER BY from_state_version DESC
    LIMIT 1
) earah ON TRUE;")
            .ToListAsync(token);

        return aggregates;
    }
}
