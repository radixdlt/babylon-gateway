using Common.Database;
using Common.Database.Models;
using Common.Extensions;
using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;
using System.Numerics;

namespace DataAggregator.GlobalServices;

public interface ITransactionCommitter
{
    Task CommitTransactions(List<CommittedTransaction> committedTransactions, CancellationToken token);
}

public class TransactionCommitter : ITransactionCommitter
{
    private readonly IDbContextFactory<CommonDbContext> _contextFactory;
    private readonly IRawTransactionWriter _rawTransactionWriter;

    public TransactionCommitter(
        IDbContextFactory<CommonDbContext> contextFactory,
        IRawTransactionWriter rawTransactionWriter)
    {
        _contextFactory = contextFactory;
        _rawTransactionWriter = rawTransactionWriter;
    }

    public async Task CommitTransactions(List<CommittedTransaction> committedTransactions, CancellationToken token)
    {
        var rawTransactions = committedTransactions.Select(CreateRawTransaction).ToList();
        await _rawTransactionWriter.EnsureRawTransactionsCreatedOrUpdated(rawTransactions, token);

        // Create own context for this transaction
        await using var context = await _contextFactory.CreateDbContextAsync(token);

        // var currentContext = new
        // {
        //     epoch = 0,
        //     nextEpoch = 0,
        // };
        foreach (var committedTransaction in committedTransactions)
        {
            var ledgerTransaction = CreateLedgerTransactionShell(committedTransaction);
            await context.LedgerTransactions.AddAsync(ledgerTransaction, token);
            await context.SaveChangesAsync(token); // Attempt to save to avoid violating constraint
        }

        await context.SaveChangesAsync(token);
    }

    private static RawTransaction CreateRawTransaction(CommittedTransaction transaction)
    {
        return new RawTransaction
        {
            TransactionIdentifier = transaction.TransactionIdentifier.ConvertFromHex(),
            Payload = transaction.Metadata.Hex.ConvertFromHex(),
        };
    }

    private static LedgerTransaction CreateLedgerTransactionShell(CommittedTransaction transaction)
    {
        var transactionIndex = transaction.CommittedStateIdentifier.StateVersion - 1;
        long? parentTransactionIndex = transactionIndex > 0 ? transactionIndex - 1 : null;
        return new LedgerTransaction(
            transactionIndex: transactionIndex,
            parentTransactionIndex: parentTransactionIndex,
            transactionIdentifier: transaction.TransactionIdentifier.ConvertFromHex(),
            transactionAccumulator: transaction.CommittedStateIdentifier.TransactionAccumulator.ConvertFromHex(),
            resultantStateVersion: transaction.CommittedStateIdentifier.StateVersion,
            message: transaction.Metadata.Message?.ConvertFromHex(),
            feePaid: TokenAmount.FromString(transaction.Metadata.Fee),
            epoch: 0, // TODO - fix!
            indexInEpoch: 0, // TODO - fix!
            isEndOfEpoch: false, // TODO - fix!
            timestamp: DateTimeOffset.FromUnixTimeMilliseconds(transaction.Metadata.Timestamp).UtcDateTime
        );
    }
}
