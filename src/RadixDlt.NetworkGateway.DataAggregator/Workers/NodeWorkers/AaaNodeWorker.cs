using Microsoft.Extensions.Logging;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices;
using RadixDlt.NetworkGateway.Workers;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

public class AaaNodeWorker : NodeWorker
{
    private static readonly IDelayBetweenLoopsStrategy _delayBetweenLoopsStrategy =
        IDelayBetweenLoopsStrategy.ExponentialDelayStrategy(
            delayBetweenLoopTriggersIfSuccessful: TimeSpan.FromMilliseconds(200),
            baseDelayAfterError: TimeSpan.FromMilliseconds(1000),
            consecutiveErrorsAllowedBeforeExponentialBackoff: 1,
            delayAfterErrorExponentialRate: 2,
            maxDelayAfterError: TimeSpan.FromSeconds(30));

    public AaaNodeWorker(ILogger logger, INodeConfigProvider nodeConfigProvider)
        : base(logger, nodeConfigProvider.CoreApiNode.Name, _delayBetweenLoopsStrategy, TimeSpan.FromSeconds(60))
    {
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override bool IsEnabledByNodeConfiguration()
    {
        throw new NotImplementedException();
    }
}
