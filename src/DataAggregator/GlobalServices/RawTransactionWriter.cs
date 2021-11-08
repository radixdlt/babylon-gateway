using Common.Database;
using Common.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.GlobalServices;

public interface IRawTransactionWriter
{
    Task EnsureRawTransactionsWritten(List<RawTransaction> rawTransactions, CancellationToken token);
}

public class RawTransactionWriter : IRawTransactionWriter
{
    private readonly IDbContextFactory<CommonDbContext> _contextFactory;

    public RawTransactionWriter(IDbContextFactory<CommonDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task EnsureRawTransactionsWritten(List<RawTransaction> rawTransactions, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);

        // See https://github.com/artiomchi/FlexLabs.Upsert/wiki/Usage
        context.RawTransactions
            .UpsertRange(rawTransactions)
            .WhenMatched((existingTransaction, newTransaction) => new RawTransaction(
                newTransaction.TransactionIdentifier,
                newTransaction.SubmittedTimestamp ?? existingTransaction.SubmittedTimestamp,
                newTransaction.Payload ?? existingTransaction.Payload
            ));

        await context.SaveChangesAsync(token);
    }
}
