using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface ITopOfLedgerProvider
{
    Task<TransactionSummary> GetTopOfLedger(CancellationToken token);
}

public interface ICommittedStateIdentifiersReader
{
    Task<Abstractions.CommittedStateIdentifiers?> GetStateIdentifiersForStateVersion(long stateVersion, CancellationToken cancellationToken);
}
