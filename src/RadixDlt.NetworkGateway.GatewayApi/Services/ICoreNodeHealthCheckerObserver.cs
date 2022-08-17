using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using System;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.GatewayApi.Services;

public interface ICoreNodeHealthCheckerObserver
{
    ValueTask CountByStatus(int healthyAndSyncedCount, int healthyButLaggingCount, int unhealthyCount);

    void NodeUnhealthy((CoreApiNode CoreApiNode, long? NodeStateVersion, Exception? Exception) healthCheckData);

    void NodeHealthyButLagging((CoreApiNode CoreApiNode, long? NodeStateVersion, Exception? Exception) healthCheckData);

    void NodeHealthyAndSynced((CoreApiNode CoreApiNode, long? NodeStateVersion, Exception? Exception) healthCheckData);
}
