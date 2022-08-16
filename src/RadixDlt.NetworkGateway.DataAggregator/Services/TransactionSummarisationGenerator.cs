using NodaTime;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Common.CoreCommunications;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.DataAggregator.Services;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public static class TransactionSummarisationGenerator
{
    public static TransactionSummary GenerateSummary(TransactionSummary lastTransaction, CommittedTransaction transaction)
    {
        long? newEpoch = null;
        long? newRoundInEpoch = null;
        Instant? newRoundTimestamp = null;

        foreach (var operationGroup in transaction.OperationGroups)
        {
            foreach (var operation in operationGroup.Operations)
            {
                if (operation.IsCreateOf<EpochData>(out var epochData))
                {
                    newEpoch = epochData.Epoch;
                }

                if (operation.IsCreateOf<RoundData>(out var newRoundData))
                {
                    newRoundInEpoch = newRoundData.Round;

                    // NB - the first round of the ledger has Timestamp 0 for some reason. Let's ignore it and use the prev timestamp
                    if (newRoundData.Timestamp != 0)
                    {
                        newRoundTimestamp = Instant.FromUnixTimeMilliseconds(newRoundData.Timestamp);
                    }
                }
            }
        }

        /* NB:
           The Epoch Transition Transaction sort of fits between epochs, but it seems to fit slightly more naturally
           as the _first_ transaction of a new epoch, as creates the next EpochData, and the RoundData to 0.
        */

        var isStartOfEpoch = newEpoch != null;
        var isStartOfRound = newRoundInEpoch != null;

        var roundTimestamp = newRoundTimestamp ?? lastTransaction.RoundTimestamp;
        var createdTimestamp = SystemClock.Instance.GetCurrentInstant();
        var normalizedRoundTimestamp = // Clamp between lastTransaction.NormalizedTimestamp and createdTimestamp
            roundTimestamp < lastTransaction.NormalizedRoundTimestamp ? lastTransaction.NormalizedRoundTimestamp
            : roundTimestamp > createdTimestamp ? createdTimestamp
            : roundTimestamp;

        return new TransactionSummary(
            StateVersion: transaction.CommittedStateIdentifier.StateVersion,
            Epoch: newEpoch ?? lastTransaction.Epoch,
            IndexInEpoch: isStartOfEpoch ? 0 : lastTransaction.IndexInEpoch + 1,
            RoundInEpoch: newRoundInEpoch ?? lastTransaction.RoundInEpoch,
            IsStartOfEpoch: isStartOfEpoch,
            IsStartOfRound: isStartOfRound,
            PayloadHash: transaction.TransactionIdentifier.Hash.ConvertFromHex(),
            IntentHash: transaction.TransactionIdentifier.Hash.ConvertFromHex(), // TODO - Fix me when we read this from the Core API
            SignedTransactionHash: transaction.TransactionIdentifier.Hash.ConvertFromHex(), // TODO - Fix me when we read this from the Core API
            TransactionAccumulator: transaction.CommittedStateIdentifier.TransactionAccumulator.ConvertFromHex(),
            RoundTimestamp: roundTimestamp,
            CreatedTimestamp: createdTimestamp,
            NormalizedRoundTimestamp: normalizedRoundTimestamp
        );
    }
}
