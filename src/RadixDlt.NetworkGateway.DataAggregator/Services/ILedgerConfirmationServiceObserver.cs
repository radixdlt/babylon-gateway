using NodaTime;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface ILedgerConfirmationServiceObserver
{
    void ResetQuorum();

    void TrustWeightingRequirementsComputed(LedgerConfirmationService.TrustWeightingReport report);

    ValueTask PreHandleLedgerExtensionIfQuorum(Instant timestamp);

    void PreSubmitNodeNetworkStatus(string nodeName, long ledgerTipStateVersion, long targetStateVersion);

    void SubmitNodeNetworkStatusUnknown(string nodeName, long ledgerTipStateVersion, long targetStateVersion);

    void SubmitNodeNetworkStatusUpToDate(string nodeName, long ledgerTipStateVersion, long targetStateVersion);

    void SubmitNodeNetworkStatusOutOfDate(string nodeName, long ledgerTipStateVersion, long targetStateVersion);

    void LedgerTipInconsistentWithQuorumStatus(string inconsistentNodeName);

    void LedgerTipConsistentWithQuorumStatus(string consistentNodeName);

    void UnknownQuorumStatus();

    void QuorumLost();

    void QuorumGained();

    void ReportOnLedgerExtensionSuccess(Instant timestamp, Duration parentSummaryRoundTimestamp, long totalCommitMs, int transactionsCommittedCount);

    void RecordTopOfDbLedger(long stateVersion, Instant roundTimestamp);

    void QuorumExtensionConsistentGained();

    void QuorumExtensionConsistentLost();
}
