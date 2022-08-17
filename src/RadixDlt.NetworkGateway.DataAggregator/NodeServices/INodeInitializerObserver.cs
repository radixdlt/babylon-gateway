using System;

namespace RadixDlt.NetworkGateway.DataAggregator.NodeServices;

public interface INodeInitializerObserver
{
    void TrackInitializerFaultedException(Type worker, string nodeName, bool isStopRequested, Exception exception);
}
