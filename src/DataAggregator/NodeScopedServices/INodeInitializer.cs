namespace DataAggregator.NodeScopedServices;

public interface INodeInitializer
{
    public Task Initialize(CancellationToken token);
}
