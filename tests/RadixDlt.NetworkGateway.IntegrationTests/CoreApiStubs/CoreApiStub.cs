using RadixDlt.CoreApiSdk.Api;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.DataAggregator.NodeServices.ApiReaders;
using RadixDlt.NetworkGateway.GatewayApi.Configuration;
using RadixDlt.NetworkGateway.GatewayApi.CoreCommunications;
using RadixDlt.NetworkGateway.GatewayApi.Services;
using RadixDlt.NetworkGateway.IntegrationTests.Utilities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PublicKey = RadixDlt.CoreApiSdk.Model.PublicKey;
using TransactionIdentifier = RadixDlt.CoreApiSdk.Model.TransactionIdentifier;

namespace RadixDlt.NetworkGateway.IntegrationTests.CoreApiStubs;

public class CoreApiStub :
    ICoreNodeHealthChecker,
    INetworkConfigurationReader,
    ITransactionLogReader,
    DataAggregator.NodeServices.ApiReaders.ICoreApiProvider,
    GatewayApi.CoreCommunications.ICoreApiProvider,
    ICoreApiHandler
{
    #region responses

    private CoreApiNode _coreApiNode = new()
    {
        Enabled = true, Name = "node1", RequestWeighting = 1, CoreApiAddress = "3333",
    };

    private static readonly string _hash = "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20";

    private CoreNodeHealthResult _coreNodeHealthResult = new(
        new Dictionary<CoreNodeStatus, List<CoreApiNode>>()
        {
            {
                CoreNodeStatus.HealthyAndSynced, new List<CoreApiNode>()
                {
                    new()
                    {
                        Enabled = true, Name = "node1", RequestWeighting = 1, CoreApiAddress = "3333",
                    },
                }
            },
        }
    );

    private NetworkConfigurationResponse _networkConfigurationResponse = new(
        version: new NetworkConfigurationResponseVersion(coreVersion: "1.0.0", apiVersion: "1.1.1"),
        networkIdentifier: new NetworkIdentifier(network: DbSeedHelper.NetworkName),
        bech32HumanReadableParts: new Bech32HRPs(
            accountHrp: "ddx",
            validatorHrp: "dv",
            nodeHrp: "dn",
            resourceHrpSuffix: "_dr"));

    private CommittedTransactionsResponse _committedTransactionsResponse = new(
        stateIdentifier: new StateIdentifier(stateVersion: 0L, transactionAccumulator: "transaction accumulator"),
        transactions: new List<CommittedTransaction>()
        {
            new CommittedTransaction(
                new TransactionIdentifier(hash: _hash),
                committedStateIdentifier: new StateIdentifier(
                    stateVersion: 0L,
                    transactionAccumulator: "transaction accumulator"),
                operationGroups: new List<OperationGroup>()
                {
                    new OperationGroup(
                        operations: new List<Operation>()
                        {
                            new Operation(
                                type: "operation",
                                new EntityIdentifier(
                                    address: "address",
                                    subEntity: new SubEntity(
                                        address: "address",
                                        metadata: new SubEntityMetadata(
                                            validatorAddress: "validator address",
                                            epochUnlock: 0L)))),
                        }),
                },
                metadata: new CommittedTransactionMetadata(
                    size: 0,
                    hex: "0x12345",
                    fee: new ResourceAmount(
                        value: "1000",
                        resourceIdentifier: new ResourceIdentifier(type: "XRD")),
                    signedBy: new PublicKey(hex: "0x12345"),
                    message: "message"
                )
            ),
        }
    );

    private MempoolResponse _mempoolResponse = new MempoolResponse(
        transactionIdentifiers: new List<TransactionIdentifier>()
    {
        new(_hash),
    });

    private MempoolRequest _mempoolRequest =
        new MempoolRequest(networkIdentifier: new NetworkIdentifier(DbSeedHelper.NetworkName));

    private ConstructionBuildResponse _constructionBuildResponse = new ConstructionBuildResponse(
        unsignedTransaction: "unsigned transaction",
        payloadToSign: "payload to sign");

    private ConstructionFinalizeResponse _constructionFinalizeResponse = new ConstructionFinalizeResponse(
        signedTransaction: "signed transaction");

    private ConstructionParseResponse _constructionParseResponse = new ConstructionParseResponse(
        operationGroups: new List<OperationGroup>(),
        metadata: new ParsedTransactionMetadata(
        fee: new ResourceAmount(
        value: "1000",
        resourceIdentifier: new ResourceIdentifier(type: "token")),
        message: "default message"));

    private ConstructionHashResponse _constructionHashResponse = new ConstructionHashResponse(
        transactionIdentifier: new TransactionIdentifier(hash: "hash"));

    private ConstructionSubmitResponse _constructionSubmitResponse = new ConstructionSubmitResponse(
        transactionIdentifier: new TransactionIdentifier(hash: "hash"));

    private NetworkIdentifier _networkIdentifier = new NetworkIdentifier(network: DbSeedHelper.NetworkName);
    #endregion

    #region injected stubs
    public Task<CoreNodeHealthResult> CheckCoreNodeHealth(CancellationToken cancellationToken)
    {
        return Task.FromResult(_coreNodeHealthResult);
    }

    public Task<NetworkConfigurationResponse> GetNetworkConfiguration(CancellationToken token)
    {
        return Task.FromResult(_networkConfigurationResponse);
    }

    public Task<CommittedTransactionsResponse> GetTransactions(long stateVersion, int count, CancellationToken token)
    {
        return Task.FromResult(_committedTransactionsResponse);
    }

    public ConstructionApi ConstructionApi
    {
        get
        {
            return new ConstructionApi();
        }
    }

    public TransactionsApi TransactionsApi
    {
        get
        {
            return new TransactionsApi();
        }
    }

    public MempoolApi MempoolApi
    {
        get
        {
            return new MempoolApiStub(_mempoolResponse);
        }
    }

    public NetworkApi NetworkApi
    {
        get
        {
            return new NetworkApi();
        }
    }

    public CoreApiNode CoreApiNode
    {
        get
        {
            return _coreApiNode;
        }
    }

    public NetworkIdentifier GetNetworkIdentifier()
    {
        return _networkIdentifier;
    }

    public CoreApiNode GetCoreNodeConnectedTo()
    {
        return _coreApiNode;
    }

    public Task<ConstructionBuildResponse> BuildTransaction(ConstructionBuildRequest request, CancellationToken token = default)
    {
        return Task.FromResult(_constructionBuildResponse);
    }

    public Task<ConstructionParseResponse> ParseTransaction(ConstructionParseRequest request, CancellationToken token = default)
    {
        return Task.FromResult(_constructionParseResponse);
    }

    public Task<ConstructionFinalizeResponse> FinalizeTransaction(ConstructionFinalizeRequest request, CancellationToken token = default)
    {
        return Task.FromResult(_constructionFinalizeResponse);
    }

    public Task<ConstructionHashResponse> GetTransactionHash(ConstructionHashRequest request, CancellationToken token = default)
    {
        return Task.FromResult(_constructionHashResponse);
    }

    public Task<ConstructionSubmitResponse> SubmitTransaction(ConstructionSubmitRequest request, CancellationToken token = default)
    {
        return Task.FromResult(_constructionSubmitResponse);
    }
    #endregion
}
