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
            .AnnotateMetricName("GetOwnerRoles")
            .ToListAsync(token);

        var entries = await _dbContext
            .EntityRoleAssignmentsEntryHistory
            .Where(e => roleAssignmentsHistory.Contains(e.Id))
            .Where(e => !e.IsDeleted)
            .AnnotateMetricName("GetRoleEntires")
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
            .AnnotateMetricName()
            .ToListAsync(token);

        return aggregates;
    }
}
