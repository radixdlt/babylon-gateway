using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.Monitoring;

public interface IAggregatorHealthCheckObserver
{
    ValueTask HealthReport(bool isHealthy, bool isPrimary);
}
