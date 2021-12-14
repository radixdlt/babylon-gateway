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

using Common.Extensions;
using DataAggregator.DependencyInjection;
using DataAggregator.GlobalServices;
using DataAggregator.GlobalWorkers;
using DataAggregator.LedgerExtension;
using DataAggregator.Monitoring;
using DataAggregator.NodeScopedServices;
using DataAggregator.NodeScopedServices.ApiReaders;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.NodeScopedWorkers;

/// <summary>
/// Responsible for syncing the transaction stream from a node.
/// </summary>
public class NodeMempoolCommitWhenUnsyncedWorker : LoopedWorkerBase, INodeWorker
{
    /* Dependencies */
    private readonly ILogger<NodeMempoolCommitWhenUnsyncedWorker> _logger;
    private readonly IServiceProvider _services;
    private readonly ISystemStatusService _systemStatusService;

    private long? _lastLedgerTip;

    // NB - So that we can get new transient dependencies each iteration, we create most dependencies
    //      from the service provider.
    public NodeMempoolCommitWhenUnsyncedWorker(
        ILogger<NodeMempoolCommitWhenUnsyncedWorker> logger,
        IServiceProvider services,
        ISystemStatusService systemStatusService
    )
        : base(logger, TimeSpan.FromMilliseconds(200), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(60))
    {
        _logger = logger;
        _services = services;
        _systemStatusService = systemStatusService;
    }

    public bool IsEnabled()
    {
        var nodeConfig = _services.GetRequiredService<INodeConfigProvider>();
        return nodeConfig.NodeAppSettings.Enabled && !nodeConfig.NodeAppSettings.DisabledForTopOfTransactionReadingIfNotFullySynced;
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        if (_systemStatusService.IsSyncedUp())
        {
            // This service isn't needed because the main transaction commit loop will take care of things
            _lastLedgerTip = null;
            return;
        }

        if (_lastLedgerTip == null)
        {
            // The data aggregator has just been turned on, or it's not synced up
            var networkStatus =
                await _services.GetRequiredService<INetworkStatusReader>().GetNetworkStatus(stoppingToken);
            _lastLedgerTip = networkStatus.CurrentStateIdentifier.StateVersion;
            _logger.LogInformation(
                "As the db isn't synced up, this service will read from the log from state version {StateVersion} to ensure submitted transactions are marked committed correctly",
                _lastLedgerTip
            );
        }

        await FetchAndMarkTransactionsCommitted(stoppingToken);
    }

    private async Task FetchAndMarkTransactionsCommitted(CancellationToken stoppingToken)
    {
        const int MaxTransactionsToPull = 1000;

        var transactionsResponse = await _services.GetRequiredService<ITransactionLogReader>()
            .GetTransactions(_lastLedgerTip!.Value, MaxTransactionsToPull, stoppingToken);

        if (transactionsResponse.Transactions.Count == 0)
        {
            return;
        }

        var currSummary = TransactionSummarisation.PreGenesisTransactionSummary(); // It doesn't actually matter
        var committedTransactionsToMark = new List<CommittedTransactionData>();

        foreach (var transaction in transactionsResponse.Transactions)
        {
            currSummary = TransactionSummarisation.GenerateSummary(currSummary, transaction);
            if (!currSummary.IsOnlyRoundChange && !currSummary.IsStartOfEpoch)
            {
                committedTransactionsToMark.Add(new CommittedTransactionData(
                    transaction,
                    currSummary,
                    transaction.Metadata.Hex.ConvertFromHex()
                ));
            }
        }

        if (committedTransactionsToMark.Count == 0)
        {
            _lastLedgerTip = transactionsResponse.Transactions.Last().CommittedStateIdentifier.StateVersion;
            return;
        }

        _logger.LogInformation(
            "{CommittedUserTransactionsCount} user transactions committed to the tip of the ledger which will be marked as committed if in the mempool",
            committedTransactionsToMark.Count
        );

        var dbContextFactory = _services.GetRequiredService<IDbContextFactory<AggregatorDbContext>>();
        var rawTransactionWriter = _services.GetRequiredService<IRawTransactionWriter>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(stoppingToken);
        await rawTransactionWriter.EnsureMempoolTransactionsMarkedAsCommitted(
            dbContext,
            committedTransactionsToMark,
            stoppingToken
        );

        _lastLedgerTip = transactionsResponse.Transactions.Last().CommittedStateIdentifier.StateVersion;
    }
}
