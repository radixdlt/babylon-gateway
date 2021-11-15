using Common.Database;
using Common.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAggregator.GlobalServices;

public interface IRawTransactionWriter
{
    Task EnsureRawTransactionsCreatedOrUpdated(CommonDbContext context, IEnumerable<RawTransaction> rawTransactions, CancellationToken token);
}

public class RawTransactionWriter : IRawTransactionWriter
{
    public async Task EnsureRawTransactionsCreatedOrUpdated(CommonDbContext context, IEnumerable<RawTransaction> rawTransactions, CancellationToken token)
    {
        // See https://github.com/artiomchi/FlexLabs.Upsert/wiki/Usage
        await context.RawTransactions
            .UpsertRange(rawTransactions)
            .WhenMatched((existingTransaction, newTransaction) => new RawTransaction
            {
                TransactionIdentifierHash = newTransaction.TransactionIdentifierHash,
                SubmittedTimestamp = newTransaction.SubmittedTimestamp ?? existingTransaction.SubmittedTimestamp,
                Payload = newTransaction.Payload ?? existingTransaction.Payload,
            })
            .RunAsync(token);

        await context.SaveChangesAsync(token);
    }
}
