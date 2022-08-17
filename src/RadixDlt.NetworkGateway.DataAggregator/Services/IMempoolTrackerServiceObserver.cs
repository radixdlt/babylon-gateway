namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface IMempoolTrackerServiceObserver
{
    void CombinedMempoolCurrentSizeCount(int count);

    void TransactionsReappearedCount(int count);

    void TransactionsAddedDueToNodeMempoolAppearanceCount(int count);

    void TransactionsMarkedAsMissing();
}
