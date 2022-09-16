using RadixDlt.NetworkGateway.Commons.Addressing;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public sealed record TmpSomeResult();

public interface IStateQuerier
{
    Task<TmpSomeResult> TmpAccountResourcesSnapshot(RadixAddress radixAddress, LedgerState ledgerState, CancellationToken token = default);
}
