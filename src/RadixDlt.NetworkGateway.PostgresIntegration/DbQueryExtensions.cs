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

using Microsoft.EntityFrameworkCore;
using RadixDlt.NetworkGateway.PostgresIntegration.Metrics;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RadixDlt.NetworkGateway.PostgresIntegration;

internal static class DbQueryExtensions
{
    /// <summary>
    /// Returns most recently committed ledger transaction.
    /// </summary>
    /// <remarks>
    /// A LedgerTransaction row contains large blobs, so you must SELECT the fields you need after using this, and not pull down the whole
    /// ledger transaction row, to avoid possible performance issues.
    /// </remarks>
    public static IQueryable<LedgerTransaction> GetTopLedgerTransaction<TDbContext>(this TDbContext dbContext)
        where TDbContext : CommonDbContext
    {
        return dbContext
            .LedgerTransactions
            .OrderByDescending(lt => lt.StateVersion)
            .Take(1)
            .AnnotateMetricName();
    }

    /// <summary>
    /// Returns ledger transaction committed at given state version.
    /// </summary>
    /// <remarks>
    /// A LedgerTransaction row contains large blobs, so you must SELECT the fields you need after using this, and not pull down the whole
    /// ledger transaction row, to avoid possible performance issues.
    /// </remarks>
    public static IQueryable<LedgerTransaction> GetLatestLedgerTransactionAtStateVersion<TDbContext>(this TDbContext dbContext, long stateVersion)
        where TDbContext : CommonDbContext
    {
        return dbContext
            .LedgerTransactions
            .Where(lt => lt.StateVersion == stateVersion)
            .Take(1)
            .AnnotateMetricName();
    }

    /// <summary>
    /// Returns ledger transaction committed at given epoch and round.
    /// </summary>
    /// <remarks>
    /// A LedgerTransaction row contains large blobs, so you must SELECT the fields you need after using this, and not pull down the whole
    /// ledger transaction row, to avoid possible performance issues.
    /// </remarks>
    public static IQueryable<LedgerTransaction> GetLedgerTransactionAtEpochAndRound<TDbContext>(this TDbContext dbContext, long epoch, long round)
        where TDbContext : CommonDbContext
    {
        return dbContext
            .LedgerTransactions
            .Where(lt => lt.Epoch == epoch && lt.RoundInEpoch == round && lt.IndexInRound == 0)
            .Take(1)
            .AnnotateMetricName();
    }

    /// <summary>
    /// Returns the very first ledger transaction committed at given epoch.
    /// </summary>
    /// <remarks>
    /// A LedgerTransaction row contains large blobs, so you must SELECT the fields you need after using this, and not pull down the whole
    /// ledger transaction row, to avoid possible performance issues.
    /// </remarks>
    public static IQueryable<LedgerTransaction> GetLedgerTransactionAtEpochStart<TDbContext>(this TDbContext dbContext, long epoch)
        where TDbContext : CommonDbContext
    {
        return dbContext
            .LedgerTransactions
            .Where(lt => lt.Epoch == epoch && lt.IndexInRound == 0)
            .OrderBy(lt => lt.RoundInEpoch)
            .Take(1)
            .AnnotateMetricName();
    }

    /// <summary>
    /// Returns most recently committed ledger transaction at or before given timestamp.
    /// </summary>
    /// <remarks>
    /// A LedgerTransaction row contains large blobs, so you must SELECT the fields you need after using this, and not pull down the whole
    /// ledger transaction row, to avoid possible performance issues.
    /// </remarks>
    public static IQueryable<LedgerTransaction> GetLatestLedgerTransactionBeforeTimestamp<TDbContext>(this TDbContext dbContext, DateTime timestamp)
        where TDbContext : CommonDbContext
    {
        return dbContext
            .LedgerTransactions
            .Where(lt => lt.RoundTimestamp <= timestamp)
            .OrderByDescending(lt => lt.RoundTimestamp)
            .ThenByDescending(lt => lt.StateVersion)
            .Take(1)
            .AnnotateMetricName();
    }

    /// <summary>
    /// Returns the first committed ledger transaction at or after given timestamp.
    /// </summary>
    /// <remarks>
    /// A LedgerTransaction row contains large blobs, so you must SELECT the fields you need after using this, and not pull down the whole
    /// ledger transaction row, to avoid possible performance issues.
    /// </remarks>
    public static IQueryable<LedgerTransaction> GetFirstLedgerTransactionAfterTimestamp<TDbContext>(this TDbContext dbContext, DateTime timestamp)
        where TDbContext : CommonDbContext
    {
        return dbContext
            .LedgerTransactions
            .Where(lt => lt.RoundTimestamp >= timestamp)
            .OrderBy(lt => lt.RoundTimestamp)
            .ThenBy(lt => lt.StateVersion)
            .Take(1)
            .AnnotateMetricName();
    }

    public static IQueryable<T> AnnotateMetricName<T>(
        this IQueryable<T> source,
        string operationName = "",
        [CallerMemberName] string methodName = "")
    {
        var queryNameTag = SqlQueryMetricsHelper.GenerateQueryNameTag(operationName, methodName);
        return source.TagWith(queryNameTag);
    }
}
