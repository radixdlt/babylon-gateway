using System;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.NodeWorkers;

public interface INodeWorkerObserver
{
    void TrackNonFaultingExceptionInWorkLoop(Type worker, string nodeName, Exception exception);

    void TrackWorkerFaultedException(Type worker, string nodeName, Exception exception, bool isStopRequested);
}
