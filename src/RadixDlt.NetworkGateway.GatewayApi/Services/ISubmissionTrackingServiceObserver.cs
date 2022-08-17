using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface ISubmissionTrackingServiceObserver
{
    ValueTask PostMempoolTransactionAdded();

    ValueTask PostMempoolTransactionMarkedAsFailed();
}
