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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Prometheus;
using RadixDlt.NetworkGateway.Core.Database;
using RadixDlt.NetworkGateway.Core.Database.Models.Mempool;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface IMempoolPrunerService
{
    Task PruneMempool(CancellationToken token = default);
}

public class MempoolPrunerService : IMempoolPrunerService
{
    private static readonly Gauge _mempoolDbSizeByStatus = Metrics
        .CreateGauge(
            "ng_db_mempool_size_by_status_total",
            "Number of transactions currently tracked in the MempoolTransaction table, by status.",
            new GaugeConfiguration { LabelNames = new[] { "status" } }
        );

    private static readonly Counter _mempoolTransactionsPrunedCount = Metrics
        .CreateCounter(
            "ng_db_mempool_pruned_transactions_count",
            "Count of mempool transactions pruned from the DB"
        );

    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly IOptionsMonitor<MempoolOptions> _mempoolOptionsMonitor;
    private readonly ISystemStatusService _systemStatusService;
    private readonly ILogger<MempoolPrunerService> _logger;

    public MempoolPrunerService(
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        IOptionsMonitor<MempoolOptions> mempoolOptionsMonitor,
        ISystemStatusService systemStatusService,
        ILogger<MempoolPrunerService> logger
    )
    {
        _dbContextFactory = dbContextFactory;
        _mempoolOptionsMonitor = mempoolOptionsMonitor;
        _systemStatusService = systemStatusService;
        _logger = logger;
    }

    public async Task PruneMempool(CancellationToken token = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        await UpdateSizeMetrics(dbContext, token);

        var mempoolConfiguration = _mempoolOptionsMonitor.CurrentValue;

        var currTime = SystemClock.Instance.GetCurrentInstant();

        var pruneIfCommittedBefore = currTime.Minus(mempoolConfiguration.PruneCommittedAfter);
        var pruneIfLastGatewaySubmissionBefore = currTime.Minus(mempoolConfiguration.PruneMissingTransactionsAfterTimeSinceLastGatewaySubmission);
        var pruneIfFirstSeenBefore = currTime.Minus(mempoolConfiguration.PruneMissingTransactionsAfterTimeSinceFirstSeen);

        var pruneIfNotSeenSince = currTime.Minus(mempoolConfiguration.PruneRequiresMissingFromMempoolFor);

        var aggregatorIsSyncedUpEnoughToRemoveCommittedTransactions = _systemStatusService.GivenClockDriftBoundIsTopOfDbLedgerValidatorCommitTimestampConfidentlyAfter(
            mempoolConfiguration.AssumedBoundOnNetworkLedgerDataAggregatorClockDrift,
            pruneIfCommittedBefore
        );

        var transactionsToPrune = await dbContext.MempoolTransactions
            .Where(mt =>
                (
                    /* For committed transactions, remove from the mempool if we're synced up (as a committed transaction will be on ledger) */
                    mt.Status == MempoolTransactionStatus.Committed
                    && aggregatorIsSyncedUpEnoughToRemoveCommittedTransactions
                    && mt.CommitTimestamp!.Value < pruneIfCommittedBefore
                )
                ||
                (
                    /* For those submitted by this gateway, prune if it was submitted a while ago and was not seen in the mempool recently */
                    mt.SubmittedByThisGateway
                    && mt.LastSubmittedToGatewayTimestamp!.Value < pruneIfLastGatewaySubmissionBefore
                    && (mt.LastDroppedOutOfMempoolTimestamp != null && mt.LastDroppedOutOfMempoolTimestamp < pruneIfNotSeenSince)
                )
                ||
                (
                    /* For those not submitted by this gateway, prune if it first appeared a while ago and was not seen in the mempool recently */
                    !mt.SubmittedByThisGateway
                    && mt.FirstSeenInMempoolTimestamp!.Value < pruneIfFirstSeenBefore
                    && (mt.LastDroppedOutOfMempoolTimestamp != null && mt.LastDroppedOutOfMempoolTimestamp < pruneIfNotSeenSince)
                )
            )
            .ToListAsync(token);

        if (transactionsToPrune.Count > 0)
        {
            _mempoolTransactionsPrunedCount.Inc(transactionsToPrune.Count);
            _logger.LogInformation(
                "Pruning {PrunedCount} transactions from the mempool, of which {PrunedCommittedCount} were committed",
                transactionsToPrune.Count,
                transactionsToPrune.Count(t => t.Status == MempoolTransactionStatus.Committed)
            );

            dbContext.MempoolTransactions.RemoveRange(transactionsToPrune);
            await dbContext.SaveChangesAsync(token);
        }
    }

    private async Task UpdateSizeMetrics(ReadWriteDbContext dbContext, CancellationToken token)
    {
        var counts = await dbContext.MempoolTransactions
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(token);

        var existingStatusLabelsNeedingUpdating = _mempoolDbSizeByStatus.GetAllLabelValues().SelectMany(x => x).ToHashSet();

        foreach (var countByStatus in counts)
        {
            var statusName =
                MempoolTransactionStatusValueConverter.Conversion.GetValueOrDefault(countByStatus.Status) ?? "UNKNOWN";
            _mempoolDbSizeByStatus.WithLabels(statusName).Set(countByStatus.Count);
            existingStatusLabelsNeedingUpdating.Remove(statusName);
        }

        // If a known status doesn't appear in the database, it should be set to 0.
        foreach (var statusName in existingStatusLabelsNeedingUpdating)
        {
            _mempoolDbSizeByStatus.WithLabels(statusName).Set(0);
        }
    }
}
