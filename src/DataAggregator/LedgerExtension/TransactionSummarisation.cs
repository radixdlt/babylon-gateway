using Common.Database;
using Common.Extensions;
using DataAggregator.Extensions;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.LedgerExtension;

public record TransactionSummary(
    long StateVersion,
    long Epoch,
    long IndexInEpoch,
    bool IsOnlyRoundChange,
    bool IsEndOfEpoch,
    byte[] TransactionIdentifierHash,
    byte[] TransactionAccumulator,
    DateTime CurrentViewTimestamp,
    long? EndOfEpochRound
);

public static class TransactionSummarisation
{
    public static async Task<TransactionSummary> GetSummaryOfTransactionOnTopOfLedger(CommonDbContext dbContext, CancellationToken token)
    {
        var lastTransaction = await dbContext.LedgerTransactions
            .AsNoTracking()
            .OrderByDescending(lt => lt.ResultantStateVersion)
            .FirstOrDefaultAsync(token);

        var lastOverview = lastTransaction == null ? null : new TransactionSummary(
            lastTransaction.ResultantStateVersion,
            lastTransaction.Epoch,
            lastTransaction.IndexInEpoch,
            lastTransaction.IsOnlyRoundChange,
            lastTransaction.IsEndOfEpoch,
            lastTransaction.TransactionIdentifierHash,
            lastTransaction.TransactionAccumulator,
            lastTransaction.Timestamp,
            lastTransaction.EndOfEpochRound
        );

        return lastOverview ?? PreGenesisTransactionSummary();
    }

    public static TransactionSummary GenerateSummary(TransactionSummary lastTransaction, CommittedTransaction transaction)
    {
        long? newEpochForNextTransaction = null;
        long? endOfEpochRound = null;
        DateTime? newViewTimestamp = null;
        var isOnlyRoundChange = true;
        foreach (var operationGroup in transaction.OperationGroups)
        {
            foreach (var operation in operationGroup.Operations)
            {
                if (operation.IsNotRoundDataOrValidatorBftData())
                {
                    isOnlyRoundChange = false;
                }

                if (operation.IsCreateOf<EpochData>(out var epochData))
                {
                    newEpochForNextTransaction = epochData.Epoch;
                }

                if (operation.IsDeleteOf<RoundData>(out var endRoundData))
                {
                    endOfEpochRound = endRoundData.Round;
                }

                // NB - the first view of the ledger has Timestamp 0 for some reason. Let's filter it out.
                if (operation.IsCreateOf<RoundData>(out var newRoundData) && newRoundData.Timestamp != 0)
                {
                    newViewTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(newRoundData.Timestamp).UtcDateTime;
                }
            }
        }

        return new TransactionSummary(
            StateVersion: transaction.CommittedStateIdentifier.StateVersion,
            Epoch: lastTransaction.IsEndOfEpoch ? lastTransaction.Epoch + 1 : lastTransaction.Epoch,
            IndexInEpoch: lastTransaction.IsEndOfEpoch ? 0 : lastTransaction.IndexInEpoch + 1,
            IsOnlyRoundChange: isOnlyRoundChange,
            IsEndOfEpoch: newEpochForNextTransaction != null,
            TransactionIdentifierHash: transaction.TransactionIdentifier.Hash.ConvertFromHex(),
            TransactionAccumulator: transaction.CommittedStateIdentifier.TransactionAccumulator.ConvertFromHex(),
            CurrentViewTimestamp: newViewTimestamp ?? lastTransaction.CurrentViewTimestamp,
            EndOfEpochRound: endOfEpochRound
        );
    }

    private static TransactionSummary PreGenesisTransactionSummary()
    {
        return new TransactionSummary(
            StateVersion: 0,
            Epoch: 0,
            IndexInEpoch: -1, // Sight hack to make the first transaction be index 0 in Epoch 0
            IsEndOfEpoch: false,
            IsOnlyRoundChange: false,
            TransactionIdentifierHash: Array.Empty<byte>(), // Unused
            TransactionAccumulator: new byte[32], // All 0s
            CurrentViewTimestamp: DateTime.UnixEpoch,
            EndOfEpochRound: null
        );
    }
}
