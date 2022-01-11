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

using Common.Database.Models.Ledger;
using Common.Database.Models.Ledger.History;
using Common.Database.Models.Ledger.Normalization;
using Common.Database.Models.Ledger.Substates;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using NpgsqlTypes;
using System.Linq.Expressions;

namespace Common.Database;

public static class DbQueryExtensions
{
    public static IQueryable<Account> Account<TDbContext>(
        this TDbContext dbContext,
        string accountAddress,
        long stateVersion
    )
        where TDbContext : CommonDbContext
    {
        return dbContext.Set<Account>()
            .Where(a => a.Address == accountAddress && a.FromStateVersion <= stateVersion);
    }

    public static IQueryable<Resource> Resource<TDbContext>(
        this TDbContext dbContext,
        string resourceIdentifier,
        long stateVersion
    )
        where TDbContext : CommonDbContext
    {
        return dbContext.Set<Resource>()
            .Where(r => r.ResourceIdentifier == resourceIdentifier && r.FromStateVersion <= stateVersion);
    }

    public static IQueryable<Validator> Validator<TDbContext>(
        this TDbContext dbContext,
        string validatorAddress,
        long stateVersion
    )
        where TDbContext : CommonDbContext
    {
        return dbContext.Set<Validator>()
            .Where(v => v.Address == validatorAddress && v.FromStateVersion <= stateVersion);
    }

    public static IQueryable<LedgerTransaction> GetTopLedgerTransaction<TDbContext>(this TDbContext dbContext)
        where TDbContext : CommonDbContext
    {
        return dbContext.LedgerStatus
            .Select(lt => lt.TopOfLedgerTransaction);
    }

    public static IQueryable<LedgerTransaction> GetLatestLedgerTransactionBeforeStateVersion<TDbContext>(this TDbContext dbContext, long beforeStateVersion)
        where TDbContext : CommonDbContext
    {
        return dbContext.LedgerTransactions
            .Where(lt => lt.ResultantStateVersion <= beforeStateVersion)
            .OrderByDescending(lt => lt.ResultantStateVersion)
            .Take(1);
    }

    public static IQueryable<LedgerTransaction> GetLatestLedgerTransactionBeforeTimestamp<TDbContext>(this TDbContext dbContext, Instant timestamp)
        where TDbContext : CommonDbContext
    {
        return dbContext.LedgerTransactions
            .Where(lt => lt.RoundTimestamp <= timestamp)
            .OrderByDescending(lt => lt.RoundTimestamp)
            .ThenByDescending(lt => lt.ResultantStateVersion)
            .Take(1);
    }

    public static IQueryable<LedgerTransaction> GetLatestLedgerTransactionAtEpochRound<TDbContext>(this TDbContext dbContext, long epoch, long round)
        where TDbContext : CommonDbContext
    {
        return dbContext.LedgerTransactions
            .Where(lt => lt.Epoch == epoch && lt.RoundInEpoch <= round && lt.IsStartOfRound)
            .OrderByDescending(lt => lt.ResultantStateVersion)
            .Take(1);
    }

    public static IQueryable<TSubstate> UpAtVersion<TSubstate>(
        this DbSet<TSubstate> dbSet,
        long stateVersion
    )
        where TSubstate : SubstateBase
    {
        // This could be re-written to take the top upStateVersion based on some key; which might make better use
        // of the indices
        return dbSet.
            Where(s => s.UpStateVersion <= stateVersion && (s.DownStateVersion == null || s.DownStateVersion > stateVersion));
    }

    public static IQueryable<THistory> GetSingleHistoryEntryAtVersion<THistory>(
        this DbSet<THistory> dbSet,
        Expression<Func<THistory, bool>> keySelector,
        long stateVersion
    )
        where THistory : HistoryBase
    {
        return dbSet
            .Where(keySelector)
            .Where(h => h.FromStateVersion <= stateVersion)
            .OrderByDescending(h => h.FromStateVersion)
            .Take(1);
    }

    /* TODO:NG-39 - Check all these history queries (when filtered further to a sub-part of the key)
     * result in sensible query plans which make use of the indexes we added
     */
    public static IQueryable<AccountResourceBalanceHistory> AccountResourceBalanceHistoryAtVersion<TDbContext>(
        this TDbContext dbContext,
        long stateVersion
    )
        where TDbContext : CommonDbContext
    {
        var dbSet = dbContext.Set<AccountResourceBalanceHistory>();
        var mostRecentHistoryByKeyQuery =
            from history in dbSet
            where history.FromStateVersion <= stateVersion
            group history by new { history.AccountId, history.ResourceId }
            into g
            select new
            {
                AccountId = g.Key.AccountId,
                ResourceId = g.Key.ResourceId,
                LatestUpdateStateVersion = g.Select(h => h.FromStateVersion).Max(),
            }
        ;

        return
            from fullHistory in dbSet
            join historyKeys in mostRecentHistoryByKeyQuery
                on new { fullHistory.AccountId, fullHistory.ResourceId, fullHistory.FromStateVersion }
                equals new { historyKeys.AccountId, historyKeys.ResourceId, FromStateVersion = historyKeys.LatestUpdateStateVersion }
            select fullHistory
        ;
    }

    public record AccountValidatorIds(long AccountId, long ValidatorId);

    public static IQueryable<AccountValidatorStakeHistory> BulkAccountValidatorStakeHistoryAtVersion<TDbContext>(
        this TDbContext dbContext,
        List<AccountValidatorIds> accountValidatorIds,
        long stateVersion
    )
        where TDbContext : CommonDbContext
    {
        /*
         * Performance Notes:
         *
         * This was chosen as the best query structure by comparing on mainnet (for ~200 validators) with other choices for queries:
         * - INNER JOIN LATERAL - 2ms Execution
         * - JOIN against GROUP BY with MAX - 200ms Execution
         * - Using variants of PARTITION BY - 750ms-1s Execution
         */

        var accountIdsParameter = new NpgsqlParameter("@account_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
             { Value = accountValidatorIds.Select(av => av.AccountId).ToList() };
        var validatorIdsParameter = new NpgsqlParameter("@validator_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
            { Value = accountValidatorIds.Select(av => av.ValidatorId).ToList() };
        var stateVersionParameter = new NpgsqlParameter("@state_version", NpgsqlDbType.Bigint)
            { Value = stateVersion };

        // NB - UNNEST can be used to zip arrays together
        return dbContext.Set<AccountValidatorStakeHistory>()
            .FromSqlInterpolated($@"
SELECT h.*
FROM UNNEST({accountIdsParameter}, {validatorIdsParameter}) av (account_id, validator_id)
INNER JOIN LATERAL (
    SELECT
        h0.*
    FROM account_validator_stake_history h0
	WHERE
		h0.account_id = av.account_id AND
		h0.validator_id = av.validator_id AND
		h0.from_state_version <= {stateVersionParameter}
	ORDER BY h0.from_state_version DESC
	LIMIT 1
) h ON (true)
");
    }

    public static IQueryable<AccountValidatorStakeHistory> PossiblySlowGroupedAccountValidatorStakeHistoryAtVersion<TDbContext>(
        this TDbContext dbContext,
        long stateVersion
    )
        where TDbContext : CommonDbContext
    {
        var dbSet = dbContext.Set<AccountValidatorStakeHistory>();
        var mostRecentHistoryByKeyQuery =
                from history in dbSet
                where history.FromStateVersion <= stateVersion
                group history by new { history.AccountId, history.ValidatorId }
                into g
                select new
                {
                    AccountId = g.Key.AccountId,
                    ValidatorId = g.Key.ValidatorId,
                    LatestUpdateStateVersion = g.Select(h => h.FromStateVersion).Max(),
                }
            ;

        return
            from fullHistory in dbSet
            join historyKeys in mostRecentHistoryByKeyQuery
                on new { fullHistory.AccountId, fullHistory.ValidatorId, fullHistory.FromStateVersion }
                equals new { historyKeys.AccountId, historyKeys.ValidatorId, FromStateVersion = historyKeys.LatestUpdateStateVersion }
            select fullHistory
            ;
    }

    public static IQueryable<ValidatorStakeHistory> ValidatorStakeHistoryAtVersion<TDbContext>(
        this TDbContext dbContext,
        long stateVersion
    )
        where TDbContext : CommonDbContext
    {
        var dbSet = dbContext.Set<ValidatorStakeHistory>();
        var mostRecentHistoryByKeyQuery =
            from history in dbSet
            where history.FromStateVersion <= stateVersion
            group history by history.ValidatorId
            into g
            select new
            {
                ValidatorId = g.Key,
                LatestUpdateStateVersion = g.Select(h => h.FromStateVersion).Max(),
            }
        ;

        return
            from fullHistory in dbSet
            join historyKeys in mostRecentHistoryByKeyQuery
                on new { fullHistory.ValidatorId, fullHistory.FromStateVersion }
                equals new { historyKeys.ValidatorId, FromStateVersion = historyKeys.LatestUpdateStateVersion }
            select fullHistory
        ;
    }

    public static IQueryable<ResourceSupplyHistory> ResourceSupplyHistoryAtVersionForRri<TDbContext>(
        this TDbContext dbContext,
        long stateVersion,
        string rri
    )
        where TDbContext : CommonDbContext
    {
        return dbContext.Set<ResourceSupplyHistory>()
            .Where(h =>
                h.ResourceId == dbContext.Resource(rri, stateVersion)
                    .Select(r => r.Id)
                    .FirstOrDefault() // This is actually done in a sub-query server-side.
                && h.FromStateVersion <= stateVersion
            )
            .OrderByDescending(h => h.FromStateVersion)
            .Take(1);

        // NB - other options considered instead of the sub query:
        // * Using group by from SlowGroupedResourceSupplyHistoryAtVersion is too slow because it doesn't use the indexes :(
        // * Using Lateral Join (code below) doesn't work due to being too slow https://github.com/dotnet/efcore/issues/17936
        // return
        //     from resource in dbContext.Resource(rri, stateVersion)
        //     from supplyHistory in dbContext.Set<ResourceSupplyHistory>()
        //         .Where(h => h.ResourceId == resource.Id && h.FromStateVersion <= stateVersion)
        //         .OrderByDescending(h => h.FromStateVersion)
        //         .Take(1)
        //     select supplyHistory;
        // * This variant of the lateral join doesn't work because dbContext.ResourceSupplyHistoryFromResourceIdAtVersion
        //   doesn't return an IQueryable with a parametrised expression (by resource.Id, say)
        // return
        //     from resource in dbContext.Resource(rri, stateVersion)
        //     from supplyHistory in dbContext.ResourceSupplyHistoryFromResourceIdAtVersion(resource.Id, stateVersion)
        //     select supplyHistory;
    }

    public static IQueryable<ResourceSupplyHistory> ResourceSupplyHistoryFromResourceIdAtVersion<TDbContext>(
        this TDbContext dbContext,
        long resourceId,
        long stateVersion
    )
        where TDbContext : CommonDbContext
    {
        return dbContext.Set<ResourceSupplyHistory>()
            .Where(h => h.ResourceId == resourceId && h.FromStateVersion <= stateVersion)
            .OrderByDescending(h => h.FromStateVersion)
            .Take(1);
    }

    public static IQueryable<ResourceSupplyHistory> SlowGroupedResourceSupplyHistoryAtVersion<TDbContext>(
        this TDbContext dbContext,
        long stateVersion
    )
        where TDbContext : CommonDbContext
    {
        var dbSet = dbContext.Set<ResourceSupplyHistory>();
        var mostRecentHistoryByKeyQuery =
            from history in dbSet
            where history.FromStateVersion <= stateVersion
            group history by history.ResourceId
            into g
            select new
            {
                ResourceId = g.Key,
                LatestUpdateStateVersion = g.Select(h => h.FromStateVersion).Max(),
            }
        ;

        return
            from fullHistory in dbSet
            join historyKeys in mostRecentHistoryByKeyQuery
                on new { fullHistory.ResourceId, fullHistory.FromStateVersion }
                equals new { historyKeys.ResourceId, FromStateVersion = historyKeys.LatestUpdateStateVersion }
            select fullHistory
        ;
    }
}
