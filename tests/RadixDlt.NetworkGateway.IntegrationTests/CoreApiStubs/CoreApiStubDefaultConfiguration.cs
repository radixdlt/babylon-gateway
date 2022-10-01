#nullable disable

using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.DataAggregator.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using System.Collections.Generic;
using TransactionSubmitResponse = RadixDlt.CoreApiSdk.Model.TransactionSubmitResponse;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class CoreApiStubDefaultConfiguration
{
    public CoreApiStubDefaultConfiguration()
    {
        CreateDefault();
    }

    public string ApiVersion { get; set; }

    public CommittedTransactionsResponse CommittedTransactionsResponse { get; set; }

    public CommittedTransactionsResponse CommittedGenesisTransactionsResponse { get; set; }

    public CoreNodeHealthResult CoreNodeHealthResult { get; set; }

    public string CoreVersion { get; set; }

    public string GatewayApiVersion { get; set; }

    public string GatewayOpenApiSchemaVersion { get; set; }

    public CoreApiNode DataAggregatorCoreApiNode { get; set; }

    public GatewayApi.Configuration.CoreApiNode GatewayCoreApiNode { get; set; }

    public string Hash { get; set; }

    public string MempoolTransactionHash { get; set; }

    public MempoolTransactionStatus MempoolTransactionStatus { get; set; }

    public NetworkConfigurationResponse NetworkConfigurationResponse { get; set; }

    public string NetworkName { get; set; }

    public NetworkStatusResponse NetworkStatusResponse { get; set; }

    public string SubmitTransaction { get; set; }

    public RadixDlt.NetworkGateway.GatewayApiSdk.Model.TransactionPreviewRequest TransactionPreviewRequest { get; set; }

    public TransactionPreviewResponse TransactionPreviewResponse { get; set; }

    public TransactionSubmitResponse TransactionSubmitResponse { get; set; }

    public void CreateDefault()
    {
        NetworkName = "integrationtestsnet";

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

        NetworkConfigurationResponse = new NetworkConfigurationResponse(
            new NetworkConfigurationResponseVersion(CoreVersion, ApiVersion),
            NetworkName,
            "loc"
        );

        SubmitTransaction =
            $"{{\"network_identifier\": {{\"network\": \"{NetworkName}\"}}, \"notarized_transaction\": \"10020000001002000000100200000010070000000701110f000000496e7465726e616c546573746e6574000000000a00000000000000000a64000000000000000a0600000000000000912100000002f9308a019258c31049344f85f89d5229b531c845836f99b08601f113bce036f9010010010000003011010000000d000000436c656172417574685a6f6e65000000003023020000000200000091210000000279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f817989240000000d6f37bebb4c67ebb0844dd48e447c415a13b47fafdf13495f58b21826dc044a043fb00243cfe573bbb38b8ae9371801c2b91ec92ae764238e4ff40d857e58a3002000000912100000002c6047f9441ed7d6d3045406e95c07cd85c778e4b8cef3ca7abac09b95c709ee5924000000047da0da82cdceed2a227ebd305ece670cf12aaedad6863ebce173d0952eea73b3a1b136bae431d82bae822ceb11eaed406dddc1a4a94756201cb7292139584bf9240000000a767554290bd2cba8e63bc1feeefc1534ebcd33fe345f9a8d0ac76abc1d3bd5968e847ec5ca55d6e9fe18227f13c5c114463751e9bc5a38f563ba8819d7fc882\"}}";

        TransactionSubmitResponse = new TransactionSubmitResponse();

        // TransactionSummary = new TransactionSummary(
        //     CommittedTransactionsResponse.Transactions[0].StateVersion,
        //     0,
        //     0,
        //     0,
        //     false,
        //     false,
        //     Hash.ConvertFromHex(),
        //     Hash.ConvertFromHex(),
        //     Hash.ConvertFromHex(),
        //     BitConverter.GetBytes(CommittedTransactionsResponse.Transactions[0].StateVersion),
        //     new FakeClock().UtcNow,
        //     new FakeClock().UtcNow,
        //     new FakeClock().UtcNow
        // );

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
