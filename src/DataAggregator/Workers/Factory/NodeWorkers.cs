namespace DataAggregator.Workers.Factory;

public class NodeWorkers
{
    public CancellationTokenSource CancellationTokenSource { get; }
    public List<IHostedService> Workers { get; }
    public NodeWorkerStatus Status { get; private set; }
    private readonly object _statusLock = new();

    public NodeWorkers(CancellationTokenSource cancellationTokenSource, List<IHostedService> workers)
    {
        CancellationTokenSource = cancellationTokenSource;
        Workers = workers;
        Status = NodeWorkerStatus.Unstarted;
    }

    /// <summary>
    ///  Starts all workers. Throws if called more than once.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public async Task StartWorkers(CancellationToken cancellationToken)
    {
        lock (_statusLock)
        {
            if (Status != NodeWorkerStatus.Unstarted)
            {
                throw new Exception("Workers have already been started");
            }

            Status = NodeWorkerStatus.Starting;
        }

        var combinedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await Task.WhenAll(Workers.Select(w => w.StartAsync(combinedCancellationSource.Token)));
        lock (_statusLock)
        {
            switch (Status)
            {
                case NodeWorkerStatus.Unstarted:
                case NodeWorkerStatus.Starting:
                    Status = NodeWorkerStatus.Running;
                    return;
                case NodeWorkerStatus.Running:
                case NodeWorkerStatus.Stopping:
                case NodeWorkerStatus.Stopped:
                    return; // Don't change state back
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    ///  Stops all workers. It's safe to call this multiple times.
    /// </summary>
    /// <param name="nonGracefulShutdownToken"></param>
    public async Task StopWorkersSafe(CancellationToken nonGracefulShutdownToken)
    {
        lock (_statusLock)
        {
            switch (Status)
            {
                case NodeWorkerStatus.Unstarted:
                    Status = NodeWorkerStatus.Stopped;
                    return;
                case NodeWorkerStatus.Starting:
                    Status = NodeWorkerStatus.Stopping;
                    CancellationTokenSource.Cancel();
                    break; // Continue
                case NodeWorkerStatus.Running:
                    Status = NodeWorkerStatus.Stopping;
                    break; // Continue
                case NodeWorkerStatus.Stopping:
                    break; // Continue to await existing stops
                case NodeWorkerStatus.Stopped:
                    return; // Don't do anything
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        // StopAsync should be safe to be called multiple times, and will wait till each service stops
        await Task.WhenAll(Workers.Select(w => w.StopAsync(nonGracefulShutdownToken)));
        lock (_statusLock)
        {
            Status = NodeWorkerStatus.Stopped;
        }
    }
}
