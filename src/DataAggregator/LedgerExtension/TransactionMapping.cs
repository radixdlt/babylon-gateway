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
        var fee = transaction.Metadata.Fee == null
            ? TokenAmount.Zero
            : TokenAmount.FromSubUnitsString(transaction.Metadata.Fee.Value);
        var signedBy = transaction.Metadata.SignedBy?.Hex.ConvertFromHex();
        return new LedgerTransaction(
            resultantStateVersion: resultantStateVersion,
            parentStateVersion: parentStateVersion,
            transactionIdentifierHash: summary.TransactionIdentifierHash,
            transactionAccumulator: summary.TransactionAccumulator,
            message: transaction.Metadata.Message?.ConvertFromHex(),
            feePaid: fee,
            signedBy: signedBy,
            epoch: summary.Epoch,
            indexInEpoch: summary.IndexInEpoch,
            isOnlyRoundChange: summary.IsOnlyRoundChange,
            isEndOfEpoch: summary.IsEndOfEpoch,
            timestamp: summary.CurrentViewTimestamp,
            endOfEpochRound: summary.EndOfEpochRound
        );
    }
}
