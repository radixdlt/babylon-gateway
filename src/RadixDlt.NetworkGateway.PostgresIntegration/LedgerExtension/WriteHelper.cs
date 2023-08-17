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
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class WriteHelper
{
    private readonly NpgsqlConnection _connection;
    private readonly IModel _model;

    public WriteHelper(ReadWriteDbContext dbContext)
    {
        _connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        _model = dbContext.Model;
    }

    public async Task<int> CopyEntity(ICollection<Entity> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entities (id, from_state_version, address, is_global, ancestor_ids, parent_ancestor_id, owner_ancestor_id, global_ancestor_id, correlated_entities, discriminator, package_id, blueprint_name, divisibility, non_fungible_id_type, vm_type, stake_vault_entity_id, pending_xrd_withdraw_vault_entity_id, locked_owner_stake_unit_vault_entity_id, pending_owner_stake_unit_unlock_vault_entity_id, resource_entity_id, royalty_vault_of_entity_id) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            var discriminator = GetDiscriminator<EntityType>(e.GetType());

            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Address.ToString(), NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.IsGlobal, NpgsqlDbType.Boolean, token);
            await writer.WriteNullableAsync(e.AncestorIds?.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(e.ParentAncestorId, NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(e.OwnerAncestorId, NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(e.GlobalAncestorId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.CorrelatedEntities.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(discriminator, "entity_type", token);
            await writer.WriteNullableAsync(e is ComponentEntity ce1 ? ce1.PackageId : null, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e is ComponentEntity ce2 ? ce2.BlueprintName : null, NpgsqlDbType.Text, token);
            await writer.WriteNullableAsync(e is GlobalFungibleResourceEntity frme ? frme.Divisibility : null, NpgsqlDbType.Integer, token);

            if (e is GlobalNonFungibleResourceEntity nfrme)
            {
                await writer.WriteAsync(nfrme.NonFungibleIdType, "non_fungible_id_type", token);
            }
            else
            {
                await writer.WriteNullAsync(token);
            }

            if (e is GlobalPackageEntity packageEntity)
            {
                await writer.WriteAsync(packageEntity.VmType, "package_vm_type", token);
            }
            else
            {
                await writer.WriteNullAsync(token);
            }

            if (e is GlobalValidatorEntity validatorEntity)
            {
                await writer.WriteAsync(validatorEntity.StakeVaultEntityId, NpgsqlDbType.Bigint, token);
                await writer.WriteAsync(validatorEntity.PendingXrdWithdrawVault, NpgsqlDbType.Bigint, token);
                await writer.WriteAsync(validatorEntity.LockedOwnerStakeUnitVault, NpgsqlDbType.Bigint, token);
                await writer.WriteAsync(validatorEntity.PendingOwnerStakeUnitUnlockVault, NpgsqlDbType.Bigint, token);
            }
            else
            {
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
            }

            if (e is InternalFungibleVaultEntity fve)
            {
                await writer.WriteNullableAsync(fve.ResourceEntityId, NpgsqlDbType.Bigint, token);
                await writer.WriteNullableAsync(fve.RoyaltyVaultOfEntityId, NpgsqlDbType.Bigint, token);
            }
            else if (e is InternalNonFungibleVaultEntity nfve)
            {
                await writer.WriteNullableAsync(nfve.ResourceEntityId, NpgsqlDbType.Bigint, token);
                await writer.WriteNullAsync(token);
            }
            else
            {
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
            }
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyLedgerTransaction(ICollection<LedgerTransaction> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY ledger_transactions (state_version, epoch, round_in_epoch, index_in_epoch, index_in_round, fee_paid, tip_paid, affected_global_entities, round_timestamp, created_timestamp, normalized_round_timestamp, raw_payload, receipt_state_updates, receipt_status, receipt_fee_summary, receipt_error_message, receipt_output, receipt_next_epoch, receipt_event_sbors, receipt_event_schema_hashes, receipt_event_type_indexes, receipt_event_sbor_type_kinds, discriminator, payload_hash, intent_hash, signed_intent_hash, message) FROM STDIN (FORMAT BINARY)", token);

        foreach (var lt in entities)
        {
            var discriminator = GetDiscriminator<LedgerTransactionType>(lt.GetType());

            await writer.StartRowAsync(token);
            await writer.WriteAsync(lt.StateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.Epoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.RoundInEpoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.IndexInEpoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.IndexInRound, NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(lt.FeePaid?.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteNullableAsync(lt.TipPaid?.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(lt.AffectedGlobalEntities, NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.RoundTimestamp, NpgsqlDbType.TimestampTz, token);
            await writer.WriteAsync(lt.CreatedTimestamp, NpgsqlDbType.TimestampTz, token);
            await writer.WriteAsync(lt.NormalizedRoundTimestamp, NpgsqlDbType.TimestampTz, token);
            await writer.WriteAsync(lt.RawPayload, NpgsqlDbType.Bytea, token);

            await writer.WriteAsync(lt.EngineReceipt.StateUpdates, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.Status, "ledger_transaction_status", token);
            await writer.WriteAsync(lt.EngineReceipt.FeeSummary, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.ErrorMessage, NpgsqlDbType.Text, token);
            await writer.WriteAsync(lt.EngineReceipt.Output, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.NextEpoch, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(lt.EngineReceipt.EventsSbors, NpgsqlDbType.Array | NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(lt.EngineReceipt.EventSchemaHashes, NpgsqlDbType.Array | NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(lt.EngineReceipt.EventTypeIndexes, NpgsqlDbType.Array | NpgsqlDbType.Integer, token);
            await writer.WriteAsync(lt.EngineReceipt.EventSborTypeKinds, "sbor_type_kind[]", token);
            await writer.WriteAsync(discriminator, "ledger_transaction_type", token);

            switch (lt)
            {
                case GenesisLedgerTransaction:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case UserLedgerTransaction ult:
                    await writer.WriteAsync(ult.PayloadHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(ult.IntentHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(ult.SignedIntentHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(ult.Message, NpgsqlDbType.Jsonb, token);
                    break;
                case RoundUpdateLedgerTransaction:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lt), lt, null);
            }
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyLedgerTransactionMarkers(ICollection<LedgerTransactionMarker> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY ledger_transaction_markers (id, state_version, discriminator, event_type, entity_id, resource_entity_id, quantity, operation_type, origin_type) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            var discriminator = GetDiscriminator<LedgerTransactionMarkerType>(e.GetType());

            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.StateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(discriminator, "ledger_transaction_marker_type", token);

            switch (e)
            {
                case EventLedgerTransactionMarker eltm:
                    await writer.WriteAsync(eltm.EventType, "ledger_transaction_marker_event_type", token);
                    await writer.WriteAsync(eltm.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(eltm.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(eltm.Quantity.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case ManifestAddressLedgerTransactionMarker maltm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(maltm.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(maltm.OperationType, "ledger_transaction_marker_operation_type", token);
                    await writer.WriteNullAsync(token);
                    break;
                case OriginLedgerTransactionMarker oltm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(oltm.OriginType, "ledger_transaction_marker_origin_type", token);
                    break;
                case AffectedGlobalEntityTransactionMarker oltm:
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(oltm.EntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityMetadataHistory(ICollection<EntityMetadataHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_metadata_history (id, from_state_version, entity_id, key, value, is_deleted, is_locked) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Key, NpgsqlDbType.Text, token);
            await writer.WriteNullableAsync(e.Value, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityMetadataAggregateHistory(ICollection<EntityMetadataAggregateHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_metadata_aggregate_history (id, from_state_version, entity_id, metadata_ids) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.MetadataIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityRoleAssignmentsOwnerRoleHistory(List<EntityRoleAssignmentsOwnerRoleHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_role_assignments_owner_role_history (id, from_state_version, entity_id, role_assignments) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.RoleAssignments, NpgsqlDbType.Jsonb, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityRoleAssignmentsRulesEntryHistory(List<EntityRoleAssignmentsEntryHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_role_assignments_entry_history (id, from_state_version, entity_id, key_role, key_module, role_assignments, is_deleted) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyRole, NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.KeyModule, "object_module_id", token);
            await writer.WriteAsync(e.RoleAssignments, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityRoleAssignmentsAggregateHistory(List<EntityRoleAssignmentsAggregateHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_role_assignments_aggregate_history (id, from_state_version, entity_id, owner_role_id, entry_ids) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.OwnerRoleId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntryIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyStateHistory(ICollection<StateHistory> stateHistory, CancellationToken token)
    {
        if (!stateHistory.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY state_history (id, from_state_version, entity_id, json_state, sbor_state) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in stateHistory)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.JsonState, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(e.SborState, NpgsqlDbType.Bytea, token);
        }

        await writer.CompleteAsync(token);

        return stateHistory.Count;
    }

    public async Task<int> CopyValidatorKeyHistory(ICollection<ValidatorPublicKeyHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY validator_public_key_history (id, from_state_version, validator_entity_id, key_type, key) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ValidatorEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyType, "public_key_type", token);
            await writer.WriteAsync(e.Key, NpgsqlDbType.Bytea, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyValidatorActiveSetHistory(ICollection<ValidatorActiveSetHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY validator_active_set_history (id, from_state_version, epoch, validator_public_key_history_id, stake) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Epoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ValidatorPublicKeyHistoryId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Stake.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyResourceEntitySupplyHistory(ICollection<ResourceEntitySupplyHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY resource_entity_supply_history (id, from_state_version, resource_entity_id, total_supply, total_minted, total_burned) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalSupply.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(e.TotalMinted.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(e.TotalBurned.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityResourceAggregatedVaultsHistory(ICollection<EntityResourceAggregatedVaultsHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_resource_aggregated_vaults_history (id, from_state_version, entity_id, resource_entity_id, discriminator, balance, total_count) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(GetDiscriminator<ResourceType>(e.GetType()), "resource_type", token);

            if (e is EntityFungibleResourceAggregatedVaultsHistory fe)
            {
                await writer.WriteAsync(fe.Balance.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
                await writer.WriteNullAsync(token);
            }
            else if (e is EntityNonFungibleResourceAggregatedVaultsHistory nfe)
            {
                await writer.WriteNullAsync(token);
                await writer.WriteAsync(nfe.TotalCount, NpgsqlDbType.Bigint, token);
            }
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityResourceAggregateHistory(ICollection<EntityResourceAggregateHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_resource_aggregate_history (id, from_state_version, entity_id, fungible_resource_entity_ids, fungible_resource_significant_update_state_versions, non_fungible_resource_entity_ids, non_fungible_resource_significant_update_state_versions) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FungibleResourceEntityIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FungibleResourceSignificantUpdateStateVersions.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleResourceEntityIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleResourceSignificantUpdateStateVersions.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityResourceVaultAggregateHistory(ICollection<EntityResourceVaultAggregateHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_resource_vault_aggregate_history (id, from_state_version, entity_id, resource_entity_id, vault_entity_ids) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityVaultHistory(ICollection<EntityFungibleVaultHistory> fungibleEntities, ICollection<EntityNonFungibleVaultHistory> nonFungibleEntities, CancellationToken token)
    {
        if (!fungibleEntities.Any() && !nonFungibleEntities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_vault_history (id, from_state_version, owner_entity_id, global_entity_id, vault_entity_id, resource_entity_id, discriminator, balance, is_royalty_vault, non_fungible_ids) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in fungibleEntities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.OwnerEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.GlobalEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(GetDiscriminator<ResourceType>(e.GetType()), "resource_type", token);
            await writer.WriteAsync(e.Balance.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(e.IsRoyaltyVault, NpgsqlDbType.Boolean, token);
            await writer.WriteNullAsync(token);
        }

        foreach (var e in nonFungibleEntities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.OwnerEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.GlobalEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(GetDiscriminator<ResourceType>(e.GetType()), "resource_type", token);
            await writer.WriteNullAsync(token);
            await writer.WriteNullAsync(token);
            await writer.WriteAsync(e.NonFungibleIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return fungibleEntities.Count + nonFungibleEntities.Count;
    }

    public async Task<int> CopyComponentMethodRoyalties(List<ComponentMethodRoyaltyEntryHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY component_method_royalty_entry_history (id, from_state_version, entity_id, method_name, royalty_amount, is_locked) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.MethodName, NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.RoyaltyAmount, NpgsqlDbType.Jsonb, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyAccountDefaultDepositRuleHistory(List<AccountDefaultDepositRuleHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY account_default_deposit_rule_history (id, from_state_version, account_entity_id, default_deposit_rule) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.DefaultDepositRule, "account_default_deposit_rule", token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyValidatorEmissionStatistics(ICollection<ValidatorEmissionStatistics> entries, CancellationToken token)
    {
        if (!entries.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY validator_emission_statistics (id, validator_entity_id, epoch_number, proposals_made, proposals_missed) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entries)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ValidatorEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EpochNumber, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ProposalsMade, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ProposalsMissed, NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return entries.Count;
    }

    public async Task<int> CopyAccountResourcePreferenceRuleHistory(List<AccountResourcePreferenceRuleHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY account_resource_preference_rule_history (id, from_state_version, account_entity_id, resource_entity_id, account_resource_preference_rule, is_deleted) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(e.AccountResourcePreferenceRule, "account_resource_preference_rule", token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyNonFungibleIdData(ICollection<NonFungibleIdData> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY non_fungible_id_data (id, from_state_version, non_fungible_resource_entity_id, non_fungible_id) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleId, NpgsqlDbType.Text, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyNonFungibleIdDataHistory(ICollection<NonFungibleIdDataHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY non_fungible_id_data_history (id, from_state_version, non_fungible_id_data_id, data, is_deleted, is_locked) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleIdDataId, NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(e.Data, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyNonFungibleIdStoreHistory(ICollection<NonFungibleIdStoreHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY non_fungible_id_store_history (id, from_state_version, non_fungible_resource_entity_id, non_fungible_id_data_ids) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleIdDataIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyPackageBlueprintHistory(ICollection<PackageBlueprintHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY package_blueprint_history (id, from_state_version, package_entity_id, name, version, definition, dependant_entity_ids, auth_template, auth_template_is_locked, royalty_config, royalty_config_is_locked) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.PackageEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Name, NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.Version, NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.Definition, NpgsqlDbType.Jsonb, token);
            await writer.WriteNullableAsync(e.DependantEntityIds?.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AuthTemplate, NpgsqlDbType.Jsonb, token);
            await writer.WriteNullableAsync(e.AuthTemplateIsLocked, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.RoyaltyConfig, NpgsqlDbType.Jsonb, token);
            await writer.WriteNullableAsync(e.RoyaltyConfigIsLocked, NpgsqlDbType.Boolean, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyPackageCodeHistory(ICollection<PackageCodeHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY package_code_history (id, from_state_version, package_entity_id, code_hash, code) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.PackageEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.CodeHash, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.Code, NpgsqlDbType.Bytea, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyPackageSchemaHistory(ICollection<PackageSchemaHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY package_schema_history (id, from_state_version, package_entity_id, schema_hash, schema) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.PackageEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.SchemaHash, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.Schema, NpgsqlDbType.Bytea, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyKeyValueStoreEntryHistory(List<KeyValueStoreEntryHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY key_value_store_entry_history (id, from_state_version, key_value_store_entity_id, key, value, is_deleted, is_locked) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyValueStoreEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Key.ToArray(), NpgsqlDbType.Bytea, token);
            await writer.WriteNullableAsync(e.Value?.ToArray(), NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyNonFungibleDataSchemaHistory(ICollection<NonFungibleSchemaHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY non_fungible_schema_history (id, from_state_version, entity_id, schema, sbor_type_kind, type_index) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Schema, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.SborTypeKind, "sbor_type_kind", token);
            await writer.WriteAsync(e.TypeIndex, NpgsqlDbType.Integer, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyKeyValueStoreSchemaHistory(ICollection<KeyValueStoreSchemaHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY key_value_store_schema_history (id, from_state_version, key_value_store_entity_id, schema, key_sbor_type_kind, key_type_index, value_sbor_type_kind, value_type_index) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyValueStoreEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Schema, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.KeySborTypeKind, "sbor_type_kind", token);
            await writer.WriteAsync(e.KeyTypeIndex, NpgsqlDbType.Integer, token);
            await writer.WriteAsync(e.ValueSborTypeKind, "sbor_type_kind", token);
            await writer.WriteAsync(e.ValueTypeIndex, NpgsqlDbType.Integer, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task UpdateSequences(SequencesHolder sequences, CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
SELECT
    setval('account_default_deposit_rule_history_id_seq', @accountDefaultDepositRuleHistorySequence),
    setval('account_resource_preference_rule_history_id_seq', @accountResourceDepositRuleHistorySequence),
    setval('state_history_id_seq', @stateHistorySequence),
    setval('entities_id_seq', @entitySequence),
    setval('entity_metadata_history_id_seq', @entityMetadataHistorySequence),
    setval('entity_metadata_aggregate_history_id_seq', @entityMetadataAggregateHistorySequence),
    setval('entity_resource_aggregated_vaults_history_id_seq', @entityResourceAggregatedVaultsHistorySequence),
    setval('entity_resource_aggregate_history_id_seq', @entityResourceAggregateHistorySequence),
    setval('entity_resource_vault_aggregate_history_id_seq', @entityResourceVaultAggregateHistorySequence),
    setval('entity_vault_history_id_seq', @entityVaultHistorySequence),
    setval('entity_role_assignments_aggregate_history_id_seq', @entityRoleAssignmentsAggregateHistorySequence),
    setval('entity_role_assignments_entry_history_id_seq', @entityRoleAssignmentsEntryHistorySequence),
    setval('entity_role_assignments_owner_role_history_id_seq', @entityRoleAssignmentsOwnerRoleHistorySequence),
    setval('component_method_royalty_entry_history_id_seq', @componentMethodRoyaltyEntryHistorySequence),
    setval('resource_entity_supply_history_id_seq', @resourceEntitySupplyHistorySequence),
    setval('non_fungible_id_data_id_seq', @nonFungibleIdDataSequence),
    setval('non_fungible_id_data_history_id_seq', @nonFungibleIdDataHistorySequence),
    setval('non_fungible_id_store_history_id_seq', @nonFungibleIdStoreHistorySequence),
    setval('validator_public_key_history_id_seq', @validatorPublicKeyHistorySequence),
    setval('validator_active_set_history_id_seq', @validatorActiveSetHistorySequence),
    setval('ledger_transaction_markers_id_seq', @ledgerTransactionMarkerSequence),
    setval('package_blueprint_history_id_seq', @packageBlueprintHistorySequence),
    setval('package_code_history_id_seq', @packageCodeHistorySequence),
    setval('package_schema_history_id_seq', @packageSchemaHistorySequence),
    setval('key_value_store_entry_history_id_seq', @keyValueStoreEntryHistorySequence),
    setval('validator_emission_statistics_id_seq', @validatorEmissionStatisticsSequence),
    setval('non_fungible_schema_history_id_seq', @NonFungibleSchemaHistorySequence),
    setval('key_value_store_schema_history_id_seq', @KeyValueSchemaHistorySequence)",
            parameters: new
            {
                accountDefaultDepositRuleHistorySequence = sequences.AccountDefaultDepositRuleHistorySequence,
                accountResourceDepositRuleHistorySequence = sequences.AccountResourceDepositRuleHistorySequence,
                stateHistorySequence = sequences.StateHistorySequence,
                entitySequence = sequences.EntitySequence,
                entityMetadataHistorySequence = sequences.EntityMetadataHistorySequence,
                entityMetadataAggregateHistorySequence = sequences.EntityMetadataAggregateHistorySequence,
                entityResourceAggregatedVaultsHistorySequence = sequences.EntityResourceAggregatedVaultsHistorySequence,
                entityResourceAggregateHistorySequence = sequences.EntityResourceAggregateHistorySequence,
                entityResourceVaultAggregateHistorySequence = sequences.EntityResourceVaultAggregateHistorySequence,
                entityVaultHistorySequence = sequences.EntityVaultHistorySequence,
                entityRoleAssignmentsAggregateHistorySequence = sequences.EntityRoleAssignmentsAggregateHistorySequence,
                entityRoleAssignmentsEntryHistorySequence = sequences.EntityRoleAssignmentsEntryHistorySequence,
                entityRoleAssignmentsOwnerRoleHistorySequence = sequences.EntityRoleAssignmentsOwnerRoleHistorySequence,
                componentMethodRoyaltyEntryHistorySequence = sequences.ComponentMethodRoyaltyEntryHistorySequence,
                resourceEntitySupplyHistorySequence = sequences.ResourceEntitySupplyHistorySequence,
                nonFungibleIdDataSequence = sequences.NonFungibleIdDataSequence,
                nonFungibleIdDataHistorySequence = sequences.NonFungibleIdDataHistorySequence,
                nonFungibleIdStoreHistorySequence = sequences.NonFungibleIdStoreHistorySequence,
                validatorPublicKeyHistorySequence = sequences.ValidatorPublicKeyHistorySequence,
                validatorActiveSetHistorySequence = sequences.ValidatorActiveSetHistorySequence,
                ledgerTransactionMarkerSequence = sequences.LedgerTransactionMarkerSequence,
                packageBlueprintHistorySequence = sequences.PackageBlueprintHistorySequence,
                packageCodeHistorySequence = sequences.PackageCodeHistorySequence,
                packageSchemaHistorySequence = sequences.PackageSchemaHistorySequence,
                keyValueStoreEntryHistorySequence = sequences.KeyValueStoreEntryHistorySequence,
                validatorEmissionStatisticsSequence = sequences.ValidatorEmissionStatisticsSequence,
                nonFungibleSchemaHistorySequence = sequences.NonFungibleSchemaHistorySequence,
                keyValueSchemaHistorySequence = sequences.KeyValueSchemaHistorySequence,
            },
            cancellationToken: token);

        await _connection.ExecuteAsync(cd);
    }

    private T GetDiscriminator<T>(Type type)
    {
        if (_model.FindEntityType(type)?.GetDiscriminatorValue() is not T discriminator)
        {
            throw new InvalidOperationException($"Unable to determine discriminator of {type.Name}");
        }

        return discriminator;
    }
}
