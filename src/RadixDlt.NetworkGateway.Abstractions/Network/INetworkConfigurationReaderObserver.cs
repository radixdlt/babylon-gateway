using System;
using System.Threading.Tasks;

namespace RadixDlt.NetworkGateway.Abstractions.Network;

public interface INetworkConfigurationReaderObserver
{
    ValueTask GetNetworkConfigurationFailed(string nodeName, Exception exception);
}
