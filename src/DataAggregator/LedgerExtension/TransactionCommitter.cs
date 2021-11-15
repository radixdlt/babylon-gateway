using Common.Database;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.LedgerExtension;

public interface ITransactionCommitter
{
    Task CommitTransactions(TransactionSummary parentSummary, List<CommittedTransaction> committedTransactions);
}

/// <summary>
/// A short-lived class, used to commit a batch of transactions to the database.
/// </summary>
public class TransactionCommitter : ITransactionCommitter
{
    private readonly CommonDbContext _dbContext;
    private readonly CancellationToken _cancellationToken;

    public TransactionCommitter(CommonDbContext dbContext, CancellationToken cancellationToken)
    {
        _dbContext = dbContext;
        _cancellationToken = cancellationToken;
    }

    public async Task CommitTransactions(TransactionSummary parentSummary, List<CommittedTransaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            var summary = TransactionSummarisation.GenerateSummary(parentSummary, transaction);
            TransactionConsistency.AssertChildTransactionConsistent(parentSummary, summary);

            CommitCheckedTransaction(transaction, summary);
            parentSummary = summary;
        }

        await _dbContext.SaveChangesAsync(_cancellationToken);
    }

    private async void CommitCheckedTransaction(CommittedTransaction transaction, TransactionSummary summary)
    {
        var ledgerTransaction = TransactionMapping.CreateLedgerTransaction(transaction, summary);
        await _dbContext.LedgerTransactions.AddAsync(ledgerTransaction, _cancellationToken);
    }
}
