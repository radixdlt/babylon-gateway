using System;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;

public interface INetworkConfigurationReaderObserver
{
    ValueTask GetNetworkConfigurationFailed(string nodeName, Exception exception);
}
