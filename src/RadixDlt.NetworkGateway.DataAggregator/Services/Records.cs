using NodaTime;
using RadixDlt.CoreApiSdk.Model;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public record TransactionSummary(
    long StateVersion,
    long Epoch,
    long IndexInEpoch,
    long RoundInEpoch,
    bool IsStartOfEpoch,
    bool IsStartOfRound,
    byte[] PayloadHash,
    byte[] IntentHash,
    byte[] SignedTransactionHash,
    byte[] TransactionAccumulator,
    Instant RoundTimestamp,
    Instant CreatedTimestamp,
    Instant NormalizedRoundTimestamp
);

public record CommittedTransactionData(
    CommittedTransaction CommittedTransaction,
    TransactionSummary TransactionSummary,
    byte[] TransactionContents
);

public record SyncTargetCarrier(
    long TargetStateVersion
);
