﻿/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
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
 * B. any failure in a transmission or loss of any token or assets keys or othsoer digital
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class EntitiesByRoleRequirementProcessor : IProcessorBase, ISubstateScanUpsertProcessor
{
    private readonly record struct ResourceDbLookup(string EntityAddress, string ResourceAddress);

    private readonly record struct NonFungibleDbLookup(string EntityAddress, string ResourceAddress, string NonFungibleLocalId);

    private readonly ProcessorContext _context;
    private readonly IEnumerable<ILedgerExtenderServiceObserver> _observers;
    private readonly Dictionary<ResourceDbLookup, long> _observedByResource = new();
    private readonly Dictionary<NonFungibleDbLookup, long> _observedByNonFungible = new();
    private readonly List<EntitiesByRoleRequirementEntryDefinition> _toAdd = new();
    private readonly CommonDbContext _dbContext;
    private readonly ILogger<EntitiesByRoleRequirementProcessor> _logger;
    private readonly IOptionsMonitor<LedgerProcessorsOptions> _optionsMonitor;

    private readonly ReferencedEntityDictionary _referencedEntityDictionary;

    public EntitiesByRoleRequirementProcessor(
        ProcessorContext context,
        CommonDbContext dbContext,
        ReferencedEntityDictionary referencedEntityDictionary,
        IOptionsMonitor<LedgerProcessorsOptions> optionsMonitor,
        IEnumerable<ILedgerExtenderServiceObserver> observers,
        ILogger<EntitiesByRoleRequirementProcessor> logger)
    {
        _context = context;
        _observers = observers;
        _logger = logger;
        _optionsMonitor = optionsMonitor;
        _dbContext = dbContext;
        _referencedEntityDictionary = referencedEntityDictionary;
    }

    public Task LoadDependenciesAsync()
    {
        return Task.CompletedTask;
    }

    public void OnUpsertScan(CoreModel.IUpsertedSubstate substate, ReferencedEntity referencedEntity, long stateVersion)
    {
        var substateData = substate.Value.SubstateData;

        if (substateData is CoreModel.RoleAssignmentModuleFieldOwnerRoleSubstate accessRulesFieldOwnerRole)
        {
            if (accessRulesFieldOwnerRole.Value?.OwnerRole?.Rule == null)
            {
                return;
            }

            var ownerRule = accessRulesFieldOwnerRole.Value.OwnerRole.Rule;
            var extracted = ExtractFromAccessRule(ownerRule);

            AddUsages(extracted, referencedEntity.Address, stateVersion);
        }

        if (substateData is CoreModel.RoleAssignmentModuleRuleEntrySubstate roleAssignmentEntry)
        {
            if (roleAssignmentEntry.Value?.AccessRule == null)
            {
                return;
            }

            var extracted = ExtractFromAccessRule(roleAssignmentEntry.Value.AccessRule);
            AddUsages(extracted, referencedEntity.Address, stateVersion);
        }

        if (substateData is CoreModel.PackageBlueprintAuthTemplateEntrySubstate blueprintAuthTemplate)
        {
            if (blueprintAuthTemplate.Value.AuthConfig.FunctionAccessRules == null)
            {
                return;
            }

            foreach (var functionAccessRule in blueprintAuthTemplate.Value.AuthConfig.FunctionAccessRules)
            {
                var extracted = ExtractFromAccessRule(functionAccessRule.Value);
                AddUsages(extracted, referencedEntity.Address, stateVersion);
            }
        }
    }

    public void ProcessChanges()
    {
        foreach (var entry in _observedByResource)
        {
            _toAdd.Add(
                new EntitiesByResourceRoleRequirementEntryDefinition
                {
                    Id = _context.Sequences.EntitiesByRoleRequirementSequence++,
                    EntityId = _referencedEntityDictionary.Get((EntityAddress)entry.Key.EntityAddress).DatabaseId,
                    ResourceEntityId = _referencedEntityDictionary.Get((EntityAddress)entry.Key.ResourceAddress).DatabaseId,
                    FirstSeenStateVersion = entry.Value,
                });
        }

        foreach (var entry in _observedByNonFungible)
        {
            _toAdd.Add(
                new EntitiesByNonFungibleRoleRequirementEntryDefinition
                {
                    Id = _context.Sequences.EntitiesByRoleRequirementSequence++,
                    EntityId = _referencedEntityDictionary.Get((EntityAddress)entry.Key.EntityAddress).DatabaseId,
                    ResourceEntityId = _referencedEntityDictionary.Get((EntityAddress)entry.Key.ResourceAddress).DatabaseId,
                    NonFungibleLocalId = entry.Key.NonFungibleLocalId,
                    FirstSeenStateVersion = entry.Value,
                });
        }

        foreach (var stateVersionEntries in _toAdd.GroupBy(x => x.FirstSeenStateVersion))
        {
            if (stateVersionEntries.Count() > _optionsMonitor.CurrentValue.EntitiesByRoleRequirementsPerStateVersionWarningThreshold)
            {
                _logger.LogWarning(
                    $"State version {stateVersionEntries.Key} has more than {_optionsMonitor.CurrentValue.EntitiesByRoleRequirementsPerStateVersionWarningThreshold} `entities_by_role_requirement_entry_definition` entries to add.");
            }
        }
    }

    public async Task<int> SaveEntitiesAsync()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyEntityByRoleRequirements();

        return rowsInserted;
    }

    private void AddUsages(AccessRuleRequirementExtractor.ExtractedRequirements toolkitResponse, string entityAddress, long stateVersion)
    {
        foreach (var resource in toolkitResponse.ResourceRequirements)
        {
            _referencedEntityDictionary.MarkSeenAddress((EntityAddress)resource);
            _observedByResource.TryAdd(new ResourceDbLookup(entityAddress, resource), stateVersion);
        }

        foreach (var nonFungible in toolkitResponse.NonFungibleRequirements)
        {
            _referencedEntityDictionary.MarkSeenAddress((EntityAddress)nonFungible.ResourceAddress);
            _observedByNonFungible.TryAdd(new NonFungibleDbLookup(entityAddress, nonFungible.ResourceAddress, nonFungible.NonFungibleLocalId), stateVersion);
        }
    }

    private AccessRuleRequirementExtractor.ExtractedRequirements ExtractFromAccessRule(CoreModel.AccessRule accessRule)
    {
        var extractor = new AccessRuleRequirementExtractor();
        return extractor.Extract(accessRule);
    }

    private async Task<int> CopyEntityByRoleRequirements()
    {
        var entities = _toAdd;

        if (entities.Count == 0)
        {
            return 0;
        }

        var connection = (NpgsqlConnection)_dbContext.Database.GetDbConnection();

        var sw = Stopwatch.GetTimestamp();

        await using var createTempTableCommand = connection.CreateCommand();
        createTempTableCommand.CommandText = @"
CREATE TEMP TABLE tmp_entities_by_role_requirement_entry_definition
(LIKE entities_by_role_requirement_entry_definition INCLUDING DEFAULTS)
ON COMMIT DROP";

        await createTempTableCommand.ExecuteNonQueryAsync(_context.Token);

        await using var writer =
            await connection.BeginBinaryImportAsync(
                "COPY tmp_entities_by_role_requirement_entry_definition (id, entity_id, first_seen_state_version, discriminator, resource_entity_id, non_fungible_local_id) FROM STDIN (FORMAT BINARY)",
                _context.Token);

        foreach (var e in entities)
        {
            var discriminator = _context.WriteHelper.GetDiscriminator<EntityRoleRequirementType>(e.GetType());

            await writer.StartRowAsync(_context.Token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, _context.Token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, _context.Token);
            await writer.WriteAsync(e.FirstSeenStateVersion, NpgsqlDbType.Bigint, _context.Token);
            await writer.WriteAsync(discriminator, "entity_role_requirement_type", _context.Token);

            if (e is EntitiesByResourceRoleRequirementEntryDefinition entitiesByResourceRoleRequirement)
            {
                await writer.WriteAsync(entitiesByResourceRoleRequirement.ResourceEntityId, NpgsqlDbType.Bigint, _context.Token);
                await writer.WriteNullAsync(_context.Token);
            }

            if (e is EntitiesByNonFungibleRoleRequirementEntryDefinition entitiesByNonFungibleRoleRequirement)
            {
                await writer.WriteAsync(entitiesByNonFungibleRoleRequirement.ResourceEntityId, NpgsqlDbType.Bigint, _context.Token);
                await writer.WriteAsync(entitiesByNonFungibleRoleRequirement.NonFungibleLocalId, NpgsqlDbType.Text, _context.Token);
            }
        }

        await writer.CompleteAsync(_context.Token);
        await writer.DisposeAsync();

        await using var mergeCommand = connection.CreateCommand();
        mergeCommand.CommandText = @"
MERGE INTO entities_by_role_requirement_entry_definition ebrr
USING (SELECT * FROM tmp_entities_by_role_requirement_entry_definition WHERE discriminator = 'resource') tmp
ON
    ebrr.discriminator = tmp.discriminator AND
    ebrr.entity_id = tmp.entity_id AND
    ebrr.resource_entity_id = tmp.resource_entity_id
WHEN NOT MATCHED THEN INSERT VALUES (id, entity_id, first_seen_state_version, discriminator, resource_entity_id, non_fungible_local_id);

MERGE INTO entities_by_role_requirement_entry_definition ebrr
USING (SELECT * FROM tmp_entities_by_role_requirement_entry_definition WHERE discriminator = 'non_fungible') tmp
ON
    ebrr.discriminator = tmp.discriminator AND
    ebrr.entity_id = tmp.entity_id AND
    ebrr.resource_entity_id = tmp.resource_entity_id AND
    ebrr.non_fungible_local_id = tmp.non_fungible_local_id
WHEN NOT MATCHED THEN INSERT VALUES(id, entity_id, first_seen_state_version, discriminator, resource_entity_id, non_fungible_local_id);";

        await mergeCommand.ExecuteNonQueryAsync(_context.Token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(CopyEntityByRoleRequirements), Stopwatch.GetElapsedTime(sw), entities.Count));

        return entities.Count;
    }
}
