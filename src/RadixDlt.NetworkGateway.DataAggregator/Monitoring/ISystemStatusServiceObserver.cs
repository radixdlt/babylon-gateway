namespace RadixDlt.NetworkGateway.DataAggregator.Monitoring;

public interface ISystemStatusServiceObserver
{
    void SetIsPrimary(bool isPrimary);
}
