#nullable disable

using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using System.Collections.Generic;
using TransactionSubmitResponse = RadixDlt.CoreApiSdk.Model.TransactionSubmitResponse;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class CoreApiStubRequestsAndResponses
{
    public CoreApiStubRequestsAndResponses()
    {
        CreateDefault();
    }

    public string ApiVersion { get; set; }

    public CoreNodeHealthResult CoreNodeHealthResult { get; set; }

    public string CoreVersion { get; set; }

    public string GatewayApiVersion { get; set; }

    public string GatewayOpenApiSchemaVersion { get; set; }

    public CoreApiNode DataAggregatorCoreApiNode { get; set; }

    public GatewayApi.Configuration.CoreApiNode GatewayCoreApiNode { get; set; }

    public string Hash { get; set; }

    public string MempoolTransactionHash { get; set; }

    public NetworkStatusResponse NetworkStatusResponse { get; set; }

    public TransactionPreviewResponse TransactionPreviewResponse { get; set; }

    public TransactionSubmitResponse TransactionSubmitResponse { get; set; }

    private void CreateDefault()
    {
        CoreVersion = "1.0";

        ApiVersion = "2.0";

        GatewayApiVersion = "2.0.0";

        GatewayOpenApiSchemaVersion = "3.0.0";

        Hash = "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";

        MempoolTransactionHash = "0607030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1fff";

        GatewayCoreApiNode = new GatewayApi.Configuration.CoreApiNode
        {
            Enabled = true, Name = "node1", RequestWeighting = 1, CoreApiAddress = "3333",
        };

        DataAggregatorCoreApiNode = new CoreApiNode
        {
            CoreApiAddress = GatewayCoreApiNode.CoreApiAddress,
            Enabled = GatewayCoreApiNode.Enabled,
            Name = GatewayCoreApiNode.Name,
            RequestWeighting = GatewayCoreApiNode.RequestWeighting,
            TrustWeighting = 1,
        };

        TransactionSubmitResponse = new TransactionSubmitResponse();

        CoreNodeHealthResult = new CoreNodeHealthResult(
            new Dictionary<CoreNodeStatus, List<GatewayApi.Configuration.CoreApiNode>>
            {
                {
                    CoreNodeStatus.HealthyAndSynced,
                    new List<GatewayApi.Configuration.CoreApiNode> { GatewayCoreApiNode }
                },
            });

        NetworkStatusResponse = new NetworkStatusResponse(
            new CommittedStateIdentifier(1L),
            new CommittedStateIdentifier(1L),
            new CommittedStateIdentifier(1L));
    }
}
