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

using Common.Utilities;
using DataAggregator.DependencyInjection;
using DataAggregator.LedgerExtension;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.Generated.Model;

namespace DataAggregator.GlobalServices;

public interface ILedgerExtenderService
{
    Task<CommitTransactionsReport> CommitTransactions(StateIdentifier parentStateIdentifier, List<CommittedTransaction> committedTransactions, CancellationToken token);

    Task<TransactionSummary> GetTopOfLedger(CancellationToken token);
}

public record PreparationForLedgerExtensionReport(
    TransactionSummary ParentSummary,
    long RawTxnPersistenceMs,
    int RawTxnUpsertTouchedRecords
);

public record CommitTransactionsReport(
    int TransactionsCommittedCount,
    TransactionSummary FinalTransaction,
    long RawTxnPersistenceMs,
    long TransactionContentHandlingMs,
    long DbDependenciesLoadingMs,
    int TransactionContentDbActionsCount,
    long LocalDbContextActionsMs,
    long DbPersistanceMs,
    int DbEntriesWritten
);

public class LedgerExtenderService : ILedgerExtenderService
{
    private readonly ILogger<LedgerExtenderService> _logger;
    private readonly IDbContextFactory<AggregatorDbContext> _dbContextFactory;
    private readonly IRawTransactionWriter _rawTransactionWriter;
    private readonly IEntityDeterminer _entityDeterminer;
    private readonly INetworkConfigurationProvider _networkConfigurationProvider;

    public LedgerExtenderService(
        ILogger<LedgerExtenderService> logger,
        IDbContextFactory<AggregatorDbContext> dbContextFactory,
        IRawTransactionWriter rawTransactionWriter,
        IEntityDeterminer entityDeterminer,
        INetworkConfigurationProvider networkConfigurationProvider
    )
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _rawTransactionWriter = rawTransactionWriter;
        _entityDeterminer = entityDeterminer;
        _networkConfigurationProvider = networkConfigurationProvider;
    }

    public async Task<TransactionSummary> GetTopOfLedger(CancellationToken token)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        return await TransactionSummarisation.GetSummaryOfTransactionOnTopOfLedger(dbContext, token);
    }

    public async Task<CommitTransactionsReport> CommitTransactions(StateIdentifier parentStateIdentifier, List<CommittedTransaction> transactions, CancellationToken token)
    {
        // Create own context for the preparation unit of work.
        await using var preparationDbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var preparationReport = await PrepareForLedgerExtension(preparationDbContext, parentStateIdentifier, transactions, token);

        var preparationEntriesTouched = await preparationDbContext.SaveChangesAsync(token);

        // Create own context for ledger extension unit of work
        await using var ledgerExtensionDbContext = await _dbContextFactory.CreateDbContextAsync(token);

        var bulkTransactionCommitReport = await new BulkTransactionCommitter(_entityDeterminer, ledgerExtensionDbContext, token)
            .CommitTransactions(preparationReport.ParentSummary, transactions);

        var (ledgerExtensionEntriesWritten, dbPersistenceMs) = await CodeStopwatch.TimeInMs(
            () => ledgerExtensionDbContext.SaveChangesAsync(token)
        );

        return new CommitTransactionsReport(
            transactions.Count,
            bulkTransactionCommitReport.FinalTransaction,
            preparationReport.RawTxnPersistenceMs,
            bulkTransactionCommitReport.TransactionContentHandlingMs,
            bulkTransactionCommitReport.DbDependenciesLoadingMs,
            bulkTransactionCommitReport.TransactionContentDbActionsCount,
            bulkTransactionCommitReport.LocalDbContextActionsMs,
            dbPersistenceMs,
            preparationReport.RawTxnUpsertTouchedRecords + preparationEntriesTouched + ledgerExtensionEntriesWritten
        );
    }

    /// <summary>
    ///  This should be idempotent - ie can be repeated if the main commit task fails.
    /// </summary>
    private async Task<PreparationForLedgerExtensionReport> PrepareForLedgerExtension(
        AggregatorDbContext dbContext,
        StateIdentifier parentStateIdentifier,
        List<CommittedTransaction> transactions,
        CancellationToken token
    )
    {
        var parentSummary = await TransactionSummarisation.GetSummaryOfTransactionOnTopOfLedger(dbContext, token);
        TransactionConsistency.AssertEqualParentIdentifiers(parentStateIdentifier, parentSummary);

        if (parentSummary.StateVersion == 0)
        {
            await EnsureDbLedgerIsInitialized(token);
        }

        var rawTransactions = transactions.Select(TransactionMapping.CreateRawTransaction).ToList();

        rawTransactions.ForEach(TransactionConsistency.AssertTransactionHashCorrect);

        var (rawTransactionsTouched, rawTransactionCommitMs) = await CodeStopwatch.TimeInMs(
            () => _rawTransactionWriter.EnsureRawTransactionsCreatedOrUpdated(
                dbContext,
                rawTransactions,
                token
            )
        );

        return new PreparationForLedgerExtensionReport(
            parentSummary,
            rawTransactionCommitMs,
            rawTransactionsTouched
        );
    }

    private async Task EnsureDbLedgerIsInitialized(CancellationToken token)
    {
        var created = await _networkConfigurationProvider.SaveLedgerNetworkConfigurationToDatabaseOnInitIfNotExists(token);
        if (created)
        {
            _logger.LogInformation(
                "Ledger initialized with network: {NetworkName}",
                _networkConfigurationProvider.GetNetworkName()
            );
        }
    }
}
