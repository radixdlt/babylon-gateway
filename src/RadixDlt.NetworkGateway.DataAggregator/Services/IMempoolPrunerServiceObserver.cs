using System.Collections.Generic;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface IMempoolPrunerServiceObserver
{
    ValueTask PreMempoolPrune(List<MempoolStatusCount> mempoolCountByStatus);

    ValueTask PreMempoolTransactionPruned(int count);
}

public record MempoolStatusCount(string Status, int Count);
