namespace RadixDlt.NetworkGateway.Common.Model;

public enum MempoolTransactionStatus
{
    SubmittedOrKnownInNodeMempool, // We believe the transaction is in at least one node mempool, or will hopefully (just) be entering one, or has entered one
    Missing,       // A transaction which was previously SubmittedOrKnownInNodeMempool, but at last check, was past its
    // post-submission grace period, and no longer seen in any mempool.
    // After transitioning to Missing, we wait for a further delay before attempting resubmission, to allow the
    // Gateway DB time to sync and mark it committed
    ResolvedButUnknownTillSyncedUp, // A transaction has been marked as substate not found by a node at resubmission, but we've yet to see it on ledger
    // because the aggregator service is not sufficiently synced up - so we don't know if it's been committed
    // and detected itself, or clashed with another transaction.
    Failed,        // A transaction which we have tried to (re)submit, but it returns a permanent error from the node (eg substate clash)
    Committed,     // A transaction which we know got committed to the ledger
}
