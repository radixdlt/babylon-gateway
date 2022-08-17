using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface IRawTransactionWriterObserver
{
    ValueTask TransactionsMarkedCommittedWhichWasFailed();

    ValueTask TransactionsMarkedCommittedCount(int count);
}
