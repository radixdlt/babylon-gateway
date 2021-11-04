using Shared.Utilities;

namespace DataAggregator.GlobalWorkers;

/// <summary>
/// Responsible for syncing the transaction stream from a node.
/// </summary>
public abstract class LoopedWorkerBase : BackgroundService
{
    private readonly LogLimiter _logLimiter;
    private readonly ILogger _logger;
    private readonly TimeSpan _minDelayBetweenLoops;

    public LoopedWorkerBase(ILogger logger, TimeSpan minDelayBetweenLoops, TimeSpan minDelayBetweenInfoLogs)
    {
        _logger = logger;
        _minDelayBetweenLoops = minDelayBetweenLoops;
        _logLimiter = new LogLimiter(minDelayBetweenInfoLogs, LogLevel.Information, LogLevel.Debug);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Log(_logLimiter.GetLogLevel(), "Starting at: {Time}", DateTimeOffset.Now);
        var finished = false;

        while (!stoppingToken.IsCancellationRequested && !finished)
        {
            try
            {
                await ExecuteLoopIteration(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // We catch the TaskCancelledException so we get the Stopping message :)
                finished = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loop iteration errored at: {Time}", DateTimeOffset.Now);
            }
        }

        await OnStop(stoppingToken);
    }

    protected abstract Task DoWork(CancellationToken stoppingToken);

    protected virtual Task OnStop(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping at: {Time}", DateTimeOffset.Now);
        return Task.CompletedTask;
    }

    private async Task ExecuteLoopIteration(CancellationToken stoppingToken)
    {
        var minDelayBetweenLoops = Task.Delay(_minDelayBetweenLoops, stoppingToken);
        _logger.Log(_logLimiter.GetLogLevel(),  "Still running at {Time}", DateTimeOffset.Now);
        await DoWork(stoppingToken);
        await minDelayBetweenLoops;
    }
}
