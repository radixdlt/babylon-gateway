using Moq;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System.Collections.Generic;
using System.Threading;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreMocks;

public class CoreNodeHealthCheckerMock : Mock<ICoreNodeHealthChecker>
{
    public CoreNodeHealthCheckerMock()
    {
        Setup(x => x.CheckCoreNodeHealth(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoreNodeHealthResult(
                new Dictionary<CoreNodeStatus, List<CoreApiNode>>()
                {
                    {
                        CoreNodeStatus.HealthyAndSynced,
                        new List<CoreApiNode>()
                        {
                            new CoreApiNode()
                            {
                                Enabled = true,
                                Name = "node1",
                                RequestWeighting = 1,
                                CoreApiAddress = "3333",
                            },
                        }
                    },
                }));
    }
}
