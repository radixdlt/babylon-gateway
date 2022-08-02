using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RadixDlt.NetworkGateway.Configuration;

namespace RadixDlt.NetworkGateway;

public class LedgerSynchronizationWorker : BackgroundService
{
    private readonly IOptionsMonitor<DataAggregatorOptions> _optionsMonitor;

    public LedgerSynchronizationWorker(IOptionsMonitor<DataAggregatorOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var finalTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            Console.WriteLine(nameof(LedgerSynchronizationWorker) + " --> " + _optionsMonitor.CurrentValue.NetworkName);

            await Task.Delay(5_000, finalTokenSource.Token);
        }
    }
}
