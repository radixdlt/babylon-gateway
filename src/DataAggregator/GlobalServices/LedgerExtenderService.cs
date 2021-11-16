using Common.Database;
using DataAggregator.LedgerExtension;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;
using System.Diagnostics.CodeAnalysis;

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
    private readonly IEntityDeterminer _entityDeterminer;

    public LedgerExtenderService(
        IDbContextFactory<CommonDbContext> dbContextFactory,
        IRawTransactionWriter rawTransactionWriter,
        IEntityDeterminer entityDeterminer
    )
    {
        _dbContextFactory = dbContextFactory;
        _rawTransactionWriter = rawTransactionWriter;
        _entityDeterminer = entityDeterminer;
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
            transactions.Select(TransactionMapping.CreateRawTransaction),
            token
        );

        await new BulkTransactionCommitter(_entityDeterminer, dbContext, token)
            .CommitTransactions(parentSummary, transactions);
    }
}
