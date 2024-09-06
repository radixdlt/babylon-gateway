using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface IPackageQuerier
{
    Task<GatewayApiSdk.Model.StatePackageCodePageResponse?> PackageCodes(
        IEntityStateQuerier.PageRequest pageRequest,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default);

    Task<GatewayApiSdk.Model.StatePackageBlueprintPageResponse?> PackageBlueprints(
        IEntityStateQuerier.PageRequest pageRequest,
        GatewayApiSdk.Model.LedgerState ledgerState,
        CancellationToken token = default);
}
