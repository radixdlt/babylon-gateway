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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal readonly record struct VaultBalanceHistoryDbLookup(long VaultEntityId);

internal readonly record struct NonFungibleIdDefinitionDbLookup(long NonFungibleResourceEntityId, string NonFungibleId);

internal readonly record struct NonFungibleVaultEntryDefinitionDbLookup(long VaultEntityId, long NonFungibleIdDefinitionId);

internal class VaultProcessor
{
    private readonly record struct FungibleVaultSnapshot(VaultEntity VaultEntity, TokenAmount Balance, long StateVersion);

    private readonly record struct NonFungibleVaultChange(VaultEntity VaultEntity, string NonFungibleLocalId, bool IsDeposit, long StateVersion);

    private readonly ProcessorContext _context;

    private readonly List<FungibleVaultSnapshot> _observedFungibleVaultSnapshots = new();
    private readonly List<NonFungibleVaultChange> _observedNonFungibleVaultChanges = new();
    private readonly Dictionary<NonFungibleIdDefinitionDbLookup, long> _observedNonFungibleDataEntries = new();
    private readonly Dictionary<VaultBalanceHistoryDbLookup, VaultBalanceHistory> _mostRecentNonFungibleVaultBalanceHistory = new();
    private readonly Dictionary<NonFungibleIdDefinitionDbLookup, NonFungibleIdDefinition> _existingNonFungibleIdDefinitions = new();
    private readonly Dictionary<NonFungibleVaultEntryDefinitionDbLookup, NonFungibleVaultEntryDefinition> _existingNonFungibleVaultEntryDefinitions = new();
    private readonly List<VaultBalanceHistory> _balanceHistoryToAdd = new();
    private readonly List<NonFungibleIdDefinition> _nonFungibleIdDefinitionsToAdd = new();
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

            _observedFungibleVaultSnapshots.Add(new FungibleVaultSnapshot(vaultEntity, amount, stateVersion));
        }

        if (substateData is CoreModel.NonFungibleVaultContentsIndexEntrySubstate nonFungibleVaultContentsIndexEntrySubstate)
        {
            var vaultEntity = referencedEntity.GetDatabaseEntity<InternalNonFungibleVaultEntity>();
            var simpleRep = nonFungibleVaultContentsIndexEntrySubstate.Key.NonFungibleLocalId.SimpleRep;

            _observedNonFungibleVaultChanges.Add(new NonFungibleVaultChange(vaultEntity, simpleRep, true, stateVersion));
        }

        if (substateData is CoreModel.NonFungibleResourceManagerDataEntrySubstate)
        {
            var nonFungibleId = ScryptoSborUtils.GetNonFungibleId(((CoreModel.MapSubstateKey)substate.SubstateId.SubstateKey).KeyHex);

            _observedNonFungibleDataEntries.TryAdd(new NonFungibleIdDefinitionDbLookup(referencedEntity.DatabaseId, nonFungibleId), stateVersion);
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
        _mostRecentNonFungibleVaultBalanceHistory.AddRange(await MostRecentNonFungibleVaultBalanceHistory());
        _existingNonFungibleIdDefinitions.AddRange(await ExistingNonFungibleIdDefinitions());
        _existingNonFungibleVaultEntryDefinitions.AddRange(await ExistingNonFungibleVaultEntryDefinitions());
    }

    public void ProcessChanges()
    {
        _balanceHistoryToAdd.AddRange(_observedFungibleVaultSnapshots.Select(x => new VaultBalanceHistory
        {
            Id = _context.Sequences.VaultBalanceHistorySequence++,
            FromStateVersion = x.StateVersion,
            VaultEntityId = x.VaultEntity.Id,
            Balance = x.Balance,
        }));

        foreach (var (lookup, stateVersion) in _observedNonFungibleDataEntries)
        {
            _existingNonFungibleIdDefinitions.GetOrAdd(lookup, _ =>
            {
                var definition = new NonFungibleIdDefinition
                {
                    Id = _context.Sequences.NonFungibleIdDefinitionSequence++,
                    FromStateVersion = stateVersion,
                    NonFungibleResourceEntityId = lookup.NonFungibleResourceEntityId,
                    NonFungibleId = lookup.NonFungibleId,
                };

                _nonFungibleIdDefinitionsToAdd.Add(definition);

                return definition;
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

            VaultBalanceHistory balanceHistory;

            if (!_mostRecentNonFungibleVaultBalanceHistory.TryGetValue(new VaultBalanceHistoryDbLookup(change.VaultEntity.Id), out var previousBalanceHistory) || previousBalanceHistory.FromStateVersion != change.StateVersion)
            {
                balanceHistory = new VaultBalanceHistory
                {
                    Id = _context.Sequences.VaultBalanceHistorySequence++,
                    FromStateVersion = change.StateVersion,
                    VaultEntityId = change.VaultEntity.Id,
                    Balance = TokenAmount.Zero,
                };

                _balanceHistoryToAdd.Add(balanceHistory);
                _mostRecentNonFungibleVaultBalanceHistory[new VaultBalanceHistoryDbLookup(change.VaultEntity.Id)] = balanceHistory;
            }
            else
            {
                balanceHistory = previousBalanceHistory;
            }

            balanceHistory.Balance += change.IsDeposit ? TokenAmount.One : TokenAmount.MinusOne;

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

            _nonFungibleVaultEntryHistoryToAdd.Add(new NonFungibleVaultEntryHistory
            {
                Id = _context.Sequences.NonFungibleVaultEntryHistorySequence++,
                FromStateVersion = change.StateVersion,
                NonFungibleVaultEntryDefinitionId = nonFungibleVaultEntryDefinition.Id,
                IsDeleted = !change.IsDeposit,
            });
        }
    }

    public Dictionary<NonFungibleIdDefinitionDbLookup, NonFungibleIdDefinition> TempGetExistingNonFungibleIdDefinitions()
    {
        return _existingNonFungibleIdDefinitions;
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyVaultBalanceHistory();
        rowsInserted += await CopyNonFungibleIdDefinitions();
        rowsInserted += await CopyNonFungibleVaultEntryDefinitionsToAdd();
        rowsInserted += await CopyNonFungibleVaultEntryHistoryToAdd();

        return rowsInserted;
    }

    private async Task<IDictionary<VaultBalanceHistoryDbLookup, VaultBalanceHistory>> MostRecentNonFungibleVaultBalanceHistory()
    {
        var vaultEntityIds = _observedNonFungibleVaultChanges
            .Select(x => x.VaultEntity.Id)
            .ToHashSet()
            .ToList();

        if (!vaultEntityIds.Any())
        {
            return ImmutableDictionary<VaultBalanceHistoryDbLookup, VaultBalanceHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<VaultBalanceHistoryDbLookup, VaultBalanceHistory>(
            @$"
WITH variables (vault_entity_id) AS (
    SELECT unnest({vaultEntityIds})
)
SELECT th.*
FROM variables var
INNER JOIN LATERAL (
    SELECT *
    FROM vault_balance_history
    WHERE vault_entity_id = var.vault_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) th ON true;",
            e => new VaultBalanceHistoryDbLookup(e.VaultEntityId));
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
            if (_existingNonFungibleIdDefinitions.TryGetValue(new NonFungibleIdDefinitionDbLookup(change.VaultEntity.GetResourceEntityId(), change.NonFungibleLocalId), out var nonFungibleIdDefinition))
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

    private Task<int> CopyNonFungibleVaultEntryDefinitionsToAdd() => _context.WriteHelper.Copy(
        _nonFungibleVaultEntryDefinitionsToAdd,
        "COPY non_fungible_vault_entry_definition (id, from_state_version, vault_entity_id, non_fungible_id_definition_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.NonFungibleIdDefinitionId, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyNonFungibleVaultEntryHistoryToAdd() => _context.WriteHelper.Copy(
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
