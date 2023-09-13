using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface ITopOfLedgerProvider
{
    Task<TransactionSummary> GetTopOfLedger(CancellationToken token);
}
