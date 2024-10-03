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

using NpgsqlTypes;
using RadixDlt.NetworkGateway.Abstractions.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal class VaultProcessor
{
    private readonly record struct NonFungibleIdDefinitionDbLookup(long NonFungibleResourceEntityId, string NonFungibleId);

    private readonly record struct NonFungibleVaultEntryDefinitionDbLookup(long VaultEntityId, long NonFungibleIdDefinitionId);

    private readonly record struct VaultBalanceSnapshot(VaultEntity VaultEntity, TokenAmount Balance, long StateVersion);

    private readonly record struct NonFungibleVaultChange(VaultEntity VaultEntity, string NonFungibleLocalId, bool IsDeposit, long StateVersion);

    private readonly record struct NonFungibleIdChange(bool IsDeleted, bool IsLocked, byte[]? MutableData, long StateVersion);

    private readonly ProcessorContext _context;

    private readonly List<VaultBalanceSnapshot> _observedVaultBalanceSnapshots = new();
    private readonly List<NonFungibleVaultChange> _observedNonFungibleVaultChanges = new();
    private readonly Dictionary<NonFungibleIdDefinitionDbLookup, NonFungibleIdChange> _observedNonFungibleDataEntries = new();
    private readonly Dictionary<NonFungibleIdDefinitionDbLookup, NonFungibleIdDefinition> _existingNonFungibleIdDefinitions = new();
    private readonly Dictionary<NonFungibleVaultEntryDefinitionDbLookup, NonFungibleVaultEntryDefinition> _existingNonFungibleVaultEntryDefinitions = new();
    private readonly List<VaultBalanceHistory> _balanceHistoryToAdd = new();
    private readonly List<NonFungibleIdDefinition> _nonFungibleIdDefinitionsToAdd = new();
    private readonly List<NonFungibleIdDataHistory> _nonFungibleIdDataHistoryToAdd = new();
    private readonly List<NonFungibleIdLocationHistory> _nonFungibleIdLocationHistoryToAdd = new();
    private readonly List<NonFungibleVaultEntryDefinition> _nonFungibleVaultEntryDefinitionsToAdd = new();
    private readonly List<NonFungibleVaultEntryHistory> _nonFungibleVaultEntryHistoryToAdd = new();

    public VaultProcessor(ProcessorContext context)
    {
        _context = context;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion, CoreModel.IUpsertedSubstate substate)
    {
        if (substateData is CoreModel.FungibleVaultFieldBalanceSubstate fungibleVaultFieldBalanceSubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalFungibleVaultEntity>();
            var amount = TokenAmount.FromDecimalString(fungibleVaultFieldBalanceSubstate.Value.Amount);

            _observedVaultBalanceSnapshots.Add(new VaultBalanceSnapshot(vaultEntity, amount, stateVersion));
        }

        if (substateData is CoreModel.NonFungibleVaultFieldBalanceSubstate nonFungibleVaultFieldBalanceSubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
            var amount = TokenAmount.FromDecimalString(nonFungibleVaultFieldBalanceSubstate.Value.Amount);

            _observedVaultBalanceSnapshots.Add(new VaultBalanceSnapshot(vaultEntity, amount, stateVersion));
        }

        if (substateData is CoreModel.NonFungibleResourceManagerDataEntrySubstate nonFungibleResourceManagerDataEntrySubstate)
        {
            var nonFungibleId = ScryptoSborUtils.GetNonFungibleId(((CoreModel.MapSubstateKey)substate.SubstateId.SubstateKey).KeyHex);

            _observedNonFungibleDataEntries.TryAdd(
                new NonFungibleIdDefinitionDbLookup(referencedEntity.DatabaseId, nonFungibleId),
                new NonFungibleIdChange(
                    nonFungibleResourceManagerDataEntrySubstate.Value == null,
                    nonFungibleResourceManagerDataEntrySubstate.IsLocked,
                    nonFungibleResourceManagerDataEntrySubstate.Value?.DataStruct.StructData.GetDataBytes(),
                    stateVersion));
        }

        if (substateData is CoreModel.NonFungibleVaultContentsIndexEntrySubstate nonFungibleVaultContentsIndexEntrySubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
            var simpleRep = nonFungibleVaultContentsIndexEntrySubstate.Key.NonFungibleLocalId.SimpleRep;

            _observedNonFungibleVaultChanges.Add(new NonFungibleVaultChange(vaultEntity, simpleRep, true, stateVersion));
        }
    }

    public void VisitDelete(CoreModel.SubstateId substateId, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateId.SubstateType == CoreModel.SubstateType.NonFungibleVaultContentsIndexEntry)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
            var simpleRep = ScryptoSborUtils.GetNonFungibleId(((CoreModel.MapSubstateKey)substateId.SubstateKey).KeyHex);

            _observedNonFungibleVaultChanges.Add(new NonFungibleVaultChange(vaultEntity, simpleRep, false, stateVersion));
        }
    }

    public async Task LoadDependencies()
    {
        _existingNonFungibleIdDefinitions.AddRange(await ExistingNonFungibleIdDefinitions());
        _existingNonFungibleVaultEntryDefinitions.AddRange(await ExistingNonFungibleVaultEntryDefinitions());
    }

    public void ProcessChanges()
    {
        _balanceHistoryToAdd.AddRange(
            _observedVaultBalanceSnapshots.Select(
                x => new VaultBalanceHistory
                {
                    Id = _context.Sequences.VaultBalanceHistorySequence++,
                    FromStateVersion = x.StateVersion,
                    VaultEntityId = x.VaultEntity.Id,
                    Balance = x.Balance,
                }));

        foreach (var (lookup, nonFungibleIdChange) in _observedNonFungibleDataEntries)
        {
            var definition = _existingNonFungibleIdDefinitions.GetOrAdd(
                lookup,
                _ =>
                {
                    var definition = new NonFungibleIdDefinition
                    {
                        Id = _context.Sequences.NonFungibleIdDefinitionSequence++,
                        FromStateVersion = nonFungibleIdChange.StateVersion,
                        NonFungibleResourceEntityId = lookup.NonFungibleResourceEntityId,
                        NonFungibleId = lookup.NonFungibleId,
                    };

                    _nonFungibleIdDefinitionsToAdd.Add(definition);

                    return definition;
                });

            _nonFungibleIdDataHistoryToAdd.Add(
                new NonFungibleIdDataHistory
                {
                    Id = _context.Sequences.NonFungibleIdDataHistorySequence++,
                    FromStateVersion = nonFungibleIdChange.StateVersion,
                    NonFungibleIdDefinitionId = definition.Id,
                    Data = nonFungibleIdChange.MutableData,
                    IsDeleted = nonFungibleIdChange.IsDeleted,
                    IsLocked = nonFungibleIdChange.IsLocked,
                });
        }

        foreach (var change in _observedNonFungibleVaultChanges)
        {
            var nonFungibleIdDefinition = _existingNonFungibleIdDefinitions.GetOrAdd(
                new NonFungibleIdDefinitionDbLookup(change.VaultEntity.GetResourceEntityId(), change.NonFungibleLocalId),
                lookup =>
                {
                    var definition = new NonFungibleIdDefinition
                    {
                        Id = _context.Sequences.NonFungibleIdDefinitionSequence++,
                        FromStateVersion = change.StateVersion,
                        NonFungibleResourceEntityId = lookup.NonFungibleResourceEntityId,
                        NonFungibleId = lookup.NonFungibleId,
                    };

                    _nonFungibleIdDefinitionsToAdd.Add(definition);

                    return definition;
                });

            var nonFungibleVaultEntryDefinition = _existingNonFungibleVaultEntryDefinitions.GetOrAdd(
                new NonFungibleVaultEntryDefinitionDbLookup(change.VaultEntity.Id, nonFungibleIdDefinition.Id),
                lookup =>
                {
                    var definition = new NonFungibleVaultEntryDefinition
                    {
                        Id = _context.Sequences.NonFungibleVaultEntryDefinitionSequence++,
                        FromStateVersion = change.StateVersion,
                        VaultEntityId = lookup.VaultEntityId,
                        NonFungibleIdDefinitionId = lookup.NonFungibleIdDefinitionId,
                    };

                    _nonFungibleVaultEntryDefinitionsToAdd.Add(definition);

                    return definition;
                });

            _nonFungibleVaultEntryHistoryToAdd.Add(
                new NonFungibleVaultEntryHistory
                {
                    Id = _context.Sequences.NonFungibleVaultEntryHistorySequence++,
                    FromStateVersion = change.StateVersion,
                    NonFungibleVaultEntryDefinitionId = nonFungibleVaultEntryDefinition.Id,
                    IsDeleted = !change.IsDeposit,
                });

            if (change.IsDeposit)
            {
                _nonFungibleIdLocationHistoryToAdd.Add(
                    new NonFungibleIdLocationHistory
                    {
                        Id = _context.Sequences.NonFungibleIdLocationHistorySequence++,
                        FromStateVersion = change.StateVersion,
                        NonFungibleIdDefinitionId = nonFungibleIdDefinition.Id,
                        VaultEntityId = nonFungibleVaultEntryDefinition.VaultEntityId,
                    });
            }
        }
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyVaultBalanceHistory();
        rowsInserted += await CopyNonFungibleIdDefinitions();
        rowsInserted += await CopyNonFungibleIdDataHistory();
        rowsInserted += await CopyNonFungibleIdLocationHistory();
        rowsInserted += await CopyNonFungibleVaultEntryDefinitions();
        rowsInserted += await CopyNonFungibleVaultEntryHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<NonFungibleIdDefinitionDbLookup, NonFungibleIdDefinition>> ExistingNonFungibleIdDefinitions()
    {
        var lookup = _observedNonFungibleVaultChanges
            .Select(x => new NonFungibleIdDefinitionDbLookup(x.VaultEntity.GetResourceEntityId(), x.NonFungibleLocalId))
            .ToHashSet();

        lookup.UnionWith(_observedNonFungibleDataEntries.Keys);

        if (!lookup.Unzip(x => x.NonFungibleResourceEntityId, x => x.NonFungibleId, out var resourceEntityIds, out var nonFungibleLocalIds))
        {
            return ImmutableDictionary<NonFungibleIdDefinitionDbLookup, NonFungibleIdDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<NonFungibleIdDefinitionDbLookup, NonFungibleIdDefinition>(
            @$"
WITH variables (resource_entity_id, non_fungible_id) AS (
    SELECT unnest({resourceEntityIds}), unnest({nonFungibleLocalIds})
)
SELECT *
FROM non_fungible_id_definition
WHERE (non_fungible_resource_entity_id, non_fungible_id) IN (SELECT * FROM variables);",
            e => new NonFungibleIdDefinitionDbLookup(e.NonFungibleResourceEntityId, e.NonFungibleId));
    }

    private async Task<IDictionary<NonFungibleVaultEntryDefinitionDbLookup, NonFungibleVaultEntryDefinition>> ExistingNonFungibleVaultEntryDefinitions()
    {
        var lookup = new HashSet<NonFungibleVaultEntryDefinitionDbLookup>();

        foreach (var change in _observedNonFungibleVaultChanges)
        {
            if (_existingNonFungibleIdDefinitions.TryGetValue(
                    new NonFungibleIdDefinitionDbLookup(change.VaultEntity.GetResourceEntityId(), change.NonFungibleLocalId),
                    out var nonFungibleIdDefinition))
            {
                lookup.Add(new NonFungibleVaultEntryDefinitionDbLookup(change.VaultEntity.Id, nonFungibleIdDefinition.Id));
            }
        }

        if (!lookup.Unzip(x => x.VaultEntityId, x => x.NonFungibleIdDefinitionId, out var vaultEntityIds, out var nonFungibleIdDefinitionIds))
        {
            return ImmutableDictionary<NonFungibleVaultEntryDefinitionDbLookup, NonFungibleVaultEntryDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<NonFungibleVaultEntryDefinitionDbLookup, NonFungibleVaultEntryDefinition>(
            @$"
WITH variables (vault_entity_id, non_fungible_id_definition_id) AS (
    SELECT unnest({vaultEntityIds}), unnest({nonFungibleIdDefinitionIds})
)
SELECT *
FROM non_fungible_vault_entry_definition
WHERE (vault_entity_id, non_fungible_id_definition_id) IN (SELECT * FROM variables);",
            e => new NonFungibleVaultEntryDefinitionDbLookup(e.VaultEntityId, e.NonFungibleIdDefinitionId));
    }

    private Task<int> CopyVaultBalanceHistory() => _context.WriteHelper.Copy(
        _balanceHistoryToAdd,
        "COPY vault_balance_history (id, from_state_version, vault_entity_id, balance) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Balance.GetSubUnitsSafeForPostgres(), NpgsqlDbType.Numeric, token);
        });

    private Task<int> CopyNonFungibleIdLocationHistory() => _context.WriteHelper.Copy(
        _nonFungibleIdLocationHistoryToAdd,
        "COPY non_fungible_id_location_history (id, from_state_version, non_fungible_id_definition_id, vault_entity_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleIdDefinitionId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityId, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyNonFungibleIdDefinitions() => _context.WriteHelper.Copy(
        _nonFungibleIdDefinitionsToAdd,
        "COPY non_fungible_id_definition (id, from_state_version, non_fungible_resource_entity_id, non_fungible_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleId, NpgsqlDbType.Text, token);
        });

    private Task<int> CopyNonFungibleIdDataHistory() => _context.WriteHelper.Copy(
        _nonFungibleIdDataHistoryToAdd,
        "COPY non_fungible_id_data_history (id, from_state_version, non_fungible_id_definition_id, data, is_deleted, is_locked) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleIdDefinitionId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.Data, NpgsqlDbType.Bytea, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(e.IsLocked, NpgsqlDbType.Boolean, token);
        });

    private Task<int> CopyNonFungibleVaultEntryDefinitions() => _context.WriteHelper.Copy(
        _nonFungibleVaultEntryDefinitionsToAdd,
        "COPY non_fungible_vault_entry_definition (id, from_state_version, vault_entity_id, non_fungible_id_definition_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleIdDefinitionId, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyNonFungibleVaultEntryHistory() => _context.WriteHelper.Copy(
        _nonFungibleVaultEntryHistoryToAdd,
        "COPY non_fungible_vault_entry_history (id, from_state_version, non_fungible_vault_entry_definition_id, is_deleted) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleVaultEntryDefinitionId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
        });
}
