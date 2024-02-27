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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct AccountResourcePreferenceRuleDbLookup(long AccountEntityId, long ResourceEntityId);

internal record struct AccountResourcePreferenceRuleChangePointerLookup(long AccountEntityId, long ResourceEntityId, long StateVersion);

internal record AccountResourcePreferenceRuleChangePointer
{
    public List<CoreModel.AccountResourcePreferenceEntrySubstate> Entries { get; } = new();
}

internal class AccountResourcePreferenceRulesProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedEntityDictionary _referencedEntityDictionary;

    private readonly ChangeTracker<AccountResourcePreferenceRuleChangePointerLookup, AccountResourcePreferenceRuleChangePointer> _changes = new();

    private readonly Dictionary<long, AccountResourcePreferenceRuleAggregateHistory> _mostRecentAggregates = new();
    private readonly Dictionary<AccountResourcePreferenceRuleDbLookup, AccountResourcePreferenceRuleHistory> _mostRecentEntries = new();

    private readonly List<AccountResourcePreferenceRuleAggregateHistory> _accountResourcePreferenceAggregatesToAdd = new();
    private readonly List<AccountResourcePreferenceRuleHistory> _accountResourcePreferenceEntriesToAdd = new();

    public AccountResourcePreferenceRulesProcessor(ProcessorContext context, ReferencedEntityDictionary referencedEntityDictionary)
    {
        _context = context;
        _referencedEntityDictionary = referencedEntityDictionary;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.AccountResourcePreferenceEntrySubstate accountResourcePreferenceEntry)
        {
            _changes
                .GetOrAdd(
                    new AccountResourcePreferenceRuleChangePointerLookup(
                        referencedEntity.DatabaseId,
                        _referencedEntityDictionary.Get((EntityAddress)accountResourcePreferenceEntry.Key.ResourceAddress).DatabaseId,
                        stateVersion
                    ),
                    _ => new AccountResourcePreferenceRuleChangePointer())
                .Entries
                .Add(accountResourcePreferenceEntry);
        }
    }

    public void ProcessChanges()
    {
        foreach (var (lookup, change) in _changes.AsEnumerable())
        {
            AccountResourcePreferenceRuleAggregateHistory aggregate;

            if (!_mostRecentAggregates.TryGetValue(lookup.AccountEntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new AccountResourcePreferenceRuleAggregateHistory
                {
                    Id = _context.Sequences.AccountResourcePreferenceRuleAggregateHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    AccountEntityId = lookup.AccountEntityId,
                    EntryIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.EntryIds.AddRange(previousAggregate.EntryIds);
                }

                _accountResourcePreferenceAggregatesToAdd.Add(aggregate);
                _mostRecentAggregates[lookup.AccountEntityId] = aggregate;
            }
            else
            {
                aggregate = previousAggregate;
            }

            foreach (var entry in change.Entries)
            {
                var entryLookup = new AccountResourcePreferenceRuleDbLookup(lookup.AccountEntityId, lookup.ResourceEntityId);

                var entryHistory = new AccountResourcePreferenceRuleHistory
                {
                    Id = _context.Sequences.AccountResourcePreferenceRuleHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    AccountEntityId = lookup.AccountEntityId,
                    ResourceEntityId = lookup.ResourceEntityId,
                    AccountResourcePreferenceRule = entry.Value?.ResourcePreference.ToModel(),
                    IsDeleted = entry.Value == null,
                };

                _accountResourcePreferenceEntriesToAdd.Add(entryHistory);

                if (_mostRecentEntries.TryGetValue(entryLookup, out var previousEntry))
                {
                    var currentPosition = aggregate.EntryIds.IndexOf(previousEntry.Id);

                    if (currentPosition != -1)
                    {
                        aggregate.EntryIds.RemoveAt(currentPosition);
                    }
                }

                if (entry.Value != null)
                {
                    aggregate.EntryIds.Insert(0, entryHistory.Id);
                }

                _mostRecentEntries[entryLookup] = entryHistory;
            }
        }
    }

    public async Task LoadMostRecent()
    {
        _mostRecentEntries.AddRange(await MostRecentAccountResourcePreferenceRuleHistory());
        _mostRecentAggregates.AddRange(await MostRecentAccountResourcePreferenceRuleAggregateHistory());
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyAccountResourcePreferenceRuleHistory();
        rowsInserted += await CopyAccountResourcePreferenceRuleAggregateHistory();

        return rowsInserted;
    }

    private async Task<IDictionary<AccountResourcePreferenceRuleDbLookup, AccountResourcePreferenceRuleHistory>> MostRecentAccountResourcePreferenceRuleHistory()
    {
        var lookupSet = new HashSet<AccountResourcePreferenceRuleDbLookup>();

        foreach (var (lookup, change) in _changes.AsEnumerable())
        {
            foreach (var entry in change.Entries)
            {
                lookupSet.Add(new AccountResourcePreferenceRuleDbLookup(lookup.AccountEntityId, lookup.ResourceEntityId));
            }
        }

        if (!lookupSet.Unzip(
                x => x.AccountEntityId,
                x => x.ResourceEntityId,
                out var accountEntityIds,
                out var resourceEntityIds))
        {
            return ImmutableDictionary<AccountResourcePreferenceRuleDbLookup, AccountResourcePreferenceRuleHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<AccountResourcePreferenceRuleDbLookup, AccountResourcePreferenceRuleHistory>(
            @$"
WITH variables (account_entity_id, resource_entity_id) AS (
    SELECT UNNEST({accountEntityIds}), UNNEST({resourceEntityIds})
)
SELECT arprh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM account_resource_preference_rule_history
    WHERE account_entity_id = variables.account_entity_id AND variables.resource_entity_id = resource_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) arprh ON true;",
            e => new AccountResourcePreferenceRuleDbLookup(e.AccountEntityId, e.ResourceEntityId));
    }

    private async Task<IDictionary<long, AccountResourcePreferenceRuleAggregateHistory>> MostRecentAccountResourcePreferenceRuleAggregateHistory()
    {
        var accountEntityId = _changes.Keys.Select(x => x.AccountEntityId).ToHashSet().ToList();

        if (!accountEntityId.Any())
        {
            return ImmutableDictionary<long, AccountResourcePreferenceRuleAggregateHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<long, AccountResourcePreferenceRuleAggregateHistory>(
            @$"
WITH variables (account_entity_id) AS (
    SELECT UNNEST({accountEntityId})
)
SELECT arprah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM account_resource_preference_rule_aggregate_history
    WHERE account_entity_id = variables.account_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) arprah ON true;",
            e => e.AccountEntityId);
    }

    private Task<int> CopyAccountResourcePreferenceRuleHistory() => _context.WriteHelper.Copy(
        _accountResourcePreferenceEntriesToAdd,
        "COPY account_resource_preference_rule_history (id, from_state_version, account_entity_id,  resource_entity_id, account_resource_preference_rule, is_deleted) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.ResourceEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountResourcePreferenceRule, "account_resource_preference_rule", token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
        });

    private Task<int> CopyAccountResourcePreferenceRuleAggregateHistory() => _context.WriteHelper.Copy(
        _accountResourcePreferenceAggregatesToAdd,
        "COPY account_resource_preference_rule_aggregate_history (id, from_state_version, account_entity_id, entry_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntryIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });
}
