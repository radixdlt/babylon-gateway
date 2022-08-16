using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface ILedgerExtenderService
{
    Task<CommitTransactionsReport> CommitTransactions(
        ConsistentLedgerExtension ledgerExtension,
        SyncTargetCarrier latestSyncTarget,
        CancellationToken token
    );

    Task<TransactionSummary> GetTopOfLedger(CancellationToken token);
}

public record ConsistentLedgerExtension(
    TransactionSummary ParentSummary,
    List<CommittedTransactionData> TransactionData
);

public record CommitTransactionsReport(
    int TransactionsCommittedCount,
    TransactionSummary FinalTransaction,
    long RawTxnPersistenceMs,
    long MempoolTransactionUpdateMs,
    long TransactionContentHandlingMs,
    long DbDependenciesLoadingMs,
    int TransactionContentDbActionsCount,
    long LocalDbContextActionsMs,
    long DbPersistanceMs,
    int DbEntriesWritten
);
