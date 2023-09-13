using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Services;

public interface ICommittedStateIdentifiersReader
{
    Task<Abstractions.CommittedStateIdentifiers?> GetStateIdentifiersForStateVersion(long stateVersion, CancellationToken cancellationToken);
}
