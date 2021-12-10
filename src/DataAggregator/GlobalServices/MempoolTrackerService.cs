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

using Common.CoreCommunications;
using Common.Database.Models.Mempool;
using Common.Extensions;
using Common.Utilities;
using DataAggregator.Configuration;
using DataAggregator.DependencyInjection;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.Collections.Concurrent;

using Core = RadixCoreApi.Generated.Model;

namespace DataAggregator.GlobalServices;

public record TransactionDataWithId(byte[] Id, TransactionData TransactionData);

public record TransactionData(Instant SeenAt, byte[] Payload, Core.Transaction Transaction);

public record NodeMempoolContents
{
    public Dictionary<byte[], TransactionData> Transactions { get; }

    public Instant AtTime { get; }

    public NodeMempoolContents(Dictionary<byte[], TransactionData> transactions)
    {
        Transactions = transactions;
        AtTime = SystemClock.Instance.GetCurrentInstant();
    }
}

public interface IMempoolTrackerService
{
    void RegisterNodeMempool(string nodeName, NodeMempoolContents nodeMempoolContents);

    Task HandleMempoolChanges(CancellationToken token);
}

public class MempoolTrackerService : IMempoolTrackerService
{
    private static readonly LogLimiter _combineMempoolsInfoLogLimiter = new(TimeSpan.FromSeconds(5), LogLevel.Information, LogLevel.Debug);

    private readonly IDbContextFactory<AggregatorDbContext> _dbContextFactory;
    private readonly IAggregatorConfiguration _aggregatorConfiguration;
    private readonly ILogger<MempoolTrackerService> _logger;
    private readonly ConcurrentDictionary<string, NodeMempoolContents> _latestMempoolContentsByNode = new();

    public MempoolTrackerService(
        IDbContextFactory<AggregatorDbContext> dbContextFactory,
        IAggregatorConfiguration aggregatorConfiguration,
        ILogger<MempoolTrackerService> logger
    )
    {
        _dbContextFactory = dbContextFactory;
        _aggregatorConfiguration = aggregatorConfiguration;
        _logger = logger;
    }

    public void RegisterNodeMempool(string nodeName, NodeMempoolContents nodeMempoolContents)
    {
        _latestMempoolContentsByNode[nodeName] = nodeMempoolContents;
    }

    public async Task HandleMempoolChanges(CancellationToken token)
    {
        var combinedMempool = CombineNodeMempools();

        await CreateOrUpdateMempoolTransactionTimesInDb(combinedMempool, token);
        await HandleMissingPendingTransactions(combinedMempool.Keys.ToHashSet(), token);
    }

    private Dictionary<byte[], TransactionData> CombineNodeMempools()
    {
        var lastUpdatedToBeConsidered = Duration.FromSeconds(15);

        var nodeMempoolsToConsider = _latestMempoolContentsByNode
            .Where(kvp => kvp.Value.AtTime.WithinPeriodOfNow(lastUpdatedToBeConsidered))
            .ToList();

        if (nodeMempoolsToConsider.Count == 0)
        {
            throw new Exception(
                $"Don't have any recent mempool data from nodes within {lastUpdatedToBeConsidered.FormatSecondsHumanReadable()}"
            );
        }

        var logLevel = _combineMempoolsInfoLogLimiter.GetLogLevel();

        _logger.Log(
            logLevel,
            "Combining mempool data from: {NodesUsed}",
            nodeMempoolsToConsider.Select(kvp => kvp.Key).Humanize()
        );

        var combinedMempool = new Dictionary<byte[], TransactionData>(ByteArrayEqualityComparer.Default);

        foreach (var (_, mempoolContents) in nodeMempoolsToConsider)
        {
            foreach (var (identifier, data) in mempoolContents.Transactions)
            {
                if (!combinedMempool.ContainsKey(identifier))
                {
                    combinedMempool[identifier] = data;
                }
                else if (data.SeenAt > combinedMempool[identifier].SeenAt)
                {
                    combinedMempool[identifier] = data;
                }
            }
        }

        _logger.Log(
            logLevel,
            "There are {MempoolSize} transactions in at least one of the nodes' mempools",
            combinedMempool.Count
        );

        return combinedMempool;
    }

    private async Task CreateOrUpdateMempoolTransactionTimesInDb(
        Dictionary<byte[], TransactionData> combinedMempool,
        CancellationToken token
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        await dbContext.MempoolTransactions
            .UpsertRange(combinedMempool.Select(MapToMempoolTransaction))
            .WhenMatched((dbTxn, currTxn) => new MempoolTransaction
            {
                FirstSeenInMempoolTimestamp = dbTxn.FirstSeenInMempoolTimestamp ?? currTxn.FirstSeenInMempoolTimestamp,
                LastSeenInMempoolTimestamp = currTxn.LastSeenInMempoolTimestamp,
                Status = dbTxn.Status == MempoolTransactionStatus.Committed ? MempoolTransactionStatus.Committed
                    : dbTxn.Status == MempoolTransactionStatus.Failed ? MempoolTransactionStatus.Failed
                    : MempoolTransactionStatus.InNodeMempool, // Pending or Missing => Pending
            })
            .RunAsync(token);
    }

    private MempoolTransaction MapToMempoolTransaction(KeyValuePair<byte[], TransactionData> mempoolItem)
    {
        var transactionData = mempoolItem.Value;
        return new MempoolTransaction
        {
            TransactionIdentifierHash = mempoolItem.Key,
            Payload = transactionData.Payload,
            FirstSeenInMempoolTimestamp = transactionData.SeenAt,
            LastSeenInMempoolTimestamp = transactionData.SeenAt,
            TransactionsContents = ParsedTransactionMapper.MapToGatewayTransactionContents(transactionData.Transaction),
            Status = MempoolTransactionStatus.InNodeMempool,
        };
    }

    private async Task HandleMissingPendingTransactions(
        HashSet<byte[]> seenTransactionIds,
        CancellationToken token
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var previouslyTrackedTransactionsNowMissingFromNodeMempools = await dbContext.MempoolTransactions
            .Where(mt => mt.Status == MempoolTransactionStatus.InNodeMempool)
            .Where(mempoolItem => !seenTransactionIds.Contains(mempoolItem.TransactionIdentifierHash))
            .ToListAsync(token);

        var timeouts = _aggregatorConfiguration.GetMempoolTimeouts();

        foreach (var mempoolItem in previouslyTrackedTransactionsNowMissingFromNodeMempools)
        {
            if (!mempoolItem.SubmittedByThisGateway)
            {
                mempoolItem.Status = MempoolTransactionStatus.Missing;
                continue;
            }

            var resubmissionTime = mempoolItem.LastSubmittedToNodeTimestamp == null ? SystemClock.Instance.GetCurrentInstant()
                : DateTimeExtensions.LatestOf(
                    mempoolItem.LastSubmittedToNodeTimestamp.Value + timeouts.MinDelayBetweenResubmissions,
                    SystemClock.Instance.GetCurrentInstant()
                );

            var resubmissionLimit = mempoolItem.LastSubmittedToGatewayTimestamp!.Value + timeouts.StopResubmittingAfter;

            var canResubmit = resubmissionTime <= resubmissionLimit;

            // If it can resubmit, the transaction will be picked up by the MempoolResubmissionService for retrying
            mempoolItem.Status = canResubmit ? MempoolTransactionStatus.Missing : MempoolTransactionStatus.Failed;
        }

        await dbContext.SaveChangesAsync(token);
    }
}
