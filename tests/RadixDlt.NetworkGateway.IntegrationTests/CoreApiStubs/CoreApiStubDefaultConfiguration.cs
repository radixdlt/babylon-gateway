using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.Commons.Extensions;
using RadixDlt.NetworkGateway.Commons.Model;
using RadixDlt.NetworkGateway.DataAggregator.Services;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

#nullable disable

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class CoreApiStubDefaultConfiguration
{
    public sealed record MempoolTransactionData(byte[] Id, DateTimeOffset SeenAt, byte[] Payload,
        Transaction Transaction)
    {
        public MempoolTransactionStatus TransactionStatus { get; set; }
    }

    public CoreApiStubDefaultConfiguration()
    {
        CreateDefault();
    }

    public string NetworkName { get; set; }

    public string CoreVersion { get; set; }

    public string ApiVersion { get; set; }

    public string Hash { get; set; }

    public string MempoolTransactionHash { get; set; }

    public GatewayApi.Configuration.CoreApiNode GatewayCoreApiNode { get; set; }

    public DataAggregator.Configuration.CoreApiNode DataAggregatorCoreApiNode { get; set; }

    public TransactionIdentifier TransactionIdentifier { get; set; }

    public NetworkIdentifier NetworkIdentifier { get; set; }

    public StateIdentifier StateIdentifier { get; set; }

    public CoreNodeHealthResult CoreNodeHealthResult { get; set; }

    public NetworkConfigurationResponse NetworkConfigurationResponse { get; set; }

    public CommittedTransaction CommittedTransaction { get; set; }

    public CommittedTransactionsResponse CommittedTransactionsResponse { get; set; }

    public MempoolResponse MempoolResponse { get; set; }

    public MempoolRequest MempoolRequest { get; set; }

    public ConstructionBuildResponse ConstructionBuildResponse { get; set; }

    public ConstructionFinalizeResponse ConstructionFinalizeResponse { get; set; }

    public string SubmitTransaction { get; set; }

    public ConstructionParseResponse ConstructionParseResponse { get; set; }

    public ConstructionHashResponse ConstructionHashResponse { get; set; }

    public ConstructionSubmitResponse ConstructionSubmitResponse { get; set; }

    public TransactionSummary TransactionSummary { get; set; }

    public MempoolTransactionData MempoolTransaction { get; set; }

    public void CreateDefault()
    {
        NetworkName = "integrationtestsnet";

        NetworkIdentifier = new NetworkIdentifier(NetworkName);

        CoreVersion = "1.0";

        ApiVersion = "2.0";

        Hash = "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";

        MempoolTransactionHash = "0607030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1fff";

        GatewayCoreApiNode = new()
        {
            Enabled = true, Name = "node1", RequestWeighting = 1, CoreApiAddress = "3333",
        };

        DataAggregatorCoreApiNode = new()
        {
            Enabled = GatewayCoreApiNode.Enabled,
            Name = GatewayCoreApiNode.Name,
            RequestWeighting = GatewayCoreApiNode.RequestWeighting,
            CoreApiAddress = GatewayCoreApiNode.CoreApiAddress,
        };

        TransactionIdentifier = new TransactionIdentifier(Hash);

        StateIdentifier = new StateIdentifier(
            stateVersion: 1L,
            transactionAccumulator: Hash);

        NetworkConfigurationResponse = new NetworkConfigurationResponse(
            version: new NetworkConfigurationResponseVersion(CoreVersion, ApiVersion),
            networkIdentifier: NetworkIdentifier,
            bech32HumanReadableParts: new Bech32HRPs(
                accountHrp: "ddx",
                validatorHrp: "dv",
                nodeHrp: "dn",
                resourceHrpSuffix: "_dr")
        );

        SubmitTransaction = $"{{\"network_identifier\": {{\"network\": \"{NetworkName}\"}}, \"notarized_transaction\": \"10020000001002000000100200000010070000000701110f000000496e7465726e616c546573746e6574000000000a00000000000000000a64000000000000000a0600000000000000912100000002f9308a019258c31049344f85f89d5229b531c845836f99b08601f113bce036f9010010010000003011010000000d000000436c656172417574685a6f6e65000000003023020000000200000091210000000279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f817989240000000d6f37bebb4c67ebb0844dd48e447c415a13b47fafdf13495f58b21826dc044a043fb00243cfe573bbb38b8ae9371801c2b91ec92ae764238e4ff40d857e58a3002000000912100000002c6047f9441ed7d6d3045406e95c07cd85c778e4b8cef3ca7abac09b95c709ee5924000000047da0da82cdceed2a227ebd305ece670cf12aaedad6863ebce173d0952eea73b3a1b136bae431d82bae822ceb11eaed406dddc1a4a94756201cb7292139584bf9240000000a767554290bd2cba8e63bc1feeefc1534ebcd33fe345f9a8d0ac76abc1d3bd5968e847ec5ca55d6e9fe18227f13c5c114463751e9bc5a38f563ba8819d7fc882\"}}";

        CommittedTransaction = new CommittedTransaction(
            TransactionIdentifier,
            StateIdentifier,
            operationGroups: new List<OperationGroup>()
            {
                new OperationGroup(
                    operations: new List<Operation>()
                    {
                        new Operation(
                            type: "Resource",
                            entityIdentifier: new EntityIdentifier(
                                address: "address",
                                subEntity: new SubEntity(
                                    address: "address",
                                    metadata: new SubEntityMetadata(
                                        validatorAddress: "validator address",
                                        epochUnlock: 0L)
                                )
                            )
                        ),
                    }),
            },
            metadata: new CommittedTransactionMetadata(
                size: 0,
                hex: Convert.ToHexString(Encoding.UTF8.GetBytes("0x12345")),
                fee: new ResourceAmount(
                    value: "1000",
                    resourceIdentifier: new ResourceIdentifier(type: "XRD")),
                signedBy: new PublicKey(hex: "0x12345"),
                message: Convert.ToHexString(Encoding.UTF8.GetBytes("message"))
            )
        );

        CommittedTransactionsResponse = new CommittedTransactionsResponse(
            stateIdentifier: new StateIdentifier(stateVersion: 0L, transactionAccumulator: Convert.ToHexString(Encoding.UTF8.GetBytes("transaction accumulator"))),
            transactions: new List<CommittedTransaction>() { CommittedTransaction }
        );

        MempoolResponse = new MempoolResponse(
            transactionIdentifiers: new List<TransactionIdentifier>() { TransactionIdentifier });

        MempoolRequest = new MempoolRequest(NetworkIdentifier);

        ConstructionBuildResponse = new ConstructionBuildResponse(
            unsignedTransaction: Convert.ToHexString(Encoding.UTF8.GetBytes("unsigned transaction")),
            payloadToSign: Convert.ToHexString(Encoding.UTF8.GetBytes("payload to sign")));

        ConstructionFinalizeResponse = new ConstructionFinalizeResponse(
            signedTransaction: SubmitTransaction);

        ConstructionParseResponse = new ConstructionParseResponse(
            operationGroups: new List<OperationGroup>(),
            metadata: new ParsedTransactionMetadata(
                fee: new ResourceAmount(
                    value: "1000",
                    resourceIdentifier: new ResourceIdentifier(type: "token")),
                message: "default message"));

        ConstructionHashResponse = new ConstructionHashResponse(TransactionIdentifier);

        ConstructionSubmitResponse = new ConstructionSubmitResponse(TransactionIdentifier);

        TransactionSummary = new TransactionSummary(
            StateVersion: CommittedTransaction.CommittedStateIdentifier.StateVersion,
            Epoch: 0,
            IndexInEpoch: 0,
            RoundInEpoch: 0,
            IsStartOfEpoch: false,
            IsStartOfRound: false,
            PayloadHash: CommittedTransaction.TransactionIdentifier.Hash.ConvertFromHex(),
            IntentHash: CommittedTransaction.TransactionIdentifier.Hash.ConvertFromHex(),
            SignedTransactionHash: CommittedTransaction.TransactionIdentifier.Hash.ConvertFromHex(),
            TransactionAccumulator: CommittedTransaction.CommittedStateIdentifier.TransactionAccumulator.ConvertFromHex(),
            RoundTimestamp: new FakeClock().UtcNow,
            CreatedTimestamp: new FakeClock().UtcNow,
            NormalizedRoundTimestamp: new FakeClock().UtcNow
        );

        CoreNodeHealthResult = new CoreNodeHealthResult(
            new Dictionary<CoreNodeStatus, List<CoreApiNode>>()
        {
            { CoreNodeStatus.HealthyAndSynced, new List<CoreApiNode>() { GatewayCoreApiNode } },
        });

        MempoolTransaction = new MempoolTransactionData(
            Id: Hash.ConvertFromHex(),
            SeenAt: new FakeClock().UtcNow,
            Payload: CommittedTransaction.Metadata.Hex.ConvertFromHex(),
            Transaction: new Transaction(
                new TransactionIdentifier(MempoolTransactionHash),
                CommittedTransaction.OperationGroups,
                CommittedTransaction.Metadata))
        {
            TransactionStatus = MempoolTransactionStatus.SubmittedOrKnownInNodeMempool,
        };
    }
}
