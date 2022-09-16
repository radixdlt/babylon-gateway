using Moq;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;
using System.Threading;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreMocks;

public class NetworkConfigurationReaderMock : Mock<INetworkConfigurationReader>
{
    public NetworkConfigurationReaderMock()
    {
        Setup(x => x.GetNetworkConfiguration(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NetworkConfigurationResponse(
                version: new NetworkConfigurationResponseVersion(coreVersion: "1.0.0", apiVersion: "1.1.1"),
                networkIdentifier: new NetworkIdentifier(network: DbSeedHelper.NetworkName),
                bech32HumanReadableParts: new Bech32HRPs(
                    accountHrp: "accountHrp",
                    validatorHrp: "validatorHrp",
                    nodeHrp: "nodeHrp",
                    resourceHrpSuffix: "hrp")));
    }
}
