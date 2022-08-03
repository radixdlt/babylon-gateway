using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers;

public class LedgerSynchronizationWorker : BackgroundService
{
    private readonly DistributedLockService _distributedLockService;
    private readonly IOptionsMonitor<NetworkGatewayDataAggregatorOptions> _optionsMonitor;

    public LedgerSynchronizationWorker(DistributedLockService distributedLockService, IOptionsMonitor<NetworkGatewayDataAggregatorOptions> optionsMonitor)
    {
        _distributedLockService = distributedLockService;
        _optionsMonitor = optionsMonitor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // assume this class is based on LoopedWorker

        using var myVar = _optionsMonitor.OnChange((dao, name) =>
        {
            Console.WriteLine($"Change on DataAggregatorOptions detected! {name}");
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!await _distributedLockService.TryAcquire(stoppingToken))
            {
                // failed to acquire lock
            }
            else
            {
                var distributedLockToken = _distributedLockService.CancellationToken;

                using var finalTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, distributedLockToken);

                var o = _optionsMonitor.CurrentValue;

                Console.WriteLine(nameof(LedgerSynchronizationWorker)
                                  + " --> "
                                  + o.NetworkName
                                  + " ("
                                  + o.CoreApiNodes.Count
                                  + ": "
                                  + string.Join(";", o.CoreApiNodes.Select(n => n.CoreApiAddress))
                                  + ")");
            }

            await Task.Delay(5_000, stoppingToken);
        }
    }
}

public class DistributedLockService : IDisposable
{
    private CancellationTokenSource? _cts;

    public CancellationToken CancellationToken => (_cts ?? throw new Exception("bla bla")).Token;

    public async Task<bool> TryAcquire(CancellationToken token = default)
    {
        await Task.Delay(1, token);

        // acquire token logic here

        if (_cts == null)
        {
            _cts = new CancellationTokenSource();
        }

        return true;
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }
}
