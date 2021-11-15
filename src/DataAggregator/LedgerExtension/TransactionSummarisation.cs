using Common.Database;
using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using RadixCoreApi.GeneratedClient.Model;

namespace DataAggregator.LedgerExtension;

public record TransactionSummary(
    long StateVersion,
    long Epoch,
    long IndexInEpoch,
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
        var lastOverview = await dbContext.LedgerTransactions
            .AsNoTracking()
            .OrderByDescending(lt => lt.ResultantStateVersion)
            .Select(lt => new TransactionSummary(
                lt.ResultantStateVersion,
                lt.Epoch,
                lt.IndexInEpoch,
                lt.IsEndOfEpoch,
                lt.TransactionIdentifierHash,
                lt.TransactionAccumulator,
                lt.Timestamp,
                lt.EndOfEpochRound
            ))
            .FirstOrDefaultAsync(token);

        return lastOverview ?? PreGenesisTransactionSummary();
    }

    public static TransactionSummary GenerateSummary(TransactionSummary lastTransaction, CommittedTransaction transaction)
    {
        long? newEpochForNextTransaction = null;
        long? endOfEpochRound = null;
        DateTime? newViewTimestamp = null;
        foreach (var operationGroup in transaction.OperationGroups)
        {
            foreach (var operation in operationGroup.Operations)
            {
                if (operation.Data?.Action == Data.ActionEnum.CREATE &&
                    operation.Data.DataObject is EpochData epochData)
                {
                    newEpochForNextTransaction = epochData.Epoch;
                }

                if (operation.Data?.Action == Data.ActionEnum.DELETE &&
                    operation.Data.DataObject is RoundData endRoundData)
                {
                    endOfEpochRound = endRoundData.Round;
                }

                if (operation.Data?.Action == Data.ActionEnum.CREATE &&
                    operation.Data.DataObject is RoundData newRoundData)
                {
                    newViewTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(newRoundData.Timestamp).UtcDateTime;
                }
            }
        }

        return new TransactionSummary(
            StateVersion: transaction.CommittedStateIdentifier.StateVersion,
            Epoch: lastTransaction.IsEndOfEpoch ? lastTransaction.Epoch + 1 : lastTransaction.Epoch,
            IndexInEpoch: lastTransaction.IsEndOfEpoch ? 0 : lastTransaction.IndexInEpoch + 1,
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
            TransactionIdentifierHash: Array.Empty<byte>(), // Unused
            TransactionAccumulator: new byte[32], // All 0s
            CurrentViewTimestamp: DateTime.UnixEpoch,
            EndOfEpochRound: null
        );
    }
}
