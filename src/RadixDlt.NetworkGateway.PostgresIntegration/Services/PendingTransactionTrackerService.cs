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

using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Model;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Exceptions;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.PostgresIntegration.Services;

internal class PendingTransactionTrackerService : IPendingTransactionTrackerService
{
    private static readonly LogLimiter _combineMempoolsInfoLogLimiter = new(TimeSpan.FromSeconds(10), LogLevel.Information, LogLevel.Debug);

    private readonly IDbContextFactory<ReadWriteDbContext> _dbContextFactory;
    private readonly IOptionsMonitor<MempoolOptions> _mempoolOptionsMonitor;
    private readonly ILogger<PendingTransactionTrackerService> _logger;
    private readonly ConcurrentDictionary<string, NodeMempoolHashes> _latestMempoolContentsByNode = new();
    private readonly ConcurrentLruCache<PendingTransactionHashPair, PendingTransactionData> _recentFullTransactionsFetched;
    private readonly IEnumerable<IMempoolTrackerServiceObserver> _observers;
    private readonly IClock _clock;

    public PendingTransactionTrackerService(
        IDbContextFactory<ReadWriteDbContext> dbContextFactory,
        IOptionsMonitor<MempoolOptions> mempoolOptionsMonitor,
        ILogger<PendingTransactionTrackerService> logger,
        IEnumerable<IMempoolTrackerServiceObserver> observers,
        IClock clock)
    {
        _dbContextFactory = dbContextFactory;
        _mempoolOptionsMonitor = mempoolOptionsMonitor;
        _logger = logger;
        _observers = observers;
        _clock = clock;

        _recentFullTransactionsFetched = new ConcurrentLruCache<PendingTransactionHashPair, PendingTransactionData>(
            mempoolOptionsMonitor.CurrentValue.RecentFetchedUnknownTransactionsCacheSize
        );
    }

    /// <summary>
    /// This is called regularly from the NodeMempoolTransactionIdsReaderWorker to submit the current
    /// transaction ids in each node's mempool.
    /// </summary>
    public void RegisterNodeMempoolHashes(string nodeName, NodeMempoolHashes nodeMempoolHashes)
    {
        _latestMempoolContentsByNode[nodeName] = nodeMempoolHashes;
    }

    /// <summary>
    /// This is called from the NodeMempoolFullTransactionReaderWorker (where enabled) to work out which transaction
    /// contents actually need fetching.
    /// </summary>
    public async Task<HashSet<PendingTransactionHashPair>> WhichTransactionsNeedContentFetching(IEnumerable<PendingTransactionHashPair> payloadHashes, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var payloadHashesInMempoolNotRecentlyFetched = payloadHashes
            .Where(h => !_recentFullTransactionsFetched.Contains(h))
            .Select(h => Tuple.Create(h.IntentHash, h.PayloadHash))
            .ToList(); // Npgsql optimises ToList calls but not ToHashSet

        if (payloadHashesInMempoolNotRecentlyFetched.Count == 0)
        {
            return new HashSet<PendingTransactionHashPair>();
        }

        var unfetchedTransactionsAlreadyInDatabase = await dbContext.PendingTransactions
            .Where(pt => payloadHashesInMempoolNotRecentlyFetched.Contains(Tuple.Create(pt.IntentHash, pt.PayloadHash)))
            .Select(pt => Tuple.Create(pt.IntentHash, pt.PayloadHash))
            .ToListAsync(cancellationToken);

        var payloadHashesToFetch = payloadHashesInMempoolNotRecentlyFetched.Select(t => new PendingTransactionHashPair(t.Item1, t.Item2)).ToHashSet();

        payloadHashesToFetch.ExceptWith(unfetchedTransactionsAlreadyInDatabase.Select(t => new PendingTransactionHashPair(t.Item1, t.Item2)));

        return payloadHashesToFetch;
    }

    /// <summary>
    /// This is called from the NodeMempoolFullTransactionReaderWorker (where enabled) to check if the transaction
    /// identifier still needs fetching. This is to try to not make a call if we've already got the transaction contents
    /// from another node in the mean-time.
    /// </summary>
    /// <returns>If the transaction was first seen (true) or (false).</returns>
    public bool TransactionContentsStillNeedFetching(PendingTransactionHashPair transactionIdentifier)
    {
        return !_recentFullTransactionsFetched.Contains(transactionIdentifier);
    }

    /// <summary>
    /// This is called from the NodeMempoolFullTransactionReaderWorker (where enabled) to submit fetched transaction
    /// data.
    /// </summary>
    /// <returns>If the transaction was first seen (true) or (false).</returns>
    public bool SubmitTransactionContents(PendingTransactionData pendingTransactionData)
    {
        return _recentFullTransactionsFetched.SetIfNotExists(pendingTransactionData.Hashes, pendingTransactionData);
    }

    public async Task HandleChanges(CancellationToken token)
    {
        var currentTimestamp = _clock.UtcNow;
        var mempoolConfiguration = _mempoolOptionsMonitor.CurrentValue;

        var combinedMempool = CombineNodeMempools(mempoolConfiguration);

        await Task.WhenAll(
            MarkRelevantMempoolTransactionsInCombinedMempoolAsReappeared(combinedMempool, token),
            MarkRelevantMempoolTransactionsNotInCombinedMempoolAsMissing(currentTimestamp, mempoolConfiguration, combinedMempool, token),
            CreateMempoolTransactionsFromNewTransactionsDiscoveredInCombinedMempool(mempoolConfiguration, combinedMempool, token)
        );
    }

    private Dictionary<PendingTransactionHashPair, DateTime> CombineNodeMempools(MempoolOptions mempoolOptions)
    {
        var nodeMempoolsToConsider = _latestMempoolContentsByNode
            .Where(kvp => kvp.Value.AtTime.WithinPeriodOfNow(mempoolOptions.ExcludeNodeMempoolsFromUnionIfStaleFor, _clock))
            .ToList();

        if (nodeMempoolsToConsider.Count == 0)
        {
            throw new NoMempoolDataException(
                $"Don't have any recent mempool data from nodes within {mempoolOptions.ExcludeNodeMempoolsFromUnionIfStaleFor.FormatSecondsHumanReadable()}. This may be because the service has yet to connect to the node/s."
            );
        }

        var combinedMempoolByLatestSeen = new Dictionary<PendingTransactionHashPair, DateTime>();

        foreach (var (_, mempoolContents) in nodeMempoolsToConsider)
        {
            var seenAt = mempoolContents.AtTime;
            foreach (var identifier in mempoolContents.TransactionHashes)
            {
                if (
                    !combinedMempoolByLatestSeen.ContainsKey(identifier)
                    || seenAt > combinedMempoolByLatestSeen[identifier]
                )
                {
                    combinedMempoolByLatestSeen[identifier] = seenAt;
                }
            }
        }

        _logger.Log(
            _combineMempoolsInfoLogLimiter.GetLogLevel(),
            "There are {MempoolSize} transactions across node mempools: {NodesUsed}",
            combinedMempoolByLatestSeen.Count,
            nodeMempoolsToConsider.Select(kvp => kvp.Key).Humanize()
        );

        _observers.ForEach(x => x.CombinedMempoolCurrentSizeCount(combinedMempoolByLatestSeen.Count));

        return combinedMempoolByLatestSeen;
    }

    private async Task MarkRelevantMempoolTransactionsInCombinedMempoolAsReappeared(
        Dictionary<PendingTransactionHashPair, DateTime> combinedMempoolWithLastSeen,
        CancellationToken token
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var pendingTransactionIds = combinedMempoolWithLastSeen.Keys.Select(h => h.PayloadHash).ToList(); // Npgsql optimizes List<> Contains

        var reappearedTransactions = await dbContext.PendingTransactions
            .Where(mt => pendingTransactionIds.Contains(mt.PayloadHash))
            .Where(mt => mt.Status == PendingTransactionStatus.Missing)
            .ToListAsync(token);

        if (reappearedTransactions.Count == 0)
        {
            return;
        }

        foreach (var missingTransaction in reappearedTransactions)
        {
            missingTransaction.MarkAsSeenInAMempool(_clock.UtcNow);
        }

        // If save changes partially succeeds, we might double-count these metrics
        _observers.ForEach(x => x.TransactionsReappearedCount(reappearedTransactions.Count));

        var (_, dbUpdateMs) = await CodeStopwatch.TimeInMs(
            async () => await dbContext.SaveChangesAsync(token)
        );

        _logger.LogInformation(
            "{ReappearedTransactionsCount} marked reappeared in {DbUpdatesMs}ms",
            reappearedTransactions.Count,
            dbUpdateMs
        );
    }

    private async Task CreateMempoolTransactionsFromNewTransactionsDiscoveredInCombinedMempool(
        MempoolOptions mempoolOptions,
        Dictionary<PendingTransactionHashPair, DateTime> combinedMempoolWithLastSeen,
        CancellationToken token
    )
    {
        if (!mempoolOptions.TrackTransactionsNotSubmittedByThisGateway)
        {
            return;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        // Gather the transaction contents if we've loaded them from a node recently.
        // If we don't have the transaction contents, either these transactions are already in our MempoolTransactions
        // table, or they've yet to be fetched by a node. Either way, we filter them out and don't consider them further
        // for the time being.
        var transactionsWhichMightNeedAdding = combinedMempoolWithLastSeen.Keys
            .SelectNonNull(transactionId => _recentFullTransactionsFetched.GetOrDefault(transactionId))
            .ToList();

        var transactionIdsWhichMightNeedAdding = transactionsWhichMightNeedAdding
            .Select(t => t.Hashes.PayloadHash)
            .ToList(); // Npgsql optimizes List<> Contains

        // Now check that these are actually new and need adding, by checking the transaction ids against the database.
        // We check both the PendingTransactions table, and the LedgerTransactions table.
        // If a node mempool gets really far behind, it could include committed transactions we've already pruned
        // from our MempoolTransactions table due to being committed - so let's ensure these don't get re-added.
        var transactionIdsInANodeMempoolWhichAreAlreadyAMempoolTransactionInTheDb = await dbContext.PendingTransactions
            .Where(pt => transactionIdsWhichMightNeedAdding.Contains(pt.PayloadHash))
            .Select(pt => pt.PayloadHash)
            .ToHashSetAsync(ByteArrayEqualityComparer.Default, token);

        var transactionIdsInANodeMempoolWhichAreAlreadyCommitted = await dbContext.LedgerTransactions
            .OfType<UserLedgerTransaction>()
            .Where(ult => transactionIdsWhichMightNeedAdding.Contains(ult.PayloadHash))
            .Select(ult => ult.PayloadHash)
            .ToHashSetAsync(ByteArrayEqualityComparer.Default, token);

        var transactionsToAdd = transactionsWhichMightNeedAdding
            .Where(pt =>
                !transactionIdsInANodeMempoolWhichAreAlreadyAMempoolTransactionInTheDb.Contains(pt.Hashes.PayloadHash)
                &&
                !transactionIdsInANodeMempoolWhichAreAlreadyCommitted.Contains(pt.Hashes.PayloadHash)
            )
            .ToList();

        if (transactionsToAdd.Count == 0)
        {
            return;
        }

        var newDbMempoolTransactions = transactionsToAdd
            .Select(ptd => PendingTransaction.NewFirstSeenInMempool(
                ptd.Hashes.PayloadHash,
                ptd.Hashes.IntentHash,
                ptd.Payload,
                ptd.SeenAt
            ));

        dbContext.PendingTransactions.AddRange(newDbMempoolTransactions);

        // If save changes partially succeeds, we might double-count these metrics
        _observers.ForEach(x => x.TransactionsAddedDueToNodeMempoolAppearanceCount(transactionsToAdd.Count));

        var (_, dbUpdateMs) = await CodeStopwatch.TimeInMs(
            async () => await dbContext.SaveChangesAsync(token)
        );

        _logger.LogInformation(
            "{TransactionsAddedCount} created in {DbUpdatesMs}ms",
            transactionsToAdd.Count,
            dbUpdateMs
        );
    }

    private async Task MarkRelevantMempoolTransactionsNotInCombinedMempoolAsMissing(
        DateTime currentTimestamp,
        MempoolOptions mempoolOptions,
        Dictionary<PendingTransactionHashPair, DateTime> combinedMempoolWithLastSeen,
        CancellationToken token
    )
    {
        // NB - this should be a list as Npgsql optimises that case
        var seenTransactionIds = combinedMempoolWithLastSeen.Keys.Select(h => h.PayloadHash).ToList();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var submissionGracePeriodCutOff = currentTimestamp - mempoolOptions.PostSubmissionGracePeriodBeforeCanBeMarkedMissing;

        var previouslyTrackedTransactionsNowMissingFromNodeMempools = await dbContext.PendingTransactions
            .Where(pt => pt.Status == PendingTransactionStatus.SubmittedOrKnownInNodeMempool)
            .Where(pt =>
                !pt.SubmittedByThisGateway
                ||
                (pt.SubmittedByThisGateway && (
                    pt.LastSubmittedToNodeTimestamp == null // Shouldn't happen, but protect against it regardless
                    || pt.LastSubmittedToNodeTimestamp < submissionGracePeriodCutOff
                ))
            )
            .Where(pt => !seenTransactionIds.Contains(pt.PayloadHash))
            .ToListAsync(token);

        if (previouslyTrackedTransactionsNowMissingFromNodeMempools.Count == 0)
        {
            return;
        }

        foreach (var pendingTransaction in previouslyTrackedTransactionsNowMissingFromNodeMempools)
        {
            _observers.ForEach(x => x.TransactionsMarkedAsMissing());
            pendingTransaction.MarkAsMissing(_clock.UtcNow);
        }

        var (_, dbUpdateMs) = await CodeStopwatch.TimeInMs(
            async () => await dbContext.SaveChangesAsync(token)
        );

        _logger.LogInformation(
            "{TransactionsMarkedMissing} transactions marked missing in {DbUpdatesMs}ms",
            previouslyTrackedTransactionsNowMissingFromNodeMempools.Count,
            dbUpdateMs
        );
    }
}
