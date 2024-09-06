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
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;
using GatewayModel = RadixDlt.NetworkGateway.GatewayApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct AccountLockerEntryDbLookup(long LockerEntityId, long AccountEntityId);

internal class AccountLockerProcessor
{
    private record ObservedTouch(long AccountLockerEntityId, long AccountEntityId, long StateVersion);

    private record ObservedVault(long AccountLockerEntityId, long AccountEntityId, long StateVersion, EntityAddress ResourceAddress, EntityAddress VaultAddress);

    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntities;

    private readonly HashSet<ObservedTouch> _observedTouchHistory = new();
    private readonly List<ObservedVault> _observedVaultDefinitions = new();
    private readonly Dictionary<AccountLockerEntryDbLookup, AccountLockerEntryDefinition> _existingEntryDefinitions = new();

    private readonly List<AccountLockerEntryDefinition> _definitionsToAdd = new();
    private readonly List<AccountLockerEntryResourceVaultDefinition> _resourceVaultDefinitionsToAdd = new();
    private readonly List<AccountLockerEntryTouchHistory> _touchHistoryToAdd = new();

    public AccountLockerProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntities)
    {
        _context = context;
        _referencedEntities = referencedEntities;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.AccountLockerAccountClaimsEntrySubstate accountLocker)
        {
            var account = _referencedEntities.Get((EntityAddress)accountLocker.Key.AccountAddress);
            var keyValueStore = _referencedEntities.Get((EntityAddress)accountLocker.Value.ResourceVaults.EntityAddress);

            _definitionsToAdd.Add(new AccountLockerEntryDefinition
            {
                Id = _context.Sequences.AccountLockerEntryDefinitionSequence++,
                FromStateVersion = stateVersion,
                AccountLockerEntityId = referencedEntity.DatabaseId,
                AccountEntityId = account.DatabaseId,
                KeyValueStoreEntityId = keyValueStore.DatabaseId,
            });
        }

        if (substateData is CoreModel.GenericKeyValueStoreEntrySubstate keyValueStoreEntry)
        {
            var kvse = referencedEntity.GetDatabaseEntity<InternalKeyValueStoreEntity>();

            if (kvse.TryGetAccountLockerEntryDbLookup(out var lookup))
            {
                var sborKey = ScryptoSborUtils.DataToProgrammaticJson(keyValueStoreEntry.Key.KeyData.GetDataBytes(), _context.NetworkConfiguration.Id);
                var sborValue = ScryptoSborUtils.DataToProgrammaticJson(keyValueStoreEntry.Value.Data.StructData.GetDataBytes(), _context.NetworkConfiguration.Id);

                if (sborKey is not GatewayModel.ProgrammaticScryptoSborValueReference resource)
                {
                    throw new UnreachableException("Unable to parse AccountLocker-related KVStore entry key as SBOR Reference");
                }

                if (sborValue is not GatewayModel.ProgrammaticScryptoSborValueOwn vault)
                {
                    throw new UnreachableException("Unable to parse AccountLocker-related KVStore entry value as SBOR Own");
                }

                // while neither resource address nor vault address have been explicitly loaded from the database based on above SBOR-encoded values during initial ingestion phase
                // it is believed that their addresses must be mentioned in other transaction structures actually scanned for entity addresses
                _observedVaultDefinitions.Add(new ObservedVault(lookup.LockerEntityId, lookup.AccountEntityId, stateVersion, (EntityAddress)resource.Value, (EntityAddress)vault.Value));
                _observedTouchHistory.Add(new ObservedTouch(lookup.LockerEntityId, lookup.AccountEntityId, stateVersion));
            }
        }

        if (substateData is CoreModel.FungibleVaultFieldBalanceSubstate or CoreModel.NonFungibleVaultFieldBalanceSubstate)
        {
            var ve = referencedEntity.GetDatabaseEntity<VaultEntity>();

            if (ve.TryGetAccountLockerEntryDbLookup(out var lookup))
            {
                _observedTouchHistory.Add(new ObservedTouch(lookup.LockerEntityId, lookup.AccountEntityId, stateVersion));
            }
        }
    }

    public async Task LoadDependencies()
    {
        _existingEntryDefinitions.AddRange(await ExistingAccountLockerEntryDefinitions());
    }

    public void ProcessChanges()
    {
        _existingEntryDefinitions.AddRange(_definitionsToAdd.ToDictionary(e => new AccountLockerEntryDbLookup(e.AccountLockerEntityId, e.AccountEntityId)));
        _resourceVaultDefinitionsToAdd.AddRange(_observedVaultDefinitions.Select(rv => new AccountLockerEntryResourceVaultDefinition
        {
            Id = _context.Sequences.AccountLockerEntryResourceVaultDefinitionSequence++,
            FromStateVersion = rv.StateVersion,
            AccountLockerDefinitionId = _existingEntryDefinitions[new AccountLockerEntryDbLookup(rv.AccountLockerEntityId, rv.AccountEntityId)].Id,
            ResourceEntityId = _referencedEntities.Get(rv.ResourceAddress).DatabaseId,
            VaultEntityId = _referencedEntities.Get(rv.VaultAddress).DatabaseId,
        }));
        _touchHistoryToAdd.AddRange(_observedTouchHistory.Select(th => new AccountLockerEntryTouchHistory
        {
            Id = _context.Sequences.AccountLockerEntryTouchHistorySequence++,
            FromStateVersion = th.StateVersion,
            AccountLockerDefinitionId = _existingEntryDefinitions[new AccountLockerEntryDbLookup(th.AccountLockerEntityId, th.AccountEntityId)].Id,
        }));
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyAccountLockerEntryDefinition();
        rowsInserted += await CopyAccountLockerEntryResourceVaultDefinition();
        rowsInserted += await CopyAccountLockerEntryTouchHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<AccountLockerEntryDbLookup, AccountLockerEntryDefinition>> ExistingAccountLockerEntryDefinitions()
    {
        if (!_observedTouchHistory.Unzip(x => x.AccountLockerEntityId, x => x.AccountEntityId, out var accountLockerIds, out var accountIds))
        {
            return ImmutableDictionary<AccountLockerEntryDbLookup, AccountLockerEntryDefinition>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<AccountLockerEntryDbLookup, AccountLockerEntryDefinition>(
            @$"
WITH variables (account_locker_entity_id, account_entity_id) AS (
    SELECT UNNEST({accountLockerIds}), UNNEST({accountIds})
)
SELECT *
FROM account_locker_entry_definition
WHERE (account_locker_entity_id, account_entity_id) IN (SELECT * FROM variables)",
            e => new AccountLockerEntryDbLookup(e.AccountLockerEntityId, e.AccountEntityId));
    }

    private Task<int> CopyAccountLockerEntryDefinition() => _context.WriteHelper.Copy(
        _definitionsToAdd,
        "COPY account_locker_entry_definition (id, from_state_version, account_locker_entity_id, account_entity_id, key_value_store_entity_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountLockerEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.KeyValueStoreEntityId, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyAccountLockerEntryResourceVaultDefinition() => _context.WriteHelper.Copy(
        _resourceVaultDefinitionsToAdd,
        "COPY account_locker_entry_resource_vault_definition (id, from_state_version, account_locker_definition_id, resource_entity_id, vault_entity_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountLockerDefinitionId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.VaultEntityId, NpgsqlDbType.Bigint, token);
        });

    private Task<int> CopyAccountLockerEntryTouchHistory() => _context.WriteHelper.Copy(
        _touchHistoryToAdd,
        "COPY account_locker_entry_touch_history (id, from_state_version, account_locker_definition_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountLockerDefinitionId, NpgsqlDbType.Bigint, token);
        });
}
