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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.Abstractions.Extensions;
using RadixDlt.NetworkGateway.Abstractions.Network;
using RadixDlt.NetworkGateway.Abstractions.Utilities;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.DataAggregator.Exceptions;
using RadixDlt.NetworkGateway.DataAggregator.Monitoring;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreModel = RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface ILedgerTransactionsProcessor
{
    Task ProcessTransactions(CancellationToken token);
}

public sealed class LedgerTransactionsProcessor : ILedgerTransactionsProcessor
{
    private readonly ILogger<LedgerTransactionsProcessor> _logger;
    private readonly IOptionsMonitor<LedgerConfirmationOptions> _ledgerConfirmationOptionsMonitor;
    private readonly ISystemStatusService _systemStatusService;
    private readonly ILedgerExtenderService _ledgerExtenderService;
    private readonly IEnumerable<ILedgerConfirmationServiceObserver> _observers;
    private readonly ITopOfLedgerProvider _topOfLedgerProvider;
    private readonly IFetchedTransactionStore _fetchedTransactionStore;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;
    private readonly IClock _clock;

    private LedgerConfirmationOptions Config { get; set; }

    public LedgerTransactionsProcessor(
        ILogger<LedgerTransactionsProcessor> logger,
        IOptionsMonitor<LedgerConfirmationOptions> ledgerConfirmationOptionsMonitor,
        ISystemStatusService systemStatusService,
        ILedgerExtenderService ledgerExtenderService,
        IEnumerable<ILedgerConfirmationServiceObserver> observers,
        IFetchedTransactionStore fetchedTransactionStore,
        ITopOfLedgerProvider topOfLedgerProvider,
        INetworkConfigurationProvider networkConfigurationProvider,
        IClock clock)
    {
        _logger = logger;
        _ledgerConfirmationOptionsMonitor = ledgerConfirmationOptionsMonitor;
        _systemStatusService = systemStatusService;
        _ledgerExtenderService = ledgerExtenderService;
        _observers = observers;
        _fetchedTransactionStore = fetchedTransactionStore;
        _topOfLedgerProvider = topOfLedgerProvider;
        _clock = clock;
        _networkConfigurationProvider = networkConfigurationProvider;
        Config = _ledgerConfirmationOptionsMonitor.CurrentValue;
    }

    public async Task ProcessTransactions(CancellationToken token)
    {
        var networkConfiguration = await _networkConfigurationProvider.GetNetworkConfiguration(token);
        var lastCommittedTransactionSummary = await _topOfLedgerProvider.GetTopOfLedger(token);
        await _observers.ForEachAsync(x => x.PreHandleLedgerExtension(_clock.UtcNow));

        Config = _ledgerConfirmationOptionsMonitor.CurrentValue;

        var transactions = ConstructLedgerExtension(lastCommittedTransactionSummary);

        if (transactions.Count == 0)
        {
            return;
        }

        var consistentLedgerExtension = GenerateConsistentLedgerExtension(transactions, lastCommittedTransactionSummary);

        var (commitReport, totalCommitMs) = await CodeStopwatch.TimeInMs(
            () => _ledgerExtenderService.CommitTransactions(consistentLedgerExtension, token)
        );

        HandleLedgerExtensionSuccess(consistentLedgerExtension, totalCommitMs, commitReport);

        await DelayBetweenIngestionBatchesIfRequested(commitReport);
    }

    private List<CoreModel.CommittedTransaction> ConstructLedgerExtension(TransactionSummary topOfLedger)
    {
        var startStateVersion = topOfLedger.StateVersion + 1;
        var transactions = _fetchedTransactionStore.GetTransactionBatch(startStateVersion, (int)Config.MaxCommitBatchSize, (int)Config.MinCommitBatchSize);
        return transactions;
    }

    private void HandleLedgerExtensionSuccess(ConsistentLedgerExtension ledgerExtension, long totalCommitMs, CommitTransactionsReport commitReport)
    {
        ReportOnLedgerExtensionSuccess(ledgerExtension, totalCommitMs, commitReport);
        _fetchedTransactionStore.RemoveProcessedTransactions(commitReport.FinalTransaction.StateVersion);
    }

    private async Task DelayBetweenIngestionBatchesIfRequested(CommitTransactionsReport commitReport)
    {
        var isDelayEnabled = Config.DelayBetweenLargeBatches.TotalMilliseconds > 0;
        var isLargeBatch = commitReport.TransactionsCommittedCount >= Config.LargeBatchSizeToAddDelay;

        if (!isDelayEnabled || !isLargeBatch)
        {
            return;
        }

        _logger.LogInformation(
            "Enforcing delay of {DelayMs}ms due to the size of the ingestion batch",
            Config.DelayBetweenLargeBatches.TotalMilliseconds
        );
        await Task.Delay(Config.DelayBetweenLargeBatches);
    }

    private void ReportOnLedgerExtensionSuccess(ConsistentLedgerExtension ledgerExtension, long totalCommitMs, CommitTransactionsReport commitReport)
    {
        _systemStatusService.RecordTransactionsCommitted();

        var currentTimestamp = _clock.UtcNow;
        var committedTransactionSummary = commitReport.FinalTransaction;
        var roundTimestampDiff = currentTimestamp - ledgerExtension.LatestTransactionSummary.RoundTimestamp;

        _observers.ForEach(x => x.RecordTopOfDbLedger(ledgerExtension.LatestTransactionSummary.StateVersion, ledgerExtension.LatestTransactionSummary.RoundTimestamp));
        _observers.ForEach(x => x.ReportOnLedgerExtensionSuccess(currentTimestamp, roundTimestampDiff, totalCommitMs, commitReport.TransactionsCommittedCount));

        _logger.LogInformation(
            "Committed {TransactionCount} transactions to the DB in {TotalCommitTransactionsMs}ms [EntitiesTouched={DbEntriesWritten}]",
            ledgerExtension.CommittedTransactions.Count,
            totalCommitMs,
            commitReport.DbEntriesTouched
        );

        _logger.LogInformation(
            "[TimeSplitsInMs: TxnContentHandling={TxnContentHandlingMs},DbDependencyLoading={DbDependenciesLoadingMs},DbPersistence={DbPersistanceMs}]",
            commitReport.ContentHandlingMs,
            commitReport.DbDependenciesLoadingMs,
            commitReport.DbPersistenceMs
        );

        _logger.LogInformation(
            "[NewDbLedgerTip: StateVersion={LedgerStateVersion},Epoch={LedgerEpoch},IndexInEpoch={LedgerIndexInEpoch},RoundTimestamp={RoundTimestamp}]",
            committedTransactionSummary.StateVersion,
            committedTransactionSummary.Epoch,
            committedTransactionSummary.IndexInEpoch,
            committedTransactionSummary.RoundTimestamp.AsUtcIsoDateToSecondsForLogs()
        );
    }

    private ConsistentLedgerExtension GenerateConsistentLedgerExtension(List<CoreModel.CommittedTransaction> transactions, TransactionSummary topOfLedger)
    {
        var previousStateVersion = topOfLedger.StateVersion;

        try
        {
            foreach (var transaction in transactions)
            {
                TransactionConsistencyValidator.AssertChildTransactionConsistent(previousStateVersion: previousStateVersion, stateVersion: transaction.ResultantStateIdentifiers.StateVersion);
                previousStateVersion = transaction.ResultantStateIdentifiers.StateVersion;
            }

            _observers.ForEach(x => x.ExtensionConsistencyGained());
        }
        catch (InvalidLedgerCommitException)
        {
            _observers.ForEach(x => x.ExtensionConsistencyLost());
            throw;
        }
        catch (InconsistentLedgerException)
        {
            _observers.ForEach(x => x.ExtensionConsistencyLost());
            throw;
        }

        return new ConsistentLedgerExtension(topOfLedger, transactions);
    }
}
