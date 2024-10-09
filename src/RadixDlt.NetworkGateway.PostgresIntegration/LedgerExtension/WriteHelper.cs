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

using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class WriteHelper : IWriteHelper
{
    private static readonly Dictionary<string, Dictionary<string, int>> _maxAggregateCount = new();

    private readonly NpgsqlConnection _connection;
    private readonly IModel _model;
    private readonly IEnumerable<ILedgerExtenderServiceObserver> _observers;
    private readonly CancellationToken _token;

    public WriteHelper(ReadWriteDbContext dbContext, IEnumerable<ILedgerExtenderServiceObserver> observers, CancellationToken token = default)
    {
        _connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        _model = dbContext.Model;
        _observers = observers;
        _token = token;
    }

    public async Task<int> Copy<T>(ICollection<T> entities, string copy, Func<NpgsqlBinaryImporter, T, CancellationToken, Task> callback, [CallerMemberName] string stageName = "")
    {
        if (!entities.Any())
        {
            return 0;
        }

        var sw = Stopwatch.GetTimestamp();

        await using var writer = await _connection.BeginBinaryImportAsync(copy, _token);

        foreach (var e in entities)
        {
            await HandleMaxAggregateCounts(e);
            await writer.StartRowAsync(_token);
            await callback(writer, e, _token);
        }

        await writer.CompleteAsync(_token);

        await _observers.ForEachAsync(x => x.StageCompleted(stageName, Stopwatch.GetElapsedTime(sw), entities.Count));

        return entities.Count;
    }

    public async Task<int> CopyEntity(ICollection<Entity> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        var sw = Stopwatch.GetTimestamp();

        await using var writer = await _connection.BeginBinaryImportAsync(
            "COPY entities (id, from_state_version, address, is_global, ancestor_ids, parent_ancestor_id, owner_ancestor_id, global_ancestor_id, correlated_entity_relationships, correlated_entity_ids, discriminator, blueprint_name, blueprint_version, assigned_module_ids, divisibility, non_fungible_id_type, non_fungible_data_mutable_fields) FROM STDIN (FORMAT BINARY)",
            token);

        foreach (var e in entities)
        {
            var discriminator = GetDiscriminator<EntityType>(e.GetType());

            await HandleMaxAggregateCounts(e);
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Address.ToString(), NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.IsGlobal, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.AncestorIds?.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ParentAncestorId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.OwnerAncestorId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.GlobalAncestorId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Correlations.Select(x => x.Relationship).ToArray(), "entity_relationship[]", token);
            await writer.WriteAsync(e.Correlations.Select(x => x.EntityId).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(discriminator, "entity_type", token);

            if (e is ComponentEntity ce)
            {
                await writer.WriteAsync(ce.BlueprintName, NpgsqlDbType.Text, token);
                await writer.WriteAsync(ce.BlueprintVersion, NpgsqlDbType.Text, token);
                await writer.WriteAsync(ce.AssignedModuleIds, "module_id[]", token);
            }
            else
            {
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
            }

            if (e is GlobalFungibleResourceEntity frme)
            {
                await writer.WriteAsync(frme.Divisibility, NpgsqlDbType.Integer, token);
            }
            else
            {
                await writer.WriteNullAsync(token);
            }

            if (e is GlobalNonFungibleResourceEntity nfrme)
            {
                await writer.WriteAsync(nfrme.NonFungibleIdType, "non_fungible_id_type", token);
                await writer.WriteAsync(nfrme.NonFungibleDataMutableFields, NpgsqlDbType.Array | NpgsqlDbType.Text, token);
            }
            else
            {
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
            }
        }

        await writer.CompleteAsync(token);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(CopyEntity), Stopwatch.GetElapsedTime(sw), entities.Count));

        return entities.Count;
    }

    public async Task UpdateSequences(SequencesHolder sequences, CancellationToken token)
    {
        var sw = Stopwatch.GetTimestamp();

        var parameters = new
        {
            accountLockerEntryDefinitionSequence = sequences.AccountLockerEntryDefinitionSequence,
            accountLockerEntryResourceVaultDefinitionSequence = sequences.AccountLockerEntryResourceVaultDefinitionSequence,
            accountLockerEntryTouchHistorySequence = sequences.AccountLockerEntryTouchHistorySequence,
            accountDefaultDepositRuleHistorySequence = sequences.AccountDefaultDepositRuleHistorySequence,
            accountResourcePreferenceRuleEntryHistorySequence = sequences.AccountResourcePreferenceRuleEntryHistorySequence,
            accountResourcePreferenceRuleAggregateHistorySequence = sequences.AccountResourcePreferenceRuleAggregateHistorySequence,
            stateHistorySequence = sequences.StateHistorySequence,
            entitySequence = sequences.EntitySequence,
            entityMetadataEntryHistorySequence = sequences.EntityMetadataEntryHistorySequence,
            entityMetadataEntryDefinitionSequence = sequences.EntityMetadataEntryDefinitionSequence,
            entityMetadataTotalsHistorySequence = sequences.EntityMetadataTotalsHistorySequence,
            entityRoleAssignmentsAggregateHistorySequence = sequences.EntityRoleAssignmentsAggregateHistorySequence,
            entityRoleAssignmentsEntryHistorySequence = sequences.EntityRoleAssignmentsEntryHistorySequence,
            entityRoleAssignmentsOwnerRoleHistorySequence = sequences.EntityRoleAssignmentsOwnerRoleHistorySequence,
            componentMethodRoyaltyEntryHistorySequence = sequences.ComponentMethodRoyaltyEntryHistorySequence,
            componentMethodRoyaltyAggregateHistorySequence = sequences.ComponentMethodRoyaltyAggregateHistorySequence,
            resourceEntitySupplyHistorySequence = sequences.ResourceEntitySupplyHistorySequence,
            nonFungibleIdDataSequence = sequences.NonFungibleIdDefinitionSequence,
            nonFungibleIdDataHistorySequence = sequences.NonFungibleIdDataHistorySequence,
            nonFungibleIdLocationHistorySequence = sequences.NonFungibleIdLocationHistorySequence,
            validatorPublicKeyHistorySequence = sequences.ValidatorPublicKeyHistorySequence,
            validatorActiveSetHistorySequence = sequences.ValidatorActiveSetHistorySequence,
            ledgerTransactionMarkerSequence = sequences.LedgerTransactionMarkerSequence,
            packageBlueprintHistorySequence = sequences.PackageBlueprintHistorySequence,
            packageCodeHistorySequence = sequences.PackageCodeHistorySequence,
            schemaEntryDefinitionSequence = sequences.SchemaEntryDefinitionSequence,
            schemaEntryAggregateHistorySequence = sequences.SchemaEntryAggregateHistorySequence,
            keyValueStoreEntryDefinitionSequence = sequences.KeyValueStoreEntryDefinitionSequence,
            keyValueStoreEntryHistorySequence = sequences.KeyValueStoreEntryHistorySequence,
            validatorCumulativeEmissionHistorySequence = sequences.ValidatorCumulativeEmissionHistorySequence,
            nonFungibleSchemaHistorySequence = sequences.NonFungibleSchemaHistorySequence,
            keyValueSchemaHistorySequence = sequences.KeyValueSchemaHistorySequence,
            packageBlueprintAggregateHistorySequence = sequences.PackageBlueprintAggregateHistorySequence,
            packageCodeAggregateHistorySequence = sequences.PackageCodeAggregateHistorySequence,
            accountAuthorizedDepositorEntryHistorySequence = sequences.AccountAuthorizedDepositorEntryHistorySequence,
            accountAuthorizedDepositorAggregateHistorySequence = sequences.AccountAuthorizedDepositorAggregateHistorySequence,
            unverifiedStandardMetadataAggregateHistorySequence = sequences.UnverifiedStandardMetadataAggregateHistorySequence,
            unverifiedStandardMetadataEntryHistorySequence = sequences.UnverifiedStandardMetadataEntryHistorySequence,
            resourceHoldersSequence = sequences.ResourceHoldersSequence,
            entityResourceEntryDefinitionSequence = sequences.EntityResourceEntryDefinitionSequence,
            entityResourceVaultEntryDefinitionSequence = sequences.EntityResourceVaultEntryDefinitionSequence,
            entityResourceTotalsHistorySequence = sequences.EntityResourceTotalsHistorySequence,
            entityResourceVaultTotalsHistorySequence = sequences.EntityResourceVaultTotalsHistorySequence,
            entityResourceBalanceHistorySequence = sequences.EntityResourceBalanceHistorySequence,
            vaultBalanceHistorySequence = sequences.VaultBalanceHistorySequence,
            nonFungibleVaultEntryDefinitionSequence = sequences.NonFungibleVaultEntryDefinitionSequence,
            nonFungibleVaultEntryHistorySequence = sequences.NonFungibleVaultEntryHistorySequence,
        };

        var cd = DapperExtensions.CreateCommandDefinition(
            commandText: @"
SELECT
    setval('account_locker_entry_definition_id_seq', @accountLockerEntryDefinitionSequence),
    setval('account_locker_entry_resource_vault_definition_id_seq', @accountLockerEntryResourceVaultDefinitionSequence),
    setval('account_locker_entry_touch_history_id_seq', @accountLockerEntryTouchHistorySequence),
    setval('account_default_deposit_rule_history_id_seq', @accountDefaultDepositRuleHistorySequence),
    setval('account_resource_preference_rule_entry_history_id_seq', @accountResourcePreferenceRuleEntryHistorySequence),
    setval('account_resource_preference_rule_aggregate_history_id_seq', @accountResourcePreferenceRuleAggregateHistorySequence),
    setval('state_history_id_seq', @stateHistorySequence),
    setval('entities_id_seq', @entitySequence),
    setval('entity_metadata_entry_history_id_seq', @entityMetadataEntryHistorySequence),
    setval('entity_metadata_entry_definition_id_seq', @entityMetadataEntryDefinitionSequence),
    setval('entity_metadata_totals_history_id_seq', @entityMetadataTotalsHistorySequence),
    setval('entity_role_assignments_aggregate_history_id_seq', @entityRoleAssignmentsAggregateHistorySequence),
    setval('entity_role_assignments_entry_history_id_seq', @entityRoleAssignmentsEntryHistorySequence),
    setval('entity_role_assignments_owner_role_history_id_seq', @entityRoleAssignmentsOwnerRoleHistorySequence),
    setval('component_method_royalty_entry_history_id_seq', @componentMethodRoyaltyEntryHistorySequence),
    setval('component_method_royalty_aggregate_history_id_seq', @componentMethodRoyaltyAggregateHistorySequence),
    setval('resource_entity_supply_history_id_seq', @resourceEntitySupplyHistorySequence),
    setval('non_fungible_id_definition_id_seq', @nonFungibleIdDataSequence),
    setval('non_fungible_id_data_history_id_seq', @nonFungibleIdDataHistorySequence),
    setval('non_fungible_id_location_history_id_seq', @nonFungibleIdLocationHistorySequence),
    setval('validator_public_key_history_id_seq', @validatorPublicKeyHistorySequence),
    setval('validator_active_set_history_id_seq', @validatorActiveSetHistorySequence),
    setval('ledger_transaction_markers_id_seq', @ledgerTransactionMarkerSequence),
    setval('package_blueprint_history_id_seq', @packageBlueprintHistorySequence),
    setval('package_code_history_id_seq', @packageCodeHistorySequence),
    setval('schema_entry_definition_id_seq', @schemaEntryDefinitionSequence),
    setval('schema_entry_aggregate_history_id_seq', @schemaEntryAggregateHistorySequence),
    setval('key_value_store_entry_definition_id_seq', @keyValueStoreEntryDefinitionSequence),
    setval('key_value_store_entry_history_id_seq', @keyValueStoreEntryHistorySequence),
    setval('validator_cumulative_emission_history_id_seq', @validatorCumulativeEmissionHistorySequence),
    setval('non_fungible_schema_history_id_seq', @NonFungibleSchemaHistorySequence),
    setval('key_value_store_schema_history_id_seq', @KeyValueSchemaHistorySequence),
    setval('package_blueprint_aggregate_history_id_seq', @packageBlueprintAggregateHistorySequence),
    setval('package_code_aggregate_history_id_seq', @packageCodeAggregateHistorySequence),
    setval('account_authorized_depositor_entry_history_id_seq', @accountAuthorizedDepositorEntryHistorySequence),
    setval('account_authorized_depositor_aggregate_history_id_seq', @accountAuthorizedDepositorAggregateHistorySequence),
    setval('unverified_standard_metadata_aggregate_history_id_seq', @unverifiedStandardMetadataAggregateHistorySequence),
    setval('unverified_standard_metadata_entry_history_id_seq', @unverifiedStandardMetadataEntryHistorySequence),
    setval('resource_holders_id_seq', @resourceHoldersSequence),
    setval('entity_resource_entry_definition_id_seq', @entityResourceEntryDefinitionSequence),
    setval('entity_resource_vault_entry_definition_id_seq', @entityResourceVaultEntryDefinitionSequence),
    setval('entity_resource_totals_history_id_seq', @entityResourceTotalsHistorySequence),
    setval('entity_resource_vault_totals_history_id_seq', @entityResourceVaultTotalsHistorySequence),
    setval('entity_resource_balance_history_id_seq', @entityResourceBalanceHistorySequence),
    setval('vault_balance_history_id_seq', @vaultBalanceHistorySequence),
    setval('non_fungible_vault_entry_definition_id_seq', @nonFungibleVaultEntryDefinitionSequence),
    setval('non_fungible_vault_entry_history_id_seq', @nonFungibleVaultEntryHistorySequence)
",
            parameters,
            cancellationToken: token);

        await _connection.ExecuteAsync(cd);

        await _observers.ForEachAsync(x => x.StageCompleted(nameof(UpdateSequences), Stopwatch.GetElapsedTime(sw), null));
    }

    public T GetDiscriminator<T>(Type type)
    {
        if (_model.FindEntityType(type)?.GetDiscriminatorValue() is not T discriminator)
        {
            throw new InvalidOperationException($"Unable to determine discriminator of {type.Name}");
        }

        return discriminator;
    }

    public async ValueTask HandleMaxAggregateCounts(object? entity)
    {
        if (entity is not IAggregateHolder aggregateHolder)
        {
            return;
        }

        var entityName = entity.GetType().Name;

        foreach (var (propertyName, count) in aggregateHolder.AggregateCounts())
        {
            var ec = _maxAggregateCount.GetOrAdd(entityName, _ => new Dictionary<string, int>());

            if (!ec.TryGetValue(propertyName, out var maxValueObserved) || maxValueObserved <= count)
            {
                ec[propertyName] = count;

                await _observers.ForEachAsync(x => x.AggregateMaxCount(entityName, propertyName, count));
            }
        }
    }
}
