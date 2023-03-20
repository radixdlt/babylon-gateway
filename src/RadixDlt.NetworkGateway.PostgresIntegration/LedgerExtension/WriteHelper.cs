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

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entities (id, from_state_version, address, global_address, ancestor_ids, parent_ancestor_id, owner_ancestor_id, global_ancestor_id, correlated_entities, discriminator, package_id, blueprint_name, royalty_vault_entity_id, divisibility, store_of_non_fungible_resource_manager_entity_id, non_fungible_id_type, code, code_type, stake_vault_entity_id, unstake_vault_entity_id, epoch_manager_entity_id, resource_manager_entity_id, royalty_vault_of_entity_id) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            var discriminator = GetDiscriminator<Abstractions.Model.EntityType>(e.GetType());

            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Address.AsByteArray(), NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.GlobalAddress?.ToString(), NpgsqlDbType.Text, token);
            await writer.WriteNullableAsync(e.AncestorIds?.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(e.ParentAncestorId, NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(e.OwnerAncestorId, NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(e.GlobalAncestorId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.CorrelatedEntities.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(discriminator, "entity_type", token);
            await writer.WriteNullableAsync(e is ComponentEntity ce1 ? ce1.PackageId : null, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e is ComponentEntity ce2 ? ce2.BlueprintName : null, NpgsqlDbType.Text, token);

            if (e is IRoyaltyVaultHolder rvh && rvh.RoyaltyVaultEntityId.HasValue)
            {
                await writer.WriteAsync(rvh.RoyaltyVaultEntityId.Value, token);
            }
            else
            {
                await writer.WriteNullAsync(token);
            }

            await writer.WriteNullableAsync(e is FungibleResourceManagerEntity frme ? frme.Divisibility : null, NpgsqlDbType.Integer, token);

            if (e is KeyValueStoreEntity kvse)
            {
                await writer.WriteNullableAsync(kvse.StoreOfNonFungibleResourceManagerEntityId, NpgsqlDbType.Boolean, token);
            }
            else
            {
                await writer.WriteNullAsync(token);
            }

            if (e is NonFungibleResourceManagerEntity nfrme)
            {
                await writer.WriteAsync(nfrme.NonFungibleIdType, "non_fungible_id_type", token);
            }
            else
            {
                await writer.WriteNullAsync(token);
            }

            if (e is PackageEntity pe)
            {
                await writer.WriteAsync(pe.Code, NpgsqlDbType.Bytea, token);
                await writer.WriteAsync(pe.CodeType, NpgsqlDbType.Text, token);
            }
            else
            {
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
            }

            if (e is ValidatorEntity validatorEntity)
            {
                await writer.WriteAsync(validatorEntity.StakeVaultEntityId, token);
                await writer.WriteAsync(validatorEntity.UnstakeVaultEntityId, token);
                await writer.WriteAsync(validatorEntity.EpochManagerEntityId, token);
            }
            else
            {
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
                await writer.WriteNullAsync(token);
            }

            if (e is VaultEntity ve)
            {
                await writer.WriteNullableAsync(ve.ResourceManagerEntityId, NpgsqlDbType.Bigint, token);
                await writer.WriteNullableAsync(ve.RoyaltyVaultOfEntityId, NpgsqlDbType.Bigint, token);
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

    public async Task<int> CopyLedgerTransaction(ICollection<LedgerTransaction> entities, ReferencedEntityDictionary referencedEntities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY ledger_transactions (state_version, status, error_message, transaction_accumulator, message, epoch, round_in_epoch, index_in_epoch, index_in_round, is_end_of_epoch, referenced_entities, fee_paid, tip_paid, round_timestamp, created_timestamp, normalized_round_timestamp, kind_filter_constraint, raw_payload, engine_receipt, discriminator, payload_hash, intent_hash, signed_intent_hash) FROM STDIN (FORMAT BINARY)", token);

        foreach (var lt in entities)
        {
            var discriminator = GetDiscriminator<Abstractions.Model.LedgerTransactionType>(lt.GetType());

            await writer.StartRowAsync(token);
            await writer.WriteAsync(lt.StateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.Status, "ledger_transaction_status", token);
            await writer.WriteAsync(lt.ErrorMessage, NpgsqlDbType.Text, token);
            await writer.WriteAsync(lt.TransactionAccumulator, NpgsqlDbType.Bytea, token);
            await writer.WriteNullableAsync(lt.Message, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(lt.Epoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.RoundInEpoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.IndexInEpoch, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.IndexInRound, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(lt.IsEndOfEpoch, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(referencedEntities.OfStateVersion(lt.StateVersion).Select(re => re.DatabaseId).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
            await writer.WriteNullableAsync(lt.FeePaid?.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteNullableAsync(lt.TipPaid?.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(lt.RoundTimestamp, NpgsqlDbType.TimestampTz, token);
            await writer.WriteAsync(lt.CreatedTimestamp, NpgsqlDbType.TimestampTz, token);
            await writer.WriteAsync(lt.NormalizedRoundTimestamp, NpgsqlDbType.TimestampTz, token);
            await writer.WriteNullableAsync(lt.KindFilterConstraint, "ledger_transaction_kind_filter_constraint", token);
            await writer.WriteAsync(lt.RawPayload, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(lt.EngineReceipt, NpgsqlDbType.Jsonb, token);

            switch (lt)
            {
                case UserLedgerTransaction ult:
                    await writer.WriteAsync(discriminator, "ledger_transaction_type", token);
                    await writer.WriteAsync(ult.PayloadHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(ult.IntentHash, NpgsqlDbType.Bytea, token);
                    await writer.WriteAsync(ult.SignedIntentHash, NpgsqlDbType.Bytea, token);
                    break;
                case ValidatorLedgerTransaction:
                    await writer.WriteAsync(discriminator, "ledger_transaction_type", token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    break;
                case SystemLedgerTransaction:
                    await writer.WriteAsync(discriminator, "ledger_transaction_type", token);
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

    public async Task<int> CopyEntityMetadataHistory(ICollection<EntityMetadataHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_metadata_history (id, from_state_version, entity_id, keys, values, updated_at_state_versions) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Keys.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.Values.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.UpdatedAtStateVersions.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityAccessRulesChainHistory(ICollection<EntityAccessRulesChainHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_access_rules_chain_history (id, from_state_version, entity_id, subtype, access_rules_chain) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Subtype, "access_rules_chain_subtype", token);
            await writer.WriteAsync(e.AccessRulesChain, NpgsqlDbType.Jsonb, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyEntityStateHistory(ICollection<EntityStateHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_state_history (id, from_state_version, entity_id, state) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.State, NpgsqlDbType.Jsonb, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
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

    public async Task<int> CopyResourceManagerEntitySupplyHistory(ICollection<ResourceManagerEntitySupplyHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY resource_manager_entity_supply_history (id, from_state_version, resource_manager_entity_id, total_supply, total_minted, total_burnt) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceManagerEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TotalSupply.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(e.TotalMinted.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
            await writer.WriteAsync(e.TotalBurnt.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
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

        await using var writer = await _connection.BeginBinaryImportAsync("COPY entity_resource_aggregated_vaults_history (id, from_state_version, entity_id, resource_entity_id, tmp_tmp_remove_me_once_tx_events_Become_available, discriminator, balance, total_count) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.TmpTmpRemoveMeOnceTxEventsBecomeAvailable, NpgsqlDbType.Text, token);
            await writer.WriteAsync(GetDiscriminator<Abstractions.Model.ResourceType>(e.GetType()), "resource_type", token);

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
            await writer.WriteAsync(GetDiscriminator<Abstractions.Model.ResourceType>(e.GetType()), "resource_type", token);
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
            await writer.WriteAsync(GetDiscriminator<Abstractions.Model.ResourceType>(e.GetType()), "resource_type", token);
            await writer.WriteNullAsync(token);
            await writer.WriteNullAsync(token);
            await writer.WriteAsync(e.NonFungibleIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return fungibleEntities.Count + nonFungibleEntities.Count;
    }

    public async Task<int> CopyNonFungibleIdData(ICollection<NonFungibleIdData> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY non_fungible_id_data (id, from_state_version, non_fungible_store_entity_id, non_fungible_resource_manager_entity_id, non_fungible_id, immutable_data) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleStoreEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleResourceManagerEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleId, NpgsqlDbType.Text, token);
            await writer.WriteAsync(e.ImmutableData, NpgsqlDbType.Bytea, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task<int> CopyNonFungibleIdMutableDataHistory(ICollection<NonFungibleIdMutableDataHistory> entities, CancellationToken token)
    {
        if (!entities.Any())
        {
            return 0;
        }

        await using var writer = await _connection.BeginBinaryImportAsync("COPY non_fungible_id_mutable_data_history (id, from_state_version, non_fungible_id_data_id, is_deleted, mutable_data) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleIdDataId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.MutableData, NpgsqlDbType.Bytea, token);
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

        await using var writer = await _connection.BeginBinaryImportAsync("COPY non_fungible_id_store_history (id, from_state_version, non_fungible_store_entity_id, non_fungible_resource_manager_entity_id, non_fungible_id_data_ids) FROM STDIN (FORMAT BINARY)", token);

        foreach (var e in entities)
        {
            await writer.StartRowAsync(token);
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleStoreEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleResourceManagerEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleIdDataIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        }

        await writer.CompleteAsync(token);

        return entities.Count;
    }

    public async Task UpdateSequences(SequencesHolder sequences, CancellationToken token)
    {
        var cd = new CommandDefinition(
            commandText: @"
SELECT
    setval('entity_state_history_id_seq', @EntityStateHistorySequence),
    setval('entities_id_seq', @entitySequence),
    setval('entity_access_rules_chain_history_id_seq', @entityAccessRulesChainHistorySequence),
    setval('entity_metadata_history_id_seq', @entityMetadataHistorySequence),
    setval('entity_resource_aggregated_vaults_history_id_seq', @entityResourceAggregatedVaultsHistorySequence),
    setval('entity_resource_aggregate_history_id_seq', @entityResourceAggregateHistorySequence),
    setval('entity_resource_vault_aggregate_history_id_seq', @entityResourceVaultAggregateHistorySequence),
    setval('entity_vault_history_id_seq', @entityVaultHistorySequence),
    setval('resource_manager_entity_supply_history_id_seq', @resourceManagerEntitySupplyHistorySequence),
    setval('non_fungible_id_data_id_seq', @nonFungibleIdDataSequence),
    setval('non_fungible_id_mutable_data_history_id_seq', @nonFungibleIdMutableDataHistorySequence),
    setval('non_fungible_id_store_history_id_seq', @nonFungibleIdStoreHistorySequence),
    setval('validator_public_key_history_id_seq', @validatorPublicKeyHistorySequence),
    setval('validator_active_set_history_id_seq', @validatorActiveSetHistorySequence)",
            parameters: new
            {
                entityStateHistorySequence = sequences.EntityStateHistorySequence,
                entitySequence = sequences.EntitySequence,
                entityAccessRulesChainHistorySequence = sequences.EntityAccessRulesChainHistorySequence,
                entityMetadataHistorySequence = sequences.EntityMetadataHistorySequence,
                entityResourceAggregatedVaultsHistorySequence = sequences.EntityResourceAggregatedVaultsHistorySequence,
                entityResourceAggregateHistorySequence = sequences.EntityResourceAggregateHistorySequence,
                entityResourceVaultAggregateHistorySequence = sequences.EntityResourceVaultAggregateHistorySequence,
                entityVaultHistorySequence = sequences.EntityVaultHistorySequence,
                resourceManagerEntitySupplyHistorySequence = sequences.ResourceManagerEntitySupplyHistorySequence,
                nonFungibleIdDataSequence = sequences.NonFungibleIdDataSequence,
                nonFungibleIdMutableDataHistorySequence = sequences.NonFungibleIdMutableDataHistorySequence,
                nonFungibleIdStoreHistorySequence = sequences.NonFungibleIdStoreHistorySequence,
                validatorPublicKeyHistorySequence = sequences.ValidatorPublicKeyHistorySequence,
                validatorActiveSetHistorySequence = sequences.ValidatorActiveSetHistorySequence,
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
