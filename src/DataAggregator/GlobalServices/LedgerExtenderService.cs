using Common.Database;
using Common.Database.Models;
using Common.Database.Models.Ledger;
using Common.Extensions;
using Common.Numerics;
using Common.StaticHelpers;
using DataAggregator.Exceptions;
using DataAggregator.LedgerExtension;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;
using System;

namespace DataAggregator.GlobalServices;

public interface ILedgerExtenderService
{
    Task CommitTransactions(StateIdentifier parentStateIdentifier, List<CommittedTransaction> committedTransactions, CancellationToken token);

    Task<long> GetTopOfLedgerStateVersion(CancellationToken token);
}

public class LedgerExtenderService : ILedgerExtenderService
{
    private readonly IDbContextFactory<CommonDbContext> _dbContextFactory;
    private readonly IRawTransactionWriter _rawTransactionWriter;

    public LedgerExtenderService(
        IDbContextFactory<CommonDbContext> dbContextFactory,
        IRawTransactionWriter rawTransactionWriter
    )
    {
        _dbContextFactory = dbContextFactory;
        _rawTransactionWriter = rawTransactionWriter;
    }

    public async Task<long> GetTopOfLedgerStateVersion(CancellationToken token)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);
        var lastTransactionOverview = await TransactionSummarisation.GetSummaryOfTransactionOnTopOfLedger(dbContext, token);
        return lastTransactionOverview.StateVersion;
    }

    public async Task CommitTransactions(StateIdentifier parentStateIdentifier, List<CommittedTransaction> transactions, CancellationToken token)
    {
        // Create own context for this unit of work.
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(token);

        // Check top of ledger
        var parentSummary = await TransactionSummarisation.GetSummaryOfTransactionOnTopOfLedger(dbContext, token);
        TransactionConsistency.AssertEqualParentIdentifiers(parentStateIdentifier, parentSummary);

        await _rawTransactionWriter.EnsureRawTransactionsCreatedOrUpdated(
            dbContext,
            transactions.Select(LedgerExtension.TransactionMapping.CreateRawTransaction),
            token
        );

        await new TransactionCommitter(dbContext, token).CommitTransactions(parentSummary, transactions);
    }
}
