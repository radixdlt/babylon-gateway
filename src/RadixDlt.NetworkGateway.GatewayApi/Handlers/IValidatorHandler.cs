using System.Threading;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Handlers;

public interface IValidatorHandler
{
    Task<GatewayApiSdk.Model.ValidatorsUptimeResponse> Uptime(GatewayApiSdk.Model.ValidatorsUptimeRequest request, CancellationToken token);
}
