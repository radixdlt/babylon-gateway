using Common.Database.Models;
using Common.Database.Models.Ledger;
using Common.Extensions;
using Common.Numerics;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.LedgerExtension;

public static class TransactionMapping
{
    public static RawTransaction CreateRawTransaction(CommittedTransaction transaction)
    {
        return new RawTransaction(
            transactionIdentifierHash: transaction.TransactionIdentifier.Hash.ConvertFromHex(),
            submittedTimestamp: null,
            payload: transaction.Metadata.Hex.ConvertFromHex()
        );
    }

    public static LedgerTransaction CreateLedgerTransaction(CommittedTransaction transaction, TransactionSummary summary)
    {
        var resultantStateVersion = summary.StateVersion;
        long? parentStateVersion = resultantStateVersion > 1 ? resultantStateVersion - 1 : null;
        return new LedgerTransaction(
            resultantStateVersion: resultantStateVersion,
            parentStateVersion: parentStateVersion,
            transactionIdentifierHash: summary.TransactionIdentifierHash,
            transactionAccumulator: summary.TransactionAccumulator,
            message: transaction.Metadata.Message?.ConvertFromHex(),
            feePaid: TokenAmount.FromString(transaction.Metadata.Fee.Value),
            epoch: summary.Epoch,
            indexInEpoch: summary.IndexInEpoch,
            isEndOfEpoch: summary.IsEndOfEpoch,
            timestamp: summary.CurrentViewTimestamp,
            endOfEpochRound: summary.EndOfEpochRound
        );
    }
}
