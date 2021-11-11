using Common.Database;
using Common.Database.Models;
using Common.Database.Models.Ledger;
using Common.Extensions;
using Common.Numerics;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.GlobalServices;

public interface ITransactionCommitter
{
    Task CommitTransactions(List<CommittedTransaction> committedTransactions, CancellationToken token);
}

public class TransactionCommitter : ITransactionCommitter
{
    private readonly IDbContextFactory<CommonDbContext> _contextFactory;
    private readonly IRawTransactionWriter _rawTransactionWriter;
    private readonly IAddressExtractor _addressExtractor;

    public TransactionCommitter(
        IDbContextFactory<CommonDbContext> contextFactory,
        IRawTransactionWriter rawTransactionWriter,
        IAddressExtractor addressExtractor)
    {
        _contextFactory = contextFactory;
        _rawTransactionWriter = rawTransactionWriter;
        _addressExtractor = addressExtractor;
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
        return new RawTransaction(
            transactionIdentifierHash: transaction.TransactionIdentifier.Hash.ConvertFromHex(),
            submittedTimestamp: null,
            payload: transaction.Metadata.Hex.ConvertFromHex()
        );
    }

    private void HandleOperationGroups(CommittedTransaction transaction)
    {
        foreach (var operationGroup in transaction.OperationGroups)
        {
            foreach (var operation in operationGroup.Operations)
            {
                var mainAddress = _addressExtractor.Extract(operation?.AddressIdentifier?.Address);
                var subAddress = _addressExtractor.Extract(operation?.AddressIdentifier?.SubAddress?.Address);

                // Substate ValidatorStakeData has both amount and data - the validator's stake and its fee (more efficient / faster to be on one UTXO)
                var data = operation?.Data;
                var amount = operation?.Amount;
            }
        }
    }

    private static long stateVersion = 0; // TODO - fix me (!)

    private static LedgerTransaction CreateLedgerTransactionShell(CommittedTransaction transaction)
    {
        stateVersion++;
        var resultantStateVersion = stateVersion;
        long? parentStateVersion = resultantStateVersion > 1 ? resultantStateVersion - 1 : null;
        return new LedgerTransaction(
            resultantStateVersion: resultantStateVersion,
            parentStateVersion: parentStateVersion,
            transactionIdentifierHash: transaction.TransactionIdentifier.Hash.ConvertFromHex(),
            transactionAccumulator: Array.Empty<byte>(), // TODO - fix!
            message: transaction.Metadata.Message?.ConvertFromHex(),
            feePaid: TokenAmount.FromString(transaction.Metadata.Fee),
            epoch: 0, // TODO - fix!
            indexInEpoch: 0, // TODO - fix!
            isEndOfEpoch: false, // TODO - fix!
            timestamp: DateTimeOffset.FromUnixTimeMilliseconds(transaction.Metadata.Timestamp).UtcDateTime,
            endOfEpochRound: null // TODO - fix!
        );
    }
}
