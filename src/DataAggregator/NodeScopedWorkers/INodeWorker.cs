namespace DataAggregator.NodeScopedWorkers;

/// <summary>
/// A marker interface for NodeWorkers, so Dependency Injection can pick them up.
/// </summary>
public interface INodeWorker : IHostedService, IDisposable
{
}
