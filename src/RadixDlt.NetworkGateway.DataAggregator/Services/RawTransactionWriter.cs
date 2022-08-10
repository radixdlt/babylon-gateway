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
using Prometheus;
using RadixDlt.NetworkGateway.Core.Database;
using RadixDlt.NetworkGateway.Core.Database.Models.Ledger;
using RadixDlt.NetworkGateway.Core.Database.Models.Mempool;
using RadixDlt.NetworkGateway.Core.Extensions;
using RadixDlt.NetworkGateway.Core.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.LedgerExtension;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface IRawTransactionWriter
{
    Task<int> EnsureRawTransactionsCreatedOrUpdated(ReadWriteDbContext context, List<RawTransaction> rawTransactions, CancellationToken token);

    Task<int> EnsureMempoolTransactionsMarkedAsCommitted(ReadWriteDbContext context, List<CommittedTransactionData> transactionData, CancellationToken token);
}

public class RawTransactionWriter : IRawTransactionWriter
{
    private static readonly Counter _transactionsMarkedCommittedCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_marked_committed_count",
            "Number of mempool transactions which are marked committed"
        );

    private static readonly Counter _transactionsMarkedCommittedWhichWereFailedCount = Metrics
        .CreateCounter(
            "ng_db_mempool_transactions_marked_committed_which_were_failed_count",
            "Number of mempool transactions which are marked committed which were previously marked as failed"
        );

    private readonly ILogger<RawTransactionWriter> _logger;

    public RawTransactionWriter(ILogger<RawTransactionWriter> logger)
    {
        _logger = logger;
    }

    public async Task<int> EnsureRawTransactionsCreatedOrUpdated(ReadWriteDbContext context, List<RawTransaction> rawTransactions, CancellationToken token)
    {
        // See https://github.com/artiomchi/FlexLabs.Upsert/wiki/Usage
        return await context.RawTransactions
            .UpsertRange(rawTransactions)
            .RunAsync(token);
    }

    public async Task<int> EnsureMempoolTransactionsMarkedAsCommitted(ReadWriteDbContext context, List<CommittedTransactionData> transactionData, CancellationToken token)
    {
        var transactionsById = transactionData
            .Where(td => !td.TransactionSummary.IsStartOfRound)
            .ToDictionary(
                rt => rt.TransactionSummary.PayloadHash,
                ByteArrayEqualityComparer.Default
            );

        var transactionIdList = transactionsById.Keys.ToList(); // List<> are optimised for PostgreSQL lookups

        var toUpdate = await context.MempoolTransactions
            .Where(mt => mt.Status != MempoolTransactionStatus.Committed && transactionIdList.Contains(mt.PayloadHash))
            .ToListAsync(token);

        if (toUpdate.Count == 0)
        {
            return 0;
        }

        foreach (var mempoolTransaction in toUpdate)
        {
            if (mempoolTransaction.Status == MempoolTransactionStatus.Failed)
            {
                _transactionsMarkedCommittedWhichWereFailedCount.Inc();
                _logger.LogError(
                    "Transaction with id {TransactionId} which was first/last submitted to Gateway at {FirstGatewaySubmissionTime}/{LastGatewaySubmissionTime} and last marked missing from mempool at {LastMissingFromMempoolTimestamp} was mark failed at {FailureTime} due to {FailureReason} ({FailureExplanation}) but has now been marked committed",
                    mempoolTransaction.PayloadHash.ToHex(),
                    mempoolTransaction.FirstSubmittedToGatewayTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                    mempoolTransaction.LastSubmittedToGatewayTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                    mempoolTransaction.LastDroppedOutOfMempoolTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                    mempoolTransaction.FailureTimestamp?.AsUtcIsoDateToSecondsForLogs(),
                    mempoolTransaction.FailureReason?.ToString(),
                    mempoolTransaction.FailureExplanation
                );
            }

            var transactionSummary = transactionsById[mempoolTransaction.PayloadHash].TransactionSummary;
            mempoolTransaction.MarkAsCommitted(
                transactionSummary.StateVersion,
                transactionSummary.NormalizedRoundTimestamp
            );
        }

        // If this errors (due to changes to the MempoolTransaction.Status ConcurrencyToken), we may have to consider
        // something like: https://docs.microsoft.com/en-us/ef/core/saving/concurrency
        var result = await context.SaveChangesAsync(token);

        _transactionsMarkedCommittedCount.Inc(toUpdate.Count);

        return result;
    }
}
