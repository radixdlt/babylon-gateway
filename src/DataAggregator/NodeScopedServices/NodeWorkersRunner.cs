using DataAggregator.NodeScopedWorkers;

namespace DataAggregator.NodeScopedServices;

public enum NodeWorkersRunnerStatus
{
    Uninitialized,
    Initializing,
    Initialized,
    Starting,
    Running,
    Stopping,
    Stopped,
}

/// <summary>
/// Note - this is created directly and not injected by DI - as it manages the NodeScope as well as the workers.
/// </summary>
public class NodeWorkersRunner : IDisposable
{
    public NodeWorkersRunnerStatus Status { get; private set; }

    private readonly List<INodeInitializer> _initializers;

    private readonly List<INodeWorker> _workers;

    private readonly object _statusLock = new();

    private readonly ILogger<NodeWorkersRunner> _logger;

    private CancellationTokenSource? _cancellationTokenSource;

    private IServiceScope? _nodeDependencyInjectionScope;

    private IDisposable? _logScope;

    public NodeWorkersRunner(ILogger<NodeWorkersRunner> logger, IServiceScope nodeDependencyInjectionScope, IDisposable logScope)
    {
        _logger = logger;
        _nodeDependencyInjectionScope = nodeDependencyInjectionScope;
        _logScope = logScope;
        _cancellationTokenSource = new CancellationTokenSource();
        _initializers = nodeDependencyInjectionScope.ServiceProvider.GetServices<INodeInitializer>().ToList();
        _workers = nodeDependencyInjectionScope.ServiceProvider.GetServices<INodeWorker>().ToList();
        Status = NodeWorkersRunnerStatus.Uninitialized;
    }

    /// <summary>
    ///  Runs all INodeInitializers and waits for them to complete.
    ///  Should be awaited before workers are started. Can throw if initialization fails.
    ///  If this throws, it is the caller's duty to call StopWorkersSafe or Dispose().
    /// </summary>
    public async Task Initialize(CancellationToken cancellationToken)
    {
        lock (_statusLock)
        {
            switch (Status)
            {
                case NodeWorkersRunnerStatus.Uninitialized:
                    break; // The good case
                case NodeWorkersRunnerStatus.Initializing:
                case NodeWorkersRunnerStatus.Initialized:
                case NodeWorkersRunnerStatus.Starting:
                case NodeWorkersRunnerStatus.Running:
                case NodeWorkersRunnerStatus.Stopping:
                case NodeWorkersRunnerStatus.Stopped:
                    throw new Exception("Node set-up has already been initialized");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Status = NodeWorkersRunnerStatus.Starting;
        }

        using var combinedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // ReSharper disable once AccessToDisposedClosure
        // Should be safe because Task.WhenAll waits till all tasks have run (even if one faults)
        // So the Dispose call will happen after all references to the Tokens have been used up
        await Task.WhenAll(_initializers.Select(i => i.Initialize(combinedCancellationSource.Token)));

        lock (_statusLock)
        {
            switch (Status)
            {
                case NodeWorkersRunnerStatus.Uninitialized:
                case NodeWorkersRunnerStatus.Initializing:
                    Status = NodeWorkersRunnerStatus.Initialized;
                    return;
                case NodeWorkersRunnerStatus.Initialized:
                case NodeWorkersRunnerStatus.Starting:
                case NodeWorkersRunnerStatus.Running:
                case NodeWorkersRunnerStatus.Stopping:
                case NodeWorkersRunnerStatus.Stopped:
                    return; // Don't change state back
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    ///  Starts all workers. Throws if called more than once.
    /// </summary>
    public async Task StartWorkers(CancellationToken cancellationToken)
    {
        lock (_statusLock)
        {
            switch (Status)
            {
                case NodeWorkersRunnerStatus.Uninitialized:
                case NodeWorkersRunnerStatus.Initializing:
                    throw new Exception("Should be initialized first");
                case NodeWorkersRunnerStatus.Initialized:
                    break; // The good case
                case NodeWorkersRunnerStatus.Starting:
                case NodeWorkersRunnerStatus.Running:
                case NodeWorkersRunnerStatus.Stopping:
                case NodeWorkersRunnerStatus.Stopped:
                    throw new Exception("Workers have already been started");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Status = NodeWorkersRunnerStatus.Starting;
        }

        using var combinedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // ReSharper disable once AccessToDisposedClosure
        // Should be safe because Task.WhenAll waits till all tasks have run (even if one  faults)
        // So the Dispose call will happen after all references to the Tokens have been used up
        await Task.WhenAll(_workers.Select(w => w.StartAsync(combinedCancellationSource.Token)));

        lock (_statusLock)
        {
            switch (Status)
            {
                case NodeWorkersRunnerStatus.Uninitialized:
                case NodeWorkersRunnerStatus.Initializing:
                case NodeWorkersRunnerStatus.Initialized:
                case NodeWorkersRunnerStatus.Starting:
                    Status = NodeWorkersRunnerStatus.Running;
                    return;
                case NodeWorkersRunnerStatus.Running:
                case NodeWorkersRunnerStatus.Stopping:
                case NodeWorkersRunnerStatus.Stopped:
                    return; // Don't change state back
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    ///  Stops all workers. It's safe to call this multiple times.
    /// </summary>
    public async Task StopWorkersSafe(CancellationToken nonGracefulShutdownToken)
    {
        if (EnsureServicesAreStoppingOrStoppedAndReturnTrueIfAlreadyStopped())
        {
            return;
        }

        // StopAsync should be safe to be called multiple times, and will wait till each service stops
        await Task.WhenAll(_workers.Select(w => w.StopAsync(nonGracefulShutdownToken)));

        lock (_statusLock)
        {
            Status = NodeWorkersRunnerStatus.Stopped;
        }
    }

    public void Dispose()
    {
        _logger.LogDebug("Disposing...");
        EnsureServicesAreStoppingOrStoppedAndReturnTrueIfAlreadyStopped();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _nodeDependencyInjectionScope?.Dispose();
        _nodeDependencyInjectionScope = null;
        _logScope?.Dispose();
        _logScope = null;
        _logger.LogDebug("Disposing complete");
    }

    private bool EnsureServicesAreStoppingOrStoppedAndReturnTrueIfAlreadyStopped()
    {
        lock (_statusLock)
        {
            switch (Status)
            {
                case NodeWorkersRunnerStatus.Uninitialized:
                case NodeWorkersRunnerStatus.Initialized:
                    Status = NodeWorkersRunnerStatus.Stopped;
                    return true;
                case NodeWorkersRunnerStatus.Initializing:
                case NodeWorkersRunnerStatus.Starting:
                case NodeWorkersRunnerStatus.Running:
                    Status = NodeWorkersRunnerStatus.Stopping;
                    _cancellationTokenSource?.Cancel();
                    return false;
                case NodeWorkersRunnerStatus.Stopping:
                    return false;
                case NodeWorkersRunnerStatus.Stopped:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
