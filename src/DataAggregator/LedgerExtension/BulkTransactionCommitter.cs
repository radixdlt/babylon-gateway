using Common.Database;
using DataAggregator.Extensions;
using DataAggregator.GlobalServices;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.LedgerExtension;

public interface IBulkTransactionCommitter
{
    Task CommitTransactions(TransactionSummary parentSummary, List<CommittedTransaction> committedTransactions);
}

/// <summary>
/// A short-lived class, used to commit a batch of transactions to the database.
/// </summary>
public class BulkTransactionCommitter : IBulkTransactionCommitter
{
    private readonly IEntityDeterminer _entityDeterminer;
    private readonly CommonDbContext _dbContext;
    private readonly CancellationToken _cancellationToken;

    public BulkTransactionCommitter(
        IEntityDeterminer entityDeterminer,
        CommonDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        _entityDeterminer = entityDeterminer;
        _dbContext = dbContext;
        _cancellationToken = cancellationToken;
    }

    public async Task CommitTransactions(TransactionSummary parentSummary, List<CommittedTransaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            var summary = TransactionSummarisation.GenerateSummary(parentSummary, transaction);
            TransactionConsistency.AssertChildTransactionConsistent(parentSummary, summary);

            await CommitCheckedTransaction(transaction, summary);
            parentSummary = summary;
        }

        await _dbContext.SaveChangesAsync(_cancellationToken);
    }

    private async Task CommitCheckedTransaction(CommittedTransaction transaction, TransactionSummary summary)
    {
        var ledgerTransaction = TransactionMapping.CreateLedgerTransaction(transaction, summary);
        _dbContext.LedgerTransactions.Add(ledgerTransaction);

        if (!transaction.HasSubstantiveOperations())
        {
            return;
        }

        var transactionOperationExtractor = new TransactionContentCommitter(_dbContext, _entityDeterminer, _cancellationToken);
        await transactionOperationExtractor.CommitTransactionDetails(transaction, summary);
    }
}
