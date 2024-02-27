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
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.PostgresIntegration.LedgerExtension;

internal record struct AccountAuthorizedDepositorsDbLookup(long AccountEntityId, long ResourceEntityId, long? NonFungibleIdDataId);

internal record struct AccountAuthorizedDepositorsChangePointerLookup(long AccountEntityId, long StateVersion);

internal record AccountAuthorizedDepositorsChangePointer
{
    public List<CoreModel.AccountAuthorizedDepositorEntrySubstate> AuthorizedDepositorEntries { get; } = new();
}

internal class AccountAuthorizedDepositorsProcessor
{
    private readonly ProcessorContext _context;
    private readonly ReferencedNonFungibleIdDictionary _referencedNonFungibleIdDictionary;
    private readonly ReferencedEntityDictionary _referencedEntityDictionary;

    private readonly ChangeTracker<AccountAuthorizedDepositorsChangePointerLookup, AccountAuthorizedDepositorsChangePointer> _accountAuthorizedDepositorsChanges = new();

    private readonly Dictionary<long, AccountAuthorizedDepositorAggregateHistory> _mostRecentAuthorizedDepositorAggregates = new();
    private readonly Dictionary<AccountAuthorizedDepositorsDbLookup, AccountAuthorizedDepositorHistory> _mostRecentAuthorizedDepositorEntries = new();

    private readonly List<AccountAuthorizedDepositorAggregateHistory> _accountAuthorizedDepositorAggregatesToAdd = new();
    private readonly List<AccountAuthorizedDepositorHistory> _accountAuthorizedDepositorHistoryToAdd = new();

    public AccountAuthorizedDepositorsProcessor(
        ProcessorContext context,
        ReferencedNonFungibleIdDictionary referencedNonFungibleIdDictionary,
        ReferencedEntityDictionary referencedEntityDictionary)
    {
        _context = context;
        _referencedNonFungibleIdDictionary = referencedNonFungibleIdDictionary;
        _referencedEntityDictionary = referencedEntityDictionary;
    }

    public void VisitUpsert(CoreModel.Substate substateData, ReferencedEntity referencedEntity, long stateVersion)
    {
        if (substateData is CoreModel.AccountAuthorizedDepositorEntrySubstate accountAuthorizedDepositorEntry)
        {
            _accountAuthorizedDepositorsChanges
                .GetOrAdd(
                    new AccountAuthorizedDepositorsChangePointerLookup(referencedEntity.DatabaseId, stateVersion),
                    _ => new AccountAuthorizedDepositorsChangePointer())
                .AuthorizedDepositorEntries
                .Add(accountAuthorizedDepositorEntry);
        }
    }

    public void ProcessChanges()
    {
        foreach (var (lookup, change) in _accountAuthorizedDepositorsChanges.AsEnumerable())
        {
            AccountAuthorizedDepositorAggregateHistory aggregate;

            if (!_mostRecentAuthorizedDepositorAggregates.TryGetValue(lookup.AccountEntityId, out var previousAggregate) || previousAggregate.FromStateVersion != lookup.StateVersion)
            {
                aggregate = new AccountAuthorizedDepositorAggregateHistory
                {
                    Id = _context.Sequences.AccountAuthorizedDepositorAggregateHistorySequence++,
                    FromStateVersion = lookup.StateVersion,
                    AccountEntityId = lookup.AccountEntityId,
                    EntryIds = new List<long>(),
                };

                if (previousAggregate != null)
                {
                    aggregate.EntryIds.AddRange(previousAggregate.EntryIds);
                }

                _accountAuthorizedDepositorAggregatesToAdd.Add(aggregate);
                _mostRecentAuthorizedDepositorAggregates[lookup.AccountEntityId] = aggregate;
            }
            else
            {
                aggregate = previousAggregate;
            }

            foreach (var authorizedDepositorEntry in change.AuthorizedDepositorEntries)
            {
                AccountAuthorizedDepositorHistory entryHistory;
                AccountAuthorizedDepositorsDbLookup entryDbLookup = GetDbLookup(authorizedDepositorEntry, lookup);

                if (authorizedDepositorEntry.Key.Badge is CoreModel.ResourceAuthorizedDepositorBadge resourceBadge)
                {
                    var resourceEntityId = _referencedEntityDictionary.Get((EntityAddress)resourceBadge.ResourceAddress).DatabaseId;
                    entryHistory = new AccountAuthorizedResourceBadgeDepositorHistory
                    {
                        Id = _context.Sequences.AccountAuthorizedDepositorHistorySequence++,
                        FromStateVersion = lookup.StateVersion,
                        AccountEntityId = lookup.AccountEntityId,
                        ResourceEntityId = resourceEntityId,
                        IsDeleted = authorizedDepositorEntry.Value == null,
                    };
                }
                else if (authorizedDepositorEntry.Key.Badge is CoreModel.NonFungibleAuthorizedDepositorBadge nonFungibleBadge)
                {
                    var globalIdDatabaseId = _referencedNonFungibleIdDictionary.Get(
                        new NonFungibleGlobalIdLookup(
                            (EntityAddress)nonFungibleBadge.NonFungibleGlobalId.ResourceAddress,
                            nonFungibleBadge.NonFungibleGlobalId.LocalId.SimpleRep)
                    );

                    entryHistory = new AccountAuthorizedNonFungibleBadgeDepositorHistory
                    {
                        Id = _context.Sequences.AccountAuthorizedDepositorHistorySequence++,
                        FromStateVersion = lookup.StateVersion,
                        AccountEntityId = lookup.AccountEntityId,
                        NonFungibleResourceEntityId = globalIdDatabaseId.ResourceEntityId,
                        NonFungibleIdDataId = globalIdDatabaseId.NonFungibleIdDataId,
                        IsDeleted = authorizedDepositorEntry.Value == null,
                    };
                }
                else
                {
                    throw new UnreachableException(
                        $"Expected either ResourceAuthorizedDepositorBadge or NonFungibleGlobalAuthorizedDepositorBadge but found {authorizedDepositorEntry.Key.Badge.GetType()}");
                }

                _accountAuthorizedDepositorHistoryToAdd.Add(entryHistory);

                if (_mostRecentAuthorizedDepositorEntries.TryGetValue(entryDbLookup, out var previousEntry))
                {
                    var currentPosition = aggregate.EntryIds.IndexOf(previousEntry.Id);

                    if (currentPosition != -1)
                    {
                        aggregate.EntryIds.RemoveAt(currentPosition);
                    }
                }

                if (authorizedDepositorEntry.Value != null)
                {
                    aggregate.EntryIds.Insert(0, entryHistory.Id);
                }

                _mostRecentAuthorizedDepositorEntries[entryDbLookup] = entryHistory;
            }
        }
    }

    public async Task LoadMostRecent()
    {
        _mostRecentAuthorizedDepositorEntries.AddRange(await MostRecentAccountAuthorizedDepositorHistory());
        _mostRecentAuthorizedDepositorAggregates.AddRange(await MostRecentAccountAuthorizedDepositorAggregateHistory());
    }

    public async Task<int> SaveEntities()
    {
        var rowsInserted = 0;

        rowsInserted += await CopyAccountAuthorizedDepositorHistory();
        rowsInserted += await CopyAccountAuthorizedDepositorAggregateHistory();

        return rowsInserted;
    }

    private AccountAuthorizedDepositorsDbLookup GetDbLookup(
        CoreModel.AccountAuthorizedDepositorEntrySubstate accountAuthorizedDepositorEntrySubstate,
        AccountAuthorizedDepositorsChangePointerLookup lookup
    )
    {
        AccountAuthorizedDepositorsDbLookup entryLookup;

        switch (accountAuthorizedDepositorEntrySubstate.Key.Badge)
        {
            case CoreModel.ResourceAuthorizedDepositorBadge resourceBadge:
            {
                var resourceEntityId = _referencedEntityDictionary.Get((EntityAddress)resourceBadge.ResourceAddress).DatabaseId;

                entryLookup = new AccountAuthorizedDepositorsDbLookup(
                    AccountEntityId: lookup.AccountEntityId,
                    ResourceEntityId: resourceEntityId,
                    NonFungibleIdDataId: null
                );
                break;
            }

            case CoreModel.NonFungibleAuthorizedDepositorBadge nonFungibleBadge:
            {
                var globalIdDatabaseId = _referencedNonFungibleIdDictionary.Get(
                    new NonFungibleGlobalIdLookup(
                        (EntityAddress)nonFungibleBadge.NonFungibleGlobalId.ResourceAddress,
                        nonFungibleBadge.NonFungibleGlobalId.LocalId.SimpleRep)
                );

                entryLookup = new AccountAuthorizedDepositorsDbLookup(
                    AccountEntityId: lookup.AccountEntityId,
                    ResourceEntityId: globalIdDatabaseId.ResourceEntityId,
                    NonFungibleIdDataId: globalIdDatabaseId.NonFungibleIdDataId
                );
                break;
            }

            default:
                throw new UnreachableException(
                    $"Expected either ResourceAuthorizedDepositorBadge or NonFungibleGlobalAuthorizedDepositorBadge but found {accountAuthorizedDepositorEntrySubstate.Key.Badge.GetType()}");
        }

        return entryLookup;
    }

    private async Task<IDictionary<AccountAuthorizedDepositorsDbLookup, AccountAuthorizedDepositorHistory>> MostRecentAccountAuthorizedDepositorHistory()
    {
        var lookupSet = new HashSet<AccountAuthorizedDepositorsDbLookup>();

        foreach (var (lookup, change) in _accountAuthorizedDepositorsChanges.AsEnumerable())
        {
            foreach (var entry in change.AuthorizedDepositorEntries)
            {
                lookupSet.Add(GetDbLookup(entry, lookup));
            }
        }

        var entityIds = lookupSet.Select(x => x.AccountEntityId).ToList();
        var resourceEntityIds = lookupSet.Select(x => x.ResourceEntityId).ToList();
        var nonFungibleIdDataIds = lookupSet.Select(x => x.NonFungibleIdDataId).ToList();

        return await _context.ReadHelper.LoadDependencies<AccountAuthorizedDepositorsDbLookup, AccountAuthorizedDepositorHistory>(
            @$"
WITH variables (account_entity_id, resource_entity_id, non_fungible_id_data_id) AS (
    SELECT UNNEST({entityIds}), UNNEST({resourceEntityIds}), UNNEST({nonFungibleIdDataIds})
)
SELECT aadh.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM account_authorized_depositor_history
    WHERE account_entity_id = variables.account_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) aadh ON true;",
            e =>
            {
                return e switch
                {
                    AccountAuthorizedNonFungibleBadgeDepositorHistory accountAuthorizedNonFungibleBadgeDepositorHistory
                        => new AccountAuthorizedDepositorsDbLookup(
                            e.AccountEntityId,
                            accountAuthorizedNonFungibleBadgeDepositorHistory.NonFungibleResourceEntityId,
                            accountAuthorizedNonFungibleBadgeDepositorHistory.NonFungibleIdDataId),
                    AccountAuthorizedResourceBadgeDepositorHistory accountAuthorizedResourceBadgeDepositorHistory
                        => new AccountAuthorizedDepositorsDbLookup(
                            e.AccountEntityId,
                            accountAuthorizedResourceBadgeDepositorHistory.ResourceEntityId,
                            null),
                    _ => throw new UnreachableException($"Not supported depositor type: {e.GetType()}"),
                };
            });
    }

    private async Task<IDictionary<long, AccountAuthorizedDepositorAggregateHistory>> MostRecentAccountAuthorizedDepositorAggregateHistory()
    {
        var accountEntityId = _accountAuthorizedDepositorsChanges.Keys.Select(x => x.AccountEntityId).ToHashSet().ToList();

        if (!accountEntityId.Any())
        {
            return ImmutableDictionary<long, AccountAuthorizedDepositorAggregateHistory>.Empty;
        }

        return await _context.ReadHelper.LoadDependencies<long, AccountAuthorizedDepositorAggregateHistory>(
            @$"
WITH variables (account_entity_id) AS (
    SELECT UNNEST({accountEntityId})
)
SELECT aadah.*
FROM variables
INNER JOIN LATERAL (
    SELECT *
    FROM account_authorized_depositor_aggregate_history
    WHERE account_entity_id = variables.account_entity_id
    ORDER BY from_state_version DESC
    LIMIT 1
) aadah ON true;",
            e => e.AccountEntityId);
    }

    private Task<int> CopyAccountAuthorizedDepositorHistory() => _context.WriteHelper.Copy(
        _accountAuthorizedDepositorHistoryToAdd,
        "COPY account_authorized_depositor_history (id, from_state_version, account_entity_id, is_deleted, discriminator, non_fungible_resource_entity_id, non_fungible_id_data_id, resource_entity_id) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            var discriminator = _context.WriteHelper.GetDiscriminator<AuthorizedDepositorBadgeType>(e.GetType());

            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.IsDeleted, NpgsqlDbType.Boolean, token);
            await writer.WriteAsync(discriminator, "authorized_depositor_badge_type", token);

            switch (e)
            {
                case AccountAuthorizedNonFungibleBadgeDepositorHistory accountAuthorizedNonFungibleBadgeDepositorHistory:
                    await writer.WriteAsync(accountAuthorizedNonFungibleBadgeDepositorHistory.NonFungibleResourceEntityId, NpgsqlDbType.Bigint, token);
                    await writer.WriteAsync(accountAuthorizedNonFungibleBadgeDepositorHistory.NonFungibleIdDataId, NpgsqlDbType.Bigint, token);
                    await writer.WriteNullAsync(token);
                    break;
                case AccountAuthorizedResourceBadgeDepositorHistory accountAuthorizedResourceBadgeDepositorHistory:
                    await writer.WriteNullAsync(token);
                    await writer.WriteNullAsync(token);
                    await writer.WriteAsync(accountAuthorizedResourceBadgeDepositorHistory.ResourceEntityId, NpgsqlDbType.Bigint, token);
                    break;
            }
        });

    private Task<int> CopyAccountAuthorizedDepositorAggregateHistory() => _context.WriteHelper.Copy(
        _accountAuthorizedDepositorAggregatesToAdd,
        "COPY account_authorized_depositor_aggregate_history (id, from_state_version, account_entity_id, entry_ids) FROM STDIN (FORMAT BINARY)",
        async (writer, e, token) =>
        {
            await writer.WriteAsync(e.Id, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.FromStateVersion, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.AccountEntityId, NpgsqlDbType.Bigint, token);
            await writer.WriteAsync(e.EntryIds.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Bigint, token);
        });
}
