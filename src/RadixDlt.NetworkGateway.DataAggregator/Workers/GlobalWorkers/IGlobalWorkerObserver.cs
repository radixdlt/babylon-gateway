using System;

namespace RadixDlt.NetworkGateway.DataAggregator.Workers.GlobalWorkers;

public interface IGlobalWorkerObserver
{
    void TrackNonFaultingExceptionInWorkLoop(Type worker, Exception exception);

    void TrackWorkerFaultedException(Type worker, Exception exception, bool isStopRequested);
}
